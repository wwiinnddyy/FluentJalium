using FluentJalium.Themes;
using FluentJalium.Tests.Pixel;
using Jalium.UI;
using Jalium.UI.Automation.Peers;
using Jalium.UI.Automation.Provider;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Input;
using Jalium.UI.Markup;
using Jalium.UI.Media;

namespace FluentJalium.Tests;

/// <summary>
/// The third stage-4 segment: the command bar. Everything here hangs off one measurement that changed the
/// method (spike/AppBarProbe passes 1-5, logs kept beside the source).
/// <para>
/// 26.10.9 already ships implicit styles for AppBarButton, AppBarToggleButton and AppBarSeparator, and it
/// paints its CommandBar from nine rows of its own. A row we publish under one of those names wins the
/// collision for every control built after the theme is installed, and an implicit style of ours wins over
/// the framework's outright - both read back by ReferenceEquals, not by colour match. That is why this batch
/// is mostly a row transcription and only partly a template: the framework's geometry is not upstream's (its
/// app-bar button measured 44 wide, stretched to the window, with a 10-pixel label and a 20x20 icon box
/// against upstream's 68, 12 and 16), and its resting foreground is an accent-derived purple.
/// </para>
/// <para>
/// The CommandBar itself cannot be retemplated: it stores a Template and never builds one, the MenuItem case,
/// so its surface, its 40x32 ellipsis button and its overflow list stay code-drawn and follow only the rows
/// its own painting reads. <see cref="The_open_bar_shows_its_overflow_outside_the_surface_this_capture_can_reach" /> is the one
/// place where that promise is tested against pixels rather than against markup.
/// </para>
/// <para>
/// No framework colour of this family is accepted by name: #2C2C2E and #3A3A3C for the bar and its overflow,
/// #1C1C1E, #48484A and the #680081 purple for the buttons, #673B71 and #636366 for their disabled states.
/// </para>
/// </summary>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraAppBarTests
{
    private static readonly Color SentinelMagenta = Color.FromRgb(0xFF, 0x00, 0xFF);
    private static readonly Color SentinelLime = Color.FromRgb(0x00, 0xFF, 0x00);
    private static readonly Color BrandEmerald = Color.FromRgb(0x20, 0x72, 0x45);

    /// <summary>The colours 26.10.9's own app-bar skin paints when nothing of ours reaches it.</summary>
    private static readonly Color FrameworkBarSurface = Color.FromRgb(0x2C, 0x2C, 0x2E);
    private static readonly Color FrameworkBarOverflow = Color.FromRgb(0x3A, 0x3A, 0x3C);
    private static readonly Color FrameworkButtonSurface = Color.FromRgb(0x1C, 0x1C, 0x1E);
    private static readonly Color FrameworkDivider = Color.FromRgb(0x48, 0x48, 0x4A);
    private static readonly Color FrameworkButtonForeground = Color.FromRgb(0x68, 0x00, 0x81);

    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraAppBarTests(AstraThemeRuntimeFixture fixture)
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
    [InlineData("AppBarButtonBackground", "SubtleFillColorTransparentBrush")]
    [InlineData("AppBarButtonBackgroundPointerOver", "SubtleFillColorSecondaryBrush")]
    [InlineData("AppBarButtonBackgroundPressed", "SubtleFillColorTertiaryBrush")]
    [InlineData("AppBarButtonBackgroundDisabled", "SubtleFillColorDisabledBrush")]
    [InlineData("AppBarButtonForeground", "TextFillColorPrimaryBrush")]
    [InlineData("AppBarButtonForegroundPointerOver", "TextFillColorPrimaryBrush")]
    [InlineData("AppBarButtonForegroundPressed", "TextFillColorSecondaryBrush")]
    [InlineData("AppBarButtonForegroundDisabled", "TextFillColorDisabledBrush")]
    [InlineData("AppBarButtonBorderBrush", "ControlFillColorTransparentBrush")]
    [InlineData("AppBarButtonBorderBrushPointerOver", "ControlFillColorTransparentBrush")]
    [InlineData("AppBarButtonBorderBrushPressed", "ControlFillColorTransparentBrush")]
    [InlineData("AppBarButtonBorderBrushDisabled", "ControlFillColorTransparentBrush")]
    [InlineData("AppBarToggleButtonBackground", "SubtleFillColorTransparentBrush")]
    [InlineData("AppBarToggleButtonBackgroundPointerOver", "SubtleFillColorSecondaryBrush")]
    [InlineData("AppBarToggleButtonBackgroundPressed", "SubtleFillColorTertiaryBrush")]
    [InlineData("AppBarToggleButtonBackgroundDisabled", "SubtleFillColorDisabledBrush")]
    [InlineData("AppBarToggleButtonForeground", "TextFillColorPrimaryBrush")]
    [InlineData("AppBarToggleButtonForegroundPointerOver", "TextFillColorPrimaryBrush")]
    [InlineData("AppBarToggleButtonForegroundPressed", "TextFillColorSecondaryBrush")]
    [InlineData("AppBarToggleButtonForegroundDisabled", "TextFillColorDisabledBrush")]
    [InlineData("AppBarToggleButtonBorderBrush", "ControlFillColorTransparentBrush")]
    [InlineData("AppBarToggleButtonBorderBrushPointerOver", "ControlFillColorTransparentBrush")]
    [InlineData("AppBarToggleButtonBorderBrushPressed", "ControlFillColorTransparentBrush")]
    [InlineData("AppBarToggleButtonBorderBrushDisabled", "ControlFillColorTransparentBrush")]
    [InlineData("AppBarToggleButtonBackgroundChecked", "AccentFillColorDefaultBrush")]
    [InlineData("AppBarToggleButtonForegroundChecked", "TextOnAccentFillColorPrimaryBrush")]
    [InlineData("AppBarToggleButtonBorderBrushChecked", "ControlStrokeColorOnAccentDefaultBrush")]
    [InlineData("AppBarSeparatorForeground", "DividerStrokeColorDefaultBrush")]
    [InlineData("CommandBarBackground", "ControlFillColorTransparentBrush")]
    [InlineData("CommandBarBorderBrush", "CardStrokeColorDefaultSolidBrush")]
    [InlineData("CommandBarForeground", "TextFillColorPrimaryBrush")]
    public void An_app_bar_alias_resolves_to_the_object_upstream_names(string alias, string target)
    {
        _fixture.Run(() => Assert.Same(Res(target), Res(alias)));
    }

    [Theory]
    [InlineData("AppBarButtonInnerBorderMargin", 2, 6, 2, 6)]
    [InlineData("AppBarButtonInnerBorderCompactMargin", 2, 6, 2, 22)]
    [InlineData("AppBarButtonContentViewboxCollapsedMargin", 0, 16, 0, 2)]
    [InlineData("AppBarButtonTextLabelMargin", 2, 0, 2, 8)]
    [InlineData("AppBarSeparatorMargin", 2, 8, 2, 8)]
    public void An_app_bar_geometry_row_carries_upstreams_value(string key, double left, double top, double right, double bottom)
    {
        _fixture.Run(() =>
        {
            var value = Assert.IsType<Thickness>(Application.Current!.TryFindResource(key));
            Assert.Equal(left, value.Left);
            Assert.Equal(top, value.Top);
            Assert.Equal(right, value.Right);
            Assert.Equal(bottom, value.Bottom);
        });
    }

    /// <summary>
    /// The deferral, pinned. Upstream also publishes the checked-variant pairs, the highlight-overlay set,
    /// the check-glyph set, the accelerator-text rows and the chevron rows for this family; none of them has a
    /// consumer here, because a cell needs one condition and those states are all pairs (checked AND hovered),
    /// because the overflow glyph has no flag to reveal it, and because these types carry no accelerator text
    /// and no submenu. A published key is a promise that overriding it reaches pixels.
    /// </summary>
    [Theory]
    [InlineData("AppBarToggleButtonBackgroundCheckedPointerOver")]
    [InlineData("AppBarToggleButtonBackgroundCheckedPressed")]
    [InlineData("AppBarToggleButtonBackgroundCheckedDisabled")]
    [InlineData("AppBarToggleButtonForegroundCheckedPointerOver")]
    [InlineData("AppBarToggleButtonForegroundCheckedPressed")]
    [InlineData("AppBarToggleButtonForegroundCheckedDisabled")]
    [InlineData("AppBarToggleButtonBorderBrushCheckedPointerOver")]
    [InlineData("AppBarToggleButtonBorderBrushCheckedPressed")]
    [InlineData("AppBarToggleButtonBorderBrushCheckedDisabled")]
    [InlineData("AppBarToggleButtonBackgroundHighLightOverlay")]
    [InlineData("AppBarToggleButtonCheckGlyphForeground")]
    [InlineData("AppBarToggleButtonCheckGlyphForegroundChecked")]
    [InlineData("AppBarButtonKeyboardAcceleratorTextForeground")]
    [InlineData("AppBarButtonSubItemChevronForeground")]
    [InlineData("AppBarButtonHasFlyoutChevronVisibility")]
    [InlineData("AppBarButtonFlyoutGlyph")]
    [InlineData("AppBarButtonContentViewboxMargin")]
    [InlineData("AppBarButtonInnerBorderOverflowMargin")]
    [InlineData("AppBarButtonWidth")]
    [InlineData("AppBarSeparatorWidth")]
    [InlineData("AppBarSeparatorCornerRadius")]
    [InlineData("CommandBarBackgroundOpen")]
    [InlineData("CommandBarEllipsisIconForegroundDisabled")]
    [InlineData("CommandBarLightDismissOverlayBackground")]
    [InlineData("CommandBarOverflowPresenterBackground")]
    [InlineData("CommandBarBorderThicknessOpen")]
    [InlineData("CommandBarOverflowMinWidth")]
    [InlineData("AppBarThemeCompactHeight")]
    public void A_row_with_no_consumer_is_not_published(string key)
    {
        _fixture.Run(() => Assert.Null(TryRes(key)));
    }

    /// <summary>
    /// The other half of the same honesty: one name in this family belongs to the host, not to us. The
    /// framework's own button style reads <c>AppBarButtonBackgroundHover</c> - its spelling, not upstream's
    /// <c>...PointerOver</c> - and we publish no such row, because our template never builds for a control that
    /// keeps the host's style. Absence cannot be asserted for a name the framework itself declares, so the
    /// claim is that the row is still the host's object and not the token our alias points at.
    /// </summary>
    [Theory]
    [InlineData("AppBarButtonBackgroundHover", "SubtleFillColorSecondaryBrush")]
    [InlineData("CommandBarOverflowBackground", "AcrylicInAppFillColorDefaultBrush")]
    public void A_host_row_we_do_not_publish_stays_the_frameworks(string key, string token)
    {
        _fixture.Run(() =>
        {
            // Absence cannot be asserted for a name the host itself declares - the resource space answers either
            // way - so the claim is that the object behind it is still the host's and not the token our alias
            // would have pointed at.
            Assert.NotNull(TryRes(key));
            Assert.NotSame(Res(token), TryRes(key));
        });
    }

    // ---------- what can be retemplated, and what only reads rows ----------

    [Theory]
    [InlineData("AppBarButton", "DefaultAppBarButtonStyle")]
    [InlineData("AppBarToggleButton", "DefaultAppBarToggleButtonStyle")]
    [InlineData("AppBarSeparator", "DefaultAppBarSeparatorStyle")]
    public void The_implicit_style_reaches_the_templatable_app_bar_types(string type, string styleKey)
    {
        _fixture.Run(() =>
        {
            // Typed, not by name: Type.GetType("Jalium.UI.Controls.AppBarButton") does not resolve from an
            // assembly that only references it, and a theory that silently built nothing would pass.
            FrameworkElement element = type switch
            {
                "AppBarButton" => new AppBarButton { Label = "one" },
                "AppBarToggleButton" => new AppBarToggleButton { Label = "one" },
                _ => new AppBarSeparator(),
            };
            Mount(element, 68, 64);

            // An implicit style never lands in FrameworkElement.Style on this runtime, so the template object
            // is the readable proof - and here it is also the proof that ours beat the framework's own.
            Assert.Null(element.Style);
            Assert.Same(Template(FluentThemeManager.GetStyle(styleKey)), ((Control)element).Template);
            Assert.NotNull(PixelHarness.Named(element, "Root") ?? PixelHarness.Named(element, "RootGrid"));
        });
    }

    /// <summary>
    /// The asymmetry, as an assertion. The same assignment that skins an app-bar button leaves the bar with a
    /// stored Template and no tree of ours, because the bar builds its own StackPanel, ellipsis button and
    /// popup in code. Nothing in a structure reading of the bar would say so if this were not pinned.
    /// </summary>
    [Fact]
    public void The_command_bar_stores_a_template_but_never_builds_one()
    {
        _fixture.Run(() =>
        {
            var bar = new CommandBar { Template = Template(FluentThemeManager.GetStyle("DefaultAppBarButtonStyle")) };
            Mount(bar, 420, 48);

            Assert.NotNull(bar.Template);
            Assert.Null(PixelHarness.Named(bar, "ContentViewbox"));
            Assert.Null(PixelHarness.Named(bar, "LabelText"));

            // Its own children still build, which is what makes the failure invisible rather than fatal.
            bar.PrimaryCommands.Add(new AppBarButton { Label = "save" });
            PixelHarness.Settle(30);
            var hosted = FindChild<AppBarButton>(bar);
            Assert.NotNull(hosted);
            Assert.Equal(68d, hosted!.ActualWidth, 1);
        });
    }

    [Fact]
    public void The_command_bar_holds_the_rows_its_own_painting_reads()
    {
        _fixture.Run(() =>
        {
            var bar = Mount(new CommandBar(), 420, 48);

            // By identity, not by colour: these are the objects our dictionary published, and the bar's own
            // code resolved them when the control was built.
            Assert.Same(Res("CommandBarBackground"), bar.Background);
            Assert.Same(Res("CommandBarBorderBrush"), bar.BorderBrush);
            Assert.Same(Res("CommandBarForeground"), bar.Foreground);
            Assert.False(bar.HasLocalValue(Control.BackgroundProperty));
        });
    }

    /// <summary>
    /// Two readings of the bar's open state, one per claim, and both narrower than the first draft of this test.
    /// The first draft published <c>CommandBarOverflowBackground</c> and called it unreachable because "an
    /// override of the token behind it moved no pixel of an open bar"; that null result was not attributable -
    /// the same assertion passed inside this class and failed inside the full suite, because another class's
    /// still-open acrylic popup paints the shared host window on its own. Retinting before both frames makes the
    /// reading a difference rather than an absolute count, and what it says is: the bar does open its overflow for
    /// a click on its own ellipsis button (pass 4's flag-only reading missed that), and the opened surface
    /// contributes nothing to the capture this test can take. So the seven open-state rows stay withheld, and the
    /// claim about them is "not measurable from here", not "provably does nothing".
    /// </summary>
    [Fact]
    public void The_open_bar_shows_its_overflow_outside_the_surface_this_capture_can_reach()
    {
        _fixture.Run(() =>
        {
            Assert.NotSame(Res("AcrylicInAppFillColorDefaultBrush"), TryRes("CommandBarOverflowBackground"));

            var host = PixelHarness.HostWindow();
            var bar = new CommandBar { Width = 420, Height = 48 };
            bar.SecondaryCommands.Add(new AppBarButton { Label = "secondary" });
            PixelHarness.Host(bar, 420, 300);
            PixelHarness.Settle(40);

            FluentThemeManager.OverrideBrush("AcrylicInAppFillColorDefaultBrush", SentinelLime);
            PixelHarness.Settle(60);
            var before = PixelHarness.Chrome(host);

            var more = FindChild<Button>(bar);
            Assert.NotNull(more);
            RaiseMouse(more!, UIElement.MouseDownEvent);
            RaiseMouse(more!, UIElement.MouseUpEvent);
            more!.ReleaseMouseCapture();
            bar.IsOpen = true;
            PixelHarness.Settle(80);
            var after = PixelHarness.Chrome(host);
            var popup = FindChild<Popup>(bar);
            var popupOpened = popup?.IsOpen ?? false;
            var barIsOpen = bar.IsOpen;
            var difference = after.Count(SentinelLime) - before.Count(SentinelLime);
            FluentThemeManager.OverrideBrush("AcrylicInAppFillColorDefaultBrush", null);

            // The opened overflow lives in the shared host window, so it is taken back down before returning: left
            // standing there, its acrylic surface sits in every later class's capture and their clean-frame
            // assertions fail for a reason that has nothing to do with them.
            bar.IsOpen = false;
            PixelHarness.Settle(40);

            Assert.True(barIsOpen);
            Assert.NotNull(popup);
            Assert.True(popupOpened, "the bar did not open its overflow for a click on its own ellipsis button.");
            Assert.Equal(0, difference);
            Assert.True(after.Width > 0 && after.Height > 0);
        });
    }

    // ---------- cells ----------

    [Theory]
    [InlineData("DefaultAppBarButtonStyle", "IsMouseOver=True")]
    [InlineData("DefaultAppBarButtonStyle", "IsPressed=True")]
    [InlineData("DefaultAppBarButtonStyle", "IsEnabled=False")]
    [InlineData("DefaultAppBarButtonStyle", "IsCompact=True")]
    [InlineData("DefaultAppBarToggleButtonStyle", "IsMouseOver=True")]
    [InlineData("DefaultAppBarToggleButtonStyle", "IsPressed=True")]
    [InlineData("DefaultAppBarToggleButtonStyle", "IsChecked=True")]
    [InlineData("DefaultAppBarToggleButtonStyle", "IsEnabled=False")]
    [InlineData("DefaultAppBarToggleButtonStyle", "IsCompact=True")]
    public void A_reachable_state_has_a_cell(string styleKey, string condition)
    {
        _fixture.Run(() =>
        {
            var cells = TemplateCells(Template(FluentThemeManager.GetStyle(styleKey)));
            Assert.NotNull(FindCell(cells, condition));
        });
    }

    [Theory]
    [InlineData("DefaultAppBarButtonStyle", 4)]
    [InlineData("DefaultAppBarToggleButtonStyle", 5)]
    [InlineData("DefaultAppBarSeparatorStyle", 0)]
    public void No_unreachable_state_gets_a_cell(string styleKey, int count)
    {
        _fixture.Run(() => Assert.Equal(count, TemplateCells(Template(FluentThemeManager.GetStyle(styleKey))).Count));
    }

    [Fact]
    public void Each_cell_writes_the_rows_upstream_writes_in_that_state()
    {
        _fixture.Run(() =>
        {
            var cells = TemplateCells(Template(FluentThemeManager.GetStyle("DefaultAppBarButtonStyle")));
            var expected = new[]
            {
                ("IsMouseOver=True", "AppBarButtonBackgroundPointerOver"),
                ("IsPressed=True", "AppBarButtonBackgroundPressed"),
                ("IsEnabled=False", "AppBarButtonBackgroundDisabled"),
            };

            foreach (var (condition, background) in expected)
            {
                var keys = FindCell(cells, condition).Setters.Select(ResourceKey).Where(static key => key is not null).Select(static key => key!).ToList();
                Assert.Contains(background, keys);
                // Upstream's state rows carry the state as a suffix (...ForegroundPointerOver), so the pair is
                // matched by containing the property, not by ending with it.
                Assert.Contains(keys, static key => key.Contains("Foreground", StringComparison.Ordinal) || key.Contains("BorderBrush", StringComparison.Ordinal));
            }

            // Compact is the pair upstream's ApplicationViewStates select: the label leaves, the inner border
            // takes the 2,6,2,22 margin, and the content band drops to 48 - upstream's LabelCollapsed writes
            // AppBarThemeCompactHeight onto ContentRoot.MinHeight, and IsCompact is one property on this runtime.
            var compact = FindCell(cells, "IsCompact=True").Setters;
            Assert.Contains(compact, static setter => (setter.Property?.Name ?? setter.PropertyName) == "Visibility");
            Assert.Contains(compact, static setter => (setter.Property?.Name ?? setter.PropertyName) == "Margin");
            Assert.Contains(compact, static setter => (setter.Property?.Name ?? setter.PropertyName) == "MinHeight");

            var toggle = TemplateCells(Template(FluentThemeManager.GetStyle("DefaultAppBarToggleButtonStyle")));
            var checkedKeys = FindCell(toggle, "IsChecked=True").Setters
                .Select(ResourceKey).Where(static key => key is not null).Select(static key => key!).ToList();
            Assert.Contains("AppBarToggleButtonBackgroundChecked", checkedKeys);
            Assert.Contains("AppBarToggleButtonForegroundChecked", checkedKeys);
            Assert.Contains("AppBarToggleButtonBorderBrushChecked", checkedKeys);
            // Upstream reveals no glyph in the bar: the accent fill is the mark.
            Assert.DoesNotContain(checkedKeys, static key => key.Contains("Glyph", StringComparison.Ordinal));
        });
    }

    [Fact]
    public void A_pressed_bar_button_writes_the_pressed_rows()
    {
        _fixture.Run(() =>
        {
            var button = Mount(new AppBarButton { Label = "save" }, 68, 64);
            PixelHarness.Settle(60);
            var root = (Border)Part(button, "AppBarButtonInnerBorder");
            // The root carries upstream's 83ms brush transition, so the resting state is read as a colour; the
            // identity claim lives on the label, which has no transition, and on the rows themselves.
            Assert.Equal(ColorOf(Res("AppBarButtonBackground")), ColorOf(root.Background));

            RaiseMouse(button, UIElement.MouseDownEvent);
            PixelHarness.Settle(40);

            Assert.True(button.IsPressed, "the runtime did not press the button the way it presses every other one.");
            Assert.Equal(ColorOf(Res("SubtleFillColorTertiaryBrush")), ColorOf(root.Background));
            Assert.Same(Res("TextFillColorSecondaryBrush"), ((TextBlock)Part(button, "LabelText")).Foreground);

            // A button that captured on the way down keeps the capture until something lets go, and the next
            // test in this class would then be raising its events into a stale capture.
            // Pass 4 read IsPressed as still set after a synthesized MouseUp, and an earlier draft of this file
            // shipped that as a limit on the runtime. Pass 5 measured the opposite on the shipping template, so
            // it is asserted here: the release clears the flag and the pressed rows leave with it. What pass 4
            // actually hit was its own throwaway template, whose cells never fire at all (audits/app-bar.md).
            RaiseMouse(button, UIElement.MouseUpEvent);
            button.ReleaseMouseCapture();
            PixelHarness.Settle(200);

            // Released under a pointer that never moved, which is what the routed pair leaves behind: the press
            // cell lets go and the hover cell takes the surface, exactly upstream's answer for a button the
            // pointer is still over. The three colours are distinct readings, not an interpretation - resting
            // #00FFFFFF, pressed #06000000, hover #09000000 - and the same pair with the transition zeroed on a
            // second button ends in the same hover colour, so the transition is not what holds the value.
            Assert.False(button.IsPressed, "a synthesized MouseUp left the button pressed.");
            Assert.True(button.IsMouseOver, "the routed pair did not leave the pointer over the button.");
            Assert.Equal(ColorOf(Res("AppBarButtonBackgroundPointerOver")), ColorOf(root.Background));
            // Upstream's hover foreground is the resting text colour, so this identity is the hover cell's own
            // writing rather than a leftover from the press.
            Assert.Same(Res("TextFillColorPrimaryBrush"), ((TextBlock)Part(button, "LabelText")).Foreground);
        });
    }

    [Fact]
    public void A_disabled_bar_button_writes_the_disabled_rows()
    {
        _fixture.Run(() =>
        {
            var button = Mount(new AppBarButton { Label = "save", IsEnabled = false }, 68, 64);
            PixelHarness.Settle(40);

            Assert.Equal(ColorOf(Res("SubtleFillColorDisabledBrush")), ColorOf(((Border)Part(button, "AppBarButtonInnerBorder")).Background));
            Assert.Same(Res("TextFillColorDisabledBrush"), ((TextBlock)Part(button, "LabelText")).Foreground);
        });
    }

    /// <summary>
    /// Both halves of the checked pair, in one run: the accent has to arrive with IsChecked and must not be
    /// there before it. Pass 4 left one reading ambiguous - after a checked toggle was unset, the fill stayed
    /// accent for a while - so the resting case is asserted on a control that was never checked at all.
    /// </summary>
    [Fact]
    public void A_checked_toggle_writes_the_accent_rows_and_an_untouched_one_does_not()
    {
        _fixture.Run(() =>
        {
            var untouched = Mount(new AppBarToggleButton { Label = "bold" }, 68, 64);
            PixelHarness.Settle(60);
            Assert.Equal(ColorOf(Res("SubtleFillColorTransparentBrush")), ColorOf(((Border)Part(untouched, "AppBarButtonInnerBorder")).Background));

            var toggle = Mount(new AppBarToggleButton { Label = "bold" }, 68, 64);
            toggle.IsChecked = true;
            PixelHarness.Settle(60);

            Assert.Equal(ColorOf(Res("AccentFillColorDefaultBrush")), ColorOf(((Border)Part(toggle, "AppBarButtonInnerBorder")).Background));
            Assert.Same(Res("TextOnAccentFillColorPrimaryBrush"), ((TextBlock)Part(toggle, "LabelText")).Foreground);

            toggle.IsChecked = false;
            PixelHarness.Settle(60);
            Assert.NotEqual(ColorOf(Res("AccentFillColorDefaultBrush")), ColorOf(((Border)Part(toggle, "AppBarButtonInnerBorder")).Background));
        });
    }

    [Fact]
    public void A_compact_bar_button_drops_its_label_and_widens_its_margin()
    {
        _fixture.Run(() =>
        {
            var button = Mount(new AppBarButton { Label = "save" }, 68, 64);
            var root = (Border)Part(button, "AppBarButtonInnerBorder");
            var label = (TextBlock)Part(button, "LabelText");
            Assert.Equal(new Thickness(2, 6, 2, 6), root.Margin);
            Assert.True(Shown(label));

            button.IsCompact = true;
            PixelHarness.Settle(40);

            Assert.False(Shown(label), "IsCompact left the label in.");
            Assert.Equal(new Thickness(2, 6, 2, 22), root.Margin);

            button.IsCompact = false;
            PixelHarness.Settle(40);
            Assert.True(Shown(label));
        });
    }

    /// <summary>
    /// Why the LabelOnRight rows are withheld, pinned on the types instead of in prose. The per-button enum is
    /// not the difference - <c>CommandBarLabelPosition</c> is Default/Collapsed upstream too (controls2.idl:100)
    /// and pass 5 measured that neither member moves the label or the padding on its own here, so only IsCompact
    /// gives an application-view state a cell can watch. The difference is one level up: upstream's side-by-side
    /// layout is the bar's <c>DefaultLabelPosition</c>, and a CommandBar never builds a template, so there is no
    /// part of ours for that state to reach. An upstream state group nothing here can name has no row that could
    /// ever be read, so publishing its colours would promise an override that reaches nothing.
    /// </summary>
    [Fact]
    public void LabelPosition_names_two_states_and_moves_neither()
    {
        _fixture.Run(() =>
        {
            Assert.Equal(["Default", "Collapsed"], Enum.GetNames<CommandBarLabelPosition>());
            Assert.Equal(["Bottom", "Right", "Collapsed"], Enum.GetNames<CommandBarDefaultLabelPosition>());

            foreach (var styleKey in new[] { "DefaultAppBarButtonStyle", "DefaultAppBarToggleButtonStyle" })
            {
                Assert.DoesNotContain(TemplateCells(Template(FluentThemeManager.GetStyle(styleKey))),
                    static cell => cell.Condition.StartsWith("LabelPosition", StringComparison.Ordinal));
            }

            var button = Mount(new AppBarButton { Label = "save" }, 68, 64);
            var root = (Border)Part(button, "AppBarButtonInnerBorder");
            var resting = ColorOf(root.Background);
            foreach (var name in Enum.GetNames<CommandBarLabelPosition>())
            {
                button.LabelPosition = Enum.Parse<CommandBarLabelPosition>(name);
                PixelHarness.Settle(60);
                Assert.True(Shown((TextBlock)Part(button, "LabelText")), $"LabelPosition.{name} took the label out on its own.");
                Assert.Equal(new Thickness(2, 6, 2, 6), root.Margin);
                Assert.Equal(resting, ColorOf(root.Background));
            }
        });
    }

    /// <summary>
    /// How far the hover leg can be driven without a mouse. <c>IsMouseOver</c> has no public setter and no
    /// readable key on this runtime (passes 4 and 5 both came back with MissingFieldException), so nothing can
    /// put a pointer over the control the way moving one does - but a routed MouseDown leaves the flag set by
    /// itself, and a fresh mount is not hovered, which is what makes the second reading a hover measurement
    /// rather than a leftover. What stays owed to a real pointer is the enter-and-leave cycle, not the row.
    /// </summary>
    [Fact]
    public void The_hover_row_reaches_the_surface_once_a_routed_press_leaves_the_pointer_over()
    {
        _fixture.Run(() =>
        {
            var untouched = Mount(new AppBarButton { Label = "save" }, 68, 64);
            PixelHarness.Settle(60);
            Assert.False(untouched.IsMouseOver, "a control nobody touched arrived hovered.");
            Assert.Equal(ColorOf(Res("SubtleFillColorTransparentBrush")), ColorOf(((Border)Part(untouched, "AppBarButtonInnerBorder")).Background));

            var button = Mount(new AppBarButton { Label = "save" }, 68, 64);
            PixelHarness.Settle(60);
            var root = (Border)Part(button, "AppBarButtonInnerBorder");
            Assert.NotSame(Res("AppBarButtonBackground"), Res("AppBarButtonBackgroundPointerOver"));

            RaiseMouse(button, UIElement.MouseDownEvent);
            RaiseMouse(button, UIElement.MouseUpEvent);
            button.ReleaseMouseCapture();
            PixelHarness.Settle(200);

            Assert.True(button.IsMouseOver, "the routed pair did not leave the pointer over the button.");
            Assert.Equal(ColorOf(Res("AppBarButtonBackgroundPointerOver")), ColorOf(root.Background));
        });
    }

    // ---------- behaviour, without a pointer ----------

    [Fact]
    public void A_left_button_down_on_a_bar_button_invokes_its_command_once()
    {
        _fixture.Run(() =>
        {
            var runs = 0;
            var button = Mount(new AppBarButton { Label = "save", Command = new DelegateCommand(() => runs++) }, 68, 64);

            RaiseMouse(button, UIElement.MouseDownEvent);
            RaiseMouse(button, UIElement.MouseUpEvent);
            PixelHarness.Settle(40);
            button.ReleaseMouseCapture();

            Assert.Equal(1, runs);
        });
    }

    [Fact]
    public void Invoking_through_the_peer_runs_the_command_the_same_way()
    {
        _fixture.Run(() =>
        {
            var runs = 0;
            var button = Mount(new AppBarButton { Label = "save", Command = new DelegateCommand(() => runs++) }, 68, 64);
            ((IInvokeProvider)new ButtonAutomationPeer(button)).Invoke();
            PixelHarness.Settle(40);
            Assert.Equal(1, runs);

            // The inverse of the flyout item, which answers Invoke and has no Toggle at all: this peer offers
            // a real IToggleProvider, and what it returns for Invoke is itself, which is not an invokable
            // pattern. Measured rather than assumed, and the difference is recorded in audits/app-bar.md.
            var toggle = Mount(new AppBarToggleButton { Label = "bold" }, 68, 64);
            var peer = new ToggleButtonAutomationPeer(toggle);
            Assert.IsAssignableFrom<IToggleProvider>(peer.GetPattern(PatternInterface.Toggle));
            Assert.False(peer.GetPattern(PatternInterface.Invoke) is IInvokeProvider,
                "this peer's Invoke answer is the peer itself, which is not an invokable pattern.");
            ((IToggleProvider)peer.GetPattern(PatternInterface.Toggle)!).Toggle();
            PixelHarness.Settle(40);
            Assert.True(toggle.IsChecked == true);
        });
    }

    /// <summary>
    /// The other half of the naming question. Expander reads three parts and does real work with them; this
    /// family reads none (pass 1 found no OnApplyTemplate override and no child lookup on any of the four
    /// types), so the names on our parts are upstream alignment. A renamed template therefore still works,
    /// and saying so is what keeps a later batch from promoting those names into a contract.
    /// </summary>
    [Fact]
    public void Renaming_the_parts_still_works_because_no_name_is_read()
    {
        _fixture.Run(() =>
        {
            var dictionary = (ResourceDictionary)XamlReader.Parse(RenamedMarkup)!;
            var runs = 0;
            var button = new AppBarButton
            {
                Label = "save",
                Command = new DelegateCommand(() => runs++),
                Template = (ControlTemplate)dictionary["RenamedAppBarButtonTemplate"]!,
            };
            Mount(button, 68, 64);

            Assert.Null(PixelHarness.Named(button, "Root"));
            Assert.Null(PixelHarness.Named(button, "LabelText"));
            var label = Assert.IsType<TextBlock>(PixelHarness.Named(button, "LabelZ"));
            Assert.Equal("save", label.Text);

            RaiseMouse(button, UIElement.MouseDownEvent);
            RaiseMouse(button, UIElement.MouseUpEvent);
            PixelHarness.Settle(40);
            button.ReleaseMouseCapture();
            Assert.Equal(1, runs);

            var rootZ = (Border)PixelHarness.Named(button, "RootZ")!;
            Assert.Equal(Color.FromRgb(0x00, 0xFF, 0x00), ((SolidColorBrush)rootZ.Background!).Color);
            Assert.Equal(Color.FromRgb(0xFF, 0x00, 0xFF), ((SolidColorBrush)label.Foreground!).Color);
        });
    }

    [Fact]
    public void The_template_carries_the_part_names_upstream_uses()
    {
        _fixture.Run(() =>
        {
            var button = Mount(new AppBarButton { Label = "save" }, 68, 64);
            // Upstream splits the cell into three elements and the split is the spacing: Root carries no fill,
            // AppBarButtonInnerBorder is the highlight inset inside it, ContentRoot owns the height.
            Assert.IsType<Grid>(Part(button, "Root"));
            Assert.IsType<Border>(Part(button, "AppBarButtonInnerBorder"));
            Assert.IsType<Grid>(Part(button, "ContentRoot"));
            Assert.IsType<Viewbox>(Part(button, "ContentViewbox"));
            Assert.IsType<ContentPresenter>(Part(button, "Content"));
            Assert.IsType<TextBlock>(Part(button, "LabelText"));
            Assert.Equal("save", ((TextBlock)Part(button, "LabelText")).Text);
            Assert.Equal(12d, ((TextBlock)Part(button, "LabelText")).FontSize, 1);
            Assert.Equal(16d, ((Viewbox)Part(button, "ContentViewbox")).Height, 1);
            Assert.Equal(64d, Part(button, "ContentRoot").MinHeight, 1);

            // The highlight fades over upstream's 83 ms. This batch once deleted that fade on a
            // composited-frame reading that turned out to be a grab of an undrawn surface, so the declaration
            // is pinned as a reading rather than trusted to a comment (adaptation/00 S0-r).
            var highlight = (Border)Part(button, "AppBarButtonInnerBorder");
            Assert.Equal("Background, BorderBrush", highlight.TransitionProperty);
            Assert.Equal(TimeSpan.FromMilliseconds(83), highlight.TransitionDuration);

            var separator = Mount(new AppBarSeparator { Height = 64 }, 8, 64);
            Assert.IsType<Grid>(Part(separator, "RootGrid"));
            Assert.IsType<Jalium.UI.Shapes.Rectangle>(Part(separator, "SeparatorRectangle"));
        });
    }

    /// <summary>
    /// The geometry the bar measures, read off the parts that carry it: the icon slot starts 16 below the cell
    /// top rather than centring in it, the highlight stops 6 short of the cell edges, and the label keeps its
    /// own 8 below. Compact is the state that moves both the inset and the band.
    /// </summary>
    [Fact]
    public void The_bar_geometry_is_the_cells_upstream_writes()
    {
        _fixture.Run(() =>
        {
            var button = Mount(new AppBarButton { Label = "save" }, 68, 64);
            var box = Part(button, "ContentViewbox");
            var highlight = Part(button, "AppBarButtonInnerBorder");
            var label = Part(button, "LabelText");
            Assert.Multiple(
                () => Assert.Equal(new Thickness(0, 16, 0, 2), box.Margin),
                () => Assert.Equal(new Thickness(2, 6, 2, 6), highlight.Margin),
                () => Assert.Equal(new Thickness(2, 0, 2, 8), label.Margin),
                () => Assert.Equal(68d, button.ActualWidth, 1),
                () => Assert.True(button.ActualHeight >= 64, $"the cell measured {button.ActualHeight}."));

            button.IsCompact = true;
            PixelHarness.Settle(60);
            Assert.Multiple(
                () => Assert.Equal(new Thickness(2, 6, 2, 22), highlight.Margin),
                () => Assert.Equal(48d, Part(button, "ContentRoot").MinHeight, 1),
                () => Assert.Equal(Visibility.Collapsed, label.Visibility));

            var separator = Mount(new AppBarSeparator { Height = 64 }, 8, 64);
            Assert.Equal(new Thickness(2, 8, 2, 8), Part(separator, "SeparatorRectangle").Margin);
            // Upstream gives the separator no height of its own: the bar's row is what makes the rule 48 inside
            // a 64 cell, so a MinHeight setter here would be our invention again.
            Assert.DoesNotContain(FluentThemeManager.GetStyle("DefaultAppBarSeparatorStyle")!.Setters
                .OfType<Setter>(), static setter => setter.Property?.Name == "MinHeight");
        });
    }

    [Fact]
    public void A_command_bar_hosts_its_primary_commands_under_our_template()
    {
        _fixture.Run(() =>
        {
            var bar = new CommandBar { Width = 420 };
            bar.PrimaryCommands.Add(new AppBarButton { Label = "one" });
            bar.PrimaryCommands.Add(new AppBarToggleButton { Label = "two" });
            bar.PrimaryCommands.Add(new AppBarSeparator());
            Mount(bar, 420, 48);

            var first = FindChild<AppBarButton>(bar)!;
            var second = FindChild<AppBarToggleButton>(bar)!;
            var divider = FindChild<AppBarSeparator>(bar)!;
            Assert.Same(Template(FluentThemeManager.GetStyle("DefaultAppBarButtonStyle")), first.Template);
            Assert.Same(Template(FluentThemeManager.GetStyle("DefaultAppBarToggleButtonStyle")), second.Template);
            Assert.Same(Template(FluentThemeManager.GetStyle("DefaultAppBarSeparatorStyle")), divider.Template);
            Assert.Equal(68d, first.ActualWidth, 1);
            Assert.Equal(68d, second.ActualWidth, 1);
            Assert.True(bar.ActualHeight >= 48, $"the bar collapsed to {bar.ActualHeight}.");

            // The highlight is a sibling of the content band, so it only reads as Fluent if it really arranges
            // inside the bar - a Gallery capture of the checked toggle showed no fill, which this measures.
            var highlight = Part(second, "AppBarButtonInnerBorder");
            Assert.True(highlight.ActualWidth > 40 && highlight.ActualHeight > 30,
                $"the highlight arranged {highlight.ActualWidth}x{highlight.ActualHeight} in a {second.ActualWidth}x{second.ActualHeight} cell.");
        });
    }

    // ---------- pixels ----------

    [Fact]
    public void The_app_bar_family_takes_our_style_rather_than_the_framework_palette()
    {
        _fixture.Run(() =>
        {
            var bar = new CommandBar { Width = 420 };
            bar.PrimaryCommands.Add(new AppBarButton { Label = "one" });
            bar.PrimaryCommands.Add(new AppBarToggleButton { Label = "two" });
            bar.PrimaryCommands.Add(new AppBarSeparator());
            var sample = PixelHarness.Render(bar, 420, 48);

            Assert.True(sample.PaintedPixels > 0, $"capture is empty: {sample.Top(6)}");
            Assert.Equal(0, sample.Count(FrameworkButtonSurface));
            Assert.Equal(0, sample.Count(FrameworkButtonForeground));
            Assert.Equal(0, sample.Count(BrandEmerald));
        });
    }

    [Fact]
    public void A_checked_toggle_sends_its_accent_token_into_the_pixels()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", SentinelMagenta);
            try
            {
                var toggle = new AppBarToggleButton { Label = "bold", IsChecked = true };
                PixelHarness.Build(toggle, 68, 64);
                PixelHarness.Settle(60);

                var sample = PixelHarness.Render(toggle, 68, 64);
                Assert.True(sample.Stable, $"capture never settled: {sample.Top(6)}");
                Assert.True(sample.Count(SentinelMagenta) > 1_000,
                    $"the checked row did not reach pixels; top={sample.Top(6)}");
            }
            finally
            {
                FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", null);
            }
        });
    }

    /// <summary>
    /// The same claim one level up. The Gallery hosts its buttons inside a CommandBar, and a capture of that
    /// page showed the checked cell with its black label and no accent fill, so "the row reached the property"
    /// and "the row reached the pixels" are separate claims when the bar arranges the cell.
    /// </summary>
    [Fact]
    public void A_checked_toggle_inside_the_bar_sends_its_accent_into_the_pixels()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", SentinelMagenta);
            try
            {
                var bar = new CommandBar { Width = 420 };
                bar.PrimaryCommands.Add(new AppBarButton { Label = "one" });
                bar.PrimaryCommands.Add(new AppBarToggleButton { Label = "two", IsChecked = true });
                var sample = PixelHarness.Render(bar, 420, 64);
                Assert.True(sample.Stable, $"capture never settled: {sample.Top(6)}");
                Assert.True(sample.Count(SentinelMagenta) > 1_000,
                    $"the checked row did not reach pixels inside the bar; top={sample.Top(6)}");

                var window = PixelHarness.HostWindow();
                var shownBar = new CommandBar { Width = 420 };
                shownBar.PrimaryCommands.Add(new AppBarToggleButton { Label = "two", IsChecked = true });
                PixelHarness.Host(shownBar, 420, 96);
                PixelHarness.Settle(60);
                var shown = PixelHarness.Chrome(window);
                Assert.True(shown.Count(SentinelMagenta) > 1_000,
                    $"the checked fill reached the offscreen rasteriser but not the shown window; top={shown.Top(6)}");
            }
            finally
            {
                FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", null);
            }
        });
    }

    /// <summary>
    /// The Gallery writes IsChecked on a bar that is already live, which is a different path from mounting a
    /// checked control: the brush has to change on a visual that has already been composited once. The
    /// offscreen rasteriser settles either way, so this reads the shown window and says which half works.
    /// </summary>
    [Fact]
    public void A_checked_fill_written_after_the_bar_is_live_reaches_the_shown_window()
    {
        _fixture.Run(() =>
        {
            var window = PixelHarness.HostWindow();
            var bar = new CommandBar { Width = 420 };
            var toggle = new AppBarToggleButton { Label = "two" };
            bar.PrimaryCommands.Add(toggle);
            PixelHarness.Host(bar, 420, 96);
            PixelHarness.Settle(60);

            FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", SentinelMagenta);
            try
            {
                toggle.IsChecked = true;
                PixelHarness.Settle(120);
                var shown = PixelHarness.Chrome(window);
                Assert.Equal(SentinelMagenta, ColorOf(((Border)Part(toggle, "AppBarButtonInnerBorder")).Background!));
                Assert.True(shown.Count(SentinelMagenta) > 1_000,
                    $"the property carries the accent but the shown window does not; top={shown.Top(6)}");
            }
            finally
            {
                FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", null);
            }
        });
    }

    [Fact]
    public void A_separator_paints_one_column_of_its_own_row()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.OverrideBrush("DividerStrokeColorDefaultBrush", SentinelMagenta);
            try
            {
                var separator = new AppBarSeparator { Height = 40 };
                PixelHarness.Build(separator, 40, 40);
                PixelHarness.Settle(60);

                var sample = PixelHarness.Render(separator, 40, 40);
                // One device-independent column, forty of them tall, less whatever the half-pixel corners cost:
                // the claim is a thin band, not a filled rectangle.
                Assert.True(sample.Count(SentinelMagenta) >= 20 && sample.Count(SentinelMagenta) <= 200,
                    $"separator pixels = {sample.Count(SentinelMagenta)}, top={sample.Top(6)}");
            }
            finally
            {
                FluentThemeManager.OverrideBrush("DividerStrokeColorDefaultBrush", null);
            }
        });
    }

    [Fact]
    public void The_app_bar_family_follows_the_theme()
    {
        _fixture.Run(() =>
        {
            var factory = new Func<AppBarToggleButton>(() => new AppBarToggleButton { Label = "bold", IsChecked = true });
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            var light = PixelHarness.Render(factory(), 68, 64);
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
            var dark = PixelHarness.Render(factory(), 68, 64);
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);

            Assert.True(light.PaintedPixels > 0, $"light capture is empty: {light.Top(6)}");
            Assert.True(dark.PaintedPixels > 0, $"dark capture is empty: {dark.Top(6)}");
            Assert.NotEqual(light.Top(2), dark.Top(2));
            Assert.Equal(0, light.Count(BrandEmerald));
            Assert.Equal(0, light.Count(FrameworkBarSurface));
        });
    }

    [Fact]
    public void The_bar_surface_is_upstreams_transparent_one_and_not_the_framework_grey()
    {
        _fixture.Run(() =>
        {
            var bar = new CommandBar { Width = 420 };
            var sample = PixelHarness.Render(bar, 420, 48);

            Assert.Equal(0, sample.Count(FrameworkBarSurface));
            Assert.Equal(0, sample.Count(FrameworkBarOverflow));
            Assert.Equal(0, sample.Count(FrameworkDivider));
            // Upstream's bar is transparent over the page, so an empty one paints the page, not a chrome colour.
            Assert.True(sample.PaintedPixels > 0, $"the bar painted nothing at all: {sample.Top(6)}");
        });
    }

    // ---------- markup for the renamed-part case ----------

    private const string RenamedMarkup = """
        <ResourceDictionary xmlns="http://schemas.jalium.ui/2024" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
          <ControlTemplate x:Key="RenamedAppBarButtonTemplate" TargetType="AppBarButton">
            <Border x:Name="RootZ" Background="#FF00FF00" Padding="2,6,2,6">
              <StackPanel>
                <Viewbox x:Name="IconZ" Width="16" Height="16">
                  <ContentPresenter x:Name="ContentZ" Content="{TemplateBinding Icon}" />
                </Viewbox>
                <TextBlock x:Name="LabelZ" Text="{TemplateBinding Label}" Foreground="#FFFF00FF" FontSize="12" />
              </StackPanel>
            </Border>
          </ControlTemplate>
        </ResourceDictionary>
        """;

    // ---------- helpers ----------

    private static T Mount<T>(T element, int width, int height) where T : FrameworkElement
    {
        PixelHarness.Build(element, width, height);
        PixelHarness.Settle(20);
        return element;
    }

    private static FrameworkElement Part(DependencyObject root, string name) =>
        PixelHarness.Named(root, name) ?? throw new InvalidOperationException($"No part named {name}.");

    private static T? FindChild<T>(DependencyObject? root) where T : DependencyObject
    {
        if (root is not Visual visual) return null;
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(visual); index++)
        {
            var child = VisualTreeHelper.GetChild(visual, index);
            if (child is T match) return match;
            if (FindChild<T>(child) is { } deeper) return deeper;
        }

        return null;
    }

    private static bool Shown(FrameworkElement element) => element.Visibility == Visibility.Visible;

    /// <summary>
    /// Drives the handler a physical click reaches, through the public routed-event door. This is not hardware
    /// input: the pointer never moves, so hover and press *pixels* stay owed (task 13).
    /// </summary>
    private static void RaiseMouse(FrameworkElement element, RoutedEvent routedEvent)
    {
        var arguments = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
        {
            RoutedEvent = routedEvent,
            Source = element,
        };
        element.RaiseEvent(arguments);
    }

    private static object? TryRes(string key) => Application.Current!.TryFindResource(key);

    private static Brush Res(string key) => (Brush)Application.Current!.TryFindResource(key)!;

    /// <summary>
    /// A transitioning property can hold an interpolated brush instance for the length of the transition, so the
    /// colour is the stable reading; object identity is asserted on rows and on properties that do not animate.
    /// </summary>
    private static Color ColorOf(Brush? brush) => Assert.IsType<SolidColorBrush>(brush).Color;

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

    private sealed class DelegateCommand(Action action) : System.Windows.Input.ICommand
    {
        public event EventHandler? CanExecuteChanged { add { } remove { } }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => action();
    }
}
