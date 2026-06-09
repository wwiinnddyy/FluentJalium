using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Windows.Input;
using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using FluentJalium.Gallery.Pages;
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
            AssertBasedOnStyle<FWMenuFlyoutSubItem, MenuFlyoutItem>(app.Resources);
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

        var fwMenuBarStyle = AssertStyle<FWMenuBar>(dictionary);
        AssertSetter(fwMenuBarStyle, FWMenuBar.DensityProperty);

        var menuStyle = AssertStyle<Menu>(dictionary);
        AssertSetter(menuStyle, Control.BackgroundProperty);
        AssertSetter(menuStyle, Control.ForegroundProperty);
        AssertSetter(menuStyle, Control.BorderBrushProperty);
        AssertSetter(menuStyle, Control.PaddingProperty);

        var fwMenuStyle = AssertStyle<FWMenu>(dictionary);
        AssertSetter(fwMenuStyle, FWMenu.DensityProperty);

        var menuBarItemStyle = AssertStyle<MenuBarItem>(dictionary);
        AssertForegroundSetter(menuBarItemStyle);
        AssertSetter(menuBarItemStyle, Control.BackgroundProperty);
        AssertSetter(menuBarItemStyle, Control.PaddingProperty);
        AssertSetter(menuBarItemStyle, Control.CornerRadiusProperty);

        var fwMenuBarItemStyle = AssertStyle<FWMenuBarItem>(dictionary);
        AssertSetter(fwMenuBarItemStyle, FWMenuBarItem.DensityProperty);

        var menuItemStyle = AssertStyle<MenuItem>(dictionary);
        AssertForegroundSetter(menuItemStyle);
        AssertSetter(menuItemStyle, Control.BackgroundProperty);
        AssertSetter(menuItemStyle, Control.PaddingProperty);

        var fwMenuItemStyle = AssertStyle<FWMenuItem>(dictionary);
        AssertSetter(fwMenuItemStyle, FWMenuItem.DensityProperty);

        var contextMenuStyle = AssertStyle<ContextMenu>(dictionary);
        AssertSetter(contextMenuStyle, Control.BackgroundProperty);
        AssertSetter(contextMenuStyle, Control.BorderBrushProperty);
        AssertSetter(contextMenuStyle, Control.PaddingProperty);
        AssertSetter(contextMenuStyle, Control.CornerRadiusProperty);

        var fwContextMenuStyle = AssertStyle<FWContextMenu>(dictionary);
        AssertSetter(fwContextMenuStyle, FWContextMenu.DensityProperty);

        Assert.True(dictionary.TryGetValue("FWFlyoutPresenterStyle", out var flyoutPresenterStyle));
        var flyoutStyle = Assert.IsType<Style>(flyoutPresenterStyle);
        Assert.Equal(typeof(FWFlyoutPresenter), flyoutStyle.TargetType);
        Assert.False(
            dictionary.Contains(typeof(FWFlyout)),
            "FWFlyout chrome is intentionally styled through FWFlyoutPresenterStyle because the flyout itself is a FlyoutBase, not the popup chrome.");
        AssertSetter(flyoutStyle, Control.BackgroundProperty);
        AssertSetter(flyoutStyle, Control.BorderBrushProperty);
        AssertSetter(flyoutStyle, Control.PaddingProperty);
        AssertSetter(flyoutStyle, FWFlyoutPresenter.DensityProperty);

        var menuFlyoutItemStyle = AssertStyle<MenuFlyoutItem>(dictionary);
        AssertForegroundSetter(menuFlyoutItemStyle);
        AssertSetter(menuFlyoutItemStyle, Control.BackgroundProperty);
        AssertSetter(menuFlyoutItemStyle, Control.MinHeightProperty);

        var fwMenuFlyoutItemStyle = AssertStyle<FWMenuFlyoutItem>(dictionary);
        AssertSetter(fwMenuFlyoutItemStyle, FluentMenuFlyoutItemBase.DensityProperty);

        var toggleMenuFlyoutItemStyle = AssertStyle<ToggleMenuFlyoutItem>(dictionary);
        AssertForegroundSetter(toggleMenuFlyoutItemStyle);
        AssertSetter(toggleMenuFlyoutItemStyle, Control.BackgroundProperty);
        AssertSetter(toggleMenuFlyoutItemStyle, Control.MinHeightProperty);

        var fwToggleMenuFlyoutItemStyle = AssertStyle<FWToggleMenuFlyoutItem>(dictionary);
        AssertSetter(fwToggleMenuFlyoutItemStyle, FluentToggleMenuFlyoutItemBase.DensityProperty);

        var separatorStyle = AssertStyle<MenuFlyoutSeparator>(dictionary);
        AssertSetter(separatorStyle, Control.ForegroundProperty);
        AssertSetter(separatorStyle, FrameworkElement.MarginProperty);

        var fwSeparatorStyle = AssertStyle<FWMenuFlyoutSeparator>(dictionary);
        AssertSetter(fwSeparatorStyle, FWMenuFlyoutSeparator.DensityProperty);

        var subItemStyle = AssertStyle<FWMenuFlyoutSubItem>(dictionary);
        Assert.Equal(typeof(FWMenuFlyoutSubItem), subItemStyle.TargetType);
        Assert.NotNull(subItemStyle.BasedOn);
        AssertSetter(subItemStyle, FluentMenuFlyoutItemBase.DensityProperty);

        Assert.True(dictionary.TryGetValue("FWMenuFlyoutPresenterStyle", out var presenterStyle));
        var style = Assert.IsType<Style>(presenterStyle);
        Assert.Equal(typeof(FWMenuFlyoutPresenter), style.TargetType);
        AssertSetter(style, Control.BackgroundProperty);
        AssertSetter(style, Control.BorderBrushProperty);
        AssertSetter(style, Control.PaddingProperty);
        AssertSetter(style, FWMenuFlyoutPresenter.DensityProperty);

        ResetApplicationState();
    }

    [Fact]
    public void FWMenuControls_ShouldApplyDensityPresets()
    {
        var menu = new TestMenu();

        Assert.Equal(FWMenuDensity.Comfortable, menu.Density);
        Assert.Equal(32, menu.Height);
        Assert.Equal(new Thickness(4, 0, 4, 0), menu.Padding);

        menu.Density = FWMenuDensity.Compact;

        Assert.Equal(28, menu.Height);
        Assert.Equal(new Thickness(2, 0, 2, 0), menu.Padding);

        var generated = Assert.IsType<FWMenuItem>(menu.CreateContainer("Open"));
        menu.Prepare(generated, "Open");

        Assert.Equal(FWMenuDensity.Compact, generated.Density);
        Assert.Equal(28, generated.Height);
        Assert.Equal(new Thickness(10, 0, 10, 0), generated.Padding);

        generated.Density = FWMenuDensity.Spacious;

        Assert.Equal(40, generated.Height);
        Assert.Equal(new Thickness(14, 4, 14, 4), generated.Padding);

        var contextMenu = new TestContextMenu
        {
            Density = FWMenuDensity.Spacious
        };
        var contextItem = Assert.IsType<FWMenuItem>(contextMenu.CreateContainer("Refresh"));
        contextMenu.Prepare(contextItem, "Refresh");

        Assert.Equal(new Thickness(6), contextMenu.Padding);
        Assert.Equal(new CornerRadius(10), contextMenu.CornerRadius);
        Assert.Equal(FWMenuDensity.Spacious, contextItem.Density);
        Assert.Equal(40, contextItem.Height);

        contextMenu.Density = FWMenuDensity.Compact;

        Assert.Equal(new Thickness(3), contextMenu.Padding);
        Assert.Equal(new CornerRadius(6), contextMenu.CornerRadius);
    }

    [Fact]
    public void FWFlyout_ShouldExposeContentPlacementAndPresenterState()
    {
        var target = new FWButton { Content = "Open flyout" };
        var content = new FWTextBlock { Text = "Details" };
        var flyout = new FWFlyout
        {
            Content = content,
            Placement = FlyoutPlacementMode.Right,
            Density = FWMenuDensity.Compact
        };
        var opened = 0;
        var closed = 0;
        flyout.Opened += (_, _) => opened++;
        flyout.Closed += (_, _) => closed++;

        flyout.ShowAt(target);

        var presenter = GetFlyoutPresenter<FWFlyoutPresenter>(flyout);
        Assert.True(flyout.IsOpen);
        Assert.IsAssignableFrom<IFluentJaliumControl>(flyout);
        Assert.IsAssignableFrom<IFluentJaliumControl>(presenter);
        Assert.Same(content, presenter.Content);
        Assert.Equal(FlyoutPlacementMode.Right, flyout.Placement);
        Assert.Equal(FWMenuDensity.Compact, presenter.Density);
        Assert.Equal(new Thickness(3), presenter.Padding);
        Assert.Equal(new CornerRadius(6), presenter.CornerRadius);
        Assert.Equal(1, opened);

        flyout.Content = "Updated";
        flyout.Density = FWMenuDensity.Spacious;

        Assert.Equal("Updated", presenter.Content);
        Assert.Equal(FWMenuDensity.Spacious, presenter.Density);
        Assert.Equal(new Thickness(6), presenter.Padding);
        Assert.Equal(new CornerRadius(10), presenter.CornerRadius);

        flyout.Hide();

        Assert.False(flyout.IsOpen);
        Assert.True(closed >= 1);
    }

    [Fact]
    public void FWFlyout_ShouldUseExplicitPresenterStyle()
    {
        var target = new FWButton { Content = "Open flyout" };
        var style = new Style(typeof(FWFlyoutPresenter));
        style.Setters.Add(new Setter(Control.MinWidthProperty, 240.0));
        var flyout = new FWFlyout
        {
            Content = "Settings",
            FlyoutPresenterStyle = style
        };

        flyout.ShowAt(target);

        var presenter = GetFlyoutPresenter<FWFlyoutPresenter>(flyout);
        Assert.Same(style, presenter.Style);
        Assert.Equal("Settings", presenter.Content);

        flyout.Hide();
    }

    [Fact]
    public void GalleryMenusPage_ShouldFormatFlyoutQaSnapshot()
    {
        var style = new Style(typeof(FWFlyoutPresenter));
        var template = new DataTemplate();
        var flyout = new FWFlyout
        {
            Content = "Settings",
            ContentTemplate = template,
            Placement = FlyoutPlacementMode.Bottom,
            Density = FWMenuDensity.Spacious,
            FlyoutPresenterStyle = style
        };

        var snapshot = GalleryMenusPage.CreateFlyoutQaSnapshot(flyout);
        var text = GalleryMenusPage.FormatFlyoutQa("Flyout QA", snapshot);

        Assert.False(snapshot.IsOpen);
        Assert.Equal(FlyoutPlacementMode.Bottom, snapshot.Placement);
        Assert.Equal(FWMenuDensity.Spacious, snapshot.Density);
        Assert.True(snapshot.HasContent);
        Assert.True(snapshot.HasContentTemplate);
        Assert.True(snapshot.HasExplicitPresenterStyle);
        Assert.Contains("Flyout QA: closed", text);
        Assert.Contains("placement Bottom", text);
        Assert.Contains("density Spacious", text);
        Assert.Contains("content on", text);
        Assert.Contains("template on", text);
        Assert.Contains("explicit presenter style on", text);
    }

    [Fact]
    public void FWFlyout_ShouldResolvePresenterStyleFromApplicationResources()
    {
        ResetApplicationState();
        var app = new Application();
        var target = new FWButton { Content = "Open flyout" };
        var contentTemplate = new DataTemplate();
        var style = new Style(typeof(FWFlyoutPresenter));
        style.Setters.Add(new Setter(Control.MinWidthProperty, 280.0));
        app.Resources["FWFlyoutPresenterStyle"] = style;

        try
        {
            var flyout = new FWFlyout
            {
                Content = "Resources",
                ContentTemplate = contentTemplate,
                Density = FWMenuDensity.Compact
            };

            flyout.ShowAt(target);

            var presenter = GetFlyoutPresenter<FWFlyoutPresenter>(flyout);
            Assert.Same(style, presenter.Style);
            Assert.Equal("Resources", presenter.Content);
            Assert.Same(contentTemplate, presenter.ContentTemplate);
            Assert.Equal(FWMenuDensity.Compact, presenter.Density);

            flyout.Content = "Updated resources";
            flyout.Density = FWMenuDensity.Spacious;

            Assert.Equal("Updated resources", presenter.Content);
            Assert.Equal(FWMenuDensity.Spacious, presenter.Density);

            flyout.Hide();
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    public void FWMenuFlyout_ShouldApplyDensityToItemsAndPresenter()
    {
        var target = new FWButton { Content = "Open menu" };
        var flyout = new FWMenuFlyout
        {
            Density = FWMenuDensity.Compact
        };
        var item = new FWMenuFlyoutItem { Text = "Copy" };
        var toggle = new FWToggleMenuFlyoutItem { Text = "Pin" };
        var separator = new FWMenuFlyoutSeparator();
        var fixedItem = new FWMenuFlyoutItem
        {
            Density = FWMenuDensity.Spacious,
            Text = "Fixed"
        };

        flyout.Items.Add(item);
        flyout.Items.Add(toggle);
        flyout.Items.Add(separator);
        flyout.Items.Add(fixedItem);

        Assert.Equal(FWMenuDensity.Compact, item.Density);
        Assert.Equal(28, item.Height);
        Assert.Equal(FWMenuDensity.Compact, toggle.Density);
        Assert.Equal(28, toggle.Height);
        Assert.Equal(FWMenuDensity.Compact, separator.Density);
        Assert.Equal(new Thickness(0, 3, 0, 3), separator.Margin);
        Assert.Equal(FWMenuDensity.Spacious, fixedItem.Density);
        Assert.Equal(40, fixedItem.Height);

        flyout.ShowAt(target);

        var presenter = GetFlyoutPresenter<FWMenuFlyoutPresenter>(flyout);

        Assert.Equal(FWMenuDensity.Compact, presenter.Density);
        Assert.Equal(new Thickness(3), presenter.Padding);
        Assert.Equal(new CornerRadius(6), presenter.CornerRadius);

        flyout.Density = FWMenuDensity.Spacious;

        Assert.Equal(FWMenuDensity.Spacious, item.Density);
        Assert.Equal(40, item.Height);
        Assert.Equal(FWMenuDensity.Spacious, toggle.Density);
        Assert.Equal(40, toggle.Height);
        Assert.Equal(FWMenuDensity.Spacious, separator.Density);
        Assert.Equal(new Thickness(0, 6, 0, 6), separator.Margin);
        Assert.Equal(FWMenuDensity.Spacious, fixedItem.Density);
        Assert.Equal(FWMenuDensity.Spacious, presenter.Density);
        Assert.Equal(new Thickness(6), presenter.Padding);
        Assert.Equal(new CornerRadius(10), presenter.CornerRadius);

        flyout.Hide();
    }

    [Fact]
    public void FWMenuFlyoutSubItem_ShouldPropagateDensityToSubItems()
    {
        var subItem = new FWMenuFlyoutSubItem
        {
            Density = FWMenuDensity.Spacious,
            Text = "Share"
        };
        var child = new FWMenuFlyoutItem { Text = "Copy link" };
        var fixedChild = new FWMenuFlyoutItem
        {
            Density = FWMenuDensity.Compact,
            Text = "Mail"
        };

        subItem.Items.Add(child);
        subItem.Items.Add(fixedChild);

        InvokeMenuFlyoutItem(subItem);

        Assert.Equal(FWMenuDensity.Spacious, child.Density);
        Assert.Equal(40, child.Height);
        Assert.Equal(FWMenuDensity.Compact, fixedChild.Density);
        Assert.Equal(28, fixedChild.Height);

        subItem.HideSubMenu();
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

    [Fact]
    public void FWMenuFlyout_ShouldExposeItemsPlacementAndOpenCloseEvents()
    {
        var target = new FWButton { Content = "Open" };
        var flyout = new FWMenuFlyout
        {
            Placement = FlyoutPlacementMode.Bottom
        };
        flyout.Items.Add(new FWMenuFlyoutItem { Text = "Pin" });
        flyout.Items.Add(new FWMenuFlyoutSeparator());
        flyout.Items.Add(new FWMenuFlyoutItem { Text = "Settings" });

        var opened = 0;
        var closed = 0;
        flyout.Opened += (_, _) => opened++;
        flyout.Closed += (_, _) => closed++;

        flyout.ShowAt(target);
        flyout.Hide();

        Assert.False(flyout.IsOpen);
        Assert.Equal(FlyoutPlacementMode.Bottom, flyout.Placement);
        Assert.Equal(3, flyout.Items.Count);
        Assert.Equal(1, opened);
        Assert.True(closed >= 1);
    }

    [Fact]
    public void FWCommandBarFlyout_ShouldExposeCommandCollections()
    {
        var flyout = new FWCommandBarFlyout
        {
            AlwaysExpanded = true
        };
        var primary = new FWAppBarButton { Label = "Add" };
        var secondary = new FWAppBarButton { Label = "Settings" };

        flyout.PrimaryCommands.Add(primary);
        flyout.SecondaryCommands.Add(secondary);

        Assert.True(flyout.AlwaysExpanded);
        Assert.Same(primary, Assert.Single(flyout.PrimaryCommands));
        Assert.Same(secondary, Assert.Single(flyout.SecondaryCommands));
    }

    [Fact]
    public void FWCommandBarFlyout_ShouldSyncPresenterCommands()
    {
        var target = new FWButton { Content = "More commands" };
        var flyout = new FWCommandBarFlyout();
        var primary = new FWAppBarButton { Label = "Copy" };
        var secondary = new FWAppBarButton { Label = "Rename" };

        flyout.PrimaryCommands.Add(primary);
        flyout.ShowAt(target);

        var commandBar = GetFlyoutPresenter<FWCommandBar>(flyout);
        Assert.Same(primary, Assert.Single(commandBar.PrimaryCommands));
        Assert.Empty(commandBar.SecondaryCommands);
        Assert.False(commandBar.IsOpen);

        flyout.SecondaryCommands.Add(secondary);
        flyout.AlwaysExpanded = true;

        Assert.Same(secondary, Assert.Single(commandBar.SecondaryCommands));
        Assert.True(commandBar.IsOpen);

        flyout.Hide();
    }

    [Fact]
    public void GalleryMenusPage_ShouldFormatFlyoutQaText()
    {
        var contentFlyout = new FWFlyout
        {
            Content = new FWTextBlock { Text = "Details" },
            Placement = FlyoutPlacementMode.Right,
            Density = FWMenuDensity.Spacious
        };
        var menuFlyout = new FWMenuFlyout
        {
            Placement = FlyoutPlacementMode.Bottom,
            Density = FWMenuDensity.Compact
        };
        var toggle = new FWToggleMenuFlyoutItem
        {
            Text = "Show badges",
            IsChecked = true
        };
        menuFlyout.Items.Add(new FWMenuFlyoutItem { Text = "Pin" });
        menuFlyout.Items.Add(toggle);
        menuFlyout.Items.Add(new FWMenuFlyoutSeparator());
        menuFlyout.Items.Add(new FWMenuFlyoutItem { Text = "Disabled", IsEnabled = false });

        var commandFlyout = new FWCommandBarFlyout
        {
            AlwaysExpanded = true
        };
        commandFlyout.PrimaryCommands.Add(new FWAppBarButton { Label = "Copy" });
        commandFlyout.PrimaryCommands.Add(new FWAppBarButton { Label = "Share" });
        commandFlyout.SecondaryCommands.Add(new FWAppBarButton { Label = "Rename" });

        var flyoutText = GalleryMenusPage.FormatFlyoutQa("Flyout QA", contentFlyout);
        var menuFlyoutText = GalleryMenusPage.FormatMenuFlyoutQa("MenuFlyout QA", menuFlyout);
        var commandFlyoutText = GalleryMenusPage.FormatCommandBarFlyoutQa("CommandBarFlyout QA", commandFlyout);

        Assert.Contains("Flyout QA: closed", flyoutText);
        Assert.Contains("placement Right", flyoutText);
        Assert.Contains("density Spacious", flyoutText);
        Assert.Contains("content on", flyoutText);
        Assert.Contains("template off", flyoutText);
        Assert.Contains("explicit presenter style off", flyoutText);

        Assert.Contains("MenuFlyout QA: closed", menuFlyoutText);
        Assert.Contains("placement Bottom", menuFlyoutText);
        Assert.Contains("density Compact", menuFlyoutText);
        Assert.Contains("items 4", menuFlyoutText);
        Assert.Contains("enabled 3", menuFlyoutText);
        Assert.Contains("checked toggles 1", menuFlyoutText);
        Assert.Contains("separators 1", menuFlyoutText);

        Assert.Contains("CommandBarFlyout QA: closed", commandFlyoutText);
        Assert.Contains("primary 2", commandFlyoutText);
        Assert.Contains("secondary 1", commandFlyoutText);
        Assert.Contains("expanded on", commandFlyoutText);
    }

    [Fact]
    public void FWMenuControls_ShouldComposeInsideLiquidGlassMenuWorkbench()
    {
        var menuBar = new FWMenuBar();
        var file = new FWMenuBarItem { Title = "File" };
        file.Items.Add(new FWMenuFlyoutItem { Text = "Open", KeyboardAcceleratorTextOverride = "Ctrl+O" });
        file.Items.Add(new FWMenuFlyoutSeparator());
        file.Items.Add(new FWMenuFlyoutItem { Text = "Save", Icon = "\uE74E" });
        menuBar.Items.Add(file);

        var flyout = new FWMenuFlyout();
        var pin = new FWToggleMenuFlyoutItem
        {
            Text = "Pin",
            IsChecked = true
        };
        flyout.Items.Add(new FWMenuFlyoutItem { Text = "Copy" });
        flyout.Items.Add(pin);
        flyout.Items.Add(new FWMenuFlyoutSeparator());
        flyout.Items.Add(new FWMenuFlyoutItem { Text = "Disabled", IsEnabled = false });

        var contextMenu = new FWContextMenu
        {
            Placement = PlacementMode.Bottom,
            StaysOpen = true
        };
        contextMenu.Items.Add(new FWMenuItem { Header = "Refresh" });
        contextMenu.Items.Add(new FWMenuItem { Header = "Metadata", IsCheckable = true, IsChecked = true });

        var commandFlyout = new FWCommandBarFlyout
        {
            AlwaysExpanded = true
        };
        var copy = new FWAppBarButton { Label = "Copy" };
        var share = new FWAppBarButton { Label = "Share" };
        commandFlyout.PrimaryCommands.Add(copy);
        commandFlyout.SecondaryCommands.Add(share);

        var panel = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Children =
            {
                menuBar,
                new FWDropDownButton { Content = "Flyout", Flyout = flyout, ContextMenu = contextMenu }
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

        Assert.Single(menuBar.Items);
        Assert.Equal(3, file.Items.Count);
        Assert.Equal(4, flyout.Items.Count);
        Assert.True(pin.IsChecked);
        Assert.Equal(PlacementMode.Bottom, contextMenu.Placement);
        Assert.True(contextMenu.StaysOpen);
        Assert.True(commandFlyout.AlwaysExpanded);
        Assert.Same(copy, Assert.Single(commandFlyout.PrimaryCommands));
        Assert.Same(share, Assert.Single(commandFlyout.SecondaryCommands));
        Assert.Equal(FWFluentMaterialKind.LiquidGlass, surface.MaterialKind);
        Assert.True(surface.LiquidGlass);
        Assert.Equal(70, surface.RefractionAmount);
        Assert.Equal(0.42, surface.ChromaticAberration);
        Assert.Equal(24, surface.FusionRadius);
        Assert.Equal(BorderShape.SuperEllipse, surface.Shape);
        Assert.Equal(4, surface.SuperEllipseN);
        Assert.Same(panel, surface.Child);
    }

    [Fact]
    public void FWMenuFlyoutSubItem_ShouldExposeSubItemsAndUseSubmenuInvocation()
    {
        var subItem = new FWMenuFlyoutSubItem
        {
            Text = "Share"
        };
        var copy = new FWMenuFlyoutItem { Text = "Copy link" };
        var mail = new FWMenuFlyoutItem { Text = "Mail" };
        subItem.Items.Add(copy);
        subItem.Items.Add(mail);

        InvokeMenuFlyoutItem(subItem);
        subItem.HideSubMenu();

        Assert.Equal("Share", subItem.Text);
        Assert.Equal(2, subItem.Items.Count);
        Assert.Same(copy, subItem.Items[0]);
        Assert.Same(mail, subItem.Items[1]);
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

    private static TPresenter GetFlyoutPresenter<TPresenter>(FlyoutBase flyout)
        where TPresenter : Control
    {
        var field = typeof(FlyoutBase).GetField("_presenter", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        return Assert.IsType<TPresenter>(field.GetValue(flyout));
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
