using FluentJalium.Controls.Themes;
using FluentJalium.Gallery.Controls;
using FluentJalium.Gallery.Resources;
using FluentJalium.Gallery.Services;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Media;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWButton = FluentJalium.Controls.FWButton;
using FWComboBox = FluentJalium.Controls.FWComboBox;
using FWSettingsCard = FluentJalium.Controls.FWSettingsCard;
using FWSettingsExpander = FluentJalium.Controls.FWSettingsExpander;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWToggleSwitch = FluentJalium.Controls.FWToggleSwitch;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;

namespace FluentJalium.Gallery.Pages;

internal sealed class GallerySettingsPage
{
    internal readonly record struct GallerySettingsVisualQaSnapshot(
        int CardCount,
        int ClickableCount,
        int DisabledCount,
        int ExpanderItemCount,
        int ExpandedCount,
        bool HasIconColumn,
        bool HasActionColumn,
        bool HasItemHostRows,
        bool HasAutomationName,
        bool HasAutomationHelpText,
        bool CanInvokeClickableRow,
        int DenseRowCount)
    {
        public bool IsSettingsVisualQaReady => DenseRowCount >= 6 &&
            ClickableCount > 0 &&
            DisabledCount > 0 &&
            ExpanderItemCount > 0 &&
            ExpandedCount > 0 &&
            HasIconColumn &&
            HasActionColumn &&
            HasItemHostRows &&
            HasAutomationName &&
            HasAutomationHelpText &&
            CanInvokeClickableRow;
    }

    private readonly Action<FluentThemeVariant> _applyTheme;
    private readonly Action<Color> _applyAccent;
    private readonly LocalizationService _localization;
    private TextBlock? _themeOutput;
    private TextBlock? _accentOutput;
    private TextBlock? _languageOutput;

    public GallerySettingsPage(Action<FluentThemeVariant> applyTheme, Action<Color> applyAccent)
    {
        _applyTheme = applyTheme;
        _applyAccent = applyAccent;
        _localization = LocalizationService.Instance;

        // Subscribe to localization changes to refresh UI
        _localization.PropertyChanged += (_, _) => RefreshLocalizedContent();
    }

    public UIElement CreateContent()
    {
        var panel = CreateSection(Strings.Settings_Title);
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        // Theme mode card
        examples.Children.Add(CreateSettingsCard(
            FluentIconRegular.DarkTheme24,
            Strings.Settings_Theme,
            Strings.Settings_ThemeDescription,
            CreateThemeModeSample(),
            "<FWNavigationView.FooterMenuItems>\n    <FWNavigationViewItem Content=\"Settings\" Icon=\"Settings\" />\n</FWNavigationView.FooterMenuItems>"));

        // Accent color card
        examples.Children.Add(CreateSettingsCard(
            FluentIconRegular.Color24,
            Strings.Settings_Accent,
            Strings.Settings_AccentDescription,
            CreateAccentSample(),
            "FluentThemeManager.ApplyAccent(application, Color.FromRgb(0x00, 0x78, 0xD4));"));

        // Language selection card
        examples.Children.Add(CreateSettingsCard(
            FluentIconRegular.LocalLanguage24,
            Strings.Settings_Language,
            Strings.Settings_LanguageDescription,
            CreateLanguageSample(),
            "LocalizationService.Instance.ChangeLanguage(\"zh-CN\");"));

        // Gallery diagnostics card
        examples.Children.Add(CreateSettingsCard(
            FluentIconRegular.DataUsage24,
            Strings.Settings_Diagnostics,
            Strings.Settings_DiagnosticsDescription,
            CreateDiagnosticsSample(),
            "GalleryCatalog.CreatePageInfos()\n    .Where(page => page.IsFooter || page.Status != GalleryPageStatus.Stable);"));
        examples.Children.Add(CreateSettingsCard(
            FluentIconRegular.TextBoxSettings24,
            "Settings visual QA",
            "Dense settings rows with icons, actions, disabled state, clickable row, automation text, and SettingsExpander item-host rows.",
            CreateSettingsControlsVisualQaSample(),
            """
var cards = new[] { appThemeCard, launchCard, disabledPolicyCard };
var snapshot = GallerySettingsPage.CreateSettingsVisualQaSnapshot(cards, advancedExpander);
Debug.WriteLine(GallerySettingsPage.FormatSettingsVisualQa("Settings QA", snapshot));
"""));

        panel.Children.Add(examples);
        return panel;
    }

    private void RefreshLocalizedContent()
    {
        // Update status outputs with new language
        if (_themeOutput != null)
        {
            _themeOutput.Text = string.Format(Strings.Status_Theme, FluentThemeManager.CurrentTheme);
        }

        if (_accentOutput != null)
        {
            var color = FluentThemeManager.CurrentAccentColor;
            _accentOutput.Text = string.Format(Strings.Status_Accent, $"{color.R:X2}{color.G:X2}{color.B:X2}");
        }

        if (_languageOutput != null)
        {
            _languageOutput.Text = string.Format(Strings.Status_Language, _localization.CurrentCulture.DisplayName);
        }
    }

    private UIElement CreateThemeModeSample()
    {
        _themeOutput = CreateOutput(string.Format(Strings.Status_Theme, FluentThemeManager.CurrentTheme));

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                CreateButtonRow(
                    CreateActionButton(FluentIconRegular.WeatherSunny24, Strings.Settings_ThemeLight, () =>
                    {
                        _applyTheme(FluentThemeVariant.Light);
                        _themeOutput.Text = string.Format(Strings.Status_Theme, "Light");
                    }),
                    CreateActionButton(FluentIconRegular.DarkTheme24, Strings.Settings_ThemeDark, () =>
                    {
                        _applyTheme(FluentThemeVariant.Dark);
                        _themeOutput.Text = string.Format(Strings.Status_Theme, "Dark");
                    }),
                    CreateActionButton(FluentIconRegular.Accessibility24, Strings.Settings_ThemeHighContrast, () =>
                    {
                        _applyTheme(FluentThemeVariant.HighContrast);
                        _themeOutput.Text = string.Format(Strings.Status_Theme, "HighContrast");
                    })),
                CreateStatus(_themeOutput)
            }
        };
    }

    private UIElement CreateAccentSample()
    {
        var currentColor = FluentThemeManager.CurrentAccentColor;
        _accentOutput = CreateOutput(string.Format(Strings.Status_Accent, $"{currentColor.R:X2}{currentColor.G:X2}{currentColor.B:X2}"));

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                new FWWrapPanel
                {
                    HorizontalSpacing = 10,
                    VerticalSpacing = 10,
                    Children =
                    {
                        CreateAccentButton(Strings.Color_Blue, Color.FromRgb(0x00, 0x78, 0xD4), _accentOutput),
                        CreateAccentButton(Strings.Color_Rose, Color.FromRgb(0xC2, 0x39, 0xB3), _accentOutput),
                        CreateAccentButton(Strings.Color_Orange, Color.FromRgb(0xD8, 0x3B, 0x01), _accentOutput),
                        CreateAccentButton(Strings.Color_Green, Color.FromRgb(0x10, 0x7C, 0x10), _accentOutput)
                    }
                },
                CreateStatus(_accentOutput)
            }
        };
    }

    private UIElement CreateLanguageSample()
    {
        _languageOutput = CreateOutput(string.Format(Strings.Status_Language, _localization.CurrentCulture.DisplayName));

        var comboBox = new FWComboBox
        {
            MinWidth = 280,
            SelectedIndex = 0
        };

        // Add language items
        var currentCultureName = _localization.CurrentCulture.Name;
        var selectedIndex = 0;

        for (var i = 0; i < _localization.SupportedLanguages.Count; i++)
        {
            var lang = _localization.SupportedLanguages[i];
            var item = new ComboBoxItem
            {
                Content = new FWStackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 8,
                    Children =
                    {
                        new FWTextBlock
                        {
                            Text = lang.Flag,
                            FontSize = 16,
                            VerticalAlignment = VerticalAlignment.Center
                        },
                        new FWTextBlock
                        {
                            Text = lang.DisplayName,
                            VerticalAlignment = VerticalAlignment.Center
                        }
                    }
                },
                Tag = lang.CultureName
            };

            if (lang.CultureName == currentCultureName)
            {
                selectedIndex = i;
            }

            comboBox.Items.Add(item);
        }

        comboBox.SelectedIndex = selectedIndex;
        comboBox.SelectionChanged += (_, _) =>
        {
            if (comboBox.SelectedItem is ComboBoxItem item && item.Tag is string cultureName)
            {
                _localization.ChangeLanguage(cultureName);
                var selectedLang = _localization.SupportedLanguages.FirstOrDefault(l => l.CultureName == cultureName);
                if (selectedLang != null && _languageOutput != null)
                {
                    _languageOutput.Text = string.Format(Strings.Status_Language, selectedLang.DisplayName);
                }
            }
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                comboBox,
                CreateStatus(_languageOutput)
            }
        };
    }

    private static UIElement CreateDiagnosticsSample()
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                CreateDiagnosticRow(FluentIconRegular.Navigation24, Strings.Diag_Shell, Strings.Diag_ShellDesc),
                CreateDiagnosticRow(FluentIconRegular.DatabaseSearch24, Strings.Diag_Catalog, Strings.Diag_CatalogDesc),
                CreateDiagnosticRow(FluentIconRegular.DataUsage24, Strings.Diag_StateMatrix, Strings.Diag_StateMatrixDesc),
                new FWToggleSwitch
                {
                    Header = Strings.Diag_ShowMetadata,
                    IsOn = true
                }
            }
        };
    }

    private static UIElement CreateSettingsControlsVisualQaSample()
    {
        var output = CreateOutput("Settings visual QA: waiting for dense settings evidence.");
        var appThemeCard = CreateVisualQaSettingsCard(
            FluentIconRegular.DarkTheme24,
            "App theme",
            "Follow system setting and keep long descriptions wrapped.",
            new FWToggleSwitch { IsOn = true, OnContent = "System", OffContent = "Manual" },
            actionIcon: FluentIconRegular.ChevronRight24,
            isClickEnabled: true);
        var launchCard = CreateVisualQaSettingsCard(
            FluentIconRegular.Rocket24,
            "Launch behavior",
            "Open advanced startup options.",
            new FWButton { Content = "Configure" },
            actionIcon: FluentIconRegular.Open24,
            isClickEnabled: true);
        var syncCard = CreateVisualQaSettingsCard(
            FluentIconRegular.CloudSync24,
            "Sync settings",
            "Use hover click mode for a compact settings action.",
            new FWToggleSwitch { IsOn = true, OnContent = "On", OffContent = "Off" },
            actionIcon: FluentIconRegular.AccessibilityCheckmark24,
            isClickEnabled: true,
            clickMode: ClickMode.Hover);
        var densityCard = CreateVisualQaSettingsCard(
            FluentIconRegular.TextDensity24,
            "Control density",
            "Comfortable rows preserve icon and action alignment.",
            new FWComboBox
            {
                MinWidth = 120,
                SelectedIndex = 0,
                Items = { "Comfortable", "Compact" }
            },
            actionIcon: FluentIconRegular.ChevronRight24);
        var disabledPolicyCard = CreateVisualQaSettingsCard(
            FluentIconRegular.ShieldDismiss24,
            "Enterprise policy",
            "Disabled rows keep text, icon, and action affordances aligned.",
            new FWTextBlock { Text = "Managed", Foreground = ThemeBrush("TextSecondary") },
            actionIcon: FluentIconRegular.AlertOff24,
            isClickEnabled: true,
            isEnabled: false);
        var previewCard = CreateVisualQaSettingsCard(
            FluentIconRegular.WindowWrench24,
            "Preview channel",
            "Shows a long metadata row inside the dense settings page.",
            new FWButton { Content = "Preview" },
            actionIcon: FluentIconRegular.ChevronRight24);
        var expander = new FWSettingsExpander
        {
            Header = "Advanced settings",
            Description = "SettingsExpander item-host rows use SettingsCard defaults.",
            HeaderIcon = CreateIcon(FluentIconRegular.Settings24, 20, ThemeBrush("TextPrimary")),
            IsExpanded = true
        };
        expander.AddSetting(CreateVisualQaSettingsCard(
            FluentIconRegular.Color24,
            "Accent color",
            "Preserve action alignment when rows are hosted by an expander.",
            new FWTextBlock { Text = "Blue", Foreground = ThemeBrush("TextSecondary") },
            actionIcon: FluentIconRegular.ChevronRight24,
            isClickEnabled: true));
        expander.AddSetting(CreateVisualQaSettingsCard(
            FluentIconRegular.LocalLanguage24,
            "Language",
            "Hosted row with secondary text.",
            new FWTextBlock { Text = "System", Foreground = ThemeBrush("TextSecondary") },
            actionIcon: FluentIconRegular.ChevronRight24));
        expander.AddSetting(CreateVisualQaSettingsCard(
            FluentIconRegular.DataUsage24,
            "Diagnostics",
            "Hosted row for Gallery metadata.",
            new FWButton { Content = "View" },
            actionIcon: FluentIconRegular.Open24,
            isClickEnabled: true));

        var cards = new[]
        {
            appThemeCard,
            launchCard,
            syncCard,
            densityCard,
            disabledPolicyCard,
            previewCard
        };

        void RefreshQa(string action)
        {
            output.Text = FormatSettingsVisualQa(action, CreateSettingsVisualQaSnapshot(cards, expander));
        }

        appThemeCard.Click += (_, _) => RefreshQa("App theme row invoked");
        launchCard.Click += (_, _) => RefreshQa("Launch row invoked");
        syncCard.Click += (_, _) => RefreshQa("Sync row invoked");
        expander.Expanded += (_, _) => RefreshQa("SettingsExpander expanded");
        expander.Collapsed += (_, _) => RefreshQa("SettingsExpander collapsed");
        RefreshQa("Settings QA initialized");

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Width = 560,
            Children =
            {
                appThemeCard,
                launchCard,
                syncCard,
                densityCard,
                disabledPolicyCard,
                previewCard,
                expander,
                CreateButtonRow(
                    CreateActionButton(FluentIconRegular.DataUsage24, "Refresh QA", () => RefreshQa("Settings QA refreshed")),
                    CreateActionButton(FluentIconRegular.PanelLeftContract24, "Toggle expander", () => expander.IsExpanded = !expander.IsExpanded),
                    CreateActionButton(FluentIconRegular.ShieldDismiss24, "Policy", () =>
                    {
                        disabledPolicyCard.IsEnabled = !disabledPolicyCard.IsEnabled;
                        RefreshQa("Policy row state toggled");
                    })),
                CreateStatus(output)
            }
        };
    }

    internal static GallerySettingsVisualQaSnapshot CreateSettingsVisualQaSnapshot(
        IReadOnlyList<FWSettingsCard> cards,
        FWSettingsExpander expander)
    {
        ArgumentNullException.ThrowIfNull(cards);
        ArgumentNullException.ThrowIfNull(expander);

        var diagnostics = cards.Select(card => card.GetDiagnostics()).ToArray();
        var automation = cards.Select(card => card.GetAutomationDiagnostics()).ToArray();
        return new GallerySettingsVisualQaSnapshot(
            cards.Count,
            diagnostics.Count(item => item.IsClickEnabled),
            diagnostics.Count(item => !item.IsEnabled),
            expander.ItemCount,
            expander.IsExpanded ? 1 : 0,
            cards.Any(card => card.HeaderIcon != null),
            cards.Any(card => card.ActionIcon != null),
            expander.ItemCount > 0,
            automation.Any(item => !string.IsNullOrWhiteSpace(item.Name)),
            automation.Any(item => !string.IsNullOrWhiteSpace(item.HelpText)),
            diagnostics.Any(item => item.IsInvokable),
            cards.Count + expander.ItemCount);
    }

    internal static string FormatSettingsVisualQa(string action, GallerySettingsVisualQaSnapshot snapshot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);

        return $"{action}. Settings QA. Cards: {snapshot.CardCount}. Clickable: {snapshot.ClickableCount}. Disabled: {snapshot.DisabledCount}. Expander items: {snapshot.ExpanderItemCount}. Expanded: {snapshot.ExpandedCount}. Icon/action: {FormatOnOff(snapshot.HasIconColumn)}/{FormatOnOff(snapshot.HasActionColumn)}. Item host: {FormatOnOff(snapshot.HasItemHostRows)}. Automation: {FormatOnOff(snapshot.HasAutomationName)}/{FormatOnOff(snapshot.HasAutomationHelpText)}. Invoke: {FormatOnOff(snapshot.CanInvokeClickableRow)}. Dense rows: {snapshot.DenseRowCount}. Ready: {FormatOnOff(snapshot.IsSettingsVisualQaReady)}.";
    }

    private FWButton CreateAccentButton(string label, Color color, TextBlock output)
    {
        var button = new FWButton
        {
            Content = new FWStackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children =
                {
                    new FWBorder
                    {
                        Width = 16,
                        Height = 16,
                        CornerRadius = new CornerRadius(8),
                        Background = new SolidColorBrush(color),
                        BorderBrush = ThemeBrush("ControlBorder"),
                        BorderThickness = new Thickness(1)
                    },
                    new FWTextBlock
                    {
                        Text = label,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                }
            }
        };
        button.Click += (_, _) =>
        {
            _applyAccent(color);
            output.Text = string.Format(Strings.Status_Accent, $"{color.R:X2}{color.G:X2}{color.B:X2}");
        };
        return button;
    }

    private static FWSettingsCard CreateVisualQaSettingsCard(
        FluentIconRegular icon,
        string header,
        string description,
        UIElement content,
        FluentIconRegular? actionIcon = null,
        bool isClickEnabled = false,
        bool isEnabled = true,
        ClickMode clickMode = ClickMode.Release)
    {
        return new FWSettingsCard
        {
            Header = header,
            Description = description,
            HeaderIcon = CreateIcon(icon, 20, ThemeBrush("TextSecondary")),
            ActionIcon = actionIcon.HasValue ? CreateIcon(actionIcon.Value, 16, ThemeBrush("TextSecondary")) : null,
            Content = content,
            IsClickEnabled = isClickEnabled,
            IsEnabled = isEnabled,
            ClickMode = clickMode
        };
    }

    private static FWBorder CreateDiagnosticRow(FluentIconRegular icon, string title, string description)
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
                Spacing = 10,
                Children =
                {
                    CreateIcon(icon, 18, ThemeBrush("TextPrimary")),
                    new FWStackPanel
                    {
                        Orientation = Orientation.Vertical,
                        Spacing = 3,
                        Children =
                        {
                            new FWTextBlock
                            {
                                Text = title,
                                FontSize = 13,
                                Foreground = ThemeBrush("TextPrimary")
                            },
                            new FWTextBlock
                            {
                                Text = description,
                                FontSize = 12,
                                Foreground = ThemeBrush("TextSecondary"),
                                TextWrapping = TextWrapping.Wrap
                            }
                        }
                    }
                }
            }
        };
    }

    private static FWBorder CreateSettingsCard(FluentIconRegular icon, string title, string description, UIElement content, string code)
    {
        return GallerySampleCard.Create(icon, title, description, content, code: code);
    }

    private static FWWrapPanel CreateButtonRow(params FWButton[] buttons)
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

    private static FWButton CreateActionButton(FluentIconRegular icon, string text, Action action)
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

    private static TextBlock CreateOutput(string text)
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
                        CreateIcon(FluentIconRegular.Settings24, 24, ThemeBrush("TextPrimary")),
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

    private static string FormatOnOff(bool value)
    {
        return value ? "on" : "off";
    }
}
