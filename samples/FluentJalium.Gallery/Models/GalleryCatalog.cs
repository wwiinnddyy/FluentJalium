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
    bool IsFooter = false,
    bool IsNew = false,
    bool IsUpdated = false,
    string? SourcePath = null,
    string[]? BaseClasses = null,
    string? ApiNamespace = null,
    string[]? RelatedControls = null,
    string? SampleCodeKey = null);

internal static class GalleryCatalog
{
    private static readonly GalleryCatalogEntry[] Entries =
    [
        Entry("overview", GalleryNavigationGroup.Home, FluentIconRegular.Home24, "home design system theme typography accent light dark high contrast"),
        Entry("allcontrols", GalleryNavigationGroup.Catalog, FluentIconRegular.DocumentBulletList24, "all controls catalog metadata filter FW control index", SourcePath: "/GalleryCatalog/AllControls", BaseClasses: ["GalleryPageInfo"], ApiNamespace: "FluentJalium.Gallery.Models", RelatedControls: ["FWNavigationView", "FWFrame"], SampleCodeKey: "catalog.filter.all"),
        Entry("newcontrols", GalleryNavigationGroup.Catalog, FluentIconRegular.New24, "new controls catalog metadata filter first wave FW control index", SourcePath: "/GalleryCatalog/NewControls", BaseClasses: ["GalleryPageInfo"], ApiNamespace: "FluentJalium.Gallery.Models", RelatedControls: ["FWNavigationView", "FWFrame"], SampleCodeKey: "catalog.filter.new"),
        Entry("updatedcontrols", GalleryNavigationGroup.Catalog, FluentIconRegular.ArrowClockwise24, "updated controls catalog metadata filter improved FW control index", SourcePath: "/GalleryCatalog/UpdatedControls", BaseClasses: ["GalleryPageInfo"], ApiNamespace: "FluentJalium.Gallery.Models", RelatedControls: ["FWNavigationView", "FWFrame"], SampleCodeKey: "catalog.filter.updated"),
        Entry("previewcontrols", GalleryNavigationGroup.Catalog, FluentIconRegular.Sparkle24, "preview controls catalog metadata filter experimental FW control index", SourcePath: "/GalleryCatalog/PreviewControls", BaseClasses: ["GalleryPageInfo"], ApiNamespace: "FluentJalium.Gallery.Models", RelatedControls: ["FWNavigationView", "FWFrame"], SampleCodeKey: "catalog.filter.preview"),
        Entry("diagnosticcontrols", GalleryNavigationGroup.Catalog, FluentIconRegular.DataUsage24, "diagnostic controls catalog metadata filter state matrix diagnostics FW control index", SourcePath: "/GalleryCatalog/DiagnosticControls", BaseClasses: ["GalleryPageInfo"], ApiNamespace: "FluentJalium.Gallery.Models", RelatedControls: ["FWNavigationView", "FWFrame"], SampleCodeKey: "catalog.filter.diagnostic"),
        Entry("themearchitecture", GalleryNavigationGroup.Design, FluentIconRegular.Diagram24, "Generic.jalxaml FluentResources.jalxaml FluentControls.jalxaml FluentThemeManager theme resources controls dictionary FW control architecture FluentAvalonia WinUI WPFUI gallery design", IsUpdated: true, SourcePath: "/Design/ThemeArchitecture", BaseClasses: ["ResourceDictionary", "Style", "Control"], ApiNamespace: "FluentJalium.Controls.Themes", RelatedControls: ["FluentThemeManager", "AliasStyle", "IFluentJaliumControl"], SampleCodeKey: "design.themearchitecture"),
        Entry("colors", GalleryNavigationGroup.Design, FluentIconRegular.Color24, "FluentColors AccentBrush AccentFillColor TextPrimary TextSecondary ControlFillColor LayerFillColor SelectionBackground HyperlinkForeground ProgressBarForeground semantic color token design", IsUpdated: true, SourcePath: "/Design/Colors", BaseClasses: ["ResourceDictionary", "Brush", "Color"], ApiNamespace: "FluentJalium.Controls.Themes", RelatedControls: ["FluentColors", "FluentThemeManager", "ThemeResource"], SampleCodeKey: "design.colors"),
        Entry("typography", GalleryNavigationGroup.Design, FluentIconRegular.TextFont24, "FluentTypography DisplayFontFamily BodyFontFamily MonoFontFamily FluentCaptionFontSize FluentBodyFontSize FluentSubtitleFontSize FluentTitleFontSize ControlContentThemeFontSize typography type ramp font design", IsUpdated: true, SourcePath: "/Design/Typography", BaseClasses: ["ResourceDictionary", "TextBlock", "FontFamily"], ApiNamespace: "FluentJalium.Controls.Themes", RelatedControls: ["FluentTypography", "FluentThemeManager", "ThemeResource"], SampleCodeKey: "design.typography"),
        Entry("geometry", GalleryNavigationGroup.Design, FluentIconRegular.Ruler24, "ControlCornerRadius OverlayCornerRadius CardCornerRadius CompactCornerRadius FluentControlBorderThickness ControlElevationBorderBrush AccentControlElevationBorderBrush FluentGeometry radius corner stroke border elevation shadow WinUI geometry design token", IsUpdated: true, SourcePath: "/Design/Geometry", BaseClasses: ["ResourceDictionary", "CornerRadius", "Thickness"], ApiNamespace: "FluentJalium.Controls.Themes", RelatedControls: ["FluentGeometry", "FluentThemeManager", "ThemeResource"], SampleCodeKey: "design.geometry"),
        Entry("motiontokens", GalleryNavigationGroup.Design, FluentIconRegular.SlideTransition24, "FluentMotionDurationFast FluentMotionDurationNormal FluentMotionDurationEmphasized FluentMotionConnectedAnimationDuration FluentMotionConnectedAnimationInitialOpacity FWConnectedAnimationConfiguration TransitionMode motion token connected animation design", IsUpdated: true, SourcePath: "/Design/MotionTokens", BaseClasses: ["ResourceDictionary", "TransitioningContentControl", "Duration"], ApiNamespace: "FluentJalium.Controls.Themes", RelatedControls: ["FluentMotion", "TransitionMode", "ConnectedAnimation"], SampleCodeKey: "design.motiontokens"),
        Entry("buttons", GalleryNavigationGroup.ControlSurfaces, FluentIconRegular.ControlButton24, "FWButton FWRepeatButton FWHyperlinkButton FWDropDownButton FWSplitButton FWToggleSplitButton FWAppBarButton FWAppBarToggleButton FWAppBarSeparator FWCommandBar FWToolBar FWToolBarTray command bar toolbar primary secondary density compact material liquid glass split drop down", IsUpdated: true, SourcePath: "/Buttons/FWCommandBar", BaseClasses: ["Button", "ToggleButton", "CommandBar", "ToolBar"], ApiNamespace: "FluentJalium.Controls", RelatedControls: ["FWButton", "FWRepeatButton", "FWHyperlinkButton", "FWDropDownButton", "FWSplitButton", "FWToggleSplitButton", "FWAppBarButton", "FWAppBarToggleButton", "FWCommandBar", "FWToolBar"], SampleCodeKey: "buttons.commandbar"),
        Entry("switches", GalleryNavigationGroup.ControlSurfaces, FluentIconRegular.ToggleMultiple24, "FWToggleButton FWToggleSwitch checked unchecked indeterminate disabled toggled keyboard drag material settings boolean state", IsUpdated: true, SourcePath: "/Switches/FWToggleSwitch", BaseClasses: ["ToggleButton", "CheckBox"], ApiNamespace: "FluentJalium.Controls", RelatedControls: ["FWToggleButton", "FWToggleSwitch", "FWCheckBox", "FWSettingsCard"], SampleCodeKey: "switches.toggle"),
        Entry("textinput", GalleryNavigationGroup.Input, FluentIconRegular.Textbox24, "FWTextBox FWPasswordBox FWNumberBox FWAutoCompleteBox FWAutoSuggestBox FWRichTextBox QuerySubmitted SuggestionChosen LastTextChangeReason search form input password reveal number step autocomplete autosuggest suggestions rich text material WinUI AutoSuggestBox", IsUpdated: true, SourcePath: "/TextInput/FWAutoSuggestBox", BaseClasses: ["TextBox", "AutoCompleteBox", "RichTextBox"], ApiNamespace: "FluentJalium.Controls", RelatedControls: ["FWTextBox", "FWPasswordBox", "FWNumberBox", "FWAutoCompleteBox", "FWAutoSuggestBox", "FWRichTextBox"], SampleCodeKey: "textinput.autosuggestbox"),
        Entry("selection", GalleryNavigationGroup.Input, FluentIconRegular.CheckboxChecked24, "FWCheckBox FWRadioButton FWRadioButtons FWComboBox FWComboBoxItem FWRatingControl RatingControl pick choose select all three state radio group editable star rating material", IsUpdated: true, SourcePath: "/Selection/FWRadioButtons", BaseClasses: ["Selector", "ItemsControl"], ApiNamespace: "FluentJalium.Controls", RelatedControls: ["FWCheckBox", "FWRadioButton", "FWRadioButtons", "FWComboBox", "FWRatingControl"], SampleCodeKey: "selection.radiobuttons"),
        Entry("range", GalleryNavigationGroup.Input, FluentIconRegular.Gauge24, "FWSlider FWRangeSlider FWProgressBar FWProgressRing value loading progress ring range snap tick vertical indeterminate density material", IsUpdated: true, SourcePath: "/Range/FWSlider", BaseClasses: ["RangeBase", "Slider", "ProgressBar", "Control"], ApiNamespace: "FluentJalium.Controls", RelatedControls: ["FWSlider", "FWRangeSlider", "FWProgressBar", "FWProgressRing"], SampleCodeKey: "range.slider.progress"),
        Entry("dateandtime", GalleryNavigationGroup.Input, FluentIconRegular.CalendarLtr24, "FWDatePicker FWCalendarDatePicker FWTimePicker FWCalendar FWCalendarView schedule calendar date time appointment clock 12 hour 24 hour minute increment blackout bounds material liquid glass planning WinUI CalendarDatePicker CalendarView", IsUpdated: true, SourcePath: "/DateTime/FWCalendarDatePicker", BaseClasses: ["DatePicker", "TimePicker", "Calendar"], ApiNamespace: "FluentJalium.Controls", RelatedControls: ["FWDatePicker", "FWCalendarDatePicker", "FWTimePicker", "FWCalendar", "FWCalendarView"], SampleCodeKey: "datetime.calendarpicker.calendarview"),
        Entry("contentandlayout", GalleryNavigationGroup.LayoutAndMedia, FluentIconRegular.LayoutColumnTwo24, "FWTextBlock FWAccessText FWCanvas FWBorder FWContentControl FWContentPresenter FWStackPanel FWWrapPanel FWGrid FWRelativePanel FWSplitView FWTwoPaneView FWParallaxView FWSettingsCard FWSettingsCardDiagnostics FWSettingsCardAutomationPeer FWSettingsCardAutomationDiagnostics FWTransitioningContentControl layout content host splitview pane adaptive settings row rows SettingsCard Click Command CanExecute CommandParameter ClickMode Hover action keyboard invocation invoke pattern automation peer automation diagnostics IsInvokable IsInteractionPressed Name HelpText settings page value material liquid glass WinUI Fluent CommunityToolkit WPFUI IsPaneOpen DisplayMode PanePlacement OpenPaneLength CompactPaneLength ActualPaneLength CompactInline CompactOverlay ActualMode VisiblePane Diagnostics Progress CurrentOffset SourceKind SourceOrientation ScrollViewer FWScroller adaptive diagnostics parallax progress offset scroll source", IsUpdated: true, SourcePath: "/Layout/FWSplitView", BaseClasses: ["ContentControl", "Control", "Panel", "Grid", "FrameworkElement"], ApiNamespace: "FluentJalium.Controls", RelatedControls: ["FWSplitView", "FWTwoPaneView", "FWParallaxView", "FWScroller", "FWSettingsCard", "FWSettingsCardDiagnostics", "FWSettingsCardAutomationPeer", "FWSettingsCardAutomationDiagnostics", "FWSettingsExpander", "FWContentControl", "FWStackPanel", "FWGrid", "FWButton"], SampleCodeKey: "layout.splitview.settingscard"),
        Entry("visuals", GalleryNavigationGroup.LayoutAndMedia, FluentIconRegular.Image24, "FWImage FWBitmapIcon FWImageIcon FWRichTextBlock FWFontIcon FWSymbolIcon FWPathIcon FWLabel FWSeparator FWViewbox FWPersonPicture FWMarkdown FWQRCode FWRectangle FWEllipse FWLine FWPolyline FWPolygon FWPath FluentIcon icon library regular filled visual image stretch zoom label separator viewbox rich text markdown avatar person picture profile initials badge qr qrcode barcode shape rectangle ellipse line polyline polygon path geometry fill stroke material liquid glass", IsUpdated: true, SourcePath: "/Visuals/FWPersonPicture", BaseClasses: ["Image", "TextBlock", "Control", "Markdown", "QRCode", "Shape"], ApiNamespace: "FluentJalium.Controls", RelatedControls: ["FWPersonPicture", "FWMarkdown", "FWQRCode", "FWRectangle", "FWEllipse", "FWPath", "FWImage", "FWBitmapIcon", "FWImageIcon", "FWRichTextBlock", "FWFontIcon", "FWSymbolIcon", "FWPathIcon", "FWLabel", "FWSeparator", "FWViewbox"], SampleCodeKey: "visuals.icons.richtext.personpicture.markdown.qrcode.shapes"),
        Entry("interaction", GalleryNavigationGroup.LayoutAndMedia, FluentIconRegular.CursorClick24, "FWScrollViewer FWSwipeControl FWGridSplitter scrolling scroll viewer auto hide offset swipe reveal execute archive delete grid splitter resize keyboard drag increment material liquid glass interaction", IsUpdated: true, SourcePath: "/Interaction/FWScrollViewer", BaseClasses: ["ScrollViewer", "SwipeControl", "GridSplitter", "Control"], ApiNamespace: "FluentJalium.Controls", RelatedControls: ["FWScrollViewer", "FWSwipeControl", "FWGridSplitter", "FWScroller", "FWRefreshContainer", "FWAnnotatedScrollBar"], SampleCodeKey: "interaction.scrollviewer.swipe.splitter"),
        Entry("inputandmedia", GalleryNavigationGroup.LayoutAndMedia, FluentIconRegular.Color24, "FWColorPicker FWInkCanvas FWInkPresenter FWMediaElement color picker alpha hex compact spectrum ink canvas draw erase taper stroke presenter media element play pause stop mute stretch playback material liquid glass", IsUpdated: true, SourcePath: "/InputMedia/FWColorPicker", BaseClasses: ["Control", "Canvas", "MediaElement"], ApiNamespace: "FluentJalium.Controls", RelatedControls: ["FWColorPicker", "FWInkCanvas", "FWInkPresenter", "FWMediaElement"], SampleCodeKey: "inputmedia.color.ink.media"),
        Entry("collections", GalleryNavigationGroup.CollectionsAndData, FluentIconRegular.Table24, "FWListBox FWListView FWGridView FWGridViewItem FWTreeView FWDataGrid FWTreeDataGrid FWProgressBar table list data grid gridview hierarchy selection empty loading grouped group rows density compact high-density state matrix material liquid glass row height headers WinUI GridView", IsUpdated: true, SourcePath: "/Collections/CollectionStates", BaseClasses: ["ListBox", "ListView", "ListViewItem", "GridView", "DataGrid"], ApiNamespace: "FluentJalium.Controls", RelatedControls: ["FWListBox", "FWListView", "FWGridView", "FWGridViewItem", "FWTreeView", "FWDataGrid", "FWTreeDataGrid", "FWProgressBar"], SampleCodeKey: "collections.gridview"),
        Entry("advancedcollections", GalleryNavigationGroup.CollectionsAndData, FluentIconRegular.AppFolder24, "FWItemsRepeater FWScroller items repeater viewport scroll source range realization recycling diagnostics layout stack grid cache estimated item extent advanced collections first wave", GalleryPageStatus.Preview, IsNew: true, SourcePath: "/Collections/FWItemsRepeater", BaseClasses: ["Panel", "FrameworkElement"], ApiNamespace: "FluentJalium.Controls", RelatedControls: ["FWItemsRepeater", "FWScroller", "FWScrollViewer", "FWListView", "FWTreeView", "FWDataGrid"], SampleCodeKey: "advancedcollections.itemsrepeater"),
        Entry("selectorsandproperties", GalleryNavigationGroup.CollectionsAndData, FluentIconRegular.DatabaseSearch24, "FWTreeSelector FWTreeSelectorItem FWPropertyGrid tree selector property grid search categorized alphabetical cascade checkbox material liquid glass editor metadata", IsUpdated: true, SourcePath: "/Collections/FWTreeSelector", BaseClasses: ["TreeView", "Control", "ItemsControl"], ApiNamespace: "FluentJalium.Controls", RelatedControls: ["FWTreeSelector", "FWTreeSelectorItem", "FWPropertyGrid", "FWTreeView"], SampleCodeKey: "selectors.properties"),
        Entry("datainspectors", GalleryNavigationGroup.CollectionsAndData, FluentIconRegular.Code24, "FWDiffViewer FWHexEditor FWJsonTreeViewer diff hex json code data inspector binary material liquid glass minimap ascii diagnostics", IsUpdated: true, SourcePath: "/Data/FWDiffViewer", BaseClasses: ["Control", "TextBox", "ItemsControl"], ApiNamespace: "FluentJalium.Controls", RelatedControls: ["FWDiffViewer", "FWHexEditor", "FWJsonTreeViewer", "FWDataGrid"], SampleCodeKey: "datainspectors.viewers"),
        Entry("charts", GalleryNavigationGroup.CollectionsAndData, FluentIconRegular.ChartMultiple24, "FWLineChart FWBarChart FWPieChart FWScatterPlot FWHeatmap FWSparkline FWGaugeChart FWTreeMap FWCandlestickChart FWNetworkGraph FWGanttChart FWSankeyDiagram FWChartLegend FWChartTooltip chart charts graph analytics line bar pie scatter heatmap sparkline gauge treemap candlestick network gantt sankey legend tooltip palette axis dashboard material liquid glass data visualization WinUI FluentAvalonia WPFUI", IsUpdated: true, SourcePath: "/Charts/FWLineChart", BaseClasses: ["ChartBase", "AxisChartBase", "Control", "ContentControl"], ApiNamespace: "FluentJalium.Controls", RelatedControls: ["FWLineChart", "FWBarChart", "FWPieChart", "FWScatterPlot", "FWHeatmap", "FWSparkline", "FWGaugeChart", "FWTreeMap", "FWCandlestickChart", "FWNetworkGraph", "FWGanttChart", "FWSankeyDiagram", "FWChartLegend", "FWChartTooltip"], SampleCodeKey: "charts.family"),
        Entry("navigation", GalleryNavigationGroup.AppStructure, FluentIconRegular.Navigation24, "FWNavigationView FWNavigationViewItem FWNavigationViewItemHeader FWNavigationViewItemSeparator FWNavigationService FWNavigationRoute FWNavigationServiceDiagnostics FWBreadcrumbBar FWPipsPager FWSelectorBar FWSelectorBarItem FWSelectorBarDiagnostics FWTabView FWTabViewItem FWTabViewDiagnostics FWTitleBar FWTitleBarButton RouteKey route routing page service navigation service diagnostics pane display mode LeftCompact LeftMinimal Top hierarchy breadcrumb trail pips pager page index selector selectorbar selected index selected text item count indicator placement tab tabview selected header selected content tab width close overlay titlebar title bar shell app shell window chrome back forward history material liquid glass WPFUI", IsUpdated: true, SourcePath: "/Navigation/FWNavigationService", BaseClasses: ["NavigationView", "Frame", "Page", "Control", "Selector", "TabItem", "ContentControl", "TitleBar"], ApiNamespace: "FluentJalium.Controls", RelatedControls: ["FWNavigationService", "FWNavigationRoute", "FWNavigationServiceDiagnostics", "FWNavigationView", "FWNavigationViewItem", "FWNavigationViewItemHeader", "FWNavigationViewItemSeparator", "FWFrame", "FWBreadcrumbBar", "FWPipsPager", "FWSelectorBar", "FWSelectorBarDiagnostics", "FWSelectorBarItem", "FWTabView", "FWTabViewDiagnostics", "FWTabViewItem", "FWTitleBar", "FWTitleBarButton"], SampleCodeKey: "navigation.breadcrumb.pips.selector.tabview.titlebar"),
        Entry("windowbackdrops", GalleryNavigationGroup.Materials, FluentIconRegular.WindowBrush24, "FWFluentWindowSurface FWFluentWindowMaterialProfile FWFluentWindowBackdropKind FWFluentWindowBackdropRecipe WindowBackdropType SystemBackdrop DWM Mica MicaAlt Acrylic solid shell window backdrop material root surface auto apply profile", IsUpdated: true, SourcePath: "/Materials/FWFluentWindowSurface", BaseClasses: ["Border", "FWFluentMaterialSurface"], ApiNamespace: "FluentJalium.Controls", RelatedControls: ["FWFluentWindowSurface", "FWFluentWindowBackdropRecipe", "FWFluentWindowMaterialProfileRecipe", "FWFluentMaterialSurface"], SampleCodeKey: "materials.windowbackdrop"),
        Entry("materialsand effects".Replace(" ", string.Empty), GalleryNavigationGroup.Materials, FluentIconRegular.TransparencySquare24, "FWFluentMaterialSurface FWLayerSurface FWMicaSurface FWMicaAltSurface FWAcrylicSurface FWFrostedGlassSurface FWCardSurface FWFlyoutSurface FWFocusGlassSurface FWFluentWindowSurface FWFluentMaterialKind FWFluentMaterialRecipe BackdropEffect BlurEffect AcrylicEffect MicaEffect FrostedGlassEffect DropShadowEffect material role layer transient focused reveal HLSL shader liquid glass recipe preset derived convenience surfaces", IsUpdated: true, SourcePath: "/Materials/FWFluentMaterialSurface", BaseClasses: ["Border"], ApiNamespace: "FluentJalium.Controls", RelatedControls: ["FWFluentMaterialSurface", "FWLayerSurface", "FWMicaSurface", "FWMicaAltSurface", "FWAcrylicSurface", "FWFrostedGlassSurface", "FWCardSurface", "FWFlyoutSurface", "FWFocusGlassSurface", "FWFluentWindowSurface"], SampleCodeKey: "materials.derivedsurfaces"),
        Entry("materialprimitives", GalleryNavigationGroup.Materials, FluentIconRegular.WindowBrush24, "FWBackdrop FWAcrylicBrush acrylic mica micaalt tabbed material primitives backdrop brush", GalleryPageStatus.Preview, IsUpdated: true, SourcePath: "/Materials/FWBackdrop", BaseClasses: ["Control", "Object"], ApiNamespace: "FluentJalium.Controls", RelatedControls: ["FWBackdrop", "FWAcrylicBrush", "FWFluentMaterialSurface"], SampleCodeKey: "materialprimitives.backdrop"),
        Entry("motionandtransitions", GalleryNavigationGroup.Motion, FluentIconRegular.SlideTransition24, "FWConnectedAnimationService FWTransitioningContentControl FWContentTransitionRecipe connected animation shared element motion content transition continuity navigation suppress entrance drill in slide crossfade liquid morph profile timing", IsUpdated: true, SourcePath: "/Motion/FWConnectedAnimationService", BaseClasses: ["TransitioningContentControl", "Service", "DependencyObject"], ApiNamespace: "FluentJalium.Controls", RelatedControls: ["FWConnectedAnimationService", "FWConnectedAnimationOptions", "FWTransitioningContentControl", "FWContentTransitionRecipe"], SampleCodeKey: "motion.transitions"),
        Entry("animatedcontrols", GalleryNavigationGroup.Motion, FluentIconRegular.Sparkle24, "FWAnimatedIcon FWAnimatedVisualPlayer animated icon visual player lottie playback motion controls first wave", GalleryPageStatus.Preview, IsUpdated: true, SourcePath: "/Motion/FWAnimatedIcon", BaseClasses: ["Control", "FrameworkElement"], ApiNamespace: "FluentJalium.Controls", RelatedControls: ["FWAnimatedIcon", "FWAnimatedVisualPlayer", "FWConnectedAnimationService"], SampleCodeKey: "animatedcontrols.motion"),
        Entry("menus", GalleryNavigationGroup.AppStructure, FluentIconRegular.List24, "FWMenuBar FWMenu FWContextMenu FWFlyout FWFlyoutPresenter FWMenuFlyout FWMenuFlyoutItem FWToggleMenuFlyoutItem FWMenuFlyoutSubItem FWMenuFlyoutSeparator FWCommandBarFlyout command menu content flyout presenter submenu shortcut primary secondary commandbar material liquid glass workbench", IsUpdated: true, SourcePath: "/Menus/FWFlyout", BaseClasses: ["Menu", "MenuItem", "ContextMenu", "FlyoutBase", "ContentControl"], ApiNamespace: "FluentJalium.Controls", RelatedControls: ["FWFlyout", "FWFlyoutPresenter", "FWMenuBar", "FWMenu", "FWContextMenu", "FWMenuFlyout", "FWMenuFlyoutItem", "FWMenuFlyoutSubItem", "FWCommandBarFlyout"], SampleCodeKey: "menus.flyout.commandbar"),
        Entry("disclosure", GalleryNavigationGroup.AppStructure, FluentIconRegular.PanelLeft24, "FWExpander FWSettingsExpander FWToolTip FWTeachingTip FWContentDialog FWTaskDialog FWTaskDialogHost FWTaskDialogAutomationDiagnostics FWTaskDialogButtonAutomationMetadata FWTaskDialogAutomationPeer FWTaskDialogHostAutomationPeer FWTaskDialogHostDiagnostics FWTaskDialogHostKeyboardRequest FWGroupBox dialog tooltip teaching tip teachingtip expander task dialog modal host overlay ShowAsync awaitable result default button cancel button automation diagnostics automation peer automation id AutomationId Name HelpText button automation metadata light dismiss focus restore focus trap keyboard diagnostics LastKeyboardRequest LastFocusTarget TabForward TabBackward closing closed PrimaryButtonCommand SecondaryButtonCommand CloseButtonCommand CommandExecuted CanExecute Escape focus visibility enabled target placement tail hero action settings group box disclosure panel SettingsExpander settings row rows item items host ItemsHost ItemCount ItemsChanged AddSetting ClearSettings ContentProperty collection grouped nested settings card SettingsCard WinUI Fluent CommunityToolkit WPFUI FluentAvalonia material liquid glass", IsUpdated: true, SourcePath: "/Disclosure/FWTaskDialog", BaseClasses: ["ContentControl", "Expander"], ApiNamespace: "FluentJalium.Controls", RelatedControls: ["FWTaskDialog", "FWTaskDialogHost", "FWTaskDialogAutomationDiagnostics", "FWTaskDialogButtonAutomationMetadata", "FWTaskDialogAutomationPeer", "FWTaskDialogHostAutomationPeer", "FWTaskDialogHostDiagnostics", "FWSettingsExpander", "FWSettingsCard", "FWTeachingTip", "FWExpander", "FWGroupBox", "FWContentDialog", "FWToolTip"], SampleCodeKey: "disclosure.settings.teachingtip"),
        Entry("advancedinteraction", GalleryNavigationGroup.LayoutAndMedia, FluentIconRegular.Filter24, "FWRefreshContainer FWRefreshContainerDiagnostics FWScroller FWScrollerViewportDiagnostics FWAnnotatedScrollBar FWAnnotatedScrollBarDiagnostics pull refresh request deferral progress visualizer diagnostics scroller snap annotated annotation label detail scrollbar advanced interaction viewport offset extent scroll viewer", GalleryPageStatus.Preview, IsUpdated: true, SourcePath: "/Interaction/FWScroller", BaseClasses: ["ContentControl", "ScrollBar"], ApiNamespace: "FluentJalium.Controls", RelatedControls: ["FWRefreshContainer", "FWRefreshContainerDiagnostics", "FWScroller", "FWScrollerViewportDiagnostics", "FWAnnotatedScrollBar", "FWAnnotatedScrollBarDiagnostics", "FWScrollViewer"], SampleCodeKey: "advancedinteraction.scroller"),
        Entry("status", GalleryNavigationGroup.AppStructure, FluentIconRegular.AlertBadge24, "FWInfoBar FWInfoBadge FWSnackbar FWSnackbarHost FWSnackbarOverlayHost FWSnackbarService FWSnackbarCloseReason FWSnackbarClosingEventArgs FWSnackbarTransitionRequestedEventArgs FWSnackbarHostDiagnostics FWToastNotificationHost FWToastNotificationItem FWStatusBar FWStatusBarItem notification message severity snackbar host presenter service queue max visible pending current action command ActionCommand auto dismiss pause hover focus Closing cancel RequestClose ShowAsync ShowForResultAsync EnqueueForResultAsync WaitForCloseAsync LastCloseReason close reason result task Placement Spacing VerticalContentAlignment top bottom alignment OverlayTarget OverlayPlacement IsOverlayOpen IsOverlayAutoOpenEnabled popup root host overlay TransitionProfile SnackbarTransitionDuration TransitionOffset TransitionRequested QueueChanged diagnostics status bar item material liquid glass operations WPFUI", IsUpdated: true, SourcePath: "/Status/FWSnackbar", BaseClasses: ["ContentControl", "Control"], ApiNamespace: "FluentJalium.Controls", RelatedControls: ["FWSnackbar", "FWSnackbarHost", "FWSnackbarOverlayHost", "FWSnackbarService", "FWInfoBar", "FWInfoBadge", "FWToastNotificationHost", "FWStatusBar"], SampleCodeKey: "status.snackbar"),
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
        bool IsFooter = false,
        bool IsNew = false,
        bool IsUpdated = false,
        string? SourcePath = null,
        string[]? BaseClasses = null,
        string? ApiNamespace = null,
        string[]? RelatedControls = null,
        string? SampleCodeKey = null)
    {
        return new GalleryCatalogEntry(
            uniqueId,
            groupId,
            icon,
            keywords,
            status,
            IsFooter,
            IsNew,
            IsUpdated,
            SourcePath,
            BaseClasses,
            ApiNamespace,
            RelatedControls,
            SampleCodeKey);
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
            group,
            entry.Icon,
            tags,
            entry.RelatedControls ?? CreateRelatedControls(tags),
            CreateDocumentationLinks(entry.UniqueId, title, tags, localization),
            entry.Status,
            entry.IsFooter,
            entry.IsNew,
            entry.IsUpdated,
            entry.SourcePath,
            entry.BaseClasses,
            entry.ApiNamespace,
            entry.SampleCodeKey);

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
