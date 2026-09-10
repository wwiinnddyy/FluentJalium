namespace FluentJalium.WinUI;

/// <summary>
/// The WinUI / UWP XML namespace spellings this facade answers to. Kept as constants so the
/// assembly-level namespace mappings in AssemblyInfo.cs stay in sync with anything that has to
/// compare against the same strings.
/// </summary>
public static class WinUiXmlNamespaces
{
    public const string Controls = "using:Microsoft.UI.Xaml.Controls";
    public const string Primitives = "using:Microsoft.UI.Xaml.Controls.Primitives";
    public const string Media = "using:Microsoft.UI.Xaml.Media";
    public const string Data = "using:Microsoft.UI.Xaml.Data";
    public const string Interactivity = "using:Microsoft.Xaml.Interactivity";
    public const string Xaml = "using:Microsoft.UI.Xaml";
    public const string XamlMarkup = "using:Microsoft.UI.Xaml.Markup";

    /// <summary>UWP-era spelling still present in older samples.</summary>
    public const string LegacyControls = "using:Windows.UI.Xaml.Controls";

    /// <summary>UWP-era spelling still present in older samples.</summary>
    public const string LegacyXaml = "using:Windows.UI.Xaml";
}
