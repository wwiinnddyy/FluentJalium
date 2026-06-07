using FluentJalium.Gallery.Controls;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Media;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWButton = FluentJalium.Controls.FWButton;
using FWCheckBox = FluentJalium.Controls.FWCheckBox;
using FWContentDialog = FluentJalium.Controls.FWContentDialog;
using FWExpander = FluentJalium.Controls.FWExpander;
using FWFluentMaterialKind = FluentJalium.Controls.FWFluentMaterialKind;
using FWFluentMaterialSurface = FluentJalium.Controls.FWFluentMaterialSurface;
using FWGroupBox = FluentJalium.Controls.FWGroupBox;
using FWSettingsCard = FluentJalium.Controls.FWSettingsCard;
using FWSettingsExpander = FluentJalium.Controls.FWSettingsExpander;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWTaskDialog = FluentJalium.Controls.FWTaskDialog;
using FWTaskDialogButton = FluentJalium.Controls.FWTaskDialogButton;
using FWTaskDialogButtonClickEventArgs = FluentJalium.Controls.FWTaskDialogButtonClickEventArgs;
using FWTeachingTip = FluentJalium.Controls.FWTeachingTip;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWTextBox = FluentJalium.Controls.FWTextBox;
using FWToolTip = FluentJalium.Controls.FWToolTip;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;
using TeachingTipPlacementMode = FluentJalium.Controls.TeachingTipPlacementMode;
using TeachingTipTailVisibility = FluentJalium.Controls.TeachingTipTailVisibility;
using ICommand = System.Windows.Input.ICommand;

namespace FluentJalium.Gallery.Pages;

internal sealed class GalleryDisclosurePage
{
    public UIElement CreateContent()
    {
        var panel = CreateSection("Disclosure and Dialogs");
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        examples.Children.Add(CreateDisclosureExampleCard(
            FluentIconRegular.PanelLeft24,
            "FWExpander",
            "Expanded, collapsed, disabled, and command-driven disclosure states.",
            CreateExpanderSample()));
        examples.Children.Add(CreateDisclosureExampleCard(
            FluentIconRegular.TextBoxSettings24,
            "FWGroupBox",
            "Grouped inputs, header background, nested options, and command output.",
            CreateGroupBoxSample()));
        examples.Children.Add(CreateDisclosureExampleCard(
            FluentIconRegular.Info24,
            "FWToolTip",
            "Tooltip placement, timing, open/close state, and icon button target.",
            CreateToolTipSample()));
        examples.Children.Add(CreateDisclosureExampleCard(
            FluentIconRegular.TargetSparkle24,
            "FWTeachingTip",
            "Targeted guidance with title, subtitle, hero content, action, close, placement, and tail states.",
            CreateTeachingTipSample()));
        examples.Children.Add(CreateDisclosureExampleCard(
            FluentIconRegular.WindowNew24,
            "FWContentDialog",
            "Primary, secondary, close, default button, full-size, and disabled button configuration.",
            CreateContentDialogSample()));
        examples.Children.Add(CreateDisclosureExampleCard(
            FluentIconRegular.WindowWrench24,
            "FWTaskDialog",
            "Lightweight task prompts expose awaitable result, default button, cancel, and requestable command events.",
            CreateTaskDialogSample()));
        examples.Children.Add(CreateDisclosureExampleCard(
            FluentIconRegular.Settings24,
            "FWSettingsExpander",
            "Settings-style disclosure combines icon, description, grouped rows, and expand/collapse state.",
            CreateSettingsExpanderSample()));
        examples.Children.Add(CreateDisclosureExampleCard(
            FluentIconRegular.LayerDiagonalSparkle24,
            "Material disclosure panel",
            "Expander, tooltip, group box, and dialog commands remain readable on LiquidGlass.",
            CreateMaterialDisclosurePanelSample()));

        panel.Children.Add(examples);
        return panel;
    }

    private static UIElement CreateExpanderSample()
    {
        var output = CreateDisclosureOutput("Expander: advanced options open.");
        var primary = new FWExpander
        {
            Header = "Advanced options",
            IsExpanded = true,
            ExpandDirection = ExpandDirection.Down,
            HeaderBackground = ThemeBrush("LayerFillColorDefaultBrush"),
            Content = new FWTextBlock
            {
                Text = "Expanded content keeps a subtle Fluent surface and accent chevron state.",
                TextWrapping = TextWrapping.Wrap,
                Foreground = ThemeBrush("TextPrimary")
            }
        };
        var disabled = new FWExpander
        {
            Header = "Disabled",
            IsExpanded = false,
            IsEnabled = false,
            Content = new FWTextBlock
            {
                Text = "Disabled expander",
                Foreground = ThemeBrush("TextSecondary")
            }
        };
        primary.Expanded += (_, _) => output.Text = "Expander: advanced options open.";
        primary.Collapsed += (_, _) => output.Text = "Expander: advanced options collapsed.";

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                primary,
                disabled,
                CreateDisclosureButtonRow(
                    CreateDisclosureActionButton(FluentIconRegular.PanelLeftExpand24, "Expand", () => primary.IsExpanded = true),
                    CreateDisclosureActionButton(FluentIconRegular.PanelLeftContract24, "Collapse", () => primary.IsExpanded = false),
                    CreateDisclosureActionButton(FluentIconRegular.ArrowSortDown24, "Direction", () =>
                    {
                        primary.ExpandDirection = primary.ExpandDirection == ExpandDirection.Down
                            ? ExpandDirection.Up
                            : ExpandDirection.Down;
                        output.Text = $"Expander direction: {primary.ExpandDirection}";
                    })),
                CreateDisclosureStatus(output)
            }
        };
    }

    private static UIElement CreateGroupBoxSample()
    {
        var output = CreateDisclosureOutput("GroupBox: sync enabled, mode grouped.");
        var textBox = new FWTextBox
        {
            Text = "Grouped text",
            Width = 240
        };
        var sync = new FWCheckBox
        {
            Content = "Sync group option",
            IsChecked = true
        };
        var groupBox = new FWGroupBox
        {
            Header = "FWGroupBox",
            Width = 320,
            HeaderBackground = ThemeBrush("LayerFillColorDefaultBrush"),
            Padding = new Thickness(12),
            Content = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 8,
                Children =
                {
                    sync,
                    textBox
                }
            }
        };
        sync.Checked += (_, _) => output.Text = "GroupBox: sync enabled.";
        sync.Unchecked += (_, _) => output.Text = "GroupBox: sync disabled.";
        textBox.TextChanged += (_, _) => output.Text = $"GroupBox text: {textBox.Text}";

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                groupBox,
                CreateDisclosureButtonRow(
                    CreateDisclosureActionButton(FluentIconRegular.CheckboxChecked24, "Toggle sync", () =>
                    {
                        sync.IsChecked = sync.IsChecked != true;
                        output.Text = $"GroupBox sync: {FormatOnOff(sync.IsChecked == true)}";
                    }),
                    CreateDisclosureActionButton(FluentIconRegular.TextEditStyle24, "Edit", () => textBox.Text = "Updated grouped text"),
                    CreateDisclosureActionButton(FluentIconRegular.Color24, "Header", () =>
                    {
                        groupBox.HeaderBackground = ThemeBrush("AccentFillColorDefaultBrush");
                        output.Text = "GroupBox: accent header background.";
                    })),
                CreateDisclosureStatus(output)
            }
        };
    }

    private static UIElement CreateToolTipSample()
    {
        var output = CreateDisclosureOutput("ToolTip: closed, placement top.");
        var toolTip = new FWToolTip
        {
            Content = new FWTextBlock
            {
                Text = "FWToolTip follows Fluent popup resources.",
                Foreground = ThemeBrush("ToolTipForeground")
            },
            Placement = PlacementMode.Top,
            InitialShowDelay = 200,
            ShowDuration = int.MaxValue
        };
        var target = new FWButton
        {
            Content = CreateDisclosureButtonContent(FluentIconRegular.Info24, "Hover for FWToolTip"),
            MinWidth = 190,
            ToolTip = toolTip
        };
        toolTip.PlacementTarget = target;
        toolTip.Opened += (_, _) => output.Text = $"ToolTip: open at {toolTip.Placement}.";
        toolTip.Closed += (_, _) => output.Text = "ToolTip: closed.";

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                target,
                CreateDisclosureButtonRow(
                    CreateDisclosureActionButton(FluentIconRegular.Open24, "Open", () =>
                    {
                        toolTip.IsOpen = true;
                        output.Text = $"ToolTip: open at {toolTip.Placement}.";
                    }),
                    CreateDisclosureActionButton(FluentIconRegular.DismissCircle24, "Close", () =>
                    {
                        toolTip.IsOpen = false;
                        output.Text = "ToolTip: closed.";
                    }),
                    CreateDisclosureActionButton(FluentIconRegular.ArrowDownload24, "Bottom", () =>
                    {
                        toolTip.Placement = PlacementMode.Bottom;
                        output.Text = $"ToolTip placement: {toolTip.Placement}";
                    }),
                    CreateDisclosureActionButton(FluentIconRegular.ArrowUpload24, "Top", () =>
                    {
                        toolTip.Placement = PlacementMode.Top;
                        output.Text = $"ToolTip placement: {toolTip.Placement}";
                    })),
                CreateDisclosureStatus(output)
            }
        };
    }

    private static UIElement CreateTeachingTipSample()
    {
        var output = CreateDisclosureOutput("TeachingTip: open below target, tail visible.");
        var placementIndex = 0;
        var tailIndex = 0;
        var heroUsesAccent = false;
        var placements = new[]
        {
            TeachingTipPlacementMode.Bottom,
            TeachingTipPlacementMode.Top,
            TeachingTipPlacementMode.Right,
            TeachingTipPlacementMode.Left
        };
        var tailStates = new[]
        {
            TeachingTipTailVisibility.Visible,
            TeachingTipTailVisibility.Auto,
            TeachingTipTailVisibility.Collapsed
        };
        var target = new FWButton
        {
            Content = CreateDisclosureButtonContent(FluentIconRegular.Target24, "Open targeted tip"),
            MinWidth = 210
        };
        var teachingTip = new FWTeachingTip
        {
            Target = target,
            IsOpen = true,
            Title = "Review new density options",
            Subtitle = "FWTeachingTip anchors guidance to a live target.",
            IconSource = CreateIcon(FluentIconRegular.InfoSparkle24, 20, ThemeBrush("TextPrimary")),
            ActionButtonContent = "Apply",
            CloseButtonContent = "Later",
            PreferredPlacement = placements[placementIndex],
            TailVisibility = tailStates[tailIndex],
            IsLightDismissEnabled = true,
            Width = 340,
            HeroContent = CreateTeachingTipHero(heroUsesAccent),
            Content = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 6,
                Children =
                {
                    new FWTextBlock
                    {
                        Text = "Use the action button to accept the suggestion, or close it when the target no longer needs guidance.",
                        Foreground = ThemeBrush("TextPrimary"),
                        TextWrapping = TextWrapping.Wrap
                    },
                    new FWTextBlock
                    {
                        Text = "Content area: contextual onboarding copy and lightweight details.",
                        FontSize = 12,
                        Foreground = ThemeBrush("TextSecondary"),
                        TextWrapping = TextWrapping.Wrap
                    }
                }
            }
        };

        void UpdateTeachingTipStatus(string action)
        {
            output.Text = $"{action}. Open: {FormatOnOff(teachingTip.IsOpen)}, placement: {teachingTip.PreferredPlacement}, tail: {teachingTip.TailVisibility}, action: {teachingTip.ActionButtonContent}, close: {teachingTip.CloseButtonContent}.";
        }

        target.Click += (_, _) =>
        {
            teachingTip.IsOpen = true;
            UpdateTeachingTipStatus("TeachingTip opened from target");
        };
        teachingTip.ActionButtonClick += (_, _) =>
        {
            teachingTip.IsOpen = false;
            UpdateTeachingTipStatus("TeachingTip action clicked");
        };
        teachingTip.Closing += (_, args) =>
        {
            output.Text = $"TeachingTip closing: {args.Reason}.";
        };
        teachingTip.Closed += (_, args) =>
        {
            output.Text = $"TeachingTip closed: {args.Reason}.";
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                target,
                teachingTip,
                CreateDisclosureButtonRow(
                    CreateDisclosureActionButton(FluentIconRegular.Open24, "Open", () =>
                    {
                        teachingTip.IsOpen = true;
                        UpdateTeachingTipStatus("TeachingTip opened");
                    }),
                    CreateDisclosureActionButton(FluentIconRegular.DismissCircle24, "Close", () =>
                    {
                        teachingTip.IsOpen = false;
                        UpdateTeachingTipStatus("TeachingTip closed programmatically");
                    }),
                    CreateDisclosureActionButton(FluentIconRegular.ArrowSortDown24, "Placement", () =>
                    {
                        placementIndex = (placementIndex + 1) % placements.Length;
                        teachingTip.PreferredPlacement = placements[placementIndex];
                        UpdateTeachingTipStatus("TeachingTip placement changed");
                    }),
                    CreateDisclosureActionButton(FluentIconRegular.Cursor24, "Tail", () =>
                    {
                        tailIndex = (tailIndex + 1) % tailStates.Length;
                        teachingTip.TailVisibility = tailStates[tailIndex];
                        UpdateTeachingTipStatus("TeachingTip tail visibility changed");
                    }),
                    CreateDisclosureActionButton(FluentIconRegular.ImageSparkle24, "Hero", () =>
                    {
                        heroUsesAccent = !heroUsesAccent;
                        teachingTip.HeroContent = CreateTeachingTipHero(heroUsesAccent);
                        UpdateTeachingTipStatus("TeachingTip hero content refreshed");
                    }),
                    CreateDisclosureActionButton(FluentIconRegular.ControlButton24, "Buttons", () =>
                    {
                        if (Equals(teachingTip.ActionButtonContent, "Apply"))
                        {
                            teachingTip.ActionButtonContent = "Learn more";
                            teachingTip.CloseButtonContent = "Done";
                        }
                        else
                        {
                            teachingTip.ActionButtonContent = "Apply";
                            teachingTip.CloseButtonContent = "Later";
                        }

                        UpdateTeachingTipStatus("TeachingTip button content changed");
                    })),
                CreateDisclosureStatus(output)
            }
        };
    }

    private static UIElement CreateContentDialogSample()
    {
        var output = CreateDisclosureOutput("Dialog: configured, not shown.");
        var dialog = CreateSampleContentDialog();
        dialog.PrimaryButtonClick += (_, _) => output.Text = "Dialog primary clicked.";
        dialog.SecondaryButtonClick += (_, _) => output.Text = "Dialog secondary clicked.";
        dialog.CloseButtonClick += (_, _) => output.Text = "Dialog close clicked.";
        var dialogButton = new FWButton
        {
            Content = CreateDisclosureButtonContent(FluentIconRegular.WindowNew24, "Show FWContentDialog"),
            MinWidth = 210
        };
        dialogButton.Click += async (_, _) =>
        {
            var result = await dialog.ShowAsync();
            output.Text = $"Dialog result: {result}";
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                CreateDialogSummary(dialog),
                dialogButton,
                CreateDisclosureButtonRow(
                    CreateDisclosureActionButton(FluentIconRegular.CheckmarkCircle24, "Primary", () =>
                    {
                        dialog.DefaultButton = ContentDialogButton.Primary;
                        dialog.IsPrimaryButtonEnabled = true;
                        output.Text = "Dialog default button: primary.";
                    }),
                    CreateDisclosureActionButton(FluentIconRegular.Prohibited24, "Disable primary", () =>
                    {
                        dialog.IsPrimaryButtonEnabled = !dialog.IsPrimaryButtonEnabled;
                        output.Text = $"Dialog primary enabled: {dialog.IsPrimaryButtonEnabled}";
                    }),
                    CreateDisclosureActionButton(FluentIconRegular.FullScreenMaximize24, "Full size", () =>
                    {
                        dialog.FullSizeDesired = !dialog.FullSizeDesired;
                        output.Text = $"Dialog full size: {FormatOnOff(dialog.FullSizeDesired)}";
                    })),
                CreateDisclosureStatus(output)
            }
        };
    }

    private static UIElement CreateTaskDialogSample()
    {
        var output = CreateDisclosureOutput("TaskDialog: ready for ShowAsync flow. Default: Primary, primary command: on, cancel guard: off.");
        var cancelCloseRequests = false;
        var commandExecutions = 0;
        var lastRequest = "No button requests yet.";
        var lastCommand = "No button commands executed yet.";
        var deleteCommand = new GalleryTaskDialogCommand(parameter =>
        {
            commandExecutions++;
            lastCommand = $"Command {commandExecutions}: primary executed with parameter {parameter}.";
        });
        var archiveCommand = new GalleryTaskDialogCommand(parameter =>
        {
            commandExecutions++;
            lastCommand = $"Command {commandExecutions}: secondary/cancel executed with parameter {parameter}.";
        });
        var closeCommand = new GalleryTaskDialogCommand(parameter =>
        {
            commandExecutions++;
            lastCommand = $"Command {commandExecutions}: close executed with parameter {parameter}.";
        });
        var taskDialog = new FWTaskDialog
        {
            Title = "Delete temporary layout cache?",
            Subtitle = "FWTaskDialog keeps awaitable command semantics without requiring a modal host.",
            Icon = CreateIcon(FluentIconRegular.Warning24, 22, ThemeBrush("TextPrimary")),
            PrimaryButtonText = "Delete",
            SecondaryButtonText = "Archive",
            CloseButtonText = "Cancel",
            DefaultButton = FWTaskDialogButton.Primary,
            CancelButton = FWTaskDialogButton.Close,
            PrimaryButtonCommand = deleteCommand,
            PrimaryButtonCommandParameter = "delete-cache",
            SecondaryButtonCommand = archiveCommand,
            SecondaryButtonCommandParameter = "archive-cache",
            CloseButtonCommand = closeCommand,
            CloseButtonCommandParameter = "cancel-dialog",
            IsOpen = true,
            Content = new FWTextBlock
            {
                Text = "Start the async flow, then request the default, primary, secondary, or cancel command. Escape routes through the configured CancelButton.",
                Foreground = ThemeBrush("TextPrimary"),
                TextWrapping = TextWrapping.Wrap
            }
        };
        taskDialog.PrimaryButtonClick += (_, args) => UpdateRequestEvent("Primary", args);
        taskDialog.SecondaryButtonClick += (_, args) => UpdateRequestEvent("Secondary", args);
        taskDialog.CloseButtonClick += (_, args) =>
        {
            args.Cancel = cancelCloseRequests;
            UpdateRequestEvent("Cancel", args);
        };

        void UpdateRequestEvent(string command, FWTaskDialogButtonClickEventArgs args)
        {
            lastRequest = $"{command} requested result {args.Result}; command executed: {FormatOnOff(args.CommandExecuted)}; cancel: {FormatOnOff(args.Cancel)}.";
        }

        void UpdateAfterRequest(string action, bool? requestCompleted = null)
        {
            var requestText = requestCompleted.HasValue
                ? $", request: {(requestCompleted.Value ? "completed" : "canceled")}"
                : string.Empty;
            output.Text = $"{action}. Open: {FormatOnOff(taskDialog.IsOpen)}, result: {taskDialog.Result}, default: {taskDialog.DefaultButton}, primary command: {FormatOnOff(deleteCommand.CanExecuteResult)}, cancel guard: {FormatOnOff(cancelCloseRequests)}{requestText}. {lastRequest} {lastCommand}";
        }

        bool RequestDefaultButton()
        {
            return InvokeTaskDialogBooleanRequest(taskDialog, "RequestDefaultButtonClick", () =>
            {
                return taskDialog.DefaultButton switch
                {
                    FWTaskDialogButton.Primary => taskDialog.RequestPrimaryButtonClick(),
                    FWTaskDialogButton.Secondary => taskDialog.RequestSecondaryButtonClick(),
                    FWTaskDialogButton.Close => RequestCancelButton(),
                    _ => false
                };
            });
        }

        bool RequestCancelButton()
        {
            return InvokeTaskDialogBooleanRequest(taskDialog, "RequestCancelButtonClick",
                () => taskDialog.RequestCloseButtonClick());
        }

        async Task RunShowAsyncFlowAsync()
        {
            output.Text = $"TaskDialog ShowAsync flow started. Default: {taskDialog.DefaultButton}, cancel guard: {FormatOnOff(cancelCloseRequests)}.";

            try
            {
                var result = await TryShowTaskDialogAsync(taskDialog);
                if (result is null)
                {
                    UpdateAfterRequest("ShowAsync API not available yet; opened request flow");
                    return;
                }

                output.Text = $"TaskDialog ShowAsync completed. Final result: {result}. Open: {FormatOnOff(taskDialog.IsOpen)}, default: {taskDialog.DefaultButton}, cancel guard: {FormatOnOff(cancelCloseRequests)}.";
            }
            catch (Exception ex)
            {
                output.Text = $"TaskDialog ShowAsync failed: {GetTaskDialogExceptionMessage(ex)}";
            }
        }

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                taskDialog,
                CreateDisclosureButtonRow(
                    CreateDisclosureActionButton(FluentIconRegular.Play24, "ShowAsync", () => _ = RunShowAsyncFlowAsync()),
                    CreateDisclosureActionButton(FluentIconRegular.Open24, "Open", () =>
                    {
                        taskDialog.Open();
                        UpdateAfterRequest("TaskDialog opened");
                    }),
                    CreateDisclosureActionButton(FluentIconRegular.Send24, "Default request", () =>
                    {
                        var completed = RequestDefaultButton();
                        UpdateAfterRequest("Default command requested", completed);
                    }),
                    CreateDisclosureActionButton(FluentIconRegular.Delete24, "Primary", () =>
                    {
                        var completed = taskDialog.RequestPrimaryButtonClick();
                        UpdateAfterRequest("Primary command requested", completed);
                    }),
                    CreateDisclosureActionButton(FluentIconRegular.Archive24, "Secondary", () =>
                    {
                        var completed = taskDialog.RequestSecondaryButtonClick();
                        UpdateAfterRequest("Secondary command requested", completed);
                    }),
                    CreateDisclosureActionButton(FluentIconRegular.Dismiss24, "Cancel", () =>
                    {
                        var completed = RequestCancelButton();
                        UpdateAfterRequest("Cancel command requested", completed);
                    }),
                    CreateDisclosureActionButton(FluentIconRegular.CheckmarkCircle24, "Default", () =>
                    {
                        taskDialog.DefaultButton = taskDialog.DefaultButton switch
                        {
                            FWTaskDialogButton.Primary => FWTaskDialogButton.Secondary,
                            FWTaskDialogButton.Secondary => FWTaskDialogButton.Close,
                            FWTaskDialogButton.Close => FWTaskDialogButton.None,
                            _ => FWTaskDialogButton.Primary
                        };
                        UpdateAfterRequest("Default button changed");
                    }),
                    CreateDisclosureActionButton(FluentIconRegular.Prohibited24, "Primary command", () =>
                    {
                        deleteCommand.CanExecuteResult = !deleteCommand.CanExecuteResult;
                        deleteCommand.RaiseCanExecuteChanged();
                        UpdateAfterRequest("Primary command CanExecute changed");
                    }),
                    CreateDisclosureActionButton(FluentIconRegular.ShieldDismiss24, "Guard close", () =>
                    {
                        cancelCloseRequests = !cancelCloseRequests;
                        UpdateAfterRequest("Cancel close guard changed");
                    })),
                CreateDisclosureStatus(output)
            }
        };
    }

    private static bool InvokeTaskDialogBooleanRequest(FWTaskDialog taskDialog, string methodName, Func<bool> fallback)
    {
        var method = taskDialog.GetType().GetMethod(methodName, Type.EmptyTypes);
        if (method is null)
        {
            return fallback();
        }

        var result = method.Invoke(taskDialog, null);
        return result is not bool completed || completed;
    }

    private static async Task<object?> TryShowTaskDialogAsync(FWTaskDialog taskDialog)
    {
        var method = taskDialog.GetType().GetMethod("ShowAsync", Type.EmptyTypes);
        if (method is null)
        {
            taskDialog.Open();
            return null;
        }

        var result = method.Invoke(taskDialog, null);
        if (result is Task task)
        {
            await task;
            return ReadTaskDialogTaskResult(task) ?? taskDialog.Result;
        }

        if (result is not null)
        {
            var asTask = result.GetType().GetMethod("AsTask", Type.EmptyTypes);
            if (asTask?.Invoke(result, null) is Task valueTask)
            {
                await valueTask;
                return ReadTaskDialogTaskResult(valueTask) ?? taskDialog.Result;
            }
        }

        return result ?? taskDialog.Result;
    }

    private static object? ReadTaskDialogTaskResult(Task task)
    {
        return task.GetType().GetProperty("Result")?.GetValue(task);
    }

    private static string GetTaskDialogExceptionMessage(Exception ex)
    {
        return ex is System.Reflection.TargetInvocationException { InnerException: not null }
            ? ex.InnerException.Message
            : ex.Message;
    }

    private sealed class GalleryTaskDialogCommand : ICommand
    {
        private readonly Action<object?> _execute;

        public GalleryTaskDialogCommand(Action<object?> execute)
        {
            _execute = execute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecuteResult { get; set; } = true;

        public bool CanExecute(object? parameter) => CanExecuteResult;

        public void Execute(object? parameter)
        {
            _execute(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private static UIElement CreateSettingsExpanderSample()
    {
        var output = CreateDisclosureOutput("SettingsExpander: appearance settings open. Items: 3.");
        var expander = new FWSettingsExpander
        {
            Header = "Appearance",
            Description = "Theme, material, and density options grouped in settings rows.",
            HeaderIcon = CreateIcon(FluentIconRegular.Settings24, 20, ThemeBrush("TextSecondary")),
            IsExpanded = true
        };
        expander.AddSetting(CreateSettingsRow("App theme", "Use system setting", FluentIconRegular.DarkTheme24, "System"));
        expander.AddSetting(CreateCommandSettingsRow("Window material", "Preview the shell backdrop choice", FluentIconRegular.LayerDiagonalSparkle24, "Preview", () =>
        {
            output.Text = "SettingsExpander command: window material preview requested.";
        }));
        expander.AddSetting(CreateSettingsRow("Control density", "Comfortable touch targets", FluentIconRegular.TextDensity24, "Comfort"));
        expander.ItemsChanged += (_, e) => output.Text = $"SettingsExpander items: {expander.ItemCount}. Change: {e.Action}.";
        expander.Expanded += (_, _) => output.Text = $"SettingsExpander: appearance settings open. Items: {expander.ItemCount}.";
        expander.Collapsed += (_, _) => output.Text = $"SettingsExpander: appearance settings collapsed. Items: {expander.ItemCount}.";

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                expander,
                CreateDisclosureButtonRow(
                    CreateDisclosureActionButton(FluentIconRegular.PanelLeftExpand24, "Expand", () => expander.IsExpanded = true),
                    CreateDisclosureActionButton(FluentIconRegular.PanelLeftContract24, "Collapse", () => expander.IsExpanded = false),
                    CreateDisclosureActionButton(FluentIconRegular.TextDescription24, "Describe", () =>
                    {
                        expander.Description = expander.Description?.ToString()?.Contains("updated", StringComparison.OrdinalIgnoreCase) == true
                            ? "Theme, material, and density options grouped in settings rows."
                            : "Updated description shows SettingsExpander metadata can change live.";
                        output.Text = "SettingsExpander description updated.";
                    }),
                    CreateDisclosureActionButton(FluentIconRegular.Add24, "Add row", () =>
                    {
                        expander.AddSetting(CreateSettingsRow("Generated setting", "Added through the item host API", FluentIconRegular.Sparkle24, $"Row {expander.ItemCount + 1}"));
                    }),
                    CreateDisclosureActionButton(FluentIconRegular.Delete24, "Clear rows", () =>
                    {
                        expander.ClearSettings();
                    })),
                CreateDisclosureStatus(output)
            }
        };
    }

    private static UIElement CreateMaterialDisclosurePanelSample()
    {
        var output = CreateDisclosureOutput("Surface: LiquidGlass. Expander open, tooltip top, dialog primary.");
        var expander = new FWExpander
        {
            Header = "Surface options",
            IsExpanded = true,
            HeaderBackground = ThemeBrush("LayerFillColorDefaultBrush"),
            Content = new FWTextBlock
            {
                Text = "Layered disclosure content keeps readable text over the material.",
                TextWrapping = TextWrapping.Wrap,
                Foreground = ThemeBrush("TextPrimary")
            }
        };
        var groupBox = new FWGroupBox
        {
            Header = "Material settings",
            HeaderBackground = ThemeBrush("LayerFillColorDefaultBrush"),
            Padding = new Thickness(12),
            Content = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 8,
                Children =
                {
                    new FWCheckBox { Content = "Enable reveal hints", IsChecked = true },
                    new FWTextBox { Text = "LiquidGlass", Width = 220 }
                }
            }
        };
        var toolTip = new FWToolTip
        {
            Content = "LiquidGlass tooltip",
            Placement = PlacementMode.Top,
            InitialShowDelay = 200
        };
        var tipButton = new FWButton
        {
            Content = CreateDisclosureButtonContent(FluentIconRegular.Info24, "Material tip"),
            ToolTip = toolTip,
            MinWidth = 150
        };
        toolTip.PlacementTarget = tipButton;
        var dialog = CreateSampleContentDialog();

        return new FWFluentMaterialSurface
        {
            Width = 540,
            MaterialKind = FWFluentMaterialKind.LiquidGlass,
            TintColor = Color.FromArgb(180, 20, 84, 145),
            TintOpacity = 0.2,
            BlurRadius = 14,
            RefractionAmount = 70,
            ChromaticAberration = 0.42,
            FusionRadius = 24,
            Background = new SolidColorBrush(Color.FromArgb(66, 255, 255, 255)),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Shape = BorderShape.SuperEllipse,
            SuperEllipseN = 4,
            Padding = new Thickness(16),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 12,
                Children =
                {
                    CreateMaterialHeader(),
                    expander,
                    groupBox,
                    new FWWrapPanel
                    {
                        HorizontalSpacing = 10,
                        VerticalSpacing = 10,
                        Children =
                        {
                            tipButton,
                            CreateDisclosureActionButton(FluentIconRegular.PanelLeftExpand24, "Expand", () =>
                            {
                                expander.IsExpanded = true;
                                output.Text = "Surface: expander open.";
                            }),
                            CreateDisclosureActionButton(FluentIconRegular.Info24, "Tooltip", () =>
                            {
                                toolTip.IsOpen = !toolTip.IsOpen;
                                output.Text = $"Surface: tooltip open {FormatOnOff(toolTip.IsOpen)}.";
                            }),
                            CreateDisclosureActionButton(FluentIconRegular.WindowNew24, "Dialog", () =>
                            {
                                dialog.FullSizeDesired = !dialog.FullSizeDesired;
                                output.Text = $"Surface: dialog full size {FormatOnOff(dialog.FullSizeDesired)}.";
                            })
                        }
                    },
                    CreateDisclosureStatus(output)
                }
            }
        };
    }

    private static FWBorder CreateTeachingTipHero(bool accent)
    {
        var foreground = ThemeBrush(accent ? "TextOnAccent" : "TextPrimary");

        return new FWBorder
        {
            Background = ThemeBrush(accent ? "AccentFillColorDefaultBrush" : "LayerFillColorDefaultBrush"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children =
                {
                    CreateIcon(accent ? FluentIconRegular.Sparkle24 : FluentIconRegular.TargetSparkle24, 18, foreground),
                    new FWTextBlock
                    {
                        Text = accent ? "Accent hero content" : "Targeted hero content",
                        FontSize = 13,
                        Foreground = foreground,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                }
            }
        };
    }

    private static FWSettingsCard CreateSettingsRow(string header, string description, FluentIconRegular icon, string value)
    {
        return new FWSettingsCard
        {
            Header = header,
            Description = description,
            HeaderIcon = CreateIcon(icon, 18, ThemeBrush("TextSecondary")),
            ActionIcon = CreateIcon(FluentIconRegular.ChevronRight24, 16, ThemeBrush("TextSecondary")),
            Content = new FWTextBlock
            {
                Text = value,
                Foreground = ThemeBrush("TextSecondary"),
                VerticalAlignment = VerticalAlignment.Center
            },
            IsClickEnabled = true
        };
    }

    private static FWSettingsCard CreateCommandSettingsRow(string header, string description, FluentIconRegular icon, string commandText, Action command)
    {
        var card = new FWSettingsCard
        {
            Header = header,
            Description = description,
            HeaderIcon = CreateIcon(icon, 18, ThemeBrush("TextSecondary")),
            ActionIcon = CreateIcon(FluentIconRegular.CursorClick24, 16, ThemeBrush("TextSecondary")),
            Content = new FWTextBlock
            {
                Text = commandText,
                Foreground = ThemeBrush("TextSecondary"),
                VerticalAlignment = VerticalAlignment.Center
            },
            IsClickEnabled = true
        };
        card.Click += (_, _) => command();
        return card;
    }

    private static FWContentDialog CreateSampleContentDialog()
    {
        return new FWContentDialog
        {
            Title = "Save gallery changes?",
            PrimaryButtonText = "Save",
            SecondaryButtonText = "Review",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            IsPrimaryButtonEnabled = true,
            IsSecondaryButtonEnabled = true,
            FullSizeDesired = false,
            Content = new FWTextBlock
            {
                Text = "FWContentDialog uses the Fluent dialog card, overlay, title, and command button resources.",
                Foreground = ThemeBrush("TextPrimary"),
                TextWrapping = TextWrapping.Wrap,
                Width = 340
            }
        };
    }

    private static FWBorder CreateDialogSummary(FWContentDialog dialog)
    {
        return new FWBorder
        {
            Background = ThemeBrush("LayerFillColorDefaultBrush"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 4,
                Children =
                {
                    new FWTextBlock
                    {
                        Text = dialog.Title?.ToString() ?? "FWContentDialog",
                        FontSize = 14,
                        Foreground = ThemeBrush("TextPrimary")
                    },
                    new FWTextBlock
                    {
                        Text = $"{dialog.PrimaryButtonText} / {dialog.SecondaryButtonText} / {dialog.CloseButtonText}",
                        FontSize = 12,
                        Foreground = ThemeBrush("TextSecondary")
                    }
                }
            }
        };
    }

    private static FWStackPanel CreateMaterialHeader()
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                CreateIcon(FluentIconRegular.LayerDiagonalSparkle24, 18, ThemeBrush("TextPrimary")),
                new FWTextBlock
                {
                    Text = "Layered disclosure surface",
                    FontSize = 15,
                    Foreground = ThemeBrush("TextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
    }

    private static FWBorder CreateDisclosureExampleCard(FluentIconRegular icon, string title, string description, UIElement content)
    {
        return GallerySampleCard.Create(icon, title, description, content, code: CreateSampleCode(title));
    }

    private static string CreateSampleCode(string title)
    {
        return title switch
        {
            "FWExpander" => "<FWExpander Header=\"Advanced options\" IsExpanded=\"True\">\n    <FWTextBlock Text=\"Expanded content\" />\n</FWExpander>",
            "FWGroupBox" => "<FWGroupBox Header=\"Settings\">\n    <FWCheckBox Content=\"Sync group option\" />\n</FWGroupBox>",
            "FWToolTip" => "<FWButton Content=\"Hover for FWToolTip\">\n    <FWButton.ToolTip>\n        <FWToolTip Placement=\"Top\" />\n    </FWButton.ToolTip>\n</FWButton>",
            "FWTeachingTip" => "<FWButton x:Name=\"DensityButton\" Content=\"Open targeted tip\" />\n<FWTeachingTip Title=\"Review new density options\"\n               Subtitle=\"FWTeachingTip anchors guidance to a live target.\"\n               Target=\"{Binding ElementName=DensityButton}\"\n               ActionButtonContent=\"Apply\"\n               CloseButtonContent=\"Later\"\n               PreferredPlacement=\"Bottom\"\n               TailVisibility=\"Visible\">\n    <FWTeachingTip.HeroContent>\n        <FWBorder Background=\"{ThemeResource LayerFillColorDefaultBrush}\" />\n    </FWTeachingTip.HeroContent>\n    <FWTextBlock Text=\"Use targeted guidance for contextual onboarding.\" />\n</FWTeachingTip>",
            "FWContentDialog" => "<FWContentDialog Title=\"Save gallery changes?\" PrimaryButtonText=\"Save\" SecondaryButtonText=\"Review\" CloseButtonText=\"Cancel\" />",
            "FWTaskDialog" => "<FWTaskDialog Title=\"Delete temporary layout cache?\" PrimaryButtonText=\"Delete\" SecondaryButtonText=\"Archive\" CloseButtonText=\"Cancel\" IsOpen=\"True\" />",
            "FWSettingsExpander" => "<FWSettingsExpander Header=\"Appearance\" Description=\"Theme and material options\" IsExpanded=\"True\">\n    <FWSettingsCard Header=\"App theme\" />\n    <FWSettingsCard Header=\"Window material\" IsClickEnabled=\"True\" />\n    <FWSettingsCard Header=\"Control density\" />\n</FWSettingsExpander>",
            "Material disclosure panel" => "<FWFluentMaterialSurface MaterialKind=\"LiquidGlass\">\n    <FWExpander Header=\"Surface options\" />\n    <FWGroupBox Header=\"Material settings\" />\n</FWFluentMaterialSurface>",
            _ => "<FWExpander />"
        };
    }

    private static FWWrapPanel CreateDisclosureButtonRow(params FWButton[] buttons)
    {
        var row = new FWWrapPanel
        {
            HorizontalSpacing = 8,
            VerticalSpacing = 8
        };

        foreach (var button in buttons)
        {
            row.Children.Add(button);
        }

        return row;
    }

    private static FWButton CreateDisclosureActionButton(FluentIconRegular icon, string text, Action action)
    {
        var button = new FWButton
        {
            Content = CreateDisclosureButtonContent(icon, text)
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static FWStackPanel CreateDisclosureButtonContent(FluentIconRegular icon, string text)
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Children =
            {
                CreateIcon(icon, 16, ThemeBrush("TextPrimary")),
                new FWTextBlock
                {
                    Text = text,
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
    }

    private static TextBlock CreateDisclosureOutput(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        };
    }

    private static FWBorder CreateDisclosureStatus(TextBlock status)
    {
        return new FWBorder
        {
            Background = ThemeBrush("LayerFillColorDefaultBrush"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children =
                {
                    CreateIcon(FluentIconRegular.InfoSparkle24, 18, ThemeBrush("TextSecondary")),
                    status
                }
            }
        };
    }

    private static string FormatOnOff(bool value)
    {
        return value ? "on" : "off";
    }

    private static FWStackPanel CreateSection(string title)
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 14,
            Children =
            {
                new FWStackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 10,
                    Children =
                    {
                        CreateIcon(FluentIconRegular.PanelLeft24, 24, ThemeBrush("TextPrimary")),
                        new FWTextBlock
                        {
                            Text = title,
                            FontSize = 22,
                            Foreground = ThemeBrush("TextPrimary"),
                            VerticalAlignment = VerticalAlignment.Center
                        }
                    }
                }
            }
        };
    }

    private static FluentIcon CreateIcon(FluentIconRegular icon, double size = FluentIcon.DefaultSize, Brush? foreground = null)
    {
        return FluentIconFactory.Regular(icon, size, foreground ?? ThemeBrush("TextPrimary"));
    }

    private static Brush ThemeBrush(string key)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Brush brush)
        {
            return brush;
        }

        return new SolidColorBrush(Colors.Transparent);
    }
}
