using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWButton = FluentJalium.Controls.FWButton;
using FWCheckBox = FluentJalium.Controls.FWCheckBox;
using FWComboBox = FluentJalium.Controls.FWComboBox;
using FWComboBoxItem = FluentJalium.Controls.FWComboBoxItem;
using FWDropDownButton = FluentJalium.Controls.FWDropDownButton;
using FWFluentMaterialKind = FluentJalium.Controls.FWFluentMaterialKind;
using FWFluentMaterialSurface = FluentJalium.Controls.FWFluentMaterialSurface;
using FWMenuFlyout = FluentJalium.Controls.FWMenuFlyout;
using FWMenuFlyoutItem = FluentJalium.Controls.FWMenuFlyoutItem;
using FWMenuFlyoutSeparator = FluentJalium.Controls.FWMenuFlyoutSeparator;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWToggleButton = FluentJalium.Controls.FWToggleButton;
using FWToggleSplitButton = FluentJalium.Controls.FWToggleSplitButton;
using FWToggleSwitch = FluentJalium.Controls.FWToggleSwitch;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;

namespace FluentJalium.Gallery.Pages;

internal sealed class GalleryStateMatrixPage
{
    public UIElement CreateContent()
    {
        var panel = CreateSection("State Matrix");
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        examples.Children.Add(CreateStateMatrixCard(
            FluentIconRegular.ControlButton24,
            "Command states",
            "Normal, flyout, checked split, and disabled command surfaces.",
            CreateCommandStateRow()));
        examples.Children.Add(CreateStateMatrixCard(
            FluentIconRegular.ToggleMultiple24,
            "Toggle states",
            "ToggleButton and ToggleSwitch surfaces across off, on, selected, and disabled states.",
            CreateToggleStateRow()));
        examples.Children.Add(CreateStateMatrixCard(
            FluentIconRegular.CheckboxChecked24,
            "Selection states",
            "CheckBox and ComboBox states for unchecked, selected, indeterminate, and disabled layouts.",
            CreateSelectionStateRows()));
        examples.Children.Add(CreateStateMatrixCard(
            FluentIconRegular.LayerDiagonalSparkle24,
            "Material state review",
            "Common state surfaces stay legible on the LiquidGlass layer used across FluentJalium pages.",
            CreateMaterialStateReview()));

        panel.Children.Add(examples);
        return panel;
    }

    private static UIElement CreateCommandStateRow()
    {
        return CreateStateColumns(
            CreateStateColumn("Normal", FluentIconRegular.ControlButton24, new FWButton { Content = "Normal" }),
            CreateStateColumn("DropDown", FluentIconRegular.ChevronDown24, new FWDropDownButton { Content = "Open", Flyout = CreateSampleFlyout() }),
            CreateStateColumn("Selected", FluentIconRegular.CheckmarkCircle24, new FWToggleSplitButton { Content = "Selected", IsChecked = true, Flyout = CreateSampleFlyout() }),
            CreateStateColumn("Disabled", FluentIconRegular.DismissCircle24, new FWButton { Content = "Disabled", IsEnabled = false }));
    }

    private static UIElement CreateToggleStateRow()
    {
        return CreateStateColumns(
            CreateStateColumn("Off", FluentIconRegular.DismissCircle24, new FWToggleButton { Content = "Off" }),
            CreateStateColumn("On", FluentIconRegular.CheckmarkCircle24, new FWToggleButton { Content = "On", IsChecked = true }),
            CreateStateColumn("Switch", FluentIconRegular.Power24, new FWToggleSwitch { Header = "On switch", IsOn = true }),
            CreateStateColumn("Disabled", FluentIconRegular.Pause24, new FWToggleSwitch { Header = "Disabled", IsOn = true, IsEnabled = false }));
    }

    private static UIElement CreateSelectionStateRows()
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Children =
            {
                CreateStateColumns(
                    CreateStateColumn("Unchecked", FluentIconRegular.Square24, new FWCheckBox { Content = "Unchecked" }),
                    CreateStateColumn("Checked", FluentIconRegular.CheckboxChecked24, new FWCheckBox { Content = "Checked", IsChecked = true }),
                    CreateStateColumn("Mixed", FluentIconRegular.CheckboxIndeterminate24, new FWCheckBox { Content = "Indeterminate", IsThreeState = true, IsChecked = null }),
                    CreateStateColumn("Disabled", FluentIconRegular.DismissCircle24, new FWCheckBox { Content = "Disabled", IsChecked = true, IsEnabled = false })),
                CreateStateColumns(
                    CreateStateColumn("Placeholder", FluentIconRegular.Textbox24, CreateSampleComboBox("Normal", -1, true)),
                    CreateStateColumn("Selected", FluentIconRegular.List24, CreateSampleComboBox("Selected", 1, true)),
                    CreateStateColumn("Toolkit", FluentIconRegular.AppsListDetail24, CreateSampleComboBox("Toolkit", 2, true)),
                    CreateStateColumn("Disabled", FluentIconRegular.DismissCircle24, CreateSampleComboBox("Disabled", 0, false)))
            }
        };
    }

    private static UIElement CreateMaterialStateReview()
    {
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
                    CreateMaterialStateRow("Commands", new FWButton { Content = "Normal" }, new FWToggleSplitButton { Content = "Selected", IsChecked = true, Flyout = CreateSampleFlyout() }),
                    CreateMaterialStateRow("Toggles", new FWToggleButton { Content = "On", IsChecked = true }, new FWToggleSwitch { Header = "Disabled", IsOn = true, IsEnabled = false }),
                    CreateMaterialStateRow("Selection", new FWCheckBox { Content = "Mixed", IsThreeState = true, IsChecked = null }, CreateSampleComboBox("Selected", 1, true))
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
                CreateIcon(FluentIconRegular.DataUsage24, 18, ThemeBrush("TextPrimary")),
                new FWTextBlock
                {
                    Text = "State matrix on material",
                    FontSize = 15,
                    Foreground = ThemeBrush("TextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
    }

    private static FWBorder CreateMaterialStateRow(string label, UIElement first, UIElement second)
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
                Spacing = 12,
                Children =
                {
                    new FWTextBlock
                    {
                        Text = label,
                        Width = 88,
                        Foreground = ThemeBrush("TextSecondary"),
                        VerticalAlignment = VerticalAlignment.Center
                    },
                    first,
                    second
                }
            }
        };
    }

    private static FWWrapPanel CreateStateColumns(params FWBorder[] columns)
    {
        var row = new FWWrapPanel
        {
            HorizontalSpacing = 10,
            VerticalSpacing = 10
        };

        foreach (var column in columns)
        {
            row.Children.Add(column);
        }

        return row;
    }

    private static FWBorder CreateStateColumn(string title, FluentIconRegular icon, UIElement content)
    {
        return new FWBorder
        {
            Width = 132,
            Background = ThemeBrush("LayerFillColorDefaultBrush"),
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
                    new FWStackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 6,
                        Children =
                        {
                            CreateIcon(icon, 16, ThemeBrush("TextSecondary")),
                            new FWTextBlock
                            {
                                Text = title,
                                FontSize = 12,
                                Foreground = ThemeBrush("TextSecondary"),
                                VerticalAlignment = VerticalAlignment.Center
                            }
                        }
                    },
                    content
                }
            }
        };
    }

    private static FWBorder CreateStateMatrixCard(FluentIconRegular icon, string title, string description, UIElement content)
    {
        return new FWBorder
        {
            Width = 570,
            Background = ThemeBrush("ControlBackground"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
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
                            CreateIcon(icon, 20, ThemeBrush("TextPrimary")),
                            new FWTextBlock
                            {
                                Text = title,
                                FontSize = 15,
                                Foreground = ThemeBrush("TextPrimary"),
                                VerticalAlignment = VerticalAlignment.Center
                            }
                        }
                    },
                    new FWTextBlock
                    {
                        Text = description,
                        FontSize = 12,
                        Foreground = ThemeBrush("TextSecondary"),
                        TextWrapping = TextWrapping.Wrap
                    },
                    content
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
                        CreateIcon(FluentIconRegular.DataUsage24, 24, ThemeBrush("TextPrimary")),
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

    private static FWMenuFlyout CreateSampleFlyout()
    {
        var flyout = new FWMenuFlyout();
        flyout.Items.Add(new FWMenuFlyoutItem { Text = "Create", Icon = IconGlyph(FluentIconRegular.Add24) });
        flyout.Items.Add(new FWMenuFlyoutItem { Text = "Open", Icon = IconGlyph(FluentIconRegular.Open24) });
        flyout.Items.Add(new FWMenuFlyoutSeparator());
        flyout.Items.Add(new FWMenuFlyoutItem { Text = "Export", Icon = IconGlyph(FluentIconRegular.ArrowDownload24) });
        return flyout;
    }

    private static FWComboBox CreateSampleComboBox(string placeholder, int selectedIndex, bool isEnabled)
    {
        var comboBox = new FWComboBox
        {
            Width = 160,
            PlaceholderText = placeholder,
            IsEnabled = isEnabled
        };

        comboBox.Items.Add(new FWComboBoxItem { Content = "Fluent" });
        comboBox.Items.Add(new FWComboBoxItem { Content = "WinUI" });
        comboBox.Items.Add(new FWComboBoxItem { Content = "Toolkit" });
        comboBox.SelectedIndex = selectedIndex;
        return comboBox;
    }

    private static FluentIcon CreateIcon(FluentIconRegular icon, double size = FluentIcon.DefaultSize, Brush? foreground = null)
    {
        return FluentIconFactory.Regular(icon, size, foreground ?? ThemeBrush("TextPrimary"));
    }

    private static string IconGlyph(FluentIconRegular icon) => icon.GetString();

    private static Brush ThemeBrush(string key)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Brush brush)
        {
            return brush;
        }

        return new SolidColorBrush(Colors.Transparent);
    }
}
