using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using Jalium.UI.Media.Animation;
using FWAccessText = FluentJalium.Controls.FWAccessText;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWButton = FluentJalium.Controls.FWButton;
using FWContentControl = FluentJalium.Controls.FWContentControl;
using FWContentPresenter = FluentJalium.Controls.FWContentPresenter;
using FWFluentMaterialKind = FluentJalium.Controls.FWFluentMaterialKind;
using FWFluentMaterialSurface = FluentJalium.Controls.FWFluentMaterialSurface;
using FWGrid = FluentJalium.Controls.FWGrid;
using FWLabel = FluentJalium.Controls.FWLabel;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWTransitioningContentControl = FluentJalium.Controls.FWTransitioningContentControl;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;

namespace FluentJalium.Gallery.Pages;

internal sealed class GalleryContentLayoutPage
{
    public UIElement CreateContent()
    {
        var panel = CreateSection("Content and Layout");
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        examples.Children.Add(CreateLayoutExampleCard(
            FluentIconRegular.Textbox24,
            "Text and access text",
            "TextBlock, selectable body text, wrapping, trimming, label, and AccessText state.",
            CreateTextContentSample()));
        examples.Children.Add(CreateLayoutExampleCard(
            FluentIconRegular.Layer24,
            "Border and content hosts",
            "Border, ContentControl, ContentPresenter, padding, corner radius, and inherited foreground.",
            CreateContentHostSample()));
        examples.Children.Add(CreateLayoutExampleCard(
            FluentIconRegular.LayoutColumnTwo24,
            "Stack, wrap, and grid layout",
            "StackPanel spacing, WrapPanel chips, Grid spacing, rows, columns, and spanning cells.",
            CreatePanelLayoutSample()));
        examples.Children.Add(CreateLayoutExampleCard(
            FluentIconRegular.SlideTransition24,
            "Transitioning content",
            "TransitioningContentControl switches between slide and LiquidMorph content surfaces.",
            CreateTransitioningContentSample()));
        examples.Children.Add(CreateLayoutExampleCard(
            FluentIconRegular.LayerDiagonalSparkle24,
            "Material layout surface",
            "Text, content hosts, grid cells, and transition content stay readable on LiquidGlass.",
            CreateMaterialLayoutSurfaceSample()));

        panel.Children.Add(examples);
        return panel;
    }

    private static UIElement CreateTextContentSample()
    {
        var output = CreateLayoutOutput("Text: selection enabled, access key O.");
        var title = new FWTextBlock
        {
            Text = "Title text",
            FontFamily = "Segoe UI Variable Display",
            FontSize = 22,
            Foreground = ThemeBrush("TextPrimary")
        };
        var body = new FWTextBlock
        {
            Text = "Selectable body copy follows Fluent typography and wraps inside its layout column.",
            IsTextSelectionEnabled = true,
            TextWrapping = TextWrapping.Wrap,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Foreground = ThemeBrush("TextSecondary")
        };
        var accessText = new FWAccessText
        {
            Text = "_Open command",
            Foreground = ThemeBrush("TextPrimary")
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 360,
            Children =
            {
                new FWLabel { Content = "FWTextBlock" },
                title,
                body,
                accessText,
                CreateLayoutButtonRow(
                    CreateLayoutActionButton(FluentIconRegular.TextBold24, "Emphasis", () =>
                    {
                        title.FontSize = title.FontSize == 22 ? 26 : 22;
                        output.Text = $"Title font size: {title.FontSize}";
                    }),
                    CreateLayoutActionButton(FluentIconRegular.TextEditStyle24, "Replace", () =>
                    {
                        body.Text = "Updated selectable copy keeps wrapping inside the same surface.";
                        output.Text = "Body text replaced.";
                    })),
                CreateLayoutStatus(output)
            }
        };
    }

    private static UIElement CreateContentHostSample()
    {
        var output = CreateLayoutOutput("Hosts: Border child and ContentControl content ready.");
        var hostedText = new FWTextBlock
        {
            Text = "FWContentControl hosts text content with inherited Fluent text styling.",
            Foreground = ThemeBrush("TextPrimary"),
            TextWrapping = TextWrapping.Wrap
        };
        var border = new FWBorder
        {
            Background = ThemeBrush("SurfaceBackground"),
            BorderBrush = ThemeBrush("ContentSurfaceBorderBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(14),
            Child = new FWContentControl
            {
                Content = hostedText,
                Foreground = ThemeBrush("TextPrimary"),
                Padding = new Thickness(0)
            }
        };
        var presenterText = new FWTextBlock
        {
            Text = "FWContentPresenter mirrors content without adding surface chrome.",
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        };
        var presenter = new FWContentPresenter
        {
            Content = presenterText
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 390,
            Children =
            {
                new FWLabel { Content = "FWBorder and content hosts" },
                border,
                presenter,
                CreateLayoutButtonRow(
                    CreateLayoutActionButton(FluentIconRegular.BorderOutside24, "Radius", () =>
                    {
                        border.CornerRadius = border.CornerRadius.TopLeft == 6
                            ? new CornerRadius(2)
                            : new CornerRadius(6);
                        output.Text = $"Border radius: {border.CornerRadius.TopLeft}";
                    }),
                    CreateLayoutActionButton(FluentIconRegular.TextEditStyle24, "Presenter", () =>
                    {
                        presenter.Content = new FWTextBlock
                        {
                            Text = "Presenter content updated.",
                            Foreground = ThemeBrush("TextPrimary")
                        };
                        output.Text = "ContentPresenter content replaced.";
                    })),
                CreateLayoutStatus(output)
            }
        };
    }

    private static UIElement CreatePanelLayoutSample()
    {
        var output = CreateLayoutOutput("Panels: wrap chips and grid cells ready.");
        var chips = new FWWrapPanel
        {
            HorizontalSpacing = 8,
            VerticalSpacing = 8,
            Children =
            {
                CreateLayoutChip("Stack"),
                CreateLayoutChip("Wrap"),
                CreateLayoutChip("Grid"),
                CreateLayoutChip("Content"),
                CreateLayoutChip("Surface")
            }
        };
        var grid = CreateSampleGrid();

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 390,
            Children =
            {
                new FWLabel { Content = "FWStackPanel, FWWrapPanel, FWGrid" },
                chips,
                grid,
                CreateLayoutButtonRow(
                    CreateLayoutActionButton(FluentIconRegular.Table24, "Spacing", () =>
                    {
                        grid.ColumnSpacing = grid.ColumnSpacing == 8 ? 14 : 8;
                        grid.RowSpacing = grid.RowSpacing == 8 ? 14 : 8;
                        output.Text = $"Grid spacing: {grid.ColumnSpacing}";
                    }),
                    CreateLayoutActionButton(FluentIconRegular.Add24, "Chip", () =>
                    {
                        chips.Children.Add(CreateLayoutChip($"Item {chips.Children.Count + 1}"));
                        output.Text = $"WrapPanel children: {chips.Children.Count}";
                    })),
                CreateLayoutStatus(output)
            }
        };
    }

    private static UIElement CreateTransitioningContentSample()
    {
        var output = CreateLayoutOutput("Transition: SlideLeft with text content.");
        var transitionHost = new FWTransitioningContentControl
        {
            Width = 320,
            Height = 92,
            TransitionMode = TransitionMode.SlideLeft,
            Content = CreateTransitionCard("Slide content", "TransitioningContentControl")
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 390,
            Children =
            {
                transitionHost,
                CreateLayoutButtonRow(
                    CreateLayoutActionButton(FluentIconRegular.SlideTransition24, "Slide", () =>
                    {
                        transitionHost.TransitionMode = TransitionMode.SlideLeft;
                        transitionHost.Content = CreateTransitionCard("Slide content", "Transition mode");
                        output.Text = "Transition: SlideLeft.";
                    }),
                    CreateLayoutActionButton(FluentIconRegular.LayerDiagonalSparkle24, "Liquid", () =>
                    {
                        transitionHost.TransitionMode = TransitionMode.LiquidMorph;
                        transitionHost.Content = CreateTransitionCard("LiquidMorph content", "Material motion");
                        output.Text = "Transition: LiquidMorph.";
                    })),
                CreateLayoutStatus(output)
            }
        };
    }

    private static UIElement CreateMaterialLayoutSurfaceSample()
    {
        var output = CreateLayoutOutput("Surface: LiquidGlass. Grid spacing 8. Transition SlideLeft.");
        var grid = CreateSampleGrid();
        var transitionHost = new FWTransitioningContentControl
        {
            Width = 470,
            Height = 86,
            TransitionMode = TransitionMode.SlideLeft,
            Content = CreateTransitionCard("Layered content", "LiquidGlass layout host")
        };

        return new FWFluentMaterialSurface
        {
            Width = 540,
            MaterialKind = FWFluentMaterialKind.LiquidGlass,
            TintColor = Color.FromArgb(180, 20, 84, 145),
            TintOpacity = 0.2,
            BlurRadius = 14,
            RefractionAmount = 70,
            ChromaticAberration = 0.42,
            FusionRadius = 24,
            Background = new SolidColorBrush(Color.FromArgb(66, 255, 255, 255)),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Shape = BorderShape.SuperEllipse,
            SuperEllipseN = 4,
            Padding = new Thickness(16),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 12,
                Children =
                {
                    CreateMaterialHeader(),
                    new FWTextBlock
                    {
                        Text = "Content hosts and layout panels keep contrast while glass and refraction are active.",
                        Foreground = ThemeBrush("TextSecondary"),
                        TextWrapping = TextWrapping.Wrap
                    },
                    new FWWrapPanel
                    {
                        HorizontalSpacing = 8,
                        VerticalSpacing = 8,
                        Children =
                        {
                            CreateLayoutChip("Text"),
                            CreateLayoutChip("Border"),
                            CreateLayoutChip("Grid"),
                            CreateLayoutChip("Presenter")
                        }
                    },
                    grid,
                    transitionHost,
                    CreateLayoutButtonRow(
                        CreateLayoutActionButton(FluentIconRegular.Table24, "Spacing", () =>
                        {
                            grid.ColumnSpacing = grid.ColumnSpacing == 8 ? 14 : 8;
                            grid.RowSpacing = grid.RowSpacing == 8 ? 14 : 8;
                            output.Text = $"Surface grid spacing: {grid.ColumnSpacing}";
                        }),
                        CreateLayoutActionButton(FluentIconRegular.LayerDiagonalSparkle24, "Morph", () =>
                        {
                            transitionHost.TransitionMode = TransitionMode.LiquidMorph;
                            transitionHost.Content = CreateTransitionCard("LiquidMorph content", "Shared surface");
                            output.Text = "Surface transition: LiquidMorph.";
                        })),
                    CreateLayoutStatus(output)
                }
            }
        };
    }

    private static FWGrid CreateSampleGrid()
    {
        var grid = new FWGrid
        {
            Width = 320,
            ColumnSpacing = 8,
            RowSpacing = 8
        };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var cellA = CreateGridCell("Auto", "Row 0");
        var cellB = CreateGridCell("Star", "Column 1");
        var cellC = CreateGridCell("Span", "Two columns");
        Grid.SetColumn(cellB, 1);
        Grid.SetRow(cellC, 1);
        Grid.SetColumnSpan(cellC, 2);
        grid.Children.Add(cellA);
        grid.Children.Add(cellB);
        grid.Children.Add(cellC);
        return grid;
    }

    private static FWBorder CreateTransitionCard(string title, string detail)
    {
        return new FWBorder
        {
            Background = ThemeBrush("ControlBackground"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 4,
                Children =
                {
                    new FWTextBlock
                    {
                        Text = title,
                        Foreground = ThemeBrush("TextPrimary"),
                        FontSize = 14
                    },
                    new FWTextBlock
                    {
                        Text = detail,
                        Foreground = ThemeBrush("TextSecondary"),
                        FontSize = 12
                    }
                }
            }
        };
    }

    private static FWStackPanel CreateMaterialHeader()
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                CreateIcon(FluentIconRegular.LayerDiagonalSparkle24, 18, ThemeBrush("TextPrimary")),
                new FWTextBlock
                {
                    Text = "Layered layout surface",
                    FontSize = 15,
                    Foreground = ThemeBrush("TextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
    }

    private static FWBorder CreateLayoutExampleCard(FluentIconRegular icon, string title, string description, UIElement content)
    {
        return new FWBorder
        {
            Width = 570,
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
                    new FWStackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 8,
                        Children =
                        {
                            CreateIcon(icon, 20, ThemeBrush("TextPrimary")),
                            new FWTextBlock
                            {
                                Text = title,
                                FontSize = 15,
                                Foreground = ThemeBrush("TextPrimary"),
                                VerticalAlignment = VerticalAlignment.Center
                            }
                        }
                    },
                    new FWTextBlock
                    {
                        Text = description,
                        FontSize = 12,
                        Foreground = ThemeBrush("TextSecondary"),
                        TextWrapping = TextWrapping.Wrap
                    },
                    content
                }
            }
        };
    }

    private static FWWrapPanel CreateLayoutButtonRow(params FWButton[] buttons)
    {
        var row = new FWWrapPanel
        {
            HorizontalSpacing = 8,
            VerticalSpacing = 8
        };

        foreach (var button in buttons)
        {
            row.Children.Add(button);
        }

        return row;
    }

    private static FWButton CreateLayoutActionButton(FluentIconRegular icon, string text, Action action)
    {
        var button = new FWButton
        {
            Content = CreateLayoutButtonContent(icon, text)
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static FWStackPanel CreateLayoutButtonContent(FluentIconRegular icon, string text)
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Children =
            {
                CreateIcon(icon, 16, ThemeBrush("TextPrimary")),
                new FWTextBlock
                {
                    Text = text,
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
    }

    private static TextBlock CreateLayoutOutput(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        };
    }

    private static FWBorder CreateLayoutStatus(TextBlock status)
    {
        return new FWBorder
        {
            Background = ThemeBrush("LayerFillColorDefaultBrush"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children =
                {
                    CreateIcon(FluentIconRegular.InfoSparkle24, 18, ThemeBrush("TextSecondary")),
                    status
                }
            }
        };
    }

    private static FWBorder CreateLayoutChip(string text)
    {
        return new FWBorder
        {
            Background = ThemeBrush("SelectionBackgroundWeak"),
            BorderBrush = ThemeBrush("AccentBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(10, 4, 10, 4),
            Child = new FWTextBlock
            {
                Text = text,
                Foreground = ThemeBrush("TextPrimary"),
                FontSize = 12
            }
        };
    }

    private static FWBorder CreateGridCell(string title, string detail)
    {
        return new FWBorder
        {
            Background = ThemeBrush("ControlBackground"),
            BorderBrush = ThemeBrush("ControlBorder"),
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
                        Foreground = ThemeBrush("TextPrimary"),
                        FontSize = 13
                    },
                    new FWTextBlock
                    {
                        Text = detail,
                        Foreground = ThemeBrush("TextSecondary"),
                        FontSize = 12
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
                        CreateIcon(FluentIconRegular.LayoutColumnTwo24, 24, ThemeBrush("TextPrimary")),
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

    private static FluentIcon CreateIcon(FluentIconRegular icon, double size, Brush? foreground = null)
    {
        return FluentIconFactory.Regular(icon, size, foreground ?? ThemeBrush("TextPrimary"));
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
