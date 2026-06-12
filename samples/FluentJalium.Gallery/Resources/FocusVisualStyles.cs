using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;

namespace FluentJalium.Gallery.Resources;

/// <summary>
/// Fluent Design System focus visual styles with dual-ring focus indicator.
/// Based on WinUI 3 and FluentAvalonia focus visual patterns.
/// </summary>
internal static class FocusVisualStyles
{
    /// <summary>
    /// Creates a dual-ring focus visual style matching Fluent Design System.
    /// Outer ring: 2px white/black stroke with 3px offset
    /// Inner ring: 1px black/white stroke with 1px offset
    /// </summary>
    public static Style CreateFluentFocusVisualStyle()
    {
        var style = new Style(typeof(Control));

        var outerBorder = new Border
        {
            BorderBrush = GetFocusVisualBrush("FocusVisualPrimaryBrush"),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(4),
            Margin = new Thickness(-3),
            IsHitTestVisible = false
        };

        var innerBorder = new Border
        {
            BorderBrush = GetFocusVisualBrush("FocusVisualSecondaryBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(2),
            Margin = new Thickness(-1),
            IsHitTestVisible = false
        };

        var grid = new Grid();
        grid.Children.Add(outerBorder);
        grid.Children.Add(innerBorder);

        style.Setters.Add(new Setter(Control.TemplateProperty, grid));
        return style;
    }

    /// <summary>
    /// Registers focus visual brushes in application resources.
    /// Light theme: outer white, inner black
    /// Dark theme: outer black, inner white
    /// </summary>
    public static void RegisterFocusVisualBrushes(ResourceDictionary resources)
    {
        if (resources == null) return;

        resources["FocusVisualPrimaryBrush"] = new SolidColorBrush(Colors.White);
        resources["FocusVisualSecondaryBrush"] = new SolidColorBrush(Colors.Black);
        resources["FocusVisualPrimaryBrushDark"] = new SolidColorBrush(Colors.Black);
        resources["FocusVisualSecondaryBrushDark"] = new SolidColorBrush(Colors.White);
    }

    /// <summary>
    /// Creates a compact focus visual style for dense UI controls (smaller offset, thinner rings).
    /// </summary>
    public static Style CreateCompactFocusVisualStyle()
    {
        var style = new Style(typeof(Control));

        var outerBorder = new Border
        {
            BorderBrush = GetFocusVisualBrush("FocusVisualPrimaryBrush"),
            BorderThickness = new Thickness(1.5),
            CornerRadius = new CornerRadius(3),
            Margin = new Thickness(-2),
            IsHitTestVisible = false
        };

        var innerBorder = new Border
        {
            BorderBrush = GetFocusVisualBrush("FocusVisualSecondaryBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(2),
            Margin = new Thickness(-0.5),
            IsHitTestVisible = false
        };

        var grid = new Grid();
        grid.Children.Add(outerBorder);
        grid.Children.Add(innerBorder);

        style.Setters.Add(new Setter(Control.TemplateProperty, grid));
        return style;
    }

    private static Brush GetFocusVisualBrush(string key)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Brush brush)
        {
            return brush;
        }
        return new SolidColorBrush(Colors.White);
    }
}
