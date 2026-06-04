using System.Diagnostics.CodeAnalysis;
using FluentJalium.Gallery.Controls;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWButton = FluentJalium.Controls.FWButton;
using FWFluentMaterialKind = FluentJalium.Controls.FWFluentMaterialKind;
using FWFluentMaterialSurface = FluentJalium.Controls.FWFluentMaterialSurface;
using FWFrame = FluentJalium.Controls.FWFrame;
using FWNavigationView = FluentJalium.Controls.FWNavigationView;
using FWNavigationViewItem = FluentJalium.Controls.FWNavigationViewItem;
using FWNavigationViewItemHeader = FluentJalium.Controls.FWNavigationViewItemHeader;
using FWNavigationViewItemSeparator = FluentJalium.Controls.FWNavigationViewItemSeparator;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWTabControl = FluentJalium.Controls.FWTabControl;
using FWTabItem = FluentJalium.Controls.FWTabItem;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWTextBox = FluentJalium.Controls.FWTextBox;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;

namespace FluentJalium.Gallery.Pages;

internal sealed class GalleryNavigationPage
{
    public UIElement CreateContent()
    {
        var panel = CreateSection("Navigation");
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        examples.Children.Add(CreateNavigationExampleCard(
            FluentIconRegular.Navigation24,
            "FWNavigationView",
            "Left navigation with pane header, footer menu items, nested items, and live selection output.",
            CreateNavigationViewSample()));
        examples.Children.Add(CreateNavigationExampleCard(
            FluentIconRegular.PanelLeft24,
            "Pane modes and hierarchy",
            "Switch Left, LeftCompact, LeftMinimal, and Top display modes while preserving hierarchy state.",
            CreateNavigationPaneModeSample()));
        examples.Children.Add(CreateNavigationExampleCard(
            FluentIconRegular.TabDesktop24,
            "FWTabControl",
            "Top, bottom, left, disabled, and swipe-enabled tab states with selected content output.",
            CreateTabControlNavigationSample()));
        examples.Children.Add(CreateNavigationExampleCard(
            FluentIconRegular.Window24,
            "FWFrame",
            "Frame navigation, back stack, forward stack, and content host surface.",
            CreateFrameNavigationSample()));
        examples.Children.Add(CreateNavigationExampleCard(
            FluentIconRegular.LayerDiagonalSparkle24,
            "Material navigation shell",
            "NavigationView, tabs, and command states remain readable inside a LiquidGlass app shell.",
            CreateMaterialNavigationShellSample()));

        panel.Children.Add(examples);
        return panel;
    }

    private static UIElement CreateNavigationViewSample()
    {
        var output = CreateNavigationOutput("Selected: Dashboard");
        var dashboardItem = new FWNavigationViewItem
        {
            Content = "Dashboard",
            Icon = CreateIcon(FluentIconRegular.Home24)
        };
        var controlsItem = new FWNavigationViewItem
        {
            Content = "Controls",
            Icon = CreateIcon(FluentIconRegular.ControlButton24),
            IsExpanded = true
        };
        controlsItem.MenuItems.Add(new FWNavigationViewItem { Content = "Buttons", Icon = CreateIcon(FluentIconRegular.ControlButton24) });
        controlsItem.MenuItems.Add(new FWNavigationViewItem { Content = "Collections", Icon = CreateIcon(FluentIconRegular.Table24) });
        var galleryItem = new FWNavigationViewItem { Content = "Gallery", Icon = CreateIcon(FluentIconRegular.Folder24) };
        var settingsItem = new FWNavigationViewItem { Content = "Settings", Icon = CreateIcon(FluentIconRegular.Settings24) };

        var navigationView = new FWNavigationView
        {
            Width = 500,
            Height = 260,
            PaneTitle = "FluentJalium",
            Header = "NavigationView",
            OpenPaneLength = 220,
            CompactPaneLength = 48,
            PaneHeader = CreateNavigationPaneHeader("Controls"),
            PaneFooter = CreateNavigationPaneFooter("vNext"),
            SelectedItem = dashboardItem,
            Content = CreateNavigationContent("Dashboard", "Selected, nested, footer, and separator states share FluentJalium tokens.")
        };
        navigationView.MenuItems.Add(new FWNavigationViewItemHeader { Content = "Workspace" });
        navigationView.MenuItems.Add(dashboardItem);
        navigationView.MenuItems.Add(controlsItem);
        navigationView.MenuItems.Add(new FWNavigationViewItemSeparator());
        navigationView.MenuItems.Add(galleryItem);
        navigationView.FooterMenuItems.Add(settingsItem);
        navigationView.SelectionChanged += (_, e) =>
        {
            output.Text = $"Selected: {e.SelectedItem?.Content}";
            navigationView.Content = CreateNavigationContent(
                e.SelectedItem?.Content?.ToString() ?? "NavigationView",
                $"Previous: {e.PreviousSelectedItem?.Content ?? "none"}");
        };
        navigationView.UpdateMenuItems();

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                navigationView,
                CreateNavigationButtonRow(
                    CreateNavigationActionButton(FluentIconRegular.Home24, "Dashboard", () => navigationView.SelectedItem = dashboardItem),
                    CreateNavigationActionButton(FluentIconRegular.Table24, "Collections", () => navigationView.SelectedItem = controlsItem.MenuItems[1]),
                    CreateNavigationActionButton(FluentIconRegular.PanelLeftContract24, "Pane", () =>
                    {
                        navigationView.IsPaneOpen = !navigationView.IsPaneOpen;
                        output.Text = $"Pane open: {navigationView.IsPaneOpen}";
                    }),
                    CreateNavigationActionButton(FluentIconRegular.Settings24, "Footer", () => navigationView.SelectedItem = settingsItem)),
                CreateNavigationStatus(output)
            }
        };
    }

    private static UIElement CreateNavigationPaneModeSample()
    {
        var output = CreateNavigationOutput("PaneDisplayMode: Left. Document options expanded: true");
        var homeItem = new FWNavigationViewItem { Content = "Home", Icon = CreateIcon(FluentIconRegular.Home24) };
        var accountItem = new FWNavigationViewItem
        {
            Content = "Account",
            Icon = CreateIcon(FluentIconRegular.Person24)
        };
        accountItem.MenuItems.Add(new FWNavigationViewItem { Content = "Mail", Icon = CreateIcon(FluentIconRegular.Mail24) });
        accountItem.MenuItems.Add(new FWNavigationViewItem { Content = "Calendar", Icon = CreateIcon(FluentIconRegular.CalendarLtr24) });
        var documentOptionsItem = new FWNavigationViewItem
        {
            Content = "Document options",
            Icon = CreateIcon(FluentIconRegular.Document24),
            IsExpanded = true,
            SelectsOnInvoked = false
        };
        documentOptionsItem.MenuItems.Add(new FWNavigationViewItem { Content = "Create new", Icon = CreateIcon(FluentIconRegular.Add24) });
        documentOptionsItem.MenuItems.Add(new FWNavigationViewItem { Content = "Upload file", Icon = CreateIcon(FluentIconRegular.ArrowUpload24) });

        var navigationView = new FWNavigationView
        {
            Width = 500,
            Height = 250,
            Header = "Pane modes",
            PaneTitle = "Workspace",
            PaneDisplayMode = NavigationViewPaneDisplayMode.Left,
            OpenPaneLength = 220,
            CompactPaneLength = 48,
            IsBackButtonVisible = NavigationViewBackButtonVisible.Visible,
            IsBackEnabled = true,
            SelectedItem = homeItem,
            Content = CreateNavigationContent("Home", "Switch the pane display mode from the options below.")
        };
        navigationView.MenuItems.Add(homeItem);
        navigationView.MenuItems.Add(accountItem);
        navigationView.MenuItems.Add(documentOptionsItem);
        navigationView.SelectionChanged += (_, e) =>
        {
            output.Text = $"PaneDisplayMode: {navigationView.PaneDisplayMode}. Selected: {e.SelectedItem?.Content}";
        };
        navigationView.ItemInvoked += (_, e) =>
        {
            if (!e.InvokedItem.SelectsOnInvoked)
            {
                output.Text = $"Invoked non-selecting item: {e.InvokedItem.Content}. Expanded: {e.InvokedItem.IsExpanded}";
            }
        };
        navigationView.UpdateMenuItems();

        void SetMode(NavigationViewPaneDisplayMode mode)
        {
            navigationView.PaneDisplayMode = mode;
            navigationView.IsPaneOpen = mode != NavigationViewPaneDisplayMode.LeftCompact;
            output.Text = $"PaneDisplayMode: {mode}. Document options expanded: {documentOptionsItem.IsExpanded}";
        }

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                navigationView,
                CreateNavigationButtonRow(
                    CreateNavigationActionButton(FluentIconRegular.PanelLeft24, "Left", () => SetMode(NavigationViewPaneDisplayMode.Left)),
                    CreateNavigationActionButton(FluentIconRegular.PanelLeftContract24, "Compact", () => SetMode(NavigationViewPaneDisplayMode.LeftCompact)),
                    CreateNavigationActionButton(FluentIconRegular.PanelLeftExpand24, "Minimal", () => SetMode(NavigationViewPaneDisplayMode.LeftMinimal)),
                    CreateNavigationActionButton(FluentIconRegular.PanelTopGallery24, "Top", () => SetMode(NavigationViewPaneDisplayMode.Top)),
                    CreateNavigationActionButton(FluentIconRegular.BranchFork24, "Tree", () =>
                    {
                        documentOptionsItem.IsExpanded = !documentOptionsItem.IsExpanded;
                        output.Text = $"PaneDisplayMode: {navigationView.PaneDisplayMode}. Document options expanded: {documentOptionsItem.IsExpanded}";
                    })),
                CreateNavigationStatus(output)
            }
        };
    }

    private static UIElement CreateTabControlNavigationSample()
    {
        var output = CreateNavigationOutput("Selected tab: Overview");
        var tabControl = new FWTabControl
        {
            Width = 500,
            Height = 160,
            IsSwipeEnabled = true
        };
        tabControl.Items.Add(new FWTabItem
        {
            Header = "Overview",
            Content = CreateTabContent("Navigation items share the Fluent selection pill and hover states.")
        });
        tabControl.Items.Add(new FWTabItem
        {
            Header = "Details",
            Content = CreateTabContent("Tabs use the shared accent indicator and theme-aware strip colors.")
        });
        tabControl.Items.Add(new FWTabItem
        {
            Header = "Disabled",
            Content = CreateTabContent("Disabled tab sample"),
            IsEnabled = false
        });
        tabControl.SelectedIndex = 0;
        tabControl.SelectionChanged += (_, _) =>
        {
            output.Text = $"Selected tab: {(tabControl.SelectedItem as FWTabItem)?.Header}";
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                tabControl,
                CreateNavigationButtonRow(
                    CreateNavigationActionButton(FluentIconRegular.Home24, "Overview", () => tabControl.SelectedIndex = 0),
                    CreateNavigationActionButton(FluentIconRegular.Document24, "Details", () => tabControl.SelectedIndex = 1),
                    CreateNavigationActionButton(FluentIconRegular.TabDesktop24, "Top", () =>
                    {
                        tabControl.TabStripPlacement = Dock.Top;
                        output.Text = "TabStripPlacement: Top";
                    }),
                    CreateNavigationActionButton(FluentIconRegular.TabDesktopBottom24, "Bottom", () =>
                    {
                        tabControl.TabStripPlacement = Dock.Bottom;
                        output.Text = "TabStripPlacement: Bottom";
                    }),
                    CreateNavigationActionButton(FluentIconRegular.PanelLeft24, "Left", () =>
                    {
                        tabControl.TabStripPlacement = Dock.Left;
                        output.Text = "TabStripPlacement: Left";
                    })),
                CreateNavigationStatus(output)
            }
        };
    }

    [RequiresUnreferencedCode("Gallery sample navigates to local Page types by typeof literals.")]
    private static UIElement CreateFrameNavigationSample()
    {
        var output = CreateNavigationOutput("Frame: not navigated");
        var frame = new FWFrame
        {
            Width = 500,
            Height = 160,
            Padding = new Thickness(14),
            BorderThickness = new Thickness(1)
        };
        frame.Navigated += (_, _) =>
        {
            output.Text = $"Frame: {frame.SourcePageType?.Name}, BackStack: {frame.BackStackDepth}, CanGoForward: {frame.CanGoForward}";
        };

        frame.Navigate(typeof(GalleryNavigationOverviewPage), "Overview");

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                frame,
                CreateNavigationButtonRow(
                    CreateNavigationActionButton(FluentIconRegular.Home24, "Overview", () => frame.Navigate(typeof(GalleryNavigationOverviewPage), "Overview")),
                    CreateNavigationActionButton(FluentIconRegular.Document24, "Details", () => frame.Navigate(typeof(GalleryNavigationDetailsPage), "Details")),
                    CreateNavigationActionButton(FluentIconRegular.ArrowLeft24, "Back", () =>
                    {
                        if (!frame.GoBack())
                        {
                            output.Text = "Frame: no back entry";
                        }
                    }),
                    CreateNavigationActionButton(FluentIconRegular.ArrowRight24, "Forward", () =>
                    {
                        if (!frame.GoForward())
                        {
                            output.Text = "Frame: no forward entry";
                        }
                    })),
                CreateNavigationStatus(output)
            }
        };
    }

    private static UIElement CreateMaterialNavigationShellSample()
    {
        var output = CreateNavigationOutput("Shell: LiquidGlass. Pane compact. Tab: Overview.");
        var overviewItem = new FWNavigationViewItem { Content = "Overview", Icon = CreateIcon(FluentIconRegular.Home24) };
        var controlsItem = new FWNavigationViewItem { Content = "Controls", Icon = CreateIcon(FluentIconRegular.ControlButton24) };
        var materialsItem = new FWNavigationViewItem { Content = "Materials", Icon = CreateIcon(FluentIconRegular.LayerDiagonalSparkle24) };
        var settingsItem = new FWNavigationViewItem { Content = "Settings", Icon = CreateIcon(FluentIconRegular.Settings24) };
        var tabControl = new FWTabControl
        {
            Height = 122,
            IsSwipeEnabled = true
        };
        tabControl.Items.Add(new FWTabItem
        {
            Header = "Overview",
            Content = CreateTabContent("Compact navigation keeps command surfaces close to the app content.")
        });
        tabControl.Items.Add(new FWTabItem
        {
            Header = "Details",
            Content = CreateTabContent("LiquidGlass uses Jalium element effects while preserving Fluent selection contrast.")
        });
        tabControl.SelectedIndex = 0;

        var navigationView = new FWNavigationView
        {
            Width = 500,
            Height = 260,
            PaneTitle = "Shell",
            Header = "Material navigation shell",
            PaneDisplayMode = NavigationViewPaneDisplayMode.LeftCompact,
            IsPaneOpen = false,
            OpenPaneLength = 210,
            CompactPaneLength = 48,
            SelectedItem = overviewItem,
            Content = tabControl
        };
        navigationView.MenuItems.Add(overviewItem);
        navigationView.MenuItems.Add(controlsItem);
        navigationView.MenuItems.Add(materialsItem);
        navigationView.FooterMenuItems.Add(settingsItem);
        navigationView.SelectionChanged += (_, e) =>
        {
            output.Text = $"Shell: LiquidGlass. Selected: {e.SelectedItem?.Content}. Tab: {(tabControl.SelectedItem as FWTabItem)?.Header}.";
        };
        tabControl.SelectionChanged += (_, _) =>
        {
            output.Text = $"Shell: LiquidGlass. Selected: {(navigationView.SelectedItem as FWNavigationViewItem)?.Content}. Tab: {(tabControl.SelectedItem as FWTabItem)?.Header}.";
        };
        navigationView.UpdateMenuItems();

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
                    navigationView,
                    CreateNavigationButtonRow(
                        CreateNavigationActionButton(FluentIconRegular.PanelLeftExpand24, "Open pane", () =>
                        {
                            navigationView.IsPaneOpen = !navigationView.IsPaneOpen;
                            output.Text = $"Shell: LiquidGlass. Pane open: {navigationView.IsPaneOpen}.";
                        }),
                        CreateNavigationActionButton(FluentIconRegular.LayerDiagonalSparkle24, "Materials", () => navigationView.SelectedItem = materialsItem),
                        CreateNavigationActionButton(FluentIconRegular.TabDesktop24, "Next tab", () =>
                        {
                            tabControl.SelectedIndex = tabControl.SelectedIndex == 0 ? 1 : 0;
                            output.Text = $"Shell: LiquidGlass. Tab: {(tabControl.SelectedItem as FWTabItem)?.Header}.";
                        })),
                    CreateNavigationStatus(output)
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
                CreateIcon(FluentIconRegular.WindowBrush24, 18, ThemeBrush("TextPrimary")),
                new FWTextBlock
                {
                    Text = "Layered app shell",
                    FontSize = 15,
                    Foreground = ThemeBrush("TextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
    }

    private static UIElement CreateNavigationContent(string title, string description)
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Margin = new Thickness(18),
            Children =
            {
                new FWTextBlock
                {
                    Text = title,
                    FontSize = 18,
                    Foreground = ThemeBrush("TextPrimary")
                },
                new FWTextBlock
                {
                    Text = description,
                    Foreground = ThemeBrush("TextSecondary"),
                    TextWrapping = TextWrapping.Wrap
                }
            }
        };
    }

    private static UIElement CreateNavigationPaneHeader(string text)
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 6,
            Children =
            {
                new FWTextBlock { Text = "Search", FontSize = 12, Foreground = ThemeBrush("TextSecondary") },
                new FWTextBox { Text = text, MinHeight = 32, PlaceholderText = "Search navigation" }
            }
        };
    }

    private static UIElement CreateNavigationPaneFooter(string text)
    {
        return new FWTextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary")
        };
    }

    private static UIElement CreateTabContent(string text)
    {
        return new FWTextBlock
        {
            Text = text,
            Margin = new Thickness(12),
            Foreground = ThemeBrush("TextPrimary"),
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center
        };
    }

    private static FWBorder CreateNavigationExampleCard(FluentIconRegular icon, string title, string description, UIElement content)
    {
        return GallerySampleCard.Create(icon, title, description, content, code: CreateSampleCode(title));
    }

    private static string CreateSampleCode(string title)
    {
        return title switch
        {
            "FWNavigationView" => "<FWNavigationView PaneTitle=\"FluentJalium\" Header=\"NavigationView\">\n    <FWNavigationViewItem Content=\"Dashboard\" />\n    <FWNavigationViewItem Content=\"Controls\" />\n</FWNavigationView>",
            "Pane modes and hierarchy" => "<FWNavigationView PaneDisplayMode=\"LeftCompact\" IsPaneOpen=\"False\">\n    <FWNavigationViewItem Content=\"Document options\" IsExpanded=\"True\" SelectsOnInvoked=\"False\" />\n</FWNavigationView>",
            "FWTabControl" => "<FWTabControl TabStripPlacement=\"Top\" IsSwipeEnabled=\"True\">\n    <FWTabItem Header=\"Overview\" />\n    <FWTabItem Header=\"Details\" />\n</FWTabControl>",
            "FWFrame" => "var frame = new FWFrame();\nframe.Navigate(typeof(GalleryNavigationOverviewPage), \"Overview\");\nframe.GoBack();",
            "Material navigation shell" => "<FWFluentMaterialSurface MaterialKind=\"LiquidGlass\">\n    <FWNavigationView PaneDisplayMode=\"LeftCompact\" />\n    <FWTabControl />\n</FWFluentMaterialSurface>",
            _ => "<FWNavigationView />"
        };
    }

    private static FWWrapPanel CreateNavigationButtonRow(params FWButton[] buttons)
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

    private static FWButton CreateNavigationActionButton(FluentIconRegular icon, string text, Action action)
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

    private static TextBlock CreateNavigationOutput(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        };
    }

    private static FWBorder CreateNavigationStatus(TextBlock status)
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
                        CreateIcon(FluentIconRegular.Navigation24, 24, ThemeBrush("TextPrimary")),
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

    private static FluentIcon CreateIcon(FluentIconRegular icon, double size = 20, Brush? foreground = null)
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
