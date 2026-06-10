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

    public GalleryHostPage()
    {
        LocalizationService.Instance.PropertyChanged += (s, e) =>
        {
            RefreshTheme();
        };
    }

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
        var scrollViewer = new FWScrollViewer
        {
            Background = GalleryThemeResources.Brush("NavigationViewContentBackground"),
            Padding = new Thickness(0),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            IsScrollBarAutoHideEnabled = true
        };

        var mainPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 36,
            Margin = new Thickness(36, 40, 36, 40)
        };

        var header = CreateWinUiHeader(page);
        mainPanel.Children.Add(header);

        mainPanel.Children.Add(page.CreateContent());

        var metadataExpander = CreateCollapsibleMetadata(page);
        if (metadataExpander != null)
        {
            mainPanel.Children.Add(metadataExpander);
        }

        scrollViewer.Content = mainPanel;
        return scrollViewer;
    }

    private static UIElement CreateWinUiHeader(GalleryPage page)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        var leftPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8
        };

        var titleRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12
        };

        titleRow.Children.Add(CreateIcon(page.Icon, 28));
        titleRow.Children.Add(new FWTextBlock
        {
            Text = page.Title,
            FontSize = 36,
            FontFamily = "Segoe UI Variable Display",
            FontWeight = FontWeights.SemiBold,
            Foreground = GalleryThemeResources.Brush("TextPrimary"),
            VerticalAlignment = VerticalAlignment.Center
        });

        if (page.Status != GalleryPageStatus.Stable)
        {
            titleRow.Children.Add(CreateStatusPill(page.Status));
        }

        leftPanel.Children.Add(titleRow);

        leftPanel.Children.Add(new FWTextBlock
        {
            Text = page.Description,
            FontSize = 14,
            Foreground = GalleryThemeResources.Brush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 4, 0, 0)
        });

        Grid.SetColumn(leftPanel, 0);
        grid.Children.Add(leftPanel);

        var rightPanel = new FWWrapPanel
        {
            HorizontalSpacing = 8,
            VerticalSpacing = 8,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(16, 0, 0, 0)
        };

        if (!string.IsNullOrWhiteSpace(page.SourcePath))
        {
            var sourceBtn = new FWButton
            {
                Density = FWButtonDensity.Compact,
                Content = new FWStackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 6,
                    Children =
                    {
                        CreateIcon(FluentIconRegular.FolderOpen24, 14),
                        new FWTextBlock { Text = "Source", FontSize = 12 }
                    }
                }
            };
            sourceBtn.Click += (_, _) => Clipboard.SetText(page.SourcePath);
            rightPanel.Children.Add(sourceBtn);
        }

        foreach (var link in page.DocumentationLinks)
        {
            var docBtn = new FWButton
            {
                Density = FWButtonDensity.Compact,
                Content = new FWStackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 6,
                    Children =
                    {
                        CreateIcon(FluentIconRegular.DocumentLink24, 14),
                        new FWTextBlock { Text = link.Title, FontSize = 12 }
                    }
                }
            };
            docBtn.Click += (_, _) => Clipboard.SetText(link.Uri);
            rightPanel.Children.Add(docBtn);
        }

        if (rightPanel.Children.Count > 0)
        {
            Grid.SetColumn(rightPanel, 1);
            grid.Children.Add(rightPanel);
        }

        return grid;
    }

    private static UIElement? CreateCollapsibleMetadata(GalleryPage page)
    {
        bool hasMetadata = !string.IsNullOrWhiteSpace(page.ApiNamespace) ||
                           page.BaseClasses.Count > 0 ||
                           page.RelatedControls.Count > 0 ||
                           GallerySampleCodeRegistry.TryGetSampleCode(page.Info, out _);

        if (!hasMetadata) return null;

        var detailsPanel = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Margin = new Thickness(0, 10, 0, 0),
            Visibility = Visibility.Collapsed
        };

        detailsPanel.Children.Add(CreateMetadataSummary(page));

        if (!string.IsNullOrWhiteSpace(page.ApiNamespace))
        {
            detailsPanel.Children.Add(CreateMetadataLine(FluentIconRegular.Braces24, "API", page.ApiNamespace, true));
        }

        if (page.BaseClasses.Count > 0)
        {
            detailsPanel.Children.Add(CreateMetadataGroup(FluentIconRegular.BranchFork24, "Base", page.BaseClasses));
        }

        if (page.RelatedControls.Count > 0)
        {
            detailsPanel.Children.Add(CreateMetadataGroup(FluentIconRegular.Tag24, "Related", page.RelatedControls));
        }

        if (GallerySampleCodeRegistry.TryGetSampleCode(page.Info, out var sampleCode))
        {
            detailsPanel.Children.Add(CreateSampleCodeBlock(sampleCode));
        }

        var toggleButton = new FWButton
        {
            Content = new FWStackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children =
                {
                    CreateIcon(FluentIconRegular.ChevronDown24, 14),
                    new FWTextBlock { Text = "Show Developer Details", FontSize = 12, FontWeight = FontWeights.SemiBold }
                }
            },
            HorizontalAlignment = HorizontalAlignment.Left
        };

        toggleButton.Click += (_, _) =>
        {
            if (detailsPanel.Visibility == Visibility.Visible)
            {
                detailsPanel.Visibility = Visibility.Collapsed;
                toggleButton.Content = new FWStackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 8,
                    Children =
                    {
                        CreateIcon(FluentIconRegular.ChevronDown24, 14),
                        new FWTextBlock { Text = "Show Developer Details", FontSize = 12, FontWeight = FontWeights.SemiBold }
                    }
                };
            }
            else
            {
                detailsPanel.Visibility = Visibility.Visible;
                toggleButton.Content = new FWStackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 8,
                    Children =
                    {
                        CreateIcon(FluentIconRegular.ChevronUp24, 14),
                        new FWTextBlock { Text = "Hide Developer Details", FontSize = 12, FontWeight = FontWeights.SemiBold }
                    }
                };
            }
        };

        var border = new FWBorder
        {
            Background = GalleryThemeResources.Brush("CardBackgroundFillColorDefaultBrush"),
            BorderBrush = GalleryThemeResources.Brush("ControlElevationBorderBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(16),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 16,
                Children =
                {
                    toggleButton,
                    detailsPanel
                }
            }
        };

        return border;
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
