using System;
using System.Collections.Generic;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using Jalium.UI.Data;
using FluentJalium.Controls;

namespace FluentJalium.Gallery.Pages;

/// <summary>
/// Gallery page demonstrating Batch 16 advanced motion and animation controls.
/// </summary>
public class MotionControlsPage : Page
{
    public MotionControlsPage()
    {
        Title = "Motion & Animation";
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Padding = new Thickness(24)
        };

        var mainStack = new StackPanel
        {
            Spacing = 32
        };

        // FWAnimatedIcon Section
        mainStack.Children.Add(CreateSectionHeader("FWAnimatedIcon",
            "State-driven animated icons with smooth transitions"));
        mainStack.Children.Add(CreateAnimatedIconSection());

        // FWAnimatedVisualPlayer Section
        mainStack.Children.Add(CreateSectionHeader("FWAnimatedVisualPlayer",
            "Lottie-style vector animation player with playback controls"));
        mainStack.Children.Add(CreateAnimatedVisualPlayerSection());

        scrollViewer.Content = mainStack;
        Content = scrollViewer;
    }

    private UIElement CreateSectionHeader(string title, string description)
    {
        var stack = new StackPanel { Spacing = 8 };

        var titleBlock = new TextBlock
        {
            Text = title,
            FontSize = 24,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 4)
        };

        var descBlock = new TextBlock
        {
            Text = description,
            FontSize = 14,
            Opacity = 0.7
        };

        stack.Children.Add(titleBlock);
        stack.Children.Add(descBlock);

        return stack;
    }

    private UIElement CreateAnimatedIconSection()
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            },
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto }
            },
            ColumnSpacing = 16
        };

        // Example 1: Auto-play icon
        var card1 = CreateDemoCard(
            "Auto-play",
            CreateAnimatedIconDemo(true, "Normal")
        );
        Grid.SetColumn(card1, 0);

        // Example 2: Manual control icon
        var card2 = CreateDemoCard(
            "Manual Control",
            CreateAnimatedIconDemo(false, "Hover")
        );
        Grid.SetColumn(card2, 1);

        // Example 3: RTL mirrored icon
        var card3 = CreateDemoCard(
            "RTL Mirrored",
            CreateAnimatedIconDemo(true, "Normal", true)
        );
        Grid.SetColumn(card3, 2);

        grid.Children.Add(card1);
        grid.Children.Add(card2);
        grid.Children.Add(card3);

        return grid;
    }

    private UIElement CreateAnimatedIconDemo(bool autoPlay, string state, bool mirrored = false)
    {
        var icon = new FWAnimatedIcon
        {
            Width = 48,
            Height = 48,
            AutoPlay = autoPlay,
            State = state,
            MirroredWhenRightToLeft = mirrored,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            FallbackIconSource = "" // Play icon fallback
        };

        return icon;
    }

    private UIElement CreateAnimatedVisualPlayerSection()
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            },
            ColumnSpacing = 24
        };

        // Player preview
        var playerCard = CreateDemoCard(
            "Animation Player",
            CreateAnimatedPlayerDemo()
        );
        Grid.SetColumn(playerCard, 0);

        // Controls
        var controlsCard = CreateDemoCard(
            "Playback Controls",
            CreatePlayerControlsDemo()
        );
        Grid.SetColumn(controlsCard, 1);

        grid.Children.Add(playerCard);
        grid.Children.Add(controlsCard);

        return grid;
    }

    private UIElement CreateAnimatedPlayerDemo()
    {
        var player = new FWAnimatedVisualPlayer
        {
            Width = 200,
            Height = 200,
            AutoPlay = true,
            IsLooping = true,
            PlaybackRate = 1.0,
            Stretch = Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        return player;
    }

    private UIElement CreatePlayerControlsDemo()
    {
        var stack = new StackPanel { Spacing = 12 };

        var playButton = new Button
        {
            Content = "Play",
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        var pauseButton = new Button
        {
            Content = "Pause",
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        var stopButton = new Button
        {
            Content = "Stop",
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        var rateSlider = new StackPanel { Spacing = 4 };
        rateSlider.Children.Add(new TextBlock { Text = "Playback Rate" });
        rateSlider.Children.Add(new Slider
        {
            Minimum = 0.1,
            Maximum = 5.0,
            Value = 1.0,
            TickFrequency = 0.5,
            IsSnapToTickEnabled = true
        });

        var loopingToggle = new CheckBox
        {
            Content = "Loop Animation",
            IsChecked = true
        };

        stack.Children.Add(playButton);
        stack.Children.Add(pauseButton);
        stack.Children.Add(stopButton);
        stack.Children.Add(rateSlider);
        stack.Children.Add(loopingToggle);

        return stack;
    }

    private Border CreateDemoCard(string title, UIElement content)
    {
        var stack = new StackPanel { Spacing = 12 };

        var titleBlock = new TextBlock
        {
            Text = title,
            FontSize = 16,
            FontWeight = FontWeights.Medium
        };

        stack.Children.Add(titleBlock);
        stack.Children.Add(content);

        return new Border
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
