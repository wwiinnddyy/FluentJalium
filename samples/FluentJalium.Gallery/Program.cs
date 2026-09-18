using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Interop;
using Jalium.UI.Markup;

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
        var window = new MainWindow();
        var startPage = Option(args, "--page");
        if (startPage != null) window.SetStartPage(startPage);
        application.MainWindow = window;
        window.Show();
        window.Activate();
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
