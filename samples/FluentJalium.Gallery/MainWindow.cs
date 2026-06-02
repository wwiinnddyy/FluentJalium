using FluentJalium.Controls.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Media;

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
        page.Children.Add(CreateBasicsSection());
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
            Text = "Fluent Design resources and core Jalium control styles.",
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

    private UIElement CreateBasicsSection()
    {
        var panel = CreateSection("Buttons");
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12
        };

        row.Children.Add(new Button { Content = "Button" });
        row.Children.Add(new RepeatButton { Content = "Repeat" });
        row.Children.Add(new HyperlinkButton { Content = "Hyperlink" });
        row.Children.Add(new ToggleButton { Content = "Toggle", IsChecked = true });
        row.Children.Add(new Button { Content = "Disabled", IsEnabled = false });

        panel.Children.Add(row);
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
        header.Children.Add(CreateCaption("Pressed"));
        header.Children.Add(CreateCaption("Selected"));
        header.Children.Add(CreateCaption("Disabled"));
        panel.Children.Add(header);

        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12
        };

        row.Children.Add(new Button { Content = "Normal" });
        row.Children.Add(new Button { Content = "Press me" });
        row.Children.Add(new ToggleButton { Content = "Selected", IsChecked = true });
        row.Children.Add(new Button { Content = "Disabled", IsEnabled = false });
        panel.Children.Add(row);

        var checkRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 24,
            Margin = new Thickness(0, 10, 0, 0)
        };
        checkRow.Children.Add(new CheckBox { Content = "Unchecked" });
        checkRow.Children.Add(new CheckBox { Content = "Checked", IsChecked = true });
        checkRow.Children.Add(new RadioButton { Content = "Selected", IsChecked = true });
        checkRow.Children.Add(new ToggleSwitch { Header = "Disabled", IsOn = true, IsEnabled = false });
        panel.Children.Add(checkRow);

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
}
