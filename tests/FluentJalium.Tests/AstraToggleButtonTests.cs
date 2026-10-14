using FluentJalium.Themes;
using FluentJalium.Tests.Pixel;
using Jalium.UI;
using Jalium.UI.Automation.Peers;
using Jalium.UI.Automation.Provider;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Markup;
using Jalium.UI.Media;

namespace FluentJalium.Tests;

/// <summary>
/// ToggleButton, the last control of the selection batch and the only button-family member with a third
/// state to carry. Upstream draws it as one <c>ContentPresenter</c> root driven by a single
/// <c>VisualStateGroup</c> of twelve states, each writing Background + BorderBrush + Foreground, so the
/// mapping here is eleven cells over the same three properties (docs/astra/audits/togglebutton.md).
/// <para>
/// The batch exists because two of its claims were unverifiable by pixels and one was unverifiable by
/// markup. Upstream maps every <c>*Indeterminate*</c> row to the same palette brush as its rest row, so a
/// tri-state capture cannot tell "the null cell fired" from "no cell fired" - <see cref="A_null_checked_value_fires_the_indeterminate_cell"/>
/// settles that with a probe style whose indeterminate fill is a brush nothing else on the control reads.
/// And a setter that carries the right key proves nothing about the tree, so every state here is also read
/// back off a mounted control.
/// </para>
/// </summary>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraToggleButtonTests
{
    private static readonly Color SurfaceSentinel = Color.FromRgb(0xFF, 0x00, 0xFF);
    private static readonly Color BrandEmerald = Color.FromRgb(0x20, 0x72, 0x45);

    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraToggleButtonTests(AstraThemeRuntimeFixture fixture)
    {
        _fixture = fixture;
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            FluentThemeManager.ApplyAccent(null);
        });
    }

    /// <summary>
    /// The three hover foreground rows are the reason this batch started: upstream's PointerOver,
    /// CheckedPointerOver and IndeterminatePointerOver states each write Foreground, our cells wrote two of
    /// the three properties, and the key-consumption gate turned the difference into a list.
    /// </summary>
    [Theory]
    [InlineData("ToggleButtonForegroundPointerOver", "TextFillColorPrimaryBrush")]
    [InlineData("ToggleButtonForegroundCheckedPointerOver", "TextOnAccentFillColorPrimaryBrush")]
    [InlineData("ToggleButtonForegroundIndeterminatePointerOver", "TextFillColorPrimaryBrush")]
    [InlineData("ToggleButtonBackgroundChecked", "AccentFillColorDefaultBrush")]
    [InlineData("ToggleButtonBackgroundCheckedDisabled", "AccentFillColorDisabledBrush")]
    [InlineData("ToggleButtonBackgroundIndeterminate", "ControlFillColorDefaultBrush")]
    [InlineData("ToggleButtonBorderBrushCheckedPressed", "ControlFillColorTransparentBrush")]
    public void A_toggle_alias_resolves_to_the_object_upstream_names(string alias, string target)
    {
        _fixture.Run(() => Assert.Same(Res(target), Res(alias)));
    }

    [Fact]
    public void The_toggle_reads_its_own_border_thickness_row()
    {
        // Upstream's toggle style borrows nothing here: BorderThickness comes from
        // ToggleButtonBorderThemeThickness, while the shared layout style hands the button family
        // ButtonBorderThemeThickness. Both are 1, so this is about which name an app can override - and the
        // value read off the built control is what keeps the borrowed row from silently winning.
        _fixture.Run(() =>
        {
            Assert.Equal(new Thickness(1), Res("ToggleButtonBorderThemeThickness"));
            Assert.Equal("ToggleButtonBorderThemeThickness",
                SetterKey(FluentThemeManager.GetStyle("DefaultToggleButtonStyle"), "BorderThickness"));

            var toggle = Mount(new ToggleButton { Content = "toggle" });
            Assert.Equal(new Thickness(1), toggle.BorderThickness);
        });
    }

    /// <summary>
    /// DoD step 4 as a table: one cell per upstream state, and the cell carries the row upstream names for
    /// each of the three properties it writes. A cell missing a row is a key that no app can override,
    /// which is what the consumption gate now refuses; this test says which row has to sit in which cell.
    /// </summary>
    [Fact]
    public void The_toggle_style_carries_one_cell_per_upstream_state()
    {
        List<Cell> cells = [];
        _fixture.Run(() => cells = Cells(FluentThemeManager.GetStyle("DefaultToggleButtonStyle")));

        Assert.Equal(
            new[]
            {
                "IsMouseOver=True",
                "IsPressed=True",
                "IsChecked=True",
                "IsChecked=True+IsMouseOver=True",
                "IsChecked=True+IsPressed=True",
                "IsEnabled=False",
                "IsChecked=True+IsEnabled=False",
                "IsChecked=null",
                "IsChecked=null+IsMouseOver=True",
                "IsChecked=null+IsPressed=True",
                "IsChecked=null+IsEnabled=False",
            },
            cells.Select(static cell => cell.Condition));

        AssertCell(cells, "IsMouseOver=True", ("Background", "ToggleButtonBackgroundPointerOver"),
            ("Foreground", "ToggleButtonForegroundPointerOver"), ("BorderBrush", "ToggleButtonBorderBrushPointerOver"));
        AssertCell(cells, "IsPressed=True", ("Background", "ToggleButtonBackgroundPressed"),
            ("Foreground", "ToggleButtonForegroundPressed"), ("BorderBrush", "ToggleButtonBorderBrushPressed"));
        AssertCell(cells, "IsChecked=True", ("Background", "ToggleButtonBackgroundChecked"),
            ("Foreground", "ToggleButtonForegroundChecked"), ("BorderBrush", "ToggleButtonBorderBrushChecked"));
        AssertCell(cells, "IsChecked=True+IsMouseOver=True", ("Background", "ToggleButtonBackgroundCheckedPointerOver"),
            ("Foreground", "ToggleButtonForegroundCheckedPointerOver"), ("BorderBrush", "ToggleButtonBorderBrushCheckedPointerOver"));
        AssertCell(cells, "IsChecked=True+IsPressed=True", ("Background", "ToggleButtonBackgroundCheckedPressed"),
            ("Foreground", "ToggleButtonForegroundCheckedPressed"), ("BorderBrush", "ToggleButtonBorderBrushCheckedPressed"));
        AssertCell(cells, "IsEnabled=False", ("Background", "ToggleButtonBackgroundDisabled"),
            ("Foreground", "ToggleButtonForegroundDisabled"), ("BorderBrush", "ToggleButtonBorderBrushDisabled"));
        AssertCell(cells, "IsChecked=True+IsEnabled=False", ("Background", "ToggleButtonBackgroundCheckedDisabled"),
            ("Foreground", "ToggleButtonForegroundCheckedDisabled"), ("BorderBrush", "ToggleButtonBorderBrushCheckedDisabled"));
        AssertCell(cells, "IsChecked=null", ("Background", "ToggleButtonBackgroundIndeterminate"),
            ("Foreground", "ToggleButtonForegroundIndeterminate"), ("BorderBrush", "ToggleButtonBorderBrushIndeterminate"));
        AssertCell(cells, "IsChecked=null+IsMouseOver=True", ("Background", "ToggleButtonBackgroundIndeterminatePointerOver"),
            ("Foreground", "ToggleButtonForegroundIndeterminatePointerOver"), ("BorderBrush", "ToggleButtonBorderBrushIndeterminatePointerOver"));
        AssertCell(cells, "IsChecked=null+IsPressed=True", ("Background", "ToggleButtonBackgroundIndeterminatePressed"),
            ("Foreground", "ToggleButtonForegroundIndeterminatePressed"), ("BorderBrush", "ToggleButtonBorderBrushIndeterminatePressed"));
        AssertCell(cells, "IsChecked=null+IsEnabled=False", ("Background", "ToggleButtonBackgroundIndeterminateDisabled"),
            ("Foreground", "ToggleButtonForegroundIndeterminateDisabled"), ("BorderBrush", "ToggleButtonBorderBrushIndeterminateDisabled"));
    }

    /// <summary>
    /// No cell may reach for a palette brush directly: an alias row is the layer an app overrides, and a
    /// template that reads the token itself silently opts out of every toggle-specific name upstream has.
    /// </summary>
    [Fact]
    public void Every_cell_names_a_toggle_row_and_never_a_palette_brush()
    {
        List<string> offenders = [];
        _fixture.Run(() =>
        {
            foreach (var cell in Cells(FluentThemeManager.GetStyle("DefaultToggleButtonStyle")))
            {
                foreach (var setter in cell.Setters)
                {
                    var key = KeyOf(setter);
                    if (key is null || !key.StartsWith("ToggleButton", StringComparison.Ordinal))
                        offenders.Add($"{cell.Condition}: {setter.Property?.Name}={key ?? setter.Value?.GetType().Name}");
                }
            }
        });

        Assert.False(offenders.Count > 0, "Non-toggle values in state cells:" + Environment.NewLine + string.Join(Environment.NewLine, offenders));
    }

    /// <summary>
    /// The measured resting value, written down because the whole indeterminate mapping depends on it: if
    /// the framework ever starts a toggle in null, every two-state toggle on screen is an indeterminate one
    /// and the difference is invisible in colour (upstream maps both to the same brush).
    /// </summary>
    [Fact]
    public void A_toggle_button_starts_unchecked_not_indeterminate()
    {
        _fixture.Run(() =>
        {
            var toggle = new ToggleButton { Content = "toggle" };
            Assert.False(toggle.IsThreeState);
            Assert.False(toggle.IsChecked!.Value);
        });
    }

    [Fact]
    public void A_checked_toggle_wears_the_checked_alias_instances()
    {
        // Read back off the control the style pipeline built, not off the setter. A local value or a
        // framework default would satisfy a colour comparison and still not be the object the kernel
        // re-tints, so identity is the claim (docs/astra/audits/togglebutton.md S3).
        _fixture.Run(() =>
        {
            var toggle = Mount(new ToggleButton { Content = "toggle" });
            PixelHarness.Settle(20);
            Assert.Multiple(
                () => Assert.Same(Res("ToggleButtonBackground"), toggle.Background),
                () => Assert.Same(Res("ToggleButtonForeground"), toggle.Foreground),
                () => Assert.Same(Res("ToggleButtonBorderBrush"), toggle.BorderBrush));

            toggle.IsChecked = true;
            PixelHarness.Settle(30);
            Assert.Multiple(
                () => Assert.Same(Res("ToggleButtonBackgroundChecked"), toggle.Background),
                () => Assert.Same(Res("ToggleButtonForegroundChecked"), toggle.Foreground),
                () => Assert.Same(Res("ToggleButtonBorderBrushChecked"), toggle.BorderBrush));

            toggle.IsChecked = false;
            PixelHarness.Settle(30);
            Assert.Same(Res("ToggleButtonBackground"), toggle.Background);
        });
    }

    [Fact]
    public void The_checked_disabled_cell_lands_last_so_it_wins_the_checked_surface()
    {
        // Upstream keeps CheckedDisabled in the same group as Checked; here the two are separate cells that
        // both match at once, so the outcome depends on the reader applying the later one. Measured: it
        // does, which is why the markup order above is load-bearing rather than cosmetic.
        _fixture.Run(() =>
        {
            var toggle = Mount(new ToggleButton { Content = "toggle", IsChecked = true, IsEnabled = false });
            PixelHarness.Settle(30);
            Assert.Multiple(
                () => Assert.Same(Res("ToggleButtonBackgroundCheckedDisabled"), toggle.Background),
                () => Assert.Same(Res("ToggleButtonForegroundCheckedDisabled"), toggle.Foreground));
        });
    }

    /// <summary>
    /// The claim a capture cannot make. Upstream maps <c>ToggleButtonBackgroundIndeterminate</c> and
    /// <c>ToggleButtonBackground</c> to the same palette object, so a tri-state toggle shows the same
    /// surface whether or not the null cell ever matched - and a trigger condition can be structurally
    /// perfect and still never fire (adaptation/00 S0-g). This probe gives the null condition a fill nothing
    /// else on the control reads, so the only way the surface turns accent is the cell matching, and back to
    /// the rest fill when it stops matching.
    /// </summary>
    [Fact]
    public void A_null_checked_value_fires_the_indeterminate_cell()
    {
        _fixture.Run(() =>
        {
            var dictionary = (ResourceDictionary)XamlReader.Parse(ProbeMarkup)!;
            var style = (Style)dictionary["ProbeToggleNullStyle"]!;
            var toggle = new ToggleButton { Content = "mixed", Style = style, IsThreeState = true, IsChecked = null };
            PixelHarness.Build(toggle, 160, 32);
            PixelHarness.Settle(30);

            Assert.True(toggle.IsChecked is null, "the probe never entered the indeterminate value.");
            Assert.Same(Res("AccentFillColorDefaultBrush"), toggle.Background);

            toggle.IsChecked = false;
            PixelHarness.Settle(30);
            Assert.Same(Res("ControlFillColorDefaultBrush"), toggle.Background);
        });
    }

    [Fact]
    public void The_indeterminate_cell_takes_the_surface_too_when_the_accent_moves()
    {
        // Same probe, asserted where it lands in the rasteriser, so the null-condition result is not only a
        // property value the compositor ignores.
        _fixture.Run(() =>
        {
            var dictionary = (ResourceDictionary)XamlReader.Parse(ProbeMarkup)!;
            var toggle = new ToggleButton
            {
                Content = "mixed",
                Style = (Style)dictionary["ProbeToggleNullStyle"]!,
                IsThreeState = true,
                IsChecked = null,
            };

            FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", SurfaceSentinel);
            try
            {
                var mixed = PixelHarness.Render(toggle, 200, 44);
                Assert.True(mixed.Stable, $"capture never settled: {mixed.Top(6)}");
                Assert.True(mixed.Count(SurfaceSentinel) > 6_000,
                    $"indeterminate cell did not reach the surface; top={mixed.Top(6)}");

                toggle.IsChecked = false;
                var rest = PixelHarness.Render(toggle, 200, 44);
                Assert.Equal(0, rest.Count(SurfaceSentinel));
            }
            finally
            {
                FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", null);
            }
        });
    }

    /// <summary>
    /// The cycle the framework owns, driven through its automation pattern rather than a synthetic pointer:
    /// <c>IToggleProvider.Toggle</c> is the same entry point a screen reader uses and lands on
    /// <c>OnClick</c> → <c>OnToggle</c>, so a template that broke the click path would fail here too.
    /// </summary>
    [Fact]
    public void A_toggle_cycle_walks_unchecked_checked_unchecked()
    {
        _fixture.Run(() =>
        {
            var toggle = Mount(new ToggleButton { Content = "toggle" });
            var raised = new List<string>();
            toggle.Checked += (_, _) => raised.Add("checked");
            toggle.Unchecked += (_, _) => raised.Add("unchecked");
            toggle.Indeterminate += (_, _) => raised.Add("indeterminate");

            Cycle(toggle);
            Assert.True(toggle.IsChecked!.Value, "the first cycle did not check the toggle.");
            Cycle(toggle);
            Assert.False(toggle.IsChecked!.Value, "the second cycle did not uncheck the toggle.");
            Assert.Equal(new[] { "checked", "unchecked" }, raised);
        });
    }

    [Fact]
    public void A_three_state_cycle_passes_through_indeterminate_and_back()
    {
        _fixture.Run(() =>
        {
            var toggle = Mount(new ToggleButton { Content = "mixed", IsThreeState = true });
            var raised = new List<string>();
            toggle.Checked += (_, _) => raised.Add("checked");
            toggle.Unchecked += (_, _) => raised.Add("unchecked");
            toggle.Indeterminate += (_, _) => raised.Add("indeterminate");

            Cycle(toggle);
            Cycle(toggle);
            Cycle(toggle);

            Assert.Equal(3, raised.Count);
            Assert.Equal("indeterminate", raised[1]);
            Assert.False(toggle.IsChecked!.Value, "the cycle did not return to unchecked.");
        });
    }

    /// <summary>
    /// The framework refuses a disabled toggle by throwing rather than by ignoring the call, which is the
    /// same contract WinUI's pattern has (ElementNotEnabledException). Pinned as a reading because an app
    /// that drives toggles from automation has to catch it, and because a silent no-op here would have been
    /// an equally acceptable behaviour to lose.
    /// </summary>
    [Fact]
    public void A_disabled_toggle_refuses_the_cycle_and_keeps_its_state()
    {
        _fixture.Run(() =>
        {
            var toggle = Mount(new ToggleButton { Content = "toggle", IsEnabled = false });
            var failure = Assert.Throws<InvalidOperationException>(() => Cycle(toggle));

            Assert.Contains("Cannot toggle a disabled control", failure.Message, StringComparison.Ordinal);
            Assert.False(toggle.IsChecked!.Value);
        });
    }

    /// <summary>
    /// Where a style trigger sits against a local value, measured rather than assumed: WinUI drives these
    /// states from VisualState storyboards, which outrank a local fill, and this runtime's precedence is its
    /// own. The reading is pinned so a later kernel change that flips it is a decision, not a surprise.
    /// </summary>
    [Fact]
    public void A_local_fill_keeps_the_surface_over_the_checked_cell()
    {
        var local = new SolidColorBrush(Color.FromRgb(0xFF, 0xA5, 0x00));
        _fixture.Run(() =>
        {
            var toggle = Mount(new ToggleButton { Content = "toggle", Background = local, IsChecked = true });
            PixelHarness.Settle(30);

            Assert.Same(local, toggle.Background);
            Assert.NotSame(Res("ToggleButtonBackgroundChecked"), toggle.Background);
        });
    }

    [Fact]
    public void The_shared_focus_ring_reaches_a_toggle_too()
    {
        // The layout style is shared with Button, so the ring is not this control's markup - but the implicit
        // toggle style is the one that inherits it, and a BasedOn that lost the property would leave a
        // keyboard user with no ring on exactly the control the Gallery tabs to. It reads the FocusVisualStyle
        // now rather than a part, because the ring left the template with the rest of them.
        _fixture.Run(() =>
        {
            var toggle = Mount(new ToggleButton { Content = "toggle" });
            Assert.Null(PixelHarness.Named(toggle, "FocusOutline"));
            Assert.Same(FluentThemeManager.GetStyle("FocusVisualRingStyle"), toggle.FocusVisualStyle);
        });
    }

    [Fact]
    public void The_resting_toggle_follows_the_theme_and_shows_no_brand_emerald()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            var (lightInk, light) = PixelHarness.AssertSurfaceLands(
                new ToggleButton { Content = "toggle", Width = 200, Height = 44 }, PixelHarness.Self,
                PixelHarness.LightPage, 200, 44, 2_000, "the toggle button's resting surface");

            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
            var (darkInk, dark) = PixelHarness.AssertSurfaceLands(
                new ToggleButton { Content = "toggle", Width = 200, Height = 44 }, PixelHarness.Self,
                PixelHarness.DarkPage, 200, 44, 2_000, "the toggle button's resting surface");

            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);

            Assert.NotEqual(lightInk, darkInk);
            Assert.Equal(0, light.Count(BrandEmerald));
            Assert.Equal(0, dark.Count(BrandEmerald));
        });
    }

    /// <summary>
    /// The rest-fill pixel claim for this control, which the Button batch never took (it captured a checked
    /// toggle and a resting repeat button). It deliberately cannot tell the toggle's own alias layer from
    /// the button's: upstream maps <c>ToggleButtonBackground</c> and <c>ButtonBackground</c> to the same
    /// palette token, so the two surfaces move together by design. Which rows the cells name is the table
    /// test's job; this one only says the token reaches this control's pixels.
    /// </summary>
    [Fact]
    public void The_resting_toggle_paints_the_control_fill_token()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.OverrideBrush("ControlFillColorDefaultBrush", SurfaceSentinel);
            try
            {
                var rest = PixelHarness.Render(new ToggleButton { Content = "toggle", Width = 200, Height = 44 }, 200, 44);
                Assert.True(rest.Stable, $"capture never settled: {rest.Top(6)}");
                Assert.True(rest.Count(SurfaceSentinel) > 6_000, $"rest fill did not reach the toggle; top={rest.Top(6)}");
            }
            finally
            {
                FluentThemeManager.OverrideBrush("ControlFillColorDefaultBrush", null);
            }
        });
    }

    /// <summary>
    /// A probe style rather than the shipped one, because the shipped indeterminate rows point at the same
    /// brushes as their rest rows by upstream's own design. Only the null condition is copied from the real
    /// style; the fills are chosen so a match is visible.
    /// </summary>
    private const string ProbeMarkup = """
        <ResourceDictionary xmlns='http://schemas.jalium.ui/2024'
                            xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
          <Style x:Key='ProbeToggleNullStyle' TargetType='ToggleButton'>
            <Setter Property='Background' Value='{ThemeResource ControlFillColorDefaultBrush}' />
            <Style.Triggers>
              <Trigger Property='IsChecked' Value='{x:Null}'>
                <Setter Property='Background' Value='{ThemeResource AccentFillColorDefaultBrush}' />
              </Trigger>
            </Style.Triggers>
          </Style>
        </ResourceDictionary>
        """;

    private object? Res(string key) => _fixture.Application.TryFindResource(key);

    private ToggleButton Mount(ToggleButton toggle)
    {
        PixelHarness.Build(toggle, 160, 32);
        return toggle;
    }

    /// <summary>The framework's own invoke path, reached through the pattern it exposes for it.</summary>
    private static void Cycle(ToggleButton toggle) => ((IToggleProvider)new ToggleButtonAutomationPeer(toggle)).Toggle();

    private static FrameworkElement Part(DependencyObject root, string name) =>
        PixelHarness.Named(root, name) ?? throw new InvalidOperationException($"No part named {name} in the built tree.");

    private static List<Cell> Cells(Style style) =>
        style.Triggers.Cast<object>().Select(static trigger => trigger switch
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

    private static string? KeyOf(Setter setter) =>
        setter.Value?.GetType().GetProperty("ResourceKey")?.GetValue(setter.Value) as string;

    private static string? SetterKey(Style style, string property) =>
        style.Setters.Cast<object>().OfType<Setter>()
            .FirstOrDefault(setter => (setter.Property?.Name ?? setter.PropertyName) == property) is { } found
            ? KeyOf(found)
            : null;

    private static void AssertCell(List<Cell> cells, string condition, params (string Property, string Key)[] wanted)
    {
        var cell = cells.Find(candidate => candidate.Condition == condition)
            ?? throw new InvalidOperationException($"No cell for {condition}; the style carries " +
                string.Join(" | ", cells.Select(static candidate => candidate.Condition)));

        foreach (var (name, key) in wanted)
        {
            var setter = cell.Setters.FirstOrDefault(candidate =>
                    (candidate.Property?.Name ?? candidate.PropertyName) == name)
                ?? throw new InvalidOperationException($"{condition}: no {name} setter; the cell carries " +
                    string.Join(", ", cell.Setters.Select(static candidate =>
                        candidate.Property?.Name ?? candidate.PropertyName ?? "?")));

            Assert.Equal(key, KeyOf(setter));
        }
    }

    private sealed record Cell(string Condition, Setter[] Setters);
}
