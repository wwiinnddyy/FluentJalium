using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
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
        _jobs.CompleteAdding();
        _thread.Join(TimeSpan.FromSeconds(10));
        _jobs.Dispose();
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
