using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Themes;
using Jalium.UI.Documents;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using JaliumThemeManager = Jalium.UI.Controls.Themes.ThemeManager;

namespace FluentJalium.Tests;

[Collection("Application")]
public sealed class FluentNavigationControlsTests
{
    private static readonly string[] NavigationResourceKeys =
    [
        "NavigationViewPaneBackground",
        "NavigationViewContentBackground",
        "NavigationViewItemBackground",
        "NavigationViewItemBackgroundHover",
        "NavigationViewItemBackgroundPressed",
        "NavigationViewItemBackgroundSelected",
        "NavigationViewItemBackgroundSelectedHover",
        "NavigationViewItemForeground",
        "NavigationViewItemForegroundSecondary",
        "NavigationViewItemForegroundDisabled",
        "TabStripBackground",
        "TabStripBorder",
        "TabContentBackground",
        "TabItemHoverBackground",
        "TabItemSelectedBackground",
        "TabItemIndicator",
        "FrameBackground",
        "FrameBorderBrush"
    ];

    [Theory]
    [InlineData(FluentThemeVariant.Dark)]
    [InlineData(FluentThemeVariant.Light)]
    [InlineData(FluentThemeVariant.HighContrast)]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void ThemeDictionary_ShouldExposeNavigationTokensForEveryTheme(FluentThemeVariant theme)
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = theme.ToString();

        var dictionary = LoadGenericThemeDictionary();

        foreach (var key in NavigationResourceKeys)
        {
            Assert.True(dictionary.TryGetValue(key, out var value), $"{key} was not found in the {theme} theme.");
            Assert.IsType<SolidColorBrush>(value);
        }

        ResetApplicationState();
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void Apply_ShouldRegisterFwNavigationStylesBasedOnJaliumStyles()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            AssertBasedOnStyle<FWNavigationView, NavigationView>(app.Resources);
            AssertBasedOnStyle<FWNavigationViewItem, NavigationViewItem>(app.Resources);
            AssertBasedOnStyle<FWNavigationViewItemHeader, NavigationViewItemHeader>(app.Resources);
            AssertBasedOnStyle<FWNavigationViewItemSeparator, NavigationViewItemSeparator>(app.Resources);
            AssertStyle<FWBreadcrumbBar>(app.Resources);
            AssertStyle<FWPipsPager>(app.Resources);
            AssertBasedOnStyle<FWTabControl, TabControl>(app.Resources);
            AssertBasedOnStyle<FWTabItem, TabItem>(app.Resources);
            AssertStyle<FWTabView>(app.Resources);
            AssertBasedOnStyle<FWTabViewItem, TabItem>(app.Resources);
            AssertStyle<FWSelectorBar>(app.Resources);
            AssertStyle<FWSelectorBarItem>(app.Resources);
            AssertBasedOnStyle<FWFrame, Frame>(app.Resources);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void GenericTheme_ShouldDefineNavigationBaseStylesAndFluentSetters()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = FluentThemeVariant.Dark.ToString();

        var dictionary = LoadGenericThemeDictionary();

        var navigationViewStyle = AssertStyle<NavigationView>(dictionary);
        AssertSetter(navigationViewStyle, Control.BackgroundProperty);
        AssertSetter(navigationViewStyle, NavigationView.PaneBackgroundProperty);
        AssertSetter(navigationViewStyle, NavigationView.ContentBackgroundProperty);

        var fwNavigationViewStyle = AssertStyle<FWNavigationView>(dictionary);
        Assert.Equal(typeof(NavigationView), fwNavigationViewStyle.BasedOn?.TargetType);
        AssertSetter(fwNavigationViewStyle, FWNavigationView.DensityProperty);

        var navigationItemStyle = AssertStyle<NavigationViewItem>(dictionary);
        AssertSetter(navigationItemStyle, Control.BackgroundProperty);
        AssertSetter(navigationItemStyle, TextElement.ForegroundProperty);
        AssertSetter(navigationItemStyle, Control.CornerRadiusProperty);
        AssertSetter(navigationItemStyle, FrameworkElement.MarginProperty);

        var fwNavigationItemStyle = AssertStyle<FWNavigationViewItem>(dictionary);
        Assert.Equal(typeof(NavigationViewItem), fwNavigationItemStyle.BasedOn?.TargetType);
        AssertSetter(fwNavigationItemStyle, FWNavigationViewItem.DensityProperty);

        var breadcrumbBarStyle = AssertStyle<FWBreadcrumbBar>(dictionary);
        AssertSetter(breadcrumbBarStyle, FWBreadcrumbBar.DensityProperty);
        AssertSetter(breadcrumbBarStyle, FWBreadcrumbBar.MaxItemsProperty);

        var pipsPagerStyle = AssertStyle<FWPipsPager>(dictionary);
        AssertSetter(pipsPagerStyle, FWPipsPager.NumberOfPagesProperty);
        AssertSetter(pipsPagerStyle, FWPipsPager.MaxVisiblePipsProperty);
        AssertSetter(pipsPagerStyle, FWPipsPager.PreviousButtonVisibilityProperty);
        AssertSetter(pipsPagerStyle, FWPipsPager.NextButtonVisibilityProperty);

        var tabControlStyle = AssertStyle<TabControl>(dictionary);
        AssertSetter(tabControlStyle, Control.BackgroundProperty);
        AssertSetter(tabControlStyle, TabControl.TabStripBackgroundProperty);
        AssertSetter(tabControlStyle, TabControl.TabStripBorderBrushProperty);
        AssertSetter(tabControlStyle, TabControl.TabStripHeightProperty);

        var fwTabControlStyle = AssertStyle<FWTabControl>(dictionary);
        Assert.Equal(typeof(TabControl), fwTabControlStyle.BasedOn?.TargetType);
        AssertSetter(fwTabControlStyle, FWTabControl.DensityProperty);

        var tabItemStyle = AssertStyle<TabItem>(dictionary);
        var fwTabItemStyle = AssertStyle<FWTabItem>(dictionary);
        Assert.Equal(typeof(TabItem), fwTabItemStyle.BasedOn?.TargetType);
        AssertSetter(fwTabItemStyle, FWTabItem.DensityProperty);

        var tabViewStyle = AssertStyle<FWTabView>(dictionary);
        AssertSetter(tabViewStyle, FWTabView.DensityProperty);
        AssertSetter(tabViewStyle, FWTabView.TabWidthModeProperty);
        AssertSetter(tabViewStyle, FWTabView.CloseButtonOverlayModeProperty);
        AssertTargetTriggerSetter(tabViewStyle, FWTabView.TabStripPlacementProperty, Jalium.UI.Controls.Dock.Bottom, "PART_TabStrip", Grid.RowProperty, 1);
        AssertTargetTriggerSetter(tabViewStyle, FWTabView.TabStripPlacementProperty, Jalium.UI.Controls.Dock.Bottom, "PART_SelectedContentPresenter", Grid.RowProperty, 0);
        AssertTargetTriggerSetter(tabViewStyle, FWTabView.TabStripPlacementProperty, Jalium.UI.Controls.Dock.Bottom, "PART_SelectedContentPresenter", FrameworkElement.MarginProperty, new Thickness(0, 0, 0, 8));

        var fwTabViewItemStyle = AssertStyle<FWTabViewItem>(dictionary);
        Assert.Equal(typeof(TabItem), fwTabViewItemStyle.BasedOn?.TargetType);
        AssertSetter(fwTabViewItemStyle, FWTabViewItem.IsClosableProperty);

        var selectorBarStyle = AssertStyle<FWSelectorBar>(dictionary);
        AssertSetter(selectorBarStyle, FWSelectorBar.DensityProperty);
        AssertSetter(selectorBarStyle, FWSelectorBar.OrientationProperty);
        AssertSetter(selectorBarStyle, FWSelectorBar.SelectionIndicatorPlacementProperty);

        var selectorBarItemStyle = AssertStyle<FWSelectorBarItem>(dictionary);
        AssertSetter(selectorBarItemStyle, Control.PaddingProperty);
        AssertSetter(selectorBarItemStyle, Control.BackgroundProperty);
        AssertSetter(selectorBarItemStyle, Control.BorderBrushProperty);

        var frameStyle = AssertStyle<Frame>(dictionary);
        AssertSetter(frameStyle, Control.BackgroundProperty);
        AssertSetter(frameStyle, Control.BorderBrushProperty);

        var fwFrameStyle = AssertStyle<FWFrame>(dictionary);
        Assert.Equal(typeof(Frame), fwFrameStyle.BasedOn?.TargetType);

        ResetApplicationState();
    }

    [Fact]
    public void FWNavigationControls_ShouldApplyDensityPresets()
    {
        var navigationView = new FWNavigationView();

        Assert.Equal(FWNavigationDensity.Comfortable, navigationView.Density);
        Assert.Equal(280, navigationView.OpenPaneLength);
        Assert.Equal(48, navigationView.CompactPaneLength);

        navigationView.Density = FWNavigationDensity.Compact;

        Assert.Equal(240, navigationView.OpenPaneLength);
        Assert.Equal(40, navigationView.CompactPaneLength);

        navigationView.Density = FWNavigationDensity.Spacious;

        Assert.Equal(320, navigationView.OpenPaneLength);
        Assert.Equal(56, navigationView.CompactPaneLength);

        var item = new FWNavigationViewItem();

        Assert.Equal(FWNavigationDensity.Comfortable, item.Density);
        Assert.Equal(36, item.MinHeight);
        Assert.Equal(new Thickness(6, 2, 6, 2), item.Margin);

        item.Density = FWNavigationDensity.Compact;

        Assert.Equal(32, item.MinHeight);
        Assert.Equal(new Thickness(4, 1, 4, 1), item.Margin);

        item.Density = FWNavigationDensity.Spacious;

        Assert.Equal(44, item.MinHeight);
        Assert.Equal(new Thickness(8, 2, 8, 2), item.Margin);

        var tabControl = new FWTabControl();

        Assert.Equal(FWNavigationDensity.Comfortable, tabControl.Density);
        Assert.Equal(40, tabControl.TabStripHeight);

        tabControl.Density = FWNavigationDensity.Compact;

        Assert.Equal(36, tabControl.TabStripHeight);

        var tabItem = new FWTabItem
        {
            Density = FWNavigationDensity.Spacious
        };

        Assert.Equal(44, tabItem.MinHeight);
        Assert.Equal(new Thickness(18, 12, 18, 12), tabItem.Padding);

        tabItem.Density = FWNavigationDensity.Compact;

        Assert.Equal(32, tabItem.MinHeight);
        Assert.Equal(new Thickness(12, 7, 12, 7), tabItem.Padding);
    }

    [Fact]
    public void FWNavigationViewItem_ShouldExposeRouteKey()
    {
        var item = new FWNavigationViewItem
        {
            Content = "Overview",
            RouteKey = "overview"
        };

        Assert.Equal("overview", item.RouteKey);
        Assert.Equal("overview", item.GetValue(FWNavigationViewItem.RouteKeyProperty));

        item.SetValue(FWNavigationViewItem.RouteKeyProperty, "details");

        Assert.Equal("details", item.RouteKey);
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises FWNavigationService Frame page activation by type.")]
    public void FWNavigationService_ShouldNavigateSelectionAndSynchronizeHistory()
    {
        var navigationView = new FWNavigationView
        {
            IsBackButtonVisible = NavigationViewBackButtonVisible.Visible
        };
        var overviewItem = new FWNavigationViewItem
        {
            Content = "Overview",
            RouteKey = "overview"
        };
        var detailsItem = new FWNavigationViewItem
        {
            Content = "Details"
        };
        navigationView.MenuItems.Add(overviewItem);
        navigationView.MenuItems.Add(detailsItem);

        var frame = new FWFrame();
        var service = new FWNavigationService();
        var navigatedRoutes = new List<string>();
        service.Navigated += (_, route) => navigatedRoutes.Add(route.RouteKey);
        var overviewRoute = service.RegisterRoute(overviewItem, typeof(NavigationOverviewPage), "overview-parameter");
        var detailsRoute = service.RegisterRoute(detailsItem, typeof(NavigationDetailsPage), "details-parameter");

        service.Attach(navigationView, frame);
        navigationView.SelectedItem = overviewItem;

        Assert.Equal("overview", overviewRoute.RouteKey);
        Assert.Equal("Details", detailsRoute.RouteKey);
        Assert.Equal("Overview", overviewRoute.Item?.Content);
        Assert.Equal(typeof(NavigationOverviewPage), frame.SourcePageType);
        Assert.Equal("overview-parameter", frame.CurrentPage!.NavigationParameter);
        Assert.Equal("overview", service.CurrentRouteKey);
        Assert.False(navigationView.IsBackEnabled);
        Assert.Equal(["overview"], navigatedRoutes);

        navigationView.SelectedItem = detailsItem;

        Assert.Equal(typeof(NavigationDetailsPage), frame.SourcePageType);
        Assert.Equal("details-parameter", frame.CurrentPage!.NavigationParameter);
        Assert.Equal("Details", service.CurrentRouteKey);
        Assert.True(navigationView.IsBackEnabled);
        Assert.Equal(1, frame.BackStackDepth);

        Assert.True(service.GoBack());

        Assert.Equal(typeof(NavigationOverviewPage), frame.SourcePageType);
        Assert.Equal(overviewItem, navigationView.SelectedItem);
        Assert.Equal("overview", service.CurrentRouteKey);
        Assert.False(navigationView.IsBackEnabled);
        Assert.True(frame.CanGoForward);

        Assert.True(service.GoForward());

        Assert.Equal(typeof(NavigationDetailsPage), frame.SourcePageType);
        Assert.Equal(detailsItem, navigationView.SelectedItem);
        Assert.Equal("Details", service.CurrentRouteKey);

        var diagnostics = service.GetDiagnostics();
        Assert.True(diagnostics.IsAttached);
        Assert.Equal(2, diagnostics.RouteCount);
        Assert.Equal("Details", diagnostics.CurrentRouteKey);
        Assert.Equal(typeof(NavigationDetailsPage), diagnostics.CurrentPageType);
        Assert.True(diagnostics.CanGoBack);
        Assert.False(diagnostics.CanGoForward);
        Assert.Equal(1, diagnostics.BackStackDepth);
        Assert.False(diagnostics.IsSynchronizingSelection);
        Assert.Equal(["overview", "Details", "overview", "Details"], navigatedRoutes);
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises FWNavigationService Frame page activation by type.")]
    public void FWNavigationService_ShouldDetachAndUnregisterRoutes()
    {
        var navigationView = new FWNavigationView();
        var item = new FWNavigationViewItem
        {
            Content = "Overview",
            RouteKey = "overview"
        };
        var frame = new FWFrame();
        var service = new FWNavigationService();
        service.RegisterRoute(item, typeof(NavigationOverviewPage));

        service.Attach(navigationView, frame);

        Assert.True(service.NavigateToRoute("overview"));
        Assert.Equal("overview", service.CurrentRouteKey);
        Assert.True(service.UnregisterRoute("overview"));
        Assert.Null(service.CurrentRouteKey);
        Assert.False(service.NavigateToRoute("overview"));

        service.RegisterRoute(item, typeof(NavigationOverviewPage));
        Assert.True(service.NavigateToRoute("overview"));

        service.Detach();

        Assert.False(service.IsAttached);
        Assert.Null(service.CurrentRouteKey);
        Assert.False(service.NavigateToRoute("overview"));
        Assert.False(service.GoBack());
        Assert.False(service.GoForward());
    }

    [Fact]
    public void FWBreadcrumbBar_ShouldExposeDensityItemsAndMaxItemsState()
    {
        var breadcrumbBar = new FWBreadcrumbBar
        {
            ItemsSource = new[] { "Home", "Library", "Docs", "Controls" },
            MaxItems = 3
        };

        Assert.Equal(FWNavigationDensity.Comfortable, breadcrumbBar.Density);
        Assert.Equal(36, breadcrumbBar.MinHeight);
        Assert.Equal(new Thickness(10, 6, 10, 6), breadcrumbBar.Padding);
        Assert.Equal(3, breadcrumbBar.MaxItems);
        Assert.NotNull(breadcrumbBar.ItemsSource);

        breadcrumbBar.Density = FWNavigationDensity.Compact;

        Assert.Equal(32, breadcrumbBar.MinHeight);
        Assert.Equal(new Thickness(8, 4, 8, 4), breadcrumbBar.Padding);

        breadcrumbBar.Density = FWNavigationDensity.Spacious;

        Assert.Equal(44, breadcrumbBar.MinHeight);
        Assert.Equal(new Thickness(12, 8, 12, 8), breadcrumbBar.Padding);
    }

    [Fact]
    public void FWPipsPager_ShouldCoerceSelectionAndExposeNavigationState()
    {
        var pager = new FWPipsPager
        {
            NumberOfPages = 4,
            MaxVisiblePips = 3
        };
        var changes = new List<(int OldIndex, int NewIndex)>();
        pager.SelectedIndexChanged += (_, args) => changes.Add((args.OldIndex, args.NewIndex));

        pager.SelectedPageIndex = 2;

        Assert.Equal(2, pager.SelectedPageIndex);
        Assert.Single(changes);
        Assert.Equal((0, 2), changes[0]);

        pager.SelectedPageIndex = 99;

        Assert.Equal(3, pager.SelectedPageIndex);
        Assert.Equal((2, 3), changes[^1]);

        pager.NumberOfPages = 2;

        Assert.Equal(2, pager.NumberOfPages);
        Assert.Equal(1, pager.SelectedPageIndex);
        Assert.Equal(3, pager.MaxVisiblePips);

        pager.PreviousButtonVisibility = PipsPagerButtonVisibility.VisibleOnPointerOver;
        pager.NextButtonVisibility = PipsPagerButtonVisibility.Collapsed;
        pager.Orientation = Orientation.Vertical;

        Assert.Equal(PipsPagerButtonVisibility.VisibleOnPointerOver, pager.PreviousButtonVisibility);
        Assert.Equal(PipsPagerButtonVisibility.Collapsed, pager.NextButtonVisibility);
        Assert.Equal(Orientation.Vertical, pager.Orientation);
    }

    [Fact]
    public void FWTabView_ShouldExposeSelectionAddCloseAndDensityState()
    {
        var overview = new FWTabViewItem
        {
            Header = "Overview",
            Content = "Overview content",
            Icon = "Home"
        };
        var details = new FWTabViewItem
        {
            Header = "Details",
            Content = "Details content"
        };
        var tabView = new FWTabView
        {
            Header = "Workspaces",
            Footer = "Actions",
            Density = FWNavigationDensity.Spacious,
            TabStripPlacement = Jalium.UI.Controls.Dock.Bottom,
            TabWidthMode = FWTabViewWidthMode.SizeToContent,
            CloseButtonOverlayMode = FWTabViewCloseButtonOverlayMode.Always,
            CanReorderTabs = true
        };
        var selectionChanged = 0;
        var addRequested = 0;
        var closeRequested = 0;
        tabView.SelectionChanged += (_, _) => selectionChanged++;
        tabView.AddTabButtonClick += (_, args) =>
        {
            addRequested++;
            args.NewItem = new FWTabViewItem
            {
                Header = "New",
                Content = "New content",
                IsClosable = false
            };
        };
        tabView.TabCloseRequested += (_, args) =>
        {
            closeRequested++;
            Assert.Same(details, args.Tab);
            Assert.Equal(1, args.Index);
        };

        tabView.Items.Add(overview);
        tabView.Items.Add(details);
        tabView.SelectedIndex = 0;
        tabView.SelectTab(details);

        Assert.Equal("Workspaces", tabView.Header);
        Assert.Equal("Actions", tabView.Footer);
        Assert.Equal(FWNavigationDensity.Spacious, tabView.Density);
        Assert.Equal(52, tabView.MinHeight);
        Assert.Equal(new Thickness(16, 10, 16, 10), tabView.Padding);
        Assert.Equal(Jalium.UI.Controls.Dock.Bottom, tabView.TabStripPlacement);
        Assert.Equal(FWTabViewWidthMode.SizeToContent, tabView.TabWidthMode);
        Assert.Equal(FWTabViewCloseButtonOverlayMode.Always, tabView.CloseButtonOverlayMode);
        Assert.True(tabView.CanReorderTabs);
        Assert.True(details.IsSelected);
        Assert.False(overview.IsSelected);
        Assert.Equal("Details content", tabView.SelectedContent);

        Assert.True(tabView.RequestCloseTab(details));

        Assert.Single(tabView.Items);
        Assert.Equal(overview, tabView.SelectedItem);
        Assert.True(overview.IsSelected);
        Assert.Equal(1, closeRequested);

        Assert.True(tabView.RequestAddTab());

        Assert.Equal(2, tabView.Items.Count);
        Assert.Equal("New content", tabView.SelectedContent);
        Assert.IsType<FWTabViewItem>(tabView.SelectedItem);
        Assert.False(((FWTabViewItem)tabView.SelectedItem!).IsClosable);
        Assert.Equal(1, addRequested);
        Assert.True(selectionChanged >= 3);
    }

    [Fact]
    public void FWTabView_ShouldNotCloseNonClosableTabs()
    {
        var locked = new FWTabViewItem
        {
            Header = "Pinned",
            Content = "Pinned content",
            IsClosable = false
        };
        var tabView = new FWTabView();
        var closeRequested = 0;
        tabView.TabCloseRequested += (_, _) => closeRequested++;
        tabView.Items.Add(locked);
        tabView.SelectedItem = locked;

        Assert.False(tabView.RequestCloseTab(locked));

        Assert.Single(tabView.Items);
        Assert.Same(locked, tabView.SelectedItem);
        Assert.Equal("Pinned content", tabView.SelectedContent);
        Assert.Equal(0, closeRequested);
    }

    [Fact]
    public void FWSelectorBar_ShouldExposeSelectionAndDensityState()
    {
        var overview = new FWSelectorBarItem
        {
            Text = "Overview",
            Icon = "Home"
        };
        var activity = new FWSelectorBarItem
        {
            Text = "Activity"
        };
        var selectorBar = new FWSelectorBar
        {
            Orientation = Orientation.Vertical,
            Density = FWNavigationDensity.Compact,
            SelectionIndicatorPlacement = FWSelectorBarSelectionIndicatorPlacement.Left
        };
        var selectionChanged = 0;
        selectorBar.SelectionChanged += (_, _) => selectionChanged++;
        selectorBar.Items.Add(overview);
        selectorBar.Items.Add(activity);

        selectorBar.SelectedIndex = 0;
        selectorBar.SelectItem(activity);

        Assert.Equal(Orientation.Vertical, selectorBar.Orientation);
        Assert.Equal(FWNavigationDensity.Compact, selectorBar.Density);
        Assert.Equal(32, selectorBar.MinHeight);
        Assert.Equal(new Thickness(4), selectorBar.Padding);
        Assert.Equal(FWSelectorBarSelectionIndicatorPlacement.Left, selectorBar.SelectionIndicatorPlacement);
        Assert.Equal("Overview", overview.Text);
        Assert.Equal("Home", overview.Icon);
        Assert.Same(activity, selectorBar.SelectedItem);
        Assert.False(overview.IsSelected);
        Assert.True(activity.IsSelected);
        Assert.Equal(2, selectionChanged);
    }

    [Fact]
    public void FWSelectorBar_ShouldCreateDefaultItemsPanelFromOrientation()
    {
        var selectorBar = new TestSelectorBar
        {
            Orientation = Orientation.Vertical
        };

        var verticalPanel = Assert.IsType<StackPanel>(selectorBar.CreateDefaultItemsPanel());

        Assert.Equal(Orientation.Vertical, verticalPanel.Orientation);
        Assert.Equal(4, verticalPanel.Spacing);

        selectorBar.Orientation = Orientation.Horizontal;

        var horizontalPanel = Assert.IsType<StackPanel>(selectorBar.CreateDefaultItemsPanel());

        Assert.Equal(Orientation.Horizontal, horizontalPanel.Orientation);
        Assert.Equal(4, horizontalPanel.Spacing);
    }

    [Fact]
    public void FWNavigationView_ShouldExposePaneHeaderFooterModesAndSelectionEvents()
    {
        var navigationView = new FWNavigationView
        {
            PaneTitle = "FluentJalium",
            PaneHeader = new FWTextBox { Text = "Search" },
            PaneFooter = new FWTextBlock { Text = "vNext" },
            PaneDisplayMode = NavigationViewPaneDisplayMode.Left,
            IsBackButtonVisible = NavigationViewBackButtonVisible.Visible,
            IsBackEnabled = true,
            OpenPaneLength = 220,
            CompactPaneLength = 48
        };
        var home = new FWNavigationViewItem { Content = "Home" };
        var controls = new FWNavigationViewItem { Content = "Controls", IsExpanded = true };
        var documentOptions = new FWNavigationViewItem
        {
            Content = "Document options",
            SelectsOnInvoked = false
        };
        documentOptions.MenuItems.Add(new FWNavigationViewItem { Content = "Create new" });
        var settings = new FWNavigationViewItem { Content = "Settings" };
        var itemInvoked = 0;
        var selectionChanged = 0;
        var opened = 0;
        var closed = 0;

        navigationView.ItemInvoked += (_, _) => itemInvoked++;
        navigationView.SelectionChanged += (_, _) => selectionChanged++;
        navigationView.PaneOpened += (_, _) => opened++;
        navigationView.PaneClosed += (_, _) => closed++;
        navigationView.MenuItems.Add(home);
        navigationView.MenuItems.Add(controls);
        navigationView.MenuItems.Add(documentOptions);
        navigationView.FooterMenuItems.Add(settings);
        navigationView.SelectedItem = home;
        navigationView.SelectedItem = controls;
        InvokeNavigationItem(navigationView, documentOptions);
        navigationView.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftCompact;
        navigationView.IsPaneOpen = false;
        navigationView.PaneDisplayMode = NavigationViewPaneDisplayMode.Top;
        navigationView.IsPaneOpen = true;

        Assert.Equal("FluentJalium", navigationView.PaneTitle);
        Assert.IsType<FWTextBox>(navigationView.PaneHeader);
        Assert.IsType<FWTextBlock>(navigationView.PaneFooter);
        Assert.Single(navigationView.FooterMenuItems);
        Assert.Equal(NavigationViewPaneDisplayMode.Top, navigationView.PaneDisplayMode);
        Assert.Equal(NavigationViewBackButtonVisible.Visible, navigationView.IsBackButtonVisible);
        Assert.True(navigationView.IsBackEnabled);
        Assert.True(documentOptions.IsExpanded);
        Assert.Equal(3, itemInvoked);
        Assert.Equal(2, selectionChanged);
        Assert.Equal(controls, navigationView.SelectedItem);
        Assert.Equal(1, closed);
        Assert.Equal(1, opened);
    }

    [Fact]
    public void FWTabControl_ShouldExposePlacementSwipeAndDisabledTabState()
    {
        var first = new FWTabItem
        {
            Header = "Overview",
            Content = "Overview content"
        };
        var second = new FWTabItem
        {
            Header = "Details",
            Content = "Details content"
        };
        var disabled = new FWTabItem
        {
            Header = "Disabled",
            Content = "Disabled content",
            IsEnabled = false
        };
        var tabControl = new FWTabControl
        {
            TabStripPlacement = Jalium.UI.Controls.Dock.Left,
            IsSwipeEnabled = false
        };
        tabControl.Items.Add(first);
        tabControl.Items.Add(second);
        tabControl.Items.Add(disabled);

        tabControl.SelectedIndex = 1;
        tabControl.TabStripPlacement = Jalium.UI.Controls.Dock.Bottom;
        tabControl.IsSwipeEnabled = true;

        Assert.Equal(Jalium.UI.Controls.Dock.Bottom, tabControl.TabStripPlacement);
        Assert.True(tabControl.IsSwipeEnabled);
        Assert.True(second.IsSelected);
        Assert.False(first.IsSelected);
        Assert.False(disabled.IsEnabled);
        Assert.Equal("Details content", tabControl.SelectedContent);
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises Frame page activation by type.")]
    public void FWFrame_ShouldNavigateWithBackForwardStackAndCache()
    {
        var frame = new FWFrame
        {
            CacheSize = 2
        };
        var navigated = 0;
        frame.Navigated += (_, _) => navigated++;

        Assert.True(frame.Navigate(typeof(NavigationOverviewPage), "overview"));
        Assert.True(frame.Navigate(typeof(NavigationDetailsPage), "details"));
        Assert.Equal(typeof(NavigationDetailsPage), frame.SourcePageType);
        Assert.True(frame.CanGoBack);
        Assert.Equal(1, frame.BackStackDepth);

        Assert.True(frame.GoBack());
        Assert.Equal(typeof(NavigationOverviewPage), frame.SourcePageType);
        Assert.True(frame.CanGoForward);
        Assert.Equal("overview", frame.CurrentPage!.NavigationParameter);

        Assert.True(frame.GoForward());
        Assert.Equal(typeof(NavigationDetailsPage), frame.SourcePageType);
        Assert.Equal("details", frame.CurrentPage!.NavigationParameter);
        Assert.Equal(4, navigated);
    }

    [Fact]
    public void FWNavigationControls_ShouldExposeMaterialShellState()
    {
        var overviewItem = new FWNavigationViewItem { Content = "Overview" };
        var controlsItem = new FWNavigationViewItem { Content = "Controls" };
        var settingsItem = new FWNavigationViewItem { Content = "Settings" };
        var tabControl = new FWTabControl
        {
            IsSwipeEnabled = true,
            TabStripPlacement = Jalium.UI.Controls.Dock.Top
        };
        tabControl.Items.Add(new FWTabItem
        {
            Header = "Overview",
            Content = "Overview content"
        });
        tabControl.Items.Add(new FWTabItem
        {
            Header = "Details",
            Content = "Details content"
        });
        tabControl.SelectedIndex = 0;

        var navigationView = new FWNavigationView
        {
            PaneTitle = "Material shell",
            Header = "Navigation shell",
            PaneDisplayMode = NavigationViewPaneDisplayMode.LeftCompact,
            IsPaneOpen = false,
            OpenPaneLength = 220,
            CompactPaneLength = 48,
            Content = tabControl
        };
        navigationView.MenuItems.Add(overviewItem);
        navigationView.MenuItems.Add(controlsItem);
        navigationView.FooterMenuItems.Add(settingsItem);
        navigationView.SelectedItem = overviewItem;

        var surface = new FWFluentMaterialSurface
        {
            MaterialKind = FWFluentMaterialKind.LiquidGlass,
            TintOpacity = 0.2,
            BlurRadius = 14,
            RefractionAmount = 70,
            ChromaticAberration = 0.42,
            FusionRadius = 24,
            Shape = BorderShape.SuperEllipse,
            SuperEllipseN = 4,
            Child = navigationView
        };

        Assert.Equal(NavigationViewPaneDisplayMode.LeftCompact, navigationView.PaneDisplayMode);
        Assert.False(navigationView.IsPaneOpen);
        Assert.Equal(220, navigationView.OpenPaneLength);
        Assert.Equal(48, navigationView.CompactPaneLength);
        Assert.Equal(overviewItem, navigationView.SelectedItem);
        Assert.Single(navigationView.FooterMenuItems);
        Assert.Equal(Jalium.UI.Controls.Dock.Top, tabControl.TabStripPlacement);
        Assert.True(tabControl.IsSwipeEnabled);
        Assert.Equal("Overview content", tabControl.SelectedContent);
        Assert.Equal(FWFluentMaterialKind.LiquidGlass, surface.MaterialKind);
        Assert.True(surface.LiquidGlass);
        Assert.Equal(70, surface.RefractionAmount);
        Assert.Equal(0.42, surface.ChromaticAberration);
        Assert.Equal(24, surface.FusionRadius);
        Assert.Equal(BorderShape.SuperEllipse, surface.Shape);
        Assert.Equal(4, surface.SuperEllipseN);
        Assert.Same(navigationView, surface.Child);
    }

    private sealed class NavigationOverviewPage : Page
    {
    }

    private sealed class NavigationDetailsPage : Page
    {
    }

    private sealed class TestSelectorBar : FWSelectorBar
    {
        public Panel CreateDefaultItemsPanel() => CreateItemsPanel();
    }

    private static ResourceDictionary LoadGenericThemeDictionary()
    {
        var loaded = ResourceDictionary.SourceLoader?.Invoke(
            new ResourceDictionary(),
            new Uri("/FluentJalium;component/Themes/Generic.jalxaml", UriKind.Relative),
            FluentThemeManager.ThemeAssembly);

        return Assert.IsType<ResourceDictionary>(loaded);
    }

    private static Style AssertStyle<TControl>(ResourceDictionary dictionary)
        where TControl : FrameworkElement
    {
        Assert.True(dictionary.TryGetValue(typeof(TControl), out var value), $"{typeof(TControl).Name} style was not found.");
        return Assert.IsType<Style>(value);
    }

    private static void AssertBasedOnStyle<TFluentControl, TJaliumControl>(ResourceDictionary dictionary)
        where TFluentControl : TJaliumControl, IFluentJaliumControl
        where TJaliumControl : FrameworkElement
    {
        var baseStyle = AssertStyle<TJaliumControl>(dictionary);
        var fluentStyle = AssertStyle<TFluentControl>(dictionary);

        Assert.Equal(typeof(TFluentControl), fluentStyle.TargetType);
        Assert.Same(baseStyle, fluentStyle.BasedOn);
    }

    private static void AssertSetter(Style style, DependencyProperty property)
    {
        Assert.Contains(style.Setters, setter => setter.Property == property);
    }

    private static void AssertTargetTriggerSetter(
        Style style,
        DependencyProperty triggerProperty,
        object triggerValue,
        string targetName,
        DependencyProperty setterProperty,
        object expectedSetterValue)
    {
        var trigger = Assert.Single(
            style.Triggers.OfType<Trigger>(),
            candidate => candidate.Property == triggerProperty && TriggerValueEquals(candidate.Value, triggerValue));
        var setter = Assert.Single(
            trigger.Setters,
            candidate => candidate.TargetName == targetName && SetterTargetsProperty(candidate, setterProperty));

        Assert.True(
            TriggerValueEquals(setter.Value, expectedSetterValue),
            $"Expected trigger setter value {expectedSetterValue}, got {setter.Value}.");
    }

    private static bool SetterTargetsProperty(Setter setter, DependencyProperty property)
    {
        return setter.Property == property ||
            string.Equals(setter.PropertyName, $"{property.OwnerType.Name}.{property.Name}", StringComparison.Ordinal);
    }

    private static bool TriggerValueEquals(object? actual, object expected)
    {
        if (Equals(actual, expected))
        {
            return true;
        }

        if (actual is null)
        {
            return false;
        }

        return string.Equals(actual.ToString(), expected.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private static void InvokeNavigationItem(NavigationView navigationView, NavigationViewItem item)
    {
        var method = typeof(NavigationView).GetMethod("HandleItemClicked", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);
        method.Invoke(navigationView, [item]);
    }

    private static void ResetApplicationState()
    {
        var currentField = typeof(Application).GetField("_current", BindingFlags.NonPublic | BindingFlags.Static);
        currentField?.SetValue(null, null);

        var jaliumReset = typeof(JaliumThemeManager).GetMethod("Reset", BindingFlags.NonPublic | BindingFlags.Static);
        jaliumReset?.Invoke(null, null);

        var fluentReset = typeof(FluentThemeManager).GetMethod("Reset", BindingFlags.NonPublic | BindingFlags.Static);
        fluentReset?.Invoke(null, null);
    }
}
