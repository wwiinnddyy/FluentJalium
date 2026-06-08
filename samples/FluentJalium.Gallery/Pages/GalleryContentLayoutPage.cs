using System.Windows.Input;
using FluentJalium.Icon;
using FluentJalium.Gallery.Controls;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Media;
using Jalium.UI.Media.Animation;
using FWAccessText = FluentJalium.Controls.FWAccessText;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWButton = FluentJalium.Controls.FWButton;
using FWCanvas = FluentJalium.Controls.FWCanvas;
using FWContentControl = FluentJalium.Controls.FWContentControl;
using FWContentPresenter = FluentJalium.Controls.FWContentPresenter;
using FWFluentMaterialKind = FluentJalium.Controls.FWFluentMaterialKind;
using FWFluentMaterialSurface = FluentJalium.Controls.FWFluentMaterialSurface;
using FWGrid = FluentJalium.Controls.FWGrid;
using FWLabel = FluentJalium.Controls.FWLabel;
using FWParallaxView = FluentJalium.Controls.FWParallaxView;
using FWRelativePanel = FluentJalium.Controls.FWRelativePanel;
using FWSettingsCard = FluentJalium.Controls.FWSettingsCard;
using FWSettingsCardAutomationDiagnostics = FluentJalium.Controls.FWSettingsCardAutomationDiagnostics;
using FWSettingsCardDiagnostics = FluentJalium.Controls.FWSettingsCardDiagnostics;
using FWScroller = FluentJalium.Controls.FWScroller;
using FWScrollViewer = FluentJalium.Controls.FWScrollViewer;
using FWSplitView = FluentJalium.Controls.FWSplitView;
using FWSplitViewDisplayMode = FluentJalium.Controls.FWSplitViewDisplayMode;
using FWSplitViewPanePlacement = FluentJalium.Controls.FWSplitViewPanePlacement;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWTwoPaneView = FluentJalium.Controls.FWTwoPaneView;
using FWTwoPaneViewMode = FluentJalium.Controls.FWTwoPaneViewMode;
using FWTwoPaneViewPriority = FluentJalium.Controls.FWTwoPaneViewPriority;
using FWTransitioningContentControl = FluentJalium.Controls.FWTransitioningContentControl;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;

namespace FluentJalium.Gallery.Pages;

internal sealed class GalleryContentLayoutPage
{
    public UIElement CreateContent()
    {
        var panel = CreateSection("Content and Layout");
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        examples.Children.Add(CreateLayoutExampleCard(
            FluentIconRegular.Textbox24,
            "Text and access text",
            "TextBlock, selectable body text, wrapping, trimming, label, and AccessText state.",
            CreateTextContentSample()));
        examples.Children.Add(CreateLayoutExampleCard(
            FluentIconRegular.Layer24,
            "Border and content hosts",
            "Border, ContentControl, ContentPresenter, padding, corner radius, and inherited foreground.",
            CreateContentHostSample()));
        examples.Children.Add(CreateLayoutExampleCard(
            FluentIconRegular.LayoutColumnTwo24,
            "Stack, wrap, and grid layout",
            "StackPanel spacing, WrapPanel chips, Grid spacing, rows, columns, and spanning cells.",
            CreatePanelLayoutSample()));
        examples.Children.Add(CreateLayoutExampleCard(
            FluentIconRegular.PanelLeftContract24,
            "SplitView pane layout",
            "SplitView pane placement, compact length, overlay modes, and open/close state.",
            CreateSplitViewLayoutSample()));
        examples.Children.Add(CreateLayoutExampleCard(
            FluentIconRegular.DualScreenTablet24,
            "Adaptive settings layout",
            "TwoPaneView ActualMode, VisiblePane, diagnostics, and SettingsCard command state.",
            CreateAdaptiveSettingsLayoutSample()));
        examples.Children.Add(CreateLayoutExampleCard(
            FluentIconRegular.LayerDiagonalSparkle24,
            "Canvas, relative, and parallax layout",
            "Canvas placement, RelativePanel compatibility, and ParallaxView Progress/CurrentOffset diagnostics.",
            CreatePositioningParallaxSample()));
        examples.Children.Add(CreateLayoutExampleCard(
            FluentIconRegular.SlideTransition24,
            "Transitioning content",
            "TransitioningContentControl switches between slide and LiquidMorph content surfaces.",
            CreateTransitioningContentSample()));
        examples.Children.Add(CreateLayoutExampleCard(
            FluentIconRegular.LayerDiagonalSparkle24,
            "Material layout surface",
            "Text, content hosts, grid cells, and transition content stay readable on LiquidGlass.",
            CreateMaterialLayoutSurfaceSample()));

        panel.Children.Add(examples);
        return panel;
    }

    private static UIElement CreateTextContentSample()
    {
        var output = CreateLayoutOutput("Text: selection enabled, access key O.");
        var title = new FWTextBlock
        {
            Text = "Title text",
            FontFamily = "Segoe UI Variable Display",
            FontSize = 22,
            Foreground = ThemeBrush("TextPrimary")
        };
        var body = new FWTextBlock
        {
            Text = "Selectable body copy follows Fluent typography and wraps inside its layout column.",
            IsTextSelectionEnabled = true,
            TextWrapping = TextWrapping.Wrap,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Foreground = ThemeBrush("TextSecondary")
        };
        var accessText = new FWAccessText
        {
            Text = "_Open command",
            Foreground = ThemeBrush("TextPrimary")
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 360,
            Children =
            {
                new FWLabel { Content = "FWTextBlock" },
                title,
                body,
                accessText,
                CreateLayoutButtonRow(
                    CreateLayoutActionButton(FluentIconRegular.TextBold24, "Emphasis", () =>
                    {
                        title.FontSize = title.FontSize == 22 ? 26 : 22;
                        output.Text = $"Title font size: {title.FontSize}";
                    }),
                    CreateLayoutActionButton(FluentIconRegular.TextEditStyle24, "Replace", () =>
                    {
                        body.Text = "Updated selectable copy keeps wrapping inside the same surface.";
                        output.Text = "Body text replaced.";
                    })),
                CreateLayoutStatus(output)
            }
        };
    }

    private static UIElement CreateContentHostSample()
    {
        var output = CreateLayoutOutput("Hosts: Border child and ContentControl content ready.");
        var hostedText = new FWTextBlock
        {
            Text = "FWContentControl hosts text content with inherited Fluent text styling.",
            Foreground = ThemeBrush("TextPrimary"),
            TextWrapping = TextWrapping.Wrap
        };
        var border = new FWBorder
        {
            Background = ThemeBrush("SurfaceBackground"),
            BorderBrush = ThemeBrush("ContentSurfaceBorderBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(14),
            Child = new FWContentControl
            {
                Content = hostedText,
                Foreground = ThemeBrush("TextPrimary"),
                Padding = new Thickness(0)
            }
        };
        var presenterText = new FWTextBlock
        {
            Text = "FWContentPresenter mirrors content without adding surface chrome.",
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        };
        var presenter = new FWContentPresenter
        {
            Content = presenterText
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 390,
            Children =
            {
                new FWLabel { Content = "FWBorder and content hosts" },
                border,
                presenter,
                CreateLayoutButtonRow(
                    CreateLayoutActionButton(FluentIconRegular.BorderOutside24, "Radius", () =>
                    {
                        border.CornerRadius = border.CornerRadius.TopLeft == 6
                            ? new CornerRadius(2)
                            : new CornerRadius(6);
                        output.Text = $"Border radius: {border.CornerRadius.TopLeft}";
                    }),
                    CreateLayoutActionButton(FluentIconRegular.TextEditStyle24, "Presenter", () =>
                    {
                        presenter.Content = new FWTextBlock
                        {
                            Text = "Presenter content updated.",
                            Foreground = ThemeBrush("TextPrimary")
                        };
                        output.Text = "ContentPresenter content replaced.";
                    })),
                CreateLayoutStatus(output)
            }
        };
    }

    private static UIElement CreatePanelLayoutSample()
    {
        var output = CreateLayoutOutput("Panels: wrap chips and grid cells ready.");
        var chips = new FWWrapPanel
        {
            HorizontalSpacing = 8,
            VerticalSpacing = 8,
            Children =
            {
                CreateLayoutChip("Stack"),
                CreateLayoutChip("Wrap"),
                CreateLayoutChip("Grid"),
                CreateLayoutChip("Content"),
                CreateLayoutChip("Surface")
            }
        };
        var grid = CreateSampleGrid();

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 390,
            Children =
            {
                new FWLabel { Content = "FWStackPanel, FWWrapPanel, FWGrid" },
                chips,
                grid,
                CreateLayoutButtonRow(
                    CreateLayoutActionButton(FluentIconRegular.Table24, "Spacing", () =>
                    {
                        grid.ColumnSpacing = grid.ColumnSpacing == 8 ? 14 : 8;
                        grid.RowSpacing = grid.RowSpacing == 8 ? 14 : 8;
                        output.Text = $"Grid spacing: {grid.ColumnSpacing}";
                    }),
                    CreateLayoutActionButton(FluentIconRegular.Add24, "Chip", () =>
                    {
                        chips.Children.Add(CreateLayoutChip($"Item {chips.Children.Count + 1}"));
                        output.Text = $"WrapPanel children: {chips.Children.Count}";
                    })),
                CreateLayoutStatus(output)
            }
        };
    }

    private static UIElement CreateAdaptiveSettingsLayoutSample()
    {
        var output = CreateLayoutOutput("TwoPaneView diagnostics ready.");
        FWSettingsCard? modeCard = null;
        var configureCommand = new GalleryLayoutCommand(parameter =>
        {
            var automation = modeCard != null
                ? FormatSettingsCardAutomation(modeCard.GetAutomationDiagnostics())
                : "pending";
            output.Text = $"SettingsCard command executed: {parameter}. Automation: {automation}.";
        });
        modeCard = new FWSettingsCard
        {
            Header = "Display mode",
            Description = "Switches the sample between wide, tall, and single pane.",
            HeaderIcon = CreateIcon(FluentIconRegular.LayoutColumnTwo24, 18, ThemeBrush("TextSecondary")),
            ActionIcon = CreateIcon(FluentIconRegular.ChevronRight24, 16, ThemeBrush("TextSecondary")),
            Content = new FWButton { Content = "Configure" },
            IsClickEnabled = true,
            Command = configureCommand,
            CommandParameter = "display-mode",
            ClickMode = ClickMode.Release
        };
        var syncCard = new FWSettingsCard
        {
            Header = "Sync layout",
            Description = "A compact settings row with a live command area.",
            HeaderIcon = CreateIcon(FluentIconRegular.CloudSync24, 18, ThemeBrush("TextSecondary")),
            Content = new FWButton { Content = "On" },
            IsClickEnabled = true
        };
        var twoPaneView = new FWTwoPaneView
        {
            Width = 500,
            Height = 230,
            Mode = FWTwoPaneViewMode.Wide,
            PanePriority = FWTwoPaneViewPriority.Pane1,
            MinWideModeWidth = 480,
            MinTallModeHeight = 200,
            Background = ThemeBrush("SurfaceBackground"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10),
            Pane1 = CreateAdaptivePane("Navigation", "Master pane"),
            Pane2 = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 8,
                Children =
                {
                    modeCard,
                    syncCard
                }
            }
        };

        void UpdateState(string action)
        {
            var diagnostics = twoPaneView.GetDiagnostics();
            output.Text = $"{action}. Requested: {diagnostics.RequestedMode}; actual: {diagnostics.ActualMode}; visible: {diagnostics.VisiblePane}; priority: {diagnostics.PanePriority}. SettingsCard: {FormatSettingsCardDiagnostics(modeCard.GetDiagnostics())}. Automation: {FormatSettingsCardAutomation(modeCard.GetAutomationDiagnostics())}.";
        }

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 520,
            Children =
            {
                twoPaneView,
                CreateLayoutButtonRow(
                    CreateLayoutActionButton(FluentIconRegular.LayoutColumnTwo24, "Wide", () =>
                    {
                        twoPaneView.Mode = FWTwoPaneViewMode.Wide;
                        UpdateState("TwoPaneView requested Wide");
                    }),
                    CreateLayoutActionButton(FluentIconRegular.LayoutRowTwo24, "Tall", () =>
                    {
                        twoPaneView.Mode = FWTwoPaneViewMode.Tall;
                        UpdateState("TwoPaneView requested Tall");
                    }),
                    CreateLayoutActionButton(FluentIconRegular.PanelLeftContract24, "Single", () =>
                    {
                        twoPaneView.Mode = FWTwoPaneViewMode.SinglePane;
                        UpdateState("TwoPaneView requested SinglePane");
                    }),
                    CreateLayoutActionButton(FluentIconRegular.ChevronRight24, "Priority", () =>
                    {
                        twoPaneView.PanePriority = twoPaneView.PanePriority == FWTwoPaneViewPriority.Pane1
                            ? FWTwoPaneViewPriority.Pane2
                            : FWTwoPaneViewPriority.Pane1;
                        UpdateState("Pane priority toggled");
                    })),
                CreateLayoutButtonRow(
                    CreateLayoutActionButton(FluentIconRegular.DataUsage24, "Diagnostics", () => UpdateState("TwoPaneView diagnostics refreshed")),
                    CreateLayoutActionButton(FluentIconRegular.CursorClick24, "Invoke card", () =>
                    {
                        if (!modeCard.PerformClick())
                        {
                            output.Text = $"SettingsCard blocked. CanExecute: {modeCard.CanExecute}.";
                        }
                    }),
                    CreateLayoutActionButton(FluentIconRegular.Power24, "Command", () =>
                    {
                        configureCommand.CanExecuteResult = !configureCommand.CanExecuteResult;
                        configureCommand.RaiseCanExecuteChanged();
                        output.Text = $"SettingsCard: {FormatSettingsCardDiagnostics(modeCard.GetDiagnostics())}. Automation: {FormatSettingsCardAutomation(modeCard.GetAutomationDiagnostics())}.";
                    }),
                    CreateLayoutActionButton(FluentIconRegular.CursorHover24, "Hover mode", () =>
                    {
                        modeCard.ClickMode = modeCard.ClickMode == ClickMode.Hover
                            ? ClickMode.Release
                            : ClickMode.Hover;
                        output.Text = $"SettingsCard: {FormatSettingsCardDiagnostics(modeCard.GetDiagnostics())}. Automation: {FormatSettingsCardAutomation(modeCard.GetAutomationDiagnostics())}.";
                    })),
                CreateLayoutStatus(output)
            }
        };
    }

    private static UIElement CreateSplitViewLayoutSample()
    {
        var output = CreateLayoutOutput("SplitView: CompactInline closed, left pane length 56.");
        var splitView = new FWSplitView
        {
            Width = 520,
            Height = 230,
            DisplayMode = FWSplitViewDisplayMode.CompactInline,
            PanePlacement = FWSplitViewPanePlacement.Left,
            OpenPaneLength = 208,
            CompactPaneLength = 56,
            IsPaneOpen = false,
            IsLightDismissEnabled = true,
            Background = ThemeBrush("SurfaceBackground"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            PaneBackground = ThemeBrush("LayerFillColorDefaultBrush"),
            ContentBackground = ThemeBrush("ControlBackground"),
            Pane = CreateSplitViewPaneContent(),
            Content = CreateSplitViewMainContent()
        };

        void UpdateState(string action)
        {
            output.Text = $"{action}. Mode: {splitView.DisplayMode}; placement: {splitView.PanePlacement}; open: {FormatOnOff(splitView.IsPaneOpen)}; pane length: {splitView.ActualPaneLength:0}.";
        }

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 540,
            Children =
            {
                splitView,
                CreateLayoutButtonRow(
                    CreateLayoutActionButton(FluentIconRegular.PanelLeftContract24, "Toggle", () =>
                    {
                        splitView.TogglePane();
                        UpdateState("SplitView toggled");
                    }),
                    CreateLayoutActionButton(FluentIconRegular.LayoutColumnTwo24, "Inline", () =>
                    {
                        splitView.DisplayMode = FWSplitViewDisplayMode.CompactInline;
                        splitView.ClosePane();
                        UpdateState("CompactInline requested");
                    }),
                    CreateLayoutActionButton(FluentIconRegular.Layer24, "Overlay", () =>
                    {
                        splitView.DisplayMode = FWSplitViewDisplayMode.CompactOverlay;
                        splitView.OpenPane();
                        UpdateState("CompactOverlay requested");
                    }),
                    CreateLayoutActionButton(FluentIconRegular.ChevronRight24, "Side", () =>
                    {
                        splitView.PanePlacement = splitView.PanePlacement == FWSplitViewPanePlacement.Left
                            ? FWSplitViewPanePlacement.Right
                            : FWSplitViewPanePlacement.Left;
                        UpdateState("Pane placement toggled");
                    })),
                CreateLayoutStatus(output)
            }
        };
    }

    private static string FormatSettingsCardDiagnostics(FWSettingsCardDiagnostics diagnostics)
    {
        return $"invokable {FormatOnOff(diagnostics.IsInvokable)}, click {FormatOnOff(diagnostics.IsClickEnabled)}, can execute {FormatOnOff(diagnostics.CanExecute)}, pressed {FormatOnOff(diagnostics.IsInteractionPressed)}, mode {diagnostics.ClickMode}";
    }

    private static string FormatSettingsCardAutomation(FWSettingsCardAutomationDiagnostics diagnostics)
    {
        return $"{diagnostics.ControlType}/{diagnostics.Name}; invoke: {FormatOnOff(diagnostics.IsInvokePatternAvailable)}; help: {diagnostics.HelpText}";
    }

    private static string FormatOnOff(bool value) => value ? "on" : "off";

    private static UIElement CreatePositioningParallaxSample()
    {
        var output = CreateLayoutOutput("Parallax progress: 0.00, offset 0,0.");
        var canvas = new FWCanvas
        {
            Width = 225,
            Height = 118,
            Background = ThemeBrush("LayerFillColorDefaultBrush")
        };
        var badge = CreatePositionedBadge("Canvas", FluentIconRegular.MoreHorizontal24);
        var action = CreatePositionedBadge("Absolute", FluentIconRegular.TargetSparkle24);
        var note = CreatePositionedBadge("42 px", FluentIconRegular.Ruler24);
        Canvas.SetLeft(badge, 12);
        Canvas.SetTop(badge, 12);
        Canvas.SetLeft(action, 78);
        Canvas.SetTop(action, 50);
        Canvas.SetLeft(note, 146);
        Canvas.SetTop(note, 24);
        canvas.Children.Add(badge);
        canvas.Children.Add(action);
        canvas.Children.Add(note);

        var relativePanel = new FWRelativePanel
        {
            Width = 225,
            Height = 118,
            ColumnSpacing = 8,
            RowSpacing = 8
        };
        var primary = CreateRelativeTile("Anchor", 92, 40);
        var secondary = CreateRelativeTile("RightOf", 92, 40);
        var footer = CreateRelativeTile("Below", 192, 40);
        FWRelativePanel.SetAlignLeftWithPanel(primary, true);
        FWRelativePanel.SetAlignTopWithPanel(primary, true);
        FWRelativePanel.SetRightOf(secondary, primary);
        FWRelativePanel.SetAlignTopWith(secondary, primary);
        FWRelativePanel.SetBelow(footer, primary);
        FWRelativePanel.SetAlignLeftWith(footer, primary);
        relativePanel.Children.Add(primary);
        relativePanel.Children.Add(secondary);
        relativePanel.Children.Add(footer);

        var parallaxView = new FWParallaxView
        {
            Width = 225,
            Height = 82,
            HorizontalShift = 18,
            VerticalShift = 28,
            IsHorizontalShiftEnabled = true,
            Background = ThemeBrush("ControlBackground"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Content = CreateTransitionCard("Depth layer", "ParallaxView")
        };
        var sourceScrollViewer = new FWScrollViewer
        {
            Width = 225,
            Height = 52,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Content = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Children =
                {
                    new FWTextBlock { Text = "Scroll source 0", Margin = new Thickness(8) },
                    new FWTextBlock { Text = "Scroll source 50", Margin = new Thickness(8) },
                    new FWTextBlock { Text = "Scroll source 100", Margin = new Thickness(8) },
                    new FWTextBlock { Text = "Scroll source end", Margin = new Thickness(8) }
                }
            }
        };
        var scroller = new FWScroller();
        scroller.AttachScrollViewer(sourceScrollViewer);
        parallaxView.Source = scroller;

        void SetProgress(double progress)
        {
            parallaxView.Progress = progress;
            var diagnostics = parallaxView.GetDiagnostics();
            output.Text = $"Parallax progress: {diagnostics.Progress:0.00}, source {diagnostics.SourceKind}, current offset {diagnostics.CurrentOffset.X:0},{diagnostics.CurrentOffset.Y:0}.";
        }

        void ScrollSource(double offset)
        {
            sourceScrollViewer.ScrollToVerticalOffset(offset);
            parallaxView.RefreshProgressFromSource();
            var diagnostics = parallaxView.GetDiagnostics();
            output.Text = $"Parallax progress: {diagnostics.Progress:0.00}, source {diagnostics.SourceKind}, current offset {diagnostics.CurrentOffset.X:0},{diagnostics.CurrentOffset.Y:0}.";
        }

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 520,
            Children =
            {
                new FWWrapPanel
                {
                    HorizontalSpacing = 12,
                    VerticalSpacing = 12,
                    Children =
                    {
                        CreateLayoutFrame("FWCanvas", canvas),
                        CreateLayoutFrame("FWRelativePanel", relativePanel),
                        CreateLayoutFrame("FWParallaxView", new FWBorder
                        {
                            Width = 225,
                            Height = 116,
                            Background = ThemeBrush("LayerFillColorDefaultBrush"),
                            BorderBrush = ThemeBrush("ControlBorder"),
                            BorderThickness = new Thickness(1),
                            CornerRadius = new CornerRadius(6),
                            Padding = new Thickness(10),
                            ClipToBounds = true,
                            Child = parallaxView
                        }),
                        CreateLayoutFrame("Source", sourceScrollViewer)
                    }
                },
                CreateLayoutButtonRow(
                    CreateLayoutActionButton(FluentIconRegular.NumberSymbol24, "0%", () => SetProgress(0)),
                    CreateLayoutActionButton(FluentIconRegular.DataUsage24, "50%", () => SetProgress(0.5)),
                    CreateLayoutActionButton(FluentIconRegular.CheckmarkCircle24, "100%", () => SetProgress(1)),
                    CreateLayoutActionButton(FluentIconRegular.ArrowDown24, "Scroll source", () => ScrollSource(84))),
                CreateLayoutStatus(output)
            }
        };
    }

    private static UIElement CreateTransitioningContentSample()
    {
        var output = CreateLayoutOutput("Transition: SlideLeft with text content.");
        var transitionHost = new FWTransitioningContentControl
        {
            Width = 320,
            Height = 92,
            TransitionMode = TransitionMode.SlideLeft,
            Content = CreateTransitionCard("Slide content", "TransitioningContentControl")
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Width = 390,
            Children =
            {
                transitionHost,
                CreateLayoutButtonRow(
                    CreateLayoutActionButton(FluentIconRegular.SlideTransition24, "Slide", () =>
                    {
                        transitionHost.TransitionMode = TransitionMode.SlideLeft;
                        transitionHost.Content = CreateTransitionCard("Slide content", "Transition mode");
                        output.Text = "Transition: SlideLeft.";
                    }),
                    CreateLayoutActionButton(FluentIconRegular.LayerDiagonalSparkle24, "Liquid", () =>
                    {
                        transitionHost.TransitionMode = TransitionMode.LiquidMorph;
                        transitionHost.Content = CreateTransitionCard("LiquidMorph content", "Material motion");
                        output.Text = "Transition: LiquidMorph.";
                    })),
                CreateLayoutStatus(output)
            }
        };
    }

    private static UIElement CreateMaterialLayoutSurfaceSample()
    {
        var output = CreateLayoutOutput("Surface: LiquidGlass. Grid spacing 8. Transition SlideLeft.");
        var grid = CreateSampleGrid();
        var transitionHost = new FWTransitioningContentControl
        {
            Width = 470,
            Height = 86,
            TransitionMode = TransitionMode.SlideLeft,
            Content = CreateTransitionCard("Layered content", "LiquidGlass layout host")
        };

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
                    new FWTextBlock
                    {
                        Text = "Content hosts and layout panels keep contrast while glass and refraction are active.",
                        Foreground = ThemeBrush("TextSecondary"),
                        TextWrapping = TextWrapping.Wrap
                    },
                    new FWWrapPanel
                    {
                        HorizontalSpacing = 8,
                        VerticalSpacing = 8,
                        Children =
                        {
                            CreateLayoutChip("Text"),
                            CreateLayoutChip("Border"),
                            CreateLayoutChip("Grid"),
                            CreateLayoutChip("Presenter")
                        }
                    },
                    grid,
                    transitionHost,
                    CreateLayoutButtonRow(
                        CreateLayoutActionButton(FluentIconRegular.Table24, "Spacing", () =>
                        {
                            grid.ColumnSpacing = grid.ColumnSpacing == 8 ? 14 : 8;
                            grid.RowSpacing = grid.RowSpacing == 8 ? 14 : 8;
                            output.Text = $"Surface grid spacing: {grid.ColumnSpacing}";
                        }),
                        CreateLayoutActionButton(FluentIconRegular.LayerDiagonalSparkle24, "Morph", () =>
                        {
                            transitionHost.TransitionMode = TransitionMode.LiquidMorph;
                            transitionHost.Content = CreateTransitionCard("LiquidMorph content", "Shared surface");
                            output.Text = "Surface transition: LiquidMorph.";
                        })),
                    CreateLayoutStatus(output)
                }
            }
        };
    }

    private static FWGrid CreateSampleGrid()
    {
        var grid = new FWGrid
        {
            Width = 320,
            ColumnSpacing = 8,
            RowSpacing = 8
        };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var cellA = CreateGridCell("Auto", "Row 0");
        var cellB = CreateGridCell("Star", "Column 1");
        var cellC = CreateGridCell("Span", "Two columns");
        Grid.SetColumn(cellB, 1);
        Grid.SetRow(cellC, 1);
        Grid.SetColumnSpan(cellC, 2);
        grid.Children.Add(cellA);
        grid.Children.Add(cellB);
        grid.Children.Add(cellC);
        return grid;
    }

    private static FWBorder CreateTransitionCard(string title, string detail)
    {
        return new FWBorder
        {
            Background = ThemeBrush("ControlBackground"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 4,
                Children =
                {
                    new FWTextBlock
                    {
                        Text = title,
                        Foreground = ThemeBrush("TextPrimary"),
                        FontSize = 14
                    },
                    new FWTextBlock
                    {
                        Text = detail,
                        Foreground = ThemeBrush("TextSecondary"),
                        FontSize = 12
                    }
                }
            }
        };
    }

    private static FWBorder CreateAdaptivePane(string title, string detail)
    {
        return new FWBorder
        {
            Background = ThemeBrush("LayerFillColorDefaultBrush"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 6,
                Children =
                {
                    CreateIcon(FluentIconRegular.DualScreenTablet24, 22, ThemeBrush("TextPrimary")),
                    new FWTextBlock
                    {
                        Text = title,
                        FontSize = 14,
                        Foreground = ThemeBrush("TextPrimary")
                    },
                    new FWTextBlock
                    {
                        Text = detail,
                        FontSize = 12,
                        Foreground = ThemeBrush("TextSecondary"),
                        TextWrapping = TextWrapping.Wrap
                    }
                }
            }
        };
    }

    private static FWBorder CreatePositionedBadge(string text, FluentIconRegular icon)
    {
        return new FWBorder
        {
            Width = 68,
            Height = 36,
            Background = ThemeBrush("ControlBackground"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(6),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 4,
                Children =
                {
                    CreateIcon(icon, 14, ThemeBrush("TextSecondary")),
                    new FWTextBlock
                    {
                        Text = text,
                        FontSize = 11,
                        Foreground = ThemeBrush("TextPrimary"),
                        VerticalAlignment = VerticalAlignment.Center
                    }
                }
            }
        };
    }

    private static FWBorder CreateRelativeTile(string text, double width, double height)
    {
        return new FWBorder
        {
            Width = width,
            Height = height,
            Background = ThemeBrush("ControlBackground"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(8),
            Child = new FWTextBlock
            {
                Text = text,
                FontSize = 12,
                Foreground = ThemeBrush("TextPrimary"),
                VerticalAlignment = VerticalAlignment.Center
            }
        };
    }

    private static FWBorder CreateLayoutFrame(string label, UIElement content)
    {
        return new FWBorder
        {
            Background = ThemeBrush("SurfaceBackground"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 8,
                Children =
                {
                    new FWTextBlock
                    {
                        Text = label,
                        FontSize = 12,
                        Foreground = ThemeBrush("TextSecondary")
                    },
                    content
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
                    Text = "Layered layout surface",
                    FontSize = 15,
                    Foreground = ThemeBrush("TextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
    }

    private static FWBorder CreateLayoutExampleCard(FluentIconRegular icon, string title, string description, UIElement content)
    {
        return GallerySampleCard.Create(icon, title, description, content, code: CreateSampleCode(title));
    }

    private static string CreateSampleCode(string title)
    {
        return title switch
        {
            "Text and access text" => "<FWTextBlock Text=\"Selectable body copy\" IsTextSelectionEnabled=\"True\" />\n<FWAccessText Text=\"_Open command\" />",
            "Border and content hosts" => "<FWBorder Padding=\"14\" CornerRadius=\"6\">\n  <FWContentControl Content=\"Hosted content\" />\n</FWBorder>",
            "Stack, wrap, and grid layout" => "<FWStackPanel Spacing=\"10\" />\n<FWWrapPanel HorizontalSpacing=\"8\" />\n<FWGrid ColumnSpacing=\"8\" RowSpacing=\"8\" />",
            "SplitView pane layout" => "<FWSplitView DisplayMode=\"CompactInline\"\n             PanePlacement=\"Left\"\n             IsPaneOpen=\"False\"\n             OpenPaneLength=\"208\"\n             CompactPaneLength=\"56\">\n  <FWSplitView.Pane>\n    <FWStackPanel Spacing=\"8\" />\n  </FWSplitView.Pane>\n  <FWBorder Padding=\"14\">\n    <FWTextBlock Text=\"Primary content\" />\n  </FWBorder>\n</FWSplitView>\n<!-- ActualPaneLength reflects compact, open, and overlay states. -->",
            "Adaptive settings layout" => "<FWTwoPaneView x:Name=\"AdaptiveView\" Mode=\"Wide\">\n  <FWSettingsCard Header=\"Display mode\"\n                  Description=\"Adaptive settings row\"\n                  IsClickEnabled=\"True\"\n                  Command=\"{Binding ConfigureCommand}\"\n                  CommandParameter=\"display-mode\" />\n</FWTwoPaneView>\n<!-- AdaptiveView.ActualMode / AdaptiveView.VisiblePane expose the resolved state. -->",
            "Canvas, relative, and parallax layout" => "var source = new FWScroller();\nsource.AttachScrollViewer(scrollViewer);\nvar parallax = new FWParallaxView\n{\n    Source = source,\n    HorizontalShift = 18,\n    VerticalShift = 28,\n    IsHorizontalShiftEnabled = true\n};\nparallax.RefreshProgressFromSource();\n// parallax.CurrentOffset exposes the resolved parallax offset.",
            "Transitioning content" => "<FWTransitioningContentControl TransitionMode=\"SlideLeft\" />",
            _ => "<FWFluentMaterialSurface MaterialKind=\"LiquidGlass\">\n  <FWGrid ColumnSpacing=\"8\" RowSpacing=\"8\" />\n</FWFluentMaterialSurface>"
        };
    }

    private static FWWrapPanel CreateLayoutButtonRow(params FWButton[] buttons)
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

    private static FWButton CreateLayoutActionButton(FluentIconRegular icon, string text, Action action)
    {
        var button = new FWButton
        {
            Content = CreateLayoutButtonContent(icon, text)
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static FWStackPanel CreateLayoutButtonContent(FluentIconRegular icon, string text)
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

    private static TextBlock CreateLayoutOutput(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        };
    }

    private static FWBorder CreateLayoutStatus(TextBlock status)
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

    private static FWBorder CreateLayoutChip(string text)
    {
        return new FWBorder
        {
            Background = ThemeBrush("SelectionBackgroundWeak"),
            BorderBrush = ThemeBrush("AccentBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(10, 4, 10, 4),
            Child = new FWTextBlock
            {
                Text = text,
                Foreground = ThemeBrush("TextPrimary"),
                FontSize = 12
            }
        };
    }

    private static FWStackPanel CreateSplitViewPaneContent()
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Children =
            {
                CreateSplitViewPaneItem(FluentIconRegular.LayoutColumnTwo24, "Overview"),
                CreateSplitViewPaneItem(FluentIconRegular.DataUsage24, "Reports"),
                CreateSplitViewPaneItem(FluentIconRegular.CloudSync24, "Sync")
            }
        };
    }

    private static FWBorder CreateSplitViewMainContent()
    {
        return new FWBorder
        {
            Padding = new Thickness(14),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 10,
                Children =
                {
                    new FWStackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 8,
                        Children =
                        {
                            CreateIcon(FluentIconRegular.Layer24, 18, ThemeBrush("TextPrimary")),
                            new FWTextBlock
                            {
                                Text = "Primary content",
                                Foreground = ThemeBrush("TextPrimary"),
                                FontSize = 15,
                                VerticalAlignment = VerticalAlignment.Center
                            }
                        }
                    },
                    new FWTextBlock
                    {
                        Text = "The pane can stay compact inline, open beside content, or overlay the same content region.",
                        Foreground = ThemeBrush("TextSecondary"),
                        TextWrapping = TextWrapping.Wrap
                    },
                    CreateSampleGrid()
                }
            }
        };
    }

    private static FWBorder CreateSplitViewPaneItem(FluentIconRegular icon, string text)
    {
        return new FWBorder
        {
            Padding = new Thickness(12, 8, 12, 8),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children =
                {
                    CreateIcon(icon, 18, ThemeBrush("TextPrimary")),
                    new FWTextBlock
                    {
                        Text = text,
                        Foreground = ThemeBrush("TextPrimary"),
                        VerticalAlignment = VerticalAlignment.Center
                    }
                }
            }
        };
    }

    private static FWBorder CreateGridCell(string title, string detail)
    {
        return new FWBorder
        {
            Background = ThemeBrush("ControlBackground"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 3,
                Children =
                {
                    new FWTextBlock
                    {
                        Text = title,
                        Foreground = ThemeBrush("TextPrimary"),
                        FontSize = 13
                    },
                    new FWTextBlock
                    {
                        Text = detail,
                        Foreground = ThemeBrush("TextSecondary"),
                        FontSize = 12
                    }
                }
            }
        };
    }

    private sealed class GalleryLayoutCommand : ICommand
    {
        private readonly Action<object?> _execute;

        public GalleryLayoutCommand(Action<object?> execute)
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
                        CreateIcon(FluentIconRegular.LayoutColumnTwo24, 24, ThemeBrush("TextPrimary")),
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

    private static Brush ThemeBrush(string key)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Brush brush)
        {
            return brush;
        }

        return new SolidColorBrush(Colors.Transparent);
    }
}
