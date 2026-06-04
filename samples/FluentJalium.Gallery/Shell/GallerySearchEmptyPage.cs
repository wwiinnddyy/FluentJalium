using FluentJalium.Gallery.Services;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using FWScrollViewer = FluentJalium.Controls.FWScrollViewer;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;

namespace FluentJalium.Gallery.Shell;

internal sealed class GallerySearchEmptyPage : Page
{
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        Content = CreateSearchEmptyContent(e.Parameter as string ?? string.Empty);
    }

    private static UIElement CreateSearchEmptyContent(string searchText)
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
                Spacing = 10,
                Margin = new Thickness(40, 36, 40, 40),
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 12,
                        Children =
                        {
                            FluentIconFactory.Regular(FluentIconRegular.SearchInfo24, 28, GalleryThemeResources.Brush("TextPrimary")),
                            new FWTextBlock
                            {
                                Text = "No results",
                                FontSize = 28,
                                FontFamily = "Segoe UI Variable Display",
                                Foreground = GalleryThemeResources.Brush("TextPrimary"),
                                VerticalAlignment = VerticalAlignment.Center
                            }
                        }
                    },
                    new FWTextBlock
                    {
                        Text = string.IsNullOrWhiteSpace(searchText)
                            ? "No Gallery pages are available."
                            : $"No Gallery pages match \"{searchText.Trim()}\".",
                        FontSize = 14,
                        Foreground = GalleryThemeResources.Brush("TextSecondary"),
                        TextWrapping = TextWrapping.Wrap
                    }
                }
            }
        };
    }
}
