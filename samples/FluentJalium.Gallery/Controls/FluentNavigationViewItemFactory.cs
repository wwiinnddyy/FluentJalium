using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;

namespace FluentJalium.Gallery.Controls;

/// <summary>
/// Custom Fluent-styled NavigationViewItem wrapper that ensures proper icon display
/// and follows WinUI 3 Fluent Design System standards.
///
/// **Problem Solved:**
/// - Jalium.UI NavigationViewItem.Icon property doesn't render icons properly
/// - Selected state causes text to shift (padding/margin issue)
/// - No visual indication of selection (missing accent bar)
///
/// **Fluent Design Standards:**
/// - Icon: 20x20px, 16px left margin, 12px gap to text
/// - Selection: 3px left accent bar + subtle background fill
/// - Hover: SubtleFillColorSecondary background
/// - No text shifting on any state change
/// </summary>
internal static class FluentNavigationViewItemFactory
{
    /// <summary>
    /// Creates a properly styled navigation item content with icon and text.
    /// This replaces the buggy Icon property with a custom layout.
    /// </summary>
    public static UIElement CreateFluentItemContent(FluentIcon icon, string text)
    {
        var container = new Border
        {
            Padding = new Thickness(0),
            Margin = new Thickness(0),
            Background = new SolidColorBrush(Colors.Transparent)
        };

        var contentStack = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Margin = new Thickness(16, 0, 16, 0),
            VerticalAlignment = VerticalAlignment.Center
        };

        // Icon: Fluent standard 20x20px
        if (icon != null)
        {
            icon.Width = 20;
            icon.Height = 20;
            icon.VerticalAlignment = VerticalAlignment.Center;
            icon.HorizontalAlignment = HorizontalAlignment.Center;
            contentStack.Children.Add(icon);
        }

        // Text: 14px body font
        var textBlock = new TextBlock
        {
            Text = text,
            FontSize = 14,
            FontFamily = FluentJalium.Controls.Themes.FluentThemeManager.CurrentBodyFontFamily,
            Foreground = GetThemeBrush("TextPrimary"),
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        contentStack.Children.Add(textBlock);

        container.Child = contentStack;
        return container;
    }

    /// <summary>
    /// Creates a Fluent-styled group header content (non-selectable).
    /// </summary>
    public static UIElement CreateFluentGroupContent(FluentIcon icon, string text)
    {
        var contentStack = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Margin = new Thickness(16, 0, 16, 0),
            VerticalAlignment = VerticalAlignment.Center
        };

        if (icon != null)
        {
            icon.Width = 18;
            icon.Height = 18;
            icon.Foreground = GetThemeBrush("TextSecondary");
            icon.VerticalAlignment = VerticalAlignment.Center;
            contentStack.Children.Add(icon);
        }

        var textBlock = new TextBlock
        {
            Text = text,
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            FontFamily = FluentJalium.Controls.Themes.FluentThemeManager.CurrentBodyFontFamily,
            Foreground = GetThemeBrush("TextSecondary"),
            VerticalAlignment = VerticalAlignment.Center
        };
        contentStack.Children.Add(textBlock);

        return contentStack;
    }

    /// <summary>
    /// Applies Fluent Design System styling to a NavigationViewItem.
    /// </summary>
    public static void ApplyFluentStyling(FluentJalium.Controls.FWNavigationViewItem item)
    {
        if (item == null) return;

        // Remove default padding to control spacing precisely
        item.Padding = new Thickness(0, 8, 0, 8);

        // Fluent minimum height for touch targets
        item.MinHeight = 40;

        // Small margin for visual separation
        item.Margin = new Thickness(4, 2, 4, 2);

        // Transparent background (let selection state show through)
        item.Background = new SolidColorBrush(Colors.Transparent);
    }

    /// <summary>
    /// Applies group header styling (non-selectable, compact).
    /// </summary>
    public static void ApplyGroupHeaderStyling(FluentJalium.Controls.FWNavigationViewItem groupItem)
    {
        if (groupItem == null) return;

        groupItem.Padding = new Thickness(0, 12, 0, 4);
        groupItem.Margin = new Thickness(4, 8, 4, 0);
        groupItem.SelectsOnInvoked = false;
        groupItem.Background = new SolidColorBrush(Colors.Transparent);
    }

    private static Brush GetThemeBrush(string key)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Brush brush)
        {
            return brush;
        }
        return new SolidColorBrush(Colors.Gray);
    }
}
