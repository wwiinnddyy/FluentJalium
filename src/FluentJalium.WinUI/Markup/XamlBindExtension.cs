using System.Threading;
using Jalium.UI.Markup;

namespace FluentJalium.WinUI.Markup;

/// <summary>
/// SPIKE — not an implementation. Establishes one fact: does a WinUI <c>{x:Bind}</c> expression,
/// which the source generator passes through as a verbatim string
/// (<c>XamlBuilder.SetProperty(el, "IsEnabled", "{x:Bind ...}", ctx)</c>), reach Jalium's runtime
/// markup-extension resolution at all?
/// </summary>
/// <remarks>
/// <c>XamlReader.BuilderSetProperty</c> documents that it mirrors "SetProperty's markup-extension
/// branch", and <c>XamlReader.ResolveTypeUncached</c> step 5 turns a failed <c>Bind</c> lookup
/// into a <c>BindExtension</c> lookup by simple name. If this type is instantiated at runtime,
/// <c>x:Bind</c> has an interception point inside the facade and M2 needs no upstream change.
/// If it is not, the generator's string passthrough never enters that branch and <c>x:Bind</c>
/// can only be handled by rewriting the markup before compilation.
/// </remarks>
public sealed class XamlBindExtension : MarkupExtension
{
    /// <summary>How many times the parser constructed and evaluated this extension.</summary>
    public static int HitCount => Volatile.Read(ref _hitCount);

    private static int _hitCount;

    public string? Path { get; set; }

    public string Mode { get; set; } = "OneTime";

    public XamlBindExtension()
    {
    }

    public XamlBindExtension(string path)
    {
        Path = path;
    }

    public override object? ProvideValue(IServiceProvider serviceProvider)
    {
        Interlocked.Increment(ref _hitCount);

        // Returning false is the probe: the target property (Button.IsEnabled) defaults to true,
        // so a false value can only mean this method ran.
        return false;
    }
}
