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

internal sealed class GalleryControlGapPage
{
    public UIElement CreateContent()
    {
        var panel = CreateSection("Control Gap Matrix");
        var snapshot = CreateSnapshot();

        panel.Children.Add(CreateSummary(snapshot));

        var cards = new FWWrapPanel
        {
            HorizontalSpacing = 14,
            VerticalSpacing = 14
        };

        foreach (var entry in GalleryControlGapCatalog.CreateEntries())
        {
            cards.Children.Add(CreateGapCard(entry));
        }

        panel.Children.Add(cards);
        return panel;
    }

    internal static GalleryControlGapSnapshot CreateSnapshot()
    {
        return GalleryControlGapCatalog.CreateSnapshot();
    }

    internal static string FormatGapEntry(GalleryControlGapEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var required = entry.RequiredBeforePublicApi.Length == 0
            ? "none"
            : string.Join(", ", entry.RequiredBeforePublicApi);

        return $"{entry.CandidateControl}: priority {entry.Priority}, area {entry.AreaId}, reference {entry.ReferenceInput}, stage {entry.Stage}, page {entry.PageId}, sample {entry.SampleCodeKey}, decision {entry.Decision}, next {entry.NextAction}, evidence {string.Join("; ", entry.Evidence)}, public API blockers {required}.";
    }

    private static FWBorder CreateSummary(GalleryControlGapSnapshot snapshot)
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
                    CreateHeaderRow(FluentIconRegular.DocumentBulletList24, "Backlog snapshot"),
                    new FWTextBlock
                    {
                        Text = GalleryControlGapCatalog.FormatSnapshot(snapshot),
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
                            CreatePill($"{snapshot.EntryCount} candidates"),
                            CreatePill($"{snapshot.PublicControlCount} public FW"),
                            CreatePill($"{snapshot.RecipeOnlyCount} recipes"),
                            CreatePill($"{snapshot.EvaluateCount} evaluate"),
                            CreatePill($"{snapshot.RenderedQaRequiredCount} rendered QA"),
                            CreatePill($"P0/P1/P2 {snapshot.P0Count}/{snapshot.P1Count}/{snapshot.P2Count}")
                        }
                    }
                }
            }
        };
    }

    private static FWBorder CreateGapCard(GalleryControlGapEntry entry)
    {
        return new FWBorder
        {
            Width = 420,
            MinHeight = 250,
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
                    CreateHeaderRow(ResolveIcon(entry), entry.CandidateControl),
                    CreateWrap("Stage", [entry.Priority, entry.Stage, entry.AreaId, entry.ReferenceInput]),
                    CreateMetaText(entry.Decision),
                    CreateMetaText($"Next: {entry.NextAction}"),
                    CreateWrap("Evidence", entry.Evidence),
                    CreateWrap("Before public API", entry.RequiredBeforePublicApi.Length == 0 ? ["No blocker recorded"] : entry.RequiredBeforePublicApi),
                    CreateMetaText($"{entry.PageId} / {entry.SampleCodeKey}")
                }
            }
        };
    }

    private static FluentIconRegular ResolveIcon(GalleryControlGapEntry entry)
    {
        if (entry.IsPublicFwControl)
        {
            return FluentIconRegular.CheckmarkCircle24;
        }

        if (entry.IsRecipeOnly)
        {
            return FluentIconRegular.Beaker24;
        }

        return entry.RequiresRenderedQa
            ? FluentIconRegular.WindowWrench24
            : FluentIconRegular.QuestionCircle24;
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
            MaxWidth = 360,
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
                        FluentIconFactory.Regular(FluentIconRegular.DocumentBulletList24, 24, ThemeBrush("TextPrimary")),
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
