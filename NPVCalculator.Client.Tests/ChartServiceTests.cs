using FluentAssertions;
using Microsoft.JSInterop;
using Moq;
using NPVCalculator.Client.Services;
using NPVCalculator.Shared.Models;

namespace NPVCalculator.Client.Tests.Services
{
    public class ChartServiceTests
    {
        private readonly Mock<IJSRuntime> _mockJSRuntime;
        private readonly ChartService _service;

        public ChartServiceTests()
        {
            _mockJSRuntime = new Mock<IJSRuntime>();
            _service = new ChartService(_mockJSRuntime.Object);
        }

        [Fact]
        public async Task RenderNpvChart_WithValidResults_ShouldCallJavaScript()
        {
            // Arrange
            var canvasId = "test-chart";
            var results = new List<NpvResult>
            {
                new() { Rate = 1m, Value = 100m },
                new() { Rate = 2m, Value = 50m },
                new() { Rate = 3m, Value = -25m }
            };

            // Act
            await _service.RenderNpvChart(canvasId, results);

            // Assert
            _mockJSRuntime.Verify(
                x => x.InvokeAsync<object>(
                    "eval",
                    It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public async Task RenderNpvChart_WithEmptyResults_ShouldNotCallJavaScript()
        {
            // Arrange
            var canvasId = "test-chart";
            var results = new List<NpvResult>();

            // Act
            await _service.RenderNpvChart(canvasId, results);

            // Assert
            _mockJSRuntime.Verify(
                x => x.InvokeAsync<object>(
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Never);
        }

        [Fact]
        public async Task RenderNpvChart_WithJSException_ShouldHandleGracefully()
        {
            // Arrange
            var canvasId = "test-chart";
            var results = new List<NpvResult>
            {
                new() { Rate = 1m, Value = 100m }
            };

            _mockJSRuntime.Setup(x => x.InvokeAsync<object>(
                    It.IsAny<string>(),
                    It.IsAny<object[]>()))
                .ThrowsAsync(new JSException("Chart.js not loaded"));

            // Act & Assert
            var action = async () => await _service.RenderNpvChart(canvasId, results);
            await action.Should().NotThrowAsync();
        }
    }
}