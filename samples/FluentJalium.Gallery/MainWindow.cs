using FluentJalium.Controls.Themes;
using FluentJalium.Gallery.Models;
using FluentJalium.Gallery.Pages;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Ink;
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
using FWCommandBar = FluentJalium.Controls.FWCommandBar;
using FWCommandBarFlyout = FluentJalium.Controls.FWCommandBarFlyout;
using FWDatePicker = FluentJalium.Controls.FWDatePicker;
using FWDiffViewer = FluentJalium.Controls.FWDiffViewer;
using FWDropDownButton = FluentJalium.Controls.FWDropDownButton;
using FWExpander = FluentJalium.Controls.FWExpander;
using FWFrame = FluentJalium.Controls.FWFrame;
using FWGroupBox = FluentJalium.Controls.FWGroupBox;
using FWHexEditor = FluentJalium.Controls.FWHexEditor;
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
using FWContextMenu = FluentJalium.Controls.FWContextMenu;
using FWMenu = FluentJalium.Controls.FWMenu;
using FWMenuBar = FluentJalium.Controls.FWMenuBar;
using FWMenuBarItem = FluentJalium.Controls.FWMenuBarItem;
using FWMenuFlyout = FluentJalium.Controls.FWMenuFlyout;
using FWMenuFlyoutItem = FluentJalium.Controls.FWMenuFlyoutItem;
using FWMenuFlyoutSeparator = FluentJalium.Controls.FWMenuFlyoutSeparator;
using FWMenuFlyoutSubItem = FluentJalium.Controls.FWMenuFlyoutSubItem;
using FWMenuItem = FluentJalium.Controls.FWMenuItem;
using FWMediaElement = FluentJalium.Controls.FWMediaElement;
using FWNavigationView = FluentJalium.Controls.FWNavigationView;
using FWNavigationViewItem = FluentJalium.Controls.FWNavigationViewItem;
using FWNavigationViewItemHeader = FluentJalium.Controls.FWNavigationViewItemHeader;
using FWNavigationViewItemSeparator = FluentJalium.Controls.FWNavigationViewItemSeparator;
using FWJsonTreeViewer = FluentJalium.Controls.FWJsonTreeViewer;
using FWNumberBox = FluentJalium.Controls.FWNumberBox;
using FWPasswordBox = FluentJalium.Controls.FWPasswordBox;
using FWPropertyGrid = FluentJalium.Controls.FWPropertyGrid;
using FWProgressBar = FluentJalium.Controls.FWProgressBar;
using FWPathIcon = FluentJalium.Controls.FWPathIcon;
using FWRepeatButton = FluentJalium.Controls.FWRepeatButton;
using FWRichTextBox = FluentJalium.Controls.FWRichTextBox;
using FWSeparator = FluentJalium.Controls.FWSeparator;
using FWSplitButton = FluentJalium.Controls.FWSplitButton;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWStatusBar = FluentJalium.Controls.FWStatusBar;
using FWScrollViewer = FluentJalium.Controls.FWScrollViewer;
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
using FWToolBar = FluentJalium.Controls.FWToolBar;
using FWToolBarTray = FluentJalium.Controls.FWToolBarTray;
using FWTreeSelector = FluentJalium.Controls.FWTreeSelector;
using FWTreeSelectorItem = FluentJalium.Controls.FWTreeSelectorItem;
using FWToggleButton = FluentJalium.Controls.FWToggleButton;
using FWToggleMenuFlyoutItem = FluentJalium.Controls.FWToggleMenuFlyoutItem;
using FWToggleSplitButton = FluentJalium.Controls.FWToggleSplitButton;
using FWToggleSwitch = FluentJalium.Controls.FWToggleSwitch;
using FWViewbox = FluentJalium.Controls.FWViewbox;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;

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
        _pages = GalleryCatalog.Create(CreateGalleryPageContentFactories());
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

    private GalleryPageContentFactories CreateGalleryPageContentFactories()
    {
        return new GalleryPageContentFactories(
            Overview: () => CreatePageStack(CreateThemeControls()),
            Buttons: () => CreatePageStack(CreateButtonsSection(), CreateCommandButtonsSection()),
            Switches: () => CreatePageStack(new GallerySwitchesPage().CreateContent()),
            TextInput: () => CreatePageStack(CreateTextSection()),
            Selection: () => CreatePageStack(new GallerySelectionPage().CreateContent()),
            Range: () => CreatePageStack(new GalleryRangePage().CreateContent()),
            DateAndTime: () => CreatePageStack(CreateDateTimeSection()),
            ContentAndLayout: () => CreatePageStack(CreateContentLayoutSection()),
            Visuals: () => CreatePageStack(CreateVisualsSection()),
            Interaction: () => CreatePageStack(CreateInteractionSection()),
            InputAndMedia: () => CreatePageStack(CreateAdvancedInputMediaSection()),
            Collections: () => CreatePageStack(new GalleryCollectionsPage().CreateContent()),
            SelectorsAndProperties: () => CreatePageStack(CreateAdvancedSelectionPropertiesSection()),
            DataInspectors: () => CreatePageStack(CreateDataInspectorsSection()),
            Navigation: () => CreatePageStack(CreateNavigationSection()),
            MaterialsAndEffects: () => CreatePageStack(new GalleryMaterialsPage(this).CreateContent()),
            MotionAndTransitions: () => CreatePageStack(new GalleryMotionPage().CreateContent()),
            Menus: () => CreatePageStack(CreateMenusSection()),
            Disclosure: () => CreatePageStack(CreateDisclosureDialogsSection()),
            Status: () => CreatePageStack(CreateStatusSection()),
            StateMatrix: () => CreatePageStack(CreateStateMatrix()));
    }

    private void PopulateNavigationItems(FWNavigationView navigationView, GalleryPage[] pages, string searchText)
    {
        _navigationItems.Clear();
        navigationView.MenuItems.Clear();
        navigationView.FooterMenuItems.Clear();

        var matchingPages = pages
            .Where(page => page.MatchesSearch(searchText))
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
            Icon = CreateIcon(GalleryNavigationGroup.GetIcon(groupName)),
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
            Icon = CreateIcon(page.Icon),
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

        panel.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Children =
            {
                CreateIcon(FluentIconRegular.WindowBrush24, 24),
                new TextBlock
                {
                    Text = "FluentJalium",
                    FontSize = 24,
                    FontFamily = "Segoe UI Variable Display",
                    Foreground = ThemeBrush("TextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
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

        stack.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Children =
            {
                CreateIcon(page.Icon, 30),
                new TextBlock
                {
                    Text = page.Title,
                    FontSize = 30,
                    FontFamily = "Segoe UI Variable Display",
                    Foreground = ThemeBrush("TextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
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

        stack.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Children =
            {
                CreateIcon(FluentIconRegular.SearchInfo24, 28),
                new TextBlock
                {
                    Text = "No results",
                    FontSize = 28,
                    FontFamily = "Segoe UI Variable Display",
                    Foreground = ThemeBrush("TextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
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

    private static FWBorder CreateCollectionExampleCard(string title, string description, UIElement content)
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

    private static FWWrapPanel CreateCollectionButtonRow(params FWButton[] buttons)
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

    private static FWButton CreateCollectionActionButton(string text, Action action)
    {
        var button = new FWButton
        {
            Content = text
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static TextBlock CreateCollectionOutput(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        };
    }

    private UIElement CreateAdvancedSelectionPropertiesSection()
    {
        var panel = CreateSection("Selectors and Properties");
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        examples.Children.Add(CreateCollectionExampleCard(
            "FWTreeSelector",
            "Searchable hierarchical picker with a path display, dropdown state, and selection output.",
            CreateTreeSelectorSingleSelectionSample()));
        examples.Children.Add(CreateCollectionExampleCard(
            "FWTreeSelectorItem cascade",
            "Multiple selection with checkboxes, cascade state, selected chips, and tree item expansion.",
            CreateTreeSelectorCascadeSample()));
        examples.Children.Add(CreateCollectionExampleCard(
            "FWPropertyGrid",
            "Object property editing with search, categorized/alphabetical modes, read-only state, and description area.",
            CreatePropertyGridSample()));

        panel.Children.Add(examples);
        return panel;
    }

    private static UIElement CreateTreeSelectorSingleSelectionSample()
    {
        var output = CreateCollectionOutput("Selected: none. Drop-down: closed");
        var selector = new FWTreeSelector
        {
            Width = 380,
            SelectionMode = SelectionMode.Single,
            PlaceholderText = "Choose a workspace area",
            IsSearchEnabled = true,
            MaxDropDownHeight = 240
        };
        PopulateGalleryTreeSelector(selector);

        void UpdateOutput()
        {
            output.Text = selector.SelectedItem == null
                ? $"Selected: none. Search: {FormatSearchText(selector.SearchText)}. Drop-down: {FormatOpenState(selector.IsDropDownOpen)}"
                : $"Selected: {selector.SelectedItem}. Search: {FormatSearchText(selector.SearchText)}. Drop-down: {FormatOpenState(selector.IsDropDownOpen)}";
        }

        selector.SelectionChanged += (_, _) => UpdateOutput();
        selector.DropDownOpened += (_, _) => UpdateOutput();
        selector.DropDownClosed += (_, _) => UpdateOutput();

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                selector,
                CreateCollectionButtonRow(
                    CreateIconCollectionActionButton(FluentIconRegular.Search24, "Search data", () =>
                    {
                        selector.IsDropDownOpen = true;
                        selector.SearchText = "data";
                        UpdateOutput();
                    }),
                    CreateIconCollectionActionButton(FluentIconRegular.CheckmarkCircle24, "Select themes", () =>
                    {
                        selector.SearchText = string.Empty;
                        selector.IsDropDownOpen = true;
                        selector.SelectedItem = "Theme resources";
                        UpdateOutput();
                    }),
                    CreateIconCollectionActionButton(FluentIconRegular.Dismiss24, "Clear", () =>
                    {
                        selector.UnselectAll();
                        selector.SearchText = string.Empty;
                        UpdateOutput();
                    }),
                    CreateIconCollectionActionButton(FluentIconRegular.ChevronDown24, "Toggle", () =>
                    {
                        selector.IsDropDownOpen = !selector.IsDropDownOpen;
                        UpdateOutput();
                    })),
                output
            }
        };
    }

    private static UIElement CreateTreeSelectorCascadeSample()
    {
        var output = CreateCollectionOutput("Checked: none. Selected: none");
        var selector = new FWTreeSelector
        {
            Width = 400,
            SelectionMode = SelectionMode.Multiple,
            ShowCheckBoxes = true,
            CheckCascadeMode = TreeSelectorCheckCascadeMode.Cascade,
            PlaceholderText = "Choose related areas",
            MaxDropDownHeight = 260,
            IsDropDownOpen = true
        };
        var foundation = CreateGalleryTreeSelectorItem("Foundation", isExpanded: true,
            CreateGalleryTreeSelectorItem("Theme resources"),
            CreateGalleryTreeSelectorItem("Typography"),
            CreateGalleryTreeSelectorItem("Icon module"));
        var controls = CreateGalleryTreeSelectorItem("Controls", isExpanded: true,
            CreateGalleryTreeSelectorItem("Buttons"),
            CreateGalleryTreeSelectorItem("Selectors"),
            CreateGalleryTreeSelectorItem("Tables"));
        selector.Items.Add(foundation);
        selector.Items.Add(controls);

        void UpdateOutput()
        {
            var checkedText = selector.CheckedItems.Count == 0 ? "none" : FormatCollectionItems(selector.CheckedItems);
            var selectedText = selector.SelectedItems.Count == 0 ? "none" : FormatCollectionItems(selector.SelectedItems);
            output.Text = $"Checked: {checkedText}. Selected: {selectedText}. Foundation: {FormatNullableBool(foundation.IsChecked)}";
        }

        selector.SelectionChanged += (_, _) => UpdateOutput();
        selector.ItemCheckStateChanged += (_, _) => UpdateOutput();
        UpdateOutput();

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                selector,
                CreateCollectionButtonRow(
                    CreateIconCollectionActionButton(FluentIconRegular.CheckboxChecked24, "Check root", () =>
                    {
                        foundation.IsChecked = true;
                        UpdateOutput();
                    }),
                    CreateIconCollectionActionButton(FluentIconRegular.CheckboxIndeterminate24, "Mix child", () =>
                    {
                        foundation.IsChecked = true;
                        if (foundation.Items[1] is FWTreeSelectorItem typography)
                        {
                            typography.IsChecked = false;
                        }
                        UpdateOutput();
                    }),
                    CreateIconCollectionActionButton(FluentIconRegular.CheckboxUnchecked24, "Clear", () =>
                    {
                        foundation.IsChecked = false;
                        controls.IsChecked = false;
                        selector.UnselectAll();
                        UpdateOutput();
                    }),
                    CreateIconCollectionActionButton(FluentIconRegular.ChevronRight24, "Collapse", () =>
                    {
                        foundation.IsExpanded = !foundation.IsExpanded;
                        controls.IsExpanded = !controls.IsExpanded;
                        UpdateOutput();
                    })),
                output
            }
        };
    }

    private static UIElement CreatePropertyGridSample()
    {
        var sample = new GalleryPropertySample();
        var output = CreateCollectionOutput("Sort: Categorized. Search: empty. Read-only: false");
        var propertyGrid = new FWPropertyGrid
        {
            Width = 470,
            Height = 310,
            SelectedObject = sample,
            SortMode = PropertyGridSortMode.Categorized,
            ShowSearchBox = true,
            ShowDescription = true,
            ShowToolBar = true,
            NameColumnWidth = 150
        };

        void UpdateOutput()
        {
            output.Text = $"Sort: {propertyGrid.SortMode}. Search: {FormatSearchText(propertyGrid.SearchText)}. Read-only: {propertyGrid.IsReadOnly}. Selected: {propertyGrid.SelectedProperty?.DisplayName ?? "none"}";
        }

        propertyGrid.SelectedPropertyChanged += (_, _) => UpdateOutput();
        propertyGrid.PropertyValueChanged += (_, args) =>
        {
            output.Text = $"Changed: {args.PropertyName} from {args.OldValue ?? "null"} to {args.NewValue ?? "null"}";
        };
        UpdateOutput();

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                propertyGrid,
                CreateCollectionButtonRow(
                    CreateIconCollectionActionButton(FluentIconRegular.GroupList24, "Categorized", () =>
                    {
                        propertyGrid.SortMode = PropertyGridSortMode.Categorized;
                        UpdateOutput();
                    }),
                    CreateIconCollectionActionButton(FluentIconRegular.TextSortAscending24, "Alphabetical", () =>
                    {
                        propertyGrid.SortMode = PropertyGridSortMode.Alphabetical;
                        UpdateOutput();
                    }),
                    CreateIconCollectionActionButton(FluentIconRegular.Search24, "Search layout", () =>
                    {
                        propertyGrid.SearchText = propertyGrid.SearchText.Length == 0 ? "layout" : string.Empty;
                        UpdateOutput();
                    }),
                    CreateIconCollectionActionButton(FluentIconRegular.EditLock24, "Read-only", () =>
                    {
                        propertyGrid.IsReadOnly = !propertyGrid.IsReadOnly;
                        UpdateOutput();
                    })),
                output
            }
        };
    }

    private static FWButton CreateIconCollectionActionButton(FluentIconRegular icon, string text, Action action)
    {
        var button = new FWButton
        {
            Content = new FWStackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 6,
                Children =
                {
                    CreateIcon(icon),
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

    private static void PopulateGalleryTreeSelector(FWTreeSelector selector)
    {
        selector.Items.Add(CreateGalleryTreeSelectorItem("FluentJalium", isExpanded: true,
            CreateGalleryTreeSelectorItem("Theme resources"),
            CreateGalleryTreeSelectorItem("FW controls"),
            CreateGalleryTreeSelectorItem("Gallery pages")));
        selector.Items.Add(CreateGalleryTreeSelectorItem("Data surfaces", isExpanded: true,
            CreateGalleryTreeSelectorItem("Tree selector"),
            CreateGalleryTreeSelectorItem("Property grid"),
            CreateGalleryTreeSelectorItem("Tables")));
        selector.Items.Add(CreateGalleryTreeSelectorItem("Diagnostics", isExpanded: false,
            CreateGalleryTreeSelectorItem("State matrix"),
            CreateGalleryTreeSelectorItem("High contrast")));
    }

    private static FWTreeSelectorItem CreateGalleryTreeSelectorItem(string header, bool isExpanded = false, params FWTreeSelectorItem[] children)
    {
        var item = new FWTreeSelectorItem
        {
            Header = header,
            IsExpanded = isExpanded
        };

        foreach (var child in children)
        {
            item.Items.Add(child);
        }

        return item;
    }

    private static string FormatOpenState(bool isOpen) => isOpen ? "open" : "closed";

    private static string FormatSearchText(string? searchText) => string.IsNullOrWhiteSpace(searchText) ? "empty" : searchText;

    private static string FormatNullableBool(bool? value)
    {
        return value switch
        {
            true => "checked",
            false => "unchecked",
            _ => "mixed"
        };
    }

    private static string FormatCollectionItems(System.Collections.IEnumerable items)
    {
        var names = new List<string>();

        foreach (var item in items)
        {
            names.Add(item switch
            {
                TreeSelectorItem treeSelectorItem => treeSelectorItem.Header?.ToString() ?? string.Empty,
                GalleryRow row => row.Name,
                GalleryTreeRow row => row.Name,
                _ => item?.ToString() ?? string.Empty
            });
        }

        return string.Join(", ", names);
    }

    private UIElement CreateDataInspectorsSection()
    {
        var panel = CreateSection("Data Inspectors");
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        examples.Children.Add(CreateCollectionExampleCard(
            "FWDiffViewer",
            "Side-by-side and unified diffs with line numbers, minimap state, and change navigation.",
            CreateDiffViewerSample()));
        examples.Children.Add(CreateCollectionExampleCard(
            "FWHexEditor",
            "Binary data surface with offset, hex, ASCII, grouping, find, and replacement state.",
            CreateHexEditorSample()));
        examples.Children.Add(CreateCollectionExampleCard(
            "FWJsonTreeViewer",
            "Searchable JSON tree with type colors, path status, expand depth, and item count display.",
            CreateJsonTreeViewerSample()));

        panel.Children.Add(examples);
        return panel;
    }

    private static UIElement CreateDiffViewerSample()
    {
        var output = CreateCollectionOutput("Mode: SideBySide. Changes: pending");
        var viewer = new FWDiffViewer
        {
            Width = 520,
            Height = 300,
            OriginalText = "theme: dark\naccent: blue\ncontrols: 32\nstatus: preview",
            ModifiedText = "theme: dark\naccent: teal\ncontrols: 36\nstatus: ready\nicons: fluent",
            ViewMode = DiffViewMode.SideBySide,
            ShowLineNumbers = true,
            ShowMinimap = true,
            GutterWidth = 56
        };

        void UpdateOutput()
        {
            output.Text = $"Mode: {viewer.ViewMode}. Line numbers: {viewer.ShowLineNumbers}. Minimap: {viewer.ShowMinimap}. Changes: {viewer.GetChangeCount()}";
        }

        viewer.DiffComputed += (_, _) => UpdateOutput();
        UpdateOutput();

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                viewer,
                CreateCollectionButtonRow(
                    CreateIconCollectionActionButton(FluentIconRegular.TextSortAscending24, "Unified", () =>
                    {
                        viewer.ViewMode = viewer.ViewMode == DiffViewMode.SideBySide ? DiffViewMode.Unified : DiffViewMode.SideBySide;
                        UpdateOutput();
                    }),
                    CreateIconCollectionActionButton(FluentIconRegular.NumberRow24, "Line nums", () =>
                    {
                        viewer.ShowLineNumbers = !viewer.ShowLineNumbers;
                        UpdateOutput();
                    }),
                    CreateIconCollectionActionButton(FluentIconRegular.DataUsage24, "Minimap", () =>
                    {
                        viewer.ShowMinimap = !viewer.ShowMinimap;
                        UpdateOutput();
                    }),
                    CreateIconCollectionActionButton(FluentIconRegular.ArrowDown24, "Next", () =>
                    {
                        viewer.NavigateToNextChange();
                        UpdateOutput();
                    })),
                output
            }
        };
    }

    private static UIElement CreateHexEditorSample()
    {
        var output = CreateCollectionOutput("Bytes per row: 16. ASCII: on. Interpretation: off");
        var editor = new FWHexEditor
        {
            Width = 520,
            Height = 300,
            Data = CreateHexSampleData(),
            BytesPerRow = 16,
            ColumnGroupSize = 8,
            ShowAsciiColumn = true,
            ShowOffsetColumn = true,
            ShowDataInterpretation = false,
            SelectionStart = 0,
            SelectionLength = 4
        };

        void UpdateOutput()
        {
            output.Text = $"Bytes per row: {editor.BytesPerRow}. ASCII: {FormatOnOff(editor.ShowAsciiColumn)}. Interpretation: {FormatOnOff(editor.ShowDataInterpretation)}. Selection: {editor.SelectionStart}+{editor.SelectionLength}";
        }

        editor.SelectionChanged += (_, _) => UpdateOutput();
        UpdateOutput();

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                editor,
                CreateCollectionButtonRow(
                    CreateIconCollectionActionButton(FluentIconRegular.TableResizeColumn24, "Rows", () =>
                    {
                        editor.BytesPerRow = editor.BytesPerRow == 16 ? 8 : 16;
                        UpdateOutput();
                    }),
                    CreateIconCollectionActionButton(FluentIconRegular.TextChangeCase24, "ASCII", () =>
                    {
                        editor.ShowAsciiColumn = !editor.ShowAsciiColumn;
                        UpdateOutput();
                    }),
                    CreateIconCollectionActionButton(FluentIconRegular.DataHistogram24, "Interpret", () =>
                    {
                        editor.ShowDataInterpretation = !editor.ShowDataInterpretation;
                        UpdateOutput();
                    }),
                    CreateIconCollectionActionButton(FluentIconRegular.Search24, "Find", () =>
                    {
                        var offset = editor.FindBytes([0x46, 0x57]);
                        output.Text = $"Found FW at offset {offset}";
                    })),
                output
            }
        };
    }

    private static UIElement CreateJsonTreeViewerSample()
    {
        var output = CreateCollectionOutput("Search: empty. Expand depth: 2");
        var viewer = new FWJsonTreeViewer
        {
            Width = 520,
            Height = 300,
            JsonText = """
                {
                  "library": "FluentJalium",
                  "theme": {
                    "variant": "Dark",
                    "accent": "Teal"
                  },
                  "controls": ["FWDiffViewer", "FWHexEditor", "FWJsonTreeViewer"],
                  "ready": true
                }
                """,
            ExpandDepth = 2,
            SearchText = string.Empty,
            ShowItemCount = true,
            ShowTypeIndicators = true
        };

        void UpdateOutput()
        {
            output.Text = $"Search: {FormatSearchText(viewer.SearchText)}. Expand depth: {viewer.ExpandDepth}. Type indicators: {FormatOnOff(viewer.ShowTypeIndicators)}";
        }

        viewer.SelectedNodeChanged += (_, _) => UpdateOutput();
        UpdateOutput();

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                viewer,
                CreateCollectionButtonRow(
                    CreateIconCollectionActionButton(FluentIconRegular.Search24, "Search theme", () =>
                    {
                        viewer.SearchText = viewer.SearchText.Length == 0 ? "theme" : string.Empty;
                        UpdateOutput();
                    }),
                    CreateIconCollectionActionButton(FluentIconRegular.ArrowExpand24, "Expand", () =>
                    {
                        viewer.ExpandAll();
                        UpdateOutput();
                    }),
                    CreateIconCollectionActionButton(FluentIconRegular.ArrowCollapseAll24, "Collapse", () =>
                    {
                        viewer.CollapseAll();
                        UpdateOutput();
                    }),
                    CreateIconCollectionActionButton(FluentIconRegular.Braces24, "Types", () =>
                    {
                        viewer.ShowTypeIndicators = !viewer.ShowTypeIndicators;
                        UpdateOutput();
                    })),
                output
            }
        };
    }

    private static byte[] CreateHexSampleData()
    {
        return
        [
            0x46, 0x57, 0x20, 0x44, 0x61, 0x74, 0x61, 0x20,
            0x49, 0x6E, 0x73, 0x70, 0x65, 0x63, 0x74, 0x6F,
            0x72, 0x0A, 0x01, 0x02, 0x03, 0x04, 0x20, 0x26,
            0x46, 0x6C, 0x75, 0x65, 0x6E, 0x74, 0x0D, 0x0A
        ];
    }

    private UIElement CreateNavigationSection()
    {
        var panel = CreateSection("Navigation");
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };
        examples.Children.Add(CreateNavigationExampleCard(
            "FWNavigationView",
            "Left navigation with pane header, footer menu items, nested items, and live selection output.",
            CreateNavigationViewSample()));
        examples.Children.Add(CreateNavigationExampleCard(
            "Pane modes and hierarchy",
            "Switch Left, LeftCompact, LeftMinimal, and Top display modes while preserving hierarchy state.",
            CreateNavigationPaneModeSample()));
        examples.Children.Add(CreateNavigationExampleCard(
            "FWTabControl",
            "Top, bottom, left, disabled, and swipe-enabled tab states with selected content output.",
            CreateTabControlNavigationSample()));
        examples.Children.Add(CreateNavigationExampleCard(
            "FWFrame",
            "Frame navigation, back stack, forward stack, and content host surface.",
            CreateFrameNavigationSample()));

        panel.Children.Add(examples);
        return panel;
    }

    private static UIElement CreateNavigationViewSample()
    {
        var output = CreateNavigationOutput("Selected: Dashboard");
        var dashboardItem = new FWNavigationViewItem
        {
            Content = "Dashboard",
            Icon = CreateIcon(FluentIconRegular.Home24)
        };
        var controlsItem = new FWNavigationViewItem
        {
            Content = "Controls",
            Icon = CreateIcon(FluentIconRegular.ControlButton24),
            IsExpanded = true
        };
        controlsItem.MenuItems.Add(new FWNavigationViewItem { Content = "Buttons", Icon = CreateIcon(FluentIconRegular.ControlButton24) });
        controlsItem.MenuItems.Add(new FWNavigationViewItem { Content = "Collections", Icon = CreateIcon(FluentIconRegular.Table24) });
        var galleryItem = new FWNavigationViewItem { Content = "Gallery", Icon = CreateIcon(FluentIconRegular.Folder24) };
        var settingsItem = new FWNavigationViewItem { Content = "Settings", Icon = CreateIcon(FluentIconRegular.Settings24) };

        var navigationView = new FWNavigationView
        {
            Width = 470,
            Height = 260,
            PaneTitle = "FluentJalium",
            Header = "NavigationView",
            OpenPaneLength = 220,
            CompactPaneLength = 48,
            PaneHeader = CreateNavigationPaneHeader("Controls"),
            PaneFooter = CreateNavigationPaneFooter("vNext"),
            SelectedItem = dashboardItem,
            Content = CreateNavigationContent("Dashboard", "Selected, nested, footer, and separator states share FluentJalium tokens.")
        };
        navigationView.MenuItems.Add(new FWNavigationViewItemHeader { Content = "Workspace" });
        navigationView.MenuItems.Add(dashboardItem);
        navigationView.MenuItems.Add(controlsItem);
        navigationView.MenuItems.Add(new FWNavigationViewItemSeparator());
        navigationView.MenuItems.Add(galleryItem);
        navigationView.FooterMenuItems.Add(settingsItem);
        navigationView.SelectionChanged += (_, e) =>
        {
            output.Text = $"Selected: {e.SelectedItem?.Content}";
            navigationView.Content = CreateNavigationContent(
                e.SelectedItem?.Content?.ToString() ?? "NavigationView",
                $"Previous: {e.PreviousSelectedItem?.Content ?? "none"}");
        };
        navigationView.UpdateMenuItems();

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                navigationView,
                CreateNavigationButtonRow(
                    CreateNavigationActionButton("Dashboard", () => navigationView.SelectedItem = dashboardItem),
                    CreateNavigationActionButton("Collections", () => navigationView.SelectedItem = controlsItem.MenuItems[1]),
                    CreateNavigationActionButton("Toggle pane", () =>
                    {
                        navigationView.IsPaneOpen = !navigationView.IsPaneOpen;
                        output.Text = $"Pane open: {navigationView.IsPaneOpen}";
                    }),
                    CreateNavigationActionButton("Footer", () => navigationView.SelectedItem = settingsItem)),
                output
            }
        };
    }

    private static UIElement CreateNavigationPaneModeSample()
    {
        var output = CreateNavigationOutput("PaneDisplayMode: Left. Document options expanded: true");
        var homeItem = new FWNavigationViewItem { Content = "Home", Icon = CreateIcon(FluentIconRegular.Home24) };
        var accountItem = new FWNavigationViewItem
        {
            Content = "Account",
            Icon = CreateIcon(FluentIconRegular.Person24)
        };
        accountItem.MenuItems.Add(new FWNavigationViewItem { Content = "Mail", Icon = CreateIcon(FluentIconRegular.Mail24) });
        accountItem.MenuItems.Add(new FWNavigationViewItem { Content = "Calendar", Icon = CreateIcon(FluentIconRegular.CalendarLtr24) });
        var documentOptionsItem = new FWNavigationViewItem
        {
            Content = "Document options",
            Icon = CreateIcon(FluentIconRegular.Document24),
            IsExpanded = true,
            SelectsOnInvoked = false
        };
        documentOptionsItem.MenuItems.Add(new FWNavigationViewItem { Content = "Create new", Icon = CreateIcon(FluentIconRegular.Add24) });
        documentOptionsItem.MenuItems.Add(new FWNavigationViewItem { Content = "Upload file", Icon = CreateIcon(FluentIconRegular.ArrowUpload24) });

        var navigationView = new FWNavigationView
        {
            Width = 470,
            Height = 250,
            Header = "Pane modes",
            PaneTitle = "Workspace",
            PaneDisplayMode = NavigationViewPaneDisplayMode.Left,
            OpenPaneLength = 220,
            CompactPaneLength = 48,
            IsBackButtonVisible = NavigationViewBackButtonVisible.Visible,
            IsBackEnabled = true,
            SelectedItem = homeItem,
            Content = CreateNavigationContent("Home", "Switch the pane display mode from the options below.")
        };
        navigationView.MenuItems.Add(homeItem);
        navigationView.MenuItems.Add(accountItem);
        navigationView.MenuItems.Add(documentOptionsItem);
        navigationView.SelectionChanged += (_, e) =>
        {
            output.Text = $"PaneDisplayMode: {navigationView.PaneDisplayMode}. Selected: {e.SelectedItem?.Content}";
        };
        navigationView.ItemInvoked += (_, e) =>
        {
            if (!e.InvokedItem.SelectsOnInvoked)
            {
                output.Text = $"Invoked non-selecting item: {e.InvokedItem.Content}. Expanded: {e.InvokedItem.IsExpanded}";
            }
        };
        navigationView.UpdateMenuItems();

        void SetMode(NavigationViewPaneDisplayMode mode)
        {
            navigationView.PaneDisplayMode = mode;
            navigationView.IsPaneOpen = mode != NavigationViewPaneDisplayMode.LeftCompact;
            output.Text = $"PaneDisplayMode: {mode}. Document options expanded: {documentOptionsItem.IsExpanded}";
        }

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                navigationView,
                CreateNavigationButtonRow(
                    CreateNavigationActionButton("Left", () => SetMode(NavigationViewPaneDisplayMode.Left)),
                    CreateNavigationActionButton("Compact", () => SetMode(NavigationViewPaneDisplayMode.LeftCompact)),
                    CreateNavigationActionButton("Minimal", () => SetMode(NavigationViewPaneDisplayMode.LeftMinimal)),
                    CreateNavigationActionButton("Top", () => SetMode(NavigationViewPaneDisplayMode.Top)),
                    CreateNavigationActionButton("Toggle tree", () =>
                    {
                        documentOptionsItem.IsExpanded = !documentOptionsItem.IsExpanded;
                        output.Text = $"PaneDisplayMode: {navigationView.PaneDisplayMode}. Document options expanded: {documentOptionsItem.IsExpanded}";
                    })),
                output
            }
        };
    }

    private static UIElement CreateTabControlNavigationSample()
    {
        var output = CreateNavigationOutput("Selected tab: Overview");
        var tabControl = new FWTabControl
        {
            Width = 470,
            Height = 160,
            SelectedIndex = 0,
            IsSwipeEnabled = true
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
        tabControl.SelectionChanged += (_, _) =>
        {
            output.Text = $"Selected tab: {(tabControl.SelectedItem as FWTabItem)?.Header}";
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                tabControl,
                CreateNavigationButtonRow(
                    CreateNavigationActionButton("Overview", () => tabControl.SelectedIndex = 0),
                    CreateNavigationActionButton("Details", () => tabControl.SelectedIndex = 1),
                    CreateNavigationActionButton("Top", () =>
                    {
                        tabControl.TabStripPlacement = Jalium.UI.Controls.Dock.Top;
                        output.Text = "TabStripPlacement: Top";
                    }),
                    CreateNavigationActionButton("Bottom", () =>
                    {
                        tabControl.TabStripPlacement = Jalium.UI.Controls.Dock.Bottom;
                        output.Text = "TabStripPlacement: Bottom";
                    }),
                    CreateNavigationActionButton("Left", () =>
                    {
                        tabControl.TabStripPlacement = Jalium.UI.Controls.Dock.Left;
                        output.Text = "TabStripPlacement: Left";
                    })),
                output
            }
        };
    }

    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("Gallery sample navigates to local Page types by typeof literals.")]
    private static UIElement CreateFrameNavigationSample()
    {
        var output = CreateNavigationOutput("Frame: not navigated");
        var frame = new FWFrame
        {
            Width = 470,
            Height = 160,
            Padding = new Thickness(14),
            BorderThickness = new Thickness(1)
        };
        frame.Navigated += (_, _) =>
        {
            output.Text = $"Frame: {frame.SourcePageType?.Name}, BackStack: {frame.BackStackDepth}, CanGoForward: {frame.CanGoForward}";
        };

        frame.Navigate(typeof(GalleryNavigationOverviewPage), "Overview");

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                frame,
                CreateNavigationButtonRow(
                    CreateNavigationActionButton("Overview", () => frame.Navigate(typeof(GalleryNavigationOverviewPage), "Overview")),
                    CreateNavigationActionButton("Details", () => frame.Navigate(typeof(GalleryNavigationDetailsPage), "Details")),
                    CreateNavigationActionButton("Back", () =>
                    {
                        if (!frame.GoBack())
                        {
                            output.Text = "Frame: no back entry";
                        }
                    }),
                    CreateNavigationActionButton("Forward", () =>
                    {
                        if (!frame.GoForward())
                        {
                            output.Text = "Frame: no forward entry";
                        }
                    })),
                output
            }
        };
    }

    private static UIElement CreateNavigationContent(string title, string description)
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Margin = new Thickness(18),
            Children =
            {
                new FWTextBlock
                {
                    Text = title,
                    FontSize = 18,
                    Foreground = ThemeBrush("TextPrimary")
                },
                new FWTextBlock
                {
                    Text = description,
                    Foreground = ThemeBrush("TextSecondary"),
                    TextWrapping = TextWrapping.Wrap
                }
            }
        };
    }

    private static UIElement CreateNavigationPaneHeader(string text)
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 6,
            Children =
            {
                new FWTextBlock { Text = "Search", FontSize = 12, Foreground = ThemeBrush("TextSecondary") },
                new FWTextBox { Text = text, MinHeight = 32, PlaceholderText = "Search navigation" }
            }
        };
    }

    private static UIElement CreateNavigationPaneFooter(string text)
    {
        return new FWTextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary")
        };
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
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };
        examples.Children.Add(CreateCommandExampleCard(
            "Split command buttons",
            "Split, toggle split, and drop-down command surfaces with the same flyout menu affordance.",
            CreateSplitCommandButtonsSample()));
        examples.Children.Add(CreateCommandExampleCard(
            "FWCommandBar",
            "Primary and secondary commands with labels, overflow state, toggle commands, and live output.",
            CreateCommandBarSample()));
        examples.Children.Add(CreateCommandExampleCard(
            "FWToolBar and FWToolBarTray",
            "Document and formatting command groups hosted in a tray with band, index, lock, and overflow metadata.",
            CreateToolBarCommandSample()));

        panel.Children.Add(examples);
        return panel;
    }

    private static UIElement CreateSplitCommandButtonsSample()
    {
        var output = CreateCommandOutput("Split commands: ready");
        var splitButton = new FWSplitButton
        {
            Content = "FWSplitButton",
            Width = 170,
            Flyout = CreateSampleFlyout()
        };
        var toggleSplitButton = new FWToggleSplitButton
        {
            Content = "FWToggleSplit",
            Width = 180,
            IsChecked = true,
            Flyout = CreateSampleFlyout()
        };
        splitButton.Click += (_, _) => output.Text = "Primary split command invoked";
        toggleSplitButton.Click += (_, _) =>
        {
            toggleSplitButton.IsChecked = !toggleSplitButton.IsChecked;
            output.Text = $"Toggle split checked: {FormatOnOff(toggleSplitButton.IsChecked == true)}";
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
                            Content = "FWDropDown",
                            Width = 150,
                            Flyout = CreateSampleFlyout()
                        },
                        new FWButton
                        {
                            Content = "Disabled",
                            IsEnabled = false
                        }
                    }
                },
                output
            }
        };
    }

    private static UIElement CreateCommandBarSample()
    {
        var output = CreateCommandOutput("CommandBar: closed, 2 secondary commands");
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
                CreateCommandButtonRow(
                    CreateCommandActionButton("Open", () => commandBar.IsOpen = true),
                    CreateCommandActionButton("Close", () => commandBar.IsOpen = false),
                    CreateCommandActionButton("Labels", () =>
                    {
                        commandBar.DefaultLabelPosition = commandBar.DefaultLabelPosition == CommandBarDefaultLabelPosition.Bottom
                            ? CommandBarDefaultLabelPosition.Collapsed
                            : CommandBarDefaultLabelPosition.Bottom;
                        commandBar.InvalidateMeasure();
                        output.Text = $"DefaultLabelPosition: {commandBar.DefaultLabelPosition}";
                    }),
                    CreateCommandActionButton("Add secondary", () =>
                    {
                        commandBar.SecondaryCommands.Add(CreateAppBarButton("Export", FluentIconRegular.ArrowDownload24, output));
                        output.Text = $"Secondary commands: {commandBar.SecondaryCommands.Count}";
                    }),
                    CreateCommandActionButton("Remove secondary", () =>
                    {
                        if (commandBar.SecondaryCommands.Count > 0)
                        {
                            commandBar.SecondaryCommands.RemoveAt(commandBar.SecondaryCommands.Count - 1);
                        }
                        output.Text = $"Secondary commands: {commandBar.SecondaryCommands.Count}";
                    })),
                output
            }
        };
    }

    private static UIElement CreateToolBarCommandSample()
    {
        var output = CreateCommandOutput("ToolBarTray: unlocked, horizontal, 2 bands");
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
                CreateCommandButtonRow(
                    CreateCommandActionButton("Lock tray", () =>
                    {
                        tray.IsLocked = !tray.IsLocked;
                        output.Text = $"ToolBarTray locked: {FormatOnOff(tray.IsLocked)}";
                    }),
                    CreateCommandActionButton("Vertical", () =>
                    {
                        tray.Orientation = tray.Orientation == Orientation.Horizontal
                            ? Orientation.Vertical
                            : Orientation.Horizontal;
                        output.Text = $"ToolBarTray orientation: {tray.Orientation}";
                    }),
                    CreateCommandActionButton("Overflow open", () =>
                    {
                        documentBar.IsOverflowOpen = !documentBar.IsOverflowOpen;
                        output.Text = $"Document toolbar overflow open: {FormatOnOff(documentBar.IsOverflowOpen)}";
                    })),
                output
            }
        };
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
                new FluentIcon
                {
                    Icon = FluentIconRegular.Save24,
                    Size = 24
                },
                FluentIconFactory.Filled(FluentIconRegular.Share24, 24, ThemeBrush("TextPrimary")),
                new FluentIcon
                {
                    Icon = FluentIconRegular.Share24,
                    Size = 24
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
                ("Archive", FluentIconRegular.Archive24, "SwipeItemBackgroundSecondary"),
                ("Flag", FluentIconRegular.Flag24, "SwipeItemBackground")),
            RightItems = CreateSwipeItems(SwipeMode.Execute,
                ("Delete", FluentIconRegular.Delete24, "SwipeItemBackgroundDestructive")),
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
                        new FluentIcon
                        {
                            Icon = FluentIconRegular.Play24,
                            Size = 28,
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
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };
        examples.Children.Add(CreateMenuExampleCard(
            "FWMenuBar",
            "Top-level app commands with WinUI-style menu flyout items, separators, icons, disabled states, and shortcuts.",
            CreateMenuBarMenuSample()));
        examples.Children.Add(CreateMenuExampleCard(
            "FWMenu and FWMenuItem",
            "Classic WPF-compatible menu surface with nested headers, checkable items, icon columns, shortcuts, and submenu output.",
            CreateTraditionalMenuSample()));
        examples.Children.Add(CreateMenuExampleCard(
            "FWContextMenu",
            "Context menu placement, open and close events, command shortcuts, disabled items, and checkable state.",
            CreateContextMenuSample()));
        examples.Children.Add(CreateMenuExampleCard(
            "FWMenuFlyout",
            "Drop-down command flyout using FWMenuFlyoutItem, FWToggleMenuFlyoutItem, and FWMenuFlyoutSeparator.",
            CreateMenuFlyoutItemSample()));
        examples.Children.Add(CreateMenuExampleCard(
            "FWMenuFlyoutSubItem",
            "Nested flyout command menu with submenu placement, icons, shortcuts, disabled state, and open or hide actions.",
            CreateMenuFlyoutSubItemSample()));
        examples.Children.Add(CreateMenuExampleCard(
            "FWCommandBarFlyout",
            "Compact command surface with primary app bar actions, secondary commands, and a toggle command state.",
            CreateCommandBarFlyoutSample()));

        panel.Children.Add(examples);
        return panel;
    }

    private static UIElement CreateMenuBarMenuSample()
    {
        var output = CreateMenuOutput("MenuBar: File ready");
        var menuBar = new FWMenuBar
        {
            Width = 480
        };

        var file = CreateMenuBarItem(
            "File",
            ("New", "Ctrl+N", IconGlyph(FluentIconRegular.Document24)),
            ("Open", "Ctrl+O", IconGlyph(FluentIconRegular.FolderOpen24)),
            ("Save", "Ctrl+S", IconGlyph(FluentIconRegular.Save24)));
        var edit = CreateMenuBarItem(
            "Edit",
            ("Undo", "Ctrl+Z", null),
            ("Redo", "Ctrl+Y", null),
            ("Preferences", string.Empty, IconGlyph(FluentIconRegular.Settings24)));
        var view = CreateMenuBarItem(
            "View",
            ("Zoom in", "Ctrl++", null),
            ("Zoom out", "Ctrl+-", null),
            ("Actual size", "Ctrl+0", null));

        menuBar.Items.Add(file);
        menuBar.Items.Add(edit);
        menuBar.Items.Add(view);

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                menuBar,
                CreateMenuButtonRow(
                    CreateMenuActionButton("File", () =>
                    {
                        CloseMenuBarItems(edit, view);
                        file.OpenMenu();
                        output.Text = $"MenuBar: {file.Title} open";
                    }),
                    CreateMenuActionButton("Edit", () =>
                    {
                        CloseMenuBarItems(file, view);
                        edit.OpenMenu();
                        output.Text = $"MenuBar: {edit.Title} open";
                    }),
                    CreateMenuActionButton("View", () =>
                    {
                        CloseMenuBarItems(file, edit);
                        view.OpenMenu();
                        output.Text = $"MenuBar: {view.Title} open";
                    }),
                    CreateMenuActionButton("Close", () =>
                    {
                        CloseMenuBarItems(file, edit, view);
                        output.Text = "MenuBar: all menus closed";
                    })),
                output
            }
        };
    }

    private static UIElement CreateTraditionalMenuSample()
    {
        var output = CreateMenuOutput("Menu: Project ready");
        var menu = new FWMenu
        {
            Width = 480
        };
        var project = new FWMenuItem
        {
            Header = "Project",
            Icon = IconGlyph(FluentIconRegular.Folder24)
        };
        var build = new FWMenuItem
        {
            Header = "Build",
            InputGestureText = "Ctrl+B",
            Icon = IconGlyph(FluentIconRegular.Play24)
        };
        var run = new FWMenuItem
        {
            Header = "Run",
            InputGestureText = "F5",
            Icon = IconGlyph(FluentIconRegular.Play24)
        };
        var livePreview = new FWMenuItem
        {
            Header = "Live preview",
            IsCheckable = true,
            IsChecked = true,
            StaysOpenOnClick = true
        };
        project.Items.Add(build);
        project.Items.Add(run);
        project.Items.Add(livePreview);

        var tools = new FWMenuItem
        {
            Header = "Tools",
            Icon = IconGlyph(FluentIconRegular.Settings24)
        };
        tools.Items.Add(new FWMenuItem { Header = "Options" });
        tools.Items.Add(new FWMenuItem { Header = "Diagnostics", InputGestureText = "F12" });

        menu.Items.Add(project);
        menu.Items.Add(tools);
        menu.Items.Add("Generated");
        menu.Items.Add(new FWMenuItem { Header = "Disabled", IsEnabled = false });

        build.Click += (_, _) => output.Text = "Menu: Build clicked";
        run.Click += (_, _) => output.Text = "Menu: Run clicked";
        project.SubmenuOpened += (_, _) => output.Text = "Menu: Project submenu opened";
        project.SubmenuClosed += (_, _) => output.Text = "Menu: Project submenu closed";
        livePreview.Checked += (_, _) => output.Text = "Menu: Live preview checked";
        livePreview.Unchecked += (_, _) => output.Text = "Menu: Live preview unchecked";

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                menu,
                CreateMenuButtonRow(
                    CreateMenuActionButton("Open project", () =>
                    {
                        project.IsSubmenuOpen = true;
                        output.Text = "Menu: Project submenu opened";
                    }),
                    CreateMenuActionButton("Close", () =>
                    {
                        project.IsSubmenuOpen = false;
                        output.Text = "Menu: Project submenu closed";
                    }),
                    CreateMenuActionButton("Preview", () =>
                    {
                        livePreview.IsChecked = !livePreview.IsChecked;
                        output.Text = $"Menu: Live preview {FormatOnOff(livePreview.IsChecked)}";
                    }),
                    CreateMenuActionButton("Disable run", () =>
                    {
                        run.IsEnabled = !run.IsEnabled;
                        output.Text = $"Menu: Run enabled {run.IsEnabled}";
                    })),
                output
            }
        };
    }

    private static UIElement CreateContextMenuSample()
    {
        var output = CreateMenuOutput("ContextMenu: closed");
        var contextMenu = new FWContextMenu
        {
            Placement = PlacementMode.Bottom,
            StaysOpen = true
        };
        var detailsItem = new FWMenuItem
        {
            Header = "Show details",
            IsCheckable = true,
            IsChecked = true,
            StaysOpenOnClick = true
        };
        contextMenu.Items.Add(new FWMenuItem { Header = "Refresh", InputGestureText = "F5", Icon = IconGlyph(FluentIconRegular.ArrowClockwise24) });
        contextMenu.Items.Add(new FWMenuItem { Header = "Rename", InputGestureText = "F2", Icon = IconGlyph(FluentIconRegular.Rename24) });
        contextMenu.Items.Add(detailsItem);
        contextMenu.Items.Add(new FWMenuItem { Header = "Disabled", IsEnabled = false });
        contextMenu.Opened += (_, _) => output.Text = $"ContextMenu: open at {contextMenu.Placement}";
        contextMenu.Closed += (_, _) => output.Text = "ContextMenu: closed";
        detailsItem.Checked += (_, _) => output.Text = "ContextMenu: details checked";
        detailsItem.Unchecked += (_, _) => output.Text = "ContextMenu: details unchecked";

        var target = new FWBorder
        {
            Width = 300,
            Height = 96,
            Background = ThemeBrush("ControlBackground"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12),
            ContextMenu = contextMenu,
            Child = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 6,
                Children =
                {
                    new FWTextBlock
                    {
                        Text = "Document item",
                        Foreground = ThemeBrush("TextPrimary")
                    },
                    new FWTextBlock
                    {
                        Text = "Open the attached context menu from the options below.",
                        FontSize = 12,
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = ThemeBrush("TextSecondary")
                    }
                }
            }
        };
        contextMenu.PlacementTarget = target;

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                target,
                CreateMenuButtonRow(
                    CreateMenuActionButton("Open", () => ContextMenuService.Open(target, contextMenu)),
                    CreateMenuActionButton("Close", () => contextMenu.Close()),
                    CreateMenuActionButton("Details", () =>
                    {
                        detailsItem.IsChecked = !detailsItem.IsChecked;
                        output.Text = $"ContextMenu: details {FormatOnOff(detailsItem.IsChecked)}";
                    }),
                    CreateMenuActionButton("Mouse point", () =>
                    {
                        contextMenu.Placement = PlacementMode.MousePoint;
                        output.Text = $"ContextMenu placement: {contextMenu.Placement}";
                    }),
                    CreateMenuActionButton("Bottom", () =>
                    {
                        contextMenu.Placement = PlacementMode.Bottom;
                        output.Text = $"ContextMenu placement: {contextMenu.Placement}";
                    })),
                output
            }
        };
    }

    private static UIElement CreateMenuFlyoutItemSample()
    {
        var output = CreateMenuOutput("MenuFlyout: ready");
        var flyout = CreateMenuControlsFlyout(output);
        var button = new FWDropDownButton
        {
            Content = "Actions",
            Width = 160,
            Flyout = flyout
        };
        var toggle = flyout.Items.OfType<FWToggleMenuFlyoutItem>().First();
        var disabled = flyout.Items.OfType<FWMenuFlyoutItem>().Last(item => item.Text == "Disabled");

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                button,
                CreateMenuButtonRow(
                    CreateMenuActionButton("Open", () =>
                    {
                        flyout.ShowAt(button);
                        output.Text = "MenuFlyout: open";
                    }),
                    CreateMenuActionButton("Hide", () =>
                    {
                        flyout.Hide();
                        output.Text = "MenuFlyout: hidden";
                    }),
                    CreateMenuActionButton("Badges", () =>
                    {
                        toggle.IsChecked = !toggle.IsChecked;
                        output.Text = $"MenuFlyout: badges {FormatOnOff(toggle.IsChecked)}";
                    }),
                    CreateMenuActionButton("Disable", () =>
                    {
                        disabled.IsEnabled = !disabled.IsEnabled;
                        output.Text = $"MenuFlyout: disabled item enabled {disabled.IsEnabled}";
                    })),
                output
            }
        };
    }

    private static UIElement CreateMenuFlyoutSubItemSample()
    {
        var output = CreateMenuOutput("SubMenuFlyout: ready");
        var flyout = new FWMenuFlyout
        {
            Placement = FlyoutPlacementMode.Bottom
        };
        var export = new FWMenuFlyoutSubItem
        {
            Text = "Export",
            Icon = IconGlyph(FluentIconRegular.ArrowDownload24),
            KeyboardAcceleratorTextOverride = "Ctrl+E"
        };
        var pdf = new FWMenuFlyoutItem
        {
            Text = "PDF document",
            Icon = IconGlyph(FluentIconRegular.DocumentPdf24)
        };
        var package = new FWMenuFlyoutItem
        {
            Text = "Project package",
            Icon = IconGlyph(FluentIconRegular.Box24)
        };
        var disabled = new FWMenuFlyoutItem
        {
            Text = "Cloud archive",
            IsEnabled = false
        };
        var recent = new FWMenuFlyoutSubItem
        {
            Text = "Recent formats",
            Icon = IconGlyph(FluentIconRegular.History24)
        };
        recent.Items.Add(new FWMenuFlyoutItem { Text = "Markdown", Icon = IconGlyph(FluentIconRegular.TextBold24) });
        recent.Items.Add(new FWMenuFlyoutItem { Text = "HTML", Icon = IconGlyph(FluentIconRegular.Code24) });
        export.Items.Add(pdf);
        export.Items.Add(package);
        export.Items.Add(disabled);
        export.Items.Add(recent);

        var publish = new FWMenuFlyoutItem
        {
            Text = "Publish",
            Icon = IconGlyph(FluentIconRegular.Send24)
        };
        flyout.Items.Add(export);
        flyout.Items.Add(new FWMenuFlyoutSeparator());
        flyout.Items.Add(publish);

        pdf.Click += (_, _) => output.Text = "SubMenuFlyout: PDF selected";
        package.Click += (_, _) => output.Text = "SubMenuFlyout: package selected";
        publish.Click += (_, _) => output.Text = "SubMenuFlyout: publish clicked";

        var button = new FWDropDownButton
        {
            Content = "Export options",
            Width = 180,
            Flyout = flyout
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                button,
                CreateMenuButtonRow(
                    CreateMenuActionButton("Open", () =>
                    {
                        flyout.ShowAt(button);
                        output.Text = "SubMenuFlyout: open";
                    }),
                    CreateMenuActionButton("Submenu", () =>
                    {
                        flyout.ShowAt(button);
                        export.ShowSubMenu();
                        output.Text = "SubMenuFlyout: export submenu open";
                    }),
                    CreateMenuActionButton("Recent", () =>
                    {
                        flyout.ShowAt(button);
                        export.ShowSubMenu();
                        recent.ShowSubMenu();
                        output.Text = "SubMenuFlyout: recent submenu open";
                    }),
                    CreateMenuActionButton("Hide", () =>
                    {
                        recent.HideSubMenu();
                        export.HideSubMenu();
                        flyout.Hide();
                        output.Text = "SubMenuFlyout: hidden";
                    })),
                output
            }
        };
    }

    private static UIElement CreateCommandBarFlyoutSample()
    {
        var output = CreateMenuOutput("CommandBarFlyout: ready");
        var flyout = new FWCommandBarFlyout
        {
            AlwaysExpanded = true
        };
        var button = new FWButton
        {
            Content = new FWStackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 6,
                Children =
                {
                    CreateIcon(FluentIconRegular.MoreHorizontal24),
                    new FWTextBlock
                    {
                        Text = "More commands",
                        Foreground = ThemeBrush("TextPrimary")
                    }
                }
            },
            MinWidth = 170
        };

        flyout.PrimaryCommands.Add(CreateAppBarButton("Copy", FluentIconRegular.Copy24, output));
        flyout.PrimaryCommands.Add(CreateAppBarButton("Share", FluentIconRegular.Share24, output));
        flyout.PrimaryCommands.Add(CreateAppBarToggleButton("Pin", FluentIconRegular.Pin24, output, isChecked: true));
        flyout.SecondaryCommands.Add(CreateAppBarButton("Rename", FluentIconRegular.Rename24, output));
        flyout.SecondaryCommands.Add(CreateAppBarButton("Delete", FluentIconRegular.Delete24, output));
        button.Click += (_, _) =>
        {
            flyout.ShowAt(button);
            output.Text = "CommandBarFlyout: open";
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                button,
                CreateMenuButtonRow(
                    CreateMenuActionButton("Open", () =>
                    {
                        flyout.ShowAt(button);
                        output.Text = "CommandBarFlyout: open";
                    }),
                    CreateMenuActionButton("Hide", () =>
                    {
                        flyout.Hide();
                        output.Text = "CommandBarFlyout: hidden";
                    }),
                    CreateMenuActionButton("Collapse", () =>
                    {
                        flyout.AlwaysExpanded = false;
                        output.Text = "CommandBarFlyout: secondary commands collapsed on next open";
                    }),
                    CreateMenuActionButton("Expand", () =>
                    {
                        flyout.AlwaysExpanded = true;
                        output.Text = "CommandBarFlyout: secondary commands expanded on next open";
                    })),
                output
            }
        };
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
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        examples.Children.Add(CreateDateTimeExampleCard(
            "FWDatePicker",
            "Header, placeholder, long and short formats, bounded dates, dropdown events, and live selected-date output.",
            CreateDatePickerDateTimeSample()));
        examples.Children.Add(CreateDateTimeExampleCard(
            "FWTimePicker",
            "Minute increments, 12-hour and 24-hour clocks, dropdown state, keyboard-ready selection, and output.",
            CreateTimePickerDateTimeSample()));
        examples.Children.Add(CreateDateTimeExampleCard(
            "FWCalendar",
            "CalendarView-style single-date selection with first-day-of-week, today highlight, blackout, and range bounds.",
            CreateCalendarDateTimeSample()));
        examples.Children.Add(CreateDateTimeExampleCard(
            "States and bounds",
            "Disabled, placeholder, focused-border, bounded range, and high-contrast-friendly resource states.",
            CreateDateTimeStateSample()));

        panel.Children.Add(examples);
        return panel;
    }

    private static UIElement CreateDatePickerDateTimeSample()
    {
        var today = DateTime.Today;
        var output = CreateDateTimeOutput($"Selected date: {FormatDateTimeDate(today)}");
        var datePicker = new FWDatePicker
        {
            Header = "Appointment date",
            Width = 260,
            PlaceholderText = "Pick a date",
            DisplayDateStart = today.AddDays(-14),
            DisplayDateEnd = today.AddDays(45),
            SelectedDate = today,
            SelectedDateFormat = DatePickerFormat.Long
        };
        datePicker.SelectedDateChanged += (_, _) =>
        {
            output.Text = $"Selected date: {FormatDateTimeDate(datePicker.SelectedDate)}";
        };
        datePicker.CalendarOpened += (_, _) => output.Text = "Calendar opened";
        datePicker.CalendarClosed += (_, _) => output.Text = $"Calendar closed. Selected date: {FormatDateTimeDate(datePicker.SelectedDate)}";

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                datePicker,
                CreateDateTimeButtonRow(
                    CreateDateTimeActionButton("Today", () => datePicker.SelectedDate = today),
                    CreateDateTimeActionButton("Next week", () => datePicker.SelectedDate = today.AddDays(7)),
                    CreateDateTimeActionButton("Short", () =>
                    {
                        datePicker.SelectedDateFormat = DatePickerFormat.Short;
                        output.Text = "SelectedDateFormat: Short";
                    }),
                    CreateDateTimeActionButton("Long", () =>
                    {
                        datePicker.SelectedDateFormat = DatePickerFormat.Long;
                        output.Text = "SelectedDateFormat: Long";
                    }),
                    CreateDateTimeActionButton("Toggle flyout", () => datePicker.IsDropDownOpen = !datePicker.IsDropDownOpen)),
                output
            }
        };
    }

    private static UIElement CreateTimePickerDateTimeSample()
    {
        var output = CreateDateTimeOutput("Selected time: 10:30 AM");
        var timePicker = new FWTimePicker
        {
            Header = "Arrival time",
            Width = 220,
            PlaceholderText = "Pick a time",
            SelectedTime = new TimeSpan(10, 30, 0),
            MinuteIncrement = 15
        };
        timePicker.SelectedTimeChanged += (_, e) =>
        {
            output.Text = $"Selected time: {FormatDateTimeTime(e.NewTime, timePicker.ClockIdentifier)}";
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                timePicker,
                CreateDateTimeButtonRow(
                    CreateDateTimeActionButton("10:30", () => timePicker.SelectedTime = new TimeSpan(10, 30, 0)),
                    CreateDateTimeActionButton("18:45", () => timePicker.SelectedTime = new TimeSpan(18, 45, 0)),
                    CreateDateTimeActionButton("12 hour", () =>
                    {
                        timePicker.ClockIdentifier = "12HourClock";
                        output.Text = $"ClockIdentifier: {timePicker.ClockIdentifier}";
                    }),
                    CreateDateTimeActionButton("24 hour", () =>
                    {
                        timePicker.ClockIdentifier = "24HourClock";
                        output.Text = $"ClockIdentifier: {timePicker.ClockIdentifier}";
                    }),
                    CreateDateTimeActionButton("15 min", () =>
                    {
                        timePicker.MinuteIncrement = 15;
                        output.Text = "MinuteIncrement: 15";
                    }),
                    CreateDateTimeActionButton("Toggle flyout", () => timePicker.IsDropDownOpen = !timePicker.IsDropDownOpen)),
                output
            }
        };
    }

    private static UIElement CreateCalendarDateTimeSample()
    {
        var today = DateTime.Today;
        var output = CreateDateTimeOutput($"Selected: {FormatDateTimeDate(today.AddDays(1))}");
        var calendar = new FWCalendar
        {
            DisplayDate = today,
            DisplayDateStart = today.AddDays(-14),
            DisplayDateEnd = today.AddDays(45),
            FirstDayOfWeek = DayOfWeek.Monday,
            IsTodayHighlighted = true,
            SelectedDate = today.AddDays(1),
            SelectionMode = CalendarSelectionMode.SingleDate
        };
        calendar.BlackoutDates.Add(today.AddDays(2).Date);
        calendar.SelectedDateChanged += (_, _) =>
        {
            output.Text = $"Selected: {FormatDateTimeDate(calendar.SelectedDate)}";
        };
        calendar.DisplayDateChanged += (_, e) =>
        {
            output.Text = $"Display month: {e.AddedDate?.ToString("MMMM yyyy") ?? "none"}";
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                calendar,
                CreateDateTimeButtonRow(
                    CreateDateTimeActionButton("Today", () => calendar.SelectedDate = today),
                    CreateDateTimeActionButton("Next month", () => calendar.DisplayDate = calendar.DisplayDate.AddMonths(1)),
                    CreateDateTimeActionButton("Monday", () =>
                    {
                        calendar.FirstDayOfWeek = DayOfWeek.Monday;
                        output.Text = "FirstDayOfWeek: Monday";
                    }),
                    CreateDateTimeActionButton("Sunday", () =>
                    {
                        calendar.FirstDayOfWeek = DayOfWeek.Sunday;
                        output.Text = "FirstDayOfWeek: Sunday";
                    }),
                    CreateDateTimeActionButton("Today ring", () =>
                    {
                        calendar.IsTodayHighlighted = !calendar.IsTodayHighlighted;
                        output.Text = $"IsTodayHighlighted: {calendar.IsTodayHighlighted}";
                    })),
                output
            }
        };
    }

    private static UIElement CreateDateTimeStateSample()
    {
        var today = DateTime.Today;
        var boundedDate = new FWDatePicker
        {
            Header = "Bounded date",
            Width = 240,
            PlaceholderText = "Within the next 30 days",
            DisplayDateStart = today,
            DisplayDateEnd = today.AddDays(30)
        };
        var disabledDate = new FWDatePicker
        {
            Header = "Disabled date",
            Width = 240,
            PlaceholderText = "Unavailable",
            IsEnabled = false
        };
        var disabledTime = new FWTimePicker
        {
            Header = "Disabled time",
            Width = 220,
            SelectedTime = new TimeSpan(8, 0, 0),
            IsEnabled = false
        };
        var disabledCalendar = new FWCalendar
        {
            DisplayDate = today,
            SelectedDate = today,
            IsEnabled = false
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                boundedDate,
                disabledDate,
                disabledTime,
                disabledCalendar,
                CreateDateTimeOutput("Disabled and bounded controls reuse DatePicker, TimePicker, and CalendarView tokens.")
            }
        };
    }

    private UIElement CreateStatusSection()
    {
        var panel = CreateSection("Notifications and Status");

        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };
        examples.Children.Add(CreateStatusExampleCard(
            "FWInfoBar",
            "Severity, open state, close events, icon visibility, and long-message layouts.",
            CreateInfoBarStatusSample()));
        examples.Children.Add(CreateStatusExampleCard(
            "FWInfoBadge",
            "Dot, value, overflow, icon, and severity resources for lightweight status indicators.",
            CreateInfoBadgeStatusSample()));
        examples.Children.Add(CreateStatusExampleCard(
            "FWToastNotificationHost",
            "Interactive toast queue, severity actions, visible-count limit, position, and dismiss behavior.",
            CreateToastNotificationStatusSample()));
        examples.Children.Add(CreateStatusExampleCard(
            "FWStatusBar",
            "Bottom app status with text items, disabled items, progress content, and status badges.",
            CreateStatusBarStatusSample()));

        panel.Children.Add(examples);
        return panel;
    }

    private static UIElement CreateInfoBarStatusSample()
    {
        var output = CreateStatusOutput("Closed events: 0");
        var infoBar = CreateInfoBar("Information", "A normal FluentJalium status message.", InfoBarSeverity.Informational);
        infoBar.IsClosable = true;

        var closedEvents = 0;
        infoBar.Closed += (_, _) =>
        {
            closedEvents++;
            output.Text = $"Closed events: {closedEvents}";
        };

        void ShowInfoBar(InfoBarSeverity severity, string title, string message)
        {
            infoBar.Severity = severity;
            infoBar.Title = title;
            infoBar.Message = message;
            infoBar.IsOpen = true;
            output.Text = $"Showing: {severity}";
        }

        var actionRow = CreateStatusButtonRow(
            CreateStatusActionButton("Info", () => ShowInfoBar(InfoBarSeverity.Informational, "Information", "A normal FluentJalium status message.")),
            CreateStatusActionButton("Success", () => ShowInfoBar(InfoBarSeverity.Success, "Success", "The selected operation completed.")),
            CreateStatusActionButton("Warning", () => ShowInfoBar(InfoBarSeverity.Warning, "Warning", "Review settings before continuing.")),
            CreateStatusActionButton("Error", () => ShowInfoBar(InfoBarSeverity.Error, "Error", "A required resource could not be loaded.")));

        var optionRow = CreateStatusButtonRow(
            CreateStatusActionButton("Toggle icon", () =>
            {
                infoBar.IsIconVisible = !infoBar.IsIconVisible;
                infoBar.InvalidateVisual();
                output.Text = $"Icon visible: {infoBar.IsIconVisible}";
            }),
            CreateStatusActionButton("Long message", () => ShowInfoBar(
                InfoBarSeverity.Warning,
                "Review required",
                "This longer message checks wrapping, icon spacing, close affordance, and the Fluent severity color resources under the current theme.")),
            CreateStatusActionButton("Close", () => infoBar.IsOpen = false));

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                infoBar,
                actionRow,
                optionRow,
                output
            }
        };
    }

    private static UIElement CreateInfoBadgeStatusSample()
    {
        var output = CreateStatusOutput("Preview: Critical overflow value 99+");
        var preview = new FWInfoBadge
        {
            Value = 128,
            MaxValue = 99,
            Severity = FWInfoBadgeSeverity.Critical
        };

        var previewRow = new FWStackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Children =
            {
                CreateStatusCaption("Preview"),
                preview
            }
        };

        void SetSeverity(FWInfoBadgeSeverity severity)
        {
            preview.Severity = severity;
            output.Text = $"Preview severity: {severity}, kind: {preview.DisplayKind}";
        }

        var severityRow = CreateStatusButtonRow(
            CreateStatusActionButton("Attention", () => SetSeverity(FWInfoBadgeSeverity.Attention)),
            CreateStatusActionButton("Info", () => SetSeverity(FWInfoBadgeSeverity.Informational)),
            CreateStatusActionButton("Success", () => SetSeverity(FWInfoBadgeSeverity.Success)),
            CreateStatusActionButton("Caution", () => SetSeverity(FWInfoBadgeSeverity.Caution)),
            CreateStatusActionButton("Critical", () => SetSeverity(FWInfoBadgeSeverity.Critical)));

        var modeRow = CreateStatusButtonRow(
            CreateStatusActionButton("Dot", () =>
            {
                preview.Value = -1;
                preview.IconGlyph = null;
                output.Text = $"Preview kind: {preview.DisplayKind}";
            }),
            CreateStatusActionButton("Value", () =>
            {
                preview.IconGlyph = null;
                preview.Value = 8;
                output.Text = $"Preview value: {preview.DisplayValueText}";
            }),
            CreateStatusActionButton("Overflow", () =>
            {
                preview.IconGlyph = null;
                preview.Value = 128;
                preview.MaxValue = 99;
                output.Text = $"Preview value: {preview.DisplayValueText}";
            }),
            CreateStatusActionButton("Icon", () =>
            {
                preview.Value = -1;
                preview.IconGlyph = IconGlyph(FluentIconRegular.Badge24);
                output.Text = $"Preview kind: {preview.DisplayKind}";
            }));

        var matrix = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8
        };
        matrix.Children.Add(CreateInfoBadgeSeverityRow("Attention", FWInfoBadgeSeverity.Attention));
        matrix.Children.Add(CreateInfoBadgeSeverityRow("Info", FWInfoBadgeSeverity.Informational));
        matrix.Children.Add(CreateInfoBadgeSeverityRow("Success", FWInfoBadgeSeverity.Success));
        matrix.Children.Add(CreateInfoBadgeSeverityRow("Caution", FWInfoBadgeSeverity.Caution));
        matrix.Children.Add(CreateInfoBadgeSeverityRow("Critical", FWInfoBadgeSeverity.Critical));

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                previewRow,
                severityRow,
                modeRow,
                matrix,
                output
            }
        };
    }

    private static UIElement CreateToastNotificationStatusSample()
    {
        var output = CreateStatusOutput("Visible toasts: 0");
        var toastHost = new FWToastNotificationHost
        {
            Width = 470,
            Height = 220,
            MaxVisibleToasts = 3,
            Position = ToastPosition.TopLeft,
            ToastWidth = 430,
            Spacing = 8
        };

        void UpdateOutput(string action)
        {
            output.Text = $"{action}. Visible toasts: {toastHost.Children.Count}";
        }

        void ShowToast(ToastSeverity severity, string title, string message)
        {
            var toast = toastHost.Show(severity, title, message, TimeSpan.FromSeconds(12));
            toast.IsAutoDismissEnabled = false;
            toast.Closed += (_, _) => UpdateOutput($"Closed {severity}");
            UpdateOutput($"Shown {severity}");
        }

        var actionRow = CreateStatusButtonRow(
            CreateStatusActionButton("Info", () => ShowToast(ToastSeverity.Information, "Information", "A normal in-app toast notification.")),
            CreateStatusActionButton("Success", () => ShowToast(ToastSeverity.Success, "Build complete", "The latest gallery sample finished successfully.")),
            CreateStatusActionButton("Warning", () => ShowToast(ToastSeverity.Warning, "Review settings", "MaxVisibleToasts keeps the newest three items.")),
            CreateStatusActionButton("Error", () => ShowToast(ToastSeverity.Error, "Load failed", "A required resource could not be loaded.")));

        var optionRow = CreateStatusButtonRow(
            CreateStatusActionButton("Top left", () =>
            {
                toastHost.Position = ToastPosition.TopLeft;
                UpdateOutput("Position: TopLeft");
            }),
            CreateStatusActionButton("Bottom right", () =>
            {
                toastHost.Position = ToastPosition.BottomRight;
                UpdateOutput("Position: BottomRight");
            }),
            CreateStatusActionButton("Dismiss all", () =>
            {
                toastHost.DismissAll();
                UpdateOutput("Dismissed all");
            }));

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                new FWBorder
                {
                    Width = 470,
                    Height = 220,
                    Background = ThemeBrush("SurfaceBackground"),
                    BorderBrush = ThemeBrush("ControlBorder"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    Child = toastHost
                },
                actionRow,
                optionRow,
                output
            }
        };
    }

    private static UIElement CreateStatusBarStatusSample()
    {
        var output = CreateStatusOutput("StatusBar items: 5");
        var disabledItem = new FWStatusBarItem { Content = "Read-only", IsEnabled = false };
        var statusBar = new FWStatusBar
        {
            Width = 470
        };
        statusBar.Items.Add(new FWStatusBarItem { Content = "Ready" });
        statusBar.Items.Add(new FWStatusBarItem { Content = "Line 42" });
        statusBar.Items.Add(new FWStatusBarItem { Content = "UTF-8" });
        statusBar.Items.Add(new FWStatusBarItem
        {
            Content = new FWStackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 6,
                Children =
                {
                    new FWInfoBadge { Severity = FWInfoBadgeSeverity.Success },
                    new FWTextBlock { Text = "Online", FontSize = 12, Foreground = ThemeBrush("StatusBarForeground") }
                }
            }
        });
        statusBar.Items.Add(disabledItem);

        var progressItem = new FWStatusBarItem
        {
            Content = new FWStackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children =
                {
                    new FWTextBlock { Text = "Sync", FontSize = 12, Foreground = ThemeBrush("StatusBarForeground") },
                    new FWProgressBar { Width = 64, Height = 4, Minimum = 0, Maximum = 100, Value = 72 }
                }
            }
        };

        var optionRow = CreateStatusButtonRow(
            CreateStatusActionButton("Toggle disabled", () =>
            {
                disabledItem.IsEnabled = !disabledItem.IsEnabled;
                output.Text = $"Read-only enabled: {disabledItem.IsEnabled}";
            }),
            CreateStatusActionButton("Add progress", () =>
            {
                if (!statusBar.Items.Contains(progressItem))
                {
                    statusBar.Items.Add(progressItem);
                }
                output.Text = $"StatusBar items: {statusBar.Items.Count}";
            }),
            CreateStatusActionButton("Remove progress", () =>
            {
                statusBar.Items.Remove(progressItem);
                output.Text = $"StatusBar items: {statusBar.Items.Count}";
            }));

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                statusBar,
                optionRow,
                output
            }
        };
    }

    private static FWBorder CreateStatusExampleCard(string title, string description, UIElement content)
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

    private static FWWrapPanel CreateStatusButtonRow(params FWButton[] buttons)
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

    private static FWButton CreateStatusActionButton(string text, Action action)
    {
        var button = new FWButton
        {
            Content = text
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static FWStackPanel CreateInfoBadgeSeverityRow(string label, FWInfoBadgeSeverity severity)
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Children =
            {
                new FWTextBlock
                {
                    Text = label,
                    Width = 76,
                    FontSize = 12,
                    Foreground = ThemeBrush("TextSecondary"),
                    VerticalAlignment = VerticalAlignment.Center
                },
                new FWInfoBadge { Severity = severity },
                new FWInfoBadge { Severity = severity, Value = 8 },
                new FWInfoBadge { Severity = severity, Value = 128, MaxValue = 99 },
                new FWInfoBadge { Severity = severity, IconGlyph = IconGlyph(FluentIconRegular.Badge24) }
            }
        };
    }

    private static TextBlock CreateStatusCaption(string text)
    {
        return new TextBlock
        {
            Text = text,
            Width = 76,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary"),
            VerticalAlignment = VerticalAlignment.Center
        };
    }

    private static FWBorder CreateDateTimeExampleCard(string title, string description, UIElement content)
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

    private static FWWrapPanel CreateDateTimeButtonRow(params FWButton[] buttons)
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

    private static FWButton CreateDateTimeActionButton(string text, Action action)
    {
        var button = new FWButton
        {
            Content = text
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static TextBlock CreateDateTimeOutput(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        };
    }

    private static string FormatDateTimeDate(DateTime? date)
    {
        return date?.ToString("D") ?? "none";
    }

    private static string FormatDateTimeTime(TimeSpan? time, string clockIdentifier)
    {
        if (!time.HasValue)
        {
            return "none";
        }

        if (clockIdentifier == "24HourClock")
        {
            return $"{time.Value.Hours:D2}:{time.Value.Minutes:D2}";
        }

        var hour = time.Value.Hours % 12;
        if (hour == 0)
        {
            hour = 12;
        }

        return $"{hour}:{time.Value.Minutes:D2} {(time.Value.Hours >= 12 ? "PM" : "AM")}";
    }

    private static FWBorder CreateNavigationExampleCard(string title, string description, UIElement content)
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

    private static FWWrapPanel CreateNavigationButtonRow(params FWButton[] buttons)
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

    private static FWButton CreateNavigationActionButton(string text, Action action)
    {
        var button = new FWButton
        {
            Content = text
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static TextBlock CreateNavigationOutput(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        };
    }

    private static TextBlock CreateStatusOutput(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        };
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

    private static SwipeItems CreateSwipeItems(SwipeMode mode, params (string Text, FluentIconRegular Icon, string BackgroundKey)[] items)
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

    private static FWBorder CreateCommandExampleCard(string title, string description, UIElement content)
    {
        return new FWBorder
        {
            Width = 560,
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

    private static FWWrapPanel CreateCommandButtonRow(params FWButton[] buttons)
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

    private static FWButton CreateCommandActionButton(string text, Action action)
    {
        var button = new FWButton
        {
            Content = text
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static TextBlock CreateCommandOutput(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
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
            Content = new FWStackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 6,
                Children =
                {
                    CreateIcon(icon),
                    new FWTextBlock
                    {
                        Text = label,
                        Foreground = ThemeBrush("TextPrimary")
                    }
                }
            },
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

    private static FWBorder CreateMenuExampleCard(string title, string description, UIElement content)
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

    private static FWWrapPanel CreateMenuButtonRow(params FWButton[] buttons)
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

    private static FWButton CreateMenuActionButton(string text, Action action)
    {
        var button = new FWButton
        {
            Content = text
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static TextBlock CreateMenuOutput(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        };
    }

    private static void CloseMenuBarItems(params FWMenuBarItem[] items)
    {
        foreach (var item in items)
        {
            item.CloseMenu();
        }
    }

    private static string FormatOnOff(bool value)
    {
        return value ? "on" : "off";
    }

    private static FWMenuBarItem CreateMenuBarItem(string title, params (string Text, string Shortcut, object? Icon)[] items)
    {
        var menuBarItem = new FWMenuBarItem
        {
            Title = title
        };

        for (var index = 0; index < items.Length; index++)
        {
            var (text, shortcut, icon) = items[index];
            menuBarItem.Items.Add(new FWMenuFlyoutItem
            {
                Text = text,
                KeyboardAcceleratorTextOverride = shortcut,
                Icon = icon
            });
        }

        if (items.Length > 1)
        {
            menuBarItem.Items.Insert(items.Length - 1, new FWMenuFlyoutSeparator());
        }

        return menuBarItem;
    }

    private static FWMenuFlyout CreateMenuControlsFlyout(TextBlock? output = null)
    {
        var flyout = new FWMenuFlyout();
        var pin = new FWMenuFlyoutItem
        {
            Text = "Pin",
            Icon = IconGlyph(FluentIconRegular.Pin24),
            KeyboardAcceleratorTextOverride = "Ctrl+P"
        };
        var badges = new FWToggleMenuFlyoutItem
        {
            Text = "Show badges",
            IsChecked = true
        };
        var settings = new FWMenuFlyoutItem
        {
            Text = "Settings",
            Icon = IconGlyph(FluentIconRegular.Settings24)
        };
        var disabled = new FWMenuFlyoutItem
        {
            Text = "Disabled",
            IsEnabled = false
        };

        pin.Click += (_, _) =>
        {
            if (output != null)
            {
                output.Text = "MenuFlyout: Pin clicked";
            }
        };
        badges.Click += (_, _) =>
        {
            if (output != null)
            {
                output.Text = $"MenuFlyout: badges {FormatOnOff(badges.IsChecked)}";
            }
        };
        settings.Click += (_, _) =>
        {
            if (output != null)
            {
                output.Text = "MenuFlyout: Settings clicked";
            }
        };

        flyout.Items.Add(pin);
        flyout.Items.Add(badges);
        flyout.Items.Add(new FWMenuFlyoutSeparator());
        flyout.Items.Add(settings);
        flyout.Items.Add(disabled);
        return flyout;
    }

    private static FWMenuFlyout CreateSampleFlyout()
    {
        var flyout = new FWMenuFlyout();
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

    private static FluentIcon CreateIcon(FluentIconRegular icon, double size = FluentIcon.DefaultSize, Brush? foreground = null)
    {
        return FluentIconFactory.Regular(icon, size, foreground ?? ThemeBrush("TextPrimary"));
    }

    private static string IconGlyph(FluentIconRegular icon) => icon.GetString();

}
