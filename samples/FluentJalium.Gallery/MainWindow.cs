using FluentJalium.Controls.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Media;
using FWAppBarButton = FluentJalium.Controls.FWAppBarButton;
using FWAppBarSeparator = FluentJalium.Controls.FWAppBarSeparator;
using FWAppBarToggleButton = FluentJalium.Controls.FWAppBarToggleButton;
using FWButton = FluentJalium.Controls.FWButton;
using FWDropDownButton = FluentJalium.Controls.FWDropDownButton;
using FWHyperlinkButton = FluentJalium.Controls.FWHyperlinkButton;
using FWRepeatButton = FluentJalium.Controls.FWRepeatButton;
using FWSplitButton = FluentJalium.Controls.FWSplitButton;
using FWToggleButton = FluentJalium.Controls.FWToggleButton;
using FWToggleSplitButton = FluentJalium.Controls.FWToggleSplitButton;
using FWToggleSwitch = FluentJalium.Controls.FWToggleSwitch;

namespace FluentJalium.Gallery;

public sealed class MainWindow : Window
{
    public MainWindow()
    {
        Title = "FluentJalium Gallery";
        Width = 1120;
        Height = 760;
        MinWidth = 860;
        MinHeight = 620;
        Background = ThemeBrush("WindowBackground");

        var root = new ScrollViewer
        {
            Content = BuildContent()
        };

        Content = root;
    }

    private UIElement BuildContent()
    {
        var page = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 22,
            Margin = new Thickness(28)
        };

        page.Children.Add(CreateHeader());
        page.Children.Add(CreateThemeControls());
        page.Children.Add(CreateButtonsSection());
        page.Children.Add(CreateCommandButtonsSection());
        page.Children.Add(CreateSwitchesSection());
        page.Children.Add(CreateTextSection());
        page.Children.Add(CreateSelectionSection());
        page.Children.Add(CreateRangeSection());
        page.Children.Add(CreateStateMatrix());

        return page;
    }

    private UIElement CreateHeader()
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 6
        };

        panel.Children.Add(new TextBlock
        {
            Text = "FluentJalium",
            FontSize = 30,
            FontFamily = "Segoe UI Variable Display",
            Foreground = ThemeBrush("TextPrimary")
        });

        panel.Children.Add(new TextBlock
        {
            Text = "Fluent theme overlay plus FW-prefixed button and switch controls.",
            FontSize = 14,
            Foreground = ThemeBrush("TextSecondary")
        });

        return panel;
    }

    private UIElement CreateThemeControls()
    {
        var panel = CreateSection("Theme");
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10
        };

        row.Children.Add(CreateCommandButton("Light", () => ApplyTheme(FluentThemeVariant.Light)));
        row.Children.Add(CreateCommandButton("Dark", () => ApplyTheme(FluentThemeVariant.Dark)));
        row.Children.Add(CreateCommandButton("High Contrast", () => ApplyTheme(FluentThemeVariant.HighContrast)));
        row.Children.Add(CreateCommandButton("Blue", () => ApplyAccent(Color.FromRgb(0x00, 0x78, 0xD4))));
        row.Children.Add(CreateCommandButton("Rose", () => ApplyAccent(Color.FromRgb(0xC2, 0x39, 0xB3))));
        row.Children.Add(CreateCommandButton("Orange", () => ApplyAccent(Color.FromRgb(0xD8, 0x3B, 0x01))));
        row.Children.Add(CreateCommandButton("Green", () => ApplyAccent(Color.FromRgb(0x10, 0x7C, 0x10))));

        panel.Children.Add(row);
        return panel;
    }

    private UIElement CreateButtonsSection()
    {
        var panel = CreateSection("Buttons");
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12
        };

        row.Children.Add(new FWButton { Content = "FWButton" });
        row.Children.Add(new FWRepeatButton { Content = "FWRepeat" });
        row.Children.Add(new FWHyperlinkButton { Content = "FWHyperlink" });
        row.Children.Add(new FWDropDownButton
        {
            Content = "FWDropDown",
            Flyout = CreateSampleFlyout()
        });
        row.Children.Add(new FWButton { Content = "Disabled", IsEnabled = false });

        panel.Children.Add(row);
        return panel;
    }

    private UIElement CreateCommandButtonsSection()
    {
        var panel = CreateSection("Command Buttons");

        var splitRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12
        };
        splitRow.Children.Add(new FWSplitButton
        {
            Content = "FWSplitButton",
            Width = 170,
            Flyout = CreateSampleFlyout()
        });
        splitRow.Children.Add(new FWToggleSplitButton
        {
            Content = "FWToggleSplit",
            Width = 180,
            IsChecked = true,
            Flyout = CreateSampleFlyout()
        });

        var commandBar = new CommandBar
        {
            Width = 420
        };
        commandBar.PrimaryCommands.Add(new FWAppBarButton { Label = "Save", Icon = CreateIcon("\uE74E") });
        commandBar.PrimaryCommands.Add(new FWAppBarButton { Label = "Share", Icon = CreateIcon("\uE72D") });
        commandBar.PrimaryCommands.Add(new FWAppBarSeparator());
        commandBar.PrimaryCommands.Add(new FWAppBarToggleButton { Label = "Pin", Icon = CreateIcon("\uE718"), IsChecked = true });

        panel.Children.Add(splitRow);
        panel.Children.Add(commandBar);
        return panel;
    }

    private UIElement CreateSwitchesSection()
    {
        var panel = CreateSection("Switches");

        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12
        };

        row.Children.Add(new FWToggleButton { Content = "FWToggleButton" });
        row.Children.Add(new FWToggleButton { Content = "Checked", IsChecked = true });
        row.Children.Add(new FWToggleButton { Content = "Indeterminate", IsThreeState = true, IsChecked = null });
        row.Children.Add(new FWToggleButton { Content = "Disabled", IsChecked = true, IsEnabled = false });

        var switchRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 18
        };

        switchRow.Children.Add(new FWToggleSwitch
        {
            Header = "FWToggleSwitch",
            OffContent = "Off",
            OnContent = "On"
        });
        switchRow.Children.Add(new FWToggleSwitch
        {
            Header = "On",
            IsOn = true,
            OffContent = "Off",
            OnContent = "On"
        });
        switchRow.Children.Add(new FWToggleSwitch
        {
            Header = "Disabled",
            IsOn = true,
            IsEnabled = false,
            OffContent = "Off",
            OnContent = "On"
        });

        panel.Children.Add(row);
        panel.Children.Add(switchRow);
        return panel;
    }
    private UIElement CreateTextSection()
    {
        var panel = CreateSection("Text Input");
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12
        };

        row.Children.Add(new TextBox
        {
            Text = "TextBox",
            Width = 220,
            PlaceholderText = "Enter text"
        });
        row.Children.Add(new PasswordBox
        {
            Password = "fluent",
            Width = 220,
            PlaceholderText = "Password"
        });
        row.Children.Add(new TextBox
        {
            Text = "Disabled",
            Width = 220,
            IsEnabled = false
        });

        panel.Children.Add(row);
        return panel;
    }

    private UIElement CreateSelectionSection()
    {
        var panel = CreateSection("Selection");
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 16
        };

        row.Children.Add(new CheckBox { Content = "CheckBox", IsChecked = true });
        row.Children.Add(new RadioButton { Content = "RadioButton", IsChecked = true });
        row.Children.Add(new ToggleSwitch { Header = "ToggleSwitch", IsOn = true });

        var comboBox = new ComboBox
        {
            Width = 220,
            PlaceholderText = "Choose an item",
            SelectedIndex = 0
        };
        comboBox.Items.Add("Fluent tokens");
        comboBox.Items.Add("Control styles");
        comboBox.Items.Add("Gallery sample");
        row.Children.Add(comboBox);

        panel.Children.Add(row);
        return panel;
    }

    private UIElement CreateRangeSection()
    {
        var panel = CreateSection("Range");
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 18
        };

        row.Children.Add(new Slider
        {
            Width = 260,
            Minimum = 0,
            Maximum = 100,
            Value = 64
        });
        row.Children.Add(new ProgressBar
        {
            Width = 220,
            Height = 8,
            Minimum = 0,
            Maximum = 100,
            Value = 72
        });
        row.Children.Add(new ProgressBar
        {
            Width = 220,
            Height = 8,
            IsIndeterminate = true
        });

        panel.Children.Add(row);
        return panel;
    }

    private StackPanel CreateStateMatrix()
    {
        var panel = CreateSection("State Matrix");

        var header = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Margin = new Thickness(0, 0, 0, 4)
        };
        header.Children.Add(CreateCaption("Normal"));
        header.Children.Add(CreateCaption("DropDown"));
        header.Children.Add(CreateCaption("Selected"));
        header.Children.Add(CreateCaption("Disabled"));
        panel.Children.Add(header);

        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12
        };

        row.Children.Add(new FWButton { Content = "Normal" });
        row.Children.Add(new FWDropDownButton { Content = "Open", Flyout = CreateSampleFlyout() });
        row.Children.Add(new FWToggleSplitButton { Content = "Selected", IsChecked = true, Flyout = CreateSampleFlyout() });
        row.Children.Add(new FWButton { Content = "Disabled", IsEnabled = false });
        panel.Children.Add(row);

        var switchRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Margin = new Thickness(0, 10, 0, 0)
        };
        switchRow.Children.Add(new FWToggleButton { Content = "Off" });
        switchRow.Children.Add(new FWToggleButton { Content = "On", IsChecked = true });
        switchRow.Children.Add(new FWToggleSwitch { Header = "On switch", IsOn = true });
        switchRow.Children.Add(new FWToggleSwitch { Header = "Disabled", IsOn = true, IsEnabled = false });
        panel.Children.Add(switchRow);

        return panel;
    }

    private StackPanel CreateSection(string title)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10
        };

        panel.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 18,
            Foreground = ThemeBrush("TextPrimary")
        });

        return panel;
    }

    private TextBlock CreateCaption(string text)
    {
        return new TextBlock
        {
            Text = text,
            Width = 112,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary")
        };
    }

    private Button CreateCommandButton(string text, Action action)
    {
        var button = new Button
        {
            Content = text
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static MenuFlyout CreateSampleFlyout()
    {
        var flyout = new MenuFlyout();
        flyout.Items.Add(new MenuFlyoutItem { Text = "Create" });
        flyout.Items.Add(new MenuFlyoutItem { Text = "Open" });
        flyout.Items.Add(new MenuFlyoutItem { Text = "Export" });
        return flyout;
    }

    private void ApplyTheme(FluentThemeVariant theme)
    {
        FluentThemeManager.ApplyTheme(theme);
        Background = ThemeBrush("WindowBackground");
    }

    private void ApplyAccent(Color accent)
    {
        FluentThemeManager.ApplyAccent(accent);
    }

    private static Brush ThemeBrush(string key)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Brush brush)
        {
            return brush;
        }

        return new SolidColorBrush(Colors.Transparent);
    }

    private static FontIcon CreateIcon(string glyph)
    {
        return new FontIcon
        {
            Glyph = glyph,
            FontFamily = "Segoe Fluent Icons",
            Foreground = ThemeBrush("TextPrimary")
        };
    }
}
