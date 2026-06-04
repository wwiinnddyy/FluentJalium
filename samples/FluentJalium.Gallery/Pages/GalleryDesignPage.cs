using FluentJalium.Gallery.Controls;
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

internal sealed class GalleryDesignPage
{
    public UIElement CreateContent()
    {
        var panel = CreateSection("Gallery Design");
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        panel.Children.Add(CreateReferenceStrip());
        examples.Children.Add(GallerySampleCard.Create(
            FluentIconRegular.Navigation24,
            "Shell navigation",
            "Gallery pages route through a left NavigationView and a content Frame/Page host, with search kept in the content header.",
            CreateShellSample(),
            code: "FWNavigationView.MenuItems -> GalleryPageInfo\nFWNavigationView.FooterMenuItems -> Settings / Design / State Matrix\nFWFrame.Navigate(new GalleryHostPage(pageInfo, createContent()))"));
        examples.Children.Add(GallerySampleCard.Create(
            FluentIconRegular.DatabaseSearch24,
            "Catalog metadata",
            "Catalog entries follow WinUI ControlInfoData shape: stable ids, groups, icons, tags, related controls, status, and documentation links.",
            CreateCatalogSample(),
            code: "GalleryCatalogEntry(\n    Title,\n    Description,\n    Group,\n    Icon,\n    Keywords,\n    Status,\n    IsFooter)"));
        examples.Children.Add(GallerySampleCard.Create(
            FluentIconRegular.AppFolder24,
            "Sample card anatomy",
            "Each control page uses the same Example, States, Properties, and Code / Notes rhythm so batches stay comparable.",
            CreateSampleCardSample(),
            code: "GallerySampleCard.Create(\n    icon,\n    title,\n    description,\n    sample,\n    states,\n    properties,\n    code);"));
        examples.Children.Add(GallerySampleCard.Create(
            FluentIconRegular.WindowBrush24,
            "Materials as first-class pages",
            "Window backdrops and element materials remain catalog entries, not hidden settings, because Jalium supports both shell and in-content effects.",
            CreateMaterialEntrySample(),
            code: "Entry(\"Window Backdrops\", ..., GalleryNavigationGroup.Materials, FluentIconRegular.WindowBrush24, ...)\nEntry(\"Materials and Effects\", ..., GalleryNavigationGroup.Materials, FluentIconRegular.TransparencySquare24, ...)"));

        panel.Children.Add(examples);
        return panel;
    }

    private static FWBorder CreateReferenceStrip()
    {
        return new FWBorder
        {
            Background = GalleryThemeResources.Brush("LayerFillColorDefaultBrush"),
            BorderBrush = GalleryThemeResources.Brush("ControlElevationBorderBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12),
            Child = new FWWrapPanel
            {
                HorizontalSpacing = 8,
                VerticalSpacing = 8,
                Children =
                {
                    CreateToken(FluentIconRegular.Navigation24, "WinUI", "NavigationView + ControlInfoData"),
                    CreateToken(FluentIconRegular.AppFolder24, "WPFUI", "Views/Pages + footer settings"),
                    CreateToken(FluentIconRegular.BranchFork24, "FluentAvalonia", "Navigation factory + footer entries"),
                    CreateToken(FluentIconRegular.Diagram24, "Jalium.UI", "Modular Gallery + routed pages")
                }
            }
        };
    }

    private static UIElement CreateShellSample()
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Children =
            {
                CreateNavRail(),
                new FWStackPanel
                {
                    Width = 330,
                    Orientation = Orientation.Vertical,
                    Spacing = 10,
                    Children =
                    {
                        CreateSearchBar(),
                        CreateFrameSurface()
                    }
                }
            }
        };
    }

    private static FWBorder CreateNavRail()
    {
        return new FWBorder
        {
            Width = 170,
            Background = GalleryThemeResources.Brush("LayerFillColorDefaultBrush"),
            BorderBrush = GalleryThemeResources.Brush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 8,
                Children =
                {
                    CreateNavItem(FluentIconRegular.Home24, "Overview", true),
                    CreateNavItem(FluentIconRegular.DesignIdeas24, "Design", false),
                    CreateNavItem(FluentIconRegular.ControlButton24, "Controls", false),
                    CreateNavItem(FluentIconRegular.TransparencySquare24, "Materials", false),
                    CreateDivider(),
                    CreateNavItem(FluentIconRegular.Settings24, "Settings", false)
                }
            }
        };
    }

    private static FWBorder CreateSearchBar()
    {
        return new FWBorder
        {
            Background = GalleryThemeResources.Brush("ControlBackground"),
            BorderBrush = GalleryThemeResources.Brush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10, 7, 10, 7),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children =
                {
                    CreateIcon(FluentIconRegular.Search24, 16, GalleryThemeResources.Brush("TextSecondary")),
                    new FWTextBlock
                    {
                        Text = "Search controls and tokens",
                        FontSize = 12,
                        Foreground = GalleryThemeResources.Brush("TextSecondary"),
                        VerticalAlignment = VerticalAlignment.Center
                    }
                }
            }
        };
    }

    private static FWBorder CreateFrameSurface()
    {
        return new FWBorder
        {
            Background = GalleryThemeResources.Brush("ControlBackground"),
            BorderBrush = GalleryThemeResources.Brush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 8,
                Children =
                {
                    CreateMetric(FluentIconRegular.AppFolder24, "GalleryHostPage", "Page chrome, title, and metadata chips"),
                    CreateMetric(FluentIconRegular.SlideTransition24, "FWFrame", "Navigation state and transitions"),
                    CreateMetric(FluentIconRegular.Code24, "CreateContent", "Page factory returns page sections")
                }
            }
        };
    }

    private static UIElement CreateCatalogSample()
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Children =
            {
                CreateCatalogRow("UniqueId", "Stable search/navigation key"),
                CreateCatalogRow("Group", "Navigation category and sort order"),
                CreateCatalogRow("Keywords", "Search tags and related FW controls"),
                CreateCatalogRow("Status", "Stable, preview, or diagnostic"),
                CreateCatalogRow("Factory", "Resolved separately by GalleryCatalogService")
            }
        };
    }

    private static UIElement CreateSampleCardSample()
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Children =
            {
                CreateSectionPreview("Example", "Live control surface"),
                CreateSectionPreview("States", "Normal, pointer over, pressed, selected, disabled"),
                CreateSectionPreview("Properties", "Primary FW type and important knobs"),
                CreateSectionPreview("Code / Notes", "Jalium syntax and implementation notes")
            }
        };
    }

    private static UIElement CreateMaterialEntrySample()
    {
        return new FWWrapPanel
        {
            HorizontalSpacing = 8,
            VerticalSpacing = 8,
            Children =
            {
                CreateMaterialTile(FluentIconRegular.WindowBrush24, "Window Backdrops", "Mica, Mica Alt, Acrylic, solid fallback"),
                CreateMaterialTile(FluentIconRegular.TransparencySquare24, "Materials", "Acrylic, Mica, FrostedGlass, LiquidGlass"),
                CreateMaterialTile(FluentIconRegular.LayoutColumnTwo24, "Layering", "Shell, page, card, overlay, transient"),
                CreateMaterialTile(FluentIconRegular.SlideTransition24, "Motion", "Connected animation and content transitions")
            }
        };
    }

    private static FWBorder CreateNavItem(FluentIconRegular icon, string title, bool selected)
    {
        return new FWBorder
        {
            Background = selected
                ? GalleryThemeResources.Brush("SubtleFillColorSecondaryBrush")
                : GalleryThemeResources.Brush("ControlBackground"),
            BorderBrush = GalleryThemeResources.Brush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(8, 6, 8, 6),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children =
                {
                    CreateIcon(icon, 16, GalleryThemeResources.Brush("TextPrimary")),
                    new FWTextBlock
                    {
                        Text = title,
                        FontSize = 12,
                        Foreground = GalleryThemeResources.Brush("TextPrimary"),
                        VerticalAlignment = VerticalAlignment.Center
                    }
                }
            }
        };
    }

    private static FWBorder CreateDivider()
    {
        return new FWBorder
        {
            Height = 1,
            Background = GalleryThemeResources.Brush("ControlBorder"),
            BorderThickness = new Thickness(0)
        };
    }

    private static FWBorder CreateCatalogRow(string title, string description)
    {
        return new FWBorder
        {
            Background = GalleryThemeResources.Brush("LayerFillColorDefaultBrush"),
            BorderBrush = GalleryThemeResources.Brush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 3,
                Children =
                {
                    new FWTextBlock
                    {
                        Text = title,
                        FontSize = 13,
                        Foreground = GalleryThemeResources.Brush("TextPrimary")
                    },
                    new FWTextBlock
                    {
                        Text = description,
                        FontSize = 12,
                        Foreground = GalleryThemeResources.Brush("TextSecondary"),
                        TextWrapping = TextWrapping.Wrap
                    }
                }
            }
        };
    }

    private static FWBorder CreateSectionPreview(string title, string description)
    {
        return new FWBorder
        {
            Background = GalleryThemeResources.Brush("LayerFillColorDefaultBrush"),
            BorderBrush = GalleryThemeResources.Brush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children =
                {
                    CreateIcon(FluentIconRegular.TextBulletListSquare24, 16, GalleryThemeResources.Brush("TextSecondary")),
                    new FWStackPanel
                    {
                        Orientation = Orientation.Vertical,
                        Spacing = 2,
                        Children =
                        {
                            new FWTextBlock
                            {
                                Text = title,
                                FontSize = 13,
                                Foreground = GalleryThemeResources.Brush("TextPrimary")
                            },
                            new FWTextBlock
                            {
                                Text = description,
                                FontSize = 12,
                                Foreground = GalleryThemeResources.Brush("TextSecondary"),
                                TextWrapping = TextWrapping.Wrap
                            }
                        }
                    }
                }
            }
        };
    }

    private static FWBorder CreateMaterialTile(FluentIconRegular icon, string title, string description)
    {
        return new FWBorder
        {
            Width = 250,
            Background = GalleryThemeResources.Brush("LayerFillColorDefaultBrush"),
            BorderBrush = GalleryThemeResources.Brush("ControlElevationBorderBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 6,
                Children =
                {
                    CreateIcon(icon, 20, GalleryThemeResources.Brush("TextPrimary")),
                    new FWTextBlock
                    {
                        Text = title,
                        FontSize = 13,
                        Foreground = GalleryThemeResources.Brush("TextPrimary")
                    },
                    new FWTextBlock
                    {
                        Text = description,
                        FontSize = 12,
                        Foreground = GalleryThemeResources.Brush("TextSecondary"),
                        TextWrapping = TextWrapping.Wrap
                    }
                }
            }
        };
    }

    private static FWBorder CreateMetric(FluentIconRegular icon, string title, string description)
    {
        return new FWBorder
        {
            Background = GalleryThemeResources.Brush("LayerFillColorDefaultBrush"),
            BorderBrush = GalleryThemeResources.Brush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children =
                {
                    CreateIcon(icon, 18, GalleryThemeResources.Brush("TextPrimary")),
                    new FWStackPanel
                    {
                        Orientation = Orientation.Vertical,
                        Spacing = 2,
                        Children =
                        {
                            new FWTextBlock
                            {
                                Text = title,
                                FontSize = 13,
                                Foreground = GalleryThemeResources.Brush("TextPrimary")
                            },
                            new FWTextBlock
                            {
                                Text = description,
                                FontSize = 12,
                                Foreground = GalleryThemeResources.Brush("TextSecondary"),
                                TextWrapping = TextWrapping.Wrap
                            }
                        }
                    }
                }
            }
        };
    }

    private static FWBorder CreateToken(FluentIconRegular icon, string title, string value)
    {
        return new FWBorder
        {
            Background = GalleryThemeResources.Brush("ControlBackground"),
            BorderBrush = GalleryThemeResources.Brush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10, 6, 10, 6),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children =
                {
                    CreateIcon(icon, 16, GalleryThemeResources.Brush("TextSecondary")),
                    new FWTextBlock
                    {
                        Text = title,
                        FontSize = 12,
                        Foreground = GalleryThemeResources.Brush("TextPrimary"),
                        VerticalAlignment = VerticalAlignment.Center
                    },
                    new FWTextBlock
                    {
                        Text = value,
                        FontSize = 11,
                        Foreground = GalleryThemeResources.Brush("TextSecondary"),
                        VerticalAlignment = VerticalAlignment.Center
                    }
                }
            }
        };
    }

    private static FWStackPanel CreateSection(string title)
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 14,
            Children =
            {
                new FWStackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 10,
                    Children =
                    {
                        CreateIcon(FluentIconRegular.DesignIdeas24, 24, GalleryThemeResources.Brush("TextPrimary")),
                        new FWTextBlock
                        {
                            Text = title,
                            FontSize = 22,
                            Foreground = GalleryThemeResources.Brush("TextPrimary"),
                            VerticalAlignment = VerticalAlignment.Center
                        }
                    }
                }
            }
        };
    }

    private static FluentIcon CreateIcon(FluentIconRegular icon, double size, Brush foreground)
    {
        return FluentIconFactory.Regular(icon, size, foreground);
    }
}
