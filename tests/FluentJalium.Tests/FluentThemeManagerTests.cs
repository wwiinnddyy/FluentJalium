using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Windows.Input;
using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Ink;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Data;
using Jalium.UI.Input;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using JaliumThemeManager = Jalium.UI.Controls.Themes.ThemeManager;

namespace FluentJalium.Tests;

[Collection("Application")]
public sealed class FluentThemeManagerTests
{
    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void Apply_ShouldInsertFluentDictionariesOnlyOnce()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            var baseCount = app.Resources.MergedDictionaries.Count;

            FluentThemeManager.Apply(app);
            var firstCount = app.Resources.MergedDictionaries.Count;

            FluentThemeManager.Apply(app);
            var secondCount = app.Resources.MergedDictionaries.Count;

            Assert.Equal(baseCount + 3, firstCount);
            Assert.Equal(firstCount, secondCount);
            Assert.True(app.Resources.TryGetValue(typeof(Button), out var style));
            Assert.IsType<Style>(style);
            AssertBasedOnStyle<FWButton, Button>(app.Resources);
            AssertBasedOnStyle<FWRepeatButton, RepeatButton>(app.Resources);
            AssertBasedOnStyle<FWHyperlinkButton, HyperlinkButton>(app.Resources);
            AssertBasedOnStyle<FWTextBox, TextBox>(app.Resources);
            AssertBasedOnStyle<FWPasswordBox, PasswordBox>(app.Resources);
            AssertBasedOnStyle<FWNumberBox, NumberBox>(app.Resources);
            AssertBasedOnStyle<FWAutoCompleteBox, AutoCompleteBox>(app.Resources);
            AssertBasedOnStyle<FWRichTextBox, RichTextBox>(app.Resources);
            AssertBasedOnStyle<FWCheckBox, CheckBox>(app.Resources);
            AssertBasedOnStyle<FWRadioButton, RadioButton>(app.Resources);
            AssertBasedOnStyle<FWToggleButton, ToggleButton>(app.Resources);
            AssertBasedOnStyle<FWToggleSwitch, ToggleSwitch>(app.Resources);
            AssertBasedOnStyle<FWSlider, Slider>(app.Resources);
            AssertBasedOnStyle<FWRangeSlider, RangeSlider>(app.Resources);
            AssertBasedOnStyle<FWProgressBar, ProgressBar>(app.Resources);
            AssertBasedOnStyle<FWComboBox, ComboBox>(app.Resources);
            AssertBasedOnStyle<FWComboBoxItem, ComboBoxItem>(app.Resources);
            AssertOwnedStyle<FWRatingControl>(app.Resources);
            AssertBasedOnStyle<FWListBox, ListBox>(app.Resources);
            AssertBasedOnStyle<FWListBoxItem, ListBoxItem>(app.Resources);
            AssertBasedOnStyle<FWListView, ListView>(app.Resources);
            AssertBasedOnStyle<FWListViewItem, ListViewItem>(app.Resources);
            AssertBasedOnStyle<FWTreeView, TreeView>(app.Resources);
            AssertBasedOnStyle<FWTreeViewItem, TreeViewItem>(app.Resources);
            AssertBasedOnStyle<FWDataGrid, DataGrid>(app.Resources);
            AssertBasedOnStyle<FWTreeDataGrid, TreeDataGrid>(app.Resources);
            AssertBasedOnStyle<FWTreeSelector, TreeSelector>(app.Resources);
            AssertBasedOnStyle<FWTreeSelectorItem, TreeSelectorItem>(app.Resources);
            AssertBasedOnStyle<FWPropertyGrid, PropertyGrid>(app.Resources);
            AssertBasedOnStyle<FWDiffViewer, DiffViewer>(app.Resources);
            AssertBasedOnStyle<FWHexEditor, HexEditor>(app.Resources);
            AssertBasedOnStyle<FWJsonTreeViewer, JsonTreeViewer>(app.Resources);
            AssertBasedOnStyle<FWNavigationView, NavigationView>(app.Resources);
            AssertBasedOnStyle<FWNavigationViewItem, NavigationViewItem>(app.Resources);
            AssertBasedOnStyle<FWNavigationViewItemHeader, NavigationViewItemHeader>(app.Resources);
            AssertBasedOnStyle<FWNavigationViewItemSeparator, NavigationViewItemSeparator>(app.Resources);
            AssertBasedOnStyle<FWTabControl, TabControl>(app.Resources);
            AssertBasedOnStyle<FWTabItem, TabItem>(app.Resources);
            AssertBasedOnStyle<FWFrame, Frame>(app.Resources);
            AssertBasedOnStyle<FWExpander, Expander>(app.Resources);
            AssertBasedOnStyle<FWToolTip, ToolTip>(app.Resources);
            AssertBasedOnStyle<FWContentDialog, ContentDialog>(app.Resources);
            AssertBasedOnStyle<FWGroupBox, GroupBox>(app.Resources);
            AssertBasedOnStyle<FWScrollBar, ScrollBar>(app.Resources);
            AssertBasedOnStyle<FWScrollViewer, ScrollViewer>(app.Resources);
            AssertBasedOnStyle<FWSwipeControl, SwipeControl>(app.Resources);
            AssertBasedOnStyle<FWGridSplitter, GridSplitter>(app.Resources);
            AssertBasedOnStyle<FWTextBlock, TextBlock>(app.Resources);
            AssertBasedOnStyle<FWAccessText, AccessText>(app.Resources);
            AssertBasedOnStyle<FWBorder, Border>(app.Resources);
            AssertBasedOnStyle<FWFluentMaterialSurface, Border>(app.Resources);
            AssertBasedOnStyle<FWFluentWindowSurface, Border>(app.Resources);
            AssertBasedOnStyle<FWContentControl, ContentControl>(app.Resources);
            AssertBasedOnStyle<FWTransitioningContentControl, TransitioningContentControl>(app.Resources);
            AssertBasedOnStyle<FWContentPresenter, ContentPresenter>(app.Resources);
            AssertBasedOnStyle<FWStackPanel, StackPanel>(app.Resources);
            AssertBasedOnStyle<FWWrapPanel, WrapPanel>(app.Resources);
            AssertBasedOnStyle<FWGrid, Grid>(app.Resources);
            AssertBasedOnStyle<FWColorPicker, ColorPicker>(app.Resources);
            AssertOwnedStyle<FWInkCanvas>(app.Resources);
            AssertBasedOnStyle<FWInkPresenter, InkPresenter>(app.Resources);
            AssertOwnedStyle<FWMediaElement>(app.Resources);
            AssertBasedOnStyle<FWImage, Image>(app.Resources);
            AssertBasedOnStyle<FWFontIcon, FontIcon>(app.Resources);
            AssertBasedOnStyle<FWSymbolIcon, SymbolIcon>(app.Resources);
            AssertBasedOnStyle<FWPathIcon, PathIcon>(app.Resources);
            AssertBasedOnStyle<FWViewbox, Viewbox>(app.Resources);
            AssertBasedOnStyle<FWLabel, Label>(app.Resources);
            AssertBasedOnStyle<FWSeparator, Separator>(app.Resources);
            AssertBasedOnStyle<FWMenuBar, MenuBar>(app.Resources);
            AssertBasedOnStyle<FWMenuBarItem, MenuBarItem>(app.Resources);
            AssertBasedOnStyle<FWMenu, Menu>(app.Resources);
            AssertBasedOnStyle<FWMenuItem, MenuItem>(app.Resources);
            AssertBasedOnStyle<FWContextMenu, ContextMenu>(app.Resources);
            AssertBasedOnStyle<FWMenuFlyoutItem, MenuFlyoutItem>(app.Resources);
            AssertBasedOnStyle<FWToggleMenuFlyoutItem, ToggleMenuFlyoutItem>(app.Resources);
            AssertBasedOnStyle<FWMenuFlyoutSeparator, MenuFlyoutSeparator>(app.Resources);
            AssertBasedOnStyle<FWDatePicker, DatePicker>(app.Resources);
            AssertBasedOnStyle<FWTimePicker, TimePicker>(app.Resources);
            AssertBasedOnStyle<FWCalendar, Calendar>(app.Resources);
            AssertBasedOnStyle<FWInfoBar, InfoBar>(app.Resources);
            AssertBasedOnStyle<FWToastNotificationItem, ToastNotificationItem>(app.Resources);
            AssertBasedOnStyle<FWToastNotificationHost, ToastNotificationHost>(app.Resources);
            AssertBasedOnStyle<FWStatusBar, StatusBar>(app.Resources);
            AssertBasedOnStyle<FWStatusBarItem, Jalium.UI.Controls.StatusBarItem>(app.Resources);
            AssertOwnedStyle<FWInfoBadge>(app.Resources);
            AssertOwnedStyle<FWProgressRing>(app.Resources);
            AssertOwnedStyle<FWDropDownButton>(app.Resources);
            AssertBasedOnStyle<FWSplitButton, SplitButton>(app.Resources);
            AssertOwnedStyle<FWToggleSplitButton>(app.Resources);
            AssertBasedOnStyle<FWCommandBar, CommandBar>(app.Resources);
            AssertBasedOnStyle<FWAppBarButton, AppBarButton>(app.Resources);
            AssertBasedOnStyle<FWAppBarToggleButton, AppBarToggleButton>(app.Resources);
            AssertBasedOnStyle<FWAppBarSeparator, AppBarSeparator>(app.Resources);
            AssertBasedOnStyle<FWToolBar, Jalium.UI.Controls.ToolBar>(app.Resources);
            AssertBasedOnStyle<FWToolBarTray, ToolBarTray>(app.Resources);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void ApplyTheme_ShouldUpdateCurrentThemeKeyAndThemeResources()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);
            var darkWindowBackground = GetBrushColor(app.Resources["WindowBackground"]);

            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            var lightWindowBackground = GetBrushColor(app.Resources["WindowBackground"]);

            Assert.Equal(FluentThemeVariant.Light, FluentThemeManager.CurrentTheme);
            Assert.Equal("Light", ResourceDictionary.CurrentThemeKey);
            Assert.NotEqual(darkWindowBackground, lightWindowBackground);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void ApplyAccent_ShouldGenerateDefaultHoverPressedAndDisabledBrushes()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            var accent = Color.FromRgb(0xD8, 0x3B, 0x01);
            FluentThemeManager.ApplyAccent(accent);

            Assert.Equal(accent, FluentThemeManager.CurrentAccentColor);
            Assert.Equal(accent, GetBrushColor(app.Resources["AccentBrush"]));
            Assert.Equal(accent, GetBrushColor(app.Resources["FluentAccentBrush"]));
            Assert.IsType<SolidColorBrush>(app.Resources["AccentBrushHover"]);
            Assert.IsType<SolidColorBrush>(app.Resources["AccentBrushPressed"]);
            Assert.IsType<SolidColorBrush>(app.Resources["AccentBrushDisabled"]);
            Assert.IsType<SolidColorBrush>(app.Resources["FluentAccentBrushHover"]);
            Assert.IsType<SolidColorBrush>(app.Resources["FluentAccentBrushPressed"]);
            Assert.IsType<SolidColorBrush>(app.Resources["FluentAccentBrushDisabled"]);
            Assert.Equal(accent, GetBrushColor(app.Resources["TabItemIndicator"]));
            Assert.Equal(Color.FromArgb(0x33, accent.R, accent.G, accent.B), GetBrushColor(app.Resources["NavigationViewItemBackgroundSelected"]));
            Assert.Equal(Color.FromArgb(0x66, accent.R, accent.G, accent.B), GetBrushColor(app.Resources["NavigationViewItemBackgroundSelectedHover"]));
            Assert.Equal(accent, GetBrushColor(app.Resources["ToggleSwitchOnBackground"]));
            Assert.Equal(accent, GetBrushColor(app.Resources["ToggleSwitchOnBorder"]));
            Assert.Equal(accent, GetBrushColor(app.Resources["SliderTrackValueFill"]));
            Assert.Equal(accent, GetBrushColor(app.Resources["SliderThumbBackground"]));
            Assert.Equal(accent, GetBrushColor(app.Resources["ProgressBarForeground"]));
            Assert.Equal(accent, GetBrushColor(app.Resources["ProgressRingForeground"]));
            Assert.Equal(accent, GetBrushColor(app.Resources["RatingControlSelectedForeground"]));
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void GenericTheme_ShouldLoadFromPackPathAndExposeCoreControlStyles()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = FluentThemeVariant.Dark.ToString();

        var owner = new ResourceDictionary();
        var loaded = ResourceDictionary.SourceLoader?.Invoke(
            owner,
            new Uri("/FluentJalium;component/Themes/Generic.jalxaml", UriKind.Relative),
            FluentThemeManager.ThemeAssembly);

        Assert.NotNull(loaded);
        var dictionary = loaded!;

        AssertContainsStyle<Button>(dictionary);
        AssertContainsStyle<RepeatButton>(dictionary);
        AssertContainsStyle<HyperlinkButton>(dictionary);
        AssertContainsStyle<TextBox>(dictionary);
        AssertContainsStyle<PasswordBox>(dictionary);
        AssertContainsStyle<NumberBox>(dictionary);
        AssertContainsStyle<AutoCompleteBox>(dictionary);
        AssertContainsStyle<RichTextBox>(dictionary);
        AssertContainsStyle<CheckBox>(dictionary);
        AssertContainsStyle<RadioButton>(dictionary);
        AssertContainsStyle<ToggleButton>(dictionary);
        AssertContainsStyle<ToggleSwitch>(dictionary);
        AssertContainsStyle<Slider>(dictionary);
        AssertContainsStyle<RangeSlider>(dictionary);
        AssertContainsStyle<ProgressBar>(dictionary);
        AssertContainsStyle<ComboBox>(dictionary);
        AssertContainsStyle<ComboBoxItem>(dictionary);
        AssertContainsStyle<FWRatingControl>(dictionary);
        AssertContainsStyle<ListBox>(dictionary);
        AssertContainsStyle<ListBoxItem>(dictionary);
        AssertContainsStyle<ListView>(dictionary);
        AssertContainsStyle<ListViewItem>(dictionary);
        AssertContainsStyle<GridViewColumnHeader>(dictionary);
        AssertContainsStyle<TreeView>(dictionary);
        AssertContainsStyle<TreeViewItem>(dictionary);
        AssertContainsStyle<DataGrid>(dictionary);
        AssertContainsStyle<DataGridRow>(dictionary);
        AssertContainsStyle<DataGridCell>(dictionary);
        AssertContainsStyle<Jalium.UI.Controls.DataGridColumnHeader>(dictionary);
        AssertContainsStyle<TreeDataGrid>(dictionary);
        AssertContainsStyle<TreeDataGridRow>(dictionary);
        AssertContainsStyle<TreeSelector>(dictionary);
        AssertContainsStyle<TreeSelectorItem>(dictionary);
        AssertContainsStyle<PropertyGrid>(dictionary);
        AssertContainsStyle<DiffViewer>(dictionary);
        AssertContainsStyle<HexEditor>(dictionary);
        AssertContainsStyle<JsonTreeViewer>(dictionary);
        AssertContainsStyle<NavigationView>(dictionary);
        AssertContainsStyle<NavigationViewItem>(dictionary);
        AssertContainsStyle<NavigationViewItemHeader>(dictionary);
        AssertContainsStyle<NavigationViewItemSeparator>(dictionary);
        AssertContainsStyle<TabControl>(dictionary);
        AssertContainsStyle<TabItem>(dictionary);
        AssertContainsStyle<Frame>(dictionary);
        AssertContainsStyle<Expander>(dictionary);
        AssertContainsStyle<ToolTip>(dictionary);
        AssertContainsStyle<ContentDialog>(dictionary);
        AssertContainsStyle<GroupBox>(dictionary);
        AssertContainsStyle<ScrollViewer>(dictionary);
        AssertContainsStyle<ScrollBar>(dictionary);
        AssertContainsStyle<SwipeControl>(dictionary);
        AssertContainsStyle<GridSplitter>(dictionary);
        AssertContainsStyle<TextBlock>(dictionary);
        AssertContainsStyle<AccessText>(dictionary);
        AssertContainsStyle<Border>(dictionary);
        AssertContainsStyle<FWFluentWindowSurface>(dictionary);
        AssertContainsStyle<ContentControl>(dictionary);
        AssertContainsStyle<TransitioningContentControl>(dictionary);
        AssertContainsStyle<ContentPresenter>(dictionary);
        AssertContainsStyle<StackPanel>(dictionary);
        AssertContainsStyle<WrapPanel>(dictionary);
        AssertContainsStyle<Grid>(dictionary);
        AssertContainsStyle<ColorPicker>(dictionary);
        AssertContainsStyle<InkCanvas>(dictionary);
        AssertContainsStyle<FWInkCanvas>(dictionary);
        AssertContainsStyle<InkPresenter>(dictionary);
        AssertContainsStyle<MediaElement>(dictionary);
        AssertContainsStyle<FWMediaElement>(dictionary);
        AssertContainsStyle<Image>(dictionary);
        AssertContainsStyle<FontIcon>(dictionary);
        AssertContainsStyle<SymbolIcon>(dictionary);
        AssertContainsStyle<PathIcon>(dictionary);
        AssertContainsStyle<Viewbox>(dictionary);
        AssertContainsStyle<Label>(dictionary);
        AssertContainsStyle<Separator>(dictionary);
        AssertContainsStyle<MenuBar>(dictionary);
        AssertContainsStyle<MenuBarItem>(dictionary);
        AssertContainsStyle<Menu>(dictionary);
        AssertContainsStyle<MenuItem>(dictionary);
        AssertContainsStyle<ContextMenu>(dictionary);
        AssertContainsStyle<MenuFlyoutItem>(dictionary);
        AssertContainsStyle<ToggleMenuFlyoutItem>(dictionary);
        AssertContainsStyle<MenuFlyoutSeparator>(dictionary);
        AssertContainsStyle<DatePicker>(dictionary);
        AssertContainsStyle<TimePicker>(dictionary);
        AssertContainsStyle<Calendar>(dictionary);
        AssertContainsStyle<CalendarButton>(dictionary);
        AssertContainsStyle<CalendarDayButton>(dictionary);
        AssertContainsStyle<CalendarItem>(dictionary);
        AssertContainsStyle<DatePickerTextBox>(dictionary);
        AssertContainsStyle<InfoBar>(dictionary);
        AssertContainsStyle<ToastNotificationItem>(dictionary);
        AssertContainsStyle<ToastNotificationHost>(dictionary);
        AssertContainsStyle<StatusBar>(dictionary);
        AssertContainsStyle<Jalium.UI.Controls.StatusBarItem>(dictionary);
        AssertContainsStyle<FWInfoBadge>(dictionary);
        AssertContainsStyle<FWProgressRing>(dictionary);
        AssertContainsStyle<FWDropDownButton>(dictionary);
        AssertContainsStyle<SplitButton>(dictionary);
        AssertContainsStyle<FWToggleSplitButton>(dictionary);
        AssertContainsStyle<CommandBar>(dictionary);
        AssertContainsStyle<Jalium.UI.Controls.ToolBar>(dictionary);
        AssertContainsStyle<ToolBarTray>(dictionary);
        AssertContainsStyle<AppBarButton>(dictionary);
        AssertContainsStyle<AppBarToggleButton>(dictionary);
        AssertContainsStyle<AppBarSeparator>(dictionary);
        Assert.True(dictionary.Contains("TextPrimary"));
        Assert.True(dictionary.Contains("AccentBrush"));
        Assert.True(dictionary.Contains("TextControlBackground"));
        Assert.True(dictionary.Contains("TextControlBorderFocused"));
        Assert.True(dictionary.Contains("TextControlFlyoutBackground"));
        Assert.True(dictionary.Contains("ToggleCheckedBackground"));
        Assert.True(dictionary.Contains("ToggleUncheckedBackground"));
        Assert.True(dictionary.Contains("ToggleDisabledBackground"));
        Assert.True(dictionary.Contains("ToggleSwitchOnBackground"));
        Assert.True(dictionary.Contains("ToggleSwitchOffBackground"));
        Assert.True(dictionary.Contains("ToggleSwitchThumb"));
        Assert.True(dictionary.Contains("SliderTrack"));
        Assert.True(dictionary.Contains("SliderThumb"));
        Assert.True(dictionary.Contains("SliderTrackFill"));
        Assert.True(dictionary.Contains("SliderTrackValueFill"));
        Assert.True(dictionary.Contains("SliderTrackFillDisabled"));
        Assert.True(dictionary.Contains("SliderThumbBackground"));
        Assert.True(dictionary.Contains("SliderThumbBorderBrush"));
        Assert.True(dictionary.Contains("SliderTickBarFill"));
        Assert.True(dictionary.Contains("ProgressBarBackground"));
        Assert.True(dictionary.Contains("ProgressBarForeground"));
        Assert.True(dictionary.Contains("ProgressBarIndeterminateBackground"));
        Assert.True(dictionary.Contains("ProgressBarPausedForeground"));
        Assert.True(dictionary.Contains("ProgressBarErrorForeground"));
        Assert.True(dictionary.Contains("ProgressBarDisabledForeground"));
        Assert.True(dictionary.Contains("ProgressRingForeground"));
        Assert.True(dictionary.Contains("ProgressRingBackground"));
        Assert.True(dictionary.Contains("ProgressRingDisabledForeground"));
        Assert.True(dictionary.Contains("SelectionBackgroundWeak"));
        Assert.True(dictionary.Contains("RatingControlCaptionForeground"));
        Assert.True(dictionary.Contains("MenuBarBackground"));
        Assert.True(dictionary.Contains("MenuBarItemBackgroundHover"));
        Assert.True(dictionary.Contains("MenuFlyoutPresenterBackground"));
        Assert.True(dictionary.Contains("MenuFlyoutItemBackgroundHover"));
        Assert.True(dictionary.Contains("MenuFlyoutSeparatorForeground"));
        Assert.True(dictionary.Contains("DividerStrokeColorDefaultBrush"));
        Assert.True(dictionary.Contains("CommandBarBackground"));
        Assert.True(dictionary.Contains("CommandBarOverflowBackground"));
        Assert.True(dictionary.Contains("ToolBarTrayBackground"));
        Assert.True(dictionary.Contains("ToolBarBackground"));
        Assert.True(dictionary.Contains("ToolBarBorderBrush"));
        Assert.True(dictionary.Contains("ToolBarGripBrush"));
        Assert.True(dictionary.Contains("ToolBarSeparatorForeground"));
        Assert.True(dictionary.Contains("AppBarButtonBackground"));
        Assert.True(dictionary.Contains("AppBarButtonBackgroundHover"));
        Assert.True(dictionary.Contains("AppBarButtonBackgroundPressed"));
        Assert.True(dictionary.Contains("NavigationViewPaneBackground"));
        Assert.True(dictionary.Contains("NavigationViewItemBackgroundSelected"));
        Assert.True(dictionary.Contains("TabStripBackground"));
        Assert.True(dictionary.Contains("TabItemIndicator"));
        Assert.True(dictionary.Contains("FrameBackground"));
        Assert.True(dictionary.Contains("ToolTipBackground"));
        Assert.True(dictionary.Contains("ToolTipBorderBrush"));
        Assert.True(dictionary.Contains("ExpanderHeaderBackground"));
        Assert.True(dictionary.Contains("ExpanderHeaderBackgroundExpanded"));
        Assert.True(dictionary.Contains("GroupBoxBorderBrush"));
        Assert.True(dictionary.Contains("ContentDialogBackground"));
        Assert.True(dictionary.Contains("ContentDialogOverlayBackground"));
        Assert.True(dictionary.Contains("ContentDialogAccentButtonStyle"));
        Assert.True(dictionary.Contains("ScrollBarStyle"));
        Assert.True(dictionary.Contains("ScrollBarThumbStyle"));
        Assert.True(dictionary.Contains("ScrollBarTrack"));
        Assert.True(dictionary.Contains("ScrollBarThumbHover"));
        Assert.True(dictionary.Contains("ScrollBarArrowHover"));
        Assert.True(dictionary.Contains("SwipeControlBackground"));
        Assert.True(dictionary.Contains("SwipeItemBackgroundDestructive"));
        Assert.True(dictionary.Contains("GridSplitterGripBrushHover"));
        Assert.True(dictionary.Contains("ContentSurfaceBackground"));
        Assert.True(dictionary.Contains("ContentSurfaceBorderBrush"));
        Assert.True(dictionary.Contains("ContentPanelBackground"));
        Assert.True(dictionary.Contains("FluentMaterialWindowBackdropBrush"));
        Assert.True(dictionary.Contains("FluentMaterialShellPaneBrush"));
        Assert.True(dictionary.Contains("FluentMaterialContentLayerBrush"));
        Assert.True(dictionary.Contains("FluentMaterialCardBrush"));
        Assert.True(dictionary.Contains("FluentMaterialTransientAcrylicBrush"));
        Assert.True(dictionary.Contains("FluentMaterialFocusedGlassBrush"));
        Assert.True(dictionary.Contains("FluentMaterialLayerBorderBrush"));
        Assert.True(dictionary.Contains("FluentMaterialRoleBadgeBrush"));
        Assert.True(dictionary.Contains("FluentMaterialRoleIconBackgroundBrush"));
        Assert.True(dictionary.Contains("FluentMaterialElevationShadowBrush"));
        Assert.True(dictionary.Contains("FluentMaterialMicaTintBrush"));
        Assert.True(dictionary.Contains("FluentMaterialMicaTintOpacity"));
        Assert.True(dictionary.Contains("FluentMaterialMicaBlurRadius"));
        Assert.True(dictionary.Contains("FluentMaterialMicaAltTintBrush"));
        Assert.True(dictionary.Contains("FluentMaterialMicaAltTintOpacity"));
        Assert.True(dictionary.Contains("FluentMaterialMicaAltBlurRadius"));
        Assert.True(dictionary.Contains("FluentMaterialAcrylicTintBrush"));
        Assert.True(dictionary.Contains("FluentMaterialAcrylicTintOpacity"));
        Assert.True(dictionary.Contains("FluentMaterialAcrylicBlurRadius"));
        Assert.True(dictionary.Contains("FluentMaterialAcrylicNoiseIntensity"));
        Assert.True(dictionary.Contains("FluentMaterialFrostedGlassTintBrush"));
        Assert.True(dictionary.Contains("FluentMaterialFrostedGlassTintOpacity"));
        Assert.True(dictionary.Contains("FluentMaterialFrostedGlassBlurRadius"));
        Assert.True(dictionary.Contains("FluentMaterialFrostedGlassNoiseIntensity"));
        Assert.True(dictionary.Contains("FluentMaterialLiquidGlassTintBrush"));
        Assert.True(dictionary.Contains("FluentMaterialLiquidGlassTintOpacity"));
        Assert.True(dictionary.Contains("FluentMaterialLiquidGlassBlurRadius"));
        Assert.True(dictionary.Contains("FluentMaterialLiquidGlassRefractionAmount"));
        Assert.True(dictionary.Contains("FluentMaterialLiquidGlassChromaticAberration"));
        Assert.True(dictionary.Contains("FluentMaterialLiquidGlassFusionRadius"));
        Assert.True(dictionary.Contains("FluentMaterialFocusGlassSuperEllipseN"));
        Assert.True(dictionary.Contains("FluentMaterialWindowDefaultProfile"));
        Assert.True(dictionary.Contains("FluentMaterialWindowSurfaceCornerRadius"));
        Assert.True(dictionary.Contains("FluentMaterialShellPaneCornerRadius"));
        Assert.True(dictionary.Contains("FluentMaterialContentLayerCornerRadius"));
        Assert.True(dictionary.Contains("FluentMaterialCardCornerRadius"));
        Assert.True(dictionary.Contains("FluentMaterialFlyoutCornerRadius"));
        Assert.True(dictionary.Contains("FluentMaterialFocusGlassCornerRadius"));
        Assert.True(dictionary.Contains("FluentMaterialWindowSurfacePadding"));
        Assert.True(dictionary.Contains("FluentMaterialShellPanePadding"));
        Assert.True(dictionary.Contains("FluentMaterialContentLayerPadding"));
        Assert.True(dictionary.Contains("FluentMaterialCardPadding"));
        Assert.True(dictionary.Contains("FluentMaterialFlyoutPadding"));
        Assert.True(dictionary.Contains("FluentMaterialFocusGlassPadding"));
        Assert.True(dictionary.Contains("FluentMaterialShellPaneBorderThickness"));
        Assert.True(dictionary.Contains("FluentMaterialCardBorderThickness"));
        Assert.True(dictionary.Contains("FluentMaterialFlyoutBorderThickness"));
        Assert.True(dictionary.Contains("FluentMaterialFocusGlassBorderThickness"));
        Assert.True(dictionary.Contains("FluentControlCornerRadius"));
        Assert.True(dictionary.Contains("FluentOverlayCornerRadius"));
        Assert.True(dictionary.Contains("FluentCardCornerRadius"));
        Assert.True(dictionary.Contains("FluentCompactCornerRadius"));
        Assert.True(dictionary.Contains("FluentControlBorderThickness"));
        Assert.True(dictionary.Contains("FluentControlElevationBorderBrush"));
        Assert.True(dictionary.Contains("FluentAccentControlElevationBorderBrush"));
        Assert.True(dictionary.Contains("ControlCornerRadius"));
        Assert.True(dictionary.Contains("OverlayCornerRadius"));
        Assert.True(dictionary.Contains("ControlElevationBorderBrush"));
        Assert.True(dictionary.Contains("AccentControlElevationBorderBrush"));
        Assert.True(dictionary.Contains("FluentMotionDurationFast"));
        Assert.True(dictionary.Contains("FluentMotionDurationNormal"));
        Assert.True(dictionary.Contains("FluentMotionDurationEmphasized"));
        Assert.True(dictionary.Contains("FluentMotionConnectedAnimationDuration"));
        Assert.True(dictionary.Contains("FluentMotionConnectedAnimationInitialOpacity"));
        Assert.True(dictionary.Contains("FluentMotionConnectedAnimationNavigationDuration"));
        Assert.True(dictionary.Contains("FluentMotionConnectedAnimationNavigationInitialOpacity"));
        Assert.True(dictionary.Contains("FluentMotionConnectedAnimationEntranceDuration"));
        Assert.True(dictionary.Contains("FluentMotionConnectedAnimationEntranceInitialOpacity"));
        Assert.True(dictionary.Contains("FluentMotionConnectedAnimationDirectDuration"));
        Assert.True(dictionary.Contains("FluentMotionConnectedAnimationDirectInitialOpacity"));
        Assert.True(dictionary.Contains("FluentMotionConnectedAnimationGravityDuration"));
        Assert.True(dictionary.Contains("FluentMotionConnectedAnimationGravityInitialOpacity"));
        Assert.True(dictionary.Contains("FluentMotionContentTransitionDefaultMode"));
        Assert.True(dictionary.Contains("FluentMotionContentTransitionDefaultDuration"));
        Assert.True(dictionary.Contains("FluentMotionContentTransitionDefaultTimingFunction"));
        Assert.True(dictionary.Contains("FluentMotionContentTransitionEntranceMode"));
        Assert.True(dictionary.Contains("FluentMotionContentTransitionEntranceDuration"));
        Assert.True(dictionary.Contains("FluentMotionContentTransitionEntranceTimingFunction"));
        Assert.True(dictionary.Contains("FluentMotionContentTransitionDrillInMode"));
        Assert.True(dictionary.Contains("FluentMotionContentTransitionDrillInDuration"));
        Assert.True(dictionary.Contains("FluentMotionContentTransitionDrillInTimingFunction"));
        Assert.True(dictionary.Contains("FluentMotionContentTransitionBackNavigationMode"));
        Assert.True(dictionary.Contains("FluentMotionContentTransitionBackNavigationDuration"));
        Assert.True(dictionary.Contains("FluentMotionContentTransitionBackNavigationTimingFunction"));
        Assert.True(dictionary.Contains("FluentMotionContentTransitionLiquidMorphMode"));
        Assert.True(dictionary.Contains("FluentMotionContentTransitionLiquidMorphDuration"));
        Assert.True(dictionary.Contains("FluentMotionContentTransitionLiquidMorphTimingFunction"));
        Assert.True(dictionary.Contains("FluentMotionContentTransitionSuppressMode"));
        Assert.True(dictionary.Contains("FluentMotionContentTransitionSuppressDuration"));
        Assert.True(dictionary.Contains("FluentMotionContentTransitionSuppressTimingFunction"));
        Assert.True(dictionary.Contains("ColorPickerBackground"));
        Assert.True(dictionary.Contains("ColorPickerBorderBrush"));
        Assert.True(dictionary.Contains("ColorPickerForeground"));
        Assert.True(dictionary.Contains("ColorPickerSelectorBrush"));
        Assert.True(dictionary.Contains("InkCanvasBackground"));
        Assert.True(dictionary.Contains("InkCanvasBorderBrush"));
        Assert.True(dictionary.Contains("InkCanvasDefaultStrokeBrush"));
        Assert.True(dictionary.Contains("InkPresenterBackground"));
        Assert.True(dictionary.Contains("MediaElementBackground"));
        Assert.True(dictionary.Contains("MediaElementBorderBrush"));
        Assert.True(dictionary.Contains("MediaElementOverlayBackground"));
        Assert.True(dictionary.Contains("MediaElementForeground"));
        Assert.True(dictionary.Contains("FluentInkCanvasDrawingAttributes"));
        Assert.True(dictionary.Contains("ImageBackground"));
        Assert.True(dictionary.Contains("ImageBorderBrush"));
        Assert.True(dictionary.Contains("IconForeground"));
        Assert.True(dictionary.Contains("IconForegroundDisabled"));
        Assert.True(dictionary.Contains("LabelForeground"));
        Assert.True(dictionary.Contains("SeparatorStrokeBrush"));
        Assert.True(dictionary.Contains("InfoBarForeground"));
        Assert.True(dictionary.Contains("InfoBarSuccessBackground"));
        Assert.True(dictionary.Contains("ToastForeground"));
        Assert.True(dictionary.Contains("ToastSuccessBackground"));
        Assert.True(dictionary.Contains("InfoBadgeAttentionBackground"));
        Assert.True(dictionary.Contains("InfoBadgeCriticalForeground"));
        Assert.True(dictionary.Contains("StatusBarBackground"));
        Assert.True(dictionary.Contains("StatusBarSeparatorForeground"));
        Assert.True(dictionary.Contains("ControlContentThemeFontSize"));

        var buttonStyle = Assert.IsType<Style>(dictionary[typeof(Button)]);
        AssertStyleSetterValue(buttonStyle, Control.BorderBrushProperty, "ControlElevationBorderBrush");
        AssertStyleSetterValue(buttonStyle, Control.CornerRadiusProperty, dictionary["ControlCornerRadius"]);
        var transitioningStyle = Assert.IsType<Style>(dictionary[typeof(TransitioningContentControl)]);
        AssertStyleSetterValue(transitioningStyle, TransitioningContentControl.TransitionModeProperty, "FluentMotionContentTransitionDefaultMode");
        AssertStyleSetterValue(transitioningStyle, UIElement.TransitionDurationProperty, "FluentMotionContentTransitionDefaultDuration");
        AssertStyleSetterValue(transitioningStyle, UIElement.TransitionTimingFunctionProperty, "FluentMotionContentTransitionDefaultTimingFunction");
        var windowSurfaceStyle = Assert.IsType<Style>(dictionary[typeof(FWFluentWindowSurface)]);
        AssertStyleSetterValue(windowSurfaceStyle, FWFluentWindowSurface.WindowMaterialProfileProperty, "MicaShell");
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void AggregatedThemeDictionaries_ShouldLoadResourcesAndControlsIndependently()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = FluentThemeVariant.Dark.ToString();

        var resources = ResourceDictionary.SourceLoader?.Invoke(
            new ResourceDictionary(),
            new Uri("/FluentJalium;component/Themes/FluentResources.jalxaml", UriKind.Relative),
            FluentThemeManager.ThemeAssembly);
        var controls = ResourceDictionary.SourceLoader?.Invoke(
            new ResourceDictionary(),
            new Uri("/FluentJalium;component/Themes/Controls/FluentControls.jalxaml", UriKind.Relative),
            FluentThemeManager.ThemeAssembly);

        Assert.NotNull(resources);
        Assert.NotNull(controls);
        Assert.Equal("FluentJalium.Themes.FluentResources.jalxaml", FluentThemeManager.FluentResourcesResourceName);
        Assert.Equal("FluentJalium.Themes.Controls.FluentControls.jalxaml", FluentThemeManager.FluentControlsResourceName);
        Assert.True(resources!.Contains("FluentMaterialWindowBackdropBrush"));
        Assert.True(resources.Contains("FluentMaterialWindowDefaultProfile"));
        Assert.True(resources.Contains("FluentMotionDurationNormal"));
        Assert.True(resources.Contains("FluentTitleFontSize"));
        AssertContainsStyle<Button>(controls!);
        AssertContainsStyle<NavigationView>(controls);
        AssertContainsStyle<ContentDialog>(controls);
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading through public dictionary entry points.")]
    public void FluentDictionaries_ShouldExposeStableApplicationResourceEntryPoints()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = FluentThemeVariant.Dark.ToString();

        var completeEntry = new FluentJaliumThemeDictionary();
        var resourcesEntry = new FluentJaliumResourcesDictionary();
        var controlsEntry = new FluentJaliumControlsDictionary();

        Assert.Equal(FluentJaliumDictionaryKind.Complete, completeEntry.Kind);
        Assert.Equal(FluentJaliumDictionary.GetSource(FluentJaliumDictionaryKind.Complete), completeEntry.Source);
        Assert.Equal(FluentJaliumDictionaryKind.Resources, resourcesEntry.Kind);
        Assert.Equal(FluentJaliumDictionary.GetSource(FluentJaliumDictionaryKind.Resources), resourcesEntry.Source);
        Assert.Equal(FluentJaliumDictionaryKind.Controls, controlsEntry.Kind);
        Assert.Equal(FluentJaliumDictionary.GetSource(FluentJaliumDictionaryKind.Controls), controlsEntry.Source);
        Assert.Throws<ArgumentOutOfRangeException>(() => FluentJaliumDictionary.GetSource((FluentJaliumDictionaryKind)42));

        var complete = LoadEntryDictionary(completeEntry);
        var resources = LoadEntryDictionary(resourcesEntry);
        var controls = LoadEntryDictionary(controlsEntry);

        Assert.True(complete.Contains("FluentMaterialWindowBackdropBrush"));
        Assert.True(complete.Contains("FluentMaterialWindowDefaultProfile"));
        Assert.True(complete.Contains("FluentMotionContentTransitionDefaultDuration"));
        AssertContainsStyle<Button>(complete);
        Assert.True(resources.Contains("FluentMaterialWindowBackdropBrush"));
        Assert.True(resources.Contains("FluentMaterialWindowDefaultProfile"));
        Assert.True(resources.Contains("FluentMotionContentTransitionDefaultDuration"));
        Assert.False(resources.Contains(typeof(Button)));
        AssertContainsStyle<Button>(controls);
        AssertContainsStyle<TransitioningContentControl>(controls);
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void ButtonBatch_ShouldExposeFwStylesForButtonAndCommandControls()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            AssertBasedOnStyle<FWButton, Button>(app.Resources);
            AssertBasedOnStyle<FWRepeatButton, RepeatButton>(app.Resources);
            AssertBasedOnStyle<FWHyperlinkButton, HyperlinkButton>(app.Resources);
            AssertOwnedStyle<FWDropDownButton>(app.Resources);
            AssertBasedOnStyle<FWSplitButton, SplitButton>(app.Resources);
            AssertOwnedStyle<FWToggleSplitButton>(app.Resources);
            AssertBasedOnStyle<FWCommandBar, CommandBar>(app.Resources);
            AssertBasedOnStyle<FWAppBarButton, AppBarButton>(app.Resources);
            AssertBasedOnStyle<FWAppBarToggleButton, AppBarToggleButton>(app.Resources);
            AssertBasedOnStyle<FWAppBarSeparator, AppBarSeparator>(app.Resources);
            AssertBasedOnStyle<FWToolBar, Jalium.UI.Controls.ToolBar>(app.Resources);
            AssertBasedOnStyle<FWToolBarTray, ToolBarTray>(app.Resources);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void TextInputBatch_ShouldExposeFwStylesForTextInputControls()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            AssertBasedOnStyle<FWTextBox, TextBox>(app.Resources);
            AssertBasedOnStyle<FWPasswordBox, PasswordBox>(app.Resources);
            AssertBasedOnStyle<FWNumberBox, NumberBox>(app.Resources);
            AssertBasedOnStyle<FWAutoCompleteBox, AutoCompleteBox>(app.Resources);
            AssertBasedOnStyle<FWRichTextBox, RichTextBox>(app.Resources);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void TextInputBatch_ShouldDefineFwNumberBoxDensityDefaults()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            var numberBoxStyle = Assert.IsType<Style>(app.Resources[typeof(NumberBox)]);
            var fwNumberBoxStyle = Assert.IsType<Style>(app.Resources[typeof(FWNumberBox)]);

            Assert.Same(numberBoxStyle, fwNumberBoxStyle.BasedOn);
            AssertStyleSetterValue(fwNumberBoxStyle, FWNumberBox.DensityProperty, "Comfortable");
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void SwitchBatch_ShouldExposeFwStylesForSwitchControls()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            AssertBasedOnStyle<FWToggleButton, ToggleButton>(app.Resources);
            AssertBasedOnStyle<FWToggleSwitch, ToggleSwitch>(app.Resources);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void RangeBatch_ShouldExposeFwStylesForRangeControls()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            AssertBasedOnStyle<FWSlider, Slider>(app.Resources);
            AssertBasedOnStyle<FWRangeSlider, RangeSlider>(app.Resources);
            AssertBasedOnStyle<FWProgressBar, ProgressBar>(app.Resources);
            AssertOwnedStyle<FWProgressRing>(app.Resources);
            Assert.IsType<SolidColorBrush>(app.Resources["SliderTrackFill"]);
            Assert.IsType<SolidColorBrush>(app.Resources["SliderTrackValueFill"]);
            Assert.IsType<SolidColorBrush>(app.Resources["SliderThumbBackground"]);
            Assert.IsType<SolidColorBrush>(app.Resources["ProgressBarBackground"]);
            Assert.IsType<SolidColorBrush>(app.Resources["ProgressBarForeground"]);
            Assert.IsType<SolidColorBrush>(app.Resources["ProgressRingBackground"]);
            Assert.IsType<SolidColorBrush>(app.Resources["ProgressRingForeground"]);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void SelectionBatch_ShouldExposeFwStylesForSelectionControls()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            AssertBasedOnStyle<FWCheckBox, CheckBox>(app.Resources);
            AssertBasedOnStyle<FWRadioButton, RadioButton>(app.Resources);
            AssertBasedOnStyle<FWComboBox, ComboBox>(app.Resources);
            AssertBasedOnStyle<FWComboBoxItem, ComboBoxItem>(app.Resources);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void VisualBatch_ShouldExposeFwStylesForVisualAndIconControls()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            AssertBasedOnStyle<FWImage, Image>(app.Resources);
            AssertBasedOnStyle<FWFontIcon, FontIcon>(app.Resources);
            AssertBasedOnStyle<FWSymbolIcon, SymbolIcon>(app.Resources);
            AssertBasedOnStyle<FWPathIcon, PathIcon>(app.Resources);
            AssertBasedOnStyle<FWViewbox, Viewbox>(app.Resources);
            AssertBasedOnStyle<FWLabel, Label>(app.Resources);
            AssertBasedOnStyle<FWSeparator, Separator>(app.Resources);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void InteractionBatch_ShouldExposeFwStylesForScrollAndGestureControls()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            AssertBasedOnStyle<FWScrollBar, ScrollBar>(app.Resources);
            AssertBasedOnStyle<FWScrollViewer, ScrollViewer>(app.Resources);
            AssertBasedOnStyle<FWSwipeControl, SwipeControl>(app.Resources);
            AssertBasedOnStyle<FWGridSplitter, GridSplitter>(app.Resources);
            Assert.IsType<Style>(app.Resources["ScrollBarStyle"]);
            Assert.IsType<Style>(app.Resources["ScrollBarThumbStyle"]);
            Assert.IsType<SolidColorBrush>(app.Resources["SwipeItemBackground"]);
            Assert.IsType<SolidColorBrush>(app.Resources["GridSplitterGripBrush"]);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void ContentLayoutBatch_ShouldExposeFwStylesForContentAndLayoutControls()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            AssertBasedOnStyle<FWTextBlock, TextBlock>(app.Resources);
            AssertBasedOnStyle<FWAccessText, AccessText>(app.Resources);
            AssertBasedOnStyle<FWBorder, Border>(app.Resources);
            AssertBasedOnStyle<FWFluentMaterialSurface, Border>(app.Resources);
            AssertBasedOnStyle<FWFluentWindowSurface, Border>(app.Resources);
            AssertBasedOnStyle<FWContentControl, ContentControl>(app.Resources);
            AssertBasedOnStyle<FWTransitioningContentControl, TransitioningContentControl>(app.Resources);
            AssertBasedOnStyle<FWContentPresenter, ContentPresenter>(app.Resources);
            AssertBasedOnStyle<FWStackPanel, StackPanel>(app.Resources);
            AssertBasedOnStyle<FWWrapPanel, WrapPanel>(app.Resources);
            AssertBasedOnStyle<FWGrid, Grid>(app.Resources);
            Assert.IsType<SolidColorBrush>(app.Resources["ContentSurfaceBorderBrush"]);
            Assert.IsType<SolidColorBrush>(app.Resources["ContentPanelBackground"]);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void InputMediaBatch_ShouldExposeFwStylesForAdvancedInputAndMediaControls()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            AssertBasedOnStyle<FWColorPicker, ColorPicker>(app.Resources);
            AssertOwnedStyle<FWInkCanvas>(app.Resources);
            AssertBasedOnStyle<FWInkPresenter, InkPresenter>(app.Resources);
            AssertOwnedStyle<FWMediaElement>(app.Resources);
            Assert.IsType<SolidColorBrush>(app.Resources["ColorPickerBackground"]);
            Assert.IsType<SolidColorBrush>(app.Resources["InkCanvasBorderBrush"]);
            Assert.IsType<SolidColorBrush>(app.Resources["MediaElementForeground"]);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void CollectionsBatch_ShouldExposeFwStylesForCollectionAndTableControls()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            AssertBasedOnStyle<FWListBox, ListBox>(app.Resources);
            AssertBasedOnStyle<FWListBoxItem, ListBoxItem>(app.Resources);
            AssertBasedOnStyle<FWListView, ListView>(app.Resources);
            AssertBasedOnStyle<FWListViewItem, ListViewItem>(app.Resources);
            AssertBasedOnStyle<FWTreeView, TreeView>(app.Resources);
            AssertBasedOnStyle<FWTreeViewItem, TreeViewItem>(app.Resources);
            AssertBasedOnStyle<FWDataGrid, DataGrid>(app.Resources);
            AssertBasedOnStyle<FWTreeDataGrid, TreeDataGrid>(app.Resources);
            AssertBasedOnStyle<FWTreeSelector, TreeSelector>(app.Resources);
            AssertBasedOnStyle<FWTreeSelectorItem, TreeSelectorItem>(app.Resources);
            AssertBasedOnStyle<FWPropertyGrid, PropertyGrid>(app.Resources);
            AssertBasedOnStyle<FWDiffViewer, DiffViewer>(app.Resources);
            AssertBasedOnStyle<FWHexEditor, HexEditor>(app.Resources);
            AssertBasedOnStyle<FWJsonTreeViewer, JsonTreeViewer>(app.Resources);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void NavigationBatch_ShouldExposeFwStylesForNavigationControls()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            AssertBasedOnStyle<FWNavigationView, NavigationView>(app.Resources);
            AssertBasedOnStyle<FWNavigationViewItem, NavigationViewItem>(app.Resources);
            AssertBasedOnStyle<FWNavigationViewItemHeader, NavigationViewItemHeader>(app.Resources);
            AssertBasedOnStyle<FWNavigationViewItemSeparator, NavigationViewItemSeparator>(app.Resources);
            AssertBasedOnStyle<FWTabControl, TabControl>(app.Resources);
            AssertBasedOnStyle<FWTabItem, TabItem>(app.Resources);
            AssertBasedOnStyle<FWFrame, Frame>(app.Resources);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void MenuBatch_ShouldExposeFwStylesForMenuAndFlyoutControls()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            AssertBasedOnStyle<FWMenuBar, MenuBar>(app.Resources);
            AssertBasedOnStyle<FWMenuBarItem, MenuBarItem>(app.Resources);
            AssertBasedOnStyle<FWMenu, Menu>(app.Resources);
            AssertBasedOnStyle<FWMenuItem, MenuItem>(app.Resources);
            AssertBasedOnStyle<FWContextMenu, ContextMenu>(app.Resources);
            AssertBasedOnStyle<FWMenuFlyoutItem, MenuFlyoutItem>(app.Resources);
            AssertBasedOnStyle<FWToggleMenuFlyoutItem, ToggleMenuFlyoutItem>(app.Resources);
            AssertBasedOnStyle<FWMenuFlyoutSeparator, MenuFlyoutSeparator>(app.Resources);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void DisclosureDialogBatch_ShouldExposeFwStylesForDisclosureAndDialogControls()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            AssertBasedOnStyle<FWExpander, Expander>(app.Resources);
            AssertBasedOnStyle<FWToolTip, ToolTip>(app.Resources);
            AssertBasedOnStyle<FWContentDialog, ContentDialog>(app.Resources);
            AssertBasedOnStyle<FWGroupBox, GroupBox>(app.Resources);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void DateTimeBatch_ShouldExposeFwStylesForDateTimeControls()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            AssertBasedOnStyle<FWDatePicker, DatePicker>(app.Resources);
            AssertBasedOnStyle<FWTimePicker, TimePicker>(app.Resources);
            AssertBasedOnStyle<FWCalendar, Calendar>(app.Resources);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void NotificationStatusBatch_ShouldExposeFwStylesForNotificationAndStatusControls()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            AssertBasedOnStyle<FWInfoBar, InfoBar>(app.Resources);
            AssertOwnedStyle<FWInfoBadge>(app.Resources);
            AssertBasedOnStyle<FWToastNotificationItem, ToastNotificationItem>(app.Resources);
            AssertBasedOnStyle<FWToastNotificationHost, ToastNotificationHost>(app.Resources);
            AssertBasedOnStyle<FWStatusBar, StatusBar>(app.Resources);
            AssertBasedOnStyle<FWStatusBarItem, Jalium.UI.Controls.StatusBarItem>(app.Resources);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    public void FluentControls_ShouldExposeFwPrefixedButtonSurface()
    {
        AssertFluentControl<FWButton, Button>();
        AssertFluentControl<FWRepeatButton, RepeatButton>();
        AssertFluentControl<FWHyperlinkButton, HyperlinkButton>();
        AssertFluentControl<FWDropDownButton, Button>();
        AssertFluentControl<FWSplitButton, SplitButton>();
        AssertFluentControl<FWToggleSplitButton, SplitButton>();
        AssertFluentControl<FWCommandBar, CommandBar>();
        AssertFluentControl<FWAppBarButton, AppBarButton>();
        AssertFluentControl<FWAppBarToggleButton, AppBarToggleButton>();
        AssertFluentControl<FWAppBarSeparator, AppBarSeparator>();
        AssertFluentControl<FWToolBar, Jalium.UI.Controls.ToolBar>();
        AssertFluentControl<FWToolBarTray, ToolBarTray>();
    }

    [Fact]
    public void FluentTextInputControls_ShouldExposeFwPrefixedSurface()
    {
        AssertFluentControl<FWTextBox, TextBox>();
        AssertFluentControl<FWPasswordBox, PasswordBox>();
        AssertFluentControl<FWNumberBox, NumberBox>();
        AssertFluentControl<FWAutoCompleteBox, AutoCompleteBox>();
        AssertFluentControl<FWRichTextBox, RichTextBox>();
    }

    [Fact]
    public void FluentSwitchControls_ShouldExposeFwPrefixedSurface()
    {
        AssertFluentControl<FWToggleButton, ToggleButton>();
        AssertFluentControl<FWToggleSwitch, ToggleSwitch>();
    }

    [Fact]
    public void FluentSelectionControls_ShouldExposeFwPrefixedSurface()
    {
        AssertFluentControl<FWCheckBox, CheckBox>();
        AssertFluentControl<FWRadioButton, RadioButton>();
        AssertFluentControl<FWComboBox, ComboBox>();
        AssertFluentControl<FWComboBoxItem, ComboBoxItem>();
        AssertFluentControl<FWRatingControl, Control>();
    }

    [Fact]
    public void FluentVisualControls_ShouldExposeFwPrefixedSurface()
    {
        AssertFluentControl<FWImage, Image>();
        AssertFluentControl<FWFontIcon, FontIcon>();
        AssertFluentControl<FWSymbolIcon, SymbolIcon>();
        AssertFluentControl<FWPathIcon, PathIcon>();
        AssertFluentControl<FWViewbox, Viewbox>();
        AssertFluentControl<FWLabel, Label>();
        AssertFluentControl<FWSeparator, Separator>();
    }

    [Fact]
    public void FluentInteractionControls_ShouldExposeFwPrefixedSurface()
    {
        AssertFluentControl<FWScrollBar, ScrollBar>();
        AssertFluentControl<FWScrollViewer, ScrollViewer>();
        AssertFluentControl<FWSwipeControl, SwipeControl>();
        AssertFluentControl<FWGridSplitter, GridSplitter>();
    }

    [Fact]
    public void FluentContentLayoutControls_ShouldExposeFwPrefixedSurface()
    {
        AssertFluentControl<FWTextBlock, TextBlock>();
        AssertFluentControl<FWAccessText, AccessText>();
        AssertFluentControl<FWBorder, Border>();
        AssertFluentControl<FWFluentMaterialSurface, Border>();
        AssertFluentControl<FWFluentWindowSurface, Border>();
        AssertFluentControl<FWContentControl, ContentControl>();
        AssertFluentControl<FWTransitioningContentControl, TransitioningContentControl>();
        AssertFluentControl<FWContentPresenter, ContentPresenter>();
        AssertFluentControl<FWStackPanel, StackPanel>();
        AssertFluentControl<FWWrapPanel, WrapPanel>();
        AssertFluentControl<FWGrid, Grid>();
    }

    [Fact]
    public void FluentInputMediaControls_ShouldExposeFwPrefixedSurface()
    {
        AssertFluentControl<FWColorPicker, ColorPicker>();
        AssertFluentControl<FWInkCanvas, InkCanvas>();
        AssertFluentControl<FWInkPresenter, InkPresenter>();
        AssertFluentControl<FWMediaElement, MediaElement>();
    }

    [Fact]
    public void FluentCollectionControls_ShouldExposeFwPrefixedSurface()
    {
        AssertFluentControl<FWListBox, ListBox>();
        AssertFluentControl<FWListBoxItem, ListBoxItem>();
        AssertFluentControl<FWListView, ListView>();
        AssertFluentControl<FWListViewItem, ListViewItem>();
        AssertFluentControl<FWTreeView, TreeView>();
        AssertFluentControl<FWTreeViewItem, TreeViewItem>();
        AssertFluentControl<FWDataGrid, DataGrid>();
        AssertFluentControl<FWTreeDataGrid, TreeDataGrid>();
        AssertFluentControl<FWTreeSelector, TreeSelector>();
        AssertFluentControl<FWTreeSelectorItem, TreeSelectorItem>();
        AssertFluentControl<FWPropertyGrid, PropertyGrid>();
        AssertFluentControl<FWDiffViewer, DiffViewer>();
        AssertFluentControl<FWHexEditor, HexEditor>();
        AssertFluentControl<FWJsonTreeViewer, JsonTreeViewer>();
    }

    [Fact]
    public void FluentNavigationControls_ShouldExposeFwPrefixedSurface()
    {
        AssertFluentControl<FWNavigationView, NavigationView>();
        AssertFluentControl<FWNavigationViewItem, NavigationViewItem>();
        AssertFluentControl<FWNavigationViewItemHeader, NavigationViewItemHeader>();
        AssertFluentControl<FWNavigationViewItemSeparator, NavigationViewItemSeparator>();
        AssertFluentControl<FWTabControl, TabControl>();
        AssertFluentControl<FWTabItem, TabItem>();
        AssertFluentControl<FWFrame, Frame>();
    }

    [Fact]
    public void FluentMenuControls_ShouldExposeFwPrefixedSurface()
    {
        AssertFluentControl<FWMenuBar, MenuBar>();
        AssertFluentControl<FWMenuBarItem, MenuBarItem>();
        AssertFluentControl<FWMenu, Menu>();
        AssertFluentControl<FWMenuItem, MenuItem>();
        AssertFluentControl<FWContextMenu, ContextMenu>();
        AssertFluentControl<FWMenuFlyoutItem, MenuFlyoutItem>();
        AssertFluentControl<FWToggleMenuFlyoutItem, ToggleMenuFlyoutItem>();
        AssertFluentControl<FWMenuFlyoutSeparator, MenuFlyoutSeparator>();
    }

    [Fact]
    public void FluentDisclosureDialogControls_ShouldExposeFwPrefixedSurface()
    {
        AssertFluentControl<FWExpander, Expander>();
        AssertFluentControl<FWToolTip, ToolTip>();
        AssertFluentControl<FWContentDialog, ContentDialog>();
        AssertFluentControl<FWGroupBox, GroupBox>();
    }

    [Fact]
    public void FluentDateTimeControls_ShouldExposeFwPrefixedSurface()
    {
        AssertFluentControl<FWDatePicker, DatePicker>();
        AssertFluentControl<FWTimePicker, TimePicker>();
        AssertFluentControl<FWCalendar, Calendar>();
    }

    [Fact]
    public void FluentNotificationStatusControls_ShouldExposeFwPrefixedSurface()
    {
        AssertFluentControl<FWInfoBar, InfoBar>();
        AssertFluentControl<FWInfoBadge, Control>();
        AssertFluentControl<FWToastNotificationItem, ToastNotificationItem>();
        AssertFluentControl<FWToastNotificationHost, ToastNotificationHost>();
        AssertFluentControl<FWStatusBar, StatusBar>();
        AssertFluentControl<FWStatusBarItem, Jalium.UI.Controls.StatusBarItem>();
    }

    [Fact]
    public void FWTextBox_ShouldRaiseTextChangedAndKeepTextInputState()
    {
        var textBox = new FWTextBox
        {
            PlaceholderText = "Enter text",
            AcceptsReturn = true,
            MaxLength = 64,
            TextWrapping = TextWrapping.Wrap
        };
        var changed = 0;
        TextChangedEventArgs? lastArgs = null;
        textBox.TextChanged += (_, args) =>
        {
            changed++;
            lastArgs = args;
        };

        textBox.Text = "Fluent\r\nJalium";

        Assert.Equal("Fluent\r\nJalium", textBox.Text);
        Assert.Equal("Enter text", textBox.PlaceholderText);
        Assert.True(textBox.AcceptsReturn);
        Assert.Equal(TextWrapping.Wrap, textBox.TextWrapping);
        Assert.True(textBox.LineCount >= 2);
        Assert.Equal(1, changed);
        Assert.NotNull(lastArgs);
    }

    [Fact]
    public void FWPasswordBox_ShouldRaisePasswordChangedAndExposeRevealState()
    {
        var passwordBox = new FWPasswordBox
        {
            PlaceholderText = "Password",
            MaxLength = 24,
            RevealMode = PasswordRevealMode.Visible
        };
        var changed = 0;
        passwordBox.PasswordChanged += (_, _) => changed++;

        passwordBox.Password = "fluent";
        passwordBox.IsPasswordRevealed = true;

        Assert.Equal("fluent", passwordBox.Password);
        Assert.Equal(6, passwordBox.SecurePassword.Length);
        Assert.Equal("Password", passwordBox.PlaceholderText);
        Assert.Equal(PasswordRevealMode.Visible, passwordBox.RevealMode);
        Assert.True(passwordBox.IsPasswordRevealed);
        Assert.Equal(1, changed);
    }

    [Fact]
    public void FWNumberBox_ShouldStepCoerceWrapAndRaiseValueChanged()
    {
        var numberBox = new FWNumberBox
        {
            Density = FWNumberBoxDensity.Compact,
            Minimum = 0,
            Maximum = 10,
            SmallChange = 2,
            LargeChange = 5,
            IsWrapEnabled = true,
            Value = 8
        };
        var changed = 0;
        RoutedPropertyChangedEventArgs<double>? lastArgs = null;
        numberBox.ValueChanged += (_, args) =>
        {
            changed++;
            lastArgs = args;
        };

        numberBox.StepUp();
        numberBox.StepUp();
        numberBox.StepDown();

        Assert.Equal(10, numberBox.Value);
        Assert.Equal("10", numberBox.Text);
        Assert.Equal(FWNumberBoxDensity.Compact, numberBox.Density);
        Assert.Equal(30, numberBox.MinHeight);
        Assert.Equal(new Thickness(8, 4, 8, 5), numberBox.Padding);
        Assert.Equal(2, numberBox.SmallChange);
        Assert.Equal(5, numberBox.LargeChange);
        Assert.Equal(3, changed);
        Assert.NotNull(lastArgs);
        Assert.Equal(0, lastArgs!.OldValue);
        Assert.Equal(10, lastArgs.NewValue);

        numberBox.Density = FWNumberBoxDensity.Spacious;

        Assert.Equal(40, numberBox.MinHeight);
        Assert.Equal(new Thickness(12, 8, 12, 8), numberBox.Padding);
    }

    [Fact]
    public void FWAutoCompleteBox_ShouldFilterSuggestionsAndRaiseTextAndDropDownEvents()
    {
        var autoCompleteBox = new FWAutoCompleteBox
        {
            ItemsSource = new[] { "Fluent tokens", "Fluent controls", "WinUI Gallery", "Community Toolkit" },
            FilterMode = AutoCompleteFilterMode.Contains,
            MinimumPrefixLength = 1,
            PlaceholderText = "Search"
        };
        var textChanged = 0;
        var populating = 0;
        var opened = 0;
        var closed = 0;
        autoCompleteBox.TextChanged += (_, _) => textChanged++;
        autoCompleteBox.Populating += (_, _) => populating++;
        autoCompleteBox.DropDownOpened += (_, _) => opened++;
        autoCompleteBox.DropDownClosed += (_, _) => closed++;

        autoCompleteBox.Text = "Fluent";

        Assert.Equal("Fluent", autoCompleteBox.Text);
        Assert.Equal("Search", autoCompleteBox.PlaceholderText);
        Assert.True(autoCompleteBox.IsDropDownOpen);
        Assert.Equal(2, autoCompleteBox.FilteredItems.Count);
        Assert.Contains("Fluent tokens", autoCompleteBox.FilteredItems);
        Assert.Contains("Fluent controls", autoCompleteBox.FilteredItems);

        autoCompleteBox.Text = "Nothing";

        Assert.False(autoCompleteBox.IsDropDownOpen);
        Assert.Empty(autoCompleteBox.FilteredItems);
        Assert.Equal(2, textChanged);
        Assert.Equal(2, populating);
        Assert.Equal(1, opened);
        Assert.Equal(1, closed);
    }

    [Fact]
    public void FWRichTextBox_ShouldSetTextSelectAndClearSelection()
    {
        var richTextBox = new FWRichTextBox
        {
            AcceptsTab = true,
            IsSpellCheckEnabled = true
        };

        richTextBox.SetText("FluentJalium rich text");
        richTextBox.SelectAll();

        Assert.Contains("FluentJalium", richTextBox.GetText());
        Assert.Contains("FluentJalium", richTextBox.Selection.Text);
        Assert.True(richTextBox.AcceptsTab);
        Assert.True(richTextBox.IsSpellCheckEnabled);

        richTextBox.ClearSelection();

        Assert.True(richTextBox.Selection.IsEmpty);
    }

    [Fact]
    public void FWToggleButton_ShouldCycleCheckedStatesAndRaiseEvents()
    {
        var button = new FWToggleButton
        {
            IsThreeState = true
        };
        var checkedCount = 0;
        var uncheckedCount = 0;
        var indeterminateCount = 0;

        button.Checked += (_, _) => checkedCount++;
        button.Unchecked += (_, _) => uncheckedCount++;
        button.Indeterminate += (_, _) => indeterminateCount++;

        InvokeToggleButtonClick(button);
        Assert.True(button.IsChecked);
        Assert.Equal(1, checkedCount);

        InvokeToggleButtonClick(button);
        Assert.Null(button.IsChecked);
        Assert.Equal(1, indeterminateCount);

        InvokeToggleButtonClick(button);
        Assert.False(button.IsChecked);
        Assert.Equal(1, uncheckedCount);
    }

    [Fact]
    public void FWToggleSwitch_ShouldChangeIsOnAndRaiseToggled()
    {
        var toggleSwitch = new FWToggleSwitch
        {
            OffContent = "Off",
            OnContent = "On"
        };
        var toggled = 0;

        toggleSwitch.Toggled += (_, _) => toggled++;

        toggleSwitch.IsOn = true;

        Assert.True(toggleSwitch.IsOn);
        Assert.Equal(1, toggled);
        Assert.Equal("On", toggleSwitch.OnContent);

        toggleSwitch.IsOn = false;

        Assert.False(toggleSwitch.IsOn);
        Assert.Equal(2, toggled);
        Assert.Equal("Off", toggleSwitch.OffContent);
    }

    [Fact]
    public void FWCheckBox_ShouldCycleCheckedStatesAndRaiseEvents()
    {
        var checkBox = new FWCheckBox
        {
            IsThreeState = true
        };
        var checkedCount = 0;
        var uncheckedCount = 0;
        var indeterminateCount = 0;

        checkBox.Checked += (_, _) => checkedCount++;
        checkBox.Unchecked += (_, _) => uncheckedCount++;
        checkBox.Indeterminate += (_, _) => indeterminateCount++;

        InvokeToggleButtonClick(checkBox);
        Assert.True(checkBox.IsChecked);
        Assert.Equal(1, checkedCount);

        InvokeToggleButtonClick(checkBox);
        Assert.Null(checkBox.IsChecked);
        Assert.Equal(1, indeterminateCount);

        InvokeToggleButtonClick(checkBox);
        Assert.False(checkBox.IsChecked);
        Assert.Equal(1, uncheckedCount);
    }

    [Fact]
    public void FWRadioButton_ShouldKeepGroupSelectionExclusive()
    {
        var groupName = $"selection-{Guid.NewGuid():N}";
        var first = new FWRadioButton
        {
            Content = "One",
            GroupName = groupName
        };
        var second = new FWRadioButton
        {
            Content = "Two",
            GroupName = groupName
        };
        var checkedCount = 0;

        first.Checked += (_, _) => checkedCount++;
        second.Checked += (_, _) => checkedCount++;

        InvokeToggleButtonClick(first);
        Assert.True(first.IsChecked);
        Assert.False(second.IsChecked);

        InvokeToggleButtonClick(second);
        Assert.False(first.IsChecked);
        Assert.True(second.IsChecked);
        Assert.Equal(2, checkedCount);

        InvokeToggleButtonClick(second);
        Assert.False(first.IsChecked);
        Assert.True(second.IsChecked);
        Assert.Equal(2, checkedCount);
    }

    [Fact]
    public void FWComboBox_ShouldSynchronizeSelectionTextAndDropDownEvents()
    {
        var comboBox = new FWComboBox
        {
            PlaceholderText = "Choose an item"
        };
        comboBox.Items.Add("Fluent tokens");
        comboBox.Items.Add("Control styles");
        comboBox.Items.Add("Gallery sample");

        var selectionChanged = 0;
        var opened = 0;
        var closed = 0;
        comboBox.SelectionChanged += (_, _) => selectionChanged++;
        comboBox.DropDownOpened += (_, _) => opened++;
        comboBox.DropDownClosed += (_, _) => closed++;

        comboBox.SelectedIndex = 1;

        Assert.Equal(1, comboBox.SelectedIndex);
        Assert.Equal("Control styles", comboBox.SelectedItem);
        Assert.Equal("Control styles", comboBox.SelectedValue);
        Assert.Equal("Control styles", comboBox.SelectionBoxItem);
        Assert.Equal(1, selectionChanged);

        comboBox.IsEditable = true;
        comboBox.Text = "Gallery sample";

        Assert.Equal(2, comboBox.SelectedIndex);
        Assert.Equal("Gallery sample", comboBox.SelectionBoxItem);

        comboBox.IsDropDownOpen = true;
        comboBox.IsDropDownOpen = false;

        Assert.False(comboBox.IsDropDownOpen);
        Assert.Equal(1, opened);
        Assert.Equal(1, closed);
    }

    [Fact]
    public void FWComboBoxItem_ShouldExposeSelectionState()
    {
        var item = new FWComboBoxItem
        {
            Content = "Option"
        };

        Assert.False(item.IsSelected);

        item.IsSelected = true;

        Assert.True(item.IsSelected);
        Assert.Equal("Option", item.Content);
    }

    [Fact]
    public void FWImage_ShouldTrackSourceStretchAndZoomSettings()
    {
        var source = BitmapImage.FromPixels(CreateSamplePixels(12, 8), 12, 8);
        var image = new FWImage
        {
            Source = source,
            Stretch = Stretch.UniformToFill,
            StretchDirection = StretchDirection.DownOnly,
            IsZoomEnabled = true,
            MinZoom = 0.5,
            MaxZoom = 4.0
        };

        image.ApplyTemplate();

        Assert.Same(source, image.Source);
        Assert.Equal(Stretch.UniformToFill, image.Stretch);
        Assert.Equal(StretchDirection.DownOnly, image.StretchDirection);
        Assert.True(image.IsZoomEnabled);
        Assert.Equal(0.5, image.MinZoom);
        Assert.Equal(4.0, image.MaxZoom);
        Assert.Equal(1.0, image.CurrentZoom);
    }

    [Fact]
    public void FWIconControls_ShouldExposeGlyphSymbolAndPathData()
    {
        var fontIcon = new FWFontIcon
        {
            Glyph = "\uE72D",
            FontFamily = "Segoe Fluent Icons",
            FontSize = 24
        };
        var symbolIcon = new FWSymbolIcon
        {
            Symbol = Symbol.Save
        };
        var geometry = Geometry.Parse("M 0,0 L 20,0 L 20,20 L 0,20 Z");
        var pathIcon = new FWPathIcon
        {
            Data = geometry,
            Width = 24,
            Height = 24
        };

        Assert.Equal("\uE72D", fontIcon.Glyph);
        Assert.Equal("Segoe Fluent Icons", fontIcon.FontFamily?.ToString());
        Assert.Equal(24, fontIcon.FontSize);
        Assert.Equal(Symbol.Save, symbolIcon.Symbol);
        Assert.Same(geometry, pathIcon.Data);
        Assert.Equal(24, pathIcon.Width);
        Assert.Equal(24, pathIcon.Height);
    }

    [Fact]
    public void FWViewbox_ShouldHostChildAndHonorStretchSettings()
    {
        var child = new TextBlock
        {
            Text = "Scaled"
        };
        var viewbox = new FWViewbox
        {
            Child = child,
            Stretch = Stretch.Uniform,
            StretchDirection = StretchDirection.Both,
            Width = 120,
            Height = 60
        };

        viewbox.Measure(new Size(120, 60));
        viewbox.Arrange(new Rect(0, 0, 120, 60));

        Assert.Same(child, viewbox.Child);
        Assert.Equal(Stretch.Uniform, viewbox.Stretch);
        Assert.Equal(StretchDirection.Both, viewbox.StretchDirection);
        Assert.Equal(1, viewbox.VisualChildrenCount);
    }

    [Fact]
    public void FWScrollViewer_ShouldTrackScrollOffsetsAndRaiseScrollChanged()
    {
        var content = new StackPanel
        {
            Orientation = Orientation.Vertical
        };
        for (var index = 0; index < 12; index++)
        {
            content.Children.Add(new Border
            {
                Height = 24,
                Child = new TextBlock { Text = $"Item {index}" }
            });
        }

        var viewer = new FWScrollViewer
        {
            Width = 120,
            Height = 72,
            Content = content,
            VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            IsScrollInertiaEnabled = false,
            IsScrollBarAutoHideEnabled = false
        };
        var changed = 0;
        ScrollChangedEventArgs? lastArgs = null;
        viewer.ScrollChanged += (_, args) =>
        {
            changed++;
            lastArgs = args;
        };

        viewer.Measure(new Size(120, 72));
        viewer.Arrange(new Rect(0, 0, 120, 72));
        viewer.ScrollToVerticalOffset(36);

        Assert.True(viewer.ExtentHeight > viewer.ViewportHeight);
        Assert.Equal(36, viewer.VerticalOffset);
        Assert.True(viewer.ScrollableHeight > 0);
        Assert.Equal(1, changed);
        Assert.NotNull(lastArgs);
        Assert.Equal(36, lastArgs!.VerticalOffset);
        Assert.True(lastArgs.VerticalChange > 0);
    }

    [Fact]
    public void FWSwipeControl_ShouldRetainSwipeItemsAndInvokeCommands()
    {
        var archiveCommand = new RecordingCommand();
        var deleteCommand = new RecordingCommand();
        var archiveItem = new SwipeItem
        {
            Text = "Archive",
            IconSource = "\uE8C3",
            Background = new SolidColorBrush(Color.FromRgb(0x10, 0x7C, 0x10)),
            Foreground = new SolidColorBrush(Color.White),
            Command = archiveCommand,
            CommandParameter = "message-1"
        };
        var deleteItem = new SwipeItem
        {
            Text = "Delete",
            IconSource = "\uE74D",
            BehaviorOnInvoked = BehaviorOnInvoked.Close,
            Command = deleteCommand,
            CommandParameter = "message-1"
        };
        var leftItems = new SwipeItems
        {
            Mode = SwipeMode.Reveal
        };
        leftItems.Add(archiveItem);
        var rightItems = new SwipeItems
        {
            Mode = SwipeMode.Execute
        };
        rightItems.Add(deleteItem);
        var swipe = new FWSwipeControl
        {
            LeftItems = leftItems,
            RightItems = rightItems,
            Content = new TextBlock { Text = "Message" },
            Background = new SolidColorBrush(Color.FromRgb(0x22, 0x22, 0x22)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x44)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6)
        };

        InvokeSwipeItem(archiveItem);
        InvokeSwipeItem(deleteItem);
        swipe.Close();

        Assert.Same(leftItems, swipe.LeftItems);
        Assert.Same(rightItems, swipe.RightItems);
        Assert.Equal(SwipeMode.Reveal, swipe.LeftItems!.Mode);
        Assert.Equal(SwipeMode.Execute, swipe.RightItems!.Mode);
        Assert.Equal("Archive", swipe.LeftItems[0].Text);
        Assert.Equal("\uE8C3", swipe.LeftItems[0].IconSource);
        Assert.Equal(BehaviorOnInvoked.Close, swipe.RightItems[0].BehaviorOnInvoked);
        Assert.Equal(1, archiveCommand.ExecuteCount);
        Assert.Equal("message-1", archiveCommand.LastParameter);
        Assert.Equal(1, deleteCommand.ExecuteCount);
    }

    [Fact]
    public void FWGridSplitter_ShouldExposeResizeDirectionBehaviorAndIncrements()
    {
        var splitter = new FWGridSplitter
        {
            ResizeDirection = GridResizeDirection.Columns,
            ResizeBehavior = GridResizeBehavior.PreviousAndNext,
            ShowsPreview = true,
            DragIncrement = 4,
            KeyboardIncrement = 16,
            Width = 6
        };

        splitter.Measure(new Size(6, 120));

        Assert.Equal(GridResizeDirection.Columns, splitter.ResizeDirection);
        Assert.Equal(GridResizeBehavior.PreviousAndNext, splitter.ResizeBehavior);
        Assert.True(splitter.ShowsPreview);
        Assert.Equal(4, splitter.DragIncrement);
        Assert.Equal(16, splitter.KeyboardIncrement);
        Assert.True(splitter.Focusable);
        Assert.Equal(6, splitter.DesiredSize.Width);
    }

    [Fact]
    public void FWTextBlockAndAccessText_ShouldExposeTextAndTypographyState()
    {
        var foreground = new SolidColorBrush(Color.FromRgb(0x4C, 0xC2, 0xFF));
        var selectionBrush = new SolidColorBrush(Color.FromArgb(0x66, 0x00, 0x78, 0xD4));
        var textBlock = new FWTextBlock
        {
            Text = "Selectable Fluent text",
            Foreground = foreground,
            SelectionBrush = selectionBrush,
            IsTextSelectionEnabled = true,
            TextWrapping = TextWrapping.Wrap,
            TextTrimming = TextTrimming.CharacterEllipsis,
            FontSize = 18,
            FontWeight = FontWeights.SemiBold
        };
        var accessText = new FWAccessText
        {
            Text = "_Open file",
            Foreground = foreground,
            TextWrapping = TextWrapping.NoWrap,
            TextTrimming = TextTrimming.WordEllipsis
        };

        Assert.Equal("Selectable Fluent text", textBlock.Text);
        Assert.Same(foreground, textBlock.Foreground);
        Assert.Same(selectionBrush, textBlock.SelectionBrush);
        Assert.True(textBlock.IsTextSelectionEnabled);
        Assert.Equal(TextWrapping.Wrap, textBlock.TextWrapping);
        Assert.Equal(TextTrimming.CharacterEllipsis, textBlock.TextTrimming);
        Assert.Equal(18, textBlock.FontSize);
        Assert.Equal(FontWeights.SemiBold, textBlock.FontWeight);
        Assert.Equal('O', accessText.AccessKey);
        Assert.Equal(TextTrimming.WordEllipsis, accessText.TextTrimming);
    }

    [Fact]
    public void FWBorderAndContentHosts_ShouldRetainChildAndContent()
    {
        var child = new FWTextBlock
        {
            Text = "Hosted content"
        };
        var border = new FWBorder
        {
            Background = new SolidColorBrush(Color.FromRgb(0x20, 0x20, 0x20)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x44)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12),
            Child = child
        };
        var contentControl = new FWContentControl
        {
            Content = "ContentControl text",
            Padding = new Thickness(8)
        };
        var presenterChild = new FWTextBlock { Text = "Presented" };
        var contentPresenter = new FWContentPresenter
        {
            Content = presenterChild
        };

        border.Measure(new Size(240, 120));
        contentControl.Measure(new Size(240, 120));
        contentPresenter.Measure(new Size(240, 120));

        Assert.Same(child, border.Child);
        Assert.Equal(1, border.BorderThickness.Left);
        Assert.Equal(6, border.CornerRadius.TopLeft);
        Assert.Equal(12, border.Padding.Left);
        Assert.Equal("ContentControl text", contentControl.Content);
        Assert.Equal(1, contentControl.VisualChildrenCount);
        Assert.Same(presenterChild, contentPresenter.Content);
        Assert.Equal(1, contentPresenter.VisualChildrenCount);
    }

    [Fact]
    public void FWTransitioningContentControl_ShouldExposeTransitionModeAndContent()
    {
        var transitionHost = new FWTransitioningContentControl
        {
            TransitionMode = Jalium.UI.Media.Animation.TransitionMode.SlideLeft,
            Content = "Slide content"
        };

        transitionHost.Measure(new Size(240, 120));

        Assert.Equal(Jalium.UI.Media.Animation.TransitionMode.SlideLeft, transitionHost.TransitionMode);
        Assert.Equal("Slide content", transitionHost.Content);

        transitionHost.TransitionMode = Jalium.UI.Media.Animation.TransitionMode.LiquidMorph;
        transitionHost.Content = new FWBorder { Width = 80, Height = 40 };

        Assert.Equal(Jalium.UI.Media.Animation.TransitionMode.LiquidMorph, transitionHost.TransitionMode);
        Assert.IsType<FWBorder>(transitionHost.Content);
    }

    [Fact]
    public void FWFluentMaterialSurface_ShouldMapMaterialKindsToJaliumEffects()
    {
        var surface = new FWFluentMaterialSurface
        {
            MaterialKind = FWFluentMaterialKind.Acrylic,
            TintColor = Color.FromArgb(180, 0, 120, 212),
            TintOpacity = 0.5,
            BlurRadius = 24,
            NoiseIntensity = 0.04
        };

        var acrylic = Assert.IsType<AcrylicEffect>(surface.BackdropEffect);
        Assert.False(surface.LiquidGlass);
        Assert.Equal(24f, acrylic.BlurRadius);
        Assert.Equal(0.5f, acrylic.TintOpacity);
        Assert.Equal(0.04f, acrylic.NoiseIntensity);

        surface.MaterialKind = FWFluentMaterialKind.LiquidGlass;
        surface.RefractionAmount = 88;
        surface.ChromaticAberration = 0.75;
        surface.FusionRadius = 44;

        Assert.True(surface.LiquidGlass);
        Assert.True(surface.LiquidGlassInteractive);
        Assert.Null(surface.BackdropEffect);
        Assert.Equal(24.0, surface.LiquidGlassBlurRadius);
        Assert.Equal(88.0, surface.LiquidGlassRefractionAmount);
        Assert.Equal(0.75, surface.LiquidGlassChromaticAberration);
        Assert.Equal(44, surface.LiquidGlassFusionRadius);
    }

    [Fact]
    public void FWPanelControls_ShouldMeasureChildrenAndExposeSpacing()
    {
        var stack = new FWStackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                new FWBorder { Width = 30, Height = 20 },
                new FWBorder { Width = 40, Height = 20 }
            }
        };
        var wrap = new FWWrapPanel
        {
            Width = 120,
            Orientation = Orientation.Horizontal,
            ItemWidth = 50,
            ItemHeight = 20,
            HorizontalSpacing = 6,
            VerticalSpacing = 4,
            Children =
            {
                new FWBorder(),
                new FWBorder(),
                new FWBorder()
            }
        };
        var grid = new FWGrid
        {
            RowSpacing = 5,
            ColumnSpacing = 7
        };
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(20) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
        grid.Children.Add(new FWBorder());

        stack.Measure(new Size(160, 40));
        wrap.Measure(new Size(120, 120));
        grid.Measure(new Size(120, 120));

        Assert.Equal(Orientation.Horizontal, stack.Orientation);
        Assert.Equal(8, stack.Spacing);
        Assert.Equal(2, stack.Children.Count);
        Assert.Equal(6, wrap.HorizontalSpacing);
        Assert.Equal(4, wrap.VerticalSpacing);
        Assert.Equal(3, wrap.Children.Count);
        Assert.Equal(5, grid.RowSpacing);
        Assert.Equal(7, grid.ColumnSpacing);
        Assert.Single(grid.RowDefinitions);
        Assert.Single(grid.ColumnDefinitions);
        Assert.Single(grid.Children);
    }

    [Fact]
    public void FWColorPicker_ShouldRaiseColorChangedAndTrackPickerOptions()
    {
        var picker = new FWColorPicker
        {
            Color = Color.FromRgb(0x00, 0x78, 0xD4),
            IsAlphaEnabled = true,
            IsColorPreviewVisible = true,
            IsHexInputVisible = true,
            IsCompact = true,
            ColorSpectrumShape = ColorSpectrumShape.Ring
        };
        var changed = 0;
        ColorChangedEventArgs? lastArgs = null;
        picker.ColorChanged += (_, args) =>
        {
            changed++;
            lastArgs = args;
        };

        var nextColor = Color.FromArgb(0xCC, 0xD8, 0x3B, 0x01);
        picker.Color = nextColor;

        Assert.Equal(nextColor, picker.Color);
        Assert.True(picker.IsAlphaEnabled);
        Assert.True(picker.IsColorPreviewVisible);
        Assert.True(picker.IsHexInputVisible);
        Assert.True(picker.IsCompact);
        Assert.Equal(ColorSpectrumShape.Ring, picker.ColorSpectrumShape);
        Assert.Equal(1, changed);
        Assert.NotNull(lastArgs);
        Assert.Equal(Color.FromRgb(0x00, 0x78, 0xD4), lastArgs!.OldColor);
        Assert.Equal(nextColor, lastArgs.NewColor);
    }

    [Fact]
    public void FWInkCanvas_ShouldTrackStrokeCollectionAndFluentSurface()
    {
        var background = new SolidColorBrush(Color.FromRgb(0x20, 0x20, 0x20));
        var borderBrush = new SolidColorBrush(Color.FromRgb(0x4C, 0xC2, 0xFF));
        var strokes = new StrokeCollection
        {
            CreateTestStroke(Color.FromRgb(0x4C, 0xC2, 0xFF))
        };
        var drawingAttributes = new DrawingAttributes
        {
            Color = Color.FromRgb(0x4C, 0xC2, 0xFF),
            Width = 5,
            Height = 5,
            BrushType = BrushType.Pen,
            FitToCurve = true
        };
        var canvas = new FWInkCanvas
        {
            Background = background,
            BorderBrush = borderBrush,
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(8),
            Strokes = strokes,
            DefaultDrawingAttributes = drawingAttributes,
            DefaultStrokeTaperMode = StrokeTaperMode.TaperedEnd,
            EditingMode = InkCanvasEditingMode.Ink,
            EraserDiameter = 18
        };
        var editingModeChanged = 0;
        var strokesChanged = 0;
        canvas.EditingModeChanged += (_, _) => editingModeChanged++;
        canvas.StrokesChanged += (_, _) => strokesChanged++;

        canvas.EditingMode = InkCanvasEditingMode.EraseByStroke;
        canvas.ClearStrokes();

        Assert.Same(background, canvas.Background);
        Assert.Same(borderBrush, canvas.BorderBrush);
        Assert.Equal(2, canvas.BorderThickness.Left);
        Assert.Equal(8, canvas.CornerRadius.TopLeft);
        Assert.Same(drawingAttributes, canvas.DefaultDrawingAttributes);
        Assert.Equal(StrokeTaperMode.TaperedEnd, canvas.DefaultStrokeTaperMode);
        Assert.Equal(InkCanvasEditingMode.EraseByStroke, canvas.EditingMode);
        Assert.Equal(18, canvas.EraserDiameter);
        Assert.Empty(canvas.Strokes);
        Assert.Equal(1, editingModeChanged);
        Assert.True(strokesChanged > 0);
    }

    [Fact]
    public void FWInkPresenter_ShouldRetainStrokeCollections()
    {
        var strokes = new StrokeCollection
        {
            CreateTestStroke(Color.FromRgb(0x00, 0x78, 0xD4)),
            CreateTestStroke(Color.FromRgb(0xD8, 0x3B, 0x01))
        };
        var presenter = new FWInkPresenter
        {
            Strokes = strokes
        };

        presenter.Measure(new Size(240, 120));

        Assert.Same(strokes, presenter.Strokes);
        Assert.Equal(2, presenter.Strokes!.Count);
        Assert.True(presenter.DesiredSize.Width >= 0);
    }

    [Fact]
    public void FWMediaElement_ShouldTrackPlaybackSettingsAndFluentSurface()
    {
        var background = new SolidColorBrush(Color.Black);
        var borderBrush = new SolidColorBrush(Color.FromRgb(0x60, 0x60, 0x60));
        using var element = new FWMediaElement
        {
            Background = background,
            BorderBrush = borderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Balance = -0.25,
            IsMuted = true,
            ScrubbingEnabled = true,
            Stretch = Stretch.UniformToFill,
            StretchDirection = StretchDirection.DownOnly,
            LoadedBehavior = MediaState.Manual,
            UnloadedBehavior = MediaState.Close,
            SpeedRatio = 1.25,
            Position = TimeSpan.FromSeconds(12)
        };

        element.Stop();

        Assert.Same(background, element.Background);
        Assert.Same(borderBrush, element.BorderBrush);
        Assert.Equal(1, element.BorderThickness.Left);
        Assert.Equal(6, element.CornerRadius.TopLeft);
        Assert.Equal(-0.25, element.Balance);
        Assert.True(element.IsMuted);
        Assert.True(element.ScrubbingEnabled);
        Assert.Equal(Stretch.UniformToFill, element.Stretch);
        Assert.Equal(StretchDirection.DownOnly, element.StretchDirection);
        Assert.Equal(MediaState.Manual, element.LoadedBehavior);
        Assert.Equal(MediaState.Close, element.UnloadedBehavior);
        Assert.Equal(1.25, element.SpeedRatio);
        Assert.Equal(TimeSpan.Zero, element.Position);
        Assert.False(element.IsPlaying);
    }

    [Fact]
    public void FWLabel_ShouldKeepTargetAccessKeyAndSelectionState()
    {
        var target = new FWTextBox();
        var label = new FWLabel
        {
            Content = "Name",
            Target = target,
            AccessKey = 'N',
            IsTextSelectionEnabled = true
        };

        Assert.Equal("Name", label.Content);
        Assert.Same(target, label.Target);
        Assert.Equal('N', label.AccessKey);
        Assert.True(label.IsTextSelectionEnabled);
        Assert.Equal(string.Empty, label.SelectedText);
    }

    [Fact]
    public void FWSeparator_ShouldTrackOrientationStrokeAndThickness()
    {
        var stroke = new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA));
        var separator = new FWSeparator
        {
            Orientation = Orientation.Vertical,
            StrokeBrush = stroke,
            StrokeThickness = 2
        };

        Assert.Equal(Orientation.Vertical, separator.Orientation);
        Assert.Same(stroke, separator.StrokeBrush);
        Assert.Equal(2, separator.StrokeThickness);
        Assert.False(separator.Focusable);
        Assert.False(separator.IsHitTestVisible);
    }

    [Fact]
    public void FluentRangeControls_ShouldExposeFwPrefixedSurface()
    {
        AssertFluentControl<FWSlider, Slider>();
        AssertFluentControl<FWRangeSlider, RangeSlider>();
        AssertFluentControl<FWProgressBar, ProgressBar>();
        AssertFluentControl<FWProgressRing, RangeBase>();
    }

    [Fact]
    public void FWRangeControls_ShouldCoerceValuesAndRaiseEvents()
    {
        var slider = new FWSlider
        {
            Minimum = 0,
            Maximum = 10
        };
        var sliderChanged = 0;
        slider.ValueChanged += (_, _) => sliderChanged++;

        slider.Value = 15;

        Assert.Equal(10, slider.Value);
        Assert.Equal(1, sliderChanged);

        var progressBar = new FWProgressBar
        {
            Minimum = 0,
            Maximum = 100
        };
        var progressChanged = 0;
        progressBar.ValueChanged += (_, _) => progressChanged++;

        progressBar.Value = 250;

        Assert.Equal(100, progressBar.Value);
        Assert.Equal(1, progressChanged);

        var startRangeSlider = new FWRangeSlider
        {
            Minimum = 0,
            Maximum = 100,
            RangeStart = 20,
            RangeEnd = 80,
            MinimumRange = 10
        };
        startRangeSlider.RangeStart = 95;
        Assert.Equal(70, startRangeSlider.RangeStart);

        var endRangeSlider = new FWRangeSlider
        {
            Minimum = 0,
            Maximum = 100,
            RangeStart = 90,
            RangeEnd = 100,
            MinimumRange = 10
        };
        endRangeSlider.RangeEnd = 50;
        Assert.Equal(100, endRangeSlider.RangeEnd);
    }

    [Fact]
    public void FWProgressRing_ShouldUseRangeStateAndProgressProperties()
    {
        var ring = new FWProgressRing
        {
            Minimum = 0,
            Maximum = 100,
            StrokeThickness = 6,
            IsIndeterminate = false
        };
        var changed = 0;
        ring.ValueChanged += (_, _) => changed++;

        ring.Value = 128;

        Assert.Equal(100, ring.Value);
        Assert.Equal(1, changed);
        Assert.Equal(6, ring.StrokeThickness);

        ring.IsActive = false;
        ring.IsIndeterminate = true;

        Assert.False(ring.IsActive);
        Assert.True(ring.IsIndeterminate);
    }

    [Fact]
    public void FWListBox_ShouldSynchronizeSelectionAndSelectedItems()
    {
        var listBox = new FWListBox();
        listBox.Items.Add("Inbox");
        listBox.Items.Add("Archive");
        listBox.Items.Add("Deleted");

        var selectionChanged = 0;
        listBox.SelectionChanged += (_, _) => selectionChanged++;

        listBox.SelectedIndex = 1;

        Assert.Equal(1, listBox.SelectedIndex);
        Assert.Equal("Archive", listBox.SelectedItem);
        Assert.Equal(1, selectionChanged);

        listBox.SelectionMode = SelectionMode.Multiple;
        listBox.SelectAll();

        Assert.Equal(3, listBox.SelectedItems.Count);

        listBox.UnselectAll();

        Assert.Empty(listBox.SelectedItems);
        Assert.Null(listBox.SelectedItem);
    }

    [Fact]
    public void FWListView_ShouldExposeGridViewColumnsAndSelection()
    {
        var items = new[]
        {
            new CollectionRow("Mail", "Unread", 12),
            new CollectionRow("Calendar", "Pinned", 3)
        };
        var view = new GridView();
        view.Columns.Add(new Jalium.UI.Controls.GridViewColumn
        {
            Header = "Name",
            DisplayMemberBinding = new Binding("Name"),
            Width = 140
        });
        view.Columns.Add(new Jalium.UI.Controls.GridViewColumn
        {
            Header = "State",
            DisplayMemberBinding = new Binding("State"),
            Width = 110
        });

        var listView = new FWListView
        {
            View = view,
            ItemsSource = items
        };

        listView.SelectedIndex = 1;

        Assert.Equal(2, view.Columns.Count);
        Assert.Equal("State", view.Columns[1].Header);
        Assert.Equal(items[1], listView.SelectedItem);
    }

    [Fact]
    public void FWTreeViewItem_ShouldExposeExpandAndSelectionState()
    {
        var root = new FWTreeViewItem
        {
            Header = "Root"
        };
        root.Items.Add(new FWTreeViewItem { Header = "Child" });

        var treeView = new FWTreeView();
        var selectedChanged = 0;
        treeView.SelectedItemChanged += (_, _) => selectedChanged++;
        treeView.Items.Add(root);

        root.IsExpanded = true;
        treeView.SelectedItem = root;

        Assert.True(root.IsExpanded);
        Assert.True(root.IsSelected);
        Assert.Equal(root, treeView.SelectedItem);
        Assert.Equal(1, selectedChanged);
    }

    [Fact]
    public void FWDataGrid_ShouldGenerateColumnsAndSynchronizeSelection()
    {
        var rows = new[]
        {
            new CollectionRow("Inbox", "Unread", 12),
            new CollectionRow("Archive", "Stored", 42)
        };
        var dataGrid = new FWDataGrid
        {
            ItemsSource = rows
        };
        var selectionChanged = 0;
        dataGrid.SelectionChanged += (_, _) => selectionChanged++;

        dataGrid.SelectedIndex = 1;

        Assert.True(dataGrid.Columns.Count >= 3);
        Assert.Equal(1, dataGrid.SelectedIndex);
        Assert.Equal(rows[1], dataGrid.SelectedItem);
        Assert.Contains(rows[1], dataGrid.SelectedItems);
        Assert.Equal(1, selectionChanged);
    }

    [Fact]
    public void FWTreeDataGrid_ShouldExpandCollapseAndSynchronizeSelection()
    {
        var rows = new[]
        {
            new CollectionNode(
                "Workspace",
                "Open",
                [
                    new CollectionNode("Docs", "Synced", []),
                    new CollectionNode("Design", "Review", [])
                ]),
            new CollectionNode("Archive", "Closed", [])
        };
        var grid = new FWTreeDataGrid
        {
            ChildrenSelector = item => ((CollectionNode)item).Children,
            HasChildrenSelector = item => ((CollectionNode)item).Children.Length > 0
        };
        grid.ItemsSource = rows;
        grid.Columns.Add(new DataGridTextColumn
        {
            Header = "Name",
            Binding = new Binding("Name"),
            Width = 160
        });
        grid.Columns.Add(new DataGridTextColumn
        {
            Header = "State",
            Binding = new Binding("State"),
            Width = 120
        });

        Assert.Equal(2, grid.FlattenedCount);

        grid.ExpandAll();
        grid.SelectedIndex = 1;

        Assert.Equal(4, grid.FlattenedCount);
        Assert.Equal(rows[0].Children[0], grid.SelectedItem);
        Assert.Contains(rows[0].Children[0], grid.SelectedItems);

        grid.CollapseAll();

        Assert.Equal(2, grid.FlattenedCount);
    }

    [Fact]
    public void FWNavigationView_ShouldSynchronizeSelectionPaneAndItemEvents()
    {
        var navigationView = new FWNavigationView();
        var first = new FWNavigationViewItem { Content = "Home" };
        var second = new FWNavigationViewItem { Content = "Settings" };
        var invoked = 0;
        var itemInvoked = 0;
        var selectionChanged = 0;
        var opened = 0;
        var closed = 0;

        first.Invoked += (_, _) => invoked++;
        second.Invoked += (_, _) => invoked++;
        navigationView.ItemInvoked += (_, _) => itemInvoked++;
        navigationView.SelectionChanged += (_, _) => selectionChanged++;
        navigationView.PaneOpened += (_, _) => opened++;
        navigationView.PaneClosed += (_, _) => closed++;
        navigationView.MenuItems.Add(first);
        navigationView.MenuItems.Add(second);

        navigationView.SelectedItem = first;
        navigationView.SelectedItem = second;
        navigationView.IsPaneOpen = false;
        navigationView.IsPaneOpen = true;

        Assert.False(first.IsSelected);
        Assert.True(second.IsSelected);
        Assert.Equal(second, navigationView.SelectedItem);
        Assert.Equal(2, invoked);
        Assert.Equal(2, itemInvoked);
        Assert.Equal(2, selectionChanged);
        Assert.Equal(1, closed);
        Assert.Equal(1, opened);
    }

    [Fact]
    public void FWNavigationViewItem_ShouldExposeExpansionAndHierarchyState()
    {
        var item = new FWNavigationViewItem
        {
            Content = "Controls"
        };
        item.MenuItems.Add(new FWNavigationViewItem { Content = "Buttons" });

        var expansionChanged = 0;
        item.ExpansionChanged += (_, expanded) =>
        {
            expansionChanged++;
            Assert.True(expanded);
        };

        item.IsExpanded = true;

        Assert.True(item.HasUnrealizedChildren);
        Assert.True(item.IsExpanded);
        Assert.Equal(1, expansionChanged);
    }

    [Fact]
    public void FWTabControl_ShouldSynchronizeSelectedContentAndTabItems()
    {
        var first = new FWTabItem
        {
            Header = "Overview",
            Content = "First content"
        };
        var second = new FWTabItem
        {
            Header = "Details",
            Content = "Second content"
        };
        var tabControl = new FWTabControl();
        var selectionChanged = 0;
        tabControl.SelectionChanged += (_, _) => selectionChanged++;
        tabControl.Items.Add(first);
        tabControl.Items.Add(second);

        tabControl.SelectedIndex = 1;

        Assert.False(first.IsSelected);
        Assert.True(second.IsSelected);
        Assert.Equal(second, tabControl.SelectedItem);
        Assert.Equal("Second content", tabControl.SelectedContent);
        Assert.Equal(2, selectionChanged);

        tabControl.SelectedIndex = 0;

        Assert.True(first.IsSelected);
        Assert.False(second.IsSelected);
        Assert.Equal("First content", tabControl.SelectedContent);
        Assert.Equal(3, selectionChanged);
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises Frame page activation by type.")]
    public void FWFrame_ShouldNavigateBackAndForward()
    {
        var frame = new FWFrame();
        var navigated = 0;
        frame.Navigated += (_, _) => navigated++;

        Assert.True(frame.Navigate(typeof(NavigationTestPage), "first"));
        Assert.Equal(typeof(NavigationTestPage), frame.SourcePageType);
        Assert.IsType<NavigationTestPage>(frame.CurrentPage);
        Assert.Equal("first", frame.CurrentPage!.NavigationParameter);

        Assert.True(frame.Navigate(typeof(SecondNavigationTestPage), "second"));
        Assert.True(frame.CanGoBack);
        Assert.Equal(1, frame.BackStackDepth);
        Assert.Equal(typeof(SecondNavigationTestPage), frame.SourcePageType);

        Assert.True(frame.GoBack());
        Assert.True(frame.CanGoForward);
        Assert.Equal(typeof(NavigationTestPage), frame.SourcePageType);
        Assert.Equal("first", frame.CurrentPage!.NavigationParameter);

        Assert.True(frame.GoForward());
        Assert.Equal(typeof(SecondNavigationTestPage), frame.SourcePageType);
        Assert.Equal("second", frame.CurrentPage!.NavigationParameter);
        Assert.Equal(4, navigated);
    }

    [Fact]
    public void FWMenu_ShouldGenerateFwMenuItemContainers()
    {
        var menu = new FWMenu();
        menu.Items.Add("File");
        menu.Items.Add(new FWMenuItem { Header = "Edit" });

        menu.Measure(new Size(240, 32));

        Assert.Equal(1, menu.VisualChildrenCount);
        var host = Assert.IsAssignableFrom<Panel>(menu.GetVisualChild(0));
        Assert.Equal(2, host.Children.Count);

        var generated = Assert.IsType<FWMenuItem>(host.Children[0]);
        Assert.Equal("File", generated.Header);
        Assert.IsType<FWMenuItem>(host.Children[1]);
    }

    [Fact]
    public void FWMenuItem_ShouldToggleCheckedStateAndRaiseSubmenuEvents()
    {
        var item = new FWMenuItem
        {
            Header = "Live preview",
            IsCheckable = true
        };
        var checkedCount = 0;
        var uncheckedCount = 0;
        var opened = 0;
        var closed = 0;

        item.Checked += (_, _) => checkedCount++;
        item.Unchecked += (_, _) => uncheckedCount++;
        item.SubmenuOpened += (_, _) => opened++;
        item.SubmenuClosed += (_, _) => closed++;
        item.Items.Add("Nested command");

        item.IsChecked = true;
        item.IsChecked = false;
        item.IsSubmenuOpen = true;
        item.IsSubmenuOpen = false;

        Assert.False(item.IsChecked);
        Assert.Equal(1, checkedCount);
        Assert.Equal(1, uncheckedCount);
        Assert.False(item.IsSubmenuOpen);
        Assert.Equal(1, opened);
        Assert.Equal(1, closed);
    }

    [Fact]
    public void FWMenuBarItem_ShouldOpenAndCloseMenu()
    {
        var menuBar = new FWMenuBar();
        var file = new FWMenuBarItem
        {
            Title = "File"
        };
        file.Items.Add(new FWMenuFlyoutItem { Text = "New" });
        menuBar.Items.Add(file);

        file.OpenMenu();
        Assert.True(file.IsMenuOpen);

        file.CloseMenu();
        Assert.False(file.IsMenuOpen);
        Assert.Contains(file, menuBar.Items);
    }

    [Fact]
    public void FWContextMenu_ShouldOpenCloseAndKeepPlacementState()
    {
        var owner = new FWButton { Content = "Target" };
        var menu = new FWContextMenu
        {
            PlacementTarget = owner,
            Placement = PlacementMode.Bottom,
            HorizontalOffset = 4,
            VerticalOffset = 8,
            StaysOpen = true
        };
        var opened = 0;
        var closed = 0;
        menu.Opened += (_, _) => opened++;
        menu.Closed += (_, _) => closed++;
        menu.Items.Add(new FWMenuItem { Header = "Refresh" });

        menu.IsOpen = true;
        menu.Close();

        Assert.False(menu.IsOpen);
        Assert.Same(owner, menu.PlacementTarget);
        Assert.Equal(PlacementMode.Bottom, menu.Placement);
        Assert.Equal(4, menu.HorizontalOffset);
        Assert.Equal(8, menu.VerticalOffset);
        Assert.True(menu.StaysOpen);
        Assert.Equal(1, opened);
        Assert.Equal(1, closed);
    }

    [Fact]
    public void FWMenuFlyoutItems_ShouldInvokeCommandsAndToggleState()
    {
        var command = new RecordingCommand();
        var item = new FWMenuFlyoutItem
        {
            Text = "Open",
            Icon = "\uE8A5",
            Command = command,
            CommandParameter = "file",
            KeyboardAcceleratorTextOverride = "Ctrl+O"
        };
        var clicked = 0;
        item.Click += (_, _) => clicked++;

        InvokeMenuFlyoutItem(item);

        Assert.Equal("Open", item.Text);
        Assert.Equal("\uE8A5", item.Icon);
        Assert.Equal("Ctrl+O", item.KeyboardAcceleratorTextOverride);
        Assert.Equal(1, clicked);
        Assert.Equal(1, command.ExecuteCount);
        Assert.Equal("file", command.LastParameter);

        var toggle = new FWToggleMenuFlyoutItem { Text = "Show badges" };

        InvokeMenuFlyoutItem(toggle);

        Assert.True(toggle.IsChecked);
    }

    [Fact]
    public void FWExpander_ShouldRaiseExpandCollapseEventsAndKeepHeaderState()
    {
        var expander = new FWExpander
        {
            Header = "Advanced options",
            Content = new TextBlock { Text = "Nested content" },
            ExpandDirection = ExpandDirection.Down
        };
        var expanded = 0;
        var collapsed = 0;
        expander.Expanded += (_, _) => expanded++;
        expander.Collapsed += (_, _) => collapsed++;

        expander.IsExpanded = true;
        expander.IsExpanded = false;

        Assert.False(expander.IsExpanded);
        Assert.Equal("Advanced options", expander.Header);
        Assert.Equal(ExpandDirection.Down, expander.ExpandDirection);
        Assert.Equal(1, expanded);
        Assert.Equal(1, collapsed);
    }

    [Fact]
    public void FWToolTip_ShouldRaiseOpenCloseEventsAndPreservePlacementState()
    {
        var owner = new FWButton { Content = "Target" };
        var toolTip = new FWToolTip
        {
            Content = "Details",
            PlacementTarget = owner,
            Placement = PlacementMode.Bottom,
            HorizontalOffset = 6,
            VerticalOffset = 10,
            InitialShowDelay = 150,
            ShowDuration = int.MaxValue
        };
        var opened = 0;
        var closed = 0;
        toolTip.Opened += (_, _) => opened++;
        toolTip.Closed += (_, _) => closed++;

        toolTip.IsOpen = true;
        toolTip.IsOpen = false;

        Assert.False(toolTip.IsOpen);
        Assert.Same(owner, toolTip.PlacementTarget);
        Assert.Equal(PlacementMode.Bottom, toolTip.Placement);
        Assert.Equal(6, toolTip.HorizontalOffset);
        Assert.Equal(10, toolTip.VerticalOffset);
        Assert.Equal(150, toolTip.InitialShowDelay);
        Assert.Equal(int.MaxValue, toolTip.ShowDuration);
        Assert.Equal(1, opened);
        Assert.Equal(1, closed);
    }

    [Fact]
    public void FWGroupBox_ShouldTrackHeaderBackgroundAndContent()
    {
        var headerBackground = new SolidColorBrush(Color.FromRgb(0x22, 0x22, 0x22));
        var content = new StackPanel();
        var groupBox = new FWGroupBox
        {
            Header = "Settings",
            HeaderBackground = headerBackground,
            Content = content,
            Padding = new Thickness(12)
        };

        Assert.Equal("Settings", groupBox.Header);
        Assert.Same(headerBackground, groupBox.HeaderBackground);
        Assert.Same(content, groupBox.Content);
        Assert.Equal(12, groupBox.Padding.Left);
    }

    [Fact]
    public void FWContentDialog_ShouldExposeButtonConfigurationAndCommands()
    {
        var primaryCommand = new RecordingCommand();
        var secondaryCommand = new RecordingCommand();
        var closeCommand = new RecordingCommand();
        var dialog = new FWContentDialog
        {
            Title = "Replace file?",
            Content = "A file with this name already exists.",
            PrimaryButtonText = "Replace",
            SecondaryButtonText = "Keep both",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            IsPrimaryButtonEnabled = false,
            IsSecondaryButtonEnabled = true,
            PrimaryButtonCommand = primaryCommand,
            PrimaryButtonCommandParameter = "replace",
            SecondaryButtonCommand = secondaryCommand,
            SecondaryButtonCommandParameter = "keep",
            CloseButtonCommand = closeCommand,
            CloseButtonCommandParameter = "cancel",
            FullSizeDesired = true
        };

        Assert.Equal("Replace file?", dialog.Title);
        Assert.Equal("A file with this name already exists.", dialog.Content);
        Assert.Equal("Replace", dialog.PrimaryButtonText);
        Assert.Equal("Keep both", dialog.SecondaryButtonText);
        Assert.Equal("Cancel", dialog.CloseButtonText);
        Assert.Equal(ContentDialogButton.Primary, dialog.DefaultButton);
        Assert.False(dialog.IsPrimaryButtonEnabled);
        Assert.True(dialog.IsSecondaryButtonEnabled);
        Assert.Same(primaryCommand, dialog.PrimaryButtonCommand);
        Assert.Equal("replace", dialog.PrimaryButtonCommandParameter);
        Assert.Same(secondaryCommand, dialog.SecondaryButtonCommand);
        Assert.Equal("keep", dialog.SecondaryButtonCommandParameter);
        Assert.Same(closeCommand, dialog.CloseButtonCommand);
        Assert.Equal("cancel", dialog.CloseButtonCommandParameter);
        Assert.True(dialog.FullSizeDesired);
    }

    [Fact]
    public void FWDatePicker_ShouldRaiseSelectionAndCalendarOpenCloseEvents()
    {
        var picker = new FWDatePicker
        {
            Header = "Date",
            PlaceholderText = "Select a date",
            DisplayDate = new DateTime(2026, 6, 1),
            DisplayDateStart = new DateTime(2026, 1, 1),
            DisplayDateEnd = new DateTime(2026, 12, 31),
            SelectedDateFormat = DatePickerFormat.Long
        };
        var selectedChanged = 0;
        var opened = 0;
        var closed = 0;
        SelectionChangedEventArgs? lastSelectionArgs = null;

        picker.SelectedDateChanged += (_, args) =>
        {
            selectedChanged++;
            lastSelectionArgs = args;
        };
        picker.CalendarOpened += (_, _) => opened++;
        picker.CalendarClosed += (_, _) => closed++;

        picker.SelectedDate = new DateTime(2026, 6, 3);
        picker.IsDropDownOpen = true;
        picker.IsDropDownOpen = false;

        Assert.Equal(new DateTime(2026, 6, 3), picker.SelectedDate);
        Assert.Equal(DatePickerFormat.Long, picker.SelectedDateFormat);
        Assert.Equal(1, selectedChanged);
        Assert.NotNull(lastSelectionArgs);
        Assert.Contains(new DateTime(2026, 6, 3), lastSelectionArgs!.AddedItems);
        Assert.Equal(1, opened);
        Assert.Equal(1, closed);
    }

    [Fact]
    public void FWTimePicker_ShouldRaiseSelectedTimeChangedAndHonorClockIdentifier()
    {
        var picker = new FWTimePicker
        {
            Header = "Time",
            ClockIdentifier = "24HourClock",
            MinuteIncrement = 15,
            PlaceholderText = "Select a time"
        };
        var changed = 0;
        TimePickerSelectedValueChangedEventArgs? lastArgs = null;
        picker.SelectedTimeChanged += (_, args) =>
        {
            changed++;
            lastArgs = args;
        };

        picker.SelectedTime = new TimeSpan(18, 45, 0);

        Assert.Equal("24HourClock", picker.ClockIdentifier);
        Assert.Equal(15, picker.MinuteIncrement);
        Assert.Equal(new TimeSpan(18, 45, 0), picker.SelectedTime);
        Assert.Equal(1, changed);
        Assert.NotNull(lastArgs);
        Assert.Null(lastArgs!.OldTime);
        Assert.Equal(new TimeSpan(18, 45, 0), lastArgs.NewTime);
    }

    [Fact]
    public void FWCalendar_ShouldTrackSelectionDisplayDateAndSelectableBounds()
    {
        var calendar = new FWCalendar
        {
            DisplayDate = new DateTime(2026, 6, 1),
            DisplayDateStart = new DateTime(2026, 6, 1),
            DisplayDateEnd = new DateTime(2026, 6, 30),
            FirstDayOfWeek = DayOfWeek.Monday,
            IsTodayHighlighted = true,
            SelectionMode = CalendarSelectionMode.SingleDate
        };
        var selectedChanged = 0;
        var displayChanged = 0;
        SelectionChangedEventArgs? lastSelectionArgs = null;
        CalendarDateChangedEventArgs? lastDisplayArgs = null;
        calendar.SelectedDateChanged += (_, args) =>
        {
            selectedChanged++;
            lastSelectionArgs = args;
        };
        calendar.DisplayDateChanged += (_, args) =>
        {
            displayChanged++;
            lastDisplayArgs = args;
        };

        calendar.BlackoutDates.Add(new DateTime(2026, 6, 10));

        InvokeCalendarSelectDate(calendar, new DateTime(2026, 6, 8));
        InvokeCalendarSelectDate(calendar, new DateTime(2026, 6, 10));
        InvokeCalendarSelectDate(calendar, new DateTime(2026, 7, 1));
        calendar.DisplayDate = new DateTime(2026, 7, 1);

        Assert.Equal(new DateTime(2026, 6, 8), calendar.SelectedDate);
        Assert.Single(calendar.SelectedDates);
        Assert.Equal(new DateTime(2026, 6, 8), calendar.SelectedDates[0]);
        Assert.Contains(new DateTime(2026, 6, 10), calendar.BlackoutDates);
        Assert.Equal(CalendarSelectionMode.SingleDate, calendar.SelectionMode);
        Assert.Equal(DayOfWeek.Monday, calendar.FirstDayOfWeek);
        Assert.Equal(1, selectedChanged);
        Assert.NotNull(lastSelectionArgs);
        Assert.Contains(new DateTime(2026, 6, 8), lastSelectionArgs!.AddedItems);
        Assert.Equal(1, displayChanged);
        Assert.NotNull(lastDisplayArgs);
        Assert.Equal(new DateTime(2026, 6, 1), lastDisplayArgs!.RemovedDate);
        Assert.Equal(new DateTime(2026, 7, 1), lastDisplayArgs.AddedDate);
    }

    [Fact]
    public void FWInfoBar_ShouldRaiseClosedWhenIsOpenChangesAndKeepSeverityState()
    {
        var infoBar = new FWInfoBar
        {
            Title = "Saved",
            Message = "Changes were applied.",
            Severity = InfoBarSeverity.Success,
            IsClosable = true,
            IsIconVisible = true
        };
        var closed = 0;
        infoBar.Closed += (_, _) => closed++;

        infoBar.IsOpen = false;

        Assert.False(infoBar.IsOpen);
        Assert.Equal(InfoBarSeverity.Success, infoBar.Severity);
        Assert.Equal("Saved", infoBar.Title);
        Assert.Equal("Changes were applied.", infoBar.Message);
        Assert.Equal(1, closed);
    }

    [Fact]
    public void FWInfoBadge_ShouldSwitchDisplayKindAndClampDisplayValue()
    {
        var badge = new FWInfoBadge();

        Assert.Equal(FWInfoBadgeDisplayKind.Dot, badge.DisplayKind);
        Assert.Equal(string.Empty, badge.DisplayValueText);

        badge.IconGlyph = "\uE7BA";

        Assert.Equal(FWInfoBadgeDisplayKind.Icon, badge.DisplayKind);

        badge.Value = 128;
        badge.MaxValue = 99;
        badge.Severity = FWInfoBadgeSeverity.Critical;

        Assert.Equal(FWInfoBadgeDisplayKind.Value, badge.DisplayKind);
        Assert.Equal("99+", badge.DisplayValueText);
        Assert.Equal(FWInfoBadgeSeverity.Critical, badge.Severity);

        badge.Value = 8;

        Assert.Equal("8", badge.DisplayValueText);
    }

    [Fact]
    public void FWToastNotificationItem_ShouldExposeSeverityDurationAndClosedEvent()
    {
        var toast = new FWToastNotificationItem
        {
            Title = "Build complete",
            Message = "Static toast",
            Severity = ToastSeverity.Warning,
            Duration = TimeSpan.FromSeconds(7),
            IsAutoDismissEnabled = false
        };
        var closed = 0;
        toast.Closed += (_, _) => closed++;

        toast.IsOpen = false;

        Assert.False(toast.IsOpen);
        Assert.Equal(ToastSeverity.Warning, toast.Severity);
        Assert.Equal(TimeSpan.FromSeconds(7), toast.Duration);
        Assert.False(toast.IsAutoDismissEnabled);
        Assert.Equal(1, closed);
    }

    [Fact]
    public void FWToastNotificationHost_ShouldShowFwItemsAndLimitVisibleToasts()
    {
        var host = new FWToastNotificationHost
        {
            MaxVisibleToasts = 2,
            Position = ToastPosition.BottomCenter,
            ToastWidth = 320,
            Spacing = 12
        };

        var first = host.ShowInformation("First", "Removed when the limit is exceeded", TimeSpan.FromSeconds(1));
        var second = host.ShowSuccess("Second", "Kept", TimeSpan.FromSeconds(2));
        var third = host.ShowError("Third", "Kept", TimeSpan.FromSeconds(3));

        Assert.IsType<FWToastNotificationItem>(first);
        Assert.Equal(2, host.Children.Count);
        Assert.DoesNotContain(first, host.Children);
        Assert.Contains(second, host.Children);
        Assert.Contains(third, host.Children);
        Assert.False(first.IsOpen);
        Assert.Equal(ToastPosition.BottomCenter, host.Position);
        Assert.Equal(320, host.ToastWidth);
        Assert.Equal(12, host.Spacing);
    }

    [Fact]
    public void FWStatusBar_ShouldHostStatusItemsAndExposeSeparatorBrush()
    {
        var separatorBrush = new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA));
        var statusBar = new FWStatusBar
        {
            SeparatorBrush = separatorBrush
        };
        statusBar.Items.Add("Ready");
        statusBar.Items.Add(new FWStatusBarItem { Content = "UTF-8" });

        statusBar.Measure(new Size(320, 24));

        Assert.Equal(separatorBrush, statusBar.SeparatorBrush);
        Assert.Equal(2, statusBar.Items.Count);
        Assert.Equal(1, statusBar.VisualChildrenCount);

        var itemsHost = Assert.IsAssignableFrom<Panel>(statusBar.GetVisualChild(0));
        Assert.Equal(2, itemsHost.Children.Count);
        Assert.IsType<FWStatusBarItem>(itemsHost.Children[0]);
        Assert.IsType<FWStatusBarItem>(itemsHost.Children[1]);
    }

    [Fact]
    public void FWDropDownButton_ShouldSynchronizeFlyoutOpenState()
    {
        var oldFlyout = new MenuFlyout();
        var newFlyout = new MenuFlyout();
        var button = new FWDropDownButton
        {
            Flyout = oldFlyout
        };

        var opened = 0;
        var closed = 0;
        button.FlyoutOpened += (_, _) => opened++;
        button.FlyoutClosed += (_, _) => closed++;

        oldFlyout.ShowAt(button);
        Assert.True(button.IsFlyoutOpen);
        Assert.Equal(1, opened);

        button.Flyout = newFlyout;
        Assert.False(button.IsFlyoutOpen);
        Assert.False(oldFlyout.IsOpen);
        Assert.Equal(1, closed);

        newFlyout.ShowAt(button);
        Assert.True(button.IsFlyoutOpen);
        Assert.Equal(2, opened);

        newFlyout.Hide();
        Assert.False(button.IsFlyoutOpen);
        Assert.Equal(2, closed);
    }

    [Fact]
    public void FWToggleSplitButton_ShouldToggleCheckedStateAndRaiseEvent()
    {
        var button = new FWToggleSplitButton();
        FWToggleSplitButtonIsCheckedChangedEventArgs? lastArgs = null;
        var changed = 0;
        var clicked = 0;

        button.IsCheckedChanged += (_, args) =>
        {
            changed++;
            lastArgs = args;
        };
        button.Click += (_, _) => clicked++;

        button.Toggle();

        Assert.True(button.IsChecked);
        Assert.Equal(1, changed);
        Assert.NotNull(lastArgs);
        Assert.False(lastArgs!.OldValue);
        Assert.True(lastArgs.NewValue);

        InvokeSplitButtonClick(button);

        Assert.False(button.IsChecked);
        Assert.Equal(2, changed);
        Assert.Equal(1, clicked);
        Assert.True(lastArgs.OldValue);
        Assert.False(lastArgs.NewValue);
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void FluentControls_ShouldInheritJaliumImplicitStylesByBaseType()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);
            var button = new FWButton();

            var lookupStyle = typeof(FrameworkElement)
                .GetMethod("LookupImplicitStyle", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(button, null);

            var fwStyle = Assert.IsType<Style>(lookupStyle);
            Assert.Equal(typeof(FWButton), fwStyle.TargetType);
            Assert.IsType<Style>(fwStyle.BasedOn);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    private static void AssertContainsStyle<TControl>(ResourceDictionary dictionary)
        where TControl : FrameworkElement
    {
        Assert.True(dictionary.TryGetValue(typeof(TControl), out var value), $"{typeof(TControl).Name} style was not found.");
        Assert.IsType<Style>(value);
    }

    private static ResourceDictionary LoadEntryDictionary(FluentJaliumDictionary dictionary)
    {
        Assert.NotNull(dictionary.Source);
        var loaded = ResourceDictionary.SourceLoader?.Invoke(
            new ResourceDictionary(),
            dictionary.Source!,
            FluentThemeManager.ThemeAssembly);

        return Assert.IsType<ResourceDictionary>(loaded);
    }

    private static void AssertBasedOnStyle<TFluentControl, TJaliumControl>(ResourceDictionary dictionary)
        where TFluentControl : TJaliumControl, IFluentJaliumControl
        where TJaliumControl : FrameworkElement
    {
        Assert.True(dictionary.TryGetValue(typeof(TJaliumControl), out var baseValue), $"{typeof(TJaliumControl).Name} base style was not found.");
        var baseStyle = Assert.IsType<Style>(baseValue);

        Assert.True(dictionary.TryGetValue(typeof(TFluentControl), out var fluentValue), $"{typeof(TFluentControl).Name} FW style was not found.");
        var fluentStyle = Assert.IsType<Style>(fluentValue);

        Assert.Equal(typeof(TFluentControl), fluentStyle.TargetType);
        Assert.Same(baseStyle, fluentStyle.BasedOn);
    }

    private static void AssertOwnedStyle<TFluentControl>(ResourceDictionary dictionary)
        where TFluentControl : FrameworkElement, IFluentJaliumControl
    {
        Assert.True(dictionary.TryGetValue(typeof(TFluentControl), out var fluentValue), $"{typeof(TFluentControl).Name} FW style was not found.");
        var fluentStyle = Assert.IsType<Style>(fluentValue);

        Assert.Equal(typeof(TFluentControl), fluentStyle.TargetType);
        Assert.Null(fluentStyle.BasedOn);
    }

    private static void AssertStyleSetterValue(Style style, DependencyProperty property, object? expectedValue)
    {
        var setter = style.Setters.OfType<Setter>().FirstOrDefault(candidate => candidate.Property == property);

        Assert.NotNull(setter);

        if (setter!.Value is IDynamicResourceReference dynamicReference)
        {
            Assert.Equal(expectedValue, dynamicReference.ResourceKey);
            return;
        }

        Assert.Equal(expectedValue, setter.Value);
    }

    private static void AssertFluentControl<TFluentControl, TJaliumControl>()
        where TFluentControl : TJaliumControl, IFluentJaliumControl, new()
        where TJaliumControl : FrameworkElement
    {
        var control = new TFluentControl();
        Assert.IsAssignableFrom<TJaliumControl>(control);
        Assert.StartsWith("FW", typeof(TFluentControl).Name, StringComparison.Ordinal);
    }

    private static void InvokeToggleButtonClick(ToggleButton button)
    {
        typeof(ToggleButton)
            .GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(button, null);
    }

    private static void InvokeSplitButtonClick(SplitButton button)
    {
        typeof(SplitButton)
            .GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(button, [new SplitButtonClickEventArgs()]);
    }

    private static void InvokeMenuFlyoutItem(MenuFlyoutItem item)
    {
        typeof(MenuFlyoutItem)
            .GetMethod("InvokeItem", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(item, null);
    }

    private static void InvokeSwipeItem(SwipeItem item)
    {
        typeof(SwipeItem)
            .GetMethod("RaiseInvoked", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(item, null);
    }

    private static void InvokeCalendarSelectDate(Calendar calendar, DateTime date)
    {
        typeof(Calendar)
            .GetMethod("SelectDate", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(calendar, [date]);
    }

    private static Stroke CreateTestStroke(Color color)
    {
        return new Stroke(
            new StylusPointCollection(
            [
                new StylusPoint(12, 24),
                new StylusPoint(36, 48),
                new StylusPoint(72, 32)
            ]),
            new DrawingAttributes
            {
                Color = color,
                Width = 4,
                Height = 4,
                BrushType = BrushType.Pen,
                FitToCurve = true
            })
        {
            TaperMode = StrokeTaperMode.TaperedEnd
        };
    }

    private static Color GetBrushColor(object? value)
    {
        return Assert.IsType<SolidColorBrush>(value).Color;
    }

    private static byte[] CreateSamplePixels(int width, int height)
    {
        var pixels = new byte[width * height * 4];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var index = ((y * width) + x) * 4;
                pixels[index] = (byte)(0x40 + x);
                pixels[index + 1] = (byte)(0x80 + y);
                pixels[index + 2] = 0xC0;
                pixels[index + 3] = 0xFF;
            }
        }

        return pixels;
    }

    private sealed record CollectionRow(string Name, string State, int Count);

    private sealed record CollectionNode(string Name, string State, CollectionNode[] Children);

    private sealed class NavigationTestPage : Page
    {
    }

    private sealed class SecondNavigationTestPage : Page
    {
    }

    private sealed class RecordingCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public int ExecuteCount { get; private set; }

        public object? LastParameter { get; private set; }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            ExecuteCount++;
            LastParameter = parameter;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private static void ResetApplicationState()
    {
        var currentField = typeof(Application).GetField("_current", BindingFlags.NonPublic | BindingFlags.Static);
        currentField?.SetValue(null, null);

        var jaliumReset = typeof(JaliumThemeManager).GetMethod("Reset", BindingFlags.NonPublic | BindingFlags.Static);
        jaliumReset?.Invoke(null, null);

        var fluentReset = typeof(FluentThemeManager).GetMethod("Reset", BindingFlags.NonPublic | BindingFlags.Static);
        fluentReset?.Invoke(null, null);
    }
}
