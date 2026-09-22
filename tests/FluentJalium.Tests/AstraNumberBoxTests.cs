using System.Reflection;
using FluentJalium.Themes;
using FluentJalium.Tests.Pixel;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Media;

namespace FluentJalium.Tests;

/// <summary>
/// NumberBox, the fifth control of the selection batch and the first one the objective marked "own type"
/// that turns out not to need one: Jalium ships a native NumberBox that already carries a code-built default
/// template, and a style with a Template setter displaces it (docs/astra/audits/numberbox.md). The template
/// therefore has to honour three measured framework contracts instead of inventing parts, and every cell is
/// read back the way the Slider and ComboBox batches established - a named condition and the value the tree
/// ends up holding, not a markup string.
/// </summary>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraNumberBoxTests
{
    private static readonly Color SurfaceSentinel = Color.FromRgb(0xFF, 0x00, 0xFF);
    private static readonly Color AccentSentinel = Color.FromRgb(0x00, 0xFF, 0x80);
    private static readonly Color BrandEmerald = Color.FromRgb(0x20, 0x72, 0x45);

    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraNumberBoxTests(AstraThemeRuntimeFixture fixture)
    {
        _fixture = fixture;
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            FluentThemeManager.ApplyAccent(null);
        });
    }

    /// <summary>The four popup alias rows, by upstream name and upstream target.</summary>
    [Theory]
    [InlineData("NumberBoxPopupIndicatorForeground", "TextFillColorSecondaryBrush")]
    [InlineData("NumberBoxPopupBackground", "FlyoutPresenterBackground")]
    [InlineData("NumberBoxPopupBorderBrush", "SurfaceStrokeColorFlyoutBrush")]
    [InlineData("NumberBoxPopupSpinButtonBackground", "SubtleFillColorTransparentBrush")]
    public void A_numberbox_alias_resolves_to_its_target(string alias, string target)
    {
        _fixture.Run(() => Assert.Same(Res(target), Res(alias)));
    }

    /// <summary>The eight TextControlButton* rows the spinners read, transcribed with the TextBox batch's file.</summary>
    [Theory]
    [InlineData("TextControlButtonForeground", "TextFillColorSecondaryBrush")]
    [InlineData("TextControlButtonForegroundPointerOver", "TextFillColorSecondaryBrush")]
    [InlineData("TextControlButtonForegroundPressed", "TextFillColorTertiaryBrush")]
    [InlineData("TextControlButtonBackgroundPointerOver", "SubtleFillColorSecondaryBrush")]
    [InlineData("TextControlButtonBackgroundPressed", "SubtleFillColorTertiaryBrush")]
    [InlineData("TextControlButtonBorderBrush", "ControlFillColorTransparentBrush")]
    [InlineData("TextControlButtonBorderBrushPointerOver", "ControlFillColorTransparentBrush")]
    [InlineData("TextControlButtonBorderBrushPressed", "ControlFillColorTransparentBrush")]
    public void A_textcontrol_button_row_resolves_to_its_target(string alias, string target)
    {
        _fixture.Run(() => Assert.Same(Res(target), Res(alias)));
    }

    [Fact]
    public void The_numberbox_metric_rows_are_upstreams()
    {
        _fixture.Run(() =>
        {
            Assert.Equal(new Thickness(0, 1, 1, 1), Res("NumberBoxSpinButtonBorderThickness"));
            Assert.Equal(new Thickness(10, 0, 0, 0), Res("NumberBoxIconMargin"));
            Assert.Equal(new Thickness(1), Res("NumberBoxPopupBorderThickness"));
            Assert.Equal(new Thickness(0), Res("NumberBoxPopupSpinButtonBorderThickness"));
            Assert.Equal(new Thickness(0, 0, 0, 8), Res("TextBoxTopHeaderMargin"));
        });
    }

    /// <summary>
    /// Five upstream keys get no row, and the four x:Double ones only work because their values are in the
    /// style - a {ThemeResource} on a key that does not exist fails silently on this reader, so the literal
    /// has to be proven present.
    /// </summary>
    [Fact]
    public void The_x_double_rows_stay_literals_because_this_reader_cannot_parse_them()
    {
        _fixture.Run(() =>
        {
            foreach (var key in new[]
                     {
                         "NumberBoxPopupHorizonalOffset", "NumberBoxPopupVerticalOffset",
                         "NumberBoxPopupShadowDepth", "NumberBoxMinWidth", "NumberBoxPopupIndicatorMargin",
                     })
            {
                Assert.Null(_fixture.Application.TryFindResource(key));
            }

            Assert.Equal(120d, Setter(FluentThemeManager.GetStyle("DefaultNumberBoxStyle"), "MinWidth").Value);
        });
    }

    /// <summary>
    /// Upstream keeps its RepeatButton* alias block inside the template precisely because those names belong
    /// to the repeat button. Hoisting them to this application-level dictionary clobbered that control's own
    /// rows on the first run of this batch and two Button pixel claims failed, so the block is not transcribed
    /// and this pins the reason.
    /// </summary>
    [Fact]
    public void The_repeatbutton_names_still_belong_to_the_repeat_button()
    {
        _fixture.Run(() =>
        {
            Assert.Same(Res("ControlFillColorDefaultBrush"), Res("RepeatButtonBackground"));
            Assert.Same(Res("TextFillColorPrimaryBrush"), Res("RepeatButtonForeground"));
            Assert.Same(Res("ControlStrokeColorDefaultBrush"), Res("RepeatButtonBorderBrush"));
        });
    }

    [Fact]
    public void The_numberbox_template_carries_one_cell_per_upstream_state()
    {
        List<Cell> cells = [];
        _fixture.Run(() => cells = Cells(NumberBoxTemplate()));
        Assert.Equal(
            new[]
            {
                "IsMouseOver=True",
                "IsKeyboardFocusWithin=True",
                "IsEnabled=False",
                // Not an upstream cell. Upstream never has to switch the header off: its presenter is
                // Collapsed and lazily loaded, and the control opens it only when a Header exists. This
                // runtime has no defer in a template and the header's 0,0,0,8 is a local margin, so a
                // null Header still cost 8 DIP. This row is the adaptation, and it is what
                // A_headerless_numberbox_does_not_pay_the_headers_gap holds honest.
                "Header=null",
                "SpinButtonPlacementMode=Inline",
                "SpinButtonPlacementMode=Hidden",
                "SpinButtonPlacementMode=Compact",
                "SpinButtonPlacementMode=Compact+IsKeyboardFocusWithin=True",
            },
            cells.Select(static cell => cell.Condition));

        // TextBox CommonStates, which is what upstream paints the NumberBox input surface with.
        AssertCell(cells, "IsMouseOver=True",
            ("OuterBorder", "Background", "TextControlBackgroundPointerOver"),
            ("OuterBorder", "BorderBrush", "TextControlBorderBrushPointerOver"),
            ("self", "Foreground", "TextControlForegroundPointerOver"));
        AssertCell(cells, "IsKeyboardFocusWithin=True",
            ("OuterBorder", "Background", "TextControlBackgroundFocused"),
            ("OuterBorder", "BorderThickness", "TextControlBorderThemeThicknessFocused"),
            ("BottomEdge", "Background", "TextControlBorderBrushFocused"),
            ("self", "Foreground", "TextControlForegroundFocused"));
        AssertCell(cells, "IsEnabled=False",
            ("OuterBorder", "Background", "TextControlBackgroundDisabled"),
            ("OuterBorder", "BorderBrush", "TextControlBorderBrushDisabled"),
            ("BottomEdge", "Background", "TextControlBorderBrushDisabled"),
            ("HeaderContentPresenter", "Foreground", "TextControlHeaderForegroundDisabled"),
            ("self", "Foreground", "TextControlForegroundDisabled"),
            ("self", "CaretBrush", "TextControlForegroundDisabled"));

        // SpinButtonStates: collapsed / visible / popup.
        AssertCell(cells, "SpinButtonPlacementMode=Inline", ("InlineSpinners", "Visibility", null));
        AssertCell(cells, "SpinButtonPlacementMode=Hidden",
            ("InlineSpinners", "Visibility", null), ("PopupIndicator", "Visibility", null));
        AssertCell(cells, "SpinButtonPlacementMode=Compact",
            ("InlineSpinners", "Visibility", null), ("PopupIndicator", "Visibility", null));
        AssertCell(cells, "SpinButtonPlacementMode=Compact+IsKeyboardFocusWithin=True",
            ("UpDownPopup", "IsOpen", null));
    }

    [Fact]
    public void The_spinner_template_carries_the_three_button_states()
    {
        List<Cell> cells = [];
        _fixture.Run(() => cells = Cells(SpinnerTemplate()));
        Assert.Equal(new[] { "IsMouseOver=True", "IsPressed=True", "IsEnabled=False" },
            cells.Select(static cell => cell.Condition));

        AssertCell(cells, "IsMouseOver=True",
            ("ButtonBorder", "Background", "TextControlButtonBackgroundPointerOver"),
            ("ButtonBorder", "BorderBrush", "TextControlButtonBorderBrushPointerOver"),
            ("self", "Foreground", "TextControlButtonForegroundPointerOver"));
        AssertCell(cells, "IsPressed=True",
            ("ButtonBorder", "Background", "TextControlButtonBackgroundPressed"),
            ("ButtonBorder", "BorderBrush", "TextControlButtonBorderBrushPressed"),
            ("self", "Foreground", "TextControlButtonForegroundPressed"));

        // Upstream has no disabled fill for these buttons: its alias forwards the base border row.
        AssertCell(cells, "IsEnabled=False", ("ButtonBorder", "BorderBrush", "TextControlButtonBorderBrush"));
    }

    [Fact]
    public void The_numberbox_style_reads_upstreams_surface_rows()
    {
        _fixture.Run(() =>
        {
            var style = FluentThemeManager.GetStyle("DefaultNumberBoxStyle");
            foreach (var (property, key) in new[]
                     {
                         ("Foreground", "TextControlForeground"), ("Background", "TextControlBackground"),
                         ("BorderBrush", "TextControlBorderBrush"), ("BorderThickness", "TextControlBorderThemeThickness"),
                         ("Padding", "TextControlThemePadding"), ("CornerRadius", "ControlCornerRadius"),
                         ("SelectionBrush", "TextControlSelectionHighlightColor"), ("CaretBrush", "TextControlForeground"),
                     })
            {
                Assert.Equal(key, KeyOf(Setter(style, property)));
            }

            var box = Mount(new NumberBox());
            // Brushes are asserted by instance - that is what the in-place retint model promises - while the
            // three metrics come back boxed, so they are asserted by value.
            foreach (var (property, key) in new[]
                     {
                         ("Foreground", "TextControlForeground"), ("Background", "TextControlBackground"),
                         ("BorderBrush", "TextControlBorderBrush"), ("SelectionBrush", "TextControlSelectionHighlightColor"),
                         ("CaretBrush", "TextControlForeground"),
                     })
            {
                Assert.Same(Res(key), Get(box, property));
            }

            Assert.Equal(Res("TextControlBorderThemeThickness"), Get(box, "BorderThickness"));
            Assert.Equal(Res("TextControlThemePadding"), Get(box, "Padding"));
            Assert.Equal(Res("ControlCornerRadius"), Get(box, "CornerRadius"));
        });
    }

    [Fact]
    public void The_spinner_style_carries_upstreams_spin_button_setters()
    {
        _fixture.Run(() =>
        {
            var style = FluentThemeManager.GetStyle("NumberBoxSpinButtonStyle");
            Assert.Equal("NumberBoxSpinButtonBorderThickness", KeyOf(Setter(style, "BorderThickness")));
            Assert.Equal("TextControlButtonForeground", KeyOf(Setter(style, "Foreground")));
            Assert.Equal("TextControlButtonBorderBrush", KeyOf(Setter(style, "BorderBrush")));
            Assert.Equal(false, Setter(style, "IsTabStop").Value);
            Assert.Equal(32d, Setter(style, "MinWidth").Value);
            Assert.Equal(12d, Setter(style, "FontSize").Value);

            var popup = FluentThemeManager.GetStyle("NumberBoxPopupSpinButtonStyle");
            Assert.Equal("NumberBoxPopupSpinButtonBorderThickness", KeyOf(Setter(popup, "BorderThickness")));
            Assert.Equal("NumberBoxPopupSpinButtonBackground", KeyOf(Setter(popup, "Background")));
            Assert.Equal(36d, Setter(popup, "Width").Value);
        });
    }

    /// <summary>
    /// What the framework keeps after we replace its template: it grafts its own text host into the panel named
    /// PART_ContentHost, and it writes local BorderThickness and CornerRadius onto the two named spin buttons.
    /// A local value outranks our markup, so the second half of this is the measured winner, not a wish.
    /// </summary>
    [Fact]
    public void The_framework_keeps_what_it_took_ownership_of()
    {
        _fixture.Run(() =>
        {
            var box = Mount(new NumberBox());
            var host = Part(box, "PART_ContentHost");
            Assert.NotNull(PixelHarness.Descendant<FrameworkElement>(host)
                ?? throw new InvalidOperationException("the framework grafted no content host into PART_ContentHost"));

            var up = (RepeatButton)Part(box, "PART_UpSpinButton");
            var down = (RepeatButton)Part(box, "PART_DownSpinButton");
            Assert.Equal(new Thickness(1, 0, 0, 0), up.BorderThickness);
            Assert.Equal(new Thickness(1, 0, 0, 0), down.BorderThickness);
            Assert.Equal(new CornerRadius(0, 4, 0, 0), up.CornerRadius);
            Assert.Equal(new CornerRadius(0, 0, 4, 0), down.CornerRadius);

            // The control itself carries no framework local values at rest, unlike a mounted TextBox.
            foreach (var property in new[] { "Background", "Foreground", "BorderBrush", "Padding" })
            {
                Assert.Equal(DependencyProperty.UnsetValue,
                    box.ReadLocalValue(DependencyProperty.FromName(typeof(NumberBox), property)!));
            }
        });
    }

    [Fact]
    public void The_placement_cells_move_the_spinners_and_the_indicator()
    {
        _fixture.Run(() =>
        {
            var box = Mount(new NumberBox());
            Assert.Equal(Visibility.Visible, ((UIElement)Part(box, "InlineSpinners")).Visibility);
            Assert.Equal(Visibility.Collapsed, ((UIElement)Part(box, "PopupIndicator")).Visibility);

            box.SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Hidden;
            PixelHarness.Settle();
            Assert.Equal(Visibility.Collapsed, ((UIElement)Part(box, "InlineSpinners")).Visibility);
            Assert.Equal(Visibility.Collapsed, ((UIElement)Part(box, "PopupIndicator")).Visibility);

            box.SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact;
            PixelHarness.Settle();
            Assert.Equal(Visibility.Collapsed, ((UIElement)Part(box, "InlineSpinners")).Visibility);
            Assert.Equal(Visibility.Visible, ((UIElement)Part(box, "PopupIndicator")).Visibility);

            box.SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline;
            PixelHarness.Settle();
            Assert.Equal(Visibility.Visible, ((UIElement)Part(box, "InlineSpinners")).Visibility);
        });
    }

    [Fact]
    public void A_focused_compact_numberbox_opens_its_spinner_popup()
    {
        _fixture.Run(() =>
        {
            var box = Mount(new NumberBox { SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact });
            var popup = (Popup)Part(box, "UpDownPopup");
            Assert.False(popup.IsOpen);

            // Both sides read inside the call that changes the state (spike/PopupLadderProbe S12): focus has the
            // spinner popup up at rung 0, and switching the placement to Inline takes it down again in the same
            // write. The pumps this used to sit behind only opened a window in which something else could
            // deactivate the host and close a light-dismiss popup for reasons that are not this claim.
            Assert.True(box.Focus());
            Assert.True(popup.IsOpen);

            box.SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline;
            Assert.False(popup.IsOpen);
        });
    }

    /// <summary>The value engine lives in the control, so stepping survives the retemplating.</summary>
    [Fact]
    public void Stepping_and_formatting_survive_the_new_template()
    {
        _fixture.Run(() =>
        {
            var box = Mount(new NumberBox { Value = 42 });
            box.StepUp();
            Assert.Equal(43d, box.Value);
            Assert.Equal("43", box.Text);
            box.StepDown();
            box.StepDown();
            Assert.Equal(41d, box.Value);
            Assert.Equal("41", box.Text);

            box.Value = 7;
            Assert.Equal("7", box.Text);
        });
    }

    [Fact]
    public void A_disabled_numberbox_reaches_the_disabled_rows_and_a_header_shows_its_text()
    {
        _fixture.Run(() =>
        {
            var box = Mount(new NumberBox { Header = "Count", Value = 3 });
            var header = Part(box, "HeaderContentPresenter");
            Assert.Equal("Count", Get(header, "Content"));
            var generated = PixelHarness.Descendant<TextBlock>(header);
            Assert.NotNull(generated);

            var headerForeground = CellProperty(NumberBoxTemplate(), "IsEnabled=False", "HeaderContentPresenter", "Foreground");
            var disabled = FluentThemeManager.GetBrush("TextFillColorDisabledBrush");
            Assert.NotSame(disabled, header.GetValue(headerForeground));

            box.IsEnabled = false;
            PixelHarness.Settle();
            var surface = (Border)Part(box, "OuterBorder");
            Assert.Same(Res("TextControlBackgroundDisabled"), surface.Background);

            // Measured limit: the framework writes a local Foreground onto the generated text element, so the
            // disabled header row cannot reach the glyphs. Pinned rather than quietly claimed.
            Assert.NotSame(disabled, generated!.Foreground);
        });
    }

    [Fact]
    public void A_headerless_numberbox_does_not_pay_the_headers_gap()
    {
        // Upstream's header presenter is Collapsed and lazily loaded, so an absent Header costs nothing.
        // Here the 0,0,0,8 is a local margin on an element that still lays out, which measured the box 39
        // tall against its own 31-tall surface. The null-Header trigger has to work in both directions,
        // so this reads the gap out when there is no header and back in the moment there is one.
        _fixture.Run(() =>
        {
            var box = Mount(new NumberBox { Value = 3 });
            // The header carrier is a ContentControl since the foreground sweep: the row it has to carry is a
            // Foreground, and this runtime's ContentPresenter has no such member to write.
            var presenter = (FrameworkElement)Part(box, "HeaderContentPresenter")!;
            var surface = (Border)Part(box, "OuterBorder")!;
            Assert.Multiple(
                () => Assert.Equal(Visibility.Collapsed, presenter.Visibility),
                () => Assert.Equal(32d, box.ActualHeight, 0.01),
                () => Assert.Equal(surface.ActualHeight, box.ActualHeight, 0.01));

            box.Header = "Count";
            PixelHarness.Settle();
            Assert.Multiple(
                () => Assert.Equal(Visibility.Visible, presenter.Visibility),
                () => Assert.Equal(presenter.ActualHeight + 8, box.ActualHeight - surface.ActualHeight, 0.01));
        });
    }

    [Fact]
    public void A_resting_numberbox_paints_its_rows_and_nothing_invented()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.OverrideBrush("ControlFillColorDefaultBrush", SurfaceSentinel);
            FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", AccentSentinel);
            try
            {
                var sample = PixelHarness.Render(new NumberBox { Value = 12 }, 220, 32);
                Assert.True(sample.Stable, $"capture never settled: {sample.Top(6)}");
                Assert.True(sample.Count(SurfaceSentinel) > 3_000,
                    $"surface row did not reach the pixels; top={sample.Top(8)}");
                Assert.Equal(0, sample.Count(BrandEmerald));
                Assert.Equal(0, sample.Count(AccentSentinel));
            }
            finally
            {
                FluentThemeManager.OverrideBrush("ControlFillColorDefaultBrush", null);
                FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", null);
            }
        });
    }

    [Fact]
    public void The_numberbox_surface_changes_between_light_and_dark()
    {
        _fixture.Run(() =>
        {
            var box = new Func<NumberBox>(() => new NumberBox { Value = 12 });
            var (lightInk, _) = PixelHarness.AssertSurfaceLands(
                box(), PixelHarness.Self, PixelHarness.LightPage, 220, 32, 2_000, "the number box's own surface");
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
            try
            {
                var (darkInk, _) = PixelHarness.AssertSurfaceLands(
                    box(), PixelHarness.Self, PixelHarness.DarkPage, 220, 32, 2_000, "the number box's own surface");
                Assert.NotEqual(lightInk, darkInk);
            }
            finally
            {
                FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            }
        });
    }

    /// <summary>
    /// A cell that writes a literal colour is invisible to the alias layer, so an app overriding a row sees
    /// nothing. Every state cell here has to carry a resource key.
    /// </summary>
    [Fact]
    public void Every_numberbox_cell_names_a_row_and_never_a_palette_brush()
    {
        List<ControlTemplate> templates = [];
        _fixture.Run(() => templates = [NumberBoxTemplate(), SpinnerTemplate()]);
        foreach (var template in templates)
        {
            foreach (var cell in Cells(template))
            {
                foreach (var setter in cell.Setters)
                {
                    if (setter.Value is not null && (setter.Property?.Name ?? setter.PropertyName) is "Visibility" or "IsOpen")
                    {
                        continue;
                    }

                    Assert.NotNull(KeyOf(setter));
                }
            }
        }
    }

    private object? Res(string key) => _fixture.Application.TryFindResource(key);

    private NumberBox Mount(NumberBox box)
    {
        box.Width = 220;
        box.Height = 32;
        PixelHarness.Build(box, 220, 32);
        return box;
    }

    private static ControlTemplate NumberBoxTemplate() => Template("DefaultNumberBoxStyle");

    private static ControlTemplate SpinnerTemplate() => Template("NumberBoxSpinButtonStyle");

    private static ControlTemplate Template(string key) =>
        (FluentThemeManager.GetStyle(key).Setters.Cast<object>().OfType<Setter>()
            .First(static setter => (setter.Property?.Name ?? setter.PropertyName) == "Template").Value as ControlTemplate)!;

    private static List<Cell> Cells(ControlTemplate template) =>
        template.Triggers.Cast<object>().Select(static trigger => trigger switch
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

    private static string Form(object? value) => value?.ToString() ?? "null";

    private static FrameworkElement Part(DependencyObject root, string name) =>
        PixelHarness.Named(root, name) ?? throw new InvalidOperationException($"No part named {name} in the built tree.");

    private static object Get(FrameworkElement part, string property)
    {
        var clr = part.GetType().GetProperty(property, BindingFlags.Public | BindingFlags.Instance);
        if (clr is not null) return clr.GetValue(part) ?? throw new InvalidOperationException($"{part.GetType().Name}.{property} is null.");
        var dp = DependencyProperty.FromName(part.GetType(), property)
            ?? throw new InvalidOperationException($"{part.GetType().Name} has no {property}.");
        return part.GetValue(dp) ?? throw new InvalidOperationException($"{part.GetType().Name}.{property} is null.");
    }

    /// <summary>
    /// The dependency property a cell writes, taken off the hydrated setter itself. ContentPresenter has no
    /// Foreground member to read by name, so this is the only way to prove the part carries the very property
    /// the cell names.
    /// </summary>
    private static DependencyProperty CellProperty(ControlTemplate template, string condition, string part, string property)
    {
        var cell = Cells(template).Find(candidate => candidate.Condition == condition)
            ?? throw new InvalidOperationException($"No cell for {condition}.");
        var setter = cell.Setters.FirstOrDefault(candidate =>
                (candidate.TargetName ?? "self") == part && (candidate.Property?.Name ?? candidate.PropertyName) == property)
            ?? throw new InvalidOperationException($"{condition} carries no {part}.{property} setter.");
        return setter.Property!;
    }

    private static Setter Setter(Style style, string property) => style.Setters.Cast<object>().OfType<Setter>()
        .First(setter => (setter.Property?.Name ?? setter.PropertyName) == property);

    private static string? KeyOf(Setter setter) =>
        setter.Value?.GetType().GetProperty("ResourceKey")?.GetValue(setter.Value) as string;

    private static void AssertCell(List<Cell> cells, string condition, params (string Part, string Property, string? Key)[] wanted)
    {
        var cell = cells.Find(candidate => candidate.Condition == condition)
            ?? throw new InvalidOperationException($"No cell for {condition}; the template carries " +
                string.Join(" | ", cells.Select(static candidate => candidate.Condition)));

        foreach (var (part, name, key) in wanted)
        {
            var setter = cell.Setters.FirstOrDefault(candidate =>
                    (candidate.TargetName ?? "self") == part && (candidate.Property?.Name ?? candidate.PropertyName) == name)
                ?? throw new InvalidOperationException($"{condition}: no {part}.{name} setter; the cell carries " +
                    string.Join(", ", cell.Setters.Select(static candidate =>
                        $"{candidate.TargetName ?? "self"}.{candidate.Property?.Name ?? candidate.PropertyName}")));
            if (key is not null)
            {
                Assert.Equal(key, KeyOf(setter));
            }
        }
    }

    private sealed record Cell(string Condition, Setter[] Setters);
}
