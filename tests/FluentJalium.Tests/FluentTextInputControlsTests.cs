using FluentJalium.Controls;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;

namespace FluentJalium.Tests;

[Collection("Application")]
public sealed class FluentTextInputControlsTests
{
    [Fact]
    public void FWTextInputControls_ShouldExposeMaterialInputPanelState()
    {
        var textBox = new FWTextBox
        {
            Text = "FluentJalium",
            PlaceholderText = "Enter text",
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap
        };
        var passwordBox = new FWPasswordBox
        {
            Password = "material",
            PlaceholderText = "Token",
            RevealMode = PasswordRevealMode.Peek,
            IsPasswordRevealed = true
        };
        var numberBox = new FWNumberBox
        {
            Density = FWNumberBoxDensity.Spacious,
            Minimum = 0,
            Maximum = 100,
            Value = 24,
            SmallChange = 4,
            LargeChange = 12,
            DecimalPlaces = 0,
            AcceptsExpression = true,
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Hidden
        };
        var autoCompleteBox = new FWAutoCompleteBox
        {
            ItemsSource = new[] { "Fluent tokens", "Fluent controls", "WinUI Gallery", "Community Toolkit" },
            Text = "Fluent",
            FilterMode = AutoCompleteFilterMode.Contains,
            MinimumPrefixLength = 1,
            PlaceholderText = "Search"
        };
        var richTextBox = new FWRichTextBox
        {
            AcceptsTab = true,
            IsSpellCheckEnabled = true
        };
        richTextBox.SetText("Layered input surfaces keep focus and text contrast.");

        var panel = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Children =
            {
                textBox,
                passwordBox,
                numberBox,
                autoCompleteBox,
                richTextBox
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
            Child = panel
        };

        Assert.Equal("FluentJalium", textBox.Text);
        Assert.Equal("Enter text", textBox.PlaceholderText);
        Assert.True(textBox.AcceptsReturn);
        Assert.Equal(TextWrapping.Wrap, textBox.TextWrapping);
        Assert.Equal("material", passwordBox.Password);
        Assert.Equal(PasswordRevealMode.Peek, passwordBox.RevealMode);
        Assert.True(passwordBox.IsPasswordRevealed);
        Assert.Equal(24, numberBox.Value);
        Assert.Equal(FWNumberBoxDensity.Spacious, numberBox.Density);
        Assert.Equal(40, numberBox.MinHeight);
        Assert.Equal(new Thickness(12, 8, 12, 8), numberBox.Padding);
        Assert.Equal(4, numberBox.SmallChange);
        Assert.Equal(12, numberBox.LargeChange);
        Assert.Equal(0, numberBox.DecimalPlaces);
        Assert.True(numberBox.AcceptsExpression);
        Assert.Equal(NumberBoxSpinButtonPlacementMode.Hidden, numberBox.SpinButtonPlacementMode);
        Assert.Equal("Fluent", autoCompleteBox.Text);
        Assert.Equal(AutoCompleteFilterMode.Contains, autoCompleteBox.FilterMode);
        Assert.Equal(2, autoCompleteBox.FilteredItems.Count);
        Assert.Contains("Fluent tokens", autoCompleteBox.FilteredItems);
        Assert.Contains("Fluent controls", autoCompleteBox.FilteredItems);
        Assert.True(richTextBox.AcceptsTab);
        Assert.True(richTextBox.IsSpellCheckEnabled);
        Assert.Contains("Layered input surfaces", richTextBox.GetText());
        Assert.Equal(FWFluentMaterialKind.LiquidGlass, surface.MaterialKind);
        Assert.True(surface.LiquidGlass);
        Assert.Equal(70, surface.RefractionAmount);
        Assert.Equal(0.42, surface.ChromaticAberration);
        Assert.Equal(24, surface.FusionRadius);
        Assert.Equal(BorderShape.SuperEllipse, surface.Shape);
        Assert.Equal(4, surface.SuperEllipseN);
        Assert.Same(panel, surface.Child);
    }
}
