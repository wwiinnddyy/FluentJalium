using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Interop;
using Jalium.UI.Markup;
using Jalium.UI.Threading;

namespace FluentJalium.Gallery;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        var renderContext = RenderContext.GetOrCreateCurrent(RenderBackend.Auto);
        renderContext.DefaultRenderingEngine = RenderingEngine.Impeller;
        ThemeLoader.Initialize();

        var application = new Application();
        FluentThemeManager.Apply(application);

        // TEMPORARY DIAGNOSTIC, NOT FOR COMMIT: pin the start theme before the first frame, then flip on a live window.
        var startTheme = Environment.GetEnvironmentVariable("ASTRA_START_THEME");
        if (startTheme == "light") FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
        if (startTheme == "dark") FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);

        var window = new MainWindow();
        var startPage = Option(args, "--page");
        if (startPage != null) window.SetStartPage(startPage);
        application.MainWindow = window;
        window.Show();
        window.Activate();

        if (Environment.GetEnvironmentVariable("ASTRA_FLIP_MS") is { } flip && int.TryParse(flip, out var flipMs))
        {
            var target = Environment.GetEnvironmentVariable("ASTRA_FLIP_TO") == "light"
                ? FluentThemeVariant.Light
                : FluentThemeVariant.Dark;
            var timer = new DispatcherTimer(
                TimeSpan.FromMilliseconds(flipMs), DispatcherPriority.Background,
                (_, _) => FluentThemeManager.ApplyTheme(target),
                Dispatcher.CurrentDispatcher);
            timer.Start();
        }

        return application.Run();
    }

    private static string? Option(string[] args, string name)
    {
        for (var index = 0; index < args.Length - 1; index++)
        {
            if (string.Equals(args[index], name, StringComparison.OrdinalIgnoreCase)) return args[index + 1];
        }

        return null;
    }
}
