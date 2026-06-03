using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Windows.Input;
using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Controls.Themes;
using Jalium.UI.Documents;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using JaliumThemeManager = Jalium.UI.Controls.Themes.ThemeManager;

namespace FluentJalium.Tests;

[Collection("Application")]
public sealed class FluentMenuControlsTests
{
    private static readonly string[] MenuResourceKeys =
    [
        "MenuBarBackground",
        "MenuBarBorderBrush",
        "MenuBarItemBackground",
        "MenuBarItemBackgroundHover",
        "MenuBarItemBackgroundPressed",
        "MenuFlyoutPresenterBackground",
        "MenuFlyoutPresenterBorderBrush",
        "MenuFlyoutItemBackground",
        "MenuFlyoutItemBackgroundHover",
        "MenuFlyoutItemBackgroundPressed",
        "MenuFlyoutItemForegroundChecked",
        "MenuFlyoutSeparatorForeground"
    ];

    [Theory]
    [InlineData(FluentThemeVariant.Dark)]
    [InlineData(FluentThemeVariant.Light)]
    [InlineData(FluentThemeVariant.HighContrast)]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void ThemeDictionary_ShouldExposeMenuTokensForEveryTheme(FluentThemeVariant theme)
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = theme.ToString();

        var dictionary = LoadGenericThemeDictionary();

        foreach (var key in MenuResourceKeys)
        {
            Assert.True(dictionary.TryGetValue(key, out var value), $"{key} was not found in the {theme} theme.");
            Assert.IsType<SolidColorBrush>(value);
        }

        ResetApplicationState();
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void Apply_ShouldRegisterFwMenuStylesBasedOnJaliumStyles()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            AssertBasedOnStyle<FWMenuBar, MenuBar>(app.Resources);
            AssertBasedOnStyle<FWMenuBarItem, MenuBarItem>(app.Resources);
            AssertBasedOnStyle<FWMenu, Menu>(app.Resources);
            AssertBasedOnStyle<FWMenuItem, MenuItem>(app.Resources);
            AssertBasedOnStyle<FWContextMenu, ContextMenu>(app.Resources);
            AssertBasedOnStyle<FWMenuFlyoutItem, MenuFlyoutItem>(app.Resources);
            AssertBasedOnStyle<FWToggleMenuFlyoutItem, ToggleMenuFlyoutItem>(app.Resources);
            AssertBasedOnStyle<FWMenuFlyoutSeparator, MenuFlyoutSeparator>(app.Resources);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void GenericTheme_ShouldDefineMenuBaseStylesAndFluentSetters()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = FluentThemeVariant.Dark.ToString();

        var dictionary = LoadGenericThemeDictionary();

        var menuBarStyle = AssertStyle<MenuBar>(dictionary);
        AssertSetter(menuBarStyle, Control.BackgroundProperty);
        AssertSetter(menuBarStyle, Control.ForegroundProperty);
        AssertSetter(menuBarStyle, Control.MinHeightProperty);

        var menuStyle = AssertStyle<Menu>(dictionary);
        AssertSetter(menuStyle, Control.BackgroundProperty);
        AssertSetter(menuStyle, Control.ForegroundProperty);
        AssertSetter(menuStyle, Control.BorderBrushProperty);
        AssertSetter(menuStyle, Control.PaddingProperty);

        var menuBarItemStyle = AssertStyle<MenuBarItem>(dictionary);
        AssertForegroundSetter(menuBarItemStyle);
        AssertSetter(menuBarItemStyle, Control.BackgroundProperty);
        AssertSetter(menuBarItemStyle, Control.PaddingProperty);
        AssertSetter(menuBarItemStyle, Control.CornerRadiusProperty);

        var menuItemStyle = AssertStyle<MenuItem>(dictionary);
        AssertForegroundSetter(menuItemStyle);
        AssertSetter(menuItemStyle, Control.BackgroundProperty);
        AssertSetter(menuItemStyle, Control.PaddingProperty);

        var contextMenuStyle = AssertStyle<ContextMenu>(dictionary);
        AssertSetter(contextMenuStyle, Control.BackgroundProperty);
        AssertSetter(contextMenuStyle, Control.BorderBrushProperty);
        AssertSetter(contextMenuStyle, Control.PaddingProperty);
        AssertSetter(contextMenuStyle, Control.CornerRadiusProperty);

        var menuFlyoutItemStyle = AssertStyle<MenuFlyoutItem>(dictionary);
        AssertForegroundSetter(menuFlyoutItemStyle);
        AssertSetter(menuFlyoutItemStyle, Control.BackgroundProperty);
        AssertSetter(menuFlyoutItemStyle, Control.MinHeightProperty);

        var toggleMenuFlyoutItemStyle = AssertStyle<ToggleMenuFlyoutItem>(dictionary);
        AssertForegroundSetter(toggleMenuFlyoutItemStyle);
        AssertSetter(toggleMenuFlyoutItemStyle, Control.BackgroundProperty);
        AssertSetter(toggleMenuFlyoutItemStyle, Control.MinHeightProperty);

        var separatorStyle = AssertStyle<MenuFlyoutSeparator>(dictionary);
        AssertSetter(separatorStyle, Control.ForegroundProperty);
        AssertSetter(separatorStyle, FrameworkElement.MarginProperty);

        ResetApplicationState();
    }

    [Fact]
    public void FWMenu_ShouldGenerateFwMenuItemContainersForPlainItems()
    {
        var menu = new TestMenu();
        var generated = menu.CreateContainer("File");
        menu.Prepare(generated, "File");

        var ownContainer = new FWMenuItem { Header = "Edit" };

        Assert.IsType<FWMenuItem>(generated);
        Assert.Equal("File", Assert.IsType<FWMenuItem>(generated).Header);
        Assert.True(menu.IsOwnContainer(ownContainer));
        Assert.False(menu.IsOwnContainer("Plain item"));
    }

    [Fact]
    public void FWMenuItem_ShouldGenerateNestedFwMenuItemsAndRaiseCheckedSubmenuEvents()
    {
        var item = new TestMenuItem
        {
            Header = "Live preview",
            IsCheckable = true
        };
        var generated = item.CreateContainer("Nested command");
        item.Prepare(generated, "Nested command");
        var checkedCount = 0;
        var uncheckedCount = 0;
        var opened = 0;
        var closed = 0;

        item.Checked += (_, _) => checkedCount++;
        item.Unchecked += (_, _) => uncheckedCount++;
        item.SubmenuOpened += (_, _) => opened++;
        item.SubmenuClosed += (_, _) => closed++;

        item.IsChecked = true;
        item.IsChecked = false;
        item.IsSubmenuOpen = true;
        item.IsSubmenuOpen = false;

        Assert.IsType<FWMenuItem>(generated);
        Assert.Equal("Nested command", Assert.IsType<FWMenuItem>(generated).Header);
        Assert.False(item.IsChecked);
        Assert.False(item.IsSubmenuOpen);
        Assert.Equal(1, checkedCount);
        Assert.Equal(1, uncheckedCount);
        Assert.Equal(1, opened);
        Assert.Equal(1, closed);
    }

    [Fact]
    public void FWMenuBarItem_ShouldOpenAndCloseMenuAndPreserveFlyoutItems()
    {
        var menuBar = new FWMenuBar();
        var file = new FWMenuBarItem
        {
            Title = "File"
        };
        file.Items.Add(new FWMenuFlyoutItem { Text = "New", KeyboardAcceleratorTextOverride = "Ctrl+N" });
        file.Items.Add(new FWMenuFlyoutSeparator());
        file.Items.Add(new FWMenuFlyoutItem { Text = "Save", IsEnabled = false });
        menuBar.Items.Add(file);

        file.OpenMenu();
        Assert.True(file.IsMenuOpen);

        file.CloseMenu();

        Assert.False(file.IsMenuOpen);
        Assert.Contains(file, menuBar.Items);
        Assert.Equal(3, file.Items.Count);
    }

    [Fact]
    public void FWContextMenu_ShouldGenerateFwItemsOpenCloseAndKeepPlacementState()
    {
        var owner = new FWButton { Content = "Target" };
        var menu = new TestContextMenu
        {
            PlacementTarget = owner,
            Placement = PlacementMode.Bottom,
            HorizontalOffset = 4,
            VerticalOffset = 8,
            StaysOpen = true
        };
        var generated = menu.CreateContainer("Refresh");
        menu.Prepare(generated, "Refresh");
        var opened = 0;
        var closed = 0;
        menu.Opened += (_, _) => opened++;
        menu.Closed += (_, _) => closed++;
        menu.Items.Add(generated);

        menu.IsOpen = true;
        menu.Close();

        Assert.IsType<FWMenuItem>(generated);
        Assert.Equal("Refresh", Assert.IsType<FWMenuItem>(generated).Header);
        Assert.False(menu.IsOpen);
        Assert.Same(owner, menu.PlacementTarget);
        Assert.Equal(PlacementMode.Bottom, menu.Placement);
        Assert.Equal(4, menu.HorizontalOffset);
        Assert.Equal(8, menu.VerticalOffset);
        Assert.True(menu.StaysOpen);
        Assert.Equal(1, opened);
        Assert.Equal(1, closed);
    }

    [Fact]
    public void FWMenuFlyoutItems_ShouldInvokeCommandsToggleAndPreserveDisplayProperties()
    {
        var command = new RecordingCommand();
        var item = new FWMenuFlyoutItem
        {
            Text = "Open",
            Icon = "\uE8A5",
            Command = command,
            CommandParameter = "file",
            KeyboardAcceleratorTextOverride = "Ctrl+O"
        };
        var clicked = 0;
        item.Click += (_, _) => clicked++;

        InvokeMenuFlyoutItem(item);

        Assert.Equal("Open", item.Text);
        Assert.Equal("\uE8A5", item.Icon);
        Assert.Equal("Ctrl+O", item.KeyboardAcceleratorTextOverride);
        Assert.Equal(1, clicked);
        Assert.Equal(1, command.ExecuteCount);
        Assert.Equal("file", command.LastParameter);

        var toggle = new FWToggleMenuFlyoutItem { Text = "Show badges" };

        InvokeMenuFlyoutItem(toggle);

        Assert.True(toggle.IsChecked);
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

    private static void AssertForegroundSetter(Style style)
    {
        Assert.Contains(
            style.Setters,
            setter => setter.Property == Control.ForegroundProperty ||
                setter.Property == TextElement.ForegroundProperty);
    }

    private static void InvokeMenuFlyoutItem(MenuFlyoutItem item)
    {
        typeof(MenuFlyoutItem)
            .GetMethod("InvokeItem", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(item, null);
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

    private sealed class TestMenu : FWMenu
    {
        public FrameworkElement CreateContainer(object item) => GetContainerForItem(item);

        public bool IsOwnContainer(object item) => IsItemItsOwnContainer(item);

        public void Prepare(FrameworkElement element, object item) => PrepareContainerForItem(element, item);
    }

    private sealed class TestMenuItem : FWMenuItem
    {
        public FrameworkElement CreateContainer(object item) => GetContainerForItem(item);

        public void Prepare(FrameworkElement element, object item) => PrepareContainerForItem(element, item);
    }

    private sealed class TestContextMenu : FWContextMenu
    {
        public FrameworkElement CreateContainer(object item) => GetContainerForItem(item);

        public void Prepare(FrameworkElement element, object item) => PrepareContainerForItem(element, item);
    }

    private sealed class RecordingCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public int ExecuteCount { get; private set; }

        public object? LastParameter { get; private set; }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            ExecuteCount++;
            LastParameter = parameter;
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
