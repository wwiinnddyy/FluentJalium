using FluentJalium.Controls.Themes;
using FluentJalium.Gallery.Shell;
using FluentJalium.Gallery.Services;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using FWBackdrop = FluentJalium.Controls.FWBackdrop;
using FWBackdropType = FluentJalium.Controls.FWBackdropType;

namespace FluentJalium.Gallery;

public sealed class MainWindow : Window
{
    private readonly FWBackdrop _backdrop;

    public MainWindow()
    {
        Title = "FluentJalium Gallery";
        Width = 1280;
        Height = 820;
        MinWidth = 980;
        MinHeight = 620;

        _backdrop = new FWBackdrop
        {
            Type = FWBackdropType.Mica,
            TintOpacity = 0.82,
            LuminosityOpacity = 0.85,
            FallbackColor = FallbackColorFor(FluentThemeManager.CurrentTheme)
        };

        var root = new Grid();
        root.Children.Add(_backdrop);
        root.Children.Add(new GalleryShell(this, new GalleryCatalogService(), ApplyTheme, ApplyAccent));

        Background = GalleryThemeResources.Brush("WindowBackground");
        Content = root;
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
        _backdrop.FallbackColor = FallbackColorFor(FluentThemeManager.CurrentTheme);

        if (Content is Grid root)
        {
            foreach (var child in root.Children)
            {
                if (child is GalleryShell shell)
                {
                    shell.RefreshTheme();
                }
            }
        }
    }

    private static Color FallbackColorFor(FluentThemeVariant theme)
    {
        // The Mica fallback shows wherever the compositor cannot sample behind the window
        // (and through the shell's transparent layers), so it must track the theme like
        // WindowBackground does. A fixed light fallback washes the content out in dark mode.
        return theme switch
        {
            FluentThemeVariant.Light => Color.FromRgb(0xF3, 0xF3, 0xF3),
            FluentThemeVariant.HighContrast => Color.FromRgb(0x00, 0x00, 0x00),
            _ => Color.FromRgb(0x20, 0x20, 0x20)
        };
    }
}
