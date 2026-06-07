using FluentJalium.Gallery.Models;
using FluentJalium.Gallery.Services;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWButton = FluentJalium.Controls.FWButton;
using FWButtonDensity = FluentJalium.Controls.FWButtonDensity;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;
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
                    CreateMetadataPanel(page),
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

    private static UIElement CreateMetadataPanel(GalleryPage page)
    {
        var panel = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Children =
            {
                CreateMetadataSummary(page)
            }
        };

        AddOptionalMetadataLine(panel, FluentIconRegular.FolderOpen24, "Source", page.SourcePath, true);
        AddOptionalMetadataLine(panel, FluentIconRegular.Code24, "Sample", page.SampleCodeKey, true);
        AddOptionalMetadataLine(panel, FluentIconRegular.Braces24, "API", page.ApiNamespace, true);

        if (page.BaseClasses.Count > 0)
        {
            panel.Children.Add(CreateMetadataGroup(FluentIconRegular.BranchFork24, "Base", page.BaseClasses));
        }

        if (page.RelatedControls.Count > 0)
        {
            panel.Children.Add(CreateMetadataGroup(FluentIconRegular.Tag24, "Related", page.RelatedControls));
        }

        if (page.DocumentationLinks.Count > 0)
        {
            panel.Children.Add(CreateDocumentationGroup(page.DocumentationLinks));
        }

        if (GallerySampleCodeRegistry.TryGetSampleCode(page.Info, out var sampleCode))
        {
            panel.Children.Add(CreateSampleCodeBlock(sampleCode));
        }

        return new FWBorder
        {
            Background = GalleryThemeResources.Brush("ControlBackground"),
            BorderBrush = GalleryThemeResources.Brush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(14),
            Child = panel
        };
    }

    private static UIElement CreateMetadataSummary(GalleryPage page)
    {
        var row = new FWWrapPanel
        {
            HorizontalSpacing = 6,
            VerticalSpacing = 6
        };

        row.Children.Add(CreateMetadataPill(page.Group));
        row.Children.Add(CreateMetadataPill(page.UniqueId));
        row.Children.Add(CreateMetadataPill(page.Status.ToString()));

        if (page.IsNew)
        {
            row.Children.Add(CreateMetadataPill("New"));
        }

        if (page.IsUpdated)
        {
            row.Children.Add(CreateMetadataPill("Updated"));
        }

        return row;
    }

    private static void AddOptionalMetadataLine(
        FWStackPanel panel,
        FluentIconRegular icon,
        string label,
        string? value,
        bool canCopy)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            panel.Children.Add(CreateMetadataLine(icon, label, value, canCopy));
        }
    }

    private static UIElement CreateMetadataLine(
        FluentIconRegular icon,
        string label,
        string value,
        bool canCopy,
        double labelWidth = 58)
    {
        var row = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        var iconElement = CreateIcon(icon, 16, "TextSecondary");
        iconElement.VerticalAlignment = VerticalAlignment.Center;
        Grid.SetColumn(iconElement, 0);
        row.Children.Add(iconElement);

        var labelElement = new FWTextBlock
        {
            Text = label,
            Width = labelWidth,
            Margin = new Thickness(8, 0, 10, 0),
            FontSize = 12,
            FontWeight = FontWeights.SemiBold,
            Foreground = GalleryThemeResources.Brush("TextSecondary"),
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(labelElement, 1);
        row.Children.Add(labelElement);

        var valueElement = new FWTextBlock
        {
            Text = value,
            FontFamily = label is "Source" or "Sample" or "API" ? "Cascadia Code" : "Segoe UI Variable Text",
            FontSize = 12,
            Foreground = GalleryThemeResources.Brush("TextPrimary"),
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(valueElement, 2);
        row.Children.Add(valueElement);

        if (canCopy)
        {
            var button = CreateCopyButton(value);
            Grid.SetColumn(button, 3);
            row.Children.Add(button);
        }

        return row;
    }

    private static UIElement CreateMetadataGroup(
        FluentIconRegular icon,
        string label,
        IReadOnlyList<string> values)
    {
        var group = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 6,
            Children =
            {
                CreateMetadataGroupHeader(icon, label)
            }
        };

        var wrap = new FWWrapPanel
        {
            HorizontalSpacing = 6,
            VerticalSpacing = 6
        };

        foreach (var value in values)
        {
            wrap.Children.Add(CreateMetadataPill(value));
        }

        group.Children.Add(wrap);
        return group;
    }

    private static UIElement CreateDocumentationGroup(IReadOnlyList<GalleryDocumentationLink> links)
    {
        var group = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 6,
            Children =
            {
                CreateMetadataGroupHeader(FluentIconRegular.DocumentLink24, "Docs")
            }
        };

        foreach (var link in links)
        {
            group.Children.Add(CreateMetadataLine(FluentIconRegular.Link24, link.Title, link.Uri, true, 120));
        }

        return group;
    }

    private static UIElement CreateMetadataGroupHeader(FluentIconRegular icon, string label)
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                CreateIcon(icon, 16, "TextSecondary"),
                new FWTextBlock
                {
                    Text = label,
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = GalleryThemeResources.Brush("TextSecondary"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
    }

    private static UIElement CreateSampleCodeBlock(string sampleCode)
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 6,
            Children =
            {
                CreateSampleCodeHeader(sampleCode),
                new FWBorder
                {
                    Background = GalleryThemeResources.Brush("LayerFillColorDefaultBrush"),
                    BorderBrush = GalleryThemeResources.Brush("ControlBorder"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(10),
                    Child = new FWTextBlock
                    {
                        Text = sampleCode,
                        FontFamily = "Cascadia Code",
                        FontSize = 12,
                        Foreground = GalleryThemeResources.Brush("TextPrimary"),
                        TextWrapping = TextWrapping.Wrap
                    }
                }
            }
        };
    }

    private static UIElement CreateSampleCodeHeader(string sampleCode)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        var header = CreateMetadataGroupHeader(FluentIconRegular.ClipboardCode24, "Code");
        Grid.SetColumn(header, 0);
        grid.Children.Add(header);

        var copyButton = CreateCopyButton(sampleCode, "Copy code");
        Grid.SetColumn(copyButton, 1);
        grid.Children.Add(copyButton);

        return grid;
    }

    private static FWButton CreateCopyButton(string text, string label = "Copy")
    {
        var button = new FWButton
        {
            Density = FWButtonDensity.Compact,
            Content = CreateButtonContent(FluentIconRegular.Copy24, label),
            Margin = new Thickness(10, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center
        };

        button.Click += (_, _) => Clipboard.SetText(text);
        return button;
    }

    private static UIElement CreateButtonContent(FluentIconRegular icon, string text)
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Children =
            {
                CreateIcon(icon, 14, "TextPrimary"),
                new FWTextBlock
                {
                    Text = text,
                    FontSize = 12,
                    Foreground = GalleryThemeResources.Brush("TextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
    }

    private static UIElement CreateStatusPill(GalleryPageStatus status)
    {
        return CreateMetadataPill(status.ToString());
    }

    private static UIElement CreateMetadataPill(string text)
    {
        return new FWBorder
        {
            Background = GalleryThemeResources.Brush("LayerFillColorDefaultBrush"),
            BorderBrush = GalleryThemeResources.Brush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(999),
            Padding = new Thickness(9, 3, 9, 3),
            VerticalAlignment = VerticalAlignment.Center,
            Child = new FWTextBlock
            {
                Text = text,
                FontSize = 12,
                Foreground = GalleryThemeResources.Brush("TextSecondary")
            }
        };
    }

    private static FluentIcon CreateIcon(FluentIconRegular icon, double size)
    {
        return CreateIcon(icon, size, "TextPrimary");
    }

    private static FluentIcon CreateIcon(FluentIconRegular icon, double size, string brushKey)
    {
        return FluentIconFactory.Regular(icon, size, GalleryThemeResources.Brush(brushKey));
    }
}
