using FluentJalium.Icon;
using FluentJalium.Gallery.Controls;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWButton = FluentJalium.Controls.FWButton;
using FWFluentMaterialKind = FluentJalium.Controls.FWFluentMaterialKind;
using FWFluentMaterialSurface = FluentJalium.Controls.FWFluentMaterialSurface;
using FWFontIcon = FluentJalium.Controls.FWFontIcon;
using FWImage = FluentJalium.Controls.FWImage;
using FWLabel = FluentJalium.Controls.FWLabel;
using FWPathIcon = FluentJalium.Controls.FWPathIcon;
using FWSeparator = FluentJalium.Controls.FWSeparator;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWSymbolIcon = FluentJalium.Controls.FWSymbolIcon;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWTextBox = FluentJalium.Controls.FWTextBox;
using FWViewbox = FluentJalium.Controls.FWViewbox;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;

namespace FluentJalium.Gallery.Pages;

internal sealed class GalleryVisualsPage
{
    public UIElement CreateContent()
    {
        var panel = CreateSection("Visuals and Icons");
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        examples.Children.Add(CreateVisualExampleCard(
            FluentIconRegular.AppGeneric24,
            "Fluent icon library",
            "Regular, filled, font, symbol, and path icons share Fluent sizing, foreground, and command states.",
            CreateIconLibrarySample()));
        examples.Children.Add(CreateVisualExampleCard(
            FluentIconRegular.Image24,
            "FWImage",
            "Image stretch, zoom settings, and clipped Fluent surfaces for media thumbnails.",
            CreateImageSample()));
        examples.Children.Add(CreateVisualExampleCard(
            FluentIconRegular.Tag24,
            "FWLabel and FWSeparator",
            "Labels target input controls while horizontal and vertical separators organize dense content.",
            CreateLabelSeparatorSample()));
        examples.Children.Add(CreateVisualExampleCard(
            FluentIconRegular.ResizeLarge24,
            "FWViewbox",
            "Viewbox scales a composed Fluent surface while preserving aspect and text hierarchy.",
            CreateViewboxSample()));
        examples.Children.Add(CreateVisualExampleCard(
            FluentIconRegular.LayerDiagonalSparkle24,
            "Material visual surface",
            "Icons, images, labels, separators, and scaled content remain readable on LiquidGlass.",
            CreateMaterialVisualSurfaceSample()));

        panel.Children.Add(examples);
        return panel;
    }

    private static UIElement CreateIconLibrarySample()
    {
        var output = CreateVisualOutput("Icons: regular, filled, font, symbol, and path variants.");
        var regularIcon = CreateIconTile(
            "Regular",
            new FluentIcon
            {
                Icon = FluentIconRegular.Save24,
                Size = 24,
                Foreground = ThemeBrush("TextPrimary")
            });
        var filledIcon = CreateIconTile(
            "Filled",
            FluentIconFactory.Filled(FluentIconRegular.Share24, 24, ThemeBrush("TextPrimary")));
        var fontIcon = CreateIconTile(
            "Font",
            new FWFontIcon
            {
                Glyph = "\uE72D",
                FontFamily = FluentIcon.SegoeFontFamily,
                FontSize = 24,
                Foreground = ThemeBrush("TextPrimary")
            });
        var symbolIcon = CreateIconTile(
            "Symbol",
            new FWSymbolIcon
            {
                Symbol = Symbol.Save,
                Foreground = ThemeBrush("TextPrimary")
            });
        var pathIcon = CreateIconTile(
            "Path",
            new FWPathIcon
            {
                Data = Geometry.Parse("M 0,10 L 8,0 L 16,10 L 12,10 L 12,18 L 4,18 L 4,10 Z"),
                Width = 24,
                Height = 24,
                Foreground = ThemeBrush("TextPrimary")
            });

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 390,
            Children =
            {
                new FWLabel { Content = "FluentJalium icon variants" },
                new FWWrapPanel
                {
                    HorizontalSpacing = 8,
                    VerticalSpacing = 8,
                    Children =
                    {
                        regularIcon,
                        filledIcon,
                        fontIcon,
                        symbolIcon,
                        pathIcon
                    }
                },
                CreateVisualButtonRow(
                    CreateVisualActionButton(FluentIconRegular.Color24, "Accent", () =>
                    {
                        SetIconTileAccent(regularIcon, filledIcon, fontIcon, symbolIcon, pathIcon);
                        output.Text = "Icons: accent foreground applied.";
                    }),
                    CreateVisualActionButton(FluentIconRegular.TextFontSize24, "Size", () =>
                    {
                        SetIconTileSize(regularIcon, 28);
                        SetIconTileSize(filledIcon, 28);
                        SetIconTileSize(fontIcon, 28);
                        SetIconTileSize(symbolIcon, 28);
                        SetIconTileSize(pathIcon, 28);
                        output.Text = "Icons: tiles resized to 28.";
                    })),
                CreateVisualStatus(output)
            }
        };
    }

    private static UIElement CreateImageSample()
    {
        var output = CreateVisualOutput("Image: UniformToFill, zoom enabled.");
        var image = new FWImage
        {
            Width = 300,
            Height = 160,
            Source = CreateSampleBitmap(),
            Stretch = Stretch.UniformToFill,
            IsZoomEnabled = true,
            MinZoom = 0.75,
            MaxZoom = 4
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 390,
            Children =
            {
                new FWLabel { Content = "FWImage stretch" },
                new FWBorder
                {
                    Width = 320,
                    Background = ThemeBrush("LayerFillColorDefaultBrush"),
                    BorderBrush = ThemeBrush("ControlBorder"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(10),
                    Child = image
                },
                CreateVisualButtonRow(
                    CreateVisualActionButton(FluentIconRegular.Image24, "Fill", () =>
                    {
                        image.Stretch = Stretch.UniformToFill;
                        output.Text = "Image stretch: UniformToFill.";
                    }),
                    CreateVisualActionButton(FluentIconRegular.ResizeLarge24, "Fit", () =>
                    {
                        image.Stretch = Stretch.Uniform;
                        output.Text = "Image stretch: Uniform.";
                    }),
                    CreateVisualActionButton(FluentIconRegular.ZoomIn24, "Zoom", () =>
                    {
                        image.IsZoomEnabled = !image.IsZoomEnabled;
                        output.Text = $"Image zoom: {FormatOnOff(image.IsZoomEnabled)}.";
                    })),
                CreateVisualStatus(output)
            }
        };
    }

    private static UIElement CreateLabelSeparatorSample()
    {
        var output = CreateVisualOutput("Label: target set. Separators: horizontal and vertical.");
        var textBox = new FWTextBox
        {
            Text = "FWLabel target",
            Width = 190
        };
        var label = new FWLabel
        {
            Content = "Name",
            Target = textBox,
            AccessKey = 'N',
            Width = 72
        };
        textBox.TextChanged += (_, _) => output.Text = $"Label target text: {textBox.Text}";

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 390,
            Children =
            {
                new FWStackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 8,
                    Children =
                    {
                        label,
                        textBox
                    }
                },
                new FWSeparator(),
                new FWStackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 10,
                    Children =
                    {
                        CreateSeparatorMetric("Left", "Primary label"),
                        new FWSeparator { Orientation = Orientation.Vertical, Height = 44 },
                        CreateSeparatorMetric("Right", "Secondary label")
                    }
                },
                CreateVisualButtonRow(
                    CreateVisualActionButton(FluentIconRegular.TextEditStyle24, "Edit", () =>
                    {
                        textBox.Text = "Updated target";
                        output.Text = "Label target text updated.";
                    }),
                    CreateVisualActionButton(FluentIconRegular.Prohibited24, "Disable", () =>
                    {
                        textBox.IsEnabled = !textBox.IsEnabled;
                        output.Text = $"Label target enabled: {FormatOnOff(textBox.IsEnabled)}.";
                    })),
                CreateVisualStatus(output)
            }
        };
    }

    private static UIElement CreateViewboxSample()
    {
        var output = CreateVisualOutput("Viewbox: Uniform stretch, both directions.");
        var viewbox = new FWViewbox
        {
            Width = 320,
            Height = 150,
            Stretch = Stretch.Uniform,
            StretchDirection = StretchDirection.Both,
            Child = CreateScaledVisualSurface("Scaled Fluent surface", "Viewbox child")
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 390,
            Children =
            {
                new FWLabel { Content = "FWViewbox scaling" },
                viewbox,
                CreateVisualButtonRow(
                    CreateVisualActionButton(FluentIconRegular.ResizeLarge24, "Uniform", () =>
                    {
                        viewbox.Stretch = Stretch.Uniform;
                        output.Text = "Viewbox stretch: Uniform.";
                    }),
                    CreateVisualActionButton(FluentIconRegular.FullScreenMaximize24, "Fill", () =>
                    {
                        viewbox.Stretch = Stretch.Fill;
                        output.Text = "Viewbox stretch: Fill.";
                    }),
                    CreateVisualActionButton(FluentIconRegular.ArrowAutofitHeight24, "Down", () =>
                    {
                        viewbox.StretchDirection = StretchDirection.DownOnly;
                        output.Text = "Viewbox direction: DownOnly.";
                    })),
                CreateVisualStatus(output)
            }
        };
    }

    private static UIElement CreateMaterialVisualSurfaceSample()
    {
        var output = CreateVisualOutput("Surface: LiquidGlass, Uniform image, Uniform viewbox.");
        var image = new FWImage
        {
            Width = 155,
            Height = 96,
            Source = CreateSampleBitmap(),
            Stretch = Stretch.UniformToFill,
            IsZoomEnabled = true
        };
        var viewbox = new FWViewbox
        {
            Width = 200,
            Height = 96,
            Stretch = Stretch.Uniform,
            Child = CreateScaledVisualSurface("Glass scale", "LiquidGlass")
        };
        var labelTarget = new FWTextBox
        {
            Text = "Material visual",
            Width = 180
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
                    new FWWrapPanel
                    {
                        HorizontalSpacing = 12,
                        VerticalSpacing = 12,
                        Children =
                        {
                            CreateMaterialPreview("Image", image),
                            CreateMaterialPreview("Viewbox", viewbox)
                        }
                    },
                    new FWSeparator(),
                    new FWStackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 8,
                        Children =
                        {
                            new FWLabel
                            {
                                Content = "Title",
                                Target = labelTarget,
                                AccessKey = 'T',
                                Width = 52
                            },
                            labelTarget
                        }
                    },
                    CreateVisualButtonRow(
                        CreateVisualActionButton(FluentIconRegular.Image24, "Image", () =>
                        {
                            image.Stretch = image.Stretch == Stretch.UniformToFill ? Stretch.Uniform : Stretch.UniformToFill;
                            output.Text = $"Surface image stretch: {image.Stretch}.";
                        }),
                        CreateVisualActionButton(FluentIconRegular.ResizeLarge24, "Viewbox", () =>
                        {
                            viewbox.Stretch = viewbox.Stretch == Stretch.Uniform ? Stretch.Fill : Stretch.Uniform;
                            output.Text = $"Surface viewbox stretch: {viewbox.Stretch}.";
                        }),
                        CreateVisualActionButton(FluentIconRegular.Color24, "Tint", () =>
                        {
                            labelTarget.Text = "Accent visual";
                            output.Text = "Surface label target updated.";
                        })),
                    CreateVisualStatus(output)
                }
            }
        };
    }

    private static FWBorder CreateIconTile(string label, FrameworkElement icon)
    {
        icon.Width = 28;
        icon.Height = 28;

        return new FWBorder
        {
            Width = 70,
            Background = ThemeBrush("LayerFillColorDefaultBrush"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(8),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 6,
                HorizontalAlignment = HorizontalAlignment.Center,
                Children =
                {
                    icon,
                    new FWTextBlock
                    {
                        Text = label,
                        FontSize = 11,
                        Foreground = ThemeBrush("TextSecondary"),
                        HorizontalAlignment = HorizontalAlignment.Center
                    }
                }
            }
        };
    }

    private static void SetIconTileAccent(params FWBorder[] tiles)
    {
        foreach (var tile in tiles)
        {
            if (tile.Child is FWStackPanel stack && stack.Children.Count > 0 && stack.Children[0] is IconElement icon)
            {
                icon.Foreground = ThemeBrush("AccentBrush");
            }
        }
    }

    private static void SetIconTileSize(FWBorder tile, double size)
    {
        if (tile.Child is FWStackPanel stack && stack.Children.Count > 0 && stack.Children[0] is FrameworkElement icon)
        {
            icon.Width = size;
            icon.Height = size;
            if (icon is FWFontIcon fontIcon)
            {
                fontIcon.FontSize = size;
            }

            if (icon is FluentIcon fluentIcon)
            {
                fluentIcon.Size = size;
            }
        }
    }

    private static FWBorder CreateSeparatorMetric(string title, string detail)
    {
        return new FWBorder
        {
            Width = 130,
            Background = ThemeBrush("LayerFillColorDefaultBrush"),
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

    private static FWBorder CreateScaledVisualSurface(string title, string detail)
    {
        return new FWBorder
        {
            Width = 180,
            Height = 86,
            Background = ThemeBrush("SelectionBackgroundWeak"),
            BorderBrush = ThemeBrush("AccentBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 5,
                Children =
                {
                    CreateIcon(FluentIconRegular.LayerDiagonal24, 22, ThemeBrush("TextPrimary")),
                    new FWTextBlock
                    {
                        Text = title,
                        Foreground = ThemeBrush("TextPrimary"),
                        TextWrapping = TextWrapping.Wrap
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

    private static FWBorder CreateMaterialPreview(string label, UIElement content)
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
                Spacing = 8,
                Children =
                {
                    new FWTextBlock
                    {
                        Text = label,
                        Foreground = ThemeBrush("TextSecondary"),
                        FontSize = 12
                    },
                    content
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
                    Text = "Layered visual surface",
                    FontSize = 15,
                    Foreground = ThemeBrush("TextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
    }

    private static FWBorder CreateVisualExampleCard(FluentIconRegular icon, string title, string description, UIElement content)
    {
        return GallerySampleCard.Create(icon, title, description, content, code: CreateSampleCode(title));
    }

    private static string CreateSampleCode(string title)
    {
        return title switch
        {
            "Fluent icon library" => "<FluentIcon Icon=\"Save24\" />\n<FWFontIcon Glyph=\"&#xE72D;\" />\n<FWSymbolIcon Symbol=\"Save\" />",
            "FWImage" => "<FWImage Stretch=\"UniformToFill\" IsZoomEnabled=\"True\" MinZoom=\"0.75\" MaxZoom=\"4\" />",
            "FWLabel and FWSeparator" => "<FWLabel Content=\"Name\" Target=\"{Binding ElementName=NameBox}\" AccessKey=\"N\" />\n<FWSeparator />",
            "FWViewbox" => "<FWViewbox Stretch=\"Uniform\" StretchDirection=\"Both\" />",
            _ => "<FWFluentMaterialSurface MaterialKind=\"LiquidGlass\">\n  <FWImage Stretch=\"UniformToFill\" />\n  <FWViewbox Stretch=\"Uniform\" />\n</FWFluentMaterialSurface>"
        };
    }

    private static FWWrapPanel CreateVisualButtonRow(params FWButton[] buttons)
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

    private static FWButton CreateVisualActionButton(FluentIconRegular icon, string text, Action action)
    {
        var button = new FWButton
        {
            Content = CreateVisualButtonContent(icon, text)
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static FWStackPanel CreateVisualButtonContent(FluentIconRegular icon, string text)
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

    private static TextBlock CreateVisualOutput(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        };
    }

    private static FWBorder CreateVisualStatus(TextBlock status)
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
                        CreateIcon(FluentIconRegular.Image24, 24, ThemeBrush("TextPrimary")),
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

    private static FluentIcon CreateIcon(FluentIconRegular icon, double size = FluentIcon.DefaultSize, Brush? foreground = null)
    {
        return FluentIconFactory.Regular(icon, size, foreground ?? ThemeBrush("TextPrimary"));
    }

    private static string FormatOnOff(bool value)
    {
        return value ? "on" : "off";
    }

    private static BitmapImage CreateSampleBitmap()
    {
        const int width = 96;
        const int height = 64;
        var pixels = new byte[width * height * 4];

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var index = ((y * width) + x) * 4;
                pixels[index] = (byte)(0x72 + (x * 0x30 / width));
                pixels[index + 1] = (byte)(0x45 + (y * 0x70 / height));
                pixels[index + 2] = (byte)(0xD4 - (x * 0x60 / width));
                pixels[index + 3] = 0xFF;
            }
        }

        return BitmapImage.FromPixels(pixels, width, height);
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
