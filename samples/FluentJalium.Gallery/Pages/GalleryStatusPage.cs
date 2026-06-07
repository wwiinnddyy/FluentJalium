using FluentJalium.Gallery.Controls;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWButton = FluentJalium.Controls.FWButton;
using FWFluentMaterialKind = FluentJalium.Controls.FWFluentMaterialKind;
using FWFluentMaterialSurface = FluentJalium.Controls.FWFluentMaterialSurface;
using FWInfoBadge = FluentJalium.Controls.FWInfoBadge;
using FWInfoBadgeSeverity = FluentJalium.Controls.FWInfoBadgeSeverity;
using FWInfoBar = FluentJalium.Controls.FWInfoBar;
using FWProgressBar = FluentJalium.Controls.FWProgressBar;
using FWSnackbar = FluentJalium.Controls.FWSnackbar;
using FWSnackbarCloseReason = FluentJalium.Controls.FWSnackbarCloseReason;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWStatusBar = FluentJalium.Controls.FWStatusBar;
using FWStatusBarItem = FluentJalium.Controls.FWStatusBarItem;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWToastNotificationHost = FluentJalium.Controls.FWToastNotificationHost;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;
using FWSnackbarHost = FluentJalium.Controls.FWSnackbarHost;
using FWSnackbarPlacement = FluentJalium.Controls.FWSnackbarPlacement;
using FWSnackbarService = FluentJalium.Controls.FWSnackbarService;
using System.Windows.Input;

namespace FluentJalium.Gallery.Pages;

internal sealed class GalleryStatusPage
{
    public UIElement CreateContent()
    {
        var panel = CreateSection("Notifications and Status");
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        examples.Children.Add(CreateStatusExampleCard(
            FluentIconRegular.Info24,
            "FWInfoBar",
            "Severity, open state, close events, icon visibility, and long-message layouts.",
            CreateInfoBarStatusSample()));
        examples.Children.Add(CreateStatusExampleCard(
            FluentIconRegular.AlertBadge24,
            "FWInfoBadge",
            "Dot, value, overflow, icon, and severity resources for lightweight status indicators.",
            CreateInfoBadgeStatusSample()));
        examples.Children.Add(CreateStatusExampleCard(
            FluentIconRegular.Alert24,
            "FWToastNotificationHost",
            "Interactive toast queue, severity actions, visible-count limit, position, and dismiss behavior.",
            CreateToastNotificationStatusSample()));
        examples.Children.Add(CreateStatusExampleCard(
            FluentIconRegular.AlertSnooze24,
            "FWSnackbar",
            "Snackbar host queues transient messages with max-visible slots, action commands, close state, and auto-dismiss controls.",
            CreateSnackbarStatusSample()));
        examples.Children.Add(CreateStatusExampleCard(
            FluentIconRegular.Status24,
            "FWStatusBar",
            "Bottom app status with text items, disabled items, progress content, and status badges.",
            CreateStatusBarStatusSample()));
        examples.Children.Add(CreateStatusExampleCard(
            FluentIconRegular.LayerDiagonalSparkle24,
            "Material operations console",
            "InfoBar, badges, toast queue, and StatusBar remain readable on a LiquidGlass status surface.",
            CreateMaterialOperationsConsoleSample()));

        panel.Children.Add(examples);
        return panel;
    }

    private static UIElement CreateInfoBarStatusSample()
    {
        var output = CreateStatusOutput("Closed events: 0");
        var infoBar = CreateInfoBar("Information", "A normal FluentJalium status message.", InfoBarSeverity.Informational);
        infoBar.IsClosable = true;

        var closedEvents = 0;
        infoBar.Closed += (_, _) =>
        {
            closedEvents++;
            output.Text = $"Closed events: {closedEvents}";
        };

        void ShowInfoBar(InfoBarSeverity severity, string title, string message)
        {
            infoBar.Severity = severity;
            infoBar.Title = title;
            infoBar.Message = message;
            infoBar.IsOpen = true;
            output.Text = $"Showing: {severity}";
        }

        var actionRow = CreateStatusButtonRow(
            CreateStatusActionButton(FluentIconRegular.Info24, "Info", () => ShowInfoBar(InfoBarSeverity.Informational, "Information", "A normal FluentJalium status message.")),
            CreateStatusActionButton(FluentIconRegular.CheckmarkCircle24, "Success", () => ShowInfoBar(InfoBarSeverity.Success, "Success", "The selected operation completed.")),
            CreateStatusActionButton(FluentIconRegular.Warning24, "Warning", () => ShowInfoBar(InfoBarSeverity.Warning, "Warning", "Review settings before continuing.")),
            CreateStatusActionButton(FluentIconRegular.ErrorCircle24, "Error", () => ShowInfoBar(InfoBarSeverity.Error, "Error", "A required resource could not be loaded.")));

        var optionRow = CreateStatusButtonRow(
            CreateStatusActionButton(FluentIconRegular.Badge24, "Icon", () =>
            {
                infoBar.IsIconVisible = !infoBar.IsIconVisible;
                infoBar.InvalidateVisual();
                output.Text = $"Icon visible: {infoBar.IsIconVisible}";
            }),
            CreateStatusActionButton(FluentIconRegular.TextDescription24, "Long", () => ShowInfoBar(
                InfoBarSeverity.Warning,
                "Review required",
                "This longer message checks wrapping, icon spacing, close affordance, and the Fluent severity color resources under the current theme.")),
            CreateStatusActionButton(FluentIconRegular.DismissCircle24, "Close", () => infoBar.IsOpen = false));

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                infoBar,
                actionRow,
                optionRow,
                CreateStatus(output)
            }
        };
    }

    private static UIElement CreateInfoBadgeStatusSample()
    {
        var output = CreateStatusOutput("Preview: Critical overflow value 99+");
        var preview = new FWInfoBadge
        {
            Value = 128,
            MaxValue = 99,
            Severity = FWInfoBadgeSeverity.Critical
        };

        var previewRow = new FWStackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Children =
            {
                CreateStatusCaption("Preview"),
                preview
            }
        };

        void SetSeverity(FWInfoBadgeSeverity severity)
        {
            preview.Severity = severity;
            output.Text = $"Preview severity: {severity}, kind: {preview.DisplayKind}";
        }

        var severityRow = CreateStatusButtonRow(
            CreateStatusActionButton(FluentIconRegular.AlertBadge24, "Attention", () => SetSeverity(FWInfoBadgeSeverity.Attention)),
            CreateStatusActionButton(FluentIconRegular.Info24, "Info", () => SetSeverity(FWInfoBadgeSeverity.Informational)),
            CreateStatusActionButton(FluentIconRegular.CheckmarkCircle24, "Success", () => SetSeverity(FWInfoBadgeSeverity.Success)),
            CreateStatusActionButton(FluentIconRegular.Warning24, "Caution", () => SetSeverity(FWInfoBadgeSeverity.Caution)),
            CreateStatusActionButton(FluentIconRegular.ErrorCircle24, "Critical", () => SetSeverity(FWInfoBadgeSeverity.Critical)));

        var modeRow = CreateStatusButtonRow(
            CreateStatusActionButton(FluentIconRegular.CircleMultipleConcentric24, "Dot", () =>
            {
                preview.Value = -1;
                preview.IconGlyph = null;
                output.Text = $"Preview kind: {preview.DisplayKind}";
            }),
            CreateStatusActionButton(FluentIconRegular.NumberSymbol24, "Value", () =>
            {
                preview.IconGlyph = null;
                preview.Value = 8;
                output.Text = $"Preview value: {preview.DisplayValueText}";
            }),
            CreateStatusActionButton(FluentIconRegular.DataUsage24, "Overflow", () =>
            {
                preview.IconGlyph = null;
                preview.Value = 128;
                preview.MaxValue = 99;
                output.Text = $"Preview value: {preview.DisplayValueText}";
            }),
            CreateStatusActionButton(FluentIconRegular.Badge24, "Icon", () =>
            {
                preview.Value = -1;
                preview.IconGlyph = IconGlyph(FluentIconRegular.Badge24);
                output.Text = $"Preview kind: {preview.DisplayKind}";
            }));

        var matrix = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8
        };
        matrix.Children.Add(CreateInfoBadgeSeverityRow("Attention", FWInfoBadgeSeverity.Attention));
        matrix.Children.Add(CreateInfoBadgeSeverityRow("Info", FWInfoBadgeSeverity.Informational));
        matrix.Children.Add(CreateInfoBadgeSeverityRow("Success", FWInfoBadgeSeverity.Success));
        matrix.Children.Add(CreateInfoBadgeSeverityRow("Caution", FWInfoBadgeSeverity.Caution));
        matrix.Children.Add(CreateInfoBadgeSeverityRow("Critical", FWInfoBadgeSeverity.Critical));

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                previewRow,
                severityRow,
                modeRow,
                matrix,
                CreateStatus(output)
            }
        };
    }

    private static UIElement CreateToastNotificationStatusSample()
    {
        var output = CreateStatusOutput("Visible toasts: 0");
        var toastHost = new FWToastNotificationHost
        {
            Width = 470,
            Height = 220,
            MaxVisibleToasts = 3,
            Position = ToastPosition.TopLeft,
            ToastWidth = 430,
            Spacing = 8
        };

        void UpdateOutput(string action)
        {
            output.Text = $"{action}. Visible toasts: {toastHost.Children.Count}";
        }

        void ShowToast(ToastSeverity severity, string title, string message)
        {
            var toast = toastHost.Show(severity, title, message, TimeSpan.FromSeconds(12));
            toast.IsAutoDismissEnabled = false;
            toast.Closed += (_, _) => UpdateOutput($"Closed {severity}");
            UpdateOutput($"Shown {severity}");
        }

        var actionRow = CreateStatusButtonRow(
            CreateStatusActionButton(FluentIconRegular.Info24, "Info", () => ShowToast(ToastSeverity.Information, "Information", "A normal in-app toast notification.")),
            CreateStatusActionButton(FluentIconRegular.CheckmarkCircle24, "Success", () => ShowToast(ToastSeverity.Success, "Build complete", "The latest gallery sample finished successfully.")),
            CreateStatusActionButton(FluentIconRegular.Warning24, "Warning", () => ShowToast(ToastSeverity.Warning, "Review settings", "MaxVisibleToasts keeps the newest three items.")),
            CreateStatusActionButton(FluentIconRegular.ErrorCircle24, "Error", () => ShowToast(ToastSeverity.Error, "Load failed", "A required resource could not be loaded.")));

        var optionRow = CreateStatusButtonRow(
            CreateStatusActionButton(FluentIconRegular.ArrowUpLeft24, "Top left", () =>
            {
                toastHost.Position = ToastPosition.TopLeft;
                UpdateOutput("Position: TopLeft");
            }),
            CreateStatusActionButton(FluentIconRegular.ArrowDownRight24, "Bottom right", () =>
            {
                toastHost.Position = ToastPosition.BottomRight;
                UpdateOutput("Position: BottomRight");
            }),
            CreateStatusActionButton(FluentIconRegular.DismissCircle24, "Dismiss", () =>
            {
                toastHost.DismissAll();
                UpdateOutput("Dismissed all");
            }));

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                new FWBorder
                {
                    Width = 470,
                    Height = 220,
                    Background = ThemeBrush("SurfaceBackground"),
                    BorderBrush = ThemeBrush("ControlBorder"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    Child = toastHost
                },
                actionRow,
                optionRow,
                CreateStatus(output)
            }
        };
    }

    private static UIElement CreateSnackbarStatusSample()
    {
        var output = CreateStatusOutput("Host queue ready: current Draft archived. Visible 1/2, queued 1, closed 0, actions 0.");
        var host = new FWSnackbarHost
        {
            Width = 470,
            MaxVisibleSnackbars = 2,
            Placement = FWSnackbarPlacement.Bottom
        };
        var service = new FWSnackbarService();
        service.SetHost(host);

        var autoDismissEnabled = false;
        var closedEvents = 0;
        var lastCloseReason = FWSnackbarCloseReason.None;
        var actionRequests = 0;
        var lastCommandParameter = "none";
        var actionCommand = new GallerySnackbarCommand(parameter =>
        {
            actionRequests++;
            lastCommandParameter = parameter?.ToString() ?? "null";
        });

        void UpdateOutput(string action)
        {
            var currentTitle = host.CurrentSnackbar?.Title?.ToString() ?? "none";
            output.Text = $"{action}. Current: {currentTitle}. Visible: {host.Snackbars.Count}/{host.MaxVisibleSnackbars}. Queued: {host.PendingCount}. Closed: {closedEvents}. Last close: {lastCloseReason}. Actions: {actionRequests}. Last parameter: {lastCommandParameter}. Auto-dismiss: {(autoDismissEnabled ? "On" : "Off")}.";
        }

        FWSnackbar CreateQueuedSnackbar(ToastSeverity severity, string title, string message, string actionContent, string actionParameter)
        {
            var snackbar = new FWSnackbar
            {
                Width = 430,
                Severity = severity,
                Title = title,
                Message = message,
                ActionContent = actionContent,
                ActionCommand = actionCommand,
                ActionCommandParameter = actionParameter,
                IsClosable = true,
                IsAutoDismissEnabled = autoDismissEnabled,
                Duration = TimeSpan.FromSeconds(8)
            };
            snackbar.ActionClick += (_, args) =>
            {
                args.Handled = true;
                snackbar.Message = $"Action command handled with parameter: {snackbar.ActionCommandParameter}.";
                UpdateOutput($"Action event handled. Command executed: {args.CommandExecuted}");
            };
            return snackbar;
        }

        void EnqueueSnackbar(ToastSeverity severity, string title, string message, string actionContent, string actionParameter)
        {
            var snackbar = service.Enqueue(CreateQueuedSnackbar(severity, title, message, actionContent, actionParameter));
            snackbar.Closed += (_, _) =>
            {
                closedEvents++;
                lastCloseReason = snackbar.LastCloseReason;
                UpdateOutput($"Closed {title}");
            };
            UpdateOutput($"Enqueued {severity}");
        }

        void EnqueueBatch()
        {
            EnqueueSnackbar(ToastSeverity.Information, "Draft archived", "Undo can restore the current gallery draft.", "Undo", "draft-archive");
            EnqueueSnackbar(ToastSeverity.Success, "Preview refreshed", "The host keeps no more than two snackbar items visible.", "Open", "preview-refresh");
            EnqueueSnackbar(ToastSeverity.Warning, "Review queued", "Pending snackbar requests wait until a visible item closes.", "Review", "review-queue");
            UpdateOutput("Enqueued three snackbar requests");
        }

        void ToggleAutoDismiss()
        {
            autoDismissEnabled = !autoDismissEnabled;
            foreach (var snackbar in host.Snackbars)
            {
                snackbar.IsAutoDismissEnabled = autoDismissEnabled;
            }

            UpdateOutput($"Auto-dismiss {(autoDismissEnabled ? "enabled" : "disabled")}");
        }

        EnqueueSnackbar(ToastSeverity.Information, "Draft archived", "The current gallery draft moved to the archive.", "Undo", "draft-archive");
        EnqueueSnackbar(ToastSeverity.Success, "Changes saved", "The local Gallery sample was saved successfully.", "Open", "changes-saved");
        UpdateOutput("Host queue ready");

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                new FWBorder
                {
                    Width = 470,
                    Height = 170,
                    Background = ThemeBrush("SurfaceBackground"),
                    BorderBrush = ThemeBrush("ControlBorder"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(10),
                    Child = host
                },
                CreateStatusButtonRow(
                    CreateStatusActionButton(FluentIconRegular.Info24, "Info", () => EnqueueSnackbar(
                        ToastSeverity.Information,
                        "Draft archived",
                        "The current gallery draft moved to the archive.",
                        "Undo",
                        "draft-archive")),
                    CreateStatusActionButton(FluentIconRegular.CheckmarkCircle24, "Success", () => EnqueueSnackbar(
                        ToastSeverity.Success,
                        "Changes saved",
                        "The local Gallery sample was saved successfully.",
                        "Open",
                        "changes-saved")),
                    CreateStatusActionButton(FluentIconRegular.Warning24, "Warning", () => EnqueueSnackbar(
                        ToastSeverity.Warning,
                        "Review before upload",
                        "The next push includes Gallery-only changes.",
                        "Review",
                        "review-before-upload")),
                    CreateStatusActionButton(FluentIconRegular.DocumentQueueAdd24, "Queue x3", EnqueueBatch)),
                CreateStatusButtonRow(
                    CreateStatusActionButton(FluentIconRegular.ArrowUndo24, "Action", () =>
                    {
                        var handled = host.CurrentSnackbar?.RequestAction();
                        UpdateOutput($"Current action requested. Handled: {handled?.ToString() ?? "none"}");
                    }),
                    CreateStatusActionButton(FluentIconRegular.Dismiss24, "Close", () =>
                    {
                        var closed = service.CloseCurrent();
                        if (!closed)
                        {
                            UpdateOutput("Close requested with no current snackbar");
                        }
                    }),
                    CreateStatusActionButton(FluentIconRegular.Clock24, "Auto", ToggleAutoDismiss),
                    CreateStatusActionButton(FluentIconRegular.DismissCircle24, "Clear", () =>
                    {
                        service.Clear();
                        lastCloseReason = FWSnackbarCloseReason.HostCleared;
                        UpdateOutput("Cleared host queue");
                    })),
                CreateStatus(output)
            }
        };
    }

    private static UIElement CreateStatusBarStatusSample()
    {
        var output = CreateStatusOutput("StatusBar items: 5");
        var disabledItem = new FWStatusBarItem { Content = "Read-only", IsEnabled = false };
        var statusBar = new FWStatusBar
        {
            Width = 470
        };
        statusBar.Items.Add(new FWStatusBarItem { Content = "Ready" });
        statusBar.Items.Add(new FWStatusBarItem { Content = "Line 42" });
        statusBar.Items.Add(new FWStatusBarItem { Content = "UTF-8" });
        statusBar.Items.Add(new FWStatusBarItem
        {
            Content = new FWStackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 6,
                Children =
                {
                    new FWInfoBadge { Severity = FWInfoBadgeSeverity.Success },
                    new FWTextBlock { Text = "Online", FontSize = 12, Foreground = ThemeBrush("StatusBarForeground") }
                }
            }
        });
        statusBar.Items.Add(disabledItem);

        var progressItem = new FWStatusBarItem
        {
            Content = new FWStackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children =
                {
                    new FWTextBlock { Text = "Sync", FontSize = 12, Foreground = ThemeBrush("StatusBarForeground") },
                    new FWProgressBar { Width = 64, Height = 4, Minimum = 0, Maximum = 100, Value = 72 }
                }
            }
        };

        var optionRow = CreateStatusButtonRow(
            CreateStatusActionButton(FluentIconRegular.DismissCircle24, "Disabled", () =>
            {
                disabledItem.IsEnabled = !disabledItem.IsEnabled;
                output.Text = $"Read-only enabled: {disabledItem.IsEnabled}";
            }),
            CreateStatusActionButton(FluentIconRegular.CloudSync24, "Progress", () =>
            {
                if (!statusBar.Items.Contains(progressItem))
                {
                    statusBar.Items.Add(progressItem);
                }

                output.Text = $"StatusBar items: {statusBar.Items.Count}";
            }),
            CreateStatusActionButton(FluentIconRegular.SubtractCircle24, "Remove", () =>
            {
                statusBar.Items.Remove(progressItem);
                output.Text = $"StatusBar items: {statusBar.Items.Count}";
            }));

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                statusBar,
                optionRow,
                CreateStatus(output)
            }
        };
    }

    private static UIElement CreateMaterialOperationsConsoleSample()
    {
        var output = CreateStatusOutput("Console: LiquidGlass. Visible toasts: 0. Status items: 4.");
        var infoBar = CreateInfoBar("Deployment monitor", "Pipeline is ready and watching service health.", InfoBarSeverity.Success);
        infoBar.Width = 500;
        infoBar.IsClosable = true;

        var healthMetricBadge = new FWInfoBadge
        {
            Severity = FWInfoBadgeSeverity.Success,
            IconGlyph = IconGlyph(FluentIconRegular.CheckmarkCircle24)
        };
        var healthStatusBadge = new FWInfoBadge
        {
            Severity = FWInfoBadgeSeverity.Success
        };
        var eventMetricBadge = new FWInfoBadge
        {
            Severity = FWInfoBadgeSeverity.Critical,
            Value = 128,
            MaxValue = 99
        };
        var eventStatusBadge = new FWInfoBadge
        {
            Severity = FWInfoBadgeSeverity.Critical,
            Value = 128,
            MaxValue = 99
        };
        var toastHost = new FWToastNotificationHost
        {
            Width = 500,
            Height = 172,
            MaxVisibleToasts = 3,
            Position = ToastPosition.BottomRight,
            ToastWidth = 460,
            Spacing = 8
        };
        var statusBar = new FWStatusBar
        {
            Width = 500
        };
        statusBar.Items.Add(new FWStatusBarItem { Content = "Ready" });
        statusBar.Items.Add(new FWStatusBarItem { Content = "Region: West" });
        statusBar.Items.Add(new FWStatusBarItem
        {
            Content = new FWStackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 6,
                Children =
                {
                    healthStatusBadge,
                    new FWTextBlock { Text = "Healthy", FontSize = 12, Foreground = ThemeBrush("StatusBarForeground") }
                }
            }
        });
        statusBar.Items.Add(new FWStatusBarItem
        {
            Content = new FWStackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children =
                {
                    new FWTextBlock { Text = "Queue", FontSize = 12, Foreground = ThemeBrush("StatusBarForeground") },
                    eventStatusBadge
                }
            }
        });

        var syncItem = new FWStatusBarItem
        {
            Content = new FWStackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children =
                {
                    new FWTextBlock { Text = "Sync", FontSize = 12, Foreground = ThemeBrush("StatusBarForeground") },
                    new FWProgressBar { Width = 72, Height = 4, Minimum = 0, Maximum = 100, Value = 64 }
                }
            }
        };

        void UpdateOutput(string action)
        {
            output.Text = $"{action}. Visible toasts: {toastHost.Children.Count}. Status items: {statusBar.Items.Count}.";
        }

        void ShowToast(ToastSeverity severity, string title, string message)
        {
            var toast = toastHost.Show(severity, title, message, TimeSpan.FromSeconds(12));
            toast.IsAutoDismissEnabled = false;
            toast.Closed += (_, _) => UpdateOutput($"Closed {severity}");
            UpdateOutput($"Shown {severity}");
        }

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
                    infoBar,
                    new FWStackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 12,
                        Children =
                        {
                            CreateBadgeMetric(FluentIconRegular.Server24, "Health", healthMetricBadge),
                            CreateBadgeMetric(FluentIconRegular.AlertBadge24, "Events", eventMetricBadge)
                        }
                    },
                    new FWBorder
                    {
                        Width = 500,
                        Height = 172,
                        Background = ThemeBrush("SurfaceBackground"),
                        BorderBrush = ThemeBrush("ControlBorder"),
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(6),
                        Child = toastHost
                    },
                    statusBar,
                    CreateStatusButtonRow(
                        CreateStatusActionButton(FluentIconRegular.CheckmarkCircle24, "Success", () => ShowToast(ToastSeverity.Success, "Deployment complete", "All services reported healthy.")),
                        CreateStatusActionButton(FluentIconRegular.Warning24, "Warning", () =>
                        {
                            infoBar.Severity = InfoBarSeverity.Warning;
                            infoBar.Title = "Review required";
                            infoBar.Message = "One region has elevated latency.";
                            infoBar.IsOpen = true;
                            healthMetricBadge.Severity = FWInfoBadgeSeverity.Caution;
                            healthStatusBadge.Severity = FWInfoBadgeSeverity.Caution;
                            ShowToast(ToastSeverity.Warning, "Latency warning", "West region is above the preferred threshold.");
                        }),
                        CreateStatusActionButton(FluentIconRegular.ErrorCircle24, "Error", () =>
                        {
                            infoBar.Severity = InfoBarSeverity.Error;
                            infoBar.Title = "Incident";
                            infoBar.Message = "A required endpoint is unavailable.";
                            infoBar.IsOpen = true;
                            healthMetricBadge.Severity = FWInfoBadgeSeverity.Critical;
                            healthStatusBadge.Severity = FWInfoBadgeSeverity.Critical;
                            ShowToast(ToastSeverity.Error, "Endpoint failed", "Investigate the service endpoint before retrying.");
                        }),
                        CreateStatusActionButton(FluentIconRegular.CloudSync24, "Sync", () =>
                        {
                            if (!statusBar.Items.Contains(syncItem))
                            {
                                statusBar.Items.Add(syncItem);
                            }

                            UpdateOutput("Sync status added");
                        }),
                        CreateStatusActionButton(FluentIconRegular.DismissCircle24, "Clear", () =>
                        {
                            toastHost.DismissAll();
                            statusBar.Items.Remove(syncItem);
                            infoBar.IsOpen = !infoBar.IsOpen;
                            UpdateOutput($"InfoBar open: {infoBar.IsOpen}");
                        })),
                    CreateStatus(output)
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
                    Text = "Layered operations surface",
                    FontSize = 15,
                    Foreground = ThemeBrush("TextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
    }

    private static FWBorder CreateBadgeMetric(FluentIconRegular icon, string label, FWInfoBadge badge)
    {
        return new FWBorder
        {
            Width = 244,
            Background = ThemeBrush("LayerFillColorDefaultBrush"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10,
                Children =
                {
                    CreateIcon(icon, 18, ThemeBrush("TextSecondary")),
                    new FWTextBlock
                    {
                        Text = label,
                        Foreground = ThemeBrush("TextPrimary"),
                        VerticalAlignment = VerticalAlignment.Center
                    },
                    badge
                }
            }
        };
    }

    private static FWBorder CreateStatusExampleCard(FluentIconRegular icon, string title, string description, UIElement content)
    {
        return GallerySampleCard.Create(icon, title, description, content, code: CreateSampleCode(title));
    }

    private static string CreateSampleCode(string title)
    {
        return title switch
        {
            "FWInfoBar" => "<FWInfoBar Severity=\"Warning\" Title=\"Review required\" Message=\"Check settings before continuing\" IsOpen=\"True\" />",
            "FWInfoBadge" => "<FWInfoBadge Severity=\"Critical\" Value=\"128\" MaxValue=\"99\" />\n<FWInfoBadge Severity=\"Success\" IconGlyph=\"CheckmarkCircle24\" />",
            "FWToastNotificationHost" => "<FWToastNotificationHost MaxVisibleToasts=\"3\" Position=\"BottomRight\" />",
            "FWSnackbar" => "var service = new FWSnackbarService();\nservice.SetHost(snackbarHost);\nvar snackbar = service.Enqueue(new FWSnackbar\n{\n    Title = \"Draft archived\",\n    ActionContent = \"Undo\",\n    ActionCommand = UndoCommand,\n    ActionCommandParameter = \"draft-archive\"\n});\nvar closeReason = snackbar.LastCloseReason;",
            "FWStatusBar" => "<FWStatusBar>\n    <FWStatusBarItem Content=\"Ready\" />\n    <FWStatusBarItem Content=\"UTF-8\" />\n</FWStatusBar>",
            "Material operations console" => "<FWFluentMaterialSurface MaterialKind=\"LiquidGlass\">\n    <FWInfoBar Severity=\"Success\" IsOpen=\"True\" />\n    <FWStatusBar />\n</FWFluentMaterialSurface>",
            _ => "<FWInfoBar IsOpen=\"True\" />"
        };
    }

    private static FWWrapPanel CreateStatusButtonRow(params FWButton[] buttons)
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

    private static FWButton CreateStatusActionButton(FluentIconRegular icon, string text, Action action)
    {
        var button = new FWButton
        {
            Content = new FWStackPanel
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
            }
        };
        button.Click += (_, _) => action();
        return button;
    }

    private sealed class GallerySnackbarCommand : ICommand
    {
        private readonly Action<object?> _execute;

        public GallerySnackbarCommand(Action<object?> execute)
        {
            _execute = execute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => _execute(parameter);
    }

    private static FWStackPanel CreateInfoBadgeSeverityRow(string label, FWInfoBadgeSeverity severity)
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Children =
            {
                new FWTextBlock
                {
                    Text = label,
                    Width = 76,
                    FontSize = 12,
                    Foreground = ThemeBrush("TextSecondary"),
                    VerticalAlignment = VerticalAlignment.Center
                },
                new FWInfoBadge { Severity = severity },
                new FWInfoBadge { Severity = severity, Value = 8 },
                new FWInfoBadge { Severity = severity, Value = 128, MaxValue = 99 },
                new FWInfoBadge { Severity = severity, IconGlyph = IconGlyph(FluentIconRegular.Badge24) }
            }
        };
    }

    private static TextBlock CreateStatusCaption(string text)
    {
        return new TextBlock
        {
            Text = text,
            Width = 76,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary"),
            VerticalAlignment = VerticalAlignment.Center
        };
    }

    private static TextBlock CreateStatusOutput(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        };
    }

    private static FWBorder CreateStatus(TextBlock status)
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

    private static FWInfoBar CreateInfoBar(string title, string message, InfoBarSeverity severity)
    {
        return new FWInfoBar
        {
            Title = title,
            Message = message,
            Severity = severity,
            IsOpen = true,
            IsClosable = false,
            Width = 460
        };
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
                        CreateIcon(FluentIconRegular.AlertBadge24, 24, ThemeBrush("TextPrimary")),
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

    private static FluentIcon CreateIcon(FluentIconRegular icon, double size, Brush? foreground = null)
    {
        return FluentIconFactory.Regular(icon, size, foreground ?? ThemeBrush("TextPrimary"));
    }

    private static string IconGlyph(FluentIconRegular icon)
    {
        return icon.GetString();
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
