using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
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

        var toolBarStyle = AssertStyle<Jalium.UI.Controls.ToolBar>(dictionary);
        AssertSetter(toolBarStyle, Control.BackgroundProperty);
        AssertSetter(toolBarStyle, Control.BorderBrushProperty);
        AssertSetter(toolBarStyle, Control.BorderThicknessProperty);
        AssertSetter(toolBarStyle, Control.PaddingProperty);
        AssertSetter(toolBarStyle, Control.TemplateProperty);

        var toolBarTrayStyle = AssertStyle<ToolBarTray>(dictionary);
        AssertSetter(toolBarTrayStyle, ToolBarTray.BackgroundProperty);
        AssertSetter(toolBarTrayStyle, FrameworkElement.MinHeightProperty);

        ResetApplicationState();
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
