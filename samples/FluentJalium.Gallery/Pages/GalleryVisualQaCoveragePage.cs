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

internal sealed class GalleryVisualQaCoveragePage
{
    public UIElement CreateContent()
    {
        var panel = CreateSection("Visual QA Coverage");
        var snapshot = CreateSnapshot();

        panel.Children.Add(CreateSummary(snapshot));

        var cards = new FWWrapPanel
        {
            HorizontalSpacing = 14,
            VerticalSpacing = 14
        };

        foreach (var family in GalleryVisualQaCoverageCatalog.CreateFamilies())
        {
            cards.Children.Add(CreateFamilyCard(family));
        }

        panel.Children.Add(cards);
        return panel;
    }

    internal static GalleryVisualQaCoverageSnapshot CreateSnapshot()
    {
        return GalleryVisualQaCoverageCatalog.CreateSnapshot();
    }

    internal static string FormatFamilyCoverage(GalleryVisualQaCoverageFamily family)
    {
        ArgumentNullException.ThrowIfNull(family);

        return $"{family.Title}: page {family.PageId}, sample {family.SampleCodeKey}, controls {string.Join(", ", family.Controls)}, states {string.Join(", ", family.CoveredStates)}, evidence {string.Join("; ", family.Evidence)}.";
    }

    private static FWBorder CreateSummary(GalleryVisualQaCoverageSnapshot snapshot)
    {
        return new FWBorder
        {
            Background = ThemeBrush("LayerFillColorDefaultBrush"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(14),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 10,
                Children =
                {
                    CreateHeaderRow(FluentIconRegular.DataUsage24, "Coverage snapshot"),
                    new FWTextBlock
                    {
                        Text = GalleryVisualQaCoverageCatalog.FormatSnapshot(snapshot),
                        FontSize = 13,
                        Foreground = ThemeBrush("TextSecondary"),
                        TextWrapping = TextWrapping.Wrap
                    },
                    new FWWrapPanel
                    {
                        HorizontalSpacing = 8,
                        VerticalSpacing = 8,
                        Children =
                        {
                            CreatePill($"{snapshot.FamilyCount} families"),
                            CreatePill($"{snapshot.ControlCount} controls"),
                            CreatePill($"{snapshot.StateCount} state tokens"),
                            CreatePill($"{snapshot.DiagnosticFamilyCount} diagnostic families")
                        }
                    }
                }
            }
        };
    }

    private static FWBorder CreateFamilyCard(GalleryVisualQaCoverageFamily family)
    {
        return new FWBorder
        {
            Width = 380,
            MinHeight = 230,
            Background = ThemeBrush("ControlBackground"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(14),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 10,
                Children =
                {
                    CreateHeaderRow(FluentIconRegular.AppFolder24, family.Title),
                    CreateMetaText($"{family.PageId} / {family.SampleCodeKey}"),
                    CreateWrap("Controls", family.Controls),
                    CreateWrap("States", family.CoveredStates),
                    CreateWrap("Evidence", family.Evidence)
                }
            }
        };
    }

    private static UIElement CreateWrap(string title, IReadOnlyList<string> values)
    {
        var stack = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 5
        };

        stack.Children.Add(CreateMetaText(title));

        var wrap = new FWWrapPanel
        {
            HorizontalSpacing = 6,
            VerticalSpacing = 6
        };

        foreach (var value in values)
        {
            wrap.Children.Add(CreateChip(value));
        }

        stack.Children.Add(wrap);
        return stack;
    }

    private static FWStackPanel CreateHeaderRow(FluentIconRegular icon, string text)
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                FluentIconFactory.Regular(icon, 18, ThemeBrush("TextPrimary")),
                new FWTextBlock
                {
                    Text = text,
                    FontSize = 15,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = ThemeBrush("TextPrimary"),
                    TextWrapping = TextWrapping.Wrap,
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
    }

    private static FWTextBlock CreateMetaText(string text)
    {
        return new FWTextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        };
    }

    private static FWBorder CreatePill(string text)
    {
        return new FWBorder
        {
            Background = ThemeBrush("ControlBackground"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(999),
            Padding = new Thickness(10, 5, 10, 5),
            Child = CreateMetaText(text)
        };
    }

    private static FWBorder CreateChip(string text)
    {
        return new FWBorder
        {
            MaxWidth = 330,
            Background = ThemeBrush("LayerFillColorDefaultBrush"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(8, 4, 8, 4),
            Child = CreateMetaText(text)
        };
    }

    private static FWStackPanel CreateSection(string title)
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 16,
            Children =
            {
                new FWStackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 10,
                    Children =
                    {
                        FluentIconFactory.Regular(FluentIconRegular.DataUsage24, 24, ThemeBrush("TextPrimary")),
                        new FWTextBlock
                        {
                            Text = title,
                            FontSize = 22,
                            Foreground = ThemeBrush("TextPrimary"),
                            VerticalAlignment = VerticalAlignment.Center
                        }
                    }
                }
            }
        };
    }

    private static Brush ThemeBrush(string key)
    {
        return GalleryThemeResources.Brush(key);
    }
}
