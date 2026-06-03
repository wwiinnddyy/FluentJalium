using FluentJalium.Controls;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;

namespace FluentJalium.Tests;

[Collection("Application")]
public sealed class FluentStateMatrixControlsTests
{
    [Fact]
    public void FWStateMatrixControls_ShouldComposeCommonStatesInsideLiquidGlassSurface()
    {
        var commandFlyout = CreateSampleFlyout();
        var normalButton = new FWButton { Content = "Normal" };
        var dropDownButton = new FWDropDownButton
        {
            Content = "Open",
            Flyout = commandFlyout
        };
        var selectedSplitButton = new FWToggleSplitButton
        {
            Content = "Selected",
            IsChecked = true,
            Flyout = CreateSampleFlyout()
        };
        var disabledButton = new FWButton
        {
            Content = "Disabled",
            IsEnabled = false
        };
        var offToggle = new FWToggleButton { Content = "Off" };
        var onToggle = new FWToggleButton
        {
            Content = "On",
            IsChecked = true
        };
        var onSwitch = new FWToggleSwitch
        {
            Header = "On switch",
            IsOn = true
        };
        var disabledSwitch = new FWToggleSwitch
        {
            Header = "Disabled",
            IsOn = true,
            IsEnabled = false
        };
        var uncheckedBox = new FWCheckBox { Content = "Unchecked" };
        var checkedBox = new FWCheckBox
        {
            Content = "Checked",
            IsChecked = true
        };
        var mixedBox = new FWCheckBox
        {
            Content = "Indeterminate",
            IsThreeState = true,
            IsChecked = null
        };
        var disabledBox = new FWCheckBox
        {
            Content = "Disabled",
            IsChecked = true,
            IsEnabled = false
        };
        var placeholderCombo = CreateSampleComboBox("Normal", -1, true);
        var selectedCombo = CreateSampleComboBox("Selected", 1, true);
        var disabledCombo = CreateSampleComboBox("Disabled", 2, false);
        var commandRow = CreateRow(normalButton, dropDownButton, selectedSplitButton, disabledButton);
        var toggleRow = CreateRow(offToggle, onToggle, onSwitch, disabledSwitch);
        var selectionRow = CreateRow(uncheckedBox, checkedBox, mixedBox, disabledBox);
        var comboRow = CreateRow(placeholderCombo, selectedCombo, disabledCombo);
        var stack = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Children =
            {
                commandRow,
                toggleRow,
                selectionRow,
                comboRow
            }
        };
        var surface = new FWFluentMaterialSurface
        {
            MaterialKind = FWFluentMaterialKind.LiquidGlass,
            TintOpacity = 0.2,
            BlurRadius = 14,
            RefractionAmount = 70,
            ChromaticAberration = 0.42,
            FusionRadius = 24,
            Shape = BorderShape.SuperEllipse,
            SuperEllipseN = 4,
            Child = stack
        };

        Assert.Equal("Normal", normalButton.Content);
        Assert.True(dropDownButton.ShowChevronArrow);
        Assert.Same(commandFlyout, dropDownButton.Flyout);
        Assert.Equal(4, commandFlyout.Items.Count);
        Assert.True(selectedSplitButton.IsChecked);
        Assert.False(disabledButton.IsEnabled);
        Assert.False(offToggle.IsChecked);
        Assert.True(onToggle.IsChecked);
        Assert.True(onSwitch.IsOn);
        Assert.False(disabledSwitch.IsEnabled);
        Assert.False(uncheckedBox.IsChecked);
        Assert.True(checkedBox.IsChecked);
        Assert.Null(mixedBox.IsChecked);
        Assert.True(mixedBox.IsThreeState);
        Assert.False(disabledBox.IsEnabled);
        Assert.Equal(-1, placeholderCombo.SelectedIndex);
        Assert.Equal(1, selectedCombo.SelectedIndex);
        Assert.False(disabledCombo.IsEnabled);
        Assert.Equal(4, commandRow.Children.Count);
        Assert.Equal(4, toggleRow.Children.Count);
        Assert.Equal(4, selectionRow.Children.Count);
        Assert.Equal(3, comboRow.Children.Count);
        Assert.Equal(4, stack.Children.Count);
        Assert.Equal(FWFluentMaterialKind.LiquidGlass, surface.MaterialKind);
        Assert.True(surface.LiquidGlass);
        Assert.Equal(BorderShape.SuperEllipse, surface.Shape);
        Assert.Same(stack, surface.Child);
    }

    private static FWWrapPanel CreateRow(params UIElement[] elements)
    {
        var row = new FWWrapPanel
        {
            HorizontalSpacing = 10,
            VerticalSpacing = 10
        };

        foreach (var element in elements)
        {
            row.Children.Add(element);
        }

        return row;
    }

    private static FWMenuFlyout CreateSampleFlyout()
    {
        var flyout = new FWMenuFlyout();
        flyout.Items.Add(new FWMenuFlyoutItem { Text = "Create" });
        flyout.Items.Add(new FWMenuFlyoutItem { Text = "Open" });
        flyout.Items.Add(new FWMenuFlyoutSeparator());
        flyout.Items.Add(new FWMenuFlyoutItem { Text = "Export" });
        return flyout;
    }

    private static FWComboBox CreateSampleComboBox(string placeholder, int selectedIndex, bool isEnabled)
    {
        var comboBox = new FWComboBox
        {
            PlaceholderText = placeholder,
            IsEnabled = isEnabled
        };

        comboBox.Items.Add(new FWComboBoxItem { Content = "Fluent" });
        comboBox.Items.Add(new FWComboBoxItem { Content = "WinUI" });
        comboBox.Items.Add(new FWComboBoxItem { Content = "Toolkit" });
        comboBox.SelectedIndex = selectedIndex;
        return comboBox;
    }
}
