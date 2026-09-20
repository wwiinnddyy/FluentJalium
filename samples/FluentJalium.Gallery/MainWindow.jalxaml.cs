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
    private int _splitCount;
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
            [(FluentNavigationItem)SurfacesItem!] = (FrameworkElement)SurfacesPage!,
            [(FluentNavigationItem)MenusItem!] = (FrameworkElement)MenusPage!,
            [(FluentNavigationItem)CommandBarItem!] = (FrameworkElement)CommandBarPage!,
            [(FluentNavigationItem)SettingsItem!] = (FrameworkElement)SettingsPage!,
        };
        _pageIds[(FluentNavigationItem)OverviewItem!] = "overview";
        _pageIds[(FluentNavigationItem)ButtonsItem!] = "buttons";
        _pageIds[(FluentNavigationItem)InputsItem!] = "inputs";
        _pageIds[(FluentNavigationItem)SelectionItem!] = "selection";
        _pageIds[(FluentNavigationItem)NavigationItem!] = "navigation";
        _pageIds[(FluentNavigationItem)SurfacesItem!] = "surfaces";
        _pageIds[(FluentNavigationItem)MenusItem!] = "menus";
        _pageIds[(FluentNavigationItem)CommandBarItem!] = "command-bar";
        _pageIds[(FluentNavigationItem)SettingsItem!] = "settings";

        // Only the active page is in the visual tree and tab order. Reusing the same
        // page instances preserves native input state when moving between examples.
        ContentHost.Children.Clear();
        Navigation.SelectionChanged += OnNavigationChanged;
        WireButtons();
        WireInputs();
        WireSelection();
        WireSurfaces();
        WireMenus();
        WireCommandBar();
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

        var split = (SplitButton)SplitActionButton!;
        split.Click += (_, _) => ((TextBlock)SplitReadout!).Text = $"Split primary action ran {++_splitCount} time(s).";
        split.Flyout!.Opened += (_, _) => ((TextBlock)SplitReadout!).Text = "Split flyout opened - the left half did not run.";
        split.Flyout.Closed += (_, _) => ((TextBlock)SplitReadout!).Text = "Split flyout closed.";

        var dropDown = (FluentDropDownButton)DropDownMoreButton!;
        dropDown.Flyout!.Opened += (_, _) => ((TextBlock)SplitReadout!).Text = $"Drop-down opened; IsExpanded={dropDown.IsExpanded}.";
        dropDown.Flyout.Closed += (_, _) => ((TextBlock)SplitReadout!).Text = $"Drop-down closed; IsExpanded={dropDown.IsExpanded}.";
    }

    private void WireCommandBar()
    {
        var bar = (CommandBar)SampleCommandBar!;
        var save = new AppBarButton { Label = "Save", Icon = new SymbolIcon { Symbol = Symbol.Save } };
        var bold = new AppBarToggleButton { Label = "Bold", Icon = new SymbolIcon { Symbol = Symbol.Bold } };
        var share = new AppBarButton { Label = "Share", Icon = new SymbolIcon { Symbol = Symbol.Share } };
        var disabled = new AppBarButton { Label = "Delete", Icon = new SymbolIcon { Symbol = Symbol.Delete }, IsEnabled = false };
        var without = new AppBarButton { Label = "No icon" };
        bar.PrimaryCommands.Add(save);
        bar.PrimaryCommands.Add(bold);
        bar.PrimaryCommands.Add(share);
        bar.PrimaryCommands.Add(new AppBarSeparator());
        bar.PrimaryCommands.Add(disabled);
        bar.PrimaryCommands.Add(without);
        bar.SecondaryCommands.Add(new AppBarButton { Label = "Properties" });
        bar.SecondaryCommands.Add(new AppBarButton { Label = "Version history" });

        save.Click += (_, _) => ((TextBlock)CommandBarReadout!).Text = "Save invoked - the fill under the label is AppBarButtonBackground, and the press that lights it is the IsPressed cell.";
        share.Click += (_, _) => ((TextBlock)CommandBarReadout!).Text = "Share invoked through the same routed handler a click reaches.";
        bold.IsChecked = true;
        bold.Checked += (_, _) => ((TextBlock)CommandBarReadout!).Text = $"Bold checked: the accent fill is AppBarToggleButtonBackgroundChecked, which is the whole mark - upstream shows no glyph in a bar.";
        bold.Unchecked += (_, _) => ((TextBlock)CommandBarReadout!).Text = "Bold unchecked: the resting row is transparent, so the button reads as the bar again.";
        disabled.Click += (_, _) => ((TextBlock)CommandBarReadout!).Text = "A disabled bar button answered a click.";

        ((Button)CommandBarOpenButton!).Click += (_, _) =>
        {
            bar.IsOpen = !bar.IsOpen;
            ((TextBlock)CommandBarReadout!).Text = $"IsOpen={bar.IsOpen} - the overflow list is a popup the bar builds for itself, and its colour is the one row this batch had to withhold.";
        };

        var compact = false;
        ((Button)CommandBarCompactButton!).Click += (_, _) =>
        {
            compact = !compact;
            foreach (var element in bar.PrimaryCommands.OfType<AppBarButton>())
            {
                element.IsCompact = compact;
            }

            ((Button)CommandBarCompactButton!).Content = compact ? "Show the labels" : "Compact the buttons";
            ((TextBlock)CommandBarReadout!).Text = compact
                ? "IsCompact dropped every label - the one application-view state this runtime exposes as a property a cell can watch."
                : "Labels back: the side-by-side layout upstream has is picked by the bar's DefaultLabelPosition, and the bar never builds a template of ours, so no label position but this one is reachable.";
        };

        ((Button)CommandBarClearButton!).Click += (_, _) =>
        {
            bold.IsChecked = false;
            ((TextBlock)CommandBarReadout!).Text = "Check cleared: the accent fill left with it.";
        };
    }

    private void WireMenus()
    {
        foreach (var title in new[] { "View", "Help" })
        {
            ((MenuBar)SampleMenuBar!).Items.Add(new MenuBarItem { Title = title });
        }

        var menu = (Menu)SampleMenu!;
        menu.AddHandler(MenuItem.ClickEvent, new RoutedEventHandler((sender, arguments) =>
            ((TextBlock)MenuBarReadout!).Text =
            $"{((MenuItem)arguments.OriginalSource!).Header} clicked - the label and the disabled colour are ours; the highlight and the check mark are the control's own drawing."));

        var flyout = new MenuFlyout();
        flyout.Items.Add(new MenuFlyoutItem
        {
            Text = "Rename",
            Icon = new TextBlock { Text = "\uE713", FontFamily = new FontFamily("Segoe Fluent Icons"), FontSize = 14 },
            KeyboardAcceleratorTextOverride = "F2",
        });
        flyout.Items.Add(new MenuFlyoutItem { Text = "Archive", KeyboardAcceleratorTextOverride = "Ctrl+Shift+A" });
        flyout.Items.Add(new ToggleMenuFlyoutItem { Text = "Show read receipts", IsChecked = true });
        flyout.Items.Add(new MenuFlyoutSeparator());
        var share = new MenuFlyoutSubItem { Text = "Share with" };
        share.Items.Add(new MenuFlyoutItem { Text = "Team channel" });
        share.Items.Add(new MenuFlyoutItem { Text = "Direct link" });
        flyout.Items.Add(share);
        flyout.Items.Add(new MenuFlyoutItem { Text = "Delete", IsEnabled = false });
        flyout.Closed += (_, _) => ((TextBlock)MenuReadout!).Text = "Flyout closed.";

        ((Button)ShowFlyoutButton!).Click += (_, _) => flyout.ShowAt((Button)ShowFlyoutButton!);

        var contextMenu = new ContextMenu();
        contextMenu.Items.Add(new MenuItem { Header = "Copy path", Icon = new TextBlock { Text = "\uE8C8", FontFamily = new FontFamily("Segoe Fluent Icons"), FontSize = 14 } });
        contextMenu.Items.Add(new MenuItem { Header = "Open containing folder" });
        contextMenu.Items.Add(new Separator());
        contextMenu.Items.Add(new MenuItem { Header = "Properties", IsEnabled = false });
        ((Button)ShowContextMenuButton!).Click += (_, _) => contextMenu.Open(new Point(360, 320));
    }

    private void WireSurfaces()
    {
        var expander = (Expander)SampleExpander!;
        expander.Expanded += (_, _) => ((TextBlock)ExpanderReadout!).Text =
            "Expanded. The control wrote the content part's visibility and rotated its chevron; this page drives neither.";
        expander.Collapsed += (_, _) => ((TextBlock)ExpanderReadout!).Text = "Collapsed.";

        var bar = (FluentInfoBar)SampleInfoBar!;
        bar.CloseButtonClick += (_, _) =>
        {
            ((TextBlock)InfoBarReadout!).Text =
                "CloseButtonClick came from the base class, which also set IsOpen=false; the style collapses the bar.";
            Report("Info bar closed.");
        };

        foreach (var (button, severity) in new[]
                 {
                     ((Button)SeverityInformationalButton!, InfoBarSeverity.Informational),
                     ((Button)SeveritySuccessButton!, InfoBarSeverity.Success),
                     ((Button)SeverityWarningButton!, InfoBarSeverity.Warning),
                     ((Button)SeverityErrorButton!, InfoBarSeverity.Error),
                 })
        {
            button.Click += (_, _) =>
            {
                bar.Severity = severity;
                bar.IsOpen = true;
                ((TextBlock)InfoBarReadout!).Text = $"Severity {severity}: the fill is upstream's severity row.";
            };
        }

        ((Button)ShowInfoBarButton!).Click += (_, _) =>
        {
            bar.IsOpen = true;
            ((TextBlock)InfoBarReadout!).Text = "IsOpen=true again.";
        };
        ((ToggleButton)InfoBarClosableToggle!).Click += (_, _) =>
            bar.IsClosable = ((ToggleButton)InfoBarClosableToggle!).IsChecked == true;
        ((ToggleButton)InfoBarIconToggle!).Click += (_, _) =>
            bar.IsIconVisible = ((ToggleButton)InfoBarIconToggle!).IsChecked == true;

        foreach (var (button, shapes) in new[]
                 {
                     ((Button)DialogThreeButton!, 3),
                     ((Button)DialogTwoButton!, 2),
                     ((Button)DialogOneButton!, 1),
                 })
        {
            button.Click += (_, _) => _ = ShowDialogAsync(shapes);
        }

        WireTeachingTip();
    }

    /// <summary>
    /// The tip is on the page rather than built on the click, because that is the shape the control exists for:
    /// it points at an element that is already in the tree. The one thing it cannot do here is name that element
    /// in markup - {x:Bind} is dropped silently on this runtime - so the target is assigned from code.
    /// </summary>
    private void WireTeachingTip()
    {
        var tip = (FluentTeachingTip)SampleTeachingTip!;
        var anchor = (Button)TipShowButton!;
        var readout = (TextBlock)TipReadout!;
        var target = anchor;
        var reason = "Nothing has happened yet.";
        tip.Target = target;

        anchor.Click += (_, _) =>
        {
            tip.Target = target;
            reason = $"Opened at the {tip.PreferredPlacement} side.";
            tip.IsOpen = !tip.IsOpen;
        };

        ((Button)TipSideButton!).Click += (_, _) =>
        {
            tip.PreferredPlacement = tip.PreferredPlacement switch
            {
                FluentTeachingTipPlacementMode.Top => FluentTeachingTipPlacementMode.Right,
                FluentTeachingTipPlacementMode.Right => FluentTeachingTipPlacementMode.Bottom,
                FluentTeachingTipPlacementMode.Bottom => FluentTeachingTipPlacementMode.Left,
                _ => FluentTeachingTipPlacementMode.Top,
            };
            reason = $"Preferred side is {tip.PreferredPlacement}.";
            if (tip.IsOpen)
            {
                readout.Text = reason + $" The control put it on {tip.EffectivePlacement}.";
            }
        };

        ((Button)TipNoTargetButton!).Click += (_, _) =>
        {
            target = target is null ? anchor : null!;
            tip.Target = target;
            reason = target is null
                ? "No target: the tail is gone and Auto falls to Bottom."
                : "Targeted again.";
            if (tip.IsOpen)
            {
                readout.Text = reason;
            }
        };

        ((ToggleButton)TipTailToggle!).Click += (_, _) =>
        {
            tip.TailVisibility = ((ToggleButton)TipTailToggle!).IsChecked == true
                ? FluentTeachingTipTailVisibility.Auto
                : FluentTeachingTipTailVisibility.Collapsed;
            reason = $"TailVisibility is {tip.TailVisibility}.";
            if (tip.IsOpen)
            {
                readout.Text = reason;
            }
        };

        ((ToggleButton)TipButtonsToggle!).Click += (_, _) =>
        {
            var both = ((ToggleButton)TipButtonsToggle!).IsChecked == true;
            tip.ActionButtonContent = both ? "Got it" : null;
            tip.CloseButtonContent = both ? "Dismiss" : null;
            reason = both ? "Both buttons are back." : "No button content: the row collapses.";
            if (tip.IsOpen)
            {
                readout.Text = reason;
            }
        };

        tip.Opened += (_, _) => readout.Text = reason + $" Card on {tip.EffectivePlacement}, " +
            $"{(tip.Target is null ? "no target" : "target " + tip.Target.GetType().Name)}.";
        tip.Closed += (_, _) => readout.Text = "Closed. " + reason;
        tip.ActionButtonClick += (_, _) =>
        {
            // Upstream's action button announces itself and leaves the card open; closing here is this page's choice.
            reason = "ActionButtonClick fired and the page closed the tip.";
            tip.IsOpen = false;
        };
        tip.CloseButtonClick += (_, _) => reason = "CloseButtonClick fired; the control set IsOpen=false.";
    }

    /// <summary>
    /// Builds the dialog on the click instead of placing one on the page: the control hosts a shown dialog in the
    /// window's overlay layer and throws if the instance is already in a tree, which is also why the gate opens
    /// one through ShowAsync rather than mounting it (docs/astra/audits/content-dialog.md).
    /// </summary>
    private async Task ShowDialogAsync(int shapes)
    {
        var dialog = new ContentDialog
        {
            Title = "Publish this draft?",
            Content = "Once it is public, readers and search engines can see the version as of now.",
            PrimaryButtonText = shapes >= 2 ? "Publish" : null!,
            SecondaryButtonText = shapes == 3 ? "Save draft" : null!,
            CloseButtonText = "Cancel",
            FullSizeDesired = ((ToggleButton)DialogFullSizeToggle!).IsChecked == true,
        };
        if (((ToggleButton)DialogDefaultToggle!).IsChecked == true)
        {
            dialog.DefaultButton = ContentDialogButton.Primary;
        }

        var result = await dialog.ShowAsync();
        ((TextBlock)DialogReadout!).Text = $"ShowAsync completed with {result}; the click that ended it came from the template's own part.";
        Report($"Dialog closed: {result}.");
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

        // The overflow list only reports the last row the control named, because SelectedItems was measured not to
        // follow a SelectedIndex write on this runtime (AstraListBoxTests drives multi-selection through the rows
        // themselves), and a readout that listed nothing would claim the framework does something it does not.
        var overflow = (ListBox)SampleOverflowList!;
        overflow.SelectionChanged += (_, _) => Report($"Overflow list last change: {Describe(overflow.SelectedItem)}.");

        var view = (ListView)SampleListView!;
        view.SelectionChanged += (_, _) => Report($"List view selection: {Describe(view.SelectedItem)}.");

        var tree = (TreeView)SampleTreeView!;
        tree.SelectedItemChanged += (_, _) => Report($"Tree selection: {Describe(tree.SelectedItem)}.");
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
    /// <remarks>
    /// Status and open gaps land in one TextBlock on purpose. Two stacked wrapping TextBlocks overlap
    /// here: the first is measured at a wider constraint than it is eventually arranged at, so it asks
    /// for one line and paints two, and the second is placed on top of the overflow. A single element
    /// inside the footer's fixed-height scroll host cannot do that.
    /// </remarks>
    private void ShowParity(FluentNavigationItem? item)
    {
        var status = (TextBlock)ParityStatus!;
        if (item is null)
        {
            status.Text = string.Empty;
            return;
        }

        if (_catalog.Controls.Length == 0)
        {
            status.Text = "Catalog.json did not reach the output folder, so no parity claim on this page is checked.";
            return;
        }

        if (!_pageIds.TryGetValue(item, out var pageId))
        {
            status.Text = $"{item.Content} is not in the catalog.";
            return;
        }

        var onPage = _catalog.OnPage(pageId);
        var title = _catalog.Page(pageId)?.Title ?? pageId;
        var claim = onPage.Length == 0
            ? $"{title} · composes the {_catalog.Controls.Length} restyled types listed on their own pages"
            : $"{title} · {string.Join(", ", onPage.Select(control => $"{control.Name} {control.Parity}"))} · {onPage.Length} of {_catalog.Controls.Length} restyled types";
        var open = _catalog.GapsLine(pageId);
        status.Text = open.Length == 0 ? claim : $"{claim}\nNot claimed: {open}";
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
