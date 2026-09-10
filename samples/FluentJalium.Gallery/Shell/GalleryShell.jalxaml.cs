using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using FluentJalium.Gallery.Controls;
using FluentJalium.Gallery.Pages;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using FWAutoSuggestBox = FluentJalium.Controls.FWAutoSuggestBox;
using FWAutoSuggestBoxQuerySubmittedEventArgs = FluentJalium.Controls.FWAutoSuggestBoxQuerySubmittedEventArgs;

namespace FluentJalium.Gallery.Shell;

/// <summary>
/// Jalxaml-defined Gallery shell.
/// Supports dual navigation presentation styles (mirrors ModernWPF & WPF-UI multi-mode designs):
/// - <see cref="FluentNavigationItemStyle.Tree"/>: Canonical WinUI 3 hierarchical tree list.
/// - <see cref="FluentNavigationItemStyle.Fluent"/>: Modern Fluent card layout (Windows Store / Settings style).
/// </summary>
public sealed partial class GalleryShell : UserControl
{
    private const double ExpandedModeThresholdWidth = 1008;
    private const double CompactModeThresholdWidth = 641;

    private static FluentNavigationItemStyle s_activeSidebarStyle = FluentNavigationItemStyle.Tree;

    /// <summary>
    /// Gets or sets the globally active navigation sidebar style in the Gallery.
    /// Changing this property immediately notifies active shells to re-populate their nav tree.
    /// </summary>
    public static FluentNavigationItemStyle ActiveSidebarStyle
    {
        get => s_activeSidebarStyle;
        set
        {
            if (s_activeSidebarStyle == value) return;
            s_activeSidebarStyle = value;
            SidebarStyleChanged?.Invoke();
        }
    }

    public static event Action? SidebarStyleChanged;

    private readonly Window _owner;
    private readonly IReadOnlyList<GalleryEntry> _entries = GalleryPages.All;
    private readonly List<FWNavigationViewItem> _groupItems = new();
    private ContentControl _contentHost = null!;
    private FWNavigationView _navigation = null!;
    private FWAutoSuggestBox _searchBox = null!;
    private GalleryEntry? _current;

    public GalleryShell(Window owner)
    {
        _owner = owner;
        InitializeComponent();

        _contentHost = ContentHost;
        _navigation = Navigation;
        _searchBox = SearchBox;
        _searchBox.QuerySubmitted += OnSearchSubmitted;
        _navigation.SelectionChanged += OnSelectionChanged;
        FluentThemeManager.ThemeChanged += OnThemeChanged;
        SidebarStyleChanged += OnSidebarStyleChanged;
        Unloaded += OnShellUnloaded;

        ApplyCurrentSidebarStyle();
        PopulateNavigation();
        Navigate(_entries[0]);
        SelectItem(_entries[0].Key);
        _owner.SizeChanged += OnWindowSizeChanged;
    }

    private void ApplyCurrentSidebarStyle()
    {
        _navigation.ItemStyle = ActiveSidebarStyle;
    }

    private void PopulateNavigation()
    {
        _navigation.MenuItems.Clear();
        _navigation.FooterMenuItems.Clear();
        _groupItems.Clear();

        if (ActiveSidebarStyle == FluentNavigationItemStyle.Fluent)
        {
            // Modern Fluent Card Style (WPF-UI / Windows Store flat catalog):
            // All pages listed directly as comfortable 40px cards with generous icon padding.
            _navigation.MenuItems.Add(CreateLeaf(_entries[0]));
            _navigation.MenuItems.Add(new FWNavigationViewItemSeparator());
            _navigation.MenuItems.Add(new FWNavigationViewItemHeader { Content = "Controls" });

            foreach (var entry in _entries.Skip(1))
            {
                _navigation.MenuItems.Add(CreateLeaf(entry));
            }
        }
        else
        {
            // WinUI 3 Canonical Tree Style:
            // Home / "Controls" header / All / expandable category groups.
            _navigation.MenuItems.Add(CreateLeaf(_entries[0]));
            _navigation.MenuItems.Add(new FWNavigationViewItemHeader { Content = "Controls" });
            _navigation.MenuItems.Add(CreateLeaf(_entries[1]));

            var groups = _entries.Skip(2)
                .GroupBy(entry => entry.Group)
                .OrderBy(group => group.Key, StringComparer.Ordinal);

            foreach (var group in groups)
            {
                var children = group.ToList();
                if (children.Count == 1)
                {
                    _navigation.MenuItems.Add(CreateLeaf(children[0]));
                    continue;
                }

                var sectionEntry = new GalleryEntry(
                    "section:" + group.Key,
                    group.Key,
                    group.Key,
                    $"Browse the {group.Key} section.",
                    GalleryPages.GroupIcon(group.Key),
                    navigate => GalleryPages.SectionPage(group.Key, children, navigate));

                var parent = new FWNavigationViewItem
                {
                    Content = group.Key,
                    Icon = GalleryIcon.Create(sectionEntry.Icon, 16, Brush("TextFillColorPrimaryBrush")),
                    RouteKey = sectionEntry.Key,
                    Tag = sectionEntry
                };
                foreach (var child in children)
                {
                    parent.MenuItems.Add(CreateLeaf(child));
                }
                _groupItems.Add(parent);
                _navigation.MenuItems.Add(parent);
            }
        }

        // Settings in footer
        _navigation.FooterMenuItems.Add(new FWNavigationViewItem
        {
            Content = GalleryPages.Settings.Title,
            Icon = GalleryIcon.Create(GalleryPages.Settings.Icon, 16, Brush("TextFillColorPrimaryBrush")),
            RouteKey = GalleryPages.Settings.Key,
            Tag = GalleryPages.Settings
        });

        _navigation.UpdateMenuItems();
    }

    private static FWNavigationViewItem CreateLeaf(GalleryEntry entry) => new()
    {
        Content = entry.Title,
        Icon = GalleryIcon.Create(entry.Icon, 16, Brush("TextFillColorPrimaryBrush")),
        RouteKey = entry.Key,
        Tag = entry
    };

    private void SelectItem(string key)
    {
        foreach (var item in EnumerateItems())
        {
            if (item.Tag is GalleryEntry entry && entry.Key == key)
            {
                _navigation.SelectedItem = item;
                return;
            }
        }
    }

    private IEnumerable<FWNavigationViewItem> EnumerateItems()
    {
        foreach (var item in _navigation.MenuItems.OfType<FWNavigationViewItem>())
        {
            yield return item;
            foreach (var child in item.MenuItems.OfType<FWNavigationViewItem>()) yield return child;
        }
        foreach (var item in _navigation.FooterMenuItems.OfType<FWNavigationViewItem>())
        {
            yield return item;
        }
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
        _current = entry;
        _searchBox.Text = string.Empty;
        _contentHost.Content = entry.CreateContent(Navigate);
    }

    private void OnSearchSubmitted(object? sender, FWAutoSuggestBoxQuerySubmittedEventArgs e)
    {
        var query = e.QueryText?.Trim();
        if (string.IsNullOrWhiteSpace(query)) return;

        var match = _entries.Concat(new[] { GalleryPages.Settings }).FirstOrDefault(entry =>
            entry.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            entry.Description.Contains(query, StringComparison.OrdinalIgnoreCase));
        if (match is null) return;

        SelectItem(match.Key);
        Navigate(match);
    }

    private void OnWindowSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (e.NewSize.Width <= 0) return;

        if (e.NewSize.Width >= ExpandedModeThresholdWidth)
        {
            _navigation.PaneDisplayMode = NavigationViewPaneDisplayMode.Left;
            _navigation.IsPaneOpen = true;
        }
        else if (e.NewSize.Width >= CompactModeThresholdWidth)
        {
            _navigation.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftCompact;
            _navigation.IsPaneOpen = false;
        }
        else
        {
            _navigation.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal;
            _navigation.IsPaneOpen = false;
        }
    }

    private void OnSidebarStyleChanged()
    {
        ApplyCurrentSidebarStyle();
        PopulateNavigation();
        if (_current is not null)
        {
            SelectItem(_current.Key);
        }
    }

    private void OnThemeChanged()
    {
        PopulateNavigation();
        if (_current is not null)
        {
            SelectItem(_current.Key);
        }
    }

    private void OnShellUnloaded(object? sender, RoutedEventArgs e)
    {
        FluentThemeManager.ThemeChanged -= OnThemeChanged;
        SidebarStyleChanged -= OnSidebarStyleChanged;
        Unloaded -= OnShellUnloaded;
    }

    private static Brush Brush(string key) =>
        Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Brush brush
            ? brush
            : new SolidColorBrush(Colors.Transparent);
}
