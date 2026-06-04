using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Media;
using Jalium.UI.Media.Effects;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWButton = FluentJalium.Controls.FWButton;
using FWFluentMaterialKind = FluentJalium.Controls.FWFluentMaterialKind;
using FWFluentMaterialSurface = FluentJalium.Controls.FWFluentMaterialSurface;
using FWGrid = FluentJalium.Controls.FWGrid;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWToggleSwitch = FluentJalium.Controls.FWToggleSwitch;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;

namespace FluentJalium.Gallery.Pages;

internal sealed class GalleryMaterialsPage
{
    private readonly Window _window;

    public GalleryMaterialsPage(Window window)
    {
        _window = window;
    }

    public UIElement CreateContent()
    {
        var panel = CreateSection("Materials and Effects");
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        examples.Children.Add(CreateMaterialsExampleCard(
            FluentIconRegular.WindowBrush24,
            "Window SystemBackdrop",
            "Jalium windows expose WinUI-style Mica, Mica Alt, and Acrylic backdrops for app shell depth.",
            CreateWindowBackdropMaterialSample()));
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
            "Fluent material roles",
            "Use window backdrops for the shell, layer fills for content, acrylic for transient UI, and LiquidGlass for focused Jalium element effects.",
            CreateMaterialRoleMapSample()));

        panel.Children.Add(examples);
        return panel;
    }

    private UIElement CreateWindowBackdropMaterialSample()
    {
        var status = CreateMaterialOutputText($"Current window backdrop: {_window.SystemBackdrop}");

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                CreateBackdropPreview("System backdrop", "Mica / MicaAlt / Acrylic are window-level materials, not just panel colors.", ThemeBrush("LayerFillColorDefaultBrush")),
                CreateMaterialButtonRow(
                    CreateMaterialActionButton(FluentIconRegular.DismissCircle24, "None", () => ApplySystemBackdrop(WindowBackdropType.None, status)),
                    CreateMaterialActionButton(FluentIconRegular.WindowBrush24, "Mica", () => ApplySystemBackdrop(WindowBackdropType.Mica, status)),
                    CreateMaterialActionButton(FluentIconRegular.LayerDiagonal24, "Mica Alt", () => ApplySystemBackdrop(WindowBackdropType.MicaAlt, status)),
                    CreateMaterialActionButton(FluentIconRegular.TransparencySquare24, "Acrylic", () => ApplySystemBackdrop(WindowBackdropType.Acrylic, status))),
                CreateMaterialStatus(status)
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
                    "Mica, Mica Alt, and Acrylic belong on the Window so the app frame carries depth.",
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

    private void ApplySystemBackdrop(WindowBackdropType backdropType, TextBlock status)
    {
        _window.SystemBackdrop = backdropType;
        status.Text = $"Current window backdrop: {backdropType}";
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
                ? Color.FromArgb(44, 0, 120, 212)
                : Color.FromArgb(110, 255, 255, 255)),
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

    private static FWBorder CreateBackdropPreview(string title, string description, Brush layerBrush)
    {
        return new FWBorder
        {
            Width = 490,
            Height = 150,
            Background = ThemeBrush("SurfaceBackground"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(14),
            Child = new FWGrid
            {
                Children =
                {
                    new FWBorder
                    {
                        Background = ThemeBrush("AccentBrush"),
                        CornerRadius = new CornerRadius(8),
                        Opacity = 0.24
                    },
                    CreateLayeredSurface(FluentIconRegular.WindowBrush24, title, description, layerBrush)
                }
            }
        };
    }

    private static FWBorder CreateMaterialsExampleCard(FluentIconRegular icon, string title, string description, UIElement content)
    {
        return new FWBorder
        {
            Width = 520,
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

    private static FWStackPanel CreateMaterialButtonRow(params FWButton[] buttons)
    {
        var row = new FWStackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8
        };

        foreach (var button in buttons)
        {
            row.Children.Add(button);
        }

        return row;
    }

    private static FWButton CreateMaterialActionButton(FluentIconRegular icon, string text, Action action)
    {
        var button = new FWButton
        {
            Content = CreateMaterialButtonContent(icon, text)
        };
        button.Click += (_, _) => action();
        return button;
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

    private static TextBlock CreateMaterialOutputText(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        };
    }

    private static FWBorder CreateMaterialStatus(TextBlock status)
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

    private static Brush ThemeBrush(string key)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Brush brush)
        {
            return brush;
        }

        return new SolidColorBrush(Colors.Transparent);
    }
}
