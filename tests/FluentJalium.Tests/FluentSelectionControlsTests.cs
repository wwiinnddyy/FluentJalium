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
public sealed class FluentSelectionControlsTests
{
    private static readonly string[] SelectionResourceKeys =
    [
        "ControlBackground",
        "ControlBackgroundHover",
        "ControlBackgroundDisabled",
        "ControlBorder",
        "ControlBorderHover",
        "ControlBorderFocused",
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
        "CheckBoxGlyphForeground",
        "CheckBoxGlyphForegroundDisabled",
        "RadioButtonDotForeground",
        "RadioButtonDotForegroundDisabled",
        "SelectionBackground",
        "SelectionBackgroundWeak",
        "ComboBoxBackground",
        "ComboBoxBackgroundPointerOver",
        "ComboBoxBackgroundPressed",
        "ComboBoxBackgroundDisabled",
        "ComboBoxForeground",
        "ComboBoxForegroundPlaceholder",
        "ComboBoxForegroundDisabled",
        "ComboBoxBorderBrush",
        "ComboBoxBorderBrushPointerOver",
        "ComboBoxBorderBrushFocused",
        "ComboBoxBorderBrushDisabled",
        "ComboBoxDropDownBackground",
        "ComboBoxDropDownBorderBrush",
        "ComboBoxDropDownGlyphForeground",
        "ComboBoxDropDownGlyphForegroundPointerOver",
        "ComboBoxDropDownGlyphForegroundDisabled",
        "ComboBoxItemBackground",
        "ComboBoxItemBackgroundPointerOver",
        "ComboBoxItemBackgroundSelected",
        "ComboBoxItemBackgroundSelectedPointerOver",
        "ComboBoxItemForeground",
        "ComboBoxItemForegroundSelected",
        "ComboBoxItemForegroundDisabled",
        "ComboBoxItemSelectedIndicator"
    ];

    [Theory]
    [InlineData(FluentThemeVariant.Dark)]
    [InlineData(FluentThemeVariant.Light)]
    [InlineData(FluentThemeVariant.HighContrast)]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void ThemeDictionary_ShouldExposeCurrentSelectionTokensForEveryTheme(FluentThemeVariant theme)
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = theme.ToString();

        var dictionary = LoadGenericThemeDictionary();

        foreach (var key in SelectionResourceKeys)
        {
            Assert.True(dictionary.TryGetValue(key, out var value), $"{key} was not found in the {theme} theme.");
            Assert.IsType<SolidColorBrush>(value);
        }

        ResetApplicationState();
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void ApplyAccent_ShouldUpdateAccentDrivenSelectionResources()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            var accent = Color.FromRgb(0x00, 0xA3, 0xC8);
            FluentThemeManager.ApplyAccent(accent);

            Assert.Equal(accent, GetBrushColor(app.Resources["AccentBrush"]));
            Assert.Equal(accent, GetBrushColor(app.Resources["ToggleCheckedBackground"]));
            Assert.Equal(accent, GetBrushColor(app.Resources["ToggleCheckedBorder"]));
            Assert.Equal(Color.FromArgb(0x66, accent.R, accent.G, accent.B), GetBrushColor(app.Resources["SelectionBackground"]));
            Assert.Equal(Color.FromArgb(0x33, accent.R, accent.G, accent.B), GetBrushColor(app.Resources["SelectionBackgroundWeak"]));
            Assert.Equal(accent, GetBrushColor(app.Resources["ComboBoxBorderBrushFocused"]));
            Assert.Equal(accent, GetBrushColor(app.Resources["ComboBoxItemSelectedIndicator"]));
            Assert.Equal(Color.FromArgb(0x33, accent.R, accent.G, accent.B), GetBrushColor(app.Resources["ComboBoxItemBackgroundSelected"]));
            Assert.Equal(Color.FromArgb(0x66, accent.R, accent.G, accent.B), GetBrushColor(app.Resources["ComboBoxItemBackgroundSelectedPointerOver"]));
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void Apply_ShouldRegisterFwSelectionStylesBasedOnJaliumStyles()
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
    public void GenericTheme_ShouldDefineSelectionBaseStylesAndFluentSetters()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = FluentThemeVariant.Dark.ToString();

        var dictionary = LoadGenericThemeDictionary();

        var checkBoxStyle = AssertStyle<CheckBox>(dictionary);
        AssertSetter(checkBoxStyle, Control.BackgroundProperty);
        AssertSetter(checkBoxStyle, Control.ForegroundProperty);
        AssertSetter(checkBoxStyle, Control.BorderBrushProperty);
        AssertSetter(checkBoxStyle, Control.BorderThicknessProperty);
        AssertSetter(checkBoxStyle, Control.CornerRadiusProperty);

        var radioButtonStyle = AssertStyle<RadioButton>(dictionary);
        AssertSetter(radioButtonStyle, Control.BackgroundProperty);
        AssertSetter(radioButtonStyle, Control.ForegroundProperty);
        AssertSetter(radioButtonStyle, Control.BorderBrushProperty);
        AssertSetter(radioButtonStyle, Control.BorderThicknessProperty);

        var comboBoxStyle = AssertStyle<ComboBox>(dictionary);
        AssertSetter(comboBoxStyle, Control.BackgroundProperty);
        AssertSetter(comboBoxStyle, Control.ForegroundProperty);
        AssertSetter(comboBoxStyle, Control.BorderBrushProperty);
        AssertSetter(comboBoxStyle, Control.BorderThicknessProperty);
        AssertSetter(comboBoxStyle, Control.CornerRadiusProperty);

        var comboBoxItemStyle = AssertStyle<ComboBoxItem>(dictionary);
        AssertSetter(comboBoxItemStyle, Control.BackgroundProperty);
        AssertSetter(comboBoxItemStyle, Control.ForegroundProperty);
        AssertSetter(comboBoxItemStyle, Control.PaddingProperty);

        ResetApplicationState();
    }

    [Fact]
    public void FWCheckBox_ShouldCycleUncheckedCheckedIndeterminateAndRaiseStateEvents()
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

        Assert.False(checkBox.IsChecked);

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
    public void FWRadioButton_ShouldKeepNamedGroupSelectionExclusive()
    {
        var groupName = $"selection-{Guid.NewGuid():N}";
        var first = new FWRadioButton
        {
            Content = "First",
            GroupName = groupName
        };
        var second = new FWRadioButton
        {
            Content = "Second",
            GroupName = groupName
        };
        var checkedCount = 0;
        var uncheckedCount = 0;

        first.Checked += (_, _) => checkedCount++;
        second.Checked += (_, _) => checkedCount++;
        first.Unchecked += (_, _) => uncheckedCount++;
        second.Unchecked += (_, _) => uncheckedCount++;

        InvokeToggleButtonClick(first);

        Assert.True(first.IsChecked);
        Assert.False(second.IsChecked);
        Assert.Equal(1, checkedCount);

        InvokeToggleButtonClick(second);

        Assert.False(first.IsChecked);
        Assert.True(second.IsChecked);
        Assert.Equal(2, checkedCount);
        Assert.Equal(1, uncheckedCount);

        InvokeToggleButtonClick(second);

        Assert.False(first.IsChecked);
        Assert.True(second.IsChecked);
        Assert.Equal(2, checkedCount);
        Assert.Equal(1, uncheckedCount);
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

        Assert.Equal("Choose an item", comboBox.SelectionBoxItem);

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
    public void FWComboBoxItem_ShouldExposeSelectionStateAndContent()
    {
        var item = new FWComboBoxItem
        {
            Content = "Option"
        };

        Assert.False(item.IsSelected);

        item.IsSelected = true;

        Assert.True(item.IsSelected);
        Assert.Equal("Option", item.Content);

        item.IsSelected = false;

        Assert.False(item.IsSelected);
    }

    [Fact]
    public void FWComboBox_ShouldGenerateFwComboBoxItemContainersForPlainItems()
    {
        var comboBox = new FWComboBox();
        comboBox.Items.Add("Fluent");
        comboBox.Items.Add(new FWComboBoxItem { Content = "Existing" });

        comboBox.Measure(new Size(240, 40));

        var itemsHost = Assert.IsAssignableFrom<Panel>(FindVisualDescendant<Panel>(comboBox, panel => panel.Children.Count >= 2));

        var generated = Assert.IsType<FWComboBoxItem>(itemsHost.Children[0]);
        Assert.Equal("Fluent", generated.Content);
        Assert.IsType<FWComboBoxItem>(itemsHost.Children[1]);
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

    private static T? FindVisualDescendant<T>(Visual visual, Func<T, bool>? predicate = null)
        where T : Visual
    {
        if (visual is T candidate && (predicate == null || predicate(candidate)))
        {
            return candidate;
        }

        for (var index = 0; index < visual.VisualChildrenCount; index++)
        {
            var child = visual.GetVisualChild(index);
            if (child != null && FindVisualDescendant(child, predicate) is { } match)
            {
                return match;
            }
        }

        return null;
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
