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
using ShapePath = Jalium.UI.Shapes.Path;

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
    [InlineData("MenuFlyoutSeparatorBackground", "DividerStrokeColorDefaultBrush")]
    [InlineData("MenuFlyoutItemBackground", "SubtleFillColorTransparentBrush")]
    [InlineData("MenuFlyoutItemBackgroundPointerOver", "SubtleFillColorSecondaryBrush")]
    [InlineData("MenuFlyoutItemForeground", "TextFillColorPrimaryBrush")]
    [InlineData("MenuFlyoutItemForegroundDisabled", "TextFillColorDisabledBrush")]
    [InlineData("MenuFlyoutItemKeyboardAcceleratorTextForeground", "TextFillColorSecondaryBrush")]
    [InlineData("MenuFlyoutSubItemBackgroundPointerOver", "SubtleFillColorSecondaryBrush")]
    [InlineData("MenuFlyoutSubItemForeground", "TextFillColorPrimaryBrush")]
    [InlineData("MenuFlyoutSubItemChevron", "TextFillColorSecondaryBrush")]
    [InlineData("MenuFlyoutSubItemChevronDisabled", "TextFillColorDisabledBrush")]
    [InlineData("ToggleMenuFlyoutItemKeyboardAcceleratorTextForeground", "TextFillColorSecondaryBrush")]
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
    [InlineData("MenuFlyoutItemChevronMargin", 24, 0, 0, -1)]
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

    [Fact]
    public void The_flyout_item_keeps_upstreams_five_names()
    {
        _fixture.Run(() =>
        {
            var item = Mount(new MenuFlyoutItem { Text = "item", Icon = new TextBlock { Text = "*" } }, 240, 38);

            Assert.IsType<Border>(Part(item, "LayoutRoot"));
            Assert.IsType<Border>(Part(item, "IconRoot"));
            Assert.IsType<ContentPresenter>(Part(item, "IconContent"));
            Assert.IsType<TextBlock>(Part(item, "TextBlock"));
            Assert.IsType<TextBlock>(Part(item, "KeyboardAcceleratorTextBlock"));
            Assert.Equal("item", ((TextBlock)Part(item, "TextBlock")).Text);
        });
    }

    [Fact]
    public void The_toggle_and_the_sub_item_keep_their_own_names()
    {
        _fixture.Run(() =>
        {
            var toggle = Mount(new ToggleMenuFlyoutItem { Text = "toggle" }, 240, 38);
            Assert.IsType<ShapePath>(Part(toggle, "CheckGlyph"));
            Assert.IsType<Border>(Part(toggle, "CheckPlaceholder"));

            var sub = Mount(new MenuFlyoutSubItem { Text = "sub" }, 240, 38);
            Assert.IsType<ShapePath>(Part(sub, "SubItemChevron"));
            Assert.IsType<ContentPresenter>(Part(sub, "IconContent"));
        });
    }

    [Fact]
    public void The_menu_bar_item_keeps_the_three_names_upstream_uses()
    {
        _fixture.Run(() =>
        {
            var bar = new MenuBar();
            bar.Items.Add(new MenuBarItem { Title = "view" });
            Mount(bar, 320, 40);
            var item = (MenuBarItem)bar.Items[0];

            Assert.IsType<Grid>(Part(item, "ContentRoot"));
            Assert.IsType<Border>(Part(item, "Background"));
            // The title rides on a real Button: upstream does the same so the item keeps its click and focus
            // behaviour, and measured, that button picks up our own Button template's Surface part.
            var button = (Button)Part(item, "ContentButton");
            Assert.Equal("view", button.Content as string);
            Assert.NotNull(PixelHarness.Named(button, "Surface"));
        });
    }

    [Theory]
    [InlineData("DefaultMenuFlyoutItemStyle", new[] { "Icon=null", "KeyboardAcceleratorTextOverride=", "KeyboardAcceleratorTextOverride=null", "IsMouseOver=True", "IsEnabled=False" })]
    [InlineData("DefaultToggleMenuFlyoutItemStyle", new[] { "Icon=null", "KeyboardAcceleratorTextOverride=", "KeyboardAcceleratorTextOverride=null", "IsChecked=True", "IsMouseOver=True", "IsEnabled=False" })]
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

    [Theory]
    [InlineData("DefaultMenuFlyoutItemStyle", "IsMouseOver=True", "Background", "MenuFlyoutItemBackgroundPointerOver")]
    [InlineData("DefaultMenuFlyoutItemStyle", "IsEnabled=False", "Background", "MenuFlyoutItemBackgroundDisabled")]
    [InlineData("DefaultMenuFlyoutItemStyle", "IsEnabled=False", "Foreground", "MenuFlyoutItemForegroundDisabled")]
    [InlineData("DefaultMenuFlyoutSubItemStyle", "IsMouseOver=True", "Background", "MenuFlyoutSubItemBackgroundPointerOver")]
    [InlineData("DefaultMenuFlyoutSubItemStyle", "IsMouseOver=True", "Stroke", "MenuFlyoutSubItemChevronPointerOver")]
    [InlineData("DefaultMenuFlyoutSubItemStyle", "IsEnabled=False", "Stroke", "MenuFlyoutSubItemChevronDisabled")]
    [InlineData("DefaultToggleMenuFlyoutItemStyle", "IsChecked=True", "Opacity", null)]
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

    [Fact]
    public void Checking_a_toggle_raises_the_mark_and_unchecking_lowers_it()
    {
        _fixture.Run(() =>
        {
            var toggle = Mount(new ToggleMenuFlyoutItem { Text = "toggle" }, 240, 38);
            var mark = (ShapePath)Part(toggle, "CheckGlyph");
            Assert.Equal(0d, mark.Opacity);

            toggle.IsChecked = true;
            PixelHarness.Settle(20);
            Assert.Equal(1d, mark.Opacity);

            toggle.IsChecked = false;
            PixelHarness.Settle(20);
            Assert.Equal(0d, mark.Opacity);
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

    [Fact]
    public void The_separator_and_the_chevron_paint_their_own_rows()
    {
        _fixture.Run(() =>
        {
            var separator = new MenuFlyoutSeparator();
            PixelHarness.Build(separator, 240, 12);
            PixelHarness.Settle(20);
            FluentThemeManager.OverrideBrush("DividerStrokeColorDefaultBrush", SeparatorSentinel);

            var sub = new MenuFlyoutSubItem { Text = "sub" };
            PixelHarness.Build(sub, 240, 38);
            PixelHarness.Settle(20);
            FluentThemeManager.OverrideBrush("TextFillColorSecondaryBrush", ChevronSentinel);
            try
            {
                var line = PixelHarness.Render(separator, 240, 12);
                Assert.True(line.Count(SeparatorSentinel) > 40,
                    $"the separator row did not reach pixels; top={line.Top(6)}");

                var chevron = PixelHarness.Render(sub, 240, 38);
                Assert.True(chevron.Count(ChevronSentinel) > 8,
                    $"the sub-item chevron did not reach pixels; top={chevron.Top(6)}");
            }
            finally
            {
                FluentThemeManager.OverrideBrush("DividerStrokeColorDefaultBrush", null);
                FluentThemeManager.OverrideBrush("TextFillColorSecondaryBrush", null);
            }
        });
    }

    /// <summary>
    /// The theme flip has to be asked of something that actually paints: a text-only subject writes no
    /// pixels here, so the chevron - a stroked Path reading a row whose light and dark values differ - is
    /// what carries the claim.
    /// </summary>
    [Fact]
    public void The_sub_item_chevron_follows_the_theme()
    {
        _fixture.Run(() =>
        {
            var sub = new MenuFlyoutSubItem { Text = "sub" };
            PixelHarness.Build(sub, 240, 38);
            PixelHarness.Settle(30);

            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            var light = PixelHarness.Render(sub, 240, 38);
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
            var dark = PixelHarness.Render(sub, 240, 38);
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);

            Assert.True(light.PaintedPixels > 0, $"light capture is empty: {light.Top(6)}");
            Assert.True(dark.PaintedPixels > 0, $"dark capture is empty: {dark.Top(6)}");
            Assert.NotEqual(light.Top(2), dark.Top(2));
            Assert.Equal(0, dark.Count(BrandEmerald));
        });
    }

    // ---------- helpers ----------

    private static T Mount<T>(T element, int width, int height) where T : FrameworkElement
    {
        PixelHarness.Build(element, width, height);
        PixelHarness.Settle(20);
        return element;
    }

    private static FrameworkElement Part(DependencyObject root, string name) =>
        PixelHarness.Named(root, name) ?? throw new InvalidOperationException($"No part named {name}.");

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
