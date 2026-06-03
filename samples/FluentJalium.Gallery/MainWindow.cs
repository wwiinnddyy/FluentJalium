using FluentJalium.Controls.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Media;
using FWAppBarButton = FluentJalium.Controls.FWAppBarButton;
using FWAppBarSeparator = FluentJalium.Controls.FWAppBarSeparator;
using FWAppBarToggleButton = FluentJalium.Controls.FWAppBarToggleButton;
using FWButton = FluentJalium.Controls.FWButton;
using FWCheckBox = FluentJalium.Controls.FWCheckBox;
using FWComboBox = FluentJalium.Controls.FWComboBox;
using FWCommandBar = FluentJalium.Controls.FWCommandBar;
using FWExpander = FluentJalium.Controls.FWExpander;
using FWDropDownButton = FluentJalium.Controls.FWDropDownButton;
using FWHyperlinkButton = FluentJalium.Controls.FWHyperlinkButton;
using FWInfoBadge = FluentJalium.Controls.FWInfoBadge;
using FWInfoBadgeSeverity = FluentJalium.Controls.FWInfoBadgeSeverity;
using FWInfoBar = FluentJalium.Controls.FWInfoBar;
using FWMenuBar = FluentJalium.Controls.FWMenuBar;
using FWMenuBarItem = FluentJalium.Controls.FWMenuBarItem;
using FWMenuFlyoutItem = FluentJalium.Controls.FWMenuFlyoutItem;
using FWNavigationView = FluentJalium.Controls.FWNavigationView;
using FWNavigationViewItem = FluentJalium.Controls.FWNavigationViewItem;
using FWNumberBox = FluentJalium.Controls.FWNumberBox;
using FWPasswordBox = FluentJalium.Controls.FWPasswordBox;
using FWProgressBar = FluentJalium.Controls.FWProgressBar;
using FWProgressRing = FluentJalium.Controls.FWProgressRing;
using FWRadioButton = FluentJalium.Controls.FWRadioButton;
using FWRatingControl = FluentJalium.Controls.FWRatingControl;
using FWRepeatButton = FluentJalium.Controls.FWRepeatButton;
using FWSlider = FluentJalium.Controls.FWSlider;
using FWSplitButton = FluentJalium.Controls.FWSplitButton;
using FWTextBox = FluentJalium.Controls.FWTextBox;
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
        page.Children.Add(CreateBasicsSection());
        page.Children.Add(CreateTextSection());
        page.Children.Add(CreateSelectionSection());
        page.Children.Add(CreateRangeSection());
        page.Children.Add(CreateAdvancedSection());
        page.Children.Add(CreateCommandSection());
        page.Children.Add(CreateNavigationSection());
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
            Text = "Fluent theme overlay plus FW-prefixed FluentJalium controls.",
            FontSize = 14,
            Foreground = ThemeBrush("TextSecondary")
        });

        return panel;
    }

    private UIElement CreateAdvancedSection()
    {
        var panel = CreateSection("Advanced Inputs");

        panel.Children.Add(new FWInfoBar
        {
            Title = "FWInfoBar",
            Message = "Fluent severity resources are supplied by FluentJalium.",
            Severity = InfoBarSeverity.Success,
            IsClosable = false
        });

        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12
        };

        row.Children.Add(new FWNumberBox
        {
            Header = "FWNumberBox",
            Width = 180,
            Minimum = 0,
            Maximum = 100,
            Value = 42,
            PlaceholderText = "Number"
        });
        row.Children.Add(new FWSplitButton
        {
            Content = "FWSplitButton",
            Width = 170,
            Flyout = CreateSampleFlyout()
        });
        row.Children.Add(new FWToggleSplitButton
        {
            Content = "FWToggleSplit",
            Width = 180,
            IsChecked = true,
            Flyout = CreateSampleFlyout()
        });
        row.Children.Add(new FWExpander
        {
            Header = "FWExpander",
            IsExpanded = true,
            Width = 320,
            Content = new TextBlock
            {
                Text = "Expanded content uses the same Fluent surface and border tokens.",
                Foreground = ThemeBrush("TextSecondary")
            }
        });

        panel.Children.Add(row);

        var badgeRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10
        };
        badgeRow.Children.Add(new FWInfoBadge());
        badgeRow.Children.Add(new FWInfoBadge { Value = 7 });
        badgeRow.Children.Add(new FWInfoBadge { Value = 142, MaxValue = 99 });
        badgeRow.Children.Add(new FWInfoBadge { IconGlyph = "\uE946", Severity = FWInfoBadgeSeverity.Informational });
        badgeRow.Children.Add(new FWInfoBadge { Value = 1, Severity = FWInfoBadgeSeverity.Success });
        badgeRow.Children.Add(new FWInfoBadge { IconGlyph = "\uE7BA", Severity = FWInfoBadgeSeverity.Caution });
        badgeRow.Children.Add(new FWInfoBadge { Value = 3, Severity = FWInfoBadgeSeverity.Critical });

        panel.Children.Add(badgeRow);

        var ratingRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 20
        };
        ratingRow.Children.Add(new FWRatingControl
        {
            Caption = "FWRatingControl",
            Value = 3
        });
        ratingRow.Children.Add(new FWRatingControl
        {
            Caption = "Placeholder",
            PlaceholderValue = 4
        });
        ratingRow.Children.Add(new FWRatingControl
        {
            Caption = "Read only",
            Value = 4,
            IsReadOnly = true
        });
        ratingRow.Children.Add(new FWRatingControl
        {
            Caption = "No clear",
            Value = 1,
            IsClearEnabled = false
        });

        panel.Children.Add(ratingRow);
        return panel;
    }

    private UIElement CreateCommandSection()
    {
        var panel = CreateSection("Commands");
        var menu = new FWMenuBar();
        var fileMenu = new FWMenuBarItem { Title = "File" };
        fileMenu.Items.Add(new FWMenuFlyoutItem { Text = "New" });
        fileMenu.Items.Add(new FWMenuFlyoutItem { Text = "Open" });
        fileMenu.Items.Add(new FWMenuFlyoutItem { Text = "Save" });
        var editMenu = new FWMenuBarItem { Title = "Edit" };
        editMenu.Items.Add(new FWMenuFlyoutItem { Text = "Undo", KeyboardAcceleratorTextOverride = "Ctrl+Z" });
        editMenu.Items.Add(new FWMenuFlyoutItem { Text = "Redo", KeyboardAcceleratorTextOverride = "Ctrl+Y" });
        menu.Items.Add(fileMenu);
        menu.Items.Add(editMenu);

        var commandBar = new FWCommandBar
        {
            Width = 420
        };
        commandBar.PrimaryCommands.Add(new FWAppBarButton { Label = "Save", Icon = CreateIcon("\uE74E") });
        commandBar.PrimaryCommands.Add(new FWAppBarButton { Label = "Share", Icon = CreateIcon("\uE72D") });
        commandBar.PrimaryCommands.Add(new FWAppBarSeparator());
        commandBar.PrimaryCommands.Add(new FWAppBarToggleButton { Label = "Pin", Icon = CreateIcon("\uE718"), IsChecked = true });

        panel.Children.Add(menu);
        panel.Children.Add(commandBar);
        return panel;
    }

    private UIElement CreateNavigationSection()
    {
        var panel = CreateSection("Navigation");
        var navigationView = new FWNavigationView
        {
            Width = 620,
            Height = 220,
            PaneTitle = "FluentJalium",
            Content = new Border
            {
                Background = ThemeBrush("SurfaceBackground"),
                Padding = new Thickness(18),
                Child = new TextBlock
                {
                    Text = "FWNavigationView content surface",
                    Foreground = ThemeBrush("TextPrimary")
                }
            }
        };

        var home = new FWNavigationViewItem
        {
            Content = "Home",
            IsSelected = true
        };
        var controls = new FWNavigationViewItem
        {
            Content = "Controls"
        };
        controls.MenuItems.Add(new FWNavigationViewItem { Content = "Inputs" });
        controls.MenuItems.Add(new FWNavigationViewItem { Content = "Commands" });
        navigationView.MenuItems.Add(home);
        navigationView.MenuItems.Add(controls);
        navigationView.FooterMenuItems.Add(new FWNavigationViewItem { Content = "Settings" });

        panel.Children.Add(navigationView);
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

        row.Children.Add(new FWButton { Content = "FWButton" });
        row.Children.Add(new FWRepeatButton { Content = "FWRepeat" });
        row.Children.Add(new FWHyperlinkButton { Content = "FWHyperlink" });
        row.Children.Add(new FWDropDownButton
        {
            Content = "FWDropDown",
            Flyout = CreateSampleFlyout()
        });
        row.Children.Add(new FWToggleButton { Content = "FWToggle", IsChecked = true });
        row.Children.Add(new FWButton { Content = "Disabled", IsEnabled = false });

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

        row.Children.Add(new FWTextBox
        {
            Text = "FWTextBox",
            Width = 220,
            PlaceholderText = "Enter text"
        });
        row.Children.Add(new FWPasswordBox
        {
            Password = "fluent",
            Width = 220,
            PlaceholderText = "Password"
        });
        row.Children.Add(new FWTextBox
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

        row.Children.Add(new FWCheckBox { Content = "FWCheckBox", IsChecked = true });
        row.Children.Add(new FWRadioButton { Content = "FWRadioButton", IsChecked = true });
        row.Children.Add(new FWToggleSwitch { Header = "FWToggleSwitch", IsOn = true });

        var comboBox = new FWComboBox
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

        row.Children.Add(new FWSlider
        {
            Width = 260,
            Minimum = 0,
            Maximum = 100,
            Value = 64
        });
        row.Children.Add(new FWProgressBar
        {
            Width = 220,
            Height = 8,
            Minimum = 0,
            Maximum = 100,
            Value = 72
        });
        row.Children.Add(new FWProgressBar
        {
            Width = 220,
            Height = 8,
            IsIndeterminate = true
        });
        row.Children.Add(new FWProgressRing
        {
            Width = 36,
            Height = 36,
            IsIndeterminate = true
        });
        row.Children.Add(new FWProgressRing
        {
            Width = 36,
            Height = 36,
            IsIndeterminate = false,
            Value = 72
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

        row.Children.Add(new FWButton { Content = "Normal" });
        row.Children.Add(new FWButton { Content = "Press me" });
        row.Children.Add(new FWToggleButton { Content = "Selected", IsChecked = true });
        row.Children.Add(new FWButton { Content = "Disabled", IsEnabled = false });
        panel.Children.Add(row);

        var checkRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 24,
            Margin = new Thickness(0, 10, 0, 0)
        };
        checkRow.Children.Add(new FWCheckBox { Content = "Unchecked" });
        checkRow.Children.Add(new FWCheckBox { Content = "Checked", IsChecked = true });
        checkRow.Children.Add(new FWRadioButton { Content = "Selected", IsChecked = true });
        checkRow.Children.Add(new FWToggleSwitch { Header = "Disabled", IsOn = true, IsEnabled = false });
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

    private static MenuFlyout CreateSampleFlyout()
    {
        var flyout = new MenuFlyout();
        flyout.Items.Add(new FWMenuFlyoutItem { Text = "Create" });
        flyout.Items.Add(new FWMenuFlyoutItem { Text = "Open" });
        flyout.Items.Add(new FWMenuFlyoutItem { Text = "Export" });
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
