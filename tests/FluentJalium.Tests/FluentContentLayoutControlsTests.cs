using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Themes;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Media.Animation;
using AnimationDuration = Jalium.UI.Media.Animation.Duration;
using JaliumThemeManager = Jalium.UI.Controls.Themes.ThemeManager;

namespace FluentJalium.Tests;

[Collection("Application")]
public sealed class FluentContentLayoutControlsTests
{
    [Fact]
    public void FWContentLayoutControls_ShouldComposeInsideLiquidGlassSurface()
    {
        var foreground = new SolidColorBrush(Color.FromRgb(0x4C, 0xC2, 0xFF));
        var textBlock = new FWTextBlock
        {
            Text = "Selectable Fluent text",
            Foreground = foreground,
            IsTextSelectionEnabled = true,
            TextWrapping = TextWrapping.Wrap,
            TextTrimming = TextTrimming.CharacterEllipsis,
            FontSize = 18
        };
        var accessText = new FWAccessText
        {
            Text = "_Open command",
            Foreground = foreground
        };
        var contentControl = new FWContentControl
        {
            Content = textBlock,
            Padding = new Thickness(8)
        };
        var presenterChild = new FWTextBlock
        {
            Text = "Presented content"
        };
        var contentPresenter = new FWContentPresenter
        {
            Content = presenterChild
        };
        var border = new FWBorder
        {
            Background = new SolidColorBrush(Color.FromRgb(0x20, 0x20, 0x20)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x44)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12),
            Child = contentControl
        };
        var stack = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Children =
            {
                border,
                accessText,
                contentPresenter
            }
        };
        var wrap = new FWWrapPanel
        {
            HorizontalSpacing = 6,
            VerticalSpacing = 4,
            Children =
            {
                new FWBorder(),
                new FWBorder(),
                new FWBorder()
            }
        };
        var grid = new FWGrid
        {
            RowSpacing = 8,
            ColumnSpacing = 10
        };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        grid.Children.Add(wrap);
        var transitionHost = new FWTransitioningContentControl
        {
            TransitionMode = TransitionMode.LiquidMorph,
            Content = grid
        };
        var splitView = new FWSplitView
        {
            Pane = stack,
            Content = transitionHost,
            DisplayMode = FWSplitViewDisplayMode.CompactInline,
            PanePlacement = FWSplitViewPanePlacement.Right,
            OpenPaneLength = 300,
            CompactPaneLength = 56,
            IsPaneOpen = false,
            IsLightDismissEnabled = false
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
            Child = splitView
        };

        Assert.Equal("Selectable Fluent text", textBlock.Text);
        Assert.Same(foreground, textBlock.Foreground);
        Assert.True(textBlock.IsTextSelectionEnabled);
        Assert.Equal(TextWrapping.Wrap, textBlock.TextWrapping);
        Assert.Equal(TextTrimming.CharacterEllipsis, textBlock.TextTrimming);
        Assert.Equal(18, textBlock.FontSize);
        Assert.Equal('O', accessText.AccessKey);
        Assert.Same(textBlock, contentControl.Content);
        Assert.Equal(8, contentControl.Padding.Left);
        Assert.Same(presenterChild, contentPresenter.Content);
        Assert.Same(contentControl, border.Child);
        Assert.Equal(1, border.BorderThickness.Left);
        Assert.Equal(6, border.CornerRadius.TopLeft);
        Assert.Equal(12, border.Padding.Left);
        Assert.Equal(Orientation.Vertical, stack.Orientation);
        Assert.Equal(12, stack.Spacing);
        Assert.Equal(3, stack.Children.Count);
        Assert.Equal(6, wrap.HorizontalSpacing);
        Assert.Equal(4, wrap.VerticalSpacing);
        Assert.Equal(3, wrap.Children.Count);
        Assert.Equal(8, grid.RowSpacing);
        Assert.Equal(10, grid.ColumnSpacing);
        Assert.Single(grid.RowDefinitions);
        Assert.Single(grid.ColumnDefinitions);
        Assert.Single(grid.Children);
        Assert.Same(wrap, grid.Children[0]);
        Assert.Equal(TransitionMode.LiquidMorph, transitionHost.TransitionMode);
        Assert.Same(grid, transitionHost.Content);
        Assert.Same(stack, splitView.Pane);
        Assert.Same(transitionHost, splitView.Content);
        Assert.Equal(FWSplitViewDisplayMode.CompactInline, splitView.DisplayMode);
        Assert.Equal(FWSplitViewPanePlacement.Right, splitView.PanePlacement);
        Assert.Equal(56, splitView.ActualPaneLength);
        Assert.False(splitView.IsOverlayMode);
        Assert.False(splitView.IsLightDismissEnabled);
        splitView.OpenPane();
        Assert.True(splitView.IsPaneOpen);
        Assert.Equal(300, splitView.ActualPaneLength);
        splitView.TogglePane();
        Assert.False(splitView.IsPaneOpen);
        Assert.Equal(56, splitView.ActualPaneLength);

        var inlineSplitView = new FWSplitView
        {
            DisplayMode = FWSplitViewDisplayMode.Inline,
            OpenPaneLength = 240,
            CompactPaneLength = 52
        };
        Assert.Equal(240, inlineSplitView.ActualPaneLength);
        inlineSplitView.ClosePane();
        Assert.Equal(0, inlineSplitView.ActualPaneLength);
        inlineSplitView.DisplayMode = FWSplitViewDisplayMode.CompactOverlay;
        Assert.Equal(52, inlineSplitView.ActualPaneLength);
        inlineSplitView.OpenPane();
        Assert.Equal(240, inlineSplitView.ActualPaneLength);
        inlineSplitView.DisplayMode = FWSplitViewDisplayMode.Overlay;
        inlineSplitView.ClosePane();
        Assert.Equal(0, inlineSplitView.ActualPaneLength);

        Assert.Equal(FWFluentMaterialKind.LiquidGlass, surface.MaterialKind);
        Assert.True(surface.LiquidGlass);
        Assert.Equal(70, surface.RefractionAmount);
        Assert.Equal(0.42, surface.ChromaticAberration);
        Assert.Equal(24, surface.FusionRadius);
        Assert.Equal(BorderShape.SuperEllipse, surface.Shape);
        Assert.Equal(4, surface.SuperEllipseN);
        Assert.Same(splitView, surface.Child);
    }

    [Fact]
    public void ContentTransitionRecipe_ShouldExposeFluentMotionProfiles()
    {
        var defaultRecipe = FWContentTransitionRecipe.Create(FWContentTransitionProfile.Default);
        var entranceRecipe = FWContentTransitionRecipe.Create(FWContentTransitionProfile.Entrance);
        var drillInRecipe = FWContentTransitionRecipe.Create(FWContentTransitionProfile.DrillIn);
        var backRecipe = FWContentTransitionRecipe.Create(FWContentTransitionProfile.BackNavigation);
        var liquidRecipe = FWContentTransitionRecipe.Create(FWContentTransitionProfile.LiquidMorph);
        var suppressRecipe = FWContentTransitionRecipe.Create(FWContentTransitionProfile.Suppress);

        Assert.Equal(TransitionMode.Crossfade, defaultRecipe.TransitionMode);
        Assert.Equal(new AnimationDuration(TimeSpan.FromMilliseconds(280)), defaultRecipe.Duration);
        Assert.Equal(TransitionTimingFunction.Recommended, defaultRecipe.TimingFunction);
        Assert.Equal(TransitionMode.SlideLeft, entranceRecipe.TransitionMode);
        Assert.Equal(new AnimationDuration(TimeSpan.FromMilliseconds(320)), entranceRecipe.Duration);
        Assert.Equal(TransitionTimingFunction.EaseOut, entranceRecipe.TimingFunction);
        Assert.Equal(TransitionMode.ZoomIn, drillInRecipe.TransitionMode);
        Assert.Equal(new AnimationDuration(TimeSpan.FromMilliseconds(360)), drillInRecipe.Duration);
        Assert.Equal(TransitionTimingFunction.EaseInOut, drillInRecipe.TimingFunction);
        Assert.Equal(TransitionMode.SlideRight, backRecipe.TransitionMode);
        Assert.Equal(new AnimationDuration(TimeSpan.FromMilliseconds(280)), backRecipe.Duration);
        Assert.Equal(TransitionMode.LiquidMorph, liquidRecipe.TransitionMode);
        Assert.Equal(new AnimationDuration(TimeSpan.FromMilliseconds(420)), liquidRecipe.Duration);
        Assert.Null(suppressRecipe.TransitionMode);
        Assert.Equal(new AnimationDuration(TimeSpan.Zero), suppressRecipe.Duration);
        Assert.Equal(TransitionTimingFunction.Linear, suppressRecipe.TimingFunction);
        Assert.Throws<ArgumentOutOfRangeException>(() => FWContentTransitionRecipe.Create((FWContentTransitionProfile)42));
    }

    [Fact]
    public void ContentTransitionRecipe_ShouldCreateProfilesFromMotionResources()
    {
        var resources = new ResourceDictionary
        {
            ["FluentMotionContentTransitionEntranceMode"] = "SlideUp",
            ["FluentMotionContentTransitionEntranceDuration"] = new AnimationDuration(TimeSpan.FromMilliseconds(500)),
            ["FluentMotionContentTransitionEntranceTimingFunction"] = "EaseInOut",
            ["FluentMotionContentTransitionSuppressMode"] = "Suppress",
            ["FluentMotionContentTransitionSuppressDuration"] = 0,
            ["FluentMotionContentTransitionSuppressTimingFunction"] = TransitionTimingFunction.Linear
        };

        var entranceRecipe = FWContentTransitionRecipe.Create(FWContentTransitionProfile.Entrance, resources);
        var suppressRecipe = FWContentTransitionRecipe.Create(FWContentTransitionProfile.Suppress, resources);

        Assert.Equal(TransitionMode.SlideUp, entranceRecipe.TransitionMode);
        Assert.Equal(new AnimationDuration(TimeSpan.FromMilliseconds(500)), entranceRecipe.Duration);
        Assert.Equal(TransitionTimingFunction.EaseInOut, entranceRecipe.TimingFunction);
        Assert.Null(suppressRecipe.TransitionMode);
        Assert.Equal(new AnimationDuration(TimeSpan.Zero), suppressRecipe.Duration);
        Assert.Equal(TransitionTimingFunction.Linear, suppressRecipe.TimingFunction);
    }

    [Fact]
    public void FWTransitioningContentControl_ShouldApplyFluentTransitionProfiles()
    {
        var transitionHost = new FWTransitioningContentControl();

        Assert.Equal(FWContentTransitionProfile.Default, transitionHost.TransitionProfile);
        Assert.Equal(TransitionMode.Crossfade, transitionHost.TransitionMode);
        Assert.Equal(new AnimationDuration(TimeSpan.FromMilliseconds(280)), transitionHost.TransitionDuration);
        Assert.Equal(TransitionTimingFunction.Recommended, transitionHost.TransitionTimingFunction);

        transitionHost.TransitionProfile = FWContentTransitionProfile.Entrance;

        Assert.Equal(TransitionMode.SlideLeft, transitionHost.TransitionMode);
        Assert.Equal(new AnimationDuration(TimeSpan.FromMilliseconds(320)), transitionHost.TransitionDuration);
        Assert.Equal(TransitionTimingFunction.EaseOut, transitionHost.TransitionTimingFunction);

        transitionHost.TransitionProfile = FWContentTransitionProfile.LiquidMorph;

        Assert.Equal(TransitionMode.LiquidMorph, transitionHost.TransitionMode);
        Assert.Equal(new AnimationDuration(TimeSpan.FromMilliseconds(420)), transitionHost.TransitionDuration);
        Assert.Equal(TransitionTimingFunction.EaseInOut, transitionHost.TransitionTimingFunction);

        transitionHost.TransitionProfile = FWContentTransitionProfile.Suppress;

        Assert.Null(transitionHost.TransitionMode);
        Assert.Equal(new AnimationDuration(TimeSpan.Zero), transitionHost.TransitionDuration);
        Assert.Equal(TransitionTimingFunction.Linear, transitionHost.TransitionTimingFunction);
        Assert.ThrowsAny<ArgumentException>(() => transitionHost.TransitionProfile = (FWContentTransitionProfile)42);
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void GenericTheme_ShouldDefineSplitViewStyle()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = FluentThemeVariant.Dark.ToString();

        var dictionary = LoadGenericThemeDictionary();

        var splitViewStyle = AssertStyle<FWSplitView>(dictionary);
        Assert.Equal(typeof(ContentControl), splitViewStyle.BasedOn?.TargetType);
        AssertSetter(splitViewStyle, FWSplitView.DisplayModeProperty);
        AssertSetter(splitViewStyle, FWSplitView.PanePlacementProperty);
        AssertSetter(splitViewStyle, FWSplitView.OpenPaneLengthProperty);
        AssertSetter(splitViewStyle, FWSplitView.CompactPaneLengthProperty);
        AssertSetter(splitViewStyle, FWSplitView.PaneBackgroundProperty);
        AssertSetter(splitViewStyle, FWSplitView.ContentBackgroundProperty);

        ResetApplicationState();
    }

    [Fact]
    public void FWTransitioningContentControl_ShouldApplyExplicitTransitionRecipe()
    {
        var transitionHost = new FWTransitioningContentControl();
        var recipe = new FWContentTransitionRecipe(
            FWContentTransitionProfile.DrillIn,
            TransitionMode.WaveDistortion,
            new AnimationDuration(TimeSpan.FromMilliseconds(640)),
            TransitionTimingFunction.EaseInOut);

        transitionHost.ApplyTransitionRecipe(recipe);

        Assert.Equal(FWContentTransitionProfile.DrillIn, transitionHost.TransitionProfile);
        Assert.Equal(TransitionMode.WaveDistortion, transitionHost.TransitionMode);
        Assert.Equal(new AnimationDuration(TimeSpan.FromMilliseconds(640)), transitionHost.TransitionDuration);
        Assert.Equal(TransitionTimingFunction.EaseInOut, transitionHost.TransitionTimingFunction);
    }

    private static ResourceDictionary LoadGenericThemeDictionary()
    {
        var loaded = ResourceDictionary.SourceLoader?.Invoke(
            new ResourceDictionary(),
            new Uri("/FluentJalium;component/Themes/Generic.jalxaml", UriKind.Relative),
            FluentThemeManager.ThemeAssembly);

        return Assert.IsType<ResourceDictionary>(loaded);
    }

    private static Style AssertStyle<TControl>(ResourceDictionary dictionary)
        where TControl : FrameworkElement
    {
        Assert.True(dictionary.TryGetValue(typeof(TControl), out var value), $"{typeof(TControl).Name} style was not found.");
        return Assert.IsType<Style>(value);
    }

    private static void AssertSetter(Style style, DependencyProperty property)
    {
        Assert.Contains(style.Setters, setter => setter.Property == property);
    }

    private static void ResetApplicationState()
    {
        var currentField = typeof(Application).GetField("_current", BindingFlags.NonPublic | BindingFlags.Static);
        currentField?.SetValue(null, null);

        var jaliumReset = typeof(JaliumThemeManager).GetMethod("Reset", BindingFlags.NonPublic | BindingFlags.Static);
        jaliumReset?.Invoke(null, null);

        var fluentReset = typeof(FluentThemeManager).GetMethod("Reset", BindingFlags.NonPublic | BindingFlags.Static);
        fluentReset?.Invoke(null, null);
    }
}
