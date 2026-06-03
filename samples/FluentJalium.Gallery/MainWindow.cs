using FluentJalium.Controls.Themes;
using FluentJalium.Gallery.Models;
using FluentJalium.Gallery.Pages;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Ink;
using Jalium.UI.Input;
using Jalium.UI.Media;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWButton = FluentJalium.Controls.FWButton;
using FWCheckBox = FluentJalium.Controls.FWCheckBox;
using FWColorPicker = FluentJalium.Controls.FWColorPicker;
using FWComboBox = FluentJalium.Controls.FWComboBox;
using FWComboBoxItem = FluentJalium.Controls.FWComboBoxItem;
using FWDropDownButton = FluentJalium.Controls.FWDropDownButton;
using FWInkCanvas = FluentJalium.Controls.FWInkCanvas;
using FWInkPresenter = FluentJalium.Controls.FWInkPresenter;
using FWGrid = FluentJalium.Controls.FWGrid;
using FWLabel = FluentJalium.Controls.FWLabel;
using FWMenuFlyout = FluentJalium.Controls.FWMenuFlyout;
using FWMenuFlyoutItem = FluentJalium.Controls.FWMenuFlyoutItem;
using FWMenuFlyoutSeparator = FluentJalium.Controls.FWMenuFlyoutSeparator;
using FWMediaElement = FluentJalium.Controls.FWMediaElement;
using FWNavigationView = FluentJalium.Controls.FWNavigationView;
using FWNavigationViewItem = FluentJalium.Controls.FWNavigationViewItem;
using FWNavigationViewItemSeparator = FluentJalium.Controls.FWNavigationViewItemSeparator;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWScrollViewer = FluentJalium.Controls.FWScrollViewer;
using FWTextBox = FluentJalium.Controls.FWTextBox;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWToggleButton = FluentJalium.Controls.FWToggleButton;
using FWToggleSplitButton = FluentJalium.Controls.FWToggleSplitButton;
using FWToggleSwitch = FluentJalium.Controls.FWToggleSwitch;
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
            Buttons: () => CreatePageStack(new GalleryButtonsPage().CreateContent()),
            Switches: () => CreatePageStack(new GallerySwitchesPage().CreateContent()),
            TextInput: () => CreatePageStack(new GalleryTextInputPage().CreateContent()),
            Selection: () => CreatePageStack(new GallerySelectionPage().CreateContent()),
            Range: () => CreatePageStack(new GalleryRangePage().CreateContent()),
            DateAndTime: () => CreatePageStack(new GalleryDateTimePage().CreateContent()),
            ContentAndLayout: () => CreatePageStack(new GalleryContentLayoutPage().CreateContent()),
            Visuals: () => CreatePageStack(new GalleryVisualsPage().CreateContent()),
            Interaction: () => CreatePageStack(new GalleryInteractionPage().CreateContent()),
            InputAndMedia: () => CreatePageStack(CreateAdvancedInputMediaSection()),
            Collections: () => CreatePageStack(new GalleryCollectionsPage().CreateContent()),
            SelectorsAndProperties: () => CreatePageStack(new GallerySelectorsPropertiesPage().CreateContent()),
            DataInspectors: () => CreatePageStack(new GalleryDataInspectorsPage().CreateContent()),
            Navigation: () => CreatePageStack(new GalleryNavigationPage().CreateContent()),
            MaterialsAndEffects: () => CreatePageStack(new GalleryMaterialsPage(this).CreateContent()),
            MotionAndTransitions: () => CreatePageStack(new GalleryMotionPage().CreateContent()),
            Menus: () => CreatePageStack(new GalleryMenusPage().CreateContent()),
            Disclosure: () => CreatePageStack(new GalleryDisclosurePage().CreateContent()),
            Status: () => CreatePageStack(new GalleryStatusPage().CreateContent()),
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

    private static FWMenuFlyout CreateSampleFlyout()
    {
        var flyout = new FWMenuFlyout();
        flyout.Items.Add(new FWMenuFlyoutItem { Text = "Create", Icon = IconGlyph(FluentIconRegular.Add24) });
        flyout.Items.Add(new FWMenuFlyoutItem { Text = "Open", Icon = IconGlyph(FluentIconRegular.Open24) });
        flyout.Items.Add(new FWMenuFlyoutSeparator());
        flyout.Items.Add(new FWMenuFlyoutItem { Text = "Export", Icon = IconGlyph(FluentIconRegular.ArrowDownload24) });
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
