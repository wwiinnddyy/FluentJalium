using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using FluentJalium.Gallery.Pages;
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

        var fwDiffStyle = AssertStyle<FWDiffViewer>(dictionary);
        Assert.Same(diffStyle, fwDiffStyle.BasedOn);
        AssertSetter(fwDiffStyle, FWDiffViewer.DensityProperty);

        var hexStyle = AssertStyle<HexEditor>(dictionary);
        AssertSetter(hexStyle, Control.BackgroundProperty);
        AssertSetter(hexStyle, Control.BorderBrushProperty);
        AssertSetter(hexStyle, HexEditor.OffsetForegroundProperty);
        AssertSetter(hexStyle, HexEditor.SelectionBrushProperty);
        AssertSetter(hexStyle, Control.TemplateProperty);

        var fwHexStyle = AssertStyle<FWHexEditor>(dictionary);
        Assert.Same(hexStyle, fwHexStyle.BasedOn);
        AssertSetter(fwHexStyle, FWHexEditor.DensityProperty);

        var jsonStyle = AssertStyle<JsonTreeViewer>(dictionary);
        AssertSetter(jsonStyle, Control.BackgroundProperty);
        AssertSetter(jsonStyle, Control.BorderBrushProperty);
        AssertSetter(jsonStyle, JsonTreeViewer.ObjectBrushProperty);
        AssertSetter(jsonStyle, JsonTreeViewer.KeyBrushProperty);
        AssertSetter(jsonStyle, Control.TemplateProperty);

        var fwJsonStyle = AssertStyle<FWJsonTreeViewer>(dictionary);
        Assert.Same(jsonStyle, fwJsonStyle.BasedOn);
        AssertSetter(fwJsonStyle, FWJsonTreeViewer.DensityProperty);

        ResetApplicationState();
    }

    [Fact]
    public void FWDataInspectors_ShouldApplyDensityPresets()
    {
        var diffViewer = new FWDiffViewer();

        Assert.Equal(FWDataInspectorDensity.Comfortable, diffViewer.Density);
        Assert.Equal(13, diffViewer.FontSize);
        Assert.Equal(60, diffViewer.GutterWidth);
        Assert.Equal(240, diffViewer.MinWidth);
        Assert.Equal(140, diffViewer.MinHeight);
        Assert.Equal(new CornerRadius(6), diffViewer.CornerRadius);

        diffViewer.Density = FWDataInspectorDensity.Compact;

        Assert.Equal(12, diffViewer.FontSize);
        Assert.Equal(48, diffViewer.GutterWidth);
        Assert.Equal(220, diffViewer.MinWidth);
        Assert.Equal(120, diffViewer.MinHeight);
        Assert.Equal(new CornerRadius(4), diffViewer.CornerRadius);

        diffViewer.Density = FWDataInspectorDensity.Spacious;

        Assert.Equal(14, diffViewer.FontSize);
        Assert.Equal(72, diffViewer.GutterWidth);
        Assert.Equal(280, diffViewer.MinWidth);
        Assert.Equal(180, diffViewer.MinHeight);
        Assert.Equal(new CornerRadius(8), diffViewer.CornerRadius);

        var hexEditor = new FWHexEditor();

        Assert.Equal(FWDataInspectorDensity.Comfortable, hexEditor.Density);
        Assert.Equal(13, hexEditor.FontSize);
        Assert.Equal(new Thickness(6), hexEditor.Padding);
        Assert.Equal(240, hexEditor.MinWidth);
        Assert.Equal(140, hexEditor.MinHeight);
        Assert.Equal(new CornerRadius(6), hexEditor.CornerRadius);

        hexEditor.Density = FWDataInspectorDensity.Compact;

        Assert.Equal(12, hexEditor.FontSize);
        Assert.Equal(new Thickness(4), hexEditor.Padding);
        Assert.Equal(220, hexEditor.MinWidth);
        Assert.Equal(120, hexEditor.MinHeight);
        Assert.Equal(new CornerRadius(4), hexEditor.CornerRadius);

        hexEditor.Density = FWDataInspectorDensity.Spacious;

        Assert.Equal(14, hexEditor.FontSize);
        Assert.Equal(new Thickness(8), hexEditor.Padding);
        Assert.Equal(280, hexEditor.MinWidth);
        Assert.Equal(180, hexEditor.MinHeight);
        Assert.Equal(new CornerRadius(8), hexEditor.CornerRadius);

        var jsonViewer = new FWJsonTreeViewer();

        Assert.Equal(FWDataInspectorDensity.Comfortable, jsonViewer.Density);
        Assert.Equal(13, jsonViewer.FontSize);
        Assert.Equal(20, jsonViewer.IndentSize);
        Assert.Equal(240, jsonViewer.MinWidth);
        Assert.Equal(180, jsonViewer.MinHeight);
        Assert.Equal(new CornerRadius(6), jsonViewer.CornerRadius);

        jsonViewer.Density = FWDataInspectorDensity.Compact;

        Assert.Equal(12, jsonViewer.FontSize);
        Assert.Equal(16, jsonViewer.IndentSize);
        Assert.Equal(220, jsonViewer.MinWidth);
        Assert.Equal(160, jsonViewer.MinHeight);
        Assert.Equal(new CornerRadius(4), jsonViewer.CornerRadius);

        jsonViewer.Density = FWDataInspectorDensity.Spacious;

        Assert.Equal(14, jsonViewer.FontSize);
        Assert.Equal(24, jsonViewer.IndentSize);
        Assert.Equal(300, jsonViewer.MinWidth);
        Assert.Equal(220, jsonViewer.MinHeight);
        Assert.Equal(new CornerRadius(8), jsonViewer.CornerRadius);
    }

    [Fact]
    public void FWDiffViewer_ShouldExposeDiffModesAndNavigation()
    {
        var viewer = new FWDiffViewer
        {
            Density = FWDataInspectorDensity.Spacious,
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
        Assert.Equal(FWDataInspectorDensity.Spacious, viewer.Density);
        Assert.Equal(14, viewer.FontSize);
        Assert.True(viewer.GetChangeCount() > 0);
    }

    [Fact]
    public void FWHexEditor_ShouldExposeBinaryViewingEditingState()
    {
        var editor = new FWHexEditor
        {
            Density = FWDataInspectorDensity.Compact,
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
        Assert.Equal(FWDataInspectorDensity.Compact, editor.Density);
        Assert.Equal(12, editor.FontSize);
        Assert.Equal([0x48, 0x45, 0x4C, 0x6C, 0x6F], editor.Data);
    }

    [Fact]
    public void FWJsonTreeViewer_ShouldParseSearchAndExpandJson()
    {
        var viewer = new FWJsonTreeViewer
        {
            Density = FWDataInspectorDensity.Spacious,
            JsonText = """
                {
                  "theme": "Fluent",
                  "items": [1, 2, 3],
                  "enabled": true
                }
                """,
            SearchText = "theme",
            IsEditable = true,
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
        Assert.Equal(2, viewer.ExpandDepth);
        Assert.Equal(8, viewer.MaxRenderDepth);
        Assert.True(viewer.ShowTypeIndicators);
        Assert.True(viewer.ShowItemCount);
        Assert.Equal(FWDataInspectorDensity.Spacious, viewer.Density);
        Assert.Equal(24, viewer.IndentSize);
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

    [Fact]
    public void GalleryDataInspectorsPage_ShouldFormatWorkbenchQaSnapshot()
    {
        var diffViewer = new FWDiffViewer
        {
            OriginalText = "theme: dark\naccent: blue",
            ModifiedText = "theme: dark\naccent: teal\nstatus: ready",
            ViewMode = DiffViewMode.Unified,
            ShowMinimap = true,
            IsReadOnly = true
        };
        var hexEditor = new FWHexEditor
        {
            Data = [0x46, 0x57, 0x20, 0x51, 0x41],
            BytesPerRow = 8,
            ShowDataInterpretation = true,
            SelectionLength = 2,
            IsReadOnly = true
        };
        var jsonViewer = new FWJsonTreeViewer
        {
            JsonText = "{ \"theme\": \"Fluent\", \"ready\": true }",
            ExpandDepth = 2,
            MaxRenderDepth = 8,
            ShowTypeIndicators = true,
            ShowItemCount = true,
            IsEditable = false
        };
        var surface = new FWFluentMaterialSurface
        {
            MaterialKind = FWFluentMaterialKind.LiquidGlass,
            Child = diffViewer
        };

        var snapshot = GalleryDataInspectorsPage.CreateDataInspectorWorkbenchSnapshot(
            diffViewer,
            hexEditor,
            jsonViewer,
            surface);
        var text = GalleryDataInspectorsPage.FormatDataInspectorWorkbenchQa("Data Inspectors workbench QA", snapshot);

        Assert.True(snapshot.IsReady);
        Assert.True(snapshot.HasDiffContent);
        Assert.Equal(DiffViewMode.Unified, snapshot.DiffViewMode);
        Assert.True(snapshot.DiffChangeCount > 0);
        Assert.True(snapshot.IsDiffMinimapVisible);
        Assert.True(snapshot.IsDiffReadOnly);
        Assert.True(snapshot.HasHexContent);
        Assert.Equal(5, snapshot.HexByteCount);
        Assert.Equal(8, snapshot.HexBytesPerRow);
        Assert.True(snapshot.IsHexReadOnly);
        Assert.True(snapshot.IsHexDataInterpretationVisible);
        Assert.Equal(2, snapshot.HexSelectionLength);
        Assert.True(snapshot.HasJsonContent);
        Assert.Equal(2, snapshot.JsonExpandDepth);
        Assert.Equal(8, snapshot.JsonMaxRenderDepth);
        Assert.False(snapshot.IsJsonEditable);
        Assert.True(snapshot.IsJsonTypeIndicatorsVisible);
        Assert.True(snapshot.IsJsonItemCountVisible);
        Assert.Equal(FWFluentMaterialKind.LiquidGlass, snapshot.MaterialKind);
        Assert.Contains("Data Inspectors workbench QA", text);
        Assert.Contains("diff on Unified", text);
        Assert.Contains("minimap on", text);
        Assert.Contains("hex on bytes 5", text);
        Assert.Contains("interpretation on", text);
        Assert.Contains("json on", text);
        Assert.Contains("expand depth 2/8", text);
        Assert.Contains("material LiquidGlass", text);
        Assert.Contains("ready on", text);
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
