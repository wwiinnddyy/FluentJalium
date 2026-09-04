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
/// WinUI Gallery's section page: every sample of one navigation group as cards.
/// Reached by invoking a group header; card activation routes through
/// <see cref="GalleryNavigationBroker"/>.
/// </summary>
public sealed class GallerySectionPage : Page
{
    private readonly GalleryLocalizationService _localization = new();

    public void ApplyNavigationParameter(object? parameter)
    {
        Content = CreateContent(parameter as string ?? string.Empty);
    }

    internal static GalleryPageInfo[] CreateSnapshot(GalleryPageInfo[] pageInfos, string groupId)
    {
        ArgumentNullException.ThrowIfNull(pageInfos);

        if (string.IsNullOrWhiteSpace(groupId))
        {
            return [];
        }

        return pageInfos
            .Where(page => !page.IsFooter && string.Equals(page.GroupId, groupId, StringComparison.Ordinal))
            .ToArray();
    }

    private UIElement CreateContent(string groupId)
    {
        var localization = new GalleryLocalizationService();
        var pageInfos = GalleryCatalog.CreatePageInfos(localization);
        var matches = CreateSnapshot(pageInfos, groupId);

        var panel = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 16,
            Margin = new Thickness(36, 24, 36, 36)
        };

        panel.Children.Add(new FWTextBlock
        {
            Text = matches.Length > 0 ? matches[0].Group : localization.GroupName(groupId),
            FontSize = 28,
            FontFamily = "Segoe UI Variable Display",
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
            grid.Children.Add(CreateSectionCard(match));
        }
        AutomationProperties.SetAutomationId(grid, "ItemGridView");
        panel.Children.Add(grid);

        return new FWScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Content = panel
        };
    }

    private static UIElement CreateSectionCard(GalleryPageInfo page)
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

    private static Brush ThemeBrush(string key)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Brush brush)
        {
            return brush;
        }

        return new SolidColorBrush(Colors.Transparent);
    }
}
