using FluentJalium.Gallery.Models;
using FluentJalium.Gallery.Services;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using FWScrollViewer = FluentJalium.Controls.FWScrollViewer;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;

namespace FluentJalium.Gallery.Shell;

internal sealed class GalleryHostPage : Page
{
    private GalleryPage? _galleryPage;

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is GalleryPage page)
        {
            LoadPage(page);
        }
    }

    public void RefreshTheme()
    {
        if (_galleryPage != null)
        {
            LoadPage(_galleryPage);
        }
    }

    private void LoadPage(GalleryPage page)
    {
        _galleryPage = page;
        Title = page.Title;
        Content = CreatePageContent(page);
    }

    private static UIElement CreatePageContent(GalleryPage page)
    {
        return new FWScrollViewer
        {
            Background = GalleryThemeResources.Brush("NavigationViewContentBackground"),
            Padding = new Thickness(0),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            IsScrollBarAutoHideEnabled = true,
            Content = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 18,
                Margin = new Thickness(40, 32, 40, 40),
                Children =
                {
                    CreatePageHeader(page),
                    new FWTextBlock
                    {
                        Text = page.Description,
                        FontSize = 14,
                        Foreground = GalleryThemeResources.Brush("TextSecondary"),
                        TextWrapping = TextWrapping.Wrap
                    },
                    page.CreateContent()
                }
            }
        };
    }

    private static UIElement CreatePageHeader(GalleryPage page)
    {
        var header = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Children =
            {
                CreateIcon(page.Icon, 30),
                new FWTextBlock
                {
                    Text = page.Title,
                    FontSize = 30,
                    FontFamily = "Segoe UI Variable Display",
                    Foreground = GalleryThemeResources.Brush("TextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };

        if (page.Status != GalleryPageStatus.Stable)
        {
            header.Children.Add(CreateStatusPill(page.Status));
        }

        return header;
    }

    private static UIElement CreateStatusPill(GalleryPageStatus status)
    {
        return new Border
        {
            Background = GalleryThemeResources.Brush("LayerFillColorDefaultBrush"),
            BorderBrush = GalleryThemeResources.Brush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(999),
            Padding = new Thickness(9, 3, 9, 3),
            VerticalAlignment = VerticalAlignment.Center,
            Child = new FWTextBlock
            {
                Text = status.ToString(),
                FontSize = 12,
                Foreground = GalleryThemeResources.Brush("TextSecondary")
            }
        };
    }

    private static FluentIcon CreateIcon(FluentIconRegular icon, double size)
    {
        return FluentIconFactory.Regular(icon, size, GalleryThemeResources.Brush("TextPrimary"));
    }
}
