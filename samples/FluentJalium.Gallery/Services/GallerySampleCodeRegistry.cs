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
        ["layout.settingscard"] = """
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
    VerticalCacheLength = 200
};
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

navigationView.MenuItems.Add(new FWNavigationViewItem
{
    Content = "Overview",
    Icon = FluentIconFactory.Regular(FluentIconRegular.Home24)
});
navigationView.FooterMenuItems.Add(new FWNavigationViewItem
{
    Content = "Settings",
    Icon = FluentIconFactory.Regular(FluentIconRegular.Settings24)
});

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
        ["disclosure.taskdialog"] = """
var deleteCommand = new RelayCommand(parameter => DeleteTemporaryCache(parameter));
var archiveCommand = new RelayCommand(parameter => ArchiveTemporaryCache(parameter));
var cancelCommand = new RelayCommand(parameter => Debug.WriteLine(parameter));

var taskDialog = new FWTaskDialog
{
    Title = "Delete temporary layout cache?",
    Subtitle = "FWTaskDialog keeps awaitable command semantics.",
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
""",
        ["disclosure.settings.teachingtip"] = """
var expander = new FWSettingsExpander
{
    Header = "Appearance",
    Description = "Theme and material options",
    IsExpanded = true
};

expander.AddSetting(new FWSettingsCard
{
    Header = "App theme",
    Description = "Use system setting"
});
expander.AddSetting(new FWSettingsCard
{
    Header = "Window material",
    Description = "Preview shell backdrop",
    IsClickEnabled = true,
    Command = PreviewMaterialCommand
});

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
var host = new FWSnackbarHost
{
    Width = 470,
    MaxVisibleSnackbars = 2,
    Placement = FWSnackbarPlacement.Bottom,
    Spacing = 8
};
var service = new FWSnackbarService();
service.SetHost(host);

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
