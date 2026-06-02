using FluentJalium.Controls.Themes;
using Jalium.UI;
using Jalium.UI.Controls.Themes;

namespace FluentJalium.Gallery;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        var app = new Application();
        ThemeManager.Initialize(app);
        FluentThemeManager.Apply(app);

        app.Run(new MainWindow());
    }
}
