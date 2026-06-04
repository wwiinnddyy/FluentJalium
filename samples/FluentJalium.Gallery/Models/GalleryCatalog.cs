using FluentJalium.Icon;
using Jalium.UI;

namespace FluentJalium.Gallery.Models;

internal sealed record GalleryCatalogEntry(
    string Title,
    string Description,
    string Group,
    FluentIconRegular Icon,
    string Keywords,
    GalleryPageStatus Status = GalleryPageStatus.Stable,
    bool IsFooter = false);

internal static class GalleryCatalog
{
    private static readonly GalleryCatalogEntry[] Entries =
    [
        Entry("Overview", "Theme, typography, and accent controls for validating FluentJalium across variants.", GalleryNavigationGroup.Home, FluentIconRegular.Home24, "home design system theme typography accent light dark high contrast"),
        Entry("Theme Architecture", "How FluentJalium splits stable theme entry points, design resources, control dictionaries, and FW control surfaces.", GalleryNavigationGroup.Design, FluentIconRegular.Diagram24, "Generic.jalxaml FluentResources.jalxaml FluentControls.jalxaml FluentThemeManager theme resources controls dictionary FW control architecture FluentAvalonia WinUI WPFUI gallery design"),
        Entry("Colors", "Accent, text, fill, and semantic color tokens for FluentJalium themes and FW controls.", GalleryNavigationGroup.Design, FluentIconRegular.Color24, "FluentColors AccentBrush AccentFillColor TextPrimary TextSecondary ControlFillColor LayerFillColor SelectionBackground HyperlinkForeground ProgressBarForeground semantic color token design"),
        Entry("Typography", "Font families, type ramp, and control text roles used by FluentJalium themes.", GalleryNavigationGroup.Design, FluentIconRegular.TextFont24, "FluentTypography DisplayFontFamily BodyFontFamily MonoFontFamily FluentCaptionFontSize FluentBodyFontSize FluentSubtitleFontSize FluentTitleFontSize ControlContentThemeFontSize typography type ramp font design"),
        Entry("Geometry", "Corner radius, stroke, and elevation tokens for FluentJalium control surfaces.", GalleryNavigationGroup.Design, FluentIconRegular.Ruler24, "ControlCornerRadius OverlayCornerRadius CardCornerRadius CompactCornerRadius FluentControlBorderThickness ControlElevationBorderBrush AccentControlElevationBorderBrush FluentGeometry radius corner stroke border elevation shadow WinUI geometry design token"),
        Entry("Motion Tokens", "Duration, connected animation, and transition role tokens that keep FluentJalium motion aligned with WinUI pacing.", GalleryNavigationGroup.Design, FluentIconRegular.SlideTransition24, "FluentMotionDurationFast FluentMotionDurationNormal FluentMotionDurationEmphasized FluentMotionConnectedAnimationDuration FluentMotionConnectedAnimationInitialOpacity FWConnectedAnimationConfiguration TransitionMode motion token connected animation design"),
        Entry("Buttons", "Button and command surfaces, including split, drop-down, app bar, toolbar, and material command decks.", GalleryNavigationGroup.ControlSurfaces, FluentIconRegular.ControlButton24, "FWButton FWRepeatButton FWHyperlinkButton FWDropDownButton FWSplitButton FWToggleSplitButton FWAppBarButton FWAppBarToggleButton FWAppBarSeparator FWCommandBar FWToolBar FWToolBarTray command bar toolbar material liquid glass split drop down"),
        Entry("Switches", "ToggleButton and ToggleSwitch states, events, keyboard toggles, and material-aware setting rows.", GalleryNavigationGroup.ControlSurfaces, FluentIconRegular.ToggleMultiple24, "FWToggleButton FWToggleSwitch checked unchecked indeterminate disabled toggled keyboard drag material settings"),
        Entry("Text Input", "TextBox, PasswordBox, NumberBox, AutoCompleteBox, and RichTextBox surfaces with states, filtering, and material input panels.", GalleryNavigationGroup.Input, FluentIconRegular.Textbox24, "FWTextBox FWPasswordBox FWNumberBox FWAutoCompleteBox FWRichTextBox search form input password reveal number step autocomplete suggestions rich text material"),
        Entry("Selection", "CheckBox, RadioButton, ComboBox, ComboBoxItem, and RatingControl surfaces with Select all, named groups, editable input, star ratings, and material settings.", GalleryNavigationGroup.Input, FluentIconRegular.CheckboxChecked24, "FWCheckBox FWRadioButton FWComboBox FWComboBoxItem FWRatingControl RatingControl pick choose select all three state radio group editable star rating material"),
        Entry("Range", "Slider, RangeSlider, ProgressBar, and ProgressRing controls with live values, snapped ranges, and material progress states.", GalleryNavigationGroup.Input, FluentIconRegular.Gauge24, "FWSlider FWRangeSlider FWProgressBar FWProgressRing value loading progress ring range snap tick vertical material"),
        Entry("Date and Time", "DatePicker, TimePicker, and Calendar controls with bounds, clock formats, blackout dates, and material planning surfaces.", GalleryNavigationGroup.Input, FluentIconRegular.CalendarLtr24, "FWDatePicker FWTimePicker FWCalendar schedule calendar date time appointment clock 12 hour 24 hour minute increment blackout bounds material liquid glass planning"),
        Entry("Content and Layout", "TextBlock, AccessText, Border, content hosts, panels, Grid, transitioning content, and LiquidGlass layout surfaces.", GalleryNavigationGroup.LayoutAndMedia, FluentIconRegular.LayoutColumnTwo24, "FWTextBlock FWAccessText FWBorder FWContentControl FWContentPresenter FWStackPanel FWWrapPanel FWGrid FWTransitioningContentControl layout content host material liquid glass"),
        Entry("Visuals", "Fluent icon library, image stretch and zoom, label targets, separators, Viewbox scaling, and LiquidGlass visual surfaces.", GalleryNavigationGroup.LayoutAndMedia, FluentIconRegular.Image24, "FWImage FWFontIcon FWSymbolIcon FWPathIcon FWLabel FWSeparator FWViewbox FluentIcon icon library regular filled visual image stretch zoom label separator viewbox material liquid glass"),
        Entry("Interaction", "ScrollViewer, SwipeControl, and GridSplitter controls with offset commands, swipe actions, keyboard increments, and LiquidGlass interaction surfaces.", GalleryNavigationGroup.LayoutAndMedia, FluentIconRegular.CursorClick24, "FWScrollViewer FWSwipeControl FWGridSplitter scrolling scroll viewer auto hide offset swipe reveal execute archive delete grid splitter resize keyboard drag increment material liquid glass interaction"),
        Entry("Input and Media", "ColorPicker, InkCanvas, InkPresenter, and MediaElement controls with alpha, hex, ink modes, stroke presentation, playback surfaces, and LiquidGlass media workbenches.", GalleryNavigationGroup.LayoutAndMedia, FluentIconRegular.Color24, "FWColorPicker FWInkCanvas FWInkPresenter FWMediaElement color picker alpha hex compact spectrum ink canvas draw erase taper stroke presenter media element play pause stop mute stretch playback material liquid glass"),
        Entry("Collections", "ListBox, ListView, TreeView, DataGrid, and TreeDataGrid controls with selection, hierarchy, table options, and material data surfaces.", GalleryNavigationGroup.CollectionsAndData, FluentIconRegular.Table24, "FWListBox FWListView FWTreeView FWDataGrid FWTreeDataGrid table list data grid hierarchy selection material liquid glass row height headers"),
        Entry("Selectors and Properties", "TreeSelector and PropertyGrid surfaces for hierarchical selection, object editing, cascade checks, and material property panels.", GalleryNavigationGroup.CollectionsAndData, FluentIconRegular.DatabaseSearch24, "FWTreeSelector FWTreeSelectorItem FWPropertyGrid tree selector property grid search categorized alphabetical cascade checkbox material liquid glass editor"),
        Entry("Data Inspectors", "DiffViewer, HexEditor, and JsonTreeViewer developer surfaces with diff, binary, JSON, and material inspection workbench states.", GalleryNavigationGroup.CollectionsAndData, FluentIconRegular.Code24, "FWDiffViewer FWHexEditor FWJsonTreeViewer diff hex json code data inspector binary material liquid glass minimap ascii"),
        Entry("Navigation", "NavigationView, pane modes, hierarchy, tabs, Frame navigation, and material app shell states.", GalleryNavigationGroup.AppStructure, FluentIconRegular.Navigation24, "FWNavigationView FWNavigationViewItem FWNavigationViewItemHeader FWNavigationViewItemSeparator pane display mode LeftCompact LeftMinimal Top hierarchy FWTabControl FWTabItem FWFrame page shell back forward material liquid glass app shell"),
        Entry("Window Backdrops", "DWM-backed shell materials for Jalium windows, including Mica, Mica Alt, Acrylic, and solid shell fallback.", GalleryNavigationGroup.Materials, FluentIconRegular.WindowBrush24, "FWFluentWindowBackdropKind FWFluentWindowBackdropRecipe WindowBackdropType SystemBackdrop DWM Mica MicaAlt Acrylic solid shell window backdrop material"),
        Entry("Materials and Effects", "Element backdrops, material roles, and WinUI-style layering for FluentJalium surfaces.", GalleryNavigationGroup.Materials, FluentIconRegular.TransparencySquare24, "FWFluentMaterialSurface FWFluentMaterialKind FWFluentMaterialRecipe BackdropEffect BlurEffect AcrylicEffect MicaEffect FrostedGlassEffect DropShadowEffect material role layer transient focused reveal HLSL shader liquid glass recipe preset"),
        Entry("Motion and Transitions", "Connected animation, shared element motion, and FW content transitions for FluentJalium navigation continuity.", GalleryNavigationGroup.Motion, FluentIconRegular.SlideTransition24, "FWConnectedAnimationService FWTransitioningContentControl connected animation shared element motion content transition continuity navigation suppress entrance drill in slide crossfade liquid morph"),
        Entry("Menus", "MenuBar, Menu, ContextMenu, MenuFlyout, and CommandBarFlyout surfaces with submenu, shortcut, and LiquidGlass workbench states.", GalleryNavigationGroup.AppStructure, FluentIconRegular.List24, "FWMenuBar FWMenu FWContextMenu FWMenuFlyout FWMenuFlyoutItem FWToggleMenuFlyoutItem FWMenuFlyoutSubItem FWMenuFlyoutSeparator FWCommandBarFlyout command menu flyout submenu shortcut material liquid glass workbench"),
        Entry("Disclosure", "Expander, ToolTip, ContentDialog, and GroupBox controls with command states and LiquidGlass disclosure panels.", GalleryNavigationGroup.AppStructure, FluentIconRegular.PanelLeft24, "FWExpander FWToolTip FWContentDialog FWGroupBox dialog tooltip expander group box disclosure material liquid glass panel"),
        Entry("Status", "InfoBar, InfoBadge, ToastNotification, and StatusBar controls with severity, queue, and material operation states.", GalleryNavigationGroup.AppStructure, FluentIconRegular.AlertBadge24, "FWInfoBar FWInfoBadge FWToastNotificationHost FWToastNotificationItem FWStatusBar FWStatusBarItem notification message severity toast queue max visible status bar item material liquid glass operations"),
        Entry("Design", "Gallery shell, catalog, sample card, and material entry design rules exposed as a footer navigation entry.", GalleryNavigationGroup.Diagnostics, FluentIconRegular.DesignIdeas24, "design footer gallery shell NavigationView Frame Page ControlInfoData catalog metadata sample card ControlExample materials backdrops FluentAvalonia WinUI WPFUI UI.WPF.Modern Jalium.UI", IsFooter: true),
        Entry("Settings", "Theme, accent, and Gallery diagnostics exposed as a footer navigation entry.", GalleryNavigationGroup.Diagnostics, FluentIconRegular.Settings24, "settings footer theme accent diagnostics NavigationView FooterMenuItems FluentThemeManager", IsFooter: true),
        Entry("State Matrix", "Cross-control normal, selected, disabled, and flyout state checks.", GalleryNavigationGroup.Diagnostics, FluentIconRegular.DataUsage24, "states normal hover pressed selected disabled light dark high contrast", GalleryPageStatus.Diagnostic, IsFooter: true)
    ];

    public static GalleryPageInfo[] CreatePageInfos()
    {
        return Entries.Select(CreatePageInfo).ToArray();
    }

    public static GalleryPage[] Create(IReadOnlyDictionary<string, Func<UIElement>> contentFactories)
    {
        return CreatePageInfos()
            .Select(pageInfo => new GalleryPage(pageInfo, ResolveFactory(pageInfo, contentFactories)))
            .ToArray();
    }

    private static GalleryCatalogEntry Entry(
        string title,
        string description,
        string group,
        FluentIconRegular icon,
        string keywords,
        GalleryPageStatus status = GalleryPageStatus.Stable,
        bool IsFooter = false)
    {
        return new GalleryCatalogEntry(title, description, group, icon, keywords, status, IsFooter);
    }

    private static GalleryPageInfo CreatePageInfo(GalleryCatalogEntry entry)
    {
        var tags = entry.Keywords.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var info = new GalleryPageInfo(
            CreateUniqueId(entry.Title),
            entry.Title,
            CreateSubtitle(entry.Group, entry.Status),
            entry.Description,
            entry.Group,
            entry.Icon,
            tags,
            CreateRelatedControls(tags),
            CreateDocumentationLinks(entry.Title, tags),
            entry.Status,
            entry.IsFooter);

        return info;
    }

    private static Func<UIElement> ResolveFactory(GalleryPageInfo pageInfo, IReadOnlyDictionary<string, Func<UIElement>> contentFactories)
    {
        if (contentFactories.TryGetValue(pageInfo.UniqueId, out var createContent))
        {
            return createContent;
        }

        throw new InvalidOperationException($"Gallery page factory is missing for '{pageInfo.UniqueId}' ({pageInfo.Title}).");
    }

    internal static string CreateUniqueId(string title)
    {
        return string.Concat(title
            .Where(character => char.IsLetterOrDigit(character))
            .Select(char.ToLowerInvariant));
    }

    private static string CreateSubtitle(string group, GalleryPageStatus status)
    {
        return status == GalleryPageStatus.Stable
            ? group
            : $"{group} - {status}";
    }

    private static string[] CreateRelatedControls(IEnumerable<string> tags)
    {
        return tags
            .Where(tag => tag.StartsWith("FW", StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .Take(8)
            .ToArray();
    }

    private static GalleryDocumentationLink[] CreateDocumentationLinks(string title, IEnumerable<string> tags)
    {
        var links = new List<GalleryDocumentationLink>
        {
            new("WinUI Gallery", $"https://github.com/microsoft/WinUI-Gallery/search?q={Uri.EscapeDataString(title)}"),
            new("FluentJalium source", $"https://github.com/search?q={Uri.EscapeDataString("FluentJalium " + title)}")
        };

        var firstControl = tags.FirstOrDefault(tag => tag.StartsWith("FW", StringComparison.Ordinal));
        if (!string.IsNullOrWhiteSpace(firstControl))
        {
            links.Add(new("Related FW control", $"https://github.com/search?q={Uri.EscapeDataString("FluentJalium " + firstControl)}"));
        }

        return links.ToArray();
    }
}
