using FluentJalium.Controls;
using Jalium.UI.Controls;

namespace FluentJalium.Gallery.Pages;

/// <summary>
/// A Jalxaml-first sample for the data-inspection controls.  The markup owns the visual
/// composition while code-behind supplies real JSON, source text, and bytes.
/// </summary>
public sealed partial class DataInspectorSample : UserControl
{
    public DataInspectorSample()
    {
        InitializeComponent();
        JsonViewer.JsonText = "{\"project\":\"FluentJalium\",\"version\":1,\"features\":[\"Jalxaml\",\"Mica\"]}";
        DiffViewer.OriginalText = "AccentFillColor = Blue\nCornerRadius = 8";
        DiffViewer.ModifiedText = "AccentFillColor = Teal\nCornerRadius = 10\nDensity = Comfortable";
        HexViewer.Data = new byte[] { 0x46, 0x6C, 0x75, 0x65, 0x6E, 0x74, 0x4A, 0x61, 0x6C, 0x69, 0x75, 0x6D };
    }
}
