using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;

namespace FluentJalium.Gallery.Styles;

/// <summary>
/// Applies WinUI 3 Fluent Design System styles to FWNavigationView and FWNavigationViewItem.
///
/// **Fluent Design Standard Requirements:**
/// - Icons must be visible on the left side of each item (20x20px)
/// - Selected state: Left accent bar (3px width) + subtle background fill
/// - Hover state: Subtle background fill (SubtleFillColorSecondary)
/// - Pressed state: Darker background fill (SubtleFillColorTertiary)
/// - Proper spacing: Icon 16px left padding, 12px gap to text
/// - No text shifting on selection
/// </summary>
internal static class FluentNavigationViewStyles
{
    /// <summary>
    /// Configures a FWNavigationView with Fluent Design System compliant styling.
    /// </summary>
    public static void ApplyFluentStyle(FWNavigationView navigationView)
    {
        if (navigationView == null) return;

        // Fluent Design: Transparent background to let Mica/Acrylic show through
        navigationView.Background = new SolidColorBrush(Colors.Transparent);

        // Fluent Design: Acrylic pane background with proper opacity
        navigationView.PaneBackground = GetThemeBrush("FluentMaterialShellPaneBrush");

        // Fluent Design: Content area should be transparent
        navigationView.ContentBackground = new SolidColorBrush(Colors.Transparent);

        // Fluent Design: Standard pane metrics (320px open, 48px compact)
        navigationView.OpenPaneLength = 320;
        navigationView.CompactPaneLength = 48;

        // Fluent Design: Pane toggle button should be visible
        navigationView.IsPaneToggleButtonVisible = true;
    }

    /// <summary>
    /// Wraps a FWNavigationViewItem to ensure icon visibility and proper Fluent styling.
    /// Creates a styled container with Icon + Text layout.
    /// </summary>
    public static void ApplyFluentItemStyle(FWNavigationViewItem item, FluentIcon icon, string text)
    {
        if (item == null) return;

        // Create Fluent-compliant content layout: [Icon] [Text]
        var contentPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Margin = new Thickness(16, 0, 16, 0),
            VerticalAlignment = VerticalAlignment.Center
        };

        // Icon: 20x20px as per Fluent Design
        if (icon != null)
        {
            icon.Width = 20;
            icon.Height = 20;
            icon.VerticalAlignment = VerticalAlignment.Center;
            contentPanel.Children.Add(icon);
        }

        // Text: Fluent standard font (14px BodyFontFamily)
        var textBlock = new TextBlock
        {
            Text = text,
            FontSize = 14,
            FontFamily = FluentThemeManager.CurrentBodyFontFamily,
            Foreground = GetThemeBrush("TextPrimary"),
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        contentPanel.Children.Add(textBlock);

        // Replace NavigationViewItem content with styled panel
        item.Content = contentPanel;

        // Fluent Design: Remove default padding to control spacing precisely
        item.Padding = new Thickness(0, 8, 0, 8);

        // Fluent Design: Minimum height for touch targets (40px)
        item.MinHeight = 40;
    }

    /// <summary>
    /// Creates a Fluent-styled group header item (non-selectable, with icon).
    /// </summary>
    public static void ApplyFluentGroupStyle(FWNavigationViewItem groupItem, FluentIcon icon, string text)
    {
        if (groupItem == null) return;

        var contentPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Margin = new Thickness(16, 0, 16, 0)
        };

        if (icon != null)
        {
            icon.Width = 18;
            icon.Height = 18;
            icon.Foreground = GetThemeBrush("TextSecondary");
            contentPanel.Children.Add(icon);
        }

        var textBlock = new TextBlock
        {
            Text = text,
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            FontFamily = FluentThemeManager.CurrentBodyFontFamily,
            Foreground = GetThemeBrush("TextSecondary"),
            VerticalAlignment = VerticalAlignment.Center
        };
        contentPanel.Children.Add(textBlock);

        groupItem.Content = contentPanel;
        groupItem.Padding = new Thickness(0, 12, 0, 4);
        groupItem.SelectsOnInvoked = false;
    }

    /// <summary>
    /// Applies Fluent Design visual states to a NavigationViewItem.
    /// This includes hover, pressed, selected, and focus states.
    /// </summary>
    public static void ConfigureFluentVisualStates(FWNavigationViewItem item)
    {
        if (item == null) return;

        // Note: Visual state animations would ideally be in XAML templates.
        // Since FluentJalium uses code-based theming, we set static properties here.
        // Full hover/pressed animations require template modification in Jalium.UI.

        // Fluent Design: Selected items have accent-colored left bar
        // This would need to be implemented in the control template

        // For now, we ensure the item has proper margins for the accent bar
        item.Margin = new Thickness(4, 2, 4, 2);
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
