using System.Globalization;
using FluentJalium.Gallery.Controls;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using FWAutoCompleteBox = FluentJalium.Controls.FWAutoCompleteBox;
using FWAutoSuggestBox = FluentJalium.Controls.FWAutoSuggestBox;
using FWAutoSuggestBoxTextChangeReason = FluentJalium.Controls.FWAutoSuggestBoxTextChangeReason;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWButton = FluentJalium.Controls.FWButton;
using FWFluentMaterialKind = FluentJalium.Controls.FWFluentMaterialKind;
using FWFluentMaterialSurface = FluentJalium.Controls.FWFluentMaterialSurface;
using FWNumberBox = FluentJalium.Controls.FWNumberBox;
using FWNumberBoxDensity = FluentJalium.Controls.FWNumberBoxDensity;
using FWPasswordBox = FluentJalium.Controls.FWPasswordBox;
using FWRichTextBox = FluentJalium.Controls.FWRichTextBox;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWTextBox = FluentJalium.Controls.FWTextBox;
using FWTextInputDensity = FluentJalium.Controls.FWTextInputDensity;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;

namespace FluentJalium.Gallery.Pages;

internal sealed class GalleryTextInputPage
{
    public UIElement CreateContent()
    {
        var panel = CreateSection("Text Input");
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        examples.Children.Add(CreateTextInputExampleCard(
            FluentIconRegular.Textbox24,
            "FWTextBox",
            "Single-line, multiline, wrapping, placeholder, disabled, and live TextChanged output.",
            CreateTextBoxInputSample()));
        examples.Children.Add(CreateTextInputExampleCard(
            FluentIconRegular.Password24,
            "FWPasswordBox and FWNumberBox",
            "Reveal modes, password state, numeric stepping, coercion, decimal precision, and live output.",
            CreatePasswordNumberInputSample()));
        examples.Children.Add(CreateTextInputExampleCard(
            FluentIconRegular.Search24,
            "FWAutoCompleteBox",
            "Contains filtering, drop-down state, suggestion counts, and search-style placeholder text.",
            CreateAutoCompleteInputSample()));
        examples.Children.Add(CreateTextInputExampleCard(
            FluentIconRegular.SearchSparkle24,
            "FWAutoSuggestBox",
            "WinUI-style suggest naming with filtering, drop-down state, density, and live suggestion counts.",
            CreateAutoSuggestInputSample()));
        examples.Children.Add(CreateTextInputExampleCard(
            FluentIconRegular.DocumentText24,
            "FWRichTextBox",
            "Rich text editing surface with tab input, spell checking, selection, and clear-selection state.",
            CreateRichTextInputSample()));
        examples.Children.Add(CreateTextInputExampleCard(
            FluentIconRegular.LayerDiagonalSparkle24,
            "Material input panel",
            "TextBox, PasswordBox, NumberBox, AutoCompleteBox, and RichTextBox remain readable on LiquidGlass.",
            CreateMaterialInputPanelSample()));

        panel.Children.Add(examples);
        return panel;
    }

    private static UIElement CreateTextBoxInputSample()
    {
        var output = CreateTextInputOutput("TextBox: ready");
        var textBox = new FWTextBox
        {
            Text = "FWTextBox",
            Width = 260,
            PlaceholderText = "Enter text"
        };
        var multiline = new FWTextBox
        {
            Text = "FluentJalium\r\nText input",
            Width = 260,
            Height = 78,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            PlaceholderText = "Multiline text"
        };
        textBox.TextChanged += (_, _) => output.Text = $"TextBox text: {textBox.Text}";
        multiline.TextChanged += (_, _) => output.Text = $"Multiline lines: {multiline.LineCount}";

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                new FWWrapPanel
                {
                    HorizontalSpacing = 12,
                    VerticalSpacing = 10,
                    Children =
                    {
                        textBox,
                        multiline,
                        new FWTextBox
                        {
                            Text = "Disabled",
                            Width = 220,
                            IsEnabled = false
                        }
                    }
                },
                CreateTextInputButtonRow(
                    CreateTextInputActionButton(FluentIconRegular.TextEditStyle24, "Replace", () => textBox.Text = "Edited text"),
                    CreateTextInputActionButton(FluentIconRegular.Keyboard24, "Append", () => multiline.Text += "\r\nNew line"),
                    CreateTextInputActionButton(FluentIconRegular.DismissCircle24, "Clear", () =>
                    {
                        textBox.Text = string.Empty;
                        multiline.Text = string.Empty;
                        output.Text = "TextBox values cleared";
                    })),
                CreateTextInputStatus(output)
            }
        };
    }

    private static UIElement CreatePasswordNumberInputSample()
    {
        var output = CreateTextInputOutput("NumberBox value: 42. Density: comfortable. Spin: inline");
        var passwordBox = new FWPasswordBox
        {
            Password = "fluent",
            Width = 240,
            PlaceholderText = "Password",
            RevealMode = PasswordRevealMode.Visible
        };
        var numberBox = new FWNumberBox
        {
            Header = "FWNumberBox",
            Width = 200,
            Density = FWNumberBoxDensity.Comfortable,
            Minimum = 0,
            Maximum = 100,
            Value = 42,
            SmallChange = 2,
            LargeChange = 10,
            DecimalPlaces = 0,
            PlaceholderText = "0-100"
        };
        passwordBox.PasswordChanged += (_, _) => output.Text = $"Password length: {passwordBox.Password.Length}";
        numberBox.ValueChanged += (_, e) =>
            output.Text = $"NumberBox value: {FormatNumberBoxValue(e.NewValue, numberBox.DecimalPlaces)}. Density: {FormatDensity(numberBox.Density)}. Spin: {FormatSpinPlacement(numberBox.SpinButtonPlacementMode)}";

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                new FWWrapPanel
                {
                    HorizontalSpacing = 12,
                    VerticalSpacing = 10,
                    Children =
                    {
                        passwordBox,
                        numberBox,
                        new FWNumberBox
                        {
                            Header = "Hidden spin",
                            Width = 180,
                            Density = FWNumberBoxDensity.Compact,
                            Value = 12,
                            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Hidden
                        },
                        new FWNumberBox
                        {
                            Header = "Disabled",
                            Width = 180,
                            Density = FWNumberBoxDensity.Spacious,
                            Value = 24,
                            IsEnabled = false
                        }
                    }
                },
                CreateTextInputButtonRow(
                    CreateTextInputActionButton(FluentIconRegular.Eye24, "Reveal", () =>
                    {
                        passwordBox.IsPasswordRevealed = !passwordBox.IsPasswordRevealed;
                        output.Text = $"Password revealed: {FormatOnOff(passwordBox.IsPasswordRevealed)}";
                    }),
                    CreateTextInputActionButton(FluentIconRegular.NumberSymbol24, "Step up", () => numberBox.StepUp()),
                    CreateTextInputActionButton(FluentIconRegular.NumberSymbol24, "Step down", () => numberBox.StepDown()),
                    CreateTextInputActionButton(FluentIconRegular.TextDensity24, "Density", () =>
                    {
                        numberBox.Density = NextDensity(numberBox.Density);
                        output.Text = $"NumberBox value: {FormatNumberBoxValue(numberBox.Value, numberBox.DecimalPlaces)}. Density: {FormatDensity(numberBox.Density)}. Spin: {FormatSpinPlacement(numberBox.SpinButtonPlacementMode)}";
                    }),
                    CreateTextInputActionButton(FluentIconRegular.TextBoxSettings24, "Spin", () =>
                    {
                        numberBox.SpinButtonPlacementMode = NextSpinPlacement(numberBox.SpinButtonPlacementMode);
                        output.Text = $"NumberBox value: {FormatNumberBoxValue(numberBox.Value, numberBox.DecimalPlaces)}. Density: {FormatDensity(numberBox.Density)}. Spin: {FormatSpinPlacement(numberBox.SpinButtonPlacementMode)}";
                    }),
                    CreateTextInputActionButton(FluentIconRegular.TextBoxSettings24, "Wrap", () =>
                    {
                        numberBox.IsWrapEnabled = !numberBox.IsWrapEnabled;
                        output.Text = $"NumberBox wrap: {FormatOnOff(numberBox.IsWrapEnabled)}";
                    }),
                    CreateTextInputActionButton(FluentIconRegular.DecimalArrowLeft24, "Decimals", () =>
                    {
                        numberBox.DecimalPlaces = numberBox.DecimalPlaces == 0 ? 2 : 0;
                        output.Text = $"NumberBox value: {FormatNumberBoxValue(numberBox.Value, numberBox.DecimalPlaces)}. Decimals: {numberBox.DecimalPlaces}";
                    })),
                CreateTextInputStatus(output)
            }
        };
    }

    private static UIElement CreateAutoCompleteInputSample()
    {
        var output = CreateTextInputOutput("AutoCompleteBox: ready");
        var autoCompleteBox = new FWAutoCompleteBox
        {
            Width = 320,
            ItemsSource = SearchItems,
            Text = "Fl",
            PlaceholderText = "Search controls",
            FilterMode = AutoCompleteFilterMode.Contains,
            MinimumPrefixLength = 1
        };
        autoCompleteBox.TextChanged += (_, _) => output.Text = $"Search: {autoCompleteBox.Text}, matches: {autoCompleteBox.FilteredItems.Count}";
        autoCompleteBox.DropDownOpened += (_, _) => output.Text = $"Drop-down open: {autoCompleteBox.FilteredItems.Count} matches";
        autoCompleteBox.DropDownClosed += (_, _) => output.Text = "Drop-down closed";

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                autoCompleteBox,
                CreateTextInputButtonRow(
                    CreateTextInputActionButton(FluentIconRegular.Search24, "Fluent", () => autoCompleteBox.Text = "Fluent"),
                    CreateTextInputActionButton(FluentIconRegular.Search24, "Toolkit", () => autoCompleteBox.Text = "Toolkit"),
                    CreateTextInputActionButton(FluentIconRegular.ChevronDown24, "Drop-down", () =>
                    {
                        autoCompleteBox.IsDropDownOpen = !autoCompleteBox.IsDropDownOpen;
                        output.Text = $"AutoCompleteBox drop-down: {FormatOnOff(autoCompleteBox.IsDropDownOpen)}";
                    }),
                    CreateTextInputActionButton(FluentIconRegular.DismissCircle24, "Clear", () => autoCompleteBox.Text = string.Empty)),
                CreateTextInputStatus(output)
            }
        };
    }

    private static UIElement CreateAutoSuggestInputSample()
    {
        var output = CreateTextInputOutput("AutoSuggestBox: ready");
        var autoSuggestBox = new FWAutoSuggestBox
        {
            Width = 320,
            ItemsSource = SearchItems,
            Text = "Auto",
            PlaceholderText = "Search FluentJalium",
            FilterMode = AutoCompleteFilterMode.Contains,
            MinimumPrefixLength = 1,
            Density = FWTextInputDensity.Comfortable
        };

        void UpdateOutput(string reason)
        {
            var selected = autoSuggestBox.SelectedItem?.ToString() ?? "none";
            output.Text = $"{reason}: {autoSuggestBox.Text}, matches: {autoSuggestBox.FilteredItems.Count}, selected: {selected}, reason: {autoSuggestBox.LastTextChangeReason}, density: {FormatDensity(autoSuggestBox.Density)}, drop-down: {FormatOnOff(autoSuggestBox.IsDropDownOpen)}";
        }

        autoSuggestBox.TextChanged += (_, _) => UpdateOutput("Suggest");
        autoSuggestBox.SuggestionChosen += (_, args) => output.Text = $"Suggestion chosen: {args.SelectedItem}. Text: {autoSuggestBox.Text}, reason: {autoSuggestBox.LastTextChangeReason}.";
        autoSuggestBox.QuerySubmitted += (_, args) => output.Text = $"Query submitted: {args.QueryText}, chosen: {args.ChosenSuggestion?.ToString() ?? "none"}.";
        autoSuggestBox.DropDownOpened += (_, _) => UpdateOutput("Drop-down open");
        autoSuggestBox.DropDownClosed += (_, _) => UpdateOutput("Drop-down closed");
        UpdateOutput("Suggest");

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                autoSuggestBox,
                CreateTextInputButtonRow(
                    CreateTextInputActionButton(FluentIconRegular.Search24, "Auto", () =>
                    {
                        autoSuggestBox.SetQueryText("Auto", FWAutoSuggestBoxTextChangeReason.UserInput);
                        UpdateOutput("Suggest");
                    }),
                    CreateTextInputActionButton(FluentIconRegular.SearchSparkle24, "Calendar", () =>
                    {
                        autoSuggestBox.SetQueryText("Calendar", FWAutoSuggestBoxTextChangeReason.UserInput);
                        UpdateOutput("Suggest");
                    }),
                    CreateTextInputActionButton(FluentIconRegular.CheckmarkCircle24, "Choose first", () =>
                    {
                        if (autoSuggestBox.FilteredItems.Count > 0)
                        {
                            autoSuggestBox.RequestSuggestionChosen(autoSuggestBox.FilteredItems[0]);
                        }
                        else
                        {
                            UpdateOutput("No suggestion");
                        }
                    }),
                    CreateTextInputActionButton(FluentIconRegular.Send24, "Submit", () => autoSuggestBox.RequestQuerySubmitted()),
                    CreateTextInputActionButton(FluentIconRegular.TextDensity24, "Density", () =>
                    {
                        autoSuggestBox.Density = NextDensity(autoSuggestBox.Density);
                        UpdateOutput("Density");
                    }),
                    CreateTextInputActionButton(FluentIconRegular.ChevronDown24, "Drop-down", () =>
                    {
                        autoSuggestBox.IsDropDownOpen = !autoSuggestBox.IsDropDownOpen;
                        UpdateOutput("Drop-down");
                    }),
                    CreateTextInputActionButton(FluentIconRegular.DismissCircle24, "Clear", () =>
                    {
                        autoSuggestBox.Text = string.Empty;
                        UpdateOutput("Clear");
                    })),
                CreateTextInputStatus(output)
            }
        };
    }

    private static UIElement CreateRichTextInputSample()
    {
        var output = CreateTextInputOutput("RichTextBox: ready");
        var richTextBox = new FWRichTextBox
        {
            Width = 500,
            Height = 120,
            AcceptsTab = true,
            IsSpellCheckEnabled = true
        };
        richTextBox.SetText("FWRichTextBox uses the same text input resource tokens.");

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                richTextBox,
                CreateTextInputButtonRow(
                    CreateTextInputActionButton(FluentIconRegular.TextEditStyle24, "Select all", () =>
                    {
                        richTextBox.SelectAll();
                        output.Text = $"Selected: {richTextBox.Selection.Text.Length} chars";
                    }),
                    CreateTextInputActionButton(FluentIconRegular.DocumentText24, "Replace", () =>
                    {
                        richTextBox.SetText("Rich text content updated inside the FluentJalium gallery.");
                        output.Text = "RichTextBox text replaced";
                    }),
                    CreateTextInputActionButton(FluentIconRegular.DismissCircle24, "Clear selection", () =>
                    {
                        richTextBox.ClearSelection();
                        output.Text = $"Selection empty: {FormatOnOff(richTextBox.Selection.IsEmpty)}";
                    })),
                CreateTextInputStatus(output)
            }
        };
    }

    private static UIElement CreateMaterialInputPanelSample()
    {
        var output = CreateTextInputOutput("Material inputs: LiquidGlass, search ready, number 24.");
        var searchBox = new FWAutoSuggestBox
        {
            Width = 480,
            ItemsSource = SearchItems,
            Text = "Fluent",
            PlaceholderText = "Search FluentJalium",
            FilterMode = AutoCompleteFilterMode.Contains,
            MinimumPrefixLength = 1,
            Density = FWTextInputDensity.Comfortable
        };
        var numberBox = new FWNumberBox
        {
            Header = "Opacity",
            Width = 170,
            Minimum = 0,
            Maximum = 100,
            Value = 24,
            SmallChange = 4,
            DecimalPlaces = 0
        };
        var passwordBox = new FWPasswordBox
        {
            Width = 220,
            Password = "material",
            PlaceholderText = "Token",
            RevealMode = PasswordRevealMode.Peek
        };
        var notes = new FWRichTextBox
        {
            Width = 480,
            Height = 104,
            AcceptsTab = true,
            IsSpellCheckEnabled = true
        };
        notes.SetText("Layered input surfaces keep focus and text contrast on LiquidGlass.");

        searchBox.TextChanged += (_, _) => output.Text = $"Material search: {searchBox.FilteredItems.Count} matches";
        numberBox.ValueChanged += (_, e) => output.Text = $"Material opacity value: {e.NewValue}";
        passwordBox.PasswordChanged += (_, _) => output.Text = $"Material token length: {passwordBox.Password.Length}";

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
                    searchBox,
                    new FWWrapPanel
                    {
                        HorizontalSpacing = 12,
                        VerticalSpacing = 10,
                        Children =
                        {
                            numberBox,
                            passwordBox
                        }
                    },
                    notes,
                    CreateTextInputButtonRow(
                        CreateTextInputActionButton(FluentIconRegular.Search24, "Controls", () => searchBox.Text = "controls"),
                        CreateTextInputActionButton(FluentIconRegular.NumberSymbol24, "Step", () => numberBox.StepUp()),
                        CreateTextInputActionButton(FluentIconRegular.Eye24, "Reveal", () =>
                        {
                            passwordBox.IsPasswordRevealed = !passwordBox.IsPasswordRevealed;
                            output.Text = $"Material token revealed: {FormatOnOff(passwordBox.IsPasswordRevealed)}";
                        }),
                        CreateTextInputActionButton(FluentIconRegular.DocumentText24, "Select notes", () =>
                        {
                            notes.SelectAll();
                            output.Text = $"Material notes selected: {notes.Selection.Text.Length} chars";
                        })),
                    CreateTextInputStatus(output)
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
                    Text = "Layered input surface",
                    FontSize = 15,
                    Foreground = ThemeBrush("TextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
    }

    private static FWBorder CreateTextInputExampleCard(FluentIconRegular icon, string title, string description, UIElement content)
    {
        return GallerySampleCard.Create(icon, title, description, content, code: CreateSampleCode(title));
    }

    private static string CreateSampleCode(string title)
    {
        return title switch
        {
            "FWTextBox" => "<FWTextBox PlaceholderText=\"Enter text\" />\n<FWTextBox AcceptsReturn=\"True\" TextWrapping=\"Wrap\" />",
            "FWPasswordBox and FWNumberBox" => "<FWPasswordBox RevealMode=\"Visible\" PlaceholderText=\"Password\" />\n<FWNumberBox Minimum=\"0\" Maximum=\"100\" Value=\"42\" SpinButtonPlacementMode=\"Inline\" />",
            "FWAutoCompleteBox" => "<FWAutoCompleteBox ItemsSource=\"{Binding SearchItems}\" FilterMode=\"Contains\" MinimumPrefixLength=\"1\" />",
            "FWAutoSuggestBox" => "<FWAutoSuggestBox ItemsSource=\"{Binding SearchItems}\"\n                  Text=\"Auto\"\n                  FilterMode=\"Contains\"\n                  MinimumPrefixLength=\"1\"\n                  Density=\"Comfortable\" />",
            "FWRichTextBox" => "<FWRichTextBox AcceptsTab=\"True\" IsSpellCheckEnabled=\"True\" />",
            "Material input panel" => "<FWFluentMaterialSurface MaterialKind=\"LiquidGlass\">\n    <FWAutoSuggestBox PlaceholderText=\"Search FluentJalium\" />\n</FWFluentMaterialSurface>",
            _ => "<FWTextBox />"
        };
    }

    private static FWWrapPanel CreateTextInputButtonRow(params FWButton[] buttons)
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

    private static FWButton CreateTextInputActionButton(FluentIconRegular icon, string text, Action action)
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

    private static TextBlock CreateTextInputOutput(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        };
    }

    private static FWBorder CreateTextInputStatus(TextBlock status)
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

    private static FWNumberBoxDensity NextDensity(FWNumberBoxDensity density)
    {
        return density switch
        {
            FWNumberBoxDensity.Compact => FWNumberBoxDensity.Comfortable,
            FWNumberBoxDensity.Comfortable => FWNumberBoxDensity.Spacious,
            _ => FWNumberBoxDensity.Compact
        };
    }

    private static string FormatDensity(FWNumberBoxDensity density)
    {
        return density switch
        {
            FWNumberBoxDensity.Compact => "compact",
            FWNumberBoxDensity.Spacious => "spacious",
            _ => "comfortable"
        };
    }

    private static FWTextInputDensity NextDensity(FWTextInputDensity density)
    {
        return density switch
        {
            FWTextInputDensity.Compact => FWTextInputDensity.Comfortable,
            FWTextInputDensity.Comfortable => FWTextInputDensity.Spacious,
            _ => FWTextInputDensity.Compact
        };
    }

    private static string FormatDensity(FWTextInputDensity density)
    {
        return density switch
        {
            FWTextInputDensity.Compact => "compact",
            FWTextInputDensity.Spacious => "spacious",
            _ => "comfortable"
        };
    }

    private static NumberBoxSpinButtonPlacementMode NextSpinPlacement(NumberBoxSpinButtonPlacementMode mode)
    {
        return mode switch
        {
            NumberBoxSpinButtonPlacementMode.Inline => NumberBoxSpinButtonPlacementMode.Hidden,
            NumberBoxSpinButtonPlacementMode.Hidden => NumberBoxSpinButtonPlacementMode.Compact,
            _ => NumberBoxSpinButtonPlacementMode.Inline
        };
    }

    private static string FormatSpinPlacement(NumberBoxSpinButtonPlacementMode mode)
    {
        return mode switch
        {
            NumberBoxSpinButtonPlacementMode.Hidden => "hidden",
            NumberBoxSpinButtonPlacementMode.Compact => "compact",
            _ => "inline"
        };
    }

    private static string FormatNumberBoxValue(double value, int decimalPlaces)
    {
        return decimalPlaces >= 0
            ? value.ToString($"F{decimalPlaces}", CultureInfo.CurrentCulture)
            : value.ToString("G", CultureInfo.CurrentCulture);
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
                        CreateIcon(FluentIconRegular.Textbox24, 24, ThemeBrush("TextPrimary")),
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

    private static readonly string[] SearchItems =
    [
        "Fluent tokens",
        "Fluent controls",
        "WinUI Gallery",
        "Community Toolkit",
        "TextBox",
        "PasswordBox",
        "NumberBox",
        "AutoCompleteBox",
        "AutoSuggestBox",
        "RichTextBox"
    ];
}
