using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using FluentJalium.Gallery.Shell;
using Jalium.UI;

namespace FluentJalium.Gallery;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        ShellHost.Content = new GalleryShell(this);

        // The window has no native handle until it is shown, so the chrome (DWM caption/border and
        // immersive dark mode) must be synchronised once Loaded fires. Runtime theme switches are
        // handled centrally by FluentThemeManager.
        Loaded += (_, _) => FluentWindowChrome.Apply(this);
    }
}
