using FluentJalium.Icon;
using Jalium.UI;

namespace FluentJalium.Gallery.Models;

internal sealed record GalleryPageContentFactories(
    Func<UIElement> Overview,
    Func<UIElement> ThemeArchitecture,
    Func<UIElement> Colors,
    Func<UIElement> Typography,
    Func<UIElement> Geometry,
    Func<UIElement> MotionTokens,
    Func<UIElement> Buttons,
    Func<UIElement> Switches,
    Func<UIElement> TextInput,
    Func<UIElement> Selection,
    Func<UIElement> Range,
    Func<UIElement> DateAndTime,
    Func<UIElement> ContentAndLayout,
    Func<UIElement> Visuals,
    Func<UIElement> Interaction,
    Func<UIElement> InputAndMedia,
    Func<UIElement> Collections,
    Func<UIElement> SelectorsAndProperties,
    Func<UIElement> DataInspectors,
    Func<UIElement> Navigation,
    Func<UIElement> WindowBackdrops,
    Func<UIElement> MaterialsAndEffects,
    Func<UIElement> MotionAndTransitions,
    Func<UIElement> Menus,
    Func<UIElement> Disclosure,
    Func<UIElement> Status,
    Func<UIElement> StateMatrix);

internal static class GalleryCatalog
{
    public static GalleryPage[] Create(GalleryPageContentFactories content)
    {
        return
        [
            new GalleryPage("Overview", "Theme, typography, and accent controls for validating FluentJalium across variants.", GalleryNavigationGroup.Home, FluentIconRegular.Home24, content.Overview, "home design system theme typography accent light dark high contrast"),
            new GalleryPage("Theme Architecture", "How FluentJalium splits stable theme entry points, design resources, control dictionaries, and FW control surfaces.", GalleryNavigationGroup.Design, FluentIconRegular.Diagram24, content.ThemeArchitecture, "Generic.jalxaml FluentResources.jalxaml FluentControls.jalxaml FluentThemeManager theme resources controls dictionary FW control architecture FluentAvalonia WinUI WPFUI gallery design"),
            new GalleryPage("Colors", "Accent, text, fill, and semantic color tokens for FluentJalium themes and FW controls.", GalleryNavigationGroup.Design, FluentIconRegular.Color24, content.Colors, "FluentColors AccentBrush AccentFillColor TextPrimary TextSecondary ControlFillColor LayerFillColor SelectionBackground HyperlinkForeground ProgressBarForeground semantic color token design"),
            new GalleryPage("Typography", "Font families, type ramp, and control text roles used by FluentJalium themes.", GalleryNavigationGroup.Design, FluentIconRegular.TextFont24, content.Typography, "FluentTypography DisplayFontFamily BodyFontFamily MonoFontFamily FluentCaptionFontSize FluentBodyFontSize FluentSubtitleFontSize FluentTitleFontSize ControlContentThemeFontSize typography type ramp font design"),
            new GalleryPage("Geometry", "Corner radius, stroke, and elevation tokens for FluentJalium control surfaces.", GalleryNavigationGroup.Design, FluentIconRegular.Ruler24, content.Geometry, "ControlCornerRadius OverlayCornerRadius CardCornerRadius CompactCornerRadius FluentControlBorderThickness ControlElevationBorderBrush AccentControlElevationBorderBrush FluentGeometry radius corner stroke border elevation shadow WinUI geometry design token"),
            new GalleryPage("Motion Tokens", "Duration, connected animation, and transition role tokens that keep FluentJalium motion aligned with WinUI pacing.", GalleryNavigationGroup.Design, FluentIconRegular.SlideTransition24, content.MotionTokens, "FluentMotionDurationFast FluentMotionDurationNormal FluentMotionDurationEmphasized FluentMotionConnectedAnimationDuration FluentMotionConnectedAnimationInitialOpacity FWConnectedAnimationConfiguration TransitionMode motion token connected animation design"),
            new GalleryPage("Buttons", "Button and command surfaces, including split, drop-down, app bar, toolbar, and material command decks.", GalleryNavigationGroup.ControlSurfaces, FluentIconRegular.ControlButton24, content.Buttons, "FWButton FWRepeatButton FWHyperlinkButton FWDropDownButton FWSplitButton FWToggleSplitButton FWAppBarButton FWAppBarToggleButton FWAppBarSeparator FWCommandBar FWToolBar FWToolBarTray command bar toolbar material liquid glass split drop down"),
            new GalleryPage("Switches", "ToggleButton and ToggleSwitch states, events, keyboard toggles, and material-aware setting rows.", GalleryNavigationGroup.ControlSurfaces, FluentIconRegular.ToggleMultiple24, content.Switches, "FWToggleButton FWToggleSwitch checked unchecked indeterminate disabled toggled keyboard drag material settings"),
            new GalleryPage("Text Input", "TextBox, PasswordBox, NumberBox, AutoCompleteBox, and RichTextBox surfaces with states, filtering, and material input panels.", GalleryNavigationGroup.Input, FluentIconRegular.Textbox24, content.TextInput, "FWTextBox FWPasswordBox FWNumberBox FWAutoCompleteBox FWRichTextBox search form input password reveal number step autocomplete suggestions rich text material liquid glass"),
            new GalleryPage("Selection", "CheckBox, RadioButton, ComboBox, and ComboBoxItem controls with Select all, named groups, editable input, and material settings.", GalleryNavigationGroup.Input, FluentIconRegular.CheckboxChecked24, content.Selection, "FWCheckBox FWRadioButton FWComboBox FWComboBoxItem pick choose select all three state radio group editable material"),
            new GalleryPage("Range", "Slider, RangeSlider, ProgressBar, and ProgressRing controls with live values, snapped ranges, and material progress states.", GalleryNavigationGroup.Input, FluentIconRegular.Gauge24, content.Range, "FWSlider FWRangeSlider FWProgressBar FWProgressRing value loading progress ring range snap tick vertical material"),
            new GalleryPage("Date and Time", "DatePicker, TimePicker, and Calendar controls with bounds, clock formats, blackout dates, and material planning surfaces.", GalleryNavigationGroup.Input, FluentIconRegular.CalendarLtr24, content.DateAndTime, "FWDatePicker FWTimePicker FWCalendar schedule calendar date time appointment clock 12 hour 24 hour minute increment blackout bounds material liquid glass planning"),
            new GalleryPage("Content and Layout", "TextBlock, AccessText, Border, content hosts, panels, Grid, transitioning content, and LiquidGlass layout surfaces.", GalleryNavigationGroup.LayoutAndMedia, FluentIconRegular.LayoutColumnTwo24, content.ContentAndLayout, "FWTextBlock FWAccessText FWBorder FWContentControl FWContentPresenter FWStackPanel FWWrapPanel FWGrid FWTransitioningContentControl layout content host material liquid glass"),
            new GalleryPage("Visuals", "Fluent icon library, image stretch and zoom, label targets, separators, Viewbox scaling, and LiquidGlass visual surfaces.", GalleryNavigationGroup.LayoutAndMedia, FluentIconRegular.Image24, content.Visuals, "FWImage FWFontIcon FWSymbolIcon FWPathIcon FWLabel FWSeparator FWViewbox FluentIcon icon library regular filled visual image stretch zoom label separator viewbox material liquid glass"),
            new GalleryPage("Interaction", "ScrollViewer, SwipeControl, and GridSplitter controls with offset commands, swipe actions, keyboard increments, and LiquidGlass interaction surfaces.", GalleryNavigationGroup.LayoutAndMedia, FluentIconRegular.CursorClick24, content.Interaction, "FWScrollViewer FWSwipeControl FWGridSplitter scrolling scroll viewer auto hide offset swipe reveal execute archive delete grid splitter resize keyboard drag increment material liquid glass interaction"),
            new GalleryPage("Input and Media", "ColorPicker, InkCanvas, InkPresenter, and MediaElement controls with alpha, hex, ink modes, stroke presentation, playback surfaces, and LiquidGlass media workbenches.", GalleryNavigationGroup.LayoutAndMedia, FluentIconRegular.Color24, content.InputAndMedia, "FWColorPicker FWInkCanvas FWInkPresenter FWMediaElement color picker alpha hex compact spectrum ink canvas draw erase taper stroke presenter media element play pause stop mute stretch playback material liquid glass"),
            new GalleryPage("Collections", "ListBox, ListView, TreeView, DataGrid, and TreeDataGrid controls with selection, hierarchy, table options, and material data surfaces.", GalleryNavigationGroup.CollectionsAndData, FluentIconRegular.Table24, content.Collections, "FWListBox FWListView FWTreeView FWDataGrid FWTreeDataGrid table list data grid hierarchy selection material liquid glass row height headers"),
            new GalleryPage("Selectors and Properties", "TreeSelector and PropertyGrid surfaces for hierarchical selection, object editing, cascade checks, and material property panels.", GalleryNavigationGroup.CollectionsAndData, FluentIconRegular.DatabaseSearch24, content.SelectorsAndProperties, "FWTreeSelector FWTreeSelectorItem FWPropertyGrid tree selector property grid search categorized alphabetical cascade checkbox material liquid glass editor"),
            new GalleryPage("Data Inspectors", "DiffViewer, HexEditor, and JsonTreeViewer developer surfaces with diff, binary, JSON, and material inspection workbench states.", GalleryNavigationGroup.CollectionsAndData, FluentIconRegular.Code24, content.DataInspectors, "FWDiffViewer FWHexEditor FWJsonTreeViewer diff hex json code data inspector binary material liquid glass minimap ascii"),
            new GalleryPage("Navigation", "NavigationView, pane modes, hierarchy, tabs, Frame navigation, and material app shell states.", GalleryNavigationGroup.AppStructure, FluentIconRegular.Navigation24, content.Navigation, "FWNavigationView FWNavigationViewItem FWNavigationViewItemHeader FWNavigationViewItemSeparator pane display mode LeftCompact LeftMinimal Top hierarchy FWTabControl FWTabItem FWFrame page shell back forward material liquid glass app shell"),
            new GalleryPage("Window Backdrops", "DWM-backed shell materials for Jalium windows, including Mica, Mica Alt, Acrylic, and solid shell fallback.", GalleryNavigationGroup.Materials, FluentIconRegular.WindowBrush24, content.WindowBackdrops, "FWFluentWindowBackdropKind FWFluentWindowBackdropRecipe WindowBackdropType SystemBackdrop DWM Mica MicaAlt Acrylic solid shell window backdrop material"),
            new GalleryPage("Materials and Effects", "Element backdrops, material roles, and WinUI-style layering for FluentJalium surfaces.", GalleryNavigationGroup.Materials, FluentIconRegular.TransparencySquare24, content.MaterialsAndEffects, "FWFluentMaterialSurface FWFluentMaterialKind FWFluentMaterialRecipe BackdropEffect BlurEffect AcrylicEffect MicaEffect FrostedGlassEffect DropShadowEffect material role layer transient focused reveal HLSL shader liquid glass recipe preset"),
            new GalleryPage("Motion and Transitions", "Connected animation, shared element motion, and FW content transitions for FluentJalium navigation continuity.", GalleryNavigationGroup.Motion, FluentIconRegular.SlideTransition24, content.MotionAndTransitions, "FWConnectedAnimationService FWTransitioningContentControl connected animation shared element motion content transition continuity navigation suppress entrance drill in slide crossfade liquid morph"),
            new GalleryPage("Menus", "MenuBar, Menu, ContextMenu, MenuFlyout, and CommandBarFlyout surfaces with submenu, shortcut, and LiquidGlass workbench states.", GalleryNavigationGroup.AppStructure, FluentIconRegular.List24, content.Menus, "FWMenuBar FWMenu FWContextMenu FWMenuFlyout FWMenuFlyoutItem FWToggleMenuFlyoutItem FWMenuFlyoutSubItem FWMenuFlyoutSeparator FWCommandBarFlyout command menu flyout submenu shortcut material liquid glass workbench"),
            new GalleryPage("Disclosure", "Expander, ToolTip, ContentDialog, and GroupBox controls with command states and LiquidGlass disclosure panels.", GalleryNavigationGroup.AppStructure, FluentIconRegular.PanelLeft24, content.Disclosure, "FWExpander FWToolTip FWContentDialog FWGroupBox dialog tooltip expander group box disclosure material liquid glass panel"),
            new GalleryPage("Status", "InfoBar, InfoBadge, ToastNotification, and StatusBar controls with severity, queue, and material operation states.", GalleryNavigationGroup.AppStructure, FluentIconRegular.AlertBadge24, content.Status, "FWInfoBar FWInfoBadge FWToastNotificationHost FWToastNotificationItem FWStatusBar FWStatusBarItem notification message severity toast queue max visible status bar item material liquid glass operations"),
            new GalleryPage("State Matrix", "Cross-control normal, selected, disabled, and flyout state checks.", GalleryNavigationGroup.Diagnostics, FluentIconRegular.DataUsage24, content.StateMatrix, "states normal hover pressed selected disabled light dark high contrast", IsFooter: true)
        ];
    }
}
