using FluentJalium.Icon;
using FluentJalium.Gallery.Controls;
using FluentJalium.Gallery.Resources;
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
        var panel = CreateSection(Strings.Switches_Title);
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        examples.Children.Add(CreateSwitchExampleCard(
            FluentIconRegular.ToggleMultiple24,
            Strings.Switches_ToggleButton,
            Strings.Switches_ToggleButton_Desc,
            CreateToggleButtonStateSample()));
        examples.Children.Add(CreateSwitchExampleCard(
            FluentIconRegular.Power24,
            Strings.Switches_ToggleSwitch,
            Strings.Switches_ToggleSwitch_Desc,
            CreateToggleSwitchStateSample()));
        examples.Children.Add(CreateSwitchExampleCard(
            FluentIconRegular.CursorClick24,
            Strings.Switches_Events,
            Strings.Switches_Events_Desc,
            CreateInteractiveSwitchSample()));
        examples.Children.Add(CreateSwitchExampleCard(
            FluentIconRegular.LayerDiagonalSparkle24,
            Strings.Switches_MaterialRow,
            Strings.Switches_MaterialRow_Desc,
            CreateMaterialSwitchSample()));

        panel.Children.Add(examples);
        return panel;
    }

    private static UIElement CreateToggleButtonStateSample()
    {
        var output = CreateSwitchOutputText(Strings.Switches_StateWaiting);
        var indeterminate = new FWToggleButton
        {
            Content = CreateButtonContent(FluentIconRegular.Flash24, Strings.Switches_Mixed),
            Density = FWSwitchDensity.Comfortable,
            IsThreeState = true,
            IsChecked = null
        };
        indeterminate.Checked += (_, _) => output.Text = Strings.Switches_StateChecked;
        indeterminate.Unchecked += (_, _) => output.Text = Strings.Switches_StateUnchecked;
        indeterminate.Indeterminate += (_, _) => output.Text = Strings.Switches_StateIndeterminate;

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Children =
            {
                CreateSwitchButtonRow(
                    new FWToggleButton { Content = CreateButtonContent(FluentIconRegular.DismissCircle24, Strings.Switches_Off), Density = FWSwitchDensity.Compact },
                    new FWToggleButton { Content = CreateButtonContent(FluentIconRegular.CheckmarkCircle24, Strings.Switches_On), Density = FWSwitchDensity.Comfortable, IsChecked = true },
                    indeterminate,
                    new FWToggleButton { Content = CreateButtonContent(FluentIconRegular.Pause24, Strings.Button_Label_Disabled), Density = FWSwitchDensity.Spacious, IsChecked = true, IsEnabled = false }),
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
                    Header = Strings.Switches_Default,
                    Density = FWSwitchDensity.Compact,
                    OffContent = Strings.Switches_Off,
                    OnContent = Strings.Switches_On
                },
                new FWToggleSwitch
                {
                    Header = Strings.Switches_On,
                    Density = FWSwitchDensity.Comfortable,
                    IsOn = true,
                    OffContent = Strings.Switches_Off,
                    OnContent = Strings.Switches_On
                },
                new FWToggleSwitch
                {
                    Header = Strings.Switches_Status,
                    Description = Strings.Switches_ComfortableRow,
                    Density = FWSwitchDensity.Comfortable,
                    IsOn = true,
                    OffContent = Strings.Switches_Paused,
                    OnContent = Strings.Switches_Running
                },
                new FWToggleSwitch
                {
                    Header = Strings.Button_Label_Disabled,
                    Description = Strings.Switches_SpaciousDisabledRow,
                    Density = FWSwitchDensity.Spacious,
                    IsOn = true,
                    IsEnabled = false,
                    OffContent = Strings.Switches_Off,
                    OnContent = Strings.Switches_On
                }
            }
        };
    }

    private static UIElement CreateInteractiveSwitchSample()
    {
        var toggleSwitch = new FWToggleSwitch
        {
            Header = Strings.Switches_LiveSetting,
            Description = Strings.Switches_LiveSetting_Desc,
            Density = FWSwitchDensity.Comfortable,
            OffContent = Strings.Switches_NotificationsOff,
            OnContent = Strings.Switches_NotificationsOn
        };
        var output = CreateSwitchOutputText(string.Format(Strings.Switches_LiveOutput, Strings.Switches_Off, FormatDensity(toggleSwitch.Density)));

        toggleSwitch.Toggled += (_, _) =>
        {
            output.Text = string.Format(Strings.Switches_LiveOutput, toggleSwitch.IsOn ? Strings.Switches_On : Strings.Switches_Off, FormatDensity(toggleSwitch.Density));
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Children =
            {
                toggleSwitch,
                CreateSwitchButtonRow(
                    CreateSwitchActionButton(FluentIconRegular.Play24, Strings.Switches_TurnOn, () => toggleSwitch.IsOn = true),
                    CreateSwitchActionButton(FluentIconRegular.DismissCircle24, Strings.Switches_TurnOff, () => toggleSwitch.IsOn = false),
                    CreateSwitchActionButton(FluentIconRegular.Keyboard24, Strings.Switches_Flip, () => toggleSwitch.IsOn = !toggleSwitch.IsOn),
                    CreateSwitchActionButton(FluentIconRegular.TextDensity24, Strings.Switches_Density, () =>
                    {
                        toggleSwitch.Density = NextDensity(toggleSwitch.Density);
                        output.Text = string.Format(Strings.Switches_LiveOutput, toggleSwitch.IsOn ? Strings.Switches_On : Strings.Switches_Off, FormatDensity(toggleSwitch.Density));
                    })),
                CreateSwitchStatus(output)
            }
        };
    }

    private static UIElement CreateMaterialSwitchSample()
    {
        return new FWFluentMaterialSurface
        {
            Width = double.NaN,
            MaxWidth = 520,
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
                    CreateMaterialSwitchRow(Strings.Switches_MicaLayer, Strings.Switches_MicaLayer_Desc, FWSwitchDensity.Compact, isOn: true),
                    CreateMaterialSwitchRow(Strings.Switches_AcrylicPass, Strings.Switches_AcrylicPass_Desc, FWSwitchDensity.Comfortable, isOn: true),
                    CreateMaterialSwitchRow(Strings.Switches_ReducedMotion, Strings.Switches_ReducedMotion_Desc, FWSwitchDensity.Spacious, isOn: false)
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
                    Text = Strings.Overview_Settings,
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
                OffContent = Strings.Switches_Off,
                OnContent = Strings.Switches_On,
                VerticalAlignment = VerticalAlignment.Center
            }
        };
    }

    private static FWBorder CreateSwitchExampleCard(FluentIconRegular icon, string title, string description, UIElement content)
    {
        return GallerySampleCard.Create(icon, title, description, content, code: CreateSampleCode(title), width: double.NaN);
    }

    private static string CreateSampleCode(string title)
    {
        return title switch
        {
            "FWToggleButton" => "<FWToggleButton Content=\"On\" IsChecked=\"True\" />\n<FWToggleButton IsThreeState=\"True\" IsChecked=\"{x:Null}\" />",
            "FWToggleSwitch" => "<FWToggleSwitch Header=\"Notifications\" OffContent=\"Off\" OnContent=\"On\" />",
            "Events and content" => "<FWToggleSwitch Header=\"Live setting\" Toggled=\"OnToggled\" />\n<FWButton Content=\"Flip\" />",
            _ => "<FWFluentMaterialSurface MaterialKind=\"LiquidGlass\">\n  <FWToggleSwitch Header=\"Mica layer\" IsOn=\"True\" />\n</FWFluentMaterialSurface>"
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
            FWSwitchDensity.Compact => Strings.Switches_Density_Compact,
            FWSwitchDensity.Spacious => Strings.Switches_Density_Spacious,
            _ => Strings.Switches_Density_Comfortable
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
