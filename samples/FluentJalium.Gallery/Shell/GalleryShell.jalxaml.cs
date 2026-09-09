using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using FluentJalium.Gallery.Controls;
using FluentJalium.Gallery.Pages;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Automation;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using FWAutoSuggestBox = FluentJalium.Controls.FWAutoSuggestBox;
using FWAutoSuggestBoxQuerySubmittedEventArgs = FluentJalium.Controls.FWAutoSuggestBoxQuerySubmittedEventArgs;

namespace FluentJalium.Gallery.Shell;

/// <summary>
/// Jalxaml-defined Gallery shell. Markup owns the visual tree; code-behind only supplies the
/// catalog and navigation behavior, mirroring WinUI Gallery's separation of chrome and content.
/// </summary>
public sealed partial class GalleryShell : UserControl
{
    private readonly Window _owner;
    private readonly IReadOnlyList<GalleryEntry> _entries = GalleryPages.All;
    private ContentControl _contentHost = null!;
    private FWNavigationView _navigation = null!;
    private FWAutoSuggestBox _searchBox = null!;

    public GalleryShell(Window owner)
    {
        _owner = owner;
        InitializeComponent();

        _contentHost = ContentHost;
        _navigation = Navigation;
        _searchBox = SearchBox;
        _searchBox.QuerySubmitted += OnSearchSubmitted;
        _navigation.SelectionChanged += OnSelectionChanged;
        ThemeSwitch.Toggled += OnThemeToggled;

        PopulateNavigation();
        _navigation.SelectedItem = _navigation.MenuItems.OfType<FWNavigationViewItem>().First();
        Navigate(_entries[0]);
        _owner.SizeChanged += OnWindowSizeChanged;
    }

    private void PopulateNavigation()
    {
        string? lastGroup = null;
        foreach (var entry in _entries)
        {
            if (lastGroup is not null && !string.Equals(lastGroup, entry.Group, StringComparison.Ordinal))
            {
                _navigation.MenuItems.Add(new FWNavigationViewItemSeparator());
            }

            if (lastGroup is null || !string.Equals(lastGroup, entry.Group, StringComparison.Ordinal))
            {
                _navigation.MenuItems.Add(new FWNavigationViewItemHeader { Content = entry.Group });
            }

            _navigation.MenuItems.Add(new FWNavigationViewItem
            {
                Content = entry.Title,
                Icon = GalleryIcon.Create(entry.Icon, 20, Brush("TextFillColorPrimaryBrush")),
                RouteKey = entry.Key,
                Tag = entry
            });
            lastGroup = entry.Group;
        }

        _navigation.FooterMenuItems.Add(new FWNavigationViewItem
        {
            Content = "About FluentJalium",
            Icon = GalleryIcon.Create(FluentIconRegular.BookInformation24, 20, Brush("TextFillColorPrimaryBrush")),
            Tag = GalleryPages.About
        });
        _navigation.UpdateMenuItems();
    }

    private void OnSelectionChanged(object? sender, FluentNavigationViewSelectionChangedEventArgs e)
    {
        if (e.SelectedItem is FWNavigationViewItem { Tag: GalleryEntry entry })
        {
            Navigate(entry);
        }
    }

    private void Navigate(GalleryEntry entry)
    {
        _searchBox.Text = string.Empty;
        _contentHost.Content = entry.CreateContent(Navigate);
    }

    private void OnSearchSubmitted(object? sender, FWAutoSuggestBoxQuerySubmittedEventArgs e)
    {
        var query = e.QueryText?.Trim();
        if (string.IsNullOrWhiteSpace(query)) return;

        var match = _entries.FirstOrDefault(entry =>
            entry.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            entry.Description.Contains(query, StringComparison.OrdinalIgnoreCase));
        if (match is null) return;

        if (_navigation.MenuItems.OfType<FWNavigationViewItem>().FirstOrDefault(item => item.Tag is GalleryEntry entry && entry.Key == match.Key) is { } item)
        {
            _navigation.SelectedItem = item;
        }
        Navigate(match);
    }

    private void OnWindowSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        var compact = e.NewSize.Width < 1040;
        _navigation.IsPaneOpen = !compact;
        _navigation.PaneDisplayMode = compact
            ? NavigationViewPaneDisplayMode.LeftCompact
            : NavigationViewPaneDisplayMode.Left;
    }

    private void OnThemeToggled(object? sender, RoutedEventArgs e)
    {
        FluentThemeManager.ApplyTheme(ThemeSwitch.IsOn ? FluentThemeVariant.Dark : FluentThemeVariant.Light);
        ThemeSwitch.Header = ThemeSwitch.IsOn ? "Dark theme" : "Light theme";
    }

    private static Brush Brush(string key) =>
        Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Brush brush
            ? brush
            : new SolidColorBrush(Colors.Transparent);
}
