using AI.Invoicer.Infrastructure.Interface;
using Microsoft.ML.OnnxRuntimeGenAI;
using System;
using System.Collections.Generic;
using System.Text;

namespace AI.Invoicer.Infrastructure.AIService
{
    public class OnnxInferenceService : IInferenceService,IDisposable
    {
        private Model? _model;
        private bool _disposed;
        private bool _isInitialized;
        private Tokenizer? _tokenizer;
        private readonly int _maxTokensToGenerate;

        public OnnxInferenceService(int maxTokensToGenerate = 256)
        {
            _maxTokensToGenerate = maxTokensToGenerate;
            _isInitialized = false;
            _disposed = false;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _tokenizer?.Dispose();
            _model?.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        private void ThrowIfNotInitialized()
        {
            if (!_isInitialized)
                throw new InvalidOperationException(
                    "Le service d'inférence n'a pas été initialisé. " +
                    "Appelez InitializeAsync() d'abord.");

            if (_disposed)
                throw new ObjectDisposedException(nameof(OnnxInferenceService));
        }

        /// <summary>
        /// Nettoie la réponse en extrayant uniquement la partie pertinente de l'assistant.
        /// </summary>
        private string CleanResponse(string fullResponseText)
        {
            if (string.IsNullOrWhiteSpace(fullResponseText))
                return string.Empty;

            // Chercher les marqueurs de fin d'assistant (selon le modèle)
            var assistantEndMarkers = new[] { "<|end|>", "</s>", "[END]" };

            foreach (var marker in assistantEndMarkers)
            {
                var index = fullResponseText.LastIndexOf(marker, StringComparison.OrdinalIgnoreCase);
                if (index != -1)
                {
                    return fullResponseText.Substring(0, index).Trim();
                }
            }

            return fullResponseText.Trim();
        }

        public async Task<string> GenerateResponseAsync(string prompt)
        {
            ThrowIfNotInitialized();

            if (string.IsNullOrWhiteSpace(prompt))
                throw new ArgumentException("Le prompt ne peut pas être vide.", nameof(prompt));

            return await Task.Run(() => GenerateResponseInternal(prompt));
        }


        private string GenerateResponseInternal(string prompt)
        {
            if (_model == null || _tokenizer == null)
                throw new InvalidOperationException("Le service n'est pas correctement initialisé.");

            try
            {
                // --- Étape 1: Tokeniser le prompt ---
                var inputSequence = _tokenizer.Encode(prompt);

                // --- Étape 2: Préparer les paramètres de génération ---
                using var generatorParams = new GeneratorParams(_model);
                generatorParams.SetSearchOption("max_length", _maxTokensToGenerate);

                // --- Étape 3: Créer le générateur et générer la réponse ---
                using var generator = new Generator(_model, generatorParams);
                generator.AppendTokenSequences(inputSequence);
                using var tokenizerStream = _tokenizer.CreateStream();

                var responseBuilder = new StringBuilder();

                // Générer les tokens un par un
                while (!generator.IsDone())
                {
                    generator.GenerateNextToken();

                    // Récupérer et décoder le dernier token généré
                    var sequence = generator.GetSequence(0);
                    var lastToken = sequence[^1];
                    var decodedText = tokenizerStream.Decode(lastToken);

                    responseBuilder.Append(decodedText);
                }

                var fullResponse = responseBuilder.ToString();
                return CleanResponse(fullResponse);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Erreur lors de la génération de la réponse: {ex.Message}", ex);
            }
        }


        public async Task InitializeAsync(string modelPath)
        {
            if (string.IsNullOrWhiteSpace(modelPath))
                throw new ArgumentNullException(nameof(modelPath), "Le chemin du modèle ne peut pas être vide.");

            if (!System.IO.Directory.Exists(modelPath))
                throw new DirectoryNotFoundException($"Le répertoire du modèle n'existe pas: {modelPath}");

            await Task.Run(() =>
            {
                try
                {
                    // Charger le modèle ONNX GenAI
                    // Le répertoire doit contenir config.json et model.onnx
                    _model = new Model(modelPath);

                    // Créer le tokenizer à partir du modèle chargé
                    _tokenizer = new Tokenizer(_model);

                    _isInitialized = true;
                }
                catch (DirectoryNotFoundException ex)
                {
                    throw new InvalidOperationException(
                        $"Le répertoire du modèle n'existe pas: {modelPath}", ex);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"Erreur lors de l'initialisation du service d'inférence. " +
                        $"Assurez-vous que le répertoire '{modelPath}' contient les fichiers requis " +
                        $"(config.json, model.onnx et fichiers de tokenizer). Détail: {ex.Message}", ex);
                }
            });
        }
    }
}
