using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Ink;
using Jalium.UI.Input;
using Jalium.UI.Media;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWButton = FluentJalium.Controls.FWButton;
using FWColorPicker = FluentJalium.Controls.FWColorPicker;
using FWFluentMaterialKind = FluentJalium.Controls.FWFluentMaterialKind;
using FWFluentMaterialSurface = FluentJalium.Controls.FWFluentMaterialSurface;
using FWInkCanvas = FluentJalium.Controls.FWInkCanvas;
using FWInkPresenter = FluentJalium.Controls.FWInkPresenter;
using FWLabel = FluentJalium.Controls.FWLabel;
using FWMediaElement = FluentJalium.Controls.FWMediaElement;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;

namespace FluentJalium.Gallery.Pages;

internal sealed class GalleryInputMediaPage
{
    public UIElement CreateContent()
    {
        var panel = CreateSection("Advanced Input and Media");
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        examples.Children.Add(CreateInputMediaExampleCard(
            FluentIconRegular.Color24,
            "FWColorPicker",
            "Alpha, compact, preview, hex input, spectrum shape, and live color change states.",
            CreateColorPickerSample()));
        examples.Children.Add(CreateInputMediaExampleCard(
            FluentIconRegular.InkStroke24,
            "FWInkCanvas",
            "Ink and erase modes, default drawing attributes, tapering, and Fluent surface border.",
            CreateInkCanvasSample()));
        examples.Children.Add(CreateInputMediaExampleCard(
            FluentIconRegular.Pen24,
            "FWInkPresenter",
            "Existing stroke collections presented as a read-only Fluent ink surface.",
            CreateInkPresenterSample()));
        examples.Children.Add(CreateInputMediaExampleCard(
            FluentIconRegular.VideoPlayPause24,
            "FWMediaElement",
            "Manual playback surface with play, pause, stop, stretch, mute, and surface styling states.",
            CreateMediaElementSample()));
        examples.Children.Add(CreateInputMediaExampleCard(
            FluentIconRegular.LayerDiagonalSparkle24,
            "Material input and media workbench",
            "Color, ink, stroke presenter, and media surfaces remain readable on LiquidGlass.",
            CreateMaterialInputMediaWorkbenchSample()));

        panel.Children.Add(examples);
        return panel;
    }

    private static UIElement CreateColorPickerSample()
    {
        var output = CreateInputMediaOutput("ColorPicker: blue, alpha on, preview and hex visible.");
        var picker = new FWColorPicker
        {
            Color = Color.FromRgb(0x00, 0x78, 0xD4),
            IsAlphaEnabled = true,
            IsColorPreviewVisible = true,
            IsHexInputVisible = true,
            ColorSpectrumShape = ColorSpectrumShape.Box
        };
        var compact = new FWColorPicker
        {
            Color = Color.FromRgb(0xD8, 0x3B, 0x01),
            IsCompact = true,
            IsAlphaEnabled = false,
            IsColorPreviewVisible = true,
            IsHexInputVisible = true
        };
        picker.ColorChanged += (_, args) => output.Text = $"Color changed: A{args.NewColor.A:X2} R{args.NewColor.R:X2} G{args.NewColor.G:X2} B{args.NewColor.B:X2}.";

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 390,
            Children =
            {
                new FWLabel { Content = "FWColorPicker modes" },
                picker,
                compact,
                CreateInputMediaButtonRow(
                    CreateInputMediaActionButton(FluentIconRegular.Color24, "Blue", () =>
                    {
                        picker.Color = Color.FromRgb(0x00, 0x78, 0xD4);
                        output.Text = "ColorPicker: blue accent.";
                    }),
                    CreateInputMediaActionButton(FluentIconRegular.TextColor24, "Rose", () =>
                    {
                        picker.Color = Color.FromArgb(0xCC, 0xC2, 0x39, 0xB3);
                        output.Text = "ColorPicker: rose with alpha.";
                    }),
                    CreateInputMediaActionButton(FluentIconRegular.DrawShape24, "Ring", () =>
                    {
                        picker.ColorSpectrumShape = picker.ColorSpectrumShape == ColorSpectrumShape.Box
                            ? ColorSpectrumShape.Ring
                            : ColorSpectrumShape.Box;
                        output.Text = $"Color spectrum: {picker.ColorSpectrumShape}.";
                    })),
                CreateInputMediaStatus(output)
            }
        };
    }

    private static UIElement CreateInkCanvasSample()
    {
        var output = CreateInputMediaOutput("InkCanvas: ink mode, tapered stroke, blue pen.");
        var canvas = new FWInkCanvas
        {
            Width = 330,
            Height = 180,
            Background = ThemeBrush("InkCanvasBackground"),
            BorderBrush = ThemeBrush("InkCanvasBorderBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            DefaultDrawingAttributes = CreateInkDrawingAttributes(Color.FromRgb(0x4C, 0xC2, 0xFF), 3),
            DefaultStrokeTaperMode = StrokeTaperMode.TaperedEnd,
            EditingMode = InkCanvasEditingMode.Ink,
            EraserDiameter = 18
        };
        canvas.EditingModeChanged += (_, _) => output.Text = $"InkCanvas mode: {canvas.EditingMode}.";
        canvas.StrokesChanged += (_, _) => output.Text = $"InkCanvas strokes: {canvas.Strokes.Count}.";

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 390,
            Children =
            {
                new FWLabel { Content = "FWInkCanvas surface" },
                canvas,
                CreateInputMediaButtonRow(
                    CreateInputMediaActionButton(FluentIconRegular.Pen24, "Draw", () =>
                    {
                        canvas.EditingMode = InkCanvasEditingMode.Ink;
                        output.Text = "InkCanvas mode: Ink.";
                    }),
                    CreateInputMediaActionButton(FluentIconRegular.Eraser24, "Erase", () =>
                    {
                        canvas.EditingMode = InkCanvasEditingMode.EraseByStroke;
                        output.Text = "InkCanvas mode: EraseByStroke.";
                    }),
                    CreateInputMediaActionButton(FluentIconRegular.DismissCircle24, "Clear", () =>
                    {
                        canvas.ClearStrokes();
                        output.Text = "InkCanvas strokes cleared.";
                    })),
                CreateInputMediaStatus(output)
            }
        };
    }

    private static UIElement CreateInkPresenterSample()
    {
        var output = CreateInputMediaOutput("InkPresenter: two strokes loaded.");
        var strokes = CreateSampleStrokes();
        var presenter = new FWInkPresenter
        {
            Strokes = strokes
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 390,
            Children =
            {
                new FWLabel { Content = "FWInkPresenter strokes" },
                new FWBorder
                {
                    Width = 330,
                    Height = 150,
                    Background = ThemeBrush("InkCanvasBackground"),
                    BorderBrush = ThemeBrush("InkCanvasBorderBrush"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    Child = presenter
                },
                CreateInputMediaButtonRow(
                    CreateInputMediaActionButton(FluentIconRegular.InkStroke24, "Reload", () =>
                    {
                        presenter.Strokes = CreateSampleStrokes();
                        output.Text = $"InkPresenter strokes: {presenter.Strokes?.Count}.";
                    }),
                    CreateInputMediaActionButton(FluentIconRegular.Color24, "Accent", () =>
                    {
                        presenter.Strokes = new StrokeCollection
                        {
                            CreateStroke(Color.FromRgb(0x00, 0x78, 0xD4),
                                new StylusPoint(22, 78),
                                new StylusPoint(100, 32),
                                new StylusPoint(220, 92))
                        };
                        output.Text = "InkPresenter: accent stroke.";
                    })),
                CreateInputMediaStatus(output)
            }
        };
    }

    private static UIElement CreateMediaElementSample()
    {
        var output = CreateInputMediaOutput("MediaElement: manual playback surface, uniform stretch.");
        var media = CreateMediaElement(width: 320, height: 180);

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 390,
            Children =
            {
                new FWLabel { Content = "FWMediaElement playback surface" },
                CreateMediaPreview(media, "Media surface"),
                CreateInputMediaButtonRow(
                    CreateInputMediaActionButton(FluentIconRegular.Play24, "Play", () =>
                    {
                        media.LoadedBehavior = MediaState.Manual;
                        output.Text = "MediaElement play command prepared.";
                    }),
                    CreateInputMediaActionButton(FluentIconRegular.Pause24, "Mute", () =>
                    {
                        media.IsMuted = !media.IsMuted;
                        output.Text = $"MediaElement muted: {FormatOnOff(media.IsMuted)}.";
                    }),
                    CreateInputMediaActionButton(FluentIconRegular.Stop24, "Fill", () =>
                    {
                        media.Stretch = media.Stretch == Stretch.Uniform ? Stretch.UniformToFill : Stretch.Uniform;
                        output.Text = $"MediaElement stretch: {media.Stretch}.";
                    })),
                CreateInputMediaStatus(output)
            }
        };
    }

    private static UIElement CreateMaterialInputMediaWorkbenchSample()
    {
        var output = CreateInputMediaOutput("Workbench: LiquidGlass. Color, ink, presenter, and media ready.");
        var picker = new FWColorPicker
        {
            Color = Color.FromRgb(0x00, 0x78, 0xD4),
            IsCompact = true,
            IsAlphaEnabled = true,
            IsColorPreviewVisible = true,
            IsHexInputVisible = true
        };
        var canvas = new FWInkCanvas
        {
            Width = 220,
            Height = 126,
            Background = ThemeBrush("InkCanvasBackground"),
            BorderBrush = ThemeBrush("InkCanvasBorderBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            DefaultDrawingAttributes = CreateInkDrawingAttributes(Color.FromRgb(0x4C, 0xC2, 0xFF), 3),
            DefaultStrokeTaperMode = StrokeTaperMode.TaperedEnd,
            EditingMode = InkCanvasEditingMode.Ink
        };
        var presenter = new FWInkPresenter
        {
            Strokes = CreateSampleStrokes()
        };
        var media = CreateMediaElement(width: 220, height: 126);

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
                            CreateMaterialPreview("Color", picker),
                            CreateMaterialPreview("Ink", canvas),
                            CreateMaterialPreview("Strokes", new FWBorder
                            {
                                Width = 220,
                                Height = 126,
                                Background = ThemeBrush("InkCanvasBackground"),
                                BorderBrush = ThemeBrush("InkCanvasBorderBrush"),
                                BorderThickness = new Thickness(1),
                                CornerRadius = new CornerRadius(6),
                                Child = presenter
                            }),
                            CreateMaterialPreview("Media", CreateMediaPreview(media, "Media"))
                        }
                    },
                    CreateInputMediaButtonRow(
                        CreateInputMediaActionButton(FluentIconRegular.Color24, "Color", () =>
                        {
                            picker.Color = Color.FromRgb(0xC2, 0x39, 0xB3);
                            output.Text = "Workbench color: rose.";
                        }),
                        CreateInputMediaActionButton(FluentIconRegular.Eraser24, "Erase", () =>
                        {
                            canvas.EditingMode = InkCanvasEditingMode.EraseByStroke;
                            output.Text = "Workbench ink: erase mode.";
                        }),
                        CreateInputMediaActionButton(FluentIconRegular.Play24, "Media", () =>
                        {
                            media.IsMuted = !media.IsMuted;
                            output.Text = $"Workbench media muted: {FormatOnOff(media.IsMuted)}.";
                        })),
                    CreateInputMediaStatus(output)
                }
            }
        };
    }

    private static FWMediaElement CreateMediaElement(double width, double height)
    {
        return new FWMediaElement
        {
            Width = width,
            Height = height,
            Background = ThemeBrush("MediaElementBackground"),
            BorderBrush = ThemeBrush("MediaElementBorderBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            LoadedBehavior = MediaState.Manual,
            UnloadedBehavior = MediaState.Close,
            Stretch = Stretch.Uniform,
            StretchDirection = StretchDirection.Both,
            ScrubbingEnabled = true
        };
    }

    private static Grid CreateMediaPreview(FWMediaElement media, string title)
    {
        return new Grid
        {
            Width = media.Width,
            Height = media.Height,
            Children =
            {
                media,
                new FWStackPanel
                {
                    Orientation = Orientation.Vertical,
                    Spacing = 6,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Children =
                    {
                        CreateIcon(FluentIconRegular.Play24, 28, ThemeBrush("MediaElementForeground")),
                        new FWTextBlock
                        {
                            Text = title,
                            Foreground = ThemeBrush("MediaElementForeground")
                        }
                    }
                }
            }
        };
    }

    private static DrawingAttributes CreateInkDrawingAttributes(Color color, double size)
    {
        return new DrawingAttributes
        {
            Color = color,
            Width = size,
            Height = size,
            BrushType = BrushType.Pen,
            FitToCurve = true
        };
    }

    private static StrokeCollection CreateSampleStrokes()
    {
        return new StrokeCollection
        {
            CreateStroke(
                Color.FromRgb(0x4C, 0xC2, 0xFF),
                new StylusPoint(24, 74),
                new StylusPoint(62, 42),
                new StylusPoint(108, 72),
                new StylusPoint(162, 34),
                new StylusPoint(210, 64)),
            CreateStroke(
                Color.FromRgb(0xD8, 0x3B, 0x01),
                new StylusPoint(28, 92),
                new StylusPoint(84, 102),
                new StylusPoint(142, 90),
                new StylusPoint(202, 104))
        };
    }

    private static Stroke CreateStroke(Color color, params StylusPoint[] points)
    {
        return new Stroke(
            new StylusPointCollection(points),
            new DrawingAttributes
            {
                Color = color,
                Width = 4,
                Height = 4,
                BrushType = BrushType.Pen,
                FitToCurve = true
            })
        {
            TaperMode = StrokeTaperMode.TaperedEnd
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
                    Text = "Layered input and media surface",
                    FontSize = 15,
                    Foreground = ThemeBrush("TextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
    }

    private static FWBorder CreateInputMediaExampleCard(FluentIconRegular icon, string title, string description, UIElement content)
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

    private static FWWrapPanel CreateInputMediaButtonRow(params FWButton[] buttons)
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

    private static FWButton CreateInputMediaActionButton(FluentIconRegular icon, string text, Action action)
    {
        var button = new FWButton
        {
            Content = CreateInputMediaButtonContent(icon, text)
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static FWStackPanel CreateInputMediaButtonContent(FluentIconRegular icon, string text)
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

    private static TextBlock CreateInputMediaOutput(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        };
    }

    private static FWBorder CreateInputMediaStatus(TextBlock status)
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
                        CreateIcon(FluentIconRegular.Color24, 24, ThemeBrush("TextPrimary")),
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

    private static Brush ThemeBrush(string key)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Brush brush)
        {
            return brush;
        }

        return new SolidColorBrush(Colors.Transparent);
    }
}
