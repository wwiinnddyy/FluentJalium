using System.Reflection;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;

namespace DataGridProbe;

/// <summary>
/// Pass 2: the token half of the question. Pass 1 (datagrid-probe-all.txt) settled the structural half - a mounted
/// DataGrid reports Template=SET with Style=null and a full named part tree, and all three markup routes land OUR
/// template on it, so this family is re-templateable the way ListBox is and NOT the way TabControl is (S1-h).
/// Pass 1 also falsified two recorded claims while doing it: the framework ships per-control theme dictionaries
/// (Jalium.UI.Controls/Themes/Controls/DataGrid.jalxaml, compiled into the __JalxamlGenerated._Dict_* types the
/// census saw as "zero embedded .jalxaml"), and those dictionaries carry implicit TargetType styles whose setters
/// really apply - the "0 of 163 controls have a framework default style" reading was taken off an unmounted
/// control's public Style property, which stays null even when the theme style has landed.
/// So the DataGrid appearance is driven by eleven names that file reads:
///   SurfaceBackground, TextPrimary, TextSecondary, TextDisabled, ControlBorder, ControlBorderFocused,
///   AccentBrush, TextOnAccent, LayerFillColorAltBrush, SubtleFillColorSecondaryBrush, CaptionFontSize
/// and only the last two are rows Astra publishes. Before choosing between "retint the framework's template" and
/// "replace it", each name has to be measured for what Astra can actually reach:
///   G identity  : does the brush instance a framework part carries BE the instance the app-level lookup returns
///                 for that name? Identity is the only reading that says "this token is a hook"; the window-shell
///                 batch (S0-s) established that an application-level alias row can override the framework's own
///                 default, and the AutoCompleteBox batch established that a generated text element can carry a
///                 LOCAL brush that no name reaches.
///   H ordering  : what does the framework's grid look like BEFORE Astra's dictionaries are applied, and does
///                 Apply change it - measured on one control instance in one process, because ThemeMode is the
///                 only driver and it re-resolves what is already applied.
///   I override  : does an implicit app-level style for typeof(DataGrid) replace the framework's style wholesale,
///                 i.e. are RowHeight / ColumnHeaderHeight / gridline brushes lost with it? That is the cost line
///                 for shipping our own template, and it is the same precedence the TabView batch had to pay.
///   J states    : the framework template already carries the state matrix (IsKeyboardFocused -> ControlBorderFocused,
///                 row IsSelected -> AccentBrush + TextOnAccent, row IsMouseOver -> SubtleFillColorSecondary,
///                 disabled -> TextDisabled + 0.56 opacity). Which of those survive on a mounted grid, read off the
///                 part that is supposed to change, with selection driven by the control's own API and no pointer.
/// Modes appended to pass 1: pass2 (G-J). The pass-1 modes are unchanged.
/// </summary>
internal static partial class Program
{
    private static readonly string[] FrameworkTokens =
    [
        "SurfaceBackground", "TextPrimary", "TextSecondary", "TextDisabled", "ControlBorder",
        "ControlBorderFocused", "AccentBrush", "TextOnAccent", "LayerFillColorAltBrush",
        "SubtleFillColorSecondaryBrush", "CaptionFontSize",
    ];

    private static void TokenRoute(Panel root, Window window)
    {
        Note("");
        Note("=== G/H. the framework's grid before Astra's dictionaries are applied ===");
        // A grid mounted from a mode that skipped FluentThemeManager.Apply: everything below is the framework's own
        // look, which is the baseline the pass-1 numbers (r=12, row 30, header 34) came from.
        var bare = NewGrid();
        if (bare is not null)
        {
            var element = (FrameworkElement)bare;
            root.Children.Add(element);
            Pump(10);
            Note($"  rendered={Rendered(element)} size={SizeOf(element)}");
            ReportGeometry("framework-only", element);
            ReportTokens(element);
            Note($"  selected row via SelectedItem -> {Select(element)}");
            Pump(4);
            ReportSelection(element);
            Note($"  IsEnabled=false -> {Set(element, "IsEnabled", false)}");
            Pump(4);
            ReportDisabled(element);
            _ = Set(element, "IsEnabled", true);
            root.Children.Remove(element);
            Pump(2);
        }

        Note("");
        Note("  --- now apply Astra's dictionaries and re-read the SAME mechanism ---");
        Note($"  FluentThemeManager.Apply -> {Try(() => FluentThemeManager.Apply(_application))}");
        Pump(6);
        var after = NewGrid();
        if (after is not null)
        {
            var element = (FrameworkElement)after;
            root.Children.Add(element);
            Pump(10);
            ReportGeometry("with Astra applied", element);
            ReportTokens(element);
            root.Children.Remove(element);
            Pump(2);
        }
    }

    /// <summary>
    /// Does our own implicit style for the host displace the framework's, and what does that cost?
    /// </summary>
    private static void OverrideCost(Panel root, Window window)
    {
        Note("");
        Note("=== I. what an app-level implicit style for typeof(DataGrid) costs ===");
        var gridType = TypeByName("DataGrid")!;
        var before = _application.TryFindResource(gridType);
        Note($"  app-level resource keyed by typeof(DataGrid) BEFORE ours: {(before is null ? "null" : before.GetType().Name)} target={Show(Prop(before, "TargetType"))}");
        var styleObject = before;
        if (styleObject is not null)
        {
            var setters = Prop(styleObject, "Setters") as System.Collections.IEnumerable;
            var list = setters?.Cast<object>().ToList();
            Note($"  framework style setter count={list?.Count ?? -1}");
            foreach (var setter in list ?? [])
            {
                var dp = Prop(setter, "Property");
                Note($"    {Show(Prop(dp, "Name"))} = {Trim(Show(Prop(setter, "Value")))}");
            }
        }

        // Now install ours over it and read what the control keeps.
        var markup = @"
<ResourceDictionary xmlns='https://schemas.jalium.dev/jalxaml/presentation'
                    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <Style x:Key='ProbeDataGridStyle' TargetType='DataGrid'>
    <Setter Property='Template'>
      <ControlTemplate>
        <Border x:Name='ProbeTemplateRoot' Background='#FFFF00FF'>
          <ItemsPresenter x:Name='ProbeItems'/>
        </Border>
      </ControlTemplate>
    </Setter>
  </Style>
</ResourceDictionary>";
        var dictionary = ParseDictionary(markup);
        var probeStyle = dictionary?["ProbeDataGridStyle"] as Style;
        Note($"  probe style parsed: {(probeStyle is null ? "FAILED" : "ok")}");
        if (probeStyle is null)
        {
            return;
        }

        _application.Resources[gridType] = probeStyle;
        var grid = NewGrid();
        if (grid is not null)
        {
            var element = (FrameworkElement)grid;
            root.Children.Add(element);
            Pump(10);
            Note($"  after our style: Style={(Prop(element, "Style") is null ? "null" : "SET")} template is ours={FirstNamed(element, "ProbeTemplateRoot") is not null}");
            foreach (var name in new[] { "RowHeight", "ColumnHeaderHeight", "BorderThickness", "GridLinesVisibility", "RowBackgroundResource" })
            {
                Note($"    {name} = {Show(Prop(element, name))}");
            }

            var row = AllOfType(element, TypeByName("DataGridRow")!).FirstOrDefault();
            Note($"    row: {(row is null ? "none built" : $"{SizeOf((FrameworkElement)row)} cells={CountTypeName((DependencyObject)row, "DataGridCell")}")}");
            Note($"    outer border present={FirstNamed(element, "PART_OuterBorder") is not null} rows host={FirstNamed(element, "PART_RowsHost")?.GetType().Name ?? "-"}");
            Note("    tree:");
            PrintTree(element, "      ", 12);
            root.Children.Remove(element);
            _application.Resources.Remove(gridType);
            Pump(2);
        }
    }

    private static void ReportGeometry(string label, FrameworkElement element)
    {
        var outer = FirstNamed(element, "PART_OuterBorder") as Border;
        var headers = FirstNamed(element, "PART_ColumnHeadersBorder") as FrameworkElement;
        var rowsHost = FirstNamed(element, "PART_RowsHost") as FrameworkElement;
        var row = AllOfType(element, TypeByName("DataGridRow")!).Cast<FrameworkElement>().FirstOrDefault();
        var cell = AllOfType(element, TypeByName("DataGridCell")!).Cast<FrameworkElement>().FirstOrDefault();
        var header = AllOfType(element, TypeByName("DataGridColumnHeader")!).Cast<FrameworkElement>().FirstOrDefault();
        Note($"  [{label}] outer bg={Hex(outer?.Background)} border={Hex(outer?.BorderBrush)} bt={outer?.BorderThickness} r={outer?.CornerRadius} clip={Show(Prop(outer, "ClipToBounds"))}");
        Note($"  [{label}] host={SizeOf(element)} headers={SizeOf(headers!)} rowsHost={SizeOf(rowsHost!)} row={SizeOf(row!)} cell={SizeOf(cell!)} header={SizeOf(header!)}");
        Note($"  [{label}] cell padding={Show(Prop(cell, "Padding"))} header padding={Show(Prop(header, "Padding"))} gridRowHeight={Show(Prop(element, "RowHeight"))} columnHeaderHeight={Show(Prop(element, "ColumnHeaderHeight"))}");
        var rowBorder = row is null ? null : FirstNamed((DependencyObject)row, "PART_RowBorder") as Border;
        Note($"  [{label}] row border bg={Hex(rowBorder?.Background)} bt={rowBorder?.BorderThickness} bc={Hex(rowBorder?.BorderBrush)} rowFg={Hex((Brush?)Prop(row, "Foreground"))}");
        var headerBorder = header is null ? null : FirstNamed((DependencyObject)header, "PART_HeaderBorder") as Border;
        Note($"  [{label}] header border bg={Hex(headerBorder?.Background)} bc={Hex(headerBorder?.BorderBrush)} bt={headerBorder?.BorderThickness} sortIndicator={Show(Prop(FirstNamed((DependencyObject)header, "PART_SortIndicator"), "Text"))} size={SizeOf((FrameworkElement)(FirstNamed((DependencyObject)header, "PART_ResizeGrip") ?? new Border())!)}");
    }

    private static void ReportTokens(FrameworkElement element)
    {
        Note("  token identity (name -> app lookup instance vs the instance the part carries):");
        var outer = FirstNamed(element, "PART_OuterBorder") as Border;
        var header = AllOfType(element, TypeByName("DataGridColumnHeader")!).Cast<FrameworkElement>().FirstOrDefault();
        var headerBorder = header is null ? null : FirstNamed((DependencyObject)header, "PART_HeaderBorder") as Border;
        var rows = AllOfType(element, TypeByName("DataGridRow")!).Cast<FrameworkElement>().ToList();
        var rowBorders = rows
            .Select(row => FirstNamed((DependencyObject)row, "PART_RowBorder") as Border)
            .Where(border => border is not null)
            .ToList();
        var pairs = new (string Name, Brush? Carried)[]
        {
            ("SurfaceBackground", outer?.Background),
            ("TextPrimary", (Brush?)Prop(element, "Foreground")),
            ("ControlBorder", outer?.BorderBrush),
            ("LayerFillColorAltBrush", headerBorder?.Background),
            ("SubtleFillColorSecondaryBrush", rowBorders.Count > 1 ? rowBorders[1]!.Background : null),
            ("AccentBrush", rowBorders.Count > 0 ? rowBorders[0]!.Background : null),
        };
        foreach (var (name, carried) in pairs)
        {
            var resolved = _application.TryFindResource(name) as Brush;
            Note($"    {name}: lookup={Hex(resolved)} carried={Hex(carried)} sameInstance={ReferenceEquals(resolved, carried)} inAstra={(FluentBrush(name) is not null ? "yes" : "no")}");
        }

        foreach (var name in FrameworkTokens)
        {
            var value = _application.TryFindResource(name);
            Note($"    plain lookup {name}: {(value is null ? "MISSING" : $"{value.GetType().Name} {Trim(Show(Prop(value, "Color"))) }{Trim(Show(value))}")}");
        }
    }

    private static Brush? FluentBrush(string name)
    {
        try
        {
            return FluentThemeManager.GetBrush(name);
        }
        catch
        {
            return null;
        }
    }

    private static string Select(FrameworkElement element)
    {
        var item = Prop(element, "ItemsSource") as System.Collections.IEnumerable;
        var first = item?.Cast<object>().FirstOrDefault();
        if (first is null)
        {
            return "no items";
        }

        return Text(() =>
        {
            element.GetType().GetProperty("SelectedItem", BindingFlags.Public | BindingFlags.Instance)?
                .SetValue(element, first);
            return (object)$"SelectedItem set, SelectedIndex={Show(Prop(element, "SelectedIndex"))}";
        });
    }

    private static void ReportSelection(FrameworkElement element)
    {
        var rows = AllOfType(element, TypeByName("DataGridRow")!).Cast<FrameworkElement>().ToList();
        for (var index = 0; index < rows.Count; index++)
        {
            var row = rows[index];
            var border = FirstNamed((DependencyObject)row, "PART_RowBorder") as Border;
            Note($"    selected[{index}] row.IsSelected={Show(Prop(row, "IsSelected"))} PART_RowBorder.bg={Hex(border?.Background)} row.Foreground={Hex((Brush?)Prop(row, "Foreground"))} cell.Foreground={Hex((Brush?)Prop(AllOfType((DependencyObject)row, TypeByName("DataGridCell")!).FirstOrDefault()!, "Foreground"))}");
        }
    }

    private static void ReportDisabled(FrameworkElement element)
    {
        var outer = FirstNamed(element, "PART_OuterBorder") as Border;
        Note($"    disabled: grid.Foreground={Hex((Brush?)Prop(element, "Foreground"))} opacity={Show(Prop(element, "Opacity"))} outer.bg={Hex(outer?.Background)}");
        Note($"    TextDisabled lookup={Hex(_application.TryFindResource("TextDisabled") as Brush)} same={ReferenceEquals(_application.TryFindResource("TextDisabled"), Prop(element, "Foreground"))}");
    }

    private static string Try(Action action)
    {
        try
        {
            action();
            return "ok";
        }
        catch (Exception exception)
        {
            return exception.GetType().Name + ": " + Trim(exception.Message);
        }
    }
}
