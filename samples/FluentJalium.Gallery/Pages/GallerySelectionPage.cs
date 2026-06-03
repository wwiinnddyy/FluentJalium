using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWCheckBox = FluentJalium.Controls.FWCheckBox;
using FWComboBox = FluentJalium.Controls.FWComboBox;
using FWComboBoxItem = FluentJalium.Controls.FWComboBoxItem;
using FWFluentMaterialKind = FluentJalium.Controls.FWFluentMaterialKind;
using FWFluentMaterialSurface = FluentJalium.Controls.FWFluentMaterialSurface;
using FWRadioButton = FluentJalium.Controls.FWRadioButton;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;

namespace FluentJalium.Gallery.Pages;

internal sealed class GallerySelectionPage
{
    public UIElement CreateContent()
    {
        var panel = CreateSection("Selection");
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        examples.Children.Add(CreateSelectionExampleCard(
            FluentIconRegular.CheckboxChecked24,
            "FWCheckBox",
            "Two-state, three-state, disabled, and Select all patterns.",
            CreateCheckBoxSelectionSample()));
        examples.Children.Add(CreateSelectionExampleCard(
            FluentIconRegular.RadioButton24,
            "FWRadioButton",
            "Named groups keep a single selected option and report the current choice.",
            CreateRadioButtonSelectionSample()));
        examples.Children.Add(CreateSelectionExampleCard(
            FluentIconRegular.ChevronDown24,
            "FWComboBox",
            "Inline items, editable text, placeholder, disabled, and selection output.",
            CreateComboBoxSelectionSample()));
        examples.Children.Add(CreateSelectionExampleCard(
            FluentIconRegular.List24,
            "FWComboBoxItem",
            "Selected, hover-ready, and disabled dropdown item states use Fluent resources.",
            CreateComboBoxItemStateSample()));
        examples.Children.Add(CreateSelectionExampleCard(
            FluentIconRegular.LayerDiagonalSparkle24,
            "Material selection settings",
            "Selection controls stay readable on FluentJalium layered material surfaces.",
            CreateMaterialSelectionSample()));

        panel.Children.Add(examples);
        return panel;
    }

    private static UIElement CreateCheckBoxSelectionSample()
    {
        var output = CreateSelectionOutput("Selected: Fluent, Gallery");
        var selectAll = new FWCheckBox
        {
            Content = "Select all",
            IsThreeState = true,
            IsChecked = null
        };
        var fluent = new FWCheckBox { Content = "Fluent", IsChecked = true };
        var controls = new FWCheckBox { Content = "Controls" };
        var gallery = new FWCheckBox { Content = "Gallery", IsChecked = true };
        var disabled = new FWCheckBox { Content = "Disabled option", IsChecked = true, IsEnabled = false };

        var children = new[] { fluent, controls, gallery };
        var isUpdating = false;

        void UpdateOutput()
        {
            var selected = children
                .Where(checkBox => checkBox.IsChecked == true)
                .Select(checkBox => checkBox.Content?.ToString())
                .Where(text => !string.IsNullOrEmpty(text));
            var selectedText = string.Join(", ", selected);
            output.Text = $"Selected: {(selectedText.Length > 0 ? selectedText : "none")}";

            if (isUpdating)
            {
                return;
            }

            isUpdating = true;
            var selectedCount = children.Count(checkBox => checkBox.IsChecked == true);
            selectAll.IsChecked = selectedCount == 0 ? false : selectedCount == children.Length ? true : null;
            isUpdating = false;
        }

        selectAll.Checked += (_, _) =>
        {
            if (isUpdating)
            {
                return;
            }

            isUpdating = true;
            foreach (var child in children)
            {
                child.IsChecked = true;
            }
            isUpdating = false;
            UpdateOutput();
        };
        selectAll.Unchecked += (_, _) =>
        {
            if (isUpdating)
            {
                return;
            }

            isUpdating = true;
            foreach (var child in children)
            {
                child.IsChecked = false;
            }
            isUpdating = false;
            UpdateOutput();
        };

        foreach (var child in children)
        {
            child.Checked += (_, _) => UpdateOutput();
            child.Unchecked += (_, _) => UpdateOutput();
        }

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 9,
            Children =
            {
                selectAll,
                fluent,
                controls,
                gallery,
                new FWCheckBox { Content = "Indeterminate", IsThreeState = true, IsChecked = null },
                disabled,
                CreateSelectionStatus(output)
            }
        };
    }

    private static UIElement CreateRadioButtonSelectionSample()
    {
        var output = CreateSelectionOutput("Selected: Windows");
        var groupName = $"GallerySelection{Guid.NewGuid():N}";

        FWRadioButton CreateOption(string text, bool isChecked = false, bool isEnabled = true)
        {
            var radioButton = new FWRadioButton
            {
                Content = text,
                GroupName = groupName,
                IsChecked = isChecked,
                IsEnabled = isEnabled
            };
            radioButton.Checked += (_, _) => output.Text = $"Selected: {text}";
            return radioButton;
        }

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 9,
            Children =
            {
                CreateOption("Windows", true),
                CreateOption("Toolkit"),
                CreateOption("Jalium"),
                CreateOption("Disabled", false, false),
                CreateSelectionStatus(output)
            }
        };
    }

    private static UIElement CreateComboBoxSelectionSample()
    {
        var output = CreateSelectionOutput("Selected: Control styles");
        var comboBox = new FWComboBox
        {
            Width = 260,
            PlaceholderText = "Choose an item"
        };
        comboBox.Items.Add(new FWComboBoxItem { Content = "Fluent tokens" });
        comboBox.Items.Add(new FWComboBoxItem { Content = "Control styles" });
        comboBox.Items.Add(new FWComboBoxItem { Content = "Gallery sample" });
        comboBox.SelectedIndex = 1;
        comboBox.SelectionChanged += (_, _) => output.Text = $"Selected: {comboBox.SelectionBoxItem}";

        var editableOutput = CreateSelectionOutput("Editable text: Custom value");
        var editableComboBox = new FWComboBox
        {
            Width = 260,
            PlaceholderText = "Type or select",
            IsEditable = true,
            Text = "Custom value",
            StaysOpenOnEdit = true
        };
        editableComboBox.Items.Add("Custom value");
        editableComboBox.Items.Add("Preset A");
        editableComboBox.Items.Add("Preset B");
        editableComboBox.SelectionChanged += (_, _) => editableOutput.Text = $"Editable text: {editableComboBox.SelectionBoxItem}";

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                CreateSelectionCaption("Inline items"),
                comboBox,
                CreateSelectionStatus(output),
                CreateSelectionCaption("Editable"),
                editableComboBox,
                CreateSelectionStatus(editableOutput),
                CreateSelectionCaption("Disabled"),
                CreateDisabledSelectionComboBox()
            }
        };
    }

    private static FWComboBox CreateDisabledSelectionComboBox()
    {
        var comboBox = new FWComboBox
        {
            Width = 260,
            PlaceholderText = "Disabled",
            IsEnabled = false
        };
        comboBox.Items.Add(new FWComboBoxItem { Content = "Unavailable" });
        comboBox.SelectedIndex = 0;
        return comboBox;
    }

    private static UIElement CreateComboBoxItemStateSample()
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Children =
            {
                new FWComboBoxItem { Content = "Normal item", Width = 260 },
                new FWComboBoxItem { Content = "Selected item", Width = 260, IsSelected = true },
                new FWComboBoxItem { Content = "Disabled item", Width = 260, IsEnabled = false },
                CreateSelectionStatus(CreateSelectionOutput("Open a ComboBox above to validate live dropdown behavior."))
            }
        };
    }

    private static UIElement CreateMaterialSelectionSample()
    {
        var densityOutput = CreateSelectionOutput("Density: Comfortable");
        var colorOutput = CreateSelectionOutput("Accent set: Mica");
        var accentGroupName = $"MaterialSelection{Guid.NewGuid():N}";

        FWRadioButton CreateAccentOption(string text, bool isChecked = false)
        {
            var radioButton = new FWRadioButton
            {
                Content = text,
                GroupName = accentGroupName,
                IsChecked = isChecked
            };
            radioButton.Checked += (_, _) => colorOutput.Text = $"Accent set: {text}";
            return radioButton;
        }

        var densityComboBox = new FWComboBox
        {
            Width = 200,
            SelectedIndex = 1
        };
        densityComboBox.Items.Add(new FWComboBoxItem { Content = "Compact" });
        densityComboBox.Items.Add(new FWComboBoxItem { Content = "Comfortable" });
        densityComboBox.Items.Add(new FWComboBoxItem { Content = "Spacious" });
        densityComboBox.SelectionChanged += (_, _) => densityOutput.Text = $"Density: {densityComboBox.SelectionBoxItem}";

        return new FWFluentMaterialSurface
        {
            Width = 456,
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
                    CreateMaterialRow("Accent", CreateAccentOption("Mica", true), CreateAccentOption("Acrylic"), CreateAccentOption("Glass")),
                    CreateSelectionStatus(colorOutput),
                    CreateMaterialRow("Density", densityComboBox),
                    CreateSelectionStatus(densityOutput),
                    new FWCheckBox { Content = "Show selection indicators", IsChecked = true }
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
                CreateIcon(FluentIconRegular.Settings24, 18, ThemeBrush("TextPrimary")),
                new FWTextBlock
                {
                    Text = "Selection settings",
                    FontSize = 15,
                    Foreground = ThemeBrush("TextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
    }

    private static FWBorder CreateMaterialRow(string label, params UIElement[] controls)
    {
        var row = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8
        };
        row.Children.Add(new FWTextBlock
        {
            Text = label,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary")
        });

        var controlsRow = new FWWrapPanel
        {
            HorizontalSpacing = 12,
            VerticalSpacing = 8
        };
        foreach (var control in controls)
        {
            controlsRow.Children.Add(control);
        }
        row.Children.Add(controlsRow);

        return new FWBorder
        {
            Background = ThemeBrush("LayerFillColorDefaultBrush"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10),
            Child = row
        };
    }

    private static FWBorder CreateSelectionExampleCard(FluentIconRegular icon, string title, string description, UIElement content)
    {
        return new FWBorder
        {
            Width = 520,
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

    private static TextBlock CreateSelectionCaption(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary")
        };
    }

    private static TextBlock CreateSelectionOutput(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        };
    }

    private static FWBorder CreateSelectionStatus(TextBlock status)
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
                        CreateIcon(FluentIconRegular.CheckboxChecked24, 24, ThemeBrush("TextPrimary")),
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
