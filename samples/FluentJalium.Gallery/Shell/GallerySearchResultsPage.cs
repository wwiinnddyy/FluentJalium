using FluentJalium.Gallery.Controls;
using FluentJalium.Gallery.Models;
using FluentJalium.Gallery.Services;
using Jalium.UI;
using Jalium.UI.Automation;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using FWScrollViewer = FluentJalium.Controls.FWScrollViewer;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;

namespace FluentJalium.Gallery.Shell;

/// <summary>
/// WinUI Gallery's search results: matching samples as cards, or the no-results state.
/// Card activation routes through <see cref="GalleryNavigationBroker"/> because the frame
/// instantiates pages without callbacks.
/// </summary>
public sealed class GallerySearchResultsPage : Page
{
    private readonly GalleryLocalizationService _localization = new();

    public void ApplyNavigationParameter(object? parameter)
    {
        Content = CreateContent(parameter as string ?? string.Empty);
    }

    internal static GalleryPageInfo[] CreateSnapshot(GalleryPageInfo[] pageInfos, string query)
    {
        ArgumentNullException.ThrowIfNull(pageInfos);

        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        return pageInfos
            .Where(page => !page.IsFooter && page.MatchesSearch(query))
            .ToArray();
    }

    private UIElement CreateContent(string query)
    {
        var pageInfos = GalleryCatalog.CreatePageInfos(new GalleryLocalizationService());
        var matches = CreateSnapshot(pageInfos, query);

        var panel = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 16,
            Margin = new Thickness(36, 24, 36, 36)
        };

        if (matches.Length == 0)
        {
            panel.Children.Add(new FWTextBlock
            {
                Text = _localization.Text("search.noResults"),
                FontSize = 20,
                FontWeight = FontWeights.SemiBold,
                Foreground = ThemeBrush("TextPrimary")
            });
            panel.Children.Add(new FWTextBlock
            {
                Text = string.Format(_localization.Text("search.noResultsHint"), query.Trim()),
                FontSize = 14,
                Foreground = ThemeBrush("TextSecondary"),
                TextWrapping = TextWrapping.Wrap
            });
            return WrapInScroller(panel);
        }

        panel.Children.Add(new FWTextBlock
        {
            Text = string.Format(_localization.Text("search.resultsCount"), matches.Length, query.Trim()),
            FontSize = 20,
            FontWeight = FontWeights.SemiBold,
            Foreground = ThemeBrush("TextPrimary")
        });

        var grid = new FWWrapPanel
        {
            HorizontalSpacing = 12,
            VerticalSpacing = 12
        };
        foreach (var match in matches)
        {
            grid.Children.Add(CreateResultCard(match));
        }
        AutomationProperties.SetAutomationId(grid, "ResultsGridView");
        panel.Children.Add(grid);

        return WrapInScroller(panel);
    }

    private static UIElement CreateResultCard(GalleryPageInfo page)
    {
        var card = new GalleryControlCard
        {
            Title = page.Title,
            Subtitle = page.Group,
            Glyph = GalleryGlyph.Glyph(page.Icon)
        };
        var target = page.UniqueId;
        card.Activated += (_, _) => GalleryNavigationBroker.RequestNavigate(target);
        return card;
    }

    private static UIElement WrapInScroller(UIElement content)
    {
        return new FWScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Content = content
        };
    }

    private static Brush ThemeBrush(string key)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Brush brush)
        {
            return brush;
        }

        return new SolidColorBrush(Colors.Transparent);
    }
}
