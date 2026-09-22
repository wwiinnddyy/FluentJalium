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
    [InlineData("TextSecondary", "TextFillColorSecondaryBrush", FluentThemeVariant.Light)]
    [InlineData("TextSecondary", "TextFillColorSecondaryBrush", FluentThemeVariant.Dark)]
    [InlineData("TextPrimary", "TextFillColorPrimaryBrush", FluentThemeVariant.Light)]
    [InlineData("TextPrimary", "TextFillColorPrimaryBrush", FluentThemeVariant.Dark)]
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
    /// What <c>ThemeResources/FrameworkRetints.jalxaml</c> now ships for <c>TextPrimary</c>, read without installing
    /// anything: the name resolves to the palette instance itself in both variants, so the framework's own opaque
    /// near-black (measured pre-row at #FF1D1D1F light / #FFF5F5F7 dark, recorded in docs/astra/ROADMAP.md #12 - that
    /// reading is no longer reachable through the lookup, which is why it lives in the ledger and not in an
    /// assertion) is gone. Instance identity is the half that could silently break: if the alias forwarded an object
    /// the palette no longer uses, the ink would still follow the theme and quietly drift off ApplyAccent and
    /// OverrideBrush. The second half is the finding that earned the row - the self-drawn <see cref="MenuItem"/>'s
    /// own resolver hands back that same instance, so the row reaches a surface no template of ours can replace.
    /// This is not a pixel claim; <see cref="A_self_drawn_menu_item_reads_the_primary_text_name" /> carries that limit.
    /// </summary>
    [Fact]
    public void The_published_row_moves_the_frameworks_primary_text_name_onto_our_ink()
    {
        _fixture.Run(() =>
        {
            var application = Application.Current!;
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            try
            {
                var lightTwin = Assert.IsType<SolidColorBrush>(FluentThemeManager.GetBrush("TextFillColorPrimaryBrush"));
                var lightPublished = application.TryFindResource("TextPrimary");
                var lightMenuItem = ResolveMenuItemPrimaryText(new MenuItem());
                // Colour, not brush: the palette re-tints this one instance in place, so holding the object across the
                // flip holds whatever variant is active when the assertion finally reads it. Assert.Multiple defers
                // every lambda below to the end of the block, which is after Dark has been applied.
                var lightColour = lightTwin.Color;
                FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
                var darkTwin = Assert.IsType<SolidColorBrush>(FluentThemeManager.GetBrush("TextFillColorPrimaryBrush"));
                var darkPublished = application.TryFindResource("TextPrimary");
                var darkColour = darkTwin.Color;
                var labelColour = ColorOf(InkedForeground());

                Assert.Multiple(
                    () => Assert.Same(lightTwin, lightPublished),
                    () => Assert.Same(darkTwin, darkPublished),
                    // One instance for both variants, which is what the alias forwards and what the in-place
                    // re-tint makes observable: the light handle IS the dark brush.
                    () => Assert.Same(lightTwin, darkTwin),
                    () => Assert.Equal(Color.FromArgb(0xE4, 0x00, 0x00, 0x00), lightColour),
                    () => Assert.Equal(Color.FromRgb(0xFF, 0xFF, 0xFF), darkColour),
                    // The framework's own reader hands back the forwarded instance, not a copy of its old fallback.
                    () => Assert.Same(lightTwin, lightMenuItem),
                    // A native Label still reads the *secondary* token, so the two names stay distinguishable.
                    () => Assert.NotEqual(labelColour, lightColour));
            }
            finally
            {
                FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            }
        });
    }

    /// <summary>
    /// The palette-family text names the A2 layer was asked to consider that are still NOT published, measured as
    /// live framework projections: each is a non-null app-level entry and a *different object* from the shipped
    /// WinUI-literal token of the same meaning - the framework hands out opaque macOS-flavoured greys (and a constant
    /// white for <c>TextOnAccent</c>) where our palette ships translucent WinUI values that flip by variant. That
    /// divergence is why these stay candidates rather than rows: publishing <c>name → token</c> would move any
    /// name-reader *toward* 1:1, so the only thing holding a row back is the absence of a demonstrated reader we
    /// cannot already re-template - not a fidelity cost. If a shipped control starts consuming one of these names, the
    /// row is worth shipping; the measured colours below are the baseline that decision revisits.
    ///
    /// This theory used to carry a <c>TextSecondary</c> leg. That leg was deleted rather than re-pinned when the row
    /// shipped, because every claim it made is now false by design and the same instrument covers the published case
    /// better: <see cref="A_retint_row_follows_the_theme_flip_to_the_variant_it_declares" /> pins the name to the
    /// palette instance in both variants, and
    /// <see cref="The_flyout_rows_own_text_is_painted_from_the_secondary_text_name" /> pins what it paints.
    /// </summary>
    [Theory]
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

    /// <summary>
    /// The non-retemplatable reader that <c>TextPrimary</c>'s "no demonstrated reader" verdict was missing. A
    /// <see cref="MenuItem"/> is self-drawn (its <c>OnRender</c> resolves text colour by name, and the menu batch found
    /// no instantiable template to replace), so the framework's own name lookup is the only lever over its ink — and
    /// that lever is live for <c>TextPrimary</c>: <c>ResolvePrimaryTextBrush</c> returns the <c>TextPrimary</c>
    /// projection, and installing a sentinel under <c>TextPrimary</c> in <c>Application.Resources</c> makes the same
    /// call return that sentinel, so the resolver reads the name per call, exactly like <c>ControlBorderFocused</c>'s
    /// focused-border path. Proving this is what made a <c>TextPrimary → TextFillColorPrimaryBrush</c> alias row
    /// warranted, and the row now ships: <see cref="The_published_row_moves_the_frameworks_primary_text_name_onto_our_ink" />
    /// reads the resolver's resting value back as the palette instance. What this class still does not have is a
    /// PIXEL for that surface - every capture of a built MenuItem came back uniform #000000 (the #50 glyph limit),
    /// so the claim here is deliberately about resolution, not about appearance.
    /// </summary>
    [Fact]
    public void A_self_drawn_menu_item_reads_the_primary_text_name()
    {
        _fixture.Run(() =>
        {
            var application = Application.Current!;
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            try
            {
                var item = new MenuItem();
                var resting = ResolveMenuItemPrimaryText(item);
                // The resolver's resting value is the framework's TextPrimary projection...
                Assert.Equal(ColorOf(application.TryFindResource("TextPrimary")), Assert.IsType<SolidColorBrush>(resting).Color);
                // ...and it follows whatever an application-level entry under that name holds.
                var sentinel = new SolidColorBrush(Probe);
                application.Resources["TextPrimary"] = sentinel;
                try
                {
                    Assert.True(ReferenceEquals(sentinel, ResolveMenuItemPrimaryText(item)),
                        "MenuItem.ResolvePrimaryTextBrush did not follow an installed TextPrimary entry, so the name "
                        + "is not read per call on this runtime and an alias row would reach nothing");
                }
                finally
                {
                    application.Resources.Remove("TextPrimary");
                }
            }
            finally
            {
                FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            }
        });
    }

    /// <summary>
    /// The reader that earned the fifth row in <c>ThemeResources/FrameworkRetints.jalxaml</c>, proven at the PIXEL
    /// rather than at the dictionary. A <see cref="MenuFlyoutSubItem"/> paints its own row text (the ghost batch found
    /// the control's label unreachable from a template), and the name it paints from is <c>TextSecondary</c>: a probe
    /// brush installed under that name replaces the whole glyph run, while probes under the neighbouring names leave
    /// it on the value the shipped row forwards.
    ///
    /// Both branches are captured on an opaque plate, because the row's token is translucent (#9E000000) and a capture
    /// on nothing composites it onto the black harness host and reports no ink at all - measured after the row, 9120 of
    /// 9120 pixels black - which would make this a vacuous pass rather than a reading. The colour claimed is therefore
    /// the composite <see cref="PixelHarness.Over" /> predicts, not the brush's own bytes.
    /// </summary>
    [Theory]
    [InlineData("TextSecondary", true)]
    [InlineData("TextPrimary", false)]
    [InlineData("TextDisabled", false)]
    public void The_flyout_rows_own_text_is_painted_from_the_secondary_text_name(string frameworkName, bool isRead)
    {
        _fixture.Run(() =>
        {
            var application = Application.Current!;
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            var host = PixelHarness.Backdrop(new MenuFlyoutSubItem { Text = "sub" }, PixelHarness.LightPage);
            PixelHarness.Build(host, 240, 38);
            PixelHarness.Settle(20);

            var token = Assert.IsType<SolidColorBrush>(FluentThemeManager.GetBrush("TextFillColorSecondaryBrush"));
            var rowInk = PixelHarness.Over(PixelHarness.LightPage, token.Color);
            var resting = PixelHarness.Render(host, 240, 38);
            var sentinel = new SolidColorBrush(Probe);
            application.Resources[frameworkName] = sentinel;
            try
            {
                PixelHarness.Settle(10);
                var painted = PixelHarness.Render(host, 240, 38);
                Assert.Multiple(
                    // The instrument reads an inked row on the plate, and no probe ink in it yet.
                    () => Assert.True(resting.Count(rowInk) > PaintedFloor,
                        $"the row's own text did not reach pixels before the probe; top={resting.Top(4)}"),
                    () => Assert.Equal(0, resting.Count(Probe)),
                    // Whether the name is on the paint path is decided by the same two counts after the probe lands.
                    () => Assert.Equal(isRead ? 0 : resting.Count(rowInk), painted.Count(rowInk)),
                    () => Assert.True(isRead ? painted.Count(Probe) > PaintedFloor : painted.Count(Probe) == 0,
                        $"expected {frameworkName} to{(isRead ? "" : " not")} reach the row text; top={painted.Top(4)}"));
            }
            finally
            {
                application.Resources.Remove(frameworkName);
            }
        });
    }

    /// <summary>
    /// The same name on a surface this library never restyles at all: a native <see cref="Label"/> resolves its
    /// foreground from <c>TextSecondary</c>, which is why the shipped row reaches a control that carries no style of
    /// ours. Before the row this ink was the framework's opaque <c>#FF6E6E73</c>; it is now the palette instance the
    /// row forwards, and the probe leg is what shows the value still arrives through a per-call lookup rather than a
    /// load-time substitution. A built control keeps what it resolved at build time, so the probe has to be installed
    /// before the element is built for the reading to mean anything.
    /// </summary>
    [Fact]
    public void A_native_label_resolves_its_ink_from_the_same_name()
    {
        _fixture.Run(() =>
        {
            var application = Application.Current!;
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            var token = Assert.IsType<SolidColorBrush>(FluentThemeManager.GetBrush("TextFillColorSecondaryBrush"));
            try
            {
                // The row forwards the palette instance itself, so the Label's ink is that object, not a copy of it.
                Assert.Same(token, InkedForeground());

                application.Resources["TextSecondary"] = new SolidColorBrush(Probe);
                var probed = InkedForeground();

                Assert.Multiple(
                    // The name reaches the property, not just the dictionary: the built element resolved the probe.
                    () => Assert.Equal(Probe, ColorOf(probed)),
                    // And the neighbouring name is not what carries this ink - the Label follows TextSecondary only.
                    () => Assert.NotEqual(ColorOf(application.TryFindResource("TextPrimary")), ColorOf(probed)));
            }
            finally
            {
                application.Resources.Remove("TextSecondary");
                Assert.Same(token, InkedForeground());
            }
        });
    }

    // ---------- helpers ----------

    /// <summary>The pixel floor <see cref="AstraMenuTests"/> uses for "this ink is really painted", measured here at
    /// 38 pixels on a 240x38 row, so the floor is the menu batch's own and not a number picked for this file.</summary>
    private const int PaintedFloor = 8;

    /// <summary>MenuItem's own per-call text-colour resolution. Not public; reaching it is a test-only instrument, the
    /// same shape as <see cref="ResolveFocusedBorder" /> — the product ban on framework reflection is unaffected.</summary>
    private static Brush? ResolveMenuItemPrimaryText(MenuItem item)
    {
        var resolver = typeof(MenuItem).GetMethod("ResolvePrimaryTextBrush", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException(
                "MenuItem has no ResolvePrimaryTextBrush on this runtime, so the menu-ink reading does not describe 26.10.9");
        return (Brush?)resolver.Invoke(item, null);
    }

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
