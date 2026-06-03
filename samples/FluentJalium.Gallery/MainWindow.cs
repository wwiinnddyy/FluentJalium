using FluentJalium.Controls.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Ink;
using Jalium.UI.Data;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Input;
using Jalium.UI.Media;
using FWAppBarButton = FluentJalium.Controls.FWAppBarButton;
using FWAppBarSeparator = FluentJalium.Controls.FWAppBarSeparator;
using FWAppBarToggleButton = FluentJalium.Controls.FWAppBarToggleButton;
using FWAccessText = FluentJalium.Controls.FWAccessText;
using FWAutoCompleteBox = FluentJalium.Controls.FWAutoCompleteBox;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWButton = FluentJalium.Controls.FWButton;
using FWCheckBox = FluentJalium.Controls.FWCheckBox;
using FWColorPicker = FluentJalium.Controls.FWColorPicker;
using FWComboBox = FluentJalium.Controls.FWComboBox;
using FWComboBoxItem = FluentJalium.Controls.FWComboBoxItem;
using FWCalendar = FluentJalium.Controls.FWCalendar;
using FWContentControl = FluentJalium.Controls.FWContentControl;
using FWContentDialog = FluentJalium.Controls.FWContentDialog;
using FWContentPresenter = FluentJalium.Controls.FWContentPresenter;
using FWDataGrid = FluentJalium.Controls.FWDataGrid;
using FWDatePicker = FluentJalium.Controls.FWDatePicker;
using FWDropDownButton = FluentJalium.Controls.FWDropDownButton;
using FWExpander = FluentJalium.Controls.FWExpander;
using FWFrame = FluentJalium.Controls.FWFrame;
using FWFontIcon = FluentJalium.Controls.FWFontIcon;
using FWGroupBox = FluentJalium.Controls.FWGroupBox;
using FWHyperlinkButton = FluentJalium.Controls.FWHyperlinkButton;
using FWImage = FluentJalium.Controls.FWImage;
using FWInkCanvas = FluentJalium.Controls.FWInkCanvas;
using FWInkPresenter = FluentJalium.Controls.FWInkPresenter;
using FWInfoBadge = FluentJalium.Controls.FWInfoBadge;
using FWInfoBadgeSeverity = FluentJalium.Controls.FWInfoBadgeSeverity;
using FWInfoBar = FluentJalium.Controls.FWInfoBar;
using FWGrid = FluentJalium.Controls.FWGrid;
using FWGridSplitter = FluentJalium.Controls.FWGridSplitter;
using FWLabel = FluentJalium.Controls.FWLabel;
using FWListBox = FluentJalium.Controls.FWListBox;
using FWListView = FluentJalium.Controls.FWListView;
using FWContextMenu = FluentJalium.Controls.FWContextMenu;
using FWMenu = FluentJalium.Controls.FWMenu;
using FWMenuBar = FluentJalium.Controls.FWMenuBar;
using FWMenuBarItem = FluentJalium.Controls.FWMenuBarItem;
using FWMenuFlyoutItem = FluentJalium.Controls.FWMenuFlyoutItem;
using FWMenuFlyoutSeparator = FluentJalium.Controls.FWMenuFlyoutSeparator;
using FWMenuItem = FluentJalium.Controls.FWMenuItem;
using FWMediaElement = FluentJalium.Controls.FWMediaElement;
using FWNavigationView = FluentJalium.Controls.FWNavigationView;
using FWNavigationViewItem = FluentJalium.Controls.FWNavigationViewItem;
using FWNavigationViewItemHeader = FluentJalium.Controls.FWNavigationViewItemHeader;
using FWNavigationViewItemSeparator = FluentJalium.Controls.FWNavigationViewItemSeparator;
using FWNumberBox = FluentJalium.Controls.FWNumberBox;
using FWPasswordBox = FluentJalium.Controls.FWPasswordBox;
using FWProgressBar = FluentJalium.Controls.FWProgressBar;
using FWProgressRing = FluentJalium.Controls.FWProgressRing;
using FWRangeSlider = FluentJalium.Controls.FWRangeSlider;
using FWPathIcon = FluentJalium.Controls.FWPathIcon;
using FWRadioButton = FluentJalium.Controls.FWRadioButton;
using FWRepeatButton = FluentJalium.Controls.FWRepeatButton;
using FWRichTextBox = FluentJalium.Controls.FWRichTextBox;
using FWSeparator = FluentJalium.Controls.FWSeparator;
using FWSlider = FluentJalium.Controls.FWSlider;
using FWSplitButton = FluentJalium.Controls.FWSplitButton;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWStatusBar = FluentJalium.Controls.FWStatusBar;
using FWScrollViewer = FluentJalium.Controls.FWScrollViewer;
using FWSymbolIcon = FluentJalium.Controls.FWSymbolIcon;
using FWStatusBarItem = FluentJalium.Controls.FWStatusBarItem;
using FWSwipeControl = FluentJalium.Controls.FWSwipeControl;
using FWTabControl = FluentJalium.Controls.FWTabControl;
using FWTabItem = FluentJalium.Controls.FWTabItem;
using FWTextBox = FluentJalium.Controls.FWTextBox;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWTimePicker = FluentJalium.Controls.FWTimePicker;
using FWToastNotificationHost = FluentJalium.Controls.FWToastNotificationHost;
using FWToastNotificationItem = FluentJalium.Controls.FWToastNotificationItem;
using FWToolTip = FluentJalium.Controls.FWToolTip;
using FWTreeDataGrid = FluentJalium.Controls.FWTreeDataGrid;
using FWTreeView = FluentJalium.Controls.FWTreeView;
using FWTreeViewItem = FluentJalium.Controls.FWTreeViewItem;
using FWToggleButton = FluentJalium.Controls.FWToggleButton;
using FWToggleMenuFlyoutItem = FluentJalium.Controls.FWToggleMenuFlyoutItem;
using FWToggleSplitButton = FluentJalium.Controls.FWToggleSplitButton;
using FWToggleSwitch = FluentJalium.Controls.FWToggleSwitch;
using FWViewbox = FluentJalium.Controls.FWViewbox;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;
using GridViewColumn = Jalium.UI.Controls.GridViewColumn;

namespace FluentJalium.Gallery;

public sealed class MainWindow : Window
{
    private readonly List<FWNavigationViewItem> _navigationItems = [];
    private GalleryPage[] _pages = [];
    private FWNavigationView? _navigationView;
    private FWScrollViewer? _contentScrollViewer;
    private FWTextBox? _searchBox;
    private GalleryPage? _selectedPage;
    private string _navigationSearchText = string.Empty;
    private bool _isShowingSearchEmptyState;

    public MainWindow()
    {
        Title = "FluentJalium Gallery";
        Width = 1280;
        Height = 820;
        MinWidth = 980;
        MinHeight = 620;
        Background = ThemeBrush("WindowBackground");
        Content = BuildShell();
    }

    private UIElement BuildShell()
    {
        _pages = CreateGalleryPages();
        _contentScrollViewer = new FWScrollViewer
        {
            Background = ThemeBrush("NavigationViewContentBackground"),
            Padding = new Thickness(0),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            IsScrollBarAutoHideEnabled = true
        };

        _navigationView = new FWNavigationView
        {
            Background = ThemeBrush("WindowBackground"),
            PaneBackground = ThemeBrush("NavigationViewPaneBackground"),
            ContentBackground = ThemeBrush("NavigationViewContentBackground"),
            PaneDisplayMode = NavigationViewPaneDisplayMode.Left,
            IsPaneOpen = true,
            OpenPaneLength = 320,
            CompactPaneLength = 48,
            PaneHeader = CreatePaneHeader(),
            Content = _contentScrollViewer
        };
        _navigationView.SelectionChanged += OnNavigationSelectionChanged;

        PopulateNavigationItems(_navigationView, _pages, _navigationSearchText);
        _navigationView.SelectedItem = _navigationItems[0];
        SelectPage(_pages[0]);
        return _navigationView;
    }

    private GalleryPage[] CreateGalleryPages()
    {
        return
        [
            new GalleryPage("Overview", "Theme, typography, and accent controls for validating FluentJalium across variants.", GalleryNavigationGroup.Home, "\uE80F", () => CreatePageStack(CreateThemeControls()), "home design system theme typography accent light dark high contrast"),
            new GalleryPage("Buttons", "Button and command surfaces, including split, drop-down, and app bar buttons.", GalleryNavigationGroup.ControlSurfaces, "\uE8FD", () => CreatePageStack(CreateButtonsSection(), CreateCommandButtonsSection()), "FWButton FWRepeatButton FWHyperlinkButton FWDropDownButton FWSplitButton FWToggleSplitButton FWAppBarButton FWAppBarToggleButton FWAppBarSeparator command bar"),
            new GalleryPage("Switches", "ToggleButton and ToggleSwitch states for checked, unchecked, indeterminate, and disabled surfaces.", GalleryNavigationGroup.ControlSurfaces, "\uE7F4", () => CreatePageStack(CreateSwitchesSection()), "FWToggleButton FWToggleSwitch checked unchecked indeterminate"),
            new GalleryPage("Text Input", "TextBox, PasswordBox, NumberBox, AutoCompleteBox, and RichTextBox surfaces.", GalleryNavigationGroup.Input, "\uE8D2", () => CreatePageStack(CreateTextSection()), "FWTextBox FWPasswordBox FWNumberBox FWAutoCompleteBox FWRichTextBox search form input"),
            new GalleryPage("Selection", "CheckBox, RadioButton, ComboBox, and ComboBoxItem controls.", GalleryNavigationGroup.Input, "\uE73A", () => CreatePageStack(CreateSelectionSection()), "FWCheckBox FWRadioButton FWComboBox FWComboBoxItem pick choose"),
            new GalleryPage("Range", "Slider, RangeSlider, ProgressBar, and ProgressRing controls.", GalleryNavigationGroup.Input, "\uE9F5", () => CreatePageStack(CreateRangeSection()), "FWSlider FWRangeSlider FWProgressBar FWProgressRing value loading"),
            new GalleryPage("Date and Time", "DatePicker, TimePicker, and Calendar controls.", GalleryNavigationGroup.Input, "\uE787", () => CreatePageStack(CreateDateTimeSection()), "FWDatePicker FWTimePicker FWCalendar schedule calendar"),
            new GalleryPage("Content and Layout", "TextBlock, AccessText, Border, content hosts, StackPanel, WrapPanel, and Grid foundations.", GalleryNavigationGroup.LayoutAndMedia, "\uE8A9", () => CreatePageStack(CreateContentLayoutSection()), "FWTextBlock FWAccessText FWBorder FWContentControl FWContentPresenter FWStackPanel FWWrapPanel FWGrid layout"),
            new GalleryPage("Visuals", "Image, icon, label, separator, and Viewbox foundation controls.", GalleryNavigationGroup.LayoutAndMedia, "\uE8B9", () => CreatePageStack(CreateVisualsSection()), "FWImage FWFontIcon FWSymbolIcon FWPathIcon FWLabel FWSeparator FWViewbox visual icon"),
            new GalleryPage("Interaction", "ScrollViewer, SwipeControl, and GridSplitter controls.", GalleryNavigationGroup.LayoutAndMedia, "\uE7C9", () => CreatePageStack(CreateInteractionSection()), "FWScrollViewer FWSwipeControl FWGridSplitter scrolling resize"),
            new GalleryPage("Input and Media", "ColorPicker, InkCanvas, InkPresenter, and MediaElement surfaces.", GalleryNavigationGroup.LayoutAndMedia, "\uE7FC", () => CreatePageStack(CreateAdvancedInputMediaSection()), "FWColorPicker FWInkCanvas FWInkPresenter FWMediaElement color ink media"),
            new GalleryPage("Collections", "ListBox, ListView, TreeView, DataGrid, and TreeDataGrid controls.", GalleryNavigationGroup.CollectionsAndData, "\uE8A9", () => CreatePageStack(CreateCollectionsSection()), "FWListBox FWListView FWTreeView FWDataGrid FWTreeDataGrid table list data"),
            new GalleryPage("Navigation", "NavigationView, TabControl, TabItem, and Frame controls.", GalleryNavigationGroup.AppStructure, "\uE700", () => CreatePageStack(CreateNavigationSection()), "FWNavigationView FWNavigationViewItem FWTabControl FWTabItem FWFrame page shell"),
            new GalleryPage("Menus", "MenuBar, Menu, ContextMenu, and MenuFlyout item surfaces.", GalleryNavigationGroup.AppStructure, "\uE8FD", () => CreatePageStack(CreateMenusSection()), "FWMenuBar FWMenu FWContextMenu FWMenuFlyoutItem FWToggleMenuFlyoutItem FWMenuFlyoutSeparator command menu"),
            new GalleryPage("Disclosure", "Expander, ToolTip, ContentDialog, and GroupBox controls.", GalleryNavigationGroup.AppStructure, "\uE70E", () => CreatePageStack(CreateDisclosureDialogsSection()), "FWExpander FWToolTip FWContentDialog FWGroupBox dialog flyout disclosure"),
            new GalleryPage("Status", "InfoBar, InfoBadge, ToastNotification, and StatusBar controls.", GalleryNavigationGroup.AppStructure, "\uE946", () => CreatePageStack(CreateStatusSection()), "FWInfoBar FWInfoBadge FWToastNotificationHost FWToastNotificationItem FWStatusBar notification message severity"),
            new GalleryPage("State Matrix", "Cross-control normal, selected, disabled, and flyout state checks.", GalleryNavigationGroup.Diagnostics, "\uE9D9", () => CreatePageStack(CreateStateMatrix()), "states normal hover pressed selected disabled light dark high contrast", IsFooter: true)
        ];
    }

    private void PopulateNavigationItems(FWNavigationView navigationView, GalleryPage[] pages, string searchText)
    {
        _navigationItems.Clear();
        navigationView.MenuItems.Clear();
        navigationView.FooterMenuItems.Clear();

        var matchingPages = pages
            .Where(page => MatchesNavigationSearch(page, searchText))
            .ToArray();

        var homePage = matchingPages.FirstOrDefault(page => page.Group == GalleryNavigationGroup.Home);
        if (homePage != null)
        {
            navigationView.MenuItems.Add(CreateNavigationItem(homePage));
        }

        var groupedPages = GalleryNavigationGroup.Order
            .Select(groupName => new
            {
                GroupName = groupName,
                Pages = matchingPages
                    .Where(page => !page.IsFooter && page.Group == groupName)
                    .ToArray()
            })
            .Where(group => group.Pages.Length > 0)
            .ToArray();

        if (homePage != null && groupedPages.Length > 0)
        {
            navigationView.MenuItems.Add(new FWNavigationViewItemSeparator());
        }

        foreach (var group in groupedPages)
        {
            var groupItem = CreateNavigationGroupItem(group.GroupName);
            foreach (var page in group.Pages)
            {
                groupItem.MenuItems.Add(CreateNavigationItem(page));
            }

            navigationView.MenuItems.Add(groupItem);
        }

        foreach (var page in matchingPages.Where(page => page.IsFooter))
        {
            navigationView.FooterMenuItems.Add(CreateNavigationItem(page));
        }

        navigationView.UpdateMenuItems();
    }

    private static FWNavigationViewItem CreateNavigationGroupItem(string groupName)
    {
        return new FWNavigationViewItem
        {
            Content = groupName,
            Icon = CreateIcon(GalleryNavigationGroup.GetGlyph(groupName)),
            IsExpanded = true,
            SelectsOnInvoked = false,
            Tag = groupName
        };
    }

    private FWNavigationViewItem CreateNavigationItem(GalleryPage page)
    {
        var item = new FWNavigationViewItem
        {
            Content = page.Title,
            Icon = CreateIcon(page.Glyph),
            Tag = page
        };
        _navigationItems.Add(item);
        return item;
    }

    private UIElement CreatePaneHeader()
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Margin = new Thickness(0, 4, 0, 2)
        };

        panel.Children.Add(new TextBlock
        {
            Text = "FluentJalium",
            FontSize = 24,
            FontFamily = "Segoe UI Variable Display",
            Foreground = ThemeBrush("TextPrimary")
        });
        panel.Children.Add(new TextBlock
        {
            Text = "Control gallery",
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary")
        });

        _searchBox = new FWTextBox
        {
            Text = _navigationSearchText,
            PlaceholderText = "Search controls and samples",
            MinHeight = 32,
            Margin = new Thickness(0, 2, 0, 0)
        };
        _searchBox.TextChanged += OnNavigationSearchTextChanged;
        panel.Children.Add(_searchBox);
        return panel;
    }

    private UIElement CreatePageStack(params UIElement[] sections)
    {
        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 22
        };

        foreach (var section in sections)
        {
            stack.Children.Add(section);
        }

        return stack;
    }

    private UIElement CreatePageContent(GalleryPage page)
    {
        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 18,
            Margin = new Thickness(40, 32, 40, 40)
        };

        stack.Children.Add(new TextBlock
        {
            Text = page.Title,
            FontSize = 30,
            FontFamily = "Segoe UI Variable Display",
            Foreground = ThemeBrush("TextPrimary")
        });
        stack.Children.Add(new TextBlock
        {
            Text = page.Description,
            FontSize = 14,
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        });
        stack.Children.Add(page.CreateContent());
        return stack;
    }

    private UIElement CreateSearchEmptyContent(string searchText)
    {
        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Margin = new Thickness(40, 36, 40, 40)
        };

        stack.Children.Add(new TextBlock
        {
            Text = "No results",
            FontSize = 28,
            FontFamily = "Segoe UI Variable Display",
            Foreground = ThemeBrush("TextPrimary")
        });
        stack.Children.Add(new TextBlock
        {
            Text = string.IsNullOrWhiteSpace(searchText)
                ? "No Gallery pages are available."
                : $"No Gallery pages match \"{searchText.Trim()}\".",
            FontSize = 14,
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        });

        return stack;
    }

    private void OnNavigationSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not FWTextBox textBox)
        {
            return;
        }

        var searchText = textBox.Text ?? string.Empty;
        if (string.Equals(_navigationSearchText, searchText, StringComparison.Ordinal))
        {
            return;
        }

        _navigationSearchText = searchText;
        RefreshNavigationForSearch();
    }

    private void RefreshNavigationForSearch()
    {
        if (_navigationView == null)
        {
            return;
        }

        var preferredPage = _selectedPage;
        PopulateNavigationItems(_navigationView, _pages, _navigationSearchText);

        if (preferredPage != null && TrySelectNavigationPage(preferredPage))
        {
            return;
        }

        if (_navigationItems.Count > 0 && _navigationItems[0].Tag is GalleryPage firstPage)
        {
            _navigationView.SelectedItem = _navigationItems[0];
            SelectPage(firstPage);
            return;
        }

        _navigationView.SelectedItem = null;
        _isShowingSearchEmptyState = true;
        if (_contentScrollViewer != null)
        {
            _contentScrollViewer.Content = CreateSearchEmptyContent(_navigationSearchText);
        }
    }

    private bool TrySelectNavigationPage(GalleryPage page)
    {
        if (_navigationView == null)
        {
            return false;
        }

        var item = _navigationItems.FirstOrDefault(navigationItem => ReferenceEquals(navigationItem.Tag, page));
        if (item == null)
        {
            return false;
        }

        _navigationView.SelectedItem = item;
        SelectPage(page);
        return true;
    }

    private void OnNavigationSelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs e)
    {
        if (e.SelectedItem?.Tag is GalleryPage page)
        {
            SelectPage(page);
        }
    }

    private void SelectPage(GalleryPage page)
    {
        _selectedPage = page;
        _isShowingSearchEmptyState = false;

        if (_contentScrollViewer != null)
        {
            _contentScrollViewer.Content = CreatePageContent(page);
        }
    }

    private void RefreshCurrentPage()
    {
        Background = ThemeBrush("WindowBackground");

        if (_navigationView != null)
        {
            _navigationView.Background = ThemeBrush("WindowBackground");
            _navigationView.PaneBackground = ThemeBrush("NavigationViewPaneBackground");
            _navigationView.ContentBackground = ThemeBrush("NavigationViewContentBackground");
            _navigationView.PaneHeader = CreatePaneHeader();
        }
        if (_contentScrollViewer != null)
        {
            _contentScrollViewer.Background = ThemeBrush("NavigationViewContentBackground");

            if (_isShowingSearchEmptyState)
            {
                _contentScrollViewer.Content = CreateSearchEmptyContent(_navigationSearchText);
            }
            else if (_selectedPage != null)
            {
                _contentScrollViewer.Content = CreatePageContent(_selectedPage);
            }
        }
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
            Text = "Fluent theme overlay plus FW-prefixed button, text input, switch, range, selection, visual, interaction, input/media, collection, navigation, disclosure, dialog, menu, date/time, notification, and status controls.",
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

        var toggleButtonRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12
        };

        toggleButtonRow.Children.Add(new FWToggleButton { Content = "FWToggleButton" });
        toggleButtonRow.Children.Add(new FWToggleButton { Content = "Checked", IsChecked = true });
        toggleButtonRow.Children.Add(new FWToggleButton { Content = "Indeterminate", IsThreeState = true, IsChecked = null });
        toggleButtonRow.Children.Add(new FWToggleButton { Content = "Disabled", IsChecked = true, IsEnabled = false });

        var switchRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 20,
            Margin = new Thickness(0, 4, 0, 0)
        };

        switchRow.Children.Add(new FWToggleSwitch
        {
            Header = "Default",
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
            Header = "Content",
            IsOn = true,
            OffContent = "Paused",
            OnContent = "Running"
        });
        switchRow.Children.Add(new FWToggleSwitch
        {
            Header = "Disabled",
            IsOn = true,
            IsEnabled = false,
            OffContent = "Off",
            OnContent = "On"
        });

        panel.Children.Add(toggleButtonRow);
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
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        examples.Children.Add(CreateSelectionExampleCard(
            "FWCheckBox",
            "Two-state, three-state, disabled, and Select all patterns.",
            CreateCheckBoxSelectionSample()));
        examples.Children.Add(CreateSelectionExampleCard(
            "FWRadioButton",
            "Named groups keep a single selected option and report the current choice.",
            CreateRadioButtonSelectionSample()));
        examples.Children.Add(CreateSelectionExampleCard(
            "FWComboBox",
            "Inline items, editable text, placeholder, disabled, and selection output.",
            CreateComboBoxSelectionSample()));
        examples.Children.Add(CreateSelectionExampleCard(
            "FWComboBoxItem",
            "Selected, hover-ready, and disabled dropdown item states use Fluent resources.",
            CreateComboBoxItemStateSample()));

        panel.Children.Add(examples);
        return panel;
    }

    private static UIElement CreateCheckBoxSelectionSample()
    {
        var output = CreateSelectionOutput("Selected: Fluent, Gallery");
        var selectAll = new FWCheckBox
        {
            Content = "Select all",
            IsThreeState = true,
            IsChecked = null
        };
        var fluent = new FWCheckBox { Content = "Fluent", IsChecked = true };
        var controls = new FWCheckBox { Content = "Controls" };
        var gallery = new FWCheckBox { Content = "Gallery", IsChecked = true };
        var disabled = new FWCheckBox { Content = "Disabled option", IsChecked = true, IsEnabled = false };

        var children = new[] { fluent, controls, gallery };
        var isUpdating = false;

        void UpdateOutput()
        {
            var selected = children
                .Where(checkBox => checkBox.IsChecked == true)
                .Select(checkBox => checkBox.Content?.ToString())
                .Where(text => !string.IsNullOrEmpty(text));
            var selectedText = string.Join(", ", selected);
            output.Text = $"Selected: {(selectedText.Length > 0 ? selectedText : "none")}";

            if (isUpdating)
            {
                return;
            }

            isUpdating = true;
            var selectedCount = children.Count(checkBox => checkBox.IsChecked == true);
            selectAll.IsChecked = selectedCount == 0 ? false : selectedCount == children.Length ? true : null;
            isUpdating = false;
        }

        selectAll.Checked += (_, _) =>
        {
            if (isUpdating)
            {
                return;
            }

            isUpdating = true;
            foreach (var child in children)
            {
                child.IsChecked = true;
            }
            isUpdating = false;
            UpdateOutput();
        };
        selectAll.Unchecked += (_, _) =>
        {
            if (isUpdating)
            {
                return;
            }

            isUpdating = true;
            foreach (var child in children)
            {
                child.IsChecked = false;
            }
            isUpdating = false;
            UpdateOutput();
        };

        foreach (var child in children)
        {
            child.Checked += (_, _) => UpdateOutput();
            child.Unchecked += (_, _) => UpdateOutput();
        }

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 9,
            Children =
            {
                selectAll,
                fluent,
                controls,
                gallery,
                new FWCheckBox { Content = "Indeterminate", IsThreeState = true, IsChecked = null },
                disabled,
                output
            }
        };
    }

    private static UIElement CreateRadioButtonSelectionSample()
    {
        var output = CreateSelectionOutput("Selected: Windows");
        var groupName = $"GallerySelection{Guid.NewGuid():N}";

        FWRadioButton CreateOption(string text, bool isChecked = false, bool isEnabled = true)
        {
            var radioButton = new FWRadioButton
            {
                Content = text,
                GroupName = groupName,
                IsChecked = isChecked,
                IsEnabled = isEnabled
            };
            radioButton.Checked += (_, _) => output.Text = $"Selected: {text}";
            return radioButton;
        }

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 9,
            Children =
            {
                CreateOption("Windows", true),
                CreateOption("Toolkit"),
                CreateOption("Jalium"),
                CreateOption("Disabled", false, false),
                output
            }
        };
    }

    private static UIElement CreateComboBoxSelectionSample()
    {
        var output = CreateSelectionOutput("Selected: Control styles");
        var comboBox = new FWComboBox
        {
            Width = 260,
            PlaceholderText = "Choose an item"
        };
        comboBox.Items.Add(new FWComboBoxItem { Content = "Fluent tokens" });
        comboBox.Items.Add(new FWComboBoxItem { Content = "Control styles" });
        comboBox.Items.Add(new FWComboBoxItem { Content = "Gallery sample" });
        comboBox.SelectedIndex = 1;
        comboBox.SelectionChanged += (_, _) => output.Text = $"Selected: {comboBox.SelectionBoxItem}";

        var editableOutput = CreateSelectionOutput("Editable text: Custom value");
        var editableComboBox = new FWComboBox
        {
            Width = 260,
            PlaceholderText = "Type or select",
            IsEditable = true,
            Text = "Custom value",
            StaysOpenOnEdit = true
        };
        editableComboBox.Items.Add("Custom value");
        editableComboBox.Items.Add("Preset A");
        editableComboBox.Items.Add("Preset B");
        editableComboBox.SelectionChanged += (_, _) => editableOutput.Text = $"Editable text: {editableComboBox.SelectionBoxItem}";

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                CreateSelectionCaption("Inline items"),
                comboBox,
                output,
                CreateSelectionCaption("Editable"),
                editableComboBox,
                editableOutput,
                CreateSelectionCaption("Disabled"),
                CreateDisabledSelectionComboBox()
            }
        };
    }

    private static FWComboBox CreateDisabledSelectionComboBox()
    {
        var comboBox = new FWComboBox
        {
            Width = 260,
            PlaceholderText = "Disabled",
            IsEnabled = false
        };
        comboBox.Items.Add(new FWComboBoxItem { Content = "Unavailable" });
        comboBox.SelectedIndex = 0;
        return comboBox;
    }

    private static UIElement CreateComboBoxItemStateSample()
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Children =
            {
                new FWComboBoxItem { Content = "Normal item", Width = 260 },
                new FWComboBoxItem { Content = "Selected item", Width = 260, IsSelected = true },
                new FWComboBoxItem { Content = "Disabled item", Width = 260, IsEnabled = false },
                CreateSelectionOutput("Open a ComboBox above to validate live dropdown behavior.")
            }
        };
    }

    private static FWBorder CreateSelectionExampleCard(string title, string description, UIElement content)
    {
        return new FWBorder
        {
            Width = 430,
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
                    new FWTextBlock
                    {
                        Text = title,
                        FontSize = 15,
                        Foreground = ThemeBrush("TextPrimary")
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

    private static TextBlock CreateSelectionCaption(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary")
        };
    }

    private static TextBlock CreateSelectionOutput(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        };
    }

    private UIElement CreateContentLayoutSection()
    {
        var panel = CreateSection("Content and Layout");

        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 16
        };

        var textColumn = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 260
        };
        textColumn.Children.Add(new FWLabel { Content = "FWTextBlock" });
        textColumn.Children.Add(new FWTextBlock
        {
            Text = "Title text",
            FontFamily = "Segoe UI Variable Display",
            FontSize = 22,
            Foreground = ThemeBrush("TextPrimary")
        });
        textColumn.Children.Add(new FWTextBlock
        {
            Text = "Selectable body copy follows Fluent typography and wraps inside its layout column.",
            IsTextSelectionEnabled = true,
            TextWrapping = TextWrapping.Wrap,
            Foreground = ThemeBrush("TextSecondary")
        });
        textColumn.Children.Add(new FWAccessText
        {
            Text = "_Open command",
            Foreground = ThemeBrush("TextPrimary")
        });
        row.Children.Add(textColumn);

        var surfaceColumn = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 280
        };
        surfaceColumn.Children.Add(new FWLabel { Content = "FWBorder and content hosts" });
        surfaceColumn.Children.Add(new FWBorder
        {
            Background = ThemeBrush("SurfaceBackground"),
            BorderBrush = ThemeBrush("ContentSurfaceBorderBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(14),
            Child = new FWContentControl
            {
                Content = "FWContentControl hosts text content with inherited Fluent text styling.",
                Foreground = ThemeBrush("TextPrimary"),
                Padding = new Thickness(0)
            }
        });
        surfaceColumn.Children.Add(new FWContentPresenter
        {
            Content = new FWTextBlock
            {
                Text = "FWContentPresenter mirrors content without adding surface chrome.",
                Foreground = ThemeBrush("TextSecondary"),
                TextWrapping = TextWrapping.Wrap
            }
        });
        row.Children.Add(surfaceColumn);

        var layoutColumn = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 320
        };
        layoutColumn.Children.Add(new FWLabel { Content = "FWStackPanel, FWWrapPanel, FWGrid" });
        layoutColumn.Children.Add(new FWWrapPanel
        {
            HorizontalSpacing = 8,
            VerticalSpacing = 8,
            Children =
            {
                CreateLayoutChip("Stack"),
                CreateLayoutChip("Wrap"),
                CreateLayoutChip("Grid"),
                CreateLayoutChip("Content"),
                CreateLayoutChip("Surface")
            }
        });

        var grid = new FWGrid
        {
            Width = 290,
            ColumnSpacing = 8,
            RowSpacing = 8
        };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var cellA = CreateGridCell("Auto", "Row 0");
        var cellB = CreateGridCell("Star", "Column 1");
        var cellC = CreateGridCell("Span", "Two columns");
        Grid.SetColumn(cellB, 1);
        Grid.SetRow(cellC, 1);
        Grid.SetColumnSpan(cellC, 2);
        grid.Children.Add(cellA);
        grid.Children.Add(cellB);
        grid.Children.Add(cellC);
        layoutColumn.Children.Add(grid);
        row.Children.Add(layoutColumn);

        panel.Children.Add(row);
        return panel;
    }

    private UIElement CreateVisualsSection()
    {
        var panel = CreateSection("Visuals and Icons");

        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 16
        };

        var iconColumn = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 260
        };
        iconColumn.Children.Add(new FWLabel { Content = "IconElement variants" });
        iconColumn.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 18,
            Children =
            {
                new FWSymbolIcon
                {
                    Symbol = Symbol.Save,
                    Width = 24,
                    Height = 24
                },
                new FWFontIcon
                {
                    Glyph = "\uE72D",
                    FontFamily = "Segoe Fluent Icons",
                    FontSize = 24
                },
                new FWPathIcon
                {
                    Data = Geometry.Parse("M 0,10 L 8,0 L 16,10 L 12,10 L 12,18 L 4,18 L 4,10 Z"),
                    Width = 24,
                    Height = 24
                }
            }
        });
        iconColumn.Children.Add(new FWSeparator());
        iconColumn.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                new FWLabel { Content = "Target label", Width = 96 },
                new FWTextBox { Text = "FWLabel", Width = 140 }
            }
        });
        row.Children.Add(iconColumn);

        var imageColumn = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 260
        };
        imageColumn.Children.Add(new FWLabel { Content = "FWImage stretch" });
        imageColumn.Children.Add(new FWImage
        {
            Width = 220,
            Height = 120,
            Source = CreateSampleBitmap(),
            Stretch = Stretch.UniformToFill,
            IsZoomEnabled = true
        });
        imageColumn.Children.Add(new TextBlock
        {
            Text = "UniformToFill with Fluent border and clipping.",
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        });
        row.Children.Add(imageColumn);

        var viewboxColumn = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 260
        };
        viewboxColumn.Children.Add(new FWLabel { Content = "FWViewbox" });
        viewboxColumn.Children.Add(new FWViewbox
        {
            Width = 220,
            Height = 120,
            Stretch = Stretch.Uniform,
            Child = new Border
            {
                Width = 140,
                Height = 70,
                Background = ThemeBrush("SelectionBackgroundWeak"),
                BorderBrush = ThemeBrush("AccentBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12),
                Child = new TextBlock
                {
                    Text = "Scaled Fluent surface",
                    Foreground = ThemeBrush("TextPrimary"),
                    TextWrapping = TextWrapping.Wrap,
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        });
        viewboxColumn.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                new TextBlock { Text = "Left", Foreground = ThemeBrush("TextSecondary") },
                new FWSeparator { Orientation = Orientation.Vertical, Height = 18 },
                new TextBlock { Text = "Right", Foreground = ThemeBrush("TextSecondary") }
            }
        });
        row.Children.Add(viewboxColumn);

        panel.Children.Add(row);
        return panel;
    }

    private UIElement CreateInteractionSection()
    {
        var panel = CreateSection("Interaction and Scrolling");

        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 16
        };

        var scrollColumn = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 300
        };
        scrollColumn.Children.Add(new FWLabel { Content = "FWScrollViewer" });
        scrollColumn.Children.Add(new FWScrollViewer
        {
            Width = 280,
            Height = 150,
            VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Background = ThemeBrush("SwipeControlBackground"),
            BorderBrush = ThemeBrush("SwipeControlBorderBrush"),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(12),
            IsScrollBarAutoHideEnabled = false,
            Content = CreateScrollableItems()
        });
        row.Children.Add(scrollColumn);

        var swipeColumn = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 330
        };
        swipeColumn.Children.Add(new FWLabel { Content = "FWSwipeControl" });
        swipeColumn.Children.Add(new FWSwipeControl
        {
            Width = 310,
            Height = 68,
            LeftItems = CreateSwipeItems(SwipeMode.Reveal,
                ("Archive", "\uE8C3", "SwipeItemBackgroundSecondary"),
                ("Flag", "\uE7C1", "SwipeItemBackground")),
            RightItems = CreateSwipeItems(SwipeMode.Execute,
                ("Delete", "\uE74D", "SwipeItemBackgroundDestructive")),
            Content = new Border
            {
                Background = ThemeBrush("SwipeControlBackground"),
                Padding = new Thickness(14, 8, 14, 8),
                Child = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Spacing = 2,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = "Swipe action row",
                            Foreground = ThemeBrush("TextPrimary")
                        },
                        new TextBlock
                        {
                            Text = "Reveal from the left, execute from the right.",
                            Foreground = ThemeBrush("TextSecondary"),
                            FontSize = 12
                        }
                    }
                }
            }
        });
        row.Children.Add(swipeColumn);

        var splitterColumn = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 390
        };
        splitterColumn.Children.Add(new FWLabel { Content = "FWGridSplitter" });
        splitterColumn.Children.Add(CreateSplitterPreview());
        row.Children.Add(splitterColumn);

        panel.Children.Add(row);
        return panel;
    }

    private UIElement CreateAdvancedInputMediaSection()
    {
        var panel = CreateSection("Advanced Input and Media");

        var topRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 16
        };

        var colorColumn = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 260
        };
        colorColumn.Children.Add(new FWLabel { Content = "FWColorPicker" });
        colorColumn.Children.Add(new FWColorPicker
        {
            Color = Color.FromRgb(0x00, 0x78, 0xD4),
            IsAlphaEnabled = true,
            IsColorPreviewVisible = true,
            IsHexInputVisible = true
        });
        colorColumn.Children.Add(new FWColorPicker
        {
            Color = Color.FromRgb(0xD8, 0x3B, 0x01),
            IsCompact = true,
            IsAlphaEnabled = false
        });
        topRow.Children.Add(colorColumn);

        var inkColumn = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 320
        };
        inkColumn.Children.Add(new FWLabel { Content = "FWInkCanvas" });
        inkColumn.Children.Add(new FWInkCanvas
        {
            Width = 300,
            Height = 180,
            Background = ThemeBrush("InkCanvasBackground"),
            BorderBrush = ThemeBrush("InkCanvasBorderBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            DefaultDrawingAttributes = CreateInkDrawingAttributes(),
            DefaultStrokeTaperMode = StrokeTaperMode.TaperedEnd,
            EditingMode = InkCanvasEditingMode.Ink
        });
        inkColumn.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                new FWButton { Content = "Draw" },
                new FWButton { Content = "Erase" },
                new FWButton { Content = "Clear", IsEnabled = false }
            }
        });
        topRow.Children.Add(inkColumn);

        var bottomRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 16
        };

        var presenterColumn = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 260
        };
        presenterColumn.Children.Add(new FWLabel { Content = "FWInkPresenter" });
        presenterColumn.Children.Add(new Border
        {
            Width = 240,
            Height = 120,
            Background = ThemeBrush("InkCanvasBackground"),
            BorderBrush = ThemeBrush("InkCanvasBorderBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Child = new FWInkPresenter
            {
                Strokes = CreateSampleStrokes()
            }
        });
        presenterColumn.Children.Add(new TextBlock
        {
            Text = "A presentation surface for existing stroke collections.",
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        });
        bottomRow.Children.Add(presenterColumn);

        var mediaColumn = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 280
        };
        mediaColumn.Children.Add(new FWLabel { Content = "FWMediaElement" });
        mediaColumn.Children.Add(new Grid
        {
            Width = 260,
            Height = 146,
            Children =
            {
                new FWMediaElement
                {
                    Width = 260,
                    Height = 146,
                    Background = ThemeBrush("MediaElementBackground"),
                    BorderBrush = ThemeBrush("MediaElementBorderBrush"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    LoadedBehavior = MediaState.Manual,
                    UnloadedBehavior = MediaState.Close,
                    Stretch = Stretch.Uniform
                },
                new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Spacing = 6,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Children =
                    {
                        new FWFontIcon
                        {
                            Glyph = "\uE768",
                            FontFamily = "Segoe Fluent Icons",
                            FontSize = 28,
                            Foreground = ThemeBrush("MediaElementForeground")
                        },
                        new TextBlock
                        {
                            Text = "Media surface",
                            Foreground = ThemeBrush("MediaElementForeground")
                        }
                    }
                }
            }
        });
        mediaColumn.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                new FWButton { Content = "Play" },
                new FWButton { Content = "Pause" },
                new FWButton { Content = "Stop", IsEnabled = false }
            }
        });
        bottomRow.Children.Add(mediaColumn);

        panel.Children.Add(topRow);
        panel.Children.Add(bottomRow);
        return panel;
    }

    private UIElement CreateMenusSection()
    {
        var panel = CreateSection("Menus and Flyouts");
        var menuBar = new FWMenuBar
        {
            Width = 520
        };
        menuBar.Items.Add(CreateMenuBarItem("File", ("New", "Ctrl+N"), ("Open", "Ctrl+O"), ("Save", "Ctrl+S")));
        menuBar.Items.Add(CreateMenuBarItem("Edit", ("Undo", "Ctrl+Z"), ("Redo", "Ctrl+Y"), ("Preferences", string.Empty)));
        menuBar.Items.Add(CreateMenuBarItem("View", ("Zoom in", "Ctrl++"), ("Zoom out", "Ctrl+-"), ("Actual size", "Ctrl+0")));

        var menu = new FWMenu
        {
            Width = 520
        };
        var project = new FWMenuItem
        {
            Header = "Project"
        };
        project.Items.Add(new FWMenuItem { Header = "Build", InputGestureText = "Ctrl+B", Icon = "\uE768" });
        project.Items.Add(new FWMenuItem { Header = "Run", InputGestureText = "F5", Icon = "\uE768" });
        project.Items.Add(new FWMenuItem { Header = "Live preview", IsCheckable = true, IsChecked = true });
        menu.Items.Add(project);
        menu.Items.Add(new FWMenuItem { Header = "Tools" });
        menu.Items.Add(new FWMenuItem { Header = "Disabled", IsEnabled = false });

        var flyoutButton = new FWDropDownButton
        {
            Content = "MenuFlyout items",
            Width = 180,
            Flyout = CreateMenuControlsFlyout()
        };

        var contextMenu = new FWContextMenu
        {
            Placement = PlacementMode.Bottom,
            StaysOpen = true
        };
        contextMenu.Items.Add(new FWMenuItem { Header = "Refresh", InputGestureText = "F5", Icon = "\uE72C" });
        contextMenu.Items.Add(new FWMenuItem { Header = "Rename", InputGestureText = "F2", Icon = "\uE70F" });
        contextMenu.Items.Add(new FWMenuItem { Header = "Show details", IsCheckable = true, IsChecked = true });

        var contextButton = new FWButton
        {
            Content = "Open context menu",
            Width = 180,
            ContextMenu = contextMenu
        };
        contextButton.Click += (_, _) => ContextMenuService.Open(contextButton, contextMenu);

        var topRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 16
        };
        topRow.Children.Add(menuBar);
        topRow.Children.Add(flyoutButton);
        topRow.Children.Add(contextButton);

        panel.Children.Add(topRow);
        panel.Children.Add(menu);
        return panel;
    }

    private UIElement CreateDisclosureDialogsSection()
    {
        var panel = CreateSection("Disclosure and Dialogs");

        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 16
        };

        var expanderColumn = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 330
        };
        expanderColumn.Children.Add(new FWExpander
        {
            Header = "FWExpander",
            IsExpanded = true,
            Content = new TextBlock
            {
                Text = "Expanded content keeps a subtle Fluent surface and accent chevron state.",
                TextWrapping = TextWrapping.Wrap,
                Foreground = ThemeBrush("TextPrimary")
            }
        });
        expanderColumn.Children.Add(new FWExpander
        {
            Header = "Disabled",
            IsExpanded = false,
            IsEnabled = false,
            Content = new TextBlock
            {
                Text = "Disabled expander",
                Foreground = ThemeBrush("TextSecondary")
            }
        });
        row.Children.Add(expanderColumn);

        var groupBox = new FWGroupBox
        {
            Header = "FWGroupBox",
            Width = 300,
            Content = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 8,
                Children =
                {
                    new FWCheckBox { Content = "Group option", IsChecked = true },
                    new FWTextBox { Text = "Grouped text", Width = 240 },
                    new FWButton { Content = "Apply" }
                }
            }
        };
        row.Children.Add(groupBox);

        var actionColumn = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Width = 270
        };
        var tipButton = new FWButton
        {
            Content = "Hover for FWToolTip",
            Width = 190,
            ToolTip = new FWToolTip
            {
                Content = new TextBlock
                {
                    Text = "FWToolTip follows Fluent popup resources.",
                    Foreground = ThemeBrush("ToolTipForeground")
                },
                Placement = PlacementMode.Top,
                InitialShowDelay = 200
            }
        };
        var dialogResult = new TextBlock
        {
            Text = "Dialog result: not shown",
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        };
        var dialogButton = new FWButton
        {
            Content = "Show FWContentDialog",
            Width = 190
        };
        dialogButton.Click += async (_, _) => await ShowDisclosureDialogAsync(dialogResult);

        actionColumn.Children.Add(tipButton);
        actionColumn.Children.Add(dialogButton);
        actionColumn.Children.Add(dialogResult);
        row.Children.Add(actionColumn);

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
        var panel = CreateSection("Range and Progress");

        var sliderValueOutput = CreateRangeOutput("Value: 64");
        var slider = new FWSlider
        {
            Width = 320,
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

        var sliderStates = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 20
        };
        sliderStates.Children.Add(new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Children =
            {
                CreateRangeOutput("Stepped"),
                new FWSlider
                {
                    Width = 220,
                    Minimum = 0,
                    Maximum = 100,
                    Value = 40,
                    TickFrequency = 20,
                    IsSnapToTickEnabled = true
                }
            }
        });
        sliderStates.Children.Add(new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Children =
            {
                CreateRangeOutput("Vertical"),
                verticalSlider,
                verticalSliderOutput
            }
        });
        sliderStates.Children.Add(new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Children =
            {
                CreateRangeOutput("Disabled"),
                new FWSlider
                {
                    Width = 220,
                    Minimum = 0,
                    Maximum = 100,
                    Value = 55,
                    IsEnabled = false
                }
            }
        });

        var rangeOutput = CreateRangeOutput("Range: 24 to 76");
        var rangeSlider = new FWRangeSlider
        {
            Width = 320,
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

        var rangeStates = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 20
        };
        rangeStates.Children.Add(new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Children =
            {
                CreateRangeOutput("MinimumRange = 20"),
                new FWRangeSlider
                {
                    Width = 220,
                    Minimum = 0,
                    Maximum = 100,
                    RangeStart = 30,
                    RangeEnd = 70,
                    MinimumRange = 20,
                    TickFrequency = 10,
                    IsSnapToTickEnabled = true
                }
            }
        });
        rangeStates.Children.Add(new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Children =
            {
                CreateRangeOutput("Disabled"),
                new FWRangeSlider
                {
                    Width = 220,
                    Minimum = 0,
                    Maximum = 100,
                    RangeStart = 18,
                    RangeEnd = 64,
                    MinimumRange = 8,
                    IsEnabled = false
                }
            }
        });

        var progressOutput = CreateRangeOutput("Progress: 72%");
        var progressBar = new FWProgressBar
        {
            Width = 320,
            Height = 8,
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

        var progressStates = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10
        };
        progressStates.Children.Add(CreateRangeStateRow("Determinate", new FWProgressBar
        {
            Width = 260,
            Height = 8,
            Minimum = 0,
            Maximum = 100,
            Value = 35
        }));
        progressStates.Children.Add(CreateRangeStateRow("Indeterminate", new FWProgressBar
        {
            Width = 260,
            Height = 8,
            IsIndeterminate = true
        }));
        progressStates.Children.Add(CreateRangeStateRow("Disabled", new FWProgressBar
        {
            Width = 260,
            Height = 8,
            Minimum = 0,
            Maximum = 100,
            Value = 55,
            IsEnabled = false
        }));

        var ringOutput = CreateRangeOutput("Ring value: 72%");
        var determinateRing = new FWProgressRing
        {
            Width = 48,
            Height = 48,
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

        var ringStates = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 22
        };
        ringStates.Children.Add(CreateRangeRingState("Indeterminate", new FWProgressRing
        {
            Width = 48,
            Height = 48,
            IsIndeterminate = true
        }));
        ringStates.Children.Add(CreateRangeRingState("Determinate", determinateRing));
        ringStates.Children.Add(CreateRangeRingState("Inactive", new FWProgressRing
        {
            Width = 48,
            Height = 48,
            Value = 42,
            IsActive = false,
            IsIndeterminate = false
        }));
        ringStates.Children.Add(CreateRangeRingState("Disabled", new FWProgressRing
        {
            Width = 48,
            Height = 48,
            Value = 64,
            IsIndeterminate = false,
            IsEnabled = false
        }));

        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };
        examples.Children.Add(CreateRangeExampleCard(
            "FWSlider",
            "Interactive value output, stepped movement, vertical orientation, and disabled state.",
            slider,
            sliderValueOutput,
            sliderStates));
        examples.Children.Add(CreateRangeExampleCard(
            "FWRangeSlider",
            "Two-thumb value output with snapped steps, minimum range, and disabled state.",
            rangeSlider,
            rangeOutput,
            rangeStates));
        examples.Children.Add(CreateRangeExampleCard(
            "FWProgressBar",
            "Determinate value driven by a slider plus indeterminate and disabled states.",
            progressBar,
            progressOutput,
            progressSlider,
            progressStates));
        examples.Children.Add(CreateRangeExampleCard(
            "FWProgressRing",
            "Active indeterminate ring, determinate value output, inactive, and disabled states.",
            ringStates,
            ringOutput,
            ringSlider));

        panel.Children.Add(examples);
        return panel;
    }

    private static FWBorder CreateRangeExampleCard(string title, string description, params UIElement[] children)
    {
        var stack = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10
        };
        stack.Children.Add(new FWTextBlock
        {
            Text = title,
            FontSize = 15,
            Foreground = ThemeBrush("TextPrimary")
        });
        stack.Children.Add(new FWTextBlock
        {
            Text = description,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        });

        foreach (var child in children)
        {
            stack.Children.Add(child);
        }

        return new FWBorder
        {
            Width = 430,
            Background = ThemeBrush("ControlBackground"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(14),
            Child = stack
        };
    }

    private static StackPanel CreateRangeStateRow(string label, UIElement control)
    {
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Children =
            {
                new TextBlock
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

    private static StackPanel CreateRangeRingState(string label, UIElement ring)
    {
        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Children =
            {
                ring,
                new TextBlock
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
            Foreground = ThemeBrush("TextSecondary")
        };
    }

    private static string FormatRangeValue(string label, double value)
    {
        return $"{label}: {value:0}";
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

    private static async Task ShowDisclosureDialogAsync(TextBlock resultText)
    {
        var dialog = new FWContentDialog
        {
            Title = "Save gallery changes?",
            PrimaryButtonText = "Save",
            SecondaryButtonText = "Review",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            Content = new TextBlock
            {
                Text = "FWContentDialog uses the Fluent dialog card, overlay, title, and command button resources.",
                Foreground = ThemeBrush("TextPrimary"),
                TextWrapping = TextWrapping.Wrap,
                Width = 340
            }
        };

        var result = await dialog.ShowAsync();
        resultText.Text = $"Dialog result: {result}";
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

    private static FWBorder CreateLayoutChip(string text)
    {
        return new FWBorder
        {
            Background = ThemeBrush("SelectionBackgroundWeak"),
            BorderBrush = ThemeBrush("AccentBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(10, 4, 10, 4),
            Child = new FWTextBlock
            {
                Text = text,
                Foreground = ThemeBrush("TextPrimary"),
                FontSize = 12
            }
        };
    }

    private static FWBorder CreateGridCell(string title, string detail)
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
                Spacing = 3,
                Children =
                {
                    new FWTextBlock
                    {
                        Text = title,
                        Foreground = ThemeBrush("TextPrimary"),
                        FontSize = 13
                    },
                    new FWTextBlock
                    {
                        Text = detail,
                        Foreground = ThemeBrush("TextSecondary"),
                        FontSize = 12
                    }
                }
            }
        };
    }

    private static StackPanel CreateScrollableItems()
    {
        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 6
        };

        for (var index = 1; index <= 9; index++)
        {
            stack.Children.Add(new Border
            {
                Height = 28,
                Background = index % 2 == 0 ? ThemeBrush("ControlBackground") : ThemeBrush("SelectionBackgroundWeak"),
                BorderBrush = ThemeBrush("ControlBorder"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(8, 0, 8, 0),
                Child = new TextBlock
                {
                    Text = $"Scrollable item {index}",
                    Foreground = ThemeBrush("TextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            });
        }

        return stack;
    }

    private static SwipeItems CreateSwipeItems(SwipeMode mode, params (string Text, string Icon, string BackgroundKey)[] items)
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
                IconSource = item.Icon,
                Background = ThemeBrush(item.BackgroundKey),
                Foreground = ThemeBrush("SwipeItemForeground")
            });
        }

        return swipeItems;
    }

    private static Grid CreateSplitterPreview()
    {
        var grid = new Grid
        {
            Width = 370,
            Height = 150,
            Background = ThemeBrush("SwipeControlBackground")
        };
        grid.ColumnDefinitions.Add(new ColumnDefinition
        {
            Width = new GridLength(150),
            MinWidth = 96
        });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(6) });
        grid.ColumnDefinitions.Add(new ColumnDefinition
        {
            Width = new GridLength(1, GridUnitType.Star),
            MinWidth = 140
        });

        var left = CreateSplitterPane("Outline", "Theme tokens\nFW controls\nGallery states");
        Grid.SetColumn(left, 0);
        grid.Children.Add(left);

        var splitter = new FWGridSplitter
        {
            Width = 6,
            ResizeDirection = GridResizeDirection.Columns,
            ResizeBehavior = GridResizeBehavior.PreviousAndNext,
            KeyboardIncrement = 12,
            DragIncrement = 1
        };
        Grid.SetColumn(splitter, 1);
        grid.Children.Add(splitter);

        var right = CreateSplitterPane("Preview", "Drag the splitter to resize this Fluent layout.");
        Grid.SetColumn(right, 2);
        grid.Children.Add(right);

        return grid;
    }

    private static Border CreateSplitterPane(string title, string text)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8
        };
        panel.Children.Add(new TextBlock
        {
            Text = title,
            Foreground = ThemeBrush("TextPrimary"),
            FontSize = 14
        });
        panel.Children.Add(new TextBlock
        {
            Text = text,
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        });

        return new Border
        {
            Background = ThemeBrush("ControlBackground"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(12),
            Child = panel
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

    private static FWMenuBarItem CreateMenuBarItem(string title, params (string Text, string Shortcut)[] items)
    {
        var menuBarItem = new FWMenuBarItem
        {
            Title = title
        };

        for (var index = 0; index < items.Length; index++)
        {
            var (text, shortcut) = items[index];
            menuBarItem.Items.Add(new FWMenuFlyoutItem
            {
                Text = text,
                KeyboardAcceleratorTextOverride = shortcut,
                Icon = index == 0 ? "\uE8A5" : null
            });
        }

        if (items.Length > 1)
        {
            menuBarItem.Items.Insert(items.Length - 1, new FWMenuFlyoutSeparator());
        }

        return menuBarItem;
    }

    private static MenuFlyout CreateMenuControlsFlyout()
    {
        var flyout = new MenuFlyout();
        flyout.Items.Add(new FWMenuFlyoutItem { Text = "Pin", Icon = "\uE718", KeyboardAcceleratorTextOverride = "Ctrl+P" });
        flyout.Items.Add(new FWToggleMenuFlyoutItem { Text = "Show badges", IsChecked = true });
        flyout.Items.Add(new FWMenuFlyoutSeparator());
        flyout.Items.Add(new FWMenuFlyoutItem { Text = "Settings", Icon = "\uE713" });
        flyout.Items.Add(new FWMenuFlyoutItem { Text = "Disabled", IsEnabled = false });
        return flyout;
    }

    private static MenuFlyout CreateSampleFlyout()
    {
        var flyout = new MenuFlyout();
        flyout.Items.Add(new FWMenuFlyoutItem { Text = "Create" });
        flyout.Items.Add(new FWMenuFlyoutItem { Text = "Open" });
        flyout.Items.Add(new FWMenuFlyoutSeparator());
        flyout.Items.Add(new FWMenuFlyoutItem { Text = "Export" });
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

    private static BitmapImage CreateSampleBitmap()
    {
        const int width = 96;
        const int height = 64;
        var pixels = new byte[width * height * 4];

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var index = ((y * width) + x) * 4;
                pixels[index] = (byte)(0x72 + (x * 0x30 / width));
                pixels[index + 1] = (byte)(0x45 + (y * 0x70 / height));
                pixels[index + 2] = (byte)(0xD4 - (x * 0x60 / width));
                pixels[index + 3] = 0xFF;
            }
        }

        return BitmapImage.FromPixels(pixels, width, height);
    }

    private static DrawingAttributes CreateInkDrawingAttributes()
    {
        return new DrawingAttributes
        {
            Color = Color.FromRgb(0x4C, 0xC2, 0xFF),
            Width = 3,
            Height = 3,
            BrushType = BrushType.Pen,
            FitToCurve = true
        };
    }

    private static StrokeCollection CreateSampleStrokes()
    {
        return new StrokeCollection
        {
            CreateStroke(
                Color.FromRgb(0x4C, 0xC2, 0xFF),
                new StylusPoint(24, 74),
                new StylusPoint(62, 42),
                new StylusPoint(108, 72),
                new StylusPoint(162, 34),
                new StylusPoint(210, 64)),
            CreateStroke(
                Color.FromRgb(0xD8, 0x3B, 0x01),
                new StylusPoint(28, 92),
                new StylusPoint(84, 102),
                new StylusPoint(142, 90),
                new StylusPoint(202, 104))
        };
    }

    private static Stroke CreateStroke(Color color, params StylusPoint[] points)
    {
        return new Stroke(
            new StylusPointCollection(points),
            new DrawingAttributes
            {
                Color = color,
                Width = 4,
                Height = 4,
                BrushType = BrushType.Pen,
                FitToCurve = true
            })
        {
            TaperMode = StrokeTaperMode.TaperedEnd
        };
    }

    private void ApplyTheme(FluentThemeVariant theme)
    {
        FluentThemeManager.ApplyTheme(theme);
        RefreshCurrentPage();
    }

    private void ApplyAccent(Color accent)
    {
        FluentThemeManager.ApplyAccent(accent);
        RefreshCurrentPage();
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

    private sealed record GalleryPage(
        string Title,
        string Description,
        string Group,
        string Glyph,
        Func<UIElement> CreateContent,
        string SearchText,
        bool IsFooter = false);

    private static bool MatchesNavigationSearch(GalleryPage page, string searchText)
    {
        var query = searchText.Trim();
        if (query.Length == 0)
        {
            return true;
        }

        foreach (var token in query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!ContainsIgnoreCase(page.Title, token)
                && !ContainsIgnoreCase(page.Description, token)
                && !ContainsIgnoreCase(page.Group, token)
                && !ContainsIgnoreCase(page.SearchText, token))
            {
                return false;
            }
        }

        return true;
    }

    private static bool ContainsIgnoreCase(string value, string query)
    {
        return value.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static class GalleryNavigationGroup
    {
        public const string Home = "Home";
        public const string ControlSurfaces = "Control surfaces";
        public const string Input = "Input";
        public const string LayoutAndMedia = "Layout and media";
        public const string CollectionsAndData = "Collections and data";
        public const string AppStructure = "App structure";
        public const string Diagnostics = "Diagnostics";

        public static readonly string[] Order =
        [
            ControlSurfaces,
            Input,
            LayoutAndMedia,
            CollectionsAndData,
            AppStructure
        ];

        public static string GetGlyph(string groupName)
        {
            return groupName switch
            {
                ControlSurfaces => "\uE8FD",
                Input => "\uE8D2",
                LayoutAndMedia => "\uE8A9",
                CollectionsAndData => "\uE8F1",
                AppStructure => "\uE700",
                Diagnostics => "\uE9D9",
                _ => "\uE80F"
            };
        }
    }
}
