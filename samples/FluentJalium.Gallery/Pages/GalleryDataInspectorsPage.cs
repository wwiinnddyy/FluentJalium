using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWButton = FluentJalium.Controls.FWButton;
using FWDiffViewer = FluentJalium.Controls.FWDiffViewer;
using FWFluentMaterialKind = FluentJalium.Controls.FWFluentMaterialKind;
using FWFluentMaterialSurface = FluentJalium.Controls.FWFluentMaterialSurface;
using FWHexEditor = FluentJalium.Controls.FWHexEditor;
using FWJsonTreeViewer = FluentJalium.Controls.FWJsonTreeViewer;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;

namespace FluentJalium.Gallery.Pages;

internal sealed class GalleryDataInspectorsPage
{
    internal readonly record struct GalleryDataInspectorWorkbenchSnapshot(
        bool HasDiffContent,
        DiffViewMode DiffViewMode,
        int DiffChangeCount,
        bool IsDiffMinimapVisible,
        bool IsDiffReadOnly,
        bool HasHexContent,
        int HexByteCount,
        int HexBytesPerRow,
        bool IsHexReadOnly,
        bool IsHexDataInterpretationVisible,
        long HexSelectionLength,
        bool HasJsonContent,
        int JsonExpandDepth,
        int JsonMaxRenderDepth,
        bool IsJsonEditable,
        bool IsJsonTypeIndicatorsVisible,
        bool IsJsonItemCountVisible,
        FWFluentMaterialKind MaterialKind)
    {
        public bool IsReady => HasDiffContent && HasHexContent && HasJsonContent;
    }

    public UIElement CreateContent()
    {
        var panel = CreateSection("Data Inspectors");
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        examples.Children.Add(CreateDataInspectorCard(
            FluentIconRegular.DocumentSearch24,
            "FWDiffViewer",
            "Side-by-side and unified diffs with line numbers, minimap state, and change navigation.",
            CreateDiffViewerSample()));
        examples.Children.Add(CreateDataInspectorCard(
            FluentIconRegular.DataHistogram24,
            "FWHexEditor",
            "Binary data surface with offset, hex, ASCII, grouping, find, and replacement state.",
            CreateHexEditorSample()));
        examples.Children.Add(CreateDataInspectorCard(
            FluentIconRegular.Braces24,
            "FWJsonTreeViewer",
            "Searchable JSON tree with type colors, path status, expand depth, and item count display.",
            CreateJsonTreeViewerSample()));
        examples.Children.Add(CreateDataInspectorCard(
            FluentIconRegular.LayerDiagonalSparkle24,
            "Material inspection workbench",
            "Diff, binary, and JSON inspector states remain readable on FluentJalium material layers.",
            CreateMaterialInspectionWorkbench()));

        panel.Children.Add(examples);
        return panel;
    }

    private static UIElement CreateDiffViewerSample()
    {
        var output = CreateOutput("Mode: SideBySide. Changes: pending");
        var viewer = CreateSampleDiffViewer(width: 520, height: 300);

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
                CreateButtonRow(
                    CreateIconActionButton(FluentIconRegular.TextSortAscending24, "Unified", () =>
                    {
                        viewer.ViewMode = viewer.ViewMode == DiffViewMode.SideBySide ? DiffViewMode.Unified : DiffViewMode.SideBySide;
                        UpdateOutput();
                    }),
                    CreateIconActionButton(FluentIconRegular.NumberRow24, "Line nums", () =>
                    {
                        viewer.ShowLineNumbers = !viewer.ShowLineNumbers;
                        UpdateOutput();
                    }),
                    CreateIconActionButton(FluentIconRegular.DataUsage24, "Minimap", () =>
                    {
                        viewer.ShowMinimap = !viewer.ShowMinimap;
                        UpdateOutput();
                    }),
                    CreateIconActionButton(FluentIconRegular.ArrowDown24, "Next", () =>
                    {
                        viewer.NavigateToNextChange();
                        UpdateOutput();
                    })),
                CreateStatus(output)
            }
        };
    }

    private static UIElement CreateHexEditorSample()
    {
        var output = CreateOutput("Bytes per row: 16. ASCII: on. Interpretation: off");
        var editor = CreateSampleHexEditor(width: 520, height: 300);

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
                CreateButtonRow(
                    CreateIconActionButton(FluentIconRegular.TableResizeColumn24, "Rows", () =>
                    {
                        editor.BytesPerRow = editor.BytesPerRow == 16 ? 8 : 16;
                        UpdateOutput();
                    }),
                    CreateIconActionButton(FluentIconRegular.TextChangeCase24, "ASCII", () =>
                    {
                        editor.ShowAsciiColumn = !editor.ShowAsciiColumn;
                        UpdateOutput();
                    }),
                    CreateIconActionButton(FluentIconRegular.DataHistogram24, "Interpret", () =>
                    {
                        editor.ShowDataInterpretation = !editor.ShowDataInterpretation;
                        UpdateOutput();
                    }),
                    CreateIconActionButton(FluentIconRegular.Search24, "Find", () =>
                    {
                        var offset = editor.FindBytes([0x46, 0x57]);
                        output.Text = $"Found FW at offset {offset}";
                    })),
                CreateStatus(output)
            }
        };
    }

    private static UIElement CreateJsonTreeViewerSample()
    {
        var output = CreateOutput("Search: empty. Expand depth: 2");
        var viewer = CreateSampleJsonTreeViewer(width: 520, height: 300);

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
                CreateButtonRow(
                    CreateIconActionButton(FluentIconRegular.Search24, "Search theme", () =>
                    {
                        viewer.SearchText = viewer.SearchText.Length == 0 ? "theme" : string.Empty;
                        UpdateOutput();
                    }),
                    CreateIconActionButton(FluentIconRegular.ArrowExpand24, "Expand", () =>
                    {
                        viewer.ExpandAll();
                        UpdateOutput();
                    }),
                    CreateIconActionButton(FluentIconRegular.ArrowCollapseAll24, "Collapse", () =>
                    {
                        viewer.CollapseAll();
                        UpdateOutput();
                    }),
                    CreateIconActionButton(FluentIconRegular.Braces24, "Types", () =>
                    {
                        viewer.ShowTypeIndicators = !viewer.ShowTypeIndicators;
                        UpdateOutput();
                    })),
                CreateStatus(output)
            }
        };
    }

    private static UIElement CreateMaterialInspectionWorkbench()
    {
        var diffViewer = CreateSampleDiffViewer(width: 420, height: 190);
        diffViewer.ViewMode = DiffViewMode.Unified;
        diffViewer.GutterWidth = 48;
        diffViewer.ContextLines = 1;
        diffViewer.IsReadOnly = true;

        var hexEditor = CreateSampleHexEditor(width: 420, height: 160);
        hexEditor.BytesPerRow = 8;
        hexEditor.ShowDataInterpretation = true;
        hexEditor.IsReadOnly = true;

        var jsonViewer = CreateSampleJsonTreeViewer(width: 420, height: 160);
        jsonViewer.SearchText = "theme";
        jsonViewer.MaxRenderDepth = 8;
        jsonViewer.IsEditable = false;

        var output = CreateOutput("Workbench QA pending.");
        var workbenchPanel = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Children =
            {
                CreateMaterialHeader(),
                diffViewer,
                hexEditor,
                jsonViewer
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
            Child = workbenchPanel
        };

        void UpdateWorkbenchOutput(string action)
        {
            var snapshot = CreateDataInspectorWorkbenchSnapshot(diffViewer, hexEditor, jsonViewer, surface);
            output.Text = FormatDataInspectorWorkbenchQa(action, snapshot);
        }

        workbenchPanel.Children.Add(CreateButtonRow(
            CreateIconActionButton(FluentIconRegular.DocumentSearch24, "Diff focus", () =>
            {
                diffViewer.ShowMinimap = !diffViewer.ShowMinimap;
                UpdateWorkbenchOutput("Diff focus");
            }),
            CreateIconActionButton(FluentIconRegular.DataHistogram24, "Hex group", () =>
            {
                hexEditor.BytesPerRow = hexEditor.BytesPerRow == 8 ? 16 : 8;
                UpdateWorkbenchOutput("Hex group");
            }),
            CreateIconActionButton(FluentIconRegular.Braces24, "JSON depth", () =>
            {
                jsonViewer.ExpandDepth = jsonViewer.ExpandDepth == 2 ? 4 : 2;
                UpdateWorkbenchOutput("JSON depth");
            }),
            CreateIconActionButton(FluentIconRegular.Search24, "Find FW", () =>
            {
                var offset = hexEditor.FindBytes([0x46, 0x57]);
                UpdateWorkbenchOutput($"Find FW {offset}");
            })));
        workbenchPanel.Children.Add(CreateStatus(output));
        UpdateWorkbenchOutput("Data Inspectors workbench QA");
        return surface;
    }

    internal static GalleryDataInspectorWorkbenchSnapshot CreateDataInspectorWorkbenchSnapshot(
        FWDiffViewer diffViewer,
        FWHexEditor hexEditor,
        FWJsonTreeViewer jsonViewer,
        FWFluentMaterialSurface? materialSurface = null)
    {
        return new GalleryDataInspectorWorkbenchSnapshot(
            HasDiffContent: !string.IsNullOrWhiteSpace(diffViewer.OriginalText) || !string.IsNullOrWhiteSpace(diffViewer.ModifiedText),
            DiffViewMode: diffViewer.ViewMode,
            DiffChangeCount: diffViewer.GetChangeCount(),
            IsDiffMinimapVisible: diffViewer.ShowMinimap,
            IsDiffReadOnly: diffViewer.IsReadOnly,
            HasHexContent: hexEditor.Data?.Length > 0,
            HexByteCount: hexEditor.Data?.Length ?? 0,
            HexBytesPerRow: hexEditor.BytesPerRow,
            IsHexReadOnly: hexEditor.IsReadOnly,
            IsHexDataInterpretationVisible: hexEditor.ShowDataInterpretation,
            HexSelectionLength: hexEditor.SelectionLength,
            HasJsonContent: !string.IsNullOrWhiteSpace(jsonViewer.JsonText) && jsonViewer.RootNode != null,
            JsonExpandDepth: jsonViewer.ExpandDepth,
            JsonMaxRenderDepth: jsonViewer.MaxRenderDepth,
            IsJsonEditable: jsonViewer.IsEditable,
            IsJsonTypeIndicatorsVisible: jsonViewer.ShowTypeIndicators,
            IsJsonItemCountVisible: jsonViewer.ShowItemCount,
            MaterialKind: materialSurface?.MaterialKind ?? FWFluentMaterialKind.None);
    }

    internal static string FormatDataInspectorWorkbenchQa(string action, GalleryDataInspectorWorkbenchSnapshot snapshot)
    {
        return $"{action}: diff {FormatOnOff(snapshot.HasDiffContent)} {snapshot.DiffViewMode} changes {snapshot.DiffChangeCount}, minimap {FormatOnOff(snapshot.IsDiffMinimapVisible)}, read-only {FormatOnOff(snapshot.IsDiffReadOnly)}; hex {FormatOnOff(snapshot.HasHexContent)} bytes {snapshot.HexByteCount}, row {snapshot.HexBytesPerRow}, read-only {FormatOnOff(snapshot.IsHexReadOnly)}, interpretation {FormatOnOff(snapshot.IsHexDataInterpretationVisible)}, selection {snapshot.HexSelectionLength}; json {FormatOnOff(snapshot.HasJsonContent)}, expand depth {snapshot.JsonExpandDepth}/{snapshot.JsonMaxRenderDepth}, editable {FormatOnOff(snapshot.IsJsonEditable)}, types {FormatOnOff(snapshot.IsJsonTypeIndicatorsVisible)}, item count {FormatOnOff(snapshot.IsJsonItemCountVisible)}; material {snapshot.MaterialKind}; ready {FormatOnOff(snapshot.IsReady)}.";
    }

    private static FWDiffViewer CreateSampleDiffViewer(double width, double height)
    {
        return new FWDiffViewer
        {
            Width = width,
            Height = height,
            OriginalText = "theme: dark\naccent: blue\ncontrols: 32\nstatus: preview",
            ModifiedText = "theme: dark\naccent: teal\ncontrols: 36\nstatus: ready\nicons: fluent",
            ViewMode = DiffViewMode.SideBySide,
            ShowLineNumbers = true,
            ShowMinimap = true,
            GutterWidth = 56
        };
    }

    private static FWHexEditor CreateSampleHexEditor(double width, double height)
    {
        return new FWHexEditor
        {
            Width = width,
            Height = height,
            Data = CreateHexSampleData(),
            BytesPerRow = 16,
            ColumnGroupSize = 8,
            ShowAsciiColumn = true,
            ShowOffsetColumn = true,
            ShowDataInterpretation = false,
            SelectionStart = 0,
            SelectionLength = 4
        };
    }

    private static FWJsonTreeViewer CreateSampleJsonTreeViewer(double width, double height)
    {
        return new FWJsonTreeViewer
        {
            Width = width,
            Height = height,
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
    }

    private static FWStackPanel CreateMaterialHeader()
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                CreateIcon(FluentIconRegular.Bug24, 18, ThemeBrush("TextPrimary")),
                new FWTextBlock
                {
                    Text = "Layered inspection workbench",
                    FontSize = 15,
                    Foreground = ThemeBrush("TextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
    }

    private static FWBorder CreateDataInspectorCard(FluentIconRegular icon, string title, string description, UIElement content)
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

    private static string FormatSearchText(string? searchText) => string.IsNullOrWhiteSpace(searchText) ? "empty" : searchText;

    private static string FormatOnOff(bool value) => value ? "on" : "off";

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
                        CreateIcon(FluentIconRegular.Code24, 24, ThemeBrush("TextPrimary")),
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
