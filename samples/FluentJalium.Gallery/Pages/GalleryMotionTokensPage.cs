using System.Globalization;
using FluentJalium.Controls;
using FluentJalium.Gallery.Controls;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using Jalium.UI.Media.Animation;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;
using AnimationDuration = Jalium.UI.Media.Animation.Duration;

namespace FluentJalium.Gallery.Pages;

internal sealed class GalleryMotionTokensPage
{
    public UIElement CreateContent()
    {
        var panel = CreateSection("Motion Tokens");
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        panel.Children.Add(CreateMotionTokenStrip());
        examples.Children.Add(CreateMotionExampleCard(
            FluentIconRegular.Timer24,
            "Duration scale",
            "Fast, normal, and emphasized timings map FluentJalium control changes to WinUI-style motion roles.",
            CreateDurationScaleSample()));
        examples.Children.Add(CreateMotionExampleCard(
            FluentIconRegular.Connected24,
            "Connected animation recipe",
            "Shared element motion exposes the same core timing and opacity values used by FWConnectedAnimationService.",
            CreateConnectedAnimationRecipeSample()));
        examples.Children.Add(CreateMotionExampleCard(
            FluentIconRegular.SlideTransition24,
            "Transition roles",
            "FWTransitioningContentControl names transition modes by interaction role so Gallery pages can stay paged and spatially clear.",
            CreateTransitionRoleSample()));

        panel.Children.Add(examples);
        return panel;
    }

    private static FWBorder CreateMotionTokenStrip()
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
                    CreateMotionTokenPill(FluentIconRegular.FastForward24, "Fast", "FluentMotionDurationFast", FormatResourceValue("FluentMotionDurationFast")),
                    CreateMotionTokenPill(FluentIconRegular.Clock24, "Normal", "FluentMotionDurationNormal", FormatResourceValue("FluentMotionDurationNormal")),
                    CreateMotionTokenPill(FluentIconRegular.ArrowTrending24, "Emphasized", "FluentMotionDurationEmphasized", FormatResourceValue("FluentMotionDurationEmphasized")),
                    CreateMotionTokenPill(FluentIconRegular.Connected24, "Connected", "FluentMotionConnectedAnimationDuration", FormatResourceValue("FluentMotionConnectedAnimationDuration")),
                    CreateMotionTokenPill(FluentIconRegular.Gauge24, "Initial opacity", "FluentMotionConnectedAnimationInitialOpacity", FormatPercentResourceValue("FluentMotionConnectedAnimationInitialOpacity"))
                }
            }
        };
    }

    private static UIElement CreateDurationScaleSample()
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                CreateDurationRow(FluentIconRegular.FastForward24, "Fast", "FluentMotionDurationFast", "Hover feedback, small opacity changes, and compact state swaps."),
                CreateDurationRow(FluentIconRegular.Clock24, "Normal", "FluentMotionDurationNormal", "Default content replacement and common FW control transitions."),
                CreateDurationRow(FluentIconRegular.ArrowTrending24, "Emphasized", "FluentMotionDurationEmphasized", "Larger spatial moves and transitions that should read as intentional.")
            }
        };
    }

    private static UIElement CreateConnectedAnimationRecipeSample()
    {
        return new FWWrapPanel
        {
            HorizontalSpacing = 10,
            VerticalSpacing = 10,
            Children =
            {
                CreateRecipeTile(FluentIconRegular.Connected24, "Duration", "FluentMotionConnectedAnimationDuration", FormatResourceValue("FluentMotionConnectedAnimationDuration")),
                CreateRecipeTile(FluentIconRegular.Gauge24, "Initial opacity", "FluentMotionConnectedAnimationInitialOpacity", FormatPercentResourceValue("FluentMotionConnectedAnimationInitialOpacity")),
                CreateRecipeTile(FluentIconRegular.Target24, "Direct", nameof(FWConnectedAnimationConfiguration.Direct), "Translate and scale directly from source to destination."),
                CreateRecipeTile(FluentIconRegular.AlignCenterHorizontal24, "Gravity", nameof(FWConnectedAnimationConfiguration.Gravity), "Move toward the destination center while preserving spatial context.")
            }
        };
    }

    private static UIElement CreateTransitionRoleSample()
    {
        return new FWWrapPanel
        {
            HorizontalSpacing = 10,
            VerticalSpacing = 10,
            Children =
            {
                CreateTransitionTile(FluentIconRegular.SlideTransition24, nameof(TransitionMode.Crossfade), "Subtle content replacement."),
                CreateTransitionTile(FluentIconRegular.ArrowRight24, nameof(TransitionMode.SlideLeft), "Directional navigation motion."),
                CreateTransitionTile(FluentIconRegular.LayerDiagonalSparkle24, nameof(TransitionMode.LiquidMorph), "Material-aware surface morphing."),
                CreateTransitionTile(FluentIconRegular.Sparkle24, nameof(TransitionMode.SketchReveal), "Expressive reveal for generated or illustrative surfaces.")
            }
        };
    }

    private static FWBorder CreateDurationRow(FluentIconRegular icon, string title, string tokenKey, string description)
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
                Orientation = Orientation.Vertical,
                Spacing = 9,
                Children =
                {
                    new FWStackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 8,
                        Children =
                        {
                            CreateIcon(icon, 18, ThemeBrush("TextSecondary")),
                            new FWTextBlock
                            {
                                Text = title,
                                Width = 92,
                                FontSize = 13,
                                Foreground = ThemeBrush("TextPrimary"),
                                VerticalAlignment = VerticalAlignment.Center
                            },
                            new FWTextBlock
                            {
                                Text = FormatResourceValue(tokenKey),
                                Width = 72,
                                FontSize = 12,
                                Foreground = ThemeBrush("TextSecondary"),
                                VerticalAlignment = VerticalAlignment.Center
                            },
                            CreateDurationTrack(tokenKey)
                        }
                    },
                    new FWTextBlock
                    {
                        Text = description,
                        FontSize = 12,
                        Foreground = ThemeBrush("TextSecondary"),
                        TextWrapping = TextWrapping.Wrap
                    },
                    new FWTextBlock
                    {
                        Text = tokenKey,
                        FontSize = 11,
                        Foreground = ThemeBrush("TextSecondary"),
                        TextWrapping = TextWrapping.Wrap
                    }
                }
            }
        };
    }

    private static FWBorder CreateDurationTrack(string tokenKey)
    {
        return new FWBorder
        {
            Width = 170,
            Height = 8,
            Background = ThemeBrush("ControlBackground"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Child = new FWBorder
            {
                Width = TrackWidth(tokenKey),
                Height = 6,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Background = ThemeBrush("AccentBrush"),
                CornerRadius = new CornerRadius(3)
            }
        };
    }

    private static FWBorder CreateMotionTokenPill(FluentIconRegular icon, string title, string tokenKey, string value)
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

    private static FWBorder CreateRecipeTile(FluentIconRegular icon, string title, string tokenKey, string description)
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
                    CreateIcon(icon, 20, ThemeBrush("TextPrimary")),
                    new FWTextBlock
                    {
                        Text = title,
                        FontSize = 14,
                        Foreground = ThemeBrush("TextPrimary")
                    },
                    new FWTextBlock
                    {
                        Text = tokenKey,
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
        };
    }

    private static FWBorder CreateTransitionTile(FluentIconRegular icon, string title, string description)
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
                Orientation = Orientation.Horizontal,
                Spacing = 10,
                Children =
                {
                    CreateIcon(icon, 22, ThemeBrush("TextPrimary")),
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

    private static FWBorder CreateMotionExampleCard(FluentIconRegular icon, string title, string description, UIElement content)
    {
        return GallerySampleCard.Create(icon, title, description, content, code: CreateSampleCode(title), width: 540);
    }

    private static string CreateSampleCode(string title)
    {
        return title switch
        {
            "Duration scale" => "<Duration x:Key=\"FluentMotionDurationFast\">0:0:0.083</Duration>\n<Duration x:Key=\"FluentMotionDurationNormal\">0:0:0.167</Duration>",
            "Connected animation recipe" => "var options = FWConnectedAnimationOptions.CreateDefault();\noptions.Configuration = FWConnectedAnimationConfiguration.Gravity;",
            "Transition roles" => "<FWTransitioningContentControl TransitionMode=\"SlideLeft\" />\n<FWTransitioningContentControl TransitionMode=\"LiquidMorph\" />",
            _ => "<FWTransitioningContentControl TransitionMode=\"Crossfade\" />"
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

    private static double TrackWidth(string key)
    {
        var milliseconds = ResourceMilliseconds(key);
        if (milliseconds <= 0)
        {
            return 34;
        }

        return Math.Min(156, Math.Max(34, milliseconds / 320 * 156));
    }

    private static double ResourceMilliseconds(string key)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) != true)
        {
            return 0;
        }

        return value switch
        {
            AnimationDuration duration when duration.HasTimeSpan => duration.TimeSpan.TotalMilliseconds,
            TimeSpan timeSpan => timeSpan.TotalMilliseconds,
            string text when TimeSpan.TryParse(text, CultureInfo.InvariantCulture, out var timeSpan) => timeSpan.TotalMilliseconds,
            _ => 0
        };
    }

    private static string FormatPercentResourceValue(string key)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) == true &&
            double.TryParse(value?.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
        {
            return number.ToString("P0", CultureInfo.InvariantCulture);
        }

        return FormatResourceValue(key);
    }

    private static string FormatResourceValue(string key)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) != true)
        {
            return "-";
        }

        return value switch
        {
            AnimationDuration duration => FormatDuration(duration),
            TimeSpan timeSpan => FormatTimeSpan(timeSpan),
            string text => text,
            double number => number.ToString("0.##", CultureInfo.InvariantCulture),
            int number => number.ToString(CultureInfo.InvariantCulture),
            _ => value.ToString() ?? "-"
        };
    }

    private static string FormatDuration(AnimationDuration duration)
    {
        return duration.HasTimeSpan ? FormatTimeSpan(duration.TimeSpan) : duration.ToString();
    }

    private static string FormatTimeSpan(TimeSpan timeSpan)
    {
        return timeSpan.TotalMilliseconds >= 1
            ? $"{timeSpan.TotalMilliseconds:0} ms"
            : timeSpan.ToString();
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
