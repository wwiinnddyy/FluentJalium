using FluentJalium.Gallery.Controls;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using FWAutoSuggestBox = FluentJalium.Controls.FWAutoSuggestBox;
using FWAutoSuggestBoxTextChangeReason = FluentJalium.Controls.FWAutoSuggestBoxTextChangeReason;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWButton = FluentJalium.Controls.FWButton;
using FWCheckBox = FluentJalium.Controls.FWCheckBox;
using FWInfoBar = FluentJalium.Controls.FWInfoBar;
using FWLabel = FluentJalium.Controls.FWLabel;
using FWRadioButtons = FluentJalium.Controls.FWRadioButtons;
using FWSelectionDensity = FluentJalium.Controls.FWSelectionDensity;
using FWSettingsCard = FluentJalium.Controls.FWSettingsCard;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWSwitchDensity = FluentJalium.Controls.FWSwitchDensity;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWTextBox = FluentJalium.Controls.FWTextBox;
using FWTextInputDensity = FluentJalium.Controls.FWTextInputDensity;
using FWToggleSwitch = FluentJalium.Controls.FWToggleSwitch;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;
using ICommand = System.Windows.Input.ICommand;

namespace FluentJalium.Gallery.Pages;

internal sealed class GalleryFormsPage
{
    public UIElement CreateContent()
    {
        var panel = CreateSection("Forms");
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        examples.Children.Add(CreateFormsExampleCard(
            FluentIconRegular.FormNew24,
            "Profile form",
            "Label targets, required fields, AutoSuggestBox suggestions, grouped radio choices, validation messages, and submit/reset commands.",
            CreateProfileFormSample(),
            CreateSampleCode("Profile form"),
            width: 680));
        examples.Children.Add(CreateFormsExampleCard(
            FluentIconRegular.FormMultiple24,
            "Submission settings",
            "SettingsCard rows coordinate switches, optional fields, compact density, command diagnostics, and a form summary InfoBar.",
            CreateSubmissionSettingsSample(),
            CreateSampleCode("Submission settings"),
            width: 680));

        panel.Children.Add(examples);
        return panel;
    }

    private static UIElement CreateProfileFormSample()
    {
        var output = CreateFormsOutput("Profile form ready. Validation has not run yet.");
        var displayName = new FWTextBox
        {
            Text = "Rhea Holloway",
            Width = 260,
            PlaceholderText = "Full name",
            Density = FWTextInputDensity.Comfortable
        };
        var email = new FWTextBox
        {
            Text = "rhea@contoso.com",
            Width = 260,
            PlaceholderText = "name@example.com",
            Density = FWTextInputDensity.Comfortable
        };
        var team = new FWAutoSuggestBox
        {
            Text = "Design Systems",
            Width = 260,
            ItemsSource = TeamSuggestions,
            PlaceholderText = "Search teams",
            FilterMode = AutoCompleteFilterMode.Contains,
            MinimumPrefixLength = 1,
            Density = FWTextInputDensity.Comfortable
        };
        var accountType = new FWRadioButtons
        {
            Header = "Account type",
            Width = 260,
            Density = FWSelectionDensity.Comfortable
        };
        accountType.Items.Add("Member");
        accountType.Items.Add("Maintainer");
        accountType.Items.Add("Guest");
        accountType.SelectedIndex = 0;

        var validation = new FWInfoBar
        {
            Title = "Ready",
            Message = "The profile has enough information to submit.",
            Severity = InfoBarSeverity.Success,
            IsOpen = true,
            IsClosable = false,
            Width = 566
        };

        void ApplyValidation(string reason)
        {
            var issues = ValidateProfile(displayName.Text, email.Text, team.Text, accountType.SelectedItem);
            if (issues.Length == 0)
            {
                validation.Title = "Ready";
                validation.Message = "The profile has enough information to submit.";
                validation.Severity = InfoBarSeverity.Success;
                output.Text = $"{reason}: valid. Account: {accountType.SelectedItem}. Team: {team.Text}.";
                return;
            }

            validation.Title = "Review required";
            validation.Message = string.Join(" ", issues);
            validation.Severity = issues.Length > 1 ? InfoBarSeverity.Error : InfoBarSeverity.Warning;
            output.Text = $"{reason}: {issues.Length} issue(s). {validation.Message}";
        }

        displayName.TextChanged += (_, _) => ApplyValidation("Name changed");
        email.TextChanged += (_, _) => ApplyValidation("Email changed");
        team.TextChanged += (_, _) => ApplyValidation("Team changed");
        team.SuggestionChosen += (_, args) =>
        {
            output.Text = $"Suggestion chosen: {args.SelectedItem}.";
            ApplyValidation("Suggestion");
        };
        team.QuerySubmitted += (_, args) =>
        {
            output.Text = $"Submitted team query: {args.QueryText}.";
            ApplyValidation("Query submitted");
        };
        accountType.SelectionChanged += (_, _) => ApplyValidation("Account type changed");

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Children =
            {
                new FWWrapPanel
                {
                    HorizontalSpacing = 16,
                    VerticalSpacing = 12,
                    Children =
                    {
                        CreateField("Display name", displayName, 'D'),
                        CreateField("Email", email, 'E'),
                        CreateField("Team", team, 'T'),
                        accountType
                    }
                },
                validation,
                CreateFormsButtonRow(
                    CreateFormsActionButton(FluentIconRegular.CheckmarkCircle24, "Validate", () => ApplyValidation("Validate")),
                    CreateFormsActionButton(FluentIconRegular.Send24, "Submit", () =>
                    {
                        ApplyValidation("Submit");
                        if (validation.Severity == InfoBarSeverity.Success)
                        {
                            output.Text = $"Submitted profile for {displayName.Text} as {accountType.SelectedItem}.";
                        }
                    }),
                    CreateFormsActionButton(FluentIconRegular.DismissCircle24, "Reset", () =>
                    {
                        displayName.Text = string.Empty;
                        email.Text = string.Empty;
                        team.SetQueryText(string.Empty, FWAutoSuggestBoxTextChangeReason.ProgrammaticChange);
                        accountType.SelectedIndex = 0;
                        ApplyValidation("Reset");
                    })),
                CreateFormsStatus(output)
            }
        };
    }

    private static UIElement CreateSubmissionSettingsSample()
    {
        var output = CreateFormsOutput("Submission settings ready. Approval on, confirmation on, compact density off.");
        var summary = new FWInfoBar
        {
            Title = "Submission workflow",
            Message = "Approval and confirmation are enabled.",
            Severity = InfoBarSeverity.Informational,
            IsOpen = true,
            IsClosable = false,
            Width = 566
        };
        var approver = new FWAutoSuggestBox
        {
            Width = 260,
            Text = "Ada",
            ItemsSource = ReviewerSuggestions,
            PlaceholderText = "Approver",
            FilterMode = AutoCompleteFilterMode.Contains,
            MinimumPrefixLength = 1,
            Density = FWTextInputDensity.Comfortable
        };
        var note = new FWTextBox
        {
            Width = 260,
            Text = "Notify the reviewer before publishing.",
            PlaceholderText = "Reviewer note",
            Density = FWTextInputDensity.Comfortable
        };
        var requireApproval = new FWToggleSwitch
        {
            IsOn = true,
            OnContent = "Required",
            OffContent = "Optional",
            Density = FWSwitchDensity.Comfortable
        };
        var sendConfirmation = new FWToggleSwitch
        {
            IsOn = true,
            OnContent = "Send",
            OffContent = "Skip",
            Density = FWSwitchDensity.Comfortable
        };
        var compactDensity = new FWCheckBox
        {
            Content = "Compact input density",
            IsChecked = false
        };

        var submitCommand = new GalleryFormsCommand(parameter =>
        {
            summary.Severity = InfoBarSeverity.Success;
            summary.Title = "Submit command invoked";
            summary.Message = $"Parameter: {parameter}. Approver: {approver.Text}.";
            output.Text = $"SettingsCard command invoked. Approval {FormatOnOff(requireApproval.IsOn)}, confirmation {FormatOnOff(sendConfirmation.IsOn)}, compact {FormatOnOff(compactDensity.IsChecked == true)}.";
        });
        var submitCard = new FWSettingsCard
        {
            Header = "Submit action",
            Description = "Click the settings row to invoke the submit command and inspect command diagnostics.",
            HeaderIcon = CreateIcon(FluentIconRegular.Send24, 20, ThemeBrush("TextPrimary")),
            ActionIcon = CreateIcon(FluentIconRegular.Send24, 18, ThemeBrush("TextSecondary")),
            Content = new FWTextBlock
            {
                Text = "Invoke",
                Foreground = ThemeBrush("TextSecondary")
            },
            IsClickEnabled = true,
            Command = submitCommand,
            CommandParameter = "forms.submit"
        };

        void RefreshSummary(string reason)
        {
            var approval = requireApproval.IsOn ? "approval required" : "approval optional";
            var confirmation = sendConfirmation.IsOn ? "confirmation on" : "confirmation off";
            var density = compactDensity.IsChecked == true ? FWTextInputDensity.Compact : FWTextInputDensity.Comfortable;
            approver.Density = density;
            note.Density = density;
            summary.Severity = requireApproval.IsOn && string.IsNullOrWhiteSpace(approver.Text)
                ? InfoBarSeverity.Warning
                : InfoBarSeverity.Informational;
            summary.Title = summary.Severity == InfoBarSeverity.Warning ? "Approver needed" : "Submission workflow";
            summary.Message = $"{approval}, {confirmation}, {FormatDensity(density)} density.";

            var diagnostics = submitCard.GetDiagnostics();
            output.Text = $"{reason}: {summary.Message} Can execute {FormatOnOff(diagnostics.CanExecute)}. Invokable {FormatOnOff(diagnostics.IsInvokable)}.";
        }

        requireApproval.Toggled += (_, _) => RefreshSummary("Approval changed");
        sendConfirmation.Toggled += (_, _) => RefreshSummary("Confirmation changed");
        compactDensity.Checked += (_, _) => RefreshSummary("Density changed");
        compactDensity.Unchecked += (_, _) => RefreshSummary("Density changed");
        approver.TextChanged += (_, _) => RefreshSummary("Approver changed");
        note.TextChanged += (_, _) => RefreshSummary("Note changed");

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Children =
            {
                new FWWrapPanel
                {
                    HorizontalSpacing = 12,
                    VerticalSpacing = 12,
                    Children =
                    {
                        CreateSettingsRow(FluentIconRegular.Shield24, "Approval", "Route submissions through a named reviewer.", requireApproval),
                        CreateSettingsRow(FluentIconRegular.Mail24, "Confirmation", "Send a receipt after submit.", sendConfirmation),
                        CreateSettingsRow(FluentIconRegular.TextDensity24, "Density", "Apply compact density to form fields.", compactDensity)
                    }
                },
                new FWWrapPanel
                {
                    HorizontalSpacing = 16,
                    VerticalSpacing = 12,
                    Children =
                    {
                        CreateField("Approver", approver, 'A'),
                        CreateField("Review note", note, 'R')
                    }
                },
                submitCard,
                summary,
                CreateFormsStatus(output)
            }
        };
    }

    private static FWBorder CreateSettingsRow(FluentIconRegular icon, string header, string description, UIElement content)
    {
        return new FWBorder
        {
            Width = 274,
            Child = new FWSettingsCard
            {
                Header = header,
                Description = description,
                HeaderIcon = CreateIcon(icon, 20, ThemeBrush("TextPrimary")),
                Content = content
            }
        };
    }

    private static FWStackPanel CreateField(string labelText, UIElement target, char accessKey)
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 5,
            Children =
            {
                new FWLabel
                {
                    Content = labelText,
                    Target = target,
                    AccessKey = accessKey
                },
                target
            }
        };
    }

    private static string[] ValidateProfile(string? displayName, string? email, string? team, object? accountType)
    {
        var issues = new List<string>();
        if (string.IsNullOrWhiteSpace(displayName))
        {
            issues.Add("Display name is required.");
        }

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@', StringComparison.Ordinal))
        {
            issues.Add("Email must include an @ sign.");
        }

        if (string.IsNullOrWhiteSpace(team))
        {
            issues.Add("Choose or enter a team.");
        }

        if (accountType == null)
        {
            issues.Add("Select an account type.");
        }

        return issues.ToArray();
    }

    private static FWBorder CreateFormsExampleCard(FluentIconRegular icon, string title, string description, UIElement content, string code, double width)
    {
        return GallerySampleCard.Create(icon, title, description, content, code: code, width: width);
    }

    private static string CreateSampleCode(string title)
    {
        return title switch
        {
            "Profile form" => """
var nameBox = new FWTextBox { PlaceholderText = "Full name" };
var nameLabel = new FWLabel { Content = "Display name", Target = nameBox };
var teamBox = new FWAutoSuggestBox { ItemsSource = teams, FilterMode = AutoCompleteFilterMode.Contains };
var accountType = new FWRadioButtons { Header = "Account type", SelectedIndex = 0 };
var validation = new FWInfoBar { Severity = InfoBarSeverity.Warning, IsOpen = true };
""",
            "Submission settings" => """
var submitCard = new FWSettingsCard
{
    Header = "Submit action",
    Description = "Invoke the submit command from a settings row.",
    Content = new FWToggleSwitch { IsOn = true },
    IsClickEnabled = true,
    Command = SubmitCommand,
    CommandParameter = "forms.submit"
};

var diagnostics = submitCard.GetDiagnostics();
""",
            _ => "var form = new FWStackPanel();"
        };
    }

    private static FWWrapPanel CreateFormsButtonRow(params FWButton[] buttons)
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

    private static FWButton CreateFormsActionButton(FluentIconRegular icon, string text, Action action)
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

    private static TextBlock CreateFormsOutput(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        };
    }

    private static FWBorder CreateFormsStatus(TextBlock status)
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
                        CreateIcon(FluentIconRegular.FormMultiple24, 24, ThemeBrush("TextPrimary")),
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

    private static string FormatDensity(FWTextInputDensity density)
    {
        return density switch
        {
            FWTextInputDensity.Compact => "compact",
            FWTextInputDensity.Spacious => "spacious",
            _ => "comfortable"
        };
    }

    private static string FormatOnOff(bool value)
    {
        return value ? "on" : "off";
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

    private sealed class GalleryFormsCommand : ICommand
    {
        private readonly Action<object?> _execute;

        public GalleryFormsCommand(Action<object?> execute)
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

    private static readonly string[] TeamSuggestions =
    [
        "Design Systems",
        "Platform Engineering",
        "Gallery Operations",
        "Accessibility Review",
        "Release Readiness",
        "Documentation"
    ];

    private static readonly string[] ReviewerSuggestions =
    [
        "Ada",
        "Grace",
        "Katherine",
        "Margaret",
        "Rhea"
    ];
}
