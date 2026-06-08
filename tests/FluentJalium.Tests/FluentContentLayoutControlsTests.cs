using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Windows.Input;
using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
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
        var canvasChild = new FWBorder
        {
            Width = 24,
            Height = 24
        };
        var canvas = new FWCanvas
        {
            Children =
            {
                canvasChild
            }
        };
        Canvas.SetLeft(canvasChild, 16);
        Canvas.SetTop(canvasChild, 20);
        var relativePanel = new FWRelativePanel
        {
            RowSpacing = 4,
            ColumnSpacing = 4,
            Children =
            {
                canvas
            }
        };
        var twoPaneView = new FWTwoPaneView
        {
            Pane1 = relativePanel,
            Pane2 = transitionHost,
            Mode = FWTwoPaneViewMode.Tall,
            PanePriority = FWTwoPaneViewPriority.Pane2,
            MinWideModeWidth = 720,
            MinTallModeHeight = 560
        };
        var parallaxView = new FWParallaxView
        {
            Content = twoPaneView,
            Source = grid,
            HorizontalShift = 12,
            VerticalShift = 48,
            StartOffset = 0.25,
            EndOffset = 0.75,
            IsHorizontalShiftEnabled = true,
            Progress = 0.5
        };
        var settingsCard = new FWSettingsCard
        {
            Header = "Use adaptive panes",
            Description = "Switches between wide, tall, and single-pane layouts.",
            HeaderIcon = new FWFontIcon(),
            ActionIcon = new FWFontIcon(),
            Content = new FWToggleSwitch(),
            IsClickEnabled = true
        };
        var splitView = new FWSplitView
        {
            Pane = stack,
            Content = parallaxView,
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
        Assert.IsAssignableFrom<IFluentJaliumControl>(canvas);
        Assert.Single(canvas.Children);
        Assert.Equal(16, Canvas.GetLeft(canvasChild));
        Assert.Equal(20, Canvas.GetTop(canvasChild));
        Assert.Same(canvas, relativePanel.Children[0]);
        Assert.Equal(4, relativePanel.RowSpacing);
        Assert.Equal(4, relativePanel.ColumnSpacing);
        Assert.Same(relativePanel, twoPaneView.Pane1);
        Assert.Same(transitionHost, twoPaneView.Pane2);
        Assert.Equal(FWTwoPaneViewMode.Tall, twoPaneView.Mode);
        Assert.Equal(FWTwoPaneViewMode.Tall, twoPaneView.ActualMode);
        Assert.Equal(FWTwoPaneViewPriority.Pane2, twoPaneView.PanePriority);
        Assert.Equal(FWTwoPaneViewVisiblePane.Both, twoPaneView.VisiblePane);
        Assert.Same(transitionHost, twoPaneView.ActivePane);
        Assert.Equal(720, twoPaneView.MinWideModeWidth);
        Assert.Equal(560, twoPaneView.MinTallModeHeight);
        Assert.Same(twoPaneView, parallaxView.Content);
        Assert.Same(grid, parallaxView.Source);
        Assert.Equal(new Point(6, 24), parallaxView.GetParallaxOffset(0.5));
        Assert.Equal(new Point(6, 24), parallaxView.CurrentOffset);
        Assert.Equal("Use adaptive panes", settingsCard.Header);
        Assert.True(settingsCard.IsClickEnabled);
        Assert.IsType<FWToggleSwitch>(settingsCard.Content);
        Assert.Same(stack, splitView.Pane);
        Assert.Same(parallaxView, splitView.Content);
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
    public void FWTwoPaneView_ShouldExposeActualModeVisiblePaneAndDiagnostics()
    {
        var pane1 = new FWTextBlock { Text = "Master" };
        var pane2 = new FWTextBlock { Text = "Detail" };
        var twoPaneView = new FWTwoPaneView
        {
            Pane1 = pane1,
            Pane2 = pane2,
            Mode = FWTwoPaneViewMode.Wide,
            PanePriority = FWTwoPaneViewPriority.Pane1,
            MinWideModeWidth = 720,
            MinTallModeHeight = 560
        };

        Assert.Equal(FWTwoPaneViewMode.Wide, twoPaneView.ActualMode);
        Assert.Equal(FWTwoPaneViewVisiblePane.Both, twoPaneView.VisiblePane);
        Assert.Same(pane1, twoPaneView.ActivePane);

        twoPaneView.PanePriority = FWTwoPaneViewPriority.Pane2;

        Assert.Equal(FWTwoPaneViewVisiblePane.Both, twoPaneView.VisiblePane);
        Assert.Same(pane2, twoPaneView.ActivePane);

        twoPaneView.Mode = FWTwoPaneViewMode.SinglePane;

        var pane2Diagnostics = twoPaneView.GetDiagnostics();

        Assert.Equal(FWTwoPaneViewMode.SinglePane, twoPaneView.ActualMode);
        Assert.Equal(FWTwoPaneViewVisiblePane.Pane2, twoPaneView.VisiblePane);
        Assert.Same(pane2, twoPaneView.ActivePane);
        Assert.Equal(FWTwoPaneViewMode.SinglePane, pane2Diagnostics.RequestedMode);
        Assert.Equal(FWTwoPaneViewMode.SinglePane, pane2Diagnostics.ActualMode);
        Assert.Equal(FWTwoPaneViewPriority.Pane2, pane2Diagnostics.PanePriority);
        Assert.Equal(FWTwoPaneViewVisiblePane.Pane2, pane2Diagnostics.VisiblePane);
        Assert.False(pane2Diagnostics.ShowsPane1);
        Assert.True(pane2Diagnostics.ShowsPane2);
        Assert.Same(pane2, pane2Diagnostics.ActivePane);
        Assert.Equal(720, pane2Diagnostics.MinWideModeWidth);
        Assert.Equal(560, pane2Diagnostics.MinTallModeHeight);

        twoPaneView.PanePriority = FWTwoPaneViewPriority.Pane1;

        var pane1Diagnostics = twoPaneView.GetDiagnostics();

        Assert.Equal(FWTwoPaneViewVisiblePane.Pane1, twoPaneView.VisiblePane);
        Assert.Same(pane1, twoPaneView.ActivePane);
        Assert.True(pane1Diagnostics.ShowsPane1);
        Assert.False(pane1Diagnostics.ShowsPane2);

        twoPaneView.Mode = FWTwoPaneViewMode.Wide;
        twoPaneView.Measure(new Size(600, 480));

        var narrowDiagnostics = twoPaneView.GetDiagnostics();

        Assert.Equal(FWTwoPaneViewMode.Wide, narrowDiagnostics.RequestedMode);
        Assert.Equal(FWTwoPaneViewMode.SinglePane, narrowDiagnostics.ActualMode);
        Assert.Equal(FWTwoPaneViewVisiblePane.Pane1, narrowDiagnostics.VisiblePane);

        twoPaneView.Measure(new Size(800, 480));

        Assert.Equal(FWTwoPaneViewMode.Wide, twoPaneView.ActualMode);
        Assert.Equal(FWTwoPaneViewVisiblePane.Both, twoPaneView.VisiblePane);

        twoPaneView.Mode = FWTwoPaneViewMode.Tall;
        twoPaneView.Measure(new Size(800, 480));

        Assert.Equal(FWTwoPaneViewMode.SinglePane, twoPaneView.ActualMode);
    }

    [Fact]
    public void FWParallaxView_ShouldExposeProgressCurrentOffsetAndDiagnostics()
    {
        var source = new FWGrid();
        var parallaxView = new FWParallaxView
        {
            Source = source,
            HorizontalShift = 20,
            VerticalShift = 40,
            StartOffset = 0.25,
            EndOffset = 0.75,
            IsHorizontalShiftEnabled = true,
            IsVerticalShiftEnabled = true
        };

        Assert.Equal(0, parallaxView.Progress);
        Assert.Equal(new Point(5, 10), parallaxView.CurrentOffset);

        parallaxView.Progress = 0.5;

        Assert.Equal(0.5, parallaxView.Progress);
        Assert.Equal(new Point(10, 20), parallaxView.CurrentOffset);
        Assert.Equal(new Point(10, 20), parallaxView.GetParallaxOffset(0.5));

        parallaxView.Progress = 2;

        Assert.Equal(1, parallaxView.Progress);
        Assert.Equal(new Point(15, 30), parallaxView.CurrentOffset);

        parallaxView.IsHorizontalShiftEnabled = false;

        var diagnostics = parallaxView.GetDiagnostics();

        Assert.True(diagnostics.HasSource);
        Assert.Equal(1, diagnostics.Progress);
        Assert.Equal(new Point(0, 30), diagnostics.CurrentOffset);
        Assert.Equal(20, diagnostics.HorizontalShift);
        Assert.Equal(40, diagnostics.VerticalShift);
        Assert.Equal(0.25, diagnostics.StartOffset);
        Assert.Equal(0.75, diagnostics.EndOffset);
        Assert.False(diagnostics.IsHorizontalShiftEnabled);
        Assert.True(diagnostics.IsVerticalShiftEnabled);
    }

    [Fact]
    public void FWSettingsCard_ShouldInvokeClickAndCommandWhenEnabled()
    {
        var command = new RecordingCommand();
        var card = new FWSettingsCard
        {
            Header = "Window material",
            Description = "Open a detail surface.",
            Command = command,
            CommandParameter = "material",
            IsClickEnabled = true
        };
        var clickCount = 0;
        card.Click += (_, _) => clickCount++;

        Assert.True(card.Focusable);
        Assert.Equal(ClickMode.Release, card.ClickMode);
        Assert.True(card.CanExecute);

        Assert.True(card.Invoke());

        Assert.Equal(1, clickCount);
        Assert.Equal(1, command.ExecuteCount);
        Assert.Equal("material", command.LastParameter);

        card.IsClickEnabled = false;

        Assert.False(card.Focusable);
        Assert.False(card.Invoke());
        Assert.Equal(1, clickCount);
        Assert.Equal(1, command.ExecuteCount);

        card.IsClickEnabled = true;
        command.CanExecuteResult = false;
        command.RaiseCanExecuteChanged();

        Assert.False(card.CanExecute);
        Assert.False(card.IsEnabled);
        Assert.False(card.Invoke());
        Assert.Equal(1, clickCount);
        Assert.Equal(1, command.ExecuteCount);

        command.CanExecuteResult = true;
        command.RaiseCanExecuteChanged();

        Assert.True(card.CanExecute);
        Assert.True(card.IsEnabled);
        Assert.True(card.Invoke());
        Assert.Equal(2, clickCount);
        Assert.Equal(2, command.ExecuteCount);
    }

    [Fact]
    public void FWSettingsCard_ShouldRestoreCommandStateWhenCommandIsRemoved()
    {
        var command = new RecordingCommand
        {
            CanExecuteResult = false
        };
        var card = new FWSettingsCard
        {
            Header = "Accent color",
            IsClickEnabled = true,
            Command = command,
            CommandParameter = "accent"
        };
        var clickCount = 0;
        card.Click += (_, _) => clickCount++;

        Assert.False(card.CanExecute);
        Assert.False(card.IsEnabled);
        Assert.False(card.Invoke());

        card.Command = null;

        Assert.True(card.CanExecute);
        Assert.True(card.IsEnabled);
        Assert.True(card.Invoke());
        Assert.Equal(1, clickCount);
        Assert.Equal(0, command.ExecuteCount);
    }

    [Fact]
    public void FWSettingsCard_ShouldInvokeFromKeyboardAndHoverModes()
    {
        var command = new RecordingCommand();
        var card = new FWSettingsCard
        {
            Header = "Window material",
            IsClickEnabled = true,
            Command = command,
            CommandParameter = "material"
        };

        card.RaiseEvent(new Jalium.UI.Input.KeyEventArgs(UIElement.KeyDownEvent, Jalium.UI.Input.Key.Enter, Jalium.UI.Input.ModifierKeys.None, isDown: true, isRepeat: false, timestamp: 0));

        Assert.Equal(1, command.ExecuteCount);
        Assert.Equal("material", command.LastParameter);

        card.RaiseEvent(new Jalium.UI.Input.KeyEventArgs(UIElement.KeyDownEvent, Jalium.UI.Input.Key.Space, Jalium.UI.Input.ModifierKeys.None, isDown: true, isRepeat: false, timestamp: 0));
        Assert.Equal(1, command.ExecuteCount);

        card.RaiseEvent(new Jalium.UI.Input.KeyEventArgs(UIElement.KeyUpEvent, Jalium.UI.Input.Key.Space, Jalium.UI.Input.ModifierKeys.None, isDown: false, isRepeat: false, timestamp: 0));
        Assert.Equal(2, command.ExecuteCount);

        card.ClickMode = ClickMode.Press;
        card.RaiseEvent(new Jalium.UI.Input.KeyEventArgs(UIElement.KeyDownEvent, Jalium.UI.Input.Key.Space, Jalium.UI.Input.ModifierKeys.None, isDown: true, isRepeat: false, timestamp: 0));
        Assert.Equal(3, command.ExecuteCount);

        card.RaiseEvent(new Jalium.UI.Input.KeyEventArgs(UIElement.KeyUpEvent, Jalium.UI.Input.Key.Space, Jalium.UI.Input.ModifierKeys.None, isDown: false, isRepeat: false, timestamp: 0));
        Assert.Equal(3, command.ExecuteCount);

        card.ClickMode = ClickMode.Hover;
        card.RaiseEvent(new Jalium.UI.Input.MouseEventArgs(UIElement.MouseEnterEvent));
        Assert.Equal(4, command.ExecuteCount);
    }

    [Fact]
    public void FWSettingsCard_ShouldExposeInteractionDiagnosticsAndPressedState()
    {
        var command = new RecordingCommand();
        var card = new FWSettingsCard
        {
            Header = "Window material",
            IsClickEnabled = true,
            Command = command,
            CommandParameter = "material"
        };

        var initialDiagnostics = card.GetDiagnostics();

        Assert.True(initialDiagnostics.IsClickEnabled);
        Assert.True(initialDiagnostics.IsEnabled);
        Assert.True(initialDiagnostics.CanExecute);
        Assert.True(initialDiagnostics.IsInvokable);
        Assert.True(initialDiagnostics.HasCommand);
        Assert.Equal(ClickMode.Release, initialDiagnostics.ClickMode);
        Assert.False(initialDiagnostics.IsPointerPressed);
        Assert.False(initialDiagnostics.IsKeyboardPressed);
        Assert.False(initialDiagnostics.IsInteractionPressed);

        card.RaiseEvent(new Jalium.UI.Input.KeyEventArgs(UIElement.KeyDownEvent, Jalium.UI.Input.Key.Space, Jalium.UI.Input.ModifierKeys.None, isDown: true, isRepeat: false, timestamp: 0));

        Assert.True(card.IsKeyboardPressed);
        Assert.False(card.IsPointerPressed);
        Assert.True(card.IsInteractionPressed);
        Assert.True(card.GetDiagnostics().IsInteractionPressed);

        command.CanExecuteResult = false;
        command.RaiseCanExecuteChanged();

        Assert.False(card.CanExecute);
        Assert.False(card.IsEnabled);
        Assert.False(card.IsKeyboardPressed);
        Assert.False(card.IsPointerPressed);
        Assert.False(card.IsInteractionPressed);
        Assert.False(card.GetDiagnostics().IsInvokable);

        command.CanExecuteResult = true;
        command.RaiseCanExecuteChanged();
        card.RaiseEvent(new Jalium.UI.Input.MouseButtonEventArgs(
            UIElement.MouseDownEvent,
            new Point(4, 4),
            Jalium.UI.Input.MouseButton.Left,
            Jalium.UI.Input.MouseButtonState.Pressed,
            clickCount: 1,
            leftButton: Jalium.UI.Input.MouseButtonState.Pressed,
            middleButton: Jalium.UI.Input.MouseButtonState.Released,
            rightButton: Jalium.UI.Input.MouseButtonState.Released,
            xButton1: Jalium.UI.Input.MouseButtonState.Released,
            xButton2: Jalium.UI.Input.MouseButtonState.Released,
            modifiers: Jalium.UI.Input.ModifierKeys.None,
            timestamp: 0));

        Assert.True(card.IsPointerPressed);
        Assert.False(card.IsKeyboardPressed);
        Assert.True(card.IsInteractionPressed);

        card.IsClickEnabled = false;

        Assert.False(card.Focusable);
        Assert.False(card.IsPointerPressed);
        Assert.False(card.IsKeyboardPressed);
        Assert.False(card.IsInteractionPressed);
        Assert.False(card.GetDiagnostics().IsInvokable);
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
    public void FWRelativePanel_ShouldExposeWinUIStyleRelativeConstraints()
    {
        var anchor = new FWBorder();
        var detail = new FWBorder();

        FWRelativePanel.SetAlignLeftWithPanel(anchor, true);
        FWRelativePanel.SetAlignTopWithPanel(anchor, true);
        FWRelativePanel.SetAlignRightWithPanel(anchor, true);
        FWRelativePanel.SetAlignBottomWithPanel(anchor, true);
        FWRelativePanel.SetAlignHorizontalCenterWithPanel(anchor, true);
        FWRelativePanel.SetAlignVerticalCenterWithPanel(anchor, true);
        FWRelativePanel.SetRightOf(detail, anchor);
        FWRelativePanel.SetBelow(detail, anchor);
        FWRelativePanel.SetAlignLeftWith(detail, anchor);
        FWRelativePanel.SetAlignTopWith(detail, anchor);
        FWRelativePanel.SetAlignRightWith(detail, anchor);
        FWRelativePanel.SetAlignBottomWith(detail, anchor);
        FWRelativePanel.SetAlignHorizontalCenterWith(detail, anchor);
        FWRelativePanel.SetAlignVerticalCenterWith(detail, anchor);

        Assert.True(FWRelativePanel.GetAlignLeftWithPanel(anchor));
        Assert.True(FWRelativePanel.GetAlignTopWithPanel(anchor));
        Assert.True(FWRelativePanel.GetAlignRightWithPanel(anchor));
        Assert.True(FWRelativePanel.GetAlignBottomWithPanel(anchor));
        Assert.True(FWRelativePanel.GetAlignHorizontalCenterWithPanel(anchor));
        Assert.True(FWRelativePanel.GetAlignVerticalCenterWithPanel(anchor));
        Assert.Same(anchor, FWRelativePanel.GetRightOf(detail));
        Assert.Same(anchor, FWRelativePanel.GetBelow(detail));
        Assert.Same(anchor, FWRelativePanel.GetAlignLeftWith(detail));
        Assert.Same(anchor, FWRelativePanel.GetAlignTopWith(detail));
        Assert.Same(anchor, FWRelativePanel.GetAlignRightWith(detail));
        Assert.Same(anchor, FWRelativePanel.GetAlignBottomWith(detail));
        Assert.Same(anchor, FWRelativePanel.GetAlignHorizontalCenterWith(detail));
        Assert.Same(anchor, FWRelativePanel.GetAlignVerticalCenterWith(detail));
    }

    [Fact]
    public void FWRelativePanel_ShouldMeasureChildrenFromRelativeConstraints()
    {
        var anchor = new FWBorder { Width = 40, Height = 20 };
        var right = new FWBorder { Width = 30, Height = 12 };
        var below = new FWBorder { Width = 50, Height = 10 };
        var panel = new FWRelativePanel
        {
            ColumnSpacing = 8,
            RowSpacing = 6,
            Children =
            {
                anchor,
                right,
                below
            }
        };

        FWRelativePanel.SetAlignLeftWithPanel(anchor, true);
        FWRelativePanel.SetAlignTopWithPanel(anchor, true);
        FWRelativePanel.SetRightOf(right, anchor);
        FWRelativePanel.SetAlignTopWith(right, anchor);
        FWRelativePanel.SetBelow(below, anchor);
        FWRelativePanel.SetAlignLeftWith(below, anchor);

        panel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        panel.Arrange(new Rect(0, 0, 120, 80));

        Assert.Equal(78, panel.DesiredSize.Width);
        Assert.Equal(36, panel.DesiredSize.Height);
        Assert.Equal(new Size(120, 80), panel.RenderSize);
        Assert.Equal(new Size(40, 20), anchor.RenderSize);
        Assert.Equal(new Size(30, 12), right.RenderSize);
        Assert.Equal(new Size(50, 10), below.RenderSize);
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

        var canvasStyle = AssertStyle<FWCanvas>(dictionary);
        Assert.Equal(typeof(Canvas), canvasStyle.BasedOn?.TargetType);

        var relativePanelStyle = AssertStyle<FWRelativePanel>(dictionary);
        AssertSetter(relativePanelStyle, Panel.BackgroundProperty);
        AssertSetter(relativePanelStyle, FWRelativePanel.RowSpacingProperty);
        AssertSetter(relativePanelStyle, FWRelativePanel.ColumnSpacingProperty);

        var twoPaneViewStyle = AssertStyle<FWTwoPaneView>(dictionary);
        AssertSetter(twoPaneViewStyle, FWTwoPaneView.ModeProperty);
        AssertSetter(twoPaneViewStyle, FWTwoPaneView.PanePriorityProperty);
        AssertSetter(twoPaneViewStyle, FWTwoPaneView.MinWideModeWidthProperty);
        AssertSetter(twoPaneViewStyle, FWTwoPaneView.MinTallModeHeightProperty);
        AssertTriggerSetter(twoPaneViewStyle, FWTwoPaneView.VisiblePaneProperty, FWTwoPaneViewVisiblePane.Pane1, UIElement.VisibilityProperty, Visibility.Collapsed);
        AssertTriggerSetter(twoPaneViewStyle, FWTwoPaneView.VisiblePaneProperty, FWTwoPaneViewVisiblePane.Pane2, UIElement.VisibilityProperty, Visibility.Collapsed);

        var parallaxViewStyle = AssertStyle<FWParallaxView>(dictionary);
        Assert.Equal(typeof(ContentControl), parallaxViewStyle.BasedOn?.TargetType);
        AssertSetter(parallaxViewStyle, FWParallaxView.VerticalShiftProperty);
        AssertSetter(parallaxViewStyle, FWParallaxView.IsVerticalShiftEnabledProperty);
        AssertSetter(parallaxViewStyle, FWParallaxView.ProgressProperty);

        var settingsCardStyle = AssertStyle<FWSettingsCard>(dictionary);
        Assert.Equal(typeof(ContentControl), settingsCardStyle.BasedOn?.TargetType);
        AssertSetter(settingsCardStyle, FrameworkElement.MinHeightProperty);
        AssertSetter(settingsCardStyle, FWSettingsCard.IsClickEnabledProperty);
        AssertSetter(settingsCardStyle, FWSettingsCard.ClickModeProperty);
        AssertSetter(settingsCardStyle, UIElement.FocusableProperty);
        AssertTriggerSetter(settingsCardStyle, FWSettingsCard.IsInteractionPressedProperty, true, Control.BackgroundProperty, "ControlBackgroundPressed");
        AssertTriggerSetter(settingsCardStyle, UIElement.IsKeyboardFocusedProperty, true, Control.BorderBrushProperty, "ControlBorderFocused");
        AssertTriggerSetter(settingsCardStyle, UIElement.IsEnabledProperty, false, Control.BackgroundProperty, "ControlBackgroundDisabled");

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

    private static void AssertTriggerSetter(
        Style style,
        DependencyProperty triggerProperty,
        object triggerValue,
        DependencyProperty setterProperty,
        object expectedSetterValue)
    {
        var trigger = Assert.Single(
            style.Triggers.OfType<Trigger>(),
            candidate => candidate.Property == triggerProperty && TriggerValueEquals(candidate.Value, triggerValue));
        var setter = Assert.Single(trigger.Setters, candidate => candidate.Property == setterProperty);

        if (setter.Value is IDynamicResourceReference dynamicReference)
        {
            Assert.Equal(expectedSetterValue, dynamicReference.ResourceKey);
            return;
        }

        Assert.True(
            TriggerValueEquals(setter.Value, expectedSetterValue),
            $"Expected trigger setter value {expectedSetterValue}, got {setter.Value}.");
    }

    private static bool TriggerValueEquals(object? actual, object expected)
    {
        if (Equals(actual, expected))
        {
            return true;
        }

        if (actual is null)
        {
            return false;
        }

        return string.Equals(actual.ToString(), expected.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private sealed class RecordingCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecuteResult { get; set; } = true;

        public int ExecuteCount { get; private set; }

        public object? LastParameter { get; private set; }

        public bool CanExecute(object? parameter) => CanExecuteResult;

        public void Execute(object? parameter)
        {
            ExecuteCount++;
            LastParameter = parameter;
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
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
