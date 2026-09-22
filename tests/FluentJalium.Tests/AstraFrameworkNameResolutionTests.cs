using System.Reflection;
using FluentJalium.Tests.Pixel;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using Xunit;

namespace FluentJalium.Tests;

/// <summary>
/// Whether a framework-owned colour name is read by NAME at call time on the NuGet 26.10.9 authority, which is the
/// precondition for the A2 retint layer (`ThemeResources/FrameworkRetints.jalxaml`) reaching any surface this library
/// cannot re-template.
///
/// The reading that motivated this file came from the sibling source tree, where the text-control families are
/// `private static readonly SolidColorBrush s_fallback = new(ThemeColors.ControlBorderFocused);` plus
/// `return TryFindResource("ControlBorderFocused") as Brush ?? s_fallback;` - so the static the IL census finds in
/// `.cctor` is the right-hand side of a null-coalescing operator, not the read path. That shape is not the authority:
/// a reference tree may be newer than the shipped assembly, and an earlier batch of this project had a source-tree
/// reading contradict 26.10.9 (the framework's `Style` property never reports a theme style on a mounted control).
/// So the shape gets measured here instead of inherited, one family per leg.
///
/// Nothing publishes `ControlBorderFocused`: the row is installed into the application dictionary inside the test and
/// removed in the same dispatcher turn, because a leftover entry would silently retint framework-drawn surfaces for
/// every later class in the shared host. Measuring with a runtime insert rather than a shipped row keeps this file
/// honest about one thing and defers the other: it proves the *lever exists*, not that a colour has been chosen for
/// it, which is a transcription decision that still needs its own upstream reading.
/// </summary>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraFrameworkNameResolutionTests : IDisposable
{
    private const string FocusedBorderName = "ControlBorderFocused";

    /// <summary>A colour no token in this library paints with, so seeing it can only mean the lookup was used.</summary>
    private static readonly Color Probe = Color.FromRgb(0x11, 0x22, 0x33);

    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraFrameworkNameResolutionTests(AstraThemeRuntimeFixture fixture)
    {
        _fixture = fixture;
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            FluentThemeManager.ApplyAccent(null);
        });
    }

    public void Dispose()
    {
        _fixture.Run(() =>
        {
            Application.Current!.Resources.Remove(FocusedBorderName);
            FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", null);
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
        });
    }

    [Fact]
    public void An_application_entry_under_the_frameworks_focused_border_name_is_what_the_family_paints_with()
    {
        _fixture.Run(() =>
        {
            foreach (var family in new[] { typeof(TextBox), typeof(PasswordBox), typeof(NumberBox), typeof(AutoCompleteBox) })
            {
                var ours = new SolidColorBrush(Probe);
                var resources = Application.Current!.Resources;
                resources[FocusedBorderName] = ours;
                Brush resolved;
                try
                {
                    resolved = ResolveFocusedBorder(family);
                }
                finally
                {
                    resources.Remove(FocusedBorderName);
                }

                Assert.True(ReferenceEquals(ours, resolved),
                    $"{family.Name} returned {Describe(resolved)} instead of the installed instance, so its focused "
                    + $"border is not reachable through the name {FocusedBorderName} on this runtime");
            }
        });
    }

    [Fact]
    public void A_published_row_cannot_be_uninstalled_at_top_level_so_the_frameworks_green_is_unreachable()
    {
        // Rewritten the day the row shipped: this fact used to read the framework's own fallback, and publishing
        // ThemeResources/FrameworkRetints.jalxaml's row made that reading unconstructible - the name arrives through a
        // dictionary the manifest merges, so removing a top-level entry leaves the row in force. That is the fact now
        // pinned: the brand-green fallback is out of reach for any application-level write, and what the resolver
        // returns is the palette accent instance itself (#FF0078D4 in this host, where ApplyAccent(null) leaves the
        // palette at the system accent - measured, not inferred). Instance identity is why the row aliases instead of
        // redefining: ApplyAccent and OverrideBrush keep moving the focused border after this.
        _fixture.Run(() =>
        {
            Application.Current!.Resources.Remove(FocusedBorderName);
            var accent = FluentThemeManager.GetBrush("AccentFillColorDefaultBrush");
            var resolved = ResolveFocusedBorder(typeof(TextBox));
            Assert.Multiple(
                () => Assert.Same(accent, resolved),
                () => Assert.NotEqual(Color.FromRgb(0x1E, 0x79, 0x3F), ((SolidColorBrush)resolved).Color),
                () => Assert.NotEqual(Color.FromRgb(0x20, 0x72, 0x45), ((SolidColorBrush)resolved).Color),
                () => Assert.Equal(Describe(accent), Describe(resolved)));
        });
    }

    [Fact]
    public void The_published_row_moves_the_frameworks_focused_border_name_onto_our_accent()
    {
        // What ThemeResources/FrameworkRetints.jalxaml actually ships for this name, read without installing anything:
        // the framework's brand-green fallback must be gone from the lookup, and the value that replaces it must be
        // the same instance ApplyAccent and OverrideBrush move - otherwise the focused border follows the accent at
        // startup and drifts off it the moment the app retints.
        _fixture.Run(() =>
        {
            Application.Current!.Resources.Remove(FocusedBorderName);
            var accent = FluentThemeManager.GetBrush("AccentFillColorDefaultBrush");
            var published = Application.Current!.TryFindResource(FocusedBorderName);
            Assert.NotNull(published);
            Assert.Same(accent, published);
            Assert.Same(accent, ResolveFocusedBorder(typeof(TextBox)));
        });
    }

    /// <summary>
    /// The published retint rows, in both variants, read as the instance the palette holds for their twin and at the
    /// colour that variant declares. The instance half is what makes ApplyAccent and OverrideBrush keep working
    /// through the framework's name; the colour half is the part that could silently break: a StaticResource alias
    /// forwards one object, so if the theme flip ever stopped re-tinting that object in place the name would freeze on
    /// the variant that happened to be active at load - which is the exact failure the gradient deviation above
    /// documents for non-solid brushes. One leg per variant, applied before the read, because a built-but-not-shown
    /// element keeps whatever it resolved at build time.
    /// </summary>
    [Theory]
    [InlineData("ControlBorderFocused", "AccentFillColorDefaultBrush", FluentThemeVariant.Light)]
    [InlineData("ControlBorderFocused", "AccentFillColorDefaultBrush", FluentThemeVariant.Dark)]
    [InlineData("SurfaceBackground", "SolidBackgroundFillColorBaseBrush", FluentThemeVariant.Light)]
    [InlineData("SurfaceBackground", "SolidBackgroundFillColorBaseBrush", FluentThemeVariant.Dark)]
    [InlineData("ControlBorder", "ControlStrokeColorDefaultBrush", FluentThemeVariant.Light)]
    [InlineData("ControlBorder", "ControlStrokeColorDefaultBrush", FluentThemeVariant.Dark)]
    public void A_retint_row_follows_the_theme_flip_to_the_variant_it_declares(string frameworkName, string twinKey, FluentThemeVariant variant)
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(variant);
            try
            {
                var twin = FluentThemeManager.GetBrush(twinKey);
                var published = Application.Current!.TryFindResource(frameworkName);
                Assert.True(ReferenceEquals(twin, published),
                    $"{frameworkName} [{variant}] is not the {twinKey} instance its row names - the alias forwarded an "
                    + "object the palette no longer uses, i.e. it froze at load");
                // Solid in both variants: a non-solid twin behind an alias is the freeze case the gradient deviation
                // in FrameworkRetints.jalxaml documents, so the type is part of the claim, not an implementation detail.
                var solid = Assert.IsType<SolidColorBrush>(published);
                Assert.Equal(Assert.IsType<SolidColorBrush>(twin).Color, solid.Color);
            }
            finally
            {
                FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            }
        });
    }

    /// <summary>
    /// The <c>TextPrimary</c> name, measured on 26.10.9 in both variants. The framework publishes <c>TextPrimary</c>
    /// as a live app-level resource that tracks <see cref="Application"/> to an opaque near-black (#FF1D1D1F light /
    /// #FFF5F5F7 dark) which is <em>not</em> our shipped WinUI-literal token <c>TextFillColorPrimaryBrush</c>
    /// (#E4000000 / #FFFFFFFF) - a distinct object and a distinct colour. So unlike <c>ControlBorderFocused</c> (whose
    /// framework projection is the wrong brand-green and which has a proven reader), aliasing here would move a
    /// name-reader <em>toward</em> 1:1, not away. The row is therefore withheld purely for lack of a demonstrated
    /// reader: the one surface we never restyle, a <see cref="Label"/>, paints a third framework default
    /// (#FF6E6E73 / #FFD1D1D6), matching neither the name nor the twin, so there is nothing the row could reach that
    /// re-templating cannot already fix. If a shipped control starts consuming the name, the row earns its evidence
    /// round; the tracked colour below is the baseline that decision revisits.
    /// </summary>
    [Fact]
    public void TextPrimary_is_a_theme_tracking_framework_name_the_layer_leaves_alone()
    {
        _fixture.Run(() =>
        {
            var application = Application.Current!;
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            try
            {
                var lightRow = ColorOf(application.TryFindResource("TextPrimary"));
                var lightTwin = ColorOf(FluentThemeManager.GetBrush("TextFillColorPrimaryBrush"));
                var lightLabel = ColorOf(InkedForeground());
                FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
                var darkRow = ColorOf(application.TryFindResource("TextPrimary"));
                var darkTwin = ColorOf(FluentThemeManager.GetBrush("TextFillColorPrimaryBrush"));
                var darkLabel = ColorOf(InkedForeground());

                Assert.Multiple(
                    // The name is occupied and already tracks the theme to the palette primary ink.
                    () => Assert.Equal(Color.FromRgb(0x1D, 0x1D, 0x1F), lightRow),
                    () => Assert.Equal(Color.FromRgb(0xF5, 0xF5, 0xF7), darkRow),
                    // It is a distinct object from the transcribed WinUI twin (#E4000000 / #FFFFFFFF), so an alias
                    // row would overwrite a value that is already our ink rather than wire up a missing lever.
                    () => Assert.NotEqual(lightRow, lightTwin),
                    () => Assert.Equal(Color.FromArgb(0xE4, 0x00, 0x00, 0x00), lightTwin),
                    () => Assert.Equal(Color.FromRgb(0xFF, 0xFF, 0xFF), darkTwin),
                    // The one surface we never restyles reads neither the name nor the twin: a third default ink.
                    () => Assert.Equal(Color.FromRgb(0x6E, 0x6E, 0x73), lightLabel),
                    () => Assert.NotEqual(lightRow, lightLabel),
                    () => Assert.NotEqual(lightTwin, lightLabel),
                    () => Assert.Equal(Color.FromRgb(0xD1, 0xD1, 0xD6), darkLabel));
            }
            finally
            {
                FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            }
        });
    }

    /// <summary>
    /// The three remaining palette-family text names the A2 layer was asked to consider, measured the same way as
    /// <c>TextPrimary</c>. All three are live framework projections (non-null, app-level) and all three are a *different
    /// object* from the shipped WinUI-literal token of the same meaning: the framework hands out opaque macOS-flavoured
    /// greys (and a constant white for <c>TextOnAccent</c>) where our palette ships translucent WinUI values that flip
    /// by variant. That divergence is why these stay candidates rather than rows: publishing <c>name → token</c> would
    /// move any name-reader *toward* 1:1, so the only thing holding the row back is the absence of a demonstrated
    /// reader we cannot already re-template — not a fidelity cost. If a shipped control starts consuming one of these
    /// names, the row is worth shipping; the measured colours below are the baseline that decision revisits.
    /// </summary>
    [Theory]
    [InlineData("TextSecondary", "TextFillColorSecondaryBrush", 0x6E, 0x6E, 0x73, 0xD1, 0xD1, 0xD6)]
    [InlineData("TextDisabled", "TextFillColorDisabledBrush", 0xAE, 0xAE, 0xB2, 0x63, 0x63, 0x66)]
    [InlineData("TextOnAccent", "TextOnAccentFillColorPrimaryBrush", 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF)]
    public void A_remaining_text_name_is_a_live_projection_but_not_our_token(
        string frameworkName, string twinKey, byte lr, byte lg, byte lb, byte dr, byte dg, byte db)
    {
        _fixture.Run(() =>
        {
            var application = Application.Current!;
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            try
            {
                var light = Assert.IsType<SolidColorBrush>(application.TryFindResource(frameworkName));
                var lightTwin = FluentThemeManager.GetBrush(twinKey);
                FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
                var dark = Assert.IsType<SolidColorBrush>(application.TryFindResource(frameworkName));
                var darkTwin = Assert.IsType<SolidColorBrush>(FluentThemeManager.GetBrush(twinKey));
                Assert.Multiple(
                    () => Assert.Equal(Color.FromRgb(lr, lg, lb), light.Color),
                    () => Assert.Equal(Color.FromRgb(dr, dg, db), dark.Color),
                    // Projection is a distinct object from our token in both variants...
                    () => Assert.NotSame(light, lightTwin),
                    // ...and in Dark at least a distinct colour too (TextOnAccent's twin goes #000000 under its white).
                    () => Assert.NotEqual(dark.Color, darkTwin.Color));
            }
            finally
            {
                FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            }
        });
    }

    // ---------- helpers ----------

    /// <summary>
    /// The framework's own per-call resolution of the focused-border brush. The method is not public, and reaching it
    /// is a test-only instrument: product code may not reflect into framework internals (AGENTS.md, enforced by
    /// AstraGateTests.Theme_kernel_stays_free_of_repair_loops_and_reflection).
    /// </summary>
    private static Brush ResolveFocusedBorder(Type family)
    {
        var control = (Control)Activator.CreateInstance(family)!;
        var resolver = family.GetMethod("ResolveFocusedBorderBrush", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException(
                $"{family.Name} has no ResolveFocusedBorderBrush on this runtime, so the source-tree reading of the "
                + "name-lookup path does not describe 26.10.9 - this family needs a different instrument");
        return Assert.IsAssignableFrom<Brush>(resolver.Invoke(control, null))
            ?? throw new InvalidOperationException($"{family.Name}.{resolver.Name}() returned null");
    }

    private static string Describe(Brush? brush) => brush switch
    {
        null => "null",
        SolidColorBrush solid => $"{brush.GetType().Name} #{solid.Color.A:X2}{solid.Color.R:X2}{solid.Color.G:X2}{solid.Color.B:X2}",
        _ => brush.GetType().Name,
    };

    private static Color ColorOf(object? brush) => Assert.IsType<SolidColorBrush>(brush).Color;

    /// <summary>The foreground a <see cref="Label"/> this library never restyles resolves to, built under whatever
    /// theme variant is currently applied (a built-but-not-shown element keeps what it resolved at build time).</summary>
    private static Brush? InkedForeground()
    {
        var label = new Label { Content = "Reading" };
        PixelHarness.Build(label, 200, 40);
        return label.Foreground;
    }
}
