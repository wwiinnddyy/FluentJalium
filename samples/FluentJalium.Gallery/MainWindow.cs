using FluentJalium.Controls.Themes;
using FluentJalium.Gallery.Models;
using FluentJalium.Gallery.Pages;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using FWNavigationView = FluentJalium.Controls.FWNavigationView;
using FWNavigationViewItem = FluentJalium.Controls.FWNavigationViewItem;
using FWNavigationViewItemSeparator = FluentJalium.Controls.FWNavigationViewItemSeparator;
using FWScrollViewer = FluentJalium.Controls.FWScrollViewer;
using FWTextBox = FluentJalium.Controls.FWTextBox;

namespace FluentJalium.Gallery;

public sealed class MainWindow : Window
{
    private readonly List<FWNavigationViewItem> _navigationItems = [];
    private GalleryPage[] _pages = [];
    private FWNavigationView? _navigationView;
    private FWScrollViewer? _contentScrollViewer;
    private FWTextBox? _searchBox;
    private GalleryPage? _selectedPage;
    private string _navigationSearchText = string.Empty;
    private bool _isShowingSearchEmptyState;

    public MainWindow()
    {
        Title = "FluentJalium Gallery";
        Width = 1280;
        Height = 820;
        MinWidth = 980;
        MinHeight = 620;
        Background = ThemeBrush("WindowBackground");
        Content = BuildShell();
    }

    private UIElement BuildShell()
    {
        _pages = GalleryCatalog.Create(CreateGalleryPageContentFactories());
        _contentScrollViewer = new FWScrollViewer
        {
            Background = ThemeBrush("NavigationViewContentBackground"),
            Padding = new Thickness(0),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            IsScrollBarAutoHideEnabled = true
        };

        _navigationView = new FWNavigationView
        {
            Background = ThemeBrush("WindowBackground"),
            PaneBackground = ThemeBrush("NavigationViewPaneBackground"),
            ContentBackground = ThemeBrush("NavigationViewContentBackground"),
            PaneDisplayMode = NavigationViewPaneDisplayMode.Left,
            IsPaneOpen = true,
            OpenPaneLength = 320,
            CompactPaneLength = 48,
            PaneHeader = CreatePaneHeader(),
            Content = _contentScrollViewer
        };
        _navigationView.SelectionChanged += OnNavigationSelectionChanged;

        PopulateNavigationItems(_navigationView, _pages, _navigationSearchText);
        _navigationView.SelectedItem = _navigationItems[0];
        SelectPage(_pages[0]);
        return _navigationView;
    }

    private GalleryPageContentFactories CreateGalleryPageContentFactories()
    {
        return new GalleryPageContentFactories(
            Overview: () => CreatePageStack(new GalleryOverviewPage(ApplyTheme, ApplyAccent).CreateContent()),
            Geometry: () => CreatePageStack(new GalleryGeometryPage().CreateContent()),
            Buttons: () => CreatePageStack(new GalleryButtonsPage().CreateContent()),
            Switches: () => CreatePageStack(new GallerySwitchesPage().CreateContent()),
            TextInput: () => CreatePageStack(new GalleryTextInputPage().CreateContent()),
            Selection: () => CreatePageStack(new GallerySelectionPage().CreateContent()),
            Range: () => CreatePageStack(new GalleryRangePage().CreateContent()),
            DateAndTime: () => CreatePageStack(new GalleryDateTimePage().CreateContent()),
            ContentAndLayout: () => CreatePageStack(new GalleryContentLayoutPage().CreateContent()),
            Visuals: () => CreatePageStack(new GalleryVisualsPage().CreateContent()),
            Interaction: () => CreatePageStack(new GalleryInteractionPage().CreateContent()),
            InputAndMedia: () => CreatePageStack(new GalleryInputMediaPage().CreateContent()),
            Collections: () => CreatePageStack(new GalleryCollectionsPage().CreateContent()),
            SelectorsAndProperties: () => CreatePageStack(new GallerySelectorsPropertiesPage().CreateContent()),
            DataInspectors: () => CreatePageStack(new GalleryDataInspectorsPage().CreateContent()),
            Navigation: () => CreatePageStack(new GalleryNavigationPage().CreateContent()),
            WindowBackdrops: () => CreatePageStack(new GalleryWindowBackdropsPage(this).CreateContent()),
            MaterialsAndEffects: () => CreatePageStack(new GalleryMaterialsPage().CreateContent()),
            MotionAndTransitions: () => CreatePageStack(new GalleryMotionPage().CreateContent()),
            Menus: () => CreatePageStack(new GalleryMenusPage().CreateContent()),
            Disclosure: () => CreatePageStack(new GalleryDisclosurePage().CreateContent()),
            Status: () => CreatePageStack(new GalleryStatusPage().CreateContent()),
            StateMatrix: () => CreatePageStack(new GalleryStateMatrixPage().CreateContent()));
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
                    Foreground = ThemeBrush("TextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        });
        panel.Children.Add(new TextBlock
        {
            Text = "Control gallery",
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary")
        });

        _searchBox = new FWTextBox
        {
            Text = _navigationSearchText,
            PlaceholderText = "Search controls and samples",
            MinHeight = 32,
            Margin = new Thickness(0, 2, 0, 0)
        };
        _searchBox.TextChanged += OnNavigationSearchTextChanged;
        panel.Children.Add(_searchBox);
        return panel;
    }

    private UIElement CreatePageStack(params UIElement[] sections)
    {
        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 22
        };

        foreach (var section in sections)
        {
            stack.Children.Add(section);
        }

        return stack;
    }

    private UIElement CreatePageContent(GalleryPage page)
    {
        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 18,
            Margin = new Thickness(40, 32, 40, 40)
        };

        stack.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Children =
            {
                CreateIcon(page.Icon, 30),
                new TextBlock
                {
                    Text = page.Title,
                    FontSize = 30,
                    FontFamily = "Segoe UI Variable Display",
                    Foreground = ThemeBrush("TextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        });
        stack.Children.Add(new TextBlock
        {
            Text = page.Description,
            FontSize = 14,
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        });
        stack.Children.Add(page.CreateContent());
        return stack;
    }

    private UIElement CreateSearchEmptyContent(string searchText)
    {
        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Margin = new Thickness(40, 36, 40, 40)
        };

        stack.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Children =
            {
                CreateIcon(FluentIconRegular.SearchInfo24, 28),
                new TextBlock
                {
                    Text = "No results",
                    FontSize = 28,
                    FontFamily = "Segoe UI Variable Display",
                    Foreground = ThemeBrush("TextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        });
        stack.Children.Add(new TextBlock
        {
            Text = string.IsNullOrWhiteSpace(searchText)
                ? "No Gallery pages are available."
                : $"No Gallery pages match \"{searchText.Trim()}\".",
            FontSize = 14,
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        });

        return stack;
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
        _isShowingSearchEmptyState = true;
        if (_contentScrollViewer != null)
        {
            _contentScrollViewer.Content = CreateSearchEmptyContent(_navigationSearchText);
        }
    }

    private bool TrySelectNavigationPage(GalleryPage page)
    {
        if (_navigationView == null)
        {
            return false;
        }

        var item = _navigationItems.FirstOrDefault(navigationItem => ReferenceEquals(navigationItem.Tag, page));
        if (item == null)
        {
            return false;
        }

        _navigationView.SelectedItem = item;
        SelectPage(page);
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
        _isShowingSearchEmptyState = false;

        if (_contentScrollViewer != null)
        {
            _contentScrollViewer.Content = CreatePageContent(page);
        }
    }

    private void RefreshCurrentPage()
    {
        Background = ThemeBrush("WindowBackground");

        if (_navigationView != null)
        {
            _navigationView.Background = ThemeBrush("WindowBackground");
            _navigationView.PaneBackground = ThemeBrush("NavigationViewPaneBackground");
            _navigationView.ContentBackground = ThemeBrush("NavigationViewContentBackground");
            _navigationView.PaneHeader = CreatePaneHeader();
        }
        if (_contentScrollViewer != null)
        {
            _contentScrollViewer.Background = ThemeBrush("NavigationViewContentBackground");

            if (_isShowingSearchEmptyState)
            {
                _contentScrollViewer.Content = CreateSearchEmptyContent(_navigationSearchText);
            }
            else if (_selectedPage != null)
            {
                _contentScrollViewer.Content = CreatePageContent(_selectedPage);
            }
        }
    }

    private void ApplyTheme(FluentThemeVariant theme)
    {
        FluentThemeManager.ApplyTheme(theme);
        RefreshCurrentPage();
    }

    private void ApplyAccent(Color accent)
    {
        FluentThemeManager.ApplyAccent(accent);
        RefreshCurrentPage();
    }

    private static Brush ThemeBrush(string key)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Brush brush)
        {
            return brush;
        }

        return new SolidColorBrush(Colors.Transparent);
    }

    private static FluentIcon CreateIcon(FluentIconRegular icon, double size = FluentIcon.DefaultSize, Brush? foreground = null)
    {
        return FluentIconFactory.Regular(icon, size, foreground ?? ThemeBrush("TextPrimary"));
    }

}
