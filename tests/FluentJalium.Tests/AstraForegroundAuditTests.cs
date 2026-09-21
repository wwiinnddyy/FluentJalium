using System.Xml;
using System.Xml.Linq;
using FluentJalium.Controls;
using FluentJalium.Themes;
using FluentJalium.Tests.Pixel;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;

namespace FluentJalium.Tests;

/// <summary>
/// Whether a colour this layer writes on text actually arrives, measured across the whole library rather than one
/// control at a time.
/// </summary>
/// <remarks>
/// <para>
/// Stage 6 found a <see cref="FluentInfoBadge"/> value label wearing the framework's default dark ink while its
/// control read white, and left the rest of the library unmeasured (<c>adaptation/00</c> clause 4 recorded the
/// un-audited neighbourhood). <c>spike/ForegroundProbe</c> closed that measurement on 2026-09-21, and two of its
/// first readings turned out to be instrument errors that would each have produced a false defect: judging a part by
/// <c>ReadLocalValue</c> calls a markup write "unset", because a value applied from markup is not a local value; and
/// printing a colour without its alpha turns upstream's 61-percent-black unread star into plain black. The claim
/// list therefore comes from the markup, and the proof is either the colour the named key resolves to or the colour
/// the node holds after the theme flips underneath it.
/// </para>
/// <para>
/// Two shapes carry a foreground here, and only one of them had ever been counted: 15 attribute rows
/// (<c>Foreground="{ThemeResource …}"</c>) against 228 <c>Setter Property="Foreground"</c> rows in styles and
/// template triggers. An unknown key inside either shape is silent - the build stays green and the text falls back
/// to inherited ink - so the resolution gate walks both, in Light and in Dark.
/// </para>
/// <para>
/// What the same measurement pinned as correct, so a later edit that breaks it is a regression and not a surprise:
/// the InfoBar's severity mark reads <c>#FFFFFF</c> on the <c>#0078D4</c> disc its own sibling
/// <c>Ellipse.IconBackground</c> paints (4.53 in Light, 11.67 in Dark), and the rating control's unread stars read
/// <c>#9E000000</c> in Light and <c>#C5FFFFFF</c> in Dark, which is upstream's <c>RatingUnreadStarColor</c> pair.
/// </para>
/// <para>
/// The A/B that gave this class teeth also measured its sensitivity limit: renaming the key on
/// <c>Styles/Surfaces.jalxaml:170</c> to a key no dictionary publishes turned the two resolution rows and the
/// inventory row red, while <see cref="A_named_claim_reaches_the_part_it_names"/> stayed green - the InfoBar title's
/// token and its inherited ink are the same colour, so reading the part cannot notice. The resolution row is the
/// load-bearing gate; reading the part only catches a dead write where the token differs from what would inherit.
/// </para>
/// </remarks>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraForegroundAuditTests : IDisposable
{
    /// <summary>The nine keys the attribute shape names, as measured on 2026-09-21.</summary>
    private static readonly string[] AttributeKeys =
    [
        "BreadcrumbBarNormalForegroundBrush",
        "ComboBoxDropDownForeground",
        "InfoBarInformationalSeverityIconForeground",
        "InfoBarMessageForeground",
        "InfoBarTitleForeground",
        "RatingControlUnselectedForeground",
        "TabViewItemIconForeground",
        "TeachingTipSubtitleForegroundBrush",
        "TeachingTipTitleForegroundBrush",
    ];

    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraForegroundAuditTests(AstraThemeRuntimeFixture fixture)
    {
        _fixture = fixture;
        SetTheme(FluentThemeVariant.Light);
    }

    public void Dispose() => SetTheme(FluentThemeVariant.Light);

    [Theory]
    [InlineData("Light")]
    [InlineData("Dark")]
    public void Every_foreground_claim_names_a_key_that_resolves_to_a_brush(string theme)
    {
        SetTheme(theme == "Dark" ? FluentThemeVariant.Dark : FluentThemeVariant.Light);

        var claims = ForegroundClaims();

        // The scope floors are what make this a library gate rather than a per-control one: the setter shape carries
        // most of the surface, and a walk that quietly found only the attribute rows would still report nothing
        // unresolved. A typo'd key belongs to no dictionary and lands as inherited ink, which a build never sees.
        Assert.True(claims.Count >= 200, $"only {claims.Count} foreground claims were found; the walk lost a shape");
        Assert.True(claims.Count(claim => claim.Shape == "setter") >= 150,
            $"only {claims.Count(claim => claim.Shape == "setter")} setter claims were found");

        List<string>? offenders = null;
        _fixture.Run(() => offenders = claims
            .Where(claim => claim.Key is not null && _fixture.Application.TryFindResource(claim.Key) is not SolidColorBrush)
            .Select(claim => $"{claim.File}:{claim.Line} {claim.Shape} {claim.Key}")
            .ToList());
        Assert.Empty(offenders!);
    }

    [Fact]
    public void The_attribute_shape_names_exactly_the_keys_the_layer_publishes()
    {
        var attributes = ForegroundClaims().Where(claim => claim.Shape == "attribute").ToList();

        // The third shape is a control's own ink carried into its template; it names no key, so it is counted here
        // rather than dropped. Six rows is what the library wrote on 2026-09-21.
        Assert.Equal(6, attributes.Count(claim => claim.Key is null && claim.Raw.Contains("TemplateBinding", StringComparison.Ordinal)));
        Assert.Equal(AttributeKeys, attributes.Where(claim => claim.Key is not null)
            .Select(claim => claim.Key)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(key => key, StringComparer.Ordinal)
            .ToArray());
    }

    [Theory]
    [InlineData("Light")]
    [InlineData("Dark")]
    public void A_named_claim_reaches_the_part_it_names(string theme)
    {
        SetTheme(theme == "Dark" ? FluentThemeVariant.Dark : FluentThemeVariant.Light);

        _fixture.Run(() =>
        {
            var bar = InfoBar();

            // Each reading names the part's own Foreground, so a colour that only looks right because something
            // inherited it cannot pass as the row the markup claims to read.
            Assert.Equal(Brush("InfoBarTitleForeground"), Ink(Part(bar, "Title")));
            Assert.Equal(Brush("InfoBarMessageForeground"), Ink(Part(bar, "Message")));
            Assert.Equal(Brush("InfoBarInformationalSeverityIconForeground"), Ink(Part(bar, "IconGlyph")));
            Assert.Equal(Brush("TextFillColorPrimaryBrush"), Ink(Part(bar, "Title")));
        });
    }

    [Theory]
    [InlineData("infoBarTitle")]
    [InlineData("button")]
    [InlineData("checkBox")]
    [InlineData("listBoxItem")]
    [InlineData("treeViewItem")]
    [InlineData("ratingUnsetBackground")]
    public void Text_wearing_a_token_changes_when_the_theme_does(string subject)
    {
        SetTheme(FluentThemeVariant.Light);
        Color light = default;
        _fixture.Run(() => light = Ink(Build(subject)));

        SetTheme(FluentThemeVariant.Dark);
        Color dark = default;
        _fixture.Run(() => dark = Ink(Build(subject)));

        Assert.NotEqual(light, dark);
    }

    [Fact]
    public void The_unread_rating_star_keeps_its_alpha_not_just_its_hue()
    {
        // Upstream's unread star is 61 percent black in Light and 77 percent white in Dark. Losing the alpha turns
        // that into plain black or plain white text - the same visual defect by a different route.
        SetTheme(FluentThemeVariant.Light);
        Color light = default;
        _fixture.Run(() => light = Ink(Build("ratingUnsetBackground")));

        SetTheme(FluentThemeVariant.Dark);
        Color dark = default;
        _fixture.Run(() => dark = Ink(Build("ratingUnsetBackground")));

        Assert.Equal(0x9E, light.A);
        Assert.Equal(0xC5, dark.A);
    }

    [Theory]
    [InlineData("Light")]
    [InlineData("Dark")]
    public void A_severity_mark_lands_on_the_disc_that_paints_behind_it(string theme)
    {
        SetTheme(theme == "Dark" ? FluentThemeVariant.Dark : FluentThemeVariant.Light);

        _fixture.Run(() =>
        {
            var bar = InfoBar();
            var glyph = Ink(Part(bar, "IconGlyph"));
            var disc = ((Part(bar, "IconBackground") as Jalium.UI.Shapes.Shape)?.Fill as SolidColorBrush)?.Color
                ?? throw new InvalidOperationException("the severity disc carries no solid fill");

            // The disc is a sibling rather than an ancestor, so this pairing has to be asserted by part name: the
            // mark is legible only against the Ellipse behind it, which the probe measured at 4.53 and 11.67 and a
            // recoloured token would silently undo.
            Assert.True(Contrast(glyph, disc) >= 4.0, $"mark {Hex(glyph)} on disc {Hex(disc)}");
        });
    }

    private readonly record struct Claim(string File, int Line, string Shape, string Raw, string? Key);

    /// <summary>Both shapes this layer writes a foreground in, with the key each names.</summary>
    private static List<Claim> ForegroundClaims()
    {
        var claims = new List<Claim>();
        foreach (var file in Directory.EnumerateFiles(
            System.IO.Path.Combine(RepositoryRoot(), "src", "FluentJalium"), "*.jalxaml", SearchOption.AllDirectories))
        {
            var document = XDocument.Load(file, LoadOptions.SetLineInfo);
            var relative = System.IO.Path.GetRelativePath(RepositoryRoot(), file).Replace('\\', '/');
            foreach (var element in document.Descendants())
            {
                var isSetter = element.Name.LocalName == "Setter";
                if (isSetter && (string?)element.Attribute("Property") != "Foreground") continue;

                var raw = isSetter ? (string?)element.Attribute("Value") : (string?)element.Attribute("Foreground");
                if (raw is null) continue;

                var text = raw.Trim();
                var key = text.StartsWith("{ThemeResource ", StringComparison.Ordinal) ? text[15..].TrimEnd('}')
                    : text.StartsWith("{StaticResource ", StringComparison.Ordinal) ? text[17..].TrimEnd('}')
                    : null;
                claims.Add(new Claim(relative, ((IXmlLineInfo)element).LineNumber, isSetter ? "setter" : "attribute", raw, key));
            }
        }

        return claims;
    }

    private FrameworkElement Build(string subject)
    {
        FrameworkElement element = subject switch
        {
            "infoBarTitle" => new FluentInfoBar { Title = "title", Message = "message" },
            "button" => new Button { Content = "label" },
            "checkBox" => new CheckBox { Content = "label" },
            "listBoxItem" => new ListBoxItem { Content = "one" },
            "treeViewItem" => new TreeViewItem { Header = "one" },
            "ratingUnsetBackground" => new FluentRatingControl { Value = 2 },
            _ => throw new ArgumentOutOfRangeException(nameof(subject), subject, "no such subject row"),
        };

        PixelHarness.Build(element, 400, 120);
        return subject switch
        {
            "infoBarTitle" => Part(element, "Title"),
            "ratingUnsetBackground" => UnreadStar(element),
            _ => element,
        };
    }

    private FrameworkElement InfoBar()
    {
        var bar = new FluentInfoBar { Title = "title", Message = "message" };
        PixelHarness.Build(bar, 400, 120);
        return bar;
    }

    /// <summary>The first star the background row draws, which is where the unselected token lands.</summary>
    private static TextBlock UnreadStar(FrameworkElement rating)
    {
        var panel = Descendants(rating).OfType<StackPanel>().First(stack => stack.Name == "RatingBackgroundStackPanel");
        return Descendants(panel).OfType<TextBlock>().First(block => !string.IsNullOrEmpty(block.Text));
    }

    private static Color Ink(FrameworkElement node) =>
        (node.GetValue(TextBlock.ForegroundProperty) as SolidColorBrush)?.Color
        ?? throw new InvalidOperationException($"{node.GetType().Name} {node.Name} wears no solid ink");

    private Color Brush(string key) =>
        (_fixture.Application.TryFindResource(key) as SolidColorBrush)?.Color
        ?? throw new InvalidOperationException($"{key} resolved to no solid brush");

    private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            if (VisualTreeHelper.GetChild(root, index) is not { } child) continue;
            yield return child;
            foreach (var deeper in Descendants(child)) yield return deeper;
        }
    }

    private static FrameworkElement Part(FrameworkElement root, string name) =>
        Descendants(root).OfType<FrameworkElement>().FirstOrDefault(element => element.Name == name)
        ?? throw new InvalidOperationException($"no part named {name} under {root.GetType().Name}");

    private static string Hex(Color color) => $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";

    private static double Contrast(Color ink, Color surface)
    {
        static double Channel(double value) => value <= 0.04045 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);
        static double Luminance(Color color) =>
            0.2126 * Channel(color.R / 255d) + 0.7152 * Channel(color.G / 255d) + 0.0722 * Channel(color.B / 255d);

        var a = Luminance(ink);
        var b = Luminance(surface);
        return (Math.Max(a, b) + 0.05) / (Math.Min(a, b) + 0.05);
    }

    private void SetTheme(FluentThemeVariant variant) => _fixture.Run(() =>
    {
        FluentThemeManager.ApplyTheme(variant);
        FluentThemeManager.ApplyAccent(null);
    });

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(System.IO.Path.Combine(directory.FullName, "FluentJalium.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Cannot locate the repository root.");
    }
}
