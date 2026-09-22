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
}
