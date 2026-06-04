using FluentJalium.Controls;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Media;
using Jalium.UI.Media.Animation;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWButton = FluentJalium.Controls.FWButton;
using FWFluentMaterialKind = FluentJalium.Controls.FWFluentMaterialKind;
using FWFluentMaterialSurface = FluentJalium.Controls.FWFluentMaterialSurface;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWTransitioningContentControl = FluentJalium.Controls.FWTransitioningContentControl;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;

namespace FluentJalium.Gallery.Pages;

internal sealed class GalleryMotionPage
{
    private readonly FWConnectedAnimationService _connectedAnimationService = new();

    public UIElement CreateContent()
    {
        var panel = CreateSection("Motion and Transitions");
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        examples.Children.Add(CreateMotionExampleCard(
            FluentIconRegular.SlideTransition24,
            "Connected animation",
            "Shared elements keep spatial continuity when navigation moves content between views.",
            CreateConnectedAnimationSample()));
        examples.Children.Add(CreateMotionExampleCard(
            FluentIconRegular.FlowSparkle24,
            "Content transitions",
            "FWTransitioningContentControl wraps Jalium content transitions with Fluent motion defaults.",
            CreateContentTransitionSample()));

        panel.Children.Add(CreateMotionTokenStrip());
        panel.Children.Add(examples);
        return panel;
    }

    private UIElement CreateConnectedAnimationSample()
    {
        const string animationKey = "motion.connected-card";

        var options = new FWConnectedAnimationOptions();
        var source = CreateMotionSurface(
            FluentIconRegular.Connected24,
            "Source",
            "Prepare shared element",
            new SolidColorBrush(Color.FromArgb(110, 255, 255, 255)));
        var destination = CreateMotionSurface(
            FluentIconRegular.Sparkle24,
            "Destination",
            "Start transform",
            ThemeBrush("LayerFillColorDefaultBrush"));
        var status = CreateMotionOutputText("Connected animation: Direct configuration.");

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Children =
            {
                new FWStackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 12,
                    Children =
                    {
                        source,
                        CreateMotionArrow(),
                        destination
                    }
                },
                CreateMotionButtonRow(
                    CreateMotionActionButton(FluentIconRegular.Connected24, "Prepare", () =>
                    {
                        var prepared = _connectedAnimationService.PrepareToAnimate(animationKey, source, options);
                        if (prepared && _connectedAnimationService.TryCreatePreparedPlan(animationKey, destination, out var plan))
                        {
                            status.Text = $"Prepared {plan.Configuration}: X {plan.TranslateX:0}, Y {plan.TranslateY:0}, Scale {plan.ScaleX:0.##}.";
                        }
                        else
                        {
                            status.Text = "Prepare waits until source and destination have layout bounds.";
                        }
                    }),
                    CreateMotionActionButton(FluentIconRegular.Play24, "Start", () =>
                    {
                        var started = _connectedAnimationService.TryStart(animationKey, destination);
                        status.Text = started
                            ? $"Animating destination with {options.Configuration} motion."
                            : "Prepare the source before starting the motion.";
                    }),
                    CreateMotionActionButton(FluentIconRegular.Target24, "Direct", () =>
                    {
                        options.Configuration = FWConnectedAnimationConfiguration.Direct;
                        status.Text = "Connected animation: Direct configuration.";
                    }),
                    CreateMotionActionButton(FluentIconRegular.AlignCenterHorizontal24, "Gravity", () =>
                    {
                        options.Configuration = FWConnectedAnimationConfiguration.Gravity;
                        status.Text = "Connected animation: Gravity configuration.";
                    })),
                CreateMotionStatus(status)
            }
        };
    }

    private static UIElement CreateContentTransitionSample()
    {
        var transitionHost = new FWTransitioningContentControl
        {
            Width = 456,
            Height = 180,
            TransitionMode = TransitionMode.Crossfade,
            Content = CreateTransitionPanel(
                FluentIconRegular.SlideTransition24,
                "Crossfade",
                "Default Fluent motion for subtle content changes.",
                FWFluentMaterialKind.Acrylic)
        };
        var status = CreateMotionOutputText("Current transition: Crossfade");

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Children =
            {
                CreateTransitionHostSurface(transitionHost),
                CreateMotionButtonRow(
                    CreateTransitionModeButton(transitionHost, status, FluentIconRegular.SlideTransition24, TransitionMode.Crossfade, "Crossfade"),
                    CreateTransitionModeButton(transitionHost, status, FluentIconRegular.ArrowRight24, TransitionMode.SlideLeft, "Slide"),
                    CreateTransitionModeButton(transitionHost, status, FluentIconRegular.Glasses24, TransitionMode.LiquidMorph, "Liquid"),
                    CreateTransitionModeButton(transitionHost, status, FluentIconRegular.Sparkle24, TransitionMode.SketchReveal, "Sketch")),
                CreateMotionStatus(status)
            }
        };
    }

    private static FWFluentMaterialSurface CreateMotionSurface(FluentIconRegular icon, string title, string description, Brush background)
    {
        return new FWFluentMaterialSurface
        {
            Width = 180,
            Height = 112,
            MaterialKind = FWFluentMaterialKind.LiquidGlass,
            TintColor = Color.FromArgb(180, 20, 84, 145),
            TintOpacity = 0.2,
            BlurRadius = 10,
            RefractionAmount = 64,
            ChromaticAberration = 0.36,
            FusionRadius = 24,
            Background = background,
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Shape = BorderShape.SuperEllipse,
            SuperEllipseN = 4,
            Padding = new Thickness(14),
            Child = CreateMotionSurfaceContent(icon, title, description)
        };
    }

    private static FWStackPanel CreateMotionSurfaceContent(FluentIconRegular icon, string title, string description)
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

    private static FWBorder CreateMotionArrow()
    {
        return new FWBorder
        {
            Width = 42,
            Height = 112,
            Child = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    CreateIcon(FluentIconRegular.ArrowRight24, 24, ThemeBrush("TextSecondary"))
                }
            }
        };
    }

    private static FWBorder CreateTransitionHostSurface(FWTransitioningContentControl transitionHost)
    {
        return new FWBorder
        {
            Background = ThemeBrush("LayerFillColorDefaultBrush"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(14),
            Child = transitionHost
        };
    }

    private static FWFluentMaterialSurface CreateTransitionPanel(
        FluentIconRegular icon,
        string title,
        string description,
        FWFluentMaterialKind materialKind)
    {
        return new FWFluentMaterialSurface
        {
            MaterialKind = materialKind,
            TintColor = Color.FromArgb(180, 20, 84, 145),
            TintOpacity = 0.24,
            BlurRadius = 12,
            RefractionAmount = 62,
            ChromaticAberration = 0.38,
            Background = new SolidColorBrush(Color.FromArgb(76, 255, 255, 255)),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Shape = BorderShape.SuperEllipse,
            SuperEllipseN = 4,
            Padding = new Thickness(16),
            Child = CreateMotionSurfaceContent(icon, title, description)
        };
    }

    private static FWButton CreateTransitionModeButton(
        FWTransitioningContentControl transitionHost,
        TextBlock status,
        FluentIconRegular icon,
        TransitionMode transitionMode,
        string label)
    {
        return CreateMotionActionButton(icon, label, () =>
        {
            transitionHost.TransitionMode = transitionMode;
            transitionHost.Content = CreateTransitionPanel(
                icon,
                transitionMode.ToString(),
                $"TransitionMode.{transitionMode} rendered through a FluentJalium FW surface.",
                transitionMode == TransitionMode.LiquidMorph ? FWFluentMaterialKind.LiquidGlass : FWFluentMaterialKind.Acrylic);
            status.Text = $"Current transition: {transitionMode}";
        });
    }

    private static FWBorder CreateMotionTokenStrip()
    {
        return new FWBorder
        {
            Background = ThemeBrush("LayerFillColorDefaultBrush"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12),
            Child = new FWWrapPanel
            {
                HorizontalSpacing = 8,
                VerticalSpacing = 8,
                Children =
                {
                    CreateMotionTokenPill(FluentIconRegular.Timer24, "Fast", "120 ms"),
                    CreateMotionTokenPill(FluentIconRegular.SlideTransition24, "Normal", "280 ms"),
                    CreateMotionTokenPill(FluentIconRegular.Connected24, "Connected", "320 ms"),
                    CreateMotionTokenPill(FluentIconRegular.Gauge24, "Initial opacity", "72%")
                }
            }
        };
    }

    private static FWBorder CreateMotionTokenPill(FluentIconRegular icon, string title, string value)
    {
        return new FWBorder
        {
            Background = ThemeBrush("ControlBackground"),
            BorderBrush = ThemeBrush("ControlBorder"),
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
                    }
                }
            }
        };
    }

    private static FWBorder CreateMotionExampleCard(FluentIconRegular icon, string title, string description, UIElement content)
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

    private static FWStackPanel CreateMotionButtonRow(params FWButton[] buttons)
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

    private static FWButton CreateMotionActionButton(FluentIconRegular icon, string text, Action action)
    {
        var button = new FWButton
        {
            Content = new FWStackPanel
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
            }
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static TextBlock CreateMotionOutputText(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        };
    }

    private static FWBorder CreateMotionStatus(TextBlock status)
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
                        CreateIcon(FluentIconRegular.SlideTransition24, 24, ThemeBrush("TextPrimary")),
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
