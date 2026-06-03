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
        AssertContainsStyle<ListView>(dictionary);
        AssertContainsStyle<ListViewItem>(dictionary);
        AssertContainsStyle<GridViewColumnHeader>(dictionary);
        AssertContainsStyle<TreeView>(dictionary);
        AssertContainsStyle<TreeViewItem>(dictionary);
        AssertContainsStyle<DataGrid>(dictionary);
        AssertContainsStyle<DataGridRow>(dictionary);
        AssertContainsStyle<DataGridCell>(dictionary);
        AssertContainsStyle<DataGridColumnHeader>(dictionary);
        AssertContainsStyle<TreeDataGrid>(dictionary);
        AssertContainsStyle<TreeDataGridRow>(dictionary);

        var dataGridStyle = AssertStyle<DataGrid>(dictionary);
        AssertSetter(dataGridStyle, Control.BackgroundProperty);
        AssertSetter(dataGridStyle, Control.BorderBrushProperty);
        AssertSetter(dataGridStyle, DataGrid.RowHeightProperty);
        AssertSetter(dataGridStyle, DataGrid.ColumnHeaderHeightProperty);
        AssertSetter(dataGridStyle, DataGrid.HorizontalGridLinesBrushProperty);
        AssertSetter(dataGridStyle, DataGrid.VerticalGridLinesBrushProperty);

        var treeDataGridStyle = AssertStyle<TreeDataGrid>(dictionary);
        AssertSetter(treeDataGridStyle, Control.BackgroundProperty);
        AssertSetter(treeDataGridStyle, Control.BorderBrushProperty);
        AssertSetter(treeDataGridStyle, TreeDataGrid.RowHeightProperty);
        AssertSetter(treeDataGridStyle, TreeDataGrid.ColumnHeaderHeightProperty);
        AssertSetter(treeDataGridStyle, TreeDataGrid.HorizontalGridLinesBrushProperty);
        AssertSetter(treeDataGridStyle, TreeDataGrid.VerticalGridLinesBrushProperty);

        ResetApplicationState();
    }

    [Fact]
    public void FWCollections_ShouldGenerateFwItemContainersForPlainItems()
    {
        var listBox = new TestListBox();
        var listView = new TestListView();
        var treeView = new TestTreeView();
        var treeItem = new TestTreeViewItem();

        Assert.IsType<FWListBoxItem>(listBox.CreateContainer("ListBox item"));
        Assert.IsType<FWListViewItem>(listView.CreateContainer("ListView item"));
        Assert.IsType<FWTreeViewItem>(treeView.CreateContainer("Tree root"));
        Assert.IsType<FWTreeViewItem>(treeItem.CreateContainer("Tree child"));
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
            RowHeight = 30,
            ColumnHeaderHeight = 34,
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

        Assert.Equal(30, dataGrid.RowHeight);
        Assert.Equal(34, dataGrid.ColumnHeaderHeight);
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
