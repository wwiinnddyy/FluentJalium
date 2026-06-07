using System.Globalization;
using FluentJalium.Gallery.Controls;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Media;
using Jalium.UI.Media.Effects;
using FWAcrylicSurface = FluentJalium.Controls.FWAcrylicSurface;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWButton = FluentJalium.Controls.FWButton;
using FWCardSurface = FluentJalium.Controls.FWCardSurface;
using FWFlyoutSurface = FluentJalium.Controls.FWFlyoutSurface;
using FWFluentWindowBackdropKind = FluentJalium.Controls.FWFluentWindowBackdropKind;
using FWFluentWindowMaterialProfile = FluentJalium.Controls.FWFluentWindowMaterialProfile;
using FWFluentWindowSurface = FluentJalium.Controls.FWFluentWindowSurface;
using FWFluentMaterialKind = FluentJalium.Controls.FWFluentMaterialKind;
using FWFluentMaterialRecipe = FluentJalium.Controls.FWFluentMaterialRecipe;
using FWFluentMaterialRole = FluentJalium.Controls.FWFluentMaterialRole;
using FWFluentMaterialSurface = FluentJalium.Controls.FWFluentMaterialSurface;
using FWFocusGlassSurface = FluentJalium.Controls.FWFocusGlassSurface;
using FWFrostedGlassSurface = FluentJalium.Controls.FWFrostedGlassSurface;
using FWGrid = FluentJalium.Controls.FWGrid;
using FWLayerSurface = FluentJalium.Controls.FWLayerSurface;
using FWMicaAltSurface = FluentJalium.Controls.FWMicaAltSurface;
using FWMicaSurface = FluentJalium.Controls.FWMicaSurface;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWToggleSwitch = FluentJalium.Controls.FWToggleSwitch;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;

namespace FluentJalium.Gallery.Pages;

internal sealed class GalleryMaterialsPage
{
    public UIElement CreateContent()
    {
        var panel = CreateSection("Element Materials and Effects");
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        examples.Children.Add(CreateMaterialsExampleCard(
            FluentIconRegular.TransparencySquare24,
            "Element BackdropEffect",
            "Controls can blur, tint, and refract content behind the element with BackdropEffect and FWFluentMaterialSurface.",
            CreateElementBackdropEffectsSample()));
        examples.Children.Add(CreateMaterialsExampleCard(
            FluentIconRegular.LayerDiagonalSparkle24,
            "Layered control surfaces",
            "Controls should sit on layered materials that echo WinUI: backdrop, layer fill, subtle border, and soft elevation.",
            CreateLayeredMaterialControlsSample()));
        examples.Children.Add(CreateMaterialsExampleCard(
            FluentIconRegular.AppGeneric24,
            "Derived FW surfaces",
            "FWLayerSurface, Mica, Acrylic, card, flyout, focus glass, and window surfaces expose ready-to-use Fluent material roles.",
            CreateDerivedSurfaceControlsSample()));
        examples.Children.Add(CreateMaterialsExampleCard(
            FluentIconRegular.AppGeneric24,
            "Fluent material roles",
            "Use window backdrops for the shell, layer fills for content, acrylic for transient UI, and LiquidGlass for focused Jalium element effects.",
            CreateMaterialRoleMapSample()));

        panel.Children.Add(CreateMaterialTokenStrip());
        panel.Children.Add(examples);
        return panel;
    }

    private static FWBorder CreateMaterialTokenStrip()
    {
        return new FWBorder
        {
            Background = ThemeBrush("LayerFillColorDefaultBrush"),
            BorderBrush = ThemeBrush("ControlElevationBorderBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12),
            Child = new FWWrapPanel
            {
                HorizontalSpacing = 8,
                VerticalSpacing = 8,
                Children =
                {
                    CreateMaterialTokenPill(FluentIconRegular.WindowBrush24, "Mica", "FluentMaterialMicaTintOpacity", FormatPercentResourceValue("FluentMaterialMicaTintOpacity")),
                    CreateMaterialTokenPill(FluentIconRegular.LayerDiagonal24, "Mica Alt", "FluentMaterialMicaAltBlurRadius", FormatPixelResourceValue("FluentMaterialMicaAltBlurRadius")),
                    CreateMaterialTokenPill(FluentIconRegular.Drop24, "Acrylic", "FluentMaterialAcrylicNoiseIntensity", FormatDecimalResourceValue("FluentMaterialAcrylicNoiseIntensity")),
                    CreateMaterialTokenPill(FluentIconRegular.WeatherSnowflake24, "Frosted", "FluentMaterialFrostedGlassBlurRadius", FormatPixelResourceValue("FluentMaterialFrostedGlassBlurRadius")),
                    CreateMaterialTokenPill(FluentIconRegular.Glasses24, "LiquidGlass", "FluentMaterialLiquidGlassRefractionAmount", FormatPixelResourceValue("FluentMaterialLiquidGlassRefractionAmount"))
                }
            }
        };
    }

    private static UIElement CreateElementBackdropEffectsSample()
    {
        return new FWWrapPanel
        {
            HorizontalSpacing = 10,
            VerticalSpacing = 10,
            Children =
            {
                CreateBackdropEffectTile(FluentIconRegular.CircleMultipleConcentric24, "BlurEffect", "Backdrop blur", new Jalium.UI.Media.BlurEffect(18f)),
                CreateMaterialSurfaceTile(FluentIconRegular.Drop24, "FW Acrylic", "Tint, blur, and noise preset", FWFluentMaterialKind.Acrylic),
                CreateMaterialSurfaceTile(FluentIconRegular.WindowBrush24, "FW Mica", "Wallpaper-tinted material preset", FWFluentMaterialKind.Mica),
                CreateMaterialSurfaceTile(FluentIconRegular.WeatherSnowflake24, "FW FrostedGlass", "High diffusion glass preset", FWFluentMaterialKind.FrostedGlass),
                CreateMaterialSurfaceTile(FluentIconRegular.Glasses24, "FW LiquidGlass", "HLSL refraction and highlights", FWFluentMaterialKind.LiquidGlass),
                CreateBackdropEffectTile(FluentIconRegular.Color24, "Grayscale", "Backdrop color adjustment", ColorAdjustmentEffect.CreateGrayscale(1.0f)),
                CreateBackdropEffectTile(FluentIconRegular.PaintBrushSparkle24, "HueRotate", "Backdrop hue shift", ColorAdjustmentEffect.CreateHueRotate(72f))
            }
        };
    }

    private static UIElement CreateLayeredMaterialControlsSample()
    {
        var elevated = CreateLayeredSurface(
            FluentIconRegular.SquareShadow24,
            "Elevated command layer",
            "Soft shadow plus layer fill keeps commands readable on Mica.",
            ThemeBrush("LayerFillColorDefaultBrush"));
        elevated.Effect = new DropShadowEffect
        {
            BlurRadius = 18,
            ShadowDepth = 4,
            Direction = 270,
            Opacity = 0.22,
            Color = Color.FromArgb(255, 0, 0, 0)
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Children =
            {
                elevated,
                new FWStackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 8,
                    Children =
                    {
                        CreateMaterialCommandButton(FluentIconRegular.CheckmarkCircle24, "Primary action"),
                        CreateMaterialCommandButton(FluentIconRegular.Layer24, "Secondary"),
                        new FWToggleSwitch { Header = "Backdrop-aware", IsOn = true }
                    }
                }
            }
        };
    }

    private static UIElement CreateDerivedSurfaceControlsSample()
    {
        var windowSurface = new FWFluentWindowSurface
        {
            AutoApplyWindowBackdrop = false,
            WindowMaterialProfile = FWFluentWindowMaterialProfile.MicaShell,
            WindowBackdropKind = FWFluentWindowBackdropKind.Mica
        };
        windowSurface.ApplyWindowMaterialProfile(FWFluentWindowMaterialProfile.MicaShell);

        return new FWWrapPanel
        {
            HorizontalSpacing = 10,
            VerticalSpacing = 10,
            Children =
            {
                CreateDerivedSurfaceTile(new FWLayerSurface(), FluentIconRegular.Layer24, "FWLayerSurface", "Content layer for stable page regions."),
                CreateDerivedSurfaceTile(new FWMicaSurface(), FluentIconRegular.WindowBrush24, "FWMicaSurface", "Long-lived shell and background regions."),
                CreateDerivedSurfaceTile(new FWMicaAltSurface(), FluentIconRegular.LayerDiagonal24, "FWMicaAltSurface", "Tabbed or pane-heavy app chrome."),
                CreateDerivedSurfaceTile(new FWAcrylicSurface(), FluentIconRegular.Drop24, "FWAcrylicSurface", "Transient elevated surfaces and overlays."),
                CreateDerivedSurfaceTile(new FWFrostedGlassSurface(), FluentIconRegular.WeatherSnowflake24, "FWFrostedGlassSurface", "Soft media and preview glass."),
                CreateDerivedSurfaceTile(new FWCardSurface(), FluentIconRegular.CardUi24, "FWCardSurface", "Repeated settings and content groups."),
                CreateDerivedSurfaceTile(new FWFlyoutSurface(), FluentIconRegular.AppFolder24, "FWFlyoutSurface", "Menus, popups, and command overflow."),
                CreateDerivedSurfaceTile(new FWFocusGlassSurface(), FluentIconRegular.Glasses24, "FWFocusGlassSurface", "High-emphasis LiquidGlass previews."),
                CreateDerivedSurfaceTile(windowSurface, FluentIconRegular.AppGeneric24, "FWFluentWindowSurface", "Root surface paired with a window backdrop.")
            }
        };
    }

    private static UIElement CreateMaterialRoleMapSample()
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                CreateMaterialRoleTile(
                    FluentIconRegular.WindowBrush24,
                    "Window shell",
                    "SystemBackdrop",
                    "Mica, Mica Alt, and Acrylic are shown on the Window Backdrops page so shell materials stay separate.",
                    ThemeBrush("FluentMaterialWindowBackdropBrush")),
                CreateMaterialRoleTile(
                    FluentIconRegular.LayerDiagonal24,
                    "Content layer",
                    "Layer fill",
                    "Default pages and cards use opaque layer brushes, borders, and compact elevation.",
                    ThemeBrush("FluentMaterialContentLayerBrush")),
                CreateMaterialRoleTile(
                    FluentIconRegular.Drop24,
                    "Transient surface",
                    "Acrylic",
                    "Flyouts, menus, and teaching UI can use acrylic blur when content behind remains readable.",
                    ThemeBrush("FluentMaterialTransientAcrylicBrush")),
                CreateMaterialRoleTile(
                    FluentIconRegular.Glasses24,
                    "Focused element",
                    "LiquidGlass",
                    "Jalium HLSL surfaces are reserved for expressive controls that need refraction and highlight.",
                    ThemeBrush("FluentMaterialFocusedGlassBrush"))
            }
        };
    }

    private static FWBorder CreateBackdropEffectTile(FluentIconRegular icon, string title, string description, IBackdropEffect effect)
    {
        var preview = CreateLayeredSurface(icon, title, description, new SolidColorBrush(Color.FromArgb(110, 255, 255, 255)));
        preview.BackdropEffect = effect;
        return preview;
    }

    private static FWBorder CreateMaterialRoleTile(FluentIconRegular icon, string title, string role, string description, Brush background)
    {
        var iconFrame = new FWBorder
        {
            Width = 30,
            Height = 30,
            Background = ThemeBrush("FluentMaterialRoleIconBackgroundBrush"),
            BorderBrush = ThemeBrush("FluentMaterialLayerBorderBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Child = CreateIcon(icon, 18, ThemeBrush("TextPrimary"))
        };

        var content = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 3,
            Children =
            {
                new FWStackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 8,
                    Children =
                    {
                        new FWTextBlock
                        {
                            Text = title,
                            FontSize = 13,
                            Foreground = ThemeBrush("TextPrimary"),
                            VerticalAlignment = VerticalAlignment.Center
                        },
                        new FWBorder
                        {
                            Background = ThemeBrush("FluentMaterialRoleBadgeBrush"),
                            CornerRadius = new CornerRadius(4),
                            Padding = new Thickness(6, 2, 6, 2),
                            Child = new FWTextBlock
                            {
                                Text = role,
                                FontSize = 11,
                                Foreground = ThemeBrush("TextPrimary")
                            }
                        }
                    }
                },
                new FWTextBlock
                {
                    Text = description,
                    FontSize = 12,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = ThemeBrush("TextSecondary")
                }
            }
        };

        Grid.SetColumn(content, 1);

        return new FWBorder
        {
            Background = background,
            BorderBrush = ThemeBrush("FluentMaterialLayerBorderBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(12),
            Child = new FWGrid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(36) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                },
                Children =
                {
                    iconFrame,
                    content
                }
            }
        };
    }

    private static FWFluentMaterialSurface CreateMaterialSurfaceTile(FluentIconRegular icon, string title, string description, FWFluentMaterialKind materialKind)
    {
        var recipe = FWFluentMaterialRecipe.Create(materialKind);
        var isLiquidGlass = materialKind == FWFluentMaterialKind.LiquidGlass;
        var surface = new FWFluentMaterialSurface
        {
            Width = 236,
            Height = 130,
            Background = new SolidColorBrush(isLiquidGlass
                ? WithAlpha(recipe.TintColor, 44)
                : WithAlpha(recipe.TintColor, 110)),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Shape = isLiquidGlass ? BorderShape.SuperEllipse : BorderShape.RoundedRectangle,
            SuperEllipseN = 4,
            Padding = new Thickness(14),
            Child = CreateLayeredSurfaceContent(icon, title, description)
        };

        surface.UseMaterialRecipe(recipe);
        return surface;
    }

    private static FWFluentMaterialSurface CreateDerivedSurfaceTile(
        FWFluentMaterialSurface surface,
        FluentIconRegular icon,
        string title,
        string description)
    {
        surface.Width = 236;
        surface.Height = 136;
        surface.Background = CreateDerivedSurfaceBrush(surface);
        surface.BorderBrush = ThemeBrush("ControlBorder");
        surface.BorderThickness = new Thickness(1);
        surface.CornerRadius = new CornerRadius(8);
        surface.Padding = new Thickness(12);
        surface.Child = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Children =
            {
                new FWStackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 8,
                    Children =
                    {
                        CreateIcon(icon, 18, ThemeBrush("TextPrimary")),
                        new FWTextBlock
                        {
                            Text = title,
                            FontSize = 13,
                            Foreground = ThemeBrush("TextPrimary"),
                            VerticalAlignment = VerticalAlignment.Center
                        }
                    }
                },
                new FWWrapPanel
                {
                    HorizontalSpacing = 6,
                    VerticalSpacing = 6,
                    Children =
                    {
                        CreateSurfaceBadge(surface.MaterialRole == FWFluentMaterialRole.None ? "Recipe" : surface.MaterialRole.ToString()),
                        CreateSurfaceBadge(surface.MaterialKind.ToString())
                    }
                },
                new FWTextBlock
                {
                    Text = description,
                    FontSize = 12,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = ThemeBrush("TextSecondary")
                }
            }
        };

        return surface;
    }

    private static FWBorder CreateSurfaceBadge(string text)
    {
        return new FWBorder
        {
            Background = ThemeBrush("FluentMaterialRoleBadgeBrush"),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(6, 2, 6, 2),
            Child = new FWTextBlock
            {
                Text = text,
                FontSize = 11,
                Foreground = ThemeBrush("TextPrimary")
            }
        };
    }

    private static Brush CreateDerivedSurfaceBrush(FWFluentMaterialSurface surface)
    {
        return surface switch
        {
            FWFluentWindowSurface => ThemeBrush("FluentMaterialWindowBackdropBrush"),
            FWCardSurface => ThemeBrush("FluentMaterialCardBrush"),
            FWFlyoutSurface => ThemeBrush("AcrylicInAppFillColorDefaultBrush"),
            FWFocusGlassSurface => ThemeBrush("FluentMaterialFocusedGlassBrush"),
            FWAcrylicSurface => ThemeBrush("AcrylicInAppFillColorDefaultBrush"),
            FWFrostedGlassSurface => ThemeBrush("FluentMaterialFocusedGlassBrush"),
            FWMicaAltSurface => ThemeBrush("LayerOnMicaBaseAltFillColorDefaultBrush"),
            FWMicaSurface => ThemeBrush("MicaBackgroundFillColorBaseBrush"),
            _ => ThemeBrush("LayerFillColorDefaultBrush")
        };
    }

    private static FWBorder CreateMaterialTokenPill(FluentIconRegular icon, string title, string tokenKey, string value)
    {
        return new FWBorder
        {
            Background = ThemeBrush("ControlBackground"),
            BorderBrush = ThemeBrush("ControlElevationBorderBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
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
                    },
                    new FWTextBlock
                    {
                        Text = tokenKey,
                        FontSize = 11,
                        Foreground = ThemeBrush("TextSecondary"),
                        VerticalAlignment = VerticalAlignment.Center
                    }
                }
            }
        };
    }

    private static FWBorder CreateLayeredSurface(FluentIconRegular icon, string title, string description, Brush background)
    {
        return new FWBorder
        {
            Width = 236,
            Height = 130,
            Background = background,
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(14),
            Child = CreateLayeredSurfaceContent(icon, title, description)
        };
    }

    private static FWStackPanel CreateLayeredSurfaceContent(FluentIconRegular icon, string title, string description)
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Children =
            {
                new FWStackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 8,
                    Children =
                    {
                        CreateIcon(icon, 18, ThemeBrush("TextPrimary")),
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
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = ThemeBrush("TextSecondary")
                }
            }
        };
    }

    private static FWBorder CreateMaterialsExampleCard(FluentIconRegular icon, string title, string description, UIElement content)
    {
        return GallerySampleCard.Create(icon, title, description, content, code: CreateSampleCode(title), width: 520);
    }

    private static string CreateSampleCode(string title)
    {
        return title switch
        {
            "Element BackdropEffect" => "<FWBorder BackdropEffect=\"{BlurEffect Radius=18}\" />\n<FWFluentMaterialSurface MaterialKind=\"Acrylic\" />",
            "Layered control surfaces" => "<FWBorder Background=\"{ThemeResource LayerFillColorDefaultBrush}\">\n    <FWButton Content=\"Primary action\" />\n</FWBorder>",
            "Derived FW surfaces" => "new FWCardSurface { Child = new FWTextBlock { Text = \"Card content\" } };\nnew FWFlyoutSurface { Child = new FWButton { Content = \"Action\" } };\nnew FWFluentWindowSurface { AutoApplyWindowBackdrop = false, WindowMaterialProfile = FWFluentWindowMaterialProfile.MicaShell };",
            "Fluent material roles" => "<Window SystemBackdrop=\"Mica\">\n    <FWNavigationView />\n    <FWFluentMaterialSurface MaterialKind=\"LiquidGlass\" />\n</Window>",
            _ => "<FWFluentMaterialSurface MaterialKind=\"Mica\" />"
        };
    }

    private static FWButton CreateMaterialCommandButton(FluentIconRegular icon, string text)
    {
        return new FWButton
        {
            Content = CreateMaterialButtonContent(icon, text)
        };
    }

    private static FWStackPanel CreateMaterialButtonContent(FluentIconRegular icon, string text)
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
                        CreateIcon(FluentIconRegular.TransparencySquare24, 24, ThemeBrush("TextPrimary")),
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

    private static string FormatPercentResourceValue(string key)
    {
        var value = ResourceDouble(key, double.NaN);
        return double.IsNaN(value) ? "-" : value.ToString("P0", CultureInfo.InvariantCulture);
    }

    private static string FormatPixelResourceValue(string key)
    {
        var value = ResourceDouble(key, double.NaN);
        return double.IsNaN(value) ? "-" : $"{value:0.##} px";
    }

    private static string FormatDecimalResourceValue(string key)
    {
        var value = ResourceDouble(key, double.NaN);
        return double.IsNaN(value) ? "-" : value.ToString("0.###", CultureInfo.InvariantCulture);
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

    private static Color WithAlpha(Color color, byte alpha)
    {
        return Color.FromArgb(alpha, color.R, color.G, color.B);
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
