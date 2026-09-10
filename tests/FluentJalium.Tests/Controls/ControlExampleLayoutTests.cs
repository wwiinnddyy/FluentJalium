using FluentJalium.Controls;
using FluentJalium.Gallery.Controls;
using Jalium.UI;
using Jalium.UI.Controls;
using Xunit;

namespace FluentJalium.Tests.Controls;

/// <summary>
/// The example card has to fit the width the page gives it: its options column is capped at 320
/// and the sample surface takes the remainder, so a card that measures wider than its constraint
/// pushes the options pane off screen.
/// </summary>
[Collection("Application")]
public sealed class ControlExampleLayoutTests
{
    private const double AvailableWidth = 1100;

    [Fact]
    public void ControlExample_WhenMeasuredWithConstraint_ShouldNotExceedIt()
    {
        var card = new ControlExample
        {
            HeaderText = "Counter",
            Example = new FWButton { Content = "Click me" },
            Options = new StackPanel
            {
                Spacing = 8,
                MinWidth = 180,
                Children = { new TextBlock { Text = "Actions" } }
            }
        };

        card.Measure(new Size(AvailableWidth, 600));
        card.Arrange(new Rect(0, 0, card.DesiredSize.Width, card.DesiredSize.Height));

        Assert.True(
            card.DesiredSize.Width <= AvailableWidth,
            $"card desired {card.DesiredSize.Width} overflows the {AvailableWidth} constraint");
    }

    [Fact]
    public void ControlExample_WithoutOutputOrOptions_ShouldStillFitConstraint()
    {
        var card = new ControlExample
        {
            HeaderText = "Plain",
            Example = new FWButton { Content = "Click me" }
        };

        card.Measure(new Size(AvailableWidth, 600));

        Assert.True(
            card.DesiredSize.Width <= AvailableWidth,
            $"card desired {card.DesiredSize.Width} overflows the {AvailableWidth} constraint");
    }
}
