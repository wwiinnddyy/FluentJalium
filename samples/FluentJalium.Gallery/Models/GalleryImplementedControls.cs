using System.Reflection;
using FluentJalium.Controls;

namespace FluentJalium.Gallery.Models;

/// <summary>
/// The controls FluentJalium actually implements, read from the assembly rather than from the
/// catalog's keyword lists.
/// </summary>
/// <remarks>
/// The catalog used to count "controls" by flattening every <c>FW</c>-prefixed token out of each
/// page's related-control and keyword lists, which reported 182 for 155 real types and counted
/// enum members and automation peers as controls.
/// </remarks>
internal static class GalleryImplementedControls
{
    private static readonly Lazy<GalleryImplementedControl[]> s_all = new(Load);

    public static GalleryImplementedControl[] All => s_all.Value;

    public static int Count => All.Length;

    public static bool IsImplemented(string name) =>
        Array.FindIndex(All, control => string.Equals(control.Name, name, StringComparison.Ordinal)) >= 0;

    private static GalleryImplementedControl[] Load()
    {
        var marker = typeof(IFluentJaliumControl);

        return marker.Assembly.GetExportedTypes()
            .Where(type => type.IsClass && !type.IsAbstract && marker.IsAssignableFrom(type))
            .Select(type => new GalleryImplementedControl(
                type.Name,
                type.Namespace ?? string.Empty,
                BaseChain(type)))
            .OrderBy(control => control.Name, StringComparer.Ordinal)
            .ToArray();
    }

    private static string[] BaseChain(Type type)
    {
        var chain = new List<string>();

        for (var current = type.BaseType; current is not null && current != typeof(object); current = current.BaseType)
        {
            chain.Add(current.Name);
        }

        return [.. chain];
    }
}

internal sealed record GalleryImplementedControl(
    string Name,
    string Namespace,
    string[] BaseClasses);
