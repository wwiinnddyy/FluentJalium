using FluentJalium.Controls.Themes;
using Jalium.UI;

namespace FluentJalium.Gallery;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        var app = new Application();
        FluentThemeManager.Apply(app);

        app.Run(new MainWindow());
    }
}
