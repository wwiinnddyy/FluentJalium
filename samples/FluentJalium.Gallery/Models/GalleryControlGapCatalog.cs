namespace FluentJalium.Gallery.Models;

internal sealed record GalleryControlGapEntry(
    string CandidateControl,
    string Priority,
    string AreaId,
    string ReferenceInput,
    string Stage,
    string Decision,
    string PageId,
    string SampleCodeKey,
    string NextAction,
    string[] Evidence,
    string[] RequiredBeforePublicApi)
{
    public bool IsPublicFwControl => string.Equals(Stage, GalleryControlGapStage.PublicFwControl, StringComparison.Ordinal);
    public bool IsRecipeOnly => string.Equals(Stage, GalleryControlGapStage.GalleryRecipeOnly, StringComparison.Ordinal);
    public bool IsEvaluateOnly => string.Equals(Stage, GalleryControlGapStage.Evaluate, StringComparison.Ordinal);
    public bool RequiresRenderedQa => string.Equals(Stage, GalleryControlGapStage.RenderedQaRequired, StringComparison.Ordinal);
    public bool HasDecisionEvidence => Evidence.Length > 0 && !string.IsNullOrWhiteSpace(Decision) && !string.IsNullOrWhiteSpace(NextAction);
    public bool RequiresPublicApi => IsRecipeOnly || IsEvaluateOnly;
}

internal sealed record GalleryControlGapSnapshot(
    int EntryCount,
    int PublicControlCount,
    int RecipeOnlyCount,
    int EvaluateCount,
    int RenderedQaRequiredCount,
    int P0Count,
    int P1Count,
    int P2Count,
    string[] AreaIds,
    string[] ReferenceInputs,
    string[] SampleCodeKeys)
{
    public bool CoversArea(string areaId)
    {
        return AreaIds.Contains(areaId, StringComparer.Ordinal);
    }

    public bool CoversReference(string referenceInput)
    {
        return ReferenceInputs.Contains(referenceInput, StringComparer.Ordinal);
    }

    public bool HasSample(string sampleCodeKey)
    {
        return SampleCodeKeys.Contains(sampleCodeKey, StringComparer.Ordinal);
    }
}

internal static class GalleryControlGapStage
{
    public const string PublicFwControl = "Public FW control";
    public const string GalleryRecipeOnly = "Gallery recipe only";
    public const string Evaluate = "Evaluate";
    public const string RenderedQaRequired = "Rendered QA required";
}

internal static class GalleryControlGapCatalog
{
    public static IReadOnlyList<GalleryControlGapEntry> CreateEntries()
    {
        return Entries;
    }

    public static GalleryControlGapSnapshot CreateSnapshot()
    {
        return new GalleryControlGapSnapshot(
            Entries.Length,
            Entries.Count(entry => entry.IsPublicFwControl || entry.RequiresRenderedQa),
            Entries.Count(entry => entry.IsRecipeOnly),
            Entries.Count(entry => entry.IsEvaluateOnly),
            Entries.Count(entry => entry.RequiresRenderedQa),
            Entries.Count(entry => string.Equals(entry.Priority, "P0", StringComparison.Ordinal)),
            Entries.Count(entry => string.Equals(entry.Priority, "P1", StringComparison.Ordinal)),
            Entries.Count(entry => string.Equals(entry.Priority, "P2", StringComparison.Ordinal)),
            Entries.Select(entry => entry.AreaId).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray(),
            Entries.Select(entry => entry.ReferenceInput).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray(),
            Entries.Select(entry => entry.SampleCodeKey).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray());
    }

    public static string FormatSnapshot(GalleryControlGapSnapshot snapshot)
    {
        return $"Control gap matrix: {snapshot.EntryCount} candidates, {snapshot.PublicControlCount} public FW controls, {snapshot.RecipeOnlyCount} recipe-only candidates, {snapshot.EvaluateCount} evaluate candidates, {snapshot.RenderedQaRequiredCount} rendered-QA gates, priorities P0/P1/P2 {snapshot.P0Count}/{snapshot.P1Count}/{snapshot.P2Count}.";
    }

    private static readonly GalleryControlGapEntry[] Entries =
    [
        Entry(
            "FWFlyout / FWFlyoutPresenter",
            "P0",
            "menus",
            "WinUI / WinUI Gallery",
            GalleryControlGapStage.PublicFwControl,
            "Keep as public behavior plus presenter surface; visual chrome is owned by FWFlyoutPresenter style.",
            "menus",
            "menus.flyout.commandbar",
            "Run rendered flyout placement and presenter clipping QA across theme variants.",
            ["Public FWFlyout and FWFlyoutPresenter types", "Menu Gallery sample", "Flyout QA formatter"],
            []),
        Entry(
            "FWGridView / FWGridViewItem",
            "P0",
            "collections",
            "WinUI / WinUI Gallery",
            GalleryControlGapStage.PublicFwControl,
            "Keep as public collection surface backed by Jalium ListView/GridView columns.",
            "collections",
            "collections.gridview",
            "Inspect rendered row density, disabled items, selected rows, and column clipping.",
            ["Collections Gallery sample", "Catalog metadata", "State matrix coverage"],
            []),
        Entry(
            "FWCalendarDatePicker / FWCalendarView",
            "P0",
            "dateandtime",
            "WinUI / WinUI Gallery",
            GalleryControlGapStage.PublicFwControl,
            "Keep both WinUI names; FWCalendarView is the compatibility wrapper over Jalium Calendar.",
            "dateandtime",
            "datetime.calendarpicker.calendarview",
            "Run rendered picker flyout QA for bounds, selected dates, disabled dates, and focus.",
            ["Date and Time Gallery sample", "Calendar picker QA snapshot", "CalendarView QA snapshot"],
            []),
        Entry(
            "FWSplitView",
            "P0",
            "contentandlayout",
            "WinUI / WinUI Gallery",
            GalleryControlGapStage.PublicFwControl,
            "Keep as public adaptive layout surface with diagnostics instead of only styling a panel.",
            "contentandlayout",
            "layout.splitview.settingscard",
            "Verify rendered compact/overlay/inline pane transitions in a shell-sized host.",
            ["SplitView diagnostics", "Layout Gallery sample", "Sample registry recipe"],
            []),
        Entry(
            "FWTabView / FWTabViewItem",
            "P0",
            "navigation",
            "WinUI / WinUI Gallery",
            GalleryControlGapStage.PublicFwControl,
            "Keep as public document-tab surface; diagnostics cover selected header, close overlay, and reorder state.",
            "navigation",
            "navigation.breadcrumb.pips.selector.tabview.titlebar",
            "Render-test tab close/reorder, overflow, and document area sizing in the app-shell walkthrough.",
            ["TabView diagnostics", "Navigation Gallery sample", "Catalog metadata"],
            []),
        Entry(
            "FWSelectorBar / FWSelectorBarItem",
            "P0",
            "navigation",
            "WinUI / WinUI Gallery",
            GalleryControlGapStage.PublicFwControl,
            "Keep as public lightweight selector navigation surface with selected item diagnostics.",
            "navigation",
            "navigation.breadcrumb.pips.selector.tabview.titlebar",
            "Inspect rendered indicator placement and keyboard selection states.",
            ["SelectorBar diagnostics", "Navigation Gallery sample", "Sample registry recipe"],
            []),
        Entry(
            "FWAutoSuggestBox",
            "P0",
            "textinput",
            "WinUI / WinUI Gallery",
            GalleryControlGapStage.PublicFwControl,
            "Keep as public WinUI-named facade over Jalium AutoCompleteBox with query and suggestion events.",
            "textinput",
            "textinput.autosuggestbox",
            "Render-test suggestion popup width, Enter submit behavior, and text-change reason traces.",
            ["AutoSuggestTextChanged reason", "SuggestionChosen", "QuerySubmitted"],
            []),
        Entry(
            "FWSettingsCard / FWSettingsExpander",
            "P1",
            "contentandlayout",
            "FluentAvalonia / Community Toolkit",
            GalleryControlGapStage.RenderedQaRequired,
            "Public controls are implemented; the remaining gate is Win11 settings visual confidence.",
            "contentandlayout",
            "layout.splitview.settingscard",
            "Run rendered settings-page QA for icon/action alignment, focus, disabled rows, and dense pages.",
            ["SettingsCard diagnostics", "SettingsExpander item host", "Automation peer coverage"],
            []),
        Entry(
            "FWTaskDialog",
            "P1",
            "disclosure",
            "WinUI / WPF UI",
            GalleryControlGapStage.RenderedQaRequired,
            "Public dialog and host are implemented; root-window smoke exists but rendered focus/clipping still needs inspection.",
            "disclosure",
            "disclosure.taskdialog",
            "Capture rendered focus ring, clipping, modal layering, and close/restore timing.",
            ["TaskDialog real-window QA", "Automation diagnostics", "Focus trap diagnostics"],
            []),
        Entry(
            "FWSnackbar",
            "P1",
            "status",
            "WPF UI",
            GalleryControlGapStage.RenderedQaRequired,
            "Public snackbar host/service surface is implemented; visual queue placement still needs rendered QA.",
            "status",
            "status.snackbar",
            "Inspect overlay/root host placement, transition timing, focus return, and stacked queue clipping.",
            ["Snackbar host diagnostics", "Presenter diagnostics", "Overlay host sample"],
            []),
        Entry(
            "FWTwoPaneView / FWParallaxView",
            "P1",
            "contentandlayout",
            "WinUI / WinUI Gallery",
            GalleryControlGapStage.RenderedQaRequired,
            "Public adaptive/layout effects exist with diagnostics; richer real-window shell validation remains.",
            "contentandlayout",
            "layout.splitview.settingscard",
            "Run responsive visual QA for single-pane priority, scroll source progress, and offset transforms.",
            ["TwoPaneView diagnostics", "Parallax diagnostics", "Layout Gallery sample"],
            []),
        Entry(
            "FWRadioButtons",
            "P1",
            "selection",
            "WinUI / WinUI Gallery",
            GalleryControlGapStage.PublicFwControl,
            "Keep as public grouped radio selection surface.",
            "selection",
            "selection.radiobuttons",
            "Inspect rendered spacing, disabled options, and keyboard navigation in dense forms.",
            ["Selection Gallery sample", "Forms pattern sample", "Catalog metadata"],
            []),
        Entry(
            "FWCanvas / FWRelativePanel / FWBitmapIcon / FWImageIcon / FWRichTextBlock",
            "P2",
            "visuals",
            "WinUI / WinUI Gallery",
            GalleryControlGapStage.PublicFwControl,
            "Keep as low-risk compatibility aliases and document them through catalog metadata.",
            "visuals",
            "visuals.icons.richtext.personpicture.markdown.qrcode.shapes",
            "Keep All Controls metadata and visual primitive rendering checks complete.",
            ["Visuals Gallery sample", "Catalog control index", "Shape and icon QA"],
            []),
        Entry(
            "FWItemsView",
            "Evaluate",
            "advancedcollections",
            "WinUI / WinUI Gallery",
            GalleryControlGapStage.GalleryRecipeOnly,
            "Keep as Gallery recipe until an owned multi-select selection model and automation contract are proven.",
            "advancedcollections",
            "advancedcollections.itemsrepeater",
            "Prototype owned selection model diagnostics before promoting a public FWItemsView API.",
            ["AdvancedCollections recipe matrix", "ItemsRepeater viewport evidence", "Keyboard navigation recipe"],
            ["Owned selection model", "Automation metadata", "Selection virtualization traces"]),
        Entry(
            "FWFlipView",
            "Evaluate",
            "advancedcollections",
            "WinUI / WinUI Gallery",
            GalleryControlGapStage.GalleryRecipeOnly,
            "Keep as Gallery recipe until touch swipe gestures and animation traces are proven.",
            "advancedcollections",
            "advancedcollections.itemsrepeater",
            "Prototype gesture/animation diagnostics before promoting a public FWFlipView API.",
            ["Flip paging recipe", "PipsPager sample", "Viewport behavior evidence"],
            ["Touch swipe gesture host", "Animation trace diagnostics", "Automation metadata"]),
        Entry(
            "FWSemanticZoom",
            "Evaluate",
            "advancedcollections",
            "WinUI / WinUI Gallery",
            GalleryControlGapStage.GalleryRecipeOnly,
            "Keep as Gallery recipe until synchronized zoomed-in/out views and automation are owned.",
            "advancedcollections",
            "advancedcollections.itemsrepeater",
            "Prototype two-view synchronization diagnostics before promoting a public FWSemanticZoom API.",
            ["Semantic grouping recipe", "Grouped source evidence", "Virtualization behavior evidence"],
            ["Two-view synchronized source API", "Zoom transition diagnostics", "Automation metadata"]),
        Entry(
            "FWMaskedTextBox / FWForm",
            "Evaluate",
            "forms",
            "Community Toolkit / FluentAvalonia",
            GalleryControlGapStage.Evaluate,
            "Keep as formatting and validation recipes until repeated app scenarios justify public abstractions.",
            "forms",
            "patterns.forms",
            "Render-test forms recipes and gather repeated formatter/validation needs before adding public API.",
            ["Phone/license-key formatting recipe", "Data form validation snapshot", "Forms Gallery page"],
            ["Repeated formatter scenarios", "Validation lifecycle ownership", "Accessibility contract"]),
        Entry(
            "FWContactCard",
            "Evaluate",
            "visuals",
            "WinUI / WinUI Gallery",
            GalleryControlGapStage.Evaluate,
            "Keep contact-card behavior adjacent to FWPersonPicture until Jalium has a native card surface.",
            "visuals",
            "visuals.icons.richtext.personpicture.markdown.qrcode.shapes",
            "Expand FWPersonPicture contact recipes before considering a public FWContactCard.",
            ["FWPersonPicture display/badge coverage", "Visuals Gallery sample", "Catalog metadata"],
            ["Native contact-card base", "Command/contact action model", "Automation metadata"]),
        Entry(
            "FWInkToolbar",
            "Evaluate",
            "inputandmedia",
            "WinUI / WinUI Gallery",
            GalleryControlGapStage.Evaluate,
            "Keep as FWToolBar plus FWInkCanvas recipe until a dedicated ink toolbar base/API exists.",
            "inputandmedia",
            "inputmedia.color.ink.media",
            "Build a Gallery ink toolbar recipe with stroke, erase, selection, and keyboard command evidence.",
            ["FWInkCanvas", "FWInkPresenter", "FWToolBar"],
            ["Dedicated toolbar command model", "Ink mode diagnostics", "Automation metadata"])
    ];

    private static GalleryControlGapEntry Entry(
        string candidateControl,
        string priority,
        string areaId,
        string referenceInput,
        string stage,
        string decision,
        string pageId,
        string sampleCodeKey,
        string nextAction,
        string[] evidence,
        string[] requiredBeforePublicApi)
    {
        return new GalleryControlGapEntry(
            candidateControl,
            priority,
            areaId,
            referenceInput,
            stage,
            decision,
            pageId,
            sampleCodeKey,
            nextAction,
            evidence,
            requiredBeforePublicApi);
    }
}
