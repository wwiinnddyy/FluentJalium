using System.Reflection;
using FluentJalium.Themes;
using FluentJalium.Tests.Pixel;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;

namespace FluentJalium.Tests;

/// <summary>
/// Slider: the control that turned two open harness questions into answers. Its first Astra template was
/// held in a dictionary resource, and a keyed <see cref="ControlTemplate"/> comes back from this reader with
/// every <c>Trigger.Property</c> unresolved, so all eight of its state cells were inert - the build was green,
/// the markup was right, and nothing on screen ever changed (docs/astra/audits/slider.md). Fixing that shape
/// also gave the focus claim a working frame pump: <c>PixelHarness.Pump</c>'s watchdog had been releasing a
/// dispatcher nobody was running, which is what hung the selection batch twice (docs/astra/adaptation/06).
/// </summary>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraSliderTests
{
    private static readonly object On = true;
    private static readonly object Off = false;
    private static readonly Color TrackSentinel = Color.FromRgb(0xFF, 0x00, 0xFF);
    private static readonly Color AccentSentinel = Color.FromRgb(0x00, 0xFF, 0x80);
    private static readonly Color BrandEmerald = Color.FromRgb(0x20, 0x72, 0x45);

    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraSliderTests(AstraThemeRuntimeFixture fixture)
    {
        _fixture = fixture;
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            FluentThemeManager.ApplyAccent(null);
        });
    }

    /// <summary>
    /// Row-by-row transcription promise: an upstream name hands back the palette object the kernel re-tints,
    /// not a copy. The right column is upstream's target except on <c>SliderThumbBorderBrush</c>, where the
    /// gradient upstream uses cannot ride the in-place retint model and the solid stop stands in.
    /// </summary>
    [Theory]
    [InlineData("SliderContainerBackground", "ControlFillColorTransparentBrush")]
    [InlineData("SliderContainerBackgroundPointerOver", "ControlFillColorTransparentBrush")]
    [InlineData("SliderContainerBackgroundPressed", "ControlFillColorTransparentBrush")]
    [InlineData("SliderContainerBackgroundDisabled", "ControlFillColorTransparentBrush")]
    [InlineData("SliderThumbBackground", "AccentFillColorDefaultBrush")]
    [InlineData("SliderThumbBackgroundPointerOver", "AccentFillColorSecondaryBrush")]
    [InlineData("SliderThumbBackgroundPressed", "AccentFillColorTertiaryBrush")]
    [InlineData("SliderThumbBackgroundDisabled", "AccentFillColorDisabledBrush")]
    [InlineData("SliderThumbBorderBrush", "ControlStrokeColorDefaultBrush")]
    [InlineData("SliderOuterThumbBackground", "ControlSolidFillColorDefaultBrush")]
    [InlineData("SliderTrackFill", "ControlStrongFillColorDefaultBrush")]
    [InlineData("SliderTrackFillPointerOver", "ControlStrongFillColorDefaultBrush")]
    [InlineData("SliderTrackFillPressed", "ControlStrongFillColorDefaultBrush")]
    [InlineData("SliderTrackFillDisabled", "ControlStrongFillColorDisabledBrush")]
    [InlineData("SliderTrackValueFill", "AccentFillColorDefaultBrush")]
    [InlineData("SliderTrackValueFillPointerOver", "AccentFillColorSecondaryBrush")]
    [InlineData("SliderTrackValueFillPressed", "AccentFillColorTertiaryBrush")]
    [InlineData("SliderTrackValueFillDisabled", "AccentFillColorDisabledBrush")]
    [InlineData("SliderTickBarFill", "ControlStrongFillColorDefaultBrush")]
    [InlineData("SliderTickBarFillDisabled", "ControlStrongFillColorDisabledBrush")]
    public void An_upstream_slider_row_hands_back_the_palette_instance_it_aliases(string alias, string paletteKey)
    {
        _fixture.Run(() => Assert.Same(FluentThemeManager.GetBrush(paletteKey), Res(alias)));
    }

    /// <summary>
    /// What is left out is a claim about the runtime, so it is pinned as a reading: if Jalium ever grows a
    /// header on Slider or an inline tick placement on either enum, the three omitted rows have to be
    /// re-decided instead of drifting back in unnoticed.
    /// </summary>
    [Fact]
    public void The_rows_left_out_are_left_out_because_no_state_can_reach_them()
    {
        static bool Has(Type type, string property) =>
            type.GetProperty(property, BindingFlags.Public | BindingFlags.Instance) is not null;

        var placement = typeof(Slider).GetProperty("TickPlacement")!.PropertyType;
        Assert.DoesNotContain("Inline", Enum.GetNames(placement));
        Assert.DoesNotContain("Inline", Enum.GetNames(TickBarPlacementType()));

        Assert.False(Has(typeof(Slider), "Header"));
        Assert.True(Has(typeof(Slider), "TickFrequency"));

        // And the two rows the header would have painted are not orphaned either: the tick bars the colours
        // remain are real TickBar parts in the template, so SliderTickBarFill* has somewhere to land.
        Assert.NotNull(TickBarPlacementType());
    }

    /// <summary>
    /// The state map, read off the hydrated templates. Two claims per cell: the condition resolved to a real
    /// property (the keyed-template defect left every one of these null), and the setters carry upstream's row
    /// for that state. Order is part of the claim - the disabled cell has to come last to beat hover.
    /// </summary>
    [Fact]
    public void Both_slider_templates_carry_one_cell_per_upstream_state()
    {
        _fixture.Run(() =>
        {
            var horizontal = Cells(HorizontalTemplate());
            var vertical = Cells(VerticalTemplate());
            var states = new[] { "TickPlacement", "TickPlacement", "TickPlacement", "IsMouseOver", "IsMouseCaptured", "AreAnyTouchesCaptured", "IsKeyboardFocused", "IsEnabled" };

            Assert.Equal(states, horizontal.Select(static cell => cell.Property!.Name));
            Assert.Equal(states, vertical.Select(static cell => cell.Property!.Name));

            foreach (var cells in new[] { horizontal, vertical })
            {
                AssertCell(cells, "IsMouseOver", On,
                    ("SliderContainer", "Background", "SliderContainerBackgroundPointerOver"),
                    ("PART_Track", "Background", "SliderTrackFillPointerOver"),
                    ("PART_SelectionRange", "Background", "SliderTrackValueFillPointerOver"),
                    ("SliderInnerThumb", "Fill", "SliderThumbBackgroundPointerOver"));
                AssertCell(cells, "IsMouseCaptured", On,
                    ("SliderContainer", "Background", "SliderContainerBackgroundPressed"),
                    ("PART_Track", "Background", "SliderTrackFillPressed"),
                    ("PART_SelectionRange", "Background", "SliderTrackValueFillPressed"),
                    ("SliderInnerThumb", "Fill", "SliderThumbBackgroundPressed"));
                AssertCell(cells, "AreAnyTouchesCaptured", On,
                    ("SliderContainer", "Background", "SliderContainerBackgroundPressed"),
                    ("SliderInnerThumb", "Fill", "SliderThumbBackgroundPressed"));
                AssertCell(cells, "IsKeyboardFocused", On, ("SliderFocus", "Opacity", null));
                AssertCell(cells, "IsEnabled", Off,
                    ("SliderContainer", "Background", "SliderContainerBackgroundDisabled"),
                    ("PART_Track", "Background", "SliderTrackFillDisabled"),
                    ("PART_SelectionRange", "Background", "SliderTrackValueFillDisabled"),
                    ("SliderInnerThumb", "Fill", "SliderThumbBackgroundDisabled"));
            }

            // The tick bars are the pair each orientation names: Top/Bottom across a horizontal slider,
            // Left/Right down a vertical one.
            AssertCell(horizontal, "TickPlacement", "TopLeft", ("TopTickBar", "Visibility", null));
            AssertCell(horizontal, "TickPlacement", "BottomRight", ("BottomTickBar", "Visibility", null));
            AssertCell(horizontal, "TickPlacement", "Both",
                ("TopTickBar", "Visibility", null), ("BottomTickBar", "Visibility", null));
            AssertCell(vertical, "TickPlacement", "TopLeft", ("LeftTickBar", "Visibility", null));
            AssertCell(vertical, "TickPlacement", "BottomRight", ("RightTickBar", "Visibility", null));
            AssertCell(vertical, "TickPlacement", "Both",
                ("LeftTickBar", "Visibility", null), ("RightTickBar", "Visibility", null));

            // The disabled cell also carries the tick-bar rows and upstream's 12 DIP resting thumb.
            var disabled = horizontal.Single(static cell => cell.Property!.Name == "IsEnabled");
            var disabledSetters = disabled.Setters.Cast<object>().OfType<Setter>().ToList();
            Assert.Contains(disabledSetters, static setter =>
                setter.TargetName == "TopTickBar" && KeyOf(setter) == "SliderTickBarFillDisabled");
            Assert.Contains(disabledSetters, static setter =>
                setter.TargetName == "BottomTickBar" && KeyOf(setter) == "SliderTickBarFillDisabled");
            Assert.Contains(disabledSetters, static setter =>
                setter.TargetName == "SliderInnerThumb" && Equals(setter.Value, 12d));
        });
    }

    /// <summary>
    /// Upstream's style rows, read twice: the setter has to name the key and the control the style produced
    /// has to end up with the value. Unlike the text controls, the Slider's own Background and Foreground
    /// really do come back as the palette instances, so the track pair is driven by the transcribed rows end
    /// to end, and the two metric rows land on the parts that consume them.
    /// </summary>
    [Fact]
    public void The_slider_style_reads_upstreams_track_rows()
    {
        _fixture.Run(() =>
        {
            var style = FluentThemeManager.GetStyle("DefaultSliderStyle");
            Assert.Equal("SliderTrackFill", KeyOf(Setter(style, "Background")));
            Assert.Equal("SliderTrackValueFill", KeyOf(Setter(style, "Foreground")));
            Assert.Equal("SliderBorderThemeThickness", KeyOf(Setter(style, "BorderThickness")));
            Assert.Equal(32d, Convert.ToDouble(Setter(style, "MinHeight").Value!));
            Assert.Equal(32d, Convert.ToDouble(Setter(style, "MinWidth").Value!));

            var slider = Mount(new Slider { Value = 50 });
            Assert.Multiple(
                () => Assert.Same(Res("SliderTrackFill"), slider.Background),
                () => Assert.Same(Res("SliderTrackValueFill"), slider.Foreground),
                () => Assert.Equal(new Thickness(0), slider.BorderThickness),
                () => Assert.Equal(32d, slider.MinHeight),
                () => Assert.Equal(32d, slider.MinWidth),
                () => Assert.Same(Res("SliderContainerBackground"), Get(Part(slider, "SliderContainer"), "Background")),
                () => Assert.Same(Res("SliderOuterThumbBackground"), Ring(slider).Background),
                () => Assert.Same(Res("SliderThumbBorderBrush"), Ring(slider).BorderBrush),
                () => Assert.Same(Res("SliderThumbBackground"), Get(Part(slider, "SliderInnerThumb"), "Fill")),
                () => Assert.Equal(Res("SliderTrackCornerRadius"), Get(Part(slider, "PART_Track"), "CornerRadius")),
                () => Assert.Equal(Res("SliderThumbCornerRadius"), Ring(slider).CornerRadius));
        });
    }

    /// <summary>
    /// TickPlacement as behaviour, not markup: mounting a slider with each value has to leave exactly the bars
    /// upstream shows, laid out at upstream's 4 DIP thickness, in both orientations.
    /// </summary>
    [Fact]
    public void A_tick_placement_shows_only_the_bars_the_orientation_calls_for()
    {
        _fixture.Run(() =>
        {
            var quiet = Mount(new Slider { Value = 50 });
            Assert.Multiple(
                () => Assert.Equal(Visibility.Collapsed, (Visibility)Get(Part(quiet, "TopTickBar"), "Visibility")),
                () => Assert.Equal(Visibility.Collapsed, (Visibility)Get(Part(quiet, "BottomTickBar"), "Visibility")));

            var both = Mount(new Slider { Value = 50, TickFrequency = 10 }, "TickPlacement", "Both");
            var bar = Part(both, "TopTickBar");
            Assert.Multiple(
                () => Assert.Equal(Visibility.Visible, (Visibility)Get(bar, "Visibility")),
                () => Assert.Equal(Visibility.Visible, (Visibility)Get(Part(both, "BottomTickBar"), "Visibility")),
                () => Assert.Equal(204d, bar.ActualWidth, 3),
                () => Assert.Equal(4d, bar.ActualHeight, 3),
                () => Assert.Same(Res("SliderTickBarFill"), Get(bar, "Fill")));

            var above = Mount(new Slider { Value = 50, TickFrequency = 10 }, "TickPlacement", "TopLeft");
            Assert.Multiple(
                () => Assert.Equal(Visibility.Visible, (Visibility)Get(Part(above, "TopTickBar"), "Visibility")),
                () => Assert.Equal(Visibility.Collapsed, (Visibility)Get(Part(above, "BottomTickBar"), "Visibility")));

            var right = Mount(new Slider { Value = 50, TickFrequency = 10 }, "Orientation", "Vertical", "TickPlacement", "BottomRight");
            Assert.Multiple(
                () => Assert.Equal(Visibility.Visible, (Visibility)Get(Part(right, "RightTickBar"), "Visibility")),
                () => Assert.Equal(Visibility.Collapsed, (Visibility)Get(Part(right, "LeftTickBar"), "Visibility")));
        });
    }

    /// <summary>
    /// The framework drives the parts our template paints: it writes PART_Thumb's Margin and sizes
    /// PART_SelectionRange to the value (measured: 0, 51, 102, 153, 204 along a 204 DIP track). That is why
    /// those two names are a contract with Jalium rather than upstream's HorizontalThumb /
    /// HorizontalDecreaseRect, and it is the value-following claim without a capture.
    /// </summary>
    [Fact]
    public void The_value_fill_and_the_thumb_follow_the_value()
    {
        _fixture.Run(() =>
        {
            var fills = new List<double>();
            var thumbs = new List<double>();
            foreach (var value in new[] { 0d, 25d, 50d, 75d, 100d })
            {
                var slider = Mount(new Slider { Minimum = 0, Maximum = 100, Value = value });
                fills.Add(Math.Round(Part(slider, "PART_SelectionRange").ActualWidth));
                thumbs.Add(Math.Round(Part(slider, "PART_Thumb").Margin.Left));
            }

            Assert.Equal([0d, 51d, 102d, 153d, 204d], fills);
            Assert.Equal(fills, thumbs);
        });
    }

    [Fact]
    public void A_disabled_slider_drops_every_surface_to_its_rows()
    {
        _fixture.Run(() =>
        {
            var slider = Mount(new Slider { Value = 50, TickFrequency = 10, IsEnabled = false }, "TickPlacement", "Both");
            Assert.Multiple(
                () => Assert.Same(Res("SliderTrackFillDisabled"), Get(Part(slider, "PART_Track"), "Background")),
                () => Assert.Same(Res("SliderTrackValueFillDisabled"), Get(Part(slider, "PART_SelectionRange"), "Background")),
                () => Assert.Same(Res("SliderThumbBackgroundDisabled"), Get(Part(slider, "SliderInnerThumb"), "Fill")),
                () => Assert.Same(Res("SliderTickBarFillDisabled"), Get(Part(slider, "TopTickBar"), "Fill")),
                () => Assert.Same(Res("SliderTickBarFillDisabled"), Get(Part(slider, "BottomTickBar"), "Fill")),
                () => Assert.Same(Res("SliderContainerBackgroundDisabled"), Get(Part(slider, "SliderContainer"), "Background")),
                () => Assert.Equal(12d, (double)Get(Part(slider, "SliderInnerThumb"), "Width")),
                () => Assert.Equal(0d, (double)Get(Part(slider, "SliderFocus"), "Opacity")));
        });
    }

    /// <summary>
    /// The second keyboard-focus read-back in the repository, and the one that was impossible until the frame
    /// pump was fixed: focusing a slider starts no animation, so the old watchdog never released the frame and
    /// the fixture died at 60 seconds rather than reporting the state.
    /// </summary>
    [Fact]
    public void A_focused_slider_raises_its_focus_ring()
    {
        _fixture.Run(() =>
        {
            var slider = Mount(new Slider { Value = 50 });
            var ring = Part(slider, "SliderFocus");
            Assert.Equal(0d, (double)Get(ring, "Opacity"));

            Assert.True(slider.Focus(), "Focus() refused the slider in the host window.");
            PixelHarness.Settle();
            Assert.True(slider.IsKeyboardFocused);
            Assert.Equal(1d, (double)Get(ring, "Opacity"));
        });
    }

    /// <summary>
    /// One capture, three readings: the track row and the accent rows both reach the pixels through their
    /// aliases, and the brand emerald the framework used to paint on an untemplated slider is gone.
    /// </summary>
    [Fact]
    public void A_resting_slider_paints_its_rows_and_nothing_invented()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.OverrideBrush("ControlStrongFillColorDefaultBrush", TrackSentinel);
            FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", AccentSentinel);
            try
            {
                var sample = PixelHarness.Render(new Slider { Width = 220, Height = 32, Minimum = 0, Maximum = 100, Value = 50 }, 220, 32);
                Assert.True(sample.Stable, $"capture never settled: {sample.Top(6)}");
                Assert.True(sample.Count(TrackSentinel) > 300, $"track row did not reach the pixels; top={sample.Top(8)}");
                Assert.True(sample.Count(AccentSentinel) > 300, $"accent rows did not reach the pixels; top={sample.Top(8)}");
                Assert.Equal(0, sample.Count(BrandEmerald));
            }
            finally
            {
                FluentThemeManager.OverrideBrush("ControlStrongFillColorDefaultBrush", null);
                FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", null);
            }
        });
    }

    /// <summary>
    /// The key our own palette generator invented for the thumb ring's stroke is gone, and with it the last
    /// slider row without an upstream name: the ring now reads SliderThumbBorderBrush, which upstream has.
    /// </summary>
    [Fact]
    public void The_invented_thumb_stroke_key_is_gone_from_the_palette()
    {
        _fixture.Run(() => Assert.Null(Res("SliderThumbStrokeBrush")));
    }

    private object? Res(string key) => _fixture.Application.TryFindResource(key);

    private Style SliderStyle() => FluentThemeManager.GetStyle("DefaultSliderStyle");

    private ControlTemplate HorizontalTemplate() => SliderStyle().Setters.Cast<object>().OfType<Setter>()
        .Select(static setter => setter.Value as ControlTemplate).First(static template => template is not null)!;

    private ControlTemplate VerticalTemplate() =>
        (SliderStyle().Triggers.Cast<object>().OfType<Trigger>().First()
            .Setters.Cast<object>().OfType<Setter>().First().Value as ControlTemplate)
        ?? throw new InvalidOperationException("the Orientation cell's Template setter holds no template");

    /// <summary>Mounts a slider in the shown host, optionally flipping enum properties first.</summary>
    private Slider Mount(Slider slider, params string[] andSet)
    {
        for (var index = 0; index < andSet.Length; index += 2)
        {
            SetEnum(slider, andSet[index], andSet[index + 1]);
        }

        var vertical = slider.Orientation == Orientation.Vertical;
        var width = vertical ? 32 : 220;
        var height = vertical ? 220 : 32;
        slider.Width = width;
        slider.Height = height;
        PixelHarness.Build(slider, width, height);
        return slider;
    }

    private static void SetEnum(DependencyObject target, string property, string value)
    {
        var owner = target.GetType();
        var clr = owner.GetProperty(property) ?? throw new InvalidOperationException($"{owner.Name} has no {property}");
        var dp = DependencyProperty.FromName(owner, property) ?? throw new InvalidOperationException($"{owner.Name}.{property} is not a dependency property");
        target.SetValue(dp, Enum.Parse(clr.PropertyType, value));
    }

    private static Type TickBarPlacementType() =>
        (typeof(Slider).Assembly.GetType("Jalium.UI.Controls.Primitives.TickBar")
            ?? throw new InvalidOperationException("This runtime has no TickBar to place."))
        .GetProperty("Placement")!.PropertyType;

    private static FrameworkElement Part(DependencyObject root, string name) =>
        PixelHarness.Named(root, name) ?? throw new InvalidOperationException($"No part named {name} in the built tree.");

    private static Border Ring(DependencyObject root) =>
        PixelHarness.Descendant<Border>(Part(root, "PART_Thumb"))
        ?? throw new InvalidOperationException("No ring Border inside PART_Thumb.");

    private static object Get(FrameworkElement part, string property) =>
        part.GetType().GetProperty(property, BindingFlags.Public | BindingFlags.Instance)?.GetValue(part)
        ?? throw new InvalidOperationException($"{part.GetType().Name} has no {property}.");

    private static List<Trigger> Cells(ControlTemplate template) =>
        template.Triggers.Cast<object>().OfType<Trigger>().ToList();

    private static Setter Setter(Style style, string property) => style.Setters.Cast<object>().OfType<Setter>()
        .First(setter => (setter.Property?.Name ?? setter.PropertyName) == property);

    private static string? KeyOf(Setter setter) =>
        setter.Value?.GetType().GetProperty("ResourceKey")?.GetValue(setter.Value) as string;

    private static string? Format(object? value) => value?.ToString();

    private static void AssertCell(List<Trigger> cells, string property, object condition, params (string Part, string Property, string? Key)[] wanted)
    {
        var cell = cells.FirstOrDefault(candidate =>
            candidate.Property?.Name == property && string.Equals(Format(candidate.Value), Format(condition)))
            ?? throw new InvalidOperationException($"No cell for {property}={condition}; the template carries " +
                string.Join(", ", cells.Select(static candidate => $"{candidate.Property?.Name}={candidate.Value}")));
        var setters = cell.Setters.Cast<object>().OfType<Setter>().ToList();
        foreach (var (part, name, key) in wanted)
        {
            var setter = setters.FirstOrDefault(candidate =>
                    candidate.TargetName == part && (candidate.Property?.Name ?? candidate.PropertyName) == name)
                ?? throw new InvalidOperationException($"{property}={condition}: no {part}.{name} setter; the cell carries " +
                    string.Join(", ", setters.Select(static candidate => $"{candidate.TargetName ?? "self"}.{candidate.Property?.Name ?? candidate.PropertyName}")));
            if (key is not null)
            {
                Assert.Equal(key, KeyOf(setter));
            }
        }
    }
}
