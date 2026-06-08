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
    private TextBlock? _repeaterScenarioText;
    private TextBlock? _repeaterDiagnosticsText;
    private ObservableCollection<SampleItem> _items;
    private ItemsRepeaterGalleryProfile _currentProfile;
    private int _cacheProfileIndex;
    private string _lastQaAction = "Initialized";

    internal enum ItemsRepeaterGalleryScenario
    {
        Baseline,
        LargeListStress,
        HorizontalVirtualization
    }

    internal readonly record struct ItemsRepeaterGalleryProfile(
        ItemsRepeaterGalleryScenario Scenario,
        string Name,
        string Intent,
        int ItemCount,
        string ItemTitlePrefix,
        Orientation Orientation,
        bool UseUniformGridLayout,
        double EstimatedItemExtent,
        double HorizontalCacheLength,
        double VerticalCacheLength,
        double ViewportStart,
        double ViewportLength,
        FWItemsRepeaterViewportSource PreferredViewportSource)
    {
        public double ActiveCacheLength => Orientation == Orientation.Horizontal
            ? HorizontalCacheLength
            : VerticalCacheLength;
    }

    internal readonly record struct ItemsRepeaterCacheProfile(
        string Name,
        double HorizontalCacheLength,
        double VerticalCacheLength);

    public AdvancedCollectionsPage()
    {
        Title = "Advanced Collections";
        _currentProfile = CreateItemsRepeaterQaProfile(ItemsRepeaterGalleryScenario.Baseline);
        _items = CreateItemsRepeaterSampleItems(_currentProfile);
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

        var baselineButton = new Button
        {
            Content = "Baseline 20",
            MinWidth = 110
        };
        baselineButton.Click += (s, e) => ApplyItemsRepeaterProfile(ItemsRepeaterGalleryScenario.Baseline);

        var largeListButton = new Button
        {
            Content = "Large list",
            MinWidth = 110
        };
        largeListButton.Click += (s, e) => ApplyItemsRepeaterProfile(ItemsRepeaterGalleryScenario.LargeListStress);

        var horizontalButton = new Button
        {
            Content = "Horizontal",
            MinWidth = 110
        };
        horizontalButton.Click += (s, e) => ApplyItemsRepeaterProfile(ItemsRepeaterGalleryScenario.HorizontalVirtualization);

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
        viewportButton.Click += (s, e) => AttachViewportSource(_currentProfile.PreferredViewportSource, "Restored seeded viewport");

        var scrollViewerButton = new Button
        {
            Content = "Attach ScrollViewer",
            MinWidth = 150
        };
        scrollViewerButton.Click += (s, e) => AttachViewportSource(FWItemsRepeaterViewportSource.ScrollViewer, "Attached raw ScrollViewer");

        var scrollerButton = new Button
        {
            Content = "Attach FWScroller",
            MinWidth = 140
        };
        scrollerButton.Click += (s, e) => AttachViewportSource(FWItemsRepeaterViewportSource.Scroller, "Attached FWScroller");

        var reattachButton = new Button
        {
            Content = "Reattach swap",
            MinWidth = 130
        };
        reattachButton.Click += (s, e) => ReattachViewportSource();

        var cacheButton = new Button
        {
            Content = "Cycle cache",
            MinWidth = 120
        };
        cacheButton.Click += (s, e) => CycleViewportCache();

        _repeaterScenarioText = new TextBlock
        {
            FontSize = 13,
            FontWeight = FontWeights.Medium,
            Opacity = 0.86,
            TextWrapping = TextWrapping.Wrap
        };

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
        panel.Children.Add(baselineButton);
        panel.Children.Add(largeListButton);
        panel.Children.Add(horizontalButton);
        panel.Children.Add(firstWindowButton);
        panel.Children.Add(laterWindowButton);
        panel.Children.Add(allItemsButton);
        panel.Children.Add(viewportButton);
        panel.Children.Add(scrollViewerButton);
        panel.Children.Add(scrollerButton);
        panel.Children.Add(reattachButton);
        panel.Children.Add(cacheButton);

        stack.Children.Add(panel);
        stack.Children.Add(_repeaterScenarioText);
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
            Layout = CreateItemsRepeaterLayout(_currentProfile),
            HorizontalCacheLength = _currentProfile.HorizontalCacheLength,
            VerticalCacheLength = _currentProfile.VerticalCacheLength,
            EstimatedItemExtent = _currentProfile.EstimatedItemExtent
        };

        _repeaterScrollViewer.Content = _repeater;
        _repeaterScroller.AttachScrollViewer(_repeaterScrollViewer);
        border.Child = _repeaterScrollViewer;
        UpdateScrollModes(_currentProfile);
        AttachViewportSource(_currentProfile.PreferredViewportSource, "Initialized seeded viewport");
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
                Margin = new Thickness(0, 0, 0, 8),
                MinWidth = 168
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

            var statusBlock = new TextBlock
            {
                FontSize = 12,
                Opacity = 0.64
            };
            statusBlock.SetBinding(TextBlock.TextProperty, new Binding("Status"));

            stack.Children.Add(titleBlock);
            stack.Children.Add(descBlock);
            stack.Children.Add(statusBlock);
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
                Orientation = _currentProfile.Orientation,
                Spacing = 8
            };
            _lastQaAction = "Applied stack layout";
            UpdateRepeaterDiagnostics();
        }
    }

    private void SwitchToGridLayout()
    {
        if (_repeater != null)
        {
            _repeater.Layout = new UniformGridLayout
            {
                Orientation = _currentProfile.Orientation,
                MinItemWidth = 250,
                MinItemHeight = 80,
                MinColumnSpacing = 8,
                MinRowSpacing = 8
            };
            _lastQaAction = "Applied grid layout";
            UpdateRepeaterDiagnostics();
        }
    }

    private void AddItem()
    {
        _items.Add(CreateItemsRepeaterSampleItem(_currentProfile, _items.Count + 1));
        _lastQaAction = "Added one item";
        UpdateRepeaterDiagnostics();
    }

    private void RemoveItem()
    {
        if (_items.Count > 0)
        {
            _items.RemoveAt(_items.Count - 1);
            _lastQaAction = "Removed one item";
            UpdateRepeaterDiagnostics();
        }
    }

    private void RealizeWindow(int startIndex, int itemCount)
    {
        _repeater?.RealizeRange(startIndex, itemCount);
        _lastQaAction = $"Manual range {startIndex}-{startIndex + itemCount - 1}";
        UpdateRepeaterDiagnostics();
    }

    private void ResetRealizationWindow()
    {
        _repeater?.DetachViewport();
        _repeater?.ResetRealizationWindow();
        _lastQaAction = "Reset to all items";
        UpdateRepeaterDiagnostics();
    }

    private void ApplyItemsRepeaterProfile(ItemsRepeaterGalleryScenario scenario)
    {
        _currentProfile = CreateItemsRepeaterQaProfile(scenario);
        _items = CreateItemsRepeaterSampleItems(_currentProfile);
        _cacheProfileIndex = 0;

        if (_repeater == null)
        {
            return;
        }

        _repeater.DetachViewport();
        _repeater.ItemsSource = _items;
        _repeater.Layout = CreateItemsRepeaterLayout(_currentProfile);
        _repeater.EstimatedItemExtent = _currentProfile.EstimatedItemExtent;
        _repeater.HorizontalCacheLength = _currentProfile.HorizontalCacheLength;
        _repeater.VerticalCacheLength = _currentProfile.VerticalCacheLength;
        UpdateScrollModes(_currentProfile);
        AttachViewportSource(_currentProfile.PreferredViewportSource, $"Applied {_currentProfile.Name}");
        UpdateRepeaterDiagnostics();
    }

    private void AttachViewportSource(FWItemsRepeaterViewportSource source, string action)
    {
        if (_repeater == null || _repeaterScrollViewer == null)
        {
            return;
        }

        if (source == FWItemsRepeaterViewportSource.Scroller)
        {
            if (_repeaterScroller == null)
            {
                return;
            }

            _repeaterScroller.AttachScrollViewer(_repeaterScrollViewer);
            _repeater.AttachViewport(_repeaterScroller, _currentProfile.Orientation);
        }
        else
        {
            _repeater.AttachViewport(_repeaterScrollViewer, _currentProfile.Orientation);
        }

        SeedViewportFromCurrentProfile();
        _lastQaAction = action;
        UpdateRepeaterDiagnostics();
    }

    private void ReattachViewportSource()
    {
        if (_repeater == null)
        {
            return;
        }

        var previousSource = _repeater.AttachedViewportSource;
        var nextSource = previousSource == FWItemsRepeaterViewportSource.Scroller
            ? FWItemsRepeaterViewportSource.ScrollViewer
            : FWItemsRepeaterViewportSource.Scroller;
        AttachViewportSource(nextSource, $"Reattached {previousSource} -> {nextSource}");
    }

    private void CycleViewportCache()
    {
        if (_repeater == null)
        {
            return;
        }

        var cacheProfiles = CreateItemsRepeaterCacheProfiles();
        _cacheProfileIndex = (_cacheProfileIndex + 1) % cacheProfiles.Count;
        var cacheProfile = cacheProfiles[_cacheProfileIndex];
        _repeater.HorizontalCacheLength = cacheProfile.HorizontalCacheLength;
        _repeater.VerticalCacheLength = cacheProfile.VerticalCacheLength;
        SeedViewportFromCurrentProfile();
        _lastQaAction = $"Cache profile {cacheProfile.Name}";
        UpdateRepeaterDiagnostics();
    }

    private void UpdateRepeaterDiagnostics()
    {
        if (_repeater == null || _repeaterDiagnosticsText == null)
        {
            return;
        }

        var diagnostics = _repeater.GetDiagnostics();
        if (_repeaterScenarioText != null)
        {
            _repeaterScenarioText.Text = CreateItemsRepeaterScenarioText(_currentProfile, diagnostics, _lastQaAction);
        }

        _repeaterDiagnosticsText.Text = CreateItemsRepeaterDiagnosticsText(diagnostics);
    }

    private void SeedViewportFromCurrentProfile()
    {
        if (_repeater == null || _repeaterScrollViewer == null)
        {
            return;
        }

        if (_currentProfile.Orientation == Orientation.Horizontal)
        {
            _repeaterScrollViewer.ScrollToHorizontalOffset(_currentProfile.ViewportStart);
        }
        else
        {
            _repeaterScrollViewer.ScrollToVerticalOffset(_currentProfile.ViewportStart);
        }

        _repeater.ApplyViewport(
            _currentProfile.ViewportStart,
            _currentProfile.ViewportLength,
            _currentProfile.Orientation);
    }

    private void UpdateScrollModes(ItemsRepeaterGalleryProfile profile)
    {
        if (_repeaterScrollViewer != null)
        {
            _repeaterScrollViewer.HorizontalScrollBarVisibility = profile.Orientation == Orientation.Horizontal
                ? ScrollBarVisibility.Auto
                : ScrollBarVisibility.Disabled;
            _repeaterScrollViewer.VerticalScrollBarVisibility = profile.Orientation == Orientation.Horizontal
                ? ScrollBarVisibility.Disabled
                : ScrollBarVisibility.Auto;
        }

        if (_repeaterScroller != null)
        {
            _repeaterScroller.HorizontalScrollMode = profile.Orientation == Orientation.Horizontal
                ? ScrollMode.Auto
                : ScrollMode.Disabled;
            _repeaterScroller.VerticalScrollMode = profile.Orientation == Orientation.Horizontal
                ? ScrollMode.Disabled
                : ScrollMode.Auto;
        }
    }

    internal static ItemsRepeaterGalleryProfile CreateItemsRepeaterQaProfile(ItemsRepeaterGalleryScenario scenario)
    {
        return scenario switch
        {
            ItemsRepeaterGalleryScenario.LargeListStress => new ItemsRepeaterGalleryProfile(
                scenario,
                "Large-list stress",
                "1500 items seeded into a small viewport to verify recycler stability.",
                1500,
                "Stress Row",
                Orientation.Vertical,
                false,
                64,
                240,
                320,
                2560,
                480,
                FWItemsRepeaterViewportSource.Scroller),
            ItemsRepeaterGalleryScenario.HorizontalVirtualization => new ItemsRepeaterGalleryProfile(
                scenario,
                "Horizontal virtualization",
                "Horizontal stack window with horizontal cache and ScrollViewer reattachment checks.",
                96,
                "Lane",
                Orientation.Horizontal,
                false,
                180,
                360,
                80,
                720,
                540,
                FWItemsRepeaterViewportSource.ScrollViewer),
            _ => new ItemsRepeaterGalleryProfile(
                ItemsRepeaterGalleryScenario.Baseline,
                "Baseline viewport",
                "20 items with a seeded FWScroller viewport for quick visual QA.",
                20,
                "Sample Item",
                Orientation.Vertical,
                false,
                98,
                200,
                80,
                0,
                320,
                FWItemsRepeaterViewportSource.Scroller)
        };
    }

    internal static IReadOnlyList<ItemsRepeaterCacheProfile> CreateItemsRepeaterCacheProfiles()
    {
        return new[]
        {
            new ItemsRepeaterCacheProfile("Balanced", 200, 80),
            new ItemsRepeaterCacheProfile("Tight", 80, 40),
            new ItemsRepeaterCacheProfile("Stress buffer", 360, 260)
        };
    }

    internal static ObservableCollection<SampleItem> CreateItemsRepeaterSampleItems(ItemsRepeaterGalleryProfile profile)
    {
        if (profile.ItemCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(profile), "Item count cannot be negative.");
        }

        var items = new ObservableCollection<SampleItem>();
        for (var index = 1; index <= profile.ItemCount; index++)
        {
            items.Add(CreateItemsRepeaterSampleItem(profile, index));
        }

        return items;
    }

    internal static SampleItem CreateItemsRepeaterSampleItem(ItemsRepeaterGalleryProfile profile, int index)
    {
        if (index <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Item index must be positive.");
        }

        var paddedIndex = index.ToString("0000");
        return new SampleItem
        {
            Title = profile.Scenario == ItemsRepeaterGalleryScenario.Baseline
                ? $"{profile.ItemTitlePrefix} {index}"
                : $"{profile.ItemTitlePrefix} {paddedIndex}",
            Description = $"{profile.Name} QA item {index} of {profile.ItemCount}. {profile.Intent}",
            Status = $"{profile.Orientation} | extent {profile.EstimatedItemExtent:0}px | cache H{profile.HorizontalCacheLength:0}/V{profile.VerticalCacheLength:0}"
        };
    }

    internal static VirtualizingLayout CreateItemsRepeaterLayout(ItemsRepeaterGalleryProfile profile)
    {
        if (profile.UseUniformGridLayout)
        {
            return new UniformGridLayout
            {
                Orientation = profile.Orientation,
                MinItemWidth = profile.Orientation == Orientation.Horizontal ? 180 : 250,
                MinItemHeight = 80,
                MinColumnSpacing = 8,
                MinRowSpacing = 8
            };
        }

        return new StackLayout
        {
            Orientation = profile.Orientation,
            Spacing = profile.Orientation == Orientation.Horizontal ? 10 : 8
        };
    }

    internal static string CreateItemsRepeaterScenarioText(
        ItemsRepeaterGalleryProfile profile,
        FWItemsRepeaterDiagnostics diagnostics,
        string lastAction)
    {
        var attachment = diagnostics.IsViewportAttached
            ? $"{diagnostics.AttachedViewportSource}/{diagnostics.AttachedViewportOrientation}"
            : "detached";

        return
            $"Scenario: {profile.Name} | Goal: {profile.Intent} | Source: {attachment} | Items: {diagnostics.ItemCount} | Seed viewport: {profile.ViewportStart:0}-{profile.ViewportStart + profile.ViewportLength:0} | Last action: {lastAction}";
    }

    internal static string CreateItemsRepeaterDiagnosticsText(FWItemsRepeaterDiagnostics diagnostics)
    {
        var range = diagnostics.HasRealizedElements
            ? $"{diagnostics.FirstRealizedIndex}-{diagnostics.LastRealizedIndex}"
            : "none";
        var requested = diagnostics.RequestedRealizedItemCount > 0
            ? $"{diagnostics.RequestedFirstRealizedIndex}-{diagnostics.RequestedFirstRealizedIndex + diagnostics.RequestedRealizedItemCount - 1}"
            : "none";
        var viewportState = diagnostics.IsViewportAttached
            ? $"attached/{diagnostics.AttachedViewportSource}/{diagnostics.AttachedViewportOrientation}"
            : "detached/manual";
        var virtualizationState = diagnostics.ItemCount > 0 && diagnostics.RealizedElementCount < diagnostics.ItemCount
            ? "virtualized"
            : "all-realized";

        return
            $"Mode: {diagnostics.RealizationMode}/{diagnostics.RealizationSource} ({viewportState}) | QA: {virtualizationState} | Axis: {diagnostics.ViewportOrientation} | Items: {diagnostics.ItemCount} | Realized: {diagnostics.RealizedElementCount} | Requested: {requested} | Range: {range} | Viewport: {diagnostics.ViewportStart:0}-{diagnostics.ViewportStart + diagnostics.ViewportLength:0} @ {diagnostics.EstimatedItemExtent:0}px | Reused: {diagnostics.LastReusedElementCount} | Pool: {diagnostics.RecycledElementCount} | Cache: active {diagnostics.ActiveCacheLength:0}, H{diagnostics.HorizontalCacheLength:0}/V{diagnostics.VerticalCacheLength:0}";
    }

    internal sealed class SampleItem
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
