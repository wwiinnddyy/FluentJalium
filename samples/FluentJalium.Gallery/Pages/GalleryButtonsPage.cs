using FluentJalium.Icon;
using FluentJalium.Gallery.Controls;
using FluentJalium.Gallery.Resources;
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
        var panel = CreateSection(Strings.Buttons_Title);
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        var buttonSurfaceOutput = CreateButtonOutput(Strings.Output_Ready);
        examples.Children.Add(CreateButtonExampleCard(
            FluentIconRegular.ControlButton24,
            Strings.Buttons_Surface_Title,
            Strings.Buttons_Surface_Description,
            CreateButtonSurfaceSample(buttonSurfaceOutput),
            output: buttonSurfaceOutput,
            options: CreateButtonOptions()));

        var splitButtonOutput = CreateButtonOutput(Strings.Output_Ready);
        examples.Children.Add(CreateButtonExampleCard(
            FluentIconRegular.ChevronDown24,
            Strings.Buttons_Split_Title,
            Strings.Buttons_Split_Description,
            CreateSplitCommandButtonsSample(splitButtonOutput),
            output: splitButtonOutput,
            options: CreateSplitButtonOptions()));

        var commandBarOutput = CreateButtonOutput(Strings.Output_Ready);
        examples.Children.Add(CreateButtonExampleCard(
            FluentIconRegular.MoreHorizontal24,
            Strings.Buttons_CommandBar_Title,
            Strings.Buttons_CommandBar_Description,
            CreateCommandBarSample(commandBarOutput),
            output: commandBarOutput,
            options: CreateCommandBarOptions()));

        panel.Children.Add(examples);
        return panel;
    }

    private static UIElement CreateButtonSurfaceSample(FWTextBlock output)
    {
        var repeatCount = 0;
        var repeatButton = new FWRepeatButton
        {
            Content = CreateButtonContent(FluentIconRegular.Play24, Strings.Button_Label_Repeat),
            MinWidth = 112
        };
        repeatButton.Click += (_, _) =>
        {
            repeatCount++;
            output.Text = string.Format(Strings.Output_Repeat, repeatCount);
        };

        var dropDown = new FWDropDownButton
        {
            Content = CreateButtonContent(FluentIconRegular.ChevronDown24, Strings.Button_Label_Menu),
            MinWidth = 120,
            Flyout = CreateSampleFlyout()
        };

        var standardButton = CreateSurfaceButton(FluentIconRegular.ControlButton24, Strings.Button_Label_Button, () => output.Text = Strings.Output_ButtonClicked);

        return new FWWrapPanel
        {
            HorizontalSpacing = 12,
            VerticalSpacing = 8,
            Children =
            {
                standardButton,
                repeatButton,
                new FWHyperlinkButton
                {
                    Content = CreateButtonContent(FluentIconRegular.Open24, Strings.Button_Label_Link),
                    MinWidth = 100
                },
                dropDown,
                new FWButton
                {
                    Content = CreateButtonContent(FluentIconRegular.DismissCircle24, Strings.Button_Label_Disabled),
                    IsEnabled = false,
                    MinWidth = 110
                }
            }
        };
    }

    private static UIElement CreateButtonOptions()
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                new FWTextBlock
                {
                    Text = Strings.Options_MinWidth,
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = ThemeBrush("TextPrimary")
                },
                new TextBox
                {
                    Text = "112",
                    MinWidth = 140,
                    PlaceholderText = "e.g., 100"
                },
                new FWTextBlock
                {
                    Text = Strings.Options_State,
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = ThemeBrush("TextPrimary"),
                    Margin = new Thickness(0, 8, 0, 0)
                },
                new CheckBox
                {
                    Content = Strings.Options_IsEnabled,
                    IsChecked = true
                },
                new CheckBox
                {
                    Content = Strings.Options_IsVisible,
                    IsChecked = true
                }
            }
        };
    }

    private static UIElement CreateSplitCommandButtonsSample(FWTextBlock output)
    {
        var splitButton = new FWSplitButton
        {
            Content = CreateButtonContent(FluentIconRegular.Save24, Strings.Button_Label_Split),
            Width = 140,
            Flyout = CreateSampleFlyout()
        };

        var toggleSplitButton = new FWToggleSplitButton
        {
            Content = CreateButtonContent(FluentIconRegular.Pin24, Strings.Button_Label_Toggle),
            Width = 140,
            IsChecked = true,
            Flyout = CreateSampleFlyout()
        };

        splitButton.Click += (_, _) => output.Text = Strings.Output_SplitClicked;
        toggleSplitButton.IsCheckedChanged += (_, e) =>
        {
            output.Text = string.Format(Strings.Output_Checked, FormatOnOff(e.NewValue));
        };

        return new FWWrapPanel
        {
            HorizontalSpacing = 12,
            VerticalSpacing = 8,
            Children =
            {
                splitButton,
                toggleSplitButton,
                new FWDropDownButton
                {
                    Content = CreateButtonContent(FluentIconRegular.ChevronDown24, Strings.Button_Label_Menu),
                    Width = 120,
                    Flyout = CreateSampleFlyout()
                }
            }
        };
    }

    private static UIElement CreateCommandBarSample(FWTextBlock output)
    {
        var commandBar = new FWCommandBar
        {
            Width = 480,
            DefaultLabelPosition = CommandBarDefaultLabelPosition.Bottom
        };

        commandBar.PrimaryCommands.Add(CreateAppBarButton(Strings.Command_Add, FluentIconRegular.Add24, output));
        commandBar.PrimaryCommands.Add(CreateAppBarButton(Strings.Command_Edit, FluentIconRegular.Edit24, output));
        commandBar.PrimaryCommands.Add(CreateAppBarButton(Strings.Command_Share, FluentIconRegular.Share24, output));
        commandBar.PrimaryCommands.Add(new FWAppBarSeparator());
        commandBar.PrimaryCommands.Add(CreateAppBarToggleButton(Strings.Command_Pin, FluentIconRegular.Pin24, output, isChecked: true));

        commandBar.SecondaryCommands.Add(CreateAppBarButton(Strings.Command_Settings, FluentIconRegular.Settings24, output));
        commandBar.SecondaryCommands.Add(CreateAppBarButton(Strings.Command_Open, FluentIconRegular.Open24, output));

        commandBar.Opening += (_, _) => output.Text = Strings.Output_Opening;
        commandBar.Opened += (_, _) => output.Text = Strings.Output_Opened;
        commandBar.Closed += (_, _) => output.Text = Strings.Output_Closed;

        return commandBar;
    }

    private static UIElement CreateSplitButtonOptions()
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                new FWTextBlock
                {
                    Text = Strings.Options_Width,
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = ThemeBrush("TextPrimary")
                },
                new TextBox
                {
                    Text = "140",
                    MinWidth = 140,
                    PlaceholderText = "e.g., 150"
                },
                new FWTextBlock
                {
                    Text = Strings.Options_State,
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = ThemeBrush("TextPrimary"),
                    Margin = new Thickness(0, 8, 0, 0)
                },
                new CheckBox
                {
                    Content = Strings.Options_IsChecked,
                    IsChecked = true
                },
                new CheckBox
                {
                    Content = Strings.Options_IsEnabled,
                    IsChecked = true
                }
            }
        };
    }

    private static UIElement CreateCommandBarOptions()
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                new FWTextBlock
                {
                    Text = Strings.Options_LabelPosition,
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = ThemeBrush("TextPrimary")
                },
                new ComboBox
                {
                    SelectedIndex = 1,
                    MinWidth = 140,
                    Items =
                    {
                        Strings.Options_LabelPosition_Right,
                        Strings.Options_LabelPosition_Bottom,
                        Strings.Options_LabelPosition_Collapsed
                    }
                },
                new FWTextBlock
                {
                    Text = Strings.Options_State,
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = ThemeBrush("TextPrimary"),
                    Margin = new Thickness(0, 8, 0, 0)
                },
                new CheckBox
                {
                    Content = Strings.Options_IsOpen,
                    IsChecked = false
                },
                new CheckBox
                {
                    Content = Strings.Options_IsEnabled,
                    IsChecked = true
                }
            }
        };
    }

    private static FWBorder CreateButtonExampleCard(
        FluentIconRegular icon,
        string title,
        string description,
        UIElement content,
        UIElement? output = null,
        UIElement? options = null)
    {
        return GallerySampleCard.Create(
            icon,
            title,
            description,
            content,
            output: output,
            options: options,
            code: CreateSampleCode(title));
    }

    private static string CreateSampleCode(string title)
    {
        return title switch
        {
            "Button surfaces" => "<FWButton Content=\"Button\" />\n<FWRepeatButton Content=\"Repeat\" />\n<FWHyperlinkButton Content=\"Link\" />",
            "Split command buttons" => "<FWSplitButton Content=\"Split\" />\n<FWToggleSplitButton Content=\"Toggle\" IsChecked=\"True\" />\n<FWDropDownButton Content=\"Menu\" />",
            "FWCommandBar" => "<FWCommandBar DefaultLabelPosition=\"Bottom\">\n  <FWAppBarButton Label=\"Add\" />\n  <FWAppBarToggleButton Label=\"Pin\" />\n</FWCommandBar>",
            _ => "<FWButton Content=\"Button\" />"
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

    private static FWAppBarButton CreateAppBarButton(string label, FluentIconRegular icon, FWTextBlock output)
    {
        var button = new FWAppBarButton
        {
            Label = label,
            Icon = CreateIcon(icon, 16)
        };
        button.Click += (_, _) => output.Text = string.Format(Strings.Command_Clicked, label);
        return button;
    }

    private static FWAppBarToggleButton CreateAppBarToggleButton(string label, FluentIconRegular icon, FWTextBlock output, bool isChecked = false)
    {
        var button = new FWAppBarToggleButton
        {
            Label = label,
            Icon = CreateIcon(icon, 16),
            IsChecked = isChecked
        };
        button.Click += (_, _) => output.Text = $"{label}: {FormatOnOff(button.IsChecked == true)}";
        return button;
    }

    private static FWMenuFlyout CreateSampleFlyout()
    {
        var flyout = new FWMenuFlyout();
        flyout.Items.Add(new FWMenuFlyoutItem { Text = Strings.Command_Create, Icon = CreateIcon(FluentIconRegular.Add24, 16) });
        flyout.Items.Add(new FWMenuFlyoutItem { Text = Strings.Command_Open, Icon = CreateIcon(FluentIconRegular.Open24, 16) });
        flyout.Items.Add(new FWMenuFlyoutSeparator());
        flyout.Items.Add(new FWMenuFlyoutItem { Text = Strings.Command_Export, Icon = CreateIcon(FluentIconRegular.ArrowDownload24, 16) });
        return flyout;
    }

    private static string FormatOnOff(bool value)
    {
        return value ? "on" : "off";
    }

    private static FWTextBlock CreateButtonOutput(string initialText)
    {
        return new FWTextBlock
        {
            Text = initialText,
            FontSize = 12,
            Foreground = ThemeBrush("TextPrimary"),
            TextWrapping = TextWrapping.Wrap
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

    private static Brush ThemeBrush(string key)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Brush brush)
        {
            return brush;
        }

        return new SolidColorBrush(Colors.Transparent);
    }
}
