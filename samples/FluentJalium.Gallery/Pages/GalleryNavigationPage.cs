using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using FluentJalium.Gallery.Controls;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWBreadcrumbBar = FluentJalium.Controls.FWBreadcrumbBar;
using FWAutoSuggestBox = FluentJalium.Controls.FWAutoSuggestBox;
using FWAutoSuggestBoxTextChangeReason = FluentJalium.Controls.FWAutoSuggestBoxTextChangeReason;
using FWButton = FluentJalium.Controls.FWButton;
using FWFluentMaterialKind = FluentJalium.Controls.FWFluentMaterialKind;
using FWFluentMaterialSurface = FluentJalium.Controls.FWFluentMaterialSurface;
using FWFrame = FluentJalium.Controls.FWFrame;
using FWNavigationDensity = FluentJalium.Controls.FWNavigationDensity;
using FWNavigationService = FluentJalium.Controls.FWNavigationService;
using FWNavigationServiceDiagnostics = FluentJalium.Controls.FWNavigationServiceDiagnostics;
using FWNavigationView = FluentJalium.Controls.FWNavigationView;
using FWNavigationViewItem = FluentJalium.Controls.FWNavigationViewItem;
using FWNavigationViewItemHeader = FluentJalium.Controls.FWNavigationViewItemHeader;
using FWNavigationViewItemSeparator = FluentJalium.Controls.FWNavigationViewItemSeparator;
using FWPipsPager = FluentJalium.Controls.FWPipsPager;
using FWSelectorBar = FluentJalium.Controls.FWSelectorBar;
using FWSelectorBarItem = FluentJalium.Controls.FWSelectorBarItem;
using FWSelectorBarSelectionIndicatorPlacement = FluentJalium.Controls.FWSelectorBarSelectionIndicatorPlacement;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWTabControl = FluentJalium.Controls.FWTabControl;
using FWTabItem = FluentJalium.Controls.FWTabItem;
using FWTabView = FluentJalium.Controls.FWTabView;
using FWTabViewCloseButtonOverlayMode = FluentJalium.Controls.FWTabViewCloseButtonOverlayMode;
using FWTabViewItem = FluentJalium.Controls.FWTabViewItem;
using FWTabViewWidthMode = FluentJalium.Controls.FWTabViewWidthMode;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWTextBox = FluentJalium.Controls.FWTextBox;
using FWTextInputDensity = FluentJalium.Controls.FWTextInputDensity;
using FWTitleBar = FluentJalium.Controls.FWTitleBar;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;
using PipsPagerButtonVisibility = FluentJalium.Controls.PipsPagerButtonVisibility;

namespace FluentJalium.Gallery.Pages;

internal readonly record struct GalleryNavigationShellQaSnapshot(
    string CurrentRouteKey,
    string CurrentPageType,
    bool HasPageTypeProvider,
    bool CanGoBack,
    bool CanGoForward,
    int BackStackDepth,
    int ForwardStackDepth,
    string BreadcrumbPath,
    string SelectorText,
    int SelectorIndex,
    int SelectorItemCount,
    string TabHeader,
    int TabIndex,
    int TabItemCount,
    FWTabViewCloseButtonOverlayMode CloseButtonOverlayMode,
    int SelectedPageNumber,
    int PageCount,
    string SearchText);

internal sealed class GalleryNavigationPage
{
    public UIElement CreateContent()
    {
        var panel = CreateSection("Navigation");
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        examples.Children.Add(CreateNavigationExampleCard(
            FluentIconRegular.Navigation24,
            "FWNavigationView",
            "Left navigation with pane header, footer menu items, nested items, and live selection output.",
            CreateNavigationViewSample()));
        examples.Children.Add(CreateNavigationExampleCard(
            FluentIconRegular.PanelLeft24,
            "Pane modes and hierarchy",
            "Switch Left, LeftCompact, LeftMinimal, and Top display modes while preserving hierarchy state.",
            CreateNavigationPaneModeSample()));
        examples.Children.Add(CreateNavigationExampleCard(
            FluentIconRegular.TabDesktop24,
            "FWTabControl",
            "Top, bottom, left, disabled, and swipe-enabled tab states with selected content output.",
            CreateTabControlNavigationSample()));
        examples.Children.Add(CreateNavigationExampleCard(
            FluentIconRegular.Window24,
            "FWNavigationService + FWFrame",
            "Route NavigationView selection into Frame pages with optional provider-based page resolution and synchronized diagnostics.",
            CreateFrameNavigationSample()));
        examples.Children.Add(CreateNavigationExampleCard(
            FluentIconRegular.WindowApps24,
            "App shell walkthrough",
            "A cohesive shell combines route provider navigation, breadcrumb, search, document tabs, footer settings, pager state, and selector diagnostics.",
            CreateAppShellWalkthroughSample(),
            width: 760));
        examples.Children.Add(CreateNavigationExampleCard(
            FluentIconRegular.BranchFork24,
            "FWBreadcrumbBar",
            "Hierarchical path navigation with collapsed middle items, density changes, and item click output.",
            CreateBreadcrumbBarSample()));
        examples.Children.Add(CreateNavigationExampleCard(
            FluentIconRegular.MoreHorizontal24,
            "FWPipsPager",
            "Page index navigation with visible pips, previous/next actions, and live page status.",
            CreatePipsPagerSample()));
        examples.Children.Add(CreateNavigationExampleCard(
            FluentIconRegular.SlideTransition24,
            "FWSelectorBar",
            "Compact in-page view switching with icons, indicator placement, and orientation changes.",
            CreateSelectorBarSample()));
        examples.Children.Add(CreateNavigationExampleCard(
            FluentIconRegular.TabDesktop24,
            "FWTabView / FWTabViewItem",
            "Document-style tabs with add, close, width mode, placement, and selected content output.",
            CreateTabViewSample()));
        examples.Children.Add(CreateNavigationExampleCard(
            FluentIconRegular.Window24,
            "FWTitleBar",
            "Safe title bar preview with app commands, window button state, and demo-only command output.",
            CreateTitleBarSample()));
        examples.Children.Add(CreateNavigationExampleCard(
            FluentIconRegular.LayerDiagonalSparkle24,
            "Material navigation shell",
            "NavigationView, tabs, and command states remain readable inside a LiquidGlass app shell.",
            CreateMaterialNavigationShellSample()));

        panel.Children.Add(examples);
        return panel;
    }

    private static UIElement CreateNavigationViewSample()
    {
        var output = CreateNavigationOutput("Selected: Dashboard");
        var dashboardItem = new FWNavigationViewItem
        {
            Content = "Dashboard",
            Icon = CreateIcon(FluentIconRegular.Home24)
        };
        var controlsItem = new FWNavigationViewItem
        {
            Content = "Controls",
            Icon = CreateIcon(FluentIconRegular.ControlButton24),
            IsExpanded = true
        };
        controlsItem.MenuItems.Add(new FWNavigationViewItem { Content = "Buttons", Icon = CreateIcon(FluentIconRegular.ControlButton24) });
        controlsItem.MenuItems.Add(new FWNavigationViewItem { Content = "Collections", Icon = CreateIcon(FluentIconRegular.Table24) });
        var galleryItem = new FWNavigationViewItem { Content = "Gallery", Icon = CreateIcon(FluentIconRegular.Folder24) };
        var settingsItem = new FWNavigationViewItem { Content = "Settings", Icon = CreateIcon(FluentIconRegular.Settings24) };

        var navigationView = new FWNavigationView
        {
            Width = 500,
            Height = 260,
            PaneTitle = "FluentJalium",
            Header = "NavigationView",
            OpenPaneLength = 220,
            CompactPaneLength = 48,
            PaneHeader = CreateNavigationPaneHeader("Controls"),
            PaneFooter = CreateNavigationPaneFooter("vNext"),
            SelectedItem = dashboardItem,
            Content = CreateNavigationContent("Dashboard", "Selected, nested, footer, and separator states share FluentJalium tokens.")
        };
        navigationView.MenuItems.Add(new FWNavigationViewItemHeader { Content = "Workspace" });
        navigationView.MenuItems.Add(dashboardItem);
        navigationView.MenuItems.Add(controlsItem);
        navigationView.MenuItems.Add(new FWNavigationViewItemSeparator());
        navigationView.MenuItems.Add(galleryItem);
        navigationView.FooterMenuItems.Add(settingsItem);
        navigationView.SelectionChanged += (_, e) =>
        {
            output.Text = $"Selected: {e.SelectedItem?.Content}";
            navigationView.Content = CreateNavigationContent(
                e.SelectedItem?.Content?.ToString() ?? "NavigationView",
                $"Previous: {e.PreviousSelectedItem?.Content ?? "none"}");
        };
        navigationView.UpdateMenuItems();

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                navigationView,
                CreateNavigationButtonRow(
                    CreateNavigationActionButton(FluentIconRegular.Home24, "Dashboard", () => navigationView.SelectedItem = dashboardItem),
                    CreateNavigationActionButton(FluentIconRegular.Table24, "Collections", () => navigationView.SelectedItem = controlsItem.MenuItems[1]),
                    CreateNavigationActionButton(FluentIconRegular.PanelLeftContract24, "Pane", () =>
                    {
                        navigationView.IsPaneOpen = !navigationView.IsPaneOpen;
                        output.Text = $"Pane open: {navigationView.IsPaneOpen}";
                    }),
                    CreateNavigationActionButton(FluentIconRegular.Settings24, "Footer", () => navigationView.SelectedItem = settingsItem)),
                CreateNavigationStatus(output)
            }
        };
    }

    private static UIElement CreateNavigationPaneModeSample()
    {
        var output = CreateNavigationOutput("PaneDisplayMode: Left. Document options expanded: true");
        var homeItem = new FWNavigationViewItem { Content = "Home", Icon = CreateIcon(FluentIconRegular.Home24) };
        var accountItem = new FWNavigationViewItem
        {
            Content = "Account",
            Icon = CreateIcon(FluentIconRegular.Person24)
        };
        accountItem.MenuItems.Add(new FWNavigationViewItem { Content = "Mail", Icon = CreateIcon(FluentIconRegular.Mail24) });
        accountItem.MenuItems.Add(new FWNavigationViewItem { Content = "Calendar", Icon = CreateIcon(FluentIconRegular.CalendarLtr24) });
        var documentOptionsItem = new FWNavigationViewItem
        {
            Content = "Document options",
            Icon = CreateIcon(FluentIconRegular.Document24),
            IsExpanded = true,
            SelectsOnInvoked = false
        };
        documentOptionsItem.MenuItems.Add(new FWNavigationViewItem { Content = "Create new", Icon = CreateIcon(FluentIconRegular.Add24) });
        documentOptionsItem.MenuItems.Add(new FWNavigationViewItem { Content = "Upload file", Icon = CreateIcon(FluentIconRegular.ArrowUpload24) });

        var navigationView = new FWNavigationView
        {
            Width = 500,
            Height = 250,
            Header = "Pane modes",
            PaneTitle = "Workspace",
            PaneDisplayMode = NavigationViewPaneDisplayMode.Left,
            OpenPaneLength = 220,
            CompactPaneLength = 48,
            IsBackButtonVisible = NavigationViewBackButtonVisible.Visible,
            IsBackEnabled = true,
            SelectedItem = homeItem,
            Content = CreateNavigationContent("Home", "Switch the pane display mode from the options below.")
        };
        navigationView.MenuItems.Add(homeItem);
        navigationView.MenuItems.Add(accountItem);
        navigationView.MenuItems.Add(documentOptionsItem);
        navigationView.SelectionChanged += (_, e) =>
        {
            output.Text = $"PaneDisplayMode: {navigationView.PaneDisplayMode}. Selected: {e.SelectedItem?.Content}";
        };
        navigationView.ItemInvoked += (_, e) =>
        {
            if (!e.InvokedItem.SelectsOnInvoked)
            {
                output.Text = $"Invoked non-selecting item: {e.InvokedItem.Content}. Expanded: {e.InvokedItem.IsExpanded}";
            }
        };
        navigationView.UpdateMenuItems();

        void SetMode(NavigationViewPaneDisplayMode mode)
        {
            navigationView.PaneDisplayMode = mode;
            navigationView.IsPaneOpen = mode != NavigationViewPaneDisplayMode.LeftCompact;
            output.Text = $"PaneDisplayMode: {mode}. Document options expanded: {documentOptionsItem.IsExpanded}";
        }

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                navigationView,
                CreateNavigationButtonRow(
                    CreateNavigationActionButton(FluentIconRegular.PanelLeft24, "Left", () => SetMode(NavigationViewPaneDisplayMode.Left)),
                    CreateNavigationActionButton(FluentIconRegular.PanelLeftContract24, "Compact", () => SetMode(NavigationViewPaneDisplayMode.LeftCompact)),
                    CreateNavigationActionButton(FluentIconRegular.PanelLeftExpand24, "Minimal", () => SetMode(NavigationViewPaneDisplayMode.LeftMinimal)),
                    CreateNavigationActionButton(FluentIconRegular.PanelTopGallery24, "Top", () => SetMode(NavigationViewPaneDisplayMode.Top)),
                    CreateNavigationActionButton(FluentIconRegular.BranchFork24, "Tree", () =>
                    {
                        documentOptionsItem.IsExpanded = !documentOptionsItem.IsExpanded;
                        output.Text = $"PaneDisplayMode: {navigationView.PaneDisplayMode}. Document options expanded: {documentOptionsItem.IsExpanded}";
                    })),
                CreateNavigationStatus(output)
            }
        };
    }

    private static UIElement CreateTabControlNavigationSample()
    {
        var output = CreateNavigationOutput("Selected tab: Overview");
        var tabControl = new FWTabControl
        {
            Width = 500,
            Height = 160,
            IsSwipeEnabled = true
        };
        tabControl.Items.Add(new FWTabItem
        {
            Header = "Overview",
            Content = CreateTabContent("Navigation items share the Fluent selection pill and hover states.")
        });
        tabControl.Items.Add(new FWTabItem
        {
            Header = "Details",
            Content = CreateTabContent("Tabs use the shared accent indicator and theme-aware strip colors.")
        });
        tabControl.Items.Add(new FWTabItem
        {
            Header = "Disabled",
            Content = CreateTabContent("Disabled tab sample"),
            IsEnabled = false
        });
        tabControl.SelectedIndex = 0;
        tabControl.SelectionChanged += (_, _) =>
        {
            output.Text = $"Selected tab: {(tabControl.SelectedItem as FWTabItem)?.Header}";
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                tabControl,
                CreateNavigationButtonRow(
                    CreateNavigationActionButton(FluentIconRegular.Home24, "Overview", () => tabControl.SelectedIndex = 0),
                    CreateNavigationActionButton(FluentIconRegular.Document24, "Details", () => tabControl.SelectedIndex = 1),
                    CreateNavigationActionButton(FluentIconRegular.TabDesktop24, "Top", () =>
                    {
                        tabControl.TabStripPlacement = Dock.Top;
                        output.Text = "TabStripPlacement: Top";
                    }),
                    CreateNavigationActionButton(FluentIconRegular.TabDesktopBottom24, "Bottom", () =>
                    {
                        tabControl.TabStripPlacement = Dock.Bottom;
                        output.Text = "TabStripPlacement: Bottom";
                    }),
                    CreateNavigationActionButton(FluentIconRegular.PanelLeft24, "Left", () =>
                    {
                        tabControl.TabStripPlacement = Dock.Left;
                        output.Text = "TabStripPlacement: Left";
                    })),
                CreateNavigationStatus(output)
            }
        };
    }

    [RequiresUnreferencedCode("Gallery sample navigates to local Page types by typeof literals.")]
    private static UIElement CreateFrameNavigationSample()
    {
        var output = CreateNavigationOutput("Route: not navigated");
        var frame = new FWFrame
        {
            Width = 300,
            Height = 160,
            Padding = new Thickness(14),
            BorderThickness = new Thickness(1)
        };
        var overviewItem = new FWNavigationViewItem
        {
            Content = "Overview",
            RouteKey = "overview",
            Icon = CreateIcon(FluentIconRegular.Home24)
        };
        var detailsItem = new FWNavigationViewItem
        {
            Content = "Details",
            RouteKey = "details",
            Icon = CreateIcon(FluentIconRegular.Document24)
        };
        var navigationView = new FWNavigationView
        {
            Width = 180,
            Height = 160,
            PaneTitle = "Routes",
            Header = "Service shell",
            PaneDisplayMode = NavigationViewPaneDisplayMode.Left,
            IsBackButtonVisible = NavigationViewBackButtonVisible.Visible,
            OpenPaneLength = 160,
            CompactPaneLength = 40
        };
        navigationView.MenuItems.Add(overviewItem);
        navigationView.MenuItems.Add(detailsItem);
        navigationView.UpdateMenuItems();

        var service = new FWNavigationService();
        service.PageTypeProvider = (route, parameter) =>
            route.RouteKey == "details" && Equals(parameter, "Details route")
                ? typeof(GalleryNavigationProviderPage)
                : route.PageType;
        service.RegisterRoute(overviewItem, typeof(GalleryNavigationOverviewPage), "Overview route");
        service.RegisterRoute(detailsItem, typeof(GalleryNavigationDetailsPage), "Details route");
        service.Attach(navigationView, frame);
        service.Navigated += (_, _) => output.Text = FormatNavigationServiceDiagnostics(service.GetDiagnostics());
        service.NavigateToRoute("overview");

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                new FWWrapPanel
                {
                    HorizontalSpacing = 10,
                    VerticalSpacing = 10,
                    Children =
                    {
                        navigationView,
                        frame
                    }
                },
                CreateNavigationButtonRow(
                    CreateNavigationActionButton(FluentIconRegular.Home24, "Overview", () => service.NavigateToRoute("overview")),
                    CreateNavigationActionButton(FluentIconRegular.Document24, "Details", () => service.NavigateToRoute("details")),
                    CreateNavigationActionButton(FluentIconRegular.ArrowLeft24, "Back", () =>
                    {
                        if (!service.GoBack())
                        {
                            output.Text = "Route: no back entry";
                        }
                    }),
                    CreateNavigationActionButton(FluentIconRegular.ArrowRight24, "Forward", () =>
                    {
                        if (!service.GoForward())
                        {
                            output.Text = "Route: no forward entry";
                        }
                    })),
                CreateNavigationStatus(output)
            }
        };
    }

    private static string FormatNavigationServiceDiagnostics(FWNavigationServiceDiagnostics diagnostics)
    {
        return $"Route: {diagnostics.CurrentRouteKey ?? "none"}, Page: {diagnostics.CurrentPageType?.Name ?? "none"}, Back: {diagnostics.CanGoBack}, Forward: {diagnostics.CanGoForward}, Stack: {diagnostics.BackStackDepth}, Provider: {(diagnostics.HasPageTypeProvider ? "On" : "Off")}";
    }

    internal static GalleryNavigationShellQaSnapshot CreateNavigationShellQaSnapshot(
        FWNavigationService service,
        IEnumerable<string> breadcrumbPath,
        FWSelectorBar selectorBar,
        FWTabView tabView,
        FWPipsPager pager,
        FWAutoSuggestBox searchBox)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(breadcrumbPath);
        ArgumentNullException.ThrowIfNull(selectorBar);
        ArgumentNullException.ThrowIfNull(tabView);
        ArgumentNullException.ThrowIfNull(pager);
        ArgumentNullException.ThrowIfNull(searchBox);

        var serviceDiagnostics = service.GetDiagnostics();
        var selectorDiagnostics = selectorBar.GetDiagnostics();
        var tabDiagnostics = tabView.GetDiagnostics();
        return new GalleryNavigationShellQaSnapshot(
            serviceDiagnostics.CurrentRouteKey ?? "none",
            serviceDiagnostics.CurrentPageType?.Name ?? "none",
            serviceDiagnostics.HasPageTypeProvider,
            serviceDiagnostics.CanGoBack,
            serviceDiagnostics.CanGoForward,
            serviceDiagnostics.BackStackDepth,
            serviceDiagnostics.CanGoForward ? 1 : 0,
            string.Join(" / ", breadcrumbPath),
            string.IsNullOrWhiteSpace(selectorDiagnostics.SelectedText) ? "none" : selectorDiagnostics.SelectedText,
            selectorDiagnostics.SelectedIndex,
            selectorDiagnostics.ItemCount,
            string.IsNullOrWhiteSpace(tabDiagnostics.SelectedHeader) ? "none" : tabDiagnostics.SelectedHeader,
            tabDiagnostics.SelectedIndex,
            tabDiagnostics.ItemCount,
            tabDiagnostics.CloseButtonOverlayMode,
            pager.SelectedPageIndex + 1,
            pager.NumberOfPages,
            string.IsNullOrWhiteSpace(searchBox.Text) ? "none" : searchBox.Text);
    }

    internal static string FormatNavigationShellQa(string action, GalleryNavigationShellQaSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(action);

        return $"{action}. Route: {snapshot.CurrentRouteKey}. Page: {snapshot.CurrentPageType}. Provider: {FormatOnOff(snapshot.HasPageTypeProvider)}. Back: {FormatOnOff(snapshot.CanGoBack)} ({snapshot.BackStackDepth}). Forward: {FormatOnOff(snapshot.CanGoForward)} ({snapshot.ForwardStackDepth}). Breadcrumb: {snapshot.BreadcrumbPath}. Selector: {snapshot.SelectorText} ({FormatIndex(snapshot.SelectorIndex, snapshot.SelectorItemCount)}). Tab: {snapshot.TabHeader} ({FormatIndex(snapshot.TabIndex, snapshot.TabItemCount)}), close {snapshot.CloseButtonOverlayMode}. Pips: {snapshot.SelectedPageNumber}/{snapshot.PageCount}. Search: {snapshot.SearchText}.";
    }

    [RequiresUnreferencedCode("Gallery sample navigates to local Page types by typeof literals.")]
    private static UIElement CreateAppShellWalkthroughSample()
    {
        var output = CreateNavigationOutput("Shell ready: route overview, breadcrumb Home / Overview, tab Overview, page 1.");
        var breadcrumbPath = new ObservableCollection<string> { "Home", "Overview" };
        var breadcrumbBar = new FWBreadcrumbBar
        {
            Width = 290,
            MaxItems = 4,
            Density = FWNavigationDensity.Compact,
            ItemsSource = breadcrumbPath
        };
        var pager = new FWPipsPager
        {
            Width = 188,
            Height = 40,
            NumberOfPages = 3,
            MaxVisiblePips = 3,
            SelectedPageIndex = 0
        };
        var frame = new FWFrame
        {
            Width = 300,
            Height = 140,
            Padding = new Thickness(14),
            BorderThickness = new Thickness(1)
        };
        var tabView = new FWTabView
        {
            Width = 300,
            Height = 140,
            Header = "Documents",
            Footer = "2 tabs",
            Density = FWNavigationDensity.Compact,
            TabWidthMode = FWTabViewWidthMode.SizeToContent,
            CloseButtonOverlayMode = FWTabViewCloseButtonOverlayMode.OnPointerOver,
            CanReorderTabs = true
        };
        tabView.Items.Add(CreateTabViewItem("Overview", FluentIconRegular.Home24, "Overview document follows the current app route.", isClosable: false));
        tabView.Items.Add(CreateTabViewItem("Activity", FluentIconRegular.Clock24, "Activity document tracks provider-backed route changes."));
        tabView.SelectedIndex = 0;

        var overviewSelectorItem = new FWSelectorBarItem { Text = "Overview", Icon = CreateIcon(FluentIconRegular.Home24, 16) };
        var activitySelectorItem = new FWSelectorBarItem { Text = "Activity", Icon = CreateIcon(FluentIconRegular.Clock24, 16) };
        var settingsSelectorItem = new FWSelectorBarItem { Text = "Settings", Icon = CreateIcon(FluentIconRegular.Settings24, 16) };
        var selectorBar = new FWSelectorBar
        {
            Width = 620,
            Density = FWNavigationDensity.Compact,
            SelectionIndicatorPlacement = FWSelectorBarSelectionIndicatorPlacement.Auto
        };
        selectorBar.Items.Add(overviewSelectorItem);
        selectorBar.Items.Add(activitySelectorItem);
        selectorBar.Items.Add(settingsSelectorItem);
        selectorBar.SelectedItem = overviewSelectorItem;

        var searchBox = new FWAutoSuggestBox
        {
            Width = 180,
            Text = "Overview",
            ItemsSource = ShellRouteSuggestions,
            PlaceholderText = "Search routes",
            FilterMode = AutoCompleteFilterMode.Contains,
            MinimumPrefixLength = 1,
            Density = FWTextInputDensity.Compact
        };

        var overviewItem = new FWNavigationViewItem
        {
            Content = "Overview",
            RouteKey = "overview",
            Icon = CreateIcon(FluentIconRegular.Home24)
        };
        var activityItem = new FWNavigationViewItem
        {
            Content = "Activity",
            RouteKey = "activity",
            Icon = CreateIcon(FluentIconRegular.History24)
        };
        var settingsItem = new FWNavigationViewItem
        {
            Content = "Settings",
            RouteKey = "settings",
            Icon = CreateIcon(FluentIconRegular.Settings24)
        };
        var navigationView = new FWNavigationView
        {
            Width = 720,
            Height = 430,
            PaneTitle = "Jalium Studio",
            Header = "Route provider shell",
            PaneDisplayMode = NavigationViewPaneDisplayMode.LeftCompact,
            IsPaneOpen = false,
            IsSettingsVisible = false,
            IsBackButtonVisible = NavigationViewBackButtonVisible.Visible,
            OpenPaneLength = 220,
            CompactPaneLength = 52,
            PaneHeader = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 6,
                Children =
                {
                    new FWTextBlock { Text = "Route search", FontSize = 12, Foreground = ThemeBrush("TextSecondary") },
                    searchBox
                }
            },
            PaneFooter = CreateNavigationPaneFooter("Provider on")
        };
        navigationView.MenuItems.Add(overviewItem);
        navigationView.MenuItems.Add(activityItem);
        navigationView.FooterMenuItems.Add(settingsItem);
        navigationView.UpdateMenuItems();

        var service = new FWNavigationService
        {
            PageTypeProvider = (route, parameter) => route.RouteKey == "settings"
                ? typeof(GalleryNavigationProviderPage)
                : route.PageType
        };
        service.RegisterRoute(overviewItem, typeof(GalleryNavigationOverviewPage), "Overview route");
        service.RegisterRoute(activityItem, typeof(GalleryNavigationDetailsPage), "Activity route");
        service.RegisterRoute(settingsItem, typeof(GalleryNavigationDetailsPage), "Settings route");
        service.Attach(navigationView, frame);

        var isSynchronizingShell = false;

        void UpdateBreadcrumb(string routeKey)
        {
            breadcrumbPath.Clear();
            breadcrumbPath.Add("Home");
            breadcrumbPath.Add(ResolveShellRouteTitle(routeKey));
        }

        void EnsureRouteTab(string routeKey)
        {
            var title = ResolveShellRouteTitle(routeKey);
            var existingTab = tabView.Items.OfType<FWTabViewItem>()
                .FirstOrDefault(tab => string.Equals(tab.Header?.ToString(), title, StringComparison.Ordinal));
            if (existingTab == null)
            {
                existingTab = CreateTabViewItem(
                    title,
                    ResolveShellRouteIcon(routeKey),
                    $"{title} document was opened from the app-shell route.");
                tabView.Items.Add(existingTab);
            }

            tabView.SelectedItem = existingTab;
            tabView.Footer = $"{tabView.Items.Count} tabs";
        }

        void SynchronizeShell(string routeKey, string reason)
        {
            isSynchronizingShell = true;
            try
            {
                searchBox.SetQueryText(ResolveShellRouteTitle(routeKey), FWAutoSuggestBoxTextChangeReason.ProgrammaticChange);
                selectorBar.SelectedItem = ResolveShellSelectorItem(routeKey, overviewSelectorItem, activitySelectorItem, settingsSelectorItem);
                pager.SelectedPageIndex = ResolveShellPageIndex(routeKey);
                UpdateBreadcrumb(routeKey);
                EnsureRouteTab(routeKey);
            }
            finally
            {
                isSynchronizingShell = false;
            }

            var snapshot = CreateNavigationShellQaSnapshot(
                service,
                breadcrumbPath,
                selectorBar,
                tabView,
                pager,
                searchBox);
            output.Text = FormatNavigationShellQa(reason, snapshot);
        }

        bool NavigateShell(string routeKey, string reason)
        {
            if (!service.NavigateToRoute(routeKey))
            {
                output.Text = $"{reason}: route {routeKey} is not registered.";
                return false;
            }

            return true;
        }

        service.Navigated += (_, route) => SynchronizeShell(route.RouteKey, "Navigated");
        selectorBar.SelectionChanged += (_, _) =>
        {
            if (!isSynchronizingShell)
            {
                NavigateShell(ResolveShellRouteKey(selectorBar.SelectedItem), "Selector");
            }
        };
        pager.SelectedIndexChanged += (_, _) =>
        {
            if (!isSynchronizingShell)
            {
                NavigateShell(ResolveShellRouteKey(pager.SelectedPageIndex), "Pager");
            }
        };
        tabView.SelectionChanged += (_, _) =>
        {
            if (!isSynchronizingShell)
            {
                NavigateShell(ResolveShellRouteKey(tabView.SelectedItem), "Tab");
            }
        };
        tabView.AddTabButtonClick += (_, e) =>
        {
            var nextNumber = tabView.Items.Count + 1;
            e.NewItem = CreateTabViewItem($"Scratch {nextNumber}", FluentIconRegular.Document24, $"Scratch {nextNumber} is a local tab without a route.");
            output.Text = $"Shell tab added: Scratch {nextNumber}.";
        };
        searchBox.SuggestionChosen += (_, args) => NavigateShell(ResolveShellRouteKey(args.SelectedItem), "Search suggestion");
        searchBox.QuerySubmitted += (_, args) => NavigateShell(ResolveShellRouteKey(args.QueryText), "Search submit");

        navigationView.Content = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Margin = new Thickness(14),
            Children =
            {
                new FWWrapPanel
                {
                    HorizontalSpacing = 10,
                    VerticalSpacing = 8,
                    Children =
                    {
                        breadcrumbBar,
                        pager
                    }
                },
                selectorBar,
                new FWWrapPanel
                {
                    HorizontalSpacing = 12,
                    VerticalSpacing = 10,
                    Children =
                    {
                        frame,
                        tabView
                    }
                },
                CreateNavigationButtonRow(
                    CreateNavigationActionButton(FluentIconRegular.Home24, "Overview", () => NavigateShell("overview", "Button")),
                    CreateNavigationActionButton(FluentIconRegular.Clock24, "Activity", () => NavigateShell("activity", "Button")),
                    CreateNavigationActionButton(FluentIconRegular.Settings24, "Settings", () => NavigateShell("settings", "Button")),
                    CreateNavigationActionButton(FluentIconRegular.PanelLeft24, "Pane", () =>
                    {
                        navigationView.IsPaneOpen = !navigationView.IsPaneOpen;
                        SynchronizeShell(service.CurrentRouteKey ?? "overview", "Pane toggled");
                    }),
                    CreateNavigationActionButton(FluentIconRegular.Add24, "Tab", () =>
                    {
                        _ = tabView.RequestAddTab();
                        SynchronizeShell(service.CurrentRouteKey ?? "overview", "Tab requested");
                    })),
                CreateNavigationStatus(output)
            }
        };

        service.NavigateToRoute("overview");
        return navigationView;
    }

    private static UIElement CreateMaterialNavigationShellSample()
    {
        var output = CreateNavigationOutput("Shell: LiquidGlass. Pane compact. Tab: Overview.");
        var overviewItem = new FWNavigationViewItem { Content = "Overview", Icon = CreateIcon(FluentIconRegular.Home24) };
        var controlsItem = new FWNavigationViewItem { Content = "Controls", Icon = CreateIcon(FluentIconRegular.ControlButton24) };
        var materialsItem = new FWNavigationViewItem { Content = "Materials", Icon = CreateIcon(FluentIconRegular.LayerDiagonalSparkle24) };
        var settingsItem = new FWNavigationViewItem { Content = "Settings", Icon = CreateIcon(FluentIconRegular.Settings24) };
        var tabControl = new FWTabControl
        {
            Height = 122,
            IsSwipeEnabled = true
        };
        tabControl.Items.Add(new FWTabItem
        {
            Header = "Overview",
            Content = CreateTabContent("Compact navigation keeps command surfaces close to the app content.")
        });
        tabControl.Items.Add(new FWTabItem
        {
            Header = "Details",
            Content = CreateTabContent("LiquidGlass uses Jalium element effects while preserving Fluent selection contrast.")
        });
        tabControl.SelectedIndex = 0;

        var navigationView = new FWNavigationView
        {
            Width = 500,
            Height = 260,
            PaneTitle = "Shell",
            Header = "Material navigation shell",
            PaneDisplayMode = NavigationViewPaneDisplayMode.LeftCompact,
            IsPaneOpen = false,
            OpenPaneLength = 210,
            CompactPaneLength = 48,
            SelectedItem = overviewItem,
            Content = tabControl
        };
        navigationView.MenuItems.Add(overviewItem);
        navigationView.MenuItems.Add(controlsItem);
        navigationView.MenuItems.Add(materialsItem);
        navigationView.FooterMenuItems.Add(settingsItem);
        navigationView.SelectionChanged += (_, e) =>
        {
            output.Text = $"Shell: LiquidGlass. Selected: {e.SelectedItem?.Content}. Tab: {(tabControl.SelectedItem as FWTabItem)?.Header}.";
        };
        tabControl.SelectionChanged += (_, _) =>
        {
            output.Text = $"Shell: LiquidGlass. Selected: {(navigationView.SelectedItem as FWNavigationViewItem)?.Content}. Tab: {(tabControl.SelectedItem as FWTabItem)?.Header}.";
        };
        navigationView.UpdateMenuItems();

        return new FWFluentMaterialSurface
        {
            Width = 540,
            MaterialKind = FWFluentMaterialKind.LiquidGlass,
            TintColor = Color.FromArgb(180, 20, 84, 145),
            TintOpacity = 0.2,
            BlurRadius = 14,
            RefractionAmount = 70,
            ChromaticAberration = 0.42,
            FusionRadius = 24,
            Background = new SolidColorBrush(Color.FromArgb(66, 255, 255, 255)),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Shape = BorderShape.SuperEllipse,
            SuperEllipseN = 4,
            Padding = new Thickness(16),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 12,
                Children =
                {
                    CreateMaterialHeader(),
                    navigationView,
                    CreateNavigationButtonRow(
                        CreateNavigationActionButton(FluentIconRegular.PanelLeftExpand24, "Open pane", () =>
                        {
                            navigationView.IsPaneOpen = !navigationView.IsPaneOpen;
                            output.Text = $"Shell: LiquidGlass. Pane open: {navigationView.IsPaneOpen}.";
                        }),
                        CreateNavigationActionButton(FluentIconRegular.LayerDiagonalSparkle24, "Materials", () => navigationView.SelectedItem = materialsItem),
                        CreateNavigationActionButton(FluentIconRegular.TabDesktop24, "Next tab", () =>
                        {
                            tabControl.SelectedIndex = tabControl.SelectedIndex == 0 ? 1 : 0;
                            output.Text = $"Shell: LiquidGlass. Tab: {(tabControl.SelectedItem as FWTabItem)?.Header}.";
                        })),
                    CreateNavigationStatus(output)
                }
            }
        };
    }

    private static UIElement CreateBreadcrumbBarSample()
    {
        var output = CreateNavigationOutput("Path: Home / Samples / Navigation / BreadcrumbBar");
        var path = new ObservableCollection<string> { "Home", "Samples", "Navigation", "BreadcrumbBar" };
        var breadcrumbBar = new FWBreadcrumbBar
        {
            Width = 500,
            MaxItems = 5,
            ItemsSource = path
        };

        void UpdateStatus(string reason)
        {
            output.Text = $"{reason}: {string.Join(" / ", path)}. Density: {breadcrumbBar.Density}. MaxItems: {breadcrumbBar.MaxItems}";
        }

        breadcrumbBar.ItemClicked += (_, e) =>
        {
            while (path.Count > e.Index + 1)
            {
                path.RemoveAt(path.Count - 1);
            }

            UpdateStatus($"Clicked {e.ClickedItem}");
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                new FWBorder
                {
                    Width = 500,
                    Background = ThemeBrush("LayerFillColorDefaultBrush"),
                    BorderBrush = ThemeBrush("ControlBorder"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(12),
                    Child = breadcrumbBar
                },
                CreateNavigationButtonRow(
                    CreateNavigationActionButton(FluentIconRegular.Add24, "Add level", () =>
                    {
                        path.Add(path.Count % 2 == 0 ? "Details" : $"Level {path.Count + 1}");
                        UpdateStatus("Added breadcrumb");
                    }),
                    CreateNavigationActionButton(FluentIconRegular.ArrowLeft24, "Back", () =>
                    {
                        if (path.Count > 1)
                        {
                            path.RemoveAt(path.Count - 1);
                        }

                        UpdateStatus("Moved back");
                    }),
                    CreateNavigationActionButton(FluentIconRegular.TextDensity24, "Density", () =>
                    {
                        breadcrumbBar.Density = breadcrumbBar.Density == FWNavigationDensity.Compact
                            ? FWNavigationDensity.Comfortable
                            : FWNavigationDensity.Compact;
                        UpdateStatus("Changed density");
                    }),
                    CreateNavigationActionButton(FluentIconRegular.MoreHorizontal24, "Collapse", () =>
                    {
                        breadcrumbBar.MaxItems = breadcrumbBar.MaxItems == 4 ? 6 : 4;
                        UpdateStatus("Changed collapse limit");
                    })),
                CreateNavigationStatus(output)
            }
        };
    }

    private static UIElement CreatePipsPagerSample()
    {
        var output = CreateNavigationOutput("Page 1 of 10. Visible pips: 5. Buttons: visible");
        var pageText = new FWTextBlock
        {
            Text = "Page 1: Overview",
            FontSize = 16,
            Foreground = ThemeBrush("TextPrimary"),
            TextWrapping = TextWrapping.Wrap
        };
        var pager = new FWPipsPager
        {
            Width = 500,
            Height = 40,
            NumberOfPages = 10,
            MaxVisiblePips = 5
        };

        void UpdateStatus(string reason)
        {
            pageText.Text = $"Page {pager.SelectedPageIndex + 1}: {GetPagerPageName(pager.SelectedPageIndex)}";
            output.Text = $"{reason}: page {pager.SelectedPageIndex + 1} of {pager.NumberOfPages}. Visible pips: {pager.MaxVisiblePips}. Buttons: {pager.PreviousButtonVisibility}/{pager.NextButtonVisibility}";
        }

        pager.SelectedIndexChanged += (_, e) =>
        {
            UpdateStatus($"Page changed from {e.OldIndex + 1} to {e.NewIndex + 1}");
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                new FWBorder
                {
                    Width = 500,
                    Height = 72,
                    Background = ThemeBrush("LayerFillColorDefaultBrush"),
                    BorderBrush = ThemeBrush("ControlBorder"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(12),
                    Child = pageText
                },
                pager,
                CreateNavigationButtonRow(
                    CreateNavigationActionButton(FluentIconRegular.ArrowLeft24, "Previous", () =>
                    {
                        pager.SelectedPageIndex = Math.Max(0, pager.SelectedPageIndex - 1);
                        UpdateStatus("Previous requested");
                    }),
                    CreateNavigationActionButton(FluentIconRegular.ArrowRight24, "Next", () =>
                    {
                        pager.SelectedPageIndex = Math.Min(pager.NumberOfPages - 1, pager.SelectedPageIndex + 1);
                        UpdateStatus("Next requested");
                    }),
                    CreateNavigationActionButton(FluentIconRegular.NumberSymbol24, "Pages", () =>
                    {
                        pager.NumberOfPages = pager.NumberOfPages == 10 ? 6 : 10;
                        UpdateStatus("Page count changed");
                    }),
                    CreateNavigationActionButton(FluentIconRegular.MoreHorizontal24, "Pips", () =>
                    {
                        pager.MaxVisiblePips = pager.MaxVisiblePips == 5 ? 7 : 5;
                        UpdateStatus("Visible pips changed");
                    }),
                    CreateNavigationActionButton(FluentIconRegular.ChevronRight24, "Buttons", () =>
                    {
                        var nextVisibility = pager.NextButtonVisibility == PipsPagerButtonVisibility.Visible
                            ? PipsPagerButtonVisibility.VisibleOnPointerOver
                            : PipsPagerButtonVisibility.Visible;
                        pager.PreviousButtonVisibility = nextVisibility;
                        pager.NextButtonVisibility = nextVisibility;
                        UpdateStatus("Button visibility changed");
                    })),
                CreateNavigationStatus(output)
            }
        };
    }

    private static UIElement CreateSelectorBarSample()
    {
        var output = CreateNavigationOutput("Selected view: Overview. Index: 0/3. Indicator: Auto. Orientation: Horizontal");
        var preview = new FWTextBlock
        {
            Text = "Overview view: recent navigation health and shell links.",
            Foreground = ThemeBrush("TextPrimary"),
            TextWrapping = TextWrapping.Wrap
        };
        var overviewItem = new FWSelectorBarItem { Text = "Overview", Icon = CreateIcon(FluentIconRegular.Home24, 16) };
        var activityItem = new FWSelectorBarItem { Text = "Activity", Icon = CreateIcon(FluentIconRegular.Clock24, 16) };
        var settingsItem = new FWSelectorBarItem { Text = "Settings", Icon = CreateIcon(FluentIconRegular.Settings24, 16) };
        var selectorBar = new FWSelectorBar
        {
            Width = 500,
            Density = FWNavigationDensity.Comfortable,
            SelectionIndicatorPlacement = FWSelectorBarSelectionIndicatorPlacement.Auto
        };
        selectorBar.Items.Add(overviewItem);
        selectorBar.Items.Add(activityItem);
        selectorBar.Items.Add(settingsItem);
        selectorBar.SelectedIndex = 0;

        void UpdateStatus(string reason)
        {
            var diagnostics = selectorBar.GetDiagnostics();
            var selectedText = string.IsNullOrWhiteSpace(diagnostics.SelectedText) ? "No" : diagnostics.SelectedText;
            preview.Text = $"{selectedText} view: {GetSelectorViewDescription(diagnostics.SelectedText)}";
            output.Text = $"{reason}: {(diagnostics.HasSelection ? selectedText : "none")}. Index: {diagnostics.SelectedIndex}/{diagnostics.ItemCount}. Indicator: {diagnostics.SelectionIndicatorPlacement}. Orientation: {diagnostics.Orientation}. Density: {diagnostics.Density}";
        }

        selectorBar.SelectionChanged += (_, _) => UpdateStatus("Selected view");

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                selectorBar,
                new FWBorder
                {
                    Width = 500,
                    Height = 72,
                    Background = ThemeBrush("LayerFillColorDefaultBrush"),
                    BorderBrush = ThemeBrush("ControlBorder"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(12),
                    Child = preview
                },
                CreateNavigationButtonRow(
                    CreateNavigationActionButton(FluentIconRegular.Home24, "Overview", () => selectorBar.SelectItem(overviewItem)),
                    CreateNavigationActionButton(FluentIconRegular.Clock24, "Activity", () => selectorBar.SelectItem(activityItem)),
                    CreateNavigationActionButton(FluentIconRegular.Settings24, "Settings", () => selectorBar.SelectItem(settingsItem)),
                    CreateNavigationActionButton(FluentIconRegular.PanelLeft24, "Vertical", () =>
                    {
                        selectorBar.Orientation = selectorBar.Orientation == Orientation.Horizontal ? Orientation.Vertical : Orientation.Horizontal;
                        selectorBar.Width = selectorBar.Orientation == Orientation.Horizontal ? 500 : 180;
                        selectorBar.SelectionIndicatorPlacement = selectorBar.Orientation == Orientation.Horizontal
                            ? FWSelectorBarSelectionIndicatorPlacement.Auto
                            : FWSelectorBarSelectionIndicatorPlacement.Left;
                        UpdateStatus("Orientation changed");
                    }),
                    CreateNavigationActionButton(FluentIconRegular.TextDensity24, "Density", () =>
                    {
                        selectorBar.Density = selectorBar.Density == FWNavigationDensity.Compact
                            ? FWNavigationDensity.Spacious
                            : FWNavigationDensity.Compact;
                        UpdateStatus("Density changed");
                    })),
                CreateNavigationStatus(output)
            }
        };
    }

    private static UIElement CreateTabViewSample()
    {
        var output = CreateNavigationOutput("Selected tab: Overview. Tabs: 3. Width: SizeToContent. Close: Always");
        var tabView = new FWTabView
        {
            Width = 500,
            Height = 180,
            Header = "Workspaces",
            Footer = "3 tabs",
            Density = FWNavigationDensity.Comfortable,
            TabWidthMode = FWTabViewWidthMode.SizeToContent,
            CloseButtonOverlayMode = FWTabViewCloseButtonOverlayMode.Always,
            CanReorderTabs = true
        };
        var overviewTab = CreateTabViewItem("Overview", FluentIconRegular.Home24, "Overview workspace keeps the app-level route and command context visible.", isClosable: false);
        var detailsTab = CreateTabViewItem("Details", FluentIconRegular.Document24, "Details workspace uses closable tabs for document-like navigation.");
        var metricsTab = CreateTabViewItem("Metrics", FluentIconRegular.DataHistogram24, "Metrics workspace shows how selection updates the content presenter.");
        tabView.Items.Add(overviewTab);
        tabView.Items.Add(detailsTab);
        tabView.Items.Add(metricsTab);
        tabView.SelectedIndex = 0;

        void UpdateStatus(string reason)
        {
            var diagnostics = tabView.GetDiagnostics();
            tabView.Footer = $"{diagnostics.ItemCount} tabs";
            output.Text = $"{reason}: {(diagnostics.HasSelection ? diagnostics.SelectedHeader : "none")}. Index: {diagnostics.SelectedIndex}/{diagnostics.ItemCount}. Width: {diagnostics.TabWidthMode}. Close: {diagnostics.CloseButtonOverlayMode}. Placement: {diagnostics.TabStripPlacement}. Reorder: {diagnostics.CanReorderTabs}";
        }

        tabView.SelectionChanged += (_, _) => UpdateStatus("Selected tab");
        tabView.AddTabButtonClick += (_, e) =>
        {
            var nextNumber = tabView.Items.Count + 1;
            e.NewItem = CreateTabViewItem($"Doc {nextNumber}", FluentIconRegular.Document24, $"Doc {nextNumber} was added through AddTabButtonClick.");
            output.Text = $"Add requested: Doc {nextNumber}";
        };
        tabView.TabCloseRequested += (_, e) =>
        {
            if (!e.Tab.IsClosable || tabView.Items.Count <= 1)
            {
                e.Cancel = true;
                output.Text = $"Close canceled: {e.Tab.Header}";
                return;
            }

            output.Text = $"Close requested: {e.Tab.Header} at index {e.Index}";
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                tabView,
                CreateNavigationButtonRow(
                    CreateNavigationActionButton(FluentIconRegular.Add24, "Add", () =>
                    {
                        _ = tabView.RequestAddTab();
                        UpdateStatus("Added tab");
                    }),
                    CreateNavigationActionButton(FluentIconRegular.Dismiss24, "Close", () =>
                    {
                        if (tabView.SelectedItem is FWTabViewItem selectedTab)
                        {
                            if (!tabView.RequestCloseTab(selectedTab))
                            {
                                output.Text = $"Close unavailable: {selectedTab.Header}";
                            }
                            else
                            {
                                UpdateStatus("Closed tab");
                            }
                        }
                    }),
                    CreateNavigationActionButton(FluentIconRegular.TableResizeColumn24, "Width", () =>
                    {
                        tabView.TabWidthMode = tabView.TabWidthMode switch
                        {
                            FWTabViewWidthMode.Equal => FWTabViewWidthMode.SizeToContent,
                            FWTabViewWidthMode.SizeToContent => FWTabViewWidthMode.Compact,
                            _ => FWTabViewWidthMode.Equal
                        };
                        UpdateStatus("Width mode changed");
                    }),
                    CreateNavigationActionButton(FluentIconRegular.TabDesktopBottom24, "Placement", () =>
                    {
                        tabView.TabStripPlacement = tabView.TabStripPlacement == Dock.Top ? Dock.Bottom : Dock.Top;
                        UpdateStatus("Placement changed");
                    }),
                    CreateNavigationActionButton(FluentIconRegular.Eye24, "Close mode", () =>
                    {
                        tabView.CloseButtonOverlayMode = tabView.CloseButtonOverlayMode == FWTabViewCloseButtonOverlayMode.Always
                            ? FWTabViewCloseButtonOverlayMode.OnPointerOver
                            : FWTabViewCloseButtonOverlayMode.Always;
                        UpdateStatus("Close mode changed");
                    })),
                CreateNavigationStatus(output)
            }
        };
    }

    private static UIElement CreateTitleBarSample()
    {
        var output = CreateNavigationOutput("TitleBar preview: window buttons visible. Commands are demo-only.");
        var syncButton = new FWButton
        {
            Content = CreateIcon(FluentIconRegular.CloudSync24, 16, ThemeBrush("TextPrimary")),
            Width = 32,
            Height = 28
        };
        syncButton.Click += (_, _) => output.Text = "TitleBar command: Sync clicked.";

        var pinButton = new FWButton
        {
            Content = CreateIcon(FluentIconRegular.Pin24, 16, ThemeBrush("TextPrimary")),
            Width = 32,
            Height = 28
        };
        pinButton.Click += (_, _) => output.Text = "TitleBar command: Pin clicked.";

        var titleBar = new FWTitleBar
        {
            Width = 500,
            Height = 36,
            Title = "FluentJalium Gallery",
            IsShowIcon = false,
            IsShowTitle = true,
            ShowMinimizeButton = true,
            ShowMaximizeButton = true,
            ShowCloseButton = true,
            Background = ThemeBrush("LayerFillColorDefaultBrush"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            LeftWindowCommands = new FWStackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 6,
                Children =
                {
                    CreateIcon(FluentIconRegular.Window24, 18, ThemeBrush("TextPrimary"))
                }
            },
            RightWindowCommands = new FWStackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 4,
                Children =
                {
                    syncButton,
                    pinButton
                }
            }
        };
        titleBar.MinimizeClicked += (_, _) => output.Text = "TitleBar window button: Minimize clicked (demo only).";
        titleBar.MaximizeRestoreClicked += (_, _) =>
        {
            titleBar.IsMaximized = !titleBar.IsMaximized;
            output.Text = $"TitleBar window button: {(titleBar.IsMaximized ? "Maximize" : "Restore")} clicked (demo only).";
        };
        titleBar.CloseClicked += (_, _) => output.Text = "TitleBar window button: Close clicked (demo only).";

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                titleBar,
                CreateNavigationButtonRow(
                    CreateNavigationActionButton(FluentIconRegular.Window24, "Max state", () =>
                    {
                        titleBar.IsMaximized = !titleBar.IsMaximized;
                        output.Text = $"TitleBar preview maximized: {titleBar.IsMaximized}";
                    }),
                    CreateNavigationActionButton(FluentIconRegular.Subtract24, "Minimize", () =>
                    {
                        titleBar.ShowMinimizeButton = !titleBar.ShowMinimizeButton;
                        output.Text = $"ShowMinimizeButton: {titleBar.ShowMinimizeButton}";
                    }),
                    CreateNavigationActionButton(FluentIconRegular.Square24, "Maximize", () =>
                    {
                        titleBar.ShowMaximizeButton = !titleBar.ShowMaximizeButton;
                        output.Text = $"ShowMaximizeButton: {titleBar.ShowMaximizeButton}";
                    }),
                    CreateNavigationActionButton(FluentIconRegular.Dismiss24, "Close", () =>
                    {
                        titleBar.ShowCloseButton = !titleBar.ShowCloseButton;
                        output.Text = $"ShowCloseButton: {titleBar.ShowCloseButton}";
                    })),
                CreateNavigationStatus(output)
            }
        };
    }

    private static FWStackPanel CreateMaterialHeader()
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                CreateIcon(FluentIconRegular.WindowBrush24, 18, ThemeBrush("TextPrimary")),
                new FWTextBlock
                {
                    Text = "Layered app shell",
                    FontSize = 15,
                    Foreground = ThemeBrush("TextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
    }

    private static UIElement CreateNavigationContent(string title, string description)
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Margin = new Thickness(18),
            Children =
            {
                new FWTextBlock
                {
                    Text = title,
                    FontSize = 18,
                    Foreground = ThemeBrush("TextPrimary")
                },
                new FWTextBlock
                {
                    Text = description,
                    Foreground = ThemeBrush("TextSecondary"),
                    TextWrapping = TextWrapping.Wrap
                }
            }
        };
    }

    private static UIElement CreateNavigationPaneHeader(string text)
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 6,
            Children =
            {
                new FWTextBlock { Text = "Search", FontSize = 12, Foreground = ThemeBrush("TextSecondary") },
                new FWTextBox { Text = text, MinHeight = 32, PlaceholderText = "Search navigation" }
            }
        };
    }

    private static UIElement CreateNavigationPaneFooter(string text)
    {
        return new FWTextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary")
        };
    }

    private static UIElement CreateTabContent(string text)
    {
        return new FWTextBlock
        {
            Text = text,
            Margin = new Thickness(12),
            Foreground = ThemeBrush("TextPrimary"),
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center
        };
    }

    private static FWTabViewItem CreateTabViewItem(string header, FluentIconRegular icon, string content, bool isClosable = true)
    {
        return new FWTabViewItem
        {
            Header = header,
            Icon = CreateIcon(icon, 16),
            Content = CreateTabContent(content),
            IsClosable = isClosable
        };
    }

    private static string GetPagerPageName(int index)
    {
        return index switch
        {
            0 => "Overview",
            1 => "Collections",
            2 => "Navigation",
            3 => "Materials",
            4 => "Settings",
            _ => $"Workspace {index + 1}"
        };
    }

    private static string GetSelectorViewDescription(string? view)
    {
        return view switch
        {
            "Activity" => "recent route changes, selected tabs, and pager movement.",
            "Settings" => "density, indicator, and command preferences for this section.",
            _ => "recent navigation health and shell links."
        };
    }

    private static string FormatOnOff(bool value)
    {
        return value ? "on" : "off";
    }

    private static string FormatIndex(int index, int count)
    {
        return count <= 0 || index < 0 ? $"none/{count}" : $"{index + 1}/{count}";
    }

    private static string ResolveShellRouteKey(object? routeHint)
    {
        var text = routeHint switch
        {
            FWSelectorBarItem selectorItem => selectorItem.Text,
            FWTabViewItem tabItem => tabItem.Header?.ToString(),
            string routeText => routeText,
            _ => routeHint?.ToString()
        };

        if (string.IsNullOrWhiteSpace(text))
        {
            return "overview";
        }

        return text.Contains("setting", StringComparison.OrdinalIgnoreCase)
            ? "settings"
            : text.Contains("activity", StringComparison.OrdinalIgnoreCase)
                || text.Contains("detail", StringComparison.OrdinalIgnoreCase)
                ? "activity"
                : "overview";
    }

    private static string ResolveShellRouteKey(int pageIndex)
    {
        return pageIndex switch
        {
            1 => "activity",
            2 => "settings",
            _ => "overview"
        };
    }

    private static string ResolveShellRouteTitle(string routeKey)
    {
        return routeKey switch
        {
            "activity" => "Activity",
            "settings" => "Settings",
            _ => "Overview"
        };
    }

    private static int ResolveShellPageIndex(string routeKey)
    {
        return routeKey switch
        {
            "activity" => 1,
            "settings" => 2,
            _ => 0
        };
    }

    private static FluentIconRegular ResolveShellRouteIcon(string routeKey)
    {
        return routeKey switch
        {
            "activity" => FluentIconRegular.Clock24,
            "settings" => FluentIconRegular.Settings24,
            _ => FluentIconRegular.Home24
        };
    }

    private static FWSelectorBarItem ResolveShellSelectorItem(
        string routeKey,
        FWSelectorBarItem overviewItem,
        FWSelectorBarItem activityItem,
        FWSelectorBarItem settingsItem)
    {
        return routeKey switch
        {
            "activity" => activityItem,
            "settings" => settingsItem,
            _ => overviewItem
        };
    }

    private static FWBorder CreateNavigationExampleCard(FluentIconRegular icon, string title, string description, UIElement content, double width = 600)
    {
        return GallerySampleCard.Create(icon, title, description, content, code: CreateSampleCode(title), width: width);
    }

    private static string CreateSampleCode(string title)
    {
        return title switch
        {
            "FWNavigationView" => "<FWNavigationView PaneTitle=\"FluentJalium\" Header=\"NavigationView\">\n    <FWNavigationViewItem Content=\"Dashboard\" />\n    <FWNavigationViewItem Content=\"Controls\" />\n</FWNavigationView>",
            "Pane modes and hierarchy" => "<FWNavigationView PaneDisplayMode=\"LeftCompact\" IsPaneOpen=\"False\">\n    <FWNavigationViewItem Content=\"Document options\" IsExpanded=\"True\" SelectsOnInvoked=\"False\" />\n</FWNavigationView>",
            "FWTabControl" => "<FWTabControl TabStripPlacement=\"Top\" IsSwipeEnabled=\"True\">\n    <FWTabItem Header=\"Overview\" />\n    <FWTabItem Header=\"Details\" />\n</FWTabControl>",
            "FWNavigationService + FWFrame" => "var navigationView = new FWNavigationView();\nvar frame = new FWFrame();\nvar service = new FWNavigationService\n{\n    PageTypeProvider = (route, parameter) => route.RouteKey == \"details\"\n        ? typeof(GalleryNavigationProviderPage)\n        : route.PageType\n};\nservice.RegisterRoute(new FWNavigationViewItem { Content = \"Overview\", RouteKey = \"overview\" }, typeof(GalleryNavigationOverviewPage));\nservice.RegisterRoute(new FWNavigationViewItem { Content = \"Details\", RouteKey = \"details\" }, typeof(GalleryNavigationDetailsPage));\nservice.Attach(navigationView, frame);\nservice.NavigateToRoute(\"overview\");\nservice.NavigateToRoute(\"details\");",
            "App shell walkthrough" => "var navigationView = new FWNavigationView { PaneDisplayMode = NavigationViewPaneDisplayMode.LeftCompact };\nvar frame = new FWFrame();\nvar service = new FWNavigationService\n{\n    PageTypeProvider = (route, parameter) => route.RouteKey == \"settings\"\n        ? typeof(SettingsShellPage)\n        : route.PageType\n};\nvar breadcrumb = new FWBreadcrumbBar { ItemsSource = pathSegments };\nvar search = new FWAutoSuggestBox { ItemsSource = routeSuggestions };\nvar tabView = new FWTabView { TabWidthMode = FWTabViewWidthMode.SizeToContent };\nvar pager = new FWPipsPager { NumberOfPages = 3 };\nvar selector = new FWSelectorBar();\nservice.Attach(navigationView, frame);\nservice.NavigateToRoute(\"overview\");\nvar diagnostics = service.GetDiagnostics();",
            "FWBreadcrumbBar" => "<FWBreadcrumbBar ItemsSource=\"{Binding PathSegments}\" MaxItems=\"5\" ItemClicked=\"OnBreadcrumbClicked\" />",
            "FWPipsPager" => "<FWPipsPager NumberOfPages=\"10\" SelectedPageIndex=\"0\" MaxVisiblePips=\"5\" SelectedIndexChanged=\"OnPageChanged\" />",
            "FWSelectorBar" => "<FWSelectorBar SelectionIndicatorPlacement=\"Auto\">\n    <FWSelectorBarItem Text=\"Overview\" />\n    <FWSelectorBarItem Text=\"Activity\" />\n</FWSelectorBar>",
            "FWTabView / FWTabViewItem" => "<FWTabView Header=\"Workspaces\" TabWidthMode=\"SizeToContent\" CloseButtonOverlayMode=\"Always\">\n    <FWTabViewItem Header=\"Overview\" IsClosable=\"False\" />\n    <FWTabViewItem Header=\"Details\" />\n</FWTabView>",
            "FWTitleBar" => "<FWTitleBar Title=\"FluentJalium Gallery\" IsShowIcon=\"False\" ShowMinimizeButton=\"True\" ShowMaximizeButton=\"True\" ShowCloseButton=\"True\" />",
            "Material navigation shell" => "<FWFluentMaterialSurface MaterialKind=\"LiquidGlass\">\n    <FWNavigationView PaneDisplayMode=\"LeftCompact\" />\n    <FWTabControl />\n</FWFluentMaterialSurface>",
            _ => "<FWNavigationView />"
        };
    }

    private static FWWrapPanel CreateNavigationButtonRow(params FWButton[] buttons)
    {
        var row = new FWWrapPanel
        {
            HorizontalSpacing = 8,
            VerticalSpacing = 8
        };

        foreach (var button in buttons)
        {
            row.Children.Add(button);
        }

        return row;
    }

    private static FWButton CreateNavigationActionButton(FluentIconRegular icon, string text, Action action)
    {
        var button = new FWButton
        {
            Content = new FWStackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 6,
                Children =
                {
                    CreateIcon(icon, 16, ThemeBrush("TextPrimary")),
                    new FWTextBlock
                    {
                        Text = text,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                }
            }
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static TextBlock CreateNavigationOutput(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        };
    }

    private static FWBorder CreateNavigationStatus(TextBlock status)
    {
        return new FWBorder
        {
            Background = ThemeBrush("LayerFillColorDefaultBrush"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children =
                {
                    CreateIcon(FluentIconRegular.InfoSparkle24, 18, ThemeBrush("TextSecondary")),
                    status
                }
            }
        };
    }

    private static FWStackPanel CreateSection(string title)
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 14,
            Children =
            {
                new FWStackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 10,
                    Children =
                    {
                        CreateIcon(FluentIconRegular.Navigation24, 24, ThemeBrush("TextPrimary")),
                        new FWTextBlock
                        {
                            Text = title,
                            FontSize = 22,
                            Foreground = ThemeBrush("TextPrimary"),
                            VerticalAlignment = VerticalAlignment.Center
                        }
                    }
                }
            }
        };
    }

    private static FluentIcon CreateIcon(FluentIconRegular icon, double size = 20, Brush? foreground = null)
    {
        return FluentIconFactory.Regular(icon, size, foreground ?? ThemeBrush("TextPrimary"));
    }

    private static Brush ThemeBrush(string key)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Brush brush)
        {
            return brush;
        }

        return new SolidColorBrush(Colors.Transparent);
    }

    private static readonly string[] ShellRouteSuggestions =
    [
        "Overview",
        "Activity",
        "Settings"
    ];
}
