using FluentJalium.Gallery.Controls;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Media;
using FWAppBarButton = FluentJalium.Controls.FWAppBarButton;
using FWAppBarToggleButton = FluentJalium.Controls.FWAppBarToggleButton;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWButton = FluentJalium.Controls.FWButton;
using FWCommandBarFlyout = FluentJalium.Controls.FWCommandBarFlyout;
using FWContextMenu = FluentJalium.Controls.FWContextMenu;
using FWDropDownButton = FluentJalium.Controls.FWDropDownButton;
using FWFluentMaterialKind = FluentJalium.Controls.FWFluentMaterialKind;
using FWFluentMaterialSurface = FluentJalium.Controls.FWFluentMaterialSurface;
using FWMenu = FluentJalium.Controls.FWMenu;
using FWMenuBar = FluentJalium.Controls.FWMenuBar;
using FWMenuBarItem = FluentJalium.Controls.FWMenuBarItem;
using FWMenuFlyout = FluentJalium.Controls.FWMenuFlyout;
using FWMenuFlyoutItem = FluentJalium.Controls.FWMenuFlyoutItem;
using FWMenuFlyoutSeparator = FluentJalium.Controls.FWMenuFlyoutSeparator;
using FWMenuFlyoutSubItem = FluentJalium.Controls.FWMenuFlyoutSubItem;
using FWMenuItem = FluentJalium.Controls.FWMenuItem;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWToggleMenuFlyoutItem = FluentJalium.Controls.FWToggleMenuFlyoutItem;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;

namespace FluentJalium.Gallery.Pages;

internal sealed class GalleryMenusPage
{
    public UIElement CreateContent()
    {
        var panel = CreateSection("Menus and Flyouts");
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        examples.Children.Add(CreateMenuExampleCard(
            FluentIconRegular.List24,
            "FWMenuBar",
            "Top-level app commands with menu flyout items, separators, icons, disabled states, and shortcuts.",
            CreateMenuBarMenuSample()));
        examples.Children.Add(CreateMenuExampleCard(
            FluentIconRegular.Folder24,
            "FWMenu and FWMenuItem",
            "Classic WPF-compatible menu surface with nested headers, checkable items, icon columns, shortcuts, and submenu output.",
            CreateTraditionalMenuSample()));
        examples.Children.Add(CreateMenuExampleCard(
            FluentIconRegular.CursorClick24,
            "FWContextMenu",
            "Context menu placement, open and close events, command shortcuts, disabled items, and checkable state.",
            CreateContextMenuSample()));
        examples.Children.Add(CreateMenuExampleCard(
            FluentIconRegular.ChevronDown24,
            "FWMenuFlyout",
            "Drop-down command flyout using FWMenuFlyoutItem, FWToggleMenuFlyoutItem, and FWMenuFlyoutSeparator.",
            CreateMenuFlyoutItemSample()));
        examples.Children.Add(CreateMenuExampleCard(
            FluentIconRegular.ArrowDownload24,
            "FWMenuFlyoutSubItem",
            "Nested flyout command menu with submenu placement, icons, shortcuts, disabled state, and open or hide actions.",
            CreateMenuFlyoutSubItemSample()));
        examples.Children.Add(CreateMenuExampleCard(
            FluentIconRegular.MoreHorizontal24,
            "FWCommandBarFlyout",
            "Compact command surface with primary app bar actions, secondary commands, and a toggle command state.",
            CreateCommandBarFlyoutSample()));
        examples.Children.Add(CreateMenuExampleCard(
            FluentIconRegular.LayerDiagonalSparkle24,
            "Material menu workbench",
            "MenuBar, flyouts, context menus, and command bar flyouts remain readable on a LiquidGlass surface.",
            CreateMaterialMenuWorkbenchSample()));

        panel.Children.Add(examples);
        return panel;
    }

    private static UIElement CreateMenuBarMenuSample()
    {
        var output = CreateMenuOutput("MenuBar: File ready");
        var menuBar = new FWMenuBar
        {
            Width = 480
        };

        var file = CreateMenuBarItem(
            "File",
            ("New", "Ctrl+N", IconGlyph(FluentIconRegular.Document24)),
            ("Open", "Ctrl+O", IconGlyph(FluentIconRegular.FolderOpen24)),
            ("Save", "Ctrl+S", IconGlyph(FluentIconRegular.Save24)));
        var edit = CreateMenuBarItem(
            "Edit",
            ("Undo", "Ctrl+Z", null),
            ("Redo", "Ctrl+Y", null),
            ("Preferences", string.Empty, IconGlyph(FluentIconRegular.Settings24)));
        var view = CreateMenuBarItem(
            "View",
            ("Zoom in", "Ctrl++", null),
            ("Zoom out", "Ctrl+-", null),
            ("Actual size", "Ctrl+0", null));

        menuBar.Items.Add(file);
        menuBar.Items.Add(edit);
        menuBar.Items.Add(view);

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                menuBar,
                CreateMenuButtonRow(
                    CreateMenuActionButton(FluentIconRegular.Document24, "File", () =>
                    {
                        CloseMenuBarItems(edit, view);
                        file.OpenMenu();
                        output.Text = $"MenuBar: {file.Title} open";
                    }),
                    CreateMenuActionButton(FluentIconRegular.Edit24, "Edit", () =>
                    {
                        CloseMenuBarItems(file, view);
                        edit.OpenMenu();
                        output.Text = $"MenuBar: {edit.Title} open";
                    }),
                    CreateMenuActionButton(FluentIconRegular.WindowBrush24, "View", () =>
                    {
                        CloseMenuBarItems(file, edit);
                        view.OpenMenu();
                        output.Text = $"MenuBar: {view.Title} open";
                    }),
                    CreateMenuActionButton(FluentIconRegular.DismissCircle24, "Close", () =>
                    {
                        CloseMenuBarItems(file, edit, view);
                        output.Text = "MenuBar: all menus closed";
                    })),
                CreateMenuStatus(output)
            }
        };
    }

    private static UIElement CreateTraditionalMenuSample()
    {
        var output = CreateMenuOutput("Menu: Project ready");
        var menu = new FWMenu
        {
            Width = 480
        };
        var project = new FWMenuItem
        {
            Header = "Project",
            Icon = IconGlyph(FluentIconRegular.Folder24)
        };
        var build = new FWMenuItem
        {
            Header = "Build",
            InputGestureText = "Ctrl+B",
            Icon = IconGlyph(FluentIconRegular.Play24)
        };
        var run = new FWMenuItem
        {
            Header = "Run",
            InputGestureText = "F5",
            Icon = IconGlyph(FluentIconRegular.Play24)
        };
        var livePreview = new FWMenuItem
        {
            Header = "Live preview",
            IsCheckable = true,
            IsChecked = true,
            StaysOpenOnClick = true
        };
        project.Items.Add(build);
        project.Items.Add(run);
        project.Items.Add(livePreview);

        var tools = new FWMenuItem
        {
            Header = "Tools",
            Icon = IconGlyph(FluentIconRegular.Settings24)
        };
        tools.Items.Add(new FWMenuItem { Header = "Options" });
        tools.Items.Add(new FWMenuItem { Header = "Diagnostics", InputGestureText = "F12" });

        menu.Items.Add(project);
        menu.Items.Add(tools);
        menu.Items.Add("Generated");
        menu.Items.Add(new FWMenuItem { Header = "Disabled", IsEnabled = false });

        build.Click += (_, _) => output.Text = "Menu: Build clicked";
        run.Click += (_, _) => output.Text = "Menu: Run clicked";
        project.SubmenuOpened += (_, _) => output.Text = "Menu: Project submenu opened";
        project.SubmenuClosed += (_, _) => output.Text = "Menu: Project submenu closed";
        livePreview.Checked += (_, _) => output.Text = "Menu: Live preview checked";
        livePreview.Unchecked += (_, _) => output.Text = "Menu: Live preview unchecked";

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                menu,
                CreateMenuButtonRow(
                    CreateMenuActionButton(FluentIconRegular.FolderOpen24, "Open project", () =>
                    {
                        project.IsSubmenuOpen = true;
                        output.Text = "Menu: Project submenu opened";
                    }),
                    CreateMenuActionButton(FluentIconRegular.DismissCircle24, "Close", () =>
                    {
                        project.IsSubmenuOpen = false;
                        output.Text = "Menu: Project submenu closed";
                    }),
                    CreateMenuActionButton(FluentIconRegular.Eye24, "Preview", () =>
                    {
                        livePreview.IsChecked = !livePreview.IsChecked;
                        output.Text = $"Menu: Live preview {FormatOnOff(livePreview.IsChecked)}";
                    }),
                    CreateMenuActionButton(FluentIconRegular.Prohibited24, "Disable run", () =>
                    {
                        run.IsEnabled = !run.IsEnabled;
                        output.Text = $"Menu: Run enabled {run.IsEnabled}";
                    })),
                CreateMenuStatus(output)
            }
        };
    }

    private static UIElement CreateContextMenuSample()
    {
        var output = CreateMenuOutput("ContextMenu: closed");
        var contextMenu = new FWContextMenu
        {
            Placement = PlacementMode.Bottom,
            StaysOpen = true
        };
        var detailsItem = new FWMenuItem
        {
            Header = "Show details",
            IsCheckable = true,
            IsChecked = true,
            StaysOpenOnClick = true
        };
        contextMenu.Items.Add(new FWMenuItem { Header = "Refresh", InputGestureText = "F5", Icon = IconGlyph(FluentIconRegular.ArrowClockwise24) });
        contextMenu.Items.Add(new FWMenuItem { Header = "Rename", InputGestureText = "F2", Icon = IconGlyph(FluentIconRegular.Rename24) });
        contextMenu.Items.Add(detailsItem);
        contextMenu.Items.Add(new FWMenuItem { Header = "Disabled", IsEnabled = false });
        contextMenu.Opened += (_, _) => output.Text = $"ContextMenu: open at {contextMenu.Placement}";
        contextMenu.Closed += (_, _) => output.Text = "ContextMenu: closed";
        detailsItem.Checked += (_, _) => output.Text = "ContextMenu: details checked";
        detailsItem.Unchecked += (_, _) => output.Text = "ContextMenu: details unchecked";

        var target = CreateContextTarget("Document item", "Open the attached context menu from the options below.");
        target.ContextMenu = contextMenu;
        contextMenu.PlacementTarget = target;

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                target,
                CreateMenuButtonRow(
                    CreateMenuActionButton(FluentIconRegular.Open24, "Open", () => ContextMenuService.Open(target, contextMenu)),
                    CreateMenuActionButton(FluentIconRegular.DismissCircle24, "Close", () => contextMenu.Close()),
                    CreateMenuActionButton(FluentIconRegular.Info24, "Details", () =>
                    {
                        detailsItem.IsChecked = !detailsItem.IsChecked;
                        output.Text = $"ContextMenu: details {FormatOnOff(detailsItem.IsChecked)}";
                    }),
                    CreateMenuActionButton(FluentIconRegular.CursorClick24, "Mouse point", () =>
                    {
                        contextMenu.Placement = PlacementMode.MousePoint;
                        output.Text = $"ContextMenu placement: {contextMenu.Placement}";
                    }),
                    CreateMenuActionButton(FluentIconRegular.ArrowDownload24, "Bottom", () =>
                    {
                        contextMenu.Placement = PlacementMode.Bottom;
                        output.Text = $"ContextMenu placement: {contextMenu.Placement}";
                    })),
                CreateMenuStatus(output)
            }
        };
    }

    private static UIElement CreateMenuFlyoutItemSample()
    {
        var output = CreateMenuOutput("MenuFlyout: ready");
        var flyout = CreateMenuControlsFlyout(output);
        var button = new FWDropDownButton
        {
            Content = CreateMenuButtonContent(FluentIconRegular.ChevronDown24, "Actions"),
            Width = 160,
            Flyout = flyout
        };
        var toggle = flyout.Items.OfType<FWToggleMenuFlyoutItem>().First();
        var disabled = flyout.Items.OfType<FWMenuFlyoutItem>().Last(item => item.Text == "Disabled");

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                button,
                CreateMenuButtonRow(
                    CreateMenuActionButton(FluentIconRegular.Open24, "Open", () =>
                    {
                        flyout.ShowAt(button);
                        output.Text = "MenuFlyout: open";
                    }),
                    CreateMenuActionButton(FluentIconRegular.DismissCircle24, "Hide", () =>
                    {
                        flyout.Hide();
                        output.Text = "MenuFlyout: hidden";
                    }),
                    CreateMenuActionButton(FluentIconRegular.Badge24, "Badges", () =>
                    {
                        toggle.IsChecked = !toggle.IsChecked;
                        output.Text = $"MenuFlyout: badges {FormatOnOff(toggle.IsChecked)}";
                    }),
                    CreateMenuActionButton(FluentIconRegular.Prohibited24, "Disable", () =>
                    {
                        disabled.IsEnabled = !disabled.IsEnabled;
                        output.Text = $"MenuFlyout: disabled item enabled {disabled.IsEnabled}";
                    })),
                CreateMenuStatus(output)
            }
        };
    }

    private static UIElement CreateMenuFlyoutSubItemSample()
    {
        var output = CreateMenuOutput("SubMenuFlyout: ready");
        var flyout = new FWMenuFlyout
        {
            Placement = FlyoutPlacementMode.Bottom
        };
        var export = new FWMenuFlyoutSubItem
        {
            Text = "Export",
            Icon = IconGlyph(FluentIconRegular.ArrowDownload24),
            KeyboardAcceleratorTextOverride = "Ctrl+E"
        };
        var pdf = new FWMenuFlyoutItem
        {
            Text = "PDF document",
            Icon = IconGlyph(FluentIconRegular.DocumentPdf24)
        };
        var package = new FWMenuFlyoutItem
        {
            Text = "Project package",
            Icon = IconGlyph(FluentIconRegular.Box24)
        };
        var disabled = new FWMenuFlyoutItem
        {
            Text = "Cloud archive",
            IsEnabled = false
        };
        var recent = new FWMenuFlyoutSubItem
        {
            Text = "Recent formats",
            Icon = IconGlyph(FluentIconRegular.History24)
        };
        recent.Items.Add(new FWMenuFlyoutItem { Text = "Markdown", Icon = IconGlyph(FluentIconRegular.TextBold24) });
        recent.Items.Add(new FWMenuFlyoutItem { Text = "HTML", Icon = IconGlyph(FluentIconRegular.Code24) });
        export.Items.Add(pdf);
        export.Items.Add(package);
        export.Items.Add(disabled);
        export.Items.Add(recent);

        var publish = new FWMenuFlyoutItem
        {
            Text = "Publish",
            Icon = IconGlyph(FluentIconRegular.Send24)
        };
        flyout.Items.Add(export);
        flyout.Items.Add(new FWMenuFlyoutSeparator());
        flyout.Items.Add(publish);

        pdf.Click += (_, _) => output.Text = "SubMenuFlyout: PDF selected";
        package.Click += (_, _) => output.Text = "SubMenuFlyout: package selected";
        publish.Click += (_, _) => output.Text = "SubMenuFlyout: publish clicked";

        var button = new FWDropDownButton
        {
            Content = CreateMenuButtonContent(FluentIconRegular.ArrowDownload24, "Export options"),
            Width = 190,
            Flyout = flyout
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                button,
                CreateMenuButtonRow(
                    CreateMenuActionButton(FluentIconRegular.Open24, "Open", () =>
                    {
                        flyout.ShowAt(button);
                        output.Text = "SubMenuFlyout: open";
                    }),
                    CreateMenuActionButton(FluentIconRegular.ArrowDownload24, "Submenu", () =>
                    {
                        flyout.ShowAt(button);
                        export.ShowSubMenu();
                        output.Text = "SubMenuFlyout: export submenu open";
                    }),
                    CreateMenuActionButton(FluentIconRegular.History24, "Recent", () =>
                    {
                        flyout.ShowAt(button);
                        export.ShowSubMenu();
                        recent.ShowSubMenu();
                        output.Text = "SubMenuFlyout: recent submenu open";
                    }),
                    CreateMenuActionButton(FluentIconRegular.DismissCircle24, "Hide", () =>
                    {
                        recent.HideSubMenu();
                        export.HideSubMenu();
                        flyout.Hide();
                        output.Text = "SubMenuFlyout: hidden";
                    })),
                CreateMenuStatus(output)
            }
        };
    }

    private static UIElement CreateCommandBarFlyoutSample()
    {
        var output = CreateMenuOutput("CommandBarFlyout: ready");
        var flyout = new FWCommandBarFlyout
        {
            AlwaysExpanded = true
        };
        var button = new FWButton
        {
            Content = CreateMenuButtonContent(FluentIconRegular.MoreHorizontal24, "More commands"),
            MinWidth = 170
        };

        flyout.PrimaryCommands.Add(CreateAppBarButton("Copy", FluentIconRegular.Copy24, output));
        flyout.PrimaryCommands.Add(CreateAppBarButton("Share", FluentIconRegular.Share24, output));
        flyout.PrimaryCommands.Add(CreateAppBarToggleButton("Pin", FluentIconRegular.Pin24, output, isChecked: true));
        flyout.SecondaryCommands.Add(CreateAppBarButton("Rename", FluentIconRegular.Rename24, output));
        flyout.SecondaryCommands.Add(CreateAppBarButton("Delete", FluentIconRegular.Delete24, output));
        button.Click += (_, _) =>
        {
            flyout.ShowAt(button);
            output.Text = "CommandBarFlyout: open";
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                button,
                CreateMenuButtonRow(
                    CreateMenuActionButton(FluentIconRegular.Open24, "Open", () =>
                    {
                        flyout.ShowAt(button);
                        output.Text = "CommandBarFlyout: open";
                    }),
                    CreateMenuActionButton(FluentIconRegular.DismissCircle24, "Hide", () =>
                    {
                        flyout.Hide();
                        output.Text = "CommandBarFlyout: hidden";
                    }),
                    CreateMenuActionButton(FluentIconRegular.ChevronUp24, "Collapse", () =>
                    {
                        flyout.AlwaysExpanded = false;
                        output.Text = "CommandBarFlyout: secondary commands collapsed on next open";
                    }),
                    CreateMenuActionButton(FluentIconRegular.ChevronDown24, "Expand", () =>
                    {
                        flyout.AlwaysExpanded = true;
                        output.Text = "CommandBarFlyout: secondary commands expanded on next open";
                    })),
                CreateMenuStatus(output)
            }
        };
    }

    private static UIElement CreateMaterialMenuWorkbenchSample()
    {
        var output = CreateMenuOutput("Workbench: LiquidGlass. MenuBar closed. Command flyout expanded.");
        var menuBar = new FWMenuBar
        {
            Width = 490
        };
        var file = CreateMenuBarItem(
            "File",
            ("New", "Ctrl+N", IconGlyph(FluentIconRegular.Document24)),
            ("Open", "Ctrl+O", IconGlyph(FluentIconRegular.FolderOpen24)),
            ("Save", "Ctrl+S", IconGlyph(FluentIconRegular.Save24)));
        var view = CreateMenuBarItem(
            "View",
            ("Zoom in", "Ctrl++", null),
            ("Actual size", "Ctrl+0", null),
            ("Settings", string.Empty, IconGlyph(FluentIconRegular.Settings24)));
        menuBar.Items.Add(file);
        menuBar.Items.Add(view);

        var menuFlyout = CreateMenuControlsFlyout(output);
        var menuButton = new FWDropDownButton
        {
            Content = CreateMenuButtonContent(FluentIconRegular.ChevronDown24, "Flyout"),
            Flyout = menuFlyout,
            MinWidth = 132
        };

        var commandFlyout = new FWCommandBarFlyout
        {
            AlwaysExpanded = true
        };
        var commandButton = new FWButton
        {
            Content = CreateMenuButtonContent(FluentIconRegular.MoreHorizontal24, "Command flyout"),
            MinWidth = 178
        };
        commandFlyout.PrimaryCommands.Add(CreateAppBarButton("Copy", FluentIconRegular.Copy24, output));
        commandFlyout.PrimaryCommands.Add(CreateAppBarButton("Share", FluentIconRegular.Share24, output));
        commandFlyout.PrimaryCommands.Add(CreateAppBarToggleButton("Pin", FluentIconRegular.Pin24, output, isChecked: true));
        commandFlyout.SecondaryCommands.Add(CreateAppBarButton("Rename", FluentIconRegular.Rename24, output));
        commandButton.Click += (_, _) =>
        {
            commandFlyout.ShowAt(commandButton);
            output.Text = "Workbench: command flyout open";
        };

        var contextMenu = new FWContextMenu
        {
            Placement = PlacementMode.Bottom,
            StaysOpen = true
        };
        var details = new FWMenuItem
        {
            Header = "Show metadata",
            IsCheckable = true,
            IsChecked = true,
            StaysOpenOnClick = true
        };
        contextMenu.Items.Add(new FWMenuItem { Header = "Refresh", Icon = IconGlyph(FluentIconRegular.ArrowClockwise24) });
        contextMenu.Items.Add(new FWMenuItem { Header = "Rename", Icon = IconGlyph(FluentIconRegular.Rename24) });
        contextMenu.Items.Add(details);
        details.Checked += (_, _) => output.Text = "Workbench: metadata visible";
        details.Unchecked += (_, _) => output.Text = "Workbench: metadata hidden";

        var target = CreateContextTarget("Material document", "Context menu, flyout, and command flyout share this LiquidGlass command surface.");
        target.ContextMenu = contextMenu;
        contextMenu.PlacementTarget = target;

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
                    menuBar,
                    new FWWrapPanel
                    {
                        HorizontalSpacing = 10,
                        VerticalSpacing = 10,
                        Children =
                        {
                            menuButton,
                            commandButton
                        }
                    },
                    target,
                    CreateMenuButtonRow(
                        CreateMenuActionButton(FluentIconRegular.Document24, "File", () =>
                        {
                            CloseMenuBarItems(view);
                            file.OpenMenu();
                            output.Text = "Workbench: File menu open";
                        }),
                        CreateMenuActionButton(FluentIconRegular.Open24, "Flyout", () =>
                        {
                            menuFlyout.ShowAt(menuButton);
                            output.Text = "Workbench: menu flyout open";
                        }),
                        CreateMenuActionButton(FluentIconRegular.MoreHorizontal24, "Commands", () =>
                        {
                            commandFlyout.ShowAt(commandButton);
                            output.Text = "Workbench: command flyout open";
                        }),
                        CreateMenuActionButton(FluentIconRegular.CursorClick24, "Context", () =>
                        {
                            ContextMenuService.Open(target, contextMenu);
                            output.Text = "Workbench: context menu open";
                        })),
                    CreateMenuStatus(output)
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
                    Text = "Layered menu surface",
                    FontSize = 15,
                    Foreground = ThemeBrush("TextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
    }

    private static FWBorder CreateContextTarget(string title, string description)
    {
        return new FWBorder
        {
            Width = 300,
            Height = 96,
            Background = ThemeBrush("ControlBackground"),
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
                    new FWTextBlock
                    {
                        Text = title,
                        Foreground = ThemeBrush("TextPrimary")
                    },
                    new FWTextBlock
                    {
                        Text = description,
                        FontSize = 12,
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = ThemeBrush("TextSecondary")
                    }
                }
            }
        };
    }

    private static FWBorder CreateMenuExampleCard(FluentIconRegular icon, string title, string description, UIElement content)
    {
        return GallerySampleCard.Create(icon, title, description, content, code: CreateSampleCode(title));
    }

    private static string CreateSampleCode(string title)
    {
        return title switch
        {
            "FWMenuBar" => "<FWMenuBar>\n    <FWMenuBarItem Title=\"File\" />\n    <FWMenuBarItem Title=\"Edit\" />\n</FWMenuBar>",
            "FWMenu and FWMenuItem" => "<FWMenu>\n    <FWMenuItem Header=\"Project\">\n        <FWMenuItem Header=\"Build\" InputGestureText=\"Ctrl+B\" />\n    </FWMenuItem>\n</FWMenu>",
            "FWContextMenu" => "<FWBorder>\n    <FWBorder.ContextMenu>\n        <FWContextMenu Placement=\"Bottom\" />\n    </FWBorder.ContextMenu>\n</FWBorder>",
            "FWMenuFlyout" => "<FWDropDownButton Content=\"Actions\">\n    <FWDropDownButton.Flyout>\n        <FWMenuFlyout />\n    </FWDropDownButton.Flyout>\n</FWDropDownButton>",
            "FWMenuFlyoutSubItem" => "<FWMenuFlyoutSubItem Text=\"Export\">\n    <FWMenuFlyoutItem Text=\"PDF document\" />\n</FWMenuFlyoutSubItem>",
            "FWCommandBarFlyout" => "<FWCommandBarFlyout AlwaysExpanded=\"True\">\n    <FWAppBarButton Label=\"Copy\" />\n</FWCommandBarFlyout>",
            "Material menu workbench" => "<FWFluentMaterialSurface MaterialKind=\"LiquidGlass\">\n    <FWMenuBar />\n    <FWCommandBarFlyout />\n</FWFluentMaterialSurface>",
            _ => "<FWMenuFlyout />"
        };
    }

    private static FWWrapPanel CreateMenuButtonRow(params FWButton[] buttons)
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

    private static FWButton CreateMenuActionButton(FluentIconRegular icon, string text, Action action)
    {
        var button = new FWButton
        {
            Content = CreateMenuButtonContent(icon, text)
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static FWStackPanel CreateMenuButtonContent(FluentIconRegular icon, string text)
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

    private static TextBlock CreateMenuOutput(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        };
    }

    private static FWBorder CreateMenuStatus(TextBlock status)
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

    private static FWAppBarButton CreateAppBarButton(string label, FluentIconRegular icon, TextBlock output)
    {
        var button = new FWAppBarButton
        {
            Label = label,
            Icon = CreateIcon(icon)
        };
        button.Click += (_, _) => output.Text = $"Command invoked: {label}";
        return button;
    }

    private static FWAppBarToggleButton CreateAppBarToggleButton(string label, FluentIconRegular icon, TextBlock output, bool isChecked = false)
    {
        var button = new FWAppBarToggleButton
        {
            Label = label,
            Icon = CreateIcon(icon),
            IsChecked = isChecked
        };
        button.Checked += (_, _) => output.Text = $"{label}: on";
        button.Unchecked += (_, _) => output.Text = $"{label}: off";
        return button;
    }

    private static void CloseMenuBarItems(params FWMenuBarItem[] items)
    {
        foreach (var item in items)
        {
            item.CloseMenu();
        }
    }

    private static string FormatOnOff(bool value)
    {
        return value ? "on" : "off";
    }

    private static FWMenuBarItem CreateMenuBarItem(string title, params (string Text, string Shortcut, object? Icon)[] items)
    {
        var menuBarItem = new FWMenuBarItem
        {
            Title = title
        };

        for (var index = 0; index < items.Length; index++)
        {
            var (text, shortcut, icon) = items[index];
            menuBarItem.Items.Add(new FWMenuFlyoutItem
            {
                Text = text,
                KeyboardAcceleratorTextOverride = shortcut,
                Icon = icon
            });
        }

        if (items.Length > 1)
        {
            menuBarItem.Items.Insert(items.Length - 1, new FWMenuFlyoutSeparator());
        }

        return menuBarItem;
    }

    private static FWMenuFlyout CreateMenuControlsFlyout(TextBlock? output = null)
    {
        var flyout = new FWMenuFlyout();
        var pin = new FWMenuFlyoutItem
        {
            Text = "Pin",
            Icon = IconGlyph(FluentIconRegular.Pin24),
            KeyboardAcceleratorTextOverride = "Ctrl+P"
        };
        var badges = new FWToggleMenuFlyoutItem
        {
            Text = "Show badges",
            IsChecked = true
        };
        var settings = new FWMenuFlyoutItem
        {
            Text = "Settings",
            Icon = IconGlyph(FluentIconRegular.Settings24)
        };
        var disabled = new FWMenuFlyoutItem
        {
            Text = "Disabled",
            IsEnabled = false
        };

        pin.Click += (_, _) =>
        {
            if (output != null)
            {
                output.Text = "MenuFlyout: Pin clicked";
            }
        };
        badges.Click += (_, _) =>
        {
            if (output != null)
            {
                output.Text = $"MenuFlyout: badges {FormatOnOff(badges.IsChecked)}";
            }
        };
        settings.Click += (_, _) =>
        {
            if (output != null)
            {
                output.Text = "MenuFlyout: Settings clicked";
            }
        };

        flyout.Items.Add(pin);
        flyout.Items.Add(badges);
        flyout.Items.Add(new FWMenuFlyoutSeparator());
        flyout.Items.Add(settings);
        flyout.Items.Add(disabled);
        return flyout;
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
                        CreateIcon(FluentIconRegular.List24, 24, ThemeBrush("TextPrimary")),
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
