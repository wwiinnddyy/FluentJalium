using System.Reflection;
using FluentJalium.Themes;
using FluentJalium.Tests.Pixel;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Media;

namespace FluentJalium.Tests;

/// <summary>
/// TextBox and PasswordBox: the first pair painted entirely from transcribed upstream rows, with no glyph
/// to argue about. The readings that shaped it are in docs/astra/audits/textbox-passwordbox.md - this
/// runtime's text controls do have a selection brush and a caret brush (so
/// <c>TextControlSelectionHighlightColor</c> has a consumer) but no placeholder property at all (so the four
/// placeholder rows do not), which is the opposite of what ComboBox offers.
/// </summary>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraTextInputTests
{
    private static readonly Color SurfaceSentinel = Color.FromRgb(0xFF, 0x00, 0xFF);
    private static readonly Color AccentSentinel = Color.FromRgb(0x00, 0xFF, 0x80);

    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraTextInputTests(AstraThemeRuntimeFixture fixture)
    {
        _fixture = fixture;
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            FluentThemeManager.ApplyAccent(null);
        });
    }

    /// <summary>
    /// The transcription promise, row by row: an upstream name has to hand back the very palette object the
    /// kernel re-tints, not a copy. A copy would match on colour and still stop following the theme. The
    /// right-hand column is upstream's own target except on the three rows this runtime cannot carry
    /// verbatim, where it is what we substitute - so an unannounced change to any row fails here.
    /// </summary>
    [Theory]
    [InlineData("TextControlBackground", "ControlFillColorDefaultBrush")]
    [InlineData("TextControlBackgroundPointerOver", "ControlFillColorSecondaryBrush")]
    [InlineData("TextControlBackgroundFocused", "ControlFillColorInputActiveBrush")]
    [InlineData("TextControlBackgroundDisabled", "ControlFillColorDisabledBrush")]
    [InlineData("TextControlBorderBrush", "ControlStrokeColorDefaultBrush")]
    [InlineData("TextControlBorderBrushPointerOver", "ControlStrokeColorDefaultBrush")]
    [InlineData("TextControlBorderBrushFocused", "AccentFillColorDefaultBrush")]
    [InlineData("TextControlBorderBrushDisabled", "ControlStrokeColorDefaultBrush")]
    [InlineData("TextControlForeground", "TextFillColorPrimaryBrush")]
    [InlineData("TextControlForegroundPointerOver", "TextFillColorPrimaryBrush")]
    [InlineData("TextControlForegroundFocused", "TextFillColorPrimaryBrush")]
    [InlineData("TextControlForegroundDisabled", "TextFillColorDisabledBrush")]
    [InlineData("TextControlSelectionHighlightColor", "AccentFillColorSelectedTextBackgroundBrush")]
    public void An_upstream_text_control_row_hands_back_the_palette_instance_it_aliases(string alias, string paletteKey)
    {
        _fixture.Run(() => Assert.Same(FluentThemeManager.GetBrush(paletteKey), _fixture.Application.TryFindResource(alias)));
    }

    /// <summary>
    /// Which rows are left out is a claim about the runtime, not a preference, so it is pinned as a reading:
    /// if Jalium ever gives the text controls a placeholder property, or takes the selection brush away,
    /// this fails and the dictionary has to be re-decided instead of quietly drifting.
    /// </summary>
    [Fact]
    public void The_rows_left_out_are_left_out_because_no_property_can_read_them()
    {
        static bool Has(Type type, string property) =>
            type.GetProperty(property, BindingFlags.Public | BindingFlags.Instance) is not null;

        Assert.False(Has(typeof(TextBox), "PlaceholderText"));
        Assert.False(Has(typeof(PasswordBox), "PlaceholderText"));
        Assert.False(Has(typeof(TextBox), "Header"));
        Assert.True(Has(typeof(ComboBox), "PlaceholderText"));
        Assert.True(Has(typeof(TextBox), "SelectionBrush"));
        Assert.True(Has(typeof(PasswordBox), "SelectionBrush"));
        Assert.True(Has(typeof(TextBox), "CaretBrush"));
    }

    [Fact]
    public void The_text_box_template_carries_one_cell_per_upstream_state()
    {
        _fixture.Run(() =>
        {
            var cells = TemplateTriggers("DefaultTextBoxStyle");
            Assert.Equal(3, cells.Count);

            // Upstream's CommonStates group for a text box is Normal / Disabled / PointerOver / Focused.
            // There is no Pressed cell to port: the only Pressed story in the file belongs to the delete button.
            Assert.Equal(["IsMouseOver", "IsKeyboardFocusWithin", "IsEnabled"], cells.Select(static cell => cell.Property!.Name));
            Assert.Equal(["True", "True", "False"], cells.Select(static cell => Form(cell.Value)));

            AssertCell(cells, "IsMouseOver",
                ("OuterBorder", "Background", "TextControlBackgroundPointerOver"),
                ("OuterBorder", "BorderBrush", "TextControlBorderBrushPointerOver"),
                (null, "Foreground", "TextControlForegroundPointerOver"));

            AssertCell(cells, "IsKeyboardFocusWithin",
                ("OuterBorder", "Background", "TextControlBackgroundFocused"),
                ("OuterBorder", "BorderThickness", "TextControlBorderThemeThicknessFocused"),
                ("BottomEdge", "Background", "TextControlBorderBrushFocused"),
                (null, "Foreground", "TextControlForegroundFocused"));

            AssertCell(cells, "IsEnabled",
                ("OuterBorder", "Background", "TextControlBackgroundDisabled"),
                ("OuterBorder", "BorderBrush", "TextControlBorderBrushDisabled"),
                ("BottomEdge", "Background", "TextControlBorderBrushDisabled"),
                (null, "Foreground", "TextControlForegroundDisabled"),
                (null, "CaretBrush", "TextControlForegroundDisabled"));
        });
    }

    [Fact]
    public void The_password_box_style_reads_the_same_family_it_shares_with_the_text_box()
    {
        _fixture.Run(() =>
        {
            var cells = StyleTriggers("DefaultPasswordBoxStyle");
            Assert.Equal(["IsMouseOver", "IsKeyboardFocused", "IsEnabled"], cells.Select(static cell => cell.Property!.Name));

            // PasswordBox is drawn by the framework, so it has no bottom edge to thicken: the focused border
            // token goes on the control itself and the elevation pair stays a text-box-only detail.
            AssertCell(cells, "IsKeyboardFocused",
                (null, "Background", "TextControlBackgroundFocused"),
                (null, "BorderBrush", "TextControlBorderBrushFocused"),
                (null, "Foreground", "TextControlForegroundFocused"));
            AssertCell(cells, "IsEnabled",
                (null, "Background", "TextControlBackgroundDisabled"),
                (null, "BorderBrush", "TextControlBorderBrushDisabled"),
                (null, "Foreground", "TextControlForegroundDisabled"));
        });
    }

    /// <summary>
    /// Two readings, because one is not evidence: the setter must name the upstream metric key, and the
    /// mounted control must end up with upstream's numbers. A setter that names a key nobody declared keeps
    /// the property at its default and still leaves the dictionary green.
    /// </summary>
    [Fact]
    public void Both_text_styles_size_themselves_from_upstreams_metric_rows()
    {
        _fixture.Run(() =>
        {
            foreach (var styleKey in new[] { "DefaultTextBoxStyle", "DefaultPasswordBoxStyle" })
            {
                var style = FluentThemeManager.GetStyle(styleKey);
                Assert.Equal("TextControlThemePadding", ResourceKeyOf(Setter(style, "Padding")));
                Assert.Equal("TextControlBorderThemeThickness", ResourceKeyOf(Setter(style, "BorderThickness")));
                Assert.Equal("TextControlSelectionHighlightColor", ResourceKeyOf(Setter(style, "SelectionBrush")));
                Assert.Equal(32d, Convert.ToDouble(Value(style, "MinHeight")!));
                Assert.Equal(64d, Convert.ToDouble(Value(style, "MinWidth")!));
            }

            var box = new TextBox();
            var secret = new PasswordBox();
            PixelHarness.Build(box, 160, 32);
            PixelHarness.Build(secret, 160, 32);
            var selection = _fixture.Application.TryFindResource("TextControlSelectionHighlightColor");
            var background = _fixture.Application.TryFindResource("TextControlBackground");
            Assert.Multiple(
                () => Assert.Equal(new Thickness(10, 5, 6, 6), box.Padding),
                () => Assert.Equal(new Thickness(1), box.BorderThickness),
                () => Assert.Equal(32d, box.MinHeight),
                () => Assert.Equal(64d, box.MinWidth),
                () => Assert.Same(background, box.Background),
                () => Assert.Same(_fixture.Application.TryFindResource("TextControlForeground"), box.Foreground),
                () => Assert.Same(selection, box.SelectionBrush),
                () => Assert.Same(selection, secret.SelectionBrush),
                () => Assert.Equal(new Thickness(10, 5, 6, 6), secret.Padding),
                () => Assert.Equal(new Thickness(1), secret.BorderThickness));

            // Measured, and the reason PasswordBox claims no surface evidence: handing the native defaults
            // back to Jalium (Themes/FluentThemeManager.cs ApplyNativeThemeMode) means the framework assigns
            // its own Background here - #D9FFFFFF in Light - and a local value outranks a style setter. Our
            // state cells are style triggers too, so nothing here says what a password box paints on hover;
            // that stays an open gap rather than a claim inherited from the text box.
            Assert.NotSame(background, secret.Background);
        });
    }

    /// <summary>
    /// The focus cell is reached by really focusing the control inside the host window, not by writing a
    /// read-only state. If focus ever stops landing here, this test says so rather than passing on markup.
    /// </summary>
    [Fact]
    public void A_focused_text_box_widens_only_the_bottom_edge()
    {
        _fixture.Run(() =>
        {
            var box = new TextBox { Width = 160, Height = 32 };
            PixelHarness.Build(box, 160, 32);
            var border = (Border)AssertPart(box, "OuterBorder");
            var edge = (Border)AssertPart(box, "BottomEdge");
            Assert.Equal(new Thickness(1), border.BorderThickness);
            Assert.Same(_fixture.Application.TryFindResource("ControlStrongStrokeColorDefaultBrush"), edge.Background);

            Assert.True(box.Focus(), "Focus() refused the text box in the host window: the focused cell would have no behavioural reading.");
            PixelHarness.Settle();
            Assert.True(box.IsKeyboardFocusWithin);
            Assert.Equal(new Thickness(1, 1, 1, 2), border.BorderThickness);
            Assert.Same(_fixture.Application.TryFindResource("TextControlBackgroundFocused"), border.Background);
            Assert.Same(_fixture.Application.TryFindResource("TextControlBorderBrushFocused"), edge.Background);
        });
    }

    [Fact]
    public void A_disabled_text_box_drops_its_surfaces_to_the_disabled_rows()
    {
        _fixture.Run(() =>
        {
            // Mounted already disabled. The first version typed a string, flipped IsEnabled live and then
            // pumped frames to settle; that hit the fixture's 60-second watchdog, and which of the three
            // cost it is still unattributed - see the harness note in docs/astra/ROADMAP.md.
            var box = new TextBox { Width = 160, Height = 32, IsEnabled = false };
            PixelHarness.Build(box, 160, 32);

            var border = (Border)AssertPart(box, "OuterBorder");
            var edge = (Border)AssertPart(box, "BottomEdge");
            Assert.Same(_fixture.Application.TryFindResource("TextControlBackgroundDisabled"), border.Background);
            Assert.Same(_fixture.Application.TryFindResource("TextControlBorderBrushDisabled"), border.BorderBrush);
            Assert.Same(_fixture.Application.TryFindResource("TextControlBorderBrushDisabled"), edge.Background);

            // Measured, and not what the cell asks for: our trigger writes TextControlForegroundDisabled
            // (#5C000000) into the control's Foreground, and the reading off the mounted box comes back the
            // framework's own disabled grey (#FFAEAEB2). The native defaults we hand back to Jalium set that
            // locally, and a local value outranks a template trigger - so the disabled text colour is not
            // ours, and the key on the setter is not enough to claim it.
            Assert.NotSame(_fixture.Application.TryFindResource("TextControlForegroundDisabled"), box.Foreground);
        });
    }

    /// <summary>
    /// One capture, two sentinels: the resting surface has to come from its own token, and the accent must
    /// not be on screen at rest. The selection batch used to believe a second capture in the same fixture
    /// call was what blew its watchdog; Slider's batch proved the watchdog itself was broken
    /// (docs/astra/adaptation/06), so the focused cell now gets its own pixel claim too.
    /// </summary>
    [Fact]
    public void A_resting_text_box_paints_its_surface_token_and_nothing_accented()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.OverrideBrush("ControlFillColorDefaultBrush", SurfaceSentinel);
            FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", AccentSentinel);
            try
            {
                var sample = PixelHarness.Render(new TextBox { Width = 120, Height = 32 }, 120, 32);
                Assert.True(sample.Stable, $"capture never settled: {sample.Top(6)}");

                // A 120x32 box is 3,840 pixels; the border, the 1px bottom edge and the padding ring take a
                // few hundred of them, so a surface that only half adopted the token still fails here.
                Assert.True(sample.Count(SurfaceSentinel) > 1500, $"surface token did not reach the pixels; top={sample.Top(6)}");
                Assert.Equal(0, sample.Count(AccentSentinel));
            }
            finally
            {
                FluentThemeManager.OverrideBrush("ControlFillColorDefaultBrush", null);
                FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", null);
            }
        });
    }

    /// <summary>
    /// The focused cell as pixels rather than as a read-back: the accent bottom edge has to appear and the
    /// resting surface has to give way to its own token. Reaching this state needs a real focus, so the
    /// claim is only as good as the frame pump that let the control settle afterwards.
    /// </summary>
    [Fact]
    public void A_focused_text_box_paints_the_accent_edge_it_switches_to()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", AccentSentinel);
            FluentThemeManager.OverrideBrush("ControlFillColorInputActiveBrush", SurfaceSentinel);
            try
            {
                var box = new TextBox { Width = 120, Height = 32 };
                PixelHarness.Build(box, 120, 32);
                Assert.True(box.Focus(), "Focus() refused the text box, so the focused cell has no subject.");
                var sample = PixelHarness.Render(box, 120, 32);
                Assert.True(box.IsKeyboardFocusWithin, "the focused state did not survive the capture");
                Assert.True(sample.Stable, $"capture never settled: {sample.Top(6)}");
                Assert.True(sample.Count(AccentSentinel) > 40,
                    $"the accent edge never appeared; top={sample.Top(6)}");
                Assert.True(sample.Count(SurfaceSentinel) > 1500,
                    $"the focused surface token did not reach the pixels; top={sample.Top(6)}");
            }
            finally
            {
                FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", null);
                FluentThemeManager.OverrideBrush("ControlFillColorInputActiveBrush", null);
            }
        });
    }

    private static Setter Setter(Style style, string property) =>
        style.Setters.Cast<object>().OfType<Setter>().First(setter => NameOf(setter) == property);

    private static object? Value(Style style, string property) => Setter(style, property).Value;

    private static string? NameOf(Setter setter) => setter.Property?.Name ?? setter.PropertyName;

    private static FrameworkElement AssertPart(Visual root, string name) =>
        PixelHarness.Named(root, name) ?? throw new InvalidOperationException($"No part named {name} in the built tree.");

    private static List<Trigger> TemplateTriggers(string styleKey)
    {
        var template = FluentThemeManager.GetStyle(styleKey).Setters.Cast<object>().OfType<Setter>()
            .Select(static setter => setter.Value as ControlTemplate)
            .FirstOrDefault(static candidate => candidate is not null);
        return (template ?? throw new InvalidOperationException($"{styleKey} carries no inline template"))
            .Triggers.Cast<object>().OfType<Trigger>().ToList();
    }

    private static List<Trigger> StyleTriggers(string styleKey) =>
        FluentThemeManager.GetStyle(styleKey).Triggers.Cast<object>().OfType<Trigger>().ToList();

    private static void AssertCell(List<Trigger> cells, string condition, params (string? Part, string Property, string Key)[] wanted)
    {
        var cell = cells.FirstOrDefault(candidate => candidate.Property?.Name == condition)
            ?? throw new InvalidOperationException($"No cell for {condition}; the template carries " +
                string.Join(", ", cells.Select(static candidate => candidate.Property?.Name)));
        var setters = cell.Setters.Cast<object>().OfType<Setter>().ToList();
        foreach (var (part, property, key) in wanted)
        {
            var setter = setters.FirstOrDefault(candidate => candidate.TargetName == part && NameOf(candidate) == property)
                ?? throw new InvalidOperationException($"{condition}: no {part ?? "control"}.{property} setter; the cell carries " +
                    string.Join(", ", setters.Select(static candidate => $"{candidate.TargetName ?? "control"}.{NameOf(candidate)}")));
            Assert.Equal(key, ResourceKeyOf(setter));
        }
    }

    private static string? ResourceKeyOf(Setter setter) =>
        setter.Value?.GetType().GetProperty("ResourceKey")?.GetValue(setter.Value) as string;

    private static string Form(object? value) => value?.ToString() ?? "<null>";
}
