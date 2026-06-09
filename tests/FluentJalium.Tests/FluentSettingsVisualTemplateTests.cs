using System.Diagnostics.CodeAnalysis;
using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using FluentJalium.Gallery.Pages;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Themes;
using Jalium.UI.Documents;
using Jalium.UI.Markup;
using Jalium.UI.Media;
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

        var headerTextStyle = AssertKeyedStyle<TextBlock>(dictionary, "FWSettingsHeaderTextStyle");
        AssertSetter(headerTextStyle, TextElement.FontWeightProperty, FontWeights.SemiBold);
        AssertSetter(headerTextStyle, TextBlock.TextWrappingProperty, TextWrapping.Wrap);

        var descriptionTextStyle = AssertKeyedStyle<TextBlock>(dictionary, "FWSettingsDescriptionTextStyle");
        AssertSetter(descriptionTextStyle, TextBlock.TextWrappingProperty, TextWrapping.Wrap);

        var headerAccessTextStyle = AssertKeyedStyle<AccessText>(dictionary, "FWSettingsHeaderAccessTextStyle");
        AssertSetter(headerAccessTextStyle, AccessText.FontWeightProperty, FontWeights.SemiBold);

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

        Assert.True(dictionary.TryGetValue("FWSettingsExpanderDefaultItemTemplate", out var itemTemplate), "FWSettingsExpanderDefaultItemTemplate was not found.");
        Assert.IsType<DataTemplate>(itemTemplate);

        var headerTextStyle = AssertKeyedStyle<TextBlock>(dictionary, "FWSettingsExpanderHeaderTextStyle");
        AssertSetter(headerTextStyle, TextElement.FontWeightProperty, FontWeights.SemiBold);
        AssertSetter(headerTextStyle, TextBlock.TextWrappingProperty, TextWrapping.Wrap);

        var descriptionTextStyle = AssertKeyedStyle<TextBlock>(dictionary, "FWSettingsExpanderDescriptionTextStyle");
        AssertSetter(descriptionTextStyle, TextBlock.TextWrappingProperty, TextWrapping.Wrap);

        var settingsExpanderStyle = AssertStyle<FWSettingsExpander>(dictionary);
        AssertSetter(settingsExpanderStyle, FWSettingsExpander.ItemsPanelProperty);
        AssertSetter(settingsExpanderStyle, FWSettingsExpander.ItemTemplateProperty);

        ResetApplicationState();
    }

    [Fact]
    public void GallerySettingsPage_ShouldFormatSettingsVisualQaSnapshot()
    {
        var primary = new FWSettingsCard
        {
            Header = "App theme",
            Description = "Follow system setting.",
            HeaderIcon = new FWTextBlock { Text = "I" },
            ActionIcon = new FWTextBlock { Text = ">" },
            Content = new FWToggleSwitch { IsOn = true },
            IsClickEnabled = true
        };
        var action = new FWSettingsCard
        {
            Header = "Launch behavior",
            Description = "Open startup options.",
            HeaderIcon = new FWTextBlock { Text = "I" },
            ActionIcon = new FWTextBlock { Text = ">" },
            Content = new FWButton { Content = "Configure" },
            IsClickEnabled = true
        };
        var disabled = new FWSettingsCard
        {
            Header = "Enterprise policy",
            Description = "Disabled row keeps alignment visible.",
            HeaderIcon = new FWTextBlock { Text = "I" },
            ActionIcon = new FWTextBlock { Text = "!" },
            Content = new FWTextBlock { Text = "Managed" },
            IsClickEnabled = true,
            IsEnabled = false
        };
        var expander = new FWSettingsExpander
        {
            Header = "Advanced settings",
            IsExpanded = true
        };
        expander.AddSetting(new FWSettingsCard { Header = "Accent color" });
        expander.AddSetting(new FWSettingsCard { Header = "Language" });
        expander.AddSetting(new FWSettingsCard { Header = "Diagnostics" });

        var snapshot = GallerySettingsPage.CreateSettingsVisualQaSnapshot(
            new[] { primary, action, disabled },
            expander);
        var text = GallerySettingsPage.FormatSettingsVisualQa("Settings QA", snapshot);

        Assert.Equal(3, snapshot.CardCount);
        Assert.Equal(3, snapshot.ClickableCount);
        Assert.Equal(1, snapshot.DisabledCount);
        Assert.Equal(3, snapshot.ExpanderItemCount);
        Assert.Equal(1, snapshot.ExpandedCount);
        Assert.True(snapshot.HasIconColumn);
        Assert.True(snapshot.HasActionColumn);
        Assert.True(snapshot.HasItemHostRows);
        Assert.True(snapshot.HasAutomationName);
        Assert.True(snapshot.HasAutomationHelpText);
        Assert.True(snapshot.CanInvokeClickableRow);
        Assert.Equal(6, snapshot.DenseRowCount);
        Assert.True(snapshot.IsSettingsVisualQaReady);
        Assert.Contains("Settings QA", text);
        Assert.Contains("Cards: 3", text);
        Assert.Contains("Clickable: 3", text);
        Assert.Contains("Disabled: 1", text);
        Assert.Contains("Expander items: 3", text);
        Assert.Contains("Icon/action: on/on", text);
        Assert.Contains("Automation: on/on", text);
        Assert.Contains("Dense rows: 6", text);
        Assert.Contains("Ready: on", text);
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
