using FluentJalium.Icon;
using FluentJalium.Gallery.Controls;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using FWBitmapIcon = FluentJalium.Controls.FWBitmapIcon;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWButton = FluentJalium.Controls.FWButton;
using FWEllipse = FluentJalium.Controls.FWEllipse;
using FWFluentMaterialKind = FluentJalium.Controls.FWFluentMaterialKind;
using FWFluentMaterialSurface = FluentJalium.Controls.FWFluentMaterialSurface;
using FWFontIcon = FluentJalium.Controls.FWFontIcon;
using FWImage = FluentJalium.Controls.FWImage;
using FWImageIcon = FluentJalium.Controls.FWImageIcon;
using FWLabel = FluentJalium.Controls.FWLabel;
using FWLine = FluentJalium.Controls.FWLine;
using FWMarkdown = FluentJalium.Controls.FWMarkdown;
using FWPath = FluentJalium.Controls.FWPath;
using FWPathIcon = FluentJalium.Controls.FWPathIcon;
using FWPersonPicture = FluentJalium.Controls.FWPersonPicture;
using FWPolygon = FluentJalium.Controls.FWPolygon;
using FWPolyline = FluentJalium.Controls.FWPolyline;
using FWQRCode = FluentJalium.Controls.FWQRCode;
using FWRectangle = FluentJalium.Controls.FWRectangle;
using FWRichTextBlock = FluentJalium.Controls.FWRichTextBlock;
using FWSeparator = FluentJalium.Controls.FWSeparator;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWSymbolIcon = FluentJalium.Controls.FWSymbolIcon;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWTextBox = FluentJalium.Controls.FWTextBox;
using FWViewbox = FluentJalium.Controls.FWViewbox;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;
using ShapePointCollection = Jalium.UI.Controls.Shapes.PointCollection;

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
            "FWBitmapIcon, FWImageIcon, and FWRichTextBlock",
            "Bitmap and image icon sizing plus selectable, wrapping rich text for WinUI-style visual compatibility.",
            CreateCompatibilityVisualsSample()));
        examples.Children.Add(CreateVisualExampleCard(
            FluentIconRegular.Person32,
            "FWPersonPicture",
            "Person, group, badge, profile image, initials, and small-image preference states for avatar surfaces.",
            CreatePersonPictureSample()));
        examples.Children.Add(CreateVisualExampleCard(
            FluentIconRegular.Markdown20,
            "FWMarkdown",
            "Markdown blocks, tables, links, quotes, and code styling pick up Fluent text and accent resources.",
            CreateMarkdownSample()));
        examples.Children.Add(CreateVisualExampleCard(
            FluentIconRegular.QrCode20,
            "FWQRCode",
            "QR payloads, error correction, quiet zone, logo, module shape, and eye shape stay theme-aware.",
            CreateQRCodeSample()));
        examples.Children.Add(CreateVisualExampleCard(
            FluentIconRegular.Shapes28,
            "Fluent shape controls",
            "FWRectangle, FWEllipse, FWLine, FWPolyline, FWPolygon, and FWPath show themed fill, stroke, and disabled states.",
            CreateShapeControlsSample()));
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
                Glyph = FluentIconRegular.Share24.GetString(),
                FontFamily = FluentIcon.RegularFontFamily,
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

    private static UIElement CreateCompatibilityVisualsSample()
    {
        var output = CreateVisualOutput("BitmapIcon: 24px monochrome. ImageIcon: 32px. RichTextBlock: Wrap with selection.");
        var bitmapIcon = new FWBitmapIcon
        {
            Source = CreateSampleBitmap(),
            Width = 24,
            Height = 24,
            Stretch = Stretch.Uniform,
            ShowAsMonochrome = true
        };
        var imageIcon = new FWImageIcon
        {
            Source = CreateSampleBitmap(),
            Width = 32,
            Height = 32,
            Stretch = Stretch.UniformToFill
        };
        var richTextBlock = new FWRichTextBlock
        {
            Text = "FWRichTextBlock keeps long descriptive copy readable with wrapping and selectable text for copy-friendly gallery notes.",
            Width = 300,
            Foreground = ThemeBrush("TextPrimary"),
            TextWrapping = TextWrapping.Wrap,
            IsTextSelectionEnabled = true
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 390,
            Children =
            {
                new FWLabel { Content = "WinUI visual compatibility" },
                new FWWrapPanel
                {
                    HorizontalSpacing = 8,
                    VerticalSpacing = 8,
                    Children =
                    {
                        CreateCompatibilityIconPreview("FWBitmapIcon", "24px / monochrome", bitmapIcon),
                        CreateCompatibilityIconPreview("FWImageIcon", "32px / color image", imageIcon)
                    }
                },
                CreateRichTextPreview(richTextBlock),
                CreateVisualButtonRow(
                    CreateVisualActionButton(FluentIconRegular.Color24, "Mono", () =>
                    {
                        bitmapIcon.ShowAsMonochrome = !bitmapIcon.ShowAsMonochrome;
                        output.Text = $"BitmapIcon monochrome: {FormatOnOff(bitmapIcon.ShowAsMonochrome)}.";
                    }),
                    CreateVisualActionButton(FluentIconRegular.ResizeLarge24, "Size", () =>
                    {
                        var useLargeSize = bitmapIcon.Width <= 24;
                        var bitmapSize = useLargeSize ? 36 : 24;
                        var imageSize = useLargeSize ? 44 : 32;

                        bitmapIcon.Width = bitmapSize;
                        bitmapIcon.Height = bitmapSize;
                        imageIcon.Width = imageSize;
                        imageIcon.Height = imageSize;
                        output.Text = $"Icon sizes: bitmap {bitmapSize}px, image {imageSize}px.";
                    }),
                    CreateVisualActionButton(FluentIconRegular.ArrowAutofitHeight24, "Wrap", () =>
                    {
                        richTextBlock.TextWrapping = richTextBlock.TextWrapping == TextWrapping.Wrap
                            ? TextWrapping.NoWrap
                            : TextWrapping.Wrap;
                        output.Text = $"RichTextBlock wrapping: {richTextBlock.TextWrapping}.";
                    }),
                    CreateVisualActionButton(FluentIconRegular.TextEditStyle24, "Select", () =>
                    {
                        richTextBlock.IsTextSelectionEnabled = !richTextBlock.IsTextSelectionEnabled;
                        output.Text = $"RichTextBlock selection: {FormatOnOff(richTextBlock.IsTextSelectionEnabled)}.";
                    })),
                CreateVisualStatus(output)
            }
        };
    }

    private static UIElement CreatePersonPictureSample()
    {
        var output = CreateVisualOutput("PersonPicture: initials RH, badge 2, individual mode.");
        var personPicture = new FWPersonPicture
        {
            Width = 72,
            Height = 72,
            DisplayName = "Rhea Holloway",
            Initials = "RH",
            BadgeNumber = 2,
            BadgeGlyph = "2"
        };
        var nameText = new FWTextBlock
        {
            Foreground = ThemeBrush("TextPrimary"),
            FontSize = 14
        };
        var detailText = new FWTextBlock
        {
            Foreground = ThemeBrush("TextSecondary"),
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            Width = 195
        };

        void UpdateReadout()
        {
            nameText.Text = personPicture.DisplayName;
            detailText.Text = personPicture.ProfilePicture != null
                ? $"Profile image, badge {personPicture.BadgeNumber}, small image {FormatOnOff(personPicture.PreferSmallImage)}."
                : personPicture.IsGroup
                ? $"Group avatar, badge {personPicture.BadgeNumber}, small image {FormatOnOff(personPicture.PreferSmallImage)}."
                : $"Initials {personPicture.Initials}, badge {personPicture.BadgeNumber}, small image {FormatOnOff(personPicture.PreferSmallImage)}.";
        }

        UpdateReadout();

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 390,
            Children =
            {
                new FWLabel { Content = "FWPersonPicture states" },
                new FWBorder
                {
                    Width = 320,
                    Background = ThemeBrush("LayerFillColorDefaultBrush"),
                    BorderBrush = ThemeBrush("ControlBorder"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(12),
                    Child = new FWStackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 14,
                        Children =
                        {
                            personPicture,
                            new FWStackPanel
                            {
                                Orientation = Orientation.Vertical,
                                Spacing = 4,
                                VerticalAlignment = VerticalAlignment.Center,
                                Children =
                                {
                                    nameText,
                                    detailText
                                }
                            }
                        }
                    }
                },
                CreateVisualButtonRow(
                    CreateVisualActionButton(FluentIconRegular.Person32, "Person", () =>
                    {
                        personPicture.ProfilePicture = null;
                        personPicture.DisplayName = "Rhea Holloway";
                        personPicture.Initials = "RH";
                        personPicture.IsGroup = false;
                        personPicture.BadgeNumber = 2;
                        personPicture.BadgeGlyph = "2";
                        output.Text = "PersonPicture: initials fallback with numeric badge.";
                        UpdateReadout();
                    }),
                    CreateVisualActionButton(FluentIconRegular.Image24, "Image", () =>
                    {
                        personPicture.ProfilePicture = CreateSampleBitmap();
                        personPicture.DisplayName = "Rhea Holloway";
                        personPicture.Initials = string.Empty;
                        personPicture.IsGroup = false;
                        personPicture.BadgeNumber = 1;
                        personPicture.BadgeGlyph = "1";
                        output.Text = "PersonPicture: profile image source applied.";
                        UpdateReadout();
                    }),
                    CreateVisualActionButton(FluentIconRegular.People32, "Group", () =>
                    {
                        personPicture.ProfilePicture = null;
                        personPicture.DisplayName = "Design Review";
                        personPicture.Initials = "UX";
                        personPicture.IsGroup = true;
                        personPicture.BadgeNumber = 8;
                        personPicture.BadgeGlyph = "8";
                        output.Text = "PersonPicture: group mode with generated team initials.";
                        UpdateReadout();
                    }),
                    CreateVisualActionButton(FluentIconRegular.ResizeImage20, "Small", () =>
                    {
                        personPicture.PreferSmallImage = !personPicture.PreferSmallImage;
                        var size = personPicture.PreferSmallImage ? 56 : 72;
                        personPicture.Width = size;
                        personPicture.Height = size;
                        output.Text = $"PersonPicture PreferSmallImage: {FormatOnOff(personPicture.PreferSmallImage)}.";
                        UpdateReadout();
                    })),
                CreateVisualStatus(output)
            }
        };
    }

    private static UIElement CreateMarkdownSample()
    {
        var output = CreateVisualOutput("Markdown: overview content, relative links handled in-gallery.");
        var markdown = new FWMarkdown
        {
            Width = 320,
            Height = 190,
            Text = MarkdownOverviewText,
            BaseUri = new Uri("https://jalium.dev/fluent/"),
            OpenLinksExternally = false
        };
        markdown.LinkClicked += (_, args) =>
        {
            args.Handled = true;
            output.Text = $"Markdown link resolved: {args.Uri}.";
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 390,
            Children =
            {
                new FWLabel { Content = "FWMarkdown content" },
                new FWBorder
                {
                    Width = 342,
                    Background = ThemeBrush("LayerFillColorDefaultBrush"),
                    BorderBrush = ThemeBrush("ControlBorder"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(10),
                    Child = markdown
                },
                CreateVisualButtonRow(
                    CreateVisualActionButton(FluentIconRegular.Markdown20, "Notes", () =>
                    {
                        markdown.Text = MarkdownOverviewText;
                        output.Text = "Markdown: overview source restored.";
                    }),
                    CreateVisualActionButton(FluentIconRegular.TableSparkle20, "Table", () =>
                    {
                        markdown.Text = MarkdownTableText;
                        output.Text = "Markdown: table and inline code rendered.";
                    }),
                    CreateVisualActionButton(FluentIconRegular.Link32, "Links", () =>
                    {
                        markdown.OpenLinksExternally = !markdown.OpenLinksExternally;
                        output.Text = $"Markdown external links: {FormatOnOff(markdown.OpenLinksExternally)}.";
                    }),
                    CreateVisualActionButton(FluentIconRegular.Color24, "Tone", () =>
                    {
                        if (markdown.CodeBackground == null)
                        {
                            markdown.LinkForeground = ThemeBrush("AccentBrush");
                            markdown.CodeBackground = ThemeBrush("SelectionBackgroundWeak");
                            markdown.QuoteBackground = ThemeBrush("LayerFillColorDefaultBrush");
                            output.Text = "Markdown: accent link, code, and quote brushes.";
                        }
                        else
                        {
                            markdown.LinkForeground = null;
                            markdown.CodeBackground = null;
                            markdown.QuoteBackground = null;
                            output.Text = "Markdown: default code and quote brushes.";
                        }
                    })),
                CreateVisualStatus(output)
            }
        };
    }

    private static UIElement CreateQRCodeSample()
    {
        var output = CreateVisualOutput("QRCode: https payload, ECC Q, rounded modules.");
        var payloadBox = new FWTextBox
        {
            Text = "https://jalium.dev/fluent",
            Width = 150
        };
        var qrCode = new FWQRCode
        {
            Text = payloadBox.Text,
            Width = 155,
            Height = 155,
            QuietZoneModules = 3,
            ErrorCorrectionLevel = QRCodeErrorCorrectionLevel.Q,
            Encoding = QRCodeEncoding.Utf8,
            ModuleShape = QRModuleShape.RoundedSquare,
            EyeShape = QREyeShape.Rounded,
            IsForegroundGradient = false
        };
        payloadBox.TextChanged += (_, _) => output.Text = $"QRCode payload draft: {payloadBox.Text.Length} chars.";

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 390,
            Children =
            {
                new FWLabel { Content = "FWQRCode generator" },
                new FWStackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 14,
                    Children =
                    {
                        qrCode,
                        new FWStackPanel
                        {
                            Orientation = Orientation.Vertical,
                            Spacing = 8,
                            VerticalAlignment = VerticalAlignment.Center,
                            Children =
                            {
                                new FWTextBlock
                                {
                                    Text = "Payload",
                                    Foreground = ThemeBrush("TextSecondary"),
                                    FontSize = 12
                                },
                                payloadBox
                            }
                        }
                    }
                },
                CreateVisualButtonRow(
                    CreateVisualActionButton(FluentIconRegular.QrCode20, "Apply", () =>
                    {
                        qrCode.Text = string.IsNullOrWhiteSpace(payloadBox.Text)
                            ? "https://jalium.dev/fluent"
                            : payloadBox.Text.Trim();
                        output.Text = $"QRCode payload applied: {qrCode.Text.Length} chars.";
                    }),
                    CreateVisualActionButton(FluentIconRegular.TextBulletListSquare20, "Payload", () =>
                    {
                        payloadBox.Text = payloadBox.Text.StartsWith("WIFI:", StringComparison.Ordinal)
                            ? "https://jalium.dev/fluent"
                            : "WIFI:T:WPA;S:FluentJalium;P:samplepass;;";
                        qrCode.Text = payloadBox.Text;
                        output.Text = payloadBox.Text.StartsWith("WIFI:", StringComparison.Ordinal)
                            ? "QRCode payload: Wi-Fi credentials."
                            : "QRCode payload: gallery URL.";
                    }),
                    CreateVisualActionButton(FluentIconRegular.ShieldBadge24, "ECC", () =>
                    {
                        qrCode.ErrorCorrectionLevel = qrCode.ErrorCorrectionLevel == QRCodeErrorCorrectionLevel.H
                            ? QRCodeErrorCorrectionLevel.Q
                            : QRCodeErrorCorrectionLevel.H;
                        output.Text = $"QRCode ECC: {qrCode.ErrorCorrectionLevel}.";
                    }),
                    CreateVisualActionButton(FluentIconRegular.DrawShape24, "Shape", () =>
                    {
                        if (qrCode.ModuleShape == QRModuleShape.RoundedSquare)
                        {
                            qrCode.ModuleShape = QRModuleShape.Circle;
                            qrCode.EyeShape = QREyeShape.Leaf;
                        }
                        else
                        {
                            qrCode.ModuleShape = QRModuleShape.RoundedSquare;
                            qrCode.EyeShape = QREyeShape.Rounded;
                        }

                        output.Text = $"QRCode shape: {qrCode.ModuleShape}, eyes {qrCode.EyeShape}.";
                    }),
                    CreateVisualActionButton(FluentIconRegular.Image24, "Logo", () =>
                    {
                        qrCode.LogoImage = qrCode.LogoImage == null ? CreateSampleBitmap() : null;
                        if (qrCode.LogoImage != null)
                        {
                            qrCode.ErrorCorrectionLevel = QRCodeErrorCorrectionLevel.H;
                            qrCode.LogoSizeRatio = 0.2;
                        }

                        output.Text = qrCode.LogoImage == null
                            ? "QRCode logo removed."
                            : "QRCode logo image added with ECC H.";
                    }),
                    CreateVisualActionButton(FluentIconRegular.ArrowAutofitHeight24, "Quiet", () =>
                    {
                        qrCode.QuietZoneModules = qrCode.QuietZoneModules == 3 ? 1 : 3;
                        output.Text = $"QRCode quiet zone: {qrCode.QuietZoneModules} modules.";
                    })),
                CreateVisualStatus(output)
            }
        };
    }

    private static UIElement CreateShapeControlsSample()
    {
        var output = CreateVisualOutput("Shapes: themed fill and stroke with rounded line joins.");
        var rectangle = new FWRectangle
        {
            Width = 86,
            Height = 48,
            RadiusX = 8,
            RadiusY = 8,
            Fill = ThemeBrush("SelectionBackgroundWeak"),
            Stroke = ThemeBrush("AccentBrush"),
            StrokeThickness = 1.5
        };
        var ellipse = new FWEllipse
        {
            Width = 56,
            Height = 56,
            Fill = ThemeBrush("SelectionBackgroundWeak"),
            Stroke = ThemeBrush("AccentBrush"),
            StrokeThickness = 1.5
        };
        var line = new FWLine
        {
            Width = 88,
            Height = 58,
            X1 = 4,
            Y1 = 42,
            X2 = 84,
            Y2 = 10,
            Stroke = ThemeBrush("AccentBrush"),
            StrokeThickness = 3,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round
        };
        var polyline = new FWPolyline
        {
            Width = 88,
            Height = 58,
            Points = ShapePointCollection.Parse("0,40 18,14 38,34 58,8 82,28"),
            Stroke = ThemeBrush("AccentBrush"),
            StrokeThickness = 3,
            StrokeLineJoin = PenLineJoin.Round
        };
        var polygon = new FWPolygon
        {
            Width = 88,
            Height = 58,
            Points = ShapePointCollection.Parse("8,44 28,8 52,22 74,44"),
            Fill = ThemeBrush("SelectionBackgroundWeak"),
            Stroke = ThemeBrush("AccentBrush"),
            StrokeThickness = 1.5,
            StrokeLineJoin = PenLineJoin.Round,
            FillRule = FillRule.Nonzero
        };
        var path = new FWPath
        {
            Width = 88,
            Height = 56,
            Data = "M 4,36 C 16,6 38,6 48,30 S 74,54 84,20",
            Fill = ThemeBrush("SelectionBackgroundWeak"),
            Stroke = ThemeBrush("AccentBrush"),
            StrokeThickness = 1.5,
            StrokeLineJoin = PenLineJoin.Round,
            Stretch = Stretch.Uniform
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 390,
            Children =
            {
                new FWLabel { Content = "Fluent shape controls" },
                new FWWrapPanel
                {
                    HorizontalSpacing = 8,
                    VerticalSpacing = 8,
                    Children =
                    {
                        CreateShapePreviewTile("FWRectangle", rectangle),
                        CreateShapePreviewTile("FWEllipse", ellipse),
                        CreateShapePreviewTile("FWLine", line),
                        CreateShapePreviewTile("FWPolyline", polyline),
                        CreateShapePreviewTile("FWPolygon", polygon),
                        CreateShapePreviewTile("FWPath", path)
                    }
                },
                CreateVisualButtonRow(
                    CreateVisualActionButton(FluentIconRegular.Color24, "Accent", () =>
                    {
                        ApplyShapeBrushes(rectangle, ellipse, polygon, path, ThemeBrush("SelectionBackgroundWeak"), ThemeBrush("AccentBrush"));
                        line.Stroke = ThemeBrush("AccentBrush");
                        polyline.Stroke = ThemeBrush("AccentBrush");
                        output.Text = "Shapes: accent fill and stroke restored.";
                    }),
                    CreateVisualActionButton(FluentIconRegular.DrawShape24, "Morph", () =>
                    {
                        rectangle.RadiusX = rectangle.RadiusX == 8 ? 22 : 8;
                        rectangle.RadiusY = rectangle.RadiusY == 8 ? 22 : 8;
                        line.X2 = line.X2 == 84 ? 54 : 84;
                        line.Y2 = line.Y2 == 10 ? 42 : 10;
                        polyline.Points = polyline.Points?.Count == 5
                            ? ShapePointCollection.Parse("4,20 22,38 42,12 62,38 84,18 84,42")
                            : ShapePointCollection.Parse("0,40 18,14 38,34 58,8 82,28");
                        polygon.Points = polygon.Points?.Count == 4
                            ? ShapePointCollection.Parse("8,44 28,8 52,22 74,44")
                            : ShapePointCollection.Parse("8,28 28,8 52,8 74,28 60,48 22,48");
                        path.Data = path.Data.StartsWith("M 4,36", StringComparison.Ordinal)
                            ? "M 6,44 L 22,10 L 42,38 L 62,10 L 82,44 Z"
                            : "M 4,36 C 16,6 38,6 48,30 S 74,54 84,20";
                        output.Text = "Shapes: geometry, radius, and path data morphed.";
                    }),
                    CreateVisualActionButton(FluentIconRegular.Prohibited24, "Disable", () =>
                    {
                        var isEnabled = !rectangle.IsEnabled;
                        SetShapeEnabled(isEnabled, rectangle, ellipse, line, polyline, polygon, path);
                        output.Text = $"Shapes enabled: {FormatOnOff(isEnabled)}.";
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

    private static FWBorder CreateCompatibilityIconPreview(string label, string detail, FrameworkElement icon)
    {
        return new FWBorder
        {
            Width = 150,
            Height = 104,
            Background = ThemeBrush("LayerFillColorDefaultBrush"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10),
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
                        FontSize = 12,
                        Foreground = ThemeBrush("TextPrimary"),
                        HorizontalAlignment = HorizontalAlignment.Center
                    },
                    new FWTextBlock
                    {
                        Text = detail,
                        FontSize = 11,
                        Foreground = ThemeBrush("TextSecondary"),
                        HorizontalAlignment = HorizontalAlignment.Center
                    }
                }
            }
        };
    }

    private static FWBorder CreateRichTextPreview(FWRichTextBlock richTextBlock)
    {
        return new FWBorder
        {
            Width = 320,
            Background = ThemeBrush("LayerFillColorDefaultBrush"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10),
            Child = richTextBlock
        };
    }

    private static FWBorder CreateShapePreviewTile(string label, FrameworkElement shape)
    {
        shape.HorizontalAlignment = HorizontalAlignment.Center;
        shape.VerticalAlignment = VerticalAlignment.Center;

        return new FWBorder
        {
            Width = 104,
            Height = 96,
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
                    new FWBorder
                    {
                        Width = 88,
                        Height = 58,
                        Child = shape
                    },
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

    private static void ApplyShapeBrushes(
        FWRectangle rectangle,
        FWEllipse ellipse,
        FWPolygon polygon,
        FWPath path,
        Brush fill,
        Brush stroke)
    {
        rectangle.Fill = fill;
        rectangle.Stroke = stroke;
        ellipse.Fill = fill;
        ellipse.Stroke = stroke;
        polygon.Fill = fill;
        polygon.Stroke = stroke;
        path.Fill = fill;
        path.Stroke = stroke;
    }

    private static void SetShapeEnabled(bool isEnabled, params FrameworkElement[] shapes)
    {
        foreach (var shape in shapes)
        {
            shape.IsEnabled = isEnabled;
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
            "FWBitmapIcon, FWImageIcon, and FWRichTextBlock" => "<FWBitmapIcon Source=\"...\" ShowAsMonochrome=\"True\" Width=\"24\" Height=\"24\" />\n<FWImageIcon Source=\"...\" Width=\"32\" Height=\"32\" />\n<FWRichTextBlock TextWrapping=\"Wrap\" IsTextSelectionEnabled=\"True\" />",
            "FWPersonPicture" => "<FWPersonPicture DisplayName=\"Rhea Holloway\" Initials=\"RH\" BadgeNumber=\"2\" />",
            "FWMarkdown" => "<FWMarkdown Text=\"# Visual notes\" BaseUri=\"https://jalium.dev/fluent/\" OpenLinksExternally=\"False\" />",
            "FWQRCode" => "<FWQRCode Text=\"https://jalium.dev/fluent\" ErrorCorrectionLevel=\"Q\" ModuleShape=\"RoundedSquare\" EyeShape=\"Rounded\" />",
            "Fluent shape controls" => "<FWRectangle RadiusX=\"8\" RadiusY=\"8\" />\n<FWEllipse />\n<FWLine X1=\"4\" Y1=\"42\" X2=\"84\" Y2=\"10\" />\n<FWPolyline Points=\"0,40 18,14 38,34\" />\n<FWPolygon Points=\"8,44 28,8 52,22\" />\n<FWPath Data=\"M 4,36 C 16,6 38,6 48,30\" />",
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

    private const string MarkdownOverviewText =
        "# Visual notes\n\n" +
        "- `FWMarkdown` renders headings, lists, links, and inline code.\n" +
        "- Relative [gallery links](/gallery/visuals) resolve against `BaseUri`.\n\n" +
        "> Fluent brushes keep quotes and code blocks readable.";

    private const string MarkdownTableText =
        "## Coverage\n\n" +
        "| Control | Visual role |\n" +
        "| --- | --- |\n" +
        "| `FWMarkdown` | Rich document text |\n" +
        "| `FWQRCode` | Encoded payload visual |\n" +
        "| `FWPath` | Custom vector shape |\n\n" +
        "`OpenLinksExternally` can be toggled from the sample.";

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
