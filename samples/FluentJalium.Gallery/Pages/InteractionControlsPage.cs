using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using FluentJalium.Controls;
using FWBorder = FluentJalium.Controls.FWBorder;

namespace FluentJalium.Gallery.Pages;

/// <summary>
/// Gallery page demonstrating advanced interaction controls.
/// </summary>
public class InteractionControlsPage : Page
{
    private TextBlock? _refreshStatusText;
    private int _refreshCount = 0;

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
            Background = new SolidColorBrush(Color.FromRgb(0xF9, 0xF9, 0xF9)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0)),
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

        // Add sample content
        for (int i = 1; i <= 10; i++)
        {
            contentStack.Children.Add(new FWBorder
            {
                Padding = new Thickness(8),
                Background = new SolidColorBrush(Colors.White),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xD0, 0xD0, 0xD0)),
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

        Task.Run(async () =>
        {
            // Simulate async refresh operation
            await Task.Delay(2000);

            Dispatcher.Invoke(() =>
            {
                _refreshCount++;
                if (_refreshStatusText != null)
                {
                    _refreshStatusText.Text = $"Refreshed {_refreshCount} times";
                }
                deferral.Complete();
            });
        });
    }

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
        var scroller = new FWScroller
        {
            VerticalScrollMode = ScrollMode.Enabled,
            HorizontalScrollMode = ScrollMode.Disabled,
            Height = 200
        };

        var stack = new StackPanel { Spacing = 8 };
        for (int i = 1; i <= 20; i++)
        {
            stack.Children.Add(new FWBorder
            {
                Padding = new Thickness(8),
                Child = new TextBlock { Text = $"Scrollable Item {i}" }
            });
        }

        scroller.Content = stack;
        return scroller;
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
                Background = i % 2 == 0
                    ? new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xF0))
                    : new SolidColorBrush(Colors.White),
                Child = new TextBlock
                {
                    Text = $"Snap Item {i}",
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(12, 0, 0, 0)
                }
            });
        }

        scroller.Content = stack;
        return scroller;
    }

    private UIElement CreateAnnotatedScrollBarSection()
    {
        var border = new FWBorder
        {
            Background = new SolidColorBrush(Color.FromRgb(0xF9, 0xF9, 0xF9)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0)),
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
        Grid.SetColumn(annotatedScrollBar, 1);

        grid.Children.Add(scrollViewer);
        grid.Children.Add(annotatedScrollBar);
        border.Child = grid;

        return border;
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
            Background = new SolidColorBrush(Color.FromRgb(0xF9, 0xF9, 0xF9)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(16),
            Child = stack
        };
    }
}
