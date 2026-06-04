using FluentJalium.Controls.Themes;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;

namespace FluentJalium.Gallery.Pages;

internal sealed class GalleryThemeArchitecturePage
{
    public UIElement CreateContent()
    {
        var panel = CreateSection("Theme Architecture");
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        panel.Children.Add(CreateArchitectureTokenStrip());
        examples.Children.Add(CreateArchitectureCard(
            FluentIconRegular.Library24,
            "Theme resources",
            "FluentResources.jalxaml groups design tokens so colors, typography, geometry, material, and motion can evolve together.",
            CreateResourceLayerSample()));
        examples.Children.Add(CreateArchitectureCard(
            FluentIconRegular.PuzzlePiece24,
            "Control dictionaries",
            "FluentControls.jalxaml groups Jalium control styles by family while Generic.jalxaml stays as the stable public entry point.",
            CreateControlLayerSample()));
        examples.Children.Add(CreateArchitectureCard(
            FluentIconRegular.BranchFork24,
            "Theme and FW surfaces",
            "Theme mode updates Jalium base controls, while FW-prefixed controls provide independent FluentJalium control surfaces.",
            CreateDualSurfaceSample()));

        panel.Children.Add(examples);
        return panel;
    }

    private static FWBorder CreateArchitectureTokenStrip()
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
                    CreateArchitectureTokenPill(FluentIconRegular.AppFolder24, "Generic", FluentThemeManager.GenericThemeResourceName),
                    CreateArchitectureTokenPill(FluentIconRegular.Library24, "Resources", FluentThemeManager.FluentResourcesResourceName),
                    CreateArchitectureTokenPill(FluentIconRegular.PuzzlePiece24, "Controls", FluentThemeManager.FluentControlsResourceName)
                }
            }
        };
    }

    private static UIElement CreateResourceLayerSample()
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                CreateLayerRow(FluentIconRegular.Color24, "Colors", "FluentColors.jalxaml", "Accent, text, fill, semantic, and state brushes."),
                CreateLayerRow(FluentIconRegular.TransparencySquare24, "Materials", "FluentMaterials.jalxaml", "Window, element, acrylic, Mica, and LiquidGlass tokens."),
                CreateLayerRow(FluentIconRegular.Ruler24, "Geometry", "FluentGeometry.jalxaml", "Corner, border, elevation, and shadow roles."),
                CreateLayerRow(FluentIconRegular.SlideTransition24, "Motion", "FluentMotion.jalxaml", "Duration and connected animation resources."),
                CreateLayerRow(FluentIconRegular.TextFont24, "Typography", "FluentTypography.jalxaml", "Display, body, mono, icon, and type ramp resources.")
            }
        };
    }

    private static UIElement CreateControlLayerSample()
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                CreateLayerRow(FluentIconRegular.ControlButton24, "Controls/Button", "Button.jalxaml", "Button, split, app bar, command bar, and toolbar styles."),
                CreateLayerRow(FluentIconRegular.Textbox24, "Controls/Text", "TextControls.jalxaml", "TextBox, PasswordBox, NumberBox, AutoCompleteBox, and RichTextBox."),
                CreateLayerRow(FluentIconRegular.Table24, "Controls/Data", "CollectionControls.jalxaml", "List, tree, table, and data inspection surfaces."),
                CreateLayerRow(FluentIconRegular.Navigation24, "Controls/Shell", "NavigationControls.jalxaml", "NavigationView, tabs, frame, and app structure styles."),
                CreateLayerRow(FluentIconRegular.PanelLeft24, "Controls/Disclosure", "DisclosureControls.jalxaml", "Expander, tooltip, dialog, and grouped content surfaces.")
            }
        };
    }

    private static UIElement CreateDualSurfaceSample()
    {
        return new FWWrapPanel
        {
            HorizontalSpacing = 10,
            VerticalSpacing = 10,
            Children =
            {
                CreateSurfaceRoleTile(FluentIconRegular.DesignIdeas24, "Theme mode", "Jalium base controls receive Fluent styles through implicit resource dictionaries."),
                CreateSurfaceRoleTile(FluentIconRegular.BranchFork24, "FW controls", "FW-prefixed controls expose independent FluentJalium surfaces and behavior."),
                CreateSurfaceRoleTile(FluentIconRegular.WindowBrush24, "Window materials", "SystemBackdrop roles stay separate from element-level material effects."),
                CreateSurfaceRoleTile(FluentIconRegular.Glasses24, "Element effects", "Acrylic, Mica, FrostedGlass, and LiquidGlass live inside control content.")
            }
        };
    }

    private static FWBorder CreateLayerRow(FluentIconRegular icon, string title, string fileName, string description)
    {
        return new FWBorder
        {
            Width = 500,
            Background = ThemeBrush("LayerFillColorDefaultBrush"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10,
                Children =
                {
                    CreateIcon(icon, 20, ThemeBrush("TextPrimary")),
                    new FWStackPanel
                    {
                        Orientation = Orientation.Vertical,
                        Spacing = 4,
                        Children =
                        {
                            new FWTextBlock
                            {
                                Text = title,
                                FontSize = 13,
                                Foreground = ThemeBrush("TextPrimary")
                            },
                            new FWTextBlock
                            {
                                Text = fileName,
                                FontSize = 11,
                                Foreground = ThemeBrush("TextSecondary"),
                                TextWrapping = TextWrapping.Wrap
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
                }
            }
        };
    }

    private static FWBorder CreateSurfaceRoleTile(FluentIconRegular icon, string title, string description)
    {
        return new FWBorder
        {
            Width = 235,
            Background = ThemeBrush("LayerFillColorDefaultBrush"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 8,
                Children =
                {
                    CreateIcon(icon, 22, ThemeBrush("TextPrimary")),
                    new FWTextBlock
                    {
                        Text = title,
                        FontSize = 14,
                        Foreground = ThemeBrush("TextPrimary")
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

    private static FWBorder CreateArchitectureTokenPill(FluentIconRegular icon, string title, string value)
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
                        FontSize = 11,
                        Foreground = ThemeBrush("TextPrimary"),
                        VerticalAlignment = VerticalAlignment.Center
                    }
                }
            }
        };
    }

    private static FWBorder CreateArchitectureCard(FluentIconRegular icon, string title, string description, UIElement content)
    {
        return new FWBorder
        {
            Width = 540,
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
                        CreateIcon(FluentIconRegular.Diagram24, 24, ThemeBrush("TextPrimary")),
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
