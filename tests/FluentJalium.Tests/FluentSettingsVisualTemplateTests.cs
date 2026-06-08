using System.Diagnostics.CodeAnalysis;
using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Themes;
using Jalium.UI.Markup;
using JaliumThemeManager = Jalium.UI.Controls.Themes.ThemeManager;

namespace FluentJalium.Tests;

[Collection("Application")]
public sealed class FluentSettingsVisualTemplateTests
{
    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void ContentLayoutTheme_ShouldExposeSettingsCardPresenterPolishStyles()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = FluentThemeVariant.Dark.ToString();

        var dictionary = LoadThemeDictionary("/FluentJalium;component/Themes/Controls/ContentLayoutControls.jalxaml");

        var iconStyle = AssertKeyedStyle<ContentPresenter>(dictionary, "FWSettingsIconPresenterStyle");
        AssertSetter(iconStyle, FrameworkElement.MinWidthProperty, 20.0);
        AssertSetter(iconStyle, FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        AssertSetter(iconStyle, FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

        var actionStyle = AssertKeyedStyle<ContentPresenter>(dictionary, "FWSettingsActionPresenterStyle");
        AssertSetter(actionStyle, FrameworkElement.MinWidthProperty, 32.0);
        AssertSetter(actionStyle, FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Right);
        AssertSetter(actionStyle, FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

        ResetApplicationState();
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void DisclosureTheme_ShouldExposeSettingsExpanderItemAndPresenterPolishStyles()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = FluentThemeVariant.Dark.ToString();

        var dictionary = LoadThemeDictionary("/FluentJalium;component/Themes/Controls/DisclosureControls.jalxaml");

        var iconStyle = AssertKeyedStyle<ContentPresenter>(dictionary, "FWSettingsExpanderIconPresenterStyle");
        AssertSetter(iconStyle, FrameworkElement.MinWidthProperty, 20.0);
        AssertSetter(iconStyle, FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        AssertSetter(iconStyle, FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

        var itemHostStyle = AssertKeyedStyle<ItemsControl>(dictionary, "FWSettingsExpanderItemHostStyle");
        AssertSetter(itemHostStyle, Control.BackgroundProperty);

        var settingsExpanderStyle = AssertStyle<FWSettingsExpander>(dictionary);
        AssertSetter(settingsExpanderStyle, FWSettingsExpander.ItemsPanelProperty);

        ResetApplicationState();
    }

    private static ResourceDictionary LoadThemeDictionary(string source)
    {
        var loaded = ResourceDictionary.SourceLoader?.Invoke(
            new ResourceDictionary(),
            new Uri(source, UriKind.Relative),
            FluentThemeManager.ThemeAssembly);

        return Assert.IsType<ResourceDictionary>(loaded);
    }

    private static Style AssertStyle<TControl>(ResourceDictionary dictionary)
        where TControl : FrameworkElement
    {
        Assert.True(dictionary.TryGetValue(typeof(TControl), out var value), $"{typeof(TControl).Name} style was not found.");
        return Assert.IsType<Style>(value);
    }

    private static Style AssertKeyedStyle<TControl>(ResourceDictionary dictionary, string key)
        where TControl : FrameworkElement
    {
        Assert.True(dictionary.TryGetValue(key, out var value), $"{key} style was not found.");
        var style = Assert.IsType<Style>(value);
        Assert.Equal(typeof(TControl), style.TargetType);
        return style;
    }

    private static void AssertSetter(Style style, DependencyProperty property, object? expectedValue = null)
    {
        var setter = Assert.Single(style.Setters, candidate => candidate.Property == property);
        if (expectedValue != null)
        {
            AssertSetterValue(expectedValue, setter.Value);
        }
    }

    private static void AssertSetterValue(object expectedValue, object? actualValue)
    {
        if (expectedValue is double expectedDouble && actualValue is IConvertible actualConvertible)
        {
            Assert.Equal(expectedDouble, actualConvertible.ToDouble(System.Globalization.CultureInfo.InvariantCulture));
            return;
        }

        if (actualValue != null &&
            expectedValue.GetType() != actualValue.GetType() &&
            string.Equals(expectedValue.ToString(), actualValue.ToString(), StringComparison.Ordinal))
        {
            return;
        }

        Assert.Equal(expectedValue, actualValue);
    }

    private static void ResetApplicationState()
    {
        var currentField = typeof(Application).GetField("_current", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        currentField?.SetValue(null, null);

        var jaliumReset = typeof(JaliumThemeManager).GetMethod("Reset", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        jaliumReset?.Invoke(null, null);

        var fluentReset = typeof(FluentThemeManager).GetMethod("Reset", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        fluentReset?.Invoke(null, null);
    }
}
