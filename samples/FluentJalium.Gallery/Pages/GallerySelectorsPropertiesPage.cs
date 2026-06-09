using FluentJalium.Gallery.Models;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Media;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWButton = FluentJalium.Controls.FWButton;
using FWFluentMaterialKind = FluentJalium.Controls.FWFluentMaterialKind;
using FWFluentMaterialSurface = FluentJalium.Controls.FWFluentMaterialSurface;
using FWPropertyGrid = FluentJalium.Controls.FWPropertyGrid;
using FWPropertyGridDensity = FluentJalium.Controls.FWPropertyGridDensity;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWTreeSelector = FluentJalium.Controls.FWTreeSelector;
using FWTreeSelectorItem = FluentJalium.Controls.FWTreeSelectorItem;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;

namespace FluentJalium.Gallery.Pages;

internal sealed class GallerySelectorsPropertiesPage
{
    internal readonly record struct GallerySelectorsPropertiesQaSnapshot(
        bool HasSelectedTreeItem,
        int TreeItemCount,
        int SelectedTreeItemCount,
        int CheckedTreeItemCount,
        bool IsTreeSearchEnabled,
        bool HasTreeSearchText,
        bool IsTreeDropDownOpen,
        TreeSelectorCheckCascadeMode TreeCascadeMode,
        bool HasSelectedObject,
        PropertyGridSortMode PropertySortMode,
        string PropertyDensity,
        bool IsPropertySearchVisible,
        bool HasPropertySearchText,
        bool IsPropertyDescriptionVisible,
        bool IsPropertyToolbarVisible,
        bool IsPropertyReadOnly,
        double NameColumnWidth,
        FWFluentMaterialKind MaterialKind)
    {
        public bool HasSelectionEvidence => HasSelectedTreeItem || SelectedTreeItemCount > 0;
        public bool HasCascadeEvidence => CheckedTreeItemCount > 0 && TreeCascadeMode == TreeSelectorCheckCascadeMode.Cascade;
        public bool HasPropertyGridEvidence => HasSelectedObject && IsPropertySearchVisible && IsPropertyDescriptionVisible;
        public bool IsReady => HasSelectionEvidence && HasCascadeEvidence && HasPropertyGridEvidence;
    }

    public UIElement CreateContent()
    {
        var panel = CreateSection("Selectors and Properties");
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        examples.Children.Add(CreateSelectorsPropertiesCard(
            FluentIconRegular.DatabaseSearch24,
            "FWTreeSelector",
            "Searchable hierarchical picker with a path display, dropdown state, and selection output.",
            CreateTreeSelectorSingleSelectionSample()));
        examples.Children.Add(CreateSelectorsPropertiesCard(
            FluentIconRegular.TextBulletListTree24,
            "FWTreeSelectorItem cascade",
            "Multiple selection with checkboxes, cascade state, selected chips, and tree item expansion.",
            CreateTreeSelectorCascadeSample()));
        examples.Children.Add(CreateSelectorsPropertiesCard(
            FluentIconRegular.FormMultiple24,
            "FWPropertyGrid",
            "Object property editing with search, categorized/alphabetical modes, read-only state, and description area.",
            CreatePropertyGridSample()));
        examples.Children.Add(CreateSelectorsPropertiesCard(
            FluentIconRegular.LayerDiagonalSparkle24,
            "Material property surface",
            "Selector and property controls keep WinUI-like legibility on FluentJalium material layers.",
            CreateMaterialPropertiesSample()));

        panel.Children.Add(examples);
        return panel;
    }

    private static UIElement CreateTreeSelectorSingleSelectionSample()
    {
        var output = CreateOutput("Selected: none. Drop-down: closed");
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
                CreateButtonRow(
                    CreateIconActionButton(FluentIconRegular.Search24, "Search data", () =>
                    {
                        selector.IsDropDownOpen = true;
                        selector.SearchText = "data";
                        UpdateOutput();
                    }),
                    CreateIconActionButton(FluentIconRegular.CheckmarkCircle24, "Select themes", () =>
                    {
                        selector.SearchText = string.Empty;
                        selector.IsDropDownOpen = true;
                        selector.SelectedItem = "Theme resources";
                        UpdateOutput();
                    }),
                    CreateIconActionButton(FluentIconRegular.Dismiss24, "Clear", () =>
                    {
                        selector.UnselectAll();
                        selector.SearchText = string.Empty;
                        UpdateOutput();
                    }),
                    CreateIconActionButton(FluentIconRegular.ChevronDown24, "Toggle", () =>
                    {
                        selector.IsDropDownOpen = !selector.IsDropDownOpen;
                        UpdateOutput();
                    })),
                CreateStatus(output)
            }
        };
    }

    private static UIElement CreateTreeSelectorCascadeSample()
    {
        var output = CreateOutput("Checked: none. Selected: none");
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
            var checkedText = selector.CheckedItems.Count == 0 ? "none" : FormatSelectorItems(selector.CheckedItems);
            var selectedText = selector.SelectedItems.Count == 0 ? "none" : FormatSelectorItems(selector.SelectedItems);
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
                CreateButtonRow(
                    CreateIconActionButton(FluentIconRegular.CheckboxChecked24, "Check root", () =>
                    {
                        foundation.IsChecked = true;
                        UpdateOutput();
                    }),
                    CreateIconActionButton(FluentIconRegular.CheckboxIndeterminate24, "Mix child", () =>
                    {
                        foundation.IsChecked = true;
                        if (foundation.Items[1] is FWTreeSelectorItem typography)
                        {
                            typography.IsChecked = false;
                        }
                        UpdateOutput();
                    }),
                    CreateIconActionButton(FluentIconRegular.CheckboxUnchecked24, "Clear", () =>
                    {
                        foundation.IsChecked = false;
                        controls.IsChecked = false;
                        selector.UnselectAll();
                        UpdateOutput();
                    }),
                    CreateIconActionButton(FluentIconRegular.ChevronRight24, "Collapse", () =>
                    {
                        foundation.IsExpanded = !foundation.IsExpanded;
                        controls.IsExpanded = !controls.IsExpanded;
                        UpdateOutput();
                    })),
                CreateStatus(output)
            }
        };
    }

    private static UIElement CreatePropertyGridSample()
    {
        var sample = new GalleryPropertySample();
        var output = CreateOutput("Sort: Categorized. Density: comfortable. Search: empty. Read-only: false");
        var propertyGrid = CreateSamplePropertyGrid(sample);

        void UpdateOutput()
        {
            output.Text = $"Sort: {propertyGrid.SortMode}. Density: {FormatDensity(propertyGrid.Density)}. Search: {FormatSearchText(propertyGrid.SearchText)}. Read-only: {propertyGrid.IsReadOnly}. Selected: {propertyGrid.SelectedProperty?.DisplayName ?? "none"}";
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
                CreateButtonRow(
                    CreateIconActionButton(FluentIconRegular.GroupList24, "Categorized", () =>
                    {
                        propertyGrid.SortMode = PropertyGridSortMode.Categorized;
                        UpdateOutput();
                    }),
                    CreateIconActionButton(FluentIconRegular.TextSortAscending24, "Alphabetical", () =>
                    {
                        propertyGrid.SortMode = PropertyGridSortMode.Alphabetical;
                        UpdateOutput();
                    }),
                    CreateIconActionButton(FluentIconRegular.TextDensity24, "Density", () =>
                    {
                        propertyGrid.Density = propertyGrid.Density switch
                        {
                            FWPropertyGridDensity.Compact => FWPropertyGridDensity.Comfortable,
                            FWPropertyGridDensity.Comfortable => FWPropertyGridDensity.Spacious,
                            _ => FWPropertyGridDensity.Compact
                        };
                        UpdateOutput();
                    }),
                    CreateIconActionButton(FluentIconRegular.Search24, "Search layout", () =>
                    {
                        propertyGrid.SearchText = propertyGrid.SearchText.Length == 0 ? "layout" : string.Empty;
                        UpdateOutput();
                    }),
                    CreateIconActionButton(FluentIconRegular.EditLock24, "Read-only", () =>
                    {
                        propertyGrid.IsReadOnly = !propertyGrid.IsReadOnly;
                        UpdateOutput();
                    })),
                CreateStatus(output)
            }
        };
    }

    private static UIElement CreateMaterialPropertiesSample()
    {
        var sample = new GalleryPropertySample();
        var selectorOutput = CreateOutput("Selected: Theme resources");
        var propertyOutput = CreateOutput("Properties QA pending.");
        var selector = new FWTreeSelector
        {
            Width = 420,
            SelectionMode = SelectionMode.Single,
            ShowCheckBoxes = true,
            CheckCascadeMode = TreeSelectorCheckCascadeMode.Cascade,
            PlaceholderText = "Choose a property source",
            IsSearchEnabled = true,
            SearchText = "theme",
            MaxDropDownHeight = 220
        };
        PopulateGalleryTreeSelector(selector);
        selector.SelectedItem = "Theme resources";
        selector.SelectionChanged += (_, _) => selectorOutput.Text = $"Selected: {selector.SelectedItem ?? "none"}";
        if (selector.Items[0] is FWTreeSelectorItem root)
        {
            root.IsChecked = true;
        }

        var propertyGrid = CreateSamplePropertyGrid(sample, width: 420, height: 230);
        propertyGrid.Density = FWPropertyGridDensity.Compact;
        propertyGrid.ShowToolBar = false;
        propertyGrid.SearchText = "layout";

        var panel = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Children =
            {
                CreateMaterialHeader(),
                selector,
                CreateStatus(selectorOutput),
                propertyGrid
            }
        };
        var surface = new FWFluentMaterialSurface
        {
            Width = 520,
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
            Child = panel
        };

        void UpdatePropertyOutput(string action)
        {
            propertyOutput.Text = FormatSelectorsPropertiesQa(
                action,
                CreateSelectorsPropertiesQaSnapshot(selector, propertyGrid, surface));
        }

        panel.Children.Add(CreateButtonRow(
            CreateIconActionButton(FluentIconRegular.GroupList24, "Categorized", () =>
            {
                propertyGrid.SortMode = PropertyGridSortMode.Categorized;
                UpdatePropertyOutput("Categorized");
            }),
            CreateIconActionButton(FluentIconRegular.TextSortAscending24, "Alphabetical", () =>
            {
                propertyGrid.SortMode = PropertyGridSortMode.Alphabetical;
                UpdatePropertyOutput("Alphabetical");
            }),
            CreateIconActionButton(FluentIconRegular.TextDensity24, "Density", () =>
            {
                propertyGrid.Density = propertyGrid.Density == FWPropertyGridDensity.Compact
                    ? FWPropertyGridDensity.Comfortable
                    : FWPropertyGridDensity.Compact;
                UpdatePropertyOutput("Density");
            }),
            CreateIconActionButton(FluentIconRegular.EditLock24, "Read-only", () =>
            {
                propertyGrid.IsReadOnly = !propertyGrid.IsReadOnly;
                UpdatePropertyOutput("Read-only");
            })));
        panel.Children.Add(CreateStatus(propertyOutput));
        UpdatePropertyOutput("Selectors and properties QA");
        return surface;
    }

    internal static GallerySelectorsPropertiesQaSnapshot CreateSelectorsPropertiesQaSnapshot(
        FWTreeSelector selector,
        FWPropertyGrid propertyGrid,
        FWFluentMaterialSurface? materialSurface = null)
    {
        return new GallerySelectorsPropertiesQaSnapshot(
            HasSelectedTreeItem: selector.SelectedItem != null,
            TreeItemCount: CountTreeItems(selector.Items),
            SelectedTreeItemCount: selector.SelectedItems.Count,
            CheckedTreeItemCount: selector.CheckedItems.Count,
            IsTreeSearchEnabled: selector.IsSearchEnabled,
            HasTreeSearchText: !string.IsNullOrWhiteSpace(selector.SearchText),
            IsTreeDropDownOpen: selector.IsDropDownOpen,
            TreeCascadeMode: selector.CheckCascadeMode,
            HasSelectedObject: propertyGrid.SelectedObject != null,
            PropertySortMode: propertyGrid.SortMode,
            PropertyDensity: FormatDensity(propertyGrid.Density),
            IsPropertySearchVisible: propertyGrid.ShowSearchBox,
            HasPropertySearchText: !string.IsNullOrWhiteSpace(propertyGrid.SearchText),
            IsPropertyDescriptionVisible: propertyGrid.ShowDescription,
            IsPropertyToolbarVisible: propertyGrid.ShowToolBar,
            IsPropertyReadOnly: propertyGrid.IsReadOnly,
            NameColumnWidth: propertyGrid.NameColumnWidth,
            MaterialKind: materialSurface?.MaterialKind ?? FWFluentMaterialKind.None);
    }

    internal static string FormatSelectorsPropertiesQa(string action, GallerySelectorsPropertiesQaSnapshot snapshot)
    {
        return $"{action}: tree selected {FormatOnOff(snapshot.HasSelectionEvidence)}, items {snapshot.TreeItemCount}, selected {snapshot.SelectedTreeItemCount}, checked {snapshot.CheckedTreeItemCount}, search {FormatOnOff(snapshot.IsTreeSearchEnabled)}/{FormatOnOff(snapshot.HasTreeSearchText)}, dropdown {FormatOpenState(snapshot.IsTreeDropDownOpen)}, cascade {snapshot.TreeCascadeMode}; property object {FormatOnOff(snapshot.HasSelectedObject)}, sort {snapshot.PropertySortMode}, density {snapshot.PropertyDensity}, search {FormatOnOff(snapshot.IsPropertySearchVisible)}/{FormatOnOff(snapshot.HasPropertySearchText)}, description {FormatOnOff(snapshot.IsPropertyDescriptionVisible)}, toolbar {FormatOnOff(snapshot.IsPropertyToolbarVisible)}, read-only {FormatOnOff(snapshot.IsPropertyReadOnly)}, name width {snapshot.NameColumnWidth:0}, material {snapshot.MaterialKind}, ready {FormatOnOff(snapshot.IsReady)}.";
    }

    private static FWPropertyGrid CreateSamplePropertyGrid(GalleryPropertySample sample, double width = 470, double height = 310)
    {
        return new FWPropertyGrid
        {
            Width = width,
            Height = height,
            SelectedObject = sample,
            SortMode = PropertyGridSortMode.Categorized,
            ShowSearchBox = true,
            ShowDescription = true,
            ShowToolBar = true,
            Density = FWPropertyGridDensity.Comfortable
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
                CreateIcon(FluentIconRegular.SlideSettings24, 18, ThemeBrush("TextPrimary")),
                new FWTextBlock
                {
                    Text = "Layered property editor",
                    FontSize = 15,
                    Foreground = ThemeBrush("TextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
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

    private static FWBorder CreateSelectorsPropertiesCard(FluentIconRegular icon, string title, string description, UIElement content)
    {
        return new FWBorder
        {
            Width = 540,
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

    private static FWWrapPanel CreateButtonRow(params FWButton[] buttons)
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

    private static FWButton CreateIconActionButton(FluentIconRegular icon, string text, Action action)
    {
        var button = new FWButton
        {
            Content = new FWStackPanel
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
            }
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static TextBlock CreateOutput(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        };
    }

    private static FWBorder CreateStatus(TextBlock status)
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

    private static string FormatOpenState(bool isOpen) => isOpen ? "open" : "closed";

    private static string FormatOnOff(bool value) => value ? "on" : "off";

    private static string FormatSearchText(string? searchText) => string.IsNullOrWhiteSpace(searchText) ? "empty" : searchText;

    private static string FormatDensity(FWPropertyGridDensity density)
    {
        return density switch
        {
            FWPropertyGridDensity.Compact => "compact",
            FWPropertyGridDensity.Spacious => "spacious",
            _ => "comfortable"
        };
    }

    private static string FormatNullableBool(bool? value)
    {
        return value switch
        {
            true => "checked",
            false => "unchecked",
            _ => "mixed"
        };
    }

    private static string FormatSelectorItems(System.Collections.IEnumerable items)
    {
        var names = new List<string>();

        foreach (var item in items)
        {
            names.Add(item switch
            {
                TreeSelectorItem treeSelectorItem => treeSelectorItem.Header?.ToString() ?? string.Empty,
                _ => item?.ToString() ?? string.Empty
            });
        }

        return string.Join(", ", names);
    }

    private static int CountTreeItems(System.Collections.IEnumerable items)
    {
        var count = 0;

        foreach (var item in items)
        {
            count++;
            if (item is TreeSelectorItem treeSelectorItem)
            {
                count += CountTreeItems(treeSelectorItem.Items);
            }
        }

        return count;
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
                        CreateIcon(FluentIconRegular.DatabaseSearch24, 24, ThemeBrush("TextPrimary")),
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
