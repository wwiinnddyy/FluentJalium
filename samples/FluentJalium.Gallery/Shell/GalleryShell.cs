using FluentJalium.Controls.Themes;
using FluentJalium.Gallery.Models;
using FluentJalium.Gallery.Services;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using FWFrame = FluentJalium.Controls.FWFrame;
using FWNavigationView = FluentJalium.Controls.FWNavigationView;
using FWNavigationViewItem = FluentJalium.Controls.FWNavigationViewItem;
using FWNavigationViewItemSeparator = FluentJalium.Controls.FWNavigationViewItemSeparator;
using FWTextBox = FluentJalium.Controls.FWTextBox;

namespace FluentJalium.Gallery.Shell;

internal sealed class GalleryShell : UserControl
{
    private readonly Window _owner;
    private readonly GalleryCatalogService _catalogService;
    private readonly Action<FluentThemeVariant> _applyTheme;
    private readonly Action<Color> _applyAccent;
    private readonly List<FWNavigationViewItem> _navigationItems = [];
    private GalleryPage[] _pages = [];
    private FWNavigationView? _navigationView;
    private FWFrame? _frame;
    private FWTextBox? _searchBox;
    private GalleryPage? _selectedPage;
    private string _navigationSearchText = string.Empty;

    public GalleryShell(
        Window owner,
        GalleryCatalogService catalogService,
        Action<FluentThemeVariant> applyTheme,
        Action<Color> applyAccent)
    {
        _owner = owner;
        _catalogService = catalogService;
        _applyTheme = applyTheme;
        _applyAccent = applyAccent;
        Content = BuildShell();
    }

    public void RefreshTheme()
    {
        _pages = _catalogService.CreatePages(_owner, _applyTheme, _applyAccent);

        if (_navigationView != null)
        {
            _navigationView.Background = GalleryThemeResources.Brush("WindowBackground");
            _navigationView.PaneBackground = GalleryThemeResources.Brush("NavigationViewPaneBackground");
            _navigationView.ContentBackground = GalleryThemeResources.Brush("NavigationViewContentBackground");
            _navigationView.PaneHeader = CreatePaneHeader();
            _navigationView.Content = CreateContentHost();
            PopulateNavigationItems(_navigationView, _pages, _navigationSearchText);
        }

        if (_selectedPage != null)
        {
            var refreshedPage = _pages.FirstOrDefault(page => page.Title == _selectedPage.Title) ?? _selectedPage;
            SelectPage(refreshedPage);
        }
        else
        {
            NavigateToEmptySearchState();
        }
    }

    private UIElement BuildShell()
    {
        _pages = _catalogService.CreatePages(_owner, _applyTheme, _applyAccent);
        _frame = new FWFrame
        {
            CacheSize = 1,
            Background = GalleryThemeResources.Brush("NavigationViewContentBackground")
        };
        _frame.Navigated += OnFrameNavigated;

        _navigationView = new FWNavigationView
        {
            Background = GalleryThemeResources.Brush("WindowBackground"),
            PaneBackground = GalleryThemeResources.Brush("NavigationViewPaneBackground"),
            ContentBackground = GalleryThemeResources.Brush("NavigationViewContentBackground"),
            PaneDisplayMode = NavigationViewPaneDisplayMode.Left,
            IsPaneOpen = true,
            OpenPaneLength = 320,
            CompactPaneLength = 48,
            PaneHeader = CreatePaneHeader(),
            Content = CreateContentHost()
        };
        _navigationView.SelectionChanged += OnNavigationSelectionChanged;

        PopulateNavigationItems(_navigationView, _pages, _navigationSearchText);
        if (_navigationItems.Count > 0 && _navigationItems[0].Tag is GalleryPage firstPage)
        {
            _navigationView.SelectedItem = _navigationItems[0];
            SelectPage(firstPage);
        }

        return _navigationView;
    }

    private UIElement CreateContentHost()
    {
        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Background = GalleryThemeResources.Brush("NavigationViewContentBackground"),
            Children =
            {
                CreateSearchHeader(),
                _frame!
            }
        };
    }

    private void PopulateNavigationItems(FWNavigationView navigationView, GalleryPage[] pages, string searchText)
    {
        _navigationItems.Clear();
        navigationView.MenuItems.Clear();
        navigationView.FooterMenuItems.Clear();

        var matchingPages = pages
            .Where(page => page.MatchesSearch(searchText))
            .ToArray();

        var homePage = matchingPages.FirstOrDefault(page => page.Group == GalleryNavigationGroup.Home);
        if (homePage != null)
        {
            navigationView.MenuItems.Add(CreateNavigationItem(homePage));
        }

        var groupedPages = GalleryNavigationGroup.Order
            .Select(groupName => new
            {
                GroupName = groupName,
                Pages = matchingPages
                    .Where(page => !page.IsFooter && page.Group == groupName)
                    .ToArray()
            })
            .Where(group => group.Pages.Length > 0)
            .ToArray();

        if (homePage != null && groupedPages.Length > 0)
        {
            navigationView.MenuItems.Add(new FWNavigationViewItemSeparator());
        }

        foreach (var group in groupedPages)
        {
            var groupItem = CreateNavigationGroupItem(group.GroupName);
            foreach (var page in group.Pages)
            {
                groupItem.MenuItems.Add(CreateNavigationItem(page));
            }

            navigationView.MenuItems.Add(groupItem);
        }

        foreach (var page in matchingPages.Where(page => page.IsFooter))
        {
            navigationView.FooterMenuItems.Add(CreateNavigationItem(page));
        }

        navigationView.UpdateMenuItems();
    }

    private static FWNavigationViewItem CreateNavigationGroupItem(string groupName)
    {
        return new FWNavigationViewItem
        {
            Content = groupName,
            Icon = CreateIcon(GalleryNavigationGroup.GetIcon(groupName)),
            IsExpanded = true,
            SelectsOnInvoked = false,
            Tag = groupName
        };
    }

    private FWNavigationViewItem CreateNavigationItem(GalleryPage page)
    {
        var item = new FWNavigationViewItem
        {
            Content = page.Title,
            Icon = CreateIcon(page.Icon),
            Tag = page
        };
        _navigationItems.Add(item);
        return item;
    }

    private UIElement CreatePaneHeader()
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Margin = new Thickness(0, 4, 0, 2)
        };

        panel.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Children =
            {
                CreateIcon(FluentIconRegular.WindowBrush24, 24),
                new TextBlock
                {
                    Text = "FluentJalium",
                    FontSize = 24,
                    FontFamily = "Segoe UI Variable Display",
                    Foreground = GalleryThemeResources.Brush("TextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        });
        panel.Children.Add(new TextBlock
        {
            Text = "Control gallery",
            FontSize = 12,
            Foreground = GalleryThemeResources.Brush("TextSecondary")
        });

        return panel;
    }

    private UIElement CreateSearchHeader()
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Margin = new Thickness(40, 24, 40, 0)
        };

        panel.Children.Add(CreateIcon(FluentIconRegular.Search24, 20, GalleryThemeResources.Brush("TextSecondary")));
        _searchBox = new FWTextBox
        {
            Text = _navigationSearchText,
            PlaceholderText = "Search controls, materials, and samples",
            MinHeight = 34,
            Width = 420,
            VerticalAlignment = VerticalAlignment.Center
        };
        _searchBox.TextChanged += OnNavigationSearchTextChanged;
        panel.Children.Add(_searchBox);
        return panel;
    }

    private void OnNavigationSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not FWTextBox textBox)
        {
            return;
        }

        var searchText = textBox.Text ?? string.Empty;
        if (string.Equals(_navigationSearchText, searchText, StringComparison.Ordinal))
        {
            return;
        }

        _navigationSearchText = searchText;
        RefreshNavigationForSearch();
    }

    private void RefreshNavigationForSearch()
    {
        if (_navigationView == null)
        {
            return;
        }

        var preferredPage = _selectedPage;
        PopulateNavigationItems(_navigationView, _pages, _navigationSearchText);

        if (preferredPage != null && TrySelectNavigationPage(preferredPage))
        {
            return;
        }

        if (_navigationItems.Count > 0 && _navigationItems[0].Tag is GalleryPage firstPage)
        {
            _navigationView.SelectedItem = _navigationItems[0];
            SelectPage(firstPage);
            return;
        }

        _navigationView.SelectedItem = null;
        _selectedPage = null;
        NavigateToEmptySearchState();
    }

    private bool TrySelectNavigationPage(GalleryPage page)
    {
        if (_navigationView == null)
        {
            return false;
        }

        var item = _navigationItems.FirstOrDefault(navigationItem =>
            navigationItem.Tag is GalleryPage candidate && candidate.Title == page.Title);
        if (item == null)
        {
            return false;
        }

        if (item.Tag is GalleryPage selectedPage)
        {
            _navigationView.SelectedItem = item;
            SelectPage(selectedPage);
        }

        return true;
    }

    private void OnNavigationSelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs e)
    {
        if (e.SelectedItem?.Tag is GalleryPage page)
        {
            SelectPage(page);
        }
    }

    private void SelectPage(GalleryPage page)
    {
        _selectedPage = page;
        _frame?.Navigate(typeof(GalleryHostPage), page);
    }

    private void NavigateToEmptySearchState()
    {
        _frame?.Navigate(typeof(GallerySearchEmptyPage), _navigationSearchText);
    }

    private void OnFrameNavigated(object? sender, NavigationEventArgs e)
    {
        if (e.Content is GalleryHostPage hostPage)
        {
            hostPage.RefreshTheme();
        }
    }

    private static FluentIcon CreateIcon(FluentIconRegular icon, double size = FluentIcon.DefaultSize, Brush? foreground = null)
    {
        return FluentIconFactory.Regular(icon, size, foreground ?? GalleryThemeResources.Brush("TextPrimary"));
    }
}
