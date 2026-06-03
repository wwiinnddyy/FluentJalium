using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
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
            AssertBasedOnStyle<FWComboBox, ComboBox>(app.Resources);
            AssertOwnedStyle<FWDropDownButton>(app.Resources);
            AssertOwnedStyle<FWToggleSplitButton>(app.Resources);
            AssertOwnedStyle<FWProgressRing>(app.Resources);
            AssertOwnedStyle<FWInfoBadge>(app.Resources);
            AssertOwnedStyle<FWRatingControl>(app.Resources);
            AssertBasedOnStyle<FWInfoBar, InfoBar>(app.Resources);
            AssertBasedOnStyle<FWNumberBox, NumberBox>(app.Resources);
            AssertBasedOnStyle<FWSplitButton, SplitButton>(app.Resources);
            AssertBasedOnStyle<FWExpander, Expander>(app.Resources);
            AssertBasedOnStyle<FWCommandBar, CommandBar>(app.Resources);
            AssertBasedOnStyle<FWAppBarButton, AppBarButton>(app.Resources);
            AssertBasedOnStyle<FWAppBarToggleButton, AppBarToggleButton>(app.Resources);
            AssertBasedOnStyle<FWAppBarSeparator, AppBarSeparator>(app.Resources);
            AssertBasedOnStyle<FWNavigationView, NavigationView>(app.Resources);
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
        AssertContainsStyle<CheckBox>(dictionary);
        AssertContainsStyle<RadioButton>(dictionary);
        AssertContainsStyle<ToggleButton>(dictionary);
        AssertContainsStyle<ToggleSwitch>(dictionary);
        AssertContainsStyle<Slider>(dictionary);
        AssertContainsStyle<ProgressBar>(dictionary);
        AssertContainsStyle<ComboBox>(dictionary);
        AssertContainsStyle<FWDropDownButton>(dictionary);
        AssertContainsStyle<FWToggleSplitButton>(dictionary);
        AssertContainsStyle<FWProgressRing>(dictionary);
        AssertContainsStyle<FWInfoBadge>(dictionary);
        AssertContainsStyle<FWRatingControl>(dictionary);
        AssertContainsStyle<InfoBar>(dictionary);
        AssertContainsStyle<NumberBox>(dictionary);
        AssertContainsStyle<SplitButton>(dictionary);
        AssertContainsStyle<Expander>(dictionary);
        AssertContainsStyle<NavigationView>(dictionary);
        AssertContainsStyle<NavigationViewItem>(dictionary);
        AssertContainsStyle<CommandBar>(dictionary);
        AssertContainsStyle<AppBarButton>(dictionary);
        AssertContainsStyle<AppBarToggleButton>(dictionary);
        AssertContainsStyle<AppBarSeparator>(dictionary);
        AssertContainsStyle<MenuBar>(dictionary);
        AssertContainsStyle<MenuBarItem>(dictionary);
        AssertContainsStyle<MenuFlyoutItem>(dictionary);
        Assert.True(dictionary.Contains("TextPrimary"));
        Assert.True(dictionary.Contains("AccentBrush"));
        Assert.True(dictionary.Contains("CommandBarBackground"));
        Assert.True(dictionary.Contains("AppBarButtonBackground"));
        Assert.True(dictionary.Contains("AppBarButtonBackgroundHover"));
        Assert.True(dictionary.Contains("AppBarButtonBackgroundPressed"));
        Assert.True(dictionary.Contains("InfoBarInformationalBackground"));
        Assert.True(dictionary.Contains("InfoBadgeAttentionBackground"));
        Assert.True(dictionary.Contains("InfoBadgeCriticalForeground"));
        Assert.True(dictionary.Contains("AttentionDotInfoBadgeStyle"));
        Assert.True(dictionary.Contains("SuccessValueInfoBadgeStyle"));
        Assert.True(dictionary.Contains("CriticalIconInfoBadgeStyle"));
        Assert.True(dictionary.Contains("RatingControlSelectedForeground"));
        Assert.True(dictionary.Contains("RatingControlUnselectedForeground"));
        Assert.True(dictionary.Contains("ControlContentThemeFontSize"));
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
            AssertBasedOnStyle<FWAppBarButton, AppBarButton>(app.Resources);
            AssertBasedOnStyle<FWAppBarToggleButton, AppBarToggleButton>(app.Resources);
            AssertBasedOnStyle<FWAppBarSeparator, AppBarSeparator>(app.Resources);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    public void FluentControls_ShouldExposeFwPrefixedControlSurface()
    {
        AssertFluentControl<FWButton, Button>();
        AssertFluentControl<FWRepeatButton, RepeatButton>();
        AssertFluentControl<FWHyperlinkButton, HyperlinkButton>();
        AssertFluentControl<FWTextBox, TextBox>();
        AssertFluentControl<FWPasswordBox, PasswordBox>();
        AssertFluentControl<FWCheckBox, CheckBox>();
        AssertFluentControl<FWRadioButton, RadioButton>();
        AssertFluentControl<FWToggleButton, ToggleButton>();
        AssertFluentControl<FWToggleSwitch, ToggleSwitch>();
        AssertFluentControl<FWSlider, Slider>();
        AssertFluentControl<FWProgressBar, ProgressBar>();
        AssertFluentControl<FWComboBox, ComboBox>();
        AssertFluentControl<FWDropDownButton, Button>();
        AssertFluentControl<FWToggleSplitButton, SplitButton>();
        AssertFluentControl<FWProgressRing, RangeBase>();
        AssertFluentControl<FWInfoBadge, Control>();
        AssertFluentControl<FWRatingControl, Control>();
        AssertFluentControl<FWInfoBar, InfoBar>();
        AssertFluentControl<FWNumberBox, NumberBox>();
        AssertFluentControl<FWSplitButton, SplitButton>();
        AssertFluentControl<FWExpander, Expander>();
        AssertFluentControl<FWNavigationView, NavigationView>();
        AssertFluentControl<FWNavigationViewItem, NavigationViewItem>();
        AssertFluentControl<FWNavigationViewItemHeader, NavigationViewItemHeader>();
        AssertFluentControl<FWNavigationViewItemSeparator, NavigationViewItemSeparator>();
        AssertFluentControl<FWCommandBar, CommandBar>();
        AssertFluentControl<FWAppBarButton, AppBarButton>();
        AssertFluentControl<FWAppBarToggleButton, AppBarToggleButton>();
        AssertFluentControl<FWAppBarSeparator, AppBarSeparator>();
        AssertFluentControl<FWMenuBar, MenuBar>();
        AssertFluentControl<FWMenuBarItem, MenuBarItem>();
        AssertFluentControl<FWMenuFlyoutItem, MenuFlyoutItem>();
        AssertFluentControl<FWTabControl, TabControl>();
        AssertFluentControl<FWListView, ListView>();
        AssertFluentControl<FWListViewItem, ListViewItem>();
        AssertFluentControl<FWTreeView, TreeView>();
        AssertFluentControl<FWTreeViewItem, TreeViewItem>();
        AssertFluentControl<FWCalendar, Calendar>();
        AssertFluentControl<FWDatePicker, DatePicker>();
        AssertFluentControl<FWTimePicker, TimePicker>();
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
    public void FWInfoBadge_ShouldSwitchDisplayKindAndClampDisplayValue()
    {
        var badge = new FWInfoBadge();

        Assert.Equal(FWInfoBadgeDisplayKind.Dot, badge.DisplayKind);
        Assert.Equal(string.Empty, badge.DisplayValueText);

        badge.IconGlyph = "\uE946";
        Assert.Equal(FWInfoBadgeDisplayKind.Icon, badge.DisplayKind);

        badge.Value = 7;
        Assert.Equal(FWInfoBadgeDisplayKind.Value, badge.DisplayKind);
        Assert.Equal("7", badge.DisplayValueText);

        badge.Value = 142;
        badge.MaxValue = 99;
        Assert.Equal("99+", badge.DisplayValueText);

        badge.Value = -1;
        Assert.Equal(FWInfoBadgeDisplayKind.Icon, badge.DisplayKind);

        badge.IconGlyph = null;
        Assert.Equal(FWInfoBadgeDisplayKind.Dot, badge.DisplayKind);
    }

    [Fact]
    public void FWRatingControl_ShouldCoerceValuesClearAndRaiseEvent()
    {
        var rating = new FWRatingControl();
        FWRatingControlValueChangedEventArgs? lastArgs = null;
        var changed = 0;

        rating.ValueChanged += (_, args) =>
        {
            changed++;
            lastArgs = args;
        };

        rating.Value = 0.25;

        Assert.Equal(1.0, rating.Value);
        Assert.Equal(1, changed);
        Assert.Equal(FWRatingControl.UnsetValue, lastArgs!.OldValue);
        Assert.Equal(1.0, lastArgs.NewValue);

        rating.Value = 12;
        Assert.Equal(5.0, rating.Value);

        rating.Clear();
        Assert.Equal(FWRatingControl.UnsetValue, rating.Value);

        rating.IsClearEnabled = false;
        rating.Clear();
        Assert.Equal(1.0, rating.Value);

        rating.MaxRating = 3;
        rating.Value = 5;
        Assert.Equal(3.0, rating.Value);
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
        where TControl : Control
    {
        Assert.True(dictionary.TryGetValue(typeof(TControl), out var value), $"{typeof(TControl).Name} style was not found.");
        Assert.IsType<Style>(value);
    }

    private static void AssertBasedOnStyle<TFluentControl, TJaliumControl>(ResourceDictionary dictionary)
        where TFluentControl : TJaliumControl, IFluentJaliumControl
        where TJaliumControl : Control
    {
        Assert.True(dictionary.TryGetValue(typeof(TJaliumControl), out var baseValue), $"{typeof(TJaliumControl).Name} base style was not found.");
        var baseStyle = Assert.IsType<Style>(baseValue);

        Assert.True(dictionary.TryGetValue(typeof(TFluentControl), out var fluentValue), $"{typeof(TFluentControl).Name} FW style was not found.");
        var fluentStyle = Assert.IsType<Style>(fluentValue);

        Assert.Equal(typeof(TFluentControl), fluentStyle.TargetType);
        Assert.Same(baseStyle, fluentStyle.BasedOn);
    }

    private static void AssertOwnedStyle<TFluentControl>(ResourceDictionary dictionary)
        where TFluentControl : Control, IFluentJaliumControl
    {
        Assert.True(dictionary.TryGetValue(typeof(TFluentControl), out var fluentValue), $"{typeof(TFluentControl).Name} FW style was not found.");
        var fluentStyle = Assert.IsType<Style>(fluentValue);

        Assert.Equal(typeof(TFluentControl), fluentStyle.TargetType);
        Assert.Null(fluentStyle.BasedOn);
    }

    private static void AssertFluentControl<TFluentControl, TJaliumControl>()
        where TFluentControl : TJaliumControl, IFluentJaliumControl, new()
        where TJaliumControl : Control
    {
        var control = new TFluentControl();
        Assert.IsAssignableFrom<TJaliumControl>(control);
        Assert.StartsWith("FW", typeof(TFluentControl).Name, StringComparison.Ordinal);
    }

    private static void InvokeSplitButtonClick(SplitButton button)
    {
        typeof(SplitButton)
            .GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(button, [new SplitButtonClickEventArgs()]);
    }

    private static Color GetBrushColor(object? value)
    {
        return Assert.IsType<SolidColorBrush>(value).Color;
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
