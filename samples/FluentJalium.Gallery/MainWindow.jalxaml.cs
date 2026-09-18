using FluentJalium.Controls;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Automation;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Media;

namespace FluentJalium.Gallery;

public partial class MainWindow : Window
{
    private readonly Dictionary<FluentNavigationItem, FrameworkElement> _pages;
    private readonly Dictionary<FluentNavigationItem, string> _pageIds = [];
    private readonly GalleryCatalog _catalog = GalleryCatalog.Load();
    private FluentThemeVariant _theme = FluentThemeVariant.System;
    private bool _syncControls = true;
    private bool _loaded;
    private int _buttonCount;
    private int _repeatCount;
    private string? _startPageId;

    private FluentNavigationView Navigation => (FluentNavigationView)NavigationRoot!;
    private Grid ContentHost => (Grid)PageHost!;
    private TextBox NameInput => (TextBox)DisplayNameBox!;
    private PasswordBox PasswordInput => (PasswordBox)SamplePasswordBox!;
    private ComboBox ProfileChoice => (ComboBox)ProfileComboBox!;
    private ComboBox ThemeChoice => (ComboBox)ThemeComboBox!;
    private ComboBox AccentChoice => (ComboBox)AccentComboBox!;
    private Slider Volume => (Slider)VolumeSlider!;
    private FluentToggleSwitch Notifications => (FluentToggleSwitch)NotificationsSwitch!;
    private FluentToggleSwitch MotionPreference => (FluentToggleSwitch)ReduceMotionSwitch!;

    public MainWindow()
    {
        InitializeComponent();
        _pages = new Dictionary<FluentNavigationItem, FrameworkElement>
        {
            [(FluentNavigationItem)OverviewItem!] = (FrameworkElement)OverviewPage!,
            [(FluentNavigationItem)ButtonsItem!] = (FrameworkElement)ButtonsPage!,
            [(FluentNavigationItem)InputsItem!] = (FrameworkElement)InputsPage!,
            [(FluentNavigationItem)SelectionItem!] = (FrameworkElement)SelectionPage!,
            [(FluentNavigationItem)NavigationItem!] = (FrameworkElement)NavigationPage!,
            [(FluentNavigationItem)SettingsItem!] = (FrameworkElement)SettingsPage!,
        };
        _pageIds[(FluentNavigationItem)OverviewItem!] = "overview";
        _pageIds[(FluentNavigationItem)ButtonsItem!] = "buttons";
        _pageIds[(FluentNavigationItem)InputsItem!] = "inputs";
        _pageIds[(FluentNavigationItem)SelectionItem!] = "selection";
        _pageIds[(FluentNavigationItem)NavigationItem!] = "navigation";
        _pageIds[(FluentNavigationItem)SettingsItem!] = "settings";

        // Only the active page is in the visual tree and tab order. Reusing the same
        // page instances preserves native input state when moving between examples.
        ContentHost.Children.Clear();
        Navigation.SelectionChanged += OnNavigationChanged;
        WireButtons();
        WireInputs();
        WireSelection();
        WireAppearance();
        SetAccessibleNames();

        ProfileChoice.SelectedItem = ProfileChoice.Items[0];
        ThemeChoice.SelectedItem = ThemeChoice.Items[2];
        AccentChoice.SelectedItem = AccentChoice.Items[0];
        MotionPreference.IsChecked = FluentThemeManager.ReduceMotion;
        Navigation.SelectedItem = (FluentNavigationItem)OverviewItem!;
        _syncControls = false;

        FluentThemeManager.Changed += ApplyAppearance;
        ApplyAppearance();
        Loaded += (_, _) =>
        {
            _loaded = true;
            if (_startPageId is { } pageId) NavigateToPage(pageId);
        };
        SystemSettingsChanged += (_, _) => FluentThemeManager.ApplyTheme(_theme);
        ((FrameworkElement)Content!).SizeChanged += (_, args) =>
            ContentHost.Margin = new Thickness(args.NewSize.Width < 720 ? 16 : 24);
        Closed += (_, _) =>
        {
            _loaded = false;
            FluentThemeManager.Changed -= ApplyAppearance;
            Navigation.SelectionChanged -= OnNavigationChanged;
        };
    }

    private void WireButtons()
    {
        ((Button)ExploreInputsButton!).Click += (_, _) => Select((FluentNavigationItem)InputsItem!);
        ((Button)ExploreButtonsButton!).Click += (_, _) => Select((FluentNavigationItem)ButtonsItem!);
        ((Button)ExploreNavigationButton!).Click += (_, _) => Select((FluentNavigationItem)NavigationItem!);
        ((Button)JumpOverviewButton!).Click += (_, _) => Select((FluentNavigationItem)OverviewItem!);
        ((Button)JumpSettingsButton!).Click += (_, _) => Select((FluentNavigationItem)SettingsItem!);
        ((Button)TogglePaneButton!).Click += (_, _) =>
        {
            Navigation.IsPaneOpen = !Navigation.IsPaneOpen;
            Report(Navigation.IsPaneOpen ? "Navigation pane expanded." : "Navigation pane compact.");
        };

        foreach (var button in new[] { (Button)StandardActionButton!, (Button)AccentActionButton!, (Button)SubtleActionButton! })
            button.Click += (_, _) => Report($"{button.Content} button activated. Total actions: {++_buttonCount}.");

        ((Button)ClearOutputButton!).Click += (_, _) => Report("Output cleared. Choose a control to try it.");

        ((RepeatButton)RepeatActionButton!).Click += (_, _) =>
            ((TextBlock)RepeatReadout!).Text = $"Repeat button fired {++_repeatCount} times.";
        foreach (var toggle in new[] { (ToggleButton)QuietToggleButton!, (ToggleButton)CheckedToggleButton!, (ToggleButton)MixedToggleButton! })
            WireToggle(toggle, toggle.Content?.ToString() ?? "Toggle");

        // Set in code, not markup: IsChecked='{x:Null}' parses without error and yields false, so a
        // three-state sample written the WinUI way would sit in off while looking tri-state on screen.
        ((ToggleButton)MixedToggleButton!).IsChecked = null;
    }

    private void WireInputs()
    {
        NameInput.TextChanged += (_, _) => Report($"Display name changed: {NameInput.Text.Length} characters.");
        PasswordInput.PasswordChanged += (_, _) => Report($"Password changed: {PasswordInput.Password.Length} characters.");
        ProfileChoice.SelectionChanged += (_, _) => Report($"Profile: {Describe(ProfileChoice.SelectedItem)}.");
        Volume.ValueChanged += (_, _) =>
        {
            ((TextBlock)VolumeValue!).Text = $"{Volume.Value:0}%";
            Report($"Volume: {Volume.Value:0}%.");
        };
        WireToggle(Notifications, "Notifications");
        // AutoCompleteBox has no Items collection, only ItemsSource, so the candidate list is code-side.
        var fruits = new[] { "Apple", "Apricot", "Banana", "Blueberry", "Cherry", "Grapefruit", "Lemon", "Mango", "Orange", "Peach" };
        FruitBox.ItemsSource = fruits;
        DisabledFruitBox.ItemsSource = fruits;
        FruitBox.TextChanged += (_, _) => Report($"Fruit filter: {FruitBox.Text.Length} characters.");
        FruitBox.SelectionChanged += (_, _) => Report($"Fruit suggestion: {Describe(FruitBox.SelectedItem)}.");

        ((Button)ApplyInputsButton!).Click += (_, _) =>
        {
            var name = NameInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(name)) name = "Unnamed";
            if (name.Length > 48) name = name[..48] + "…";
            Report($"Applied locally: {name} · {Describe(ProfileChoice.SelectedItem)} · Volume {Volume.Value:0}% · Notifications {(Notifications.IsChecked == true ? "on" : "off")}.");
        };
        ((Button)ResetInputsButton!).Click += (_, _) =>
        {
            _syncControls = true;
            try
            {
                NameInput.Text = "Astra";
                PasswordInput.Password = string.Empty;
                ProfileChoice.SelectedItem = ProfileChoice.Items[0];
                Volume.Value = 45;
                Notifications.IsChecked = true;
            }
            finally { _syncControls = false; }
            Report("Input values reset.");
        };
    }

    private void WireSelection()
    {
        WireToggle((CheckBox)PreviewCheckBox!, "Preview");
        WireToggle((CheckBox)DetailsCheckBox!, "Details");
        WireToggle((CheckBox)MixedCheckBox!, "Three-state option");
        // IsChecked="{x:Null}" in markup arrives as false on this runtime, so the third state is set
        // here or the sample would quietly show an unchecked box. See AstraSelectionTests.
        ((CheckBox)MixedCheckBox!).IsChecked = null;
        ((RadioButton)ComfortableRadio!).Checked += (_, _) => Report("Density choice: Comfortable.");
        ((RadioButton)CompactRadio!).Checked += (_, _) => Report("Density choice: Compact.");
        ((ListBox)SampleListBox!).SelectionChanged += (_, _) =>
            Report($"List selection: {Describe(((ListBox)SampleListBox!).SelectedItem)}.");
    }

    private void WireAppearance()
    {
        ThemeChoice.SelectionChanged += (_, _) =>
        {
            if (_syncControls) return;
            var index = SelectedIndex(ThemeChoice);
            if (index < 0) return;
            _theme = (FluentThemeVariant)index;
            FluentThemeManager.ApplyTheme(_theme);
            Report($"Theme: {Describe(ThemeChoice.SelectedItem)}.");
        };
        AccentChoice.SelectionChanged += (_, _) =>
        {
            if (_syncControls) return;
            var index = SelectedIndex(AccentChoice);
            if (index < 0) return;
            FluentThemeManager.ApplyAccent(index switch
            {
                1 => Color.FromRgb(0, 120, 212),
                2 => Color.FromRgb(126, 70, 204),
                3 => Color.FromRgb(16, 124, 65),
                _ => (Color?)null,
            });
            Report($"Accent: {Describe(AccentChoice.SelectedItem)}.");
        };
        MotionPreference.Checked += OnMotionPreferenceChanged;
        MotionPreference.Unchecked += OnMotionPreferenceChanged;

        ((Button)ReplayEntranceButton!).Click += (_, _) =>
        {
            if (Navigation.SelectedItem is { } selected && _pages.TryGetValue(selected, out var page))
                FluentThemeManager.Enter(page);
            Report(FluentThemeManager.AnimationsEnabled ? "Page entrance replayed." : "Page shown immediately because motion is reduced.");
        };
        ((Button)RestoreDefaultsButton!).Click += (_, _) =>
        {
            _syncControls = true;
            try
            {
                _theme = FluentThemeVariant.System;
                ThemeChoice.SelectedItem = ThemeChoice.Items[2];
                AccentChoice.SelectedItem = AccentChoice.Items[0];
                FluentThemeManager.ApplyTheme(_theme);
                FluentThemeManager.ApplyAccent(null);
                FluentThemeManager.ReduceMotion = false;
                MotionPreference.IsChecked = false;
            }
            finally { _syncControls = false; }
            Report("Appearance restored to system theme and default accent.");
        };
    }

    private void OnMotionPreferenceChanged(object sender, RoutedEventArgs args)
    {
        if (_syncControls) return;
        FluentThemeManager.ReduceMotion = MotionPreference.IsChecked == true;
        Report(FluentThemeManager.ReduceMotion
            ? "Reduced motion enabled for Astra's own animations. Template transitions still use their designed durations."
            : "App animations enabled; Windows preferences still apply.");
    }

    private void WireToggle(ToggleButton control, string label)
    {
        control.Checked += Changed;
        control.Unchecked += Changed;
        control.Indeterminate += Changed;
        void Changed(object sender, RoutedEventArgs args) =>
            Report($"{label}: {(control.IsChecked == true ? "on" : control.IsChecked == false ? "off" : "mixed")}.");
    }

    private void SetAccessibleNames()
    {
        AutomationProperties.SetName(Navigation, "Gallery navigation");
        AutomationProperties.SetName(NameInput, "Display name");
        AutomationProperties.SetName(PasswordInput, "Password sample");
        AutomationProperties.SetName(ProfileChoice, "Profile");
        AutomationProperties.SetName(Notifications, "Notifications");
        AutomationProperties.SetName(Volume, "Volume");
        AutomationProperties.SetName((ListBox)SampleListBox!, "Workflow stage");
        AutomationProperties.SetName(ThemeChoice, "Gallery theme");
        AutomationProperties.SetName(AccentChoice, "Accent color");
        AutomationProperties.SetName(MotionPreference, "Reduce motion");
    }

    private void Select(FluentNavigationItem item) => Navigation.SelectedItem = item;

    /// <summary>Page id to mount on start-up instead of Overview. Must be set before Show().</summary>
    public void SetStartPage(string pageId) => _startPageId = pageId;

    public void NavigateToPage(string pageId)
    {
        foreach (var pair in _pageIds)
        {
            if (string.Equals(pair.Value, pageId, StringComparison.OrdinalIgnoreCase))
            {
                Select(pair.Key);
                return;
            }
        }

        Report($"\"{pageId}\" is not a gallery page id. Options: {string.Join(", ", _pageIds.Values)}.");
    }

    private void OnNavigationChanged(object? sender, FluentNavigationSelectionChangedEventArgs args)
    {
        if (args.SelectedItem == null || !_pages.TryGetValue(args.SelectedItem, out var page)) return;
        ProfileChoice.IsDropDownOpen = false;
        ThemeChoice.IsDropDownOpen = false;
        AccentChoice.IsDropDownOpen = false;
        ContentHost.Children.Clear();
        ContentHost.Children.Add(page);
        ((ScrollViewer)PageScrollViewer!).ScrollToVerticalOffset(0);
        if (_loaded) FluentThemeManager.Enter(page);
        ShowParity(args.SelectedItem);
        Report($"Navigated to {args.SelectedItem.Content}.");
    }

    /// <summary>
    /// The per-control parity line the nine-step exit asks for, read straight out of Catalog.json. A
    /// page with no type of its own says so instead of going quiet, and a missing catalog file says
    /// that too - an empty strip would otherwise look like "nothing restyled here".
    /// </summary>
    private void ShowParity(FluentNavigationItem? item)
    {
        var status = (TextBlock)ParityStatus!;
        var gaps = (TextBlock)ParityGaps!;
        if (item is null)
        {
            status.Text = string.Empty;
            gaps.Text = string.Empty;
            return;
        }

        if (_catalog.Controls.Length == 0)
        {
            status.Text = "Catalog.json did not reach the output folder, so no parity claim on this page is checked.";
            gaps.Text = string.Empty;
            return;
        }

        if (!_pageIds.TryGetValue(item, out var pageId))
        {
            status.Text = $"{item.Content} is not in the catalog.";
            gaps.Text = string.Empty;
            return;
        }

        var onPage = _catalog.OnPage(pageId);
        var title = _catalog.Page(pageId)?.Title ?? pageId;
        status.Text = onPage.Length == 0
            ? $"{title} · composes the {_catalog.Controls.Length} restyled types listed on their own pages"
            : $"{title} · {string.Join(", ", onPage.Select(control => $"{control.Name} {control.Parity}"))} · {onPage.Length} of {_catalog.Controls.Length} restyled types";
        var open = _catalog.GapsLine(pageId);
        gaps.Text = open.Length == 0 ? string.Empty : $"Not claimed: {open}";
    }

    private void ApplyAppearance()
    {
        Background = FluentThemeManager.GetBrush("SolidBackgroundFillColorBaseBrush");
        Foreground = FluentThemeManager.GetBrush("TextFillColorPrimaryBrush");
        if (TitleBar is { } titleBar)
        {
            titleBar.Background = Background;
            titleBar.Foreground = Foreground;
        }
        ((TextBlock)ThemeStatus!).Text = $"{_theme} · {(FluentThemeManager.AnimationsEnabled ? "Motion on" : "Reduced motion")}";
    }

    private void Report(string message)
    {
        if (!_syncControls) ((TextBlock)LiveOutput!).Text = message;
    }

    private static string Describe(object? item) => item is ContentControl control
        ? control.Content?.ToString() ?? "None"
        : item?.ToString() ?? "None";

    private static int SelectedIndex(ComboBox combo)
    {
        for (var index = 0; index < combo.Items.Count; index++)
        {
            var item = combo.Items[index];
            if (ReferenceEquals(item, combo.SelectedItem) ||
                item is ContentControl control && Equals(control.Content, combo.SelectedItem)) return index;
        }
        return -1;
    }
}
