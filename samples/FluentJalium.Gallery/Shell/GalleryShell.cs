using System.ComponentModel;
using FluentJalium.Controls.Themes;
using FluentJalium.Gallery.Controls;
using FluentJalium.Gallery.Models;
using FluentJalium.Gallery.Services;
using FluentJalium.Gallery.Resources;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using FWAutoSuggestBox = FluentJalium.Controls.FWAutoSuggestBox;
using FWAutoSuggestBoxTextChangedEventArgs = FluentJalium.Controls.FWAutoSuggestBoxTextChangedEventArgs;
using FWAutoSuggestBoxSuggestionChosenEventArgs = FluentJalium.Controls.FWAutoSuggestBoxSuggestionChosenEventArgs;
using FWAutoSuggestBoxQuerySubmittedEventArgs = FluentJalium.Controls.FWAutoSuggestBoxQuerySubmittedEventArgs;
using FWAutoSuggestBoxTextChangeReason = FluentJalium.Controls.FWAutoSuggestBoxTextChangeReason;
using Jalium.UI.Input;
using FWFrame = FluentJalium.Controls.FWFrame;
using FWGrid = FluentJalium.Controls.FWGrid;
using FWNavigationView = FluentJalium.Controls.FWNavigationView;
using FWNavigationViewItem = FluentJalium.Controls.FWNavigationViewItem;
using FWNavigationViewItemSeparator = FluentJalium.Controls.FWNavigationViewItemSeparator;
using FWSnackbarHost = FluentJalium.Controls.FWSnackbarHost;
using FWSnackbarPlacement = FluentJalium.Controls.FWSnackbarPlacement;
using FWTransitioningContentControl = FluentJalium.Controls.FWTransitioningContentControl;
using FWContentTransitionProfile = FluentJalium.Controls.FWContentTransitionProfile;
using FWTeachingTip = FluentJalium.Controls.FWTeachingTip;
using TeachingTipPlacementMode = FluentJalium.Controls.TeachingTipPlacementMode;

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
    private FWTransitioningContentControl? _transitionHost;
    private FWAutoSuggestBox? _searchBox;
    private FWSnackbarHost? _snackbarHost;
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

        LocalizationService.Instance.PropertyChanged += (s, e) =>
        {
            RefreshTheme();
        };

        Content = BuildShell();

        _owner.SizeChanged += OnOwnerSizeChanged;

        if (GalleryFirstRunService.Instance.IsFirstRun)
        {
            Loaded += (_, _) => ShowFirstRunTip();
        }
    }

    public void RefreshTheme()
    {
        _pages = _catalogService.CreatePages(_owner, _applyTheme, _applyAccent);

        if (_navigationView != null)
        {
            _navigationView.Background = new SolidColorBrush(Colors.Transparent);
            _navigationView.PaneBackground = new SolidColorBrush(Color.FromArgb(200, 0xF3, 0xF3, 0xF3));
            _navigationView.ContentBackground = new SolidColorBrush(Colors.Transparent);
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
            Background = new SolidColorBrush(Colors.Transparent)
        };
        _frame.Navigated += OnFrameNavigated;

        _transitionHost = new FWTransitioningContentControl
        {
            TransitionProfile = FWContentTransitionProfile.Entrance
        };

        _snackbarHost = new FWSnackbarHost
        {
            Placement = FWSnackbarPlacement.Bottom,
            MaxVisibleSnackbars = 1
        };
        GalleryFeedback.SetHost(_snackbarHost);

        _navigationView = new FWNavigationView
        {
            Background = new SolidColorBrush(Colors.Transparent),
            PaneBackground = new SolidColorBrush(Color.FromArgb(200, 0xF3, 0xF3, 0xF3)),
            ContentBackground = new SolidColorBrush(Colors.Transparent),
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

        var shellRoot = new Grid();
        shellRoot.Children.Add(_navigationView);
        shellRoot.Children.Add(_snackbarHost);
        return shellRoot;
    }

    private UIElement CreateContentHost()
    {
        var searchHeader = CreateSearchHeader();
        var frame = _frame!;
        var transitionHost = _transitionHost!;
        transitionHost.Content = frame;

        var host = new FWGrid
        {
            Background = new SolidColorBrush(Colors.Transparent),
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star }
            }
        };

        Grid.SetRow(searchHeader, 0);
        Grid.SetRow(transitionHost, 1);
        host.Children.Add(searchHeader);
        host.Children.Add(transitionHost);
        return host;
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
            .Select(groupId => new
            {
                GroupId = groupId,
                Pages = matchingPages
                    .Where(page => !page.IsFooter && page.GroupId == groupId)
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
            var localizedGroupName = group.Pages.First().Group;
            var groupItem = CreateNavigationGroupItem(localizedGroupName, group.GroupId);
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

    private static FWNavigationViewItem CreateNavigationGroupItem(string localizedGroupName, string groupId)
    {
        return new FWNavigationViewItem
        {
            Content = localizedGroupName,
            Icon = CreateIcon(GalleryNavigationGroup.GetIcon(groupId)),
            IsExpanded = true,
            SelectsOnInvoked = false,
            Tag = groupId
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
                    Text = Strings.Shell_Title,
                    FontSize = 24,
                    FontFamily = "Segoe UI Variable Display",
                    Foreground = GalleryThemeResources.Brush("TextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        });
        panel.Children.Add(new TextBlock
        {
            Text = Strings.Shell_Subtitle,
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
        _searchBox = new FWAutoSuggestBox
        {
            Text = _navigationSearchText,
            PlaceholderText = Strings.Shell_SearchPlaceholder,
            MinHeight = 34,
            Width = 420,
            VerticalAlignment = VerticalAlignment.Center
        };
        _searchBox.AutoSuggestTextChanged += OnAutoSuggestTextChanged;
        _searchBox.SuggestionChosen += OnSearchSuggestionChosen;
        _searchBox.QuerySubmitted += OnSearchQuerySubmitted;
        panel.Children.Add(_searchBox);
        return panel;
    }

    private void OnAutoSuggestTextChanged(object sender, FWAutoSuggestBoxTextChangedEventArgs e)
    {
        if (sender is not FWAutoSuggestBox suggestBox)
        {
            return;
        }

        var searchText = suggestBox.Text ?? string.Empty;

        if (e.Reason == FWAutoSuggestBoxTextChangeReason.SuggestionChosen)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(searchText))
        {
            suggestBox.ItemsSource = null;
            _navigationSearchText = string.Empty;
            RefreshNavigationForSearch();
            return;
        }

        var suggestions = _pages
            .Where(page => page.MatchesSearch(searchText))
            .Take(8)
            .Select(page => new SearchSuggestion(page.Icon, page.Title, page.Group, page))
            .ToArray();

        suggestBox.ItemsSource = suggestions;
    }

    private void OnSearchSuggestionChosen(object sender, FWAutoSuggestBoxSuggestionChosenEventArgs e)
    {
        if (e.SelectedItem is SearchSuggestion suggestion)
        {
            NavigateToPage(suggestion.Page);
        }
    }

    private void OnSearchQuerySubmitted(object sender, FWAutoSuggestBoxQuerySubmittedEventArgs e)
    {
        if (e.ChosenSuggestion is SearchSuggestion suggestion)
        {
            NavigateToPage(suggestion.Page);
            return;
        }

        var searchText = e.QueryText ?? string.Empty;
        if (string.Equals(_navigationSearchText, searchText, StringComparison.Ordinal))
        {
            return;
        }

        _navigationSearchText = searchText;
        RefreshNavigationForSearch();
    }

    private void NavigateToPage(GalleryPage page)
    {
        if (_navigationView == null) return;

        var item = _navigationItems.FirstOrDefault(ni => ni.Tag is GalleryPage p && p.Title == page.Title);
        if (item != null)
        {
            _navigationView.SelectedItem = item;
            SelectPage(page);
        }
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
        GalleryRecentSamplesService.Instance.RecordVisit(page);
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

        if (_transitionHost != null && e.Content is UIElement contentElement)
        {
            _transitionHost.ApplyTransitionProfile(FWContentTransitionProfile.Entrance);
        }
    }

    private void OnOwnerSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_navigationView == null) return;

        if (e.NewSize.Width < 980)
        {
            _navigationView.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftCompact;
            _navigationView.IsPaneOpen = false;
        }
        else
        {
            _navigationView.PaneDisplayMode = NavigationViewPaneDisplayMode.Left;
            _navigationView.IsPaneOpen = true;
        }
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        var modifiers = Keyboard.Modifiers;

        if (modifiers == ModifierKeys.Control && e.Key == Key.F)
        {
            _searchBox?.Focus();
            e.Handled = true;
            return;
        }

        if (modifiers == ModifierKeys.Alt)
        {
            if (e.Key == Key.Left && _frame != null && _frame.CanGoBack)
            {
                _frame.GoBack();
                e.Handled = true;
                return;
            }
            if (e.Key == Key.Right && _frame != null && _frame.CanGoForward)
            {
                _frame.GoForward();
                e.Handled = true;
                return;
            }
        }
    }

    private void ShowFirstRunTip()
    {
        if (_searchBox == null) return;

        var tip = new FWTeachingTip
        {
            Target = _searchBox,
            Title = "Quick Search",
            Subtitle = "Press Ctrl+F at any time to focus the search box and find controls instantly.",
            IsLightDismissEnabled = true,
            PreferredPlacement = TeachingTipPlacementMode.Bottom
        };
        tip.Closed += (_, _) =>
        {
            GalleryFirstRunService.Instance.MarkCompleted();
            ShowSecondTip();
        };
        tip.IsOpen = true;

        var root = Content as Grid;
        root?.Children.Add(tip);
    }

    private void ShowSecondTip()
    {
        if (_navigationView == null) return;

        var tip = new FWTeachingTip
        {
            Title = "Browse by Category",
            Subtitle = "Use the navigation pane to explore controls grouped by Design, Input, Layout, Collections, and more.",
            IsLightDismissEnabled = true,
            PreferredPlacement = TeachingTipPlacementMode.Right
        };
        tip.Closed += (_, _) =>
        {
            ShowThirdTip();
        };
        tip.IsOpen = true;

        var root = Content as Grid;
        root?.Children.Add(tip);
    }

    private void ShowThirdTip()
    {
        var tip = new FWTeachingTip
        {
            Title = "Theme & Accent",
            Subtitle = "Switch between Light, Dark, and High Contrast themes, and customize the accent color from the Settings page.",
            IsLightDismissEnabled = true,
            PreferredPlacement = TeachingTipPlacementMode.Auto
        };
        tip.IsOpen = true;

        var root = Content as Grid;
        root?.Children.Add(tip);
    }

    private static FluentIcon CreateIcon(FluentIconRegular icon, double size = FluentIcon.DefaultSize, Brush? foreground = null)
    {
        return FluentIconFactory.Regular(icon, size, foreground ?? GalleryThemeResources.Brush("TextPrimary"));
    }
}

internal sealed record SearchSuggestion(FluentIconRegular Icon, string Title, string Group, GalleryPage Page)
{
    public override string ToString() => Title;
}
