using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using FluentJalium.Gallery.Shell;
using Jalium.UI;
using Jalium.UI.Media;

namespace FluentJalium.Gallery;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Backdrop.FallbackColor = FallbackColorFor(FluentThemeManager.CurrentTheme);
        ShellHost.Content = new GalleryShell(this);
    }

    private static Color FallbackColorFor(FluentThemeVariant theme) => theme switch
    {
        FluentThemeVariant.Light => Color.FromRgb(0xF3, 0xF3, 0xF3),
        FluentThemeVariant.HighContrast => Color.FromRgb(0x00, 0x00, 0x00),
        _ => Color.FromRgb(0x20, 0x20, 0x20)
    };
}
