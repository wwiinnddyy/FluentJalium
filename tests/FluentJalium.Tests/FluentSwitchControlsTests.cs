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
public sealed class FluentSwitchControlsTests
{
    private static readonly string[] SwitchResourceKeys =
    [
        "ControlBackground",
        "ControlBackgroundHover",
        "ControlBackgroundDisabled",
        "ControlBorder",
        "ControlBorderHover",
        "ControlBorderDisabled",
        "ToggleUncheckedBackground",
        "ToggleUncheckedBackgroundHover",
        "ToggleUncheckedBorder",
        "ToggleUncheckedBorderHover",
        "ToggleCheckedBackground",
        "ToggleCheckedBackgroundHover",
        "ToggleCheckedBorder",
        "ToggleCheckedBorderHover",
        "ToggleDisabledBackground",
        "ToggleDisabledBorder",
        "ToggleSwitchOnBackground",
        "ToggleSwitchOnBackgroundHover",
        "ToggleSwitchOnBorder",
        "ToggleSwitchOnBorderHover",
        "ToggleSwitchOffBackground",
        "ToggleSwitchOffBackgroundHover",
        "ToggleSwitchOffBorder",
        "ToggleSwitchOffBorderHover",
        "ToggleSwitchThumb",
        "ToggleSwitchThumbDisabled"
    ];

    [Theory]
    [InlineData(FluentThemeVariant.Dark)]
    [InlineData(FluentThemeVariant.Light)]
    [InlineData(FluentThemeVariant.HighContrast)]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void ThemeDictionary_ShouldExposeCurrentSwitchTokensForEveryTheme(FluentThemeVariant theme)
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = theme.ToString();

        var dictionary = LoadGenericThemeDictionary();

        foreach (var key in SwitchResourceKeys)
        {
            Assert.True(dictionary.TryGetValue(key, out var value), $"{key} was not found in the {theme} theme.");
            Assert.IsType<SolidColorBrush>(value);
        }

        ResetApplicationState();
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void Apply_ShouldRegisterFwSwitchStylesBasedOnJaliumStyles()
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
    public void GenericTheme_ShouldDefineSwitchBaseStylesAndFluentSetters()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = FluentThemeVariant.Dark.ToString();

        var dictionary = LoadGenericThemeDictionary();

        var toggleButtonStyle = AssertStyle<ToggleButton>(dictionary);
        AssertSetter(toggleButtonStyle, Control.BackgroundProperty);
        AssertSetter(toggleButtonStyle, Control.ForegroundProperty);
        AssertSetter(toggleButtonStyle, Control.BorderBrushProperty);
        AssertSetter(toggleButtonStyle, Control.PaddingProperty);
        AssertSetter(toggleButtonStyle, Control.CornerRadiusProperty);
        AssertSetter(toggleButtonStyle, FrameworkElement.MinHeightProperty);

        var toggleSwitchStyle = AssertStyle<ToggleSwitch>(dictionary);
        AssertSetter(toggleSwitchStyle, Control.ForegroundProperty);
        AssertSetter(toggleSwitchStyle, Control.BackgroundProperty);
        AssertSetter(toggleSwitchStyle, Control.BorderBrushProperty);
        AssertSetter(toggleSwitchStyle, ToggleSwitch.OnBackgroundProperty);
        AssertSetter(toggleSwitchStyle, ToggleSwitch.OffBackgroundProperty);
        AssertSetter(toggleSwitchStyle, FrameworkElement.MinHeightProperty);

        var fluentToggleSwitchStyle = AssertStyle<FWToggleSwitch>(dictionary);
        Assert.Same(toggleSwitchStyle, fluentToggleSwitchStyle.BasedOn);
        AssertSetter(fluentToggleSwitchStyle, Control.TemplateProperty);
        AssertSetter(fluentToggleSwitchStyle, FrameworkElement.MinHeightProperty);

        ResetApplicationState();
    }

    [Fact]
    public void FWToggleButton_ShouldCycleUncheckedCheckedIndeterminateAndRaiseStateEvents()
    {
        var button = new FWToggleButton
        {
            Content = "Switch mode",
            IsThreeState = true
        };
        var checkedCount = 0;
        var uncheckedCount = 0;
        var indeterminateCount = 0;

        button.Checked += (_, _) => checkedCount++;
        button.Unchecked += (_, _) => uncheckedCount++;
        button.Indeterminate += (_, _) => indeterminateCount++;

        Assert.False(button.IsChecked);

        InvokeToggleButtonClick(button);
        Assert.True(button.IsChecked);
        Assert.Equal(1, checkedCount);

        InvokeToggleButtonClick(button);
        Assert.Null(button.IsChecked);
        Assert.Equal(1, indeterminateCount);

        InvokeToggleButtonClick(button);
        Assert.False(button.IsChecked);
        Assert.Equal(1, uncheckedCount);
        Assert.Equal("Switch mode", button.Content);
    }

    [Fact]
    public void FWToggleSwitch_ShouldSynchronizeOnOffContentAndRaiseToggled()
    {
        var toggleSwitch = new FWToggleSwitch
        {
            Header = "Notifications",
            Description = "Use the app setting row pattern.",
            OffContent = "Notifications off",
            OnContent = "Notifications on"
        };
        var toggled = 0;

        toggleSwitch.Toggled += (_, _) => toggled++;

        Assert.False(toggleSwitch.IsOn);
        Assert.Equal("Notifications", toggleSwitch.Header);
        Assert.Equal("Use the app setting row pattern.", toggleSwitch.Description);
        Assert.Equal("Notifications off", toggleSwitch.OffContent);

        toggleSwitch.IsOn = true;

        Assert.True(toggleSwitch.IsOn);
        Assert.Equal(1, toggled);
        Assert.Equal("Notifications on", toggleSwitch.OnContent);

        toggleSwitch.IsOn = false;

        Assert.False(toggleSwitch.IsOn);
        Assert.Equal(2, toggled);
        Assert.Equal("Notifications off", toggleSwitch.OffContent);
    }

    [Fact]
    public void FWToggleSwitch_ShouldExposeMaterialAwareBrushProperties()
    {
        var onBrush = new SolidColorBrush(Color.FromRgb(0x00, 0x78, 0xD4));
        var offBrush = new SolidColorBrush(Color.FromArgb(0x99, 0xFF, 0xFF, 0xFF));
        var toggleSwitch = new FWToggleSwitch
        {
            OnBackground = onBrush,
            OffBackground = offBrush,
            IsEnabled = false
        };

        Assert.Same(onBrush, toggleSwitch.OnBackground);
        Assert.Same(offBrush, toggleSwitch.OffBackground);
        Assert.False(toggleSwitch.IsEnabled);
    }

    private static ResourceDictionary LoadGenericThemeDictionary()
    {
        var loaded = ResourceDictionary.SourceLoader?.Invoke(
            new ResourceDictionary(),
            new Uri("/FluentJalium;component/Themes/Generic.jalxaml", UriKind.Relative),
            FluentThemeManager.ThemeAssembly);

        return Assert.IsType<ResourceDictionary>(loaded);
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

    private static void AssertSetter(Style style, DependencyProperty property)
    {
        Assert.Contains(style.Setters, setter => setter.Property == property);
    }

    private static void InvokeToggleButtonClick(ToggleButton button)
    {
        typeof(ToggleButton)
            .GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(button, null);
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
