using System;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using FluentJalium.Controls;
using FWBorder = FluentJalium.Controls.FWBorder;

namespace FluentJalium.Gallery.Pages;

/// <summary>
/// Gallery page demonstrating material and backdrop effects.
/// </summary>
public class MaterialsPage : Page
{
    public MaterialsPage()
    {
        Title = "Materials & Effects";
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Padding = new Thickness(24)
        };

        var mainStack = new StackPanel { Spacing = 32 };

        // FWBackdrop Section
        mainStack.Children.Add(CreateSectionHeader("FWBackdrop",
            "Background material effects: Acrylic, Mica, MicaAlt, and Tabbed"));
        mainStack.Children.Add(CreateBackdropSection());

        // FWAcrylicBrush Section
        mainStack.Children.Add(CreateSectionHeader("FWAcrylicBrush",
            "Reusable acrylic brush for semi-transparent blur effects"));
        mainStack.Children.Add(CreateAcrylicBrushSection());

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

    private UIElement CreateBackdropSection()
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            },
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
            },
            ColumnSpacing = 16,
            RowSpacing = 16
        };

        // Acrylic
        var acrylicCard = CreateBackdropDemo("Acrylic", FWBackdropType.Acrylic,
            "Semi-transparent with blur effect");
        Grid.SetColumn(acrylicCard, 0);
        Grid.SetRow(acrylicCard, 0);

        // Mica
        var micaCard = CreateBackdropDemo("Mica", FWBackdropType.Mica,
            "Subtle texture material");
        Grid.SetColumn(micaCard, 1);
        Grid.SetRow(micaCard, 0);

        // MicaAlt
        var micaAltCard = CreateBackdropDemo("Mica Alt", FWBackdropType.MicaAlt,
            "Darker variant for contrast");
        Grid.SetColumn(micaAltCard, 0);
        Grid.SetRow(micaAltCard, 1);

        // Tabbed
        var tabbedCard = CreateBackdropDemo("Tabbed", FWBackdropType.Tabbed,
            "Optimized for tabbed interfaces");
        Grid.SetColumn(tabbedCard, 1);
        Grid.SetRow(tabbedCard, 1);

        grid.Children.Add(acrylicCard);
        grid.Children.Add(micaCard);
        grid.Children.Add(micaAltCard);
        grid.Children.Add(tabbedCard);

        return grid;
    }

    private Border CreateBackdropDemo(string title, FWBackdropType type, string description)
    {
        var container = new FWBorder
        {
            Height = 200,
            BorderBrush = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            ClipToBounds = true
        };

        var grid = new Grid();

        // Backdrop layer
        var backdrop = new FWBackdrop
        {
            Type = type,
            TintColor = Color.FromRgb(0xF3, 0xF3, 0xF3),
            TintOpacity = 0.8,
            LuminosityOpacity = 0.85
        };
        grid.Children.Add(backdrop);

        // Content layer
        var contentStack = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Spacing = 8
        };

        contentStack.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 20,
            FontWeight = FontWeights.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Center
        });

        contentStack.Children.Add(new TextBlock
        {
            Text = description,
            FontSize = 14,
            Opacity = 0.7,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center
        });

        grid.Children.Add(contentStack);
        container.Child = grid;

        return container;
    }

    private UIElement CreateAcrylicBrushSection()
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            },
            ColumnSpacing = 16
        };

        // Light tint
        var lightCard = CreateAcrylicBrushDemo("Light Tint",
            Color.FromRgb(0xF3, 0xF3, 0xF3), 0.7);
        Grid.SetColumn(lightCard, 0);

        // Medium tint
        var mediumCard = CreateAcrylicBrushDemo("Medium Tint",
            Color.FromRgb(0xCC, 0xCC, 0xCC), 0.8);
        Grid.SetColumn(mediumCard, 1);

        // Accent tint
        var accentCard = CreateAcrylicBrushDemo("Accent Tint",
            Color.FromRgb(0x00, 0x78, 0xD4), 0.6);
        Grid.SetColumn(accentCard, 2);

        grid.Children.Add(lightCard);
        grid.Children.Add(mediumCard);
        grid.Children.Add(accentCard);

        return grid;
    }

    private Border CreateAcrylicBrushDemo(string title, Color tintColor, double opacity)
    {
        var brush = new FWAcrylicBrush
        {
            TintColor = tintColor,
            TintOpacity = opacity,
            BackgroundSource = AcrylicBackgroundSource.Backdrop
        };

        var border = new FWBorder
        {
            Height = 150,
            Background = brush.CreateBrush(), // Use CreateBrush() method
            BorderBrush = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8)
        };

        var stack = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Spacing = 8
        };

        stack.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 16,
            FontWeight = FontWeights.Medium,
            HorizontalAlignment = HorizontalAlignment.Center
        });

        stack.Children.Add(new TextBlock
        {
            Text = $"Opacity: {opacity:P0}",
            FontSize = 12,
            Opacity = 0.8,
            HorizontalAlignment = HorizontalAlignment.Center
        });

        border.Child = stack;
        return border;
    }
}
