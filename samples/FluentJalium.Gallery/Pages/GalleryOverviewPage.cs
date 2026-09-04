using FluentJalium.Gallery.Controls;
using FluentJalium.Gallery.Models;
using FluentJalium.Gallery.Resources;
using FluentJalium.Gallery.Services;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Automation;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using FWFluentMaterialKind = FluentJalium.Controls.FWFluentMaterialKind;
using FWFluentMaterialSurface = FluentJalium.Controls.FWFluentMaterialSurface;
using FWScrollViewer = FluentJalium.Controls.FWScrollViewer;
using FWSelectorBar = FluentJalium.Controls.FWSelectorBar;
using FWSelectorBarItem = FluentJalium.Controls.FWSelectorBarItem;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;

namespace FluentJalium.Gallery.Pages;

/// <summary>
/// WinUI Gallery Home: hero header, Recent/Favorites switcher, recently-visited rail,
/// recently-added-or-updated grid, and the favorites grid with its empty state.
/// Favoriting itself is wired from the item PageHeader in Phase 3; this page renders
/// whatever <see cref="GalleryFavoritesService"/> holds.
/// </summary>
internal sealed class GalleryOverviewPage
{
    internal sealed record GalleryHomeSnapshot(
        GalleryPageInfo[] Visited,
        GalleryPageInfo[] AddedOrUpdated,
        GalleryPageInfo[] Favorites);

    private readonly GalleryPageInfo[] _pages;
    private readonly Action<string> _navigate;
    private readonly GalleryLocalizationService _localization = new();

    public GalleryOverviewPage(GalleryPageInfo[] pageInfos, Action<string> navigate)
    {
        _pages = pageInfos;
        _navigate = navigate;
    }

    internal static GalleryHomeSnapshot CreateSnapshot(
        GalleryPageInfo[] pageInfos,
        IEnumerable<string> visitedIds,
        IEnumerable<string> favoriteIds,
        int maxVisited = 8,
        int maxUpdated = 12,
        int maxFavorites = 24)
    {
        ArgumentNullException.ThrowIfNull(pageInfos);
        ArgumentNullException.ThrowIfNull(visitedIds);
        ArgumentNullException.ThrowIfNull(favoriteIds);

        var byId = pageInfos.ToDictionary(page => page.UniqueId, StringComparer.Ordinal);

        GalleryPageInfo[] Resolve(IEnumerable<string> ids, int max)
        {
            return ids
                .Where(byId.ContainsKey)
                .Take(max)
                .Select(id => byId[id])
                .ToArray();
        }

        var addedOrUpdated = pageInfos
            .Where(page => !page.IsFooter && (page.IsNew || page.IsUpdated))
            .Take(maxUpdated)
            .ToArray();

        return new GalleryHomeSnapshot(
            Resolve(visitedIds, maxVisited),
            addedOrUpdated,
            Resolve(favoriteIds, maxFavorites));
    }

    public UIElement CreateContent()
    {
        var snapshot = CreateSnapshot(
            _pages,
            GalleryRecentSamplesService.Instance.GetRecent(20).Select(page => page.UniqueId),
            GalleryFavoritesService.Instance.GetFavorites());

        var root = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12
        };

        root.Children.Add(CreateHeroBanner());
        root.Children.Add(CreateFilterBar(out var recentItem, out var selector));
        root.Children.Add(CreateRecentPanel(snapshot));
        root.Children.Add(CreateFavoritesPanel(snapshot));

        var recentPanel = (UIElement)root.Children[^2];
        var favoritesPanel = (UIElement)root.Children[^1];
        favoritesPanel.Visibility = Visibility.Collapsed;

        selector.SelectionChanged += (_, _) =>
        {
            var showRecent = ReferenceEquals(selector.SelectedItem, recentItem);
            recentPanel.Visibility = showRecent ? Visibility.Visible : Visibility.Collapsed;
            favoritesPanel.Visibility = showRecent ? Visibility.Collapsed : Visibility.Visible;
        };

        return root;
    }

    private UIElement CreateHeroBanner()
    {
        var heroContent = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 16,
            Margin = new Thickness(40, 48, 40, 48)
        };

        heroContent.Children.Add(new FWTextBlock
        {
            Text = Strings.Overview_Title,
            FontSize = 40,
            FontFamily = "Segoe UI Variable Display",
            FontWeight = FontWeights.SemiBold,
            Foreground = ThemeBrush("TextPrimary")
        });

        heroContent.Children.Add(new FWTextBlock
        {
            Text = Strings.Overview_Subtitle,
            FontSize = 16,
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 600
        });

        var banner = new FWFluentMaterialSurface
        {
            MaterialKind = FWFluentMaterialKind.LiquidGlass,
            TintColor = Color.FromArgb(140, 0, 120, 212),
            TintOpacity = 0.14,
            BlurRadius = 18,
            RefractionAmount = 40,
            ChromaticAberration = 0.25,
            FusionRadius = 20,
            Background = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)),
            BorderBrush = ThemeBrush("ControlElevationBorderBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Shape = BorderShape.SuperEllipse,
            SuperEllipseN = 4,
            Child = heroContent
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Children =
            {
                banner,
                CreateQuickLinks()
            }
        };
    }

    private UIElement CreateQuickLinks()
    {
        (string Id, string BaseKey, int Glyph)[] links =
        [
            ("allcontrols", "home.tile.allcontrols", 0xE71D),
            ("newcontrols", "home.tile.newcontrols", 0xE7C9),
            ("updatedcontrols", "home.tile.updatedcontrols", 0xE823),
            ("colors", "home.tile.colors", 0xE790),
            ("textinput", "home.tile.textinput", 0xE8D2),
            ("settings", "home.tile.settings", 0xE713)
        ];

        var strip = new FWStackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12
        };

        foreach (var link in links)
        {
            var target = link.Id;
            var tile = new GalleryTile
            {
                Title = _localization.Text($"{link.BaseKey}.title"),
                Caption = _localization.Text($"{link.BaseKey}.caption"),
                Glyph = char.ConvertFromUtf32(link.Glyph)
            };
            tile.Activated += (_, _) => _navigate(target);
            strip.Children.Add(tile);
        }

        return new FWScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = strip
        };
    }

    private UIElement CreateFilterBar(out FWSelectorBarItem recentItem, out FWSelectorBar selector)
    {
        recentItem = new FWSelectorBarItem
        {
            Text = _localization.Text("home.recent"),
            Icon = GalleryGlyph.Create(FluentIconRegular.Clock24, 16, ThemeBrush("TextPrimary"))
        };
        var favoritesItem = new FWSelectorBarItem
        {
            Text = _localization.Text("home.favorites"),
            Icon = GalleryGlyph.Create(FluentIconRegular.Star24, 16, ThemeBrush("TextPrimary"))
        };

        selector = new FWSelectorBar
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(36, 24, 0, 16),
            SelectedIndex = 0
        };
        selector.Items.Add(recentItem);
        selector.Items.Add(favoritesItem);
        selector.SelectedItem = recentItem;
        return selector;
    }

    private UIElement CreateRecentPanel(GalleryHomeSnapshot snapshot)
    {
        var panel = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Margin = new Thickness(36, 0, 36, 36)
        };

        if (snapshot.Visited.Length > 0)
        {
            panel.Children.Add(CreateRailTitle(_localization.Text("home.recentlyVisited")));

            var strip = new FWStackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 12
            };
            foreach (var page in snapshot.Visited)
            {
                strip.Children.Add(CreateSampleCard(page));
            }

            var scroller = new FWScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Content = strip
            };
            AutomationProperties.SetAutomationId(scroller, "RecentlyVisitedGridView");
            panel.Children.Add(scroller);
        }

        if (snapshot.AddedOrUpdated.Length > 0)
        {
            panel.Children.Add(CreateRailTitle(_localization.Text("home.recentlyAddedOrUpdated")));

            var grid = new FWWrapPanel
            {
                HorizontalSpacing = 12,
                VerticalSpacing = 12
            };
            foreach (var page in snapshot.AddedOrUpdated)
            {
                grid.Children.Add(CreateSampleCard(page));
            }
            AutomationProperties.SetAutomationId(grid, "RecentlyAddedAndUpdatedGridView");
            panel.Children.Add(grid);
        }

        return panel;
    }

    private UIElement CreateFavoritesPanel(GalleryHomeSnapshot snapshot)
    {
        var panel = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Margin = new Thickness(36, 0, 36, 36)
        };

        if (snapshot.Favorites.Length > 0)
        {
            var grid = new FWWrapPanel
            {
                HorizontalSpacing = 12,
                VerticalSpacing = 12
            };
            foreach (var page in snapshot.Favorites)
            {
                grid.Children.Add(CreateSampleCard(page));
            }
            AutomationProperties.SetAutomationId(grid, "FavoriteSamplesGridView");
            panel.Children.Add(grid);
        }
        else
        {
            var fallback = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 8,
                Margin = new Thickness(24, 36, 24, 36),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            fallback.Children.Add(GalleryGlyph.Create(FluentIconRegular.Star24, 36, ThemeBrush("TextSecondary")));
            fallback.Children.Add(new FWTextBlock
            {
                Text = _localization.Text("home.noFavoritesTitle"),
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Foreground = ThemeBrush("TextPrimary"),
                HorizontalAlignment = HorizontalAlignment.Center
            });
            fallback.Children.Add(new FWTextBlock
            {
                Text = _localization.Text("home.noFavoritesHint"),
                FontSize = 14,
                Foreground = ThemeBrush("TextSecondary"),
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            });
            panel.Children.Add(fallback);
        }

        return panel;
    }

    private UIElement CreateSampleCard(GalleryPageInfo page)
    {
        var card = new GalleryControlCard
        {
            Title = page.Title,
            Subtitle = page.Group,
            Glyph = GalleryGlyph.Glyph(page.Icon)
        };
        var target = page.UniqueId;
        card.Activated += (_, _) => _navigate(target);
        return card;
    }

    private static UIElement CreateRailTitle(string text)
    {
        return new FWTextBlock
        {
            Text = text,
            FontSize = 16,
            FontWeight = FontWeights.SemiBold,
            Foreground = ThemeBrush("TextPrimary")
        };
    }

    private static Brush ThemeBrush(string key)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Brush brush)
        {
            return brush;
        }
        return new SolidColorBrush(Colors.Transparent);
    }
}
