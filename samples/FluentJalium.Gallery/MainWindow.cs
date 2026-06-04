using FluentJalium.Controls.Themes;
using FluentJalium.Gallery.Shell;
using FluentJalium.Gallery.Services;
using Jalium.UI.Controls;

namespace FluentJalium.Gallery;

public sealed class MainWindow : Window
{
    public MainWindow()
    {
        Title = "FluentJalium Gallery";
        Width = 1280;
        Height = 820;
        MinWidth = 980;
        MinHeight = 620;
        Background = GalleryThemeResources.Brush("WindowBackground");
        Content = new GalleryShell(this, new GalleryCatalogService(), ApplyTheme, ApplyAccent);
    }

    private void ApplyTheme(FluentThemeVariant theme)
    {
        FluentThemeManager.ApplyTheme(theme);
        RefreshTheme();
    }

    private void ApplyAccent(Jalium.UI.Media.Color accent)
    {
        FluentThemeManager.ApplyAccent(accent);
        RefreshTheme();
    }

    private void RefreshTheme()
    {
        Background = GalleryThemeResources.Brush("WindowBackground");

        if (Content is GalleryShell shell)
        {
            shell.RefreshTheme();
        }
    }
}
