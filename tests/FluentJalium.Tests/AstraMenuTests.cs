using System.Reflection;
using System.Windows.Input;
using FluentJalium.Themes;
using FluentJalium.Tests.Pixel;
using Jalium.UI;
using Jalium.UI.Automation.Peers;
using Jalium.UI.Automation.Provider;
using Jalium.UI.Controls;
using Jalium.UI.Input;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Threading;

namespace FluentJalium.Tests;

/// <summary>
/// The second stage-4 batch: the menu family. What is native here is unusual - the whole family ships, and
/// half of it can be retemplated while the other half cannot, so this file's first job is to pin which is
/// which. Measured in spike/MenuProbe (passes 1-6): Menu, ContextMenu, MenuFlyoutItem, ToggleMenuFlyoutItem,
/// MenuFlyoutSubItem, MenuFlyoutSeparator and MenuBarItem all build a template that was in place before the
/// window was shown, and their resolved brushes are our palette objects; MenuItem stores a Template and
/// never builds one, so it gets a colour-only style.
/// <para>
/// Two things are deliberately not claimed. No menu state of ours is reached by a pointer - the flyout opens
/// through ShowAt, the toggle through IsChecked, and the item's command through the automation peer, which is
/// the control's own code path but not hardware input (task 13 owns that debt). And the surface of a popped-up
/// menu belongs to the framework: it wraps the content in its own MenuPopupScrollHost with a hardcoded border,
/// so the presenter rows are only promised to reach pixels through ContextMenu, which is a control of ours.
/// </para>
/// <para>
/// The visual-defect batch narrowed what a flyout skin may own. Every row type here paints its own content in
/// OnRender - label, accelerator text, check mark, submenu chevron, separator rule - so a template that
/// presents any of them again draws the row twice, which is the ghost the user reported. The tests below now
/// pin the opposite contract: the skin carries only the surface and the icon slot (the one thing 26.10.9 never
/// paints), the label states ride on Style.Triggers because the tint pass measured that the control's painter
/// reads this Foreground, and the eleven rows that described pixels we no longer own are withdrawn rather than
/// kept as decoration. spike/FlyoutGhostProbe holds a real flyout open for an outside PrintWindow capture and
/// is what turns those claims into pixels: 1194 pure-white label pixels over the framework's own text before,
/// zero after, with the framework layer's colours unchanged to the pixel count.
/// </para>
/// </summary>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraMenuTests
{
    private static readonly Color LabelSentinel = Color.FromRgb(0xFF, 0x00, 0xFF);
    private static readonly Color HoverSentinel = Color.FromRgb(0x00, 0x80, 0xFF);
    private static readonly Color SeparatorSentinel = Color.FromRgb(0xFF, 0xD6, 0x0A);
    private static readonly Color ChevronSentinel = Color.FromRgb(0x00, 0xC0, 0xC0);
    private static readonly Color BrandEmerald = Color.FromRgb(0x20, 0x72, 0x45);

    /// <summary>The grey 26.10.9 paints for its own menu and popup chrome when nothing of ours reaches it.</summary>
    private static readonly Color FrameworkMenuSurface = Color.FromRgb(0x2C, 0x2C, 0x2E);

    private static readonly Color FrameworkMenuHighlight = Color.FromRgb(0x3A, 0x3A, 0x3C);

    /// <summary>The grey 26.10.9 paints a flyout row's own text in under the light theme.</summary>
    private static readonly Color FrameworkRowText = Color.FromRgb(0x6E, 0x6E, 0x73);

    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraMenuTests(AstraThemeRuntimeFixture fixture)
    {
        _fixture = fixture;
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            FluentThemeManager.ApplyAccent(null);
        });
    }

    // ---------- rows ----------

    [Theory]
    [InlineData("MenuFlyoutPresenterBackground", "AcrylicInAppFillColorDefaultBrush")]
    [InlineData("MenuFlyoutPresenterBorderBrush", "SurfaceStrokeColorFlyoutBrush")]
    [InlineData("MenuFlyoutItemBackground", "SubtleFillColorTransparentBrush")]
    [InlineData("MenuFlyoutItemBackgroundPointerOver", "SubtleFillColorSecondaryBrush")]
    [InlineData("MenuFlyoutItemForeground", "TextFillColorPrimaryBrush")]
    [InlineData("MenuFlyoutItemForegroundDisabled", "TextFillColorDisabledBrush")]
    [InlineData("MenuFlyoutSubItemBackgroundPointerOver", "SubtleFillColorSecondaryBrush")]
    [InlineData("MenuFlyoutSubItemForeground", "TextFillColorPrimaryBrush")]
    [InlineData("MenuFlyoutSubItemForegroundPointerOver", "TextFillColorPrimaryBrush")]
    [InlineData("MenuFlyoutSubItemForegroundDisabled", "TextFillColorDisabledBrush")]
    [InlineData("MenuBarBackground", "SubtleFillColorTransparentBrush")]
    [InlineData("MenuBarItemForeground", "TextFillColorPrimaryBrush")]
    [InlineData("MenuBarItemBackgroundPointerOver", "SubtleFillColorSecondaryBrush")]
    [InlineData("MenuBarItemBorderBrush", "ControlAltFillColorTertiaryBrush")]
    [InlineData("MenuBarItemBorderBrushPointerOver", "ControlStrokeColorDefaultBrush")]
    public void An_alias_resolves_to_the_object_upstream_names(string alias, string target)
    {
        _fixture.Run(() => Assert.Same(Res(target), Res(alias)));
    }

    [Theory]
    [InlineData("MenuFlyoutPresenterBorderThemeThickness", 1, 1, 1, 1)]
    [InlineData("MenuFlyoutPresenterThemePadding", 0, 2, 0, 2)]
    [InlineData("MenuFlyoutItemThemePadding", 11, 8, 11, 9)]
    [InlineData("MenuFlyoutItemMargin", 4, 2, 4, 2)]
    [InlineData("MenuFlyoutSeparatorThemePadding", -4, 1, -4, 1)]
    [InlineData("MenuBarItemButtonPadding", 10, 4, 10, 4)]
    [InlineData("MenuBarItemMargin", 4, 4, 4, 4)]
    public void The_layout_rows_carry_upstreams_values(params object[] expected)
    {
        _fixture.Run(() =>
        {
            var thickness = (Thickness)Application.Current!.TryFindResource(expected[0]!.ToString()!)!;
            Assert.Equal(new Thickness(Convert.ToDouble(expected[1]), Convert.ToDouble(expected[2]),
                Convert.ToDouble(expected[3]), Convert.ToDouble(expected[4])), thickness);
        });
    }

    /// <summary>
    /// The deferral, pinned. Upstream publishes eleven more <c>*Pressed*</c> rows and two
    /// <c>*SubMenuOpened*</c> rows for this family; 26.10.9's flyout item types expose IsHighlighted and
    /// IsSubMenuOpen as read-only CLR getters rather than dependency properties, so no cell can watch them
    /// (spike/MenuProbe pass 5). The reveal rows have no material to name, the placeholder and narrow-padding
    /// rows belong to visual-state groups the runtime never writes, and the MenuBar pressed/selected quartet
    /// has no state behind it because MenuBarItem's only dependency property is Title.
    /// </summary>
    [Theory]
    [InlineData("MenuFlyoutItemForegroundPressed")]
    [InlineData("MenuFlyoutItemKeyboardAcceleratorTextForegroundPressed")]
    // The eleven rows below went out with the ghost fix: the accelerator text, the chevron and the rule are
    // the control's own paint, so keeping their rows would publish tokens no consumer reads.
    [InlineData("MenuFlyoutItemKeyboardAcceleratorTextForeground")]
    [InlineData("MenuFlyoutItemKeyboardAcceleratorTextForegroundPointerOver")]
    [InlineData("MenuFlyoutItemKeyboardAcceleratorTextForegroundDisabled")]
    [InlineData("MenuFlyoutItemChevronMargin")]
    [InlineData("MenuFlyoutSeparatorBackground")]
    [InlineData("MenuFlyoutSubItemChevron")]
    [InlineData("MenuFlyoutSubItemChevronPointerOver")]
    [InlineData("MenuFlyoutSubItemChevronDisabled")]
    [InlineData("ToggleMenuFlyoutItemKeyboardAcceleratorTextForeground")]
    [InlineData("ToggleMenuFlyoutItemKeyboardAcceleratorTextForegroundPointerOver")]
    [InlineData("ToggleMenuFlyoutItemKeyboardAcceleratorTextForegroundDisabled")]
    [InlineData("MenuFlyoutSubItemBackgroundPressed")]
    [InlineData("MenuFlyoutSubItemForegroundPressed")]
    [InlineData("MenuFlyoutSubItemChevronPressed")]
    [InlineData("MenuFlyoutSubItemBackgroundSubMenuOpened")]
    [InlineData("MenuFlyoutSubItemChevronSubMenuOpened")]
    [InlineData("ToggleMenuFlyoutItemKeyboardAcceleratorTextForegroundPressed")]
    [InlineData("MenuFlyoutItemRevealBackground")]
    [InlineData("MenuFlyoutSubItemRevealBorderBrush")]
    [InlineData("MenuFlyoutItemPlaceholderThemeThickness")]
    [InlineData("MenuFlyoutItemThemePaddingNarrow")]
    [InlineData("MenuFlyoutItemTextTrimming")]
    [InlineData("MenuFlyoutPresenterScrollViewerVerticalScrollController")]
    [InlineData("MenuFlyoutSeparatorHeight")]
    [InlineData("MenuFlyoutThemeMinHeight")]
    [InlineData("MenuFlyoutLightDismissOverlayBackground")]
    [InlineData("MenuFlyoutSystemBackdrop")]
    [InlineData("SplitMenuFlyoutItemButtonDividerBrush")]
    [InlineData("RadioMenuFlyoutItemBackground")]
    [InlineData("MenuBarItemBackgroundSelected")]
    [InlineData("MenuBarItemBorderBrushPressed")]
    [InlineData("MenuBarHeight")]
    [InlineData("MenuBackground")]
    [InlineData("MenuItemBackground")]
    [InlineData("MenuItemForeground")]
    [InlineData("ContextMenuBackground")]
    [InlineData("MenuFlyoutPresenterBackgroundBrush")]
    public void A_row_with_no_consumer_is_not_published(string key)
    {
        _fixture.Run(() => Assert.Null(TryRes(key)));
    }

    // ---------- which types can be retemplated ----------

    [Theory]
    [InlineData("DefaultMenuFlyoutItemStyle", typeof(MenuFlyoutItem))]
    [InlineData("DefaultToggleMenuFlyoutItemStyle", typeof(ToggleMenuFlyoutItem))]
    [InlineData("DefaultMenuFlyoutSubItemStyle", typeof(MenuFlyoutSubItem))]
    [InlineData("DefaultMenuStyle", typeof(Menu))]
    [InlineData("DefaultContextMenuStyle", typeof(ContextMenu))]
    public void The_implicit_style_reaches_the_retemplatable_types(string key, Type type)
    {
        _fixture.Run(() =>
        {
            var element = (FrameworkElement)Activator.CreateInstance(type)!;
            Mount(element, 260, 40);

            Assert.Null(element.Style); // an implicit style never lands in FrameworkElement.Style here
            Assert.Same(Template(FluentThemeManager.GetStyle(key)), (element as Control)!.Template);
        });
    }

    /// <summary>
    /// The asymmetry this batch turns on, as an assertion: the same assignment that gives a flyout item its
    /// whole skin leaves a MenuItem with a stored Template and no tree, because that control paints itself in
    /// OnRender. This is why MenuItem gets a colour-only style instead of a template, and why its highlight
    /// stays a framework colour.
    /// </summary>
    [Fact]
    public void A_menu_item_stores_a_template_but_never_builds_one()
    {
        _fixture.Run(() =>
        {
            var item = new MenuFlyoutItem { Text = "flyout" };
            Mount(item, 240, 38);
            Assert.NotNull(PixelHarness.Named(item, "LayoutRoot"));

            var menu = new Menu();
            menu.Items.Add(new MenuItem { Header = "file" });
            Mount(menu, 320, 40);
            var menuItem = (MenuItem)menu.Items[0];

            menuItem.Template = Template(FluentThemeManager.GetStyle("DefaultMenuFlyoutItemStyle"));
            PixelHarness.Settle(20);

            Assert.NotNull(menuItem.Template);
            Assert.Null(PixelHarness.Named(menuItem, "LayoutRoot"));
            Assert.Null(PixelHarness.Named(menuItem, "TextBlock"));
        });
    }

    /// <summary>
    /// The row heights a flyout is made of, read off a realized stack instead of off a comment. An item is
    /// upstream's 38: the 11,8,11,9 padding around a 17-tall label is 34, and the 4,2,4,2 item margin adds the
    /// rest - which is why the style carries a 38 floor rather than the 32 row upstream names, because
    /// 26.10.9 measures the item itself and 32 would squeeze that label to 13 (spike/FlyoutGhostProbe).
    /// The separator is upstream's 3, and that one needs the assertion more than the item does: left alone the
    /// runtime measures this type 9 tall, and only a Height reaching the element brings it back to the rule
    /// plus its own 1,1 padding. spike/VisualQA/out/flyout-400x356-168.png is the same flyout's own window
    /// printed from outside, where the rule still spans the row at y=122.3..124.6 DIP.
    /// </summary>
    [Fact]
    public void The_flyout_rows_measure_upstreams_heights()
    {
        _fixture.Run(() =>
        {
            var item = new MenuFlyoutItem { Text = "run" };
            var separator = new MenuFlyoutSeparator();
            var stack = new StackPanel { Width = 240 };
            stack.Children.Add(item);
            stack.Children.Add(separator);
            Mount(stack, 240, 60);

            Assert.Equal(38d, item.ActualHeight);
            Assert.Equal(34d, ((Border)Part(item, "LayoutRoot")).ActualHeight);

            Assert.Equal(3d, separator.ActualHeight);
            Assert.Equal(new Thickness(0), separator.Margin);
            var rule = Assert.IsType<Border>(VisualTreeHelper.GetChild(separator, 0));
            Assert.Equal(1d, rule.ActualHeight);
            Assert.Equal(new Thickness(-4, 1, -4, 1), rule.Margin);
        });
    }

    [Fact]
    public void The_menu_lays_its_top_level_items_out_in_a_row()
    {
        // Supplying a template costs the panel that came with the framework's own: an ItemsPresenter with no
        // ItemsPanel of its own falls back to ItemsControl's vertical one, which stacked File/Edit/View/Help
        // into a column on the Menus page (spike/VisualQA/out/menus.png, before the setter). Upstream's
        // nearest shape, MenuBar, is a row, so the row has to be restated with the template.
        _fixture.Run(() =>
        {
            var menu = new Menu { Width = 420 };
            menu.Items.Add(new MenuItem { Header = "file" });
            menu.Items.Add(new MenuItem { Header = "edit" });
            Mount(menu, 420, 40);

            var first = (MenuItem)menu.Items[0];
            var second = (MenuItem)menu.Items[1];
            var firstAt = first.TranslatePoint(new Point(0, 0), menu);
            var secondAt = second.TranslatePoint(new Point(0, 0), menu);

            Assert.True(Math.Abs(secondAt.Y - firstAt.Y) < 1,
                $"the items are on different rows: {firstAt.Y} against {secondAt.Y}.");
            Assert.True(secondAt.X >= firstAt.X + first.ActualWidth - 1,
                $"the second item does not follow the first to the right: {firstAt.X}+{first.ActualWidth} against {secondAt.X}.");
        });
    }

    [Fact]
    public void A_menu_item_keeps_a_local_background_free_so_our_rows_own_its_colour()
    {
        // Measured: pass 4 saw the framework write a local Background on a press of its own item; with our
        // style installed, a driven MouseDown sets IsPressed and leaves the brush alone, so nothing outranks
        // the style. The framework's highlight colour is still refused below, in pixels.
        _fixture.Run(() =>
        {
            var menu = new Menu();
            menu.Items.Add(new MenuItem { Header = "file" });
            Mount(menu, 320, 40);
            var item = (MenuItem)menu.Items[0];

            Assert.Same(Res("MenuFlyoutItemBackground"), item.Background);
            Assert.Same(Res("MenuFlyoutItemForeground"), item.Foreground);

            RaiseMouseDown(item);
            PixelHarness.Settle(20);

            Assert.False(item.HasLocalValue(Control.BackgroundProperty), "the framework wrote a local Background over our style.");
        });
    }

    [Fact]
    public void The_disabled_menu_item_cell_reaches_the_self_painter()
    {
        _fixture.Run(() =>
        {
            var menu = new Menu();
            menu.Items.Add(new MenuItem { Header = "file" });
            Mount(menu, 320, 40);
            var item = (MenuItem)menu.Items[0];

            item.IsEnabled = false;
            PixelHarness.Settle(20);

            Assert.Same(Res("MenuFlyoutItemForegroundDisabled"), item.Foreground);
            item.IsEnabled = true;
        });
    }

    // ---------- part names and cells ----------

    /// <summary>
    /// The single-painter contract. 26.10.9 draws a flyout row's label, accelerator text, check mark and
    /// chevron in its own OnRender, so a part that presents any of them here is a second painter rather than
    /// a style choice - it is the ghost. What the skin keeps is the surface and the icon slot, because the
    /// control paints no icon at all (Icon is a plain System.Object on this runtime).
    /// </summary>
    [Fact]
    public void The_flyout_skin_carries_only_the_surface_and_the_icon_slot()
    {
        _fixture.Run(() =>
        {
            var item = Mount(new MenuFlyoutItem { Text = "item", Icon = new TextBlock { Text = "*" } }, 240, 38);
            Assert.IsType<Border>(Part(item, "LayoutRoot"));
            Assert.IsType<Border>(Part(item, "IconRoot"));
            var icon = Assert.IsType<ContentPresenter>(Part(item, "IconContent"));
            Assert.Equal("*", ((TextBlock)icon.Content!).Text);

            var toggle = Mount(new ToggleMenuFlyoutItem { Text = "toggle" }, 240, 38);
            var sub = Mount(new MenuFlyoutSubItem { Text = "sub" }, 240, 38);
            foreach (var row in new Control[] { item, toggle, sub })
            {
                foreach (var name in new[] { "TextBlock", "KeyboardAcceleratorTextBlock", "CheckGlyph", "CheckPlaceholder", "SubItemChevron" })
                {
                    Assert.Null(PixelHarness.Named(row, name));
                }
            }
        });
    }

    [Fact]
    public void The_menu_bar_item_leaves_its_title_to_the_control_that_paints_it()
    {
        _fixture.Run(() =>
        {
            var bar = new MenuBar();
            bar.Items.Add(new MenuBarItem { Title = "view" });
            Mount(bar, 320, 40);
            var item = (MenuBarItem)bar.Items[0];

            Assert.IsType<Grid>(Part(item, "ContentRoot"));
            Assert.IsType<Border>(Part(item, "Background"));
            // Upstream's item keeps a real Button for click and focus behaviour, and this runtime's MenuBarItem
            // paints its Title itself, so the button stays empty: with a label here spike/MenuBarGhostProbe
            // measured 1431 bright pixels in the bar band against the 603 the control's own paint accounts for.
            var button = (Button)Part(item, "ContentButton");
            Assert.Null(button.Content);
            Assert.DoesNotContain("view", TextsIn(item));
            Assert.NotNull(PixelHarness.Named(button, "Surface"));
        });
    }

    /// <summary>
    /// Template cells only, per style. The toggle item has no <c>IsEnabled=False</c> cell because its template wrote
    /// nothing for that state but the dead icon-foreground copy the sweep deleted (docs/astra/adaptation/00 S1-f);
    /// its disabled row lives on the style triggers, like the other two items'.
    /// </summary>
    [Theory]
    [InlineData("DefaultMenuFlyoutItemStyle", new[] { "Icon=null", "IsMouseOver=True", "IsEnabled=False" })]
    [InlineData("DefaultToggleMenuFlyoutItemStyle", new[] { "Icon=null", "IsChecked=True", "IsMouseOver=True" })]
    [InlineData("DefaultMenuFlyoutSubItemStyle", new[] { "Icon=null", "IsMouseOver=True", "IsEnabled=False" })]
    [InlineData("DefaultMenuBarItemStyle", new[] { "IsMouseOver=True" })]
    public void Each_item_style_carries_one_cell_per_reachable_state(string key, string[] expected)
    {
        _fixture.Run(() =>
        {
            var cells = TemplateCells(Template(FluentThemeManager.GetStyle(key)));
            Assert.Equal(expected, cells.Select(static cell => cell.Condition));
        });
    }

    /// <summary>
    /// The template cells only. The item's own text and icon colour rows live in the style's
    /// <c>Style.Triggers</c> - this runtime paints a template part's Foreground nowhere the icon is a
    /// ContentPresenter - so <c>MenuFlyoutItemForeground*</c> is not asserted here;
    /// <c>AstraForegroundRoutingTests.A_disabled_menu_item_carries_its_row_into_the_icon_as_well_as_the_text</c>
    /// reads the icon by effect instead.
    /// </summary>
    [Theory]
    [InlineData("DefaultMenuFlyoutItemStyle", "IsMouseOver=True", "Background", "MenuFlyoutItemBackgroundPointerOver")]
    [InlineData("DefaultMenuFlyoutItemStyle", "IsEnabled=False", "Background", "MenuFlyoutItemBackgroundDisabled")]
    [InlineData("DefaultMenuFlyoutSubItemStyle", "IsMouseOver=True", "Background", "MenuFlyoutSubItemBackgroundPointerOver")]
    [InlineData("DefaultMenuFlyoutSubItemStyle", "IsEnabled=False", "Background", "MenuFlyoutSubItemBackgroundDisabled")]
    [InlineData("DefaultToggleMenuFlyoutItemStyle", "IsChecked=True", "Visibility", null)]
    [InlineData("DefaultToggleMenuFlyoutItemStyle", "IsMouseOver=True", "Background", "MenuFlyoutSubItemBackgroundPointerOver")]
    public void A_state_cell_writes_the_row_upstream_writes(string key, string condition, string property, string? row)
    {
        _fixture.Run(() =>
        {
            var cell = FindCell(TemplateCells(Template(FluentThemeManager.GetStyle(key))), condition);
            var setter = cell.Setters.FirstOrDefault(candidate => (candidate.Property?.Name ?? candidate.PropertyName) == property);
            Assert.NotNull(setter);
            if (row is not null)
            {
                Assert.Equal(row, ResourceKey(setter!));
            }
        });
    }

    /// <summary>
    /// The label rows survive the ghost fix on the style rather than in the template, and only because the
    /// control's own painter reads this Foreground: spike/FlyoutGhostProbe's tint pass handed two rows a
    /// magenta Foreground and the captured popup came back with two magenta labels.
    /// </summary>
    [Theory]
    [InlineData("DefaultMenuFlyoutItemStyle", "IsMouseOver=True", "MenuFlyoutItemForegroundPointerOver")]
    [InlineData("DefaultMenuFlyoutItemStyle", "IsEnabled=False", "MenuFlyoutItemForegroundDisabled")]
    [InlineData("DefaultToggleMenuFlyoutItemStyle", "IsMouseOver=True", "MenuFlyoutItemForegroundPointerOver")]
    [InlineData("DefaultToggleMenuFlyoutItemStyle", "IsEnabled=False", "MenuFlyoutSubItemForegroundDisabled")]
    [InlineData("DefaultMenuFlyoutSubItemStyle", "IsMouseOver=True", "MenuFlyoutSubItemForegroundPointerOver")]
    [InlineData("DefaultMenuFlyoutSubItemStyle", "IsEnabled=False", "MenuFlyoutSubItemForegroundDisabled")]
    public void A_label_state_reaches_the_painter_through_the_style(string key, string condition, string row)
    {
        _fixture.Run(() =>
        {
            var cells = FluentThemeManager.GetStyle(key).Triggers.Cast<object>().OfType<Trigger>().Select(static trigger => new Cell(
                $"{trigger.Property?.Name ?? "UNRESOLVED"}={Form(trigger.Value)}",
                trigger.Setters.Cast<object>().OfType<Setter>().ToArray())).ToList();
            var setter = FindCell(cells, condition).Setters
                .First(candidate => (candidate.Property?.Name ?? candidate.PropertyName) == "Foreground");
            Assert.Equal(row, ResourceKey(setter));
        });
    }

    // ---------- behaviour, driven without a pointer ----------

    [Fact]
    public void Showing_a_flyout_opens_it_and_hides_it_closes_it()
    {
        _fixture.Run(() =>
        {
            var host = Mount(new Button { Content = "host" }, 140, 36);
            var flyout = new MenuFlyout();
            flyout.Items.Add(new MenuFlyoutItem { Text = "item" });

            flyout.ShowAt(host);
            PixelHarness.Settle(30);
            Assert.True(flyout.IsOpen, "ShowAt did not open the flyout.");

            flyout.Hide();
            PixelHarness.Settle(30);
            Assert.False(flyout.IsOpen, "Hide did not close the flyout.");
        });
    }

    [Fact]
    public void The_opened_flyout_paints_our_item_skin()
    {
        _fixture.Run(() =>
        {
            var host = Mount(new Button { Content = "host" }, 140, 36);
            var item = new MenuFlyoutItem { Text = "flyout item" };
            var flyout = new MenuFlyout();
            flyout.Items.Add(item);

            flyout.ShowAt(host);
            PixelHarness.Settle(40);

            Assert.Same(Template(FluentThemeManager.GetStyle("DefaultMenuFlyoutItemStyle")), item.Template);
            Assert.NotNull(PixelHarness.Named(item, "LayoutRoot"));
            flyout.Hide();
        });
    }

    [Fact]
    public void Invoking_an_item_runs_its_command_through_the_controls_own_path()
    {
        _fixture.Run(() =>
        {
            var ran = 0;
            var item = Mount(new MenuFlyoutItem { Text = "run", Command = new DelegateCommand(() => ran++) }, 240, 38);

            ((IInvokeProvider)new MenuFlyoutItemAutomationPeer(item)).Invoke();
            PixelHarness.Settle(20);

            Assert.Equal(1, ran);
        });
    }

    /// <summary>
    /// A parity note turned into a test: WinUI's ToggleMenuFlyoutItem answers Toggle, and 26.10.9's answers
    /// Invoke - the same provider the plain item uses - so an app that drives the checkbox pattern finds
    /// nothing. IsChecked remains a plain writable property, which is how the state is reachable at all.
    /// </summary>
    [Fact]
    public void The_toggle_exposes_invoke_rather_than_toggle()
    {
        _fixture.Run(() =>
        {
            var toggle = Mount(new ToggleMenuFlyoutItem { Text = "toggle" }, 240, 38);
            var peer = UIElementAutomationPeer.CreatePeerForElement(toggle)!;

            Assert.IsType<MenuFlyoutItemAutomationPeer>(peer);
            Assert.IsAssignableFrom<IInvokeProvider>(peer.GetPattern(PatternInterface.Invoke));
            Assert.Null(peer.GetPattern(PatternInterface.Toggle));
        });
    }

    /// <summary>
    /// Upstream's CheckedWithIcon state hides the icon and shows the mark in the same column, and this runtime
    /// paints the mark itself, so the cell our skin can carry is the icon's disappearance - asserted through
    /// the same IsChecked door a click reaches.
    /// </summary>
    [Fact]
    public void Checking_a_toggle_takes_the_icon_out_of_the_mark_column()
    {
        _fixture.Run(() =>
        {
            var toggle = Mount(new ToggleMenuFlyoutItem { Text = "toggle", Icon = new TextBlock { Text = "*" } }, 240, 38);
            var slot = (Border)Part(toggle, "IconRoot");
            Assert.Equal(Visibility.Visible, slot.Visibility);

            toggle.IsChecked = true;
            PixelHarness.Settle(20);
            Assert.Equal(Visibility.Collapsed, slot.Visibility);

            toggle.IsChecked = false;
            PixelHarness.Settle(20);
            Assert.Equal(Visibility.Visible, slot.Visibility);
        });
    }

    [Fact]
    public void A_context_menu_opens_on_a_point_with_its_own_surface()
    {
        _fixture.Run(() =>
        {
            var menu = new ContextMenu();
            menu.Items.Add(new MenuItem { Header = "cut" });

            menu.Open(new Point(120, 120));
            PixelHarness.Settle(40);

            try
            {
                Assert.True(menu.IsOpen, "Open(Point) did not open the context menu.");
                Assert.Same(Res("MenuFlyoutPresenterBackground"), menu.Background);
                Assert.Same(Template(FluentThemeManager.GetStyle("DefaultContextMenuStyle")), menu.Template);
            }
            finally
            {
                menu.IsOpen = false;
            }
        });
    }

    // ---------- surface identity recheck (spike/FlyoutSurfaceProbe, 2026-09-20) ----------

    /// <summary>
    /// The style-level half of the radius fix: upstream asks its flyout surface for
    /// <c>OverlayCornerRadius</c> (<c>MenuFlyout_themeresources.xaml:285</c>, key at 8 in
    /// <c>CornerRadius_themeresources.xaml:6</c>), and on this host that row only reaches the visible card
    /// through the control's own <c>CornerRadius</c> - the framework copies it onto the Border it builds.
    /// </summary>
    [Fact]
    public void The_context_menu_style_writes_upstreams_radius_on_the_row_the_framework_copies()
    {
        _fixture.Run(() =>
        {
            var setter = FluentThemeManager.GetStyle("DefaultContextMenuStyle").Setters.Cast<object>().OfType<Setter>()
                .First(static candidate => (candidate.Property?.Name ?? candidate.PropertyName) == "CornerRadius");
            Assert.Equal("OverlayCornerRadius", ResourceKey(setter));
            Assert.Equal(new CornerRadius(8), (CornerRadius)TryRes("OverlayCornerRadius")!);
        });
    }

    /// <summary>
    /// The contract that makes the setter above more than decoration: <c>ContextMenu.Open</c> grafts
    /// PopupRoot &gt; Border &gt; MenuPopupScrollHost into the host window's overlay, and that Border - which is
    /// not in any template of ours - wears our two presenter rows and our radius as <em>local</em> values.
    /// Before the setter existed the same read came back 14,14,14,14 while the control read 0,0,0,0.
    /// </summary>
    [Fact]
    public void A_context_menu_surface_is_the_frameworks_border_wearing_our_rows_and_upstreams_radius()
    {
        _fixture.Run(() =>
        {
            var menu = new ContextMenu();
            menu.Items.Add(new MenuItem { Header = "cut" });
            menu.Items.Add(new MenuItem { Header = "copy" });
            menu.Open(new Point(120, 120));
            PixelHarness.Settle(40);
            try
            {
                var surface = SurfaceOf(menu.Items[0] as DependencyObject);
                Assert.True(menu.CornerRadius == surface.CornerRadius,
                    $"the framework did not copy the control's radius: control={menu.CornerRadius} surface={surface.CornerRadius}");
                Assert.Equal((CornerRadius)TryRes("OverlayCornerRadius")!, surface.CornerRadius);
                Assert.Same(Res("MenuFlyoutPresenterBackground"), surface.Background);
                Assert.Same(Res("MenuFlyoutPresenterBorderBrush"), surface.BorderBrush);
                Assert.Equal(new Thickness(1), surface.BorderThickness);
                Assert.Multiple(
                    () => Assert.NotEqual(DependencyProperty.UnsetValue, surface.ReadLocalValue(Border.CornerRadiusProperty)),
                    () => Assert.NotEqual(DependencyProperty.UnsetValue, surface.ReadLocalValue(Border.BackgroundProperty)),
                    () => Assert.NotNull(HostOf(surface)));
            }
            finally
            {
                menu.IsOpen = false;
            }
        });
    }

    /// <summary>
    /// Same question for the other menu surface that can be opened without a pointer: a <see cref="Menu"/>'s
    /// submenu. It wears 8 without being told to - the framework's own default for that path - which is why the
    /// ContextMenu setter is a fix for one surface and not a global cure.
    /// <para>
    /// Nothing here settles, and that is a measurement rather than a shortcut. #47 kept red-lining this test's open
    /// read, and spike/SubmenuHostProbe named the cause: the write back to false comes from
    /// <c>Popup.ClosePopup → MenuItem.OnSubmenuPopupClosed</c>, which fires whenever this thread's active window
    /// changes hands or a <see cref="ContentDialog"/> opens on the window (the next test pins that one, because it
    /// is reachable in-process). Both are outside the test, so any frames spent waiting here are exposure to an
    /// event no assertion should be about. Two readings say the wait buys nothing: the probe's frame ladder found
    /// the open write lands in the click call itself at every pump count from nought up, and this test now reads
    /// the copied radius with no pump after the click at all - which is what proves the framework writes the
    /// surface as part of opening rather than a frame later.
    /// </para>
    /// <para>
    /// What is therefore not claimed: that an open submenu <em>survives</em> a focus change. It does not, and the
    /// reading in the next test says so as a fact about the runtime.
    /// </para>
    /// </summary>
    [Fact]
    public void A_submenu_surface_wears_upstreams_radius_from_the_framework_itself()
    {
        _fixture.Run(() =>
        {
            var top = new MenuItem { Header = "edit" };
            top.Items.Add(new MenuItem { Header = "copy" });
            var menu = new Menu();
            menu.Items.Add(top);
            Mount(menu, 320, 40);
            RaiseMouseDown(top);
            Assert.Equal("True", Read(top, "IsSubmenuOpen"));

            try
            {
                var nested = top.Items[0] as DependencyObject ?? throw new InvalidOperationException("no submenu container");
                var surface = SurfaceOf(nested);
                Assert.Equal(new CornerRadius(8), surface.CornerRadius);
                Assert.Same(Res("MenuFlyoutPresenterBackground"), surface.Background);
            }
            finally
            {
                // Left open, this submenu stays registered as a light-dismiss root on the shared host's overlay,
                // where the next dialog closes it along with everything else (the probe counted nine such roots
                // after five readings). A test that pops a surface owns closing it.
                top.IsSubmenuOpen = false;
            }
        });
    }

    /// <summary>
    /// The closer #47 met, pinned as a fact about the runtime rather than left as an intermittent red: a submenu
    /// that is open does not outlive a <see cref="ContentDialog"/> shown on the same window. The dialog's own
    /// resolver calls the overlay's close-every-light-dismiss-popup step, the submenu popup is light-dismiss, and
    /// <c>MenuItem</c> writes <see cref="MenuItem.IsSubmenuOpen"/> back from that popup's <c>Closed</c> event - so
    /// the state a menu test just established is revoked by an unrelated surface opening. Measured in
    /// spike/SubmenuHostProbe, where one dialog closed all nine submenus the probe had left open.
    /// <para>
    /// This is the mechanism, not the whole account: the same write also arrives when this thread's active window
    /// changes hands, which no test in a five-minute run can rule out from inside. What the suite can do is stay out
    /// of it - close what it opens - and stop pretending a settle-padded read measures the click.
    /// </para>
    /// </summary>
    [Fact]
    public void A_submenu_does_not_outlive_a_dialog_shown_on_its_window()
    {
        _fixture.Run(() =>
        {
            var top = new MenuItem { Header = "edit" };
            top.Items.Add(new MenuItem { Header = "copy" });
            var menu = new Menu();
            menu.Items.Add(top);
            Mount(menu, 320, 40);
            RaiseMouseDown(top);
            Assert.True(top.IsSubmenuOpen);
            PixelHarness.Settle(10);

            var dialog = new ContentDialog { Title = "T", Content = "C", PrimaryButtonText = "OK" };
            try
            {
                _ = dialog.ShowAsync();
                PixelHarness.Settle(20);
                Assert.False(top.IsSubmenuOpen,
                    "the dialog no longer closed the menu's light-dismiss popup, so #47's account needs re-reading");
            }
            finally
            {
                dialog.Hide();
                top.IsSubmenuOpen = false;
            }
        });
    }

    /// <summary>
    /// What <c>MenuFlyoutPresenter</c> actually is on 26.10.9 - the reading that reopens the census' claim.
    /// The type exists and really is the MenuFlyout's surface (see the next test), but it is internal and sealed
    /// with a single <c>(MenuFlyout)</c> constructor and no DP of its own, so no markup of ours can name it and
    /// no implicit style can reach it: <c>TryFindResource</c> answers null for both the type key and the name key.
    /// </summary>
    [Fact]
    public void The_flyout_presenter_type_is_real_internal_and_unstyleable_from_our_markup()
    {
        _fixture.Run(() =>
        {
            var type = typeof(MenuFlyout).Assembly.GetType("Jalium.UI.Controls.MenuFlyoutPresenter");
            Assert.NotNull(type);
            Assert.False(type!.IsPublic);
            Assert.True(type.IsSealed);
            Assert.Equal(nameof(Control), type.BaseType?.Name);
            Assert.DoesNotContain(type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly),
                static field => field.FieldType == typeof(DependencyProperty));
            var constructor = Assert.Single(type.GetConstructors(BindingFlags.Instance | BindingFlags.Public));
            Assert.Equal(nameof(MenuFlyout), constructor.GetParameters()[0].ParameterType.Name);
            Assert.Null(Application.Current!.TryFindResource(type));
            Assert.Null(TryRes("MenuFlyoutPresenter"));
        });
    }

    /// <summary>
    /// The other half of §5.11: <c>MenuFlyout.ShowAt</c> does put a presenter in the tree - as the PopupRoot's
    /// only child, in a <c>PopupWindow</c> of its own rather than in the host's overlay - and it arrives with
    /// radius 8 and border 1 written locally, but with <strong>no</strong> Background and no BorderBrush. So the
    /// two presenter rows we publish are promised to one surface (ContextMenu) and not to this one, and its card
    /// colour belongs to the framework's popup window.
    /// </summary>
    [Fact]
    public void A_flyouts_presenter_is_in_the_tree_but_paints_nothing_of_ours()
    {
        _fixture.Run(() =>
        {
            var anchor = new Button { Content = "host", Width = 140 };
            PixelHarness.Build(anchor, 140, 32);
            var flyout = new MenuFlyout();
            flyout.Items.Add(new MenuFlyoutItem { Text = "cut" });
            flyout.ShowAt(anchor);
            PixelHarness.Settle(40);
            try
            {
                Assert.Equal("True", Read(flyout, "IsOpen"));
                var presenter = PresenterOf(flyout.Items[0] as DependencyObject);
                Assert.Equal(new CornerRadius(8), (CornerRadius)ReadDP(presenter, "CornerRadius")!);
                Assert.NotEqual(DependencyProperty.UnsetValue, presenter.ReadLocalValue(Control.CornerRadiusProperty));
                Assert.Equal(new Thickness(1), (Thickness)ReadDP(presenter, "BorderThickness")!);
                Assert.Null(((Control)presenter).Background);
                Assert.Null(((Control)presenter).BorderBrush);
                Assert.NotNull(HostOf((FrameworkElement)presenter));
            }
            finally
            {
                flyout.Hide();
            }
        });
    }

    /// <summary>
    /// The surface's geometry contract, as far as the shared host lets it be read: the popup Border realizes at
    /// 0x0 inside the test window (measured - the same open call gives 74.27x70 in a window the probe shows
    /// itself), so the arc cannot be sampled from here and the corner staircase is evidenced by
    /// <c>spike/FlyoutSurfaceProbe</c> frames instead. What does not depend on that layout is the copy contract:
    /// the framework writes our radius and our 1 DIP edge onto the Border it builds, not onto ours.
    /// </summary>
    [Fact]
    public void The_copied_surface_takes_the_radius_and_the_edge_it_is_told_to_take()
    {
        _fixture.Run(() =>
        {
            var menu = new ContextMenu();
            menu.Items.Add(new MenuItem { Header = "cut" });
            menu.Open(new Point(120, 120));
            PixelHarness.Settle(40);
            try
            {
                var surface = SurfaceOf(menu.Items[0] as DependencyObject);
                Assert.Equal(new CornerRadius(8), surface.CornerRadius);
                Assert.Equal(new Thickness(1), surface.BorderThickness);
                Assert.Equal((CornerRadius)TryRes("OverlayCornerRadius")!, menu.CornerRadius);
                menu.CornerRadius = new CornerRadius(3);
                PixelHarness.Settle(10);
                Assert.Equal(new CornerRadius(3), surface.CornerRadius);
            }
            finally
            {
                menu.IsOpen = false;
            }
        });
    }

    [Fact]
    public void A_menu_keeps_hosting_its_items_under_our_template()
    {
        _fixture.Run(() =>
        {
            var menu = new Menu();
            menu.Items.Add(new MenuItem { Header = "file" });
            menu.Items.Add(new MenuItem { Header = "edit" });
            Mount(menu, 320, 40);

            var host = Part(menu, "LayoutRoot");
            Assert.IsType<Border>(host);
            Assert.True(CountMenuItems(menu) == 2, $"only {CountMenuItems(menu)} containers built under our Menu template.");
        });
    }

    // ---------- pixels ----------

    /// <summary>
    /// The surface, not the label: a text-only subject writes no pixels at all on this runtime (the note on
    /// <see cref="PixelHarness.Build" />), which is why the claim is made against the bar's own fill - the row
    /// the style resolves for Background - while the label's brush is asserted by identity above.
    /// </summary>
    [Fact]
    public void The_menu_surface_takes_our_row_and_the_framework_grey_stays_out()
    {
        _fixture.Run(() =>
        {
            var menu = new Menu();
            menu.Items.Add(new MenuItem { Header = "file" });
            PixelHarness.Build(menu, 320, 40);
            PixelHarness.Settle(30);

            var rest = PixelHarness.Render(menu, 320, 40);
            Assert.Equal(0, rest.Count(FrameworkMenuSurface));
            Assert.Equal(0, rest.Count(BrandEmerald));

            FluentThemeManager.OverrideBrush("SubtleFillColorTransparentBrush", LabelSentinel);
            try
            {
                var painted = PixelHarness.Render(menu, 320, 40);
                Assert.True(painted.Count(LabelSentinel) > 4_000,
                    $"the bar's own fill row did not reach pixels; top={painted.Top(6)}");
            }
            finally
            {
                FluentThemeManager.OverrideBrush("SubtleFillColorTransparentBrush", null);
            }
        });
    }

    [Fact]
    public void The_flyout_item_paints_its_own_surface_row()
    {
        _fixture.Run(() =>
        {
            var item = new MenuFlyoutItem { Text = "flyout item" };
            PixelHarness.Build(item, 240, 38);
            PixelHarness.Settle(30);

            FluentThemeManager.OverrideBrush("SubtleFillColorTransparentBrush", LabelSentinel);
            try
            {
                var painted = PixelHarness.Render(item, 240, 38);
                Assert.True(painted.Count(LabelSentinel) > 3_000,
                    $"the item's resting fill row did not reach pixels; top={painted.Top(6)}");
                Assert.Equal(0, painted.Count(FrameworkMenuSurface));
                Assert.Equal(0, painted.Count(BrandEmerald));
            }
            finally
            {
                FluentThemeManager.OverrideBrush("SubtleFillColorTransparentBrush", null);
            }
        });
    }

    [Fact]
    public void The_hover_row_is_a_real_change_even_though_no_pointer_reaches_it()
    {
        // IsMouseOver cannot be written from outside (docs/astra/adaptation/00 S0-m), so the pixel half of
        // this promise stays owed to task 13. What can be asserted now is the pair the pointer would trade
        // between: the cell names the hover row, and that row is not the resting one.
        _fixture.Run(() =>
        {
            var cell = FindCell(TemplateCells(Template(FluentThemeManager.GetStyle("DefaultMenuFlyoutItemStyle"))), "IsMouseOver=True");
            var background = cell.Setters.Single(setter => (setter.Property?.Name ?? setter.PropertyName) == "Background");
            Assert.Equal("MenuFlyoutItemBackgroundPointerOver", ResourceKey(background)!);
            Assert.NotSame(Res("MenuFlyoutItemBackground"), Res("MenuFlyoutItemBackgroundPointerOver"));
        });
    }

    /// <summary>
    /// Two upstream rows exist in 26.10.9's own dictionaries - <c>MenuFlyoutItemBackgroundPressed</c> and
    /// <c>MenuBarItemBackgroundPressed</c> - so they cannot be asserted absent. They are still not ours:
    /// neither resolves to the palette instance upstream aliases its pressed row to, which is what would
    /// make overriding it a promise we keep.
    /// </summary>
    [Theory]
    [InlineData("MenuFlyoutItemBackgroundPressed", "SubtleFillColorTertiaryBrush")]
    [InlineData("MenuBarItemBackgroundPressed", "SubtleFillColorTertiaryBrush")]
    public void A_framework_key_we_do_not_publish_stays_the_frameworks(string key, string upstreamTarget)
    {
        _fixture.Run(() =>
        {
            Assert.NotNull(TryRes(key));
            Assert.NotSame(Res(upstreamTarget), TryRes(key));
        });
    }

    /// <summary>
    /// Both halves of the ghost fix in one pixel reading, with the second half re-pinned by the A2 alias layer
    /// (docs/astra/ROADMAP.md #12). The row's text is the control's own paint, so it still appears in a crop that
    /// holds no label of ours - but it is no longer unreachable: <c>TextSecondary</c> is aliased onto our palette, so
    /// overriding that palette row moves the control's own text, which is precisely what the layer exists to buy.
    /// What stays unreachable is the separator, drawn from resources of the control's own that no name of ours feeds
    /// (the same reading spike/MenuProbe pass 4 took for the rest of the menu chrome).
    /// </summary>
    [Fact]
    public void The_alias_layer_reaches_the_rows_own_text_while_our_rows_still_reach_the_rule()
    {
        _fixture.Run(() =>
        {
            var separator = new MenuFlyoutSeparator();
            PixelHarness.Build(separator, 240, 12);
            PixelHarness.Settle(20);

            var sub = new MenuFlyoutSubItem { Text = "sub" };
            PixelHarness.Build(sub, 240, 38);
            PixelHarness.Settle(20);
            FluentThemeManager.OverrideBrush("DividerStrokeColorDefaultBrush", SeparatorSentinel);
            FluentThemeManager.OverrideBrush("TextFillColorSecondaryBrush", ChevronSentinel);
            try
            {
                var line = PixelHarness.Render(separator, 240, 12);
                Assert.Equal(0, line.Count(SeparatorSentinel));

                var text = PixelHarness.Render(sub, 240, 38);
                Assert.Multiple(
                    // The palette override arrives: the framework looked its name up and got the instance we moved.
                    () => Assert.True(text.Count(ChevronSentinel) > 8,
                        $"the alias layer did not carry the palette override into the row's own text; top={text.Top(4)}"),
                    // And the framework's own grey is gone from the crop it used to ink.
                    () => Assert.Equal(0, text.Count(FrameworkRowText)));
            }
            finally
            {
                FluentThemeManager.OverrideBrush("DividerStrokeColorDefaultBrush", null);
                FluentThemeManager.OverrideBrush("TextFillColorSecondaryBrush", null);
            }
        });
    }

    /// <summary>
    /// The theme flip has to be asked of something that actually paints, and a text-only subject writes no
    /// pixels here. What carries the claim is the row's own paint, which the framework resolves from a name the A2
    /// layer aliases onto <c>TextFillColorSecondaryBrush</c>. That token is translucent, so a capture on nothing
    /// reports the light branch as page-coloured ink - measured, #9E000000 over the black host leaves 9120 of 9120
    /// pixels black - and the claim would be unfalsifiable rather than merely dim. Each branch is therefore taken on
    /// its own opaque plate and asserted at the composite <see cref="PixelHarness.Over" /> predicts, not at the
    /// brush's own bytes.
    /// </summary>
    [Fact]
    public void The_sub_items_own_paint_follows_the_theme()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            var lightInk = PixelHarness.Over(PixelHarness.LightPage, SecondaryTokenColour());
            var light = RowOnPlate(PixelHarness.LightPage);
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
            var darkInk = PixelHarness.Over(PixelHarness.DarkPage, SecondaryTokenColour());
            var dark = RowOnPlate(PixelHarness.DarkPage);
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);

            // Upstream gives a menu flyout item a transparent surface, so the falsifiable pixel claim here is the
            // negative one: the item may cover none of its own page, and only the submenu arrow inks. Measured
            // 2026-09-21: 38 of 9120 pixels on each branch, and no background brush anywhere in the built tree.
            PixelHarness.AssertNoSurfaceLands(
                new MenuFlyoutSubItem { Text = "sub" }, PixelHarness.LightPage, 240, 38, "the sub item's surface");
            PixelHarness.AssertNoSurfaceLands(
                new MenuFlyoutSubItem { Text = "sub" }, PixelHarness.DarkPage, 240, 38, "the sub item's surface");

            Assert.Multiple(
                () => Assert.True(light.Count(lightInk) > 8,
                    $"the light branch did not land {PixelHarness.Hex(lightInk)} over its plate; top={light.Top(6)}"),
                () => Assert.True(dark.Count(darkInk) > 8,
                    $"the dark branch did not land {PixelHarness.Hex(darkInk)} over its plate; top={dark.Top(6)}"),
                () => Assert.NotEqual(light.Top(2), dark.Top(2)),
                () => Assert.Equal(0, dark.Count(BrandEmerald)));
        });
    }

    /// <summary>The row's own paint captured on an opaque page, which is the only way a translucent ink can be
    /// counted at all (see <see cref="The_sub_items_own_paint_follows_the_theme"/>).</summary>
    private static PixelHarness.Sample RowOnPlate(Color plate)
    {
        var host = PixelHarness.Backdrop(new MenuFlyoutSubItem { Text = "sub" }, plate);
        PixelHarness.Build(host, 240, 38);
        PixelHarness.Settle(30);
        return PixelHarness.Render(host, 240, 38);
    }

    private static Color SecondaryTokenColour() =>
        Assert.IsType<SolidColorBrush>(FluentThemeManager.GetBrush("TextFillColorSecondaryBrush")).Color;

    // ---------- helpers ----------

    private static T Mount<T>(T element, int width, int height) where T : FrameworkElement
    {
        PixelHarness.Build(element, width, height);
        PixelHarness.Settle(20);
        return element;
    }

    private static FrameworkElement Part(DependencyObject root, string name) =>
        PixelHarness.Named(root, name) ?? throw new InvalidOperationException($"No part named {name}.");

    /// <summary>
    /// The card a popped-up menu paints: climb out of the item until the first Border, which on this host is the
    /// one the framework builds for the popup rather than anything our template owns.
    /// </summary>
    private static Border SurfaceOf(DependencyObject? inside)
    {
        var current = inside;
        for (var hops = 0; hops < 24 && current is not null; hops++)
        {
            if (current is Border border && border.Background is not null)
            {
                return border;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        throw new InvalidOperationException("no surfaced Border above the menu item - the popup is not in this tree");
    }

    /// <summary>The same climb for the flyout path, where the painter is the framework's internal presenter type.</summary>
    private static DependencyObject PresenterOf(DependencyObject? inside)
    {
        var current = inside;
        for (var hops = 0; hops < 24 && current is not null; hops++)
        {
            if (current.GetType().Name == "MenuFlyoutPresenter")
            {
                return current;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        throw new InvalidOperationException("no MenuFlyoutPresenter above the flyout item - the surface is not what was measured");
    }

    /// <summary>The framework's own scroll host, the node that sits between a menu surface and its rows.</summary>
    private static DependencyObject? HostOf(DependencyObject? root)
    {
        if (root is null)
        {
            return null;
        }

        if (root.GetType().Name == "MenuPopupScrollHost")
        {
            return root;
        }

        if (root is not Visual visual)
        {
            return null;
        }

        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(visual); index++)
        {
            if (HostOf(VisualTreeHelper.GetChild(visual, index)) is { } match)
            {
                return match;
            }
        }

        return null;
    }

    private static object? ReadDP(DependencyObject node, string name)
    {
        var property = DependencyProperty.FromName(node.GetType(), name)
            ?? throw new InvalidOperationException($"{node.GetType().Name} has no {name} property.");
        return node.GetValue(property);
    }

    /// <summary>Every string a TextBlock in this subtree carries - what a skin must never hold for a control that paints its own text.</summary>
    private static List<string> TextsIn(DependencyObject? root)
    {
        var texts = new List<string>();
        if (root is null)
        {
            return texts;
        }

        if (root is TextBlock text && text.Text is { Length: > 0 } value) texts.Add(value);
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            texts.AddRange(TextsIn(VisualTreeHelper.GetChild(root, index)));
        }

        return texts;
    }

    private static int CountMenuItems(ItemsControl menu)
    {
        var count = 0;
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(menu); index++)
        {
            count += CountMenuItemDescendants(VisualTreeHelper.GetChild(menu, index));
        }

        return count;
    }

    private static int CountMenuItemDescendants(DependencyObject? node)
    {
        var count = node is MenuItem ? 1 : 0;
        if (node is Visual visual)
        {
            for (var index = 0; index < VisualTreeHelper.GetChildrenCount(visual); index++)
            {
                count += CountMenuItemDescendants(VisualTreeHelper.GetChild(visual, index));
            }
        }

        return count;
    }

    /// <summary>
    /// Drives the handler a physical click would reach, through the public routed-event door. Not hardware
    /// input: the pointer never moves, so hover and press pixels stay owed (task 13).
    /// </summary>
    private static void RaiseMouseDown(FrameworkElement element) => element.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
    {
        RoutedEvent = UIElement.MouseDownEvent,
        Source = element,
    });

    private static string Read(object target, string name) =>
        target.GetType().GetProperty(name)?.GetValue(target)?.ToString() ?? "null";

    private static object? TryRes(string key) => Application.Current!.TryFindResource(key);

    private static Brush Res(string key) => (Brush)Application.Current!.TryFindResource(key)!;

    private static ControlTemplate Template(Style style) =>
        (style.Setters.Cast<object>().OfType<Setter>()
            .First(static setter => (setter.Property?.Name ?? setter.PropertyName) == "Template").Value as ControlTemplate)!;

    private static List<Cell> TemplateCells(ControlTemplate template) => template.Triggers.Cast<object>().Select(static trigger => trigger switch
    {
        Trigger single => new Cell(
            $"{single.Property?.Name ?? "UNRESOLVED"}={Form(single.Value)}",
            single.Setters.Cast<object>().OfType<Setter>().ToArray()),
        MultiTrigger multi => new Cell(
            string.Join("+", multi.Conditions.Cast<object>().OfType<Condition>()
                .Select(condition => $"{condition.Property?.Name ?? "UNRESOLVED"}={Form(condition.Value)}")),
            multi.Setters.Cast<object>().OfType<Setter>().ToArray()),
        _ => new Cell(trigger.GetType().Name, []),
    }).ToList();

    private static Cell FindCell(List<Cell> cells, string condition) =>
        cells.Find(candidate => candidate.Condition == condition)
        ?? throw new InvalidOperationException($"No cell for {condition}; the template carries " +
            string.Join(" | ", cells.Select(static candidate => candidate.Condition)));

    private static string Form(object? value) => value?.ToString() ?? "null";

    private static string? ResourceKey(Setter setter) =>
        setter.Value?.GetType().GetProperty("ResourceKey")?.GetValue(setter.Value) as string;

    private sealed record Cell(string Condition, Setter[] Setters);

    private sealed class DelegateCommand(Action action) : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => action();
    }
}
