namespace FluentJalium.Gallery.Models;

internal sealed record GalleryVisualQaCoverageFamily(
    string FamilyId,
    string Title,
    string PageId,
    string SampleCodeKey,
    string Readiness,
    string EvidenceLevel,
    string NextAction,
    bool RequiresRenderedQa,
    string[] Controls,
    string[] CoveredStates,
    string[] Evidence)
{
    public bool Covers(string state)
    {
        return CoveredStates.Any(covered => string.Equals(covered, state, StringComparison.OrdinalIgnoreCase));
    }

    public bool HasReadinessEvidence =>
        !string.IsNullOrWhiteSpace(Readiness)
        && !string.IsNullOrWhiteSpace(EvidenceLevel)
        && !string.IsNullOrWhiteSpace(NextAction);

    public bool HasRenderedQaEvidence =>
        string.Equals(EvidenceLevel, "RenderedQa", StringComparison.Ordinal);

    public string Summary =>
        $"{Title}: {Controls.Length} controls, {CoveredStates.Length} states, {Readiness}, sample {SampleCodeKey}.";
}

internal sealed record GalleryVisualQaCoverageSnapshot(
    int FamilyCount,
    int ControlCount,
    int StateCount,
    int DiagnosticFamilyCount,
    int ReadinessFamilyCount,
    int NeedsRenderedQaCount,
    int RenderedQaFamilyCount,
    string[] PageIds,
    string[] SampleCodeKeys)
{
    public bool CoversPage(string pageId)
    {
        return PageIds.Contains(pageId, StringComparer.Ordinal);
    }

    public bool HasSample(string sampleCodeKey)
    {
        return SampleCodeKeys.Contains(sampleCodeKey, StringComparer.Ordinal);
    }
}

internal static class GalleryVisualQaCoverageCatalog
{
    public static IReadOnlyList<GalleryVisualQaCoverageFamily> CreateFamilies()
    {
        return Families;
    }

    public static GalleryVisualQaCoverageSnapshot CreateSnapshot()
    {
        return new GalleryVisualQaCoverageSnapshot(
            Families.Length,
            Families.Sum(family => family.Controls.Length),
            Families.SelectMany(family => family.CoveredStates).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
            Families.Count(family => family.Covers("diagnostics")),
            Families.Count(family => family.HasReadinessEvidence),
            Families.Count(family => family.RequiresRenderedQa),
            Families.Count(family => family.HasRenderedQaEvidence),
            Families.Select(family => family.PageId).Distinct(StringComparer.Ordinal).OrderBy(pageId => pageId, StringComparer.Ordinal).ToArray(),
            Families.Select(family => family.SampleCodeKey).Distinct(StringComparer.Ordinal).OrderBy(sampleCodeKey => sampleCodeKey, StringComparer.Ordinal).ToArray());
    }

    public static string FormatSnapshot(GalleryVisualQaCoverageSnapshot snapshot)
    {
        return $"Visual QA coverage: {snapshot.FamilyCount} families, {snapshot.ControlCount} controls, {snapshot.StateCount} state tokens, {snapshot.DiagnosticFamilyCount} diagnostic families, {snapshot.ReadinessFamilyCount} readiness-scored families, {snapshot.NeedsRenderedQaCount} need rendered QA.";
    }

    private static readonly GalleryVisualQaCoverageFamily[] Families =
    [
        Family(
            "commands",
            "Command surfaces",
            "buttons",
            "buttons.commandbar",
            "State matrix-ready",
            "StateMatrix",
            "Capture rendered CommandBar and toolbar rows across desktop/mobile density and focus states.",
            true,
            ["FWButton", "FWSplitButton", "FWCommandBar", "FWToolBar"],
            ["normal", "hover", "pressed", "disabled", "focus", "density", "material"],
            ["State Matrix command rows", "CommandBar Gallery sample", "Toolbar sample code"]),
        Family(
            "forms",
            "Forms and validation",
            "forms",
            "patterns.forms",
            "Recipe-ready",
            "RecipeSnapshot",
            "Run a rendered validation lifecycle pass for dense, disabled, focus, async submit, reset, and summary states.",
            true,
            ["FWLabel", "FWTextBox", "FWAutoSuggestBox", "FWRadioButtons", "FWInfoBar", "FWSettingsCard"],
            ["normal", "disabled", "focus", "density", "validation", "async", "diagnostics"],
            ["Forms visual QA panel", "Submit progress sample", "Catalog metadata tests"]),
        Family(
            "collections",
            "Collections and virtualization",
            "advancedcollections",
            "advancedcollections.itemsrepeater",
            "Diagnostic-ready",
            "HeadlessSnapshot",
            "Stress rendered virtualization, selection, keyboard focus, and horizontal viewport behavior with large data.",
            true,
            ["FWItemsRepeater", "FWItemsRepeaterDiagnostics", "FWGridView", "FWListView"],
            ["normal", "selected", "focus", "density", "large list", "horizontal", "diagnostics"],
            ["ItemsRepeater QA profiles", "Collection navigation recipes", "Viewport diagnostics"]),
        Family(
            "navigation",
            "Navigation shell",
            "navigation",
            "navigation.breadcrumb.pips.selector.tabview.titlebar",
            "Shell snapshot-ready",
            "HeadlessSnapshot",
            "Walk the rendered app shell with footer, search, tab close/reorder, breadcrumb, and document routes.",
            true,
            ["FWNavigationService", "FWBreadcrumbBar", "FWPipsPager", "FWSelectorBar", "FWTabView", "FWTitleBar"],
            ["normal", "selected", "disabled", "focus", "density", "diagnostics", "material"],
            ["App shell QA snapshot", "SelectorBar diagnostics", "TabView diagnostics"]),
        Family(
            "disclosure",
            "Disclosure and modal flows",
            "disclosure",
            "disclosure.taskdialog",
            "Smoke-ready",
            "RealWindowSmoke",
            "Capture rendered TaskDialog, TeachingTip, and SettingsExpander focus, clipping, and light-dismiss states.",
            true,
            ["FWTaskDialog", "FWTaskDialogHost", "FWTeachingTip", "FWSettingsExpander", "FWContentDialog"],
            ["normal", "disabled", "focus", "light dismiss", "keyboard", "automation", "diagnostics"],
            ["TaskDialog real-window QA", "TeachingTip visual QA", "SettingsExpander item host sample"]),
        Family(
            "layout",
            "Adaptive layout and settings",
            "contentandlayout",
            "layout.splitview.settingscard",
            "Settings snapshot-ready",
            "HeadlessSnapshot",
            "Inspect rendered settings cards, SplitView pane modes, TwoPane breakpoints, and Parallax scroll offsets.",
            true,
            ["FWSplitView", "FWTwoPaneView", "FWParallaxView", "FWSettingsCard"],
            ["normal", "hover", "pressed", "disabled", "focus", "density", "diagnostics"],
            ["Settings visual QA snapshot", "TwoPaneView diagnostics", "Parallax diagnostics"]),
        Family(
            "materials",
            "Materials and window backdrops",
            "windowbackdrops",
            "materials.windowbackdrop",
            "Runtime diagnostic-ready",
            "RuntimeDiagnostics",
            "Run rendered Gallery material passes for OS support, inactive windows, high contrast, and fallback brushes.",
            true,
            ["FWFluentWindowSurface", "FWFluentWindowSurfaceDiagnostics", "FWFluentMaterialSurface"],
            ["normal", "light", "dark", "high contrast", "inactive", "unsupported host", "diagnostics"],
            ["Window surface diagnostics", "High contrast fallback", "Unsupported host fallback"]),
        Family(
            "interaction",
            "Advanced interaction",
            "advancedinteraction",
            "advancedinteraction.scroller",
            "Diagnostic-ready",
            "RuntimeDiagnostics",
            "Exercise rendered refresh deferrals, snap points, scroll offsets, and annotation detail popups.",
            true,
            ["FWRefreshContainer", "FWScroller", "FWAnnotatedScrollBar"],
            ["normal", "focus", "refresh deferral", "snap points", "annotation detail", "diagnostics"],
            ["Refresh diagnostics", "Scroller viewport diagnostics", "AnnotatedScrollBar detail formatter"]),
        Family(
            "status",
            "Status and notification",
            "status",
            "status.snackbar",
            "Snapshot-ready",
            "HeadlessSnapshot",
            "Verify rendered snackbar queue, overlay host placement, focus return, and InfoBar severity states.",
            true,
            ["FWInfoBar", "FWSnackbar", "FWSnackbarPresenterDiagnostics", "FWSnackbarHost"],
            ["normal", "hover", "focus", "disabled", "queue", "overlay", "diagnostics"],
            ["Snackbar visual QA snapshot", "Presenter diagnostics", "Overlay host QA"]),
        Family(
            "visuals",
            "Visual primitives",
            "visuals",
            "visuals.icons.richtext.personpicture.markdown.qrcode.shapes",
            "Rendered QA needed",
            "SampleCode",
            "Inspect rendered visual primitive spacing, icon fallback, QR quiet zone, Markdown links, and avatar badges.",
            true,
            ["FWPersonPicture", "FWMarkdown", "FWQRCode", "FWRectangle", "FWEllipse", "FWPath"],
            ["normal", "disabled", "focus", "light", "dark", "high contrast", "diagnostics"],
            ["Shape controls QA snapshot", "Visuals Gallery sample", "Icon fallback coverage"]),
        Family(
            "datetime",
            "Date and time",
            "dateandtime",
            "datetime.calendarpicker.calendarview",
            "Snapshot-ready",
            "HeadlessSnapshot",
            "Capture rendered picker flyouts, selected ranges, bounds, disabled dates, and keyboard focus states.",
            true,
            ["FWDatePicker", "FWCalendarDatePicker", "FWCalendarView", "FWTimePicker"],
            ["normal", "selected", "disabled", "focus", "bounds", "density", "diagnostics"],
            ["CalendarDatePicker QA", "CalendarView QA", "Date/time sample code"]),
        Family(
            "charts",
            "Charts and data visualization",
            "charts",
            "charts.family",
            "Rendered QA needed",
            "SampleCode",
            "Inspect rendered chart cards at desktop/mobile sizes and verify legend/tooltip clipping and palette density.",
            true,
            ["FWLineChart", "FWBarChart", "FWPieChart", "FWHeatmap", "FWChartLegend"],
            ["normal", "selected", "disabled", "focus", "viewport", "density", "diagnostics"],
            ["Chart visual QA snapshot", "Chart legend/tooltip samples", "Palette state tests"])
    ];

    private static GalleryVisualQaCoverageFamily Family(
        string familyId,
        string title,
        string pageId,
        string sampleCodeKey,
        string readiness,
        string evidenceLevel,
        string nextAction,
        bool requiresRenderedQa,
        string[] controls,
        string[] coveredStates,
        string[] evidence)
    {
        return new GalleryVisualQaCoverageFamily(
            familyId,
            title,
            pageId,
            sampleCodeKey,
            readiness,
            evidenceLevel,
            nextAction,
            requiresRenderedQa,
            controls,
            coveredStates,
            evidence);
    }
}
