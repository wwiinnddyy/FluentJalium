using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentJalium.Gallery.Services;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using FluentJalium.Controls;
using FWButton = FluentJalium.Controls.FWButton;
using FWBorder = FluentJalium.Controls.FWBorder;

namespace FluentJalium.Gallery.Pages;

/// <summary>
/// Gallery page demonstrating advanced interaction controls.
/// </summary>
public class InteractionControlsPage : Page
{
    private TextBlock? _refreshStatusText;
    private TextBlock? _refreshDiagnosticsText;
    private int _refreshCount = 0;
    private RefreshRequestedDeferral? _pendingRefreshDeferral;

    public InteractionControlsPage()
    {
        Title = "Interaction Controls";
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

        // FWRefreshContainer Section
        mainStack.Children.Add(CreateSectionHeader("FWRefreshContainer",
            "Pull-to-refresh functionality for scrollable content"));
        mainStack.Children.Add(CreateRefreshContainerSection());

        // FWScroller Section
        mainStack.Children.Add(CreateSectionHeader("FWScroller",
            "Advanced scrolling with snap points and zoom"));
        mainStack.Children.Add(CreateScrollerSection());

        // FWAnnotatedScrollBar Section
        mainStack.Children.Add(CreateSectionHeader("FWAnnotatedScrollBar",
            "Enhanced scrollbar with position markers"));
        mainStack.Children.Add(CreateAnnotatedScrollBarSection());

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

    private UIElement CreateRefreshContainerSection()
    {
        var border = new FWBorder
        {
            Background = ThemeBrush("ControlBackground"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Height = 300
        };

        var refreshContainer = new FWRefreshContainer
        {
            PullDirection = RefreshPullDirection.TopToBottom
        };
        refreshContainer.RefreshRequested += OnRefreshRequested;

        var contentStack = new StackPanel
        {
            Spacing = 12
        };

        contentStack.Children.Add(new TextBlock
        {
            Text = "Pull down to refresh",
            FontSize = 18,
            FontWeight = FontWeights.Medium,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 20, 0, 0)
        });

        _refreshStatusText = new TextBlock
        {
            Text = $"Refreshed {_refreshCount} times",
            FontSize = 14,
            HorizontalAlignment = HorizontalAlignment.Center,
            Opacity = 0.7
        };
        contentStack.Children.Add(_refreshStatusText);

        _refreshDiagnosticsText = new TextBlock
        {
            Text = FormatRefreshContainerDiagnostics("Initial", refreshContainer.GetDiagnostics()),
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Center,
            Opacity = 0.75
        };
        contentStack.Children.Add(_refreshDiagnosticsText);

        var refreshButton = new FWButton { Content = "Request refresh" };
        refreshButton.Click += (_, _) =>
        {
            refreshContainer.RequestRefresh();
            UpdateRefreshDiagnostics("Requested", refreshContainer);
        };
        var completeButton = new FWButton { Content = "Complete" };
        completeButton.Click += (_, _) => CompletePendingRefresh(refreshContainer, incrementCount: true, "Manually completed");
        var cancelButton = new FWButton { Content = "Cancel" };
        cancelButton.Click += (_, _) => CompletePendingRefresh(refreshContainer, incrementCount: false, "Cancelled");

        contentStack.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Spacing = 8,
            Children =
            {
                refreshButton,
                completeButton,
                cancelButton
            }
        });

        // Add sample content
        for (int i = 1; i <= 10; i++)
        {
            contentStack.Children.Add(new FWBorder
            {
                Padding = new Thickness(8),
                Background = ThemeBrush(i % 2 == 0 ? "LayerFillColorDefaultBrush" : "ControlBackground"),
                BorderBrush = ThemeBrush("ControlBorder"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Margin = new Thickness(0, 4, 0, 0),
                Child = new TextBlock { Text = $"Content Item {i}" }
            });
        }

        refreshContainer.Content = contentStack;
        border.Child = refreshContainer;

        return border;
    }

    private void OnRefreshRequested(object? sender, RefreshRequestedEventArgs e)
    {
        var deferral = e.GetDeferral();
        _pendingRefreshDeferral = deferral;
        if (sender is FWRefreshContainer requestedContainer)
        {
            UpdateRefreshDiagnostics("Refresh requested", requestedContainer);
        }

        Task.Run(async () =>
        {
            // Simulate async refresh operation
            await Task.Delay(2000);

            Dispatcher.Invoke(() =>
            {
                if (sender is FWRefreshContainer completedContainer)
                {
                    CompletePendingRefresh(completedContainer, incrementCount: true, "Auto completed", deferral);
                }
            });
        });
    }

    private void CompletePendingRefresh(
        FWRefreshContainer refreshContainer,
        bool incrementCount,
        string reason,
        RefreshRequestedDeferral? expectedDeferral = null)
    {
        if (_pendingRefreshDeferral == null ||
            (expectedDeferral != null && !ReferenceEquals(_pendingRefreshDeferral, expectedDeferral)))
        {
            UpdateRefreshDiagnostics($"{reason} ignored", refreshContainer);
            return;
        }

        var deferral = _pendingRefreshDeferral;
        _pendingRefreshDeferral = null;
        if (incrementCount)
        {
            _refreshCount++;
            if (_refreshStatusText != null)
            {
                _refreshStatusText.Text = $"Refreshed {_refreshCount} times";
            }
        }

        deferral.Complete();
        UpdateRefreshDiagnostics(reason, refreshContainer);
    }

    private void UpdateRefreshDiagnostics(string reason, FWRefreshContainer refreshContainer)
    {
        if (_refreshDiagnosticsText != null)
        {
            _refreshDiagnosticsText.Text = FormatRefreshContainerDiagnostics(reason, refreshContainer.GetDiagnostics());
        }
    }

    internal static string FormatRefreshContainerDiagnostics(string reason, FWRefreshContainerDiagnostics diagnostics)
    {
        var deferralState = diagnostics.IsRefreshing ? "pending" : "idle";
        return $"{reason}: refreshing {FormatOnOff(diagnostics.IsRefreshing)}; pulling {FormatOnOff(diagnostics.IsPulling)}; deferral {deferralState}; progress {diagnostics.PullProgress:P0}; distance {diagnostics.PullDistance:0}/{diagnostics.PullThreshold:0}/{diagnostics.MaxPullDistance:0}; direction {diagnostics.PullDirection}; visualizer {diagnostics.VisualizerState}; template {FormatOnOff(diagnostics.HasScrollViewer)}; custom visualizer {FormatOnOff(diagnostics.HasCustomVisualizer)}.";
    }

    private static string FormatOnOff(bool value) => value ? "on" : "off";

    private UIElement CreateScrollerSection()
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            },
            ColumnSpacing = 16
        };

        // Basic scroller
        var basicCard = CreateDemoCard("Basic Scroller", CreateBasicScroller());
        Grid.SetColumn(basicCard, 0);

        // Scroller with snap points
        var snapCard = CreateDemoCard("With Snap Points", CreateSnapScroller());
        Grid.SetColumn(snapCard, 1);

        grid.Children.Add(basicCard);
        grid.Children.Add(snapCard);

        return grid;
    }

    private UIElement CreateBasicScroller()
    {
        var output = new TextBlock
        {
            Text = "Viewport diagnostics: not attached",
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            Opacity = 0.75
        };
        var scroller = new FWScroller
        {
            VerticalScrollMode = ScrollMode.Enabled,
            HorizontalScrollMode = ScrollMode.Disabled,
            Height = 200
        };

        var stack = CreateScrollerItemsStack();
        scroller.Content = stack;

        void UpdateDiagnostics(string reason)
        {
            var diagnostics = scroller.GetViewportDiagnostics();
            output.Text = FormatScrollerDiagnostics(reason, scroller, diagnostics);
        }

        scroller.ViewChanged += (_, _) => UpdateDiagnostics("Scrolled");
        UpdateDiagnostics("Viewport diagnostics");

        var scrollButton = new FWButton
        {
            Content = "Scroll to 80",
            HorizontalAlignment = HorizontalAlignment.Left
        };
        scrollButton.Click += (_, _) =>
        {
            scroller.ScrollTo(0, 80);
            UpdateDiagnostics("Scrolled");
        };

        return new StackPanel
        {
            Spacing = 8,
            Children =
            {
                scroller,
                output,
                scrollButton
            }
        };
    }

    private static StackPanel CreateScrollerItemsStack()
    {
        var stack = new StackPanel { Spacing = 8 };
        for (int i = 1; i <= 20; i++)
        {
            stack.Children.Add(new FWBorder
            {
                Padding = new Thickness(8),
                Child = new TextBlock { Text = $"Scrollable Item {i}" }
            });
        }

        return stack;
    }

    private UIElement CreateSnapScroller()
    {
        var scroller = new FWScroller
        {
            VerticalScrollMode = ScrollMode.Enabled,
            HorizontalScrollMode = ScrollMode.Disabled,
            VerticalSnapPointsType = SnapPointsType.Mandatory,
            Height = 200
        };

        var stack = new StackPanel { Spacing = 0 };
        for (int i = 1; i <= 10; i++)
        {
            stack.Children.Add(new FWBorder
            {
                Height = 60,
                Background = ThemeBrush(i % 2 == 0 ? "LayerFillColorDefaultBrush" : "ControlBackground"),
                BorderBrush = ThemeBrush("ControlBorder"),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Child = new TextBlock
                {
                    Text = $"Snap Item {i}",
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(12, 0, 0, 0)
                }
            });
        }

        scroller.Content = stack;
        var output = new TextBlock
        {
            Text = FormatScrollerDiagnostics("Snap viewport", scroller, scroller.GetViewportDiagnostics()),
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            Opacity = 0.75
        };
        var scrollButton = new FWButton
        {
            Content = "Snap to 180",
            HorizontalAlignment = HorizontalAlignment.Left
        };
        scrollButton.Click += (_, _) =>
        {
            scroller.ScrollTo(0, 180);
            output.Text = FormatScrollerDiagnostics("Snap requested", scroller, scroller.GetViewportDiagnostics());
        };

        return new StackPanel
        {
            Spacing = 8,
            Children =
            {
                scroller,
                output,
                scrollButton
            }
        };
    }

    private UIElement CreateAnnotatedScrollBarSection()
    {
        var border = new FWBorder
        {
            Background = ThemeBrush("ControlBackground"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(16),
            Height = 300
        };

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Hidden
        };

        var content = new StackPanel { Spacing = 8 };
        for (int i = 1; i <= 30; i++)
        {
            content.Children.Add(new FWBorder
            {
                Padding = new Thickness(8),
                Child = new TextBlock
                {
                    Text = $"Line {i}: Sample content for annotated scrollbar demo"
                }
            });
        }
        scrollViewer.Content = content;
        Grid.SetColumn(scrollViewer, 0);

        var annotatedScrollBar = new FWAnnotatedScrollBar
        {
            Orientation = Orientation.Vertical,
            Width = 16,
            Margin = new Thickness(8, 0, 0, 0),
            Labels = new List<ScrollBarLabel>
            {
                new ScrollBarLabel { ScrollOffset = 100, Content = "Important", Type = ScrollBarLabelType.Warning },
                new ScrollBarLabel { ScrollOffset = 250, Content = "Error", Type = ScrollBarLabelType.Error },
                new ScrollBarLabel { ScrollOffset = 400, Content = "Info", Type = ScrollBarLabelType.Info }
            }
        };
        var output = new TextBlock
        {
            Text = FormatAnnotatedScrollBarDiagnostics("Initial", annotatedScrollBar.GetDiagnostics()),
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            Opacity = 0.75,
            Margin = new Thickness(0, 8, 0, 0)
        };
        annotatedScrollBar.DetailLabelRequested += (_, args) =>
        {
            output.Text = FormatAnnotatedScrollBarDetail(args, annotatedScrollBar.GetDiagnostics());
        };
        Grid.SetColumn(annotatedScrollBar, 1);

        grid.Children.Add(scrollViewer);
        grid.Children.Add(annotatedScrollBar);
        border.Child = new StackPanel
        {
            Spacing = 8,
            Children =
            {
                grid,
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 8,
                    Children =
                    {
                        CreateAnnotationJumpButton("Warning", 100, annotatedScrollBar, output),
                        CreateAnnotationJumpButton("Error", 250, annotatedScrollBar, output),
                        CreateAnnotationJumpButton("Info", 400, annotatedScrollBar, output)
                    }
                },
                output
            }
        };

        return border;
    }

    internal static string FormatScrollerDiagnostics(string reason, FWScroller scroller, FWScrollerViewportDiagnostics diagnostics)
    {
        var viewport = diagnostics.HasScrollViewer
            ? $"{diagnostics.ViewportWidth:0}x{diagnostics.ViewportHeight:0}"
            : "template pending";
        var extent = diagnostics.HasScrollViewer
            ? $"{diagnostics.ExtentWidth:0}x{diagnostics.ExtentHeight:0}"
            : "template pending";

        return $"{reason}: offset {diagnostics.HorizontalOffset:0},{diagnostics.VerticalOffset:0}; viewport {viewport}; extent {extent}; zoom {diagnostics.ZoomFactor:0.##}; snap H/V {scroller.HorizontalSnapPointsType}/{scroller.VerticalSnapPointsType}; anchored H/V {FormatOnOff(scroller.IsAnchoredAtHorizontalExtent)}/{FormatOnOff(scroller.IsAnchoredAtVerticalExtent)}.";
    }

    internal static string FormatAnnotatedScrollBarDiagnostics(string reason, FWAnnotatedScrollBarDiagnostics diagnostics)
    {
        return $"{reason}: labels {diagnostics.RegisteredLabelCount}/{diagnostics.SourceLabelCount}; value {diagnostics.Value:0}/{diagnostics.Maximum:0}; viewport {diagnostics.ViewportSize:0}; orientation {diagnostics.Orientation}; canvas {FormatOnOff(diagnostics.HasDetailsCanvas)}; last {diagnostics.LastRequestedLabelType?.ToString() ?? "none"} at {diagnostics.LastRequestedScrollOffset:0}.";
    }

    internal static string FormatAnnotatedScrollBarDetail(
        DetailLabelRequestedEventArgs args,
        FWAnnotatedScrollBarDiagnostics diagnostics)
    {
        return $"{args.LabelType}: {args.Content} at {args.ScrollOffset:0}. {FormatAnnotatedScrollBarDiagnostics("Detail requested", diagnostics)}";
    }

    private static FWButton CreateAnnotationJumpButton(
        string label,
        double value,
        FWAnnotatedScrollBar annotatedScrollBar,
        TextBlock output)
    {
        var button = new FWButton
        {
            Content = label
        };
        button.Click += (_, _) =>
        {
            annotatedScrollBar.Value = value;
            output.Text = FormatAnnotatedScrollBarDiagnostics($"{label} marker", annotatedScrollBar.GetDiagnostics());
        };
        return button;
    }

    private Border CreateDemoCard(string title, UIElement content)
    {
        var stack = new StackPanel { Spacing = 12 };

        stack.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 16,
            FontWeight = FontWeights.Medium
        });

        stack.Children.Add(content);

        return new FWBorder
        {
            Background = ThemeBrush("ControlBackground"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(16),
            Child = stack
        };
    }

    private static Brush ThemeBrush(string key) => GalleryThemeResources.Brush(key);
}
