using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Media;
using FWAppBarButton = FluentJalium.Controls.FWAppBarButton;
using FWAppBarSeparator = FluentJalium.Controls.FWAppBarSeparator;
using FWAppBarToggleButton = FluentJalium.Controls.FWAppBarToggleButton;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWButton = FluentJalium.Controls.FWButton;
using FWCommandBar = FluentJalium.Controls.FWCommandBar;
using FWDropDownButton = FluentJalium.Controls.FWDropDownButton;
using FWFluentMaterialKind = FluentJalium.Controls.FWFluentMaterialKind;
using FWFluentMaterialSurface = FluentJalium.Controls.FWFluentMaterialSurface;
using FWHyperlinkButton = FluentJalium.Controls.FWHyperlinkButton;
using FWMenuFlyout = FluentJalium.Controls.FWMenuFlyout;
using FWMenuFlyoutItem = FluentJalium.Controls.FWMenuFlyoutItem;
using FWMenuFlyoutSeparator = FluentJalium.Controls.FWMenuFlyoutSeparator;
using FWRepeatButton = FluentJalium.Controls.FWRepeatButton;
using FWSeparator = FluentJalium.Controls.FWSeparator;
using FWSplitButton = FluentJalium.Controls.FWSplitButton;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWToggleSplitButton = FluentJalium.Controls.FWToggleSplitButton;
using FWToolBar = FluentJalium.Controls.FWToolBar;
using FWToolBarTray = FluentJalium.Controls.FWToolBarTray;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;

namespace FluentJalium.Gallery.Pages;

internal sealed class GalleryButtonsPage
{
    public UIElement CreateContent()
    {
        var panel = CreateSection("Buttons");
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        examples.Children.Add(CreateButtonExampleCard(
            FluentIconRegular.ControlButton24,
            "Button surfaces",
            "Button, RepeatButton, HyperlinkButton, DropDownButton, and disabled command states.",
            CreateButtonSurfaceSample()));
        examples.Children.Add(CreateButtonExampleCard(
            FluentIconRegular.ChevronDown24,
            "Split command buttons",
            "Split, toggle split, and drop-down command surfaces with the same flyout menu affordance.",
            CreateSplitCommandButtonsSample()));
        examples.Children.Add(CreateButtonExampleCard(
            FluentIconRegular.MoreHorizontal24,
            "FWCommandBar",
            "Primary and secondary commands with labels, overflow state, toggle commands, and live output.",
            CreateCommandBarSample()));
        examples.Children.Add(CreateButtonExampleCard(
            FluentIconRegular.TextBold24,
            "FWToolBar and FWToolBarTray",
            "Document and formatting command groups hosted in a tray with band, index, lock, and overflow metadata.",
            CreateToolBarCommandSample()));
        examples.Children.Add(CreateButtonExampleCard(
            FluentIconRegular.LayerDiagonalSparkle24,
            "Material command surface",
            "Buttons, split commands, CommandBar, and ToolBar remain readable on a LiquidGlass command deck.",
            CreateMaterialCommandSurfaceSample()));

        panel.Children.Add(examples);
        return panel;
    }

    private static UIElement CreateButtonSurfaceSample()
    {
        var output = CreateButtonOutput("Buttons: ready");
        var repeatCount = 0;
        var repeatButton = new FWRepeatButton
        {
            Content = CreateButtonContent(FluentIconRegular.Play24, "Repeat"),
            MinWidth = 112
        };
        repeatButton.Click += (_, _) =>
        {
            repeatCount++;
            output.Text = $"Repeat invoked: {repeatCount}";
        };

        var dropDown = new FWDropDownButton
        {
            Content = CreateButtonContent(FluentIconRegular.ChevronDown24, "Drop down"),
            MinWidth = 132,
            Flyout = CreateSampleFlyout()
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                new FWWrapPanel
                {
                    HorizontalSpacing = 12,
                    VerticalSpacing = 8,
                    Children =
                    {
                        CreateSurfaceButton(FluentIconRegular.ControlButton24, "Button", () => output.Text = "Button clicked"),
                        repeatButton,
                        new FWHyperlinkButton
                        {
                            Content = CreateButtonContent(FluentIconRegular.Open24, "Hyperlink"),
                            MinWidth = 128
                        },
                        dropDown,
                        new FWButton
                        {
                            Content = CreateButtonContent(FluentIconRegular.DismissCircle24, "Disabled"),
                            IsEnabled = false
                        }
                    }
                },
                CreateButtonButtonRow(
                    CreateButtonActionButton(FluentIconRegular.ControlButton24, "Default", () => output.Text = "Button state: default"),
                    CreateButtonActionButton(FluentIconRegular.ChevronDown24, "Open menu", () =>
                    {
                        dropDown.Flyout?.ShowAt(dropDown);
                        output.Text = "DropDownButton flyout requested";
                    }),
                    CreateButtonActionButton(FluentIconRegular.InfoSparkle24, "Output", () => output.Text = "Buttons keep icon and text aligned.")),
                CreateButtonStatus(output)
            }
        };
    }

    private static UIElement CreateSplitCommandButtonsSample()
    {
        var output = CreateButtonOutput("Split commands: ready");
        var splitButton = new FWSplitButton
        {
            Content = CreateButtonContent(FluentIconRegular.Save24, "Split"),
            Width = 150,
            Flyout = CreateSampleFlyout()
        };
        var toggleSplitButton = new FWToggleSplitButton
        {
            Content = CreateButtonContent(FluentIconRegular.Pin24, "Toggle split"),
            Width = 170,
            IsChecked = true,
            Flyout = CreateSampleFlyout()
        };
        splitButton.Click += (_, _) => output.Text = "Primary split command invoked";
        toggleSplitButton.IsCheckedChanged += (_, e) =>
        {
            output.Text = $"Toggle split checked: {FormatOnOff(e.NewValue)}";
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                new FWWrapPanel
                {
                    HorizontalSpacing = 12,
                    VerticalSpacing = 8,
                    Children =
                    {
                        splitButton,
                        toggleSplitButton,
                        new FWDropDownButton
                        {
                            Content = CreateButtonContent(FluentIconRegular.ChevronDown24, "Drop down"),
                            Width = 150,
                            Flyout = CreateSampleFlyout()
                        },
                        new FWButton
                        {
                            Content = CreateButtonContent(FluentIconRegular.DismissCircle24, "Disabled"),
                            IsEnabled = false
                        }
                    }
                },
                CreateButtonButtonRow(
                    CreateButtonActionButton(FluentIconRegular.Save24, "Primary", () => output.Text = "Primary split command invoked"),
                    CreateButtonActionButton(FluentIconRegular.Pin24, "Toggle", () =>
                    {
                        toggleSplitButton.IsChecked = !toggleSplitButton.IsChecked;
                        output.Text = $"Toggle split checked: {FormatOnOff(toggleSplitButton.IsChecked == true)}";
                    }),
                    CreateButtonActionButton(FluentIconRegular.ChevronDown24, "Flyout", () =>
                    {
                        splitButton.Flyout?.ShowAt(splitButton);
                        output.Text = "Split flyout requested";
                    })),
                CreateButtonStatus(output)
            }
        };
    }

    private static UIElement CreateCommandBarSample()
    {
        var output = CreateButtonOutput("CommandBar: closed, 2 secondary commands");
        var commandBar = new FWCommandBar
        {
            Width = 500,
            DefaultLabelPosition = CommandBarDefaultLabelPosition.Bottom
        };
        commandBar.PrimaryCommands.Add(CreateAppBarButton("Add", FluentIconRegular.Add24, output));
        commandBar.PrimaryCommands.Add(CreateAppBarButton("Edit", FluentIconRegular.Edit24, output));
        commandBar.PrimaryCommands.Add(CreateAppBarButton("Share", FluentIconRegular.Share24, output));
        commandBar.PrimaryCommands.Add(new FWAppBarSeparator());
        commandBar.PrimaryCommands.Add(CreateAppBarToggleButton("Pin", FluentIconRegular.Pin24, output, isChecked: true));
        commandBar.SecondaryCommands.Add(CreateAppBarButton("Settings", FluentIconRegular.Settings24, output));
        commandBar.SecondaryCommands.Add(CreateAppBarButton("Open", FluentIconRegular.Open24, output));
        commandBar.Opening += (_, _) => output.Text = $"CommandBar: opening, {commandBar.SecondaryCommands.Count} secondary commands";
        commandBar.Opened += (_, _) => output.Text = $"CommandBar: open, {commandBar.SecondaryCommands.Count} secondary commands";
        commandBar.Closing += (_, _) => output.Text = "CommandBar: closing";
        commandBar.Closed += (_, _) => output.Text = $"CommandBar: closed, {commandBar.SecondaryCommands.Count} secondary commands";

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                commandBar,
                CreateButtonButtonRow(
                    CreateButtonActionButton(FluentIconRegular.ChevronDown24, "Open", () => commandBar.IsOpen = true),
                    CreateButtonActionButton(FluentIconRegular.DismissCircle24, "Close", () => commandBar.IsOpen = false),
                    CreateButtonActionButton(FluentIconRegular.TextBold24, "Labels", () =>
                    {
                        commandBar.DefaultLabelPosition = commandBar.DefaultLabelPosition == CommandBarDefaultLabelPosition.Bottom
                            ? CommandBarDefaultLabelPosition.Collapsed
                            : CommandBarDefaultLabelPosition.Bottom;
                        commandBar.InvalidateMeasure();
                        output.Text = $"DefaultLabelPosition: {commandBar.DefaultLabelPosition}";
                    }),
                    CreateButtonActionButton(FluentIconRegular.Add24, "Add secondary", () =>
                    {
                        commandBar.SecondaryCommands.Add(CreateAppBarButton("Export", FluentIconRegular.ArrowDownload24, output));
                        output.Text = $"Secondary commands: {commandBar.SecondaryCommands.Count}";
                    }),
                    CreateButtonActionButton(FluentIconRegular.DismissCircle24, "Remove secondary", () =>
                    {
                        if (commandBar.SecondaryCommands.Count > 0)
                        {
                            commandBar.SecondaryCommands.RemoveAt(commandBar.SecondaryCommands.Count - 1);
                        }

                        output.Text = $"Secondary commands: {commandBar.SecondaryCommands.Count}";
                    })),
                CreateButtonStatus(output)
            }
        };
    }

    private static UIElement CreateToolBarCommandSample()
    {
        var output = CreateButtonOutput("ToolBarTray: unlocked, horizontal, 2 bands");
        var documentBar = new FWToolBar
        {
            Header = "Document",
            Band = 0,
            BandIndex = 0,
            Margin = new Thickness(0, 0, 8, 8)
        };
        documentBar.Items.Add(CreateToolBarButton(FluentIconRegular.Save24, "Save", output));
        documentBar.Items.Add(CreateToolBarButton(FluentIconRegular.Share24, "Share", output));
        documentBar.Items.Add(CreateToolBarSeparator());
        var exportButton = CreateToolBarButton(FluentIconRegular.ArrowDownload24, "Export", output);
        Jalium.UI.Controls.ToolBar.SetOverflowMode(exportButton, OverflowMode.Always);
        documentBar.Items.Add(exportButton);

        var formattingBar = new FWToolBar
        {
            Header = "Formatting",
            Band = 1,
            BandIndex = 0,
            Margin = new Thickness(0, 0, 8, 8)
        };
        formattingBar.Items.Add(CreateToolBarButton(FluentIconRegular.TextBold24, "Bold", output));
        formattingBar.Items.Add(CreateToolBarButton(FluentIconRegular.TextItalic24, "Italic", output));
        formattingBar.Items.Add(CreateToolBarButton(FluentIconRegular.TextUnderline24, "Underline", output));

        var tray = new FWToolBarTray
        {
            Background = ThemeBrush("ToolBarTrayBackground"),
            Orientation = Orientation.Horizontal
        };
        tray.ToolBars.Add(documentBar);
        tray.ToolBars.Add(formattingBar);

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                tray,
                CreateButtonButtonRow(
                    CreateButtonActionButton(FluentIconRegular.Pin24, "Lock tray", () =>
                    {
                        tray.IsLocked = !tray.IsLocked;
                        output.Text = $"ToolBarTray locked: {FormatOnOff(tray.IsLocked)}";
                    }),
                    CreateButtonActionButton(FluentIconRegular.TextUnderline24, "Vertical", () =>
                    {
                        tray.Orientation = tray.Orientation == Orientation.Horizontal
                            ? Orientation.Vertical
                            : Orientation.Horizontal;
                        output.Text = $"ToolBarTray orientation: {tray.Orientation}";
                    }),
                    CreateButtonActionButton(FluentIconRegular.MoreHorizontal24, "Overflow", () =>
                    {
                        documentBar.IsOverflowOpen = !documentBar.IsOverflowOpen;
                        output.Text = $"Document toolbar overflow open: {FormatOnOff(documentBar.IsOverflowOpen)}";
                    })),
                CreateButtonStatus(output)
            }
        };
    }

    private static UIElement CreateMaterialCommandSurfaceSample()
    {
        var output = CreateButtonOutput("Material commands: LiquidGlass, command bar closed, toolbar unlocked.");
        var splitButton = new FWSplitButton
        {
            Content = CreateButtonContent(FluentIconRegular.Save24, "Save"),
            Width = 132,
            Flyout = CreateSampleFlyout()
        };
        var toggleSplitButton = new FWToggleSplitButton
        {
            Content = CreateButtonContent(FluentIconRegular.Pin24, "Pinned"),
            Width = 136,
            IsChecked = true,
            Flyout = CreateSampleFlyout()
        };
        var commandBar = new FWCommandBar
        {
            Width = 500,
            DefaultLabelPosition = CommandBarDefaultLabelPosition.Bottom
        };
        commandBar.PrimaryCommands.Add(CreateAppBarButton("Add", FluentIconRegular.Add24, output));
        commandBar.PrimaryCommands.Add(CreateAppBarButton("Edit", FluentIconRegular.Edit24, output));
        commandBar.PrimaryCommands.Add(CreateAppBarToggleButton("Pin", FluentIconRegular.Pin24, output, isChecked: true));
        commandBar.SecondaryCommands.Add(CreateAppBarButton("Settings", FluentIconRegular.Settings24, output));

        var toolBar = new FWToolBar
        {
            Header = "Format",
            Band = 0,
            BandIndex = 0
        };
        toolBar.Items.Add(CreateToolBarButton(FluentIconRegular.TextBold24, "Bold", output));
        toolBar.Items.Add(CreateToolBarButton(FluentIconRegular.TextItalic24, "Italic", output));
        toolBar.Items.Add(CreateToolBarSeparator());
        toolBar.Items.Add(CreateToolBarButton(FluentIconRegular.ArrowDownload24, "Export", output));
        var tray = new FWToolBarTray
        {
            Background = ThemeBrush("ToolBarTrayBackground"),
            Orientation = Orientation.Horizontal
        };
        tray.ToolBars.Add(toolBar);

        splitButton.Click += (_, _) => output.Text = "Material split command invoked";
        toggleSplitButton.IsCheckedChanged += (_, e) =>
        {
            output.Text = $"Material toggle split checked: {FormatOnOff(e.NewValue)}";
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
                        HorizontalSpacing = 10,
                        VerticalSpacing = 8,
                        Children =
                        {
                            CreateSurfaceButton(FluentIconRegular.ControlButton24, "Run", () => output.Text = "Material run command clicked"),
                            splitButton,
                            toggleSplitButton,
                            new FWDropDownButton
                            {
                                Content = CreateButtonContent(FluentIconRegular.MoreHorizontal24, "More"),
                                Width = 124,
                                Flyout = CreateSampleFlyout()
                            }
                        }
                    },
                    commandBar,
                    tray,
                    CreateButtonButtonRow(
                        CreateButtonActionButton(FluentIconRegular.ChevronDown24, "CommandBar", () =>
                        {
                            commandBar.IsOpen = !commandBar.IsOpen;
                            output.Text = $"Material CommandBar open: {FormatOnOff(commandBar.IsOpen)}";
                        }),
                        CreateButtonActionButton(FluentIconRegular.Pin24, "Lock toolbar", () =>
                        {
                            tray.IsLocked = !tray.IsLocked;
                            output.Text = $"Material toolbar locked: {FormatOnOff(tray.IsLocked)}";
                        }),
                        CreateButtonActionButton(FluentIconRegular.TextBold24, "Labels", () =>
                        {
                            commandBar.DefaultLabelPosition = commandBar.DefaultLabelPosition == CommandBarDefaultLabelPosition.Bottom
                                ? CommandBarDefaultLabelPosition.Collapsed
                                : CommandBarDefaultLabelPosition.Bottom;
                            output.Text = $"Material labels: {commandBar.DefaultLabelPosition}";
                        })),
                    CreateButtonStatus(output)
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
                    Text = "Layered command deck",
                    FontSize = 15,
                    Foreground = ThemeBrush("TextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
    }

    private static FWBorder CreateButtonExampleCard(FluentIconRegular icon, string title, string description, UIElement content)
    {
        return new FWBorder
        {
            Width = 570,
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

    private static FWButton CreateSurfaceButton(FluentIconRegular icon, string text, Action action)
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

    private static FWWrapPanel CreateButtonButtonRow(params FWButton[] buttons)
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

    private static FWButton CreateButtonActionButton(FluentIconRegular icon, string text, Action action)
    {
        var button = new FWButton
        {
            Content = CreateButtonContent(icon, text)
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static TextBlock CreateButtonOutput(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        };
    }

    private static FWBorder CreateButtonStatus(TextBlock status)
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

    private static FWAppBarButton CreateAppBarButton(string label, FluentIconRegular icon, TextBlock output)
    {
        var button = new FWAppBarButton
        {
            Label = label,
            Icon = CreateIcon(icon)
        };
        button.Click += (_, _) => output.Text = $"Command invoked: {label}";
        return button;
    }

    private static FWAppBarToggleButton CreateAppBarToggleButton(string label, FluentIconRegular icon, TextBlock output, bool isChecked = false)
    {
        var button = new FWAppBarToggleButton
        {
            Label = label,
            Icon = CreateIcon(icon),
            IsChecked = isChecked
        };
        button.Checked += (_, _) => output.Text = $"{label}: on";
        button.Unchecked += (_, _) => output.Text = $"{label}: off";
        return button;
    }

    private static FWButton CreateToolBarButton(FluentIconRegular icon, string label, TextBlock output)
    {
        var button = new FWButton
        {
            Content = CreateButtonContent(icon, label),
            MinWidth = 86
        };
        button.Click += (_, _) => output.Text = $"ToolBar command: {label}. OverflowMode: {Jalium.UI.Controls.ToolBar.GetOverflowMode(button)}";
        return button;
    }

    private static FWSeparator CreateToolBarSeparator()
    {
        return new FWSeparator
        {
            Orientation = Orientation.Vertical,
            Height = 24,
            Margin = new Thickness(4, 4, 4, 4),
            StrokeBrush = ThemeBrush("ToolBarSeparatorForeground")
        };
    }

    private static FWMenuFlyout CreateSampleFlyout()
    {
        var flyout = new FWMenuFlyout();
        flyout.Items.Add(new FWMenuFlyoutItem { Text = "Create", Icon = IconGlyph(FluentIconRegular.Add24) });
        flyout.Items.Add(new FWMenuFlyoutItem { Text = "Open", Icon = IconGlyph(FluentIconRegular.Open24) });
        flyout.Items.Add(new FWMenuFlyoutSeparator());
        flyout.Items.Add(new FWMenuFlyoutItem { Text = "Export", Icon = IconGlyph(FluentIconRegular.ArrowDownload24) });
        return flyout;
    }

    private static string FormatOnOff(bool value)
    {
        return value ? "on" : "off";
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
                        CreateIcon(FluentIconRegular.ControlButton24, 24, ThemeBrush("TextPrimary")),
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

    private static string IconGlyph(FluentIconRegular icon)
    {
        return icon.GetString();
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
