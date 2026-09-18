using Jalium.UI;
using Jalium.UI.Controls;

namespace FluentJalium.Controls;

/// <summary>
/// Arranges a description and an action side by side, or stacks them at narrow widths.
/// Children keep their visual parents, bindings, margins, and keyboard focus.
/// </summary>
public sealed class FluentSettingsRow : Panel
{
    public static readonly DependencyProperty StackAtWidthProperty = DependencyProperty.Register(
        nameof(StackAtWidth), typeof(double), typeof(FluentSettingsRow),
        new FrameworkPropertyMetadata(420d, FrameworkPropertyMetadataOptions.AffectsMeasure), IsValidLength);

    public static readonly DependencyProperty ActionSpacingProperty = DependencyProperty.Register(
        nameof(ActionSpacing), typeof(double), typeof(FluentSettingsRow),
        new FrameworkPropertyMetadata(16d, FrameworkPropertyMetadataOptions.AffectsMeasure), IsValidLength);

    public static readonly DependencyProperty StackSpacingProperty = DependencyProperty.Register(
        nameof(StackSpacing), typeof(double), typeof(FluentSettingsRow),
        new FrameworkPropertyMetadata(12d, FrameworkPropertyMetadataOptions.AffectsMeasure), IsValidLength);

    public double StackAtWidth
    {
        get => (double)GetValue(StackAtWidthProperty)!;
        set => SetValue(StackAtWidthProperty, value);
    }

    public double ActionSpacing
    {
        get => (double)GetValue(ActionSpacingProperty)!;
        set => SetValue(ActionSpacingProperty, value);
    }

    public double StackSpacing
    {
        get => (double)GetValue(StackSpacingProperty)!;
        set => SetValue(StackSpacingProperty, value);
    }

    public bool IsStacked { get; private set; }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (Children.Count > 2)
            throw new InvalidOperationException("FluentSettingsRow accepts a description followed by one action. Group additional content in a panel.");
        if (Children.Count == 0) return default;

        IsStacked = double.IsFinite(availableSize.Width) && availableSize.Width < StackAtWidth;
        if (Children.Count == 1)
        {
            Children[0].Measure(availableSize);
            return Children[0].DesiredSize;
        }

        var action = Children[1];
        action.Measure(availableSize);
        var descriptionWidth = IsStacked ? availableSize.Width : Math.Max(0, availableSize.Width - action.DesiredSize.Width - ActionSpacing);
        Children[0].Measure(new Size(descriptionWidth, availableSize.Height));
        var description = Children[0].DesiredSize;
        return IsStacked
            ? new Size(Math.Max(description.Width, action.DesiredSize.Width), description.Height + StackSpacing + action.DesiredSize.Height)
            : new Size(description.Width + ActionSpacing + action.DesiredSize.Width, Math.Max(description.Height, action.DesiredSize.Height));
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Children.Count == 0) return finalSize;
        if (Children.Count == 1)
        {
            Children[0].Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
            return finalSize;
        }

        var action = Children[1];
        var actionWidth = Math.Min(finalSize.Width, action.DesiredSize.Width);
        if (IsStacked)
        {
            var descriptionHeight = Math.Min(finalSize.Height, Children[0].DesiredSize.Height);
            var actionTop = Math.Min(finalSize.Height, descriptionHeight + StackSpacing);
            Children[0].Arrange(new Rect(0, 0, finalSize.Width, descriptionHeight));
            action.Arrange(new Rect(0, actionTop, actionWidth, Math.Max(0, finalSize.Height - actionTop)));
        }
        else
        {
            Children[0].Arrange(new Rect(0, 0, Math.Max(0, finalSize.Width - actionWidth - ActionSpacing), finalSize.Height));
            action.Arrange(new Rect(finalSize.Width - actionWidth, 0, actionWidth, finalSize.Height));
        }
        return finalSize;
    }

    private static bool IsValidLength(object? value) => value is double length && double.IsFinite(length) && length >= 0;
}
