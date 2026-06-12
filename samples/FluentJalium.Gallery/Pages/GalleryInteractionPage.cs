using FluentJalium.Icon;
using FluentJalium.Gallery.Controls;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWButton = FluentJalium.Controls.FWButton;
using FWFluentMaterialKind = FluentJalium.Controls.FWFluentMaterialKind;
using FWFluentMaterialSurface = FluentJalium.Controls.FWFluentMaterialSurface;
using FWGridSplitter = FluentJalium.Controls.FWGridSplitter;
using FWLabel = FluentJalium.Controls.FWLabel;
using FWScrollViewer = FluentJalium.Controls.FWScrollViewer;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWSwipeControl = FluentJalium.Controls.FWSwipeControl;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;

namespace FluentJalium.Gallery.Pages;

internal sealed class GalleryInteractionPage
{
    public UIElement CreateContent()
    {
        var panel = CreateSection("Interaction and Scrolling");
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        examples.Children.Add(CreateInteractionExampleCard(
            FluentIconRegular.TextBulletListLtr24,
            "FWScrollViewer",
            "Visible, hidden, and auto-hide scroll bars with command-driven offset states.",
            CreateScrollViewerSample()));
        examples.Children.Add(CreateInteractionExampleCard(
            FluentIconRegular.CursorClick24,
            "FWSwipeControl",
            "Reveal and execute swipe command collections with icons, destructive actions, and close behavior.",
            CreateSwipeControlSample()));
        examples.Children.Add(CreateInteractionExampleCard(
            FluentIconRegular.PanelLeftContract24,
            "FWGridSplitter",
            "Column resizing, preview mode, keyboard increments, and Fluent splitter grip states.",
            CreateGridSplitterSample()));
        examples.Children.Add(CreateInteractionExampleCard(
            FluentIconRegular.LayerDiagonalSparkle24,
            "Material interaction workbench",
            "Scroll, swipe, and split interactions remain legible on a LiquidGlass command surface.",
            CreateMaterialInteractionWorkbenchSample()));
        examples.Children.Add(CreateInteractionExampleCard(
            FluentIconRegular.Target24,
            "Focus visual styles",
            "Dual-ring focus indicators with WinUI-style outer and inner strokes for keyboard navigation clarity.",
            CreateFocusVisualSample()));

        panel.Children.Add(examples);
        return panel;
    }

    private static UIElement CreateScrollViewerSample()
    {
        var output = CreateInteractionOutput("ScrollViewer: vertical bars visible, auto-hide off.");
        var viewer = new FWScrollViewer
        {
            Width = 330,
            Height = 170,
            VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Background = ThemeBrush("SwipeControlBackground"),
            BorderBrush = ThemeBrush("SwipeControlBorderBrush"),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(12),
            IsScrollBarAutoHideEnabled = false,
            IsScrollInertiaEnabled = false,
            Content = CreateScrollableItems("Row", 10)
        };
        viewer.ScrollChanged += (_, args) => output.Text = $"Scroll offset: {args.VerticalOffset}.";

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 390,
            Children =
            {
                new FWLabel { Content = "FWScrollViewer states" },
                viewer,
                CreateInteractionButtonRow(
                    CreateInteractionActionButton(FluentIconRegular.ArrowUp24, "Top", () =>
                    {
                        viewer.ScrollToVerticalOffset(0);
                        output.Text = "ScrollViewer: top.";
                    }),
                    CreateInteractionActionButton(FluentIconRegular.ArrowDown24, "End", () =>
                    {
                        viewer.ScrollToVerticalOffset(viewer.ScrollableHeight);
                        output.Text = $"ScrollViewer: end {viewer.ScrollableHeight}.";
                    }),
                    CreateInteractionActionButton(FluentIconRegular.Eye24, "Auto hide", () =>
                    {
                        viewer.IsScrollBarAutoHideEnabled = !viewer.IsScrollBarAutoHideEnabled;
                        output.Text = $"Scroll auto-hide: {FormatOnOff(viewer.IsScrollBarAutoHideEnabled)}.";
                    })),
                CreateInteractionStatus(output)
            }
        };
    }

    private static UIElement CreateSwipeControlSample()
    {
        var output = CreateInteractionOutput("SwipeControl: left reveal, right execute.");
        var leftItems = CreateSwipeItems(SwipeMode.Reveal,
            ("Archive", FluentIconRegular.Archive24, "SwipeItemBackgroundSecondary", BehaviorOnInvoked.RemainOpen),
            ("Flag", FluentIconRegular.Flag24, "SwipeItemBackground", BehaviorOnInvoked.Auto));
        var rightItems = CreateSwipeItems(SwipeMode.Execute,
            ("Delete", FluentIconRegular.Delete24, "SwipeItemBackgroundDestructive", BehaviorOnInvoked.Close));
        var swipe = new FWSwipeControl
        {
            Width = 340,
            Height = 78,
            LeftItems = leftItems,
            RightItems = rightItems,
            Background = ThemeBrush("SwipeControlBackground"),
            BorderBrush = ThemeBrush("SwipeControlBorderBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Content = CreateSwipeRow("Message row", "Reveal Archive and Flag from the left; Delete executes from the right.")
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 390,
            Children =
            {
                new FWLabel { Content = "FWSwipeControl commands" },
                swipe,
                CreateInteractionButtonRow(
                    CreateInteractionActionButton(FluentIconRegular.Archive24, "Archive", () =>
                    {
                        output.Text = $"Swipe left action: {leftItems[0].Text}.";
                    }),
                    CreateInteractionActionButton(FluentIconRegular.Delete24, "Delete", () =>
                    {
                        output.Text = $"Swipe right action: {rightItems[0].Text}.";
                    }),
                    CreateInteractionActionButton(FluentIconRegular.DismissCircle24, "Close", () =>
                    {
                        swipe.Close();
                        output.Text = "SwipeControl closed.";
                    })),
                CreateInteractionStatus(output)
            }
        };
    }

    private static UIElement CreateGridSplitterSample()
    {
        var output = CreateInteractionOutput("GridSplitter: columns, preview off, keyboard increment 12.");
        var splitter = new FWGridSplitter
        {
            Width = 6,
            ResizeDirection = GridResizeDirection.Columns,
            ResizeBehavior = GridResizeBehavior.PreviousAndNext,
            KeyboardIncrement = 12,
            DragIncrement = 1
        };
        var grid = CreateSplitterPreview(splitter);

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 420,
            Children =
            {
                new FWLabel { Content = "FWGridSplitter layout" },
                grid,
                CreateInteractionButtonRow(
                    CreateInteractionActionButton(FluentIconRegular.SlideSettings24, "Preview", () =>
                    {
                        splitter.ShowsPreview = !splitter.ShowsPreview;
                        output.Text = $"Splitter preview: {FormatOnOff(splitter.ShowsPreview)}.";
                    }),
                    CreateInteractionActionButton(FluentIconRegular.Keyboard24, "Increment", () =>
                    {
                        splitter.KeyboardIncrement = splitter.KeyboardIncrement == 12 ? 24 : 12;
                        output.Text = $"Keyboard increment: {splitter.KeyboardIncrement}.";
                    }),
                    CreateInteractionActionButton(FluentIconRegular.ArrowAutofitWidth24, "Columns", () =>
                    {
                        splitter.ResizeDirection = GridResizeDirection.Columns;
                        output.Text = "Splitter direction: Columns.";
                    })),
                CreateInteractionStatus(output)
            }
        };
    }

    private static UIElement CreateFocusVisualSample()
    {
        var output = CreateInteractionOutput("Focus visual: Tab through controls to see dual-ring indicators.");

        var buttonsPanel = new FWWrapPanel
        {
            HorizontalSpacing = 12,
            VerticalSpacing = 12,
            Margin = new Thickness(0, 0, 0, 12),
            Children =
            {
                CreateFocusButton("Primary", FluentIconRegular.CheckmarkCircle24),
                CreateFocusButton("Secondary", FluentIconRegular.DismissCircle24),
                CreateFocusButton("Tertiary", FluentIconRegular.Info24)
            }
        };

        var textBox = new TextBox
        {
            Width = 200,
            PlaceholderText = "Focus with Tab key",
            Margin = new Thickness(0, 0, 0, 12)
        };
        textBox.GotFocus += (s, e) => output.Text = "Focus: TextBox received keyboard focus.";
        textBox.LostFocus += (s, e) => output.Text = "Focus: TextBox lost focus.";

        var checkBox = new CheckBox
        {
            Content = "Enable dual-ring focus visual",
            IsChecked = true,
            Margin = new Thickness(0, 0, 0, 8)
        };

        var focusInfo = new FWBorder
        {
            Background = ThemeBrush("LayerFillColorDefaultBrush"),
            BorderBrush = ThemeBrush("ControlElevationBorderBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12),
            Margin = new Thickness(0, 0, 0, 12),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 6,
                Children =
                {
                    new FWTextBlock
                    {
                        Text = "Dual-ring focus visual",
                        FontSize = 13,
                        FontWeight = FontWeights.SemiBold,
                        Foreground = ThemeBrush("TextPrimary")
                    },
                    new FWTextBlock
                    {
                        Text = "Outer ring: 2px stroke, 3px offset",
                        FontSize = 12,
                        Foreground = ThemeBrush("TextSecondary")
                    },
                    new FWTextBlock
                    {
                        Text = "Inner ring: 1px stroke, 1px offset",
                        FontSize = 12,
                        Foreground = ThemeBrush("TextSecondary")
                    }
                }
            }
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 420,
            Children =
            {
                new FWLabel { Content = "Keyboard focus indicators" },
                focusInfo,
                buttonsPanel,
                textBox,
                checkBox,
                CreateInteractionButtonRow(
                    CreateInteractionActionButton(FluentIconRegular.Keyboard24, "Tab", () =>
                    {
                        output.Text = "Focus: Press Tab to navigate between focusable controls.";
                    }),
                    CreateInteractionActionButton(FluentIconRegular.Eye24, "Toggle", () =>
                    {
                        checkBox.IsChecked = !checkBox.IsChecked;
                        output.Text = $"Focus visual: {FormatOnOff(checkBox.IsChecked == true)}.";
                    })),
                CreateInteractionStatus(output)
            }
        };
    }

    private static FWButton CreateFocusButton(string text, FluentIconRegular icon)
    {
        var button = new FWButton
        {
            Content = CreateInteractionButtonContent(icon, text)
        };
        button.GotFocus += (s, e) =>
        {
            if (s is FWButton btn && btn.Content is FWStackPanel panel)
            {
                foreach (var child in panel.Children)
                {
                    if (child is FWTextBlock tb)
                    {
                        System.Diagnostics.Debug.WriteLine($"Focus: {tb.Text} button focused.");
                    }
                }
            }
        };
        return button;
    }

    private static UIElement CreateMaterialInteractionWorkbenchSample()
    {
        var output = CreateInteractionOutput("Workbench: LiquidGlass. Scroll, swipe, and splitter ready.");
        var viewer = new FWScrollViewer
        {
            Width = 245,
            Height = 116,
            VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            IsScrollBarAutoHideEnabled = true,
            Padding = new Thickness(10),
            Background = ThemeBrush("LayerFillColorDefaultBrush"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            Content = CreateScrollableItems("Task", 7)
        };
        var swipe = new FWSwipeControl
        {
            Width = 245,
            Height = 68,
            LeftItems = CreateSwipeItems(SwipeMode.Reveal,
                ("Pin", FluentIconRegular.Pin24, "SwipeItemBackground", BehaviorOnInvoked.RemainOpen)),
            RightItems = CreateSwipeItems(SwipeMode.Execute,
                ("Done", FluentIconRegular.CheckmarkCircle24, "SwipeItemBackgroundSecondary", BehaviorOnInvoked.Close)),
            Background = ThemeBrush("ControlBackground"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Content = CreateSwipeRow("Material row", "Swipe commands use Fluent item colors.")
        };
        var splitter = new FWGridSplitter
        {
            Width = 6,
            ResizeDirection = GridResizeDirection.Columns,
            ResizeBehavior = GridResizeBehavior.PreviousAndNext,
            KeyboardIncrement = 16,
            DragIncrement = 2,
            ShowsPreview = true
        };

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
                            CreateMaterialPreview("Scroll", viewer),
                            CreateMaterialPreview("Swipe", swipe)
                        }
                    },
                    CreateSplitterPreview(splitter, width: 490, height: 120),
                    CreateInteractionButtonRow(
                        CreateInteractionActionButton(FluentIconRegular.ArrowDown24, "Scroll", () =>
                        {
                            viewer.ScrollToVerticalOffset(viewer.ScrollableHeight);
                            output.Text = $"Workbench scroll: {viewer.ScrollableHeight}.";
                        }),
                        CreateInteractionActionButton(FluentIconRegular.Pin24, "Swipe", () =>
                        {
                            output.Text = $"Workbench swipe: {swipe.LeftItems?[0].Text}.";
                        }),
                        CreateInteractionActionButton(FluentIconRegular.SlideSettings24, "Preview", () =>
                        {
                            splitter.ShowsPreview = !splitter.ShowsPreview;
                            output.Text = $"Workbench splitter preview: {FormatOnOff(splitter.ShowsPreview)}.";
                        })),
                    CreateInteractionStatus(output)
                }
            }
        };
    }

    private static StackPanel CreateScrollableItems(string prefix, int count)
    {
        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 6
        };

        for (var index = 1; index <= count; index++)
        {
            stack.Children.Add(new FWBorder
            {
                Height = 28,
                Background = index % 2 == 0 ? ThemeBrush("ControlBackground") : ThemeBrush("SelectionBackgroundWeak"),
                BorderBrush = ThemeBrush("ControlBorder"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(8, 0, 8, 0),
                Child = new FWTextBlock
                {
                    Text = $"{prefix} item {index}",
                    Foreground = ThemeBrush("TextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            });
        }

        return stack;
    }

    private static SwipeItems CreateSwipeItems(SwipeMode mode, params (string Text, FluentIconRegular Icon, string BackgroundKey, BehaviorOnInvoked Behavior)[] items)
    {
        var swipeItems = new SwipeItems
        {
            Mode = mode
        };

        foreach (var item in items)
        {
            swipeItems.Add(new SwipeItem
            {
                Text = item.Text,
                IconSource = item.Icon.GetString(),
                Background = ThemeBrush(item.BackgroundKey),
                Foreground = ThemeBrush("SwipeItemForeground"),
                BehaviorOnInvoked = item.Behavior
            });
        }

        return swipeItems;
    }

    private static FWBorder CreateSwipeRow(string title, string detail)
    {
        return new FWBorder
        {
            Background = ThemeBrush("SwipeControlBackground"),
            Padding = new Thickness(14, 8, 14, 8),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 2,
                Children =
                {
                    new FWTextBlock
                    {
                        Text = title,
                        Foreground = ThemeBrush("TextPrimary")
                    },
                    new FWTextBlock
                    {
                        Text = detail,
                        Foreground = ThemeBrush("TextSecondary"),
                        FontSize = 12,
                        TextWrapping = TextWrapping.Wrap
                    }
                }
            }
        };
    }

    private static Grid CreateSplitterPreview(FWGridSplitter splitter, double width = 390, double height = 150)
    {
        var grid = new Grid
        {
            Width = width,
            Height = height,
            Background = ThemeBrush("SwipeControlBackground")
        };
        grid.ColumnDefinitions.Add(new ColumnDefinition
        {
            Width = new GridLength(150),
            MinWidth = 96
        });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(splitter.Width) });
        grid.ColumnDefinitions.Add(new ColumnDefinition
        {
            Width = new GridLength(1, GridUnitType.Star),
            MinWidth = 140
        });

        var left = CreateSplitterPane("Outline", "Theme tokens\nFW controls\nGallery states");
        Grid.SetColumn(left, 0);
        grid.Children.Add(left);

        Grid.SetColumn(splitter, 1);
        grid.Children.Add(splitter);

        var right = CreateSplitterPane("Preview", "Drag the splitter to resize this Fluent layout.");
        Grid.SetColumn(right, 2);
        grid.Children.Add(right);

        return grid;
    }

    private static FWBorder CreateSplitterPane(string title, string text)
    {
        return new FWBorder
        {
            Background = ThemeBrush("ControlBackground"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(12),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 8,
                Children =
                {
                    new FWTextBlock
                    {
                        Text = title,
                        Foreground = ThemeBrush("TextPrimary"),
                        FontSize = 14
                    },
                    new FWTextBlock
                    {
                        Text = text,
                        Foreground = ThemeBrush("TextSecondary"),
                        TextWrapping = TextWrapping.Wrap
                    }
                }
            }
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
                    Text = "Layered interaction surface",
                    FontSize = 15,
                    Foreground = ThemeBrush("TextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
    }

    private static FWBorder CreateInteractionExampleCard(FluentIconRegular icon, string title, string description, UIElement content)
    {
        return GallerySampleCard.Create(icon, title, description, content, code: CreateSampleCode(title));
    }

    private static string CreateSampleCode(string title)
    {
        return title switch
        {
            "FWScrollViewer" => "<FWScrollViewer VerticalScrollBarVisibility=\"Visible\" IsScrollBarAutoHideEnabled=\"False\" />",
            "FWSwipeControl" => "<FWSwipeControl>\n  <FWSwipeControl.LeftItems>\n    <SwipeItems Mode=\"Reveal\" />\n  </FWSwipeControl.LeftItems>\n</FWSwipeControl>",
            "FWGridSplitter" => "<FWGridSplitter ResizeDirection=\"Columns\" ResizeBehavior=\"PreviousAndNext\" KeyboardIncrement=\"12\" />",
            "Focus visual styles" => "// Dual-ring focus visual\nvar style = FocusVisualStyles.CreateFluentFocusVisualStyle();\nbutton.FocusVisualStyle = style;",
            _ => "<FWFluentMaterialSurface MaterialKind=\"LiquidGlass\">\n  <FWScrollViewer />\n  <FWSwipeControl />\n  <FWGridSplitter />\n</FWFluentMaterialSurface>"
        };
    }

    private static FWWrapPanel CreateInteractionButtonRow(params FWButton[] buttons)
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

    private static FWButton CreateInteractionActionButton(FluentIconRegular icon, string text, Action action)
    {
        var button = new FWButton
        {
            Content = CreateInteractionButtonContent(icon, text)
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static FWStackPanel CreateInteractionButtonContent(FluentIconRegular icon, string text)
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

    private static TextBlock CreateInteractionOutput(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        };
    }

    private static FWBorder CreateInteractionStatus(TextBlock status)
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
                        CreateIcon(FluentIconRegular.CursorClick24, 24, ThemeBrush("TextPrimary")),
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
