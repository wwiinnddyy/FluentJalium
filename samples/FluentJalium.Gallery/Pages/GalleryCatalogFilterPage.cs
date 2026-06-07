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
    private readonly GalleryPageInfo[] _pages;

    public GalleryCatalogFilterPage(GalleryCatalogFilter filter, IEnumerable<GalleryPageInfo> pages)
    {
        _filter = filter;
        _pages = pages
            .Where(page => page.Group != GalleryNavigationGroup.Catalog)
            .ToArray();
    }

    public UIElement CreateContent()
    {
        var matches = _pages
            .Where(MatchesFilter)
            .OrderBy(page => page.Group, StringComparer.Ordinal)
            .ThenBy(page => page.Title, StringComparer.Ordinal)
            .ToArray();

        var panel = CreateSection();
        panel.Children.Add(CreateSummary(matches));

        var cards = new FWWrapPanel
        {
            HorizontalSpacing = 14,
            VerticalSpacing = 14
        };

        foreach (var page in matches)
        {
            cards.Children.Add(CreatePageCard(page));
        }

        if (matches.Length == 0)
        {
            cards.Children.Add(CreateEmptyState());
        }

        panel.Children.Add(cards);
        return panel;
    }

    private bool MatchesFilter(GalleryPageInfo page)
    {
        return _filter switch
        {
            GalleryCatalogFilter.AllControls => IsControlEntry(page),
            GalleryCatalogFilter.New => IsControlEntry(page) && page.IsNew,
            GalleryCatalogFilter.Updated => IsControlEntry(page) && page.IsUpdated,
            GalleryCatalogFilter.Preview => IsControlEntry(page) && page.Status == GalleryPageStatus.Preview,
            GalleryCatalogFilter.Diagnostic => page.Status == GalleryPageStatus.Diagnostic,
            _ => false
        };
    }

    private static bool IsControlEntry(GalleryPageInfo page)
    {
        return !page.IsFooter
            && page.Group != GalleryNavigationGroup.Home
            && page.Tags.Any(tag => tag.StartsWith("FW", StringComparison.Ordinal));
    }

    private FWStackPanel CreateSection()
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 16
        };
    }

    private FWBorder CreateSummary(IReadOnlyCollection<GalleryPageInfo> matches)
    {
        var previewCount = matches.Count(page => page.Status == GalleryPageStatus.Preview);
        var diagnosticCount = matches.Count(page => page.Status == GalleryPageStatus.Diagnostic);
        var newCount = matches.Count(page => page.IsNew);
        var updatedCount = matches.Count(page => page.IsUpdated);

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
                    CreateSummaryPill(FluentIconRegular.DocumentBulletList24, $"{matches.Count} entries"),
                    CreateSummaryPill(FluentIconRegular.New24, $"{newCount} new"),
                    CreateSummaryPill(FluentIconRegular.ArrowClockwise24, $"{updatedCount} updated"),
                    CreateSummaryPill(FluentIconRegular.Sparkle24, $"{previewCount} preview"),
                    CreateSummaryPill(FluentIconRegular.DataUsage24, $"{diagnosticCount} diagnostic")
                }
            }
        };
    }

    private static FWBorder CreatePageCard(GalleryPageInfo page)
    {
        var body = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 9,
            Children =
            {
                CreateCardHeader(page),
                new FWTextBlock
                {
                    Text = page.Description,
                    FontSize = 12,
                    Foreground = GalleryThemeResources.Brush("TextSecondary"),
                    TextWrapping = TextWrapping.Wrap
                },
                CreateMetadataWrap(page)
            }
        };

        return new FWBorder
        {
            Width = 360,
            MinHeight = 190,
            Background = GalleryThemeResources.Brush("ControlBackground"),
            BorderBrush = GalleryThemeResources.Brush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(14),
            Child = body
        };
    }

    private static UIElement CreateCardHeader(GalleryPageInfo page)
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                FluentIconFactory.Regular(page.Icon, 20, GalleryThemeResources.Brush("TextPrimary")),
                new FWTextBlock
                {
                    Text = page.Title,
                    FontSize = 15,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = GalleryThemeResources.Brush("TextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
    }

    private static FWWrapPanel CreateMetadataWrap(GalleryPageInfo page)
    {
        var wrap = new FWWrapPanel
        {
            HorizontalSpacing = 6,
            VerticalSpacing = 6
        };

        wrap.Children.Add(CreateChip(page.Group));
        wrap.Children.Add(CreateChip(page.Status.ToString()));

        if (page.IsNew)
        {
            wrap.Children.Add(CreateChip("New"));
        }

        if (page.IsUpdated)
        {
            wrap.Children.Add(CreateChip("Updated"));
        }

        AddOptionalChip(wrap, page.ApiNamespace);
        AddOptionalChip(wrap, page.SourcePath);
        AddOptionalChip(wrap, page.SampleCodeKey);

        foreach (var baseClass in page.BaseClasses ?? [])
        {
            wrap.Children.Add(CreateChip(baseClass));
        }

        foreach (var relatedControl in page.RelatedControls.Take(5))
        {
            wrap.Children.Add(CreateChip(relatedControl));
        }

        return wrap;
    }

    private static void AddOptionalChip(FWWrapPanel wrap, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            wrap.Children.Add(CreateChip(value));
        }
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
                    FluentIconFactory.Regular(icon, 14, GalleryThemeResources.Brush("TextSecondary")),
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
            Background = GalleryThemeResources.Brush("LayerFillColorDefaultBrush"),
            BorderBrush = GalleryThemeResources.Brush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(999),
            Padding = new Thickness(8, 3, 8, 3),
            Child = new FWTextBlock
            {
                Text = text,
                FontSize = 11,
                Foreground = GalleryThemeResources.Brush("TextSecondary")
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
