using FluentJalium.Tests.Pixel;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;

namespace FluentJalium.Tests;

/// <summary>
/// The divider, on the runtime's own <see cref="Separator"/>. WinUI has no such control at the pinned commit -
/// <c>git ls-files | grep -i divider</c> is empty and no <c>.idl</c>/<c>.h</c> declares the type - so what these
/// tests can hold this layer to is the two things that commit does carry: the palette row
/// <c>DividerStrokeColorDefaultBrush</c>, whose name is transcribed verbatim, and the 1 DIP thickness every one of
/// the seven divider-role templates shares (docs/astra/audits/divider.md §2). Anything beyond that is named in the
/// audit's Known Gaps rather than asserted here.
/// </summary>
/// <remarks>
/// The style carries no template, and that is a measured choice: spike/DividerProbe mode surface found the native
/// type drawing its own line in <c>OnRender</c> and honouring <c>StrokeBrush</c>/<c>StrokeThickness</c>, and mode
/// shape found that the element upstream uses for this job - a 1 DIP <c>Rectangle</c> - prints nothing at all here.
/// </remarks>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraDividerTests
{
    /// <summary>The framework's own brand green, which no Astra token produces (adaptation/00 S1-e).</summary>
    private static readonly Color BrandEmerald = Color.FromRgb(0x20, 0x72, 0x45);

    /// <summary>
    /// The grey the native control draws with before any style exists - measured on a bare <c>Separator</c> over a
    /// white card in spike/DividerProbe mode shape. Named here so the test that the implicit style landed can tell
    /// "the token arrived" apart from "nothing happened" instead of trusting a non-null read.
    /// </summary>
    private static readonly Color FrameworkDefaultStroke = Color.FromRgb(0xA3, 0xA3, 0xA4);

    private static readonly Color White = Color.FromRgb(0xFF, 0xFF, 0xFF);

    private static readonly Color Night = Color.FromRgb(0x20, 0x20, 0x20);

    /// <summary>#0F000000 composited over white, sampled off the line rows in mode shape.</summary>
    private static readonly Color LightLine = Color.FromRgb(0xF8, 0xF8, 0xF8);

    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraDividerTests(AstraThemeRuntimeFixture fixture)
    {
        _fixture = fixture;
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            FluentThemeManager.ApplyAccent(null);
        });
    }

    [Fact]
    public void The_implicit_style_replaces_the_frameworks_own_stroke()
    {
        _fixture.Run(() =>
        {
            var divider = Mounted();

            // Read back through the element's own typed property: a dependency property's identity is tied to the
            // type that declares it, so Separator.GetValue(Control.BackgroundProperty) reads null on a control that
            // really holds a brush (adaptation/00 S1-p 7).
            Assert.Same(DividerBrush(), divider.StrokeBrush);
            Assert.NotEqual(FrameworkDefaultStroke, ((SolidColorBrush)divider.StrokeBrush!).Color);
        });
    }

    [Fact]
    public void The_row_reaches_a_brush_property_as_a_brush()
    {
        _fixture.Run(() =>
        {
            var divider = Mounted();

            // A Color value handed to a Brush property vanishes silently on this runtime (S1-o 3), so the type is
            // asserted next to the colour.
            Assert.IsType<SolidColorBrush>(divider.StrokeBrush);
            Assert.Equal(DividerBrush().Color, ((SolidColorBrush)divider.StrokeBrush!).Color);
        });
    }

    /// <summary>
    /// The style's whole setter list, read off the shipped file. The resolved <c>StrokeThickness</c> cannot carry
    /// this claim on its own - the runtime's own default is 1 too, so a property read passes with or without the
    /// style - which is exactly the vacuous-read shape adaptation/00 S1-p 7 warns about.
    /// </summary>
    [Fact]
    public void The_divider_file_carries_exactly_two_setters_and_no_template()
    {
        var markup = File.ReadAllText(Path.Combine(RepositoryRoot(), "src", "FluentJalium", "Styles", "Divider.jalxaml"));

        Assert.Equal(2, CountOccurrences(markup, "<Setter Property="));
        Assert.Contains("<Setter Property=\"StrokeBrush\" Value=\"{ThemeResource DividerStrokeColorDefaultBrush}\" />", markup, StringComparison.Ordinal);
        Assert.Contains("<Setter Property=\"StrokeThickness\" Value=\"1\" />", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("<ControlTemplate", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("<Style x:Key", markup, StringComparison.Ordinal);
    }

    [Fact]
    public void The_divider_stays_out_of_the_hit_test_path()
    {
        _fixture.Run(() =>
        {
            // Measured on a fresh instance before any style (probe mode surface): the runtime already declines
            // pointer interest for this type, and the library does not undo that to make a line clickable.
            Assert.False(new Separator().IsHitTestVisible);
        });
    }

    [Fact]
    public void Turning_the_axis_leaves_the_two_setters_alone()
    {
        _fixture.Run(() =>
        {
            var divider = Mounted();
            Assert.Equal(Jalium.UI.Controls.Orientation.Horizontal, divider.Orientation);

            divider.Orientation = Jalium.UI.Controls.Orientation.Vertical;
            Assert.Equal(Jalium.UI.Controls.Orientation.Vertical, divider.Orientation);
            Assert.Same(DividerBrush(), divider.StrokeBrush);
            Assert.Equal(1d, divider.StrokeThickness, 1);
        });
    }

    [Fact]
    public void The_horizontal_line_prints_two_rows_across_the_box()
    {
        _fixture.Run(() =>
        {
            var sample = PixelHarness.Render(Card(DividerLine()), 320, 24);

            // 600 = two rows of 300: a 1 DIP line centred in a 24 DIP box straddles a row boundary and lands half
            // on each side of it. Mode shape measured that three times over - a Border, the native stroke and the
            // token over white all printed the two rows - so the count is a consequence of the geometry, and it is
            // also the reading that says the line spans the width it was given.
            Assert.Equal(600, Inked(sample, White));
            Assert.Equal(0, sample.CountAny(BrandEmerald));
        });
    }

    [Fact]
    public void The_light_token_darkens_a_lit_card_to_its_own_composite()
    {
        _fixture.Run(() =>
        {
            var sample = PixelHarness.Render(Card(DividerLine()), 320, 24);

            // The colour is the witness that it is the 6 % black token and not the framework's grey that printed.
            Assert.Equal(600, sample.Count(LightLine));
            Assert.Equal(0, sample.Count(FrameworkDefaultStroke));
        });
    }

    [Fact]
    public void The_dark_token_brightens_a_dark_card_and_the_two_themes_disagree()
    {
        _fixture.Run(() =>
        {
            var light = PixelHarness.Render(Card(DividerLine()), 320, 24);
            var lightLine = Dominant(light, White);

            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);

            // In Dark the token is #15FFFFFF, so a white card cannot show it at all (mode shape printed 0 ink over
            // white) and the card has to be the theme's own dark.
            var dark = PixelHarness.Render(Card(DividerLine(), Night), 320, 24);
            var darkLine = Dominant(dark, Night);

            Assert.Equal(600, Inked(dark, Night));
            Assert.True(Luminance(lightLine) < Luminance(White), $"Light line #{lightLine:X6}: {light.Top(4)}");
            Assert.True(Luminance(darkLine) > Luminance(Night), $"Dark line #{darkLine:X6}: {dark.Top(4)}");
            Assert.NotEqual(lightLine, darkLine);
            Assert.Equal(0, dark.CountAny(BrandEmerald));
        });
    }

    [Fact]
    public void The_vertical_line_prints_two_columns_along_the_box()
    {
        _fixture.Run(() =>
        {
            var divider = new Separator
            {
                Orientation = Jalium.UI.Controls.Orientation.Vertical,
                Height = 100,
            };
            var sample = PixelHarness.Render(Card(divider, width: 24, height: 120), 24, 120);

            // The same 1 DIP straddling a column boundary instead of a row one: mode shape measured two columns of
            // 100 for a vertical separator at this height.
            Assert.Equal(200, Inked(sample, White));
        });
    }

    [Theory]
    [InlineData("DividerStrokeColorDefault", typeof(Color))]
    [InlineData("DividerStrokeColorDefaultBrush", typeof(SolidColorBrush))]
    public void The_consumed_rows_are_the_palettes_own_and_this_file_publishes_none(string key, Type expected)
    {
        _fixture.Run(() =>
        {
            var value = Resource(key);
            Assert.NotNull(value);
            Assert.IsType(expected, value);
        });
    }

    [Fact]
    public void Nothing_in_the_library_publishes_a_divider_only_alias()
    {
        _fixture.Run(() =>
        {
            // The five names a reader might expect from the newer control's own token block. None of them is
            // upstream at this commit, so inventing them here would be an unmarked self-authored key.
            foreach (var key in new[]
                     {
                         "DividerBackground", "DividerStroke", "DividerThickness", "DividerMargin",
                         "DividerCornerRadius",
                     })
            {
                Assert.True(Resource(key) is null,
                    $"{key} resolves to {Resource(key)?.GetType().Name ?? "?"}, which this layer does not own");
            }
        });
    }

    /// <summary>
    /// A shape guard, not a behaviour test for this control: upstream's divider-role templates are built out of a
    /// 1 DIP <c>Rectangle</c>, and this runtime prints nothing for one (mode shape: 0 ink at top, centre and bottom
    /// alignment and for a 1 DIP stroke, against 600 for the same box as a <c>Border</c>). Keeping the reading
    /// alive in a test is what makes the five such rectangles still shipped in the table and app-bar styles a
    /// measured defect rather than an opinion.
    /// </summary>
    [Fact]
    public void A_one_dip_rectangle_is_not_a_route_this_layer_can_take()
    {
        _fixture.Run(() =>
        {
            var ink = new SolidColorBrush(Color.FromRgb(0x10, 0x10, 0x10));
            var rectangle = PixelHarness.Render(
                Card(new Jalium.UI.Shapes.Rectangle { Width = 300, Height = 1, Fill = ink }), 320, 24);
            var border = PixelHarness.Render(
                Card(new Border { Width = 300, Height = 1, Background = ink }), 320, 24);

            Assert.Equal(0, Inked(rectangle, White));
            Assert.True(Inked(border, White) > 500,
                $"the same box as a Border should print; it printed {Inked(border, White)}: {border.Top(4)}");
        });
    }

    /// <summary>A separator given its width and left alone, the way a page would place one.</summary>
    private static Separator DividerLine() => new() { Width = 300 };

    /// <summary>
    /// A divider inside a shown window, which is where an implicit style is resolved: the style pipeline on this
    /// runtime never reports through <c>FrameworkElement.Style</c> (S0-l), and a detached element has no style to
    /// read at all.
    /// </summary>
    private static Separator Mounted()
    {
        var divider = new Separator();
        PixelHarness.Build(divider, 300, 24);
        return divider;
    }

    private static SolidColorBrush DividerBrush() => (SolidColorBrush)Resource("DividerStrokeColorDefaultBrush")!;

    private static Border Card(FrameworkElement child, Color? background = null, double width = 320, double height = 24) => new()
    {
        // The render target's own backdrop is black, so a lit card is what lets a 6 % line be counted at all
        // (S1-m 6).
        Background = new SolidColorBrush(background ?? White),
        Width = width,
        Height = height,
        Child = child,
    };

    /// <summary>Pixels that are not the card's own colour - the line and nothing else.</summary>
    private static int Inked(PixelHarness.Sample sample, Color background) =>
        sample.Width * sample.Height - sample.Count(background);

    /// <summary>The most common colour other than the card's, or 0 when the subject printed nothing.</summary>
    private static uint Dominant(PixelHarness.Sample sample, Color background) => sample.Histogram
        .Where(static entry => entry.Key != 0)
        .Where(entry => entry.Key != PixelHarness.PixelKey(background))
        .OrderByDescending(static entry => entry.Value)
        .Select(static entry => entry.Key)
        .FirstOrDefault();

    private static int Luminance(Color color) => color.R + color.G + color.B;

    private static int Luminance(uint key) =>
        (int)(((key >> 16) & 0xFF) + ((key >> 8) & 0xFF) + (key & 0xFF));

    private static int CountOccurrences(string text, string needle)
    {
        var count = 0;
        for (var index = text.IndexOf(needle, StringComparison.Ordinal); index >= 0; index = text.IndexOf(needle, index + 1, StringComparison.Ordinal))
        {
            count++;
        }

        return count;
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "FluentJalium.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Cannot locate the repository root.");
    }

    private static object? Resource(string key) => Application.Current?.TryFindResource(key);
}
