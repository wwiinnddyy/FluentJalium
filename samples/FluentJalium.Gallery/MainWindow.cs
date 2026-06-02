using FluentJalium.Controls.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Data;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Media;
using FWAppBarButton = FluentJalium.Controls.FWAppBarButton;
using FWAppBarSeparator = FluentJalium.Controls.FWAppBarSeparator;
using FWAppBarToggleButton = FluentJalium.Controls.FWAppBarToggleButton;
using FWAutoCompleteBox = FluentJalium.Controls.FWAutoCompleteBox;
using FWButton = FluentJalium.Controls.FWButton;
using FWCheckBox = FluentJalium.Controls.FWCheckBox;
using FWComboBox = FluentJalium.Controls.FWComboBox;
using FWComboBoxItem = FluentJalium.Controls.FWComboBoxItem;
using FWCalendar = FluentJalium.Controls.FWCalendar;
using FWDataGrid = FluentJalium.Controls.FWDataGrid;
using FWDatePicker = FluentJalium.Controls.FWDatePicker;
using FWDropDownButton = FluentJalium.Controls.FWDropDownButton;
using FWFrame = FluentJalium.Controls.FWFrame;
using FWHyperlinkButton = FluentJalium.Controls.FWHyperlinkButton;
using FWInfoBadge = FluentJalium.Controls.FWInfoBadge;
using FWInfoBadgeSeverity = FluentJalium.Controls.FWInfoBadgeSeverity;
using FWInfoBar = FluentJalium.Controls.FWInfoBar;
using FWListBox = FluentJalium.Controls.FWListBox;
using FWListView = FluentJalium.Controls.FWListView;
using FWNavigationView = FluentJalium.Controls.FWNavigationView;
using FWNavigationViewItem = FluentJalium.Controls.FWNavigationViewItem;
using FWNavigationViewItemHeader = FluentJalium.Controls.FWNavigationViewItemHeader;
using FWNavigationViewItemSeparator = FluentJalium.Controls.FWNavigationViewItemSeparator;
using FWNumberBox = FluentJalium.Controls.FWNumberBox;
using FWPasswordBox = FluentJalium.Controls.FWPasswordBox;
using FWProgressBar = FluentJalium.Controls.FWProgressBar;
using FWProgressRing = FluentJalium.Controls.FWProgressRing;
using FWRangeSlider = FluentJalium.Controls.FWRangeSlider;
using FWRadioButton = FluentJalium.Controls.FWRadioButton;
using FWRepeatButton = FluentJalium.Controls.FWRepeatButton;
using FWRichTextBox = FluentJalium.Controls.FWRichTextBox;
using FWSlider = FluentJalium.Controls.FWSlider;
using FWSplitButton = FluentJalium.Controls.FWSplitButton;
using FWStatusBar = FluentJalium.Controls.FWStatusBar;
using FWStatusBarItem = FluentJalium.Controls.FWStatusBarItem;
using FWTabControl = FluentJalium.Controls.FWTabControl;
using FWTabItem = FluentJalium.Controls.FWTabItem;
using FWTextBox = FluentJalium.Controls.FWTextBox;
using FWTimePicker = FluentJalium.Controls.FWTimePicker;
using FWToastNotificationHost = FluentJalium.Controls.FWToastNotificationHost;
using FWToastNotificationItem = FluentJalium.Controls.FWToastNotificationItem;
using FWTreeDataGrid = FluentJalium.Controls.FWTreeDataGrid;
using FWTreeView = FluentJalium.Controls.FWTreeView;
using FWTreeViewItem = FluentJalium.Controls.FWTreeViewItem;
using FWToggleButton = FluentJalium.Controls.FWToggleButton;
using FWToggleSplitButton = FluentJalium.Controls.FWToggleSplitButton;
using FWToggleSwitch = FluentJalium.Controls.FWToggleSwitch;
using GridViewColumn = Jalium.UI.Controls.GridViewColumn;

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
        page.Children.Add(CreateCollectionsSection());
        page.Children.Add(CreateNavigationSection());
        page.Children.Add(CreateDateTimeSection());
        page.Children.Add(CreateStatusSection());
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
            Text = "Fluent theme overlay plus FW-prefixed button, text input, switch, range, selection, collection, navigation, date/time, notification, and status controls.",
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

    private UIElement CreateCollectionsSection()
    {
        var panel = CreateSection("Collections and Tables");
        var sampleRows = CreateSampleRows();
        var sampleTree = CreateSampleTree();

        var topRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 14
        };

        var listBox = new FWListBox
        {
            Width = 190,
            Height = 152
        };
        listBox.Items.Add("Fluent tokens");
        listBox.Items.Add("Control states");
        listBox.Items.Add("Gallery coverage");
        listBox.Items.Add("Disabled sample");
        listBox.SelectedIndex = 1;
        topRow.Children.Add(listBox);

        var listView = new FWListView
        {
            Width = 340,
            Height = 152,
            ItemsSource = sampleRows,
            View = CreateSampleGridView()
        };
        listView.SelectedIndex = 0;
        topRow.Children.Add(listView);

        var treeView = new FWTreeView
        {
            Width = 260,
            Height = 152
        };
        treeView.Items.Add(new FWTreeViewItem
        {
            Header = "Workspace",
            IsExpanded = true,
            Items =
            {
                new FWTreeViewItem { Header = "Design" },
                new FWTreeViewItem { Header = "Build" }
            }
        });
        treeView.Items.Add(new FWTreeViewItem { Header = "Archive" });
        topRow.Children.Add(treeView);

        var dataGrid = new FWDataGrid
        {
            Width = 430,
            Height = 180,
            AutoGenerateColumns = false,
            ItemsSource = sampleRows,
            SelectedIndex = 1
        };
        dataGrid.Columns.Add(new DataGridTextColumn { Header = "Name", Binding = new Binding("Name"), Width = 150 });
        dataGrid.Columns.Add(new DataGridTextColumn { Header = "State", Binding = new Binding("State"), Width = 110 });
        dataGrid.Columns.Add(new DataGridTextColumn { Header = "Count", Binding = new Binding("Count"), Width = 80 });

        var treeDataGrid = new FWTreeDataGrid
        {
            Width = 430,
            Height = 180,
            ChildrenSelector = item => ((GalleryTreeRow)item).Children,
            HasChildrenSelector = item => ((GalleryTreeRow)item).Children.Length > 0
        };
        treeDataGrid.ItemsSource = sampleTree;
        treeDataGrid.Columns.Add(new DataGridTextColumn { Header = "Area", Binding = new Binding("Name"), Width = 170 });
        treeDataGrid.Columns.Add(new DataGridTextColumn { Header = "State", Binding = new Binding("State"), Width = 120 });
        treeDataGrid.ExpandAll();

        var gridRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 14
        };
        gridRow.Children.Add(dataGrid);
        gridRow.Children.Add(treeDataGrid);

        panel.Children.Add(topRow);
        panel.Children.Add(gridRow);
        return panel;
    }

    private UIElement CreateNavigationSection()
    {
        var panel = CreateSection("Navigation");

        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 14
        };

        var dashboardItem = new FWNavigationViewItem
        {
            Content = "Dashboard",
            Icon = CreateIcon("\uE80F")
        };
        var controlsItem = new FWNavigationViewItem
        {
            Content = "Controls",
            Icon = CreateIcon("\uECAA"),
            IsExpanded = true
        };
        controlsItem.MenuItems.Add(new FWNavigationViewItem { Content = "Buttons", Icon = CreateIcon("\uE8FD") });
        controlsItem.MenuItems.Add(new FWNavigationViewItem { Content = "Collections", Icon = CreateIcon("\uE8A9") });

        var navigationView = new FWNavigationView
        {
            Width = 520,
            Height = 230,
            PaneTitle = "FluentJalium",
            Header = "NavigationView",
            SelectedItem = dashboardItem,
            Content = CreateNavigationContent()
        };
        navigationView.MenuItems.Add(new FWNavigationViewItemHeader { Content = "Workspace" });
        navigationView.MenuItems.Add(dashboardItem);
        navigationView.MenuItems.Add(controlsItem);
        navigationView.MenuItems.Add(new FWNavigationViewItemSeparator());
        navigationView.MenuItems.Add(new FWNavigationViewItem { Content = "Gallery", Icon = CreateIcon("\uE8B7") });
        navigationView.FooterMenuItems.Add(new FWNavigationViewItem { Content = "Settings", Icon = CreateIcon("\uE713") });
        navigationView.UpdateMenuItems();
        row.Children.Add(navigationView);

        var tabAndFrame = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12
        };

        var tabControl = new FWTabControl
        {
            Width = 430,
            Height = 116,
            SelectedIndex = 0
        };
        tabControl.Items.Add(new FWTabItem
        {
            Header = "Overview",
            Content = CreateTabContent("Navigation items share the Fluent selection pill and hover states.")
        });
        tabControl.Items.Add(new FWTabItem
        {
            Header = "Details",
            Content = CreateTabContent("Tabs use the shared accent indicator and theme-aware strip colors.")
        });
        tabControl.Items.Add(new FWTabItem
        {
            Header = "Disabled",
            Content = CreateTabContent("Disabled tab sample"),
            IsEnabled = false
        });

        var frame = new FWFrame
        {
            Width = 430,
            Height = 98,
            Padding = new Thickness(14),
            BorderThickness = new Thickness(1),
            Content = new TextBlock
            {
                Text = "FWFrame content host",
                Foreground = ThemeBrush("TextPrimary"),
                VerticalAlignment = VerticalAlignment.Center
            }
        };

        tabAndFrame.Children.Add(tabControl);
        tabAndFrame.Children.Add(frame);
        row.Children.Add(tabAndFrame);

        panel.Children.Add(row);
        return panel;
    }

    private static UIElement CreateNavigationContent()
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Margin = new Thickness(18)
        };

        panel.Children.Add(new TextBlock
        {
            Text = "NavigationView content",
            FontSize = 18,
            Foreground = ThemeBrush("TextPrimary")
        });
        panel.Children.Add(new TextBlock
        {
            Text = "Selected, nested, footer, and separator states share FluentJalium tokens.",
            Foreground = ThemeBrush("TextSecondary")
        });

        return panel;
    }

    private static UIElement CreateTabContent(string text)
    {
        return new TextBlock
        {
            Text = text,
            Margin = new Thickness(12),
            Foreground = ThemeBrush("TextPrimary"),
            VerticalAlignment = VerticalAlignment.Center
        };
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
        var topRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12
        };

        topRow.Children.Add(new FWTextBox
        {
            Text = "FWTextBox",
            Width = 220,
            PlaceholderText = "Enter text"
        });
        topRow.Children.Add(new FWPasswordBox
        {
            Password = "fluent",
            Width = 220,
            PlaceholderText = "Password"
        });
        topRow.Children.Add(new FWNumberBox
        {
            Header = "FWNumberBox",
            Width = 180,
            Minimum = 0,
            Maximum = 100,
            Value = 42,
            SmallChange = 2,
            DecimalPlaces = 0
        });
        topRow.Children.Add(new FWTextBox
        {
            Text = "Disabled",
            Width = 220,
            IsEnabled = false
        });

        var lowerRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12
        };

        var autoCompleteBox = new FWAutoCompleteBox
        {
            Width = 260,
            ItemsSource = new[] { "Fluent tokens", "Fluent controls", "WinUI Gallery", "Community Toolkit" },
            Text = "Fl",
            PlaceholderText = "Search controls",
            FilterMode = AutoCompleteFilterMode.Contains
        };

        var richTextBox = new FWRichTextBox
        {
            Width = 420,
            Height = 96,
            AcceptsTab = true
        };
        richTextBox.SetText("FWRichTextBox uses the same text input resource tokens.");

        lowerRow.Children.Add(autoCompleteBox);
        lowerRow.Children.Add(richTextBox);

        panel.Children.Add(topRow);
        panel.Children.Add(lowerRow);
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
        row.Children.Add(new FWCheckBox { Content = "Indeterminate", IsThreeState = true, IsChecked = null });
        row.Children.Add(new FWRadioButton { Content = "Option A", GroupName = "SelectionDemo", IsChecked = true });
        row.Children.Add(new FWRadioButton { Content = "Option B", GroupName = "SelectionDemo" });

        var comboBox = new FWComboBox
        {
            Width = 220,
            PlaceholderText = "Choose an item"
        };
        comboBox.Items.Add(new FWComboBoxItem { Content = "Fluent tokens" });
        comboBox.Items.Add(new FWComboBoxItem { Content = "Control styles" });
        comboBox.Items.Add(new FWComboBoxItem { Content = "Gallery sample" });
        comboBox.SelectedIndex = 1;
        row.Children.Add(comboBox);

        panel.Children.Add(row);
        return panel;
    }

    private UIElement CreateDateTimeSection()
    {
        var panel = CreateSection("Date and Time");

        var pickerRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 14
        };

        pickerRow.Children.Add(new FWDatePicker
        {
            Header = "Date",
            Width = 220,
            SelectedDate = DateTime.Today,
            SelectedDateFormat = DatePickerFormat.Long
        });
        pickerRow.Children.Add(new FWDatePicker
        {
            Header = "Disabled date",
            Width = 220,
            PlaceholderText = "Select a date",
            IsEnabled = false
        });
        pickerRow.Children.Add(new FWTimePicker
        {
            Header = "Time",
            Width = 180,
            SelectedTime = new TimeSpan(10, 30, 0),
            MinuteIncrement = 15
        });
        pickerRow.Children.Add(new FWTimePicker
        {
            Header = "24 hour",
            Width = 180,
            ClockIdentifier = "24HourClock",
            SelectedTime = new TimeSpan(18, 45, 0),
            MinuteIncrement = 15
        });

        var calendar = new FWCalendar
        {
            DisplayDate = DateTime.Today,
            DisplayDateStart = DateTime.Today.AddDays(-14),
            DisplayDateEnd = DateTime.Today.AddDays(45),
            FirstDayOfWeek = DayOfWeek.Monday,
            IsTodayHighlighted = true,
            SelectedDate = DateTime.Today.AddDays(1),
            SelectionMode = CalendarSelectionMode.SingleDate
        };
        calendar.BlackoutDates.Add(DateTime.Today.AddDays(2).Date);

        panel.Children.Add(pickerRow);
        panel.Children.Add(calendar);
        return panel;
    }

    private UIElement CreateStatusSection()
    {
        var panel = CreateSection("Notifications and Status");

        var infoBarColumn = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Width = 460
        };
        infoBarColumn.Children.Add(CreateInfoBar("Information", "A normal FluentJalium status message.", InfoBarSeverity.Informational));
        infoBarColumn.Children.Add(CreateInfoBar("Success", "The selected operation completed.", InfoBarSeverity.Success));
        infoBarColumn.Children.Add(CreateInfoBar("Warning", "Review settings before continuing.", InfoBarSeverity.Warning));
        infoBarColumn.Children.Add(CreateInfoBar("Error", "A required resource could not be loaded.", InfoBarSeverity.Error));

        var badgeColumn = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 300
        };
        badgeColumn.Children.Add(new TextBlock
        {
            Text = "InfoBadge",
            FontSize = 13,
            Foreground = ThemeBrush("TextSecondary")
        });

        var badgeRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12
        };
        badgeRow.Children.Add(new FWInfoBadge { Severity = FWInfoBadgeSeverity.Attention });
        badgeRow.Children.Add(new FWInfoBadge { Value = 3, Severity = FWInfoBadgeSeverity.Informational });
        badgeRow.Children.Add(new FWInfoBadge { Value = 128, MaxValue = 99, Severity = FWInfoBadgeSeverity.Success });
        badgeRow.Children.Add(new FWInfoBadge { IconGlyph = "\uE7BA", Severity = FWInfoBadgeSeverity.Caution });
        badgeRow.Children.Add(new FWInfoBadge { Value = 1, Severity = FWInfoBadgeSeverity.Critical });
        badgeColumn.Children.Add(badgeRow);

        var statusBar = new FWStatusBar
        {
            Width = 300
        };
        statusBar.Items.Add(new FWStatusBarItem { Content = "Ready" });
        statusBar.Items.Add(new FWStatusBarItem { Content = "Line 42" });
        statusBar.Items.Add(new FWStatusBarItem { Content = "UTF-8" });
        statusBar.Items.Add(new FWStatusBarItem { Content = "Disabled", IsEnabled = false });
        badgeColumn.Children.Add(statusBar);

        var toastHost = new FWToastNotificationHost
        {
            Width = 460,
            Height = 156,
            MaxVisibleToasts = 3,
            Position = ToastPosition.TopLeft,
            ToastWidth = 430
        };
        toastHost.ShowToast(new FWToastNotificationItem
        {
            Title = "Toast notification",
            Message = "FWToastNotificationItem uses Fluent severity resources.",
            Severity = ToastSeverity.Information,
            IsAutoDismissEnabled = false
        });
        toastHost.ShowToast(new FWToastNotificationItem
        {
            Title = "Build complete",
            Message = "Static sample toast remains visible in the gallery.",
            Severity = ToastSeverity.Success,
            IsAutoDismissEnabled = false
        });

        var topRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 18
        };
        topRow.Children.Add(infoBarColumn);
        topRow.Children.Add(badgeColumn);

        panel.Children.Add(topRow);
        panel.Children.Add(toastHost);
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
        row.Children.Add(new FWRangeSlider
        {
            Width = 260,
            Minimum = 0,
            Maximum = 100,
            RangeStart = 24,
            RangeEnd = 76,
            MinimumRange = 8
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

        var ringRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 18,
            Margin = new Thickness(0, 10, 0, 0)
        };
        ringRow.Children.Add(new FWProgressRing
        {
            Width = 36,
            Height = 36,
            IsIndeterminate = true
        });
        ringRow.Children.Add(new FWProgressRing
        {
            Width = 36,
            Height = 36,
            IsIndeterminate = false,
            Value = 72
        });
        ringRow.Children.Add(new FWProgressRing
        {
            Width = 36,
            Height = 36,
            IsActive = false,
            IsIndeterminate = false,
            Value = 30
        });

        panel.Children.Add(row);
        panel.Children.Add(ringRow);
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

        var selectionRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Margin = new Thickness(0, 10, 0, 0)
        };
        selectionRow.Children.Add(new FWCheckBox { Content = "Unchecked" });
        selectionRow.Children.Add(new FWCheckBox { Content = "Checked", IsChecked = true });
        selectionRow.Children.Add(new FWCheckBox { Content = "Indeterminate", IsThreeState = true, IsChecked = null });
        selectionRow.Children.Add(new FWCheckBox { Content = "Disabled", IsChecked = true, IsEnabled = false });
        panel.Children.Add(selectionRow);

        var comboRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Margin = new Thickness(0, 10, 0, 0)
        };
        comboRow.Children.Add(CreateSampleComboBox("Normal", 0, true));
        comboRow.Children.Add(CreateSampleComboBox("Selected", 1, true));
        comboRow.Children.Add(CreateSampleComboBox("Disabled", 2, false));
        panel.Children.Add(comboRow);

        return panel;
    }

    private static FWInfoBar CreateInfoBar(string title, string message, InfoBarSeverity severity)
    {
        return new FWInfoBar
        {
            Title = title,
            Message = message,
            Severity = severity,
            IsOpen = true,
            IsClosable = false,
            Width = 460
        };
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

    private static FWComboBox CreateSampleComboBox(string placeholder, int selectedIndex, bool isEnabled)
    {
        var comboBox = new FWComboBox
        {
            Width = 160,
            PlaceholderText = placeholder,
            IsEnabled = isEnabled
        };

        comboBox.Items.Add(new FWComboBoxItem { Content = "Fluent" });
        comboBox.Items.Add(new FWComboBoxItem { Content = "WinUI" });
        comboBox.Items.Add(new FWComboBoxItem { Content = "Toolkit" });
        comboBox.SelectedIndex = selectedIndex;
        return comboBox;
    }

    private static GridView CreateSampleGridView()
    {
        var view = new GridView();
        view.Columns.Add(new GridViewColumn
        {
            Header = "Name",
            DisplayMemberBinding = new Binding("Name"),
            Width = 150
        });
        view.Columns.Add(new GridViewColumn
        {
            Header = "State",
            DisplayMemberBinding = new Binding("State"),
            Width = 110
        });
        return view;
    }

    private static GalleryRow[] CreateSampleRows()
    {
        return
        [
            new GalleryRow("Buttons", "Complete", 9),
            new GalleryRow("Selection", "Review", 4),
            new GalleryRow("Collections", "Active", 8)
        ];
    }

    private static GalleryTreeRow[] CreateSampleTree()
    {
        return
        [
            new GalleryTreeRow(
                "FluentJalium",
                "Active",
                [
                    new GalleryTreeRow("Theme resources", "Loaded", []),
                    new GalleryTreeRow("FW controls", "Expanding", [])
                ]),
            new GalleryTreeRow("Gallery", "Visible", [])
        ];
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

    private sealed record GalleryRow(string Name, string State, int Count);

    private sealed record GalleryTreeRow(string Name, string State, GalleryTreeRow[] Children);
}
