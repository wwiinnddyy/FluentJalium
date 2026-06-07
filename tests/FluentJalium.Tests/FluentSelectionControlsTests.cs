using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using FluentJalium.Icon;
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
        "ComboBoxItemSelectedIndicator",
        "RatingControlSelectedForeground",
        "RatingControlPointerOverSelectedForeground",
        "RatingControlUnselectedForeground",
        "RatingControlPointerOverUnselectedForeground",
        "RatingControlPlaceholderForeground",
        "RatingControlDisabledSelectedForeground",
        "RatingControlCaptionForeground"
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
            Assert.Equal(accent, GetBrushColor(app.Resources["RatingControlSelectedForeground"]));
            Assert.IsType<SolidColorBrush>(app.Resources["RatingControlPointerOverSelectedForeground"]);
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
            AssertOwnedStyle<FWRadioButtons>(app.Resources);
            AssertBasedOnStyle<FWComboBox, ComboBox>(app.Resources);
            AssertBasedOnStyle<FWComboBoxItem, ComboBoxItem>(app.Resources);
            AssertOwnedStyle<FWRatingControl>(app.Resources);
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
        AssertSetter(checkBoxStyle, FrameworkElement.MinHeightProperty);
        AssertSetter(checkBoxStyle, Control.PaddingProperty);

        var fluentCheckBoxStyle = AssertStyle<FWCheckBox>(dictionary);
        Assert.Equal(typeof(CheckBox), fluentCheckBoxStyle.BasedOn?.TargetType);
        AssertSetter(fluentCheckBoxStyle, FWCheckBox.DensityProperty);

        var radioButtonStyle = AssertStyle<RadioButton>(dictionary);
        AssertSetter(radioButtonStyle, Control.BackgroundProperty);
        AssertSetter(radioButtonStyle, Control.ForegroundProperty);
        AssertSetter(radioButtonStyle, Control.BorderBrushProperty);
        AssertSetter(radioButtonStyle, Control.BorderThicknessProperty);
        AssertSetter(radioButtonStyle, FrameworkElement.MinHeightProperty);
        AssertSetter(radioButtonStyle, Control.PaddingProperty);

        var fluentRadioButtonStyle = AssertStyle<FWRadioButton>(dictionary);
        Assert.Equal(typeof(RadioButton), fluentRadioButtonStyle.BasedOn?.TargetType);
        AssertSetter(fluentRadioButtonStyle, FWRadioButton.DensityProperty);

        var radioButtonsStyle = AssertStyle<FWRadioButtons>(dictionary);
        AssertSetter(radioButtonsStyle, Control.ForegroundProperty);
        AssertSetter(radioButtonsStyle, Control.FontFamilyProperty);
        AssertSetter(radioButtonsStyle, Control.FontSizeProperty);
        AssertSetter(radioButtonsStyle, Control.PaddingProperty);
        AssertSetter(radioButtonsStyle, FrameworkElement.MinHeightProperty);
        AssertSetter(radioButtonsStyle, FWRadioButtons.DensityProperty);

        var comboBoxStyle = AssertStyle<ComboBox>(dictionary);
        AssertSetter(comboBoxStyle, Control.BackgroundProperty);
        AssertSetter(comboBoxStyle, Control.ForegroundProperty);
        AssertSetter(comboBoxStyle, Control.BorderBrushProperty);
        AssertSetter(comboBoxStyle, Control.BorderThicknessProperty);
        AssertSetter(comboBoxStyle, Control.CornerRadiusProperty);

        var fluentComboBoxStyle = AssertStyle<FWComboBox>(dictionary);
        Assert.Same(comboBoxStyle, fluentComboBoxStyle.BasedOn);
        AssertSetter(fluentComboBoxStyle, FWComboBox.DensityProperty);

        var comboBoxItemStyle = AssertStyle<ComboBoxItem>(dictionary);
        AssertSetter(comboBoxItemStyle, Control.BackgroundProperty);
        AssertSetter(comboBoxItemStyle, Control.ForegroundProperty);
        AssertSetter(comboBoxItemStyle, Control.PaddingProperty);

        var fluentComboBoxItemStyle = AssertStyle<FWComboBoxItem>(dictionary);
        Assert.Same(comboBoxItemStyle, fluentComboBoxItemStyle.BasedOn);
        AssertSetter(fluentComboBoxItemStyle, FWComboBoxItem.DensityProperty);

        var ratingStyle = AssertStyle<FWRatingControl>(dictionary);
        AssertSetter(ratingStyle, Control.ForegroundProperty);
        AssertSetter(ratingStyle, FWRatingControl.GlyphFontFamilyProperty);
        AssertSetter(ratingStyle, FWRatingControl.GlyphProperty);
        AssertSetter(ratingStyle, FWRatingControl.UnsetGlyphProperty);
        AssertSetter(ratingStyle, FWRatingControl.RatingSizeProperty);
        AssertSetter(ratingStyle, FWRatingControl.RatingItemFontSizeProperty);
        AssertSetter(ratingStyle, FWRatingControl.ItemSpacingProperty);

        ResetApplicationState();
    }

    [Fact]
    public void FWSelectionControls_ShouldApplyDensityAndRatingSizePresets()
    {
        var checkBox = new FWCheckBox();

        Assert.Equal(FWSelectionDensity.Comfortable, checkBox.Density);
        Assert.Equal(24, checkBox.MinHeight);
        Assert.Equal(new Thickness(8, 0, 0, 0), checkBox.Padding);

        checkBox.Density = FWSelectionDensity.Compact;

        Assert.Equal(22, checkBox.MinHeight);
        Assert.Equal(new Thickness(6, 0, 0, 0), checkBox.Padding);

        checkBox.Density = FWSelectionDensity.Spacious;

        Assert.Equal(32, checkBox.MinHeight);
        Assert.Equal(new Thickness(10, 0, 0, 0), checkBox.Padding);

        var radioButton = new FWRadioButton
        {
            Density = FWSelectionDensity.Compact
        };

        Assert.Equal(22, radioButton.MinHeight);
        Assert.Equal(new Thickness(6, 0, 0, 0), radioButton.Padding);

        radioButton.Density = FWSelectionDensity.Spacious;

        Assert.Equal(32, radioButton.MinHeight);
        Assert.Equal(new Thickness(10, 0, 0, 0), radioButton.Padding);

        var radioButtons = new FWRadioButtons();

        Assert.IsAssignableFrom<IFluentJaliumControl>(radioButtons);
        Assert.Equal(FWSelectionDensity.Comfortable, radioButtons.Density);
        Assert.Equal(-1, radioButtons.SelectedIndex);
        Assert.Null(radioButtons.SelectedItem);
        Assert.Null(radioButtons.SelectedValue);
        Assert.Null(radioButtons.Header);
        Assert.Null(radioButtons.HeaderTemplate);
        Assert.Equal(24, radioButtons.MinHeight);
        Assert.Equal(new Thickness(0, 4, 0, 4), radioButtons.Padding);

        radioButtons.Density = FWSelectionDensity.Compact;

        Assert.Equal(22, radioButtons.MinHeight);
        Assert.Equal(new Thickness(0, 2, 0, 2), radioButtons.Padding);

        radioButtons.Density = FWSelectionDensity.Spacious;

        Assert.Equal(32, radioButtons.MinHeight);
        Assert.Equal(new Thickness(0, 6, 0, 6), radioButtons.Padding);

        var comboBox = new FWComboBox();

        Assert.Equal(FWSelectionDensity.Comfortable, comboBox.Density);
        Assert.Equal(34, comboBox.MinHeight);
        Assert.Equal(120, comboBox.MinWidth);
        Assert.Equal(new Thickness(10, 5, 8, 6), comboBox.Padding);

        comboBox.Density = FWSelectionDensity.Compact;

        Assert.Equal(30, comboBox.MinHeight);
        Assert.Equal(120, comboBox.MinWidth);
        Assert.Equal(new Thickness(8, 4, 8, 5), comboBox.Padding);

        comboBox.Density = FWSelectionDensity.Spacious;

        Assert.Equal(40, comboBox.MinHeight);
        Assert.Equal(144, comboBox.MinWidth);
        Assert.Equal(new Thickness(12, 8, 10, 8), comboBox.Padding);

        var comboBoxItem = new FWComboBoxItem();

        Assert.Equal(FWSelectionDensity.Comfortable, comboBoxItem.Density);
        Assert.Equal(32, comboBoxItem.MinHeight);
        Assert.Equal(new Thickness(9, 6, 10, 6), comboBoxItem.Padding);

        comboBoxItem.Density = FWSelectionDensity.Compact;

        Assert.Equal(28, comboBoxItem.MinHeight);
        Assert.Equal(new Thickness(8, 4, 8, 4), comboBoxItem.Padding);

        comboBoxItem.Density = FWSelectionDensity.Spacious;

        Assert.Equal(38, comboBoxItem.MinHeight);
        Assert.Equal(new Thickness(12, 8, 12, 8), comboBoxItem.Padding);

        var rating = new FWRatingControl();

        Assert.Equal(FWRatingControlSize.Medium, rating.RatingSize);
        Assert.Equal(20, rating.RatingItemFontSize);
        Assert.Equal(8, rating.ItemSpacing);
        Assert.Equal(24, rating.MinHeight);

        rating.RatingSize = FWRatingControlSize.Small;

        Assert.Equal(16, rating.RatingItemFontSize);
        Assert.Equal(6, rating.ItemSpacing);
        Assert.Equal(20, rating.MinHeight);

        rating.RatingSize = FWRatingControlSize.Large;

        Assert.Equal(24, rating.RatingItemFontSize);
        Assert.Equal(10, rating.ItemSpacing);
        Assert.Equal(30, rating.MinHeight);
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
    public void FWRadioButtons_ShouldSynchronizeGroupedSelection()
    {
        var radioButtons = new FWRadioButtons
        {
            Header = "Options"
        };
        radioButtons.Items.Add("First");
        radioButtons.Items.Add("Second");
        radioButtons.Items.Add("Third");

        var selectionChanged = 0;
        radioButtons.SelectionChanged += (_, _) => selectionChanged++;

        radioButtons.Measure(new Size(240, 120));

        var itemsHost = Assert.IsAssignableFrom<Panel>(FindVisualDescendant<Panel>(radioButtons, panel => panel.Children.Count == 3));

        var first = Assert.IsType<FWRadioButton>(itemsHost.Children[0]);
        var second = Assert.IsType<FWRadioButton>(itemsHost.Children[1]);
        var third = Assert.IsType<FWRadioButton>(itemsHost.Children[2]);

        Assert.Equal("First", first.Content);
        Assert.Equal(FWSelectionDensity.Comfortable, first.Density);
        Assert.False(first.IsChecked);

        radioButtons.SelectedIndex = 1;

        Assert.False(first.IsChecked);
        Assert.True(second.IsChecked);
        Assert.False(third.IsChecked);
        Assert.Equal("Second", radioButtons.SelectedItem);
        Assert.Equal("Second", radioButtons.SelectedValue);
        Assert.Equal(1, selectionChanged);

        InvokeToggleButtonClick(third);

        Assert.False(second.IsChecked);
        Assert.True(third.IsChecked);
        Assert.Equal(2, radioButtons.SelectedIndex);
        Assert.Equal("Third", radioButtons.SelectedItem);
        Assert.Equal("Third", radioButtons.SelectedValue);
        Assert.Equal(2, selectionChanged);

        radioButtons.Density = FWSelectionDensity.Compact;

        Assert.Equal(FWSelectionDensity.Compact, first.Density);
        Assert.Equal(FWSelectionDensity.Compact, third.Density);
    }

    [Fact]
    public void FWRadioButtons_ShouldHostExistingFwRadioButtonItems()
    {
        var first = new FWRadioButton { Content = "First" };
        var second = new FWRadioButton { Content = "Second" };
        var radioButtons = new FWRadioButtons
        {
            Density = FWSelectionDensity.Spacious
        };
        radioButtons.Items.Add(first);
        radioButtons.Items.Add(second);

        radioButtons.Measure(new Size(240, 80));

        radioButtons.SelectedItem = second;

        Assert.False(first.IsChecked);
        Assert.True(second.IsChecked);
        Assert.Equal(1, radioButtons.SelectedIndex);
        Assert.Same(second, radioButtons.SelectedItem);
        Assert.Same(second, radioButtons.SelectedValue);
        Assert.Equal(FWSelectionDensity.Spacious, first.Density);
        Assert.Equal(FWSelectionDensity.Spacious, second.Density);

        InvokeToggleButtonClick(first);

        Assert.Equal(0, radioButtons.SelectedIndex);
        Assert.Same(first, radioButtons.SelectedItem);
        Assert.True(first.IsChecked);
        Assert.False(second.IsChecked);
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
    public void FWCheckBox_ShouldSupportSelectAllAggregateThreeStatePattern()
    {
        var selectAll = new FWCheckBox
        {
            Content = "Select all",
            IsThreeState = true,
            IsChecked = null
        };
        var fluent = new FWCheckBox { Content = "Fluent", IsChecked = true };
        var controls = new FWCheckBox { Content = "Controls" };
        var gallery = new FWCheckBox { Content = "Gallery", IsChecked = true };
        var children = new[] { fluent, controls, gallery };

        static void UpdateSelectAll(FWCheckBox aggregate, FWCheckBox[] items)
        {
            var checkedCount = items.Count(checkBox => checkBox.IsChecked == true);
            aggregate.IsChecked = checkedCount == 0 ? false : checkedCount == items.Length ? true : null;
        }

        UpdateSelectAll(selectAll, children);

        Assert.Null(selectAll.IsChecked);

        foreach (var child in children)
        {
            child.IsChecked = true;
        }
        UpdateSelectAll(selectAll, children);

        Assert.True(selectAll.IsChecked);

        foreach (var child in children)
        {
            child.IsChecked = false;
        }
        UpdateSelectAll(selectAll, children);

        Assert.False(selectAll.IsChecked);
    }

    [Fact]
    public void FWComboBox_ShouldExposeEditableAndItemStatePropertiesForSelectionGallery()
    {
        var comboBox = new FWComboBox
        {
            PlaceholderText = "Type or select",
            IsEditable = true,
            StaysOpenOnEdit = true,
            Text = "Custom value"
        };
        comboBox.Items.Add("Custom value");
        comboBox.Items.Add("Preset A");
        comboBox.Items.Add("Preset B");

        var selectedItem = new FWComboBoxItem
        {
            Content = "Selected item",
            IsSelected = true
        };
        var disabledItem = new FWComboBoxItem
        {
            Content = "Disabled item",
            IsEnabled = false
        };

        Assert.True(comboBox.IsEditable);
        Assert.True(comboBox.StaysOpenOnEdit);
        Assert.Equal("Custom value", comboBox.Text);
        Assert.Equal("Custom value", comboBox.SelectionBoxItem);

        comboBox.SelectedIndex = 2;

        Assert.Equal("Preset B", comboBox.SelectedItem);
        Assert.Equal("Preset B", comboBox.SelectionBoxItem);
        Assert.True(selectedItem.IsSelected);
        Assert.False(disabledItem.IsEnabled);
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
        var comboBox = new FWComboBox
        {
            Density = FWSelectionDensity.Spacious
        };
        comboBox.Items.Add("Fluent");
        comboBox.Items.Add(new FWComboBoxItem { Content = "Existing" });

        comboBox.Measure(new Size(240, 40));

        var itemsHost = Assert.IsAssignableFrom<Panel>(FindVisualDescendant<Panel>(comboBox, panel => panel.Children.Count >= 2));

        var generated = Assert.IsType<FWComboBoxItem>(itemsHost.Children[0]);
        Assert.Equal("Fluent", generated.Content);
        Assert.Equal(FWSelectionDensity.Spacious, generated.Density);
        Assert.IsType<FWComboBoxItem>(itemsHost.Children[1]);
    }

    [Fact]
    public void FWRatingControl_ShouldExposeWinUiStyleRatingStateAndEvents()
    {
        var rating = new FWRatingControl
        {
            Value = 3,
            PlaceholderValue = 4,
            Caption = "Quality",
            MaxRating = 5,
            IsClearEnabled = true
        };
        FWRatingControlValueChangedEventArgs? lastArgs = null;
        var changed = 0;
        rating.ValueChanged += (_, args) =>
        {
            changed++;
            lastArgs = args;
        };

        rating.Value = 6;

        Assert.Equal(5, rating.Value);
        Assert.Equal(1, changed);
        Assert.Equal(3, lastArgs!.OldValue);
        Assert.Equal(5, lastArgs.NewValue);

        rating.Clear();

        Assert.Equal(FWRatingControl.UnsetValue, rating.Value);
        Assert.Equal(2, changed);
        Assert.Equal("Quality", rating.Caption);
        Assert.Equal(4, rating.PlaceholderValue);

        rating.Value = 2;
        rating.IsReadOnly = true;
        rating.Clear();

        Assert.Equal(2, rating.Value);
        Assert.Equal(3, changed);
    }

    [Fact]
    public void FWRatingControl_ShouldExposeGalleryMaterialState()
    {
        var rating = new FWRatingControl
        {
            Value = 4,
            Caption = "Fit",
            RatingSize = FWRatingControlSize.Large,
            GlyphFontFamily = FluentIconFonts.Regular
        };
        var surface = new FWFluentMaterialSurface
        {
            MaterialKind = FWFluentMaterialKind.LiquidGlass,
            TintOpacity = 0.2,
            BlurRadius = 14,
            RefractionAmount = 70,
            ChromaticAberration = 0.42,
            FusionRadius = 24,
            Child = rating
        };

        Assert.Equal(4, rating.Value);
        Assert.Equal("Fit", rating.Caption);
        Assert.Equal(FWRatingControlSize.Large, rating.RatingSize);
        Assert.Equal(24, rating.RatingItemFontSize);
        Assert.Equal(10, rating.ItemSpacing);
        Assert.Equal(FluentIconFonts.Regular, rating.GlyphFontFamily);
        Assert.Equal(FWFluentMaterialKind.LiquidGlass, surface.MaterialKind);
        Assert.Same(rating, surface.Child);
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

    private static void AssertOwnedStyle<TFluentControl>(ResourceDictionary dictionary)
        where TFluentControl : FrameworkElement, IFluentJaliumControl
    {
        var fluentStyle = AssertStyle<TFluentControl>(dictionary);

        Assert.Equal(typeof(TFluentControl), fluentStyle.TargetType);
        Assert.Null(fluentStyle.BasedOn);
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
