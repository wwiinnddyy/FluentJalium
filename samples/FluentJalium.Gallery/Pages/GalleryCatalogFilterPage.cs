using FluentJalium.Gallery.Controls;
using FluentJalium.Gallery.Models;
using FluentJalium.Gallery.Services;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;

namespace FluentJalium.Gallery.Pages;

internal sealed class GalleryCatalogFilterPage
{
    private readonly GalleryCatalogFilter _filter;
    private readonly GalleryControlInfo[] _controls;
    private readonly Action<string> _navigate;

    public GalleryCatalogFilterPage(GalleryCatalogFilter filter, IEnumerable<GalleryPageInfo> pages, Action<string>? navigate = null)
    {
        _filter = filter;
        _controls = GalleryControlInfo.CreateFromPages(pages);
        _navigate = navigate ?? (_ => { });
    }

    public UIElement CreateContent()
    {
        var snapshot = CreateSnapshot();

        var panel = CreateSection();
        panel.Children.Add(CreateSummary(snapshot));

        var cards = new FWWrapPanel
        {
            HorizontalSpacing = 14,
            VerticalSpacing = 14
        };

        foreach (var control in snapshot.Matches)
        {
            cards.Children.Add(CreateControlCard(control));
        }

        if (snapshot.Matches.Length == 0)
        {
            cards.Children.Add(CreateEmptyState());
        }

        panel.Children.Add(cards);
        return panel;
    }

    internal GalleryCatalogFilterSnapshot CreateSnapshot()
    {
        return GalleryCatalogFilterSnapshot.Create(_filter, _controls);
    }

    private FWStackPanel CreateSection()
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 16
        };
    }

    private FWBorder CreateSummary(GalleryCatalogFilterSnapshot snapshot)
    {
        return new FWBorder
        {
            Background = GalleryThemeResources.Brush("LayerFillColorDefaultBrush"),
            BorderBrush = GalleryThemeResources.Brush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(14),
            Child = new FWWrapPanel
            {
                HorizontalSpacing = 8,
                VerticalSpacing = 8,
                Children =
                {
                    CreateSummaryPill(FluentIconRegular.ControlButton24, $"{snapshot.ControlCount} controls"),
                    CreateSummaryPill(FluentIconRegular.DocumentBulletList24, $"{snapshot.PageCount} pages"),
                    CreateSummaryPill(FluentIconRegular.Tag24, $"{snapshot.GroupCounts.Length} groups"),
                    CreateSummaryPill(FluentIconRegular.New24, $"{snapshot.NewCount} new"),
                    CreateSummaryPill(FluentIconRegular.ArrowClockwise24, $"{snapshot.UpdatedCount} updated"),
                    CreateSummaryPill(FluentIconRegular.Sparkle24, $"{snapshot.PreviewCount} preview"),
                    CreateSummaryPill(FluentIconRegular.DataUsage24, $"{snapshot.DiagnosticCount} diagnostic"),
                    CreateSummaryPill(FluentIconRegular.FolderOpen24, $"{snapshot.WithSourcePathCount} sources"),
                    CreateSummaryPill(FluentIconRegular.ClipboardCode24, $"{snapshot.WithSampleCodeKeyCount} samples"),
                    CreateSummaryPill(FluentIconRegular.Braces24, $"{snapshot.WithApiNamespaceCount} api")
                }
            }
        };
    }

    private UIElement CreateControlCard(GalleryControlInfo control)
    {
        var card = new GalleryControlCard
        {
            Title = control.Name,
            Subtitle = control.Page.Group,
            Glyph = GalleryGlyph.Glyph(control.Page.Icon),
        };
        var target = control.Page.UniqueId;
        card.Activated += (_, _) => _navigate(target);
        return card;
    }

    private static FWBorder CreateSummaryPill(FluentIconRegular icon, string text)
    {
        return new FWBorder
        {
            Background = GalleryThemeResources.Brush("ControlBackground"),
            BorderBrush = GalleryThemeResources.Brush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(999),
            Padding = new Thickness(10, 5, 10, 5),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 6,
                Children =
                {
                    GalleryGlyph.Create(icon, 14, GalleryThemeResources.Brush("TextSecondary")),
                    new FWTextBlock
                    {
                        Text = text,
                        FontSize = 12,
                        Foreground = GalleryThemeResources.Brush("TextSecondary"),
                        VerticalAlignment = VerticalAlignment.Center
                    }
                }
            }
        };
    }

    private static FWBorder CreateChip(string text)
    {
        return new FWBorder
        {
            MaxWidth = 330,
            Background = GalleryThemeResources.Brush("LayerFillColorDefaultBrush"),
            BorderBrush = GalleryThemeResources.Brush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(999),
            Padding = new Thickness(8, 3, 8, 3),
            Child = new FWTextBlock
            {
                Text = text,
                FontSize = 11,
                Foreground = GalleryThemeResources.Brush("TextSecondary"),
                TextWrapping = TextWrapping.Wrap
            }
        };
    }

    private static FWBorder CreateEmptyState()
    {
        return new FWBorder
        {
            Width = 360,
            Background = GalleryThemeResources.Brush("ControlBackground"),
            BorderBrush = GalleryThemeResources.Brush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(14),
            Child = new FWTextBlock
            {
                Text = "No catalog entries match this filter yet.",
                FontSize = 13,
                Foreground = GalleryThemeResources.Brush("TextSecondary"),
                TextWrapping = TextWrapping.Wrap
            }
        };
    }
}
