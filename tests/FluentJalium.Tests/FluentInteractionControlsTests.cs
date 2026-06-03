using FluentJalium.Controls;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;

namespace FluentJalium.Tests;

[Collection("Application")]
public sealed class FluentInteractionControlsTests
{
    [Fact]
    public void FWInteractionControls_ShouldComposeInsideLiquidGlassSurface()
    {
        var scrollContent = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 6,
            Children =
            {
                new FWTextBlock { Text = "Item 1" },
                new FWTextBlock { Text = "Item 2" },
                new FWTextBlock { Text = "Item 3" }
            }
        };
        var scrollViewer = new FWScrollViewer
        {
            Width = 160,
            Height = 72,
            Content = scrollContent,
            VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            IsScrollBarAutoHideEnabled = false,
            IsScrollInertiaEnabled = false,
            Padding = new Thickness(8)
        };
        var leftItems = new SwipeItems
        {
            Mode = SwipeMode.Reveal
        };
        leftItems.Add(new SwipeItem
        {
            Text = "Archive",
            IconSource = FluentIconRegular.Archive24.GetString(),
            Background = new SolidColorBrush(Color.FromRgb(0x10, 0x7C, 0x10)),
            Foreground = new SolidColorBrush(Colors.White),
            BehaviorOnInvoked = BehaviorOnInvoked.RemainOpen
        });
        var rightItems = new SwipeItems
        {
            Mode = SwipeMode.Execute
        };
        rightItems.Add(new SwipeItem
        {
            Text = "Delete",
            IconSource = FluentIconRegular.Delete24.GetString(),
            Background = new SolidColorBrush(Color.FromRgb(0xC4, 0x2B, 0x1C)),
            Foreground = new SolidColorBrush(Colors.White),
            BehaviorOnInvoked = BehaviorOnInvoked.Close
        });
        var swipeControl = new FWSwipeControl
        {
            LeftItems = leftItems,
            RightItems = rightItems,
            Content = new FWTextBlock { Text = "Swipe row" },
            Background = new SolidColorBrush(Color.FromRgb(0x22, 0x22, 0x22)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x44)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6)
        };
        var splitter = new FWGridSplitter
        {
            ResizeDirection = GridResizeDirection.Columns,
            ResizeBehavior = GridResizeBehavior.PreviousAndNext,
            ShowsPreview = true,
            DragIncrement = 2,
            KeyboardIncrement = 16,
            Width = 6
        };
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120), MinWidth = 80 });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(6) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star, MinWidth = 100 });
        Grid.SetColumn(splitter, 1);
        grid.Children.Add(new FWBorder { Child = new FWTextBlock { Text = "Left" } });
        grid.Children.Add(splitter);
        var stack = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                scrollViewer,
                swipeControl,
                grid
            }
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
            Child = stack
        };

        Assert.Same(scrollContent, scrollViewer.Content);
        Assert.Equal(ScrollBarVisibility.Visible, scrollViewer.VerticalScrollBarVisibility);
        Assert.Equal(ScrollBarVisibility.Disabled, scrollViewer.HorizontalScrollBarVisibility);
        Assert.False(scrollViewer.IsScrollBarAutoHideEnabled);
        Assert.False(scrollViewer.IsScrollInertiaEnabled);
        Assert.Equal(8, scrollViewer.Padding.Left);
        Assert.Same(leftItems, swipeControl.LeftItems);
        Assert.Same(rightItems, swipeControl.RightItems);
        Assert.Equal(SwipeMode.Reveal, swipeControl.LeftItems!.Mode);
        Assert.Equal(SwipeMode.Execute, swipeControl.RightItems!.Mode);
        Assert.Equal("Archive", swipeControl.LeftItems[0].Text);
        Assert.Equal(FluentIconRegular.Archive24.GetString(), swipeControl.LeftItems[0].IconSource);
        Assert.Equal(BehaviorOnInvoked.RemainOpen, swipeControl.LeftItems[0].BehaviorOnInvoked);
        Assert.Equal("Delete", swipeControl.RightItems[0].Text);
        Assert.Equal(BehaviorOnInvoked.Close, swipeControl.RightItems[0].BehaviorOnInvoked);
        Assert.Equal(1, swipeControl.BorderThickness.Left);
        Assert.Equal(6, swipeControl.CornerRadius.TopLeft);
        Assert.Equal(GridResizeDirection.Columns, splitter.ResizeDirection);
        Assert.Equal(GridResizeBehavior.PreviousAndNext, splitter.ResizeBehavior);
        Assert.True(splitter.ShowsPreview);
        Assert.Equal(2, splitter.DragIncrement);
        Assert.Equal(16, splitter.KeyboardIncrement);
        Assert.Equal(6, splitter.Width);
        Assert.Equal(3, grid.ColumnDefinitions.Count);
        Assert.Equal(2, grid.Children.Count);
        Assert.Same(splitter, grid.Children[1]);
        Assert.Equal(10, stack.Spacing);
        Assert.Equal(3, stack.Children.Count);
        Assert.Equal(FWFluentMaterialKind.LiquidGlass, surface.MaterialKind);
        Assert.True(surface.LiquidGlass);
        Assert.Equal(70, surface.RefractionAmount);
        Assert.Equal(0.42, surface.ChromaticAberration);
        Assert.Equal(24, surface.FusionRadius);
        Assert.Equal(BorderShape.SuperEllipse, surface.Shape);
        Assert.Equal(4, surface.SuperEllipseN);
        Assert.Same(stack, surface.Child);
    }
}
