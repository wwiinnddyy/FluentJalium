using FluentJalium.Themes;
using FluentJalium.Tests.Pixel;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Markup;
using Jalium.UI.Media;

namespace FluentJalium.Tests;

/// <summary>
/// The selection controls' three-state path, which is the one question the Button batch left open:
/// upstream writes indeterminate as <c>IsChecked="{x:Null}"></c>, and a runtime that dropped that
/// silently would leave 12 declared keys with no consumer and a Gallery checkbox that quietly stops
/// being tri-state. Measured readings, not inference - see docs/astra/adaptation/11.
/// </summary>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraSelectionTests
{
    private static readonly Color Sentinel = Color.FromRgb(0xFF, 0x00, 0xFF);

    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraSelectionTests(AstraThemeRuntimeFixture fixture)
    {
        _fixture = fixture;
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            FluentThemeManager.ApplyAccent(null);
        });
    }

    [Fact]
    public void An_indeterminate_check_box_takes_the_accent_surface_and_shows_the_bar()
    {
        _fixture.Run(() =>
        {
            var box = new CheckBox { IsThreeState = true, Width = 40, Height = 32 };
            box.IsChecked = null;
            PixelHarness.Build(box, 40, 32);

            var surface = AssertPart(box, "CheckSurface");
            var bar = AssertPart(box, "MixedGlyph");
            var glyph = AssertPart(box, "CheckGlyph");

            Assert.Equal(1d, bar.Opacity);
            Assert.Equal(0d, glyph.Opacity);
            Assert.Same(_fixture.Application.TryFindResource("AccentFillColorDefaultBrush"), ((Border)surface).Background);

            // The surface claim, not just the object graph: the box is 20x20 opaque pixels, so a
            // re-tinted palette token has to show up in the capture.
            FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", Sentinel);
            try
            {
                var sample = PixelHarness.Render(box, 40, 32);
                Assert.True(sample.Count(Sentinel) > 150, $"indeterminate box did not paint the accent fill; top={sample.Top(6)}");
            }
            finally
            {
                FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", null);
            }
        });
    }

    [Fact]
    public void A_checked_check_box_shows_the_glyph_and_not_the_bar()
    {
        _fixture.Run(() =>
        {
            var box = new CheckBox { IsChecked = true, Width = 40, Height = 32 };
            PixelHarness.Build(box, 40, 32);

            Assert.Equal(1d, AssertPart(box, "CheckGlyph").Opacity);
            Assert.Equal(0d, AssertPart(box, "MixedGlyph").Opacity);
        });
    }

    [Fact]
    public void An_unchecked_check_box_shows_neither_glyph()
    {
        _fixture.Run(() =>
        {
            var box = new CheckBox { Width = 40, Height = 32 };
            PixelHarness.Build(box, 40, 32);

            Assert.Equal(0d, AssertPart(box, "CheckGlyph").Opacity);
            Assert.Equal(0d, AssertPart(box, "MixedGlyph").Opacity);
            Assert.Same(_fixture.Application.TryFindResource("ControlAltFillColorSecondaryBrush"),
                ((Border)AssertPart(box, "CheckSurface")).Background);
        });
    }

    [Fact]
    public void Exactly_one_template_trigger_matches_a_null_checked_value_and_keeps_it_null()
    {
        _fixture.Run(() =>
        {
            var triggers = TemplateTriggers("DefaultCheckBoxStyle").ToList();
            var nullMatches = triggers.Where(static trigger => Matches(trigger, ToggleButton.IsCheckedProperty, null)).ToList();
            Assert.Single(nullMatches);

            // Measured form of the condition values: the nullable IsChecked keeps the markup string
            // "True" and a real null, while the plain booleans arrive as Boolean. The runtime still
            // fires both kinds - the readings above are that proof - but an assertion that compared
            // values by type would be wrong, so this shape is pinned here rather than assumed.
            Assert.True(Matches(triggers.First(static trigger => Matches(trigger, ToggleButton.IsCheckedProperty, "True")),
                ToggleButton.IsCheckedProperty, "True"));
            // Same measured form on the pointer side: after the upstream rewiring every hover cell is a
            // cell of the check matrix, so the boolean arrives inside a MultiTrigger condition.
            Assert.Contains(triggers.OfType<MultiTrigger>().SelectMany(static multi => multi.Conditions.Cast<Condition>()),
                static condition => condition.Property == ToggleButton.IsMouseOverProperty && condition.Value is true);
        });
    }

    [Fact]
    public void Markup_null_for_a_nullable_bool_arrives_as_false()
    {
        // Not what upstream means, and the reason the Gallery sets indeterminate in code: a
        // {x:Null} attribute is parsed without error and yields false, so a tri-state sample written
        // the WinUI way would show an unchecked box while looking correct in markup.
        _fixture.Run(() =>
        {
            var box = (CheckBox)XamlReader.Parse(
                "<CheckBox xmlns='http://schemas.jalium.ui/2024' IsThreeState='True' IsChecked='{x:Null}' />")!;
            Assert.True(box.IsThreeState);
            Assert.False(box.IsChecked);
        });
    }

    [Fact]
    public void The_toggle_button_indeterminate_states_carry_their_upstream_keys()
    {
        _fixture.Run(() =>
        {
            var triggers = FluentThemeManager.GetStyle("DefaultToggleButtonStyle").Triggers.Cast<object>().ToList();

            AssertTriggerSetter(triggers, ToggleButton.IsCheckedProperty, null, "Background", "ToggleButtonBackgroundIndeterminate");
            AssertTriggerSetter(triggers, ToggleButton.IsCheckedProperty, null, "Foreground", "ToggleButtonForegroundIndeterminate");
            AssertTriggerSetter(triggers, ToggleButton.IsCheckedProperty, null, "BorderBrush", "ToggleButtonBorderBrushIndeterminate");
            AssertConditionSetter(triggers, "indeterminate + pointer over", "Background", "ToggleButtonBackgroundIndeterminatePointerOver",
                (ToggleButton.IsCheckedProperty, null), (ToggleButton.IsMouseOverProperty, "True"));
            AssertConditionSetter(triggers, "indeterminate + pressed", "Background", "ToggleButtonBackgroundIndeterminatePressed",
                (ToggleButton.IsCheckedProperty, null), (ToggleButton.IsPressedProperty, "True"));
            AssertConditionSetter(triggers, "indeterminate + disabled", "Background", "ToggleButtonBackgroundIndeterminateDisabled",
                (ToggleButton.IsCheckedProperty, null), (ToggleButton.IsEnabledProperty, "False"));
        });
    }

    [Fact]
    public void An_indeterminate_toggle_button_paints_the_resting_fill_not_the_accent()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.OverrideBrush("ControlFillColorDefaultBrush", Sentinel);
            try
            {
                var sample = PixelHarness.Render(new ToggleButton { Content = "mixed", IsThreeState = true, Width = 200, Height = 44, IsChecked = null }, 200, 44);
                Assert.True(sample.Stable, $"capture never settled: {sample.Top(6)}");
                Assert.True(sample.Count(Sentinel) > 6_000, $"indeterminate toggle did not paint its own rest fill; top={sample.Top(6)}");
            }
            finally
            {
                FluentThemeManager.OverrideBrush("ControlFillColorDefaultBrush", null);
            }
        });
    }

    /// <summary>
    /// Upstream's radio template drives seven properties from eight cells (CommonStates x CheckStates).
    /// Asserted as a table because the point of the batch is that every transcribed <c>RadioButton*</c>
    /// row has a consumer: a cell that quietly keeps a hardcoded token still renders, and only the key
    /// name in the setter proves the alias layer is on the path an app override would take.
    /// </summary>
    [Fact]
    public void The_radio_style_writes_upstream_seven_properties_in_every_state_cell()
    {
        _fixture.Run(() =>
        {
            var triggers = TemplateTriggers("DefaultRadioButtonStyle").ToList();

            var cells = new (string Label, (string Property, string Value)[] Conditions, string Foreground, string RingFill, string RingStroke,
                string DotFill, string DotStroke, int DotSize, bool DotVisible)[]
            {
                ("unchecked + pointer over", [("IsChecked", "False"), ("IsMouseOver", "True")], "RadioButtonForegroundPointerOver",
                    "RadioButtonOuterEllipseFillPointerOver", "RadioButtonOuterEllipseStrokePointerOver",
                    "RadioButtonCheckGlyphFillPointerOver", "RadioButtonCheckGlyphStrokePointerOver", 14, false),
                ("unchecked + pressed", [("IsChecked", "False"), ("IsPressed", "True")], "RadioButtonForegroundPressed",
                    "RadioButtonOuterEllipseFillPressed", "RadioButtonOuterEllipseStrokePressed",
                    "RadioButtonCheckGlyphFillPressed", "RadioButtonCheckGlyphStrokePressed", 10, false),
                ("checked", [("IsChecked", "True")], "RadioButtonForeground",
                    "RadioButtonOuterEllipseCheckedFill", "RadioButtonOuterEllipseCheckedStroke",
                    "RadioButtonCheckGlyphFill", "RadioButtonCheckGlyphStrokeChecked", 12, true),
                ("checked + pointer over", [("IsChecked", "True"), ("IsMouseOver", "True")], "RadioButtonForegroundPointerOver",
                    "RadioButtonOuterEllipseCheckedFillPointerOver", "RadioButtonOuterEllipseCheckedStrokePointerOver",
                    "RadioButtonCheckGlyphFillPointerOver", "RadioButtonCheckGlyphStrokeCheckedPointerOver", 14, true),
                ("checked + pressed", [("IsChecked", "True"), ("IsPressed", "True")], "RadioButtonForegroundPressed",
                    "RadioButtonOuterEllipseCheckedFillPressed", "RadioButtonOuterEllipseCheckedStrokePressed",
                    "RadioButtonCheckGlyphFillPressed", "RadioButtonCheckGlyphStrokeCheckedPressed", 10, true),
                ("unchecked + disabled", [("IsEnabled", "False")], "RadioButtonForegroundDisabled",
                    "RadioButtonOuterEllipseFillDisabled", "RadioButtonOuterEllipseStrokeDisabled",
                    "RadioButtonCheckGlyphFillDisabled", "RadioButtonCheckGlyphStrokeDisabled", 14, false),
                ("checked + disabled", [("IsChecked", "True"), ("IsEnabled", "False")], "RadioButtonForegroundDisabled",
                    "RadioButtonOuterEllipseCheckedFillDisabled", "RadioButtonOuterEllipseCheckedStrokeDisabled",
                    "RadioButtonCheckGlyphFillDisabled", "RadioButtonCheckGlyphStrokeCheckedDisabled", 14, true),
            };

            foreach (var cell in cells)
            {
                var wanted = new List<(string Part, string Property, string Key)>
                {
                    ("RadioLabel", "Foreground", cell.Foreground),
                    ("RadioRoot", "Background", cell.Foreground.Replace("Foreground", "Background")),
                    ("RadioRoot", "BorderBrush", cell.Foreground.Replace("Foreground", "BorderBrush")),
                    ("RadioRing", "Background", cell.RingFill),
                    ("RadioRing", "BorderBrush", cell.RingStroke),
                    ("RadioDot", "Fill", cell.DotFill),
                    ("RadioDot", "Stroke", cell.DotStroke),
                };
                AssertCell(triggers, cell.Label, cell.Conditions, wanted);
                AssertCellSize(triggers, cell.Label, cell.Conditions, cell.DotSize);
                AssertCellOpacity(triggers, cell.Label, cell.Conditions, cell.DotVisible);
            }
        });
    }

    [Fact]
    public void An_unchecked_radio_button_rests_on_the_transcribed_ellipse_rows()
    {
        _fixture.Run(() =>
        {
            var radio = new RadioButton { Content = "plain", Width = 120, Height = 32 };
            PixelHarness.Build(radio, 120, 32);

            var ring = (Border)AssertPart(radio, "RadioRing");
            var dot = (Jalium.UI.Shapes.Ellipse)AssertPart(radio, "RadioDot");
            Assert.Equal(0d, dot.Opacity);
            Assert.Same(_fixture.Application.TryFindResource("ControlAltFillColorSecondaryBrush"), ring.Background);
            Assert.Same(_fixture.Application.TryFindResource("ControlStrongStrokeColorDefaultBrush"), ring.BorderBrush);
        });
    }

    [Fact]
    public void A_checked_radio_button_drops_the_accent_ring_behind_the_dot()
    {
        _fixture.Run(() =>
        {
            var radio = new RadioButton { Content = "picked", IsChecked = true, Width = 120, Height = 32 };
            PixelHarness.Build(radio, 120, 32);

            var ring = (Border)AssertPart(radio, "RadioRing");
            var dot = (Jalium.UI.Shapes.Ellipse)AssertPart(radio, "RadioDot");
            Assert.Equal(1d, dot.Opacity);
            Assert.Same(_fixture.Application.TryFindResource("AccentFillColorDefaultBrush"), ring.Background);
            Assert.Same(_fixture.Application.TryFindResource("AccentFillColorDefaultBrush"), ring.BorderBrush);

            // The dot ring is the substitution's read-back: upstream asks for
            // AccentControlElevationBorderBrush, a bounding-box gradient this palette has no row for,
            // and ControlStrokeColorOnAccentDefaultBrush is what we hand it instead.
            Assert.Same(_fixture.Application.TryFindResource("ControlStrokeColorOnAccentDefaultBrush"), dot.Stroke);
        });
    }

    [Fact]
    public void A_disabled_checked_radio_button_keeps_the_dot_and_loses_the_accent()
    {
        _fixture.Run(() =>
        {
            var radio = new RadioButton { Content = "off", IsChecked = true, Width = 120, Height = 32 };
            PixelHarness.Build(radio, 120, 32);
            radio.IsEnabled = false;
            PixelHarness.Settle();

            var ring = (Border)AssertPart(radio, "RadioRing");
            var dot = (Jalium.UI.Shapes.Ellipse)AssertPart(radio, "RadioDot");
            Assert.Equal(1d, dot.Opacity);
            Assert.Same(_fixture.Application.TryFindResource("AccentFillColorDisabledBrush"), ring.Background);
            Assert.Same(_fixture.Application.TryFindResource("ControlStrokeColorDefaultBrush"), dot.Stroke);
        });
    }

    [Fact]
    public void A_checked_radio_button_paints_the_accent_ring_to_pixels()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", Sentinel);
            try
            {
                var radio = new RadioButton { Content = "picked", IsChecked = true, Width = 32, Height = 32 };
                var sample = PixelHarness.Render(radio, 32, 32);
                Assert.True(sample.Stable, $"capture never settled: {sample.Top(6)}");

                // A 20-DIP ring minus its 12-DIP dot is roughly 290 pixels; the threshold leaves room
                // for the antialiased edge but not for a ring that only half took the token.
                Assert.True(sample.Count(Sentinel) > 150, $"checked radio did not paint the accent ring; top={sample.Top(6)}");
            }
            finally
            {
                FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", null);
            }
        });
    }

    [Fact]
    public void Both_choice_controls_carry_upstream_minimum_width()
    {
        // Upstream gives both controls MinWidth 120 so a lone choice stays clickable at label width.
        // Ours shipped 0, which is the value the inkcanvas reference port also carries - matching that
        // reference here would have kept a deviation the token audit calls out, so the literal rides on.
        _fixture.Run(() =>
        {
            foreach (var styleKey in new[] { "DefaultCheckBoxStyle", "DefaultRadioButtonStyle" })
            {
                var value = FluentThemeManager.GetStyle(styleKey).Setters.Cast<object>().OfType<Setter>()
                    .First(static setter => NameOf(setter) == "MinWidth")
                    .Value;
                Assert.Equal(120d, Convert.ToDouble(value));
            }
        });
    }

    /// <summary>
    /// The catalog carried "the check glyph is not in the capture path" as a standing gap. It is wrong:
    /// with the surface token and the glyph token re-tinted to two different sentinels, both colours
    /// have to appear, and neither can come from the other part. Measured: 12 exact sentinel pixels plus
    /// 22 accent-magenta blends, since a 1.5 DIP stroke at 175% is mostly antialiased - so the claim is
    /// "appears at all", not an area. Kept to one capture and no caption after a two-capture version of
    /// this test blew the fixture's 60-second watchdog (the text and unchecked-box costs are in 06).
    /// </summary>
    [Fact]
    public void The_check_box_paints_its_glyph_token_separately_from_its_surface_token()
    {
        _fixture.Run(() =>
        {
            var accent = Color.FromRgb(0xFF, 0x80, 0x00);
            FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", accent);
            FluentThemeManager.OverrideBrush("TextOnAccentFillColorPrimaryBrush", Sentinel);
            try
            {
                var sample = PixelHarness.Render(new CheckBox { IsChecked = true, Width = 32, Height = 32 }, 32, 32);
                Assert.True(sample.Stable, $"capture never settled: {sample.Top(6)}");
                Assert.True(sample.Count(accent) > 150, $"checked surface did not paint its token; top={sample.Top(6)}");
                Assert.True(sample.Count(Sentinel) > 0, $"check mark did not paint; top={sample.Top(6)}");
            }
            finally
            {
                FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", null);
                FluentThemeManager.OverrideBrush("TextOnAccentFillColorPrimaryBrush", null);
            }
        });
    }

    /// <summary>
    /// Radio exclusivity is the one piece of behaviour a retemplated radio cannot inherit from its own
    /// template, and the catalog carried it as unmeasured. Read as a measurement, not an assumption:
    /// a plain panel is the group, and the loser is released without anyone having to clear it.
    /// </summary>
    [Fact]
    public void A_shared_parent_panel_is_the_radio_group()
    {
        _fixture.Run(() =>
        {
            var first = new RadioButton { Content = "first" };
            var second = new RadioButton { Content = "second" };
            var panel = new StackPanel();
            panel.Children.Add(first);
            panel.Children.Add(second);
            PixelHarness.Build(panel, 200, 72);

            first.IsChecked = true;
            Assert.True(first.IsChecked);
            Assert.False(second.IsChecked);

            second.IsChecked = true;
            Assert.True(second.IsChecked);
            Assert.False(first.IsChecked);
        });
    }

    /// <summary>
    /// The property a setter writes, in either of the two forms the parser produces. A setter whose
    /// target property exists on the style's own type (Background, Width) is resolved at parse time and
    /// arrives as a DependencyProperty; one that only exists on the named part (Shape.Fill, Shape.Stroke)
    /// is deferred as a name and resolved against the real element when the template is applied.
    /// Reading only <see cref="Setter.Property"/> would call those deferred setters unnamed.
    /// </summary>
    private static string? NameOf(Setter setter) => setter.Property?.Name ?? setter.PropertyName;

    private static FrameworkElement AssertPart(Visual root, string name) =>
        PixelHarness.Named(root, name) ?? throw new InvalidOperationException($"No part named {name} in the built tree.");

    private static IEnumerable<object> TemplateTriggers(string styleKey)
    {
        var template = FluentThemeManager.GetStyle(styleKey).Setters.Cast<object>()
            .OfType<Setter>()
            .Select(static setter => setter.Value as ControlTemplate)
            .FirstOrDefault(static candidate => candidate is not null);
        return (template ?? throw new InvalidOperationException($"{styleKey} carries no inline template")).Triggers.Cast<object>();
    }

    private static bool Matches(object trigger, DependencyProperty property, string? expected) => trigger switch
    {
        Trigger single => single.Property == property && Form(single.Value) == (expected ?? "<null>"),
        _ => false,
    };

    private static string Form(object? value) => value?.ToString() ?? "<null>";

    private static void AssertTriggerSetter(List<object> triggers, DependencyProperty property, string? expected, string setterProperty, string resourceKey)
    {
        var trigger = triggers.FirstOrDefault(candidate => Matches(candidate, property, expected))
            ?? throw new InvalidOperationException($"No {property.Name}={expected ?? "<null>"} trigger in the style.");
        AssertSetter(trigger, setterProperty, resourceKey);
    }

    private static void AssertConditionSetter(List<object> triggers, string label, string setterProperty, string resourceKey,
        params (DependencyProperty Property, string? Expected)[] conditions)
    {
        var wanted = conditions.Select(static condition => (condition.Property, Form(condition.Expected))).ToArray();
        var trigger = triggers.OfType<MultiTrigger>().FirstOrDefault(candidate =>
            candidate.Conditions.Cast<Condition>().Select(static condition => (condition.Property!, Form(condition.Value)))
                .SequenceEqual(wanted))
            ?? throw new InvalidOperationException($"No multi-trigger for {label}.");
        AssertSetter(trigger, setterProperty, resourceKey);
    }

    private static void AssertSetter(object trigger, string setterProperty, string resourceKey)
    {
        var setters = TriggerSetters(trigger);
        var setter = setters.OfType<Setter>().FirstOrDefault(candidate => NameOf(candidate) == setterProperty)
            ?? throw new InvalidOperationException($"No {setterProperty} setter on the trigger.");
        var key = setter.Value?.GetType().GetProperty("ResourceKey")?.GetValue(setter.Value) as string;
        Assert.Equal(resourceKey, key);
    }

    private static object FindCell(List<object> triggers, (string Property, string Value)[] conditions, string label)
    {
        var wanted = conditions.Select(static condition => (condition.Property, condition.Value)).ToArray();
        return triggers.FirstOrDefault(candidate => Conditions(candidate).SequenceEqual(wanted))
            ?? throw new InvalidOperationException($"No state cell for {label}; present: " +
                string.Join(" | ", triggers.Select(static trigger => string.Join("+", Conditions(trigger).Select(static pair => pair.Item1 + "=" + pair.Item2)))));
    }

    private static IEnumerable<(string Property, string Value)> Conditions(object trigger) => trigger switch
    {
        MultiTrigger multi => multi.Conditions.Cast<Condition>().Select(static condition => (condition.Property!.Name, Form(condition.Value))),
        Trigger single => [(single.Property!.Name, Form(single.Value))],
        _ => [],
    };

    private static IEnumerable<object> TriggerSetters(object trigger) => trigger switch
    {
        Trigger single => single.Setters.Cast<object>(),
        MultiTrigger multi => multi.Setters.Cast<object>(),
        _ => Enumerable.Empty<object>(),
    };

    private static void AssertCell(List<object> triggers, string label, (string Property, string Value)[] conditions,
        List<(string Part, string Property, string Key)> wanted)
    {
        var cell = FindCell(triggers, conditions, label);
        foreach (var (part, property, key) in wanted)
        {
            var setter = TriggerSetters(cell).OfType<Setter>()
                .FirstOrDefault(candidate => candidate.TargetName == part && NameOf(candidate) == property)
                ?? throw new InvalidOperationException($"{label}: no {part}.{property} setter; the cell carries "
                    + string.Join(", ", TriggerSetters(cell).OfType<Setter>().Select(static candidate => $"{candidate.TargetName}.{NameOf(candidate)}")));
            var actual = setter.Value?.GetType().GetProperty("ResourceKey")?.GetValue(setter.Value) as string;
            Assert.Equal(key, actual);
        }
    }

    private static void AssertCellSize(List<object> triggers, string label, (string Property, string Value)[] conditions, int size)
    {
        var setter = TriggerSetters(FindCell(triggers, conditions, label)).OfType<Setter>()
            .FirstOrDefault(candidate => candidate.TargetName == "RadioDot" && NameOf(candidate) == "Width")
            ?? throw new InvalidOperationException($"{label}: the dot has no size cell.");
        Assert.Equal(size, Convert.ToDouble(setter.Value));
    }

    private static void AssertCellOpacity(List<object> triggers, string label, (string Property, string Value)[] conditions, bool visible)
    {
        var setter = TriggerSetters(FindCell(triggers, conditions, label)).OfType<Setter>()
            .FirstOrDefault(candidate => candidate.TargetName == "RadioDot" && NameOf(candidate) == "Opacity");
        if (visible)
        {
            Assert.NotNull(setter);
            Assert.Equal(1d, Convert.ToDouble(setter!.Value));
        }
        else
        {
            // The dot is only ever revealed by a checked cell; an unchecked cell that set it would
            // make a hovered, unchecked radio show a filled ring center.
            Assert.Null(setter);
        }
    }
}
