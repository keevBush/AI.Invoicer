using AI.Invoicer.Infrastructure.AIService;
using AI.Invoicer.Infrastructure.Interface;
using FluentAssertions;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace AI.Invoicer.Infrastructure.Tests
{
    /// <summary>
    /// Tests pour le service d'inférence ONNX.
    /// Ces tests nécessitent les fichiers réels du modèle et du tokenizer.
    /// </summary>
    public class OnnxInferenceServiceTests : IAsyncLifetime
    {
        private OnnxInferenceService? _sut;
        private string? _testModelPath;

        /// <summary>
        /// Configuration initiale avant chaque test.
        /// </summary>
        public async Task InitializeAsync()
        {
            // Setup - Le modèle doit être présent pour les tests d'intégration
            _testModelPath = GetTestModelPath();
            _sut = new OnnxInferenceService(maxTokensToGenerate: 100);

            if (ModelExists())
            {
                await _sut.InitializeAsync(_testModelPath!);
            }
        }

        /// <summary>
        /// Nettoyage après chaque test.
        /// </summary>
        public async Task DisposeAsync()
        {
            _sut?.Dispose();
            await Task.CompletedTask;
        }

        #region Tests de Configuration

        [Fact]
        [Trait("Category", "Unit")]
        public void Constructor_WithDefaultParameters_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var service = new OnnxInferenceService();

            // Assert
            service.Should().NotBeNull();
            // Le service ne doit pas être initialisé avant InitializeAsync
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Constructor_WithCustomMaxTokens_ShouldAcceptIt()
        {
            // Arrange & Act
            var service = new OnnxInferenceService(maxTokensToGenerate: 512);

            // Assert
            service.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task InitializeAsync_WithNullPath_ShouldThrowArgumentNullException()
        {
            // Arrange
            var service = new OnnxInferenceService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => service.InitializeAsync(null!));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task InitializeAsync_WithEmptyPath_ShouldThrowArgumentNullException()
        {
            // Arrange
            var service = new OnnxInferenceService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => service.InitializeAsync(string.Empty));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task InitializeAsync_WithNonExistentDirectory_ShouldThrowDirectoryNotFoundException()
        {
            // Arrange
            var service = new OnnxInferenceService();
            var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            // Act & Assert
            await Assert.ThrowsAsync<DirectoryNotFoundException>(() => service.InitializeAsync(nonExistentPath));
        }

        #endregion

        #region Tests d'Inférence (nécessitent le modèle)

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("RequiresModel", "true")]
        public async Task GenerateResponseAsync_WithValidPrompt_ShouldReturnNonEmptyString()
        {
            // Skip si le modèle n'est pas disponible
            if (!ModelExists())
            {
                Assert.True(true, "Modèle non disponible - test ignoré");
                return;
            }

            // Arrange
            var prompt = "<|system|>Tu es un assistant.<|end|><|user|>Bonjour<|end|><|assistant|>";

            // Act
            var response = await _sut!.GenerateResponseAsync(prompt);

            // Assert
            response.Should().NotBeNullOrWhiteSpace();
            response.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("RequiresModel", "true")]
        public async Task GenerateResponseAsync_WithJsonPrompt_ShouldReturnValidJson()
        {
            // Skip si le modèle n'est pas disponible
            if (!ModelExists())
            {
                Assert.True(true, "Modèle non disponible - test ignoré");
                return;
            }

            // Arrange
            var prompt = @"<|system|>Tu es un assistant comptable. Réponds uniquement avec du JSON valide.<|end|>
<|user|>{""{""action"": ""addLine"", ""data"": {""description"": ""test""}}<|end|>
<|assistant|>";

            // Act
            var response = await _sut!.GenerateResponseAsync(prompt);

            // Assert
            response.Should().NotBeNullOrWhiteSpace();

            // Vérifier que la réponse ressemble à du JSON
            response.Trim().Should().StartWith("{");
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("RequiresModel", "true")]
        public async Task GenerateResponseAsync_WithEmptyPrompt_ShouldThrowArgumentException()
        {
            // Skip si le modèle n'est pas disponible
            if (!ModelExists())
            {
                Assert.True(true, "Modèle non disponible - test ignoré");
                return;
            }

            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _sut!.GenerateResponseAsync(string.Empty));
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("RequiresModel", "true")]
        public async Task GenerateResponseAsync_WithNullPrompt_ShouldThrowArgumentException()
        {
            // Skip si le modèle n'est pas disponible
            if (!ModelExists())
            {
                Assert.True(true, "Modèle non disponible - test ignoré");
                return;
            }

            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _sut!.GenerateResponseAsync(null!));
        }

        #endregion

        #region Tests de Gestion des Ressources

        [Fact]
        [Trait("Category", "Unit")]
        public void Dispose_ShouldNotThrowException()
        {
            // Arrange
            var service = new OnnxInferenceService();

            // Act
            var action = () => service.Dispose();

            // Assert
            action.Should().NotThrow();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Dispose_MultipleTimes_ShouldNotThrowException()
        {
            // Arrange
            var service = new OnnxInferenceService();

            // Act
            var action = () =>
            {
                service.Dispose();
                service.Dispose();
            };

            // Assert
            action.Should().NotThrow();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GenerateResponseAsync_BeforeInitialization_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var service = new OnnxInferenceService();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.GenerateResponseAsync("test prompt"));
        }

        #endregion

        #region Helpers

        private static string GetTestModelPath()
        {
            // Chemins possibles où le modèle pourrait se trouver
            var possiblePaths = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "TestAssets", "phi3-mini"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "TestAssets", "phi3-mini"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".aitk", "models", "Phi-3-mini-4k-instruct-onnx"),
            };

            foreach (var path in possiblePaths)
            {
                if (Directory.Exists(path))
                    return path;
            }

            return Path.Combine(Directory.GetCurrentDirectory(), "TestAssets", "phi3-mini");
        }

        private static bool ModelExists()
        {
            var modelPath = GetTestModelPath();
            return Directory.Exists(modelPath) &&
                   File.Exists(Path.Combine(modelPath, "config.json")) &&
                   File.Exists(Path.Combine(modelPath, "model.onnx"));
        }

        #endregion
    }
}