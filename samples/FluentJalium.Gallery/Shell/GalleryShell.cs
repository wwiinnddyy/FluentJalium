using System.ComponentModel;
using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using FluentJalium.Gallery.Controls;
using FluentJalium.Gallery.Models;
using FluentJalium.Gallery.Services;
using FluentJalium.Gallery.Resources;
using FluentJalium.Gallery.Styles;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Automation;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using Jalium.UI.Navigation;
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
    private TextBlock? _currentPageText;
    private Button? _goBackButton;
    private readonly GalleryLocalizationService _localization = new();
    private GalleryPage? _selectedPage;

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
        GalleryNavigationBroker.NavigateRequested += NavigateByUniqueId;

        Content = BuildShell();

        _owner.SizeChanged += OnOwnerSizeChanged;

        if (System.Environment.GetEnvironmentVariable("FJ_NO_TIP") != "1" && GalleryFirstRunService.Instance.IsFirstRun)
        {
            Loaded += (_, _) => ShowFirstRunTip();
        }
    }

    public void RefreshTheme()
    {
        _pages = _catalogService.CreatePages(_owner, _applyTheme, _applyAccent, NavigateByUniqueId);

        if (_navigationView != null)
        {
            _navigationView.Background = new SolidColorBrush(Colors.Transparent);
            _navigationView.PaneBackground = GalleryThemeResources.Brush("FluentMaterialShellPaneBrush");
            _navigationView.ContentBackground = new SolidColorBrush(Colors.Transparent);

            // PaneHeader and the content host are built once: re-assigning Content left the
            // previous host in the tree, so the page rendered twice.
            PopulateNavigationItems(_navigationView, _pages);
        }

        if (_selectedPage != null)
        {
            var refreshedPage = _pages.FirstOrDefault(page => page.Title == _selectedPage.Title) ?? _selectedPage;
            SelectPage(refreshedPage);
        }
        else if (_navigationView != null && _navigationItems.Count > 0 && _navigationItems[0].Tag is GalleryPage firstPage)
        {
            _navigationView.SelectedItem = _navigationItems[0];
            SelectPage(firstPage);
        }
    }

    private UIElement BuildShell()
    {
        _pages = _catalogService.CreatePages(_owner, _applyTheme, _applyAccent, NavigateByUniqueId);
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
            PaneBackground = GalleryThemeResources.Brush("FluentMaterialShellPaneBrush"),
            ContentBackground = new SolidColorBrush(Colors.Transparent),
            PaneDisplayMode = NavigationViewPaneDisplayMode.Left,
            IsPaneOpen = true,
            OpenPaneLength = 320,
            CompactPaneLength = 48,
            PaneHeader = CreatePaneHeader(),
            Content = CreateContentHost()
        };
        _navigationView.SelectionChanged += OnNavigationSelectionChanged;

        PopulateNavigationItems(_navigationView, _pages);
        if (_navigationItems.Count > 0 && _navigationItems[0].Tag is GalleryPage firstPage)
        {
            _navigationView.SelectedItem = _navigationItems[0];
            SelectPage(firstPage);
        }

        var shellRoot = new Grid();
        shellRoot.Children.Add(_navigationView);
        shellRoot.Children.Add(_snackbarHost);
        shellRoot.Children.Add(CreateAutomationHelper(out _currentPageText, out _goBackButton));
        return shellRoot;
    }

    /// <summary>
    /// Hidden UIA helpers mirroring WinUI Gallery's AutomationHelpers panel: zero-size and
    /// hit-test invisible (never collapsed) so automation can read the current page and drive back.
    /// </summary>
    private UIElement CreateAutomationHelper(out TextBlock currentPageText, out Button goBackButton)
    {
        currentPageText = new TextBlock
        {
            Width = 0,
            Height = 0,
            Opacity = 0,
            IsHitTestVisible = false
        };
        AutomationProperties.SetAutomationId(currentPageText, "__CurrentPage");

        goBackButton = new Button
        {
            Width = 0,
            Height = 0,
            Opacity = 0,
            IsHitTestVisible = false,
            IsTabStop = false
        };
        AutomationProperties.SetAutomationId(goBackButton, "__GoBackInvoker");
        goBackButton.Click += (_, _) =>
        {
            if (_frame?.CanGoBack == true)
            {
                _frame.GoBack();
            }
        };

        var panel = new StackPanel
        {
            Width = 0,
            Height = 0,
            Opacity = 0,
            IsHitTestVisible = false,
            Orientation = Orientation.Vertical
        };
        panel.Children.Add(currentPageText);
        panel.Children.Add(goBackButton);
        return panel;
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

    private void PopulateNavigationItems(FWNavigationView navigationView, GalleryPage[] pages)
    {
        _navigationItems.Clear();
        navigationView.MenuItems.Clear();
        navigationView.FooterMenuItems.Clear();

        var homePage = pages.FirstOrDefault(page => page.GroupId == GalleryNavigationGroup.Home);
        if (homePage != null)
        {
            navigationView.MenuItems.Add(CreateNavigationItem(homePage));
        }

        var groupedPages = GalleryNavigationGroup.Order
            .Select(groupId => new
            {
                GroupId = groupId,
                Pages = pages
                    .Where(page => !page.IsFooter && page.GroupId == groupId)
                    .ToArray()
            })
            .Where(group => group.Pages.Length > 0)
            .ToArray();

        var controlsHeaderInserted = false;
        foreach (var group in groupedPages)
        {
            if (!controlsHeaderInserted &&
                string.Equals(group.GroupId, GalleryNavigationGroup.FirstControlsGroup, StringComparison.Ordinal))
            {
                navigationView.MenuItems.Add(new FWNavigationViewItemSeparator());
                navigationView.MenuItems.Add(new FWNavigationViewItemHeader
                {
                    Content = _localization.Text("shell.controlsHeader")
                });
                controlsHeaderInserted = true;
            }

            var localizedGroupName = group.Pages.First().Group;
            var groupItem = CreateNavigationGroupItem(localizedGroupName, group.GroupId);
            foreach (var page in group.Pages)
            {
                groupItem.MenuItems.Add(CreateNavigationItem(page));
            }

            navigationView.MenuItems.Add(groupItem);
        }

        foreach (var page in pages.Where(page => page.IsFooter))
        {
            navigationView.FooterMenuItems.Add(CreateNavigationItem(page));
        }

        navigationView.UpdateMenuItems();
    }

    private FWNavigationViewItem CreateNavigationGroupItem(string localizedGroupName, string groupId)
    {
        var item = new FWNavigationViewItem
        {
            Content = localizedGroupName,
            Icon = CreateIcon(GalleryNavigationGroup.GetIcon(groupId)),
            IsExpanded = true,
            SelectsOnInvoked = false,
            Tag = groupId
        };
        item.Invoked += (_, _) => NavigateToSection(groupId);
        return item;
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
            Margin = new Thickness(36, 24, 36, 0)
        };

        _searchBox = new FWAutoSuggestBox
        {
            PlaceholderText = Strings.Shell_SearchPlaceholder,
            MinHeight = 34,
            Width = 580,
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
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return;
        }

        NavigateToSearchResults(searchText);
    }

    private void NavigateByUniqueId(string uniqueId)
    {
        var page = _pages.FirstOrDefault(candidate => string.Equals(candidate.UniqueId, uniqueId, StringComparison.Ordinal));
        if (page != null)
        {
            NavigateToPage(page);
        }
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

    private void NavigateToSearchResults(string query)
    {
        if (_currentPageText != null)
        {
            _currentPageText.Text = query;
        }
        _frame?.Navigate(typeof(GallerySearchResultsPage), query);
    }

    private void NavigateToSection(string groupId)
    {
        if (_currentPageText != null)
        {
            _currentPageText.Text = groupId;
        }
        _frame?.Navigate(typeof(GallerySectionPage), groupId);
    }

    private void OnNavigationSelectionChanged(object? sender, FluentNavigationViewSelectionChangedEventArgs e)
    {
        if (e.SelectedItem is FWNavigationViewItem item && item.Tag is GalleryPage page)
        {
            SelectPage(page);
        }
    }

    private void SelectPage(GalleryPage page)
    {
        // Setting SelectedItem re-enters here through SelectionChanged; navigating again would
        // leave the frame's cached page and the new one both on screen. Compare by UniqueId as
        // well: RefreshTheme rebuilds page objects, and re-navigating to the same sample must
        // never stack a second copy in the frame.
        if (_selectedPage is not null &&
            (ReferenceEquals(_selectedPage, page) ||
             string.Equals(_selectedPage.UniqueId, page.UniqueId, StringComparison.Ordinal)))
        {
            return;
        }

        _selectedPage = page;
        GalleryRecentSamplesService.Instance.RecordVisit(page);
        if (_currentPageText != null)
        {
            _currentPageText.Text = page.UniqueId;
        }
        _frame?.Navigate(typeof(GalleryItemHostPage), page);
    }

    private void OnFrameNavigated(object? sender, NavigationEventArgs e)
    {
        if (e.Content is GalleryItemHostPage itemPage)
        {
            itemPage.ApplyNavigationParameter(e.ExtraData);
        }

        if (e.Content is GalleryHostPage hostPage)
        {
            hostPage.ApplyNavigationParameter(e.ExtraData);
        }

        if (e.Content is GallerySearchResultsPage searchResultsPage)
        {
            searchResultsPage.ApplyNavigationParameter(e.ExtraData);
        }

        if (e.Content is GallerySectionPage sectionPage)
        {
            sectionPage.ApplyNavigationParameter(e.ExtraData);
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
        return GalleryGlyph.Create(icon, size, foreground ?? GalleryThemeResources.Brush("TextPrimary"));
    }
}

internal sealed record SearchSuggestion(FluentIconRegular Icon, string Title, string Group, GalleryPage Page)
{
    public override string ToString() => Title;
}
