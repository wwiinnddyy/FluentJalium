using System.Diagnostics.CodeAnalysis;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;
using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using Jalium.UI;
using Jalium.UI.Automation;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Themes;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Documents;
using Jalium.UI.Input;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using JaliumThemeManager = Jalium.UI.Controls.Themes.ThemeManager;
using ICommand = System.Windows.Input.ICommand;

namespace FluentJalium.Tests;

[Collection("Application")]
public sealed class FluentDisclosureControlsTests
{
    private static readonly string[] DisclosureResourceKeys =
    [
        "ToolTipBackground",
        "ToolTipForeground",
        "ToolTipBorderBrush",
        "GroupBoxBackground",
        "GroupBoxBorderBrush",
        "GroupBoxHeaderBackground",
        "ExpanderBackground",
        "ExpanderBorderBrush",
        "ExpanderBorderBrushHover",
        "ExpanderHeaderBackground",
        "ExpanderHeaderBackgroundHover",
        "ExpanderHeaderBackgroundExpanded",
        "ExpanderChevronForeground",
        "ContentDialogBackground",
        "ContentDialogBorderBrush",
        "ContentDialogOverlayBackground",
        "ContentDialogTitleForeground",
        "ContentDialogButtonPanelBackground"
    ];

    [Theory]
    [InlineData(FluentThemeVariant.Dark)]
    [InlineData(FluentThemeVariant.Light)]
    [InlineData(FluentThemeVariant.HighContrast)]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void ThemeDictionary_ShouldExposeDisclosureTokensForEveryTheme(FluentThemeVariant theme)
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = theme.ToString();

        var dictionary = LoadGenericThemeDictionary();

        foreach (var key in DisclosureResourceKeys)
        {
            Assert.True(dictionary.TryGetValue(key, out var value), $"{key} was not found in the {theme} theme.");
            Assert.IsType<SolidColorBrush>(value);
        }

        ResetApplicationState();
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void Apply_ShouldRegisterFwDisclosureStylesBasedOnJaliumStyles()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            AssertBasedOnStyle<FWExpander, Expander>(app.Resources);
            AssertStyle<FWSettingsExpander>(app.Resources);
            AssertBasedOnStyle<FWToolTip, ToolTip>(app.Resources);
            AssertBasedOnStyle<FWContentDialog, ContentDialog>(app.Resources);
            AssertOwnedStyle<FWTeachingTip>(app.Resources);
            AssertOwnedStyle<FWTaskDialog>(app.Resources);
            AssertOwnedStyle<FWTaskDialogHost>(app.Resources);
            AssertBasedOnStyle<FWGroupBox, GroupBox>(app.Resources);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void DisclosureTheme_ShouldDefineBaseStylesAndFluentSetters()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = FluentThemeVariant.Dark.ToString();

        var dictionary = LoadDisclosureControlThemeDictionary();

        var toolTipStyle = AssertStyle<ToolTip>(dictionary);
        AssertSetter(toolTipStyle, Control.BackgroundProperty);
        AssertSetter(toolTipStyle, Control.ForegroundProperty);
        AssertSetter(toolTipStyle, Control.BorderBrushProperty);
        AssertSetter(toolTipStyle, Control.PaddingProperty);

        var fwToolTipStyle = AssertStyle<FWToolTip>(dictionary);
        Assert.Same(toolTipStyle, fwToolTipStyle.BasedOn);
        AssertSetter(fwToolTipStyle, FWToolTip.DensityProperty);

        var groupBoxStyle = AssertStyle<GroupBox>(dictionary);
        AssertSetter(groupBoxStyle, Control.BackgroundProperty);
        AssertSetter(groupBoxStyle, Control.BorderBrushProperty);
        AssertSetter(groupBoxStyle, GroupBox.HeaderBackgroundProperty);
        AssertSetter(groupBoxStyle, Control.PaddingProperty);

        var fwGroupBoxStyle = AssertStyle<FWGroupBox>(dictionary);
        Assert.Same(groupBoxStyle, fwGroupBoxStyle.BasedOn);
        AssertSetter(fwGroupBoxStyle, FWGroupBox.DensityProperty);

        var expanderStyle = AssertStyle<Expander>(dictionary);
        AssertSetter(expanderStyle, Control.BackgroundProperty);
        AssertSetter(expanderStyle, Control.BorderBrushProperty);
        AssertSetter(expanderStyle, Expander.HeaderBackgroundProperty);
        AssertSetter(expanderStyle, Control.PaddingProperty);
        AssertSetter(expanderStyle, FrameworkElement.MinHeightProperty);

        var fwExpanderStyle = AssertStyle<FWExpander>(dictionary);
        Assert.Same(expanderStyle, fwExpanderStyle.BasedOn);
        AssertSetter(fwExpanderStyle, FWExpander.DensityProperty);

        var settingsExpanderStyle = AssertStyle<FWSettingsExpander>(dictionary);
        Assert.Same(expanderStyle, settingsExpanderStyle.BasedOn);
        AssertSetter(settingsExpanderStyle, FrameworkElement.MinHeightProperty);
        AssertSetter(settingsExpanderStyle, Control.PaddingProperty);
        AssertSetter(settingsExpanderStyle, FWSettingsExpander.ItemsPanelProperty);

        var contentDialogStyle = AssertStyle<ContentDialog>(dictionary);
        AssertSetter(contentDialogStyle, Control.BackgroundProperty);
        AssertSetter(contentDialogStyle, Control.BorderBrushProperty);
        AssertSetter(contentDialogStyle, Control.PaddingProperty);
        AssertSetter(contentDialogStyle, FrameworkElement.MinWidthProperty);
        AssertSetter(contentDialogStyle, FrameworkElement.MaxWidthProperty);

        var fwContentDialogStyle = AssertStyle<FWContentDialog>(dictionary);
        Assert.Same(contentDialogStyle, fwContentDialogStyle.BasedOn);
        AssertSetter(fwContentDialogStyle, FWContentDialog.DensityProperty);

        var teachingTipStyle = AssertStyle<FWTeachingTip>(dictionary);
        Assert.Null(teachingTipStyle.BasedOn);
        AssertSetter(teachingTipStyle, Control.BackgroundProperty);
        AssertSetter(teachingTipStyle, Control.ForegroundProperty);
        AssertSetter(teachingTipStyle, Control.BorderBrushProperty);
        AssertSetter(teachingTipStyle, Control.PaddingProperty);
        AssertSetter(teachingTipStyle, FrameworkElement.MinWidthProperty);
        AssertSetter(teachingTipStyle, FrameworkElement.MaxWidthProperty);
        AssertSetter(teachingTipStyle, Control.TemplateProperty);

        var taskDialogStyle = AssertStyle<FWTaskDialog>(dictionary);
        Assert.Null(taskDialogStyle.BasedOn);
        AssertSetter(taskDialogStyle, FWTaskDialog.DensityProperty);
        AssertSetter(taskDialogStyle, FWTaskDialog.DefaultButtonProperty);
        AssertSetter(taskDialogStyle, FWTaskDialog.CancelButtonProperty);
        AssertSetter(taskDialogStyle, Control.PaddingProperty);

        var taskDialogHostStyle = AssertStyle<FWTaskDialogHost>(dictionary);
        Assert.Null(taskDialogHostStyle.BasedOn);
        AssertSetter(taskDialogHostStyle, Control.BackgroundProperty);
        AssertSetter(taskDialogHostStyle, Control.PaddingProperty);
        AssertSetter(taskDialogHostStyle, FWTaskDialogHost.IsLightDismissEnabledProperty);
        AssertSetter(taskDialogHostStyle, FWTaskDialogHost.IsFocusTrapEnabledProperty);
        AssertSetter(taskDialogHostStyle, FWTaskDialogHost.RestoreFocusOnCloseProperty);
        AssertSetter(taskDialogHostStyle, Control.HorizontalContentAlignmentProperty);
        AssertSetter(taskDialogHostStyle, Control.VerticalContentAlignmentProperty);
        AssertSetter(taskDialogHostStyle, Control.TemplateProperty);

        ResetApplicationState();
    }

    [Fact]
    public void FWDisclosureControls_ShouldApplyDensityPresets()
    {
        var expander = new FWExpander();

        Assert.Equal(FWDisclosureDensity.Comfortable, expander.Density);
        Assert.Equal(new Thickness(14), expander.Padding);
        Assert.Equal(40, expander.MinHeight);

        expander.Density = FWDisclosureDensity.Compact;

        Assert.Equal(new Thickness(10), expander.Padding);
        Assert.Equal(36, expander.MinHeight);

        expander.Density = FWDisclosureDensity.Spacious;

        Assert.Equal(new Thickness(18), expander.Padding);
        Assert.Equal(48, expander.MinHeight);

        var toolTip = new FWToolTip
        {
            Density = FWDisclosureDensity.Compact
        };

        Assert.Equal(new Thickness(6, 3, 6, 3), toolTip.Padding);
        Assert.Equal(24, toolTip.MinHeight);

        toolTip.Density = FWDisclosureDensity.Spacious;

        Assert.Equal(new Thickness(10, 7, 10, 7), toolTip.Padding);
        Assert.Equal(32, toolTip.MinHeight);

        var dialog = new FWContentDialog();

        Assert.Equal(FWDisclosureDensity.Comfortable, dialog.Density);
        Assert.Equal(new Thickness(24), dialog.Padding);
        Assert.Equal(320, dialog.MinWidth);
        Assert.Equal(548, dialog.MaxWidth);

        dialog.Density = FWDisclosureDensity.Compact;

        Assert.Equal(new Thickness(20), dialog.Padding);
        Assert.Equal(300, dialog.MinWidth);
        Assert.Equal(520, dialog.MaxWidth);

        dialog.Density = FWDisclosureDensity.Spacious;

        Assert.Equal(new Thickness(28), dialog.Padding);
        Assert.Equal(340, dialog.MinWidth);
        Assert.Equal(600, dialog.MaxWidth);

        var taskDialog = new FWTaskDialog();

        Assert.Equal(FWDisclosureDensity.Comfortable, taskDialog.Density);
        Assert.Equal(new Thickness(24), taskDialog.Padding);
        Assert.Equal(320, taskDialog.MinWidth);
        Assert.Equal(548, taskDialog.MaxWidth);

        taskDialog.Density = FWDisclosureDensity.Compact;

        Assert.Equal(new Thickness(20), taskDialog.Padding);
        Assert.Equal(300, taskDialog.MinWidth);
        Assert.Equal(520, taskDialog.MaxWidth);

        var groupBox = new FWGroupBox
        {
            Density = FWDisclosureDensity.Spacious
        };

        Assert.Equal(new Thickness(18, 20, 18, 18), groupBox.Padding);
        Assert.Equal(72, groupBox.MinHeight);

        groupBox.Density = FWDisclosureDensity.Compact;

        Assert.Equal(new Thickness(10, 12, 10, 10), groupBox.Padding);
        Assert.Equal(48, groupBox.MinHeight);
    }

    [Fact]
    public void FWTeachingTip_ShouldExposeTargetActionAndClosingSemantics()
    {
        var target = new FWButton { Content = "Target" };
        var icon = new FWFontIcon();
        var hero = new FWBorder { Width = 120, Height = 48 };
        var content = new FWTextBlock { Text = "Keyboard hint" };
        var tip = new FWTeachingTip
        {
            Target = target,
            Title = "Try search suggestions",
            Subtitle = "TeachingTip anchors contextual guidance to a control.",
            IconSource = icon,
            HeroContent = hero,
            Content = content,
            ActionButtonContent = "Open docs",
            ActionButtonCommandParameter = "docs",
            CloseButtonContent = "Got it",
            PreferredPlacement = TeachingTipPlacementMode.BottomRight,
            TailVisibility = TeachingTipTailVisibility.Visible,
            IsLightDismissEnabled = false,
            IsOpen = true
        };

        Assert.IsAssignableFrom<IFluentJaliumControl>(tip);
        Assert.Same(target, tip.Target);
        Assert.Equal("Try search suggestions", tip.Title);
        Assert.Equal("TeachingTip anchors contextual guidance to a control.", tip.Subtitle);
        Assert.Same(icon, tip.IconSource);
        Assert.Same(hero, tip.HeroContent);
        Assert.Same(content, tip.Content);
        Assert.Equal("Open docs", tip.ActionButtonContent);
        Assert.Equal("docs", tip.ActionButtonCommandParameter);
        Assert.Equal("Got it", tip.CloseButtonContent);
        Assert.Equal(TeachingTipPlacementMode.BottomRight, tip.PreferredPlacement);
        Assert.Equal(TeachingTipTailVisibility.Visible, tip.TailVisibility);
        Assert.False(tip.IsLightDismissEnabled);
        Assert.True(tip.IsOpen);

        var closingArgs = new TeachingTipClosingEventArgs(FWTeachingTip.ClosingEvent, tip)
        {
            Reason = TeachingTipCloseReason.CloseButton,
            Cancel = true
        };
        var closedArgs = new TeachingTipClosedEventArgs(FWTeachingTip.ClosedEvent, tip)
        {
            Reason = TeachingTipCloseReason.Programmatic
        };

        Assert.Equal(TeachingTipCloseReason.CloseButton, closingArgs.Reason);
        Assert.True(closingArgs.Cancel);
        Assert.Equal(TeachingTipCloseReason.Programmatic, closedArgs.Reason);
    }

    [Fact]
    public async Task FWTaskDialog_ShouldExposeAwaitableResultAndDefaultCancelRequests()
    {
        var dialog = new FWTaskDialog
        {
            Title = "Reset defaults?",
            PrimaryButtonText = "Reset",
            SecondaryButtonText = "Review",
            CloseButtonText = "Cancel",
            DefaultButton = FWTaskDialogButton.Secondary,
            CancelButton = FWTaskDialogButton.Close
        };
        var openingCount = 0;
        var openedCount = 0;
        var closingCount = 0;
        var closedCount = 0;
        var cancelClose = true;
        FWTaskDialogResult? closedResult = null;
        dialog.Opening += (_, _) => openingCount++;
        dialog.Opened += (_, _) => openedCount++;
        dialog.Closing += (_, args) =>
        {
            closingCount++;
            if (cancelClose && args.Result == FWTaskDialogResult.Close)
            {
                args.Cancel = true;
            }
        };
        dialog.Closed += (_, args) =>
        {
            closedCount++;
            closedResult = args.Result;
        };

        var showTask = dialog.ShowAsync();

        Assert.True(dialog.IsOpen);
        Assert.Equal(FWTaskDialogResult.None, dialog.Result);
        Assert.Equal(1, openingCount);
        Assert.Equal(1, openedCount);

        Assert.False(dialog.RequestCancelButtonClick());
        Assert.True(dialog.IsOpen);
        Assert.False(showTask.IsCompleted);

        cancelClose = false;

        Assert.True(dialog.RequestDefaultButtonClick());

        var result = await showTask;

        Assert.Equal(FWTaskDialogResult.Secondary, result);
        Assert.Equal(FWTaskDialogResult.Secondary, dialog.Result);
        Assert.False(dialog.IsOpen);
        Assert.Equal(2, closingCount);
        Assert.Equal(1, closedCount);
        Assert.Equal(FWTaskDialogResult.Secondary, closedResult);
    }

    [Fact]
    public async Task FWTaskDialog_ShouldReuseOpenShowTaskAndCloseFromCancellationToken()
    {
        var dialog = new FWTaskDialog
        {
            Title = "Upload changes?",
            PrimaryButtonText = "Upload",
            CloseButtonText = "Cancel",
            DefaultButton = FWTaskDialogButton.Primary,
            CancelButton = FWTaskDialogButton.Close
        };
        using var cancellation = new CancellationTokenSource();

        var firstShowTask = dialog.ShowAsync(cancellation.Token);
        var secondShowTask = dialog.ShowAsync();

        Assert.Same(firstShowTask, secondShowTask);
        Assert.True(dialog.IsOpen);

        cancellation.Cancel();

        var result = await firstShowTask;

        Assert.Equal(FWTaskDialogResult.Close, result);
        Assert.Equal(FWTaskDialogResult.Close, dialog.Result);
        Assert.False(dialog.IsOpen);
    }

    [Fact]
    public async Task FWTaskDialog_ShouldExecuteButtonCommandsAndRespectCanExecute()
    {
        var primaryCommand = new RecordingCommand();
        var secondaryCommand = new RecordingCommand
        {
            CanExecuteResult = false
        };
        var dialog = new FWTaskDialog
        {
            Title = "Delete temporary layout cache?",
            PrimaryButtonText = "Delete",
            SecondaryButtonText = "Archive",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = primaryCommand,
            PrimaryButtonCommandParameter = "delete-cache",
            SecondaryButtonCommand = secondaryCommand,
            SecondaryButtonCommandParameter = "archive-cache",
            DefaultButton = FWTaskDialogButton.Primary,
            CancelButton = FWTaskDialogButton.Close
        };
        FWTaskDialogButtonClickEventArgs? primaryClick = null;
        FWTaskDialogButtonClickEventArgs? secondaryClick = null;
        dialog.PrimaryButtonClick += (_, args) => primaryClick = args;
        dialog.SecondaryButtonClick += (_, args) => secondaryClick = args;

        var showTask = dialog.ShowAsync();

        Assert.False(dialog.RequestSecondaryButtonClick());
        Assert.True(dialog.IsOpen);
        Assert.False(showTask.IsCompleted);
        Assert.Null(secondaryClick);
        Assert.Equal(0, secondaryCommand.ExecuteCount);

        Assert.True(dialog.RequestPrimaryButtonClick());

        var result = await showTask;

        Assert.Equal(FWTaskDialogResult.Primary, result);
        Assert.Equal(FWTaskDialogResult.Primary, dialog.Result);
        Assert.False(dialog.IsOpen);
        Assert.NotNull(primaryClick);
        Assert.True(primaryClick.CommandExecuted);
        Assert.Equal(FWTaskDialogResult.Primary, primaryClick.Result);
        Assert.Equal(1, primaryCommand.ExecuteCount);
        Assert.Equal("delete-cache", primaryCommand.LastParameter);
    }

    [Fact]
    public async Task FWTaskDialog_ShouldRouteEscapeToCancelButtonCommand()
    {
        var secondaryCommand = new RecordingCommand();
        var dialog = new FWTaskDialog
        {
            Title = "Archive draft?",
            PrimaryButtonText = "Publish",
            SecondaryButtonText = "Archive",
            CloseButtonText = "Cancel",
            SecondaryButtonCommand = secondaryCommand,
            SecondaryButtonCommandParameter = "archive-draft",
            DefaultButton = FWTaskDialogButton.Primary,
            CancelButton = FWTaskDialogButton.Secondary
        };
        FWTaskDialogButtonClickEventArgs? secondaryClick = null;
        dialog.SecondaryButtonClick += (_, args) => secondaryClick = args;

        var showTask = dialog.ShowAsync();
        var args = new Jalium.UI.Input.KeyEventArgs(
            UIElement.KeyDownEvent,
            Jalium.UI.Input.Key.Escape,
            Jalium.UI.Input.ModifierKeys.None,
            isDown: true,
            isRepeat: false,
            timestamp: 0);

        dialog.RaiseEvent(args);
        var result = await showTask;

        Assert.True(args.Handled);
        Assert.Equal(FWTaskDialogResult.Secondary, result);
        Assert.Equal(FWTaskDialogResult.Secondary, dialog.Result);
        Assert.False(dialog.IsOpen);
        Assert.NotNull(secondaryClick);
        Assert.True(secondaryClick.CommandExecuted);
        Assert.Equal(1, secondaryCommand.ExecuteCount);
        Assert.Equal("archive-draft", secondaryCommand.LastParameter);
    }

    [Fact]
    public void FWTaskDialog_ShouldExposeAutomationPeerAndDiagnostics()
    {
        var secondaryCommand = new RecordingCommand
        {
            CanExecuteResult = false
        };
        var dialog = new FWTaskDialog
        {
            Title = "Delete temporary layout cache?",
            Subtitle = "This action can be reviewed before it closes the dialog.",
            PrimaryButtonText = "Delete",
            SecondaryButtonText = "Archive",
            CloseButtonText = "Cancel",
            DefaultButton = FWTaskDialogButton.Primary,
            CancelButton = FWTaskDialogButton.Close,
            SecondaryButtonCommand = secondaryCommand
        };

        var peer = Assert.IsType<FWTaskDialogAutomationPeer>(dialog.GetAutomationPeer());

        Assert.Equal(nameof(FWTaskDialog), peer.GetClassName());
        Assert.Equal(AutomationControlType.Window, peer.GetAutomationControlType());
        Assert.Equal("Delete temporary layout cache?", peer.GetName());
        Assert.Equal("This action can be reviewed before it closes the dialog.", peer.GetHelpText());

        var diagnostics = dialog.GetAutomationDiagnostics();

        Assert.Equal(nameof(FWTaskDialog), diagnostics.ClassName);
        Assert.Equal(AutomationControlType.Window, diagnostics.ControlType);
        Assert.Equal("Delete temporary layout cache?", diagnostics.Name);
        Assert.Equal("This action can be reviewed before it closes the dialog.", diagnostics.HelpText);
        Assert.Equal(FWTaskDialogButton.None, diagnostics.LastFocusTarget);

        Assert.Equal(FWTaskDialogButton.Primary, diagnostics.PrimaryButton.Button);
        Assert.Equal("PrimaryButton", diagnostics.PrimaryButton.AutomationId);
        Assert.Equal("Delete", diagnostics.PrimaryButton.Name);
        Assert.Equal("Default button", diagnostics.PrimaryButton.HelpText);
        Assert.True(diagnostics.PrimaryButton.IsVisible);
        Assert.True(diagnostics.PrimaryButton.IsEnabled);
        Assert.True(diagnostics.PrimaryButton.IsDefault);
        Assert.False(diagnostics.PrimaryButton.IsCancel);

        Assert.Equal("SecondaryButton", diagnostics.SecondaryButton.AutomationId);
        Assert.Equal("Archive", diagnostics.SecondaryButton.Name);
        Assert.Equal("Task dialog button", diagnostics.SecondaryButton.HelpText);
        Assert.True(diagnostics.SecondaryButton.IsVisible);
        Assert.False(diagnostics.SecondaryButton.IsEnabled);

        Assert.Equal("CloseButton", diagnostics.CloseButton.AutomationId);
        Assert.Equal("Cancel", diagnostics.CloseButton.Name);
        Assert.Equal("Cancel button", diagnostics.CloseButton.HelpText);
        Assert.True(diagnostics.CloseButton.IsVisible);
        Assert.True(diagnostics.CloseButton.IsEnabled);
        Assert.False(diagnostics.CloseButton.IsDefault);
        Assert.True(diagnostics.CloseButton.IsCancel);
    }

    [Fact]
    public async Task FWTaskDialogHost_ShouldExposeAutomationPeerForCurrentDialog()
    {
        var host = new FWTaskDialogHost();
        var dialog = new FWTaskDialog
        {
            Title = "Replace runtime settings?",
            Subtitle = "The current profile will be updated.",
            PrimaryButtonText = "Replace",
            CloseButtonText = "Cancel"
        };
        var peer = Assert.IsType<FWTaskDialogHostAutomationPeer>(host.GetAutomationPeer());

        Assert.Equal(nameof(FWTaskDialogHost), peer.GetClassName());
        Assert.Equal(AutomationControlType.Pane, peer.GetAutomationControlType());
        Assert.Equal(nameof(FWTaskDialogHost), peer.GetName());
        Assert.Equal("Task dialog modal host with light dismiss", peer.GetHelpText());

        var showTask = host.ShowAsync(dialog);

        Assert.Equal("Replace runtime settings?", peer.GetName());
        Assert.Equal("The current profile will be updated.", peer.GetHelpText());

        Assert.True(dialog.RequestPrimaryButtonClick());
        var result = await showTask;

        Assert.Equal(FWTaskDialogResult.Primary, result);
        Assert.Equal(nameof(FWTaskDialogHost), peer.GetName());
        Assert.Equal("Task dialog modal host with light dismiss", peer.GetHelpText());
    }

    [Fact]
    public async Task FWTaskDialogHost_ShouldShowDialogWithOverlayStateAndReturnResult()
    {
        var host = new FWTaskDialogHost();
        var restoreTarget = new FWButton { Content = "Restore" };
        var dialog = new FWTaskDialog
        {
            Title = "Reset defaults?",
            PrimaryButtonText = "Reset",
            CloseButtonText = "Cancel",
            Focusable = true
        };
        host.FocusRestoreTarget = restoreTarget;

        var showTask = host.ShowAsync(dialog);

        Assert.True(host.IsOpen);
        Assert.Same(dialog, host.CurrentDialog);
        Assert.Same(dialog, host.Content);
        Assert.True(dialog.IsOpen);
        var diagnostics = host.GetDiagnostics();
        Assert.True(diagnostics.IsOpen);
        Assert.True(diagnostics.HasCurrentDialog);
        Assert.True(diagnostics.IsLightDismissEnabled);
        Assert.True(diagnostics.IsFocusTrapEnabled);
        Assert.True(diagnostics.RestoreFocusOnClose);
        Assert.True(diagnostics.HasFocusRestoreTarget);

        Assert.True(dialog.RequestPrimaryButtonClick());

        var result = await showTask;

        Assert.Equal(FWTaskDialogResult.Primary, result);
        Assert.False(host.IsOpen);
        Assert.Null(host.CurrentDialog);
        Assert.Null(host.Content);
        Assert.False(dialog.IsOpen);
    }

    [Fact]
    public async Task FWTaskDialogHost_ShouldRouteLightDismissThroughCancelButton()
    {
        var host = new FWTaskDialogHost();
        var closeCommand = new RecordingCommand();
        var dialog = new FWTaskDialog
        {
            Title = "Archive draft?",
            PrimaryButtonText = "Publish",
            CloseButtonText = "Cancel",
            CancelButton = FWTaskDialogButton.Close,
            CloseButtonCommand = closeCommand,
            CloseButtonCommandParameter = "light-dismiss"
        };
        var cancelClose = true;
        var closeRequests = 0;
        dialog.CloseButtonClick += (_, args) =>
        {
            closeRequests++;
            args.Cancel = cancelClose;
        };

        var showTask = host.ShowAsync(dialog);

        Assert.False(host.RequestLightDismiss());
        Assert.True(host.IsOpen);
        Assert.True(dialog.IsOpen);
        Assert.False(showTask.IsCompleted);
        Assert.Equal(1, closeRequests);
        Assert.Equal(1, closeCommand.ExecuteCount);
        Assert.Equal("light-dismiss", closeCommand.LastParameter);

        cancelClose = false;

        Assert.True(host.RequestLightDismiss());

        var result = await showTask;

        Assert.Equal(FWTaskDialogResult.Close, result);
        Assert.False(host.IsOpen);
        Assert.Null(host.CurrentDialog);
        Assert.Equal(2, closeRequests);
        Assert.Equal(2, closeCommand.ExecuteCount);
    }

    [Fact]
    public async Task FWTaskDialogHost_ShouldTrapKeyboardRequestsWithinCurrentDialog()
    {
        var host = new FWTaskDialogHost();
        var secondaryCommand = new RecordingCommand();
        var dialog = new FWTaskDialog
        {
            Title = "Review draft?",
            PrimaryButtonText = "Publish",
            SecondaryButtonText = "Archive",
            CloseButtonText = "Cancel",
            DefaultButton = FWTaskDialogButton.Primary,
            CancelButton = FWTaskDialogButton.Secondary,
            SecondaryButtonCommand = secondaryCommand,
            SecondaryButtonCommandParameter = "archive"
        };
        var showTask = host.ShowAsync(dialog);
        var tabArgs = new KeyEventArgs(
            UIElement.KeyDownEvent,
            Key.Tab,
            ModifierKeys.None,
            isDown: true,
            isRepeat: false,
            timestamp: 0);

        host.RaiseEvent(tabArgs);

        Assert.True(tabArgs.Handled);
        Assert.Equal(FWTaskDialogHostKeyboardRequest.TabForward, host.LastKeyboardRequest);
        Assert.True(host.LastKeyboardRequestHandled);
        Assert.True(host.IsOpen);
        Assert.Same(dialog, host.CurrentDialog);

        var escapeArgs = new KeyEventArgs(
            UIElement.KeyDownEvent,
            Key.Escape,
            ModifierKeys.None,
            isDown: true,
            isRepeat: false,
            timestamp: 0);

        host.RaiseEvent(escapeArgs);

        var result = await showTask;

        Assert.True(escapeArgs.Handled);
        Assert.Equal(FWTaskDialogHostKeyboardRequest.EscapeCancel, host.GetDiagnostics().LastKeyboardRequest);
        Assert.True(host.GetDiagnostics().LastKeyboardRequestHandled);
        Assert.Equal(FWTaskDialogResult.Secondary, result);
        Assert.Equal(1, secondaryCommand.ExecuteCount);
        Assert.Equal("archive", secondaryCommand.LastParameter);
        Assert.False(host.IsOpen);
    }

    [Fact]
    public async Task FWTaskDialogHost_ShouldExposeFocusDiagnosticsAndReuseCurrentDialogTask()
    {
        var host = new FWTaskDialogHost
        {
            IsFocusTrapEnabled = false,
            RestoreFocusOnClose = false
        };
        var dialog = new FWTaskDialog
        {
            Title = "Replace runtime settings?",
            PrimaryButtonText = "Replace",
            CloseButtonText = "Cancel",
            DefaultButton = FWTaskDialogButton.Primary,
            CancelButton = FWTaskDialogButton.Close
        };
        var otherDialog = new FWTaskDialog
        {
            Title = "Other dialog",
            CloseButtonText = "Close"
        };

        var showTask = host.ShowAsync(dialog);
        var duplicateTask = host.ShowAsync(dialog);

        Assert.Same(showTask, duplicateTask);
        Assert.Throws<InvalidOperationException>(() =>
        {
            _ = host.ShowAsync(otherDialog);
        });

        var shiftTabArgs = new KeyEventArgs(
            UIElement.KeyDownEvent,
            Key.Tab,
            ModifierKeys.Shift,
            isDown: true,
            isRepeat: false,
            timestamp: 0);

        host.RaiseEvent(shiftTabArgs);

        Assert.False(shiftTabArgs.Handled);
        Assert.Equal(FWTaskDialogHostKeyboardRequest.TabBackward, host.LastKeyboardRequest);
        Assert.False(host.LastKeyboardRequestHandled);
        var diagnostics = host.GetDiagnostics();
        Assert.True(diagnostics.IsOpen);
        Assert.True(diagnostics.HasCurrentDialog);
        Assert.False(diagnostics.IsFocusTrapEnabled);
        Assert.False(diagnostics.RestoreFocusOnClose);
        Assert.False(diagnostics.HasFocusRestoreTarget);

        host.IsFocusTrapEnabled = true;
        var tabArgs = new KeyEventArgs(
            UIElement.KeyDownEvent,
            Key.Tab,
            ModifierKeys.None,
            isDown: true,
            isRepeat: false,
            timestamp: 0);

        host.RaiseEvent(tabArgs);

        Assert.True(tabArgs.Handled);
        Assert.Equal(FWTaskDialogHostKeyboardRequest.TabForward, host.LastKeyboardRequest);
        Assert.True(host.LastKeyboardRequestHandled);

        Assert.True(host.Close(FWTaskDialogResult.Close));
        var result = await showTask;

        Assert.Equal(FWTaskDialogResult.Close, result);
        Assert.False(host.IsOpen);
        Assert.False(host.GetDiagnostics().HasCurrentDialog);
    }

    [Fact]
    public void FWSettingsExpander_ShouldExposeSettingsRowsAndSupplementaryContent()
    {
        var firstRow = new FWSettingsCard
        {
            Header = "App theme",
            Description = "Use system setting"
        };
        var secondRow = new FWSettingsCard
        {
            Header = "Window material",
            Description = "Open preview",
            IsClickEnabled = true
        };
        var itemTemplate = new DataTemplate();
        var itemsPanel = new ItemsPanelTemplate();
        var settingsContent = new FWTextBlock { Text = "Footer" };
        var legacyContent = new FWTextBlock { Text = "Legacy content" };
        var expander = new FWSettingsExpander
        {
            Header = "Appearance",
            Description = "Grouped settings rows",
            HeaderIcon = new FWFontIcon(),
            ItemTemplate = itemTemplate,
            ItemsPanel = itemsPanel,
            SettingsContent = settingsContent,
            Content = legacyContent
        };
        var itemEvents = new List<NotifyCollectionChangedAction>();
        expander.ItemsChanged += (_, e) => itemEvents.Add(e.Action);

        var contentAttribute = typeof(FWSettingsExpander).GetCustomAttribute<ContentPropertyAttribute>();
        Assert.Equal(nameof(FWSettingsExpander.Items), contentAttribute?.Name);

        Assert.Equal(0, expander.ItemCount);

        expander.AddSetting(firstRow);
        expander.AddSetting(secondRow);

        Assert.Equal(2, expander.Items.Count);
        Assert.Equal(2, expander.ItemCount);
        Assert.Contains(firstRow, expander.Items);
        Assert.Contains(secondRow, expander.Items);
        Assert.Same(itemTemplate, expander.ItemTemplate);
        Assert.Same(itemsPanel, expander.ItemsPanel);
        Assert.Same(settingsContent, expander.SettingsContent);
        Assert.Same(legacyContent, expander.Content);
        Assert.Equal([NotifyCollectionChangedAction.Add, NotifyCollectionChangedAction.Add], itemEvents);

        Assert.True(expander.RemoveSetting(firstRow));
        Assert.Equal(1, expander.ItemCount);
        Assert.Contains(NotifyCollectionChangedAction.Remove, itemEvents);

        expander.AddSetting(firstRow);

        var rows = new ObservableCollection<object>
        {
            "Density",
            new FWSettingsCard { Header = "Accent color" }
        };

        expander.ItemsSource = rows;
        rows.Add("Notifications");

        Assert.Same(rows, expander.ItemsSource);
        Assert.Equal(3, rows.Count);
        Assert.Equal(3, expander.ItemCount);
        Assert.Contains(NotifyCollectionChangedAction.Reset, itemEvents);
        Assert.Equal(NotifyCollectionChangedAction.Add, itemEvents[^1]);

        expander.ItemsSource = null;
        expander.ClearSettings();

        Assert.Equal(0, expander.ItemCount);
        Assert.Contains(NotifyCollectionChangedAction.Reset, itemEvents);
    }

    [Fact]
    public void FWDisclosureControls_ShouldComposeInsideLiquidGlassPanel()
    {
        var headerBackground = new SolidColorBrush(Color.FromRgb(0x22, 0x22, 0x22));
        var expander = new FWExpander
        {
            Header = "Surface options",
            HeaderBackground = headerBackground,
            IsExpanded = true,
            ExpandDirection = ExpandDirection.Down,
            Content = new TextBlock { Text = "Expanded material content" }
        };
        var groupContent = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Children =
            {
                new FWCheckBox { Content = "Enable reveal hints", IsChecked = true },
                new FWTextBox { Text = "LiquidGlass" }
            }
        };
        var groupBox = new FWGroupBox
        {
            Header = "Material settings",
            HeaderBackground = headerBackground,
            Padding = new Thickness(12),
            Content = groupContent
        };
        var target = new FWButton { Content = "Tip" };
        var toolTip = new FWToolTip
        {
            Content = "LiquidGlass tooltip",
            PlacementTarget = target,
            Placement = PlacementMode.Top,
            InitialShowDelay = 200,
            ShowDuration = int.MaxValue
        };
        target.ToolTip = toolTip;
        var dialog = new FWContentDialog
        {
            Title = "Save gallery changes?",
            Content = "FWContentDialog uses Fluent dialog resources.",
            PrimaryButtonText = "Save",
            SecondaryButtonText = "Review",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            IsPrimaryButtonEnabled = true,
            IsSecondaryButtonEnabled = true,
            FullSizeDesired = false
        };
        var settingsExpander = new FWSettingsExpander
        {
            Header = "Advanced settings",
            Description = "Secondary configuration rows",
            HeaderIcon = new FWFontIcon(),
            IsExpanded = true,
            Content = new FWTextBlock { Text = "Nested setting" }
        };
        var taskDialog = new FWTaskDialog
        {
            Title = "Reset defaults?",
            Subtitle = "This applies to the current profile.",
            Content = "The action can be reviewed before saving.",
            PrimaryButtonText = "Reset",
            SecondaryButtonText = "Review",
            CloseButtonText = "Cancel",
            DefaultButton = FWTaskDialogButton.Secondary
        };
        var taskClosed = 0;
        taskDialog.CloseButtonClick += (_, _) => taskClosed++;
        taskDialog.Open();
        taskDialog.RequestCloseButtonClick();
        var teachingTip = new FWTeachingTip
        {
            Target = target,
            Title = "Preview control states",
            Subtitle = "TeachingTip keeps contextual guidance anchored to the sample.",
            IconSource = new FWFontIcon(),
            HeroContent = new FWBorder { Width = 120, Height = 42 },
            Content = new FWTextBlock { Text = "Open the action to inspect related metadata." },
            ActionButtonContent = "View metadata",
            CloseButtonContent = "Dismiss",
            PreferredPlacement = TeachingTipPlacementMode.Bottom,
            TailVisibility = TeachingTipTailVisibility.Auto,
            IsOpen = true
        };
        var panel = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Children =
            {
                expander,
                settingsExpander,
                groupBox,
                target,
                teachingTip
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

        Assert.True(expander.IsExpanded);
        Assert.Equal(ExpandDirection.Down, expander.ExpandDirection);
        Assert.Same(headerBackground, expander.HeaderBackground);
        Assert.Equal("Surface options", expander.Header);
        Assert.Equal("Material settings", groupBox.Header);
        Assert.Same(headerBackground, groupBox.HeaderBackground);
        Assert.Same(groupContent, groupBox.Content);
        Assert.Equal(12, groupBox.Padding.Left);
        Assert.Same(target, toolTip.PlacementTarget);
        Assert.Equal(PlacementMode.Top, toolTip.Placement);
        Assert.Equal(200, toolTip.InitialShowDelay);
        Assert.Equal(int.MaxValue, toolTip.ShowDuration);
        Assert.Same(toolTip, target.ToolTip);
        Assert.Equal("Save gallery changes?", dialog.Title);
        Assert.Equal("Save", dialog.PrimaryButtonText);
        Assert.Equal("Review", dialog.SecondaryButtonText);
        Assert.Equal("Cancel", dialog.CloseButtonText);
        Assert.Equal(ContentDialogButton.Primary, dialog.DefaultButton);
        Assert.True(dialog.IsPrimaryButtonEnabled);
        Assert.True(dialog.IsSecondaryButtonEnabled);
        Assert.False(dialog.FullSizeDesired);
        Assert.Equal("Advanced settings", settingsExpander.Header);
        Assert.Equal("Secondary configuration rows", settingsExpander.Description);
        Assert.True(settingsExpander.IsExpanded);
        Assert.Equal("Reset defaults?", taskDialog.Title);
        Assert.Equal(FWTaskDialogButton.Secondary, taskDialog.DefaultButton);
        Assert.Equal(FWTaskDialogResult.Close, taskDialog.Result);
        Assert.False(taskDialog.IsOpen);
        Assert.Equal(1, taskClosed);
        Assert.Same(target, teachingTip.Target);
        Assert.Equal("Preview control states", teachingTip.Title);
        Assert.Equal("Dismiss", teachingTip.CloseButtonContent);
        Assert.Equal(TeachingTipPlacementMode.Bottom, teachingTip.PreferredPlacement);
        Assert.True(teachingTip.IsOpen);
        Assert.Equal(FWFluentMaterialKind.LiquidGlass, surface.MaterialKind);
        Assert.True(surface.LiquidGlass);
        Assert.Equal(70, surface.RefractionAmount);
        Assert.Equal(0.42, surface.ChromaticAberration);
        Assert.Equal(24, surface.FusionRadius);
        Assert.Equal(BorderShape.SuperEllipse, surface.Shape);
        Assert.Equal(4, surface.SuperEllipseN);
        Assert.Same(panel, surface.Child);
    }

    private static ResourceDictionary LoadGenericThemeDictionary()
    {
        var loaded = ResourceDictionary.SourceLoader?.Invoke(
            new ResourceDictionary(),
            new Uri("/FluentJalium;component/Themes/Generic.jalxaml", UriKind.Relative),
            FluentThemeManager.ThemeAssembly);

        return Assert.IsType<ResourceDictionary>(loaded);
    }

    private static ResourceDictionary LoadDisclosureControlThemeDictionary()
    {
        var loaded = ResourceDictionary.SourceLoader?.Invoke(
            new ResourceDictionary(),
            new Uri("/FluentJalium;component/Themes/Controls/DisclosureControls.jalxaml", UriKind.Relative),
            FluentThemeManager.ThemeAssembly);

        return Assert.IsType<ResourceDictionary>(loaded);
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

    private static void AssertSetter(Style style, DependencyProperty property)
    {
        Assert.Contains(style.Setters, setter => setter.Property == property);
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
