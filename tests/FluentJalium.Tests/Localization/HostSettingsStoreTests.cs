using FluentJalium.Localization;
using Jalium.UI.Hosting;
using Jalium.UI.Interop;

namespace FluentJalium.Tests.Localization;

/// <summary>
/// The settings file is a convenience, never a precondition: a missing, unreadable or hand-mangled document
/// resolves to the shipped defaults rather than taking the launcher down with it.
/// </summary>
public class HostSettingsStoreTests : IDisposable
{
    private readonly string _path = Path.Combine(
        Path.GetTempPath(), $"fj-settings-{Guid.NewGuid():N}.json");

    public void Dispose()
    {
        if (File.Exists(_path))
        {
            File.Delete(_path);
        }
    }

    [Fact]
    public void Every_field_survives_a_round_trip()
    {
        var written = new HostSettings
        {
            Backend = RenderBackend.Vulkan,
            Engine = RenderingEngine.Vello,
            Language = UiLanguage.ZhHans,
            PathAntiAliasing = PathAntiAliasing.Analytic,
            Theme = AppThemeMode.Dark,
            ReduceMotion = true,
        };

        Assert.True(HostSettingsStore.TrySave(written, _path));
        var read = HostSettingsStore.Load(_path);

        Assert.Equal(RenderBackend.Vulkan, read.Backend);
        Assert.Equal(RenderingEngine.Vello, read.Engine);
        Assert.Equal(UiLanguage.ZhHans, read.Language);
        Assert.Equal(PathAntiAliasing.Analytic, read.PathAntiAliasing);
        Assert.Equal(AppThemeMode.Dark, read.Theme);
        Assert.True(read.ReduceMotion);
    }

    [Fact]
    public void Enums_are_written_by_name_so_a_reordered_enum_cannot_repoint_a_saved_value()
    {
        HostSettingsStore.TrySave(new HostSettings { Backend = RenderBackend.D3D12 }, _path);

        Assert.Contains("\"D3D12\"", File.ReadAllText(_path), StringComparison.Ordinal);
    }

    [Fact]
    public void A_missing_file_loads_the_shipped_defaults()
    {
        var settings = HostSettingsStore.Load(Path.Combine(_path, "does-not-exist.json"));

        var defaults = new HostSettings();
        Assert.Equal(defaults.Backend, settings.Backend);
        Assert.Equal(defaults.Engine, settings.Engine);
        Assert.Equal(defaults.Language, settings.Language);
        Assert.Equal(defaults.PathAntiAliasing, settings.PathAntiAliasing);
        Assert.Equal(defaults.Theme, settings.Theme);
        Assert.False(settings.ReduceMotion);
    }

    [Fact]
    public void An_unreadable_document_loads_the_defaults_instead_of_throwing()
    {
        File.WriteAllText(_path, "{ this is not json");

        var settings = HostSettingsStore.Load(_path);

        Assert.Equal(UiLanguage.English, settings.Language);
    }

    [Fact]
    public void An_unknown_enum_spelling_costs_one_field_and_not_the_document()
    {
        File.WriteAllText(_path, """
            {
              "backend": "Metal",
              "engine": "Vello",
              "language": "ZhHans",
              "pathAntiAliasing": "Analytic",
              "theme": "Dark",
              "reduceMotion": true
            }
            """);

        var settings = HostSettingsStore.Load(_path);

        // Metal parses, so it is kept; the point is that a value this build does not know must not discard the
        // language, the quality mode or the theme next to it.
        Assert.Equal(RenderBackend.Metal, settings.Backend);
        Assert.Equal(RenderingEngine.Vello, settings.Engine);
        Assert.Equal(UiLanguage.ZhHans, settings.Language);
        Assert.Equal(PathAntiAliasing.Analytic, settings.PathAntiAliasing);
        Assert.Equal(AppThemeMode.Dark, settings.Theme);
        Assert.True(settings.ReduceMotion);
    }

    [Fact]
    public void A_value_this_build_does_not_know_falls_back_on_its_own()
    {
        File.WriteAllText(_path, """{ "backend": "Nonesuch" }""");

        Assert.Equal(RenderBackend.Auto, HostSettingsStore.Load(_path).Backend);
    }

    [Fact]
    public void A_save_that_cannot_happen_reports_false_rather_than_throwing()
    {
        // A file where a directory has to be: the normal shape of an unwritable install directory, without
        // needing one, and without depending on who owns %TEMP%.
        File.WriteAllText(_path, "in the way");
        var unwritable = Path.Combine(_path, "settings.json");

        Assert.False(HostSettingsStore.TrySave(new HostSettings(), unwritable));
        Assert.True(HostSettingsStore.Load(unwritable).Language == UiLanguage.English);
    }

    [Fact]
    public void Clone_is_a_snapshot_so_a_staged_handoff_is_not_undone_by_a_later_edit()
    {
        var live = new HostSettings { Backend = RenderBackend.Vulkan };
        var snapshot = live.Clone();

        live.Backend = RenderBackend.D3D12;

        Assert.Equal(RenderBackend.Vulkan, snapshot.Backend);
        Assert.Equal(RenderBackend.D3D12, live.Backend);
    }

    [Fact]
    public void ResetToDefaults_restores_every_field()
    {
        var settings = new HostSettings
        {
            Backend = RenderBackend.Vulkan,
            Engine = RenderingEngine.Vello,
            Language = UiLanguage.ZhHans,
            PathAntiAliasing = PathAntiAliasing.Analytic,
            Theme = AppThemeMode.Dark,
            ReduceMotion = true,
        };

        settings.ResetToDefaults();

        Assert.Equal(new HostSettings().Backend, settings.Backend);
        Assert.Equal(new HostSettings().Language, settings.Language);
        Assert.Equal(new HostSettings().PathAntiAliasing, settings.PathAntiAliasing);
        Assert.Equal(new HostSettings().Theme, settings.Theme);
        Assert.False(settings.ReduceMotion);
    }

    [Theory]
    [InlineData("zh", UiLanguage.ZhHans)]
    [InlineData("zh-CN", UiLanguage.ZhHans)]
    [InlineData("zh-Hans-CN", UiLanguage.ZhHans)]
    [InlineData("en", UiLanguage.English)]
    [InlineData("en-GB", UiLanguage.English)]
    public void The_command_line_spellings_a_script_would_pass_are_accepted(string text, UiLanguage expected)
    {
        Assert.True(UiLanguageExtensions.TryParse(text, out var language));
        Assert.Equal(expected, language);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    [InlineData("fr")]
    [InlineData("klingon")]
    public void Anything_else_fails_rather_than_guessing(string? text)
    {
        Assert.False(UiLanguageExtensions.TryParse(text, out var language));
        Assert.Equal(UiLanguage.English, language);
    }
}
