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
            AssertBasedOnStyle<FWToggleButton, ToggleButton>(app.Resources);
            AssertBasedOnStyle<FWToggleSwitch, ToggleSwitch>(app.Resources);
            AssertBasedOnStyle<FWSlider, Slider>(app.Resources);
            AssertBasedOnStyle<FWRangeSlider, RangeSlider>(app.Resources);
            AssertBasedOnStyle<FWProgressBar, ProgressBar>(app.Resources);
            AssertOwnedStyle<FWProgressRing>(app.Resources);
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
        AssertContainsStyle<ToggleButton>(dictionary);
        AssertContainsStyle<ToggleSwitch>(dictionary);
        AssertContainsStyle<Slider>(dictionary);
        AssertContainsStyle<RangeSlider>(dictionary);
        AssertContainsStyle<ProgressBar>(dictionary);
        AssertContainsStyle<FWProgressRing>(dictionary);
        AssertContainsStyle<FWDropDownButton>(dictionary);
        AssertContainsStyle<SplitButton>(dictionary);
        AssertContainsStyle<FWToggleSplitButton>(dictionary);
        AssertContainsStyle<CommandBar>(dictionary);
        AssertContainsStyle<AppBarButton>(dictionary);
        AssertContainsStyle<AppBarToggleButton>(dictionary);
        AssertContainsStyle<AppBarSeparator>(dictionary);
        Assert.True(dictionary.Contains("TextPrimary"));
        Assert.True(dictionary.Contains("AccentBrush"));
        Assert.True(dictionary.Contains("ToggleCheckedBackground"));
        Assert.True(dictionary.Contains("ToggleUncheckedBackground"));
        Assert.True(dictionary.Contains("ToggleDisabledBackground"));
        Assert.True(dictionary.Contains("SliderTrack"));
        Assert.True(dictionary.Contains("SliderThumb"));
        Assert.True(dictionary.Contains("ProgressRingForeground"));
        Assert.True(dictionary.Contains("CommandBarBackground"));
        Assert.True(dictionary.Contains("AppBarButtonBackground"));
        Assert.True(dictionary.Contains("AppBarButtonBackgroundHover"));
        Assert.True(dictionary.Contains("AppBarButtonBackgroundPressed"));
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
        AssertFluentControl<FWAppBarButton, AppBarButton>();
        AssertFluentControl<FWAppBarToggleButton, AppBarToggleButton>();
        AssertFluentControl<FWAppBarSeparator, AppBarSeparator>();
    }

    [Fact]
    public void FluentSwitchControls_ShouldExposeFwPrefixedSurface()
    {
        AssertFluentControl<FWToggleButton, ToggleButton>();
        AssertFluentControl<FWToggleSwitch, ToggleSwitch>();
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

        var rangeSlider = new FWRangeSlider
        {
            Minimum = 0,
            Maximum = 100,
            RangeStart = 20,
            RangeEnd = 80,
            MinimumRange = 10
        };

        rangeSlider.RangeStart = 95;
        Assert.Equal(70, rangeSlider.RangeStart);

        rangeSlider.RangeEnd = 50;
        Assert.Equal(80, rangeSlider.RangeEnd);
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
