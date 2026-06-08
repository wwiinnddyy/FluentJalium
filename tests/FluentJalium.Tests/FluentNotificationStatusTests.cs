using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Windows.Input;
using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using FluentJalium.Gallery.Services;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Input;
using Jalium.UI.Controls.Themes;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using JaliumThemeManager = Jalium.UI.Controls.Themes.ThemeManager;
using PopupPlacementMode = Jalium.UI.Controls.Primitives.PlacementMode;

namespace FluentJalium.Tests;

[Collection("Application")]
public sealed class FluentNotificationStatusTests
{
    private static readonly string[] NotificationStatusResourceKeys =
    [
        "InfoBarInformationalBackground",
        "InfoBarSuccessBackground",
        "InfoBarWarningBackground",
        "InfoBarErrorBackground",
        "InfoBarInfoBrush",
        "InfoBarSuccessBrush",
        "InfoBarWarningBrush",
        "InfoBarErrorBrush",
        "InfoBarForeground",
        "ToastForeground",
        "ToastInformationBackground",
        "ToastSuccessBackground",
        "ToastWarningBackground",
        "ToastErrorBackground",
        "ToastInformationIcon",
        "ToastSuccessIcon",
        "ToastWarningIcon",
        "ToastErrorIcon",
        "InfoBadgeAttentionBackground",
        "InfoBadgeInformationalBackground",
        "InfoBadgeSuccessBackground",
        "InfoBadgeCautionBackground",
        "InfoBadgeCriticalBackground",
        "InfoBadgeAttentionForeground",
        "InfoBadgeInformationalForeground",
        "InfoBadgeSuccessForeground",
        "InfoBadgeCautionForeground",
        "InfoBadgeCriticalForeground",
        "StatusBarBackground",
        "StatusBarForeground",
        "StatusBarForegroundDisabled",
        "StatusBarBorderBrush",
        "StatusBarSeparatorForeground"
    ];

    [Theory]
    [InlineData(FluentThemeVariant.Dark)]
    [InlineData(FluentThemeVariant.Light)]
    [InlineData(FluentThemeVariant.HighContrast)]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void ThemeDictionary_ShouldExposeNotificationStatusTokensForEveryTheme(FluentThemeVariant theme)
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = theme.ToString();

        var dictionary = LoadGenericThemeDictionary();

        foreach (var key in NotificationStatusResourceKeys)
        {
            Assert.True(dictionary.TryGetValue(key, out var value), $"{key} was not found in the {theme} theme.");
            Assert.IsType<SolidColorBrush>(value);
        }

        ResetApplicationState();
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void Apply_ShouldRegisterFwNotificationStatusStyles()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            AssertBasedOnStyle<FWInfoBar, InfoBar>(app.Resources);
            AssertOwnedStyle<FWInfoBadge>(app.Resources);
            AssertBasedOnStyle<FWToastNotificationItem, ToastNotificationItem>(app.Resources);
            AssertBasedOnStyle<FWToastNotificationHost, ToastNotificationHost>(app.Resources);
            AssertOwnedStyle<FWSnackbar>(app.Resources);
            AssertOwnedStyle<FWSnackbarHost>(app.Resources);
            AssertOwnedStyle<FWSnackbarOverlayHost>(app.Resources);
            AssertBasedOnStyle<FWStatusBar, StatusBar>(app.Resources);
            AssertBasedOnStyle<FWStatusBarItem, StatusBarItem>(app.Resources);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void GenericTheme_ShouldDefineNotificationStatusBaseStylesAndToastDefaults()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = FluentThemeVariant.Dark.ToString();

        var dictionary = LoadGenericThemeDictionary();

        AssertContainsStyle<InfoBar>(dictionary);
        AssertContainsStyle<ToastNotificationItem>(dictionary);
        AssertContainsStyle<ToastNotificationHost>(dictionary);
        var snackbarStyle = AssertStyle<FWSnackbar>(dictionary);
        Assert.Null(snackbarStyle.BasedOn);
        AssertSetter(snackbarStyle, FWSnackbar.SeverityProperty);
        AssertSetter(snackbarStyle, FWSnackbar.IsClosableProperty);
        AssertSetter(snackbarStyle, FWSnackbar.IsAutoDismissEnabledProperty);
        AssertSetter(snackbarStyle, FWSnackbar.IsAutoDismissPausedOnPointerOverEnabledProperty);
        AssertSetter(snackbarStyle, FWSnackbar.IsAutoDismissPausedOnFocusEnabledProperty);
        var snackbarHostStyle = AssertStyle<FWSnackbarHost>(dictionary);
        Assert.Null(snackbarHostStyle.BasedOn);
        AssertSetter(snackbarHostStyle, FWSnackbarHost.MaxVisibleSnackbarsProperty, 1);
        AssertSetter(snackbarHostStyle, FWSnackbarHost.PlacementProperty, "Bottom");
        AssertSetter(snackbarHostStyle, FWSnackbarHost.SpacingProperty, 8.0);
        AssertSetter(snackbarHostStyle, FWSnackbarHost.IsTransitionEnabledProperty);
        AssertSetter(snackbarHostStyle, FWSnackbarHost.TransitionProfileProperty, "Entrance");
        AssertSetter(snackbarHostStyle, FWSnackbarHost.SnackbarTransitionDurationProperty);
        AssertSetter(snackbarHostStyle, FWSnackbarHost.TransitionOffsetProperty, 16.0);
        AssertSetter(snackbarHostStyle, Control.HorizontalContentAlignmentProperty, "Stretch");
        AssertSetter(snackbarHostStyle, Control.VerticalContentAlignmentProperty, "Bottom");
        var snackbarOverlayHostStyle = AssertStyle<FWSnackbarOverlayHost>(dictionary);
        Assert.Null(snackbarOverlayHostStyle.BasedOn);
        AssertSetter(snackbarOverlayHostStyle, FWSnackbarHost.MaxVisibleSnackbarsProperty, 3);
        AssertSetter(snackbarOverlayHostStyle, FWSnackbarHost.PlacementProperty, "Bottom");
        AssertSetter(snackbarOverlayHostStyle, FWSnackbarHost.SpacingProperty, 8.0);
        AssertSetter(snackbarOverlayHostStyle, FWSnackbarOverlayHost.OverlayPlacementProperty, "Bottom");
        AssertSetter(snackbarOverlayHostStyle, FWSnackbarOverlayHost.IsOverlayAutoOpenEnabledProperty);
        AssertSetter(snackbarOverlayHostStyle, Control.TemplateProperty);
        AssertContainsStyle<StatusBar>(dictionary);
        AssertContainsStyle<StatusBarItem>(dictionary);

        var badgeStyle = AssertStyle<FWInfoBadge>(dictionary);
        Assert.DoesNotContain(badgeStyle.Setters, setter => setter.Property == Control.BackgroundProperty);
        Assert.DoesNotContain(badgeStyle.Setters, setter => setter.Property == Control.ForegroundProperty);
        AssertSetter(badgeStyle, Control.CornerRadiusProperty);
        AssertSetter(badgeStyle, Control.PaddingProperty);

        var toastHostStyle = AssertStyle<ToastNotificationHost>(dictionary);
        AssertSetter(toastHostStyle, ToastNotificationHost.MaxVisibleToastsProperty, 3);
        AssertSetter(toastHostStyle, ToastNotificationHost.SpacingProperty, 8.0);
        AssertSetter(toastHostStyle, ToastNotificationHost.ToastWidthProperty, 400.0);

        ResetApplicationState();
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void FWInfoBadge_DefaultStyleShouldNotOverrideSeverityResourceResolution()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);
            var critical = new FWInfoBadge
            {
                Value = 1,
                Severity = FWInfoBadgeSeverity.Critical
            };
            var caution = new FWInfoBadge
            {
                Value = 1,
                Severity = FWInfoBadgeSeverity.Caution
            };

            critical.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            caution.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            Assert.False(critical.HasLocalValue(Control.BackgroundProperty));
            Assert.False(critical.HasLocalValue(Control.ForegroundProperty));
            Assert.False(caution.HasLocalValue(Control.BackgroundProperty));
            Assert.False(caution.HasLocalValue(Control.ForegroundProperty));
            Assert.Equal(FWInfoBadgeDisplayKind.Value, critical.DisplayKind);
            Assert.Equal("1", critical.DisplayValueText);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    public void FWToastNotificationHost_ShouldExposeFluentDefaultsAndDismissVisibleItems()
    {
        var host = new FWToastNotificationHost
        {
            MaxVisibleToasts = 3,
            Position = ToastPosition.BottomRight,
            ToastWidth = 400,
            Spacing = 8
        };

        var info = host.ShowInformation("Info", "First", TimeSpan.FromSeconds(12));
        var success = host.ShowSuccess("Success", "Second", TimeSpan.FromSeconds(12));
        var warning = host.ShowWarning("Warning", "Third", TimeSpan.FromSeconds(12));
        var error = host.ShowError("Error", "Fourth", TimeSpan.FromSeconds(12));

        Assert.IsType<FWToastNotificationItem>(info);
        Assert.DoesNotContain(info, host.Children);
        Assert.Contains(success, host.Children);
        Assert.Contains(warning, host.Children);
        Assert.Contains(error, host.Children);
        Assert.Equal(3, host.Children.Count);
        Assert.Equal(ToastPosition.BottomRight, host.Position);
        Assert.Equal(400, host.ToastWidth);
        Assert.Equal(8, host.Spacing);

        success.IsOpen = false;

        Assert.Equal(2, host.Children.Count);
        Assert.DoesNotContain(success, host.Children);

        host.DismissAll();
        Assert.Equal(2, host.Children.Count);
    }

    [Fact]
    public void FWSnackbar_ShouldExposeActionCloseAndSeverityState()
    {
        var snackbar = new FWSnackbar
        {
            Title = "Undo archive",
            Message = "One item was archived.",
            ActionContent = "Undo",
            Content = new FWInfoBadge { Severity = FWInfoBadgeSeverity.Attention },
            Severity = ToastSeverity.Warning,
            Duration = TimeSpan.FromSeconds(8),
            IsAutoDismissEnabled = false,
            ActionCommand = new RecordingCommand(),
            ActionCommandParameter = "archive"
        };
        var actionCount = 0;
        var closedCount = 0;
        snackbar.ActionClick += (_, args) =>
        {
            actionCount++;
            args.Handled = true;
        };
        snackbar.Closed += (_, _) => closedCount++;

        snackbar.Show();

        Assert.True(snackbar.IsOpen);
        Assert.Equal(ToastSeverity.Warning, snackbar.Severity);
        Assert.Equal(TimeSpan.FromSeconds(8), snackbar.Duration);
        Assert.False(snackbar.IsAutoDismissEnabled);

        var handled = snackbar.RequestAction();

        Assert.True(handled);
        Assert.True(snackbar.IsOpen);
        Assert.Equal(1, actionCount);
        var command = Assert.IsType<RecordingCommand>(snackbar.ActionCommand);
        Assert.Equal(1, command.ExecuteCount);
        Assert.Equal("archive", command.LastParameter);

        snackbar.Close();

        Assert.False(snackbar.IsOpen);
        Assert.Equal(1, closedCount);
        Assert.Equal(FWSnackbarCloseReason.Programmatic, snackbar.LastCloseReason);
    }

    [Fact]
    public async Task FWSnackbar_ShouldAutoDismissAndCompleteShowAsync()
    {
        var snackbar = new FWSnackbar
        {
            Title = "Saved",
            Message = "The queued snackbar closes itself.",
            Duration = TimeSpan.FromMilliseconds(20),
            IsAutoDismissEnabled = true
        };
        var openedCount = 0;
        var closedCount = 0;
        snackbar.Opened += (_, _) => openedCount++;
        snackbar.Closed += (_, _) => closedCount++;

        var showTask = snackbar.ShowAsync();
        var completed = await Task.WhenAny(showTask, Task.Delay(TimeSpan.FromSeconds(2)));

        Assert.Same(showTask, completed);
        Assert.False(snackbar.IsOpen);
        Assert.Equal(FWSnackbarCloseReason.Timeout, snackbar.LastCloseReason);
        Assert.Equal(1, openedCount);
        Assert.Equal(1, closedCount);
    }

    [Fact]
    public async Task FWSnackbar_ShouldCompleteResultFlowWithCloseReason()
    {
        var snackbar = new FWSnackbar
        {
            Title = "Undo archive",
            ActionContent = "Undo",
            ActionCommand = new RecordingCommand(),
            ActionCommandParameter = "archive",
            IsAutoDismissEnabled = false
        };

        var showTask = snackbar.ShowForResultAsync();

        Assert.True(snackbar.IsOpen);
        Assert.Equal(FWSnackbarCloseReason.None, snackbar.LastCloseReason);
        Assert.False(snackbar.RequestAction());

        var result = await showTask;

        Assert.Equal(FWSnackbarCloseReason.Action, result);
        Assert.Equal(FWSnackbarCloseReason.Action, snackbar.LastCloseReason);
        Assert.False(snackbar.IsOpen);

        var command = Assert.IsType<RecordingCommand>(snackbar.ActionCommand);
        Assert.Equal(1, command.ExecuteCount);
        Assert.Equal("archive", command.LastParameter);
    }

    [Fact]
    public async Task FWSnackbar_ShouldAllowClosingCancellation()
    {
        var snackbar = new FWSnackbar
        {
            Title = "Upload pending",
            IsAutoDismissEnabled = false
        };
        var cancelClose = true;
        var closingCount = 0;
        var closedCount = 0;
        snackbar.Closing += (_, args) =>
        {
            closingCount++;
            Assert.Equal(FWSnackbarCloseReason.CloseButton, args.Reason);
            args.Cancel = cancelClose;
        };
        snackbar.Closed += (_, _) => closedCount++;

        var resultTask = snackbar.ShowForResultAsync();

        Assert.False(snackbar.RequestClose(FWSnackbarCloseReason.CloseButton));
        Assert.True(snackbar.IsOpen);
        Assert.False(resultTask.IsCompleted);
        Assert.Equal(FWSnackbarCloseReason.None, snackbar.LastCloseReason);

        cancelClose = false;

        Assert.True(snackbar.RequestClose(FWSnackbarCloseReason.CloseButton));

        var result = await resultTask;

        Assert.Equal(FWSnackbarCloseReason.CloseButton, result);
        Assert.Equal(FWSnackbarCloseReason.CloseButton, snackbar.LastCloseReason);
        Assert.False(snackbar.IsOpen);
        Assert.Equal(2, closingCount);
        Assert.Equal(1, closedCount);
    }

    [Fact]
    public async Task FWSnackbar_ShouldPauseAutoDismissOnPointerOverAndResumeOnLeave()
    {
        var snackbar = new FWSnackbar
        {
            Title = "Saved",
            Duration = TimeSpan.FromMilliseconds(30),
            IsAutoDismissEnabled = true
        };

        var resultTask = snackbar.ShowForResultAsync();
        snackbar.RaiseEvent(new Jalium.UI.Input.MouseEventArgs(UIElement.MouseEnterEvent));

        Assert.True(snackbar.IsAutoDismissPaused);

        await Task.Delay(TimeSpan.FromMilliseconds(100));

        Assert.True(snackbar.IsOpen);
        Assert.False(resultTask.IsCompleted);

        snackbar.RaiseEvent(new Jalium.UI.Input.MouseEventArgs(UIElement.MouseLeaveEvent));

        var completed = await Task.WhenAny(resultTask, Task.Delay(TimeSpan.FromSeconds(2)));

        Assert.Same(resultTask, completed);
        Assert.False(snackbar.IsAutoDismissPaused);
        Assert.Equal(FWSnackbarCloseReason.Timeout, await resultTask);
        Assert.False(snackbar.IsOpen);
    }

    [Fact]
    public async Task FWSnackbar_ShouldPauseAutoDismissOnFocusAndResumeOnLostFocus()
    {
        var snackbar = new FWSnackbar
        {
            Title = "Focused",
            Duration = TimeSpan.FromMilliseconds(30),
            IsAutoDismissEnabled = true
        };

        var resultTask = snackbar.ShowForResultAsync();
        snackbar.RaiseEvent(new KeyboardFocusChangedEventArgs(UIElement.GotKeyboardFocusEvent, null, snackbar));

        Assert.True(snackbar.IsAutoDismissPaused);

        await Task.Delay(TimeSpan.FromMilliseconds(100));

        Assert.True(snackbar.IsOpen);
        Assert.False(resultTask.IsCompleted);

        snackbar.RaiseEvent(new KeyboardFocusChangedEventArgs(UIElement.LostKeyboardFocusEvent, snackbar, null));

        var completed = await Task.WhenAny(resultTask, Task.Delay(TimeSpan.FromSeconds(2)));

        Assert.Same(resultTask, completed);
        Assert.False(snackbar.IsAutoDismissPaused);
        Assert.Equal(FWSnackbarCloseReason.Timeout, await resultTask);
        Assert.False(snackbar.IsOpen);
    }

    [Fact]
    public void FWSnackbarHost_ShouldQueueAndPromoteSnackbarItems()
    {
        var host = new FWSnackbarHost
        {
            MaxVisibleSnackbars = 2,
            Placement = FWSnackbarPlacement.Top,
            Spacing = 12
        };
        var first = new FWSnackbar { Title = "First", IsAutoDismissEnabled = false };
        var second = new FWSnackbar { Title = "Second", IsAutoDismissEnabled = false };
        var third = new FWSnackbar { Title = "Third", IsAutoDismissEnabled = false };

        host.Enqueue(first);
        host.Enqueue(second);
        host.Enqueue(third);

        Assert.Equal(FWSnackbarPlacement.Top, host.Placement);
        Assert.Equal(VerticalAlignment.Top, host.VerticalContentAlignment);
        Assert.Equal(HorizontalAlignment.Stretch, host.HorizontalContentAlignment);
        Assert.Equal(12, host.Spacing);
        Assert.Equal(2, host.Snackbars.Count);
        Assert.Equal(1, host.PendingCount);
        Assert.Same(first, host.CurrentSnackbar);
        Assert.True(first.IsOpen);
        Assert.True(second.IsOpen);
        Assert.False(third.IsOpen);

        first.Close();

        Assert.Equal(2, host.Snackbars.Count);
        Assert.Equal(0, host.PendingCount);
        Assert.DoesNotContain(first, host.Snackbars);
        Assert.Contains(second, host.Snackbars);
        Assert.Contains(third, host.Snackbars);
        Assert.True(third.IsOpen);

        Assert.True(host.CloseCurrent());
        Assert.Single(host.Snackbars);

        host.Clear();

        Assert.Empty(host.Snackbars);
        Assert.Equal(0, host.PendingCount);
        Assert.Equal(FWSnackbarCloseReason.Programmatic, second.LastCloseReason);
        Assert.Equal(FWSnackbarCloseReason.HostCleared, third.LastCloseReason);
    }

    [Fact]
    public void FWSnackbarHost_ShouldResolvePlacementAlignmentAndValidateSpacing()
    {
        var host = new FWSnackbarHost();

        Assert.Equal(FWSnackbarPlacement.Bottom, host.Placement);
        Assert.Equal(VerticalAlignment.Bottom, host.VerticalContentAlignment);
        Assert.Equal(HorizontalAlignment.Stretch, host.HorizontalContentAlignment);
        Assert.Equal(8, host.Spacing);

        host.Placement = FWSnackbarPlacement.Top;

        Assert.Equal(VerticalAlignment.Top, host.VerticalContentAlignment);

        host.Placement = FWSnackbarPlacement.Bottom;
        host.Spacing = 16;

        Assert.Equal(VerticalAlignment.Bottom, host.VerticalContentAlignment);
        Assert.Equal(16, host.Spacing);
        Assert.Throws<ArgumentException>(() => host.Spacing = -1);
    }

    [Fact]
    public void FWSnackbarHost_ShouldExposeTransitionDefaultsAndDiagnostics()
    {
        var host = new FWSnackbarHost();

        Assert.True(host.IsTransitionEnabled);
        Assert.Equal(FWContentTransitionProfile.Entrance, host.TransitionProfile);
        Assert.Equal(TimeSpan.FromMilliseconds(320), host.SnackbarTransitionDuration);
        Assert.Equal(16, host.TransitionOffset);

        var diagnostics = host.GetDiagnostics();

        Assert.Equal(0, diagnostics.VisibleCount);
        Assert.Equal(0, diagnostics.PendingCount);
        Assert.Equal(1, diagnostics.MaxVisibleSnackbars);
        Assert.Equal(FWSnackbarPlacement.Bottom, diagnostics.Placement);
        Assert.Equal(VerticalAlignment.Bottom, diagnostics.VerticalAlignment);
        Assert.Equal(HorizontalAlignment.Stretch, diagnostics.HorizontalAlignment);
        Assert.Equal(8, diagnostics.Spacing);
        Assert.True(diagnostics.IsTransitionEnabled);
        Assert.Equal(FWContentTransitionProfile.Entrance, diagnostics.TransitionProfile);
        Assert.Equal(TimeSpan.FromMilliseconds(320), diagnostics.SnackbarTransitionDuration);
        Assert.Equal(16, diagnostics.TransitionOffset);
        Assert.False(diagnostics.HasCurrentSnackbar);
        Assert.False(diagnostics.HasPendingSnackbars);

        host.TransitionProfile = FWContentTransitionProfile.Suppress;

        Assert.Equal(TimeSpan.Zero, host.SnackbarTransitionDuration);

        host.SnackbarTransitionDuration = TimeSpan.FromMilliseconds(180);
        host.TransitionOffset = 24;

        Assert.Equal(TimeSpan.FromMilliseconds(180), host.SnackbarTransitionDuration);
        Assert.Equal(24, host.TransitionOffset);
        Assert.Throws<ArgumentException>(() => host.TransitionProfile = (FWContentTransitionProfile)42);
        Assert.Throws<ArgumentException>(() => host.SnackbarTransitionDuration = TimeSpan.FromMilliseconds(-1));
        Assert.Throws<ArgumentException>(() => host.TransitionOffset = -1);
    }

    [Fact]
    public void FWSnackbarHost_ShouldRaiseTransitionAndQueueDiagnostics()
    {
        var host = new FWSnackbarHost
        {
            MaxVisibleSnackbars = 1,
            Placement = FWSnackbarPlacement.Top,
            Spacing = 12,
            TransitionProfile = FWContentTransitionProfile.Entrance,
            TransitionOffset = 20
        };
        var transitions = new List<FWSnackbarTransitionRequestedEventArgs>();
        var queueChanges = new List<FWSnackbarHostQueueChangedEventArgs>();
        host.TransitionRequested += (_, args) => transitions.Add(args);
        host.QueueChanged += (_, args) => queueChanges.Add(args);
        var first = new FWSnackbar { Title = "First", IsAutoDismissEnabled = false };
        var second = new FWSnackbar { Title = "Second", IsAutoDismissEnabled = false };

        host.Enqueue(first);
        host.Enqueue(second);

        Assert.Equal(new[] { FWSnackbarTransitionKind.Show }, transitions.Select(item => item.Kind).ToArray());
        Assert.Equal(new[] { FWSnackbarHostQueueChangeReason.Shown, FWSnackbarHostQueueChangeReason.Queued }, queueChanges.Select(item => item.Reason).ToArray());
        Assert.Same(first, transitions[0].Snackbar);
        Assert.Equal(FWSnackbarPlacement.Top, transitions[0].Diagnostics.Placement);
        Assert.Equal(20, transitions[0].Diagnostics.TransitionOffset);
        Assert.Equal(1, host.GetDiagnostics().VisibleCount);
        Assert.Equal(1, host.GetDiagnostics().PendingCount);
        Assert.True(host.GetDiagnostics().HasPendingSnackbars);

        first.Close();

        Assert.Equal(
            new[] { FWSnackbarTransitionKind.Show, FWSnackbarTransitionKind.Close, FWSnackbarTransitionKind.Show },
            transitions.Select(item => item.Kind).ToArray());
        Assert.Equal(FWSnackbarHostQueueChangeReason.Closed, queueChanges[^2].Reason);
        Assert.Equal(FWSnackbarHostQueueChangeReason.Shown, queueChanges[^1].Reason);
        Assert.Same(second, host.CurrentSnackbar);
        Assert.Equal(1, host.GetDiagnostics().VisibleCount);
        Assert.Equal(0, host.GetDiagnostics().PendingCount);

        host.IsTransitionEnabled = false;
        host.CloseCurrent();

        Assert.Equal(3, transitions.Count);
        Assert.Empty(host.Snackbars);
    }

    [Fact]
    public void FWSnackbar_ShouldExposePresenterMotionDiagnostics()
    {
        var snackbar = new FWSnackbar { Title = "Motion", IsAutoDismissEnabled = false };
        var initialDiagnostics = snackbar.GetPresenterDiagnostics();

        Assert.Equal(FWSnackbarPresenterState.Idle, snackbar.PresenterState);
        Assert.Equal(FWSnackbarPresenterState.Idle, initialDiagnostics.PresenterState);
        Assert.False(initialDiagnostics.HasPresenter);
        Assert.Equal(1.0, initialDiagnostics.PresenterOpacity);
        Assert.Equal(0.0, initialDiagnostics.PresenterOffset);
        Assert.Equal(TimeSpan.Zero, initialDiagnostics.TransitionDuration);
        Assert.Equal(FWSnackbarPlacement.Bottom, initialDiagnostics.Placement);
        Assert.Null(initialDiagnostics.LastTransitionKind);

        var host = new FWSnackbarHost
        {
            Placement = FWSnackbarPlacement.Top,
            SnackbarTransitionDuration = TimeSpan.FromMilliseconds(220),
            TransitionOffset = 24
        };
        FWSnackbarTransitionRequestedEventArgs? transition = null;
        host.TransitionRequested += (_, args) => transition = args;

        host.Enqueue(snackbar);

        var showDiagnostics = snackbar.GetPresenterDiagnostics();
        Assert.NotNull(transition);
        Assert.Equal(FWSnackbarTransitionKind.Show, transition!.Kind);
        Assert.Equal(FWSnackbarPresenterState.Entering, showDiagnostics.PresenterState);
        Assert.Equal(FWSnackbarPresenterState.Entering, transition.PresenterDiagnostics.PresenterState);
        Assert.False(showDiagnostics.HasPresenter);
        Assert.Equal(0.0, showDiagnostics.PresenterOpacity);
        Assert.Equal(-24.0, showDiagnostics.PresenterOffset);
        Assert.Equal(TimeSpan.FromMilliseconds(220), showDiagnostics.TransitionDuration);
        Assert.Equal(FWSnackbarPlacement.Top, showDiagnostics.Placement);
        Assert.Equal(FWSnackbarTransitionKind.Show, showDiagnostics.LastTransitionKind);

        host.IsTransitionEnabled = false;
        host.Clear();
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void FWSnackbar_TemplateShouldAttachPendingPresenterMotion()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);
            var snackbar = new FWSnackbar
            {
                Title = "Templated",
                IsAutoDismissEnabled = false,
                Width = 360,
                Style = AssertStyle<FWSnackbar>(app.Resources)
            };
            var host = new FWSnackbarHost
            {
                Placement = FWSnackbarPlacement.Top,
                SnackbarTransitionDuration = TimeSpan.FromSeconds(5),
                TransitionOffset = 18
            };

            host.Enqueue(snackbar);
            var queuedDiagnostics = snackbar.GetPresenterDiagnostics();
            Assert.Equal(FWSnackbarTransitionKind.Show, queuedDiagnostics.LastTransitionKind);
            Assert.Equal(TimeSpan.FromSeconds(5), queuedDiagnostics.TransitionDuration);
            Assert.Equal(FWSnackbarPlacement.Top, queuedDiagnostics.Placement);
            Assert.InRange(queuedDiagnostics.PresenterOpacity, 0.0, 1.0);
            Assert.InRange(queuedDiagnostics.PresenterOffset, -18.0, 0.0);
            Assert.Equal(FWSnackbarPresenterState.Entering, snackbar.PresenterState);

            snackbar.ApplyTemplate();
            snackbar.Measure(new Size(360, 120));
            snackbar.Arrange(new Rect(0, 0, 360, 120));

            var diagnostics = snackbar.GetPresenterDiagnostics();
            Assert.True(diagnostics.HasPresenter);
            Assert.Equal(FWSnackbarTransitionKind.Show, diagnostics.LastTransitionKind);
            Assert.Equal(TimeSpan.FromSeconds(5), diagnostics.TransitionDuration);
            Assert.Equal(FWSnackbarPlacement.Top, diagnostics.Placement);
            Assert.Contains(diagnostics.PresenterState, new[] { FWSnackbarPresenterState.Entering, FWSnackbarPresenterState.Visible });
            Assert.InRange(diagnostics.PresenterOpacity, 0.0, 1.0);
            Assert.InRange(diagnostics.PresenterOffset, -18.0, 0.0);

            host.IsTransitionEnabled = false;
            host.Clear();
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public async Task FWSnackbarHost_ShouldDeferRemovalDuringPresenterCloseMotion()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);
            var host = new FWSnackbarHost
            {
                IsTransitionEnabled = false,
                MaxVisibleSnackbars = 1,
                Placement = FWSnackbarPlacement.Bottom,
                SnackbarTransitionDuration = TimeSpan.FromSeconds(5),
                TransitionOffset = 22
            };
            var snackbar = new FWSnackbar
            {
                Title = "Close motion",
                IsAutoDismissEnabled = false,
                Style = AssertStyle<FWSnackbar>(app.Resources)
            };
            var resultTask = host.EnqueueForResultAsync(snackbar);
            snackbar.ApplyTemplate();
            snackbar.Measure(new Size(360, 120));
            snackbar.Arrange(new Rect(0, 0, 360, 120));

            host.IsTransitionEnabled = true;
            Assert.True(host.CloseCurrent());

            var diagnostics = snackbar.GetPresenterDiagnostics();
            Assert.False(snackbar.IsOpen);
            Assert.Equal(FWSnackbarPresenterState.Exiting, diagnostics.PresenterState);
            Assert.True(diagnostics.HasPresenter);
            Assert.InRange(diagnostics.PresenterOpacity, 0.0, 1.0);
            Assert.InRange(diagnostics.PresenterOffset, 0.0, 22.0);
            Assert.Equal(FWSnackbarTransitionKind.Close, diagnostics.LastTransitionKind);
            Assert.Contains(snackbar, host.Snackbars);
            Assert.False(resultTask.IsCompleted);

            CompleteSnackbarPresenterTransition(snackbar, FWSnackbarPresenterState.Exiting);

            Assert.Empty(host.Snackbars);
            Assert.Equal(FWSnackbarCloseReason.Programmatic, await resultTask);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    public void FWSnackbarOverlayHost_ShouldAutoOpenAndCloseWithQueueState()
    {
        var host = new FWSnackbarOverlayHost
        {
            MaxVisibleSnackbars = 1,
            OverlayPlacement = PopupPlacementMode.Top
        };
        var overlayOpened = 0;
        var overlayClosed = 0;
        host.OverlayOpened += (_, _) => overlayOpened++;
        host.OverlayClosed += (_, _) => overlayClosed++;
        var first = new FWSnackbar { Title = "First", IsAutoDismissEnabled = false };
        var second = new FWSnackbar { Title = "Second", IsAutoDismissEnabled = false };

        Assert.False(host.IsOverlayOpen);
        Assert.True(host.IsOverlayAutoOpenEnabled);
        Assert.Equal(PopupPlacementMode.Top, host.OverlayPlacement);

        host.Enqueue(first);

        Assert.True(host.IsOverlayOpen);
        Assert.Equal(1, overlayOpened);
        Assert.Equal(0, overlayClosed);
        Assert.Same(first, host.CurrentSnackbar);

        host.Enqueue(second);

        Assert.True(host.IsOverlayOpen);
        Assert.Equal(1, host.PendingCount);

        first.Close();

        Assert.True(host.IsOverlayOpen);
        Assert.Same(second, host.CurrentSnackbar);
        Assert.Equal(0, host.PendingCount);

        Assert.True(host.CloseCurrent());

        Assert.False(host.IsOverlayOpen);
        Assert.Equal(1, overlayClosed);
        Assert.Empty(host.Snackbars);
    }

    [Fact]
    public void FWSnackbarOverlayHost_ShouldSupportManualOverlayStateAndServiceRouting()
    {
        var host = new FWSnackbarOverlayHost
        {
            IsOverlayAutoOpenEnabled = false
        };
        var service = new FWSnackbarService();
        service.SetHost(host);

        var snackbar = service.Show(ToastSeverity.Information, "Saved", duration: TimeSpan.FromSeconds(8));

        Assert.Same(host, service.Host);
        Assert.True(snackbar.IsOpen);
        Assert.False(host.IsOverlayOpen);
        Assert.True(host.OpenOverlay());
        Assert.True(host.IsOverlayOpen);
        Assert.False(host.OpenOverlay());
        Assert.True(host.CloseOverlay());
        Assert.False(host.IsOverlayOpen);
        Assert.False(host.CloseOverlay());
        Assert.Throws<ArgumentException>(() => host.OverlayPlacement = (PopupPlacementMode)999);

        service.Clear();

        Assert.False(host.IsOverlayOpen);
        Assert.Empty(host.Snackbars);
    }

    [Fact]
    public void FWSnackbarOverlayHost_ShouldReportRootOverlayDiagnosticsAndServiceRouteChanges()
    {
        var rootHost = new FWSnackbarHost
        {
            MaxVisibleSnackbars = 1,
            Placement = FWSnackbarPlacement.Bottom,
            TransitionOffset = 16
        };
        var overlayTarget = new FWBorder();
        var overlayHost = new FWSnackbarOverlayHost
        {
            OverlayTarget = overlayTarget,
            OverlayPlacement = PopupPlacementMode.Top,
            MaxVisibleSnackbars = 2,
            Placement = FWSnackbarPlacement.Top,
            Spacing = 12,
            TransitionOffset = 20
        };
        var service = new FWSnackbarService();
        var hostChanges = 0;
        service.HostChanged += (_, _) => hostChanges++;
        service.SetHost(rootHost);
        service.SetHost(overlayHost);

        var transitions = new List<FWSnackbarTransitionRequestedEventArgs>();
        var queueChanges = new List<FWSnackbarHostQueueChangedEventArgs>();
        overlayHost.TransitionRequested += (_, args) => transitions.Add(args);
        overlayHost.QueueChanged += (_, args) => queueChanges.Add(args);

        var first = service.Show(ToastSeverity.Warning, "Overlay first", "Uses root overlay host.", "Review");
        var second = service.Enqueue(new FWSnackbar
        {
            Title = "Overlay second",
            IsAutoDismissEnabled = false
        });
        var diagnostics = overlayHost.GetDiagnostics();

        Assert.Equal(2, hostChanges);
        Assert.Same(overlayHost, service.Host);
        Assert.Same(overlayTarget, overlayHost.OverlayTarget);
        Assert.True(overlayHost.IsOverlayOpen);
        Assert.Equal(PopupPlacementMode.Top, overlayHost.OverlayPlacement);
        Assert.Same(first, overlayHost.CurrentSnackbar);
        Assert.True(first.IsOpen);
        Assert.True(second.IsOpen);
        Assert.Equal(2, diagnostics.VisibleCount);
        Assert.Equal(0, diagnostics.PendingCount);
        Assert.Equal(2, diagnostics.MaxVisibleSnackbars);
        Assert.Equal(FWSnackbarPlacement.Top, diagnostics.Placement);
        Assert.Equal(VerticalAlignment.Top, diagnostics.VerticalAlignment);
        Assert.Equal(HorizontalAlignment.Stretch, diagnostics.HorizontalAlignment);
        Assert.Equal(12, diagnostics.Spacing);
        Assert.Equal(20, diagnostics.TransitionOffset);
        Assert.True(diagnostics.HasCurrentSnackbar);
        Assert.False(diagnostics.HasPendingSnackbars);
        Assert.Equal(new[] { FWSnackbarTransitionKind.Show, FWSnackbarTransitionKind.Show }, transitions.Select(item => item.Kind).ToArray());
        Assert.Equal(new[] { FWSnackbarHostQueueChangeReason.Shown, FWSnackbarHostQueueChangeReason.Shown }, queueChanges.Select(item => item.Reason).ToArray());
        Assert.All(transitions, transition =>
        {
            Assert.Equal(FWSnackbarPlacement.Top, transition.Diagnostics.Placement);
            Assert.Equal(20, transition.Diagnostics.TransitionOffset);
            Assert.Equal(FWSnackbarPresenterState.Entering, transition.PresenterDiagnostics.PresenterState);
        });

        overlayHost.Placement = FWSnackbarPlacement.Bottom;
        overlayHost.OverlayPlacement = PopupPlacementMode.Bottom;
        diagnostics = overlayHost.GetDiagnostics();

        Assert.Equal(FWSnackbarPlacement.Bottom, diagnostics.Placement);
        Assert.Equal(VerticalAlignment.Bottom, diagnostics.VerticalAlignment);
        Assert.Equal(PopupPlacementMode.Bottom, overlayHost.OverlayPlacement);
        Assert.Equal(FWSnackbarHostQueueChangeReason.LayoutChanged, queueChanges[^1].Reason);

        service.Clear();

        Assert.False(overlayHost.IsOverlayOpen);
        Assert.Empty(overlayHost.Snackbars);
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void FWSnackbarHost_TemplateShouldApplyPlacementAlignmentToItemsControl()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);
            var host = new FWSnackbarHost
            {
                Width = 420,
                Height = 180,
                MaxVisibleSnackbars = 1,
                Placement = FWSnackbarPlacement.Top,
                Spacing = 14
            };
            host.Style = AssertStyle<FWSnackbarHost>(app.Resources);
            host.Enqueue(new FWSnackbar
            {
                Title = "Aligned",
                IsAutoDismissEnabled = false,
                Width = 360
            });

            host.ApplyTemplate();
            host.Measure(new Size(420, 180));
            host.Arrange(new Rect(0, 0, 420, 180));

            var itemsControl = FindVisualDescendant<ItemsControl>(host);
            Assert.NotNull(itemsControl);
            Assert.Equal(VerticalAlignment.Top, itemsControl!.VerticalAlignment);
            Assert.Equal(HorizontalAlignment.Stretch, itemsControl.HorizontalAlignment);

            host.Placement = FWSnackbarPlacement.Bottom;
            host.Measure(new Size(420, 180));
            host.Arrange(new Rect(0, 0, 420, 180));

            itemsControl = FindVisualDescendant<ItemsControl>(host);
            Assert.NotNull(itemsControl);
            Assert.Equal(VerticalAlignment.Bottom, itemsControl!.VerticalAlignment);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    public async Task FWSnackbarHost_ShouldExposeResultTasksForQueuedItems()
    {
        var host = new FWSnackbarHost
        {
            MaxVisibleSnackbars = 1
        };
        var first = new FWSnackbar { Title = "First", IsAutoDismissEnabled = false };
        var second = new FWSnackbar { Title = "Second", IsAutoDismissEnabled = false };

        var firstResult = host.EnqueueForResultAsync(first);
        var secondResult = host.EnqueueForResultAsync(second);

        Assert.True(first.IsOpen);
        Assert.False(second.IsOpen);
        Assert.False(secondResult.IsCompleted);

        Assert.True(first.RequestClose(FWSnackbarCloseReason.Action));

        Assert.Equal(FWSnackbarCloseReason.Action, await firstResult);
        Assert.True(second.IsOpen);

        host.Clear();

        Assert.Equal(FWSnackbarCloseReason.HostCleared, await secondResult);
        Assert.False(second.IsOpen);
        Assert.Empty(host.Snackbars);
    }

    [Fact]
    public void FWSnackbarHost_ShouldRespectClosingCancellationBeforePromotingPendingItems()
    {
        var host = new FWSnackbarHost
        {
            MaxVisibleSnackbars = 1
        };
        var first = new FWSnackbar { Title = "First", IsAutoDismissEnabled = false };
        var second = new FWSnackbar { Title = "Second", IsAutoDismissEnabled = false };
        first.Closing += (_, args) => args.Cancel = true;

        host.Enqueue(first);
        host.Enqueue(second);

        Assert.False(host.CloseCurrent());

        Assert.Same(first, host.CurrentSnackbar);
        Assert.True(first.IsOpen);
        Assert.False(second.IsOpen);
        Assert.Equal(1, host.PendingCount);

        host.Clear();
    }

    [Fact]
    public void FWSnackbarService_ShouldRequireHostAndRouteMessages()
    {
        var service = new FWSnackbarService();
        var hostChanged = 0;
        service.HostChanged += (_, _) => hostChanged++;

        Assert.Throws<InvalidOperationException>(() => service.Show(ToastSeverity.Information, "Saved"));

        var host = new FWSnackbarHost
        {
            MaxVisibleSnackbars = 1
        };
        service.SetHost(host);

        var first = service.Show(ToastSeverity.Information, "Saved", "Local state persisted.", "Open", TimeSpan.FromSeconds(4));
        var second = service.Enqueue(new FWSnackbar
        {
            Title = "Queued",
            IsAutoDismissEnabled = false
        });

        Assert.Equal(1, hostChanged);
        Assert.Same(host, service.Host);
        Assert.Same(first, host.CurrentSnackbar);
        Assert.Single(host.Snackbars);
        Assert.Equal(1, host.PendingCount);
        Assert.False(second.IsOpen);

        Assert.True(service.CloseCurrent());

        Assert.Same(second, host.CurrentSnackbar);
        Assert.True(second.IsOpen);

        service.Clear();

        Assert.Empty(host.Snackbars);
        Assert.Equal(FWSnackbarCloseReason.HostCleared, second.LastCloseReason);
    }

    [Fact]
    public async Task FWSnackbarService_ShouldExposeResultTasks()
    {
        var host = new FWSnackbarHost();
        var service = new FWSnackbarService();
        service.SetHost(host);

        var resultTask = service.EnqueueForResultAsync(new FWSnackbar
        {
            Title = "Awaitable",
            IsAutoDismissEnabled = false
        });

        Assert.Single(host.Snackbars);

        Assert.True(service.CloseCurrent());

        Assert.Equal(FWSnackbarCloseReason.Programmatic, await resultTask);
        Assert.Empty(host.Snackbars);
    }

    [Fact]
    public void GallerySampleCodeRegistry_ShouldExposeSnackbarRootOverlayHostQaSample()
    {
        Assert.True(GallerySampleCodeRegistry.TryGetRegisteredSampleCode("status.snackbar", out var sampleCode));

        Assert.Contains("new FWSnackbarHost", sampleCode, StringComparison.Ordinal);
        Assert.Contains("new FWSnackbarOverlayHost", sampleCode, StringComparison.Ordinal);
        Assert.Contains("OverlayTarget = rootElement", sampleCode, StringComparison.Ordinal);
        Assert.Contains("service.SetHost(rootHost)", sampleCode, StringComparison.Ordinal);
        Assert.Contains("service.SetHost(overlayHost)", sampleCode, StringComparison.Ordinal);
        Assert.Contains("FWSnackbarPlacement.Top", sampleCode, StringComparison.Ordinal);
        Assert.Contains("FWSnackbarPlacement.Bottom", sampleCode, StringComparison.Ordinal);
        Assert.Contains("PlacementMode.Top", sampleCode, StringComparison.Ordinal);
        Assert.Contains("PlacementMode.Bottom", sampleCode, StringComparison.Ordinal);
        Assert.Contains("TransitionRequested", sampleCode, StringComparison.Ordinal);
        Assert.Contains("QueueChanged", sampleCode, StringComparison.Ordinal);
        Assert.Contains("GetDiagnostics()", sampleCode, StringComparison.Ordinal);
        Assert.Contains("FWSnackbarCloseReason.CloseButton", sampleCode, StringComparison.Ordinal);
        Assert.Contains("ShouldKeepSnackbarOpen(args.Reason)", sampleCode, StringComparison.Ordinal);
        Assert.Contains("IsAutoDismissPausedOnPointerOverEnabled = true", sampleCode, StringComparison.Ordinal);
        Assert.Contains("IsAutoDismissPausedOnFocusEnabled = true", sampleCode, StringComparison.Ordinal);
        Assert.Contains("PauseAutoDismiss()", sampleCode, StringComparison.Ordinal);
        Assert.Contains("ResumeAutoDismiss()", sampleCode, StringComparison.Ordinal);
    }

    [Fact]
    public void FWInfoBar_ClosableAndIconStateShouldRemainInteractive()
    {
        var infoBar = new FWInfoBar
        {
            Title = "Review required",
            Message = "Long message",
            Severity = InfoBarSeverity.Warning,
            IsClosable = true,
            IsIconVisible = false,
            IsOpen = true
        };
        var closed = 0;
        infoBar.Closed += (_, _) => closed++;

        infoBar.IsOpen = false;

        Assert.Equal(InfoBarSeverity.Warning, infoBar.Severity);
        Assert.True(infoBar.IsClosable);
        Assert.False(infoBar.IsIconVisible);
        Assert.False(infoBar.IsOpen);
        Assert.Equal(1, closed);
    }

    [Fact]
    public void FWStatusBar_ShouldGenerateFwContainersForMixedStatusItems()
    {
        var statusBar = new FWStatusBar();
        statusBar.Items.Add("Ready");
        statusBar.Items.Add(new FWStatusBarItem { Content = "UTF-8" });
        statusBar.Items.Add(new FWStatusBarItem
        {
            Content = new FWInfoBadge
            {
                Severity = FWInfoBadgeSeverity.Success
            }
        });

        statusBar.Measure(new Size(480, 24));

        var itemsHost = Assert.IsAssignableFrom<Panel>(statusBar.GetVisualChild(0));
        Assert.Equal(3, itemsHost.Children.Count);
        Assert.All(itemsHost.Children, child => Assert.IsType<FWStatusBarItem>(child));
    }

    [Fact]
    public void FWNotificationStatusControls_ShouldExposeMaterialOperationsPanelState()
    {
        var infoBar = new FWInfoBar
        {
            Title = "Deployment monitor",
            Message = "One region has elevated latency.",
            Severity = InfoBarSeverity.Warning,
            IsOpen = true,
            IsClosable = true
        };
        var healthBadge = new FWInfoBadge
        {
            Severity = FWInfoBadgeSeverity.Caution,
            IconGlyph = "\uE930"
        };
        var eventBadge = new FWInfoBadge
        {
            Severity = FWInfoBadgeSeverity.Critical,
            Value = 128,
            MaxValue = 99
        };
        var toastHost = new FWToastNotificationHost
        {
            MaxVisibleToasts = 3,
            Position = ToastPosition.BottomRight,
            ToastWidth = 400,
            Spacing = 8
        };
        var success = toastHost.ShowSuccess("Deployment complete", "All services reported healthy.", TimeSpan.FromSeconds(12));
        var warning = toastHost.ShowWarning("Latency warning", "West region is above the preferred threshold.", TimeSpan.FromSeconds(12));
        success.IsAutoDismissEnabled = false;
        warning.IsAutoDismissEnabled = false;
        var snackbar = new FWSnackbar
        {
            Title = "Rollback available",
            Message = "A staged deployment can be reverted.",
            ActionContent = "Rollback",
            Severity = ToastSeverity.Warning,
            IsOpen = true
        };

        var statusBar = new FWStatusBar();
        statusBar.Items.Add(new FWStatusBarItem { Content = "Ready" });
        statusBar.Items.Add(new FWStatusBarItem { Content = "Region: West" });
        statusBar.Items.Add(new FWStatusBarItem { Content = healthBadge });
        statusBar.Items.Add(new FWStatusBarItem { Content = eventBadge });

        var panel = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Children =
            {
                infoBar,
                snackbar,
                toastHost,
                statusBar
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
            Child = panel
        };

        Assert.Equal(InfoBarSeverity.Warning, infoBar.Severity);
        Assert.True(infoBar.IsOpen);
        Assert.True(infoBar.IsClosable);
        Assert.Equal(FWInfoBadgeSeverity.Caution, healthBadge.Severity);
        Assert.Equal(FWInfoBadgeDisplayKind.Icon, healthBadge.DisplayKind);
        Assert.Equal(FWInfoBadgeSeverity.Critical, eventBadge.Severity);
        Assert.Equal("99+", eventBadge.DisplayValueText);
        Assert.Equal(ToastPosition.BottomRight, toastHost.Position);
        Assert.Equal(400, toastHost.ToastWidth);
        Assert.Equal(8, toastHost.Spacing);
        Assert.True(snackbar.IsOpen);
        Assert.Equal("Rollback", snackbar.ActionContent);
        Assert.Equal(ToastSeverity.Warning, snackbar.Severity);
        Assert.Equal(2, toastHost.Children.Count);
        Assert.Contains(success, toastHost.Children);
        Assert.Contains(warning, toastHost.Children);
        Assert.False(success.IsAutoDismissEnabled);
        Assert.False(warning.IsAutoDismissEnabled);
        Assert.Equal(4, statusBar.Items.Count);
        Assert.Same(healthBadge, ((FWStatusBarItem)statusBar.Items[2]).Content);
        Assert.Same(eventBadge, ((FWStatusBarItem)statusBar.Items[3]).Content);
        Assert.Equal(FWFluentMaterialKind.LiquidGlass, surface.MaterialKind);
        Assert.True(surface.LiquidGlass);
        Assert.Equal(70, surface.RefractionAmount);
        Assert.Equal(0.42, surface.ChromaticAberration);
        Assert.Equal(24, surface.FusionRadius);
        Assert.Equal(BorderShape.SuperEllipse, surface.Shape);
        Assert.Equal(4, surface.SuperEllipseN);
        Assert.Same(panel, surface.Child);
    }

    private sealed class RecordingCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public int ExecuteCount { get; private set; }

        public object? LastParameter { get; private set; }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            ExecuteCount++;
            LastParameter = parameter;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private static ResourceDictionary LoadGenericThemeDictionary()
    {
        var loaded = ResourceDictionary.SourceLoader?.Invoke(
            new ResourceDictionary(),
            new Uri("/FluentJalium;component/Themes/Generic.jalxaml", UriKind.Relative),
            FluentThemeManager.ThemeAssembly);

        return Assert.IsType<ResourceDictionary>(loaded);
    }

    private static void AssertContainsStyle<TControl>(ResourceDictionary dictionary)
        where TControl : FrameworkElement
    {
        Assert.True(dictionary.TryGetValue(typeof(TControl), out var value), $"{typeof(TControl).Name} style was not found.");
        Assert.IsType<Style>(value);
    }

    private static Style AssertStyle<TControl>(ResourceDictionary dictionary)
        where TControl : FrameworkElement
    {
        Assert.True(dictionary.TryGetValue(typeof(TControl), out var value), $"{typeof(TControl).Name} style was not found.");
        return Assert.IsType<Style>(value);
    }

    private static void AssertBasedOnStyle<TFluentControl, TJaliumControl>(ResourceDictionary dictionary)
        where TFluentControl : TJaliumControl, IFluentJaliumControl
        where TJaliumControl : FrameworkElement
    {
        var baseStyle = AssertStyle<TJaliumControl>(dictionary);
        var fluentStyle = AssertStyle<TFluentControl>(dictionary);

        Assert.Equal(typeof(TFluentControl), fluentStyle.TargetType);
        Assert.Same(baseStyle, fluentStyle.BasedOn);
    }

    private static void AssertOwnedStyle<TFluentControl>(ResourceDictionary dictionary)
        where TFluentControl : FrameworkElement, IFluentJaliumControl
    {
        var fluentStyle = AssertStyle<TFluentControl>(dictionary);

        Assert.Equal(typeof(TFluentControl), fluentStyle.TargetType);
        Assert.Null(fluentStyle.BasedOn);
    }

    private static void AssertSetter(Style style, DependencyProperty property, object? expectedValue = null)
    {
        var setter = Assert.Single(style.Setters, s => s.Property == property);
        if (expectedValue != null)
        {
            if (expectedValue is int expectedInt)
            {
                Assert.Equal(expectedInt, Convert.ToInt32(setter.Value));
            }
            else if (expectedValue is double expectedDouble)
            {
                Assert.Equal(expectedDouble, Convert.ToDouble(setter.Value));
            }
            else
            {
                Assert.Equal(expectedValue, setter.Value);
            }
        }
    }

    private static T? FindVisualDescendant<T>(Visual visual)
        where T : Visual
    {
        if (visual is T candidate)
        {
            return candidate;
        }

        for (var index = 0; index < visual.VisualChildrenCount; index++)
        {
            var child = visual.GetVisualChild(index);
            if (child != null && FindVisualDescendant<T>(child) is { } match)
            {
                return match;
            }
        }

        return null;
    }

    private static void CompleteSnackbarPresenterTransition(FWSnackbar snackbar, FWSnackbarPresenterState completedState)
    {
        typeof(FWSnackbar)
            .GetMethod("CompletePresenterTransition", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(snackbar, [completedState]);
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
