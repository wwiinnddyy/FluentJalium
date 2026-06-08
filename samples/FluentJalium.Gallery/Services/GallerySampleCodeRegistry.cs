using FluentJalium.Gallery.Models;

namespace FluentJalium.Gallery.Services;

internal static class GallerySampleCodeRegistry
{
    private static readonly IReadOnlyDictionary<string, string> SampleCodeByKey = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["catalog.filter.all"] = CreateCatalogFilterSample("AllControls"),
        ["catalog.filter.new"] = CreateCatalogFilterSample("New"),
        ["catalog.filter.updated"] = CreateCatalogFilterSample("Updated"),
        ["catalog.filter.preview"] = CreateCatalogFilterSample("Preview"),
        ["catalog.filter.diagnostic"] = CreateCatalogFilterSample("Diagnostic"),
        ["buttons.commandbar"] = """
var primaryButton = new FWButton { Content = "Save" };
var splitButton = new FWSplitButton
{
    Content = "Publish",
    Flyout = CreatePublishFlyout()
};

var commandBar = new FWCommandBar
{
    Density = FWCommandSurfaceDensity.Compact
};
commandBar.PrimaryCommands.Add(new FWAppBarButton { Label = "Copy", Icon = IconGlyph(FluentIconRegular.Copy24) });
commandBar.PrimaryCommands.Add(new FWAppBarToggleButton { Label = "Pin", Icon = IconGlyph(FluentIconRegular.Pin24), IsChecked = true });
commandBar.SecondaryCommands.Add(new FWAppBarButton { Label = "Rename", Icon = IconGlyph(FluentIconRegular.Rename24) });

var toolBar = new FWToolBar
{
    Density = FWCommandSurfaceDensity.Compact,
    Items = { new FWButton { Content = "Run" }, new FWToggleButton { Content = "Live" } }
};
""",
        ["switches.toggle"] = """
var toggleButton = new FWToggleButton
{
    Content = "Live preview",
    IsChecked = true
};

var toggleSwitch = new FWToggleSwitch
{
    Header = "Use system theme",
    OnContent = "On",
    OffContent = "Off",
    IsChecked = true
};

var indeterminate = new FWCheckBox
{
    Content = "Apply to selected profiles",
    IsThreeState = true,
    IsChecked = null
};
""",
        ["textinput.autosuggestbox"] = """
var autoSuggestBox = new FWAutoSuggestBox
{
    Width = 320,
    ItemsSource = new[] { "Fluent tokens", "Fluent controls", "WinUI Gallery", "AutoSuggestBox" },
    Text = "Fl",
    PlaceholderText = "Search controls",
    FilterMode = AutoCompleteFilterMode.Contains,
    MinimumPrefixLength = 1,
    Density = FWTextInputDensity.Comfortable
};

autoSuggestBox.TextChanged += (_, _) =>
{
    var matchCount = autoSuggestBox.FilteredItems.Count;
};

autoSuggestBox.SuggestionChosen += (_, args) =>
{
    var selected = args.SelectedItem;
};

autoSuggestBox.QuerySubmitted += (_, args) =>
{
    var query = args.QueryText;
    var chosen = args.ChosenSuggestion;
};

autoSuggestBox.SetQueryText("Calendar", FWAutoSuggestBoxTextChangeReason.UserInput);
autoSuggestBox.RequestSuggestionChosen(autoSuggestBox.FilteredItems.FirstOrDefault());
autoSuggestBox.RequestQuerySubmitted();
""",
        ["selection.radiobuttons"] = """
var radioButtons = new FWRadioButtons
{
    Header = "Launch target",
    Width = 260,
    Density = FWSelectionDensity.Comfortable
};

radioButtons.Items.Add("Windows desktop");
radioButtons.Items.Add("Gallery shell");
radioButtons.Items.Add("Control playground");
radioButtons.SelectedIndex = 1;
""",
        ["range.slider.progress"] = """
var slider = new FWSlider
{
    Width = 320,
    Density = FWRangeDensity.Comfortable,
    Minimum = 0,
    Maximum = 100,
    Value = 64,
    TickFrequency = 10,
    IsSnapToTickEnabled = true
};

var rangeSlider = new FWRangeSlider
{
    Width = 320,
    Minimum = 0,
    Maximum = 100,
    RangeStart = 24,
    RangeEnd = 76,
    MinimumRange = 8
};

var progress = new FWProgressBar
{
    Minimum = 0,
    Maximum = 100,
    Value = slider.Value
};

var ring = new FWProgressRing
{
    IsIndeterminate = true,
    RingSize = FWProgressRingSize.Large
};
""",
        ["datetime.calendarpicker.calendarview"] = """
var reviewDate = new FWCalendarDatePicker
{
    Header = "Review date",
    PlaceholderText = "Choose a date",
    DisplayDateStart = DateTime.Today,
    DisplayDateEnd = DateTime.Today.AddDays(90),
    SelectedDate = DateTime.Today.AddDays(7),
    SelectedDateFormat = DatePickerFormat.Long
};

var calendarView = new FWCalendarView
{
    DisplayDate = DateTime.Today,
    DisplayDateStart = DateTime.Today,
    DisplayDateEnd = DateTime.Today.AddDays(90),
    SelectedDate = reviewDate.SelectedDate,
    FirstDayOfWeek = DayOfWeek.Monday,
    IsTodayHighlighted = true
};
""",
        ["inputmedia.color.ink.media"] = """
var picker = new FWColorPicker
{
    Color = Color.FromRgb(0x00, 0x78, 0xD4),
    IsAlphaEnabled = true,
    IsColorPreviewVisible = true,
    IsHexInputVisible = true
};

var inkCanvas = new FWInkCanvas
{
    Width = 330,
    Height = 180,
    EditingMode = InkCanvasEditingMode.Ink,
    DefaultStrokeTaperMode = StrokeTaperMode.TaperedEnd
};

var presenter = new FWInkPresenter
{
    Strokes = inkCanvas.Strokes
};

var media = new FWMediaElement
{
    LoadedBehavior = MediaState.Manual,
    Stretch = Stretch.Uniform,
    ScrubbingEnabled = true
};
""",
        ["selectors.properties"] = """
var selector = new FWTreeSelector
{
    PlaceholderText = "Control families",
    SelectionMode = SelectionMode.Multiple,
    IsSearchEnabled = true,
    SearchText = "data"
};
selector.Items.Add(new FWTreeSelectorItem { Header = "Inputs", IsChecked = true });
selector.Items.Add(new FWTreeSelectorItem { Header = "Navigation" });

var propertyGrid = new FWPropertyGrid
{
    SelectedObject = new
    {
        Name = "FWTreeSelector",
        Category = "Collections",
        IsPreview = false
    },
    SearchText = "density"
};
""",
        ["datainspectors.viewers"] = """
var diffViewer = new FWDiffViewer
{
    OriginalText = previousDocument,
    ModifiedText = currentDocument,
    ShowMinimap = true
};

var hexEditor = new FWHexEditor
{
    Data = Encoding.UTF8.GetBytes("FluentJalium"),
    BytesPerRow = 16,
    IsReadOnly = true
};

var jsonViewer = new FWJsonTreeViewer
{
    JsonText = "{ \"control\": \"FWJsonTreeViewer\", \"state\": \"ready\" }",
    ExpandDepth = 2
};
""",
        ["motion.transitions"] = """
var transitionHost = new FWTransitioningContentControl
{
    TransitionProfile = FWContentTransitionProfile.Entrance,
    Content = new FWTextBlock { Text = "Incoming content" }
};

transitionHost.ApplyTransitionProfile(FWContentTransitionProfile.BackNavigation);

var connected = new FWConnectedAnimationService();
var options = FWConnectedAnimationOptions.CreateProfile(FWConnectedAnimationProfile.Navigation);
connected.PrepareToAnimate("card", sourceElement, options);
connected.TryStart("card", destinationElement);
""",
        ["layout.splitview.settingscard"] = """
var splitView = new FWSplitView
{
    DisplayMode = FWSplitViewDisplayMode.CompactInline,
    PanePlacement = FWSplitViewPanePlacement.Left,
    IsPaneOpen = false,
    OpenPaneLength = 208,
    CompactPaneLength = 56,
    Pane = new FWStackPanel
    {
        Spacing = 8,
        Children =
        {
            new FWButton { Content = "Overview" },
            new FWButton { Content = "Reports" }
        }
    },
    Content = new FWBorder
    {
        Padding = new Thickness(14),
        Child = new FWTextBlock { Text = "Primary content" }
    }
};

splitView.TogglePane();
var paneLength = splitView.ActualPaneLength;

var card = new FWSettingsCard
{
    Header = "Display mode",
    Description = "Switch between wide, tall, and single-pane layouts.",
    Content = new FWButton { Content = "Configure" },
    IsClickEnabled = true,
    Command = ConfigureDisplayModeCommand,
    CommandParameter = "display-mode",
    ClickMode = ClickMode.Release
};

var canExecute = card.CanExecute;
var automation = card.GetAutomationDiagnostics();
Debug.WriteLine($"SettingsCard automation: {automation.Name}; invoke: {automation.IsInvokePatternAvailable}.");

card.PerformClick();
""",
        ["visuals.icons.richtext.personpicture.markdown.qrcode.shapes"] = """
var visuals = new FWStackPanel
{
    Orientation = Orientation.Vertical,
    Spacing = 12,
    Children =
    {
        new FWPersonPicture
        {
            DisplayName = "Rhea Holloway",
            Initials = "RH",
            BadgeNumber = 2,
            Width = 64,
            Height = 64
        },
        new FWMarkdown
        {
            Width = 320,
            Text = "### Fluent visuals\nUse Markdown, QR, avatar, and icon primitives together."
        },
        new FWQRCode
        {
            Text = "https://jalium.dev/fluent",
            Width = 128,
            Height = 128,
            QuietZoneModules = 3,
            ErrorCorrectionLevel = QRCodeErrorCorrectionLevel.Q
        }
    }
};
""",
        ["interaction.scrollviewer.swipe.splitter"] = """
var scrollViewer = new FWScrollViewer
{
    Width = 360,
    Height = 180,
    VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
    IsScrollBarAutoHideEnabled = false,
    Content = CreateScrollablePreview()
};

scrollViewer.ScrollChanged += (_, args) =>
{
    Debug.WriteLine($"ScrollViewer offset: {args.VerticalOffset}; extent: {args.ExtentHeight}.");
};

scrollViewer.ScrollToVerticalOffset(96);

var leftItems = new SwipeItems { Mode = SwipeMode.Reveal };
leftItems.Add(new SwipeItem
{
    Text = "Archive",
    IconSource = IconGlyph(FluentIconRegular.Archive24),
    Command = ArchiveCommand,
    CommandParameter = "archive-row"
});

var rightItems = new SwipeItems { Mode = SwipeMode.Execute };
rightItems.Add(new SwipeItem
{
    Text = "Delete",
    IconSource = IconGlyph(FluentIconRegular.Delete24),
    Command = DeleteCommand,
    CommandParameter = "delete-row",
    BehaviorOnInvoked = BehaviorOnInvoked.Close
});

var swipeControl = new FWSwipeControl
{
    Density = FWInteractionDensity.Compact,
    LeftItems = leftItems,
    RightItems = rightItems,
    Content = new FWTextBlock { Text = "Swipe this row for contextual commands." }
};

var splitter = new FWGridSplitter
{
    Density = FWInteractionDensity.Comfortable,
    ResizeDirection = GridResizeDirection.Columns,
    ResizeBehavior = GridResizeBehavior.PreviousAndNext,
    KeyboardIncrement = 12
};
""",
        ["advancedcollections.itemsrepeater"] = """
var repeater = new FWItemsRepeater
{
    ItemsSource = new[] { "Overview", "Inputs", "Charts", "Status" },
    ItemTemplate = CreateItemTemplate(),
    Layout = new StackLayout
    {
        Orientation = Orientation.Vertical,
        Spacing = 8
    },
    HorizontalCacheLength = 200,
    VerticalCacheLength = 80,
    EstimatedItemExtent = 48
};

var scrollViewer = new FWScrollViewer
{
    Content = repeater
};

repeater.AttachViewport(scrollViewer);
var diagnostics = repeater.GetDiagnostics();
Debug.WriteLine($"Viewport source: {diagnostics.RealizationSource}; window: {diagnostics.FirstRealizedIndex}-{diagnostics.LastRealizedIndex}.");
repeater.ApplyViewport(0, 160, Orientation.Horizontal);

repeater.RealizeRange(0, 5);
repeater.ResetRealizationWindow();
""",
        ["collections.gridview"] = """
var emptyList = new FWListView
{
    Density = FWCollectionDensity.Compact,
    ItemsSource = Array.Empty<GalleryRow>()
};

var loadingGrid = new FWGridView
{
    Density = FWCollectionDensity.Compact,
    IsEnabled = false,
    ItemsSource = loadingRows
};

var loadingProgress = new FWProgressBar
{
    Density = FWRangeDensity.Compact,
    IsIndeterminate = true
};

var groupedList = new FWListBox
{
    Density = FWCollectionDensity.Compact,
    ItemsSource = groupedRows
};

var compactTable = new FWDataGrid
{
    Density = FWDataGridDensity.Compact,
    AutoGenerateColumns = false,
    ItemsSource = rows
};
""",
        ["charts.family"] = """
var chart = new FWLineChart
{
    Title = "Pipeline signal",
    Width = 482,
    Height = 220,
    LineSmoothing = true,
    ShowArea = true,
    ShowDataPoints = true,
    XAxis = CreateCategoryAxis("Day", "Mon", "Tue", "Wed", "Thu", "Fri"),
    YAxis = CreateNumericAxis("Signals", min: 0, max: 100)
};

chart.Series.Add(CreateLineSeries(
    "Current",
    PaletteBrush(0),
    Point("Mon", 62),
    Point("Tue", 70),
    Point("Wed", 66),
    Point("Thu", 82),
    Point("Fri", 78)));
""",
        ["navigation.breadcrumb.pips.selector.tabview.titlebar"] = """
var navigationView = new FWNavigationView
{
    PaneTitle = "FluentJalium",
    Header = "NavigationView",
    OpenPaneLength = 220,
    CompactPaneLength = 48
};

var frame = new FWFrame();
var navigationService = new FWNavigationService();
var overviewItem = new FWNavigationViewItem
{
    Content = "Overview",
    RouteKey = "overview",
    Icon = FluentIconFactory.Regular(FluentIconRegular.Home24)
};
var settingsItem = new FWNavigationViewItem
{
    Content = "Settings",
    RouteKey = "settings",
    Icon = FluentIconFactory.Regular(FluentIconRegular.Settings24)
};

navigationView.MenuItems.Add(overviewItem);
navigationView.FooterMenuItems.Add(settingsItem);
navigationService.RegisterRoute(overviewItem, typeof(OverviewPage), "overview");
navigationService.RegisterRoute(settingsItem, typeof(SettingsPage), "settings");
navigationService.Attach(navigationView, frame);
navigationService.NavigateToRoute("overview");

var diagnostics = navigationService.GetDiagnostics();
Debug.WriteLine($"Navigation route: {diagnostics.CurrentRouteKey}; back: {diagnostics.CanGoBack}.");

var pager = new FWPipsPager
{
    NumberOfPages = 10,
    MaxVisiblePips = 5,
    SelectedPageIndex = 0
};
""",
        ["materials.windowbackdrop"] = """
var windowSurface = new FWFluentWindowSurface
{
    WindowMaterialProfile = FWFluentWindowMaterialProfile.MicaShell,
    WindowBackdropKind = FWFluentWindowBackdropKind.Mica,
    AutoApplyWindowBackdrop = true,
    Child = new FWNavigationView
    {
        PaneTitle = "Fluent shell"
    }
};

// For an explicit host window:
windowSurface.ApplyWindowBackdrop(window);
""",
        ["materials.derivedsurfaces"] = """
var card = new FWCardSurface
{
    Child = new FWTextBlock { Text = "Stable card content" }
};

var flyout = new FWFlyoutSurface
{
    Child = new FWButton { Content = "Command" }
};

var focusGlass = new FWFocusGlassSurface
{
    Child = new FWTextBlock { Text = "Focused preview" }
};

var windowSurface = new FWFluentWindowSurface
{
    AutoApplyWindowBackdrop = false,
    WindowMaterialProfile = FWFluentWindowMaterialProfile.MicaShell,
    WindowBackdropKind = FWFluentWindowBackdropKind.Mica
};
""",
        ["materialprimitives.backdrop"] = """
var surface = new FWBorder
{
    Width = 320,
    Height = 180,
    CornerRadius = new CornerRadius(8),
    ClipToBounds = true,
    Child = new FWBackdrop
    {
        Type = FWBackdropType.Mica,
        TintColor = Color.FromRgb(0xF3, 0xF3, 0xF3),
        TintOpacity = 0.8,
        LuminosityOpacity = 0.85
    }
};
""",
        ["animatedcontrols.motion"] = """
var icon = new FWAnimatedIcon
{
    Width = 48,
    Height = 48,
    AutoPlay = true,
    State = "Normal",
    MirroredWhenRightToLeft = true,
    FallbackIconSource = "Play"
};

var player = new FWAnimatedVisualPlayer
{
    Width = 120,
    Height = 120,
    AutoPlay = true,
    IsLooping = true,
    FallbackContent = icon
};
""",
        ["menus.flyout.commandbar"] = """
var contentFlyout = new FWFlyout
{
    Placement = FlyoutPlacementMode.Right,
    Density = FWMenuDensity.Spacious,
    Content = new FWStackPanel
    {
        Spacing = 8,
        Children =
        {
            new FWTextBlock { Text = "Preview settings" },
            new FWButton { Content = "Apply" }
        }
    }
};

var menuBar = new FWMenuBar();
menuBar.Items.Add(new FWMenuBarItem
{
    Title = "File",
    Items =
    {
        new FWMenuFlyoutItem { Text = "New", Icon = IconGlyph(FluentIconRegular.DocumentAdd24) },
        new FWMenuFlyoutItem { Text = "Save", Icon = IconGlyph(FluentIconRegular.Save24) },
        new FWMenuFlyoutSeparator(),
        new FWMenuFlyoutItem { Text = "Exit" }
    }
});

var flyout = new FWMenuFlyout();
flyout.Items.Add(new FWMenuFlyoutItem { Text = "Copy" });
flyout.Items.Add(new FWMenuFlyoutSubItem
{
    Text = "Export",
    Items = { new FWMenuFlyoutItem { Text = "PDF document" } }
});

var commandFlyout = new FWCommandBarFlyout { AlwaysExpanded = true };
commandFlyout.PrimaryCommands.Add(new FWAppBarButton { Label = "Copy", Icon = IconGlyph(FluentIconRegular.Copy24) });
commandFlyout.SecondaryCommands.Add(new FWAppBarButton { Label = "Rename", Icon = IconGlyph(FluentIconRegular.Rename24) });
""",
        ["disclosure.taskdialog"] = """
var deleteCommand = new RelayCommand(parameter => DeleteTemporaryCache(parameter));
var archiveCommand = new RelayCommand(parameter => ArchiveTemporaryCache(parameter));
var cancelCommand = new RelayCommand(parameter => Debug.WriteLine(parameter));
var host = new FWTaskDialogHost
{
    IsLightDismissEnabled = true,
    IsFocusTrapEnabled = true,
    RestoreFocusOnClose = true
};

var taskDialog = new FWTaskDialog
{
    Title = "Delete temporary layout cache?",
    Subtitle = "FWTaskDialogHost wraps command semantics in a modal overlay.",
    PrimaryButtonText = "Delete",
    SecondaryButtonText = "Archive",
    CloseButtonText = "Cancel",
    DefaultButton = FWTaskDialogButton.Primary,
    CancelButton = FWTaskDialogButton.Close,
    PrimaryButtonCommand = deleteCommand,
    PrimaryButtonCommandParameter = "delete-cache",
    SecondaryButtonCommand = archiveCommand,
    SecondaryButtonCommandParameter = "archive-cache",
    CloseButtonCommand = cancelCommand,
    CloseButtonCommandParameter = "cancel-dialog",
    IsOpen = true,
    Content = new FWTextBlock
    {
        Text = "Start the async flow, then request a command. Escape routes through CancelButton.",
        TextWrapping = TextWrapping.Wrap
    }
};

taskDialog.PrimaryButtonClick += (_, args) =>
{
    Debug.WriteLine($"Primary requested. Command executed: {args.CommandExecuted}");
};

var showTask = host.ShowAsync(taskDialog);
var diagnostics = host.GetDiagnostics();
Debug.WriteLine($"TaskDialog open: {diagnostics.IsOpen}; focus trap: {diagnostics.IsFocusTrapEnabled}.");

var automation = taskDialog.GetAutomationDiagnostics();
Debug.WriteLine($"TaskDialog automation: {automation.Name}; primary id: {automation.PrimaryButton.AutomationId}; primary help: {automation.PrimaryButton.HelpText}.");
Debug.WriteLine($"Close button automation: {automation.CloseButton.Name}; cancel: {automation.CloseButton.IsCancel}.");

var result = await showTask;
Debug.WriteLine($"TaskDialog completed with {result}.");
""",
        ["disclosure.settings.teachingtip"] = """
public sealed record SettingsRow(
    string Header,
    string Description,
    object HeaderIcon,
    string Value,
    object ActionIcon,
    ICommand? Command = null,
    object? CommandParameter = null,
    bool IsClickEnabled = false);

var expander = new FWSettingsExpander
{
    Header = "Appearance",
    Description = "Theme and material options",
    IsExpanded = true,
    ItemsSource = new[]
    {
        new SettingsRow("App theme", "Use system setting", ThemeIcon, "System", ChevronIcon, IsClickEnabled: true),
        new SettingsRow("Window material", "Preview shell backdrop", MaterialIcon, "Preview", CursorIcon, PreviewMaterialCommand, "material", true),
        new SettingsRow("Control density", "Comfortable touch targets", DensityIcon, "Comfort", ChevronIcon, IsClickEnabled: true)
    }
};

var itemCount = expander.ItemCount;

var target = new FWButton
{
    Content = "Show onboarding tip"
};

var teachingTip = new FWTeachingTip
{
    Target = target,
    Title = "Use metadata filters",
    Subtitle = "TeachingTip anchors guidance to a specific control.",
    IconSource = FluentIconFactory.Regular(FluentIconRegular.Info24),
    ActionButtonContent = "Open docs",
    CloseButtonContent = "Got it",
    PreferredPlacement = TeachingTipPlacementMode.Bottom,
    TailVisibility = TeachingTipTailVisibility.Visible,
    HeroContent = new FWBorder
    {
        Height = 56,
        Background = new SolidColorBrush(Color.FromRgb(0xE8, 0xF2, 0xFF)),
        CornerRadius = new CornerRadius(6)
    },
    Content = new FWTextBlock
    {
        Text = "Pair a short title with one primary action and a clear dismiss affordance.",
        TextWrapping = TextWrapping.Wrap
    }
};

teachingTip.IsOpen = true;
""",
        ["advancedinteraction.scroller"] = """
var scroller = new FWScroller
{
    VerticalScrollMode = ScrollMode.Enabled,
    HorizontalScrollMode = ScrollMode.Disabled,
    VerticalSnapPointsType = SnapPointsType.Mandatory,
    Height = 200,
    Content = new FWStackPanel
    {
        Spacing = 8,
        Children =
        {
            new FWTextBlock { Text = "Scrollable item 1" },
            new FWTextBlock { Text = "Scrollable item 2" },
            new FWTextBlock { Text = "Scrollable item 3" }
        }
    }
};
""",
        ["status.snackbar"] = """
var host = new FWSnackbarOverlayHost
{
    Width = 470,
    OverlayTarget = rootElement,
    OverlayPlacement = PlacementMode.Bottom,
    IsOverlayAutoOpenEnabled = true,
    MaxVisibleSnackbars = 3,
    Placement = FWSnackbarPlacement.Bottom,
    Spacing = 8,
    TransitionProfile = FWContentTransitionProfile.Entrance,
    TransitionOffset = 16
};
var service = new FWSnackbarService();
service.SetHost(host);
host.TransitionRequested += (_, args) => LogTransition(args.Kind, args.Diagnostics);
host.QueueChanged += (_, args) => LogQueueDiagnostics(args.Reason, host.GetDiagnostics());

var snackbar = new FWSnackbar
{
    Severity = ToastSeverity.Success,
    Title = "Preview refreshed",
    Message = "The Gallery host keeps no more than two snackbar items visible.",
    ActionContent = "Open",
    ActionCommand = OpenPreviewCommand,
    ActionCommandParameter = "preview-refresh",
    IsClosable = true,
    IsAutoDismissEnabled = true,
    IsAutoDismissPausedOnPointerOverEnabled = true,
    IsAutoDismissPausedOnFocusEnabled = true,
    Duration = TimeSpan.FromSeconds(8)
};

snackbar.Closing += (_, args) =>
{
    args.Cancel = ShouldKeepSnackbarOpen(args.Reason);
};

var closeTask = service.EnqueueForResultAsync(snackbar);
var diagnostics = host.GetDiagnostics();
var isOverlayOpen = host.IsOverlayOpen;
snackbar.PauseAutoDismiss();
snackbar.ResumeAutoDismiss();
service.CloseCurrent();
var closeReason = await closeTask;
"""
    };

    public static bool TryGetSampleCode(GalleryPageInfo page, out string sampleCode)
    {
        ArgumentNullException.ThrowIfNull(page);

        if (!string.IsNullOrWhiteSpace(page.SampleCodeKey)
            && SampleCodeByKey.TryGetValue(page.SampleCodeKey.Trim(), out var registeredSampleCode))
        {
            sampleCode = registeredSampleCode.Trim();
            return true;
        }

        return TryCreateFallbackSample(page, out sampleCode);
    }

    private static string CreateCatalogFilterSample(string filter)
    {
        return $$"""
var localization = new GalleryLocalizationService();
var pageInfos = GalleryCatalog.CreatePageInfos(localization);

var filterPage = new GalleryCatalogFilterPage(
    GalleryCatalogFilter.{{filter}},
    pageInfos);

var content = filterPage.CreateContent();
""";
    }

    private static bool TryCreateFallbackSample(GalleryPageInfo page, out string sampleCode)
    {
        var controlName = ResolveControlName(page);
        if (string.IsNullOrWhiteSpace(controlName))
        {
            sampleCode = string.Empty;
            return false;
        }

        var title = EscapeSampleString(string.IsNullOrWhiteSpace(page.Title) ? controlName : page.Title);
        var source = EscapeSampleString(page.SourcePath ?? page.UniqueId);

        sampleCode = $$"""
var sample = new {{controlName}}
{
    Width = 320,
    HorizontalAlignment = HorizontalAlignment.Left
};

var card = new FWSettingsCard
{
    Header = "{{title}}",
    Description = "Generated from {{source}}.",
    Content = sample
};
""";
        return true;
    }

    private static string? ResolveControlName(GalleryPageInfo page)
    {
        var sourceLeaf = GetSourceLeaf(page.SourcePath);
        if (IsLikelyControlName(sourceLeaf))
        {
            return sourceLeaf;
        }

        return page.RelatedControls.FirstOrDefault(IsLikelyControlName)
            ?? page.BaseClasses?.FirstOrDefault(IsLikelyControlName);
    }

    private static string? GetSourceLeaf(string? sourcePath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            return null;
        }

        var trimmed = sourcePath.Trim().TrimEnd('/', '\\');
        if (trimmed.Length == 0)
        {
            return null;
        }

        var separatorIndex = trimmed.LastIndexOfAny(new[] { '/', '\\' });
        var leaf = separatorIndex >= 0 ? trimmed[(separatorIndex + 1)..] : trimmed;
        return IsIdentifier(leaf) ? leaf : null;
    }

    private static bool IsLikelyControlName(string? value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && value.StartsWith("FW", StringComparison.Ordinal)
            && IsIdentifier(value);
    }

    private static bool IsIdentifier(string value)
    {
        if (value.Length == 0 || (!char.IsLetter(value[0]) && value[0] != '_'))
        {
            return false;
        }

        return value.All(character => char.IsLetterOrDigit(character) || character == '_');
    }

    private static string EscapeSampleString(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal)
            .Replace("\r", string.Empty, StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal);
    }
}
