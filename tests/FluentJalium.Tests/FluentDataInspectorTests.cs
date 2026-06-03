using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using JaliumThemeManager = Jalium.UI.Controls.Themes.ThemeManager;

namespace FluentJalium.Tests;

[Collection("Application")]
public sealed class FluentDataInspectorTests
{
    private static readonly string[] DataInspectorResourceKeys =
    [
        "DataInspectorBackground",
        "DataInspectorForeground",
        "DataInspectorForegroundSecondary",
        "DataInspectorBorderBrush",
        "DataInspectorToolbarBackground",
        "DataInspectorGutterBackground",
        "DataInspectorLineNumberForeground",
        "DataInspectorSelectionBackground",
        "DataInspectorAddedLineBackground",
        "DataInspectorRemovedLineBackground",
        "DataInspectorModifiedLineBackground",
        "DataInspectorAddedWordBackground",
        "DataInspectorRemovedWordBackground",
        "DataInspectorOffsetForeground",
        "DataInspectorHexForeground",
        "DataInspectorAsciiForeground",
        "DataInspectorModifiedByteForeground",
        "DataInspectorColumnSeparatorBrush",
        "DataInspectorJsonObjectBrush",
        "DataInspectorJsonArrayBrush",
        "DataInspectorJsonStringBrush",
        "DataInspectorJsonNumberBrush",
        "DataInspectorJsonBooleanBrush",
        "DataInspectorJsonNullBrush",
        "DataInspectorJsonKeyBrush",
        "DataInspectorJsonBracketBrush"
    ];

    [Theory]
    [InlineData(FluentThemeVariant.Dark)]
    [InlineData(FluentThemeVariant.Light)]
    [InlineData(FluentThemeVariant.HighContrast)]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void ThemeDictionary_ShouldExposeDataInspectorTokensForEveryTheme(FluentThemeVariant theme)
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = theme.ToString();

        var dictionary = LoadGenericThemeDictionary();

        foreach (var key in DataInspectorResourceKeys)
        {
            Assert.True(dictionary.TryGetValue(key, out var value), $"{key} was not found in the {theme} theme.");
            Assert.IsType<SolidColorBrush>(value);
        }

        ResetApplicationState();
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void Apply_ShouldRegisterFwDataInspectorStylesBasedOnJaliumStyles()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            AssertBasedOnStyle<FWDiffViewer, DiffViewer>(app.Resources);
            AssertBasedOnStyle<FWHexEditor, HexEditor>(app.Resources);
            AssertBasedOnStyle<FWJsonTreeViewer, JsonTreeViewer>(app.Resources);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void GenericTheme_ShouldDefineDataInspectorBaseStyles()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = FluentThemeVariant.Dark.ToString();

        var dictionary = LoadGenericThemeDictionary();

        var diffStyle = AssertStyle<DiffViewer>(dictionary);
        AssertSetter(diffStyle, Control.BackgroundProperty);
        AssertSetter(diffStyle, Control.BorderBrushProperty);
        AssertSetter(diffStyle, DiffViewer.AddedLineBrushProperty);
        AssertSetter(diffStyle, DiffViewer.RemovedLineBrushProperty);
        AssertSetter(diffStyle, Control.TemplateProperty);

        var hexStyle = AssertStyle<HexEditor>(dictionary);
        AssertSetter(hexStyle, Control.BackgroundProperty);
        AssertSetter(hexStyle, Control.BorderBrushProperty);
        AssertSetter(hexStyle, HexEditor.OffsetForegroundProperty);
        AssertSetter(hexStyle, HexEditor.SelectionBrushProperty);
        AssertSetter(hexStyle, Control.TemplateProperty);

        var jsonStyle = AssertStyle<JsonTreeViewer>(dictionary);
        AssertSetter(jsonStyle, Control.BackgroundProperty);
        AssertSetter(jsonStyle, Control.BorderBrushProperty);
        AssertSetter(jsonStyle, JsonTreeViewer.ObjectBrushProperty);
        AssertSetter(jsonStyle, JsonTreeViewer.KeyBrushProperty);
        AssertSetter(jsonStyle, Control.TemplateProperty);

        ResetApplicationState();
    }

    [Fact]
    public void FWDiffViewer_ShouldExposeDiffModesAndNavigation()
    {
        var viewer = new FWDiffViewer
        {
            OriginalText = "alpha\nbeta\ngamma",
            ModifiedText = "alpha\nbeta updated\ngamma\ndelta",
            ViewMode = DiffViewMode.Unified,
            ShowLineNumbers = false,
            ShowMinimap = false,
            GutterWidth = 72,
            ContextLines = 2,
            EnableInlineEdit = true,
            IsReadOnly = false
        };

        viewer.NavigateToNextChange();

        Assert.Equal(DiffViewMode.Unified, viewer.ViewMode);
        Assert.False(viewer.ShowLineNumbers);
        Assert.False(viewer.ShowMinimap);
        Assert.Equal(72.0, viewer.GutterWidth);
        Assert.Equal(2, viewer.ContextLines);
        Assert.True(viewer.EnableInlineEdit);
        Assert.False(viewer.IsReadOnly);
        Assert.True(viewer.GetChangeCount() > 0);
    }

    [Fact]
    public void FWHexEditor_ShouldExposeBinaryViewingEditingState()
    {
        var editor = new FWHexEditor
        {
            Data = [0x48, 0x65, 0x6C, 0x6C, 0x6F],
            BytesPerRow = 8,
            ColumnGroupSize = 4,
            DisplayFormat = HexDisplayFormat.Word16,
            Endianness = Endianness.Big,
            ShowOffsetColumn = true,
            ShowAsciiColumn = false,
            ShowDataInterpretation = true,
            SelectionStart = 1,
            SelectionLength = 2,
            CaretOffset = 3,
            IsReadOnly = false
        };

        var offset = editor.FindBytes([0x6C, 0x6C]);
        editor.ReplaceBytes(1, [0x45, 0x4C]);

        Assert.Equal(2, offset);
        Assert.Equal(8, editor.BytesPerRow);
        Assert.Equal(4, editor.ColumnGroupSize);
        Assert.Equal(HexDisplayFormat.Word16, editor.DisplayFormat);
        Assert.Equal(Endianness.Big, editor.Endianness);
        Assert.True(editor.ShowOffsetColumn);
        Assert.False(editor.ShowAsciiColumn);
        Assert.True(editor.ShowDataInterpretation);
        Assert.Equal(1, editor.SelectionStart);
        Assert.Equal(2, editor.SelectionLength);
        Assert.Equal(3, editor.CaretOffset);
        Assert.Equal([0x48, 0x45, 0x4C, 0x6C, 0x6F], editor.Data);
    }

    [Fact]
    public void FWJsonTreeViewer_ShouldParseSearchAndExpandJson()
    {
        var viewer = new FWJsonTreeViewer
        {
            JsonText = """
                {
                  "theme": "Fluent",
                  "items": [1, 2, 3],
                  "enabled": true
                }
                """,
            SearchText = "theme",
            IsEditable = true,
            IndentSize = 18,
            ExpandDepth = 2,
            MaxRenderDepth = 8,
            ShowTypeIndicators = true,
            ShowItemCount = true
        };

        viewer.ExpandAll();
        viewer.CollapseAll();

        Assert.NotNull(viewer.RootNode);
        Assert.Equal("theme", viewer.SearchText);
        Assert.True(viewer.IsEditable);
        Assert.Equal(18, viewer.IndentSize);
        Assert.Equal(2, viewer.ExpandDepth);
        Assert.Equal(8, viewer.MaxRenderDepth);
        Assert.True(viewer.ShowTypeIndicators);
        Assert.True(viewer.ShowItemCount);
    }

    [Fact]
    public void FWDataInspectors_ShouldExposeMaterialWorkbenchState()
    {
        var diffViewer = new FWDiffViewer
        {
            OriginalText = "theme: dark\naccent: blue\nstatus: preview",
            ModifiedText = "theme: dark\naccent: teal\nstatus: ready",
            ViewMode = DiffViewMode.Unified,
            ShowLineNumbers = true,
            ShowMinimap = true,
            GutterWidth = 48,
            ContextLines = 1
        };
        var hexEditor = new FWHexEditor
        {
            Data = [0x46, 0x57, 0x20, 0x44, 0x61, 0x74, 0x61],
            BytesPerRow = 8,
            ColumnGroupSize = 4,
            ShowAsciiColumn = true,
            ShowOffsetColumn = true,
            ShowDataInterpretation = true,
            SelectionStart = 0,
            SelectionLength = 2
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
            Child = diffViewer
        };

        Assert.Equal(DiffViewMode.Unified, diffViewer.ViewMode);
        Assert.True(diffViewer.ShowLineNumbers);
        Assert.True(diffViewer.ShowMinimap);
        Assert.Equal(48, diffViewer.GutterWidth);
        Assert.Equal(1, diffViewer.ContextLines);
        Assert.True(diffViewer.GetChangeCount() > 0);
        Assert.Equal(0, hexEditor.FindBytes([0x46, 0x57]));
        Assert.Equal(8, hexEditor.BytesPerRow);
        Assert.True(hexEditor.ShowDataInterpretation);
        Assert.Equal(2, hexEditor.SelectionLength);
        Assert.Equal(FWFluentMaterialKind.LiquidGlass, surface.MaterialKind);
        Assert.Equal(70, surface.RefractionAmount);
        Assert.Equal(0.42, surface.ChromaticAberration);
        Assert.Equal(24, surface.FusionRadius);
        Assert.Equal(BorderShape.SuperEllipse, surface.Shape);
        Assert.Equal(4, surface.SuperEllipseN);
        Assert.Same(diffViewer, surface.Child);
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
