using FluentJalium.Tests.Pixel;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using Xunit;

namespace FluentJalium.Tests;

/// <summary>
/// Corners and content of the popup family. The reading that started this is the second half of the right-gap
/// complaint - some of these surfaces "look like their corners are wrong" - and two faults produce that
/// sentence: a surface can ask for the wrong radius, or it can ask for the right one and still be overpainted
/// by the square child laid on top of it. The first pair of cases below separates the two, the last one is
/// what the separation turned up.
/// </summary>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraFlyoutCornerTests
{
    private static readonly Color Backdrop = Color.FromRgb(0x00, 0x00, 0xFF);
    private static readonly Color Surface = Color.FromRgb(0xFF, 0xFF, 0xFF);
    private static readonly Color Child = Color.FromRgb(0xFF, 0x00, 0x00);

    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraFlyoutCornerTests(AstraThemeRuntimeFixture fixture) => _fixture = fixture;

    /// <summary>
    /// The runtime boundary the rest of the batch turns on, measured rather than assumed: a <see cref="Border"/>
    /// rounds its own fill - the backdrop shows through at (0,0) and (2,2) and the surface colour only starts
    /// at (6,6) - but it never clips the child inside it, so that child's square corner is what a viewer sees
    /// wherever content reaches a rounded surface. A radius token cannot win against this, so a popup has to
    /// keep its rows short of the arc by its own inset. The standalone probe reads the same six pixels outside
    /// the harness (spike/RightGapProbe --open corner), which rules the rasterizer out as the cause. S0-w.
    /// </summary>
    [Fact]
    public void A_rounded_border_rounds_its_fill_but_never_clips_its_child()
    {
        _fixture.Run(() =>
        {
            var square = Round(12, withChild: true);
            var bare = Round(12, withChild: false);

            Assert.Multiple(
                () => Assert.Equal(Key(Surface), PixelHarness.PixelAt(bare, 30, 30)),
                () => Assert.Equal(Key(Backdrop), PixelHarness.PixelAt(bare, 0, 0)),
                () => Assert.Equal(Key(Backdrop), PixelHarness.PixelAt(bare, 2, 2)),
                () => Assert.Equal(Key(Surface), PixelHarness.PixelAt(bare, 6, 6)),
                () => Assert.Equal(Key(Child), PixelHarness.PixelAt(square, 0, 0)),
                () => Assert.Equal(Key(Child), PixelHarness.PixelAt(square, 2, 2)));
        });
    }

    /// <summary>
    /// The corner the boundary above leaves on the table, read off the one popup family this runtime really
    /// puts on screen. The suggestions surface asks for OverlayCornerRadius and its rows stop 5 DIP short of
    /// its edge and 4 DIP short of its top, so the arc is not overwritten: all four corner pixels come back
    /// unpainted while the middle of the surface carries the fill.
    /// </summary>
    [Fact]
    public void The_suggestions_surface_keeps_its_corner_and_its_rows_short_of_it()
    {
        _fixture.Run(() =>
        {
            var (container, _, close) = OpenList();
            try
            {
                var right = (int)container.ActualWidth - 1;
                var bottom = (int)container.ActualHeight - 1;
                const uint Unpainted = 0;

                Assert.Multiple(
                    () => Assert.Equal(Unpainted, PixelHarness.PixelAt(container, 0, 0)),
                    () => Assert.Equal(Unpainted, PixelHarness.PixelAt(container, right, 0)),
                    () => Assert.Equal(Unpainted, PixelHarness.PixelAt(container, 0, bottom)),
                    () => Assert.Equal(Unpainted, PixelHarness.PixelAt(container, right, bottom)),
                    () => Assert.NotEqual(Unpainted, PixelHarness.PixelAt(container, right / 2, bottom / 2)),
                    () => Assert.NotEqual(Unpainted, PixelHarness.PixelAt(container, 1, bottom / 2)),
                    () => Assert.True(container.ActualHeight > 24, "the list never opened"));
            }
            finally
            {
                close();
            }
        });
    }

    /// <summary>
    /// What the corner audit turned up, and the defect this batch fixes. The framework gives its suggestion
    /// rows a <see cref="ComboBoxItem"/> container and writes an accent-derived brush onto it as a local value -
    /// a property a style setter and a template trigger cannot outrank. While the row template bound that property
    /// the open list painted the brand green across a Fluent flyout (measured before the fix:
    /// #1D733C..#2B804A, ~450 px per stop against 2 020 px of surface). Now the row fills from
    /// ComboBoxItemBackground, so the green stays where it belongs - on the property, not on the pixels.
    /// </summary>
    [Fact]
    public void A_suggestion_row_pays_our_token_instead_of_the_frameworks_accent_gradient()
    {
        _fixture.Run(() =>
        {
            var (container, row, close) = OpenList();
            try
            {
                var background = DependencyProperty.FromName(row.GetType(), "Background")!;
                var local = row.ReadLocalValue(background);

                // The framework still owns the property: the local value is there, which is why this had to be
                // settled in the template rather than in a setter. Its SHAPE is not what the corner audit measured:
                // the DataGrid batch's AccentBrush retint (ThemeResources/FrameworkRetints.jalxaml) reaches this
                // derived brush too, so the value now comes back a SolidColorBrush where the audit recorded a
                // LinearGradientBrush. Both halves stay pinned - ownership because it is the constraint, type
                // because a later build that re-materialises the gradient has to explain itself.
                Assert.True(local is Brush, $"the container carries no local Background at all: {local?.GetType().Name ?? "unset"}");
                Assert.IsNotType<LinearGradientBrush>(local);

                var sample = PixelHarness.Chrome(container);
                var greens = sample.Histogram
                    .Where(static entry => GreenDominant(entry.Key))
                    .Sum(static entry => entry.Value);

                // The surface's own fill, read off the strip between its border and the first row, has to be
                // the bulk of the crop. Named from the picture rather than hardcoded so the claim survives a
                // palette move.
                var fill = PixelHarness.PixelAt(container, (int)container.ActualWidth / 2, 3);

                Assert.Multiple(
                    () => Assert.Equal(0, greens),
                    () => Assert.True(sample.Histogram.GetValueOrDefault(fill) > sample.Width * sample.Height / 3,
                        $"the surface fill did not reach the pixels; top={sample.Top(6)}"));
            }
            finally
            {
                close();
            }
        });
    }

    /// <summary>An accent leak is a pixel whose green channel carries the colour on its own.</summary>
    private static bool GreenDominant(uint key)
    {
        var r = (int)(key >> 16 & 0xFF);
        var g = (int)(key >> 8 & 0xFF);
        var b = (int)(key & 0xFF);
        return g > r + 16 && g > b + 16;
    }

    /// <summary>
    /// Opens the suggestion list and hands back its surface, its first row and the close step. The caller has
    /// to read everything before closing: the grafted surface stays in the shared host window, so a test that
    /// leaves it open hands the next one this row instead of its own - and an unparented visual samples black.
    /// </summary>
    private static (FrameworkElement Container, Control Row, Action Close) OpenList()
    {
        var box = new AutoCompleteBox { Width = 260 };
        PixelHarness.Build(box, 260, 32);
        box.ItemsSource = new[] { "Apple", "Banana", "Cherry" };
        box.Text = "b";
        PixelHarness.Settle();

        var container = PixelHarness.Named(PixelHarness.HostWindow(), "SuggestionsContainer")
            ?? throw new InvalidOperationException("The suggestion list never reached the overlay layer.");
        var host = (StackPanel)PixelHarness.Named(PixelHarness.HostWindow(), "PART_DropDownItemsHost")!;
        var row = (Control)host.Children[0];
        return (container, row, () =>
        {
            box.Text = string.Empty;
            PixelHarness.Settle(10);
        });
    }

    private static uint Key(Color color) => PixelHarness.PixelKey(color);

    private static Border Round(int radius, bool withChild)
    {
        var surface = new Border
        {
            Width = 60,
            Height = 60,
            CornerRadius = new CornerRadius(radius),
            Background = new SolidColorBrush(Surface),
        };
        if (withChild)
        {
            surface.Child = new Border { Width = 60, Height = 60, Background = new SolidColorBrush(Child) };
        }

        var backdrop = new Border
        {
            Width = 60,
            Height = 60,
            Background = new SolidColorBrush(Backdrop),
            Child = surface,
        };
        PixelHarness.Build(backdrop, 60, 60);
        return backdrop;
    }
}
