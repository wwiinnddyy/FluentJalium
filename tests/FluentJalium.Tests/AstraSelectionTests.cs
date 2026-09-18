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
            Assert.True(triggers.OfType<Trigger>().Any(static trigger =>
                trigger.Property == ToggleButton.IsMouseOverProperty && trigger.Value is true));
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

    private static string? Form(object? value) => value?.ToString() ?? "<null>";

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
            candidate.Conditions.Cast<Condition>().Select(static condition => (condition.Property, Form(condition.Value)))
                .SequenceEqual(wanted))
            ?? throw new InvalidOperationException($"No multi-trigger for {label}.");
        AssertSetter(trigger, setterProperty, resourceKey);
    }

    private static void AssertSetter(object trigger, string setterProperty, string resourceKey)
    {
        var setters = trigger switch
        {
            Trigger single => single.Setters.Cast<object>(),
            MultiTrigger multi => multi.Setters.Cast<object>(),
            _ => Enumerable.Empty<object>(),
        };
        var setter = setters.OfType<Setter>().FirstOrDefault(candidate => candidate.Property?.Name == setterProperty)
            ?? throw new InvalidOperationException($"No {setterProperty} setter on the trigger.");
        var key = setter.Value?.GetType().GetProperty("ResourceKey")?.GetValue(setter.Value) as string;
        Assert.Equal(resourceKey, key);
    }
}
