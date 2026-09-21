using FluentJalium.Controls;
using FluentJalium.Tests.Pixel;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;

namespace FluentJalium.Tests;

/// <summary>
/// What the badge decides for itself and what its style has to deliver. 26.10.9 has neither <c>InfoBadge</c> nor
/// <c>Badge</c> (spike/ControlCensus), so the control, the four-kind decision and the rounded pill are this layer's;
/// the numbers, the colours, the sixteen style keys and the margins are upstream's, transcribed from
/// <c>InfoBadge_themeresources.xaml</c> blob b09b56572ac8a2159bf9ba2dda7f063ec5460c6a.
/// </summary>
/// <remarks>
/// <para>
/// The assertions split along the line where a claim can be faked. That a style setter holds a number is read off
/// the property; that a trigger fired is read off the <em>margin</em> it writes, because the visibility it also
/// writes would read the same if the trigger had matched nothing and the part had been authored visible. The
/// geometry of the pill is not treated as token evidence (adaptation/00 §S1-q lesson 6: Divider's A/B printed the
/// same counts with the style deleted) - the colour claims carry that part.
/// </para>
/// <para>
/// Nothing here is driven by a pointer or a keyboard. Upstream's <c>InfoBadge.idl</c> declares no events and its
/// style sets <c>IsTabStop=False</c>; that is asserted below, and it means the hardware-input column of this
/// control's evidence stays empty rather than passing. See <c>docs/astra/audits/info-badge.md</c>.
/// </para>
/// </remarks>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraInfoBadgeTests
{
    /// <summary>The framework's own brand green, which no Astra token produces (adaptation/00 S1-e).</summary>
    private static readonly Color BrandEmerald = Color.FromRgb(0x20, 0x72, 0x45);

    /// <summary>The two glyphs upstream's Attention and Informational icon styles name (:99, :111).</summary>
    private const string AttentionGlyph = "";

    private const string InformationalGlyph = "";

    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraInfoBadgeTests(AstraThemeRuntimeFixture fixture)
    {
        _fixture = fixture;
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            FluentThemeManager.ApplyAccent(null);
        });
    }

    [Fact]
    public void The_named_style_lands_and_the_template_realizes_every_part()
    {
        _fixture.Run(() =>
        {
            var badge = Badge();

            // The four names the template declares. A ControlTemplate that parsed and silently dropped a branch
            // would still give a badge with a background, so each part is read as its own type.
            Assert.NotNull(Part(badge, "RootGrid") as Border);
            Assert.NotNull(Part(badge, "ValueTextBlock") as TextBlock);
            Assert.NotNull(Part(badge, "IconPresenter") as Viewbox);
            Assert.NotNull(Part(badge, "IconContent") as ContentPresenter);
        });
    }

    [Fact]
    public void The_default_style_carries_upstreams_seven_setters()
    {
        _fixture.Run(() =>
        {
            var badge = Badge();

            // InfoBadge_themeresources.xaml:48-54. MinHeight/MinWidth/MaxHeight are the three x:Double rows this
            // reader cannot publish, so they arrive as the literals the style writes - faithful only because all
            // three dictionaries carry the same numbers (upstream :7-9, :21-23, :35-37).
            Assert.Equal(4d, badge.MinHeight, 1);
            Assert.Equal(4d, badge.MinWidth, 1);
            Assert.Equal(16d, badge.MaxHeight, 1);
            Assert.False(badge.IsTabStop);
            Assert.Same(Resource("InfoBadgeBackground"), badge.Background);
            Assert.Same(Resource("InfoBadgeForeground"), badge.Foreground);
            Assert.Equal(new Thickness(0), badge.Padding);
        });
    }

    [Fact]
    public void The_two_alias_rows_point_where_upstream_points()
    {
        _fixture.Run(() =>
        {
            // Both are aliases in upstream's Light and Default dictionaries (:5-6, :19-20). HighContrast re-points
            // underneath the aliases rather than here, as for ProgressBar, ProgressRing and PipsPager.
            Assert.Same(Resource("TextOnAccentFillColorPrimaryBrush"), Resource("InfoBadgeForeground"));
            Assert.Same(Resource("AccentFillColorDefaultBrush"), Resource("InfoBadgeBackground"));
            Assert.IsNotType<Color>(Resource("InfoBadgeBackground")!);
        });
    }

    [Theory]
    [InlineData(-1, null, FluentInfoBadgeDisplayKind.Dot)]
    [InlineData(0, null, FluentInfoBadgeDisplayKind.Value)]
    [InlineData(12, null, FluentInfoBadgeDisplayKind.Value)]
    [InlineData(-1, "font", FluentInfoBadgeDisplayKind.FontIcon)]
    [InlineData(-1, "symbol", FluentInfoBadgeDisplayKind.Icon)]
    [InlineData(7, "font", FluentInfoBadgeDisplayKind.Value)]
    public void The_kind_follows_the_precedence_the_C_plus_plus_walks(
        int value,
        string? icon,
        FluentInfoBadgeDisplayKind expected)
    {
        _fixture.Run(() =>
        {
            // InfoBadge.cpp:59-83: a number beats an icon, a FontIcon gets its own state, the dot is what is left.
            // The last row is the one that matters - it separates "the number wins" from "whatever changed last".
            var badge = new FluentInfoBadge { Value = value, Icon = Icon(icon) };
            Mount(badge, 16, 16);
            Assert.Equal(expected, badge.DisplayKind);
        });
    }

    [Fact]
    public void A_number_below_minus_one_is_refused_and_the_old_number_stays()
    {
        _fixture.Run(() =>
        {
            // InfoBadge.cpp:46-49 throws for anything under -1. The read-back is the point: a callback that throws
            // after the value was written leaves the control reporting the number it refused unless it hands the
            // old one back.
            var badge = new FluentInfoBadge { Value = 5 };
            Assert.Throws<ArgumentOutOfRangeException>(() => badge.Value = -2);
            Assert.Equal(5, badge.Value);
            Assert.Equal(-1, new FluentInfoBadge { Value = -1 }.Value);
        });
    }

    [Fact]
    public void The_value_kind_writes_the_number_and_aims_its_margin()
    {
        _fixture.Run(() =>
        {
            var badge = Badge(42);
            var text = (TextBlock)Part(badge, "ValueTextBlock")!;

            // The trigger's other setter (Visibility) would read Visible for a part authored Visible with no trigger
            // at all, so the margin is the part of DisplayKindStates/Value that cannot be faked.
            Assert.Equal("42", text.Text);
            Assert.Equal(Visibility.Visible, text.Visibility);
            Assert.Equal((Thickness)Resource("ValueInfoBadgeTextMargin")!, text.Margin);
            Assert.Equal(Visibility.Collapsed, ((Viewbox)Part(badge, "IconPresenter")!).Visibility);
        });
    }

    [Theory]
    [InlineData("font")]
    [InlineData("symbol")]
    public void An_icon_kind_shows_the_viewbox_and_the_margin_its_state_writes(string kind)
    {
        _fixture.Run(() =>
        {
            // Upstream gives a FontIcon a different bottom margin from any other icon (:14-16), so the two states
            // are told apart by reading which Thickness row the presenter ended up wearing.
            var badge = Badge(icon: kind);
            var presenter = (Viewbox)Part(badge, "IconPresenter")!;
            var expected = kind == "font" ? "IconInfoBadgeFontIconMargin" : "IconInfoBadgeIconMargin";

            Assert.Equal(Visibility.Visible, presenter.Visibility);
            Assert.Equal((Thickness)Resource(expected)!, presenter.Margin);
            Assert.Same(badge.Icon, ((ContentPresenter)Part(badge, "IconContent")!).Content);
            Assert.Equal(Visibility.Collapsed, ((TextBlock)Part(badge, "ValueTextBlock")!).Visibility);
        });
    }

    [Fact]
    public void A_dot_leaves_both_content_parts_collapsed()
    {
        _fixture.Run(() =>
        {
            // Upstream's Dot state has no setters at all (:61): the resting template, where both parts are collapsed
            // as authored. Nothing to trigger means this is the one kind asserted by absence.
            var badge = Badge();
            Assert.Equal(Visibility.Collapsed, ((TextBlock)Part(badge, "ValueTextBlock")!).Visibility);
            Assert.Equal(Visibility.Collapsed, ((Viewbox)Part(badge, "IconPresenter")!).Visibility);
            Assert.Equal(string.Empty, ((TextBlock)Part(badge, "ValueTextBlock")!).Text);
        });
    }

    [Fact]
    public void The_pill_radius_is_half_the_measured_height()
    {
        _fixture.Run(() =>
        {
            // InfoBadge.cpp:91-98 recomputes the radius from ActualHeight on every size change. Here the number is
            // written on the control and reaches the pill through the template's TemplateBinding, because this
            // runtime's Grid has no CornerRadius to hold it.
            var badge = Badge(7);
            var root = (Border)Part(badge, "RootGrid")!;
            Assert.True(badge.ActualHeight > 0, "the badge never measured");
            Assert.Equal(badge.ActualHeight / 2, badge.CornerRadius.TopLeft, 1);
            Assert.Equal(badge.CornerRadius, root.CornerRadius);
            Assert.Equal(root.CornerRadius.TopLeft, root.CornerRadius.BottomRight, 1);
        });
    }

    [Fact]
    public void A_locally_set_radius_is_left_alone_the_way_upstream_leaves_it()
    {
        _fixture.Run(() =>
        {
            // The other half of InfoBadge.cpp:91-98: a locally-set CornerRadius wins. The control writes through
            // SetCurrentValue, which does not create a local value, so the guard is what tells the two apart.
            var badge = new FluentInfoBadge { CornerRadius = new CornerRadius(2) };
            Mount(badge, 16, 16);
            Assert.Equal(new CornerRadius(2), badge.CornerRadius);
            Assert.Equal(new CornerRadius(2), ((Border)Part(badge, "RootGrid")!).CornerRadius);
        });
    }

    [Theory]
    [InlineData("AttentionDotInfoBadgeStyle", "SystemFillColorAttentionBrush")]
    [InlineData("InformationalDotInfoBadgeStyle", "SystemFillColorSolidNeutralBrush")]
    [InlineData("SuccessDotInfoBadgeStyle", "SystemFillColorSuccessBrush")]
    [InlineData("CautionDotInfoBadgeStyle", "SystemFillColorCautionBrush")]
    [InlineData("CriticalDotInfoBadgeStyle", "SystemFillColorCriticalBrush")]
    public void Each_severity_moves_only_the_background(string styleKey, string brushKey)
    {
        _fixture.Run(() =>
        {
            var badge = new FluentInfoBadge { Style = (Style)Resource(styleKey)! };
            Mount(badge, 16, 16);
            Assert.Same(Resource(brushKey), badge.Background);

            // The rest of the frame has to survive the BasedOn: a severity style that lost the template would paint
            // a brush and nothing else, so the same colour is read off the part that carries the picture.
            Assert.Same(Resource(brushKey), ((Border)Part(badge, "RootGrid")!).Background);
            Assert.NotNull(Part(badge, "IconContent"));
        });
    }

    [Theory]
    [InlineData("AttentionIconInfoBadgeStyle", AttentionGlyph, "")]
    [InlineData("InformationalIconInfoBadgeStyle", InformationalGlyph, "")]
    [InlineData("SuccessIconInfoBadgeStyle", "", "Accept")]
    [InlineData("CautionIconInfoBadgeStyle", "", "Important")]
    [InlineData("CriticalIconInfoBadgeStyle", "", "Cancel")]
    public void Each_icon_style_carries_the_element_upstream_names(string styleKey, string glyph, string symbol)
    {
        _fixture.Run(() =>
        {
            // :95-102, :107-114, :119-125, :130-136, :141-146. Upstream spells these as FontIconSource and
            // SymbolIconSource; no IconSource type ships here, so the same five identities are read off the element
            // the style now holds directly. The Attention and Informational rows also re-write Padding (:96, :108).
            var badge = new FluentInfoBadge { Style = (Style)Resource(styleKey)! };
            Mount(badge, 16, 16);
            Assert.NotNull(badge.Icon);
            if (glyph.Length > 0)
            {
                Assert.Equal(glyph, Assert.IsType<FontIcon>(badge.Icon).Glyph);
                Assert.Equal(new Thickness(0, 4, 0, 2), badge.Padding);
                Assert.Equal(FluentInfoBadgeDisplayKind.FontIcon, badge.DisplayKind);
            }
            else
            {
                Assert.Equal(symbol, Assert.IsType<SymbolIcon>(badge.Icon).Symbol.ToString());
                Assert.Equal(FluentInfoBadgeDisplayKind.Icon, badge.DisplayKind);
            }

            Assert.Equal(Visibility.Visible, ((Viewbox)Part(badge, "IconPresenter")!).Visibility);
        });
    }

    [Fact]
    public void Two_badges_off_one_icon_style_both_reach_their_presenter()
    {
        _fixture.Run(() =>
        {
            // Upstream hands an IconSource to a style precisely so every badge can materialise its own element. This
            // layer hands the element over directly, so the five icon styles each hold one instance that two badges
            // share. Whether the runtime notices is the reading this test exists for: an element that can only have
            // one parent would leave one of the two presenters empty, or throw on the second mount.
            var style = (Style)Resource("AttentionIconInfoBadgeStyle")!;
            var first = new FluentInfoBadge { Style = style };
            var second = new FluentInfoBadge { Style = style };
            var panel = new StackPanel { Orientation = Orientation.Vertical, Width = 40, Height = 60 };
            panel.Children.Add(first);
            panel.Children.Add(second);
            PixelHarness.Build(panel, 40, 60);

            var firstContent = (Part(first, "IconContent") as ContentPresenter)?.Content;
            var secondContent = (Part(second, "IconContent") as ContentPresenter)?.Content;
            Assert.Multiple(
                () => Assert.Same(first.Icon, firstContent),
                () => Assert.Same(second.Icon, secondContent));
        });
    }

    [Fact]
    public void All_sixteen_upstream_style_keys_resolve()
    {
        _fixture.Run(() =>
        {
            var names = new List<string> { "DefaultInfoBadgeStyle" };
            foreach (var severity in new[] { "Attention", "Informational", "Success", "Caution", "Critical" })
            {
                foreach (var kind in new[] { "Dot", "Value", "Icon" })
                {
                    names.Add($"{severity}{kind}InfoBadgeStyle");
                }
            }

            Assert.Equal(16, names.Count);
            foreach (var name in names)
            {
                var style = Resource(name) as Style;
                Assert.True(style is not null, $"upstream's {name} is not published");
                Assert.Same(typeof(FluentInfoBadge), style!.TargetType);
            }
        });
    }

    [Theory]
    [InlineData("InfoBadgeMinHeight")]
    [InlineData("InfoBadgeMinWidth")]
    [InlineData("InfoBadgeMaxHeight")]
    [InlineData("InfoBadgeValueFontSize")]
    [InlineData("InfoBadgeIconHeight")]
    [InlineData("InfoBadgeIconWidth")]
    public void The_six_numeric_rows_upstream_publishes_stay_out(string key)
    {
        _fixture.Run(() =>
        {
            // Not laziness but a measured limit: spike/DoubleRowProbe found x:Double rows rejected outright and
            // clr-namespace numeric rows carrying the type's default instead of the authored number. A published row
            // that silently reads zero would be worse than none, because every consumer would look correct.
            Assert.Null(Resource(key));
        });
    }

    [Theory]
    [InlineData("InfoBadgeSeverityBackground")]
    [InlineData("InfoBadgeCornerRadius")]
    [InlineData("InfoBadgeIconFontSize")]
    [InlineData("IconInfoBadgePadding")]
    [InlineData("ValueInfoBadgeForeground")]
    public void Names_upstream_never_writes_are_not_published(string key)
    {
        _fixture.Run(() => Assert.Null(Resource(key)));
    }

    [Fact]
    public void The_dot_pills_the_accent_onto_the_picture()
    {
        _fixture.Run(() =>
        {
            var accent = ColorOf("AccentFillColorDefaultBrush");
            var sample = PixelHarness.Render(Badge(mount: false), 60, 60);
            var inked = sample.Count(accent);

            // The pill is at most 16 across, so "a few hundred pixels" is the whole claim and zero ink is the
            // failure this is aimed at (S1-m: a thing can be in the tree, sized, and print nothing).
            Assert.True(inked > 100, $"the accent pill printed {inked} pixels: {sample.Top(6)}");
            Assert.Equal(0, sample.Count(BrandEmerald));
        });
    }

    [Fact]
    public void The_number_is_laid_out_and_wears_the_badges_own_foreground()
    {
        _fixture.Run(() =>
        {
            // The picture cannot be asserted: three readings recorded in docs/astra/audits/info-badge.md §5 and in
            // adaptation/00 S1-r clause 3 measured that no text run reaches either capture path - a bare white
            // TextBlock on a grey plate prints zero white pixels through Host() as well as through Render(). So the
            // number is asserted as far as the harness can see it: laid out, visible, wearing the badge's own brush.
            // Upstream's template names no Foreground and relies on inheritance; this runtime hands the part the
            // framework default #E4000000 instead, which is why the template binds it.
            var badge = Badge(12);
            var text = (TextBlock)Part(badge, "ValueTextBlock")!;
            Assert.Equal("12", text.Text);
            Assert.Equal(Visibility.Visible, text.Visibility);
            Assert.Same(Resource("InfoBadgeForeground"), text.Foreground);
            Assert.True(text.ActualWidth > 0 && text.ActualHeight > 0,
                $"the number never measured: {text.ActualWidth}x{text.ActualHeight}");
        });
    }

    [Fact]
    public void The_informational_pill_is_a_different_colour_in_Light_than_in_Dark()
    {
        _fixture.Run(() =>
        {
            // Upstream's Informational background is SystemFillColorSolidNeutralBrush, whose own value is theme
            // dependent, so this control does have a Light/Dark difference to assert - and it lives in a token
            // rather than in the literals the style holds.
            var style = (Style)Resource("InformationalDotInfoBadgeStyle")!;

            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            var lightBrush = ColorOf("SystemFillColorSolidNeutralBrush");
            var light = PixelHarness.Render(Loose(style), 60, 60);

            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
            var darkBrush = ColorOf("SystemFillColorSolidNeutralBrush");
            var dark = PixelHarness.Render(Loose(style), 60, 60);
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);

            Assert.NotEqual(lightBrush, darkBrush);
            Assert.True(light.Count(lightBrush) > 100, $"Light printed {light.Count(lightBrush)}: {light.Top(5)}");
            Assert.True(dark.Count(darkBrush) > 100, $"Dark printed {dark.Count(darkBrush)}: {dark.Top(5)}");
        });
    }

    /// <summary>
    /// A badge with its template realized. <c>mount: false</c> hands back an unparented one, which is what the card
    /// path needs: PixelHarness.Build already gave the mounted badge a Grid parent, and re-parenting it throws
    /// "the logical child already has a parent" before a picture can be taken.
    /// </summary>
    private static FluentInfoBadge Badge(int value = -1, string? icon = null, bool mount = true)
    {
        var badge = new FluentInfoBadge { Value = value, Icon = Icon(icon) };
        if (mount)
        {
            Mount(badge, 16, 16);
        }
        else
        {
            PixelHarness.Build(Card(badge), 60, 60);
        }

        return badge;
    }

    private static IconElement? Icon(string? kind) => kind switch
    {
        "font" => new FontIcon { Glyph = AttentionGlyph },
        "symbol" => new SymbolIcon { Symbol = Symbol.Accept },
        _ => null,
    };

    private static void Mount(FrameworkElement element, int width, int height) => PixelHarness.Build(element, width, height);

    /// <summary>An unparented badge inside a lit card, sized and measured but not yet hung on a host.</summary>
    private static FluentInfoBadge Loose(Style style)
    {
        var badge = new FluentInfoBadge { Style = style };
        PixelHarness.Build(Card(badge), 60, 60);
        return badge;
    }

    private static Border Card(FrameworkElement child) => new()
    {
        // A lit plate: the host's own backdrop would otherwise be counted instead of the badge (S1-m 6).
        Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF)),
        Width = 60,
        Height = 60,
        Child = child,
    };

    private static FrameworkElement? Part(FrameworkElement root, string name) => PixelHarness.Named(root, name);

    private static object? Resource(string key) => Application.Current?.TryFindResource(key);

    private static Color ColorOf(string key) =>
        (Resource(key) as SolidColorBrush)?.Color ?? throw new InvalidOperationException($"{key} is not a solid brush");
}
