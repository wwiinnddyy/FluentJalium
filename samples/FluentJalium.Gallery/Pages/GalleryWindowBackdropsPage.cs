using FluentJalium.Gallery.Controls;
using FluentJalium.Icon;
using FluentJalium.Controls;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using Jalium.UI.Media.Effects;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWButton = FluentJalium.Controls.FWButton;
using FWFluentWindowBackdropKind = FluentJalium.Controls.FWFluentWindowBackdropKind;
using FWFluentWindowBackdropRecipe = FluentJalium.Controls.FWFluentWindowBackdropRecipe;
using FWFluentWindowMaterialProfile = FluentJalium.Controls.FWFluentWindowMaterialProfile;
using FWFluentWindowMaterialProfileRecipe = FluentJalium.Controls.FWFluentWindowMaterialProfileRecipe;
using FWFluentWindowSurface = FluentJalium.Controls.FWFluentWindowSurface;
using FWGrid = FluentJalium.Controls.FWGrid;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;

namespace FluentJalium.Gallery.Pages;

internal sealed record GalleryWindowSurfaceDiagnostics(
    FWFluentWindowMaterialProfile Profile,
    string Role,
    FWFluentWindowBackdropKind RequestedBackdropKind,
    WindowBackdropType RequestedSystemBackdrop,
    WindowBackdropType ActualSystemBackdrop,
    FWFluentMaterialRole SurfaceRole,
    FWFluentMaterialKind SurfaceMaterialKind,
    BorderShape SurfaceShape,
    bool AutoApplyWindowBackdrop,
    bool WasApplied)
{
    public bool IsMatched => ActualSystemBackdrop == RequestedSystemBackdrop;

    public string MatchState => IsMatched ? "matched" : "pending";

    public string ApplyState => WasApplied ? "applied" : "not applied";

    public string AutoApplyText => AutoApplyWindowBackdrop ? "On" : "Off";

    public static GalleryWindowSurfaceDiagnostics Create(
        FWFluentWindowSurface surface,
        WindowBackdropType actualSystemBackdrop,
        bool wasApplied)
    {
        ArgumentNullException.ThrowIfNull(surface);

        var recipe = FWFluentWindowMaterialProfileRecipe.Create(surface.WindowMaterialProfile);
        return new GalleryWindowSurfaceDiagnostics(
            recipe.Profile,
            recipe.Role,
            recipe.WindowBackdropKind,
            recipe.SystemBackdrop,
            actualSystemBackdrop,
            surface.MaterialRole,
            surface.MaterialKind,
            surface.Shape,
            surface.AutoApplyWindowBackdrop,
            wasApplied);
    }
}

internal sealed class GalleryWindowBackdropsPage
{
    private readonly Window _window;

    public GalleryWindowBackdropsPage(Window window)
    {
        _window = window;
    }

    public UIElement CreateContent()
    {
        var panel = CreateSection("Window Backdrops");
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        examples.Children.Add(CreateWindowBackdropExampleCard(
            FluentIconRegular.WindowBrush24,
            "SystemBackdrop selector",
            "Jalium windows expose WinUI-style Mica, Mica Alt, and Acrylic through the window SystemBackdrop property.",
            CreateSystemBackdropSelector()));
        examples.Children.Add(CreateWindowBackdropExampleCard(
            FluentIconRegular.LayerDiagonal24,
            "Backdrop recipe map",
            "FluentJalium maps each shell material to an explicit role, system backdrop, and fallback behavior.",
            CreateBackdropRecipeMap()));
        examples.Children.Add(CreateWindowBackdropExampleCard(
            FluentIconRegular.DataUsage24,
            "Window surface diagnostics",
            "FWFluentWindowSurface exposes the requested profile, surface role, and actual host Window.SystemBackdrop for shell QA.",
            CreateWindowSurfaceDiagnostics()));
        examples.Children.Add(CreateWindowBackdropExampleCard(
            FluentIconRegular.AppGeneric24,
            "Shell layering",
            "Window backdrops sit behind opaque or translucent content layers, while element effects remain inside the page.",
            CreateShellLayeringPreview()));

        panel.Children.Add(examples);
        return panel;
    }

    private UIElement CreateSystemBackdropSelector()
    {
        var status = CreateBackdropOutputText(CreateWindowBackdropStatusText(_window.SystemBackdrop));

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                CreateWindowBackdropPreview(
                    "Current shell",
                    "The requested DWM backdrop is applied to the Window, then content layers draw above it.",
                    ThemeBrush("LayerFillColorDefaultBrush")),
                CreateBackdropButtonRow(
                    CreateBackdropActionButton(FluentIconRegular.DismissCircle24, "None", () => ApplySystemBackdrop(FWFluentWindowBackdropKind.None, status)),
                    CreateBackdropActionButton(FluentIconRegular.WindowBrush24, "Mica", () => ApplySystemBackdrop(FWFluentWindowBackdropKind.Mica, status)),
                    CreateBackdropActionButton(FluentIconRegular.LayerDiagonal24, "Mica Alt", () => ApplySystemBackdrop(FWFluentWindowBackdropKind.MicaAlt, status)),
                    CreateBackdropActionButton(FluentIconRegular.TransparencySquare24, "Acrylic", () => ApplySystemBackdrop(FWFluentWindowBackdropKind.Acrylic, status))),
                CreateBackdropStatus(status)
            }
        };
    }

    private UIElement CreateWindowSurfaceDiagnostics()
    {
        var surface = new FWFluentWindowSurface
        {
            Width = 490,
            Height = 182,
            AutoApplyWindowBackdrop = false
        };
        surface.ApplyWindowMaterialProfile(FWFluentWindowMaterialProfile.MicaShell);
        surface.Child = CreateWindowSurfacePreviewContent(surface, _window.SystemBackdrop, wasApplied: false);

        var status = CreateBackdropOutputText(CreateWindowSurfaceDiagnosticsText(surface, _window.SystemBackdrop, wasApplied: false));

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                surface,
                CreateBackdropButtonRow(
                    CreateWindowProfileActionButton(FluentIconRegular.DismissCircle24, "Solid", FWFluentWindowMaterialProfile.Solid, surface, status),
                    CreateWindowProfileActionButton(FluentIconRegular.WindowBrush24, "Mica", FWFluentWindowMaterialProfile.MicaShell, surface, status),
                    CreateWindowProfileActionButton(FluentIconRegular.LayerDiagonal24, "Tabbed", FWFluentWindowMaterialProfile.TabbedMicaAlt, surface, status),
                    CreateWindowProfileActionButton(FluentIconRegular.TransparencySquare24, "Acrylic", FWFluentWindowMaterialProfile.TransientAcrylic, surface, status),
                    CreateWindowProfileActionButton(FluentIconRegular.Glasses24, "Focus", FWFluentWindowMaterialProfile.FocusGlassShell, surface, status)),
                CreateBackdropStatus(status)
            }
        };
    }

    private static UIElement CreateBackdropRecipeMap()
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                CreateRecipeTile(FluentIconRegular.DismissCircle24, FWFluentWindowBackdropKind.None),
                CreateRecipeTile(FluentIconRegular.WindowBrush24, FWFluentWindowBackdropKind.Mica),
                CreateRecipeTile(FluentIconRegular.LayerDiagonal24, FWFluentWindowBackdropKind.MicaAlt),
                CreateRecipeTile(FluentIconRegular.TransparencySquare24, FWFluentWindowBackdropKind.Acrylic)
            }
        };
    }

    private static UIElement CreateShellLayeringPreview()
    {
        var shell = new FWBorder
        {
            Width = 490,
            Height = 260,
            Background = ThemeBrush("FluentMaterialWindowBackdropBrush"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(18),
            Child = new FWGrid
            {
                Children =
                {
                    new FWBorder
                    {
                        Background = ThemeBrush("AccentBrush"),
                        Opacity = 0.18,
                        CornerRadius = new CornerRadius(8)
                    },
                    CreateLayeredShellContent()
                }
            }
        };
        shell.Effect = new DropShadowEffect
        {
            BlurRadius = 18,
            ShadowDepth = 4,
            Direction = 270,
            Opacity = 0.18,
            Color = Color.FromArgb(255, 0, 0, 0)
        };
        return shell;
    }

    private static FWBorder CreateWindowSurfacePreviewContent(
        FWFluentWindowSurface surface,
        WindowBackdropType actualSystemBackdrop,
        bool wasApplied)
    {
        var diagnostics = GalleryWindowSurfaceDiagnostics.Create(surface, actualSystemBackdrop, wasApplied);

        return new FWBorder
        {
            Margin = new Thickness(14),
            Background = ThemeBrush("FluentMaterialContentLayerBrush"),
            BorderBrush = ThemeBrush("FluentMaterialLayerBorderBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
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
                            CreateIcon(FluentIconRegular.WindowBrush24, 18, ThemeBrush("TextPrimary")),
                            new FWTextBlock
                            {
                                Text = "FWFluentWindowSurface",
                                FontSize = 15,
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
                            CreateSystemBackdropBadge($"Profile {diagnostics.Profile}"),
                            CreateSystemBackdropBadge($"Requested {diagnostics.RequestedSystemBackdrop}"),
                            CreateSystemBackdropBadge($"Actual {diagnostics.ActualSystemBackdrop}"),
                            CreateSystemBackdropBadge($"Surface {diagnostics.SurfaceRole}"),
                            CreateSystemBackdropBadge($"Material {diagnostics.SurfaceMaterialKind}"),
                            CreateSystemBackdropBadge(diagnostics.MatchState),
                            CreateSystemBackdropBadge(diagnostics.ApplyState)
                        }
                    },
                    new FWTextBlock
                    {
                        Text = FormatWindowSurfaceDiagnostics(diagnostics),
                        FontSize = 12,
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = ThemeBrush("TextSecondary")
                    }
                }
            }
        };
    }

    private static FWBorder CreateLayeredShellContent()
    {
        return new FWBorder
        {
            Width = 360,
            Height = 178,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Background = ThemeBrush("FluentMaterialContentLayerBrush"),
            BorderBrush = ThemeBrush("FluentMaterialLayerBorderBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(14),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 10,
                Children =
                {
                    CreateLayerRow(FluentIconRegular.WindowBrush24, "Window", "SystemBackdrop"),
                    CreateLayerRow(FluentIconRegular.PanelLeft24, "Navigation", "Layer fill"),
                    CreateLayerRow(FluentIconRegular.AppGeneric24, "Content", "Opaque surface"),
                    CreateLayerRow(FluentIconRegular.Glasses24, "Element", "Element effect")
                }
            }
        };
    }

    private void ApplySystemBackdrop(FWFluentWindowBackdropKind backdropKind, TextBlock status)
    {
        var recipe = FWFluentWindowBackdropRecipe.Create(backdropKind);
        recipe.ApplyTo(_window);
        status.Text = $"Current window backdrop: {recipe.SystemBackdrop} ({recipe.Role})";
    }

    private void ApplyWindowSurfaceProfile(
        FWFluentWindowMaterialProfile profile,
        FWFluentWindowSurface surface,
        TextBlock status)
    {
        surface.ApplyWindowMaterialProfile(profile);
        surface.ApplyWindowBackdrop(_window);
        surface.Child = CreateWindowSurfacePreviewContent(surface, _window.SystemBackdrop, wasApplied: true);
        status.Text = CreateWindowSurfaceDiagnosticsText(surface, _window.SystemBackdrop, wasApplied: true);
    }

    private static string CreateWindowBackdropStatusText(WindowBackdropType systemBackdrop)
    {
        var kind = systemBackdrop switch
        {
            WindowBackdropType.Mica => FWFluentWindowBackdropKind.Mica,
            WindowBackdropType.MicaAlt => FWFluentWindowBackdropKind.MicaAlt,
            WindowBackdropType.Acrylic => FWFluentWindowBackdropKind.Acrylic,
            _ => FWFluentWindowBackdropKind.None
        };
        var recipe = FWFluentWindowBackdropRecipe.Create(kind);
        return $"Current window backdrop: {recipe.SystemBackdrop} ({recipe.Role})";
    }

    internal static string CreateWindowSurfaceDiagnosticsText(
        FWFluentWindowSurface surface,
        WindowBackdropType actualSystemBackdrop,
        bool wasApplied)
    {
        return FormatWindowSurfaceDiagnostics(GalleryWindowSurfaceDiagnostics.Create(surface, actualSystemBackdrop, wasApplied));
    }

    internal static string FormatWindowSurfaceDiagnostics(GalleryWindowSurfaceDiagnostics diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        return $"Profile: {diagnostics.Role}; requested: {diagnostics.RequestedSystemBackdrop}/{diagnostics.RequestedBackdropKind}; actual: {diagnostics.ActualSystemBackdrop}; surface: {diagnostics.SurfaceRole}/{diagnostics.SurfaceMaterialKind}/{diagnostics.SurfaceShape}; auto apply: {diagnostics.AutoApplyText}; window: {diagnostics.MatchState}, {diagnostics.ApplyState}.";
    }

    private static FWBorder CreateRecipeTile(FluentIconRegular icon, FWFluentWindowBackdropKind backdropKind)
    {
        var recipe = FWFluentWindowBackdropRecipe.Create(backdropKind);
        var iconFrame = new FWBorder
        {
            Width = 32,
            Height = 32,
            Background = ThemeBrush("FluentMaterialRoleIconBackgroundBrush"),
            BorderBrush = ThemeBrush("FluentMaterialLayerBorderBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Child = CreateIcon(icon, 18, ThemeBrush("TextPrimary"))
        };

        var content = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 4,
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
                            Text = recipe.Role,
                            FontSize = 13,
                            Foreground = ThemeBrush("TextPrimary"),
                            VerticalAlignment = VerticalAlignment.Center
                        },
                        CreateSystemBackdropBadge(recipe.SystemBackdrop.ToString())
                    }
                },
                new FWTextBlock
                {
                    Text = recipe.Description,
                    FontSize = 12,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = ThemeBrush("TextSecondary")
                }
            }
        };

        Grid.SetColumn(content, 1);

        return new FWBorder
        {
            Background = ThemeBrush("LayerFillColorDefaultBrush"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12),
            Child = new FWGrid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(40) },
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

    private static FWBorder CreateSystemBackdropBadge(string text)
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

    private static FWStackPanel CreateLayerRow(FluentIconRegular icon, string title, string role)
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
                    Width = 104,
                    FontSize = 12,
                    Foreground = ThemeBrush("TextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center
                },
                CreateSystemBackdropBadge(role)
            }
        };
    }

    private static FWBorder CreateWindowBackdropPreview(string title, string description, Brush layerBrush)
    {
        return new FWBorder
        {
            Width = 490,
            Height = 150,
            Background = ThemeBrush("FluentMaterialWindowBackdropBrush"),
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
            Child = new FWStackPanel
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
            }
        };
    }

    private static FWBorder CreateWindowBackdropExampleCard(FluentIconRegular icon, string title, string description, UIElement content)
    {
        return GallerySampleCard.Create(icon, title, description, content, code: CreateSampleCode(title), width: 520);
    }

    private static string CreateSampleCode(string title)
    {
        return title switch
        {
            "SystemBackdrop selector" => "var recipe = FWFluentWindowBackdropRecipe.Create(FWFluentWindowBackdropKind.Mica);\nrecipe.ApplyTo(window);",
            "Backdrop recipe map" => "FWFluentWindowBackdropRecipe.Create(FWFluentWindowBackdropKind.MicaAlt);\nFWFluentWindowBackdropRecipe.Create(FWFluentWindowBackdropKind.Acrylic);",
            "Window surface diagnostics" => "var surface = new FWFluentWindowSurface { AutoApplyWindowBackdrop = false };\nsurface.ApplyWindowMaterialProfile(FWFluentWindowMaterialProfile.FocusGlassShell);\nsurface.ApplyWindowBackdrop(window);\nvar recipe = FWFluentWindowMaterialProfileRecipe.Create(surface.WindowMaterialProfile);",
            "Shell layering" => "<Window SystemBackdrop=\"Mica\">\n    <FWNavigationView PaneDisplayMode=\"Left\" />\n    <FWBorder Background=\"{ThemeResource FluentMaterialContentLayerBrush}\" />\n</Window>",
            _ => "<Window SystemBackdrop=\"Mica\" />"
        };
    }

    private static FWStackPanel CreateBackdropButtonRow(params FWButton[] buttons)
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

    private static FWButton CreateBackdropActionButton(FluentIconRegular icon, string text, Action action)
    {
        var button = new FWButton
        {
            Content = CreateBackdropButtonContent(icon, text)
        };
        button.Click += (_, _) => action();
        return button;
    }

    private FWButton CreateWindowProfileActionButton(
        FluentIconRegular icon,
        string text,
        FWFluentWindowMaterialProfile profile,
        FWFluentWindowSurface surface,
        TextBlock status)
    {
        return CreateBackdropActionButton(icon, text, () => ApplyWindowSurfaceProfile(profile, surface, status));
    }

    private static FWStackPanel CreateBackdropButtonContent(FluentIconRegular icon, string text)
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

    private static TextBlock CreateBackdropOutputText(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        };
    }

    private static FWBorder CreateBackdropStatus(TextBlock status)
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
                        CreateIcon(FluentIconRegular.WindowBrush24, 24, ThemeBrush("TextPrimary")),
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

    private static string FormatOnOff(bool value)
    {
        return value ? "On" : "Off";
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
