using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Data;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using JaliumThemeManager = Jalium.UI.Controls.Themes.ThemeManager;

namespace FluentJalium.Tests;

[Collection("Application")]
public sealed class FluentRangeProgressTests
{
    private static readonly string[] RangeProgressResourceKeys =
    [
        "SliderTrack",
        "SliderThumb",
        "SliderTrackFill",
        "SliderTrackFillPointerOver",
        "SliderTrackFillPressed",
        "SliderTrackFillDisabled",
        "SliderTrackValueFill",
        "SliderTrackValueFillPointerOver",
        "SliderTrackValueFillPressed",
        "SliderTrackValueFillDisabled",
        "SliderThumbBackground",
        "SliderThumbBackgroundPointerOver",
        "SliderThumbBackgroundPressed",
        "SliderThumbBackgroundDisabled",
        "SliderThumbBorderBrush",
        "SliderThumbBorderBrushDisabled",
        "SliderTickBarFill",
        "SliderTickBarFillDisabled",
        "ProgressBarBackground",
        "ProgressBarForeground",
        "ProgressBarIndeterminateBackground",
        "ProgressBarPausedForeground",
        "ProgressBarErrorForeground",
        "ProgressBarDisabledBackground",
        "ProgressBarDisabledForeground",
        "ProgressRingBackground",
        "ProgressRingDisabledForeground",
        "ProgressRingForeground"
    ];

    [Theory]
    [InlineData(FluentThemeVariant.Dark)]
    [InlineData(FluentThemeVariant.Light)]
    [InlineData(FluentThemeVariant.HighContrast)]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void ThemeDictionary_ShouldExposeCurrentRangeProgressTokensForEveryTheme(FluentThemeVariant theme)
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = theme.ToString();

        var dictionary = LoadGenericThemeDictionary();

        foreach (var key in RangeProgressResourceKeys)
        {
            Assert.True(dictionary.TryGetValue(key, out var value), $"{key} was not found in the {theme} theme.");
            Assert.IsType<SolidColorBrush>(value);
        }

        ResetApplicationState();
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void ApplyAccent_ShouldUpdateAccentDrivenRangeProgressResources()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            var accent = Color.FromRgb(0xE3, 0x00, 0x8C);
            FluentThemeManager.ApplyAccent(accent);

            Assert.Equal(accent, GetBrushColor(app.Resources["SliderTrackValueFill"]));
            Assert.Equal(accent, GetBrushColor(app.Resources["SliderThumbBackground"]));
            Assert.Equal(accent, GetBrushColor(app.Resources["ProgressBarForeground"]));
            Assert.Equal(accent, GetBrushColor(app.Resources["ProgressRingForeground"]));
            Assert.IsType<SolidColorBrush>(app.Resources["SliderTrackValueFillPointerOver"]);
            Assert.IsType<SolidColorBrush>(app.Resources["SliderTrackValueFillPressed"]);
            Assert.IsType<SolidColorBrush>(app.Resources["SliderTrackValueFillDisabled"]);
            Assert.IsType<SolidColorBrush>(app.Resources["ProgressBarIndeterminateBackground"]);
            Assert.IsType<SolidColorBrush>(app.Resources["ProgressBarDisabledForeground"]);
            Assert.IsType<SolidColorBrush>(app.Resources["ProgressRingDisabledForeground"]);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void Apply_ShouldRegisterFwRangeProgressStyles()
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
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void GenericTheme_ShouldDefineRangeProgressBaseStylesAndProgressRingDefaults()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = FluentThemeVariant.Dark.ToString();

        var dictionary = LoadGenericThemeDictionary();

        AssertContainsStyle<Slider>(dictionary);
        AssertContainsStyle<RangeSlider>(dictionary);
        AssertContainsStyle<ProgressBar>(dictionary);
        AssertContainsStyle<FWProgressBar>(dictionary);

        var sliderStyle = AssertStyle<Slider>(dictionary);
        AssertSetter(sliderStyle, Control.BackgroundProperty);
        AssertSetter(sliderStyle, Control.ForegroundProperty);
        AssertSetter(sliderStyle, Slider.TrackBrushProperty);
        AssertSetter(sliderStyle, Slider.ThumbBrushProperty);

        var fwSliderStyle = AssertStyle<FWSlider>(dictionary);
        Assert.Same(sliderStyle, fwSliderStyle.BasedOn);
        AssertSetter(fwSliderStyle, FWSlider.DensityProperty);

        var rangeSliderStyle = AssertStyle<RangeSlider>(dictionary);
        AssertSetter(rangeSliderStyle, Control.BackgroundProperty);
        AssertSetter(rangeSliderStyle, Control.ForegroundProperty);
        AssertSetter(rangeSliderStyle, RangeSlider.TrackBrushProperty);
        AssertSetter(rangeSliderStyle, RangeSlider.ThumbBrushProperty);

        var fwRangeSliderStyle = AssertStyle<FWRangeSlider>(dictionary);
        Assert.Same(rangeSliderStyle, fwRangeSliderStyle.BasedOn);
        AssertSetter(fwRangeSliderStyle, FWRangeSlider.DensityProperty);

        var progressBarStyle = AssertStyle<ProgressBar>(dictionary);
        AssertSetter(progressBarStyle, Control.BackgroundProperty);
        AssertSetter(progressBarStyle, Control.ForegroundProperty);
        AssertSetter(progressBarStyle, ProgressBar.ProgressBrushProperty);

        var fwProgressBarStyle = AssertStyle<FWProgressBar>(dictionary);
        Assert.Same(progressBarStyle, fwProgressBarStyle.BasedOn);
        AssertSetter(fwProgressBarStyle, FWProgressBar.DensityProperty);
        AssertTriggerSetter(fwProgressBarStyle, FWProgressBar.ShowPausedProperty, true, ProgressBar.ProgressBrushProperty, "ProgressBarPausedForeground");
        AssertTriggerSetter(fwProgressBarStyle, FWProgressBar.ShowErrorProperty, true, ProgressBar.ProgressBrushProperty, "ProgressBarErrorForeground");
        AssertTriggerSetter(fwProgressBarStyle, Control.IsEnabledProperty, false, ProgressBar.ProgressBrushProperty, "ProgressBarDisabledForeground");

        var ringStyle = AssertStyle<FWProgressRing>(dictionary);
        Assert.Null(ringStyle.BasedOn);
        AssertSetter(ringStyle, Control.ForegroundProperty);
        AssertSetter(ringStyle, Control.BackgroundProperty);
        AssertSetter(ringStyle, FWProgressRing.ProgressBrushProperty);
        AssertSetter(ringStyle, FWProgressRing.RingSizeProperty);
        AssertSetter(ringStyle, FWProgressRing.StrokeThicknessProperty);
        AssertBooleanSetter(ringStyle, FWProgressRing.IsActiveProperty, true);
        AssertBooleanSetter(ringStyle, FWProgressRing.IsIndeterminateProperty, true);

        ResetApplicationState();
    }

    [Fact]
    public void FWRangeControls_ShouldApplyDensityAndSizePresets()
    {
        var slider = new FWSlider();

        Assert.Equal(FWRangeDensity.Comfortable, slider.Density);
        Assert.Equal(32, slider.MinHeight);
        Assert.Equal(32, slider.Height);

        slider.Density = FWRangeDensity.Compact;

        Assert.Equal(28, slider.MinHeight);
        Assert.Equal(28, slider.Height);

        slider.Density = FWRangeDensity.Spacious;

        Assert.Equal(40, slider.MinHeight);
        Assert.Equal(40, slider.Height);

        var rangeSlider = new FWRangeSlider
        {
            Density = FWRangeDensity.Spacious
        };

        Assert.Equal(40, rangeSlider.MinHeight);
        Assert.Equal(40, rangeSlider.Height);

        rangeSlider.Density = FWRangeDensity.Compact;

        Assert.Equal(28, rangeSlider.MinHeight);
        Assert.Equal(28, rangeSlider.Height);

        var progressBar = new FWProgressBar();

        Assert.Equal(FWRangeDensity.Comfortable, progressBar.Density);
        Assert.Equal(6, progressBar.MinHeight);
        Assert.Equal(6, progressBar.Height);
        Assert.Equal(new CornerRadius(3), progressBar.CornerRadius);

        progressBar.Density = FWRangeDensity.Compact;

        Assert.Equal(4, progressBar.MinHeight);
        Assert.Equal(4, progressBar.Height);
        Assert.Equal(new CornerRadius(2), progressBar.CornerRadius);

        progressBar.Density = FWRangeDensity.Spacious;

        Assert.Equal(8, progressBar.MinHeight);
        Assert.Equal(8, progressBar.Height);
        Assert.Equal(new CornerRadius(4), progressBar.CornerRadius);

        var ring = new FWProgressRing();

        Assert.Equal(FWProgressRingSize.Medium, ring.RingSize);
        Assert.Equal(32, ring.Width);
        Assert.Equal(32, ring.Height);
        Assert.Equal(4, ring.StrokeThickness);

        ring.RingSize = FWProgressRingSize.Small;

        Assert.Equal(24, ring.Width);
        Assert.Equal(24, ring.Height);
        Assert.Equal(3, ring.StrokeThickness);

        ring.RingSize = FWProgressRingSize.Large;

        Assert.Equal(48, ring.Width);
        Assert.Equal(48, ring.Height);
        Assert.Equal(5, ring.StrokeThickness);
    }

    [Fact]
    public void FWSlider_ShouldCoerceValueAndRaiseEventsForDirectValueChanges()
    {
        var slider = new FWSlider
        {
            Density = FWRangeDensity.Compact,
            Minimum = 0,
            Maximum = 100,
            Value = 80
        };
        var changed = 0;
        slider.ValueChanged += (_, _) => changed++;

        slider.Maximum = 40;

        Assert.Equal(40, slider.Value);

        slider.Minimum = 20;
        slider.Value = 10;

        Assert.Equal(20, slider.Value);

        slider.Value = 55;

        Assert.Equal(40, slider.Value);
        Assert.Equal(2, changed);
        Assert.Equal(FWRangeDensity.Compact, slider.Density);
    }

    [Fact]
    public void FWProgressBar_ShouldTrackDeterminateAndIndeterminateRangeState()
    {
        var progressBar = new FWProgressBar
        {
            Density = FWRangeDensity.Spacious,
            Minimum = 10,
            Maximum = 110,
            Value = 40,
            IsIndeterminate = true
        };
        var changed = 0;
        progressBar.ValueChanged += (_, _) => changed++;

        progressBar.Value = -20;

        Assert.True(progressBar.IsIndeterminate);
        Assert.Equal(FWRangeDensity.Spacious, progressBar.Density);
        Assert.Equal(10, progressBar.Value);
        Assert.Equal(0, progressBar.Percentage);
        Assert.Equal(1, changed);

        progressBar.IsIndeterminate = false;
        progressBar.Value = 60;

        Assert.False(progressBar.IsIndeterminate);
        Assert.Equal(0.5, progressBar.Percentage);
        Assert.Equal(2, changed);
    }

    [Fact]
    public void FWProgressBar_ShouldExposePausedAndErrorStatusStates()
    {
        var progressBar = new FWProgressBar
        {
            Minimum = 0,
            Maximum = 100,
            Value = 48,
            ShowPaused = true
        };

        Assert.True(progressBar.ShowPaused);
        Assert.False(progressBar.ShowError);

        progressBar.ShowError = true;

        Assert.True(progressBar.ShowPaused);
        Assert.True(progressBar.ShowError);
    }

    [Fact]
    public void FWRangeSlider_ShouldCoerceRangesWhenBoundsAndMinimumRangeChange()
    {
        var slider = new FWRangeSlider
        {
            Minimum = 0,
            Maximum = 100,
            RangeStart = 20,
            RangeEnd = 80
        };
        var startChanges = 0;
        var endChanges = 0;
        slider.RangeStartChanged += (_, _) => startChanges++;
        slider.RangeEndChanged += (_, _) => endChanges++;

        slider.MinimumRange = 20;
        slider.RangeStart = 25;
        slider.RangeEnd = 90;

        Assert.Equal(25, slider.RangeStart);
        Assert.Equal(90, slider.RangeEnd);
        Assert.True(startChanges >= 1);
        Assert.True(endChanges >= 1);

        slider.RangeStart = 95;

        Assert.InRange(slider.RangeStart, slider.Minimum, slider.Maximum);
        Assert.InRange(slider.RangeEnd, slider.Minimum, slider.Maximum);
        Assert.True(slider.RangeEnd - slider.RangeStart >= slider.MinimumRange);

        slider.RangeEnd = 10;

        Assert.InRange(slider.RangeStart, slider.Minimum, slider.Maximum);
        Assert.InRange(slider.RangeEnd, slider.Minimum, slider.Maximum);
        Assert.True(slider.RangeEnd >= slider.RangeStart);

        slider.Minimum = 30;
        slider.Maximum = 70;

        Assert.InRange(slider.RangeStart, slider.Minimum, slider.Maximum);
        Assert.InRange(slider.RangeEnd, slider.Minimum, slider.Maximum);
        Assert.True(slider.RangeEnd >= slider.RangeStart);
    }

    [Fact]
    public void FWRangeControls_ShouldExposeGalleryOrientationTickAndStateProperties()
    {
        var slider = new FWSlider
        {
            Orientation = Orientation.Vertical,
            TickFrequency = 10,
            IsSnapToTickEnabled = true,
            Minimum = -50,
            Maximum = 50,
            Value = 20
        };
        var rangeSlider = new FWRangeSlider
        {
            Orientation = Orientation.Vertical,
            TickFrequency = 5,
            IsSnapToTickEnabled = true,
            Minimum = 0,
            Maximum = 100,
            RangeStart = 25,
            RangeEnd = 75,
            MinimumRange = 10
        };
        var progressBar = new FWProgressBar
        {
            Minimum = 0,
            Maximum = 100,
            Value = 68,
            IsIndeterminate = false
        };
        var progressRing = new FWProgressRing
        {
            Minimum = 0,
            Maximum = 100,
            Value = 68,
            IsActive = true,
            IsIndeterminate = false
        };

        Assert.Equal(Orientation.Vertical, slider.Orientation);
        Assert.Equal(10, slider.TickFrequency);
        Assert.True(slider.IsSnapToTickEnabled);
        Assert.Equal(20, slider.Value);

        Assert.Equal(Orientation.Vertical, rangeSlider.Orientation);
        Assert.Equal(5, rangeSlider.TickFrequency);
        Assert.True(rangeSlider.IsSnapToTickEnabled);
        Assert.Equal(25, rangeSlider.RangeStart);
        Assert.Equal(75, rangeSlider.RangeEnd);
        Assert.Equal(10, rangeSlider.MinimumRange);

        Assert.False(progressBar.IsIndeterminate);
        Assert.False(progressBar.ShowPaused);
        Assert.False(progressBar.ShowError);
        Assert.InRange(progressBar.Percentage, 0.6799, 0.6801);

        Assert.True(progressRing.IsActive);
        Assert.False(progressRing.IsIndeterminate);
        Assert.Equal(68, progressRing.Value);
    }

    [Fact]
    public void FWProgressRing_ShouldCoerceRangeValuesAndKeepStateProperties()
    {
        var progressBrush = new SolidColorBrush(Color.FromRgb(0x10, 0x90, 0x68));
        var ring = new FWProgressRing
        {
            Minimum = 0,
            Maximum = 100,
            Value = 25,
            ProgressBrush = progressBrush,
            StrokeThickness = 8,
            IsActive = true,
            IsIndeterminate = false
        };
        var changed = 0;
        ring.ValueChanged += (_, _) => changed++;

        ring.Value = 160;

        Assert.Equal(100, ring.Value);
        Assert.Equal(1, changed);

        ring.Maximum = 40;

        Assert.Equal(40, ring.Value);

        ring.Minimum = 10;
        ring.Value = 5;

        Assert.Equal(10, ring.Value);
        Assert.Same(progressBrush, ring.ProgressBrush);
        Assert.Equal(8, ring.StrokeThickness);

        ring.IsActive = false;
        ring.IsIndeterminate = true;

        Assert.False(ring.IsActive);
        Assert.True(ring.IsIndeterminate);
    }

    [Fact]
    public void FWProgressRing_ShouldClampMeasureSizeAndFallbackInvalidInputToDefaultSize()
    {
        var ring = new TestProgressRing
        {
            Width = 96,
            Height = 80
        };

        var constrained = ring.MeasureForTest(new Size(double.PositiveInfinity, double.PositiveInfinity));
        var fallback = ring.MeasureForTest(new Size(double.NaN, -1));
        var requested = ring.MeasureForTest(new Size(48, 36));

        Assert.Equal(new Size(64, 64), constrained);
        Assert.Equal(new Size(32, 32), fallback);
        Assert.Equal(new Size(48, 36), requested);
    }

    private static ResourceDictionary LoadGenericThemeDictionary()
    {
        var loaded = ResourceDictionary.SourceLoader?.Invoke(
            new ResourceDictionary(),
            new Uri("/FluentJalium;component/Themes/Generic.jalxaml", UriKind.Relative),
            FluentThemeManager.ThemeAssembly);

        return Assert.IsType<ResourceDictionary>(loaded);
    }

    private static void AssertContainsStyle<TControl>(ResourceDictionary dictionary)
        where TControl : FrameworkElement
    {
        Assert.True(dictionary.TryGetValue(typeof(TControl), out var value), $"{typeof(TControl).Name} style was not found.");
        Assert.IsType<Style>(value);
    }

    private static Style AssertStyle<TControl>(ResourceDictionary dictionary)
        where TControl : FrameworkElement
    {
        Assert.True(dictionary.TryGetValue(typeof(TControl), out var value), $"{typeof(TControl).Name} style was not found.");
        return Assert.IsType<Style>(value);
    }

    private static void AssertBasedOnStyle<TFluentControl, TJaliumControl>(ResourceDictionary dictionary)
        where TFluentControl : TJaliumControl, IFluentJaliumControl
        where TJaliumControl : FrameworkElement
    {
        var baseStyle = AssertStyle<TJaliumControl>(dictionary);
        var fluentStyle = AssertStyle<TFluentControl>(dictionary);

        Assert.Equal(typeof(TFluentControl), fluentStyle.TargetType);
        Assert.Same(baseStyle, fluentStyle.BasedOn);
    }

    private static void AssertOwnedStyle<TFluentControl>(ResourceDictionary dictionary)
        where TFluentControl : FrameworkElement, IFluentJaliumControl
    {
        var fluentStyle = AssertStyle<TFluentControl>(dictionary);

        Assert.Equal(typeof(TFluentControl), fluentStyle.TargetType);
        Assert.Null(fluentStyle.BasedOn);
    }

    private static void AssertSetter(Style style, DependencyProperty property, object? expectedValue = null)
    {
        var setter = Assert.Single(style.Setters, s => s.Property == property);
        if (expectedValue != null)
        {
            Assert.Equal(expectedValue, setter.Value);
        }
    }

    private static void AssertBooleanSetter(Style style, DependencyProperty property, bool expectedValue)
    {
        var setter = Assert.Single(style.Setters, s => s.Property == property);
        Assert.Equal(expectedValue, Convert.ToBoolean(setter.Value));
    }

    private static void AssertTriggerSetter(
        Style style,
        DependencyProperty triggerProperty,
        object triggerValue,
        DependencyProperty setterProperty,
        object expectedSetterValue)
    {
        var trigger = Assert.Single(
            style.Triggers.OfType<Trigger>(),
            candidate => candidate.Property == triggerProperty && TriggerValueEquals(candidate.Value, triggerValue));
        var setter = Assert.Single(trigger.Setters, candidate => candidate.Property == setterProperty);

        if (setter.Value is IDynamicResourceReference dynamicReference)
        {
            Assert.Equal(expectedSetterValue, dynamicReference.ResourceKey);
            return;
        }

        Assert.Equal(expectedSetterValue, setter.Value);
    }

    private static bool TriggerValueEquals(object? actual, object expected)
    {
        if (Equals(actual, expected))
        {
            return true;
        }

        return actual is string text && string.Equals(text, Convert.ToString(expected), StringComparison.OrdinalIgnoreCase);
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

    private sealed class TestProgressRing : FWProgressRing
    {
        public Size MeasureForTest(Size availableSize) => MeasureOverride(availableSize);
    }
}
