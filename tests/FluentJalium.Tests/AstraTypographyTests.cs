using FluentJalium.Tests.Pixel;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Xunit;

namespace FluentJalium.Tests;

/// <summary>
/// The typography layer read against its upstream file, key by key and setter by setter:
/// `ThemeResources/Typography.jalxaml` transcribes `CommonStyles/TextBlock_themeresources.xaml` at 19e3bdc3 - ten
/// style rows, upstream's `BasedOn` chain, upstream's setter list - and this class is what makes that claim
/// falsifiable rather than a comment that says so.
///
/// Two kinds of reading appear below, and the difference matters. A *shape* reading (`A_transcribed_style_writes_
/// exactly_the_rows_upstream_writes`) looks at the hydrated `Style` object: it can see a row we added by accident and
/// a row we dropped on purpose, but a row that never reaches an element passes it unnoticed. An *arrival* reading
/// (`A_transcribed_style_lands_...`) mounts a live `TextBlock` and reads the dependency property back, which is the
/// only way to see a setter that hydrates and then lands nowhere. The two together are the whole audited claim:
/// nothing here reaches pixels, because text glyph ink is #50's unresolved gap, so a capture could not confirm any
/// of it and is not cited as if it could.
///
/// `LineStackingStrategy` is deliberately absent from the arrival list. Upstream writes `MaxHeight`, which is also
/// this host's default (measured: spike/DoubleRowProbe mode "typography"), so a read-back of `MaxHeight` cannot tell
/// an applied setter from a missing one; that row is carried for transcription fidelity and asserted only as shape.
/// </summary>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraTypographyTests
{
    /// <summary>The upstream file's seven numeric rows, named exactly as upstream names them.</summary>
    private static readonly string[] SizeRows =
    [
        "CaptionTextBlockFontSize",
        "BodyTextBlockFontSize",
        "BodyLargeTextBlockFontSize",
        "SubtitleTextBlockFontSize",
        "TitleTextBlockFontSize",
        "TitleLargeTextBlockFontSize",
        "DisplayTextBlockFontSize",
    ];

    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraTypographyTests(AstraThemeRuntimeFixture fixture)
    {
        _fixture = fixture;
        _fixture.Run(() => FluentThemeManager.ApplyTheme(FluentThemeVariant.Light));
    }

    /// <summary>
    /// Upstream's ramp, read off an element the style was applied to rather than off the style: 14 and Normal are
    /// also this host's defaults, so the sizes that prove arrival are 12/18/20/28/40/68 and the weights that prove it
    /// are the four `SemiBold` rows. `BodyStrongTextBlockStyle` carries no setter of its own upstream, which is why
    /// it is 14/SemiBold here and not a copy of `Body`.
    /// </summary>
    [Theory]
    [InlineData("BaseTextBlockStyle", 14, "SemiBold")]
    [InlineData("CaptionTextBlockStyle", 12, "Normal")]
    [InlineData("BodyTextBlockStyle", 14, "Normal")]
    [InlineData("BodyStrongTextBlockStyle", 14, "SemiBold")]
    [InlineData("BodyLargeTextBlockStyle", 18, "Normal")]
    [InlineData("BodyLargeStrongTextBlockStyle", 18, "SemiBold")]
    [InlineData("SubtitleTextBlockStyle", 20, "SemiBold")]
    [InlineData("TitleTextBlockStyle", 28, "SemiBold")]
    [InlineData("TitleLargeTextBlockStyle", 40, "SemiBold")]
    [InlineData("DisplayTextBlockStyle", 68, "SemiBold")]
    public void A_transcribed_style_lands_upstreams_size_and_weight_on_a_live_block(string key, double size, string weight)
    {
        _fixture.Run(() =>
        {
            var block = Mount(new TextBlock { Text = "ramp", Style = FluentThemeManager.GetStyle(key) });
            var expected = weight == "SemiBold" ? FontWeights.SemiBold : FontWeights.Normal;

            Assert.Multiple(
                () => Assert.Equal(size, block.FontSize),
                () => Assert.Equal(expected, block.FontWeight),
                // Both differ from this host's default (None / NoWrap), so each one can only read as it does because
                // upstream's row arrived on the element.
                () => Assert.Equal("CharacterEllipsis", block.TextTrimming.ToString()),
                () => Assert.Equal("Wrap", block.TextWrapping.ToString()));
        });
    }

    /// <summary>
    /// The exact property list upstream writes, minus the two members this runtime does not have. This is the row
    /// that keeps the transcription honest in both directions: it goes red if a `Foreground` row like the one the
    /// pre-Astra file added on `Body` and `Caption` comes back, and it goes red if a row upstream has is quietly
    /// dropped without being recorded as a drop.
    /// </summary>
    [Theory]
    [InlineData("BaseTextBlockStyle", "FontSize,FontWeight,TextTrimming,TextWrapping,LineStackingStrategy")]
    [InlineData("CaptionTextBlockStyle", "FontSize,FontWeight")]
    [InlineData("BodyTextBlockStyle", "FontWeight")]
    [InlineData("BodyStrongTextBlockStyle", "")]
    [InlineData("BodyLargeTextBlockStyle", "FontWeight,FontSize")]
    [InlineData("BodyLargeStrongTextBlockStyle", "FontSize")]
    [InlineData("SubtitleTextBlockStyle", "FontSize")]
    [InlineData("TitleTextBlockStyle", "FontSize")]
    [InlineData("TitleLargeTextBlockStyle", "FontSize")]
    [InlineData("DisplayTextBlockStyle", "FontSize")]
    public void A_transcribed_style_writes_exactly_the_rows_upstream_writes(string key, string written)
    {
        _fixture.Run(() =>
        {
            var setters = FluentThemeManager.GetStyle(key).Setters
                .Cast<object>().OfType<Setter>()
                .Select(static setter => setter.Property?.Name ?? setter.PropertyName ?? "UNRESOLVED")
                .ToArray();
            var expected = written.Length == 0 ? [] : written.Split(',');

            Assert.Equal(expected, setters);
        });
    }

    /// <summary>
    /// Why the two dropped rows are not a choice: this runtime's `TextBlock` has neither member, and a `Setter`
    /// naming one fails the whole dictionary at load ("Setter.Property 'TextLineBounds' cannot be resolved for the
    /// style target type"), so a transcription that kept them would not ship, it would take the layer down with it.
    /// </summary>
    [Theory]
    [InlineData("TextLineBounds")]
    [InlineData("OpticalMarginAlignment")]
    public void A_dropped_row_names_a_member_this_runtime_does_not_have(string property)
    {
        _fixture.Run(() => Assert.Null(typeof(TextBlock).GetProperty(property)));
    }

    /// <summary>
    /// The pre-Astra file pinned an ink onto `Body` and `Caption`; upstream writes none, so the rows are gone, and
    /// this is the fact that makes dropping them safe to state rather than merely convenient: a block whose style
    /// writes no `Foreground` still carries this library's primary token instance, because the host resolves the name
    /// `TextPrimary` and the alias layer makes that name our token. The deleted `Caption` row carried the secondary
    /// token, so the second leg is the visible half - caption text is primary ink now, which is what upstream's
    /// caption is. Read at the dependency-property level only; #50 still blocks any pixel claim about text.
    /// </summary>
    [Fact]
    public void A_block_whose_style_writes_no_ink_still_carries_our_primary_token()
    {
        _fixture.Run(() =>
        {
            var primary = FluentThemeManager.GetBrush("TextFillColorPrimaryBrush");
            var block = Mount(new TextBlock { Text = "caption", Style = FluentThemeManager.GetStyle("CaptionTextBlockStyle") });

            Assert.Multiple(
                () => Assert.NotSame(FluentThemeManager.GetBrush("TextFillColorSecondaryBrush"), block.Foreground),
                () => Assert.Same(primary, block.Foreground),
                () => Assert.Same(Application.Current!.TryFindResource("TextPrimary"), block.Foreground));
        });
    }

    /// <summary>
    /// The one part of upstream's file this layer cannot transcribe: the seven numeric rows the sizes come from. They
    /// are not withheld for lack of a reader - the styles read them - but because this markup reader publishes a
    /// number as zero. See <see cref="A_numeric_row_written_in_markup_loses_its_number_while_one_written_in_code_does_not" />.
    /// </summary>
    [Theory]
    [MemberData(nameof(SizeRowsData))]
    public void An_upstream_size_row_is_not_published(string key)
    {
        _fixture.Run(() => Assert.Null(Application.Current!.TryFindResource(key)));
    }

    public static TheoryData<string> SizeRowsData() => new(SizeRows);

    /// <summary>
    /// The measured shape of the limit, pinned so the transcription decision stays revisitable on the same evidence
    /// instead of on a comment. A `sys:Double` row parses - and then reads back as zero, in both assembly spellings
    /// and in both dictionary positions, which is why nothing may publish one: a row that carries 0 is worse than no
    /// row, since every consumer still looks correct. The route behind it is alive: the same key holding a `Double`
    /// published from code reaches `FontSize` through `{ThemeResource}`. So the gap is the reader, not the pipeline,
    /// and a future layer that seeds numerics from code could publish these names and repoint this file at them.
    /// </summary>
    [Fact]
    public void A_numeric_row_written_in_markup_loses_its_number_while_one_written_in_code_does_not()
    {
        const string key = "AstraTypographyTests.SizeRow";
        var dictionary = (ResourceDictionary)XamlReader.Parse(
            "<ResourceDictionary xmlns='http://schemas.jalium.ui/2024' " +
            "xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' " +
            "xmlns:sys='clr-namespace:System;assembly=System.Private.CoreLib'>" +
            $"<sys:Double x:Key='{key}'>18</sys:Double>" +
            "</ResourceDictionary>")!;

        // The markup leg, read straight off the parsed dictionary: no merge, no theme, nothing to blame but the row.
        Assert.True(dictionary.TryGetValue(key, out var row));
        Assert.Equal(0d, Assert.IsType<double>(row));

        var resources = Application.Current!.Resources;
        _fixture.Run(() =>
        {
            resources[key] = 18d;
            try
            {
                var styled = (ResourceDictionary)XamlReader.Parse(
                    "<ResourceDictionary xmlns='http://schemas.jalium.ui/2024' " +
                    "xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>" +
                    "<Style x:Key='S' TargetType='TextBlock'>" +
                    $"<Setter Property='FontSize' Value='{{ThemeResource {key}}}' /></Style>" +
                    "</ResourceDictionary>")!;
                var merged = Application.Current!.Resources.MergedDictionaries;
                merged.Add(styled);
                try
                {
                    var block = Mount(new TextBlock { Text = "ramp", Style = (Style)styled["S"]! });
                    Assert.Equal(18d, block.FontSize);
                }
                finally
                {
                    merged.RemoveAt(merged.Count - 1);
                }
            }
            finally
            {
                resources.Remove(key);
            }
        });
    }

    /// <summary>
    /// The framework's own type scale, read at application scope: a template this library does not own asks for a size
    /// by name (<c>{ThemeResource CaptionFontSize}</c> in the shipped DataGrid and TitleBar builders, #12's census), and
    /// the name is projected by <c>ThemeManager.ApplyTypography</c>, which <see cref="FluentThemeManager" /> calls with
    /// upstream's body size during install. The two upstream numbers are 14 and 12
    /// (microsoft-ui-xaml @19e3bdc3, dxaml/xcp/dxaml/themes/generic.xaml:13280 and :13283); the runtime's own default
    /// projection was 12/10/8. <c>SmallFontSize</c> is the framework's derivation (body minus four) and has no upstream
    /// row to answer to - it is named here because the driver moves it, so an unlisted drift would be invisible.
    /// </summary>
    [Theory]
    [InlineData("BodyFontSize", 14d)]
    [InlineData("CaptionFontSize", 12d)]
    [InlineData("SmallFontSize", 10d)]
    public void The_framework_type_scale_projects_from_upstreams_body_size(string key, double expected)
    {
        _fixture.Run(() => Assert.Equal(expected, Assert.IsType<double>(Application.Current!.TryFindResource(key))));
    }

    /// <summary>
    /// The same claim as an arrival reading, because a dictionary row is not what a template consumes: a mounted
    /// <c>TextBlock</c> whose only size row is <c>{ThemeResource BodyFontSize}</c> reads 14. The second leg is the
    /// invariant this batch was actually for - the projection and this host's own <c>TextBlock</c> default, which
    /// disagreed by two before (14 against 12), now agree, so a size named and a size inherited are the same number.
    /// Read at the dependency-property level only; #50 still blocks any pixel claim about text.
    /// </summary>
    [Fact]
    public void A_size_asked_for_by_name_arrives_as_upstreams_number_and_matches_the_inherited_one()
    {
        var styled = (ResourceDictionary)XamlReader.Parse(
            "<ResourceDictionary xmlns='http://schemas.jalium.ui/2024' " +
            "xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>" +
            "<Style x:Key='S' TargetType='TextBlock'>" +
            "<Setter Property='FontSize' Value='{ThemeResource BodyFontSize}' /></Style>" +
            "</ResourceDictionary>")!;

        _fixture.Run(() =>
        {
            var merged = Application.Current!.Resources.MergedDictionaries;
            merged.Add(styled);
            try
            {
                var named = Mount(new TextBlock { Text = "named", Style = (Style)styled["S"]! });
                var inherited = Mount(new TextBlock { Text = "inherited" });

                Assert.Multiple(
                    () => Assert.Equal(14d, named.FontSize),
                    () => Assert.Equal(14d, inherited.FontSize),
                    () => Assert.Equal(inherited.FontSize, named.FontSize));
            }
            finally
            {
                merged.RemoveAt(merged.Count - 1);
            }
        });
    }

    private static TextBlock Mount(TextBlock block)
    {
        PixelHarness.Build(block, 240, 40);
        PixelHarness.Settle(20);
        return block;
    }
}
