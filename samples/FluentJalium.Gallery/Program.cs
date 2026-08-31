using FluentJalium.Controls.Themes;
using FluentJalium.Gallery.Pages;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Themes;

namespace FluentJalium.Gallery;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        var app = new Application();
        ThemeManager.Initialize(app);
        FluentThemeManager.Apply(app);

        if (args.Contains("--poc", StringComparer.Ordinal))
        {
            app.Run(new Window { Title = "XAML Pipeline Probe", Width = 900, Height = 600, Content = new XamlPipelineProbe() });
            return;
        }

        app.Run(new MainWindow());
    }
}
