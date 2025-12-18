using AI.Invoicer.Domain.Model.AI;
using AI.Invoicer.Infrastructure.AIService;
using AI.Invoicer.Infrastructure.Interface;
using FluentAssertions;
using Moq;
using System.ComponentModel.Design;

namespace AI.Invoicer.Infrastructure.Tests
{
    public class InvoiceCommandServiceTests
    {
        private readonly IInvoiceCommandService _sut;
        private readonly Mock<IInferenceService> _mockInferenceService;

        public InvoiceCommandServiceTests()
        {
            _mockInferenceService = new Mock<IInferenceService>();
            _sut = new InvoiceCommandService(_mockInferenceService.Object);
        }

        [Fact]
        public async Task GetCommandsFromPromptAsync_WithNullPrompt_ShouldThrowArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.GetCommandsFromPromptAsync("",null!));
        }

        [Fact]
        public async Task GetCommandsFromPromptAsync_WithEmptyPrompt_ShouldThrowArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.GetCommandsFromPromptAsync(string.Empty,null));
        }

        [Fact]
        public async Task GetCommandsFromPromptAsync_WithWhitespacePrompt_ShouldThrowArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.GetCommandsFromPromptAsync("   ", null));
        }
    }
}
