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

    public GalleryCatalogFilterPage(GalleryCatalogFilter filter, IEnumerable<GalleryPageInfo> pages)
    {
        _filter = filter;
        _controls = GalleryControlInfo.CreateFromPages(pages);
    }

    public UIElement CreateContent()
    {
        var matches = _controls
            .Where(MatchesFilter)
            .OrderBy(page => page.Group, StringComparer.Ordinal)
            .ThenBy(page => page.Page.Title, StringComparer.Ordinal)
            .ThenBy(page => page.Name, StringComparer.Ordinal)
            .ToArray();

        var panel = CreateSection();
        panel.Children.Add(CreateSummary(matches));

        var cards = new FWWrapPanel
        {
            HorizontalSpacing = 14,
            VerticalSpacing = 14
        };

        foreach (var control in matches)
        {
            cards.Children.Add(CreateControlCard(control));
        }

        if (matches.Length == 0)
        {
            cards.Children.Add(CreateEmptyState());
        }

        panel.Children.Add(cards);
        return panel;
    }

    private bool MatchesFilter(GalleryControlInfo control)
    {
        return _filter switch
        {
            GalleryCatalogFilter.AllControls => true,
            GalleryCatalogFilter.New => control.IsNew,
            GalleryCatalogFilter.Updated => control.IsUpdated,
            GalleryCatalogFilter.Preview => control.Status == GalleryPageStatus.Preview,
            GalleryCatalogFilter.Diagnostic => control.Status == GalleryPageStatus.Diagnostic,
            _ => false
        };
    }

    private FWStackPanel CreateSection()
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 16
        };
    }

    private FWBorder CreateSummary(IReadOnlyCollection<GalleryControlInfo> matches)
    {
        var previewCount = matches.Count(control => control.Status == GalleryPageStatus.Preview);
        var diagnosticCount = matches.Count(control => control.Status == GalleryPageStatus.Diagnostic);
        var newCount = matches.Count(control => control.IsNew);
        var updatedCount = matches.Count(control => control.IsUpdated);
        var pageCount = matches.Select(control => control.Page.UniqueId).Distinct(StringComparer.Ordinal).Count();

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
                    CreateSummaryPill(FluentIconRegular.ControlButton24, $"{matches.Count} controls"),
                    CreateSummaryPill(FluentIconRegular.DocumentBulletList24, $"{pageCount} pages"),
                    CreateSummaryPill(FluentIconRegular.New24, $"{newCount} new"),
                    CreateSummaryPill(FluentIconRegular.ArrowClockwise24, $"{updatedCount} updated"),
                    CreateSummaryPill(FluentIconRegular.Sparkle24, $"{previewCount} preview"),
                    CreateSummaryPill(FluentIconRegular.DataUsage24, $"{diagnosticCount} diagnostic")
                }
            }
        };
    }

    private static FWBorder CreateControlCard(GalleryControlInfo control)
    {
        var body = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 9,
            Children =
            {
                CreateCardHeader(control),
                new FWTextBlock
                {
                    Text = control.Page.Title,
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = GalleryThemeResources.Brush("TextSecondary"),
                    TextWrapping = TextWrapping.Wrap
                },
                new FWTextBlock
                {
                    Text = control.Page.Description,
                    FontSize = 12,
                    Foreground = GalleryThemeResources.Brush("TextSecondary"),
                    TextWrapping = TextWrapping.Wrap
                },
                CreateMetadataWrap(control)
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

    private static UIElement CreateCardHeader(GalleryControlInfo control)
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                FluentIconFactory.Regular(control.Icon, 20, GalleryThemeResources.Brush("TextPrimary")),
                new FWTextBlock
                {
                    Text = control.Name,
                    FontSize = 15,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = GalleryThemeResources.Brush("TextPrimary"),
                    TextWrapping = TextWrapping.Wrap,
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
    }

    private static FWWrapPanel CreateMetadataWrap(GalleryControlInfo control)
    {
        var wrap = new FWWrapPanel
        {
            HorizontalSpacing = 6,
            VerticalSpacing = 6
        };

        wrap.Children.Add(CreateChip(control.Group));
        wrap.Children.Add(CreateChip(control.Status.ToString()));

        if (control.IsNew)
        {
            wrap.Children.Add(CreateChip("New"));
        }

        if (control.IsUpdated)
        {
            wrap.Children.Add(CreateChip("Updated"));
        }

        AddOptionalChip(wrap, control.ApiNamespace);
        AddOptionalChip(wrap, control.SourcePath);
        AddOptionalChip(wrap, control.SampleCodeKey);
        AddOptionalChip(wrap, control.Page.UniqueId);

        foreach (var baseClass in control.BaseClasses)
        {
            wrap.Children.Add(CreateChip(baseClass));
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
