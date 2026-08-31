using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using FluentJalium.WinUI;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Themes;
using WinUIGallery.ControlPages;
using WinUIGallery.Samples.ControlPages.Fundamentals.Controls;

namespace FluentJalium.WinUI.Probe;

// Reports what the WinUI facade actually does with each construct, rather than what the
// compiler accepted. Builds green is not the same claim as renders correct.
internal static class Program
{
    [STAThread]
    private static void Main()
    {
        var app = new Application();
        ThemeManager.Initialize(app);
        FluentThemeManager.Apply(app);
        WinUICompatibility.Initialize();

        Probe("ContentDialogExample (TextBlock/CheckBox/StackPanel, x:Class : ContentDialog)",
            () => new ContentDialogExample());

        Probe("TemperatureConverterControl (x:Name, Click=, x:Bind method call Mode=OneWay)",
            () =>
            {
                var control = new TemperatureConverterControl();
                var panel = (Panel)control.Content!;
                var button = (Button)panel.Children[1];

                // The generator emitted: SetProperty(button, "IsEnabled", "{x:Bind HasText(...), Mode=OneWay}", ctx)
                // The text box starts empty, so HasText("") is false: a binding that actually works
                // leaves the button disabled, while one that was dropped or coerced to a string does not.
                Console.WriteLine($"    IsEnabled = {button.IsEnabled}  (expected False if x:Bind evaluated)");
                return control;
            });

        Console.WriteLine("probe finished");
    }

    private static void Probe(string label, Func<object> build)
    {
        Console.WriteLine($"--- {label}");
        try
        {
            var instance = build();
            Console.WriteLine($"    OK -> {instance.GetType().FullName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"    FAILED -> {ex.GetType().Name}: {ex.Message}");
        }
    }
}
