using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Themes;
using Jalium.UI.Data;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using JaliumThemeManager = Jalium.UI.Controls.Themes.ThemeManager;

namespace FluentJalium.Tests;

[Collection("Application")]
public sealed class FluentCollectionTableTests
{
    private static readonly string[] CollectionResourceKeys =
    [
        "ControlBackground",
        "ControlBackgroundHover",
        "ControlBackgroundDisabled",
        "ControlBorder",
        "SelectionBackground",
        "SelectionBackgroundWeak",
        "HighlightBackground",
        "DividerStrokeColorDefaultBrush",
        "TextPrimary",
        "TextSecondary",
        "TextDisabled"
    ];

    [Theory]
    [InlineData(FluentThemeVariant.Dark)]
    [InlineData(FluentThemeVariant.Light)]
    [InlineData(FluentThemeVariant.HighContrast)]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void ThemeDictionary_ShouldExposeCollectionTokensForEveryTheme(FluentThemeVariant theme)
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = theme.ToString();

        var dictionary = LoadGenericThemeDictionary();

        foreach (var key in CollectionResourceKeys)
        {
            Assert.True(dictionary.TryGetValue(key, out var value), $"{key} was not found in the {theme} theme.");
            Assert.IsType<SolidColorBrush>(value);
        }

        ResetApplicationState();
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void Apply_ShouldRegisterFwCollectionTableStylesBasedOnJaliumStyles()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            AssertBasedOnStyle<FWListBox, ListBox>(app.Resources);
            AssertBasedOnStyle<FWListBoxItem, ListBoxItem>(app.Resources);
            AssertBasedOnStyle<FWListView, ListView>(app.Resources);
            AssertBasedOnStyle<FWListViewItem, ListViewItem>(app.Resources);
            AssertBasedOnStyle<FWGridView, ListView>(app.Resources);
            AssertBasedOnStyle<FWGridViewItem, ListViewItem>(app.Resources);
            AssertBasedOnStyle<FWTreeView, TreeView>(app.Resources);
            AssertBasedOnStyle<FWTreeViewItem, TreeViewItem>(app.Resources);
            AssertBasedOnStyle<FWDataGrid, DataGrid>(app.Resources);
            AssertBasedOnStyle<FWTreeDataGrid, TreeDataGrid>(app.Resources);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void GenericTheme_ShouldDefineCollectionTableBaseStyles()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = FluentThemeVariant.Dark.ToString();

        var dictionary = LoadGenericThemeDictionary();

        AssertContainsStyle<ListBox>(dictionary);
        AssertContainsStyle<ListBoxItem>(dictionary);
        AssertContainsStyle<FWListBox>(dictionary);
        AssertContainsStyle<FWListBoxItem>(dictionary);
        AssertContainsStyle<ListView>(dictionary);
        AssertContainsStyle<ListViewItem>(dictionary);
        AssertContainsStyle<FWListView>(dictionary);
        AssertContainsStyle<FWListViewItem>(dictionary);
        AssertContainsStyle<FWGridView>(dictionary);
        AssertContainsStyle<FWGridViewItem>(dictionary);
        AssertContainsStyle<GridViewColumnHeader>(dictionary);
        AssertContainsStyle<TreeView>(dictionary);
        AssertContainsStyle<TreeViewItem>(dictionary);
        AssertContainsStyle<FWTreeView>(dictionary);
        AssertContainsStyle<FWTreeViewItem>(dictionary);
        AssertContainsStyle<DataGrid>(dictionary);
        AssertContainsStyle<DataGridRow>(dictionary);
        AssertContainsStyle<DataGridCell>(dictionary);
        AssertContainsStyle<DataGridColumnHeader>(dictionary);
        AssertContainsStyle<FWDataGrid>(dictionary);
        AssertContainsStyle<TreeDataGrid>(dictionary);
        AssertContainsStyle<FWTreeDataGrid>(dictionary);
        AssertContainsStyle<TreeDataGridRow>(dictionary);

        var dataGridStyle = AssertStyle<DataGrid>(dictionary);
        AssertSetter(dataGridStyle, Control.BackgroundProperty);
        AssertSetter(dataGridStyle, Control.BorderBrushProperty);
        AssertSetter(dataGridStyle, DataGrid.RowHeightProperty);
        AssertSetter(dataGridStyle, DataGrid.ColumnHeaderHeightProperty);
        AssertSetter(dataGridStyle, DataGrid.HorizontalGridLinesBrushProperty);
        AssertSetter(dataGridStyle, DataGrid.VerticalGridLinesBrushProperty);

        var fwDataGridStyle = AssertStyle<FWDataGrid>(dictionary);
        Assert.Same(dataGridStyle, fwDataGridStyle.BasedOn);
        AssertSetter(fwDataGridStyle, FWDataGrid.DensityProperty);

        var listBoxStyle = AssertStyle<ListBox>(dictionary);
        var fwListBoxStyle = AssertStyle<FWListBox>(dictionary);
        Assert.Same(listBoxStyle, fwListBoxStyle.BasedOn);
        AssertSetter(fwListBoxStyle, FWListBox.DensityProperty);

        var listBoxItemStyle = AssertStyle<ListBoxItem>(dictionary);
        var fwListBoxItemStyle = AssertStyle<FWListBoxItem>(dictionary);
        Assert.Same(listBoxItemStyle, fwListBoxItemStyle.BasedOn);
        AssertSetter(fwListBoxItemStyle, FWListBoxItem.DensityProperty);

        var listViewStyle = AssertStyle<ListView>(dictionary);
        var fwListViewStyle = AssertStyle<FWListView>(dictionary);
        Assert.Same(listViewStyle, fwListViewStyle.BasedOn);
        AssertSetter(fwListViewStyle, FWListView.DensityProperty);

        var listViewItemStyle = AssertStyle<ListViewItem>(dictionary);
        var fwListViewItemStyle = AssertStyle<FWListViewItem>(dictionary);
        Assert.Same(listViewItemStyle, fwListViewItemStyle.BasedOn);
        AssertSetter(fwListViewItemStyle, FWListViewItem.DensityProperty);

        var fwGridViewStyle = AssertStyle<FWGridView>(dictionary);
        Assert.Same(listViewStyle, fwGridViewStyle.BasedOn);
        AssertSetter(fwGridViewStyle, FWGridView.DensityProperty);

        var fwGridViewItemStyle = AssertStyle<FWGridViewItem>(dictionary);
        Assert.Same(listViewItemStyle, fwGridViewItemStyle.BasedOn);
        AssertSetter(fwGridViewItemStyle, FWGridViewItem.DensityProperty);

        var treeViewStyle = AssertStyle<TreeView>(dictionary);
        var fwTreeViewStyle = AssertStyle<FWTreeView>(dictionary);
        Assert.Same(treeViewStyle, fwTreeViewStyle.BasedOn);
        AssertSetter(fwTreeViewStyle, FWTreeView.DensityProperty);

        var treeViewItemStyle = AssertStyle<TreeViewItem>(dictionary);
        var fwTreeViewItemStyle = AssertStyle<FWTreeViewItem>(dictionary);
        Assert.Same(treeViewItemStyle, fwTreeViewItemStyle.BasedOn);
        AssertSetter(fwTreeViewItemStyle, FWTreeViewItem.DensityProperty);

        var treeDataGridStyle = AssertStyle<TreeDataGrid>(dictionary);
        AssertSetter(treeDataGridStyle, Control.BackgroundProperty);
        AssertSetter(treeDataGridStyle, Control.BorderBrushProperty);
        AssertSetter(treeDataGridStyle, TreeDataGrid.RowHeightProperty);
        AssertSetter(treeDataGridStyle, TreeDataGrid.ColumnHeaderHeightProperty);
        AssertSetter(treeDataGridStyle, TreeDataGrid.HorizontalGridLinesBrushProperty);
        AssertSetter(treeDataGridStyle, TreeDataGrid.VerticalGridLinesBrushProperty);

        var fwTreeDataGridStyle = AssertStyle<FWTreeDataGrid>(dictionary);
        Assert.Same(treeDataGridStyle, fwTreeDataGridStyle.BasedOn);
        AssertSetter(fwTreeDataGridStyle, FWTreeDataGrid.DensityProperty);

        ResetApplicationState();
    }

    [Fact]
    public void FWCollections_ShouldGenerateFwItemContainersForPlainItems()
    {
        var listBox = new TestListBox();
        var listView = new TestListView();
        var gridView = new TestGridView();
        var treeView = new TestTreeView();
        var treeItem = new TestTreeViewItem();

        Assert.IsType<FWListBoxItem>(listBox.CreateContainer("ListBox item"));
        Assert.IsType<FWListViewItem>(listView.CreateContainer("ListView item"));
        Assert.IsType<FWGridViewItem>(gridView.CreateContainer("GridView item"));
        Assert.IsType<FWTreeViewItem>(treeView.CreateContainer("Tree root"));
        Assert.IsType<FWTreeViewItem>(treeItem.CreateContainer("Tree child"));
    }

    [Fact]
    public void FWCollections_ShouldApplyDensityPresetsAndCreateMatchingContainers()
    {
        var listBox = new TestListBox();

        Assert.Equal(FWCollectionDensity.Comfortable, listBox.Density);
        Assert.Equal(new Thickness(3), listBox.Padding);

        var comfortableListBoxItem = Assert.IsType<FWListBoxItem>(listBox.CreateContainer("Comfortable item"));
        Assert.Equal(FWCollectionDensity.Comfortable, comfortableListBoxItem.Density);
        Assert.Equal(40, comfortableListBoxItem.MinHeight);
        Assert.Equal(new Thickness(14, 6, 12, 6), comfortableListBoxItem.Padding);

        listBox.Density = FWCollectionDensity.Compact;

        Assert.Equal(new Thickness(2), listBox.Padding);

        var compactListBoxItem = Assert.IsType<FWListBoxItem>(listBox.CreateContainer("Compact item"));
        Assert.Equal(FWCollectionDensity.Compact, compactListBoxItem.Density);
        Assert.Equal(32, compactListBoxItem.MinHeight);
        Assert.Equal(new Thickness(10, 4, 10, 4), compactListBoxItem.Padding);

        var listView = new TestListView
        {
            Density = FWCollectionDensity.Spacious
        };

        Assert.Equal(new Thickness(4), listView.Padding);

        var spaciousListViewItem = Assert.IsType<FWListViewItem>(listView.CreateContainer("Spacious item"));
        Assert.Equal(FWCollectionDensity.Spacious, spaciousListViewItem.Density);
        Assert.Equal(48, spaciousListViewItem.MinHeight);
        Assert.Equal(new Thickness(16, 8, 16, 8), spaciousListViewItem.Padding);

        var gridView = new TestGridView
        {
            Density = FWCollectionDensity.Compact
        };

        Assert.Equal(new Thickness(2), gridView.Padding);

        var compactGridViewItem = Assert.IsType<FWGridViewItem>(gridView.CreateContainer("Compact grid item"));
        Assert.Equal(FWCollectionDensity.Compact, compactGridViewItem.Density);
        Assert.Equal(32, compactGridViewItem.MinHeight);
        Assert.Equal(new Thickness(10, 4, 10, 4), compactGridViewItem.Padding);

        compactGridViewItem.Density = FWCollectionDensity.Spacious;

        Assert.Equal(48, compactGridViewItem.MinHeight);
        Assert.Equal(new Thickness(16, 8, 16, 8), compactGridViewItem.Padding);

        var treeView = new TestTreeView
        {
            Density = FWCollectionDensity.Spacious
        };

        Assert.Equal(new Thickness(4), treeView.Padding);

        var spaciousTreeItem = Assert.IsType<FWTreeViewItem>(treeView.CreateContainer("Tree item"));
        Assert.Equal(FWCollectionDensity.Spacious, spaciousTreeItem.Density);
        Assert.Equal(36, spaciousTreeItem.MinHeight);
        Assert.Equal(new Thickness(12, 7, 12, 7), spaciousTreeItem.Padding);

        spaciousTreeItem.Density = FWCollectionDensity.Compact;

        Assert.Equal(24, spaciousTreeItem.MinHeight);
        Assert.Equal(new Thickness(8, 3, 8, 3), spaciousTreeItem.Padding);
    }

    [Fact]
    public void FWListBox_ShouldSwitchModesSelectAllAndClearSelection()
    {
        var listBox = new FWListBox
        {
            SelectionMode = Jalium.UI.Controls.Primitives.SelectionMode.Multiple
        };
        listBox.Items.Add("Fluent tokens");
        listBox.Items.Add("Control states");
        listBox.Items.Add("Gallery coverage");

        listBox.SelectAll();

        Assert.Equal(3, listBox.SelectedItems.Count);

        listBox.SelectionMode = Jalium.UI.Controls.Primitives.SelectionMode.Single;
        listBox.SelectedIndex = 1;

        Assert.Equal("Control states", listBox.SelectedItem);
        Assert.Single(listBox.SelectedItems);

        listBox.UnselectAll();

        Assert.Empty(listBox.SelectedItems);
        Assert.Null(listBox.SelectedItem);
    }

    [Fact]
    public void FWListView_ShouldExposeGridViewColumnsSelectionAndReorderOption()
    {
        var rows = new[]
        {
            new CollectionRow("Buttons", "Complete", 9),
            new CollectionRow("Selection", "Review", 4),
            new CollectionRow("Collections", "Active", 8)
        };
        var view = new GridView
        {
            AllowsColumnReorder = true
        };
        view.Columns.Add(new GridViewColumn { Header = "Name", DisplayMemberBinding = new Binding("Name"), Width = 140 });
        view.Columns.Add(new GridViewColumn { Header = "State", DisplayMemberBinding = new Binding("State"), Width = 110 });
        view.Columns.Add(new GridViewColumn { Header = "Count", DisplayMemberBinding = new Binding("Count"), Width = 80 });

        var listView = new FWListView
        {
            ItemsSource = rows,
            View = view,
            SelectedIndex = 2
        };

        Assert.True(view.AllowsColumnReorder);
        Assert.Equal(3, view.Columns.Count);
        Assert.Equal("Count", view.Columns[2].Header);
        Assert.Equal(rows[2], listView.SelectedItem);

        var gridView = new FWGridView
        {
            ItemsSource = rows,
            SelectedIndex = 1
        };

        Assert.Equal(rows[1], gridView.SelectedItem);
    }

    [Fact]
    public void FWDataGrid_ShouldToggleTableOptionsAndSynchronizeExtendedSelection()
    {
        var rows = new[]
        {
            new CollectionRow("Buttons", "Complete", 9),
            new CollectionRow("Selection", "Review", 4),
            new CollectionRow("Collections", "Active", 8)
        };
        var dataGrid = new FWDataGrid
        {
            AutoGenerateColumns = false,
            ItemsSource = rows,
            GridLinesVisibility = DataGridGridLinesVisibility.All,
            HeadersVisibility = DataGridHeadersVisibility.All,
            SelectionMode = DataGridSelectionMode.Extended
        };
        dataGrid.Columns.Add(new DataGridTextColumn { Header = "Name", Binding = new Binding("Name"), Width = 150 });
        dataGrid.Columns.Add(new DataGridTextColumn { Header = "State", Binding = new Binding("State"), Width = 110 });

        dataGrid.SelectAll();

        Assert.Equal(3, dataGrid.SelectedItems.Count);

        dataGrid.SelectionMode = DataGridSelectionMode.Single;
        dataGrid.SelectedIndex = 1;
        dataGrid.GridLinesVisibility = DataGridGridLinesVisibility.Horizontal;
        dataGrid.HeadersVisibility = DataGridHeadersVisibility.Column;
        dataGrid.IsReadOnly = true;

        Assert.Single(dataGrid.SelectedItems);
        Assert.Equal(rows[1], dataGrid.SelectedItem);
        Assert.Equal(DataGridGridLinesVisibility.Horizontal, dataGrid.GridLinesVisibility);
        Assert.Equal(DataGridHeadersVisibility.Column, dataGrid.HeadersVisibility);
        Assert.True(dataGrid.IsReadOnly);
    }

    [Fact]
    public void FWDataGrids_ShouldApplyDensityPresets()
    {
        var dataGrid = new FWDataGrid();

        Assert.Equal(FWDataGridDensity.Comfortable, dataGrid.Density);
        Assert.Equal(32, dataGrid.RowHeight);
        Assert.Equal(36, dataGrid.ColumnHeaderHeight);

        dataGrid.Density = FWDataGridDensity.Compact;

        Assert.Equal(26, dataGrid.RowHeight);
        Assert.Equal(30, dataGrid.ColumnHeaderHeight);

        dataGrid.Density = FWDataGridDensity.Spacious;

        Assert.Equal(40, dataGrid.RowHeight);
        Assert.Equal(44, dataGrid.ColumnHeaderHeight);

        var treeDataGrid = new FWTreeDataGrid
        {
            Density = FWDataGridDensity.Spacious
        };

        Assert.Equal(40, treeDataGrid.RowHeight);
        Assert.Equal(44, treeDataGrid.ColumnHeaderHeight);

        treeDataGrid.Density = FWDataGridDensity.Comfortable;

        Assert.Equal(32, treeDataGrid.RowHeight);
        Assert.Equal(36, treeDataGrid.ColumnHeaderHeight);
    }

    [Fact]
    public void FWDataGrid_ShouldExposeMaterialSurfaceDensityAndLayeringProperties()
    {
        var rows = new[]
        {
            new CollectionRow("Buttons", "Complete", 9),
            new CollectionRow("Selection", "Review", 4),
            new CollectionRow("Collections", "Active", 8)
        };
        var alternatingBrush = new SolidColorBrush(Color.FromArgb(32, 255, 255, 255));
        var dataGrid = new FWDataGrid
        {
            AutoGenerateColumns = false,
            ItemsSource = rows,
            Density = FWDataGridDensity.Comfortable,
            AlternatingRowBackground = alternatingBrush,
            GridLinesVisibility = DataGridGridLinesVisibility.All,
            HeadersVisibility = DataGridHeadersVisibility.All
        };
        dataGrid.Columns.Add(new DataGridTextColumn { Header = "Name", Binding = new Binding("Name"), Width = 150 });
        dataGrid.Columns.Add(new DataGridTextColumn { Header = "State", Binding = new Binding("State"), Width = 110 });
        dataGrid.SelectedIndex = 2;

        var surface = new FWFluentMaterialSurface
        {
            MaterialKind = FWFluentMaterialKind.LiquidGlass,
            TintOpacity = 0.2,
            BlurRadius = 14,
            RefractionAmount = 70,
            ChromaticAberration = 0.42,
            FusionRadius = 24,
            Shape = BorderShape.SuperEllipse,
            SuperEllipseN = 4,
            Child = dataGrid
        };

        Assert.Equal(32, dataGrid.RowHeight);
        Assert.Equal(36, dataGrid.ColumnHeaderHeight);
        Assert.Same(alternatingBrush, dataGrid.AlternatingRowBackground);
        Assert.Equal(rows[2], dataGrid.SelectedItem);
        Assert.Equal(FWFluentMaterialKind.LiquidGlass, surface.MaterialKind);
        Assert.Equal(70, surface.RefractionAmount);
        Assert.Equal(0.42, surface.ChromaticAberration);
        Assert.Equal(24, surface.FusionRadius);
        Assert.Equal(BorderShape.SuperEllipse, surface.Shape);
        Assert.Equal(4, surface.SuperEllipseN);
        Assert.Same(dataGrid, surface.Child);
    }

    [Fact]
    public void FWTreeDataGrid_ShouldExpandCollapseAndSelectVisibleRows()
    {
        var rows = new[]
        {
            new CollectionNode(
                "FluentJalium",
                "Active",
                [
                    new CollectionNode("Theme resources", "Loaded", []),
                    new CollectionNode("FW controls", "Expanding", [])
                ]),
            new CollectionNode("Gallery", "Visible", [])
        };
        var grid = new FWTreeDataGrid
        {
            ChildrenSelector = item => ((CollectionNode)item).Children,
            HasChildrenSelector = item => ((CollectionNode)item).Children.Length > 0,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            HeadersVisibility = DataGridHeadersVisibility.Column
        };
        grid.ItemsSource = rows;
        grid.Columns.Add(new DataGridTextColumn { Header = "Area", Binding = new Binding("Name"), Width = 170 });
        grid.Columns.Add(new DataGridTextColumn { Header = "State", Binding = new Binding("State"), Width = 120 });

        Assert.Equal(2, grid.FlattenedCount);

        grid.ExpandAll();
        grid.SelectedIndex = 1;

        Assert.Equal(4, grid.FlattenedCount);
        Assert.Equal(rows[0].Children[0], grid.SelectedItem);
        Assert.Equal(1, grid.GetLevel(1));
        Assert.Equal(DataGridGridLinesVisibility.Horizontal, grid.GridLinesVisibility);
        Assert.Equal(DataGridHeadersVisibility.Column, grid.HeadersVisibility);

        grid.CollapseAll();

        Assert.Equal(2, grid.FlattenedCount);
    }

    private sealed class TestListBox : FWListBox
    {
        public FrameworkElement CreateContainer(object item) => GetContainerForItem(item);
    }

    private sealed class TestListView : FWListView
    {
        public FrameworkElement CreateContainer(object item) => GetContainerForItem(item);
    }

    private sealed class TestGridView : FWGridView
    {
        public FrameworkElement CreateContainer(object item) => GetContainerForItem(item);
    }

    private sealed class TestTreeView : FWTreeView
    {
        public FrameworkElement CreateContainer(object item) => GetContainerForItem(item);
    }

    private sealed class TestTreeViewItem : FWTreeViewItem
    {
        public FrameworkElement CreateContainer(object item) => GetContainerForItem(item);
    }

    private sealed record CollectionRow(string Name, string State, int Count);

    private sealed record CollectionNode(string Name, string State, CollectionNode[] Children);

    private static ResourceDictionary LoadGenericThemeDictionary()
    {
        var loaded = ResourceDictionary.SourceLoader?.Invoke(
            new ResourceDictionary(),
            new Uri("/FluentJalium;component/Themes/Generic.jalxaml", UriKind.Relative),
            FluentThemeManager.ThemeAssembly);

        return Assert.IsType<ResourceDictionary>(loaded);
    }

    private static void AssertContainsStyle<TControl>(ResourceDictionary dictionary)
        where TControl : FrameworkElement
    {
        Assert.True(dictionary.TryGetValue(typeof(TControl), out var value), $"{typeof(TControl).Name} style was not found.");
        Assert.IsType<Style>(value);
    }

    private static Style AssertStyle<TControl>(ResourceDictionary dictionary)
        where TControl : FrameworkElement
    {
        Assert.True(dictionary.TryGetValue(typeof(TControl), out var value), $"{typeof(TControl).Name} style was not found.");
        return Assert.IsType<Style>(value);
    }

    private static void AssertBasedOnStyle<TFluentControl, TJaliumControl>(ResourceDictionary dictionary)
        where TFluentControl : TJaliumControl, IFluentJaliumControl
        where TJaliumControl : FrameworkElement
    {
        var baseStyle = AssertStyle<TJaliumControl>(dictionary);
        var fluentStyle = AssertStyle<TFluentControl>(dictionary);

        Assert.Equal(typeof(TFluentControl), fluentStyle.TargetType);
        Assert.Same(baseStyle, fluentStyle.BasedOn);
    }

    private static void AssertSetter(Style style, DependencyProperty property)
    {
        Assert.Contains(style.Setters, setter => setter.Property == property);
    }

    private static void ResetApplicationState()
    {
        var currentField = typeof(Application).GetField("_current", BindingFlags.NonPublic | BindingFlags.Static);
        currentField?.SetValue(null, null);

        var jaliumReset = typeof(JaliumThemeManager).GetMethod("Reset", BindingFlags.NonPublic | BindingFlags.Static);
        jaliumReset?.Invoke(null, null);

        var fluentReset = typeof(FluentThemeManager).GetMethod("Reset", BindingFlags.NonPublic | BindingFlags.Static);
        fluentReset?.Invoke(null, null);
    }
}
