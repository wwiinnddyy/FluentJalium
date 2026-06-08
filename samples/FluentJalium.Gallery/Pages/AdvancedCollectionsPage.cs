using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using Jalium.UI.Data;
using FluentJalium.Controls;
using FWBorder = FluentJalium.Controls.FWBorder;

namespace FluentJalium.Gallery.Pages;

/// <summary>
/// Gallery page demonstrating advanced collection controls.
/// </summary>
public class AdvancedCollectionsPage : Page
{
    private FWItemsRepeater? _repeater;
    private FWScroller? _repeaterScroller;
    private ScrollViewer? _repeaterScrollViewer;
    private TextBlock? _repeaterDiagnosticsText;
    private ObservableCollection<SampleItem> _items;

    public AdvancedCollectionsPage()
    {
        Title = "Advanced Collections";
        _items = GenerateSampleItems();
        InitializeComponent();
    }

    public UIElement CreateContent()
    {
        return Content is UIElement element
            ? element
            : new StackPanel();
    }

    private void InitializeComponent()
    {
        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Padding = new Thickness(24)
        };

        var mainStack = new StackPanel { Spacing = 32 };

        // FWItemsRepeater Section
        mainStack.Children.Add(CreateSectionHeader("FWItemsRepeater",
            "High-performance virtualizing list with flexible layouts"));
        mainStack.Children.Add(CreateItemsRepeaterSection());

        scrollViewer.Content = mainStack;
        Content = scrollViewer;
    }

    private UIElement CreateSectionHeader(string title, string description)
    {
        var stack = new StackPanel { Spacing = 8 };

        stack.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 24,
            FontWeight = FontWeights.SemiBold
        });

        stack.Children.Add(new TextBlock
        {
            Text = description,
            FontSize = 14,
            Opacity = 0.7
        });

        return stack;
    }

    private UIElement CreateItemsRepeaterSection()
    {
        var mainGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(400) }
            },
            RowSpacing = 16
        };

        // Layout controls
        var controlPanel = CreateLayoutControls();
        Grid.SetRow(controlPanel, 0);

        // ItemsRepeater container
        var repeaterContainer = CreateItemsRepeaterDemo();
        Grid.SetRow(repeaterContainer, 1);

        mainGrid.Children.Add(controlPanel);
        mainGrid.Children.Add(repeaterContainer);

        return mainGrid;
    }

    private UIElement CreateLayoutControls()
    {
        var stack = new StackPanel
        {
            Spacing = 10
        };

        var panel = new FWWrapPanel
        {
            HorizontalSpacing = 8,
            VerticalSpacing = 8
        };

        var stackLayoutButton = new Button
        {
            Content = "Stack Layout",
            MinWidth = 120
        };
        stackLayoutButton.Click += (s, e) => SwitchToStackLayout();

        var gridLayoutButton = new Button
        {
            Content = "Grid Layout",
            MinWidth = 120
        };
        gridLayoutButton.Click += (s, e) => SwitchToGridLayout();

        var addItemButton = new Button
        {
            Content = "Add Item",
            MinWidth = 100
        };
        addItemButton.Click += (s, e) => AddItem();

        var removeItemButton = new Button
        {
            Content = "Remove Item",
            MinWidth = 100
        };
        removeItemButton.Click += (s, e) => RemoveItem();

        var firstWindowButton = new Button
        {
            Content = "Window 0-5",
            MinWidth = 100
        };
        firstWindowButton.Click += (s, e) => RealizeWindow(0, 5);

        var laterWindowButton = new Button
        {
            Content = "Window 8-5",
            MinWidth = 100
        };
        laterWindowButton.Click += (s, e) => RealizeWindow(8, 5);

        var allItemsButton = new Button
        {
            Content = "All",
            MinWidth = 80
        };
        allItemsButton.Click += (s, e) => ResetRealizationWindow();

        var viewportButton = new Button
        {
            Content = "Viewport",
            MinWidth = 100
        };
        viewportButton.Click += (s, e) => AttachViewportWindow();

        var cacheButton = new Button
        {
            Content = "Cache 80/200",
            MinWidth = 120
        };
        cacheButton.Click += (s, e) => ToggleViewportCache();

        _repeaterDiagnosticsText = new TextBlock
        {
            FontSize = 13,
            Opacity = 0.72,
            TextWrapping = TextWrapping.Wrap
        };

        panel.Children.Add(stackLayoutButton);
        panel.Children.Add(gridLayoutButton);
        panel.Children.Add(addItemButton);
        panel.Children.Add(removeItemButton);
        panel.Children.Add(firstWindowButton);
        panel.Children.Add(laterWindowButton);
        panel.Children.Add(allItemsButton);
        panel.Children.Add(viewportButton);
        panel.Children.Add(cacheButton);

        stack.Children.Add(panel);
        stack.Children.Add(_repeaterDiagnosticsText);

        return stack;
    }

    private UIElement CreateItemsRepeaterDemo()
    {
        var border = new FWBorder
        {
            Background = new SolidColorBrush(Color.FromRgb(0xF9, 0xF9, 0xF9)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(12)
        };

        _repeaterScroller = new FWScroller
        {
            VerticalScrollMode = ScrollMode.Auto,
            HorizontalScrollMode = ScrollMode.Disabled
        };

        _repeaterScrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        };

        _repeater = new FWItemsRepeater
        {
            ItemsSource = _items,
            ItemTemplate = CreateItemTemplate(),
            Layout = new StackLayout
            {
                Orientation = Orientation.Vertical,
                Spacing = 8
            },
            HorizontalCacheLength = 200,
            VerticalCacheLength = 80,
            EstimatedItemExtent = 98
        };

        _repeaterScrollViewer.Content = _repeater;
        _repeaterScroller.AttachScrollViewer(_repeaterScrollViewer);
        border.Child = _repeaterScrollViewer;
        AttachViewportWindow();
        UpdateRepeaterDiagnostics();

        return border;
    }

    private DataTemplate CreateItemTemplate()
    {
        var template = new DataTemplate();

        template.SetVisualTree(() =>
        {
            var border = new FWBorder
            {
                Background = new SolidColorBrush(Colors.White),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xD0, 0xD0, 0xD0)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 0, 0, 8)
            };

            var stack = new StackPanel { Spacing = 4 };

            var titleBlock = new TextBlock
            {
                FontSize = 16,
                FontWeight = FontWeights.Medium
            };
            titleBlock.SetBinding(TextBlock.TextProperty, new Binding("Title"));

            var descBlock = new TextBlock
            {
                FontSize = 14,
                Opacity = 0.7
            };
            descBlock.SetBinding(TextBlock.TextProperty, new Binding("Description"));

            stack.Children.Add(titleBlock);
            stack.Children.Add(descBlock);
            border.Child = stack;

            return border;
        });

        template.Seal();
        return template;
    }

    private void SwitchToStackLayout()
    {
        if (_repeater != null)
        {
            _repeater.Layout = new StackLayout
            {
                Orientation = Orientation.Vertical,
                Spacing = 8
            };
            UpdateRepeaterDiagnostics();
        }
    }

    private void SwitchToGridLayout()
    {
        if (_repeater != null)
        {
            _repeater.Layout = new UniformGridLayout
            {
                Orientation = Orientation.Vertical,
                MinItemWidth = 250,
                MinItemHeight = 80,
                MinColumnSpacing = 8,
                MinRowSpacing = 8
            };
            UpdateRepeaterDiagnostics();
        }
    }

    private void AddItem()
    {
        _items.Add(new SampleItem
        {
            Title = $"Item {_items.Count + 1}",
            Description = $"Description for item {_items.Count + 1}"
        });
        UpdateRepeaterDiagnostics();
    }

    private void RemoveItem()
    {
        if (_items.Count > 0)
        {
            _items.RemoveAt(_items.Count - 1);
            UpdateRepeaterDiagnostics();
        }
    }

    private void RealizeWindow(int startIndex, int itemCount)
    {
        _repeater?.RealizeRange(startIndex, itemCount);
        UpdateRepeaterDiagnostics();
    }

    private void ResetRealizationWindow()
    {
        _repeater?.DetachViewport();
        _repeater?.ResetRealizationWindow();
        UpdateRepeaterDiagnostics();
    }

    private void AttachViewportWindow()
    {
        if (_repeater == null || _repeaterScroller == null)
        {
            return;
        }

        _repeater.AttachViewport(_repeaterScroller);
        UpdateRepeaterDiagnostics();
    }

    private void ToggleViewportCache()
    {
        if (_repeater == null)
        {
            return;
        }

        _repeater.VerticalCacheLength = Math.Abs(_repeater.VerticalCacheLength - 80) < 0.1 ? 200 : 80;
        if (_repeater.RealizationSource == FWItemsRepeaterRealizationSource.Viewport)
        {
            AttachViewportWindow();
        }
        else
        {
            UpdateRepeaterDiagnostics();
        }
    }

    private void UpdateRepeaterDiagnostics()
    {
        if (_repeater == null || _repeaterDiagnosticsText == null)
        {
            return;
        }

        var diagnostics = _repeater.GetDiagnostics();
        var range = diagnostics.HasRealizedElements
            ? $"{diagnostics.FirstRealizedIndex}-{diagnostics.LastRealizedIndex}"
            : "none";
        var viewportState = diagnostics.IsViewportAttached ? $"attached/{diagnostics.AttachedViewportSource}" : "manual";
        _repeaterDiagnosticsText.Text =
            $"Mode: {diagnostics.RealizationMode}/{diagnostics.RealizationSource} ({viewportState}) | Axis: {diagnostics.ViewportOrientation} | Items: {diagnostics.ItemCount} | Realized: {diagnostics.RealizedElementCount} | Range: {range} | Viewport: {diagnostics.ViewportStart:0}-{diagnostics.ViewportStart + diagnostics.ViewportLength:0} @ {diagnostics.EstimatedItemExtent:0}px | Reused: {diagnostics.LastReusedElementCount} | Pool: {diagnostics.RecycledElementCount} | Cache: active {diagnostics.ActiveCacheLength:0}, H{diagnostics.HorizontalCacheLength:0}/V{diagnostics.VerticalCacheLength:0}";
    }

    private ObservableCollection<SampleItem> GenerateSampleItems()
    {
        var items = new ObservableCollection<SampleItem>();
        for (int i = 1; i <= 20; i++)
        {
            items.Add(new SampleItem
            {
                Title = $"Sample Item {i}",
                Description = $"This is a description for item {i} demonstrating FWItemsRepeater"
            });
        }
        return items;
    }

    private class SampleItem
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
