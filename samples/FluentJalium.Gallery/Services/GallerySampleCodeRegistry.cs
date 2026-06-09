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
        ["diagnostics.visualqa.coverage"] = """
var families = GalleryVisualQaCoverageCatalog.CreateFamilies();
var snapshot = GalleryVisualQaCoveragePage.CreateSnapshot();
Debug.WriteLine(GalleryVisualQaCoverageCatalog.FormatSnapshot(snapshot));

foreach (var family in families)
{
    Debug.WriteLine(GalleryVisualQaCoveragePage.FormatFamilyCoverage(family));
    Debug.Assert(family.HasReadinessEvidence);
    Debug.Assert(!family.RequiresRenderedQa || !string.IsNullOrWhiteSpace(family.NextAction));
    Debug.Assert(GallerySampleCodeRegistry.ContainsRegisteredSampleCodeKey(family.SampleCodeKey));
}
""",
        ["design.themearchitecture"] = """
FluentThemeManager.Apply(app);

var genericDictionary = FluentThemeManager.GenericThemeResourceName;
var tokenDictionary = FluentThemeManager.FluentResourcesResourceName;
var controlDictionary = FluentThemeManager.FluentControlsResourceName;

// Theme mode restyles Jalium base controls; FW controls expose the public FluentJalium surface.
var surface = new FWStackPanel
{
    Children =
    {
        new FWTextBlock { Text = genericDictionary },
        new FWTextBlock { Text = tokenDictionary },
        new FWTextBlock { Text = controlDictionary }
    }
};
""",
        ["design.colors"] = """
var accentPreview = new FWBorder
{
    Width = 160,
    Height = 72,
    Background = ThemeBrush("AccentFillColorDefaultBrush"),
    BorderBrush = ThemeBrush("AccentControlElevationBorderBrush"),
    BorderThickness = new Thickness(1),
    CornerRadius = new CornerRadius(6)
};

var textPreview = new FWTextBlock
{
    Text = "Text and semantic color resources",
    Foreground = ThemeBrush("TextFillColorPrimaryBrush")
};

var selectionBrush = ThemeBrush("SelectionBackgroundWeak");
var hyperlinkBrush = ThemeBrush("HyperlinkForeground");
""",
        ["design.typography"] = """
var title = new FWTextBlock
{
    Text = "FluentJalium typography",
    FontFamily = FluentThemeManager.CurrentDisplayFontFamily,
    FontSize = ResourceDouble("FluentTitleFontSize", 20)
};

var body = new FWTextBlock
{
    Text = "Body text uses the current theme family and control content size.",
    FontFamily = FluentThemeManager.CurrentBodyFontFamily,
    FontSize = ResourceDouble("ControlContentThemeFontSize", 14)
};

var code = new FWTextBlock
{
    Text = "FWButton | FWTextBox | FWNavigationView",
    FontFamily = FluentThemeManager.CurrentMonoFontFamily,
    FontSize = ResourceDouble("FluentCaptionFontSize", 12)
};
""",
        ["design.geometry"] = """
var card = new FWBorder
{
    Width = 320,
    Padding = new Thickness(14),
    Background = ThemeBrush("LayerFillColorDefaultBrush"),
    BorderBrush = ThemeBrush("ControlElevationBorderBrush"),
    BorderThickness = ResourceThickness("FluentControlBorderThickness", 1),
    CornerRadius = ResourceCornerRadius("CardCornerRadius", 6)
};

var flyoutPresenter = new FWBorder
{
    CornerRadius = ResourceCornerRadius("OverlayCornerRadius", 8),
    BorderThickness = ResourceThickness("FluentOverlayBorderThickness", 1)
};
""",
        ["design.motiontokens"] = """
var transitionHost = new FWTransitioningContentControl
{
    TransitionProfile = FWContentTransitionProfile.Entrance,
    TransitionDuration = ResourceDuration("FluentMotionDurationNormal"),
    TransitionMode = TransitionMode.SlideUp
};

var connectedAnimation = new FWConnectedAnimationService();
var options = FWConnectedAnimationOptions.CreateProfile(FWConnectedAnimationProfile.Navigation);
connectedAnimation.PrepareToAnimate("hero", sourceElement, options);
connectedAnimation.TryStart("hero", destinationElement);
""",
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

autoSuggestBox.AutoSuggestTextChanged += (_, args) =>
{
    Debug.WriteLine($"Text changed: {args.Text}; reason: {args.Reason}");
};

autoSuggestBox.SuggestionChosen += (_, args) =>
{
    var selected = args.SelectedItem;
};

autoSuggestBox.QuerySubmitted += (_, args) =>
{
    var query = args.QueryText;
    var chosen = args.ChosenSuggestion;
    Debug.WriteLine(chosen == null
        ? $"Submitted text query: {query}"
        : $"Submitted chosen suggestion: {chosen}");
};

autoSuggestBox.SetQueryText("Calendar", FWAutoSuggestBoxTextChangeReason.UserInput);
autoSuggestBox.RequestSuggestionChosen(autoSuggestBox.FilteredItems.FirstOrDefault());
autoSuggestBox.RequestQuerySubmitted();

// Formatting recipe: keep phone/license-key masks as Gallery recipes until a public FWMaskedTextBox API is proven.
var phone = new FWTextBox
{
    Text = GalleryTextInputPage.FormatPhoneRecipe("4255550123"),
    PlaceholderText = "(425) 555-0123",
    Density = FWTextInputDensity.Comfortable
};
var licenseKey = new FWTextBox
{
    Text = GalleryTextInputPage.FormatLicenseKeyRecipe("flntjlum2026"),
    PlaceholderText = "FLNT-JLUM-2026",
    CharacterCasing = CharacterCasing.Upper,
    Density = FWTextInputDensity.Comfortable
};

phone.TextChanged += (_, _) =>
{
    phone.Text = GalleryTextInputPage.FormatPhoneRecipe(phone.Text);
};
licenseKey.TextChanged += (_, _) =>
{
    licenseKey.Text = GalleryTextInputPage.FormatLicenseKeyRecipe(licenseKey.Text);
};

var formattingSnapshot = GalleryTextInputPage.CreateFormattingRecipeSnapshot(phone.Text, licenseKey.Text);
Debug.WriteLine(GalleryTextInputPage.FormatFormattingRecipeQa("Formatting recipe QA", formattingSnapshot));
Debug.Assert(formattingSnapshot.IsRecipeOnly);
Debug.Assert(formattingSnapshot.IsReady);
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
        ["patterns.forms"] = """
var displayName = new FWTextBox
{
    PlaceholderText = "Full name",
    Text = "Rhea Holloway"
};

var displayNameLabel = new FWLabel
{
    Content = "Display name",
    Target = displayName
};

var team = new FWAutoSuggestBox
{
    ItemsSource = new[] { "Design Systems", "Platform Engineering", "Gallery Operations" },
    FilterMode = AutoCompleteFilterMode.Contains,
    MinimumPrefixLength = 1,
    PlaceholderText = "Search teams"
};

var accountType = new FWRadioButtons
{
    Header = "Account type",
    SelectedIndex = 0
};
accountType.Items.Add("Member");
accountType.Items.Add("Maintainer");
accountType.Items.Add("Guest");

var validation = new FWInfoBar
{
    Title = "Review required",
    Message = "Display name and email are required before submit.",
    Severity = InfoBarSeverity.Warning,
    IsOpen = true
};

var requireApproval = new FWToggleSwitch
{
    IsOn = true,
    OnContent = "Required",
    OffContent = "Optional"
};
var asyncSubmit = new FWCheckBox
{
    Content = "Async submit progress",
    IsChecked = true
};
var disableFields = new FWCheckBox
{
    Content = "Disable reviewer fields"
};
var progress = new FWProgressBar
{
    Density = FWRangeDensity.Compact,
    Minimum = 0,
    Maximum = 100,
    Value = 0,
    Visibility = Visibility.Collapsed
};

var dataTitle = new FWTextBox
{
    PlaceholderText = "Task title",
    Text = "Gallery release checklist"
};
var dataHours = new FWNumberBox
{
    Minimum = 0,
    Maximum = 24,
    Value = 6,
    PlaceholderText = "Hours"
};
var dataOwner = new FWAutoSuggestBox
{
    ItemsSource = new[] { "Design Systems", "Platform Engineering", "Gallery Operations" },
    FilterMode = AutoCompleteFilterMode.Contains,
    MinimumPrefixLength = 1,
    Text = "Gallery Operations"
};
var validationSummary = new FWInfoBar
{
    Title = "Validation summary",
    IsOpen = true
};
var isDirty = false;
var isSaving = false;

ValidationIssue[] issues = CreateDataFormValidationIssues(
    dataTitle.Text,
    dataHours.Value,
    dataOwner.Text,
    requiresReview: true);
validationSummary.Message = FormatDataFormValidationSummary(
    "Data form validation recipe",
    issues,
    isDirty,
    isSaving);

void ResetDataFormDraft()
{
    dataTitle.Text = "Gallery release checklist";
    dataHours.Value = 6;
    dataOwner.SetQueryText("Gallery Operations", FWAutoSuggestBoxTextChangeReason.ProgrammaticChange);
    isDirty = false;
}

async Task SaveDataFormDraftAsync()
{
    issues = CreateDataFormValidationIssues(dataTitle.Text, dataHours.Value, dataOwner.Text, requiresReview: true);
    validationSummary.Message = FormatDataFormValidationSummary("SaveDataFormDraftAsync", issues, isDirty, isSaving);
    if (issues.Length > 0)
    {
        return;
    }

    isSaving = true;
    await Task.Delay(120);
    isDirty = false;
    isSaving = false;
}

var submitCard = new FWSettingsCard
{
    Header = "Submit action",
    Description = "Invoke the submit command from a settings row.",
    Content = requireApproval,
    IsClickEnabled = true,
    Command = SubmitCommand,
    CommandParameter = "forms.submit"
};

var submitButton = new FWButton { Content = "Submit" };
submitButton.Click += async (_, _) =>
{
    approver.IsEnabled = disableFields.IsChecked != true;
    progress.Visibility = Visibility.Visible;
    submitCard.IsEnabled = false;
    await RunSubmitAsync(displayName.Text, team.Text, accountType.SelectedItem);
    submitCard.IsEnabled = true;
    progress.Visibility = Visibility.Collapsed;
};
var focusButton = new FWButton { Content = "Focus QA" };
focusButton.Click += (_, _) => displayName.Focus();

var diagnostics = submitCard.GetDiagnostics();
Debug.WriteLine($"Forms visual QA: invokable {diagnostics.IsInvokable}; disabled fields {disableFields.IsChecked}; async {asyncSubmit.IsChecked}; focusable {submitCard.Focusable}.");
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

var pickerQa = GalleryDateTimePage.CreateCalendarDatePickerQaSnapshot(reviewDate);
var calendarViewQa = GalleryDateTimePage.CreateCalendarViewQaSnapshot(calendarView);
Debug.WriteLine(GalleryDateTimePage.FormatCalendarDatePickerQa("CalendarDatePicker QA", pickerQa));
Debug.WriteLine(GalleryDateTimePage.FormatCalendarViewQa("CalendarView QA", calendarViewQa));
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

var webView = new FWWebView
{
    Width = 330,
    Height = 180,
    DefaultBackgroundColor = Colors.White,
    ZoomFactor = 1.0
};
webView.NavigateToString(GalleryInputMediaPage.CreateWebViewSampleHtml("FluentJalium WebView"));

FWWebViewDiagnostics webViewDiagnostics = webView.GetDiagnostics();
Debug.WriteLine(GalleryInputMediaPage.FormatWebViewDiagnostics("WebView diagnostics", webViewDiagnostics));
""",
        ["selectors.properties"] = """
var selector = new FWTreeSelector
{
    PlaceholderText = "Control families",
    SelectionMode = SelectionMode.Multiple,
    ShowCheckBoxes = true,
    CheckCascadeMode = TreeSelectorCheckCascadeMode.Cascade,
    IsSearchEnabled = true,
    SearchText = "data"
};
selector.Items.Add(new FWTreeSelectorItem
{
    Header = "Inputs",
    IsExpanded = true,
    IsChecked = true,
    Items =
    {
        new FWTreeSelectorItem { Header = "Text input" },
        new FWTreeSelectorItem { Header = "Selection" }
    }
});
selector.Items.Add(new FWTreeSelectorItem { Header = "Navigation", IsChecked = true });
selector.SelectedItem = "Inputs";

var propertyGrid = new FWPropertyGrid
{
    SelectedObject = new
    {
        Name = "FWTreeSelector",
        Category = "Collections",
        IsPreview = false
    },
    SortMode = PropertyGridSortMode.Categorized,
    ShowSearchBox = true,
    ShowDescription = true,
    ShowToolBar = false,
    SearchText = "density",
    Density = FWPropertyGridDensity.Compact,
    IsReadOnly = true
};

var propertySurface = new FWFluentMaterialSurface
{
    MaterialKind = FWFluentMaterialKind.LiquidGlass,
    Child = propertyGrid
};

var propertySnapshot = GallerySelectorsPropertiesPage.CreateSelectorsPropertiesQaSnapshot(
    selector,
    propertyGrid,
    propertySurface);
Debug.WriteLine(GallerySelectorsPropertiesPage.FormatSelectorsPropertiesQa("Selectors and properties QA", propertySnapshot));
Debug.Assert(propertySnapshot.IsReady);
""",
        ["datainspectors.viewers"] = """
var diffViewer = new FWDiffViewer
{
    OriginalText = previousDocument,
    ModifiedText = currentDocument,
    ViewMode = DiffViewMode.Unified,
    ShowMinimap = true,
    IsReadOnly = true
};

var hexEditor = new FWHexEditor
{
    Data = Encoding.UTF8.GetBytes("FluentJalium"),
    BytesPerRow = 8,
    ShowDataInterpretation = true,
    SelectionLength = 2,
    IsReadOnly = true
};

var jsonViewer = new FWJsonTreeViewer
{
    JsonText = "{ \"control\": \"FWJsonTreeViewer\", \"state\": \"ready\" }",
    ExpandDepth = 2,
    MaxRenderDepth = 8,
    ShowTypeIndicators = true,
    ShowItemCount = true,
    IsEditable = false
};

var workbenchSurface = new FWFluentMaterialSurface
{
    MaterialKind = FWFluentMaterialKind.LiquidGlass,
    Child = new FWStackPanel
    {
        Children = { diffViewer, hexEditor, jsonViewer }
    }
};

var workbenchSnapshot = GalleryDataInspectorsPage.CreateDataInspectorWorkbenchSnapshot(
    diffViewer,
    hexEditor,
    jsonViewer,
    workbenchSurface);
Debug.WriteLine(GalleryDataInspectorsPage.FormatDataInspectorWorkbenchQa("Data Inspectors workbench QA", workbenchSnapshot));
Debug.Assert(workbenchSnapshot.IsReady);
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
FWSplitViewDiagnostics splitViewDiagnostics = splitView.GetDiagnostics();
Debug.WriteLine(GalleryContentLayoutPage.FormatSplitViewDiagnostics(
    "SplitView toggled",
    splitViewDiagnostics));

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
var hoverCard = new FWSettingsCard
{
    Header = "Sync layout",
    Description = "Hover mode keeps the settings row invokable without a nested button.",
    IsClickEnabled = true,
    ClickMode = ClickMode.Hover
};
var disabledCard = new FWSettingsCard
{
    Header = "Enterprise policy",
    Description = "Disabled row keeps icon/text/action alignment visible.",
    IsClickEnabled = true,
    IsEnabled = false
};

var canExecute = card.CanExecute;
var automation = card.GetAutomationDiagnostics();
Debug.WriteLine($"SettingsCard automation: {automation.Name}; invoke: {automation.IsInvokePatternAvailable}.");

var interaction = card.GetDiagnostics();
Debug.WriteLine($"SettingsCard interaction: invokable {interaction.IsInvokable}; pressed {interaction.IsInteractionPressed}; click mode {interaction.ClickMode}.");
Debug.WriteLine($"Settings visual QA: primary {card.GetDiagnostics().IsInvokable}; hover {hoverCard.GetDiagnostics().ClickMode}; disabled {disabledCard.GetDiagnostics().IsEnabled}.");

var twoPaneView = new FWTwoPaneView
{
    Mode = FWTwoPaneViewMode.Wide,
    PanePriority = FWTwoPaneViewPriority.Pane1,
    Pane1 = new FWTextBlock { Text = "Navigation pane" },
    Pane2 = new FWTextBlock { Text = "Settings pane" }
};
FWTwoPaneViewDiagnostics twoPaneDiagnostics = twoPaneView.GetDiagnostics();
Debug.WriteLine($"TwoPaneView diagnostics: requested {twoPaneDiagnostics.RequestedMode}; actual {twoPaneDiagnostics.ActualMode}; visible {twoPaneDiagnostics.VisiblePane}.");
var settingsVisualQa = GalleryContentLayoutPage.CreateSettingsVisualQaSnapshot(
    twoPaneView,
    card,
    hoverCard,
    disabledCard);
Debug.WriteLine(GalleryContentLayoutPage.FormatSettingsVisualQa("Settings visual QA snapshot", settingsVisualQa));
Debug.WriteLine($"Settings visual QA ready: {settingsVisualQa.IsSettingsVisualQaReady}; adaptive {settingsVisualQa.HasAdaptiveLayoutEvidence}; command {settingsVisualQa.HasPrimaryCommandEvidence}; hover {settingsVisualQa.HasHoverStateEvidence}; disabled {settingsVisualQa.HasDisabledRowEvidence}; automation {settingsVisualQa.HasAutomationEvidence}.");

card.PerformClick();

var scrollViewer = new FWScrollViewer
{
    Height = 120,
    Content = new FWStackPanel
    {
        Children =
        {
            new FWTextBlock { Text = "Parallax source" },
            new FWTextBlock { Text = "Scroll to update depth" }
        }
    }
};
var sourceScroller = new FWScroller();
sourceScroller.AttachScrollViewer(scrollViewer);

var parallaxView = new FWParallaxView
{
    Source = sourceScroller,
    HorizontalShift = 18,
    VerticalShift = 28,
    IsHorizontalShiftEnabled = true
};
parallaxView.RefreshProgressFromSource();
FWParallaxViewDiagnostics parallaxDiagnostics = parallaxView.GetDiagnostics();
Debug.WriteLine($"Parallax diagnostics: source {parallaxDiagnostics.SourceKind}; progress {parallaxDiagnostics.Progress:P0}; offset {parallaxDiagnostics.CurrentOffset}.");
""",
        ["visuals.icons.richtext.personpicture.markdown.qrcode.shapes"] = """
var person = new FWPersonPicture
{
    DisplayName = "Rhea Holloway",
    Initials = "RH",
    BadgeNumber = 2,
    Width = 64,
    Height = 64
};

var markdown = new FWMarkdown
{
    Width = 320,
    Text = "### Fluent visuals\nUse [Markdown](https://jalium.dev/fluent), QR, avatar, and icon primitives together.",
    OpenLinksExternally = false
};

markdown.LinkClicked += (_, args) =>
{
    args.Handled = true;
    Debug.WriteLine($"Markdown link handled: {args.Uri}");
};

var qrCode = new FWQRCode
{
    Text = "https://jalium.dev/fluent",
    Width = 128,
    Height = 128,
    QuietZoneModules = 3,
    ErrorCorrectionLevel = QRCodeErrorCorrectionLevel.Q,
    ModuleShape = QRModuleShape.RoundedSquare,
    EyeShape = QREyeShape.Rounded
};

var accentStroke = ThemeBrush("AccentFillColorDefaultBrush");
var translucentFill = ThemeBrush("AccentFillColorSelectedTextBackgroundBrush");
var rectangle = new FWRectangle
{
    Width = 84,
    Height = 52,
    RadiusX = 8,
    RadiusY = 8,
    Fill = translucentFill,
    Stroke = accentStroke,
    StrokeThickness = 1.5
};
var ellipse = new FWEllipse
{
    Width = 58,
    Height = 58,
    Fill = translucentFill,
    Stroke = accentStroke,
    StrokeThickness = 1.5
};
var line = new FWLine
{
    X1 = 4,
    Y1 = 42,
    X2 = 84,
    Y2 = 10,
    Stroke = accentStroke,
    StrokeThickness = 3
};
var polyline = new FWPolyline
{
    Points = ShapePointCollection.Parse("0,40 18,14 38,34 58,8 82,28"),
    Stroke = accentStroke,
    StrokeThickness = 3
};
var polygon = new FWPolygon
{
    Points = ShapePointCollection.Parse("8,44 28,8 52,22 74,44"),
    Fill = translucentFill,
    Stroke = accentStroke,
    StrokeThickness = 1.5
};
var path = new FWPath
{
    Data = "M 4,36 C 16,6 38,6 48,30 S 74,54 84,20",
    Fill = translucentFill,
    Stroke = accentStroke,
    StrokeThickness = 1.5
};

var shapeQa = GalleryVisualsPage.CreateShapeControlsQaSnapshot(
    rectangle,
    ellipse,
    line,
    polyline,
    polygon,
    path);
Debug.WriteLine(GalleryVisualsPage.FormatShapeControlsVisualQa("Shape controls QA", shapeQa));

var visuals = new FWStackPanel
{
    Orientation = Orientation.Vertical,
    Spacing = 12,
    Children =
    {
        person,
        markdown,
        qrCode,
        rectangle,
        ellipse,
        line,
        polyline,
        polygon,
        path
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
var stressRows = Enumerable.Range(1, 1500)
    .Select(index => $"Stress Row {index:0000}")
    .ToArray();

var repeater = new FWItemsRepeater
{
    ItemsSource = stressRows,
    ItemTemplate = CreateItemTemplate(),
    Layout = new StackLayout
    {
        Orientation = Orientation.Vertical,
        Spacing = 8
    },
    HorizontalCacheLength = 240,
    VerticalCacheLength = 320,
    EstimatedItemExtent = 64
};

var scroller = new FWScroller();
var scrollViewer = new FWScrollViewer
{
    Content = repeater
};
scroller.AttachScrollViewer(scrollViewer);

repeater.AttachViewport(scroller);
repeater.ApplyViewport(2560, 480);
FWItemsRepeaterDiagnostics diagnostics = repeater.GetDiagnostics();
var largeListProfile = AdvancedCollectionsPage.CreateItemsRepeaterQaProfile(
    AdvancedCollectionsPage.ItemsRepeaterGalleryScenario.LargeListStress);
var largeListQa = AdvancedCollectionsPage.CreateItemsRepeaterVisualQaSnapshot(
    largeListProfile,
    diagnostics,
    "Applied large-list stress");
Debug.WriteLine($"Stress viewport: {diagnostics.AttachedViewportSource}; items {diagnostics.ItemCount}; window {diagnostics.FirstRealizedIndex}-{diagnostics.LastRealizedIndex}; cache {diagnostics.ActiveCacheLength}.");
Debug.WriteLine(AdvancedCollectionsPage.FormatItemsRepeaterVisualQa(largeListQa));

var horizontalProfile = AdvancedCollectionsPage.CreateItemsRepeaterQaProfile(
    AdvancedCollectionsPage.ItemsRepeaterGalleryScenario.HorizontalVirtualization);
repeater.ItemsSource = AdvancedCollectionsPage.CreateItemsRepeaterSampleItems(horizontalProfile);
repeater.Layout = new StackLayout
{
    Orientation = Orientation.Horizontal,
    Spacing = 10
};
repeater.HorizontalCacheLength = 360;
repeater.VerticalCacheLength = 80;
repeater.EstimatedItemExtent = 180;
repeater.AttachViewport(scrollViewer, Orientation.Horizontal);
repeater.ApplyViewport(720, 540, Orientation.Horizontal);
diagnostics = repeater.GetDiagnostics();
var horizontalQa = AdvancedCollectionsPage.CreateItemsRepeaterVisualQaSnapshot(
    horizontalProfile,
    diagnostics,
    "Applied horizontal virtualization");
Debug.WriteLine($"Horizontal QA: {diagnostics.AttachedViewportSource}/{diagnostics.AttachedViewportOrientation}; window {diagnostics.FirstRealizedIndex}-{diagnostics.LastRealizedIndex}; cache H{diagnostics.HorizontalCacheLength}/V{diagnostics.VerticalCacheLength}.");
Debug.WriteLine(AdvancedCollectionsPage.FormatItemsRepeaterVisualQa(horizontalQa));

repeater.AttachViewport(scroller, Orientation.Horizontal);
repeater.RealizeRange(0, 5);
repeater.ResetRealizationWindow();

var itemsViewRecipe = AdvancedCollectionsPage.CreateCollectionRecipeState(
    AdvancedCollectionsPage.CollectionRecipeKind.ItemsViewSelection);
itemsViewRecipe = AdvancedCollectionsPage.ApplyCollectionRecipeCommand(
    itemsViewRecipe,
    AdvancedCollectionsPage.CollectionRecipeCommand.Next);
itemsViewRecipe = AdvancedCollectionsPage.ApplyCollectionRecipeCommand(
    itemsViewRecipe,
    AdvancedCollectionsPage.CollectionRecipeCommand.Invoke);

var flipViewRecipe = AdvancedCollectionsPage.CreateCollectionRecipeState(
    AdvancedCollectionsPage.CollectionRecipeKind.FlipViewPaging);
var pager = new FWPipsPager
{
    NumberOfPages = flipViewRecipe.ItemCount,
    MaxVisiblePips = 5,
    SelectedPageIndex = flipViewRecipe.PageIndex
};
flipViewRecipe = AdvancedCollectionsPage.ApplyCollectionRecipeCommand(
    flipViewRecipe,
    AdvancedCollectionsPage.CollectionRecipeCommand.Next);
pager.SelectedPageIndex = flipViewRecipe.PageIndex;

var semanticZoomRecipe = AdvancedCollectionsPage.CreateCollectionRecipeState(
    AdvancedCollectionsPage.CollectionRecipeKind.SemanticZoomGrouping);
semanticZoomRecipe = AdvancedCollectionsPage.ApplyCollectionRecipeCommand(
    semanticZoomRecipe,
    AdvancedCollectionsPage.CollectionRecipeCommand.SelectNextGroup);
semanticZoomRecipe = AdvancedCollectionsPage.ApplyCollectionRecipeCommand(
    semanticZoomRecipe,
    AdvancedCollectionsPage.CollectionRecipeCommand.ToggleZoom);

Debug.WriteLine(AdvancedCollectionsPage.CreateCollectionRecipeDiagnosticsText(itemsViewRecipe, diagnostics));
Debug.WriteLine($"ItemsView recipe selected {itemsViewRecipe.SelectedIndex}, invoked {itemsViewRecipe.InvokedIndex}.");
Debug.WriteLine($"FlipView recipe page {flipViewRecipe.PageIndex + 1}/{flipViewRecipe.ItemCount}.");
Debug.WriteLine($"SemanticZoom recipe group {semanticZoomRecipe.GroupIndex}, zoomed out {semanticZoomRecipe.IsZoomedOut}.");

var evaluations = AdvancedCollectionsPage.CreateCollectionNavigationEvaluations();
var candidateControls = evaluations.Select(evaluation => evaluation.CandidateControl).ToArray();
Debug.WriteLine("FWItemsView / FWFlipView / FWSemanticZoom remain recipe-first candidates until public API contracts are proven.");
Debug.WriteLine($"Collection navigation candidates: {string.Join(", ", candidateControls)}.");
Debug.WriteLine(AdvancedCollectionsPage.CreateCollectionNavigationEvaluationSummary(evaluations));
Debug.WriteLine(AdvancedCollectionsPage.CreateCollectionNavigationEvidenceSummary(evaluations));
foreach (var evaluation in evaluations)
{
    Debug.WriteLine(AdvancedCollectionsPage.FormatCollectionNavigationEvaluation(evaluation));
    Debug.WriteLine(AdvancedCollectionsPage.FormatCollectionNavigationEvidence(evaluation));
    Debug.WriteLine($"Recipe evidence: {string.Join(", ", evaluation.RecipeEvidence)}.");
    Debug.WriteLine($"Missing public API evidence: {string.Join(", ", evaluation.MissingPublicApiEvidence)}.");
}
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

var chartQa = GalleryChartsPage.CreateChartVisualQaSnapshot(chart);
Debug.WriteLine(GalleryChartsPage.FormatChartVisualQa("Chart visual QA", chartQa));

var barChart = new FWBarChart
{
    Title = "Weekly throughput",
    Width = 320,
    Height = 180
};

var pieChart = new FWPieChart
{
    Title = "Risk split",
    Width = 240,
    Height = 180
};

var legend = new FWChartLegend
{
    Orientation = Orientation.Horizontal,
    Items = new[]
    {
        new ChartLegendItem { Label = "Current", Brush = PaletteBrush(0) },
        new ChartLegendItem { Label = "Baseline", Brush = PaletteBrush(1) },
        new ChartLegendItem { Label = "Risk", Brush = PaletteBrush(3) }
    }
};

var tooltip = new FWChartTooltip
{
    SeriesTitle = "Current",
    XValue = "Fri",
    YValue = "78",
    Background = ThemeBrush("ChartTooltipBackground"),
    BorderBrush = ThemeBrush("ChartTooltipBorderBrush"),
    Foreground = ThemeBrush("ChartTooltipForeground")
};

var legendTooltipQa = GalleryChartsPage.CreateLegendTooltipQaSnapshot(legend, tooltip);
Debug.WriteLine(GalleryChartsPage.FormatLegendTooltipVisualQa("Legend tooltip QA", legendTooltipQa));
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
var navigationService = new FWNavigationService
{
    PageTypeProvider = (route, parameter) => route.RouteKey == "settings"
        ? typeof(SettingsShellPage)
        : route.PageType
};
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
navigationService.NavigateToRoute("settings");

var diagnostics = navigationService.GetDiagnostics();
Debug.WriteLine($"Navigation route: {diagnostics.CurrentRouteKey}; provider: {diagnostics.HasPageTypeProvider}.");

var routeSuggestions = new[] { "Overview", "Activity", "Settings" };
var search = new FWAutoSuggestBox
{
    ItemsSource = routeSuggestions,
    FilterMode = AutoCompleteFilterMode.Contains,
    PlaceholderText = "Search routes",
    Text = "Settings"
};
search.QuerySubmitted += (_, args) => navigationService.NavigateToRoute(args.QueryText);

var breadcrumb = new FWBreadcrumbBar
{
    ItemsSource = new ObservableCollection<string> { "Home", "Overview" },
    MaxItems = 4
};

var pager = new FWPipsPager
{
    NumberOfPages = 10,
    MaxVisiblePips = 5,
    SelectedPageIndex = 0
};

var selectorBar = new FWSelectorBar
{
    SelectionIndicatorPlacement = FWSelectorBarSelectionIndicatorPlacement.Auto
};
selectorBar.Items.Add(new FWSelectorBarItem { Text = "Overview" });
selectorBar.Items.Add(new FWSelectorBarItem { Text = "Activity" });
selectorBar.SelectedIndex = 0;
var didSelectActivity = selectorBar.TrySelectIndex(1);
var ignoredOutOfRange = selectorBar.TrySelectIndex(99);

var selectorDiagnostics = selectorBar.GetDiagnostics();
Debug.WriteLine($"SelectorBar selected: {selectorDiagnostics.SelectedText}; index: {selectorDiagnostics.SelectedIndex}/{selectorDiagnostics.ItemCount}; selected activity: {didSelectActivity}; ignored out-of-range: {!ignoredOutOfRange}.");

var tabView = new FWTabView
{
    TabWidthMode = FWTabViewWidthMode.SizeToContent,
    CloseButtonOverlayMode = FWTabViewCloseButtonOverlayMode.Always,
    CanReorderTabs = true
};
tabView.Items.Add(new FWTabViewItem { Header = "Overview", Content = "Overview content", IsClosable = false });
tabView.Items.Add(new FWTabViewItem { Header = "Details", Content = "Details content" });
tabView.SelectedIndex = 0;
var movedTab = tabView.TryMoveTab(0, 1);

var tabDiagnostics = tabView.GetDiagnostics();
Debug.WriteLine($"TabView selected: {tabDiagnostics.SelectedHeader}; tabs: {tabDiagnostics.ItemCount}; close: {tabDiagnostics.CloseButtonOverlayMode}; reorder: {tabDiagnostics.CanReorderTabs}; moved: {movedTab}.");

var shellQa = GalleryNavigationPage.CreateNavigationShellQaSnapshot(
    navigationService,
    (IEnumerable<string>)breadcrumb.ItemsSource,
    selectorBar,
    tabView,
    pager,
    search);
Debug.WriteLine(GalleryNavigationPage.FormatNavigationShellQa("App shell QA", shellQa));
Debug.WriteLine($"App shell ready: {shellQa.IsAppShellReady}; route provider {shellQa.HasRouteProviderCoverage}; footer settings {shellQa.HasFooterSettingsCoverage}; search route {shellQa.HasSearchRouteCoverage}; documents {shellQa.HasDocumentWorkspaceCoverage}; pager {shellQa.HasPageNavigationCoverage}.");
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

var profileRecipe = FWFluentWindowMaterialProfileRecipe.Create(windowSurface.WindowMaterialProfile);
Debug.WriteLine($"Profile {profileRecipe.Role}; requested {profileRecipe.SystemBackdrop}; surface {profileRecipe.SurfaceRole}/{profileRecipe.MaterialKind}.");

windowSurface.ApplyWindowMaterialProfile(FWFluentWindowMaterialProfile.FocusGlassShell);
windowSurface.ApplyWindowBackdrop(window);

var actualBackdrop = window.SystemBackdrop;
var isMatched = actualBackdrop == FWFluentWindowMaterialProfileRecipe
    .Create(windowSurface.WindowMaterialProfile)
    .SystemBackdrop;
Debug.WriteLine($"Window backdrop QA: actual {actualBackdrop}; matched {isMatched}; auto apply {windowSurface.AutoApplyWindowBackdrop}.");

FWFluentWindowSurfaceDiagnostics surfaceDiagnostics =
    windowSurface.GetWindowSurfaceDiagnostics(actualBackdrop, wasApplied: true);
Debug.WriteLine($"Window surface diagnostics: {surfaceDiagnostics.Role}; match {surfaceDiagnostics.MatchState}; applied {surfaceDiagnostics.ApplyState}.");

var diagnostics = GalleryWindowSurfaceDiagnostics.Create(
    windowSurface,
    actualBackdrop,
    wasApplied: true);
Debug.WriteLine(GalleryWindowBackdropsPage.FormatWindowSurfaceDiagnostics(diagnostics));

var highContrastEnvironment = GalleryWindowSurfaceEnvironment.Create(
    FluentThemeVariant.HighContrast);
var highContrastActualBackdrop = GalleryWindowBackdropsPage.ResolveWindowSurfaceActualBackdrop(
    actualBackdrop,
    highContrastEnvironment);
var highContrastDiagnostics = GalleryWindowSurfaceDiagnostics.Create(
    windowSurface,
    highContrastActualBackdrop,
    wasApplied: true,
    highContrastEnvironment);

var inactiveEnvironment = GalleryWindowSurfaceEnvironment.Create(
    FluentThemeManager.CurrentTheme,
    isWindowActive: false);
var unsupportedHostEnvironment = GalleryWindowSurfaceEnvironment.Create(
    FluentThemeManager.CurrentTheme,
    isHostBackdropSupported: false);

Debug.WriteLine($"High contrast fallback: {highContrastDiagnostics.FallbackState}; {highContrastDiagnostics.EnvironmentState}.");
Debug.WriteLine($"Inactive window material: {inactiveEnvironment.FallbackState}; host: {inactiveEnvironment.HostState}.");
Debug.WriteLine($"Unsupported host fallback: {unsupportedHostEnvironment.FallbackState}; supported: {unsupportedHostEnvironment.IsHostBackdropSupported}.");
""",
        ["materials.derivedsurfaces"] = """
var layer = new FWLayerSurface
{
    Child = new FWTextBlock { Text = "Layer content" }
};

var mica = new FWMicaSurface
{
    Child = new FWTextBlock { Text = "Long-lived shell region" }
};

var micaAlt = new FWMicaAltSurface
{
    Child = new FWTextBlock { Text = "Tabbed pane region" }
};

var acrylic = new FWAcrylicSurface
{
    Child = new FWButton { Content = "Transient command" }
};

var frostedGlass = new FWFrostedGlassSurface
{
    Child = new FWTextBlock { Text = "Soft preview media" }
};

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

var acrylicRecipe = FWFluentMaterialRecipe.Create(FWFluentMaterialKind.Acrylic);
acrylic.UseMaterialRecipe(acrylicRecipe);

var focusRecipe = FWFluentMaterialRecipe.Create(FWFluentMaterialKind.LiquidGlass);
focusGlass.UseMaterialRecipe(focusRecipe);

Debug.WriteLine($"Derived surface recipes: layer {layer.MaterialKind}, mica {mica.MaterialKind}, acrylic blur {acrylicRecipe.BlurRadius}, focus {focusGlass.MaterialKind}.");
""",
        ["materialprimitives.backdrop"] = """
var micaBackdrop = new FWBackdrop
{
    Type = FWBackdropType.Mica,
    TintColor = Color.FromRgb(0xF3, 0xF3, 0xF3),
    TintOpacity = 0.8,
    LuminosityOpacity = 0.85,
    FallbackColor = Color.FromRgb(0xF8, 0xF8, 0xF8)
};

var forcedFallback = new FWBackdrop
{
    Type = FWBackdropType.Acrylic,
    AlwaysUseFallback = true,
    FallbackColor = Color.FromRgb(0x20, 0x20, 0x20)
};

var solidFallback = new FWBackdrop
{
    Type = FWBackdropType.None,
    FallbackColor = Color.FromRgb(0xFA, 0xFA, 0xFA)
};

var surface = new FWBorder
{
    Width = 320,
    Height = 180,
    CornerRadius = new CornerRadius(8),
    ClipToBounds = true,
    Child = micaBackdrop
};

Debug.WriteLine($"Backdrop primitive QA: type {micaBackdrop.Type}, tint {micaBackdrop.TintOpacity}, fallback {micaBackdrop.FallbackColor}.");
Debug.WriteLine($"Forced fallback QA: always {forcedFallback.AlwaysUseFallback}, type {forcedFallback.Type}, color {forcedFallback.FallbackColor}.");
Debug.WriteLine($"Solid fallback QA: type {solidFallback.Type}, color {solidFallback.FallbackColor}.");
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
var flyoutQa = GalleryMenusPage.CreateFlyoutQaSnapshot(contentFlyout);
Debug.WriteLine(GalleryMenusPage.FormatFlyoutQa("Flyout QA", flyoutQa));

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
var restoreTarget = new FWButton
{
    Content = "Restore focus target",
    Focusable = true
};

(bool ModalLayer, bool Focus, bool Keyboard, bool Automation, bool LightDismiss, bool CommandPath, bool RestoreTiming, bool ClipGuard, bool RootWindowSmoke, FWTaskDialogButton DefaultButton, FWTaskDialogButton CancelButton, int HostLayer, int AppLayer) CreateTaskDialogRealWindowQaSnapshot(
    FWTaskDialogHostDiagnostics hostDiagnostics,
    FWTaskDialogAutomationDiagnostics automationDiagnostics,
    int hostLayer,
    int appLayer,
    FWTaskDialogButton defaultButton,
    FWTaskDialogButton cancelButton,
    bool primaryCommandCanExecute,
    bool cancelCloseGuardEnabled)
{
    var modalLayer = hostDiagnostics.IsOpen && hostDiagnostics.HasCurrentDialog && hostLayer > appLayer;
    var focus = hostDiagnostics.RestoreFocusOnClose && hostDiagnostics.HasFocusRestoreTarget && automationDiagnostics.LastFocusTarget != FWTaskDialogButton.None;
    var keyboard = hostDiagnostics.IsFocusTrapEnabled && hostDiagnostics.LastKeyboardRequest != FWTaskDialogHostKeyboardRequest.None && hostDiagnostics.LastKeyboardRequestHandled;
    var automation = automationDiagnostics.PrimaryButton.IsVisible && automationDiagnostics.CloseButton.IsCancel;
    var commandPath = automationDiagnostics.PrimaryButton.IsEnabled && primaryCommandCanExecute;
    var restoreTiming = focus && automationDiagnostics.CloseButton.IsCancel;
    var clipGuard = modalLayer && automationDiagnostics.PrimaryButton.IsVisible && !string.IsNullOrWhiteSpace(automationDiagnostics.CloseButton.AutomationId);
    var rootWindowSmoke = modalLayer && focus && keyboard && automation && hostDiagnostics.IsLightDismissEnabled && commandPath && restoreTiming && clipGuard;
    return (
        modalLayer,
        focus,
        keyboard,
        automation,
        hostDiagnostics.IsLightDismissEnabled,
        commandPath,
        restoreTiming,
        clipGuard,
        rootWindowSmoke,
        defaultButton,
        cancelButton,
        hostLayer,
        appLayer);
}

string FormatTaskDialogRealWindowQa(string action, (bool ModalLayer, bool Focus, bool Keyboard, bool Automation, bool LightDismiss, bool CommandPath, bool RestoreTiming, bool ClipGuard, bool RootWindowSmoke, FWTaskDialogButton DefaultButton, FWTaskDialogButton CancelButton, int HostLayer, int AppLayer) snapshot)
{
    return $"{action}. TaskDialog real-window QA: modal layer {snapshot.ModalLayer}; focus {snapshot.Focus}; keyboard {snapshot.Keyboard}; automation {snapshot.Automation}; light dismiss {snapshot.LightDismiss}; command path {snapshot.CommandPath}; restore timing {snapshot.RestoreTiming}; clip guard {snapshot.ClipGuard}; root-window smoke {snapshot.RootWindowSmoke}; default {snapshot.DefaultButton}; cancel {snapshot.CancelButton}; host/app z {snapshot.HostLayer}/{snapshot.AppLayer}.";
}

var host = new FWTaskDialogHost
{
    IsLightDismissEnabled = true,
    IsFocusTrapEnabled = true,
    RestoreFocusOnClose = true,
    FocusRestoreTarget = restoreTarget
};
var root = new Grid();
root.Children.Add(new FWTextBlock { Text = "App content behind the modal layer" });
root.Children.Add(host);
Panel.SetZIndex(host, 10);

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
Debug.WriteLine($"TaskDialog real-window QA: open {diagnostics.IsOpen}; focus trap {diagnostics.IsFocusTrapEnabled}; restore target {diagnostics.HasFocusRestoreTarget}; z {Panel.GetZIndex(host)}.");

var tabArgs = new KeyEventArgs(UIElement.KeyDownEvent, Key.Tab, ModifierKeys.None, isDown: true, isRepeat: false, timestamp: Environment.TickCount);
host.RaiseEvent(tabArgs);
var shiftTabArgs = new KeyEventArgs(UIElement.KeyDownEvent, Key.Tab, ModifierKeys.Shift, isDown: true, isRepeat: false, timestamp: Environment.TickCount);
host.RaiseEvent(shiftTabArgs);
var lightDismissed = host.RequestLightDismiss();
Debug.WriteLine($"TaskDialog keyboard QA: {host.LastKeyboardRequest}; handled {host.LastKeyboardRequestHandled}; light dismiss {lightDismissed}.");

var automation = taskDialog.GetAutomationDiagnostics();
Debug.WriteLine($"TaskDialog automation: {automation.Name}; primary id: {automation.PrimaryButton.AutomationId}; primary help: {automation.PrimaryButton.HelpText}.");
Debug.WriteLine($"Close button automation: {automation.CloseButton.Name}; cancel: {automation.CloseButton.IsCancel}.");

var qaSnapshot = CreateTaskDialogRealWindowQaSnapshot(
    host.GetDiagnostics(),
    automation,
    hostLayer: Panel.GetZIndex(host),
    appLayer: 0,
    defaultButton: taskDialog.DefaultButton,
    cancelButton: taskDialog.CancelButton,
    primaryCommandCanExecute: deleteCommand.CanExecute(null),
    cancelCloseGuardEnabled: false);
Debug.WriteLine(FormatTaskDialogRealWindowQa("Real-window QA snapshot", qaSnapshot));
Debug.WriteLine($"TaskDialog root-window smoke: restore timing {qaSnapshot.RestoreTiming}; clip guard {qaSnapshot.ClipGuard}; ready {qaSnapshot.RootWindowSmoke}.");

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
var teachingTipQa = GalleryDisclosurePage.CreateTeachingTipVisualQaSnapshot(teachingTip);
Debug.WriteLine(GalleryDisclosurePage.FormatTeachingTipVisualQa("TeachingTip visual QA", teachingTipQa));
""",
        ["advancedinteraction.scroller"] = """
var refreshContainer = new FWRefreshContainer
{
    PullDirection = RefreshPullDirection.TopToBottom,
    Content = new FWStackPanel
    {
        Spacing = 8,
        Children =
        {
            new FWTextBlock { Text = "Pull down to refresh" },
            new FWTextBlock { Text = "Scrollable content" }
        }
    }
};
RefreshRequestedDeferral? pendingRefreshDeferral = null;
refreshContainer.RefreshRequested += (_, args) =>
{
    var deferral = args.GetDeferral();
    pendingRefreshDeferral = deferral;
    RefreshFeedAsync().ContinueWith(_ =>
    {
        deferral.Complete();
        pendingRefreshDeferral = null;
    });
};
refreshContainer.RequestRefresh();
pendingRefreshDeferral?.Complete(); // Complete or cancel from Gallery QA buttons.

var refreshDiagnostics = refreshContainer.GetDiagnostics();
Debug.WriteLine($"RefreshContainer: refreshing {refreshDiagnostics.IsRefreshing}; progress {refreshDiagnostics.PullProgress:P0}; visualizer {refreshDiagnostics.VisualizerState}.");
Debug.WriteLine(InteractionControlsPage.FormatRefreshContainerDiagnostics("Refresh QA", refreshDiagnostics));

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

var scrollViewer = new FWScrollViewer
{
    Content = new FWStackPanel
    {
        Children =
        {
            new FWTextBlock { Text = "Scrollable item 1" },
            new FWTextBlock { Text = "Scrollable item 2" },
            new FWTextBlock { Text = "Scrollable item 3" }
        }
    }
};
scroller.AttachScrollViewer(scrollViewer);
scroller.ScrollTo(0, 80);
scroller.VerticalSnapPointsType = SnapPointsType.Mandatory;
scroller.ScrollTo(0, 180);

var diagnostics = scroller.GetViewportDiagnostics();
Debug.WriteLine($"Scroller viewport: {diagnostics.ViewportWidth}x{diagnostics.ViewportHeight}; offset: {diagnostics.VerticalOffset}; extent: {diagnostics.ExtentHeight}.");
Debug.WriteLine(InteractionControlsPage.FormatScrollerDiagnostics("Snap requested", scroller, diagnostics));

var annotatedScrollBar = new FWAnnotatedScrollBar
{
    Orientation = Orientation.Vertical,
    Minimum = 0,
    Maximum = 500,
    Labels = new List<ScrollBarLabel>
    {
        new() { ScrollOffset = 100, Content = "Warning", Type = ScrollBarLabelType.Warning },
        new() { ScrollOffset = 250, Content = "Error", Type = ScrollBarLabelType.Error },
        new() { ScrollOffset = 400, Content = "Info", Type = ScrollBarLabelType.Info }
    }
};
annotatedScrollBar.DetailLabelRequested += (_, args) =>
{
    Debug.WriteLine($"Annotated label: {args.LabelType} {args.Content} at {args.ScrollOffset}.");
    Debug.WriteLine(InteractionControlsPage.FormatAnnotatedScrollBarDetail(args, annotatedScrollBar.GetDiagnostics()));
};
annotatedScrollBar.Value = 250;

FWAnnotatedScrollBarDiagnostics annotationDiagnostics = annotatedScrollBar.GetDiagnostics();
Debug.WriteLine($"AnnotatedScrollBar labels: {annotationDiagnostics.RegisteredLabelCount}/{annotationDiagnostics.SourceLabelCount}; value: {annotationDiagnostics.Value}; range: {annotationDiagnostics.Minimum}-{annotationDiagnostics.Maximum}.");
Debug.WriteLine(InteractionControlsPage.FormatAnnotatedScrollBarDiagnostics("Error marker", annotationDiagnostics));
""",
        ["status.snackbar"] = """
var rootHost = new FWSnackbarHost
{
    Width = 470,
    MaxVisibleSnackbars = 2,
    Placement = FWSnackbarPlacement.Bottom,
    Spacing = 8,
    TransitionProfile = FWContentTransitionProfile.Entrance,
    TransitionOffset = 16
};

var overlayHost = new FWSnackbarOverlayHost
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
service.SetHost(rootHost);
rootHost.TransitionRequested += (_, args) => LogTransition(args.Kind, args.Diagnostics);
rootHost.QueueChanged += (_, args) => LogQueueDiagnostics(args.Reason, args.Diagnostics);
overlayHost.TransitionRequested += (_, args) => LogTransition(args.Kind, args.Diagnostics);
overlayHost.QueueChanged += (_, args) => LogQueueDiagnostics(args.Reason, args.Diagnostics);
overlayHost.OverlayOpened += (_, _) => LogOverlayState(overlayHost.IsOverlayOpen);
overlayHost.OverlayClosed += (_, _) => LogOverlayState(overlayHost.IsOverlayOpen);

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

service.SetHost(overlayHost);
overlayHost.Placement = FWSnackbarPlacement.Top;
overlayHost.OverlayPlacement = PlacementMode.Top;
overlayHost.Placement = FWSnackbarPlacement.Bottom;
overlayHost.OverlayPlacement = PlacementMode.Bottom;

var closeTask = service.EnqueueForResultAsync(snackbar);
FWSnackbarPresenterDiagnostics presenterDiagnostics = snackbar.GetPresenterDiagnostics();
Debug.WriteLine($"Snackbar presenter: {presenterDiagnostics.PresenterState}; has presenter {presenterDiagnostics.HasPresenter}; opacity {presenterDiagnostics.PresenterOpacity}; offset {presenterDiagnostics.PresenterOffset}.");
var diagnostics = overlayHost.GetDiagnostics();
var isOverlayOpen = overlayHost.IsOverlayOpen;
snackbar.PauseAutoDismiss();
var visualQa = GalleryStatusPage.CreateSnackbarVisualQaSnapshot(
    "Overlay",
    overlayHost,
    overlayHost,
    autoDismissEnabled: true,
    transitionRequests: 1,
    lastTransition: FWSnackbarTransitionKind.Show,
    queueEvents: 1,
    overlayEvents: isOverlayOpen ? 1 : 0,
    closedEvents: 0,
    lastCloseReason: FWSnackbarCloseReason.None,
    actionRequests: 0,
    lastCommandParameter: "preview-refresh");
Debug.WriteLine(GalleryStatusPage.FormatSnackbarVisualQa("Snackbar root-window QA", visualQa));
snackbar.ResumeAutoDismiss();
snackbar.RequestClose(FWSnackbarCloseReason.CloseButton);
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

    public static bool TryGetRegisteredSampleCode(string sampleCodeKey, out string sampleCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sampleCodeKey);

        if (SampleCodeByKey.TryGetValue(sampleCodeKey.Trim(), out var registeredSampleCode))
        {
            sampleCode = registeredSampleCode.Trim();
            return true;
        }

        sampleCode = string.Empty;
        return false;
    }

    public static bool ContainsRegisteredSampleCodeKey(string? sampleCodeKey)
    {
        return !string.IsNullOrWhiteSpace(sampleCodeKey)
            && SampleCodeByKey.ContainsKey(sampleCodeKey.Trim());
    }

    private static string CreateCatalogFilterSample(string filter)
    {
        return $$"""
var localization = new GalleryLocalizationService();
var pageInfos = GalleryCatalog.CreatePageInfos(localization);

var filterPage = new GalleryCatalogFilterPage(
    GalleryCatalogFilter.{{filter}},
    pageInfos);

var snapshot = filterPage.CreateSnapshot();
var content = filterPage.CreateContent();
Debug.WriteLine($"{{filter}}: {snapshot.ControlCount} controls across {snapshot.PageCount} pages.");
Debug.WriteLine($"{snapshot.GroupCounts.Length} groups, {snapshot.WithSourcePathCount} sources, {snapshot.WithSampleCodeKeyCount} samples, {snapshot.WithApiNamespaceCount} api namespaces.");

foreach (var group in snapshot.GroupCounts)
{
    Debug.WriteLine($"{group.Group}: {group.ControlCount} controls across {group.PageCount} pages.");
}
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
