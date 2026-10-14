using FluentJalium.Gallery;
using FluentJalium.Localization;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Interop;
using Jalium.UI.Markup;

namespace FluentJalium.Launcher;

/// <summary>
/// Launcher entry point. Reads the saved preferences, applies the ones that are process-wide before any window
/// exists, and shows either the launcher or - for QA - the Gallery directly.
/// </summary>
/// <remarks>
/// Switches: <c>--page &lt;id&gt;</c> chooses the Gallery page to land on, <c>--lang zh|en</c> overrides the saved
/// language, and <c>--skip-launcher</c> goes straight to the Gallery on the saved settings.
/// </remarks>
internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        LauncherText.Register();

        var settings = ReadSettings(args);

        // Before any window: the backend and the path engine are the two settings a render context resolves once,
        // and the language has to be in the catalog before the first page is parsed.
        var renderContext = RenderContext.GetOrCreateCurrent(RenderBackend.Auto);
        renderContext.DefaultRenderingEngine = settings.Engine;

        // Before any JALXAML is parsed, and once per process.
        ThemeLoader.Initialize();

        var application = new Application
        {
            // OnMainWindowClose, not the default OnLastWindowClose: the handoff closes the launcher before the
            // Gallery is shown, and a window count of zero under the default mode would end the process there.
            ShutdownMode = ShutdownMode.OnMainWindowClose,
        };
        FluentThemeManager.Apply(application);
        FluentThemeManager.ApplyTheme(settings.Theme switch
        {
            AppThemeMode.Light => FluentThemeVariant.Light,
            AppThemeMode.Dark => FluentThemeVariant.Dark,
            _ => FluentThemeVariant.System,
        });
        FluentThemeManager.ReduceMotion = settings.ReduceMotion;

        if (HasFlag(args, "--skip-launcher"))
        {
            var gallery = new MainWindow();
            gallery.SetTheme(settings.Theme switch
            {
                AppThemeMode.Light => FluentThemeVariant.Light,
                AppThemeMode.Dark => FluentThemeVariant.Dark,
                _ => FluentThemeVariant.System,
            });
            if (Option(args, "--page") is { Length: > 0 } pageId)
            {
                gallery.SetStartPage(pageId);
            }

            application.MainWindow = gallery;
            gallery.Show();
            gallery.Activate();
            return application.Run();
        }

        var launcher = new LauncherWindow(settings, Option(args, "--page"));
        application.MainWindow = launcher;
        launcher.Show();
        launcher.Activate();
        return application.Run();
    }

    /// <summary>
    /// Loads the saved preferences, then lets an explicit <c>--lang</c> win. A first run with no file takes the
    /// operating system's UI language rather than the model's English default, because the model default is a
    /// round-trip baseline for the settings tests rather than a product decision about first impressions.
    /// </summary>
    private static HostSettings ReadSettings(string[] args)
    {
        var hadSavedFile = File.Exists(HostSettingsStore.DefaultPath);
        var settings = HostSettingsStore.Load();

        if (!hadSavedFile)
        {
            settings.Language = UiLanguageExtensions.FromSystemCulture();
        }

        if (Option(args, "--lang") is { } requested && UiLanguageExtensions.TryParse(requested, out var language))
        {
            settings.Language = language;
        }

        return settings;
    }

    private static string? Option(string[] args, string name)
    {
        for (var index = 0; index < args.Length - 1; index++)
        {
            if (string.Equals(args[index], name, StringComparison.OrdinalIgnoreCase))
            {
                return args[index + 1];
            }
        }

        return null;
    }

    private static bool HasFlag(string[] args, string name) =>
        Array.Exists(args, arg => string.Equals(arg, name, StringComparison.OrdinalIgnoreCase));
}
