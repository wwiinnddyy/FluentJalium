using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Themes;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Documents;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using JaliumThemeManager = Jalium.UI.Controls.Themes.ThemeManager;

namespace FluentJalium.Tests;

[Collection("Application")]
public sealed class FluentDisclosureControlsTests
{
    private static readonly string[] DisclosureResourceKeys =
    [
        "ToolTipBackground",
        "ToolTipForeground",
        "ToolTipBorderBrush",
        "GroupBoxBackground",
        "GroupBoxBorderBrush",
        "GroupBoxHeaderBackground",
        "ExpanderBackground",
        "ExpanderBorderBrush",
        "ExpanderBorderBrushHover",
        "ExpanderHeaderBackground",
        "ExpanderHeaderBackgroundHover",
        "ExpanderHeaderBackgroundExpanded",
        "ExpanderChevronForeground",
        "ContentDialogBackground",
        "ContentDialogBorderBrush",
        "ContentDialogOverlayBackground",
        "ContentDialogTitleForeground",
        "ContentDialogButtonPanelBackground"
    ];

    [Theory]
    [InlineData(FluentThemeVariant.Dark)]
    [InlineData(FluentThemeVariant.Light)]
    [InlineData(FluentThemeVariant.HighContrast)]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void ThemeDictionary_ShouldExposeDisclosureTokensForEveryTheme(FluentThemeVariant theme)
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = theme.ToString();

        var dictionary = LoadGenericThemeDictionary();

        foreach (var key in DisclosureResourceKeys)
        {
            Assert.True(dictionary.TryGetValue(key, out var value), $"{key} was not found in the {theme} theme.");
            Assert.IsType<SolidColorBrush>(value);
        }

        ResetApplicationState();
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void Apply_ShouldRegisterFwDisclosureStylesBasedOnJaliumStyles()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            AssertBasedOnStyle<FWExpander, Expander>(app.Resources);
            AssertBasedOnStyle<FWToolTip, ToolTip>(app.Resources);
            AssertBasedOnStyle<FWContentDialog, ContentDialog>(app.Resources);
            AssertBasedOnStyle<FWGroupBox, GroupBox>(app.Resources);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void DisclosureTheme_ShouldDefineBaseStylesAndFluentSetters()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = FluentThemeVariant.Dark.ToString();

        var dictionary = LoadDisclosureControlThemeDictionary();

        var toolTipStyle = AssertStyle<ToolTip>(dictionary);
        AssertSetter(toolTipStyle, Control.BackgroundProperty);
        AssertSetter(toolTipStyle, Control.ForegroundProperty);
        AssertSetter(toolTipStyle, Control.BorderBrushProperty);
        AssertSetter(toolTipStyle, Control.PaddingProperty);

        var fwToolTipStyle = AssertStyle<FWToolTip>(dictionary);
        Assert.Same(toolTipStyle, fwToolTipStyle.BasedOn);
        AssertSetter(fwToolTipStyle, FWToolTip.DensityProperty);

        var groupBoxStyle = AssertStyle<GroupBox>(dictionary);
        AssertSetter(groupBoxStyle, Control.BackgroundProperty);
        AssertSetter(groupBoxStyle, Control.BorderBrushProperty);
        AssertSetter(groupBoxStyle, GroupBox.HeaderBackgroundProperty);
        AssertSetter(groupBoxStyle, Control.PaddingProperty);

        var fwGroupBoxStyle = AssertStyle<FWGroupBox>(dictionary);
        Assert.Same(groupBoxStyle, fwGroupBoxStyle.BasedOn);
        AssertSetter(fwGroupBoxStyle, FWGroupBox.DensityProperty);

        var expanderStyle = AssertStyle<Expander>(dictionary);
        AssertSetter(expanderStyle, Control.BackgroundProperty);
        AssertSetter(expanderStyle, Control.BorderBrushProperty);
        AssertSetter(expanderStyle, Expander.HeaderBackgroundProperty);
        AssertSetter(expanderStyle, Control.PaddingProperty);
        AssertSetter(expanderStyle, FrameworkElement.MinHeightProperty);

        var fwExpanderStyle = AssertStyle<FWExpander>(dictionary);
        Assert.Same(expanderStyle, fwExpanderStyle.BasedOn);
        AssertSetter(fwExpanderStyle, FWExpander.DensityProperty);

        var contentDialogStyle = AssertStyle<ContentDialog>(dictionary);
        AssertSetter(contentDialogStyle, Control.BackgroundProperty);
        AssertSetter(contentDialogStyle, Control.BorderBrushProperty);
        AssertSetter(contentDialogStyle, Control.PaddingProperty);
        AssertSetter(contentDialogStyle, FrameworkElement.MinWidthProperty);
        AssertSetter(contentDialogStyle, FrameworkElement.MaxWidthProperty);

        var fwContentDialogStyle = AssertStyle<FWContentDialog>(dictionary);
        Assert.Same(contentDialogStyle, fwContentDialogStyle.BasedOn);
        AssertSetter(fwContentDialogStyle, FWContentDialog.DensityProperty);

        ResetApplicationState();
    }

    [Fact]
    public void FWDisclosureControls_ShouldApplyDensityPresets()
    {
        var expander = new FWExpander();

        Assert.Equal(FWDisclosureDensity.Comfortable, expander.Density);
        Assert.Equal(new Thickness(14), expander.Padding);
        Assert.Equal(40, expander.MinHeight);

        expander.Density = FWDisclosureDensity.Compact;

        Assert.Equal(new Thickness(10), expander.Padding);
        Assert.Equal(36, expander.MinHeight);

        expander.Density = FWDisclosureDensity.Spacious;

        Assert.Equal(new Thickness(18), expander.Padding);
        Assert.Equal(48, expander.MinHeight);

        var toolTip = new FWToolTip
        {
            Density = FWDisclosureDensity.Compact
        };

        Assert.Equal(new Thickness(6, 3, 6, 3), toolTip.Padding);
        Assert.Equal(24, toolTip.MinHeight);

        toolTip.Density = FWDisclosureDensity.Spacious;

        Assert.Equal(new Thickness(10, 7, 10, 7), toolTip.Padding);
        Assert.Equal(32, toolTip.MinHeight);

        var dialog = new FWContentDialog();

        Assert.Equal(FWDisclosureDensity.Comfortable, dialog.Density);
        Assert.Equal(new Thickness(24), dialog.Padding);
        Assert.Equal(320, dialog.MinWidth);
        Assert.Equal(548, dialog.MaxWidth);

        dialog.Density = FWDisclosureDensity.Compact;

        Assert.Equal(new Thickness(20), dialog.Padding);
        Assert.Equal(300, dialog.MinWidth);
        Assert.Equal(520, dialog.MaxWidth);

        dialog.Density = FWDisclosureDensity.Spacious;

        Assert.Equal(new Thickness(28), dialog.Padding);
        Assert.Equal(340, dialog.MinWidth);
        Assert.Equal(600, dialog.MaxWidth);

        var groupBox = new FWGroupBox
        {
            Density = FWDisclosureDensity.Spacious
        };

        Assert.Equal(new Thickness(18, 20, 18, 18), groupBox.Padding);
        Assert.Equal(72, groupBox.MinHeight);

        groupBox.Density = FWDisclosureDensity.Compact;

        Assert.Equal(new Thickness(10, 12, 10, 10), groupBox.Padding);
        Assert.Equal(48, groupBox.MinHeight);
    }

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

    private static ResourceDictionary LoadGenericThemeDictionary()
    {
        var loaded = ResourceDictionary.SourceLoader?.Invoke(
            new ResourceDictionary(),
            new Uri("/FluentJalium;component/Themes/Generic.jalxaml", UriKind.Relative),
            FluentThemeManager.ThemeAssembly);

        return Assert.IsType<ResourceDictionary>(loaded);
    }

    private static ResourceDictionary LoadDisclosureControlThemeDictionary()
    {
        var loaded = ResourceDictionary.SourceLoader?.Invoke(
            new ResourceDictionary(),
            new Uri("/FluentJalium;component/Themes/Controls/DisclosureControls.jalxaml", UriKind.Relative),
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
