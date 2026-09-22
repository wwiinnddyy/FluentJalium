using FluentJalium.Tests.Pixel;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using Xunit;

namespace FluentJalium.Tests;

/// <summary>
/// The calendar family (<c>Calendar</c> / <c>DatePicker</c> / <c>TimePicker</c>), read as the self-drawn controls the
/// runtime ships them as. #70 opened on a walk that counted visual children and read 0 for all three next to a
/// Button's 4, and recorded "renders nothing"; that reading was the instrument, not the subject. The same walk shape
/// had already misled this repository once (<c>ScrollBar</c>, <c>adaptation/06</c> clause 4): a control that draws in
/// <c>OnRender</c> has no children BY DESIGN, so <c>tree=0</c> cannot separate "self-drawn" from "blank".
/// <c>spike/CalendarProbe</c> re-measured with the leg that was missing - a pixel capture - and all three paint a
/// full face (39/15/30 distinct colours over a 300x300 crop, 80k+ surface pixels), and a calendar's states reach
/// pixels: a fixed month with a selected day prints <c>648</c> pixels of this library's accent token where the
/// resting month prints <c>88</c>, and clearing it returns to <c>88</c>.
///
/// Two things these tests deliberately do not claim. No text is claimed visible: the day numbers are glyphs and #50
/// still blocks glyph ink in every capture route this repository has, which is also why the <c>TextOnAccent</c>
/// alias row stays unpublished (#69) - that name paints selected-day TEXT, so on this runtime it cannot be shown to
/// move a pixel either way. And nothing here is a WinUI parity claim: the pinned commit has no <c>Calendar</c> or
/// <c>TimePicker</c> control and WinUI's <c>DatePicker</c> is a flyout over a <c>CalendarView</c>, so these facts
/// hold the framework-drawn surface honest (its token identity and its state arrival), not its geometry.
/// </summary>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraCalendarFamilyTests
{
    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraCalendarFamilyTests(AstraThemeRuntimeFixture fixture)
    {
        _fixture = fixture;
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            FluentThemeManager.ApplyAccent(null);
        });
    }

    public static TheoryData<string> Family() => new() { "Calendar", "DatePicker", "TimePicker" };

    /// <summary>
    /// Paints, and builds nothing to paint with - the two halves have to be asserted together, because either one
    /// alone was readable as the old "renders nothing" conclusion. The baseline leg says the colour count is the
    /// subject's own: an empty <c>Grid</c> of the same 300x300 box over the same host prints one flat colour.
    /// </summary>
    [Theory]
    [MemberData(nameof(Family))]
    public void A_self_drawn_family_member_prints_a_face_without_a_visual_tree(string name)
    {
        _fixture.Run(() =>
        {
            var subject = Create(name);
            var sample = PixelHarness.Render(subject, 300, 300);
            var baseline = PixelHarness.Render(new Grid { Width = 300, Height = 300 }, 300, 300);

            Assert.Multiple(
                () => Assert.Equal(0, Children(subject)),
                () => Assert.True(sample.PaintedPixels > 30_000, $"{name} printed {sample.PaintedPixels} pixels"),
                () => Assert.True(sample.Histogram.Count > 10,
                    $"{name} printed {sample.Histogram.Count} distinct colours: {sample.Top(6)}"),
                () => Assert.True(baseline.Histogram.Count <= 2,
                    $"the empty-Grid baseline is not flat ({baseline.Top(4)}), so the count above proves nothing"));
        });
    }

    /// <summary>
    /// The arrival reading for a control with no parts to write a state onto: selecting a day in a fixed month moves
    /// the accent-coloured area, and clearing it moves back. The colour is this library's own
    /// <c>AccentBrush</c> instance (<c>ThemeResources/FrameworkRetints.jalxaml</c> forwards that framework name to
    /// <c>AccentFillColorDefaultBrush</c>), so the same reading is the proof that the alias row reaches a surface no
    /// markup of ours templates - and it is the reason #69's "no surface" wording had to be corrected rather than
    /// re-asserted.
    /// </summary>
    [Fact]
    public void A_selected_day_reaches_pixels_on_a_control_that_has_no_parts_to_write_onto()
    {
        _fixture.Run(() =>
        {
            var accent = PixelHarness.PixelKey(((SolidColorBrush)Application.Current!.TryFindResource("AccentBrush")!).Color);
            var calendar = new Calendar
            {
                Width = 300,
                Height = 300,
                DisplayDate = new DateTime(2026, 9, 10),
            };
            var resting = Count(PixelHarness.Render(calendar, 300, 300), accent);

            calendar.DisplayDate = new DateTime(2026, 9, 10);
            calendar.SelectedDate = new DateTime(2026, 9, 15);
            var selected = Count(PixelHarness.Render(calendar, 300, 300), accent);

            calendar.SelectedDate = null;
            calendar.DisplayDate = new DateTime(2026, 9, 10);
            var cleared = Count(PixelHarness.Render(calendar, 300, 300), accent);

            Assert.Multiple(
                () => Assert.True(selected > resting,
                    $"selecting a day did not grow the accent area (resting={resting}, selected={selected})"),
                () => Assert.Equal(resting, cleared));
        });
    }

    /// <summary>
    /// A deviation pinned on purpose, in the shape the rest of this layer uses for "the framework does it this way":
    /// disabling the calendar moves its page and gridline colours (measured <c>#F3F3F3 → #F2F2F7</c>) but leaves the
    /// selected day at full accent - the same 648 pixels enabled and disabled. Upstream's disabled calendar mutes the
    /// selection fill, and there is no row of ours that can reach it: the value is drawn inside <c>OnRender</c> from
    /// a brush the control resolves itself, not from a template cell. Recorded as ROADMAP #76.
    /// </summary>
    [Fact]
    public void The_disabled_calendar_keeps_its_selected_day_at_full_accent()
    {
        _fixture.Run(() =>
        {
            var accent = PixelHarness.PixelKey(((SolidColorBrush)Application.Current!.TryFindResource("AccentBrush")!).Color);
            var page = PixelHarness.PixelKey(PixelHarness.LightPage);
            var calendar = new Calendar
            {
                Width = 300,
                Height = 300,
                DisplayDate = new DateTime(2026, 9, 10),
                SelectedDate = new DateTime(2026, 9, 15),
            };
            var enabled = PixelHarness.Render(calendar, 300, 300);

            calendar.IsEnabled = false;
            calendar.DisplayDate = new DateTime(2026, 9, 10);
            calendar.SelectedDate = new DateTime(2026, 9, 15);
            var disabled = PixelHarness.Render(calendar, 300, 300);

            Assert.Multiple(
                () => Assert.True(Count(enabled, accent) > 0, $"the fixture lost its selection mark ({Count(enabled, accent)} pixels)"),
                () => Assert.Equal(Count(enabled, accent), Count(disabled, accent)),
                // The rest of the face does respond to the disable, so the equal count above is not a capture that
                // never re-rendered.
                () => Assert.True(Count(disabled, page) == 0 && Count(enabled, page) > 0,
                    $"enabled page={Count(enabled, page)} disabled page={Count(disabled, page)}"));
        });
    }

    private static FrameworkElement Create(string name) => name switch
    {
        "Calendar" => new Calendar { Width = 300, Height = 300 },
        "DatePicker" => new DatePicker { Width = 300, Height = 300 },
        "TimePicker" => new TimePicker { Width = 300, Height = 300 },
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, "the family census lists three members"),
    };

    private static int Children(Visual root)
    {
        var total = 0;
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            if (VisualTreeHelper.GetChild(root, index) is Visual child)
            {
                total += 1 + Children(child);
            }
        }

        return total;
    }

    private static int Count(PixelHarness.Sample sample, uint key) => sample.Histogram.GetValueOrDefault(key, 0);
}
