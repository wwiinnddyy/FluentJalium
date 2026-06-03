using FluentJalium.Icon;
using Jalium.UI;

namespace FluentJalium.Gallery.Models;

internal sealed record GalleryPageContentFactories(
    Func<UIElement> Overview,
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
            new GalleryPage("Buttons", "Button and command surfaces, including split, drop-down, and app bar buttons.", GalleryNavigationGroup.ControlSurfaces, FluentIconRegular.ControlButton24, content.Buttons, "FWButton FWRepeatButton FWHyperlinkButton FWDropDownButton FWSplitButton FWToggleSplitButton FWAppBarButton FWAppBarToggleButton FWAppBarSeparator command bar"),
            new GalleryPage("Switches", "ToggleButton and ToggleSwitch states, events, keyboard toggles, and material-aware setting rows.", GalleryNavigationGroup.ControlSurfaces, FluentIconRegular.ToggleMultiple24, content.Switches, "FWToggleButton FWToggleSwitch checked unchecked indeterminate disabled toggled keyboard drag material settings"),
            new GalleryPage("Text Input", "TextBox, PasswordBox, NumberBox, AutoCompleteBox, and RichTextBox surfaces.", GalleryNavigationGroup.Input, FluentIconRegular.Textbox24, content.TextInput, "FWTextBox FWPasswordBox FWNumberBox FWAutoCompleteBox FWRichTextBox search form input"),
            new GalleryPage("Selection", "CheckBox, RadioButton, ComboBox, and ComboBoxItem controls with Select all, named groups, editable input, and material settings.", GalleryNavigationGroup.Input, FluentIconRegular.CheckboxChecked24, content.Selection, "FWCheckBox FWRadioButton FWComboBox FWComboBoxItem pick choose select all three state radio group editable material"),
            new GalleryPage("Range", "Slider, RangeSlider, ProgressBar, and ProgressRing controls with live values, snapped ranges, and material progress states.", GalleryNavigationGroup.Input, FluentIconRegular.Gauge24, content.Range, "FWSlider FWRangeSlider FWProgressBar FWProgressRing value loading progress ring range snap tick vertical material"),
            new GalleryPage("Date and Time", "DatePicker, TimePicker, and Calendar controls with bounds, clock formats, blackout dates, and material planning surfaces.", GalleryNavigationGroup.Input, FluentIconRegular.CalendarLtr24, content.DateAndTime, "FWDatePicker FWTimePicker FWCalendar schedule calendar date time appointment clock 12 hour 24 hour minute increment blackout bounds material liquid glass planning"),
            new GalleryPage("Content and Layout", "TextBlock, AccessText, Border, content hosts, StackPanel, WrapPanel, and Grid foundations.", GalleryNavigationGroup.LayoutAndMedia, FluentIconRegular.LayoutColumnTwo24, content.ContentAndLayout, "FWTextBlock FWAccessText FWBorder FWContentControl FWContentPresenter FWStackPanel FWWrapPanel FWGrid layout"),
            new GalleryPage("Visuals", "Image, icon, label, separator, and Viewbox foundation controls.", GalleryNavigationGroup.LayoutAndMedia, FluentIconRegular.Image24, content.Visuals, "FWImage FWFontIcon FWSymbolIcon FWPathIcon FWLabel FWSeparator FWViewbox visual icon"),
            new GalleryPage("Interaction", "ScrollViewer, SwipeControl, and GridSplitter controls.", GalleryNavigationGroup.LayoutAndMedia, FluentIconRegular.CursorClick24, content.Interaction, "FWScrollViewer FWSwipeControl FWGridSplitter scrolling resize"),
            new GalleryPage("Input and Media", "ColorPicker, InkCanvas, InkPresenter, and MediaElement surfaces.", GalleryNavigationGroup.LayoutAndMedia, FluentIconRegular.Color24, content.InputAndMedia, "FWColorPicker FWInkCanvas FWInkPresenter FWMediaElement color ink media"),
            new GalleryPage("Collections", "ListBox, ListView, TreeView, DataGrid, and TreeDataGrid controls with selection, hierarchy, table options, and material data surfaces.", GalleryNavigationGroup.CollectionsAndData, FluentIconRegular.Table24, content.Collections, "FWListBox FWListView FWTreeView FWDataGrid FWTreeDataGrid table list data grid hierarchy selection material liquid glass row height headers"),
            new GalleryPage("Selectors and Properties", "TreeSelector and PropertyGrid surfaces for hierarchical selection, object editing, cascade checks, and material property panels.", GalleryNavigationGroup.CollectionsAndData, FluentIconRegular.DatabaseSearch24, content.SelectorsAndProperties, "FWTreeSelector FWTreeSelectorItem FWPropertyGrid tree selector property grid search categorized alphabetical cascade checkbox material liquid glass editor"),
            new GalleryPage("Data Inspectors", "DiffViewer, HexEditor, and JsonTreeViewer developer surfaces with diff, binary, JSON, and material inspection workbench states.", GalleryNavigationGroup.CollectionsAndData, FluentIconRegular.Code24, content.DataInspectors, "FWDiffViewer FWHexEditor FWJsonTreeViewer diff hex json code data inspector binary material liquid glass minimap ascii"),
            new GalleryPage("Navigation", "NavigationView, pane modes, hierarchy, tabs, Frame navigation, and material app shell states.", GalleryNavigationGroup.AppStructure, FluentIconRegular.Navigation24, content.Navigation, "FWNavigationView FWNavigationViewItem FWNavigationViewItemHeader FWNavigationViewItemSeparator pane display mode LeftCompact LeftMinimal Top hierarchy FWTabControl FWTabItem FWFrame page shell back forward material liquid glass app shell"),
            new GalleryPage("Materials and Effects", "Window backdrops, backdrop effects, and WinUI-style material layering for FluentJalium surfaces.", GalleryNavigationGroup.MaterialsAndMotion, FluentIconRegular.TransparencySquare24, content.MaterialsAndEffects, "WindowBackdropType SystemBackdrop Mica MicaAlt Acrylic BackdropEffect BlurEffect AcrylicEffect MicaEffect FrostedGlassEffect DropShadowEffect material layer reveal HLSL shader liquid glass"),
            new GalleryPage("Motion and Transitions", "Connected animation, shared element motion, and FW content transitions for FluentJalium navigation continuity.", GalleryNavigationGroup.MaterialsAndMotion, FluentIconRegular.SlideTransition24, content.MotionAndTransitions, "FWConnectedAnimationService FWTransitioningContentControl connected animation shared element motion content transition continuity navigation suppress entrance drill in slide crossfade liquid morph"),
            new GalleryPage("Menus", "MenuBar, Menu, ContextMenu, MenuFlyout, and CommandBarFlyout surfaces.", GalleryNavigationGroup.AppStructure, FluentIconRegular.List24, content.Menus, "FWMenuBar FWMenu FWContextMenu FWMenuFlyout FWMenuFlyoutItem FWToggleMenuFlyoutItem FWMenuFlyoutSubItem FWMenuFlyoutSeparator FWCommandBarFlyout command menu"),
            new GalleryPage("Disclosure", "Expander, ToolTip, ContentDialog, and GroupBox controls.", GalleryNavigationGroup.AppStructure, FluentIconRegular.PanelLeft24, content.Disclosure, "FWExpander FWToolTip FWContentDialog FWGroupBox dialog flyout disclosure"),
            new GalleryPage("Status", "InfoBar, InfoBadge, ToastNotification, and StatusBar controls with severity, queue, and material operation states.", GalleryNavigationGroup.AppStructure, FluentIconRegular.AlertBadge24, content.Status, "FWInfoBar FWInfoBadge FWToastNotificationHost FWToastNotificationItem FWStatusBar FWStatusBarItem notification message severity toast queue max visible status bar item material liquid glass operations"),
            new GalleryPage("State Matrix", "Cross-control normal, selected, disabled, and flyout state checks.", GalleryNavigationGroup.Diagnostics, FluentIconRegular.DataUsage24, content.StateMatrix, "states normal hover pressed selected disabled light dark high contrast", IsFooter: true)
        ];
    }
}
