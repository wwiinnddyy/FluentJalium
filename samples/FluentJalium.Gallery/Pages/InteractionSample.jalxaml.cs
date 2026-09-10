using FluentJalium.Controls;
using Jalium.UI;
using Jalium.UI.Controls;

namespace FluentJalium.Gallery.Pages;

public sealed partial class InteractionSample : UserControl
{
    public InteractionSample()
    {
        InitializeComponent();

        Scroller.ZoomMode = ZoomMode.Enabled;
        Scroller.MinZoomFactor = 0.8;
        Scroller.MaxZoomFactor = 1.6;
        Scroller.Content = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Children =
            {
                new FWTextBlock { Text = "Viewport-aware scrolling" },
                new FWTextBlock { Text = "The Scroller exposes offsets, zoom, chaining, and snap-point contracts." },
                new FWTextBlock { Text = "Scroll or zoom this surface in a real window." },
                new FWTextBlock { Text = "All content remains inside FluentJalium controls." }
            }
        };

        AnnotatedBar.Labels = new List<ScrollBarLabel>
        {
            new() { ScrollOffset = 24, Content = "Review", Type = ScrollBarLabelType.Info },
            new() { ScrollOffset = 72, Content = "Warning", Type = ScrollBarLabelType.Warning }
        };
    }
}
