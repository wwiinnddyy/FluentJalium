namespace FluentJalium.Controls;

/// <summary>
/// Specifies the presentation layout style of items in the NavigationView sidebar.
/// Mirrors ModernWPF and WPF-UI multi-mode navigation architectures.
/// </summary>
public enum FluentNavigationItemStyle
{
    /// <summary>
    /// Standard WinUI 3 hierarchical tree list item (36px min height, 16px icon beside text, 3x16 left pill).
    /// </summary>
    Tree = 0,

    /// <summary>
    /// Modern Fluent card / tile style (WPF-UI LeftFluent / Windows Store / Settings style):
    /// 40px height with rounded card background, generous icon padding, and subtle hover animations.
    /// </summary>
    Fluent = 1,

    /// <summary>
    /// Large icon-over-text tile item (60x60px, 24px icon on top, caption underneath, 3x24 indicator pill).
    /// Ideal for dashboard and media navigation shells.
    /// </summary>
    Tile = 2
}
