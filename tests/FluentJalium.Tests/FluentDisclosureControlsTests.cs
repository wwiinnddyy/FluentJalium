using FluentJalium.Controls;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Media;

namespace FluentJalium.Tests;

[Collection("Application")]
public sealed class FluentDisclosureControlsTests
{
    [Fact]
    public void FWDisclosureControls_ShouldComposeInsideLiquidGlassPanel()
    {
        var headerBackground = new SolidColorBrush(Color.FromRgb(0x22, 0x22, 0x22));
        var expander = new FWExpander
        {
            Header = "Surface options",
            HeaderBackground = headerBackground,
            IsExpanded = true,
            ExpandDirection = ExpandDirection.Down,
            Content = new TextBlock { Text = "Expanded material content" }
        };
        var groupContent = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Children =
            {
                new FWCheckBox { Content = "Enable reveal hints", IsChecked = true },
                new FWTextBox { Text = "LiquidGlass" }
            }
        };
        var groupBox = new FWGroupBox
        {
            Header = "Material settings",
            HeaderBackground = headerBackground,
            Padding = new Thickness(12),
            Content = groupContent
        };
        var target = new FWButton { Content = "Tip" };
        var toolTip = new FWToolTip
        {
            Content = "LiquidGlass tooltip",
            PlacementTarget = target,
            Placement = PlacementMode.Top,
            InitialShowDelay = 200,
            ShowDuration = int.MaxValue
        };
        target.ToolTip = toolTip;
        var dialog = new FWContentDialog
        {
            Title = "Save gallery changes?",
            Content = "FWContentDialog uses Fluent dialog resources.",
            PrimaryButtonText = "Save",
            SecondaryButtonText = "Review",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            IsPrimaryButtonEnabled = true,
            IsSecondaryButtonEnabled = true,
            FullSizeDesired = false
        };
        var panel = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Children =
            {
                expander,
                groupBox,
                target
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

        Assert.True(expander.IsExpanded);
        Assert.Equal(ExpandDirection.Down, expander.ExpandDirection);
        Assert.Same(headerBackground, expander.HeaderBackground);
        Assert.Equal("Surface options", expander.Header);
        Assert.Equal("Material settings", groupBox.Header);
        Assert.Same(headerBackground, groupBox.HeaderBackground);
        Assert.Same(groupContent, groupBox.Content);
        Assert.Equal(12, groupBox.Padding.Left);
        Assert.Same(target, toolTip.PlacementTarget);
        Assert.Equal(PlacementMode.Top, toolTip.Placement);
        Assert.Equal(200, toolTip.InitialShowDelay);
        Assert.Equal(int.MaxValue, toolTip.ShowDuration);
        Assert.Same(toolTip, target.ToolTip);
        Assert.Equal("Save gallery changes?", dialog.Title);
        Assert.Equal("Save", dialog.PrimaryButtonText);
        Assert.Equal("Review", dialog.SecondaryButtonText);
        Assert.Equal("Cancel", dialog.CloseButtonText);
        Assert.Equal(ContentDialogButton.Primary, dialog.DefaultButton);
        Assert.True(dialog.IsPrimaryButtonEnabled);
        Assert.True(dialog.IsSecondaryButtonEnabled);
        Assert.False(dialog.FullSizeDesired);
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
