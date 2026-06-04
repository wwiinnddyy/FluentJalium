using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Controls.Themes;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using JaliumThemeManager = Jalium.UI.Controls.Themes.ThemeManager;

namespace FluentJalium.Tests;

[Collection("Application")]
public sealed class FluentAdvancedSelectionPropertyTests
{
    private static readonly string[] AdvancedSelectionPropertyResourceKeys =
    [
        "TreeSelectorBackground",
        "TreeSelectorBorderBrush",
        "TreeSelectorPopupBackground",
        "TreeSelectorItemBackgroundSelected",
        "TreeSelectorItemSelectedIndicator",
        "PropertyGridBackground",
        "PropertyGridBorderBrush",
        "PropertyGridToolbarBackground",
        "PropertyGridSearchBackground",
        "PropertyGridCategoryHeaderBackground",
        "PropertyGridCategoryHeaderForeground",
        "PropertyGridPropertyNameForeground",
        "PropertyGridRowHoverBackground",
        "PropertyGridSelectionBackground",
        "PropertyGridDescriptionBackground"
    ];

    [Theory]
    [InlineData(FluentThemeVariant.Dark)]
    [InlineData(FluentThemeVariant.Light)]
    [InlineData(FluentThemeVariant.HighContrast)]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void ThemeDictionary_ShouldExposeAdvancedSelectionPropertyTokensForEveryTheme(FluentThemeVariant theme)
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = theme.ToString();

        var dictionary = LoadGenericThemeDictionary();

        foreach (var key in AdvancedSelectionPropertyResourceKeys)
        {
            Assert.True(dictionary.TryGetValue(key, out var value), $"{key} was not found in the {theme} theme.");
            Assert.IsType<SolidColorBrush>(value);
        }

        ResetApplicationState();
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void Apply_ShouldRegisterFwAdvancedSelectionPropertyStylesBasedOnJaliumStyles()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            AssertBasedOnStyle<FWTreeSelector, TreeSelector>(app.Resources);
            AssertBasedOnStyle<FWTreeSelectorItem, TreeSelectorItem>(app.Resources);
            AssertBasedOnStyle<FWPropertyGrid, PropertyGrid>(app.Resources);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void GenericTheme_ShouldDefineAdvancedSelectionPropertyBaseStyles()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = FluentThemeVariant.Dark.ToString();

        var dictionary = LoadGenericThemeDictionary();

        var selectorStyle = AssertStyle<TreeSelector>(dictionary);
        AssertSetter(selectorStyle, Control.BackgroundProperty);
        AssertSetter(selectorStyle, Control.BorderBrushProperty);
        AssertSetter(selectorStyle, Control.TemplateProperty);

        var selectorItemStyle = AssertStyle<TreeSelectorItem>(dictionary);
        AssertSetter(selectorItemStyle, Control.BackgroundProperty);
        AssertSetter(selectorItemStyle, Control.TemplateProperty);

        var propertyGridStyle = AssertStyle<PropertyGrid>(dictionary);
        AssertSetter(propertyGridStyle, Control.BackgroundProperty);
        AssertSetter(propertyGridStyle, Control.BorderBrushProperty);
        AssertSetter(propertyGridStyle, PropertyGrid.CategoryHeaderBackgroundProperty);
        AssertSetter(propertyGridStyle, PropertyGrid.CategoryHeaderForegroundProperty);
        AssertSetter(propertyGridStyle, PropertyGrid.PropertyNameForegroundProperty);
        AssertSetter(propertyGridStyle, Control.TemplateProperty);

        var fwPropertyGridStyle = AssertStyle<FWPropertyGrid>(dictionary);
        Assert.Same(propertyGridStyle, fwPropertyGridStyle.BasedOn);
        AssertSetter(fwPropertyGridStyle, FWPropertyGrid.DensityProperty);

        Assert.True(dictionary.TryGetValue("PropertyGridToolbarButtonStyle", out var toolbarButtonStyle));
        Assert.IsType<Style>(toolbarButtonStyle);
        Assert.True(dictionary.TryGetValue("PropertyGridCategoryExpanderStyle", out var categoryStyle));
        Assert.IsType<Style>(categoryStyle);

        ResetApplicationState();
    }

    [Fact]
    public void FWTreeSelector_ShouldGenerateFwItemContainersForPlainItems()
    {
        var selector = new TestTreeSelector();
        var item = new TestTreeSelectorItem();

        Assert.IsType<FWTreeSelectorItem>(selector.CreateContainer("Root item"));
        Assert.IsType<FWTreeSelectorItem>(item.CreateContainer("Child item"));
    }

    [Fact]
    public void FWTreeSelector_ShouldSelectSearchAndRaiseDropDownEvents()
    {
        var selector = new FWTreeSelector
        {
            IsSearchEnabled = true,
            PlaceholderText = "Choose",
            PathSeparator = " > ",
            MaxDropDownHeight = 280
        };
        var root = new FWTreeSelectorItem { Header = "Root", IsExpanded = true };
        var child = new FWTreeSelectorItem { Header = "Theme resources" };
        root.Items.Add(child);
        selector.Items.Add(root);

        var opened = 0;
        var closed = 0;
        selector.DropDownOpened += (_, _) => opened++;
        selector.DropDownClosed += (_, _) => closed++;

        selector.IsDropDownOpen = true;
        selector.SearchText = "theme";
        selector.SelectedItem = "Theme resources";
        selector.IsDropDownOpen = false;

        Assert.False(selector.IsDropDownOpen);
        Assert.Equal("Theme resources", selector.SelectedItem);
        Assert.Single(selector.SelectedItems);
        Assert.True(child.IsSelected);
        Assert.Equal("theme", selector.SearchText);
        Assert.Equal("Choose", selector.PlaceholderText);
        Assert.Equal(" > ", selector.PathSeparator);
        Assert.Equal(280.0, selector.MaxDropDownHeight);
        Assert.Equal(1, opened);
        Assert.Equal(1, closed);
    }

    [Fact]
    public void FWTreeSelector_ShouldCascadeCheckStateAndTrackSelectedItems()
    {
        var selector = new FWTreeSelector
        {
            SelectionMode = SelectionMode.Multiple,
            ShowCheckBoxes = true,
            CheckCascadeMode = TreeSelectorCheckCascadeMode.Cascade
        };
        var root = new FWTreeSelectorItem { Header = "Foundation" };
        var theme = new FWTreeSelectorItem { Header = "Theme resources" };
        var typography = new FWTreeSelectorItem { Header = "Typography" };
        root.Items.Add(theme);
        root.Items.Add(typography);
        selector.Items.Add(root);

        var selectionEvents = 0;
        var checkEvents = 0;
        selector.SelectionChanged += (_, _) => selectionEvents++;
        selector.ItemCheckStateChanged += (_, _) => checkEvents++;

        root.IsChecked = true;
        selector.SelectAll();
        typography.IsChecked = false;

        Assert.True(theme.IsChecked);
        Assert.False(typography.IsChecked);
        Assert.Null(root.IsChecked);
        Assert.Equal(3, selector.SelectedItems.Count);
        Assert.True(root.IsSelected);
        Assert.True(theme.IsSelected);
        Assert.True(typography.IsSelected);
        Assert.Null(selector.SelectedItem);
        Assert.True(checkEvents > 0);
        Assert.Equal(1, selectionEvents);

        selector.UnselectAll();

        Assert.Empty(selector.SelectedItems);
        Assert.Null(selector.SelectedItem);
        Assert.Equal(2, selectionEvents);
    }

    [Fact]
    public void FWPropertyGrid_ShouldApplyDensityPresets()
    {
        var grid = new FWPropertyGrid();

        Assert.Equal(FWPropertyGridDensity.Comfortable, grid.Density);
        Assert.Equal(150, grid.NameColumnWidth);

        grid.Density = FWPropertyGridDensity.Compact;

        Assert.Equal(128, grid.NameColumnWidth);

        grid.Density = FWPropertyGridDensity.Spacious;

        Assert.Equal(176, grid.NameColumnWidth);
    }

    [Fact]
    [RequiresUnreferencedCode("Discovers sample properties through PropertyGrid reflection.")]
    public void FWPropertyGrid_ShouldExposeObjectEditingModesAndEvents()
    {
        var sample = new PropertyGridSample();
        var grid = new FWPropertyGrid
        {
            SelectedObject = sample,
            SortMode = PropertyGridSortMode.Categorized,
            ShowSearchBox = true,
            ShowDescription = true,
            ShowToolBar = true,
            SearchText = "layout",
            IsReadOnly = true,
            Density = FWPropertyGridDensity.Spacious
        };
        grid.RegisterCustomEditor(typeof(string), (_, _) => new FWTextBox());

        var selectedEvents = 0;
        grid.SelectedPropertyChanged += (_, _) => selectedEvents++;

        grid.SortMode = PropertyGridSortMode.Alphabetical;
        grid.ShowSearchBox = false;
        grid.ShowDescription = false;
        grid.ShowToolBar = false;
        grid.IsReadOnly = false;
        grid.RefreshProperties();

        var titleProperty = typeof(PropertyGridSample).GetProperty(nameof(PropertyGridSample.Title))!;
        var selectedProperty = new PropertyItem(sample, titleProperty);
        grid.SelectedProperty = selectedProperty;

        Assert.Same(sample, grid.SelectedObject);
        Assert.Equal(PropertyGridSortMode.Alphabetical, grid.SortMode);
        Assert.False(grid.ShowSearchBox);
        Assert.False(grid.ShowDescription);
        Assert.False(grid.ShowToolBar);
        Assert.Equal("layout", grid.SearchText);
        Assert.False(grid.IsReadOnly);
        Assert.Equal(176.0, grid.NameColumnWidth);
        Assert.Same(selectedProperty, grid.SelectedProperty);
        Assert.Equal(1, selectedEvents);
    }

    [Fact]
    public void FWAdvancedSelectionProperties_ShouldExposeMaterialEditorPanelState()
    {
        var sample = new PropertyGridSample();
        var selector = new FWTreeSelector
        {
            SelectionMode = SelectionMode.Single,
            PlaceholderText = "Choose a property source",
            IsSearchEnabled = true,
            SearchText = "theme",
            MaxDropDownHeight = 220
        };
        var root = new FWTreeSelectorItem { Header = "FluentJalium", IsExpanded = true };
        var theme = new FWTreeSelectorItem { Header = "Theme resources" };
        root.Items.Add(theme);
        selector.Items.Add(root);
        selector.SelectedItem = "Theme resources";

        var grid = new FWPropertyGrid
        {
            SelectedObject = sample,
            SortMode = PropertyGridSortMode.Categorized,
            ShowSearchBox = true,
            ShowDescription = true,
            ShowToolBar = false,
            Density = FWPropertyGridDensity.Compact,
            IsReadOnly = true
        };

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
            Child = grid
        };

        Assert.Equal("Theme resources", selector.SelectedItem);
        Assert.Single(selector.SelectedItems);
        Assert.True(theme.IsSelected);
        Assert.Equal("theme", selector.SearchText);
        Assert.Equal(220, selector.MaxDropDownHeight);
        Assert.Same(sample, grid.SelectedObject);
        Assert.Equal(PropertyGridSortMode.Categorized, grid.SortMode);
        Assert.True(grid.ShowSearchBox);
        Assert.True(grid.ShowDescription);
        Assert.False(grid.ShowToolBar);
        Assert.True(grid.IsReadOnly);
        Assert.Equal(128, grid.NameColumnWidth);
        Assert.Equal(FWFluentMaterialKind.LiquidGlass, surface.MaterialKind);
        Assert.Equal(70, surface.RefractionAmount);
        Assert.Equal(0.42, surface.ChromaticAberration);
        Assert.Equal(24, surface.FusionRadius);
        Assert.Equal(BorderShape.SuperEllipse, surface.Shape);
        Assert.Equal(4, surface.SuperEllipseN);
        Assert.Same(grid, surface.Child);
    }

    private sealed class TestTreeSelector : FWTreeSelector
    {
        public FrameworkElement CreateContainer(object item) => GetContainerForItem(item);
    }

    private sealed class TestTreeSelectorItem : FWTreeSelectorItem
    {
        public FrameworkElement CreateContainer(object item) => GetContainerForItem(item);
    }

    private sealed class PropertyGridSample
    {
        [Category("Appearance")]
        [Description("Displayed title.")]
        public string Title { get; set; } = "FluentJalium";

        [Category("Layout")]
        [Description("Width in pixels.")]
        public double Width { get; set; } = 420;

        [Category("Behavior")]
        [Description("Whether editing is enabled.")]
        public bool IsEnabled { get; set; } = true;
    }

    private static ResourceDictionary LoadGenericThemeDictionary()
    {
        var loaded = ResourceDictionary.SourceLoader?.Invoke(
            new ResourceDictionary(),
            new Uri("/FluentJalium;component/Themes/Generic.jalxaml", UriKind.Relative),
            FluentThemeManager.ThemeAssembly);

        return Assert.IsType<ResourceDictionary>(loaded);
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
