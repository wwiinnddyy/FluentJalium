using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Media;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWButton = FluentJalium.Controls.FWButton;
using FWFluentMaterialKind = FluentJalium.Controls.FWFluentMaterialKind;
using FWFluentMaterialSurface = FluentJalium.Controls.FWFluentMaterialSurface;
using FWProgressRingSize = FluentJalium.Controls.FWProgressRingSize;
using FWRangeDensity = FluentJalium.Controls.FWRangeDensity;
using FWProgressBar = FluentJalium.Controls.FWProgressBar;
using FWProgressRing = FluentJalium.Controls.FWProgressRing;
using FWRangeSlider = FluentJalium.Controls.FWRangeSlider;
using FWSlider = FluentJalium.Controls.FWSlider;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;

namespace FluentJalium.Gallery.Pages;

internal sealed class GalleryRangePage
{
    public UIElement CreateContent()
    {
        var panel = CreateSection("Range and Progress");
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        examples.Children.Add(CreateRangeExampleCard(
            FluentIconRegular.Gauge24,
            "FWSlider",
            "Continuous and stepped values with live output, vertical orientation, and disabled state.",
            CreateSliderSample()));
        examples.Children.Add(CreateRangeExampleCard(
            FluentIconRegular.ArrowTrending24,
            "FWRangeSlider",
            "Two-thumb range selection with snapped steps, minimum range, and disabled state.",
            CreateRangeSliderSample()));
        examples.Children.Add(CreateRangeExampleCard(
            FluentIconRegular.DataUsage24,
            "FWProgressBar",
            "Determinate progress driven by a slider plus indeterminate, paused, error, and disabled states.",
            CreateProgressBarSample()));
        examples.Children.Add(CreateRangeExampleCard(
            FluentIconRegular.CircleMultipleConcentric24,
            "FWProgressRing",
            "Active indeterminate ring, determinate value output, inactive, and disabled states.",
            CreateProgressRingSample()));
        examples.Children.Add(CreateRangeExampleCard(
            FluentIconRegular.LayerDiagonalSparkle24,
            "Material progress surface",
            "Progress indicators stay legible on FluentJalium liquid glass and layered materials.",
            CreateMaterialProgressSample()));

        panel.Children.Add(examples);
        return panel;
    }

    private static UIElement CreateSliderSample()
    {
        var sliderValueOutput = CreateRangeOutput("Value: 64");
        var slider = new FWSlider
        {
            Width = 320,
            Density = FWRangeDensity.Comfortable,
            Minimum = 0,
            Maximum = 100,
            Value = 64,
            TickFrequency = 10,
            IsSnapToTickEnabled = true
        };
        slider.ValueChanged += (_, e) => sliderValueOutput.Text = FormatRangeValue("Value", e.NewValue);

        var verticalSliderOutput = CreateRangeOutput("Vertical value: 20");
        var verticalSlider = new FWSlider
        {
            Orientation = Orientation.Vertical,
            Width = 36,
            Height = 120,
            Minimum = -50,
            Maximum = 50,
            Value = 20,
            TickFrequency = 10,
            IsSnapToTickEnabled = true
        };
        verticalSlider.ValueChanged += (_, e) => verticalSliderOutput.Text = FormatRangeValue("Vertical value", e.NewValue);

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Children =
            {
                slider,
                CreateRangeStatus(sliderValueOutput),
                CreateRangeButtonRow(
                    CreateRangeActionButton(FluentIconRegular.TextDensity24, "Density", () =>
                    {
                        slider.Density = NextDensity(slider.Density);
                        sliderValueOutput.Text = $"{FormatRangeValue("Value", slider.Value)}. Density: {FormatDensity(slider.Density)}";
                    })),
                new FWWrapPanel
                {
                    HorizontalSpacing = 18,
                    VerticalSpacing = 12,
                    Children =
                    {
                        CreateRangeStateColumn("Stepped", new FWSlider
                        {
                            Width = 220,
                            Density = FWRangeDensity.Compact,
                            Minimum = 0,
                            Maximum = 100,
                            Value = 40,
                            TickFrequency = 20,
                            IsSnapToTickEnabled = true
                        }),
                        CreateRangeStateColumn("Vertical", verticalSlider, verticalSliderOutput),
                        CreateRangeStateColumn("Disabled", new FWSlider
                        {
                            Width = 220,
                            Density = FWRangeDensity.Spacious,
                            Minimum = 0,
                            Maximum = 100,
                            Value = 55,
                            IsEnabled = false
                        })
                    }
                }
            }
        };
    }

    private static UIElement CreateRangeSliderSample()
    {
        var rangeOutput = CreateRangeOutput("Range: 24 to 76");
        var rangeSlider = new FWRangeSlider
        {
            Width = 320,
            Density = FWRangeDensity.Comfortable,
            Minimum = 0,
            Maximum = 100,
            RangeStart = 24,
            RangeEnd = 76,
            MinimumRange = 8,
            TickFrequency = 10,
            IsSnapToTickEnabled = true
        };
        void UpdateRangeOutput()
        {
            rangeOutput.Text = $"Range: {rangeSlider.RangeStart:0} to {rangeSlider.RangeEnd:0}";
        }

        rangeSlider.RangeStartChanged += (_, _) => UpdateRangeOutput();
        rangeSlider.RangeEndChanged += (_, _) => UpdateRangeOutput();

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Children =
            {
                rangeSlider,
                CreateRangeStatus(rangeOutput),
                CreateRangeButtonRow(
                    CreateRangeActionButton(FluentIconRegular.TextDensity24, "Density", () =>
                    {
                        rangeSlider.Density = NextDensity(rangeSlider.Density);
                        rangeOutput.Text = $"Range: {rangeSlider.RangeStart:0} to {rangeSlider.RangeEnd:0}. Density: {FormatDensity(rangeSlider.Density)}";
                    })),
                new FWWrapPanel
                {
                    HorizontalSpacing = 18,
                    VerticalSpacing = 12,
                    Children =
                    {
                        CreateRangeStateColumn("MinimumRange = 20", new FWRangeSlider
                        {
                            Width = 220,
                            Density = FWRangeDensity.Compact,
                            Minimum = 0,
                            Maximum = 100,
                            RangeStart = 30,
                            RangeEnd = 70,
                            MinimumRange = 20,
                            TickFrequency = 10,
                            IsSnapToTickEnabled = true
                        }),
                        CreateRangeStateColumn("Vertical", new FWRangeSlider
                        {
                            Orientation = Orientation.Vertical,
                            Width = 36,
                            Height = 120,
                            Density = FWRangeDensity.Comfortable,
                            Minimum = 0,
                            Maximum = 100,
                            RangeStart = 20,
                            RangeEnd = 80,
                            MinimumRange = 10,
                            TickFrequency = 10,
                            IsSnapToTickEnabled = true
                        }),
                        CreateRangeStateColumn("Disabled", new FWRangeSlider
                        {
                            Width = 220,
                            Density = FWRangeDensity.Spacious,
                            Minimum = 0,
                            Maximum = 100,
                            RangeStart = 18,
                            RangeEnd = 64,
                            MinimumRange = 8,
                            IsEnabled = false
                        })
                    }
                }
            }
        };
    }

    private static UIElement CreateProgressBarSample()
    {
        var progressOutput = CreateRangeOutput("Progress: 72%");
        var progressBar = new FWProgressBar
        {
            Width = 320,
            Density = FWRangeDensity.Spacious,
            Minimum = 0,
            Maximum = 100,
            Value = 72
        };
        var progressSlider = new FWSlider
        {
            Width = 320,
            Minimum = 0,
            Maximum = 100,
            Value = 72,
            TickFrequency = 10,
            IsSnapToTickEnabled = true
        };
        progressSlider.ValueChanged += (_, e) =>
        {
            progressBar.Value = e.NewValue;
            progressOutput.Text = FormatRangeValue("Progress", e.NewValue) + "%";
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Children =
            {
                progressBar,
                CreateRangeStatus(progressOutput),
                progressSlider,
                new FWStackPanel
                {
                    Orientation = Orientation.Vertical,
                    Spacing = 10,
                    Children =
                    {
                        CreateRangeStateRow("Determinate", new FWProgressBar
                        {
                            Width = 260,
                            Density = FWRangeDensity.Compact,
                            Minimum = 0,
                            Maximum = 100,
                            Value = 35
                        }),
                        CreateRangeStateRow("Indeterminate", new FWProgressBar
                        {
                            Width = 260,
                            Density = FWRangeDensity.Comfortable,
                            IsIndeterminate = true
                        }),
                        CreateRangeStateRow("Paused", new FWProgressBar
                        {
                            Width = 260,
                            Density = FWRangeDensity.Comfortable,
                            Minimum = 0,
                            Maximum = 100,
                            Value = 42,
                            ShowPaused = true
                        }),
                        CreateRangeStateRow("Error", new FWProgressBar
                        {
                            Width = 260,
                            Density = FWRangeDensity.Spacious,
                            Minimum = 0,
                            Maximum = 100,
                            Value = 86,
                            ShowError = true
                        }),
                        CreateRangeStateRow("Disabled", new FWProgressBar
                        {
                            Width = 260,
                            Density = FWRangeDensity.Comfortable,
                            Minimum = 0,
                            Maximum = 100,
                            Value = 55,
                            IsEnabled = false
                        })
                    }
                }
            }
        };
    }

    private static UIElement CreateProgressRingSample()
    {
        var ringOutput = CreateRangeOutput("Ring value: 72%");
        var determinateRing = new FWProgressRing
        {
            RingSize = FWProgressRingSize.Large,
            Minimum = 0,
            Maximum = 100,
            Value = 72,
            IsIndeterminate = false
        };
        var ringSlider = new FWSlider
        {
            Width = 260,
            Minimum = 0,
            Maximum = 100,
            Value = 72,
            TickFrequency = 10,
            IsSnapToTickEnabled = true
        };
        ringSlider.ValueChanged += (_, e) =>
        {
            determinateRing.Value = e.NewValue;
            ringOutput.Text = FormatRangeValue("Ring value", e.NewValue) + "%";
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Children =
            {
                new FWWrapPanel
                {
                    HorizontalSpacing = 22,
                    VerticalSpacing = 14,
                    Children =
                    {
                        CreateRangeRingState("Indeterminate", new FWProgressRing
                        {
                            RingSize = FWProgressRingSize.Medium,
                            IsIndeterminate = true
                        }),
                        CreateRangeRingState("Determinate", determinateRing),
                        CreateRangeRingState("Inactive", new FWProgressRing
                        {
                            RingSize = FWProgressRingSize.Small,
                            Value = 42,
                            IsActive = false,
                            IsIndeterminate = false
                        }),
                        CreateRangeRingState("Disabled", new FWProgressRing
                        {
                            RingSize = FWProgressRingSize.Large,
                            Value = 64,
                            IsIndeterminate = false,
                            IsEnabled = false
                        })
                    }
                },
                CreateRangeStatus(ringOutput),
                CreateRangeButtonRow(
                    CreateRangeActionButton(FluentIconRegular.ResizeLarge24, "Ring size", () =>
                    {
                        determinateRing.RingSize = NextRingSize(determinateRing.RingSize);
                        ringOutput.Text = $"{FormatRangeValue("Ring value", determinateRing.Value)}%. Size: {FormatRingSize(determinateRing.RingSize)}";
                    })),
                ringSlider
            }
        };
    }

    private static UIElement CreateMaterialProgressSample()
    {
        var progressBar = new FWProgressBar
        {
            Width = 336,
            Density = FWRangeDensity.Spacious,
            Minimum = 0,
            Maximum = 100,
            Value = 68
        };
        var ring = new FWProgressRing
        {
            RingSize = FWProgressRingSize.Large,
            Minimum = 0,
            Maximum = 100,
            Value = 68,
            IsIndeterminate = false
        };

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
                Spacing = 14,
                Children =
                {
                    new FWStackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 12,
                        Children =
                        {
                            ring,
                            new FWStackPanel
                            {
                                Orientation = Orientation.Vertical,
                                Spacing = 4,
                                Children =
                                {
                                    new FWTextBlock
                                    {
                                        Text = "Material transfer",
                                        FontSize = 15,
                                        Foreground = ThemeBrush("TextPrimary")
                                    },
                                    new FWTextBlock
                                    {
                                        Text = "68% complete",
                                        FontSize = 12,
                                        Foreground = ThemeBrush("TextSecondary")
                                    }
                                }
                            }
                        }
                    },
                    progressBar,
                    CreateRangeStateRow("Status", new FWProgressBar
                    {
                        Width = 260,
                        Density = FWRangeDensity.Comfortable,
                        Minimum = 0,
                        Maximum = 100,
                        Value = 38,
                        ShowPaused = true
                    })
                }
            }
        };
    }

    private static FWBorder CreateRangeExampleCard(FluentIconRegular icon, string title, string description, UIElement content)
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

    private static FWStackPanel CreateRangeStateColumn(string label, params UIElement[] controls)
    {
        var stack = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8
        };

        stack.Children.Add(CreateRangeOutput(label));
        foreach (var control in controls)
        {
            stack.Children.Add(control);
        }

        return stack;
    }

    private static FWStackPanel CreateRangeStateRow(string label, UIElement control)
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Children =
            {
                new FWTextBlock
                {
                    Text = label,
                    Width = 96,
                    FontSize = 12,
                    Foreground = ThemeBrush("TextSecondary"),
                    VerticalAlignment = VerticalAlignment.Center
                },
                control
            }
        };
    }

    private static FWWrapPanel CreateRangeButtonRow(params FWButton[] buttons)
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

    private static FWButton CreateRangeActionButton(FluentIconRegular icon, string text, Action action)
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

    private static FWStackPanel CreateRangeRingState(string label, UIElement ring)
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Children =
            {
                ring,
                new FWTextBlock
                {
                    Text = label,
                    Width = 80,
                    FontSize = 12,
                    Foreground = ThemeBrush("TextSecondary"),
                    TextAlignment = TextAlignment.Center
                }
            }
        };
    }

    private static TextBlock CreateRangeOutput(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        };
    }

    private static FWBorder CreateRangeStatus(TextBlock status)
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
                        CreateIcon(FluentIconRegular.Gauge24, 24, ThemeBrush("TextPrimary")),
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

    private static string FormatRangeValue(string label, double value)
    {
        return $"{label}: {value:0}";
    }

    private static FWRangeDensity NextDensity(FWRangeDensity density)
    {
        return density switch
        {
            FWRangeDensity.Compact => FWRangeDensity.Comfortable,
            FWRangeDensity.Comfortable => FWRangeDensity.Spacious,
            _ => FWRangeDensity.Compact
        };
    }

    private static string FormatDensity(FWRangeDensity density)
    {
        return density switch
        {
            FWRangeDensity.Compact => "compact",
            FWRangeDensity.Spacious => "spacious",
            _ => "comfortable"
        };
    }

    private static FWProgressRingSize NextRingSize(FWProgressRingSize ringSize)
    {
        return ringSize switch
        {
            FWProgressRingSize.Small => FWProgressRingSize.Medium,
            FWProgressRingSize.Medium => FWProgressRingSize.Large,
            _ => FWProgressRingSize.Small
        };
    }

    private static string FormatRingSize(FWProgressRingSize ringSize)
    {
        return ringSize switch
        {
            FWProgressRingSize.Small => "small",
            FWProgressRingSize.Large => "large",
            _ => "medium"
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
