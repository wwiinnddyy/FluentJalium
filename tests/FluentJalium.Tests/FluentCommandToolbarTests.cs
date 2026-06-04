using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Controls.Themes;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using JaliumThemeManager = Jalium.UI.Controls.Themes.ThemeManager;

namespace FluentJalium.Tests;

[Collection("Application")]
public sealed class FluentCommandToolbarTests
{
    private static readonly string[] CommandToolbarResourceKeys =
    [
        "CommandBarBackground",
        "CommandBarBorderBrush",
        "CommandBarOverflowBackground",
        "ToolBarTrayBackground",
        "ToolBarBackground",
        "ToolBarBorderBrush",
        "ToolBarGripBrush",
        "ToolBarSeparatorForeground",
        "AppBarButtonBackground",
        "AppBarButtonBackgroundHover",
        "AppBarButtonBackgroundPressed",
        "AppBarButtonForeground",
        "AppBarButtonForegroundDisabled",
        "AppBarSeparatorForeground"
    ];

    [Theory]
    [InlineData(FluentThemeVariant.Dark)]
    [InlineData(FluentThemeVariant.Light)]
    [InlineData(FluentThemeVariant.HighContrast)]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void ThemeDictionary_ShouldExposeCommandToolbarTokensForEveryTheme(FluentThemeVariant theme)
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = theme.ToString();

        var dictionary = LoadGenericThemeDictionary();

        foreach (var key in CommandToolbarResourceKeys)
        {
            Assert.True(dictionary.TryGetValue(key, out var value), $"{key} was not found in the {theme} theme.");
            Assert.IsType<SolidColorBrush>(value);
        }

        ResetApplicationState();
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void Apply_ShouldRegisterFwCommandToolbarStylesBasedOnJaliumStyles()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            AssertBasedOnStyle<FWButton, Button>(app.Resources);
            AssertBasedOnStyle<FWRepeatButton, RepeatButton>(app.Resources);
            AssertBasedOnStyle<FWHyperlinkButton, HyperlinkButton>(app.Resources);
            AssertBasedOnStyle<FWSplitButton, SplitButton>(app.Resources);
            AssertBasedOnStyle<FWCommandBar, CommandBar>(app.Resources);
            AssertBasedOnStyle<FWAppBarButton, AppBarButton>(app.Resources);
            AssertBasedOnStyle<FWAppBarToggleButton, AppBarToggleButton>(app.Resources);
            AssertBasedOnStyle<FWAppBarSeparator, AppBarSeparator>(app.Resources);
            AssertBasedOnStyle<FWToolBar, Jalium.UI.Controls.ToolBar>(app.Resources);
            AssertBasedOnStyle<FWToolBarTray, ToolBarTray>(app.Resources);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void GenericTheme_ShouldDefineCommandToolbarBaseStylesAndFluentSetters()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = FluentThemeVariant.Dark.ToString();

        var dictionary = LoadGenericThemeDictionary();

        var commandBarStyle = AssertStyle<CommandBar>(dictionary);
        AssertSetter(commandBarStyle, Control.BackgroundProperty);
        AssertSetter(commandBarStyle, Control.BorderBrushProperty);
        AssertSetter(commandBarStyle, Control.ForegroundProperty);
        AssertSetter(commandBarStyle, FrameworkElement.MinHeightProperty);

        var buttonStyle = AssertStyle<Button>(dictionary);
        AssertSetter(buttonStyle, Control.BackgroundProperty);
        AssertSetter(buttonStyle, Control.BorderBrushProperty);
        AssertSetter(buttonStyle, Control.PaddingProperty);
        AssertSetter(buttonStyle, FrameworkElement.MinWidthProperty);
        AssertSetter(buttonStyle, FrameworkElement.MinHeightProperty);

        var fluentButtonStyle = AssertStyle<FWButton>(dictionary);
        Assert.Equal(typeof(Button), fluentButtonStyle.BasedOn?.TargetType);
        AssertSetter(fluentButtonStyle, FWButton.DensityProperty);

        var repeatButtonStyle = AssertStyle<RepeatButton>(dictionary);
        AssertSetter(repeatButtonStyle, Control.BackgroundProperty);
        AssertSetter(repeatButtonStyle, Control.BorderBrushProperty);
        AssertSetter(repeatButtonStyle, Control.PaddingProperty);

        var fluentRepeatButtonStyle = AssertStyle<FWRepeatButton>(dictionary);
        Assert.Equal(typeof(RepeatButton), fluentRepeatButtonStyle.BasedOn?.TargetType);
        AssertSetter(fluentRepeatButtonStyle, FWRepeatButton.DensityProperty);

        var hyperlinkButtonStyle = AssertStyle<HyperlinkButton>(dictionary);
        AssertSetter(hyperlinkButtonStyle, Control.ForegroundProperty);
        AssertSetter(hyperlinkButtonStyle, Control.PaddingProperty);

        var fluentHyperlinkButtonStyle = AssertStyle<FWHyperlinkButton>(dictionary);
        Assert.Equal(typeof(HyperlinkButton), fluentHyperlinkButtonStyle.BasedOn?.TargetType);
        AssertSetter(fluentHyperlinkButtonStyle, FWHyperlinkButton.DensityProperty);

        var dropDownButtonStyle = AssertStyle<FWDropDownButton>(dictionary);
        Assert.Equal(typeof(FWDropDownButton), dropDownButtonStyle.TargetType);
        Assert.Null(dropDownButtonStyle.BasedOn);
        AssertSetter(dropDownButtonStyle, nameof(FWDropDownButton.Density), "Comfortable");

        var splitButtonStyle = AssertStyle<SplitButton>(dictionary);
        var fluentSplitButtonStyle = AssertStyle<FWSplitButton>(dictionary);
        Assert.Equal(typeof(SplitButton), fluentSplitButtonStyle.BasedOn?.TargetType);
        AssertSetter(fluentSplitButtonStyle, FWSplitButton.DensityProperty);
        Assert.NotNull(splitButtonStyle);

        var toggleSplitButtonStyle = AssertStyle<FWToggleSplitButton>(dictionary);
        Assert.Equal(typeof(FWToggleSplitButton), toggleSplitButtonStyle.TargetType);
        Assert.Null(toggleSplitButtonStyle.BasedOn);
        AssertSetter(toggleSplitButtonStyle, nameof(FWToggleSplitButton.Density), "Comfortable");

        var toolBarStyle = AssertStyle<Jalium.UI.Controls.ToolBar>(dictionary);
        AssertSetter(toolBarStyle, Control.BackgroundProperty);
        AssertSetter(toolBarStyle, Control.BorderBrushProperty);
        AssertSetter(toolBarStyle, Control.BorderThicknessProperty);
        AssertSetter(toolBarStyle, Control.PaddingProperty);
        AssertSetter(toolBarStyle, Control.TemplateProperty);

        var toolBarTrayStyle = AssertStyle<ToolBarTray>(dictionary);
        AssertSetter(toolBarTrayStyle, ToolBarTray.BackgroundProperty);
        AssertSetter(toolBarTrayStyle, FrameworkElement.MinHeightProperty);

        var appBarButtonStyle = AssertStyle<AppBarButton>(dictionary);
        var fluentAppBarButtonStyle = AssertStyle<FWAppBarButton>(dictionary);
        Assert.Equal(typeof(AppBarButton), fluentAppBarButtonStyle.BasedOn?.TargetType);
        AssertSetter(appBarButtonStyle, Control.BackgroundProperty);
        AssertSetter(appBarButtonStyle, FrameworkElement.MinHeightProperty);
        AssertSetter(fluentAppBarButtonStyle, nameof(FWAppBarButton.Density), "Comfortable");

        var appBarToggleButtonStyle = AssertStyle<AppBarToggleButton>(dictionary);
        var fluentAppBarToggleButtonStyle = AssertStyle<FWAppBarToggleButton>(dictionary);
        Assert.Equal(typeof(AppBarToggleButton), fluentAppBarToggleButtonStyle.BasedOn?.TargetType);
        AssertSetter(appBarToggleButtonStyle, Control.BackgroundProperty);
        AssertSetter(appBarToggleButtonStyle, FrameworkElement.MinHeightProperty);
        AssertSetter(fluentAppBarToggleButtonStyle, nameof(FWAppBarToggleButton.Density), "Comfortable");

        var appBarSeparatorStyle = AssertStyle<AppBarSeparator>(dictionary);
        var fluentAppBarSeparatorStyle = AssertStyle<FWAppBarSeparator>(dictionary);
        Assert.Equal(typeof(AppBarSeparator), fluentAppBarSeparatorStyle.BasedOn?.TargetType);
        AssertSetter(appBarSeparatorStyle, Control.ForegroundProperty);
        AssertSetter(fluentAppBarSeparatorStyle, nameof(FWAppBarSeparator.Density), "Comfortable");

        ResetApplicationState();
    }

    [Fact]
    public void FWCoreButtonControls_ShouldApplyDensityPresets()
    {
        var button = new FWButton();

        Assert.Equal(FWButtonDensity.Comfortable, button.Density);
        Assert.Equal(32, button.MinHeight);
        Assert.Equal(64, button.MinWidth);
        Assert.Equal(new Thickness(12, 5, 12, 6), button.Padding);

        button.Density = FWButtonDensity.Compact;

        Assert.Equal(30, button.MinHeight);
        Assert.Equal(56, button.MinWidth);
        Assert.Equal(new Thickness(10, 4, 10, 5), button.Padding);

        button.Density = FWButtonDensity.Spacious;

        Assert.Equal(40, button.MinHeight);
        Assert.Equal(72, button.MinWidth);
        Assert.Equal(new Thickness(14, 8, 14, 8), button.Padding);

        var repeatButton = new FWRepeatButton
        {
            Density = FWButtonDensity.Compact
        };

        Assert.Equal(30, repeatButton.MinHeight);
        Assert.Equal(56, repeatButton.MinWidth);
        Assert.Equal(new Thickness(10, 4, 10, 5), repeatButton.Padding);

        var hyperlinkButton = new FWHyperlinkButton();

        Assert.Equal(FWButtonDensity.Comfortable, hyperlinkButton.Density);
        Assert.Equal(24, hyperlinkButton.MinHeight);
        Assert.Equal(new Thickness(0, 1, 0, 2), hyperlinkButton.Padding);

        hyperlinkButton.Density = FWButtonDensity.Spacious;

        Assert.Equal(28, hyperlinkButton.MinHeight);
        Assert.Equal(new Thickness(0, 3, 0, 3), hyperlinkButton.Padding);

        var splitButton = new FWSplitButton
        {
            Density = FWButtonDensity.Spacious
        };

        Assert.Equal(40, splitButton.MinHeight);
        Assert.Equal(72, splitButton.MinWidth);
        Assert.Equal(new Thickness(14, 8, 14, 8), splitButton.Padding);
    }

    [Fact]
    public void FWDropDownAndToggleSplitButtons_ShouldApplyDensityPresets()
    {
        var dropDownButton = new FWDropDownButton();

        Assert.Equal(FWButtonDensity.Comfortable, dropDownButton.Density);
        Assert.Equal(32, dropDownButton.MinHeight);
        Assert.Equal(72, dropDownButton.MinWidth);
        Assert.Equal(new Thickness(12, 5, 10, 6), dropDownButton.Padding);

        dropDownButton.Density = FWButtonDensity.Compact;

        Assert.Equal(30, dropDownButton.MinHeight);
        Assert.Equal(64, dropDownButton.MinWidth);
        Assert.Equal(new Thickness(10, 4, 8, 5), dropDownButton.Padding);

        dropDownButton.Density = FWButtonDensity.Spacious;

        Assert.Equal(40, dropDownButton.MinHeight);
        Assert.Equal(80, dropDownButton.MinWidth);
        Assert.Equal(new Thickness(14, 8, 12, 8), dropDownButton.Padding);

        var toggleSplitButton = new FWToggleSplitButton();

        Assert.Equal(FWButtonDensity.Comfortable, toggleSplitButton.Density);
        Assert.Equal(32, toggleSplitButton.MinHeight);
        Assert.Equal(120, toggleSplitButton.MinWidth);
        Assert.Equal(new Thickness(12, 5, 12, 6), toggleSplitButton.Padding);

        toggleSplitButton.Density = FWButtonDensity.Compact;

        Assert.Equal(30, toggleSplitButton.MinHeight);
        Assert.Equal(104, toggleSplitButton.MinWidth);
        Assert.Equal(new Thickness(10, 4, 10, 5), toggleSplitButton.Padding);

        toggleSplitButton.Density = FWButtonDensity.Spacious;

        Assert.Equal(40, toggleSplitButton.MinHeight);
        Assert.Equal(136, toggleSplitButton.MinWidth);
        Assert.Equal(new Thickness(14, 8, 14, 8), toggleSplitButton.Padding);
    }

    [Fact]
    public void FWAppBarCommandControls_ShouldApplyDensityPresets()
    {
        var appBarButton = new FWAppBarButton();

        Assert.Equal(FWButtonDensity.Comfortable, appBarButton.Density);
        Assert.Equal(48, appBarButton.MinHeight);
        Assert.Equal(40, appBarButton.MinWidth);
        Assert.Equal(new Thickness(4, 4, 4, 4), appBarButton.Padding);

        appBarButton.Density = FWButtonDensity.Compact;

        Assert.Equal(40, appBarButton.MinHeight);
        Assert.Equal(40, appBarButton.MinWidth);
        Assert.Equal(new Thickness(4, 2, 4, 2), appBarButton.Padding);

        appBarButton.Density = FWButtonDensity.Spacious;

        Assert.Equal(56, appBarButton.MinHeight);
        Assert.Equal(48, appBarButton.MinWidth);
        Assert.Equal(new Thickness(6, 6, 6, 6), appBarButton.Padding);

        var appBarToggleButton = new FWAppBarToggleButton
        {
            Density = FWButtonDensity.Spacious
        };

        Assert.Equal(56, appBarToggleButton.MinHeight);
        Assert.Equal(48, appBarToggleButton.MinWidth);
        Assert.Equal(new Thickness(6, 6, 6, 6), appBarToggleButton.Padding);

        var separator = new FWAppBarSeparator();

        Assert.Equal(FWButtonDensity.Comfortable, separator.Density);
        Assert.Equal(1, separator.Width);
        Assert.Equal(new Thickness(6, 8, 6, 8), separator.Margin);

        separator.Density = FWButtonDensity.Compact;

        Assert.Equal(1, separator.Width);
        Assert.Equal(new Thickness(4, 6, 4, 6), separator.Margin);

        separator.Density = FWButtonDensity.Spacious;

        Assert.Equal(1, separator.Width);
        Assert.Equal(new Thickness(8, 10, 8, 10), separator.Margin);
    }

    [Fact]
    public void FWCommandBar_ShouldManagePrimarySecondaryCommandsAndOpenEvents()
    {
        var commandBar = new FWCommandBar
        {
            DefaultLabelPosition = CommandBarDefaultLabelPosition.Collapsed,
            OverflowButtonVisibility = CommandBarOverflowButtonVisibility.Auto,
            IsSticky = true
        };
        var save = new FWAppBarButton { Label = "Save", DynamicOverflowOrder = 1 };
        var pin = new FWAppBarToggleButton { Label = "Pin", IsChecked = true };
        var separator = new FWAppBarSeparator();
        var settings = new FWAppBarButton { Label = "Settings" };
        var opening = 0;
        var opened = 0;
        var closing = 0;
        var closed = 0;

        commandBar.Opening += (_, _) => opening++;
        commandBar.Opened += (_, _) => opened++;
        commandBar.Closing += (_, _) => closing++;
        commandBar.Closed += (_, _) => closed++;
        commandBar.PrimaryCommands.Add(save);
        commandBar.PrimaryCommands.Add(separator);
        commandBar.PrimaryCommands.Add(pin);
        commandBar.SecondaryCommands.Add(settings);

        commandBar.Measure(new Size(480, 64));
        commandBar.IsOpen = true;
        commandBar.IsOpen = false;

        Assert.Equal(3, commandBar.PrimaryCommands.Count);
        Assert.Single(commandBar.SecondaryCommands);
        Assert.True(commandBar.IsSticky);
        Assert.Equal(CommandBarDefaultLabelPosition.Collapsed, commandBar.DefaultLabelPosition);
        Assert.True(save.IsCompact);
        Assert.True(pin.IsCompact);
        Assert.Equal(1, save.DynamicOverflowOrder);
        Assert.True(pin.IsChecked);
        Assert.False(separator.IsHitTestVisible);
        Assert.Equal(1, opening);
        Assert.Equal(1, opened);
        Assert.Equal(1, closing);
        Assert.Equal(1, closed);
    }

    [Fact]
    public void FWCommandBar_ShouldCloseWhenSecondaryCommandsAreRemoved()
    {
        var commandBar = new FWCommandBar
        {
            OverflowButtonVisibility = CommandBarOverflowButtonVisibility.Visible
        };
        commandBar.SecondaryCommands.Add(new FWAppBarButton { Label = "Settings" });
        commandBar.Measure(new Size(320, 64));

        commandBar.IsOpen = true;
        commandBar.SecondaryCommands.Clear();

        Assert.False(commandBar.IsOpen);
        Assert.Empty(commandBar.SecondaryCommands);
    }

    [Fact]
    public void FWToolBar_ShouldExposeCommandLayoutPropertiesAndOverflowMetadata()
    {
        var save = new FWButton { Content = "Save" };
        var share = new FWButton { Content = "Share" };
        var toolBar = new FWToolBar
        {
            Header = "Document",
            Band = 1,
            BandIndex = 2,
            Orientation = Orientation.Horizontal,
            IsOverflowOpen = true
        };
        toolBar.Items.Add(save);
        toolBar.Items.Add(new FWSeparator());
        toolBar.Items.Add(share);
        Jalium.UI.Controls.ToolBar.SetOverflowMode(save, OverflowMode.Never);
        Jalium.UI.Controls.ToolBar.SetOverflowMode(share, OverflowMode.Always);

        Assert.Equal("Document", toolBar.Header);
        Assert.Equal(1, toolBar.Band);
        Assert.Equal(2, toolBar.BandIndex);
        Assert.Equal(Orientation.Horizontal, toolBar.Orientation);
        Assert.True(toolBar.IsOverflowOpen);
        Assert.Equal(3, toolBar.Items.Count);
        Assert.NotNull(toolBar.ItemsPanel);
        Assert.Equal(OverflowMode.Never, Jalium.UI.Controls.ToolBar.GetOverflowMode(save));
        Assert.Equal(OverflowMode.Always, Jalium.UI.Controls.ToolBar.GetOverflowMode(share));
    }

    [Fact]
    public void FWToolBarTray_ShouldHostToolBarsByBandAndLockState()
    {
        var formatting = new FWToolBar
        {
            Header = "Formatting",
            Band = 0,
            BandIndex = 1
        };
        var document = new FWToolBar
        {
            Header = "Document",
            Band = 0,
            BandIndex = 0
        };
        var tray = new FWToolBarTray
        {
            Orientation = Orientation.Horizontal,
            IsLocked = true
        };

        tray.ToolBars.Add(formatting);
        tray.ToolBars.Add(document);

        Assert.Equal(Orientation.Horizontal, tray.Orientation);
        Assert.True(tray.IsLocked);
        Assert.Equal(2, tray.ToolBars.Count);
        Assert.Same(formatting, tray.ToolBars[0]);
        Assert.Same(document, tray.ToolBars[1]);
    }

    [Fact]
    public void FWCommandControls_ShouldExposeMaterialCommandSurfaceState()
    {
        var run = new FWButton { Content = "Run" };
        var split = new FWSplitButton
        {
            Content = "Save",
            Flyout = new MenuFlyout()
        };
        var toggleSplit = new FWToggleSplitButton
        {
            Content = "Pinned",
            IsChecked = true,
            Flyout = new MenuFlyout()
        };
        var commandBar = new FWCommandBar
        {
            DefaultLabelPosition = CommandBarDefaultLabelPosition.Bottom,
            IsSticky = true
        };
        var add = new FWAppBarButton { Label = "Add" };
        var pin = new FWAppBarToggleButton { Label = "Pin", IsChecked = true };
        commandBar.PrimaryCommands.Add(add);
        commandBar.PrimaryCommands.Add(pin);
        commandBar.SecondaryCommands.Add(new FWAppBarButton { Label = "Settings" });

        var bold = new FWButton { Content = "Bold" };
        var export = new FWButton { Content = "Export" };
        var toolBar = new FWToolBar
        {
            Header = "Format",
            Band = 0,
            BandIndex = 0
        };
        toolBar.Items.Add(bold);
        toolBar.Items.Add(new FWSeparator());
        toolBar.Items.Add(export);
        Jalium.UI.Controls.ToolBar.SetOverflowMode(export, OverflowMode.Always);
        var tray = new FWToolBarTray
        {
            Orientation = Orientation.Horizontal
        };
        tray.ToolBars.Add(toolBar);

        var panel = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Children =
            {
                run,
                split,
                toggleSplit,
                commandBar,
                tray
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

        Assert.Equal("Run", run.Content);
        Assert.NotNull(split.Flyout);
        Assert.True(toggleSplit.IsChecked);
        Assert.Equal(CommandBarDefaultLabelPosition.Bottom, commandBar.DefaultLabelPosition);
        Assert.True(commandBar.IsSticky);
        Assert.Equal(2, commandBar.PrimaryCommands.Count);
        Assert.Single(commandBar.SecondaryCommands);
        Assert.Equal("Add", add.Label);
        Assert.True(pin.IsChecked);
        Assert.Equal("Format", toolBar.Header);
        Assert.Equal(3, toolBar.Items.Count);
        Assert.Equal(OverflowMode.Always, Jalium.UI.Controls.ToolBar.GetOverflowMode(export));
        Assert.Equal(Orientation.Horizontal, tray.Orientation);
        Assert.Single(tray.ToolBars);
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

    private static void AssertSetter(Style style, DependencyProperty property, string? propertyName = null)
    {
        Assert.Contains(style.Setters, setter => setter.Property == property || setter.PropertyName == propertyName);
    }

    private static void AssertSetter(Style style, string propertyName, object? value)
    {
        Assert.Contains(style.Setters, setter =>
            (setter.PropertyName == propertyName || setter.Property?.Name == propertyName)
            && SetterValueEquals(setter.Value, value));
    }

    private static bool SetterValueEquals(object? actual, object? expected)
    {
        return Equals(actual, expected)
            || string.Equals(actual?.ToString(), expected?.ToString(), StringComparison.Ordinal);
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
