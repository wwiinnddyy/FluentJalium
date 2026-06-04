using System.Globalization;
using FluentJalium.Icon;
using FluentJalium.Gallery.Controls;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using Jalium.UI.Media.Effects;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWGrid = FluentJalium.Controls.FWGrid;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;

namespace FluentJalium.Gallery.Pages;

internal sealed class GalleryGeometryPage
{
    public UIElement CreateContent()
    {
        var panel = CreateSection("Geometry");
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        panel.Children.Add(CreateGeometryTokenStrip());
        examples.Children.Add(CreateGeometryExampleCard(
            FluentIconRegular.Shapes24,
            "Corner radius scale",
            "Compact, control, card, overlay, and pill radii keep FluentJalium surfaces aligned with WinUI token roles.",
            CreateCornerRadiusScaleSample()));
        examples.Children.Add(CreateGeometryExampleCard(
            FluentIconRegular.BorderAll24,
            "Stroke and elevation borders",
            "Control strokes use one-pixel geometry while elevation borders add the subtle top-to-bottom Fluent edge.",
            CreateStrokeElevationSample()));
        examples.Children.Add(CreateGeometryExampleCard(
            FluentIconRegular.LayerDiagonal24,
            "Layered surface geometry",
            "Window, content, overlay, and command layers use distinct radius and shadow values for readable depth.",
            CreateLayeredGeometrySample()));

        panel.Children.Add(examples);
        return panel;
    }

    private static FWBorder CreateGeometryTokenStrip()
    {
        return new FWBorder
        {
            Background = ThemeBrush("LayerFillColorDefaultBrush"),
            BorderBrush = ThemeBrush("ControlElevationBorderBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = ResourceCornerRadius("CardCornerRadius", 6),
            Padding = new Thickness(12),
            Child = new FWWrapPanel
            {
                HorizontalSpacing = 8,
                VerticalSpacing = 8,
                Children =
                {
                    CreateGeometryTokenPill(FluentIconRegular.Ruler24, "Control radius", FormatResourceValue("ControlCornerRadius")),
                    CreateGeometryTokenPill(FluentIconRegular.BorderOutside24, "Overlay radius", FormatResourceValue("OverlayCornerRadius")),
                    CreateGeometryTokenPill(FluentIconRegular.AppGeneric24, "Card radius", FormatResourceValue("CardCornerRadius")),
                    CreateGeometryTokenPill(FluentIconRegular.BorderAll24, "Control stroke", FormatResourceValue("FluentControlBorderThickness")),
                    CreateGeometryTokenPill(FluentIconRegular.SquareShadow24, "Shadow depth", FormatPixelResourceValue("FluentElevationShadowDepthMedium")),
                    CreateGeometryTokenPill(FluentIconRegular.LayerDiagonal24, "Shadow blur", FormatPixelResourceValue("FluentElevationShadowBlurMedium"))
                }
            }
        };
    }

    private static UIElement CreateCornerRadiusScaleSample()
    {
        return new FWWrapPanel
        {
            HorizontalSpacing = 10,
            VerticalSpacing = 10,
            Children =
            {
                CreateRadiusTile(FluentIconRegular.RectangleLandscape24, "Compact", "FluentCompactCornerRadius", "Small focus and in-control surfaces", 126, 72),
                CreateRadiusTile(FluentIconRegular.ControlButton24, "Control", "ControlCornerRadius", "Buttons, text inputs, and dense commands", 126, 72),
                CreateRadiusTile(FluentIconRegular.AppGeneric24, "Card", "CardCornerRadius", "Gallery cards and content panels", 126, 72),
                CreateRadiusTile(FluentIconRegular.BorderOutside24, "Overlay", "OverlayCornerRadius", "Flyouts, menus, and transient layers", 126, 72),
                CreateRadiusTile(FluentIconRegular.TextBulletListSquare24, "Pill", "FluentPillCornerRadius", "Badges, chips, and compact metadata", 164, 42)
            }
        };
    }

    private static UIElement CreateStrokeElevationSample()
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Children =
            {
                new FWWrapPanel
                {
                    HorizontalSpacing = 10,
                    VerticalSpacing = 10,
                    Children =
                    {
                        CreateStrokeTile(FluentIconRegular.BorderAll24, "Control stroke", "FluentControlBorderThickness", ThemeBrush("ControlBorder")),
                        CreateStrokeTile(FluentIconRegular.BorderOutside24, "Overlay stroke", "FluentOverlayBorderThickness", ThemeBrush("ControlElevationBorderBrush")),
                        CreateStrokeTile(FluentIconRegular.SquareShadow24, "Elevation edge", "ControlElevationBorderBrush", ThemeBrush("ControlElevationBorderBrush"))
                    }
                },
                new FWWrapPanel
                {
                    HorizontalSpacing = 10,
                    VerticalSpacing = 10,
                    Children =
                    {
                        CreateElevationTile("Low", "FluentElevationShadowDepthLow", "FluentElevationShadowBlurLow"),
                        CreateElevationTile("Medium", "FluentElevationShadowDepthMedium", "FluentElevationShadowBlurMedium"),
                        CreateElevationTile("High", "FluentElevationShadowDepthHigh", "FluentElevationShadowBlurHigh")
                    }
                }
            }
        };
    }

    private static UIElement CreateLayeredGeometrySample()
    {
        return new FWBorder
        {
            Width = 490,
            Height = 250,
            Background = ThemeBrush("SurfaceBackground"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = ResourceCornerRadius("OverlayCornerRadius", 8),
            Padding = new Thickness(18),
            Child = new FWGrid
            {
                Children =
                {
                    new FWBorder
                    {
                        Background = ThemeBrush("AccentBrush"),
                        Opacity = 0.16,
                        CornerRadius = ResourceCornerRadius("OverlayCornerRadius", 8)
                    },
                    CreateLayeredGeometryCard()
                }
            }
        };
    }

    private static FWBorder CreateLayeredGeometryCard()
    {
        var card = new FWBorder
        {
            Width = 330,
            Height = 170,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Background = ThemeBrush("LayerFillColorDefaultBrush"),
            BorderBrush = ThemeBrush("ControlElevationBorderBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = ResourceCornerRadius("CardCornerRadius", 6),
            Padding = new Thickness(16),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 12,
                Children =
                {
                    CreateLayerRow(FluentIconRegular.WindowBrush24, "Window backdrop", "OverlayCornerRadius"),
                    CreateLayerRow(FluentIconRegular.AppGeneric24, "Content layer", "CardCornerRadius"),
                    CreateLayerRow(FluentIconRegular.ControlButton24, "Command surface", "ControlCornerRadius"),
                    CreateLayerRow(FluentIconRegular.TextBulletListSquare24, "Status pill", "FluentPillCornerRadius")
                }
            }
        };
        card.Effect = CreateShadow("FluentElevationShadowDepthMedium", "FluentElevationShadowBlurMedium", 0.2);
        return card;
    }

    private static FWStackPanel CreateLayerRow(FluentIconRegular icon, string title, string tokenKey)
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                CreateIcon(icon, 16, ThemeBrush("TextSecondary")),
                new FWTextBlock
                {
                    Text = title,
                    Width = 130,
                    FontSize = 12,
                    Foreground = ThemeBrush("TextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center
                },
                new FWBorder
                {
                    Width = 78,
                    Height = 22,
                    Background = ThemeBrush("ControlBackground"),
                    BorderBrush = ThemeBrush("ControlElevationBorderBrush"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = ResourceCornerRadius(tokenKey, 4),
                    Child = new FWTextBlock
                    {
                        Text = FormatResourceValue(tokenKey),
                        FontSize = 11,
                        Foreground = ThemeBrush("TextSecondary"),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                }
            }
        };
    }

    private static FWBorder CreateRadiusTile(FluentIconRegular icon, string title, string tokenKey, string description, double width, double height)
    {
        return new FWBorder
        {
            Width = 188,
            Background = ThemeBrush("ControlBackground"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = ResourceCornerRadius("CardCornerRadius", 6),
            Padding = new Thickness(12),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 8,
                Children =
                {
                    new FWBorder
                    {
                        Width = width,
                        Height = height,
                        Background = ThemeBrush("SelectionBackgroundWeak"),
                        BorderBrush = ThemeBrush("AccentBrush"),
                        BorderThickness = new Thickness(1),
                        CornerRadius = ResourceCornerRadius(tokenKey, 4),
                        Child = CreateIcon(icon, 22, ThemeBrush("TextPrimary"))
                    },
                    new FWTextBlock
                    {
                        Text = title,
                        FontSize = 14,
                        Foreground = ThemeBrush("TextPrimary")
                    },
                    new FWTextBlock
                    {
                        Text = FormatResourceValue(tokenKey),
                        FontSize = 12,
                        Foreground = ThemeBrush("TextSecondary")
                    },
                    new FWTextBlock
                    {
                        Text = description,
                        FontSize = 12,
                        Foreground = ThemeBrush("TextSecondary"),
                        TextWrapping = TextWrapping.Wrap
                    }
                }
            }
        };
    }

    private static FWBorder CreateStrokeTile(FluentIconRegular icon, string title, string tokenKey, Brush borderBrush)
    {
        return new FWBorder
        {
            Width = 150,
            Background = ThemeBrush("ControlBackground"),
            BorderBrush = borderBrush,
            BorderThickness = tokenKey.EndsWith("Brush", StringComparison.Ordinal)
                ? new Thickness(1)
                : ResourceThickness(tokenKey, 1),
            CornerRadius = ResourceCornerRadius("ControlCornerRadius", 4),
            Padding = new Thickness(12),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 8,
                Children =
                {
                    CreateIcon(icon, 20, ThemeBrush("TextPrimary")),
                    new FWTextBlock
                    {
                        Text = title,
                        FontSize = 13,
                        Foreground = ThemeBrush("TextPrimary")
                    },
                    new FWTextBlock
                    {
                        Text = FormatResourceValue(tokenKey),
                        FontSize = 12,
                        Foreground = ThemeBrush("TextSecondary"),
                        TextWrapping = TextWrapping.Wrap
                    }
                }
            }
        };
    }

    private static FWBorder CreateElevationTile(string title, string depthKey, string blurKey)
    {
        var tile = new FWBorder
        {
            Width = 150,
            Height = 92,
            Background = ThemeBrush("LayerFillColorDefaultBrush"),
            BorderBrush = ThemeBrush("ControlElevationBorderBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = ResourceCornerRadius("CardCornerRadius", 6),
            Padding = new Thickness(12),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 6,
                Children =
                {
                    CreateIcon(FluentIconRegular.SquareShadow24, 18, ThemeBrush("TextPrimary")),
                    new FWTextBlock
                    {
                        Text = title,
                        FontSize = 13,
                        Foreground = ThemeBrush("TextPrimary")
                    },
                    new FWTextBlock
                    {
                        Text = $"{FormatPixelResourceValue(depthKey)} / {FormatPixelResourceValue(blurKey)}",
                        FontSize = 12,
                        Foreground = ThemeBrush("TextSecondary")
                    }
                }
            }
        };
        tile.Effect = CreateShadow(depthKey, blurKey, 0.18);
        return tile;
    }

    private static FWBorder CreateGeometryTokenPill(FluentIconRegular icon, string title, string value)
    {
        return new FWBorder
        {
            Background = ThemeBrush("ControlBackground"),
            BorderBrush = ThemeBrush("ControlElevationBorderBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = ResourceCornerRadius("ControlCornerRadius", 4),
            Padding = new Thickness(10, 6, 10, 6),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children =
                {
                    CreateIcon(icon, 16, ThemeBrush("TextSecondary")),
                    new FWTextBlock
                    {
                        Text = title,
                        FontSize = 12,
                        Foreground = ThemeBrush("TextSecondary"),
                        VerticalAlignment = VerticalAlignment.Center
                    },
                    new FWTextBlock
                    {
                        Text = value,
                        FontSize = 12,
                        Foreground = ThemeBrush("TextPrimary"),
                        VerticalAlignment = VerticalAlignment.Center
                    }
                }
            }
        };
    }

    private static FWBorder CreateGeometryExampleCard(FluentIconRegular icon, string title, string description, UIElement content)
    {
        return GallerySampleCard.Create(icon, title, description, content, code: CreateSampleCode(title), width: 520);
    }

    private static string CreateSampleCode(string title)
    {
        return title switch
        {
            "Corner radius scale" => "CornerRadius=\"{ThemeResource ControlCornerRadius}\"\nCornerRadius=\"{ThemeResource OverlayCornerRadius}\"",
            "Stroke and elevation borders" => "BorderThickness=\"{ThemeResource FluentControlBorderThickness}\"\nBorderBrush=\"{ThemeResource ControlElevationBorderBrush}\"",
            _ => "CornerRadius=\"{ThemeResource CardCornerRadius}\"\nEffect=\"{ThemeResource FluentElevationShadowMedium}\""
        };
    }

    private static DropShadowEffect CreateShadow(string depthKey, string blurKey, double opacity)
    {
        return new DropShadowEffect
        {
            BlurRadius = ResourceDouble(blurKey, 18),
            ShadowDepth = ResourceDouble(depthKey, 4),
            Direction = 270,
            Opacity = opacity,
            Color = Color.FromArgb(255, 0, 0, 0)
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
                        CreateIcon(FluentIconRegular.Ruler24, 24, ThemeBrush("TextPrimary")),
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

    private static string FormatPixelResourceValue(string key)
    {
        var value = FormatResourceValue(key);
        return value == "-" ? value : $"{value} px";
    }

    private static string FormatResourceValue(string key)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) != true)
        {
            return "-";
        }

        return value switch
        {
            CornerRadius radius => FormatCornerRadius(radius),
            Thickness thickness => FormatThickness(thickness),
            string text => text,
            double number => number.ToString("0.##", CultureInfo.InvariantCulture),
            int number => number.ToString(CultureInfo.InvariantCulture),
            Brush _ => key,
            _ => value.ToString() ?? "-"
        };
    }

    private static string FormatCornerRadius(CornerRadius radius)
    {
        if (radius.TopLeft == radius.TopRight &&
            radius.TopLeft == radius.BottomRight &&
            radius.TopLeft == radius.BottomLeft)
        {
            return $"{radius.TopLeft:0.##} px";
        }

        return $"{radius.TopLeft:0.##}, {radius.TopRight:0.##}, {radius.BottomRight:0.##}, {radius.BottomLeft:0.##}";
    }

    private static string FormatThickness(Thickness thickness)
    {
        if (thickness.Left == thickness.Top &&
            thickness.Left == thickness.Right &&
            thickness.Left == thickness.Bottom)
        {
            return $"{thickness.Left:0.##} px";
        }

        return $"{thickness.Left:0.##}, {thickness.Top:0.##}, {thickness.Right:0.##}, {thickness.Bottom:0.##}";
    }

    private static CornerRadius ResourceCornerRadius(string key, double fallback)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) == true && value is CornerRadius radius)
        {
            return radius;
        }

        return new CornerRadius(fallback);
    }

    private static Thickness ResourceThickness(string key, double fallback)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Thickness thickness)
        {
            return thickness;
        }

        return new Thickness(fallback);
    }

    private static double ResourceDouble(string key, double fallback)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) != true)
        {
            return fallback;
        }

        return value switch
        {
            double number => number,
            int number => number,
            string text when double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var number) => number,
            _ => fallback
        };
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
