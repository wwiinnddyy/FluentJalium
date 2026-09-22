using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using FluentJalium.Tests.Pixel;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Interop;
using Jalium.UI.Markup;
using Jalium.UI.Media;

namespace FluentJalium.Tests;

/// <summary>
/// Jalium owns one application per process, and Astra keeps its dictionaries and its UI-thread
/// guard attached to that application. xunit hands each test its own worker thread, so the fixture
/// runs the application on a dedicated thread of its own and every theme call is posted to it.
/// Keep the collection non-parallel and re-establish the baseline in the test constructor so no
/// assertion depends on the order the tests ran in.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class AstraThemeRuntimeCollection : ICollectionFixture<AstraThemeRuntimeFixture>
{
    public const string Name = "Astra theme runtime";
}

public sealed class AstraThemeRuntimeFixture : IDisposable
{
    private readonly BlockingCollection<Action> _jobs = [];
    private readonly Thread _thread;

    public AstraThemeRuntimeFixture()
    {
        _thread = new Thread(Pump) { Name = "Astra theme runtime", IsBackground = true };
        _thread.SetApartmentState(ApartmentState.STA);
        _thread.Start();
        Run(() =>
        {
            // Same order the documented startup sequence uses; the pixel tests need a render
            // context on this thread, and it has to exist before the Application does.
            RenderContext.GetOrCreateCurrent(RenderBackend.Auto).DefaultRenderingEngine = RenderingEngine.Impeller;
            ThemeLoader.Initialize();
            Application = new Application();
            FluentThemeManager.Apply(Application, FluentThemeVariant.Light);
        });
    }

    /// <summary>Thread-affine. Only read it from inside a <see cref="Run"/> delegate.</summary>
    public Application Application { get; private set; } = null!;

    public void Run(Action work)
    {
        if (Thread.CurrentThread == _thread)
        {
            work();
            return;
        }

        using var done = new ManualResetEventSlim();
        Exception? failure = null;
        _jobs.Add(() =>
        {
            try { work(); }
            catch (Exception exception) { failure = exception; }
            finally { done.Set(); }
        });
        if (!done.Wait(TimeSpan.FromSeconds(60))) throw new TimeoutException("The Astra theme thread did not answer within 60 seconds.");
        if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();
    }

    public void Dispose()
    {
        ReleaseHostQuietly();
        _jobs.CompleteAdding();
        _thread.Join(TimeSpan.FromSeconds(10));
        _jobs.Dispose();
    }

    /// <summary>The pixel host is a window on this thread, so it has to be closed here, not on a worker.</summary>
    private void ReleaseHostQuietly()
    {
        if (!_thread.IsAlive) return;
        using var done = new ManualResetEventSlim();
        _jobs.Add(() =>
        {
            try { PixelHarness.ReleaseHost(); }
            finally { done.Set(); }
        });
        done.Wait(TimeSpan.FromSeconds(10));
    }

    private void Pump()
    {
        foreach (var job in _jobs.GetConsumingEnumerable()) job();
    }
}

[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraThemeRuntimeTests
{
    private const string OverrideKey = "AccentFillColorDefaultBrush";
    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraThemeRuntimeTests(AstraThemeRuntimeFixture fixture)
    {
        _fixture = fixture;
        // Tests mutate the process-wide manager. Restore the source-backed baseline for every test
        // instance, including a previous test's accent and brush override.
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            FluentThemeManager.ApplyAccent(null);
            FluentThemeManager.OverrideBrush(OverrideKey, null);
        });
    }

    [Fact]
    public void Applying_a_theme_updates_the_host_application_theme_mode()
    {
        // Read through Astra's accessor: Jalium's ThemeMode type carries [Experimental("WPF0001")],
        // and ApplicationThemeDriver is the single file allowed to name it.
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            Assert.Equal("Light", FluentThemeManager.NativeThemeMode);

            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
            Assert.Equal("Dark", FluentThemeManager.NativeThemeMode);

            FluentThemeManager.ApplyTheme(FluentThemeVariant.System);
            Assert.Equal("System", FluentThemeManager.NativeThemeMode);
        });
    }

    [Fact]
    public void Switching_palette_updates_a_retained_brush_instance_and_application_resource()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            var brush = Assert.IsType<SolidColorBrush>(FluentThemeManager.GetBrush("SolidBackgroundFillColorBaseBrush"));
            var lightColor = brush.Color;
            Assert.Same(brush, _fixture.Application.TryFindResource("SolidBackgroundFillColorBaseBrush"));

            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);

            var darkBrush = Assert.IsType<SolidColorBrush>(FluentThemeManager.GetBrush("SolidBackgroundFillColorBaseBrush"));
            Assert.Same(brush, darkBrush);
            Assert.NotEqual(lightColor, darkBrush.Color);
            Assert.Equal(Color.FromRgb(0x20, 0x20, 0x20), darkBrush.Color);
            Assert.Same(brush, _fixture.Application.TryFindResource("SolidBackgroundFillColorBaseBrush"));
        });
    }

    /// <summary>
    /// The upstream flyout names are <c>StaticResource</c> aliases onto our palette keys, so a consumer
    /// asking for <c>FlyoutPresenterBackground</c> must get the very instance the palette re-tints. If
    /// the alias ever materialises its own brush it freezes at parse time and stops following the theme.
    /// </summary>
    [Fact]
    public void Upstream_flyout_aliases_resolve_to_the_palette_brush_instance_and_follow_the_theme()
    {
        _fixture.Run(() =>
        {
            var surface = Assert.IsType<SolidColorBrush>(_fixture.Application.TryFindResource("FlyoutPresenterBackground"));
            var border = Assert.IsType<SolidColorBrush>(_fixture.Application.TryFindResource("FlyoutBorderThemeBrush"));

            Assert.Same(_fixture.Application.TryFindResource("AcrylicInAppFillColorDefaultBrush"), surface);
            Assert.Same(_fixture.Application.TryFindResource("SurfaceStrokeColorFlyoutBrush"), border);
            Assert.Equal(Color.FromRgb(0xF9, 0xF9, 0xF9), surface.Color);

            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);

            Assert.Same(surface, _fixture.Application.TryFindResource("FlyoutPresenterBackground"));
            Assert.Equal(Color.FromRgb(0x2C, 0x2C, 0x2C), surface.Color);
            // A missing key fails silently in this reader, so the non-alias key needs its own check.
            Assert.NotNull(_fixture.Application.TryFindResource("FlyoutBorderThemeThickness"));
        });
    }

    [Fact]
    public void Upstream_tooltip_aliases_resolve_to_the_same_palette_instances()
    {
        // ToolTip is the second consumer of the alias layer, and its three upstream brush names must
        // land on the objects the kernel retints rather than on copies. Its border thickness is 1 in
        // upstream's HighContrast branch too, so unlike the flyout border it carries no mode gap.
        _fixture.Run(() =>
        {
            Assert.Same(_fixture.Application.TryFindResource("TextFillColorPrimaryBrush"), _fixture.Application.TryFindResource("ToolTipForegroundBrush"));
            Assert.Same(_fixture.Application.TryFindResource("AcrylicInAppFillColorDefaultBrush"), _fixture.Application.TryFindResource("ToolTipBackgroundBrush"));
            Assert.Same(_fixture.Application.TryFindResource("SurfaceStrokeColorFlyoutBrush"), _fixture.Application.TryFindResource("ToolTipBorderBrush"));
            Assert.NotNull(_fixture.Application.TryFindResource("ToolTipBorderThemeThickness"));
            Assert.NotNull(_fixture.Application.TryFindResource("ToolTipBorderPadding"));
        });
    }

    [Fact]
    public void High_contrast_maps_semantic_roles_to_system_colors()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.HighContrast);

            Assert.Equal(SystemColors.WindowTextColor, BrushColor("TextFillColorPrimaryBrush"));
            // Upstream Common_themeresources_any.xaml @19e3bdc puts the accent fill on the window
            // background and keeps emphasis in borders and text, so this is not HighlightColor.
            Assert.Equal(SystemColors.WindowColor, BrushColor("AccentFillColorDefaultBrush"));
            Assert.Equal(SystemColors.GrayTextColor, BrushColor("TextFillColorDisabledBrush"));
            Assert.Equal(Colors.Transparent, BrushColor("SubtleFillColorTransparentBrush"));
        });
    }

    [Fact]
    public void High_contrast_mapping_covers_every_palette_brush()
    {
        // The map is generated from upstream's HighContrast branch, so a palette key with no
        // upstream decision fails here rather than falling back to a guess at runtime.
        _fixture.Run(() => Assert.Equal(
            FluentThemeManager.PaletteBrushKeys,
            FluentThemeManager.HighContrastMap.Keys.Order(StringComparer.Ordinal)));
    }

    /// <summary>
    /// The per-key half. The two tests above prove the checked-in table has a row for every brush and read four of
    /// those rows; this one resolves <b>all</b> of them and compares each brush with the system colour its own row
    /// names, then requires the theme flip to have actually moved the palette - a current table that is never applied
    /// reads green otherwise.
    /// <para>
    /// What each row pins is the <b>wiring</b>: this key is driven by that system-colour slot. For the comparison to
    /// mean anything it has to be able to tell the slots apart, and until now it could not. The eight
    /// <c>SystemColor*ColorBrush</c> rows used to carry upstream's Light/Dark placeholder literally
    /// (<c>Common_themeresources_any.xaml:178</c> and <c>:406</c> write <c>#FF00FF</c>), and Jalium resolves
    /// <c>SystemColors.&lt;Slot&gt;</c> by looking for a resource named <c>&lt;Slot&gt;Brush</c> <b>before</b> the platform -
    /// <c>spike/SystemColorProbe</c> measured that priority by writing a colour into our own row and watching
    /// <c>SystemColors.WindowTextColor</c> move with it. So all eight slots answered <c>#FFFF00FF</c>, High Contrast
    /// painted the entire palette magenta, and swapping any two rows of the table was invisible.
    /// </para>
    /// <para>
    /// Those eight rows now hold upstream's own HighContrast value-form (<c>{ThemeResource SystemColor&lt;Slot&gt;}</c>)
    /// in every mode, and neither the palette's former publisher nor the High-contrast re-tint writes them any more, so
    /// the colour inside one is the framework's own slot value as of the last refresh rather than a literal we authored.
    /// Whether such a row then follows a platform change that arrives without a refresh is unmeasured - this host has no
    /// high-contrast scheme to switch, and measured in the High Contrast state the row still answered the value the
    /// alias resolved while the runtime was in Light (<c>#FFF5F5F7</c>) while the framework's own
    /// <c>SystemColorWindowTextColor</c> resource had already moved to <c>#FF1D1D1F</c>.
    /// On this host the eight slots resolve to seven distinct colours -
    /// <c>SystemColorWindowTextColor</c> and <c>SystemColorButtonTextColor</c> answer the same. That distinction is what
    /// gives the <i>independent</i> half of this class its teeth: <see cref="High_contrast_maps_semantic_roles_to_system_colors"/>
    /// names four slots itself, and pointing <c>TextFillColorPrimaryBrush</c> at <c>SystemColorHighlightColor</c> in the
    /// map now fails there, where the same mutation read green while every slot answered <c>#FFFF00FF</c>. The loop
    /// below cannot catch a mis-pointed row at all - it compares each brush with the target the same table names - so
    /// what it pins is that the retint reaches every key, and the map's own correctness rests on
    /// <c>tools/Sync-AstraPalette.ps1 -Check</c>. What a real high-contrast user sees is still not claimed: this machine
    /// has high contrast off (<c>GetSystemMetrics(SM_HIGCONTRASTMODE) = 0x1E</c>, bit 0 clear) and none of the eight
    /// Win32 <c>GetSysColor</c> answers equals what the framework resolves, so the platform half of the value stays a
    /// Known Gap.
    /// </para>
    /// </summary>
    [Fact]
    public void Every_high_contrast_row_drives_its_own_brush()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            var light = FluentThemeManager.HighContrastMap.Keys.ToDictionary(key => key, BrushColor);
            AssertNoPlaceholder(light.Keys);

            FluentThemeManager.ApplyTheme(FluentThemeVariant.HighContrast);

            var wrong = new List<string>();
            foreach (var (key, target) in FluentThemeManager.HighContrastMap.OrderBy(entry => entry.Key, StringComparer.Ordinal))
            {
                var actual = BrushColor(key);
                if (!SystemColour(target).Equals(actual))
                {
                    wrong.Add($"{key} names {target} = {SystemColour(target)} but the brush is {actual}");
                }
            }

            var moved = FluentThemeManager.HighContrastMap.Keys.Count(key => !BrushColor(key).Equals(light[key]));
            var current = FluentThemeManager.HighContrastMap.Keys.ToDictionary(key => key, BrushColor);
            AssertNoPlaceholder(current.Keys);

            // The collapse this row used to read through is the failure this counts against: eight slots answering
            // one colour makes every per-row comparison above true no matter which slot a row names. Measured here
            // are seven of eight; the floor is a guard against the collapse, not a claim about the values.
            var targets = FluentThemeManager.HighContrastMap.Values.Distinct(StringComparer.Ordinal).ToArray();
            var distinctSlots = targets.Select(SystemColour).Distinct().ToArray();
            Assert.True(distinctSlots.Length >= 4,
                $"the {targets.Length} map targets (eight system-colour slots plus Transparent) resolve to only {distinctSlots.Length} distinct colours, so the per-row comparison above cannot tell a swapped table from a correct one.");
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);

            Assert.True(wrong.Count == 0,
                $"{wrong.Count} of {FluentThemeManager.HighContrastMap.Count} rows do not drive their own brush: {string.Join("; ", wrong.Take(6))}");
            // Measured 2026-09-21: 90 of the 101 brushes change colour on the flip; the other 11 already carry
            // their high-contrast value in Light (Transparent rows and a few tokens whose two modes coincide), and
            // the count is deterministic, so the floor sits well under the reading without hiding a lost retint.
            Assert.True(moved >= 70,
                $"the flip only moved {moved} of {FluentThemeManager.HighContrastMap.Count} palette brushes, so High Contrast is not reaching the palette.");
        });
    }

    /// <summary>
    /// Upstream's Light/Dark placeholder for the system-colour rows. It must never reach a shipped mode here, and
    /// the reason is stronger than "ugly": in this runtime a row named <c>SystemColor&lt;Slot&gt;Brush</c> <i>is</i>
    /// what <c>SystemColors.&lt;Slot&gt;</c> answers, so the placeholder silently replaces a platform colour for every
    /// reader in the process.
    /// </summary>
    private static void AssertNoPlaceholder(IEnumerable<string> keys)
    {
        var placeholder = Color.FromRgb(0xFF, 0x00, 0xFF);
        var hit = keys.Where(key => BrushColor(key).Equals(placeholder)).ToArray();
        Assert.True(hit.Length == 0,
            $"{hit.Length} palette brushes carry upstream's #FF00FF placeholder ({string.Join(", ", hit.Take(6))}); in this runtime that also overwrites the system-colour slot of the same name.");
    }

    /// <summary>
    /// The targets <c>ThemeResources/HighContrast.map</c> names, resolved through the same runtime the retint uses.
    /// An unknown name fails rather than being skipped, so a new upstream target cannot be quietly dropped.
    /// </summary>
    private static Color SystemColour(string target) => target switch
    {
        "SystemColorWindowColor" => SystemColors.WindowColor,
        "SystemColorWindowTextColor" => SystemColors.WindowTextColor,
        "SystemColorButtonFaceColor" => SystemColors.ControlColor,
        "SystemColorButtonTextColor" => SystemColors.ControlTextColor,
        "SystemColorGrayTextColor" => SystemColors.GrayTextColor,
        "SystemColorHotlightColor" => SystemColors.HotTrackColor,
        "SystemColorHighlightColor" => SystemColors.HighlightColor,
        "SystemColorHighlightTextColor" => SystemColors.HighlightTextColor,
        "Transparent" => Colors.Transparent,
        _ => throw new InvalidOperationException($"Unknown High Contrast target '{target}' - extend the resolver, do not skip the key."),
    };

    [Fact]
    public void Explicit_brush_override_wins_after_palette_refresh()
    {
        _fixture.Run(() =>
        {
            var overrideColor = Color.FromRgb(0x12, 0x34, 0x56);
            FluentThemeManager.OverrideBrush(OverrideKey, overrideColor);
            Assert.Equal(overrideColor, BrushColor(OverrideKey));

            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
            Assert.Equal(overrideColor, BrushColor(OverrideKey));

            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            Assert.Equal(overrideColor, BrushColor(OverrideKey));
        });
    }

    [Fact]
    public async Task Theme_mutation_from_a_background_thread_is_rejected()
    {
        var exception = await Record.ExceptionAsync(() => Task.Run(() => FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark)));

        Assert.IsType<InvalidOperationException>(exception);
    }

    private static Color BrushColor(string key)
    {
        return Assert.IsType<SolidColorBrush>(FluentThemeManager.GetBrush(key)).Color;
    }
}
