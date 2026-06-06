using FluentJalium.Gallery.Services;
using FluentJalium.Icon;
using Jalium.UI;

namespace FluentJalium.Gallery.Models;

internal sealed record GalleryCatalogEntry(
    string UniqueId,
    string GroupId,
    FluentIconRegular Icon,
    string Keywords,
    GalleryPageStatus Status = GalleryPageStatus.Stable,
    bool IsFooter = false);

internal static class GalleryCatalog
{
    private static readonly GalleryCatalogEntry[] Entries =
    [
        Entry("overview", GalleryNavigationGroup.Home, FluentIconRegular.Home24, "home design system theme typography accent light dark high contrast"),
        Entry("themearchitecture", GalleryNavigationGroup.Design, FluentIconRegular.Diagram24, "Generic.jalxaml FluentResources.jalxaml FluentControls.jalxaml FluentThemeManager theme resources controls dictionary FW control architecture FluentAvalonia WinUI WPFUI gallery design"),
        Entry("colors", GalleryNavigationGroup.Design, FluentIconRegular.Color24, "FluentColors AccentBrush AccentFillColor TextPrimary TextSecondary ControlFillColor LayerFillColor SelectionBackground HyperlinkForeground ProgressBarForeground semantic color token design"),
        Entry("typography", GalleryNavigationGroup.Design, FluentIconRegular.TextFont24, "FluentTypography DisplayFontFamily BodyFontFamily MonoFontFamily FluentCaptionFontSize FluentBodyFontSize FluentSubtitleFontSize FluentTitleFontSize ControlContentThemeFontSize typography type ramp font design"),
        Entry("geometry", GalleryNavigationGroup.Design, FluentIconRegular.Ruler24, "ControlCornerRadius OverlayCornerRadius CardCornerRadius CompactCornerRadius FluentControlBorderThickness ControlElevationBorderBrush AccentControlElevationBorderBrush FluentGeometry radius corner stroke border elevation shadow WinUI geometry design token"),
        Entry("motiontokens", GalleryNavigationGroup.Design, FluentIconRegular.SlideTransition24, "FluentMotionDurationFast FluentMotionDurationNormal FluentMotionDurationEmphasized FluentMotionConnectedAnimationDuration FluentMotionConnectedAnimationInitialOpacity FWConnectedAnimationConfiguration TransitionMode motion token connected animation design"),
        Entry("buttons", GalleryNavigationGroup.ControlSurfaces, FluentIconRegular.ControlButton24, "FWButton FWRepeatButton FWHyperlinkButton FWDropDownButton FWSplitButton FWToggleSplitButton FWAppBarButton FWAppBarToggleButton FWAppBarSeparator FWCommandBar FWToolBar FWToolBarTray command bar toolbar material liquid glass split drop down"),
        Entry("switches", GalleryNavigationGroup.ControlSurfaces, FluentIconRegular.ToggleMultiple24, "FWToggleButton FWToggleSwitch checked unchecked indeterminate disabled toggled keyboard drag material settings"),
        Entry("textinput", GalleryNavigationGroup.Input, FluentIconRegular.Textbox24, "FWTextBox FWPasswordBox FWNumberBox FWAutoCompleteBox FWRichTextBox search form input password reveal number step autocomplete suggestions rich text material"),
        Entry("selection", GalleryNavigationGroup.Input, FluentIconRegular.CheckboxChecked24, "FWCheckBox FWRadioButton FWComboBox FWComboBoxItem FWRatingControl RatingControl pick choose select all three state radio group editable star rating material"),
        Entry("range", GalleryNavigationGroup.Input, FluentIconRegular.Gauge24, "FWSlider FWRangeSlider FWProgressBar FWProgressRing value loading progress ring range snap tick vertical material"),
        Entry("dateandtime", GalleryNavigationGroup.Input, FluentIconRegular.CalendarLtr24, "FWDatePicker FWTimePicker FWCalendar schedule calendar date time appointment clock 12 hour 24 hour minute increment blackout bounds material liquid glass planning"),
        Entry("contentandlayout", GalleryNavigationGroup.LayoutAndMedia, FluentIconRegular.LayoutColumnTwo24, "FWTextBlock FWAccessText FWBorder FWContentControl FWContentPresenter FWStackPanel FWWrapPanel FWGrid FWTransitioningContentControl layout content host material liquid glass"),
        Entry("visuals", GalleryNavigationGroup.LayoutAndMedia, FluentIconRegular.Image24, "FWImage FWFontIcon FWSymbolIcon FWPathIcon FWLabel FWSeparator FWViewbox FluentIcon icon library regular filled visual image stretch zoom label separator viewbox material liquid glass"),
        Entry("interaction", GalleryNavigationGroup.LayoutAndMedia, FluentIconRegular.CursorClick24, "FWScrollViewer FWSwipeControl FWGridSplitter scrolling scroll viewer auto hide offset swipe reveal execute archive delete grid splitter resize keyboard drag increment material liquid glass interaction"),
        Entry("inputandmedia", GalleryNavigationGroup.LayoutAndMedia, FluentIconRegular.Color24, "FWColorPicker FWInkCanvas FWInkPresenter FWMediaElement color picker alpha hex compact spectrum ink canvas draw erase taper stroke presenter media element play pause stop mute stretch playback material liquid glass"),
        Entry("collections", GalleryNavigationGroup.CollectionsAndData, FluentIconRegular.Table24, "FWListBox FWListView FWTreeView FWDataGrid FWTreeDataGrid table list data grid hierarchy selection material liquid glass row height headers"),
        Entry("selectorsandproperties", GalleryNavigationGroup.CollectionsAndData, FluentIconRegular.DatabaseSearch24, "FWTreeSelector FWTreeSelectorItem FWPropertyGrid tree selector property grid search categorized alphabetical cascade checkbox material liquid glass editor"),
        Entry("datainspectors", GalleryNavigationGroup.CollectionsAndData, FluentIconRegular.Code24, "FWDiffViewer FWHexEditor FWJsonTreeViewer diff hex json code data inspector binary material liquid glass minimap ascii"),
        Entry("navigation", GalleryNavigationGroup.AppStructure, FluentIconRegular.Navigation24, "FWNavigationView FWNavigationViewItem FWNavigationViewItemHeader FWNavigationViewItemSeparator pane display mode LeftCompact LeftMinimal Top hierarchy FWTabControl FWTabItem FWFrame page shell back forward material liquid glass app shell"),
        Entry("windowbackdrops", GalleryNavigationGroup.Materials, FluentIconRegular.WindowBrush24, "FWFluentWindowBackdropKind FWFluentWindowBackdropRecipe WindowBackdropType SystemBackdrop DWM Mica MicaAlt Acrylic solid shell window backdrop material"),
        Entry("materialsand effects".Replace(" ", string.Empty), GalleryNavigationGroup.Materials, FluentIconRegular.TransparencySquare24, "FWFluentMaterialSurface FWFluentMaterialKind FWFluentMaterialRecipe BackdropEffect BlurEffect AcrylicEffect MicaEffect FrostedGlassEffect DropShadowEffect material role layer transient focused reveal HLSL shader liquid glass recipe preset"),
        Entry("motionandtransitions", GalleryNavigationGroup.Motion, FluentIconRegular.SlideTransition24, "FWConnectedAnimationService FWTransitioningContentControl connected animation shared element motion content transition continuity navigation suppress entrance drill in slide crossfade liquid morph"),
        Entry("menus", GalleryNavigationGroup.AppStructure, FluentIconRegular.List24, "FWMenuBar FWMenu FWContextMenu FWMenuFlyout FWMenuFlyoutItem FWToggleMenuFlyoutItem FWMenuFlyoutSubItem FWMenuFlyoutSeparator FWCommandBarFlyout command menu flyout submenu shortcut material liquid glass workbench"),
        Entry("disclosure", GalleryNavigationGroup.AppStructure, FluentIconRegular.PanelLeft24, "FWExpander FWToolTip FWContentDialog FWGroupBox dialog tooltip expander group box disclosure material liquid glass panel"),
        Entry("status", GalleryNavigationGroup.AppStructure, FluentIconRegular.AlertBadge24, "FWInfoBar FWInfoBadge FWToastNotificationHost FWToastNotificationItem FWStatusBar FWStatusBarItem notification message severity toast queue max visible status bar item material liquid glass operations"),
        Entry("design", GalleryNavigationGroup.Diagnostics, FluentIconRegular.DesignIdeas24, "design footer gallery shell NavigationView Frame Page ControlInfoData catalog metadata sample card ControlExample materials backdrops FluentAvalonia WinUI WPFUI UI.WPF.Modern Jalium.UI", IsFooter: true),
        Entry("settings", GalleryNavigationGroup.Diagnostics, FluentIconRegular.Settings24, "settings footer theme accent language diagnostics NavigationView FooterMenuItems FluentThemeManager", IsFooter: true),
        Entry("statematrix", GalleryNavigationGroup.Diagnostics, FluentIconRegular.DataUsage24, "states normal hover pressed selected disabled light dark high contrast", GalleryPageStatus.Diagnostic, IsFooter: true)
    ];

    public static GalleryPageInfo[] CreatePageInfos(GalleryLocalizationService localization)
    {
        return Entries.Select(entry => CreatePageInfo(entry, localization)).ToArray();
    }

    public static GalleryPage[] Create(GalleryLocalizationService localization, IReadOnlyDictionary<string, Func<UIElement>> contentFactories)
    {
        return CreatePageInfos(localization)
            .Select(pageInfo => new GalleryPage(pageInfo, ResolveFactory(pageInfo, contentFactories)))
            .ToArray();
    }

    private static GalleryCatalogEntry Entry(
        string uniqueId,
        string groupId,
        FluentIconRegular icon,
        string keywords,
        GalleryPageStatus status = GalleryPageStatus.Stable,
        bool IsFooter = false)
    {
        return new GalleryCatalogEntry(uniqueId, groupId, icon, keywords, status, IsFooter);
    }

    private static GalleryPageInfo CreatePageInfo(GalleryCatalogEntry entry, GalleryLocalizationService localization)
    {
        var localizedKeywords = localization.PageKeywords(entry.UniqueId);
        var tags = $"{entry.Keywords} {localizedKeywords}".Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var title = localization.PageTitle(entry.UniqueId);
        var group = localization.GroupName(entry.GroupId);
        var info = new GalleryPageInfo(
            entry.UniqueId,
            title,
            CreateSubtitle(group, entry.Status, localization),
            localization.PageDescription(entry.UniqueId),
            entry.GroupId,
            group,
            entry.Icon,
            tags,
            CreateRelatedControls(tags),
            CreateDocumentationLinks(entry.UniqueId, title, tags, localization),
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

    private static string CreateSubtitle(string group, GalleryPageStatus status, GalleryLocalizationService localization)
    {
        return status == GalleryPageStatus.Stable
            ? group
            : $"{group} - {localization.Text($"status.{status.ToString().ToLowerInvariant()}")}";
    }

    private static string[] CreateRelatedControls(IEnumerable<string> tags)
    {
        return tags
            .Where(tag => tag.StartsWith("FW", StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .Take(8)
            .ToArray();
    }

    private static GalleryDocumentationLink[] CreateDocumentationLinks(string uniqueId, string title, IEnumerable<string> tags, GalleryLocalizationService localization)
    {
        var links = new List<GalleryDocumentationLink>
        {
            new(localization.Text("doc.winuiGallery"), $"https://github.com/microsoft/WinUI-Gallery/search?q={Uri.EscapeDataString(title)}"),
            new(localization.Text("doc.fluentJaliumSource"), $"https://github.com/search?q={Uri.EscapeDataString("FluentJalium " + title)}")
        };

        var firstControl = tags.FirstOrDefault(tag => tag.StartsWith("FW", StringComparison.Ordinal));
        if (!string.IsNullOrWhiteSpace(firstControl))
        {
            links.Add(new(localization.Text("doc.relatedFwControl"), $"https://github.com/search?q={Uri.EscapeDataString("FluentJalium " + firstControl)}"));
        }

        return links.ToArray();
    }
}
