using FluentJalium.Gallery.Controls;
using FluentJalium.Gallery.Models;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Data;
using Jalium.UI.Media;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWButton = FluentJalium.Controls.FWButton;
using FWCollectionDensity = FluentJalium.Controls.FWCollectionDensity;
using FWDataGrid = FluentJalium.Controls.FWDataGrid;
using FWDataGridDensity = FluentJalium.Controls.FWDataGridDensity;
using FWFluentMaterialKind = FluentJalium.Controls.FWFluentMaterialKind;
using FWFluentMaterialSurface = FluentJalium.Controls.FWFluentMaterialSurface;
using FWGridView = FluentJalium.Controls.FWGridView;
using FWGridViewItem = FluentJalium.Controls.FWGridViewItem;
using FWListBox = FluentJalium.Controls.FWListBox;
using FWListBoxItem = FluentJalium.Controls.FWListBoxItem;
using FWListView = FluentJalium.Controls.FWListView;
using FWProgressBar = FluentJalium.Controls.FWProgressBar;
using FWRangeDensity = FluentJalium.Controls.FWRangeDensity;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWTreeDataGrid = FluentJalium.Controls.FWTreeDataGrid;
using FWTreeView = FluentJalium.Controls.FWTreeView;
using FWTreeViewItem = FluentJalium.Controls.FWTreeViewItem;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;
using GridViewColumn = Jalium.UI.Controls.GridViewColumn;

namespace FluentJalium.Gallery.Pages;

internal sealed class GalleryCollectionsPage
{
    public UIElement CreateContent()
    {
        var panel = CreateSection("Collections and Tables");
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        examples.Children.Add(CreateCollectionExampleCard(
            FluentIconRegular.List24,
            "FWListBox",
            "Selection modes, selected count, disabled state, and SelectAll/UnselectAll commands.",
            CreateListBoxCollectionSample()));
        examples.Children.Add(CreateCollectionExampleCard(
            FluentIconRegular.GroupList24,
            "FWListView",
            "GridView columns with row selection and column-reorder option.",
            CreateListViewCollectionSample()));
        examples.Children.Add(CreateCollectionExampleCard(
            FluentIconRegular.TableSimple24,
            "FWGridView / FWGridViewItem",
            "Dedicated grid rows with columns, selection, density, and item container states.",
            CreateGridViewCollectionSample()));
        examples.Children.Add(CreateCollectionExampleCard(
            FluentIconRegular.CollectionsEmpty24,
            "Collection states",
            "Empty, loading, grouped, and action-ready collection states for WinUI-style data pages.",
            CreateCollectionStatesSample()));
        examples.Children.Add(CreateCollectionExampleCard(
            FluentIconRegular.TextDensity24,
            "Density comparison",
            "Compact, comfortable, and spacious collection density presets shown side by side.",
            CreateDensityComparisonSample()));
        examples.Children.Add(CreateCollectionExampleCard(
            FluentIconRegular.BranchFork24,
            "FWTreeView",
            "Hierarchical items with expanded, collapsed, selected, and disabled states.",
            CreateTreeViewCollectionSample()));
        examples.Children.Add(CreateCollectionExampleCard(
            FluentIconRegular.Table24,
            "FWDataGrid",
            "Manual columns with selectable rows, grid-line modes, headers, and read-only state.",
            CreateDataGridCollectionSample()));
        examples.Children.Add(CreateCollectionExampleCard(
            FluentIconRegular.TableMultiple24,
            "FWTreeDataGrid",
            "Hierarchical table rows with expand/collapse commands and flattened selection output.",
            CreateTreeDataGridCollectionSample()));
        examples.Children.Add(CreateCollectionExampleCard(
            FluentIconRegular.LayerDiagonalSparkle24,
            "Material data surface",
            "Collection and table controls stay readable on FluentJalium layered material surfaces.",
            CreateMaterialDataSurfaceSample()));

        panel.Children.Add(examples);
        return panel;
    }

    private static UIElement CreateListBoxCollectionSample()
    {
        var output = CreateCollectionOutput("Selected: Control states");
        var listBox = new FWListBox
        {
            Width = 360,
            Height = 170,
            SelectionMode = SelectionMode.Multiple
        };
        listBox.Items.Add("Fluent tokens");
        listBox.Items.Add("Control states");
        listBox.Items.Add("Gallery coverage");
        listBox.Items.Add(new FWListBoxItem { Content = "Disabled item", IsEnabled = false });
        listBox.SelectedIndex = 1;

        void UpdateOutput()
        {
            output.Text = listBox.SelectedItems.Count == 0
                ? "Selected: none"
                : $"Selected ({listBox.SelectedItems.Count}): {FormatCollectionItems(listBox.SelectedItems)}";
        }

        listBox.SelectionChanged += (_, _) => UpdateOutput();
        UpdateOutput();

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                listBox,
                CreateCollectionButtonRow(
                    CreateCollectionActionButton("Single", () =>
                    {
                        listBox.SelectionMode = SelectionMode.Single;
                        listBox.SelectedIndex = 0;
                        UpdateOutput();
                    }),
                    CreateCollectionActionButton("Multiple", () =>
                    {
                        listBox.SelectionMode = SelectionMode.Multiple;
                        listBox.SelectAll();
                        UpdateOutput();
                    }),
                    CreateCollectionActionButton("Clear", () =>
                    {
                        listBox.UnselectAll();
                        UpdateOutput();
                    }),
                    CreateCollectionActionButton("Toggle disabled", () =>
                    {
                        listBox.IsEnabled = !listBox.IsEnabled;
                        output.Text = $"ListBox enabled: {listBox.IsEnabled}";
                    })),
                CreateCollectionStatus(output)
            }
        };
    }

    private static UIElement CreateListViewCollectionSample()
    {
        var rows = GallerySampleData.CreateRows();
        var output = CreateCollectionOutput("Selected: Buttons / Complete / 9");
        var view = CreateSampleGridView();
        var listView = new FWListView
        {
            Width = 470,
            Height = 170,
            ItemsSource = rows,
            SelectionMode = SelectionMode.Single,
            View = view,
            SelectedIndex = 0
        };

        void UpdateOutput()
        {
            output.Text = listView.SelectedItem is GalleryRow row
                ? $"Selected: {row.Name} / {row.State} / {row.Count}"
                : "Selected: none";
        }

        listView.SelectionChanged += (_, _) => UpdateOutput();
        UpdateOutput();

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                listView,
                CreateCollectionButtonRow(
                    CreateCollectionActionButton("First", () =>
                    {
                        listView.SelectedIndex = 0;
                        UpdateOutput();
                    }),
                    CreateCollectionActionButton("Next", () =>
                    {
                        listView.SelectedIndex = (listView.SelectedIndex + 1) % rows.Length;
                        UpdateOutput();
                    }),
                    CreateCollectionActionButton("Toggle reorder", () =>
                    {
                        view.AllowsColumnReorder = !view.AllowsColumnReorder;
                        output.Text = $"AllowsColumnReorder: {view.AllowsColumnReorder}";
                    }),
                    CreateCollectionActionButton("Toggle disabled", () =>
                    {
                        listView.IsEnabled = !listView.IsEnabled;
                        output.Text = $"ListView enabled: {listView.IsEnabled}";
                    })),
                CreateCollectionStatus(output)
            }
        };
    }

    private static UIElement CreateGridViewCollectionSample()
    {
        var rows = GallerySampleData.CreateRows();
        var output = CreateCollectionOutput("Selected: Selection / Review / 4. Density: comfortable");
        var firstItem = new FWGridViewItem { Content = rows[0] };
        var selectedItem = new FWGridViewItem { Content = rows[1], IsSelected = true };
        var disabledItem = new FWGridViewItem { Content = rows[2], IsEnabled = false };
        var gridView = new FWGridView
        {
            Width = 470,
            Height = 170,
            SelectionMode = SelectionMode.Single,
            Density = FWCollectionDensity.Comfortable
        };

        if (gridView.View is GridView view)
        {
            view.AllowsColumnReorder = true;
            view.Columns.Add(new GridViewColumn
            {
                Header = "Name",
                DisplayMemberBinding = new Binding("Content.Name"),
                Width = 150
            });
            view.Columns.Add(new GridViewColumn
            {
                Header = "State",
                DisplayMemberBinding = new Binding("Content.State"),
                Width = 110
            });
            view.Columns.Add(new GridViewColumn
            {
                Header = "Count",
                DisplayMemberBinding = new Binding("Content.Count"),
                Width = 80
            });
        }

        gridView.Items.Add(firstItem);
        gridView.Items.Add(selectedItem);
        gridView.Items.Add(disabledItem);
        gridView.SelectedIndex = 1;

        void SetDensity(FWCollectionDensity density)
        {
            gridView.Density = density;
            firstItem.Density = density;
            selectedItem.Density = density;
            disabledItem.Density = density;
        }

        string FormatDensity()
        {
            return gridView.Density.ToString().ToLowerInvariant();
        }

        void UpdateOutput()
        {
            output.Text = gridView.SelectionMode == SelectionMode.Multiple && gridView.SelectedItems.Count > 0
                ? $"Selected ({gridView.SelectedItems.Count}): {FormatCollectionItems(gridView.SelectedItems)}. Density: {FormatDensity()}"
                : gridView.SelectedItem is FWGridViewItem item && item.Content is GalleryRow row
                    ? $"Selected: {row.Name} / {row.State} / {row.Count}. Density: {FormatDensity()}"
                    : $"Selected: none. Density: {FormatDensity()}";
        }

        gridView.SelectionChanged += (_, _) => UpdateOutput();
        UpdateOutput();

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                gridView,
                CreateCollectionButtonRow(
                    CreateCollectionActionButton("Next", () =>
                    {
                        gridView.SelectionMode = SelectionMode.Single;
                        gridView.SelectedIndex = gridView.SelectedIndex == 0 ? 1 : 0;
                        UpdateOutput();
                    }),
                    CreateCollectionActionButton("Multi", () =>
                    {
                        gridView.SelectionMode = gridView.SelectionMode == SelectionMode.Multiple
                            ? SelectionMode.Single
                            : SelectionMode.Multiple;
                        if (gridView.SelectionMode == SelectionMode.Multiple)
                        {
                            firstItem.IsSelected = true;
                            selectedItem.IsSelected = true;
                        }
                        else
                        {
                            gridView.SelectedIndex = 1;
                        }

                        UpdateOutput();
                    }),
                    CreateCollectionActionButton("Density", () =>
                    {
                        var density = gridView.Density switch
                        {
                            FWCollectionDensity.Comfortable => FWCollectionDensity.Compact,
                            FWCollectionDensity.Compact => FWCollectionDensity.Spacious,
                            _ => FWCollectionDensity.Comfortable
                        };
                        SetDensity(density);
                        UpdateOutput();
                    }),
                    CreateCollectionActionButton("Disabled row", () =>
                    {
                        disabledItem.IsEnabled = !disabledItem.IsEnabled;
                        output.Text = $"Disabled row enabled: {disabledItem.IsEnabled}. Density: {FormatDensity()}";
                    })),
                CreateCollectionStatus(output)
            }
        };
    }

    private static UIElement CreateCollectionStatesSample()
    {
        var output = CreateCollectionOutput("States: empty list, loading grid, and grouped settings rows.");
        var emptyList = new FWListView
        {
            Width = 220,
            Height = 96,
            Density = FWCollectionDensity.Compact
        };
        var loadingGrid = CreateLoadingGridState();
        var progress = new FWProgressBar
        {
            Width = 220,
            Density = FWRangeDensity.Compact,
            IsIndeterminate = true
        };
        var groupedList = CreateGroupedStateList();

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Children =
            {
                new FWWrapPanel
                {
                    HorizontalSpacing = 18,
                    VerticalSpacing = 12,
                    Children =
                    {
                        CreateCollectionStateColumn(
                            "Empty",
                            emptyList,
                            new FWTextBlock
                            {
                                Text = "No controls match this filter.",
                                FontSize = 12,
                                Foreground = ThemeBrush("TextSecondary")
                            }),
                        CreateCollectionStateColumn(
                            "Loading",
                            loadingGrid,
                            progress)
                    }
                },
                CreateCollectionStateColumn("Grouped", groupedList),
                CreateCollectionButtonRow(
                    CreateCollectionActionButton("Load list", () =>
                    {
                        emptyList.Items.Clear();
                        emptyList.Items.Add("FWListView - Active");
                        emptyList.Items.Add("FWDataGrid - Updated");
                        emptyList.SelectedIndex = 0;
                        output.Text = "States: empty list replaced with loaded rows.";
                    }),
                    CreateCollectionActionButton("Clear list", () =>
                    {
                        emptyList.Items.Clear();
                        output.Text = "States: empty list restored.";
                    }),
                    CreateCollectionActionButton("Finish load", () =>
                    {
                        loadingGrid.IsEnabled = true;
                        progress.IsIndeterminate = false;
                        progress.Value = 100;
                        output.Text = "States: loading grid completed and enabled.";
                    })),
                CreateCollectionStatus(output)
            }
        };
    }

    private static UIElement CreateDensityComparisonSample()
    {
        var rows = GallerySampleData.CreateRows();
        var output = CreateCollectionOutput("Density: compact. List item height: 32. Table row height: 26.");
        var listItems = rows.Select(row => new FWListBoxItem
        {
            Content = $"{row.Name} / {row.State}",
            Density = FWCollectionDensity.Compact
        }).ToArray();
        var listBox = new FWListBox
        {
            Width = 220,
            Height = 120,
            Density = FWCollectionDensity.Compact,
            SelectionMode = SelectionMode.Single
        };
        foreach (var item in listItems)
        {
            listBox.Items.Add(item);
        }
        listBox.SelectedIndex = 1;

        var gridItems = rows.Select(row => new FWGridViewItem
        {
            Content = row,
            Density = FWCollectionDensity.Compact
        }).ToArray();
        var gridView = new FWGridView
        {
            Width = 220,
            Height = 120,
            Density = FWCollectionDensity.Compact,
            SelectionMode = SelectionMode.Single
        };
        if (gridView.View is GridView view)
        {
            view.Columns.Add(new GridViewColumn
            {
                Header = "Name",
                DisplayMemberBinding = new Binding("Content.Name"),
                Width = 115
            });
            view.Columns.Add(new GridViewColumn
            {
                Header = "State",
                DisplayMemberBinding = new Binding("Content.State"),
                Width = 88
            });
        }

        foreach (var item in gridItems)
        {
            gridView.Items.Add(item);
        }
        gridView.SelectedIndex = 0;

        var dataGrid = CreateSampleDataGrid(rows, width: 470, height: 126);
        dataGrid.Density = FWDataGridDensity.Compact;
        dataGrid.SelectedIndex = 0;

        void ApplyDensity(string label, FWCollectionDensity collectionDensity, FWDataGridDensity dataGridDensity)
        {
            listBox.Density = collectionDensity;
            gridView.Density = collectionDensity;
            dataGrid.Density = dataGridDensity;

            foreach (var item in listItems)
            {
                item.Density = collectionDensity;
            }

            foreach (var item in gridItems)
            {
                item.Density = collectionDensity;
            }

            output.Text = $"Density: {label}. List item height: {listItems[0].MinHeight:0}. Table row height: {dataGrid.RowHeight:0}.";
        }

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Children =
            {
                new FWWrapPanel
                {
                    HorizontalSpacing = 18,
                    VerticalSpacing = 12,
                    Children =
                    {
                        CreateCollectionStateColumn("FWListBox", listBox),
                        CreateCollectionStateColumn("FWGridView", gridView),
                        CreateCollectionStateColumn("FWDataGrid", dataGrid)
                    }
                },
                CreateCollectionButtonRow(
                    CreateCollectionActionButton("Compact", () => ApplyDensity("compact", FWCollectionDensity.Compact, FWDataGridDensity.Compact)),
                    CreateCollectionActionButton("Comfortable", () => ApplyDensity("comfortable", FWCollectionDensity.Comfortable, FWDataGridDensity.Comfortable)),
                    CreateCollectionActionButton("Spacious", () => ApplyDensity("spacious", FWCollectionDensity.Spacious, FWDataGridDensity.Spacious))),
                CreateCollectionStatus(output)
            }
        };
    }

    private static FWGridView CreateLoadingGridState()
    {
        var loadingGrid = new FWGridView
        {
            Width = 220,
            Height = 96,
            Density = FWCollectionDensity.Compact,
            IsEnabled = false
        };

        if (loadingGrid.View is GridView view)
        {
            view.Columns.Add(new GridViewColumn
            {
                Header = "Control",
                DisplayMemberBinding = new Binding("Content.Name"),
                Width = 118
            });
            view.Columns.Add(new GridViewColumn
            {
                Header = "State",
                DisplayMemberBinding = new Binding("Content.State"),
                Width = 82
            });
        }

        loadingGrid.Items.Add(new FWGridViewItem
        {
            Content = new GalleryRow("Rows", "Loading", 0),
            Density = FWCollectionDensity.Compact,
            IsEnabled = false
        });
        loadingGrid.Items.Add(new FWGridViewItem
        {
            Content = new GalleryRow("Tokens", "Pending", 0),
            Density = FWCollectionDensity.Compact,
            IsEnabled = false
        });

        return loadingGrid;
    }

    private static FWListBox CreateGroupedStateList()
    {
        var groupedList = new FWListBox
        {
            Width = 470,
            Height = 148,
            Density = FWCollectionDensity.Compact,
            SelectionMode = SelectionMode.Single
        };

        groupedList.Items.Add(CreateGroupHeader("Input"));
        groupedList.Items.Add("FWAutoSuggestBox - Query ready");
        groupedList.Items.Add("FWRadioButtons - Selected");
        groupedList.Items.Add(CreateGroupHeader("Data"));
        groupedList.Items.Add("FWGridView - Group row");
        groupedList.Items.Add("FWDataGrid - High density");
        groupedList.SelectedIndex = 1;

        return groupedList;
    }

    private static FWListBoxItem CreateGroupHeader(string text)
    {
        return new FWListBoxItem
        {
            Content = text,
            Density = FWCollectionDensity.Compact,
            IsEnabled = false
        };
    }

    private static FWStackPanel CreateCollectionStateColumn(string title, UIElement content, UIElement? footer = null)
    {
        var column = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 6,
            Children =
            {
                new FWTextBlock
                {
                    Text = title,
                    FontSize = 12,
                    Foreground = ThemeBrush("TextSecondary")
                },
                content
            }
        };

        if (footer != null)
        {
            column.Children.Add(footer);
        }

        return column;
    }

    private static UIElement CreateTreeViewCollectionSample()
    {
        var output = CreateCollectionOutput("Selected: Build");
        var designItem = new FWTreeViewItem { Header = "Design" };
        var buildItem = new FWTreeViewItem { Header = "Build", IsSelected = true };
        var workspaceItem = new FWTreeViewItem
        {
            Header = "Workspace",
            IsExpanded = true,
            Items =
            {
                designItem,
                buildItem
            }
        };
        var archiveItem = new FWTreeViewItem { Header = "Archive" };
        var disabledItem = new FWTreeViewItem { Header = "Disabled branch", IsEnabled = false };
        var treeView = new FWTreeView
        {
            Width = 360,
            Height = 170
        };
        treeView.Items.Add(workspaceItem);
        treeView.Items.Add(archiveItem);
        treeView.Items.Add(disabledItem);
        treeView.SelectedItem = buildItem;

        void UpdateOutput()
        {
            var selected = treeView.SelectedItem as FWTreeViewItem;
            output.Text = selected == null
                ? $"Selected: none. Workspace expanded: {workspaceItem.IsExpanded}"
                : $"Selected: {selected.Header}. Workspace expanded: {workspaceItem.IsExpanded}";
        }

        treeView.SelectedItemChanged += (_, _) => UpdateOutput();
        UpdateOutput();

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                treeView,
                CreateCollectionButtonRow(
                    CreateCollectionActionButton("Expand", () =>
                    {
                        workspaceItem.IsExpanded = true;
                        UpdateOutput();
                    }),
                    CreateCollectionActionButton("Collapse", () =>
                    {
                        workspaceItem.IsExpanded = false;
                        UpdateOutput();
                    }),
                    CreateCollectionActionButton("Select root", () =>
                    {
                        treeView.SelectedItem = workspaceItem;
                        UpdateOutput();
                    }),
                    CreateCollectionActionButton("Select child", () =>
                    {
                        workspaceItem.IsExpanded = true;
                        treeView.SelectedItem = designItem;
                        UpdateOutput();
                    })),
                CreateCollectionStatus(output)
            }
        };
    }

    private static UIElement CreateDataGridCollectionSample()
    {
        var rows = GallerySampleData.CreateRows();
        var output = CreateCollectionOutput("Selected: Selection / Review / 4");
        var dataGrid = CreateSampleDataGrid(rows);
        dataGrid.SelectedIndex = 1;

        void UpdateOutput()
        {
            output.Text = dataGrid.SelectedItem is GalleryRow row
                ? $"Selected: {row.Name} / {row.State} / {row.Count}"
                : "Selected: none";
        }

        dataGrid.SelectionChanged += (_, _) => UpdateOutput();
        UpdateOutput();

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                dataGrid,
                CreateCollectionButtonRow(
                    CreateCollectionActionButton("Next row", () =>
                    {
                        dataGrid.SelectedIndex = (dataGrid.SelectedIndex + 1) % rows.Length;
                        UpdateOutput();
                    }),
                    CreateCollectionActionButton("Grid lines", () =>
                    {
                        dataGrid.GridLinesVisibility = dataGrid.GridLinesVisibility == DataGridGridLinesVisibility.All
                            ? DataGridGridLinesVisibility.Horizontal
                            : DataGridGridLinesVisibility.All;
                        output.Text = $"GridLinesVisibility: {dataGrid.GridLinesVisibility}";
                    }),
                    CreateCollectionActionButton("Headers", () =>
                    {
                        dataGrid.HeadersVisibility = dataGrid.HeadersVisibility == DataGridHeadersVisibility.All
                            ? DataGridHeadersVisibility.Column
                            : DataGridHeadersVisibility.All;
                        output.Text = $"HeadersVisibility: {dataGrid.HeadersVisibility}";
                    }),
                    CreateCollectionActionButton("Read-only", () =>
                    {
                        dataGrid.IsReadOnly = !dataGrid.IsReadOnly;
                        output.Text = $"IsReadOnly: {dataGrid.IsReadOnly}";
                    })),
                CreateCollectionStatus(output)
            }
        };
    }

    private static UIElement CreateTreeDataGridCollectionSample()
    {
        var rows = GallerySampleData.CreateTree();
        var output = CreateCollectionOutput("Visible rows: 4. Selected: Theme resources");
        var treeDataGrid = CreateSampleTreeDataGrid(rows);
        treeDataGrid.ExpandAll();
        treeDataGrid.SelectedIndex = 1;

        void UpdateOutput()
        {
            var selected = treeDataGrid.SelectedItem as GalleryTreeRow;
            output.Text = selected == null
                ? $"Visible rows: {treeDataGrid.FlattenedCount}. Selected: none"
                : $"Visible rows: {treeDataGrid.FlattenedCount}. Selected: {selected.Name} / {selected.State}";
        }

        treeDataGrid.SelectionChanged += (_, _) => UpdateOutput();
        UpdateOutput();

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                treeDataGrid,
                CreateCollectionButtonRow(
                    CreateCollectionActionButton("Expand all", () =>
                    {
                        treeDataGrid.ExpandAll();
                        treeDataGrid.SelectedIndex = 1;
                        UpdateOutput();
                    }),
                    CreateCollectionActionButton("Collapse all", () =>
                    {
                        treeDataGrid.CollapseAll();
                        treeDataGrid.SelectedIndex = 0;
                        UpdateOutput();
                    }),
                    CreateCollectionActionButton("Next row", () =>
                    {
                        treeDataGrid.SelectedIndex = (treeDataGrid.SelectedIndex + 1) % treeDataGrid.FlattenedCount;
                        UpdateOutput();
                    }),
                    CreateCollectionActionButton("Toggle disabled", () =>
                    {
                        treeDataGrid.IsEnabled = !treeDataGrid.IsEnabled;
                        output.Text = $"TreeDataGrid enabled: {treeDataGrid.IsEnabled}";
                    })),
                CreateCollectionStatus(output)
            }
        };
    }

    private static UIElement CreateMaterialDataSurfaceSample()
    {
        var rows = GallerySampleData.CreateRows();
        var output = CreateCollectionOutput("Surface: LiquidGlass. Rows: 3. Density: comfortable");
        var dataGrid = CreateSampleDataGrid(rows, width: 420, height: 154);
        dataGrid.Density = FWDataGridDensity.Comfortable;
        dataGrid.AlternatingRowBackground = ThemeBrush("LayerFillColorDefaultBrush");
        dataGrid.SelectedIndex = 2;

        return new FWFluentMaterialSurface
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
            Child = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 12,
                Children =
                {
                    CreateMaterialHeader(),
                    dataGrid,
                    CreateCollectionButtonRow(
                        CreateCollectionActionButton("Compact", () =>
                        {
                            dataGrid.Density = FWDataGridDensity.Compact;
                            output.Text = "Surface: LiquidGlass. Rows: 3. Density: compact";
                        }),
                        CreateCollectionActionButton("Comfortable", () =>
                        {
                            dataGrid.Density = FWDataGridDensity.Comfortable;
                            output.Text = "Surface: LiquidGlass. Rows: 3. Density: comfortable";
                        }),
                        CreateCollectionActionButton("Spacious", () =>
                        {
                            dataGrid.Density = FWDataGridDensity.Spacious;
                            output.Text = "Surface: LiquidGlass. Rows: 3. Density: spacious";
                        }),
                        CreateCollectionActionButton("Toggle lines", () =>
                        {
                            dataGrid.GridLinesVisibility = dataGrid.GridLinesVisibility == DataGridGridLinesVisibility.All
                                ? DataGridGridLinesVisibility.Horizontal
                                : DataGridGridLinesVisibility.All;
                            output.Text = $"GridLinesVisibility: {dataGrid.GridLinesVisibility}";
                        })),
                    CreateCollectionStatus(output)
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
                CreateIcon(FluentIconRegular.TableSettings24, 18, ThemeBrush("TextPrimary")),
                new FWTextBlock
                {
                    Text = "Layered data view",
                    FontSize = 15,
                    Foreground = ThemeBrush("TextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
    }

    private static FWDataGrid CreateSampleDataGrid(GalleryRow[] rows, double width = 470, double height = 190)
    {
        var dataGrid = new FWDataGrid
        {
            Width = width,
            Height = height,
            AutoGenerateColumns = false,
            ItemsSource = rows,
            GridLinesVisibility = DataGridGridLinesVisibility.All,
            HeadersVisibility = DataGridHeadersVisibility.All
        };
        dataGrid.Columns.Add(new DataGridTextColumn { Header = "Name", Binding = new Binding("Name"), Width = 150 });
        dataGrid.Columns.Add(new DataGridTextColumn { Header = "State", Binding = new Binding("State"), Width = 110 });
        dataGrid.Columns.Add(new DataGridTextColumn { Header = "Count", Binding = new Binding("Count"), Width = 80 });
        return dataGrid;
    }

    private static FWTreeDataGrid CreateSampleTreeDataGrid(GalleryTreeRow[] rows)
    {
        var treeDataGrid = new FWTreeDataGrid
        {
            Width = 470,
            Height = 190,
            ChildrenSelector = item => ((GalleryTreeRow)item).Children,
            HasChildrenSelector = item => ((GalleryTreeRow)item).Children.Length > 0,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            HeadersVisibility = DataGridHeadersVisibility.Column
        };
        treeDataGrid.ItemsSource = rows;
        treeDataGrid.Columns.Add(new DataGridTextColumn { Header = "Area", Binding = new Binding("Name"), Width = 170 });
        treeDataGrid.Columns.Add(new DataGridTextColumn { Header = "State", Binding = new Binding("State"), Width = 120 });
        return treeDataGrid;
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

    private static FWBorder CreateCollectionExampleCard(FluentIconRegular icon, string title, string description, UIElement content)
    {
        return GallerySampleCard.Create(icon, title, description, content, code: CreateSampleCode(title), width: 540);
    }

    private static string CreateSampleCode(string title)
    {
        return title switch
        {
            "FWListBox" => "<FWListBox SelectionMode=\"Multiple\">\n    <FWListBoxItem Content=\"Fluent tokens\" />\n</FWListBox>",
            "FWListView" => "<FWListView ItemsSource=\"{Binding Rows}\">\n    <FWListView.View>\n        <GridView />\n    </FWListView.View>\n</FWListView>",
            "FWGridView / FWGridViewItem" => "<FWGridView SelectionMode=\"Single\" SelectedIndex=\"1\" Density=\"Comfortable\">\n    <FWGridView.View>\n        <GridView>\n            <GridViewColumn Header=\"Name\" DisplayMemberBinding=\"{Binding Content.Name}\" />\n        </GridView>\n    </FWGridView.View>\n    <FWGridViewItem Content=\"{Binding ActiveRow}\" IsSelected=\"True\" />\n    <FWGridViewItem Content=\"{Binding DisabledRow}\" IsEnabled=\"False\" />\n</FWGridView>",
            "Collection states" => "<FWListView ItemsSource=\"{Binding EmptyRows}\" />\n<FWGridView IsEnabled=\"False\" ItemsSource=\"{Binding LoadingRows}\" />\n<FWProgressBar IsIndeterminate=\"True\" />\n<FWListBox ItemsSource=\"{Binding GroupedRows}\" />",
            "Density comparison" => "<FWListBox Density=\"Compact\" />\n<FWGridView Density=\"Comfortable\" />\n<FWDataGrid Density=\"Spacious\" />",
            "FWTreeView" => "<FWTreeView>\n    <FWTreeViewItem Header=\"Workspace\" IsExpanded=\"True\" />\n</FWTreeView>",
            "FWDataGrid" => "<FWDataGrid AutoGenerateColumns=\"False\" GridLinesVisibility=\"All\" HeadersVisibility=\"All\" />",
            "FWTreeDataGrid" => "<FWTreeDataGrid ChildrenSelector=\"{Binding Children}\" GridLinesVisibility=\"Horizontal\" />",
            "Material data surface" => "<FWFluentMaterialSurface MaterialKind=\"LiquidGlass\">\n    <FWDataGrid Density=\"Comfortable\" />\n</FWFluentMaterialSurface>",
            _ => "<FWListBox />"
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

    private static FWBorder CreateCollectionStatus(TextBlock status)
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

    private static string FormatCollectionItems(System.Collections.IEnumerable items)
    {
        var names = new List<string>();

        foreach (var item in items)
        {
            names.Add(item switch
            {
                FWListBoxItem listBoxItem => listBoxItem.Content?.ToString() ?? string.Empty,
                FWGridViewItem gridViewItem => gridViewItem.Content is GalleryRow gridRow
                    ? gridRow.Name
                    : gridViewItem.Content?.ToString() ?? string.Empty,
                GalleryRow row => row.Name,
                GalleryTreeRow row => row.Name,
                _ => item?.ToString() ?? string.Empty
            });
        }

        return string.Join(", ", names);
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
                        CreateIcon(FluentIconRegular.Table24, 24, ThemeBrush("TextPrimary")),
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
