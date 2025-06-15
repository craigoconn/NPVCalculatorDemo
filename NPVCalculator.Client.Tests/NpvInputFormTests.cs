using Bunit;
using FluentAssertions;
using NPVCalculator.Client.Components;
using NPVCalculator.Client.Models;

namespace NPVCalculator.Client.Tests.Components
{
    public class NpvInputFormTests : TestContext
    {
        [Fact]
        public void NpvInputForm_ShouldRenderCorrectly()
        {
            // Arrange
            var model = new NpvInputModel();
            var errors = new List<string>();

            // Act
            var component = RenderComponent<NpvInputForm>(parameters => parameters
                .Add(p => p.Model, model)
                .Add(p => p.IsCalculating, false)
                .Add(p => p.Errors, errors));

            // Assert
            component.Find("h3").TextContent.Should().Be("NPV Calculator");

            // Look for input with the actual placeholder text
            var cashFlowInput = component.Find("input[placeholder*='-1000,300,400,500']") ??
                               component.Find("input[placeholder*='comma-separated']") ??
                               component.Find("input[placeholder*='cash']");

            cashFlowInput.Should().NotBeNull();
            component.Find("button").TextContent.Should().Contain("Calculate");
        }

        [Fact]
        public void NpvInputForm_WhenCalculating_ShouldShowSpinner()
        {
            // Arrange
            var model = new NpvInputModel();
            var errors = new List<string>();

            // Act
            var component = RenderComponent<NpvInputForm>(parameters => parameters
                .Add(p => p.Model, model)
                .Add(p => p.IsCalculating, true)
                .Add(p => p.Errors, errors));

            // Assert
            var button = component.Find("button");
            button.Should().NotBeNull();
            button.HasAttribute("disabled").Should().BeTrue();

            // Check for spinner
            var spinner = component.Find(".spinner-border");
            spinner.Should().NotBeNull();

            button.TextContent.Should().Contain("Calculating");
        }

        [Fact]
        public void NpvInputForm_WithErrors_ShouldDisplayErrorAlert()
        {
            // Arrange
            var model = new NpvInputModel();
            var errors = new List<string> { "Test error 1", "Test error 2" };

            // Act
            var component = RenderComponent<NpvInputForm>(parameters => parameters
                .Add(p => p.Model, model)
                .Add(p => p.IsCalculating, false)
                .Add(p => p.Errors, errors));

            // Assert
            var alertElement = component.Find(".alert-danger");
            alertElement.Should().NotBeNull();
            alertElement.TextContent.Should().Contain("Test error 1");
            alertElement.TextContent.Should().Contain("Test error 2");
        }

        [Fact]
        public void NpvInputForm_WithCashFlowError_ShouldShowInvalidFeedback()
        {
            // Arrange
            var model = new NpvInputModel { HasCashFlowError = true };
            var errors = new List<string>();

            // Act
            var component = RenderComponent<NpvInputForm>(parameters => parameters
                .Add(p => p.Model, model)
                .Add(p => p.IsCalculating, false)
                .Add(p => p.Errors, errors));

            // Assert
            var inputs = component.FindAll("input");
            inputs.Should().NotBeEmpty();

            // Find the cash flow input (first input)
            var cashFlowInput = inputs[0];
            cashFlowInput.ClassList.Should().Contain("is-invalid");

            var invalidFeedback = component.Find(".invalid-feedback");
            invalidFeedback.Should().NotBeNull();
        }

        [Fact]
        public void NpvInputForm_ShouldHaveCorrectInputFields()
        {
            // Arrange
            var model = new NpvInputModel();
            var errors = new List<string>();

            // Act
            var component = RenderComponent<NpvInputForm>(parameters => parameters
                .Add(p => p.Model, model)
                .Add(p => p.IsCalculating, false)
                .Add(p => p.Errors, errors));

            // Assert
            var inputs = component.FindAll("input");
            inputs.Should().HaveCountGreaterOrEqualTo(4); // Cash flows + 3 rate inputs

            // Check for labels
            var labels = component.FindAll("label");
            labels.Should().NotBeEmpty();

            // Should have button
            var button = component.Find("button");
            button.Should().NotBeNull();
        }
    }
}