using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Interop;
using Jalium.UI.Markup;

namespace FluentJalium.Gallery;

internal static class Program
{
    [STAThread]
    private static int Main()
    {
        var renderContext = RenderContext.GetOrCreateCurrent(RenderBackend.Auto);
        renderContext.DefaultRenderingEngine = RenderingEngine.Impeller;
        ThemeLoader.Initialize();

        var application = new Application();
        FluentThemeManager.Apply(application);
        var window = new MainWindow();
        application.MainWindow = window;
        window.Show();
        window.Activate();
        return application.Run();
    }
}
