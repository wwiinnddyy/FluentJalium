using System.Reflection;
using FluentJalium.Tests.Pixel;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Markup;
using Jalium.UI.Media;

namespace FluentJalium.Tests;

/// <summary>
/// The icon family: what the pinned runtime exports, what a symbol name reaches as a codepoint, which of those
/// codepoints paint the glyph upstream paints, and what this layer publishes for them.
/// </summary>
/// <remarks>
/// <para>
/// Nothing in this file is a template test, because there is nothing to template. <c>IconElement</c> derives from
/// <c>FrameworkElement</c> and not <c>Control</c> here exactly as it does upstream, and upstream gives the family
/// no default style at all: <c>CSymbolIcon::ApplyTemplate</c> builds the icon's TextBlock in C++
/// (<c>icon.cpp:323-400</c>, blob <c>7ae8225654f1d98d0da2e0a7535c32b35384ebe6</c>). The theme layer's only real
/// contribution to the family is the font row upstream's glyph templates reach through, which is what
/// <c>ThemeResources/Icons.jalxaml</c> publishes.
/// </para>
/// <para>
/// The parity that matters is not the enum's number but the glyph it produces. Upstream stores a legacy number in
/// the enum and converts it before the text is written (<c>icon.cpp:461 ConvertSymbolValueToGlyph</c>, 197 cases,
/// one per member); the shipped enum already holds post-conversion numbers, so its members are the ones WinUI
/// paints rather than the ones its IDL declares. That is measured per name in
/// <c>docs/astra/adaptation/s2-symbol-surface-raw.txt</c> and the four names where the two disagree are pinned
/// below, because a runtime that changes one of them silently changes a picture in someone else's app.
/// </para>
/// <para>
/// What is deliberately not claimed: that any glyph reaches a pixel. A control run in the same capture as these
/// icons - a plain <c>TextBlock</c> with text and a font size - also writes no ink on either path
/// (adaptation/00 S1-r clause 3, re-measured in spike/IconFamilyProbe), so a blank icon picture here would prove
/// nothing either way. The one member of the family whose ink does reach a capture is <c>PathIcon</c> with a
/// closed geometry, and that is the shape the pixel test below uses.
/// </para>
/// </remarks>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraIconFamilyTests
{
    private static readonly Color Accent = Color.FromRgb(0x00, 0x78, 0xD4);

    private static readonly Color Paper = Color.FromRgb(0xFF, 0xFF, 0xFF);

    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraIconFamilyTests(AstraThemeRuntimeFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void The_runtime_exports_three_icons_and_none_of_the_kinds_upstream_also_has()
    {
        var controls = typeof(SymbolIcon).Assembly;
        foreach (var name in new[] { "IconElement", "SymbolIcon", "FontIcon", "PathIcon" })
        {
            Assert.NotNull(controls.GetType("Jalium.UI.Controls." + name));
        }

        // IconSource and its five children are what upstream's IconElement-derived content actually carries, and
        // BitmapIcon/ImageIcon are the two icon types an app can name in WinUI XAML. None of them exists here,
        // which is why FluentInfoBadge takes an IconElement and not an IconSource.
        foreach (var name in new[] { "BitmapIcon", "ImageIcon", "IconSource", "FontIconSource", "SymbolIconSource", "BitmapIconSource" })
        {
            Assert.Null(controls.GetType("Jalium.UI.Controls." + name));
        }
    }

    [Theory]
    [InlineData(typeof(SymbolIcon), "Symbol")]
    [InlineData(typeof(FontIcon), "Glyph", "FontFamily", "FontSize")]
    [InlineData(typeof(PathIcon), "Data")]
    [InlineData(typeof(IconElement), "Foreground")]
    public void Each_type_declares_the_members_the_audit_names_and_no_others(Type type, params string[] expected)
    {
        var declared = type.GetProperties(System.Reflection.BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Select(property => property.Name).OrderBy(name => name, StringComparer.Ordinal).ToList();
        Assert.Equal(expected.OrderBy(name => name, StringComparer.Ordinal), declared);
    }

    /// <summary>
    /// Why every template in this layer writes the icon font as a literal instead of reaching upstream's
    /// <c>SymbolThemeFontFamily</c> row: the reader cannot publish a font-family value at all. Three spellings of
    /// the element were tried and each yields a <c>FontFamily</c> whose <c>Source</c> reads back empty - the value
    /// in the markup is dropped without a word, the same shape as an <c>x:Double</c> row that reads back 0
    /// (adaptation/00 S1-r) and a negative <c>StackPanel.Spacing</c> that arranges as 0
    /// (audits/rating-control.md clause 10). A string row keeps its text but does not become a font family, so it
    /// would be a dead key. The row is therefore not published, and the absent name is asserted so that a runtime
    /// which learns to carry it turns this test red instead of leaving the literal in place forever.
    /// </summary>
    [Fact]
    public void A_font_family_row_cannot_carry_its_value_so_the_upstream_key_is_not_published()
    {
        const string markup =
            "<ResourceDictionary xmlns='http://schemas.jalium.ui/2024' " +
            "xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>" +
            "<FontFamily x:Key='Text'>Segoe Fluent Icons,Segoe MDL2 Assets</FontFamily>" +
            "<FontFamily x:Key='Source' Source='Segoe Fluent Icons,Segoe MDL2 Assets' />" +
            "<FontFamily x:Key='Family' FamilyName='Segoe Fluent Icons,Segoe MDL2 Assets' />" +
            "<x:String x:Key='String'>Segoe Fluent Icons,Segoe MDL2 Assets</x:String>" +
            "</ResourceDictionary>";
        var dictionary = (ResourceDictionary)XamlReader.Parse(markup)!;

        foreach (var key in new[] { "Text", "Source", "Family" })
        {
            Assert.True(dictionary.TryGetValue(key, out var row));
            Assert.Equal(string.Empty, Assert.IsType<FontFamily>(row).Source);
        }

        Assert.True(dictionary.TryGetValue("String", out var stringRow));
        Assert.Equal("Segoe Fluent Icons,Segoe MDL2 Assets", Assert.IsType<string>(stringRow));

        _fixture.Run(() =>
        {
            var resources = Application.Current!.Resources;
            resources.Add("AstraIconFamilyTests.StringRow", "Segoe Fluent Icons");
            try
            {
                var block = (TextBlock)XamlReader.Parse(
                    "<TextBlock xmlns='http://schemas.jalium.ui/2024' " +
                    "FontFamily='{ThemeResource AstraIconFamilyTests.StringRow}' />")!;
                Assert.NotEqual("Segoe Fluent Icons", block.FontFamily?.Source ?? string.Empty);
            }
            finally
            {
                resources.Remove("AstraIconFamilyTests.StringRow");
            }
        });

        Assert.Null(Resource("SymbolThemeFontFamily"));
    }

    [Theory]
    [InlineData(Symbol.Add, 0xE710)]
    [InlineData(Symbol.Cancel, 0xE711)]
    [InlineData(Symbol.Setting, 0xE713)]
    [InlineData(Symbol.Accept, 0xE8FB)]
    [InlineData(Symbol.Remove, 0xE738)]
    [InlineData(Symbol.GlobalNavButton, 0xE700)]
    public void A_members_number_is_the_codepoint_upstream_paints_not_the_one_its_idl_declares(Symbol symbol, int painted)
    {
        // Upstream's IDL declares Add = 0xE109 and then converts it to 0xE710 before writing the glyph
        // (controls2.idl:165 vs icon.cpp:470). The shipped enum holds the converted number, so the numbers this
        // library writes into a FontIcon.Glyph and the ones upstream's SymbolIcon paints are the same picture.
        Assert.Equal(painted, (int)symbol);
    }

    [Fact]
    public void Markup_reaches_the_enum_by_name_and_the_number_it_names_is_the_measured_one()
    {
        _fixture.Run(() =>
        {
            var icon = (SymbolIcon)XamlReader.Parse(
                "<SymbolIcon xmlns='http://schemas.jalium.ui/2024' Symbol='Add' />")!;
            Assert.Equal(Symbol.Add, icon.Symbol);
            Assert.Equal(0xE710, (int)icon.Symbol);
        });
    }

    [Fact]
    public void An_unknown_symbol_name_is_parsed_into_another_member_without_a_word()
    {
        // spike/IconFamilyProbe [2]: a name that is not in the enum neither throws nor keeps the default - the
        // reader lands on Cancel. Recorded as a gate because the same class of silent substitution is what makes
        // a typo in a glyph name invisible until someone looks at the picture.
        _fixture.Run(() =>
        {
            Assert.False(Enum.TryParse<Symbol>("NoSuchMember", out _));
            var icon = (SymbolIcon)XamlReader.Parse(
                "<SymbolIcon xmlns='http://schemas.jalium.ui/2024' Symbol='NoSuchMember' />")!;
            Assert.Equal(Symbol.Cancel, icon.Symbol);
        });
    }

    [Theory]
    [InlineData(Symbol.Add)]
    [InlineData(Symbol.Calories)]
    [InlineData(Symbol.Delete)]
    [InlineData(Symbol.GlobalNavButton)]
    public void The_symbol_icon_reports_a_constant_box_whatever_the_symbol(Symbol symbol)
    {
        // Measured in spike/IconFamilyProbe [4b]: every symbol asks for the same 20x20, while a TextBlock with the
        // same font and size asks for 22x22 and a FontIcon at FontSize 8/20/40 asks for 8/20/40. The box is what
        // this layer can assert about the icon; the ink is not reachable (see the class remarks).
        _fixture.Run(() =>
        {
            var icon = new SymbolIcon { Symbol = symbol };
            var host = new Grid();
            host.Children.Add(icon);
            PixelHarness.Build(host, 64, 64);
            Assert.Equal(20d, icon.DesiredSize.Width, 1);
            Assert.Equal(20d, icon.DesiredSize.Height, 1);
        });
    }

    [Theory]
    [InlineData(8d)]
    [InlineData(20d)]
    [InlineData(40d)]
    public void The_font_icons_box_follows_its_font_size(double size)
    {
        _fixture.Run(() =>
        {
            var icon = new FontIcon { Glyph = "\uE710", FontSize = size };
            var host = new Grid();
            host.Children.Add(icon);
            PixelHarness.Build(host, 64, 64);
            Assert.Equal(size, icon.DesiredSize.Width, 1);
            Assert.Equal(size, icon.DesiredSize.Height, 1);
        });
    }

    [Fact]
    public void A_closed_geometry_reaches_the_pixel_and_fills_the_slot_it_is_given()
    {
        // The family's one honest pixel arm, and it belongs to PathIcon: a closed figure with a foreground prints,
        // and it prints to the box it is arranged into rather than to the figure's own coordinates. Upstream's
        // PathIcon is 16x16 with a uniform stretch inside; ours has no such default, so the size an app gives is
        // the size the glyph becomes. Both halves are asserted, because a test that only counted ink would pass on
        // a control that stretched nothing.
        _fixture.Run(() =>
        {
            var square = Geometry.Parse("M 0,0 L 20,0 L 20,20 L 0,20 Z");
            var fixedSize = new PathIcon { Data = square, Foreground = new SolidColorBrush(Accent), Width = 20, Height = 20 };
            var field = new Grid { Background = new SolidColorBrush(Paper) };
            field.Children.Add(fixedSize);
            PixelHarness.Build(field, 64, 64);
            Assert.Equal(400, PixelHarness.Chrome(field).Count(Accent));

            var stretched = new PathIcon { Data = square, Foreground = new SolidColorBrush(Accent) };
            var wide = new Grid { Background = new SolidColorBrush(Paper) };
            wide.Children.Add(stretched);
            PixelHarness.Build(wide, 64, 64);
            var sample = PixelHarness.Chrome(wide);
            Assert.True(sample.Count(Accent) > 3200,
                $"a PathIcon with no size should fill the 64x64 slot, but only {sample.Count(Accent)} px printed");
        });
    }

    [Theory]
    [InlineData("Account", 0xE77B, 0xE910)]
    [InlineData("Map", 0xE826, 0xE707)]
    [InlineData("MapPin", 0xE707, 0xE7B7)]
    [InlineData("Page", 0xE7C3, 0xE729)]
    public void Four_names_paint_a_different_glyph_than_upstream_paints(string name, int ours, int upstreamPaints)
    {
        // The residue of s2-symbol-surface-raw.txt [F]: 167 of the 171 names both sides hold paint the same glyph,
        // these four do not. Nothing in this layer can fix it - the number is inside the runtime's enum and the
        // type that reads it is sealed to that enum - so it is a documented break an app has to know about, and a
        // gate in case a future runtime moves one of them (which would change a picture without changing code).
        var shipped = (int)Enum.Parse<Symbol>(name);
        Assert.Equal(ours, shipped);
        Assert.NotEqual(upstreamPaints, shipped);
    }

    [Fact]
    public void Twenty_six_upstream_names_have_no_member_here_and_fourteen_of_those_codepoints_exist_nowhere()
    {
        // Names upstream's enum declares and the shipped one does not, from the same diff. The second list is the
        // harder half: for these the glyph upstream paints is not carried by any other member either, so no name
        // in this runtime reaches that picture at all.
        string[] absent =
        [
            "Bullets", "Character", "ClosedCaption", "FourBars", "GlobalNavigationButton", "GoToToday", "MailFilled",
            "Manage", "OneBar", "OutlineStar", "Page2", "Pictures", "Placeholder", "Priority", "ReShare",
            "SetLockScreen", "SetTile", "SolidStar", "StopSlideShow", "Target", "ThreeBars", "TwoBars", "UnPin",
            "UnSyncFolder", "WebCam", "ZeroBars",
        ];
        foreach (var name in absent)
        {
            Assert.False(Enum.TryParse<Symbol>(name, out _), $"{name} is in the shipped enum; the diff is stale");
        }

        Assert.Equal(26, absent.Length);
        string[] noCarrier =
        [
            "FourBars", "Manage", "OneBar", "Placeholder", "Priority", "ReShare", "SetTile", "StopSlideShow",
            "Target", "ThreeBars", "TwoBars", "ZeroBars",
        ];
        var carried = new HashSet<int>(Enum.GetValues<Symbol>().Select(symbol => (int)symbol));
        var painted = new[] { 0xE908, 0xE912, 0xE905, 0xE18A, 0xE8D0, 0xE8EB, 0xE97B, 0xE620, 0xF5F0, 0xE907, 0xE906, 0xE904 };
        foreach (var (name, codepoint) in noCarrier.Zip(painted))
        {
            Assert.False(carried.Contains(codepoint), $"{name}'s glyph (0x{codepoint:X4}) is carried by something now");
        }
    }

    [Theory]
    [InlineData("SymbolIconFontSize")]
    [InlineData("SymbolIconForeground")]
    [InlineData("FontIconFontFamily")]
    [InlineData("IconElementForeground")]
    [InlineData("DefaultSymbolIconStyle")]
    public void No_row_is_invented_for_the_family(string key)
    {
        // Upstream declares no icon metrics of its own: the family's size is code-built and its brush is
        // inherited, so a token here would be a name this library made up. The one row that is upstream's own
        // symbol font key is asserted by name in the test above.
        Assert.Null(Resource(key));
    }

    [Fact]
    public void A_template_that_hosts_an_icon_hands_it_the_carrier_ink()
    {
        // The navigation items can hand their ink over in code because they are our type; the app bar cannot, so
        // its template sets the attached source on the element that hosts the icon (Styles/AppBar.jalxaml).
        // AppBarButton is the framework's own type, which makes this the shape every other host has to use -
        // the menu family and the tab icon host ride the same line. The reading is the value the hand-off leaves
        // on the icon and that it moves with a live theme switch; the pixels are spike/NavIconRecolor/census.sh,
        // which counted 519 pixels of frozen ink on that bar before the line existed.
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            var icon = new SymbolIcon { Symbol = Symbol.Save };
            var button = new AppBarButton { Label = "Save", Icon = icon };
            PixelHarness.Build(button, 68, 64);
            PixelHarness.Settle(60);
            AssertInk(icon, button, "light");

            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
            PixelHarness.Settle(60);
            AssertInk(icon, button, "dark");

            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            PixelHarness.Settle(6);
        });
    }

    /// <summary>
    /// The icon must carry the carrier's brush by instance. Null is the defect itself (nothing hands the ink
    /// over, so the glyph keeps whatever it last drew); a different instance is a stale or wrong source.
    /// </summary>
    private static void AssertInk(IconElement icon, Control carrier, string when)
    {
        static string Hex(Brush? brush) =>
            brush is SolidColorBrush solid ? PixelHarness.Hex(solid.Color) : brush?.GetType().Name ?? "<null>";

        Assert.True(
            ReferenceEquals(carrier.Foreground, icon.Foreground),
            $"{when}: the hosted icon carries {Hex(icon.Foreground)} while its {carrier.GetType().Name} is at " +
            $"{Hex(carrier.Foreground)} - the icon is not handed the carrier's ink, so it keeps whatever it last drew.");
    }

    private static object? Resource(string key) => Application.Current?.TryFindResource(key);
}
