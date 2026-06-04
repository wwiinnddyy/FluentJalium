using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Media;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWButton = FluentJalium.Controls.FWButton;
using FWFluentMaterialKind = FluentJalium.Controls.FWFluentMaterialKind;
using FWFluentMaterialSurface = FluentJalium.Controls.FWFluentMaterialSurface;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWSwitchDensity = FluentJalium.Controls.FWSwitchDensity;
using FWToggleButton = FluentJalium.Controls.FWToggleButton;
using FWToggleSwitch = FluentJalium.Controls.FWToggleSwitch;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;

namespace FluentJalium.Gallery.Pages;

internal sealed class GallerySwitchesPage
{
    public UIElement CreateContent()
    {
        var panel = CreateSection("Switches");
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        examples.Children.Add(CreateSwitchExampleCard(
            FluentIconRegular.ToggleMultiple24,
            "FWToggleButton",
            "Command-like boolean selection with checked, unchecked, indeterminate, and disabled states.",
            CreateToggleButtonStateSample()));
        examples.Children.Add(CreateSwitchExampleCard(
            FluentIconRegular.Power24,
            "FWToggleSwitch",
            "A WinUI-style switch surface for direct on/off settings, content labels, and disabled states.",
            CreateToggleSwitchStateSample()));
        examples.Children.Add(CreateSwitchExampleCard(
            FluentIconRegular.CursorClick24,
            "Events and content",
            "Toggle state changes update the content surface and raise the Jalium routed events.",
            CreateInteractiveSwitchSample()));
        examples.Children.Add(CreateSwitchExampleCard(
            FluentIconRegular.LayerDiagonalSparkle24,
            "Material settings row",
            "Switches stay readable on FluentJalium layered material surfaces.",
            CreateMaterialSwitchSample()));

        panel.Children.Add(examples);
        return panel;
    }

    private static UIElement CreateToggleButtonStateSample()
    {
        var output = CreateSwitchOutputText("ToggleButton state: waiting for selection.");
        var indeterminate = new FWToggleButton
        {
            Content = CreateButtonContent(FluentIconRegular.Flash24, "Mixed"),
            Density = FWSwitchDensity.Comfortable,
            IsThreeState = true,
            IsChecked = null
        };
        indeterminate.Checked += (_, _) => output.Text = "ToggleButton state: checked";
        indeterminate.Unchecked += (_, _) => output.Text = "ToggleButton state: unchecked";
        indeterminate.Indeterminate += (_, _) => output.Text = "ToggleButton state: indeterminate";

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Children =
            {
                CreateSwitchButtonRow(
                    new FWToggleButton { Content = CreateButtonContent(FluentIconRegular.DismissCircle24, "Off"), Density = FWSwitchDensity.Compact },
                    new FWToggleButton { Content = CreateButtonContent(FluentIconRegular.CheckmarkCircle24, "On"), Density = FWSwitchDensity.Comfortable, IsChecked = true },
                    indeterminate,
                    new FWToggleButton { Content = CreateButtonContent(FluentIconRegular.Pause24, "Disabled"), Density = FWSwitchDensity.Spacious, IsChecked = true, IsEnabled = false }),
                CreateSwitchStatus(output)
            }
        };
    }

    private static UIElement CreateToggleSwitchStateSample()
    {
        return new FWWrapPanel
        {
            HorizontalSpacing = 18,
            VerticalSpacing = 14,
            Children =
            {
                new FWToggleSwitch
                {
                    Header = "Default",
                    Density = FWSwitchDensity.Compact,
                    OffContent = "Off",
                    OnContent = "On"
                },
                new FWToggleSwitch
                {
                    Header = "On",
                    Density = FWSwitchDensity.Comfortable,
                    IsOn = true,
                    OffContent = "Off",
                    OnContent = "On"
                },
                new FWToggleSwitch
                {
                    Header = "Status",
                    Description = "Comfortable settings row",
                    Density = FWSwitchDensity.Comfortable,
                    IsOn = true,
                    OffContent = "Paused",
                    OnContent = "Running"
                },
                new FWToggleSwitch
                {
                    Header = "Disabled",
                    Description = "Spacious disabled row",
                    Density = FWSwitchDensity.Spacious,
                    IsOn = true,
                    IsEnabled = false,
                    OffContent = "Off",
                    OnContent = "On"
                }
            }
        };
    }

    private static UIElement CreateInteractiveSwitchSample()
    {
        var toggleSwitch = new FWToggleSwitch
        {
            Header = "Live setting",
            Description = "Cycle density or flip the state.",
            Density = FWSwitchDensity.Comfortable,
            OffContent = "Notifications off",
            OnContent = "Notifications on"
        };
        var output = CreateSwitchOutputText("Live setting: off. Density: comfortable");

        toggleSwitch.Toggled += (_, _) =>
        {
            output.Text = $"Live setting: {(toggleSwitch.IsOn ? "on" : "off")}. Density: {FormatDensity(toggleSwitch.Density)}";
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Children =
            {
                toggleSwitch,
                CreateSwitchButtonRow(
                    CreateSwitchActionButton(FluentIconRegular.Play24, "Turn on", () => toggleSwitch.IsOn = true),
                    CreateSwitchActionButton(FluentIconRegular.DismissCircle24, "Turn off", () => toggleSwitch.IsOn = false),
                    CreateSwitchActionButton(FluentIconRegular.Keyboard24, "Flip", () => toggleSwitch.IsOn = !toggleSwitch.IsOn),
                    CreateSwitchActionButton(FluentIconRegular.TextDensity24, "Density", () =>
                    {
                        toggleSwitch.Density = NextDensity(toggleSwitch.Density);
                        output.Text = $"Live setting: {(toggleSwitch.IsOn ? "on" : "off")}. Density: {FormatDensity(toggleSwitch.Density)}";
                    })),
                CreateSwitchStatus(output)
            }
        };
    }

    private static UIElement CreateMaterialSwitchSample()
    {
        return new FWFluentMaterialSurface
        {
            Width = 456,
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
                    CreateMaterialSwitchRow("Mica layer", "Use backdrop-aware surfaces", FWSwitchDensity.Compact, isOn: true),
                    CreateMaterialSwitchRow("Acrylic pass", "Show translucent panels", FWSwitchDensity.Comfortable, isOn: true),
                    CreateMaterialSwitchRow("Reduced motion", "Keep state changes calm", FWSwitchDensity.Spacious, isOn: false)
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
                CreateIcon(FluentIconRegular.Settings24, 18, ThemeBrush("TextPrimary")),
                new FWTextBlock
                {
                    Text = "Fluent settings",
                    FontSize = 15,
                    Foreground = ThemeBrush("TextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
    }

    private static FWBorder CreateMaterialSwitchRow(string title, string detail, FWSwitchDensity density, bool isOn)
    {
        return new FWBorder
        {
            Background = ThemeBrush("LayerFillColorDefaultBrush"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10),
            Child = new FWToggleSwitch
            {
                Header = title,
                Description = detail,
                Density = density,
                IsOn = isOn,
                OffContent = "Off",
                OnContent = "On",
                VerticalAlignment = VerticalAlignment.Center
            }
        };
    }

    private static FWBorder CreateSwitchExampleCard(FluentIconRegular icon, string title, string description, UIElement content)
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

    private static FWWrapPanel CreateSwitchButtonRow(params FWToggleButton[] buttons)
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

    private static FWWrapPanel CreateSwitchButtonRow(params FWButton[] buttons)
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

    private static FWButton CreateSwitchActionButton(FluentIconRegular icon, string text, Action action)
    {
        var button = new FWButton
        {
            Content = CreateButtonContent(icon, text)
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static FWStackPanel CreateButtonContent(FluentIconRegular icon, string text)
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

    private static TextBlock CreateSwitchOutputText(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        };
    }

    private static FWSwitchDensity NextDensity(FWSwitchDensity density)
    {
        return density switch
        {
            FWSwitchDensity.Compact => FWSwitchDensity.Comfortable,
            FWSwitchDensity.Comfortable => FWSwitchDensity.Spacious,
            _ => FWSwitchDensity.Compact
        };
    }

    private static string FormatDensity(FWSwitchDensity density)
    {
        return density switch
        {
            FWSwitchDensity.Compact => "compact",
            FWSwitchDensity.Spacious => "spacious",
            _ => "comfortable"
        };
    }

    private static FWBorder CreateSwitchStatus(TextBlock status)
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
                        CreateIcon(FluentIconRegular.ToggleMultiple24, 24, ThemeBrush("TextPrimary")),
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
