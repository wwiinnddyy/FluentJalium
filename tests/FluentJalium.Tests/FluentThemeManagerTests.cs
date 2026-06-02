using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Data;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using JaliumThemeManager = Jalium.UI.Controls.Themes.ThemeManager;

namespace FluentJalium.Tests;

[Collection("Application")]
public sealed class FluentThemeManagerTests
{
    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void Apply_ShouldInsertFluentDictionariesOnlyOnce()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            var baseCount = app.Resources.MergedDictionaries.Count;

            FluentThemeManager.Apply(app);
            var firstCount = app.Resources.MergedDictionaries.Count;

            FluentThemeManager.Apply(app);
            var secondCount = app.Resources.MergedDictionaries.Count;

            Assert.Equal(baseCount + 3, firstCount);
            Assert.Equal(firstCount, secondCount);
            Assert.True(app.Resources.TryGetValue(typeof(Button), out var style));
            Assert.IsType<Style>(style);
            AssertBasedOnStyle<FWButton, Button>(app.Resources);
            AssertBasedOnStyle<FWRepeatButton, RepeatButton>(app.Resources);
            AssertBasedOnStyle<FWHyperlinkButton, HyperlinkButton>(app.Resources);
            AssertBasedOnStyle<FWCheckBox, CheckBox>(app.Resources);
            AssertBasedOnStyle<FWRadioButton, RadioButton>(app.Resources);
            AssertBasedOnStyle<FWToggleButton, ToggleButton>(app.Resources);
            AssertBasedOnStyle<FWToggleSwitch, ToggleSwitch>(app.Resources);
            AssertBasedOnStyle<FWSlider, Slider>(app.Resources);
            AssertBasedOnStyle<FWRangeSlider, RangeSlider>(app.Resources);
            AssertBasedOnStyle<FWProgressBar, ProgressBar>(app.Resources);
            AssertBasedOnStyle<FWComboBox, ComboBox>(app.Resources);
            AssertBasedOnStyle<FWComboBoxItem, ComboBoxItem>(app.Resources);
            AssertBasedOnStyle<FWListBox, ListBox>(app.Resources);
            AssertBasedOnStyle<FWListBoxItem, ListBoxItem>(app.Resources);
            AssertBasedOnStyle<FWListView, ListView>(app.Resources);
            AssertBasedOnStyle<FWListViewItem, ListViewItem>(app.Resources);
            AssertBasedOnStyle<FWTreeView, TreeView>(app.Resources);
            AssertBasedOnStyle<FWTreeViewItem, TreeViewItem>(app.Resources);
            AssertBasedOnStyle<FWDataGrid, DataGrid>(app.Resources);
            AssertBasedOnStyle<FWTreeDataGrid, TreeDataGrid>(app.Resources);
            AssertBasedOnStyle<FWNavigationView, NavigationView>(app.Resources);
            AssertBasedOnStyle<FWNavigationViewItem, NavigationViewItem>(app.Resources);
            AssertBasedOnStyle<FWNavigationViewItemHeader, NavigationViewItemHeader>(app.Resources);
            AssertBasedOnStyle<FWNavigationViewItemSeparator, NavigationViewItemSeparator>(app.Resources);
            AssertBasedOnStyle<FWTabControl, TabControl>(app.Resources);
            AssertBasedOnStyle<FWTabItem, TabItem>(app.Resources);
            AssertBasedOnStyle<FWFrame, Frame>(app.Resources);
            AssertBasedOnStyle<FWDatePicker, DatePicker>(app.Resources);
            AssertBasedOnStyle<FWTimePicker, TimePicker>(app.Resources);
            AssertBasedOnStyle<FWCalendar, Calendar>(app.Resources);
            AssertOwnedStyle<FWProgressRing>(app.Resources);
            AssertOwnedStyle<FWDropDownButton>(app.Resources);
            AssertBasedOnStyle<FWSplitButton, SplitButton>(app.Resources);
            AssertOwnedStyle<FWToggleSplitButton>(app.Resources);
            AssertBasedOnStyle<FWAppBarButton, AppBarButton>(app.Resources);
            AssertBasedOnStyle<FWAppBarToggleButton, AppBarToggleButton>(app.Resources);
            AssertBasedOnStyle<FWAppBarSeparator, AppBarSeparator>(app.Resources);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void ApplyTheme_ShouldUpdateCurrentThemeKeyAndThemeResources()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);
            var darkWindowBackground = GetBrushColor(app.Resources["WindowBackground"]);

            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            var lightWindowBackground = GetBrushColor(app.Resources["WindowBackground"]);

            Assert.Equal(FluentThemeVariant.Light, FluentThemeManager.CurrentTheme);
            Assert.Equal("Light", ResourceDictionary.CurrentThemeKey);
            Assert.NotEqual(darkWindowBackground, lightWindowBackground);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void ApplyAccent_ShouldGenerateDefaultHoverPressedAndDisabledBrushes()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            var accent = Color.FromRgb(0xD8, 0x3B, 0x01);
            FluentThemeManager.ApplyAccent(accent);

            Assert.Equal(accent, FluentThemeManager.CurrentAccentColor);
            Assert.Equal(accent, GetBrushColor(app.Resources["AccentBrush"]));
            Assert.Equal(accent, GetBrushColor(app.Resources["FluentAccentBrush"]));
            Assert.IsType<SolidColorBrush>(app.Resources["AccentBrushHover"]);
            Assert.IsType<SolidColorBrush>(app.Resources["AccentBrushPressed"]);
            Assert.IsType<SolidColorBrush>(app.Resources["AccentBrushDisabled"]);
            Assert.IsType<SolidColorBrush>(app.Resources["FluentAccentBrushHover"]);
            Assert.IsType<SolidColorBrush>(app.Resources["FluentAccentBrushPressed"]);
            Assert.IsType<SolidColorBrush>(app.Resources["FluentAccentBrushDisabled"]);
            Assert.Equal(accent, GetBrushColor(app.Resources["TabItemIndicator"]));
            Assert.Equal(Color.FromArgb(0x33, accent.R, accent.G, accent.B), GetBrushColor(app.Resources["NavigationViewItemBackgroundSelected"]));
            Assert.Equal(Color.FromArgb(0x66, accent.R, accent.G, accent.B), GetBrushColor(app.Resources["NavigationViewItemBackgroundSelectedHover"]));
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void GenericTheme_ShouldLoadFromPackPathAndExposeCoreControlStyles()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = FluentThemeVariant.Dark.ToString();

        var owner = new ResourceDictionary();
        var loaded = ResourceDictionary.SourceLoader?.Invoke(
            owner,
            new Uri("/FluentJalium;component/Themes/Generic.jalxaml", UriKind.Relative),
            FluentThemeManager.ThemeAssembly);

        Assert.NotNull(loaded);
        var dictionary = loaded!;

        AssertContainsStyle<Button>(dictionary);
        AssertContainsStyle<RepeatButton>(dictionary);
        AssertContainsStyle<HyperlinkButton>(dictionary);
        AssertContainsStyle<CheckBox>(dictionary);
        AssertContainsStyle<RadioButton>(dictionary);
        AssertContainsStyle<ToggleButton>(dictionary);
        AssertContainsStyle<ToggleSwitch>(dictionary);
        AssertContainsStyle<Slider>(dictionary);
        AssertContainsStyle<RangeSlider>(dictionary);
        AssertContainsStyle<ProgressBar>(dictionary);
        AssertContainsStyle<ComboBox>(dictionary);
        AssertContainsStyle<ComboBoxItem>(dictionary);
        AssertContainsStyle<ListBox>(dictionary);
        AssertContainsStyle<ListBoxItem>(dictionary);
        AssertContainsStyle<ListView>(dictionary);
        AssertContainsStyle<ListViewItem>(dictionary);
        AssertContainsStyle<GridViewColumnHeader>(dictionary);
        AssertContainsStyle<TreeView>(dictionary);
        AssertContainsStyle<TreeViewItem>(dictionary);
        AssertContainsStyle<DataGrid>(dictionary);
        AssertContainsStyle<DataGridRow>(dictionary);
        AssertContainsStyle<DataGridCell>(dictionary);
        AssertContainsStyle<Jalium.UI.Controls.DataGridColumnHeader>(dictionary);
        AssertContainsStyle<TreeDataGrid>(dictionary);
        AssertContainsStyle<TreeDataGridRow>(dictionary);
        AssertContainsStyle<NavigationView>(dictionary);
        AssertContainsStyle<NavigationViewItem>(dictionary);
        AssertContainsStyle<NavigationViewItemHeader>(dictionary);
        AssertContainsStyle<NavigationViewItemSeparator>(dictionary);
        AssertContainsStyle<TabControl>(dictionary);
        AssertContainsStyle<TabItem>(dictionary);
        AssertContainsStyle<Frame>(dictionary);
        AssertContainsStyle<DatePicker>(dictionary);
        AssertContainsStyle<TimePicker>(dictionary);
        AssertContainsStyle<Calendar>(dictionary);
        AssertContainsStyle<CalendarButton>(dictionary);
        AssertContainsStyle<CalendarDayButton>(dictionary);
        AssertContainsStyle<CalendarItem>(dictionary);
        AssertContainsStyle<DatePickerTextBox>(dictionary);
        AssertContainsStyle<FWProgressRing>(dictionary);
        AssertContainsStyle<FWDropDownButton>(dictionary);
        AssertContainsStyle<SplitButton>(dictionary);
        AssertContainsStyle<FWToggleSplitButton>(dictionary);
        AssertContainsStyle<CommandBar>(dictionary);
        AssertContainsStyle<AppBarButton>(dictionary);
        AssertContainsStyle<AppBarToggleButton>(dictionary);
        AssertContainsStyle<AppBarSeparator>(dictionary);
        Assert.True(dictionary.Contains("TextPrimary"));
        Assert.True(dictionary.Contains("AccentBrush"));
        Assert.True(dictionary.Contains("ToggleCheckedBackground"));
        Assert.True(dictionary.Contains("ToggleUncheckedBackground"));
        Assert.True(dictionary.Contains("ToggleDisabledBackground"));
        Assert.True(dictionary.Contains("SliderTrack"));
        Assert.True(dictionary.Contains("SliderThumb"));
        Assert.True(dictionary.Contains("ProgressRingForeground"));
        Assert.True(dictionary.Contains("SelectionBackgroundWeak"));
        Assert.True(dictionary.Contains("DividerStrokeColorDefaultBrush"));
        Assert.True(dictionary.Contains("CommandBarBackground"));
        Assert.True(dictionary.Contains("AppBarButtonBackground"));
        Assert.True(dictionary.Contains("AppBarButtonBackgroundHover"));
        Assert.True(dictionary.Contains("AppBarButtonBackgroundPressed"));
        Assert.True(dictionary.Contains("NavigationViewPaneBackground"));
        Assert.True(dictionary.Contains("NavigationViewItemBackgroundSelected"));
        Assert.True(dictionary.Contains("TabStripBackground"));
        Assert.True(dictionary.Contains("TabItemIndicator"));
        Assert.True(dictionary.Contains("FrameBackground"));
        Assert.True(dictionary.Contains("ControlContentThemeFontSize"));
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void ButtonBatch_ShouldExposeFwStylesForButtonAndCommandControls()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            AssertBasedOnStyle<FWButton, Button>(app.Resources);
            AssertBasedOnStyle<FWRepeatButton, RepeatButton>(app.Resources);
            AssertBasedOnStyle<FWHyperlinkButton, HyperlinkButton>(app.Resources);
            AssertOwnedStyle<FWDropDownButton>(app.Resources);
            AssertBasedOnStyle<FWSplitButton, SplitButton>(app.Resources);
            AssertOwnedStyle<FWToggleSplitButton>(app.Resources);
            AssertBasedOnStyle<FWAppBarButton, AppBarButton>(app.Resources);
            AssertBasedOnStyle<FWAppBarToggleButton, AppBarToggleButton>(app.Resources);
            AssertBasedOnStyle<FWAppBarSeparator, AppBarSeparator>(app.Resources);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void SwitchBatch_ShouldExposeFwStylesForSwitchControls()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            AssertBasedOnStyle<FWToggleButton, ToggleButton>(app.Resources);
            AssertBasedOnStyle<FWToggleSwitch, ToggleSwitch>(app.Resources);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void RangeBatch_ShouldExposeFwStylesForRangeControls()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            AssertBasedOnStyle<FWSlider, Slider>(app.Resources);
            AssertBasedOnStyle<FWRangeSlider, RangeSlider>(app.Resources);
            AssertBasedOnStyle<FWProgressBar, ProgressBar>(app.Resources);
            AssertOwnedStyle<FWProgressRing>(app.Resources);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void SelectionBatch_ShouldExposeFwStylesForSelectionControls()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            AssertBasedOnStyle<FWCheckBox, CheckBox>(app.Resources);
            AssertBasedOnStyle<FWRadioButton, RadioButton>(app.Resources);
            AssertBasedOnStyle<FWComboBox, ComboBox>(app.Resources);
            AssertBasedOnStyle<FWComboBoxItem, ComboBoxItem>(app.Resources);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void CollectionsBatch_ShouldExposeFwStylesForCollectionAndTableControls()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            AssertBasedOnStyle<FWListBox, ListBox>(app.Resources);
            AssertBasedOnStyle<FWListBoxItem, ListBoxItem>(app.Resources);
            AssertBasedOnStyle<FWListView, ListView>(app.Resources);
            AssertBasedOnStyle<FWListViewItem, ListViewItem>(app.Resources);
            AssertBasedOnStyle<FWTreeView, TreeView>(app.Resources);
            AssertBasedOnStyle<FWTreeViewItem, TreeViewItem>(app.Resources);
            AssertBasedOnStyle<FWDataGrid, DataGrid>(app.Resources);
            AssertBasedOnStyle<FWTreeDataGrid, TreeDataGrid>(app.Resources);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void NavigationBatch_ShouldExposeFwStylesForNavigationControls()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            AssertBasedOnStyle<FWNavigationView, NavigationView>(app.Resources);
            AssertBasedOnStyle<FWNavigationViewItem, NavigationViewItem>(app.Resources);
            AssertBasedOnStyle<FWNavigationViewItemHeader, NavigationViewItemHeader>(app.Resources);
            AssertBasedOnStyle<FWNavigationViewItemSeparator, NavigationViewItemSeparator>(app.Resources);
            AssertBasedOnStyle<FWTabControl, TabControl>(app.Resources);
            AssertBasedOnStyle<FWTabItem, TabItem>(app.Resources);
            AssertBasedOnStyle<FWFrame, Frame>(app.Resources);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void DateTimeBatch_ShouldExposeFwStylesForDateTimeControls()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            AssertBasedOnStyle<FWDatePicker, DatePicker>(app.Resources);
            AssertBasedOnStyle<FWTimePicker, TimePicker>(app.Resources);
            AssertBasedOnStyle<FWCalendar, Calendar>(app.Resources);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    public void FluentControls_ShouldExposeFwPrefixedButtonSurface()
    {
        AssertFluentControl<FWButton, Button>();
        AssertFluentControl<FWRepeatButton, RepeatButton>();
        AssertFluentControl<FWHyperlinkButton, HyperlinkButton>();
        AssertFluentControl<FWDropDownButton, Button>();
        AssertFluentControl<FWSplitButton, SplitButton>();
        AssertFluentControl<FWToggleSplitButton, SplitButton>();
        AssertFluentControl<FWAppBarButton, AppBarButton>();
        AssertFluentControl<FWAppBarToggleButton, AppBarToggleButton>();
        AssertFluentControl<FWAppBarSeparator, AppBarSeparator>();
    }

    [Fact]
    public void FluentSwitchControls_ShouldExposeFwPrefixedSurface()
    {
        AssertFluentControl<FWToggleButton, ToggleButton>();
        AssertFluentControl<FWToggleSwitch, ToggleSwitch>();
    }

    [Fact]
    public void FluentSelectionControls_ShouldExposeFwPrefixedSurface()
    {
        AssertFluentControl<FWCheckBox, CheckBox>();
        AssertFluentControl<FWRadioButton, RadioButton>();
        AssertFluentControl<FWComboBox, ComboBox>();
        AssertFluentControl<FWComboBoxItem, ComboBoxItem>();
    }

    [Fact]
    public void FluentCollectionControls_ShouldExposeFwPrefixedSurface()
    {
        AssertFluentControl<FWListBox, ListBox>();
        AssertFluentControl<FWListBoxItem, ListBoxItem>();
        AssertFluentControl<FWListView, ListView>();
        AssertFluentControl<FWListViewItem, ListViewItem>();
        AssertFluentControl<FWTreeView, TreeView>();
        AssertFluentControl<FWTreeViewItem, TreeViewItem>();
        AssertFluentControl<FWDataGrid, DataGrid>();
        AssertFluentControl<FWTreeDataGrid, TreeDataGrid>();
    }

    [Fact]
    public void FluentNavigationControls_ShouldExposeFwPrefixedSurface()
    {
        AssertFluentControl<FWNavigationView, NavigationView>();
        AssertFluentControl<FWNavigationViewItem, NavigationViewItem>();
        AssertFluentControl<FWNavigationViewItemHeader, NavigationViewItemHeader>();
        AssertFluentControl<FWNavigationViewItemSeparator, NavigationViewItemSeparator>();
        AssertFluentControl<FWTabControl, TabControl>();
        AssertFluentControl<FWTabItem, TabItem>();
        AssertFluentControl<FWFrame, Frame>();
    }

    [Fact]
    public void FluentDateTimeControls_ShouldExposeFwPrefixedSurface()
    {
        AssertFluentControl<FWDatePicker, DatePicker>();
        AssertFluentControl<FWTimePicker, TimePicker>();
        AssertFluentControl<FWCalendar, Calendar>();
    }

    [Fact]
    public void FWToggleButton_ShouldCycleCheckedStatesAndRaiseEvents()
    {
        var button = new FWToggleButton
        {
            IsThreeState = true
        };
        var checkedCount = 0;
        var uncheckedCount = 0;
        var indeterminateCount = 0;

        button.Checked += (_, _) => checkedCount++;
        button.Unchecked += (_, _) => uncheckedCount++;
        button.Indeterminate += (_, _) => indeterminateCount++;

        InvokeToggleButtonClick(button);
        Assert.True(button.IsChecked);
        Assert.Equal(1, checkedCount);

        InvokeToggleButtonClick(button);
        Assert.Null(button.IsChecked);
        Assert.Equal(1, indeterminateCount);

        InvokeToggleButtonClick(button);
        Assert.False(button.IsChecked);
        Assert.Equal(1, uncheckedCount);
    }

    [Fact]
    public void FWToggleSwitch_ShouldChangeIsOnAndRaiseToggled()
    {
        var toggleSwitch = new FWToggleSwitch
        {
            OffContent = "Off",
            OnContent = "On"
        };
        var toggled = 0;

        toggleSwitch.Toggled += (_, _) => toggled++;

        toggleSwitch.IsOn = true;

        Assert.True(toggleSwitch.IsOn);
        Assert.Equal(1, toggled);
        Assert.Equal("On", toggleSwitch.OnContent);

        toggleSwitch.IsOn = false;

        Assert.False(toggleSwitch.IsOn);
        Assert.Equal(2, toggled);
        Assert.Equal("Off", toggleSwitch.OffContent);
    }

    [Fact]
    public void FWCheckBox_ShouldCycleCheckedStatesAndRaiseEvents()
    {
        var checkBox = new FWCheckBox
        {
            IsThreeState = true
        };
        var checkedCount = 0;
        var uncheckedCount = 0;
        var indeterminateCount = 0;

        checkBox.Checked += (_, _) => checkedCount++;
        checkBox.Unchecked += (_, _) => uncheckedCount++;
        checkBox.Indeterminate += (_, _) => indeterminateCount++;

        InvokeToggleButtonClick(checkBox);
        Assert.True(checkBox.IsChecked);
        Assert.Equal(1, checkedCount);

        InvokeToggleButtonClick(checkBox);
        Assert.Null(checkBox.IsChecked);
        Assert.Equal(1, indeterminateCount);

        InvokeToggleButtonClick(checkBox);
        Assert.False(checkBox.IsChecked);
        Assert.Equal(1, uncheckedCount);
    }

    [Fact]
    public void FWRadioButton_ShouldKeepGroupSelectionExclusive()
    {
        var groupName = $"selection-{Guid.NewGuid():N}";
        var first = new FWRadioButton
        {
            Content = "One",
            GroupName = groupName
        };
        var second = new FWRadioButton
        {
            Content = "Two",
            GroupName = groupName
        };
        var checkedCount = 0;

        first.Checked += (_, _) => checkedCount++;
        second.Checked += (_, _) => checkedCount++;

        InvokeToggleButtonClick(first);
        Assert.True(first.IsChecked);
        Assert.False(second.IsChecked);

        InvokeToggleButtonClick(second);
        Assert.False(first.IsChecked);
        Assert.True(second.IsChecked);
        Assert.Equal(2, checkedCount);

        InvokeToggleButtonClick(second);
        Assert.False(first.IsChecked);
        Assert.True(second.IsChecked);
        Assert.Equal(2, checkedCount);
    }

    [Fact]
    public void FWComboBox_ShouldSynchronizeSelectionTextAndDropDownEvents()
    {
        var comboBox = new FWComboBox
        {
            PlaceholderText = "Choose an item"
        };
        comboBox.Items.Add("Fluent tokens");
        comboBox.Items.Add("Control styles");
        comboBox.Items.Add("Gallery sample");

        var selectionChanged = 0;
        var opened = 0;
        var closed = 0;
        comboBox.SelectionChanged += (_, _) => selectionChanged++;
        comboBox.DropDownOpened += (_, _) => opened++;
        comboBox.DropDownClosed += (_, _) => closed++;

        comboBox.SelectedIndex = 1;

        Assert.Equal(1, comboBox.SelectedIndex);
        Assert.Equal("Control styles", comboBox.SelectedItem);
        Assert.Equal("Control styles", comboBox.SelectedValue);
        Assert.Equal("Control styles", comboBox.SelectionBoxItem);
        Assert.Equal(1, selectionChanged);

        comboBox.IsEditable = true;
        comboBox.Text = "Gallery sample";

        Assert.Equal(2, comboBox.SelectedIndex);
        Assert.Equal("Gallery sample", comboBox.SelectionBoxItem);

        comboBox.IsDropDownOpen = true;
        comboBox.IsDropDownOpen = false;

        Assert.False(comboBox.IsDropDownOpen);
        Assert.Equal(1, opened);
        Assert.Equal(1, closed);
    }

    [Fact]
    public void FWComboBoxItem_ShouldExposeSelectionState()
    {
        var item = new FWComboBoxItem
        {
            Content = "Option"
        };

        Assert.False(item.IsSelected);

        item.IsSelected = true;

        Assert.True(item.IsSelected);
        Assert.Equal("Option", item.Content);
    }

    [Fact]
    public void FluentRangeControls_ShouldExposeFwPrefixedSurface()
    {
        AssertFluentControl<FWSlider, Slider>();
        AssertFluentControl<FWRangeSlider, RangeSlider>();
        AssertFluentControl<FWProgressBar, ProgressBar>();
        AssertFluentControl<FWProgressRing, RangeBase>();
    }

    [Fact]
    public void FWRangeControls_ShouldCoerceValuesAndRaiseEvents()
    {
        var slider = new FWSlider
        {
            Minimum = 0,
            Maximum = 10
        };
        var sliderChanged = 0;
        slider.ValueChanged += (_, _) => sliderChanged++;

        slider.Value = 15;

        Assert.Equal(10, slider.Value);
        Assert.Equal(1, sliderChanged);

        var progressBar = new FWProgressBar
        {
            Minimum = 0,
            Maximum = 100
        };
        var progressChanged = 0;
        progressBar.ValueChanged += (_, _) => progressChanged++;

        progressBar.Value = 250;

        Assert.Equal(100, progressBar.Value);
        Assert.Equal(1, progressChanged);

        var rangeSlider = new FWRangeSlider
        {
            Minimum = 0,
            Maximum = 100,
            RangeStart = 20,
            RangeEnd = 80,
            MinimumRange = 10
        };

        rangeSlider.RangeStart = 95;
        Assert.Equal(70, rangeSlider.RangeStart);

        rangeSlider.RangeEnd = 50;
        Assert.Equal(80, rangeSlider.RangeEnd);
    }

    [Fact]
    public void FWProgressRing_ShouldUseRangeStateAndProgressProperties()
    {
        var ring = new FWProgressRing
        {
            Minimum = 0,
            Maximum = 100,
            StrokeThickness = 6,
            IsIndeterminate = false
        };
        var changed = 0;
        ring.ValueChanged += (_, _) => changed++;

        ring.Value = 128;

        Assert.Equal(100, ring.Value);
        Assert.Equal(1, changed);
        Assert.Equal(6, ring.StrokeThickness);

        ring.IsActive = false;
        ring.IsIndeterminate = true;

        Assert.False(ring.IsActive);
        Assert.True(ring.IsIndeterminate);
    }

    [Fact]
    public void FWListBox_ShouldSynchronizeSelectionAndSelectedItems()
    {
        var listBox = new FWListBox();
        listBox.Items.Add("Inbox");
        listBox.Items.Add("Archive");
        listBox.Items.Add("Deleted");

        var selectionChanged = 0;
        listBox.SelectionChanged += (_, _) => selectionChanged++;

        listBox.SelectedIndex = 1;

        Assert.Equal(1, listBox.SelectedIndex);
        Assert.Equal("Archive", listBox.SelectedItem);
        Assert.Equal(1, selectionChanged);

        listBox.SelectionMode = SelectionMode.Multiple;
        listBox.SelectAll();

        Assert.Equal(3, listBox.SelectedItems.Count);

        listBox.UnselectAll();

        Assert.Empty(listBox.SelectedItems);
        Assert.Null(listBox.SelectedItem);
    }

    [Fact]
    public void FWListView_ShouldExposeGridViewColumnsAndSelection()
    {
        var items = new[]
        {
            new CollectionRow("Mail", "Unread", 12),
            new CollectionRow("Calendar", "Pinned", 3)
        };
        var view = new GridView();
        view.Columns.Add(new GridViewColumn
        {
            Header = "Name",
            DisplayMemberBinding = new Binding("Name"),
            Width = 140
        });
        view.Columns.Add(new GridViewColumn
        {
            Header = "State",
            DisplayMemberBinding = new Binding("State"),
            Width = 110
        });

        var listView = new FWListView
        {
            View = view,
            ItemsSource = items
        };

        listView.SelectedIndex = 1;

        Assert.Equal(2, view.Columns.Count);
        Assert.Equal("State", view.Columns[1].Header);
        Assert.Equal(items[1], listView.SelectedItem);
    }

    [Fact]
    public void FWTreeViewItem_ShouldExposeExpandAndSelectionState()
    {
        var root = new FWTreeViewItem
        {
            Header = "Root"
        };
        root.Items.Add(new FWTreeViewItem { Header = "Child" });

        var treeView = new FWTreeView();
        var selectedChanged = 0;
        treeView.SelectedItemChanged += (_, _) => selectedChanged++;
        treeView.Items.Add(root);

        root.IsExpanded = true;
        treeView.SelectedItem = root;

        Assert.True(root.IsExpanded);
        Assert.True(root.IsSelected);
        Assert.Equal(root, treeView.SelectedItem);
        Assert.Equal(1, selectedChanged);
    }

    [Fact]
    public void FWDataGrid_ShouldGenerateColumnsAndSynchronizeSelection()
    {
        var rows = new[]
        {
            new CollectionRow("Inbox", "Unread", 12),
            new CollectionRow("Archive", "Stored", 42)
        };
        var dataGrid = new FWDataGrid
        {
            ItemsSource = rows
        };
        var selectionChanged = 0;
        dataGrid.SelectionChanged += (_, _) => selectionChanged++;

        dataGrid.SelectedIndex = 1;

        Assert.True(dataGrid.Columns.Count >= 3);
        Assert.Equal(1, dataGrid.SelectedIndex);
        Assert.Equal(rows[1], dataGrid.SelectedItem);
        Assert.Contains(rows[1], dataGrid.SelectedItems);
        Assert.Equal(1, selectionChanged);
    }

    [Fact]
    public void FWTreeDataGrid_ShouldExpandCollapseAndSynchronizeSelection()
    {
        var rows = new[]
        {
            new CollectionNode(
                "Workspace",
                "Open",
                [
                    new CollectionNode("Docs", "Synced", []),
                    new CollectionNode("Design", "Review", [])
                ]),
            new CollectionNode("Archive", "Closed", [])
        };
        var grid = new FWTreeDataGrid
        {
            ChildrenSelector = item => ((CollectionNode)item).Children,
            HasChildrenSelector = item => ((CollectionNode)item).Children.Length > 0
        };
        grid.ItemsSource = rows;
        grid.Columns.Add(new DataGridTextColumn
        {
            Header = "Name",
            Binding = new Binding("Name"),
            Width = 160
        });
        grid.Columns.Add(new DataGridTextColumn
        {
            Header = "State",
            Binding = new Binding("State"),
            Width = 120
        });

        Assert.Equal(2, grid.FlattenedCount);

        grid.ExpandAll();
        grid.SelectedIndex = 1;

        Assert.Equal(4, grid.FlattenedCount);
        Assert.Equal(rows[0].Children[0], grid.SelectedItem);
        Assert.Contains(rows[0].Children[0], grid.SelectedItems);

        grid.CollapseAll();

        Assert.Equal(2, grid.FlattenedCount);
    }

    [Fact]
    public void FWNavigationView_ShouldSynchronizeSelectionPaneAndItemEvents()
    {
        var navigationView = new FWNavigationView();
        var first = new FWNavigationViewItem { Content = "Home" };
        var second = new FWNavigationViewItem { Content = "Settings" };
        var invoked = 0;
        var itemInvoked = 0;
        var selectionChanged = 0;
        var opened = 0;
        var closed = 0;

        first.Invoked += (_, _) => invoked++;
        second.Invoked += (_, _) => invoked++;
        navigationView.ItemInvoked += (_, _) => itemInvoked++;
        navigationView.SelectionChanged += (_, _) => selectionChanged++;
        navigationView.PaneOpened += (_, _) => opened++;
        navigationView.PaneClosed += (_, _) => closed++;
        navigationView.MenuItems.Add(first);
        navigationView.MenuItems.Add(second);

        navigationView.SelectedItem = first;
        navigationView.SelectedItem = second;
        navigationView.IsPaneOpen = false;
        navigationView.IsPaneOpen = true;

        Assert.False(first.IsSelected);
        Assert.True(second.IsSelected);
        Assert.Equal(second, navigationView.SelectedItem);
        Assert.Equal(2, invoked);
        Assert.Equal(2, itemInvoked);
        Assert.Equal(2, selectionChanged);
        Assert.Equal(1, closed);
        Assert.Equal(1, opened);
    }

    [Fact]
    public void FWNavigationViewItem_ShouldExposeExpansionAndHierarchyState()
    {
        var item = new FWNavigationViewItem
        {
            Content = "Controls"
        };
        item.MenuItems.Add(new FWNavigationViewItem { Content = "Buttons" });

        var expansionChanged = 0;
        item.ExpansionChanged += (_, expanded) =>
        {
            expansionChanged++;
            Assert.True(expanded);
        };

        item.IsExpanded = true;

        Assert.True(item.HasUnrealizedChildren);
        Assert.True(item.IsExpanded);
        Assert.Equal(1, expansionChanged);
    }

    [Fact]
    public void FWTabControl_ShouldSynchronizeSelectedContentAndTabItems()
    {
        var first = new FWTabItem
        {
            Header = "Overview",
            Content = "First content"
        };
        var second = new FWTabItem
        {
            Header = "Details",
            Content = "Second content"
        };
        var tabControl = new FWTabControl();
        var selectionChanged = 0;
        tabControl.SelectionChanged += (_, _) => selectionChanged++;
        tabControl.Items.Add(first);
        tabControl.Items.Add(second);

        tabControl.SelectedIndex = 1;

        Assert.False(first.IsSelected);
        Assert.True(second.IsSelected);
        Assert.Equal(second, tabControl.SelectedItem);
        Assert.Equal("Second content", tabControl.SelectedContent);
        Assert.Equal(1, selectionChanged);

        tabControl.SelectedIndex = 0;

        Assert.True(first.IsSelected);
        Assert.False(second.IsSelected);
        Assert.Equal("First content", tabControl.SelectedContent);
        Assert.Equal(2, selectionChanged);
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises Frame page activation by type.")]
    public void FWFrame_ShouldNavigateBackAndForward()
    {
        var frame = new FWFrame();
        var navigated = 0;
        frame.Navigated += (_, _) => navigated++;

        Assert.True(frame.Navigate(typeof(NavigationTestPage), "first"));
        Assert.Equal(typeof(NavigationTestPage), frame.SourcePageType);
        Assert.IsType<NavigationTestPage>(frame.CurrentPage);
        Assert.Equal("first", frame.CurrentPage!.NavigationParameter);

        Assert.True(frame.Navigate(typeof(SecondNavigationTestPage), "second"));
        Assert.True(frame.CanGoBack);
        Assert.Equal(1, frame.BackStackDepth);
        Assert.Equal(typeof(SecondNavigationTestPage), frame.SourcePageType);

        Assert.True(frame.GoBack());
        Assert.True(frame.CanGoForward);
        Assert.Equal(typeof(NavigationTestPage), frame.SourcePageType);
        Assert.Equal("first", frame.CurrentPage!.NavigationParameter);

        Assert.True(frame.GoForward());
        Assert.Equal(typeof(SecondNavigationTestPage), frame.SourcePageType);
        Assert.Equal("second", frame.CurrentPage!.NavigationParameter);
        Assert.Equal(4, navigated);
    }

    [Fact]
    public void FWDatePicker_ShouldRaiseSelectionAndCalendarOpenCloseEvents()
    {
        var picker = new FWDatePicker
        {
            Header = "Date",
            PlaceholderText = "Select a date",
            DisplayDate = new DateTime(2026, 6, 1),
            DisplayDateStart = new DateTime(2026, 1, 1),
            DisplayDateEnd = new DateTime(2026, 12, 31),
            SelectedDateFormat = DatePickerFormat.Long
        };
        var selectedChanged = 0;
        var opened = 0;
        var closed = 0;
        SelectionChangedEventArgs? lastSelectionArgs = null;

        picker.SelectedDateChanged += (_, args) =>
        {
            selectedChanged++;
            lastSelectionArgs = args;
        };
        picker.CalendarOpened += (_, _) => opened++;
        picker.CalendarClosed += (_, _) => closed++;

        picker.SelectedDate = new DateTime(2026, 6, 3);
        picker.IsDropDownOpen = true;
        picker.IsDropDownOpen = false;

        Assert.Equal(new DateTime(2026, 6, 3), picker.SelectedDate);
        Assert.Equal(DatePickerFormat.Long, picker.SelectedDateFormat);
        Assert.Equal(1, selectedChanged);
        Assert.NotNull(lastSelectionArgs);
        Assert.Contains(new DateTime(2026, 6, 3), lastSelectionArgs!.AddedItems);
        Assert.Equal(1, opened);
        Assert.Equal(1, closed);
    }

    [Fact]
    public void FWTimePicker_ShouldRaiseSelectedTimeChangedAndHonorClockIdentifier()
    {
        var picker = new FWTimePicker
        {
            Header = "Time",
            ClockIdentifier = "24HourClock",
            MinuteIncrement = 15,
            PlaceholderText = "Select a time"
        };
        var changed = 0;
        TimePickerSelectedValueChangedEventArgs? lastArgs = null;
        picker.SelectedTimeChanged += (_, args) =>
        {
            changed++;
            lastArgs = args;
        };

        picker.SelectedTime = new TimeSpan(18, 45, 0);

        Assert.Equal("24HourClock", picker.ClockIdentifier);
        Assert.Equal(15, picker.MinuteIncrement);
        Assert.Equal(new TimeSpan(18, 45, 0), picker.SelectedTime);
        Assert.Equal(1, changed);
        Assert.NotNull(lastArgs);
        Assert.Null(lastArgs!.OldTime);
        Assert.Equal(new TimeSpan(18, 45, 0), lastArgs.NewTime);
    }

    [Fact]
    public void FWCalendar_ShouldTrackSelectionDisplayDateAndSelectableBounds()
    {
        var calendar = new FWCalendar
        {
            DisplayDate = new DateTime(2026, 6, 1),
            DisplayDateStart = new DateTime(2026, 6, 1),
            DisplayDateEnd = new DateTime(2026, 6, 30),
            FirstDayOfWeek = DayOfWeek.Monday,
            IsTodayHighlighted = true,
            SelectionMode = CalendarSelectionMode.SingleDate
        };
        var selectedChanged = 0;
        var displayChanged = 0;
        SelectionChangedEventArgs? lastSelectionArgs = null;
        CalendarDateChangedEventArgs? lastDisplayArgs = null;
        calendar.SelectedDateChanged += (_, args) =>
        {
            selectedChanged++;
            lastSelectionArgs = args;
        };
        calendar.DisplayDateChanged += (_, args) =>
        {
            displayChanged++;
            lastDisplayArgs = args;
        };

        calendar.BlackoutDates.Add(new DateTime(2026, 6, 10));

        InvokeCalendarSelectDate(calendar, new DateTime(2026, 6, 8));
        InvokeCalendarSelectDate(calendar, new DateTime(2026, 6, 10));
        InvokeCalendarSelectDate(calendar, new DateTime(2026, 7, 1));
        calendar.DisplayDate = new DateTime(2026, 7, 1);

        Assert.Equal(new DateTime(2026, 6, 8), calendar.SelectedDate);
        Assert.Single(calendar.SelectedDates);
        Assert.Equal(new DateTime(2026, 6, 8), calendar.SelectedDates[0]);
        Assert.Contains(new DateTime(2026, 6, 10), calendar.BlackoutDates);
        Assert.Equal(CalendarSelectionMode.SingleDate, calendar.SelectionMode);
        Assert.Equal(DayOfWeek.Monday, calendar.FirstDayOfWeek);
        Assert.Equal(1, selectedChanged);
        Assert.NotNull(lastSelectionArgs);
        Assert.Contains(new DateTime(2026, 6, 8), lastSelectionArgs!.AddedItems);
        Assert.Equal(1, displayChanged);
        Assert.NotNull(lastDisplayArgs);
        Assert.Equal(new DateTime(2026, 6, 1), lastDisplayArgs!.RemovedDate);
        Assert.Equal(new DateTime(2026, 7, 1), lastDisplayArgs.AddedDate);
    }

    [Fact]
    public void FWDropDownButton_ShouldSynchronizeFlyoutOpenState()
    {
        var oldFlyout = new MenuFlyout();
        var newFlyout = new MenuFlyout();
        var button = new FWDropDownButton
        {
            Flyout = oldFlyout
        };

        var opened = 0;
        var closed = 0;
        button.FlyoutOpened += (_, _) => opened++;
        button.FlyoutClosed += (_, _) => closed++;

        oldFlyout.ShowAt(button);
        Assert.True(button.IsFlyoutOpen);
        Assert.Equal(1, opened);

        button.Flyout = newFlyout;
        Assert.False(button.IsFlyoutOpen);
        Assert.False(oldFlyout.IsOpen);
        Assert.Equal(1, closed);

        newFlyout.ShowAt(button);
        Assert.True(button.IsFlyoutOpen);
        Assert.Equal(2, opened);

        newFlyout.Hide();
        Assert.False(button.IsFlyoutOpen);
        Assert.Equal(2, closed);
    }

    [Fact]
    public void FWToggleSplitButton_ShouldToggleCheckedStateAndRaiseEvent()
    {
        var button = new FWToggleSplitButton();
        FWToggleSplitButtonIsCheckedChangedEventArgs? lastArgs = null;
        var changed = 0;
        var clicked = 0;

        button.IsCheckedChanged += (_, args) =>
        {
            changed++;
            lastArgs = args;
        };
        button.Click += (_, _) => clicked++;

        button.Toggle();

        Assert.True(button.IsChecked);
        Assert.Equal(1, changed);
        Assert.NotNull(lastArgs);
        Assert.False(lastArgs!.OldValue);
        Assert.True(lastArgs.NewValue);

        InvokeSplitButtonClick(button);

        Assert.False(button.IsChecked);
        Assert.Equal(2, changed);
        Assert.Equal(1, clicked);
        Assert.True(lastArgs.OldValue);
        Assert.False(lastArgs.NewValue);
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void FluentControls_ShouldInheritJaliumImplicitStylesByBaseType()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);
            var button = new FWButton();

            var lookupStyle = typeof(FrameworkElement)
                .GetMethod("LookupImplicitStyle", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(button, null);

            var fwStyle = Assert.IsType<Style>(lookupStyle);
            Assert.Equal(typeof(FWButton), fwStyle.TargetType);
            Assert.IsType<Style>(fwStyle.BasedOn);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    private static void AssertContainsStyle<TControl>(ResourceDictionary dictionary)
        where TControl : Control
    {
        Assert.True(dictionary.TryGetValue(typeof(TControl), out var value), $"{typeof(TControl).Name} style was not found.");
        Assert.IsType<Style>(value);
    }

    private static void AssertBasedOnStyle<TFluentControl, TJaliumControl>(ResourceDictionary dictionary)
        where TFluentControl : TJaliumControl, IFluentJaliumControl
        where TJaliumControl : Control
    {
        Assert.True(dictionary.TryGetValue(typeof(TJaliumControl), out var baseValue), $"{typeof(TJaliumControl).Name} base style was not found.");
        var baseStyle = Assert.IsType<Style>(baseValue);

        Assert.True(dictionary.TryGetValue(typeof(TFluentControl), out var fluentValue), $"{typeof(TFluentControl).Name} FW style was not found.");
        var fluentStyle = Assert.IsType<Style>(fluentValue);

        Assert.Equal(typeof(TFluentControl), fluentStyle.TargetType);
        Assert.Same(baseStyle, fluentStyle.BasedOn);
    }

    private static void AssertOwnedStyle<TFluentControl>(ResourceDictionary dictionary)
        where TFluentControl : Control, IFluentJaliumControl
    {
        Assert.True(dictionary.TryGetValue(typeof(TFluentControl), out var fluentValue), $"{typeof(TFluentControl).Name} FW style was not found.");
        var fluentStyle = Assert.IsType<Style>(fluentValue);

        Assert.Equal(typeof(TFluentControl), fluentStyle.TargetType);
        Assert.Null(fluentStyle.BasedOn);
    }

    private static void AssertFluentControl<TFluentControl, TJaliumControl>()
        where TFluentControl : TJaliumControl, IFluentJaliumControl, new()
        where TJaliumControl : Control
    {
        var control = new TFluentControl();
        Assert.IsAssignableFrom<TJaliumControl>(control);
        Assert.StartsWith("FW", typeof(TFluentControl).Name, StringComparison.Ordinal);
    }

    private static void InvokeToggleButtonClick(ToggleButton button)
    {
        typeof(ToggleButton)
            .GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(button, null);
    }

    private static void InvokeSplitButtonClick(SplitButton button)
    {
        typeof(SplitButton)
            .GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(button, [new SplitButtonClickEventArgs()]);
    }

    private static void InvokeCalendarSelectDate(Calendar calendar, DateTime date)
    {
        typeof(Calendar)
            .GetMethod("SelectDate", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(calendar, [date]);
    }

    private static Color GetBrushColor(object? value)
    {
        return Assert.IsType<SolidColorBrush>(value).Color;
    }

    private sealed record CollectionRow(string Name, string State, int Count);

    private sealed record CollectionNode(string Name, string State, CollectionNode[] Children);

    private sealed class NavigationTestPage : Page
    {
    }

    private sealed class SecondNavigationTestPage : Page
    {
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
