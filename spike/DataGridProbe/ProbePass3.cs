using System.Collections;
using System.Reflection;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;

namespace DataGridProbe;

/// <summary>
/// Pass 3: the two readings S1-i left open, both of which decide how much markup Styles/DataGrid.jalxaml has to
/// carry. (1) Where does the framework's grid style actually live - the application dictionary, or a theme scope
/// the merged lookup reaches? TryFindResource cannot tell those apart, so this reads the dictionary itself.
/// (2) When our own implicit style replaces it and only declares a Template, do the framework's other ten cells
/// survive or does the control fall back to dependency-property defaults? Reading the surviving values alone
/// cannot tell those apart either (RowHeight might default to 30), so the DP defaults are read first and the
/// question becomes a set difference: a surviving value that is NOT the default came from somewhere other
/// than ours.
/// Modes: pass3.
/// </summary>
internal static partial class Program
{
    private static void StyleOwnership(Panel root, Window window, string stage)
    {
        Note("");
        Note($"=== K [{stage}]. where the grid style lives, and what a Template-only style has to re-declare ===");
        var gridType = TypeByName("DataGrid")!;

        Note("  DP defaults read off the metadata (the fallback a cell-less style would show):");
        foreach (var name in new[] { "RowHeight", "ColumnHeaderHeight", "Background", "Foreground", "BorderThickness", "GridLinesVisibility", "RowBackground", "AlternatingRowBackground" })
        {
            var dp = FindDp(gridType, name);
            Note($"    {name}: dp={(dp is null ? "NOT PUBLIC" : "ok")} default={Show(Prop(dp, "DefaultValue"))}");
        }

        var resources = _application.Resources;
        Note($"  Application.Resources: {(resources is null ? "null" : resources.GetType().Name)}");
        var contains = resources?.GetType().GetMethod("ContainsKey", [typeof(object)]);
        Note($"  ContainsKey(typeof(DataGrid)) -> {(contains is null ? "no such method" : Show(Read(() => contains.Invoke(resources, [gridType]))))}");
        var keys = Prop(resources, "Keys") as IEnumerable;
        var keyList = keys?.Cast<object>().ToList() ?? [];
        Note($"  Keys count={keyList.Count}; type-keyed entries={keyList.Count(k => k is Type)}");
        foreach (var key in keyList.Where(k => k is Type).Cast<Type>().OrderBy(t => t.Name, StringComparer.Ordinal))
        {
            Note($"    type key: {key.Name}");
        }

        var merged = _application.TryFindResource(gridType);
        var direct = Read(() => resources?.GetType().GetProperty("Item", [typeof(object)])?.GetValue(resources, [gridType]));
        Note($"  merged lookup typeof(DataGrid): {(merged is null ? "null" : merged.GetType().Name)}");
        Note($"  Application.Resources[typeof(DataGrid)] directly: {(direct is null ? "null" : direct.GetType().Name)} " +
             $"sameObjectAsMerged={ReferenceEquals(merged, direct)}");

        // A grid with our template-only implicit style installed: which non-template cells still hold the
        // framework's values?
        var style = new Style(gridType);
        style.Setters.Add(new Setter(Control.TemplateProperty, ProbeTemplate()));
        _application.Resources[gridType] = style;
        var grid = NewGrid();
        if (grid is not null)
        {
            var element = (FrameworkElement)grid;
            root.Children.Add(element);
            Pump(10);
            Note("  under our Template-only style:");
            foreach (var name in new[] { "RowHeight", "ColumnHeaderHeight", "BorderThickness", "GridLinesVisibility", "RowBackground", "AlternatingRowBackground" })
            {
                Note($"    {name} = {Show(Prop(element, name))}");
            }

            var background = Prop(element, "Background") as Brush;
            var token = _application.TryFindResource("SurfaceBackground") as Brush;
            Note($"    Background = {Hex(background)}; SurfaceBackground token = {Hex(token)} sameInstance={ReferenceEquals(background, token)}");
            var foreground = Prop(element, "Foreground") as Brush;
            var textPrimary = _application.TryFindResource("TextPrimary") as Brush;
            Note($"    Foreground = {Hex(foreground)}; TextPrimary token = {Hex(textPrimary)} sameInstance={ReferenceEquals(foreground, textPrimary)}");
            root.Children.Remove(element);
            Pump(2);
        }

        _application.Resources.Remove(gridType);
        Pump(2);

        // And with nothing of ours installed, for the same six readings, so the pair is a difference and not a
        // memory of what the framework style did.
        var plain = NewGrid();
        if (plain is not null)
        {
            var element = (FrameworkElement)plain;
            root.Children.Add(element);
            Pump(10);
            Note("  under the framework style (ours removed):");
            foreach (var name in new[] { "RowHeight", "ColumnHeaderHeight", "BorderThickness", "GridLinesVisibility", "RowBackground", "AlternatingRowBackground" })
            {
                Note($"    {name} = {Show(Prop(element, name))}");
            }

            var background = Prop(element, "Background") as Brush;
            Note($"    Background = {Hex(background)} sameInstanceWithToken={ReferenceEquals(background, _application.TryFindResource("SurfaceBackground"))}");
            root.Children.Remove(element);
            Pump(2);
        }
    }

    private static ControlTemplate? ProbeTemplate()
    {
        var markup = @"
<ResourceDictionary xmlns='https://schemas.jalium.dev/jalxaml/presentation'
                    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <ControlTemplate x:Key='ProbeGridTemplate'>
    <Border x:Name='ProbeTemplateRoot' Background='#FFFF00FF'>
      <ItemsPresenter x:Name='ProbeItems'/>
    </Border>
  </ControlTemplate>
</ResourceDictionary>";
        return ParseDictionary(markup)?["ProbeGridTemplate"] as ControlTemplate;
    }
}
