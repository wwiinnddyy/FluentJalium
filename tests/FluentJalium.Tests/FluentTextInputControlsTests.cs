using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using FluentJalium.Gallery.Pages;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Controls.Themes;
using Jalium.UI.Input;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using JaliumThemeManager = Jalium.UI.Controls.Themes.ThemeManager;

namespace FluentJalium.Tests;

[Collection("Application")]
public sealed class FluentTextInputControlsTests
{
    private static readonly string[] TextInputResourceKeys =
    [
        "TextControlBackground",
        "TextControlBackgroundHover",
        "TextControlBackgroundFocused",
        "TextControlBackgroundDisabled",
        "TextControlBorder",
        "TextControlBorderHover",
        "TextControlBorderFocused",
        "TextControlBorderDisabled",
        "TextControlFlyoutBackground",
        "TextControlFlyoutBorder",
        "TextPrimary",
        "TextDisabled",
        "SelectionBackground"
    ];

    [Theory]
    [InlineData(FluentThemeVariant.Dark)]
    [InlineData(FluentThemeVariant.Light)]
    [InlineData(FluentThemeVariant.HighContrast)]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void ThemeDictionary_ShouldExposeTextInputTokensForEveryTheme(FluentThemeVariant theme)
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = theme.ToString();

        var dictionary = LoadGenericThemeDictionary();

        foreach (var key in TextInputResourceKeys)
        {
            Assert.True(dictionary.TryGetValue(key, out var value), $"{key} was not found in the {theme} theme.");
            Assert.IsType<SolidColorBrush>(value);
        }

        ResetApplicationState();
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void Apply_ShouldRegisterFwTextInputStylesBasedOnJaliumStyles()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            AssertBasedOnStyle<FWTextBox, TextBox>(app.Resources);
            AssertBasedOnStyle<FWPasswordBox, PasswordBox>(app.Resources);
            AssertBasedOnStyle<FWNumberBox, NumberBox>(app.Resources);
            AssertBasedOnStyle<FWAutoCompleteBox, AutoCompleteBox>(app.Resources);
            AssertBasedOnStyle<FWAutoSuggestBox, AutoCompleteBox>(app.Resources);
            AssertBasedOnStyle<FWRichTextBox, RichTextBox>(app.Resources);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void GenericTheme_ShouldDefineTextInputBaseStylesAndFluentSetters()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = FluentThemeVariant.Dark.ToString();

        var dictionary = LoadGenericThemeDictionary();

        var textBoxStyle = AssertStyle<TextBox>(dictionary);
        AssertSetter(textBoxStyle, Control.BackgroundProperty);
        AssertSetter(textBoxStyle, Control.ForegroundProperty);
        AssertSetter(textBoxStyle, Control.BorderBrushProperty);
        AssertSetter(textBoxStyle, Control.PaddingProperty);
        AssertSetter(textBoxStyle, Control.MinHeightProperty);

        var fwTextBoxStyle = AssertStyle<FWTextBox>(dictionary);
        Assert.Equal(typeof(TextBox), fwTextBoxStyle.BasedOn?.TargetType);
        AssertSetter(fwTextBoxStyle, FWTextBox.DensityProperty);

        var passwordBoxStyle = AssertStyle<PasswordBox>(dictionary);
        AssertSetter(passwordBoxStyle, Control.BackgroundProperty);
        AssertSetter(passwordBoxStyle, Control.ForegroundProperty);
        AssertSetter(passwordBoxStyle, Control.BorderBrushProperty);
        AssertSetter(passwordBoxStyle, Control.PaddingProperty);
        AssertSetter(passwordBoxStyle, Control.MinHeightProperty);

        var fwPasswordBoxStyle = AssertStyle<FWPasswordBox>(dictionary);
        Assert.Equal(typeof(PasswordBox), fwPasswordBoxStyle.BasedOn?.TargetType);
        AssertSetter(fwPasswordBoxStyle, FWPasswordBox.DensityProperty);

        var numberBoxStyle = AssertStyle<NumberBox>(dictionary);
        AssertSetter(numberBoxStyle, Control.BackgroundProperty);
        AssertSetter(numberBoxStyle, Control.ForegroundProperty);
        AssertSetter(numberBoxStyle, Control.BorderBrushProperty);
        AssertSetter(numberBoxStyle, Control.PaddingProperty);
        AssertSetter(numberBoxStyle, Control.MinHeightProperty);

        var fwNumberBoxStyle = AssertStyle<FWNumberBox>(dictionary);
        Assert.Equal(typeof(NumberBox), fwNumberBoxStyle.BasedOn?.TargetType);
        AssertSetter(fwNumberBoxStyle, FWNumberBox.DensityProperty);

        var autoCompleteBoxStyle = AssertStyle<AutoCompleteBox>(dictionary);
        AssertSetter(autoCompleteBoxStyle, Control.BackgroundProperty);
        AssertSetter(autoCompleteBoxStyle, Control.ForegroundProperty);
        AssertSetter(autoCompleteBoxStyle, Control.BorderBrushProperty);
        AssertSetter(autoCompleteBoxStyle, Control.PaddingProperty);
        AssertSetter(autoCompleteBoxStyle, Control.MinHeightProperty);
        AssertSetter(autoCompleteBoxStyle, AutoCompleteBox.MaxDropDownHeightProperty);

        var fwAutoCompleteBoxStyle = AssertStyle<FWAutoCompleteBox>(dictionary);
        Assert.Equal(typeof(AutoCompleteBox), fwAutoCompleteBoxStyle.BasedOn?.TargetType);
        AssertSetter(fwAutoCompleteBoxStyle, FWAutoCompleteBox.DensityProperty);

        var fwAutoSuggestBoxStyle = AssertStyle<FWAutoSuggestBox>(dictionary);
        Assert.Equal(typeof(AutoCompleteBox), fwAutoSuggestBoxStyle.BasedOn?.TargetType);
        AssertSetter(fwAutoSuggestBoxStyle, FWAutoSuggestBox.DensityProperty);

        var richTextBoxStyle = AssertStyle<RichTextBox>(dictionary);
        AssertSetter(richTextBoxStyle, Control.BackgroundProperty);
        AssertSetter(richTextBoxStyle, Control.ForegroundProperty);
        AssertSetter(richTextBoxStyle, Control.BorderBrushProperty);
        AssertSetter(richTextBoxStyle, Control.PaddingProperty);
        AssertSetter(richTextBoxStyle, Control.MinHeightProperty);

        var fwRichTextBoxStyle = AssertStyle<FWRichTextBox>(dictionary);
        Assert.Equal(typeof(RichTextBox), fwRichTextBoxStyle.BasedOn?.TargetType);
        AssertSetter(fwRichTextBoxStyle, FWRichTextBox.DensityProperty);

        ResetApplicationState();
    }

    [Fact]
    public void FWTextInputControls_ShouldApplyDensityPresets()
    {
        var textBox = new FWTextBox();

        Assert.Equal(FWTextInputDensity.Comfortable, textBox.Density);
        Assert.Equal(34, textBox.MinHeight);
        Assert.Equal(new Thickness(10, 6, 10, 6), textBox.Padding);

        textBox.Density = FWTextInputDensity.Compact;

        Assert.Equal(30, textBox.MinHeight);
        Assert.Equal(new Thickness(8, 4, 8, 5), textBox.Padding);

        textBox.Density = FWTextInputDensity.Spacious;

        Assert.Equal(40, textBox.MinHeight);
        Assert.Equal(new Thickness(12, 8, 12, 8), textBox.Padding);

        var passwordBox = new FWPasswordBox
        {
            Density = FWTextInputDensity.Compact
        };

        Assert.Equal(30, passwordBox.MinHeight);
        Assert.Equal(new Thickness(8, 4, 8, 5), passwordBox.Padding);

        var autoCompleteBox = new FWAutoCompleteBox
        {
            Density = FWTextInputDensity.Spacious
        };

        Assert.Equal(40, autoCompleteBox.MinHeight);
        Assert.Equal(new Thickness(12, 8, 12, 8), autoCompleteBox.Padding);
        Assert.Equal(288, autoCompleteBox.MaxDropDownHeight);

        autoCompleteBox.Density = FWTextInputDensity.Compact;

        Assert.Equal(30, autoCompleteBox.MinHeight);
        Assert.Equal(new Thickness(8, 4, 8, 5), autoCompleteBox.Padding);
        Assert.Equal(192, autoCompleteBox.MaxDropDownHeight);

        var autoSuggestBox = new FWAutoSuggestBox
        {
            Density = FWTextInputDensity.Compact
        };

        Assert.Equal(30, autoSuggestBox.MinHeight);
        Assert.Equal(new Thickness(8, 4, 8, 5), autoSuggestBox.Padding);
        Assert.Equal(192, autoSuggestBox.MaxDropDownHeight);

        autoSuggestBox.Density = FWTextInputDensity.Spacious;

        Assert.Equal(40, autoSuggestBox.MinHeight);
        Assert.Equal(new Thickness(12, 8, 12, 8), autoSuggestBox.Padding);
        Assert.Equal(288, autoSuggestBox.MaxDropDownHeight);

        var richTextBox = new FWRichTextBox();

        Assert.Equal(FWTextInputDensity.Comfortable, richTextBox.Density);
        Assert.Equal(96, richTextBox.MinHeight);
        Assert.Equal(new Thickness(10), richTextBox.Padding);

        richTextBox.Density = FWTextInputDensity.Compact;

        Assert.Equal(80, richTextBox.MinHeight);
        Assert.Equal(new Thickness(8), richTextBox.Padding);

        richTextBox.Density = FWTextInputDensity.Spacious;

        Assert.Equal(128, richTextBox.MinHeight);
        Assert.Equal(new Thickness(12), richTextBox.Padding);
    }

    [Fact]
    public void FWAutoSuggestBox_ShouldExposeQueryAndSuggestionSubmissionSemantics()
    {
        var autoSuggestBox = new FWAutoSuggestBox
        {
            ItemsSource = new[] { "CalendarView", "CalendarDatePicker", "GridView", "AutoSuggestBox" },
            FilterMode = AutoCompleteFilterMode.StartsWith,
            MinimumPrefixLength = 1
        };
        var suggestionChosenCount = 0;
        var querySubmittedCount = 0;
        FWAutoSuggestBoxSuggestionChosenEventArgs? chosenArgs = null;
        FWAutoSuggestBoxQuerySubmittedEventArgs? submittedArgs = null;
        autoSuggestBox.SuggestionChosen += (_, args) =>
        {
            suggestionChosenCount++;
            chosenArgs = args;
        };
        autoSuggestBox.QuerySubmitted += (_, args) =>
        {
            querySubmittedCount++;
            submittedArgs = args;
        };

        autoSuggestBox.SetQueryText("Cal", FWAutoSuggestBoxTextChangeReason.UserInput);

        Assert.Equal("Cal", autoSuggestBox.Text);
        Assert.Equal(FWAutoSuggestBoxTextChangeReason.UserInput, autoSuggestBox.LastTextChangeReason);
        Assert.Equal(2, autoSuggestBox.FilteredItems.Count);

        var suggestion = autoSuggestBox.FilteredItems[0];

        Assert.True(autoSuggestBox.RequestSuggestionChosen(suggestion));
        Assert.Equal(1, suggestionChosenCount);
        Assert.NotNull(chosenArgs);
        Assert.Same(suggestion, chosenArgs!.SelectedItem);
        Assert.Same(suggestion, autoSuggestBox.SelectedItem);
        Assert.Equal("CalendarView", autoSuggestBox.Text);
        Assert.False(autoSuggestBox.IsDropDownOpen);
        Assert.Equal(FWAutoSuggestBoxTextChangeReason.SuggestionChosen, autoSuggestBox.LastTextChangeReason);

        var returnedArgs = autoSuggestBox.RequestQuerySubmitted();

        Assert.Equal(1, querySubmittedCount);
        Assert.Same(returnedArgs, submittedArgs);
        Assert.Equal("CalendarView", submittedArgs!.QueryText);
        Assert.Same(suggestion, submittedArgs.ChosenSuggestion);

        var explicitSuggestion = "Manual suggestion";
        var explicitArgs = autoSuggestBox.RequestQuerySubmitted(explicitSuggestion);

        Assert.Equal(2, querySubmittedCount);
        Assert.Same(explicitArgs, submittedArgs);
        Assert.Same(explicitSuggestion, submittedArgs!.ChosenSuggestion);
        Assert.False(autoSuggestBox.RequestSuggestionChosen(null));
        Assert.Equal(1, suggestionChosenCount);

        var memberPathBox = new FWAutoSuggestBox
        {
            TextMemberPath = nameof(AutoSuggestRow.Name)
        };
        var row = new AutoSuggestRow("CalendarDatePicker");

        Assert.True(memberPathBox.RequestSuggestionChosen(row));
        Assert.Same(row, memberPathBox.SelectedItem);
        Assert.Equal("CalendarDatePicker", memberPathBox.Text);

        var bridgedBox = new FWAutoSuggestBox();
        var bridgedCount = 0;
        object? bridgedItem = null;
        bridgedBox.SuggestionChosen += (_, args) =>
        {
            bridgedCount++;
            bridgedItem = args.SelectedItem;
        };
        var bridgedSuggestion = "AutoSuggestBox";
        bridgedBox.SelectedItem = bridgedSuggestion;
        bridgedBox.RaiseEvent(new SelectionChangedEventArgs(
            AutoCompleteBox.SelectionChangedEvent,
            Array.Empty<object>(),
            new[] { bridgedSuggestion }));

        Assert.Equal(1, bridgedCount);
        Assert.Same(bridgedSuggestion, bridgedItem);
        Assert.Equal(FWAutoSuggestBoxTextChangeReason.SuggestionChosen, bridgedBox.LastTextChangeReason);
    }

    [Fact]
    public void FWAutoSuggestBox_ShouldNormalizeNullQueryAndFallbackSuggestionText()
    {
        var autoSuggestBox = new FWAutoSuggestBox
        {
            Text = "Calendar",
            TextMemberPath = "MissingName",
            IsDropDownOpen = true
        };
        var fallbackSuggestion = new AutoSuggestFallbackRow("Fallback suggestion");
        FWAutoSuggestBoxSuggestionChosenEventArgs? chosenArgs = null;
        autoSuggestBox.SuggestionChosen += (_, args) => chosenArgs = args;

        autoSuggestBox.SetQueryText(null, FWAutoSuggestBoxTextChangeReason.UserInput);

        Assert.Equal(string.Empty, autoSuggestBox.Text);
        Assert.Equal(FWAutoSuggestBoxTextChangeReason.UserInput, autoSuggestBox.LastTextChangeReason);

        Assert.True(autoSuggestBox.RequestSuggestionChosen(fallbackSuggestion));

        Assert.Same(fallbackSuggestion, autoSuggestBox.SelectedItem);
        Assert.Same(fallbackSuggestion, chosenArgs?.SelectedItem);
        Assert.Equal("Fallback suggestion", autoSuggestBox.Text);
        Assert.False(autoSuggestBox.IsDropDownOpen);
        Assert.Equal(FWAutoSuggestBoxTextChangeReason.SuggestionChosen, autoSuggestBox.LastTextChangeReason);
    }

    [Fact]
    public void FWAutoSuggestBox_ShouldExposeTextChangedReasonEvents()
    {
        var autoSuggestBox = new TestAutoSuggestBox
        {
            ItemsSource = new[] { "CalendarView", "CalendarDatePicker", "GridView", "AutoSuggestBox" },
            FilterMode = AutoCompleteFilterMode.Contains,
            MinimumPrefixLength = 1
        };
        var reasons = new List<FWAutoSuggestBoxTextChangeReason>();
        var textValues = new List<string>();
        autoSuggestBox.AutoSuggestTextChanged += (_, args) =>
        {
            reasons.Add(args.Reason);
            textValues.Add(args.Text);
        };

        autoSuggestBox.Text = "Grid";
        autoSuggestBox.SetQueryText("Cal", FWAutoSuggestBoxTextChangeReason.UserInput);
        autoSuggestBox.InsertUserText("endar");
        var suggestion = "CalendarView";

        Assert.True(autoSuggestBox.RequestSuggestionChosen(suggestion));

        Assert.Equal(
            [
                FWAutoSuggestBoxTextChangeReason.ProgrammaticChange,
                FWAutoSuggestBoxTextChangeReason.UserInput,
                FWAutoSuggestBoxTextChangeReason.UserInput,
                FWAutoSuggestBoxTextChangeReason.SuggestionChosen
            ],
            reasons);
        Assert.Equal("Grid", textValues[0]);
        Assert.Equal("Cal", textValues[1]);
        Assert.Contains("endar", textValues[2]);
        Assert.Equal("CalendarView", textValues[3]);
        Assert.Equal(FWAutoSuggestBoxTextChangeReason.SuggestionChosen, autoSuggestBox.LastTextChangeReason);
    }

    [Fact]
    public void FWAutoSuggestBox_ShouldSubmitEnterForTextAndChosenSuggestion()
    {
        var autoSuggestBox = new TestAutoSuggestBox
        {
            ItemsSource = new[] { "CalendarView", "CalendarDatePicker", "GridView", "AutoSuggestBox" },
            FilterMode = AutoCompleteFilterMode.StartsWith,
            MinimumPrefixLength = 1
        };
        var submittedQueries = new List<FWAutoSuggestBoxQuerySubmittedEventArgs>();
        var chosenItems = new List<object>();
        autoSuggestBox.QuerySubmitted += (_, args) => submittedQueries.Add(args);
        autoSuggestBox.SuggestionChosen += (_, args) => chosenItems.Add(args.SelectedItem);

        autoSuggestBox.SetQueryText("Unknown", FWAutoSuggestBoxTextChangeReason.UserInput);
        var textEnterArgs = autoSuggestBox.RaiseEnterKey();

        Assert.True(textEnterArgs.Handled);
        Assert.Single(submittedQueries);
        Assert.Equal("Unknown", submittedQueries[0].QueryText);
        Assert.Null(submittedQueries[0].ChosenSuggestion);
        Assert.Empty(chosenItems);

        autoSuggestBox.SetQueryText("Cal", FWAutoSuggestBoxTextChangeReason.UserInput);
        Assert.Equal(2, autoSuggestBox.FilteredItems.Count);
        var suggestionEnterArgs = autoSuggestBox.RaiseEnterKey();

        Assert.True(suggestionEnterArgs.Handled);
        Assert.Equal(2, submittedQueries.Count);
        Assert.Single(chosenItems);
        Assert.Same(autoSuggestBox.FilteredItems[0], chosenItems[0]);
        Assert.Equal("CalendarView", submittedQueries[1].QueryText);
        Assert.Same(chosenItems[0], submittedQueries[1].ChosenSuggestion);
        Assert.False(autoSuggestBox.IsDropDownOpen);
        Assert.Equal(FWAutoSuggestBoxTextChangeReason.SuggestionChosen, autoSuggestBox.LastTextChangeReason);
    }

    [Fact]
    public void FWTextInputControls_ShouldExposeMaterialInputPanelState()
    {
        var textBox = new FWTextBox
        {
            Text = "FluentJalium",
            PlaceholderText = "Enter text",
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap
        };
        var passwordBox = new FWPasswordBox
        {
            Password = "material",
            PlaceholderText = "Token",
            RevealMode = PasswordRevealMode.Peek,
            IsPasswordRevealed = true
        };
        var numberBox = new FWNumberBox
        {
            Density = FWNumberBoxDensity.Spacious,
            Minimum = 0,
            Maximum = 100,
            Value = 24,
            SmallChange = 4,
            LargeChange = 12,
            DecimalPlaces = 0,
            AcceptsExpression = true,
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Hidden
        };
        var autoCompleteBox = new FWAutoCompleteBox
        {
            ItemsSource = new[] { "Fluent tokens", "Fluent controls", "WinUI Gallery", "Community Toolkit" },
            Text = "Fluent",
            FilterMode = AutoCompleteFilterMode.Contains,
            MinimumPrefixLength = 1,
            PlaceholderText = "Search"
        };
        var autoSuggestBox = new FWAutoSuggestBox
        {
            ItemsSource = new[] { "CalendarView", "CalendarDatePicker", "GridView", "AutoSuggestBox" },
            Text = "Calendar",
            FilterMode = AutoCompleteFilterMode.StartsWith,
            MinimumPrefixLength = 1,
            IsTextCompletionEnabled = true,
            PlaceholderText = "Suggest controls"
        };
        var richTextBox = new FWRichTextBox
        {
            AcceptsTab = true,
            IsSpellCheckEnabled = true
        };
        richTextBox.SetText("Layered input surfaces keep focus and text contrast.");

        var panel = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Children =
            {
                textBox,
                passwordBox,
                numberBox,
                autoCompleteBox,
                autoSuggestBox,
                richTextBox
            }
        };
        var surface = new FWFluentMaterialSurface
        {
            MaterialKind = FWFluentMaterialKind.LiquidGlass,
            TintOpacity = 0.2,
            BlurRadius = 14,
            RefractionAmount = 70,
            ChromaticAberration = 0.42,
            FusionRadius = 24,
            Shape = BorderShape.SuperEllipse,
            SuperEllipseN = 4,
            Child = panel
        };

        Assert.Equal("FluentJalium", textBox.Text);
        Assert.Equal("Enter text", textBox.PlaceholderText);
        Assert.True(textBox.AcceptsReturn);
        Assert.Equal(TextWrapping.Wrap, textBox.TextWrapping);
        Assert.Equal("material", passwordBox.Password);
        Assert.Equal(PasswordRevealMode.Peek, passwordBox.RevealMode);
        Assert.True(passwordBox.IsPasswordRevealed);
        Assert.Equal(24, numberBox.Value);
        Assert.Equal(FWNumberBoxDensity.Spacious, numberBox.Density);
        Assert.Equal(40, numberBox.MinHeight);
        Assert.Equal(new Thickness(12, 8, 12, 8), numberBox.Padding);
        Assert.Equal(4, numberBox.SmallChange);
        Assert.Equal(12, numberBox.LargeChange);
        Assert.Equal(0, numberBox.DecimalPlaces);
        Assert.True(numberBox.AcceptsExpression);
        Assert.Equal(NumberBoxSpinButtonPlacementMode.Hidden, numberBox.SpinButtonPlacementMode);
        Assert.Equal("Fluent", autoCompleteBox.Text);
        Assert.Equal(AutoCompleteFilterMode.Contains, autoCompleteBox.FilterMode);
        Assert.Equal(2, autoCompleteBox.FilteredItems.Count);
        Assert.Contains("Fluent tokens", autoCompleteBox.FilteredItems);
        Assert.Contains("Fluent controls", autoCompleteBox.FilteredItems);
        Assert.Equal("Calendar", autoSuggestBox.Text);
        Assert.Equal(AutoCompleteFilterMode.StartsWith, autoSuggestBox.FilterMode);
        Assert.True(autoSuggestBox.IsTextCompletionEnabled);
        Assert.Equal(2, autoSuggestBox.FilteredItems.Count);
        Assert.Contains("CalendarView", autoSuggestBox.FilteredItems);
        Assert.Contains("CalendarDatePicker", autoSuggestBox.FilteredItems);
        Assert.True(richTextBox.AcceptsTab);
        Assert.True(richTextBox.IsSpellCheckEnabled);
        Assert.Contains("Layered input surfaces", richTextBox.GetText());
        Assert.Equal(FWFluentMaterialKind.LiquidGlass, surface.MaterialKind);
        Assert.True(surface.LiquidGlass);
        Assert.Equal(70, surface.RefractionAmount);
        Assert.Equal(0.42, surface.ChromaticAberration);
        Assert.Equal(24, surface.FusionRadius);
        Assert.Equal(BorderShape.SuperEllipse, surface.Shape);
        Assert.Equal(4, surface.SuperEllipseN);
        Assert.Same(panel, surface.Child);
    }

    [Fact]
    public void GalleryTextInputPage_ShouldFormatMaskedInputRecipesWithoutPublicMaskedTextBox()
    {
        var partialPhone = GalleryTextInputPage.FormatPhoneRecipe("42555");
        var fullPhone = GalleryTextInputPage.FormatPhoneRecipe("425.555.0123 ext 88");
        var license = GalleryTextInputPage.FormatLicenseKeyRecipe("flnt-jlum-2026-extra");
        var snapshot = GalleryTextInputPage.CreateFormattingRecipeSnapshot(fullPhone, license);
        var text = GalleryTextInputPage.FormatFormattingRecipeQa("Formatting recipe QA", snapshot);

        Assert.Equal("(425) 55", partialPhone);
        Assert.Equal("(425) 555-0123", fullPhone);
        Assert.Equal("FLNT-JLUM-2026", license);
        Assert.Equal("(425) 555-0123", snapshot.FormattedPhone);
        Assert.Equal("FLNT-JLUM-2026", snapshot.FormattedLicenseKey);
        Assert.True(snapshot.IsPhoneComplete);
        Assert.True(snapshot.IsLicenseKeyComplete);
        Assert.True(snapshot.IsRecipeOnly);
        Assert.True(snapshot.IsReady);
        Assert.Contains("Formatting recipe QA", text);
        Assert.Contains("recipe-only on", text);
        Assert.DoesNotContain("FWMaskedTextBox", text);
    }

    private static ResourceDictionary LoadGenericThemeDictionary()
    {
        var loaded = ResourceDictionary.SourceLoader?.Invoke(
            new ResourceDictionary(),
            new Uri("/FluentJalium;component/Themes/Generic.jalxaml", UriKind.Relative),
            FluentThemeManager.ThemeAssembly);

        return Assert.IsType<ResourceDictionary>(loaded);
    }

    private sealed record AutoSuggestRow(string Name);

    private sealed record AutoSuggestFallbackRow(string Label)
    {
        public override string ToString() => Label;
    }

    private sealed class TestAutoSuggestBox : FWAutoSuggestBox
    {
        public void InsertUserText(string text)
        {
            InsertText(text);
        }

        public KeyEventArgs RaiseEnterKey()
        {
            var args = new KeyEventArgs(
                UIElement.KeyDownEvent,
                Key.Enter,
                ModifierKeys.None,
                isDown: true,
                isRepeat: false,
                timestamp: 0);
            OnKeyDown(args);
            return args;
        }
    }

    private static Style AssertStyle<TControl>(ResourceDictionary dictionary)
        where TControl : FrameworkElement
    {
        Assert.True(dictionary.TryGetValue(typeof(TControl), out var value), $"{typeof(TControl).Name} style was not found.");
        return Assert.IsType<Style>(value);
    }

    private static void AssertBasedOnStyle<TFluentControl, TJaliumControl>(ResourceDictionary dictionary)
        where TFluentControl : TJaliumControl, IFluentJaliumControl
        where TJaliumControl : FrameworkElement
    {
        var baseStyle = AssertStyle<TJaliumControl>(dictionary);
        var fluentStyle = AssertStyle<TFluentControl>(dictionary);

        Assert.Equal(typeof(TFluentControl), fluentStyle.TargetType);
        Assert.Same(baseStyle, fluentStyle.BasedOn);
    }

    private static void AssertSetter(Style style, DependencyProperty property)
    {
        Assert.Contains(style.Setters, setter => setter.Property == property);
    }

    private static void ResetApplicationState()
    {
        var currentField = typeof(Application).GetField("_current", BindingFlags.NonPublic | BindingFlags.Static);
        currentField?.SetValue(null, null);

        var jaliumReset = typeof(JaliumThemeManager).GetMethod("Reset", BindingFlags.NonPublic | BindingFlags.Static);
        jaliumReset?.Invoke(null, null);

        var fluentReset = typeof(FluentThemeManager).GetMethod("Reset", BindingFlags.NonPublic | BindingFlags.Static);
        fluentReset?.Invoke(null, null);
    }
}
