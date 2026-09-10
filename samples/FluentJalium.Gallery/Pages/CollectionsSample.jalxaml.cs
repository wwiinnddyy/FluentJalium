using FluentJalium.Controls;
using Jalium.UI.Controls;

namespace FluentJalium.Gallery.Pages;

public sealed partial class CollectionsSample : UserControl
{
    public CollectionsSample()
    {
        InitializeComponent();
        Grid.ItemsSource = new[]
        {
            new CollectionRow("Button", "Stable", "Input"),
            new CollectionRow("ItemsRepeater", "Preview", "Collections"),
            new CollectionRow("MicaSurface", "Stable", "Materials")
        };
        List.ItemsSource = new[] { "Design system", "Gallery shell", "Theme resources", "Jalxaml sample" };
    }

    private sealed record CollectionRow(string Control, string Status, string Family);
}
