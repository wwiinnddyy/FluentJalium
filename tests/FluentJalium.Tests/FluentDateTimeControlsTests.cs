using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Controls.Themes;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using JaliumThemeManager = Jalium.UI.Controls.Themes.ThemeManager;

namespace FluentJalium.Tests;

[Collection("Application")]
public sealed class FluentDateTimeControlsTests
{
    private static readonly string[] DateTimeResourceKeys =
    [
        "DatePickerBackground",
        "DatePickerBackgroundPointerOver",
        "DatePickerBackgroundPressed",
        "DatePickerBackgroundDisabled",
        "DatePickerForeground",
        "DatePickerForegroundDisabled",
        "DatePickerPlaceholderForeground",
        "DatePickerBorderBrush",
        "DatePickerBorderBrushPointerOver",
        "DatePickerBorderBrushFocused",
        "DatePickerBorderBrushDisabled",
        "DatePickerCalendarGlyphForeground",
        "DatePickerCalendarGlyphForegroundPointerOver",
        "DatePickerCalendarGlyphForegroundDisabled",
        "DatePickerFlyoutBackground",
        "DatePickerFlyoutBorderBrush",
        "TimePickerBackground",
        "TimePickerBackgroundPointerOver",
        "TimePickerBackgroundPressed",
        "TimePickerBackgroundDisabled",
        "TimePickerForeground",
        "TimePickerForegroundDisabled",
        "TimePickerPlaceholderForeground",
        "TimePickerBorderBrush",
        "TimePickerBorderBrushPointerOver",
        "TimePickerBorderBrushFocused",
        "TimePickerBorderBrushDisabled",
        "TimePickerClockGlyphForeground",
        "TimePickerClockGlyphForegroundPointerOver",
        "TimePickerClockGlyphForegroundDisabled",
        "TimePickerFlyoutBackground",
        "TimePickerFlyoutBorderBrush",
        "TimePickerSelectedItemBackground",
        "CalendarViewBackground",
        "CalendarViewBorderBrush",
        "CalendarViewHeaderBackground",
        "CalendarViewHeaderForeground",
        "CalendarViewNavigationButtonBackgroundPointerOver",
        "CalendarViewNavigationButtonForeground",
        "CalendarViewNavigationButtonForegroundPointerOver",
        "CalendarViewDayItemBackgroundPointerOver",
        "CalendarViewDayItemBackgroundPressed",
        "CalendarViewDayItemBackgroundSelected",
        "CalendarViewDayItemBackgroundHighlighted",
        "CalendarViewDayItemForeground",
        "CalendarViewDayItemForegroundSelected",
        "CalendarViewDayItemForegroundInactive",
        "CalendarViewDayItemForegroundDisabled",
        "CalendarViewTodayBorderBrush",
        "CalendarViewDayOfWeekForeground"
    ];

    [Theory]
    [InlineData(FluentThemeVariant.Dark)]
    [InlineData(FluentThemeVariant.Light)]
    [InlineData(FluentThemeVariant.HighContrast)]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void ThemeDictionary_ShouldExposeDateTimeTokensForEveryTheme(FluentThemeVariant theme)
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = theme.ToString();

        var dictionary = LoadGenericThemeDictionary();

        foreach (var key in DateTimeResourceKeys)
        {
            Assert.True(dictionary.TryGetValue(key, out var value), $"{key} was not found in the {theme} theme.");
            Assert.IsType<SolidColorBrush>(value);
        }

        ResetApplicationState();
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void ApplyAccent_ShouldUpdateAccentDrivenDateTimeResources()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            var accent = Color.FromRgb(0x7A, 0x3E, 0xB1);
            FluentThemeManager.ApplyAccent(accent);

            Assert.Equal(accent, GetBrushColor(app.Resources["DatePickerBorderBrushFocused"]));
            Assert.Equal(accent, GetBrushColor(app.Resources["TimePickerBorderBrushFocused"]));
            Assert.Equal(Color.FromArgb(0x33, accent.R, accent.G, accent.B), GetBrushColor(app.Resources["TimePickerSelectedItemBackground"]));
            Assert.Equal(accent, GetBrushColor(app.Resources["CalendarViewDayItemBackgroundSelected"]));
            Assert.Equal(Color.FromArgb(0x33, accent.R, accent.G, accent.B), GetBrushColor(app.Resources["CalendarViewDayItemBackgroundHighlighted"]));
            Assert.Equal(accent, GetBrushColor(app.Resources["CalendarViewTodayBorderBrush"]));
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void Apply_ShouldRegisterFwDateTimeStylesBasedOnJaliumStyles()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            AssertBasedOnStyle<FWDatePicker, DatePicker>(app.Resources);
            AssertBasedOnStyle<FWCalendarDatePicker, DatePicker>(app.Resources);
            AssertBasedOnStyle<FWTimePicker, TimePicker>(app.Resources);
            AssertBasedOnStyle<FWCalendar, Calendar>(app.Resources);
            AssertBasedOnStyle<FWCalendarView, Calendar>(app.Resources);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void GenericTheme_ShouldDefineDateTimeBaseStylesAndFluentSetters()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = FluentThemeVariant.Dark.ToString();

        var dictionary = LoadGenericThemeDictionary();

        var datePickerStyle = AssertStyle<DatePicker>(dictionary);
        AssertSetter(datePickerStyle, Control.BackgroundProperty);
        AssertSetter(datePickerStyle, Control.ForegroundProperty);
        AssertSetter(datePickerStyle, Control.BorderBrushProperty);
        AssertSetter(datePickerStyle, Control.CornerRadiusProperty);
        AssertSetter(datePickerStyle, Control.MinHeightProperty);

        var fwDatePickerStyle = AssertStyle<FWDatePicker>(dictionary);
        Assert.Same(datePickerStyle, fwDatePickerStyle.BasedOn);
        AssertSetter(fwDatePickerStyle, FWDatePicker.DensityProperty);

        var fwCalendarDatePickerStyle = AssertStyle<FWCalendarDatePicker>(dictionary);
        Assert.Same(datePickerStyle, fwCalendarDatePickerStyle.BasedOn);
        AssertSetter(fwCalendarDatePickerStyle, FWCalendarDatePicker.DensityProperty);

        var timePickerStyle = AssertStyle<TimePicker>(dictionary);
        AssertSetter(timePickerStyle, Control.BackgroundProperty);
        AssertSetter(timePickerStyle, Control.ForegroundProperty);
        AssertSetter(timePickerStyle, Control.BorderBrushProperty);
        AssertSetter(timePickerStyle, Control.CornerRadiusProperty);
        AssertSetter(timePickerStyle, Control.MinWidthProperty);

        var fwTimePickerStyle = AssertStyle<FWTimePicker>(dictionary);
        Assert.Same(timePickerStyle, fwTimePickerStyle.BasedOn);
        AssertSetter(fwTimePickerStyle, FWTimePicker.DensityProperty);

        var calendarStyle = AssertStyle<Calendar>(dictionary);
        AssertSetter(calendarStyle, Control.BackgroundProperty);
        AssertSetter(calendarStyle, Control.ForegroundProperty);
        AssertSetter(calendarStyle, Control.BorderBrushProperty);
        AssertSetter(calendarStyle, Control.PaddingProperty);
        AssertSetter(calendarStyle, Control.CornerRadiusProperty);

        var fwCalendarStyle = AssertStyle<FWCalendar>(dictionary);
        Assert.Same(calendarStyle, fwCalendarStyle.BasedOn);

        var fwCalendarViewStyle = AssertStyle<FWCalendarView>(dictionary);
        Assert.Same(calendarStyle, fwCalendarViewStyle.BasedOn);

        var calendarDayButtonStyle = AssertStyle<CalendarDayButton>(dictionary);
        AssertSetter(calendarDayButtonStyle, Control.BackgroundProperty);
        AssertSetter(calendarDayButtonStyle, Control.ForegroundProperty);
        AssertSetter(calendarDayButtonStyle, Control.CornerRadiusProperty);

        var datePickerTextBoxStyle = AssertStyle<DatePickerTextBox>(dictionary);
        AssertSetter(datePickerTextBoxStyle, Control.BackgroundProperty);
        AssertSetter(datePickerTextBoxStyle, Control.ForegroundProperty);
        AssertSetter(datePickerTextBoxStyle, Control.BorderBrushProperty);
        AssertSetter(datePickerTextBoxStyle, TextBox.TextWrappingProperty);

        ResetApplicationState();
    }

    [Fact]
    public void FWDateTimePickers_ShouldApplyDensityPresets()
    {
        var datePicker = new FWDatePicker();

        Assert.Equal(FWDateTimePickerDensity.Comfortable, datePicker.Density);
        Assert.Equal(32, datePicker.MinHeight);
        Assert.Equal(200, datePicker.MinWidth);
        Assert.Equal(new Thickness(10, 5, 34, 6), datePicker.Padding);

        datePicker.Density = FWDateTimePickerDensity.Compact;

        Assert.Equal(30, datePicker.MinHeight);
        Assert.Equal(180, datePicker.MinWidth);
        Assert.Equal(new Thickness(8, 4, 30, 5), datePicker.Padding);

        datePicker.Density = FWDateTimePickerDensity.Spacious;

        Assert.Equal(36, datePicker.MinHeight);
        Assert.Equal(240, datePicker.MinWidth);
        Assert.Equal(new Thickness(12, 7, 38, 8), datePicker.Padding);

        var calendarDatePicker = new FWCalendarDatePicker
        {
            Density = FWDateTimePickerDensity.Compact
        };

        Assert.Equal(30, calendarDatePicker.MinHeight);
        Assert.Equal(180, calendarDatePicker.MinWidth);
        Assert.Equal(new Thickness(8, 4, 30, 5), calendarDatePicker.Padding);

        calendarDatePicker.Density = FWDateTimePickerDensity.Spacious;

        Assert.Equal(36, calendarDatePicker.MinHeight);
        Assert.Equal(240, calendarDatePicker.MinWidth);
        Assert.Equal(new Thickness(12, 7, 38, 8), calendarDatePicker.Padding);

        var timePicker = new FWTimePicker();

        Assert.Equal(FWDateTimePickerDensity.Comfortable, timePicker.Density);
        Assert.Equal(32, timePicker.MinHeight);
        Assert.Equal(160, timePicker.MinWidth);
        Assert.Equal(new Thickness(10, 5, 34, 6), timePicker.Padding);

        timePicker.Density = FWDateTimePickerDensity.Compact;

        Assert.Equal(30, timePicker.MinHeight);
        Assert.Equal(140, timePicker.MinWidth);
        Assert.Equal(new Thickness(8, 4, 30, 5), timePicker.Padding);

        timePicker.Density = FWDateTimePickerDensity.Spacious;

        Assert.Equal(36, timePicker.MinHeight);
        Assert.Equal(200, timePicker.MinWidth);
        Assert.Equal(new Thickness(12, 7, 38, 8), timePicker.Padding);
    }

    [Fact]
    public void FWDatePicker_ShouldTrackSelectedDateFormatBoundsAndCalendarEvents()
    {
        var today = DateTime.Today;
        var datePicker = new FWDatePicker
        {
            Header = "Appointment",
            PlaceholderText = "Pick a date",
            DisplayDateStart = today.AddDays(-7),
            DisplayDateEnd = today.AddDays(30),
            SelectedDateFormat = DatePickerFormat.Long
        };
        var changed = 0;
        var opened = 0;
        var closed = 0;
        datePicker.SelectedDateChanged += (_, e) =>
        {
            changed++;
            Assert.Contains(today, e.AddedItems.Cast<DateTime>());
        };
        datePicker.CalendarOpened += (_, _) => opened++;
        datePicker.CalendarClosed += (_, _) => closed++;

        datePicker.SelectedDate = today;
        datePicker.IsDropDownOpen = true;
        datePicker.IsDropDownOpen = false;

        Assert.Equal("Appointment", datePicker.Header);
        Assert.Equal("Pick a date", datePicker.PlaceholderText);
        Assert.Equal(today.AddDays(-7), datePicker.DisplayDateStart);
        Assert.Equal(today.AddDays(30), datePicker.DisplayDateEnd);
        Assert.Equal(DatePickerFormat.Long, datePicker.SelectedDateFormat);
        Assert.Equal(today, datePicker.SelectedDate);
        Assert.False(datePicker.IsDropDownOpen);
        Assert.Equal(1, changed);
        Assert.Equal(1, opened);
        Assert.Equal(1, closed);
    }

    [Fact]
    public void FWCalendarDatePicker_ShouldTrackSelectedDateBoundsAndCalendarEvents()
    {
        var today = DateTime.Today;
        var calendarDatePicker = new FWCalendarDatePicker
        {
            Header = "Review date",
            PlaceholderText = "Choose a date",
            DisplayDateStart = today,
            DisplayDateEnd = today.AddDays(90),
            SelectedDateFormat = DatePickerFormat.Long
        };
        var changed = 0;
        var opened = 0;
        var closed = 0;
        calendarDatePicker.SelectedDateChanged += (_, e) =>
        {
            changed++;
            Assert.Contains(today.AddDays(7), e.AddedItems.Cast<DateTime>());
        };
        calendarDatePicker.CalendarOpened += (_, _) => opened++;
        calendarDatePicker.CalendarClosed += (_, _) => closed++;

        calendarDatePicker.SelectedDate = today.AddDays(7);
        calendarDatePicker.IsDropDownOpen = true;
        calendarDatePicker.IsDropDownOpen = false;

        Assert.Equal("Review date", calendarDatePicker.Header);
        Assert.Equal("Choose a date", calendarDatePicker.PlaceholderText);
        Assert.Equal(today, calendarDatePicker.DisplayDateStart);
        Assert.Equal(today.AddDays(90), calendarDatePicker.DisplayDateEnd);
        Assert.Equal(DatePickerFormat.Long, calendarDatePicker.SelectedDateFormat);
        Assert.Equal(today.AddDays(7), calendarDatePicker.SelectedDate);
        Assert.False(calendarDatePicker.IsDropDownOpen);
        Assert.Equal(1, changed);
        Assert.Equal(1, opened);
        Assert.Equal(1, closed);
    }

    [Fact]
    public void FWTimePicker_ShouldTrackSelectedTimeClockIdentifierIncrementAndDropDown()
    {
        var timePicker = new FWTimePicker
        {
            Header = "Arrival",
            PlaceholderText = "Pick a time",
            MinuteIncrement = 15,
            ClockIdentifier = "24HourClock"
        };
        var changed = 0;
        TimeSpan? oldTime = null;
        TimeSpan? newTime = null;
        timePicker.SelectedTimeChanged += (_, e) =>
        {
            changed++;
            oldTime = e.OldTime;
            newTime = e.NewTime;
        };

        timePicker.SelectedTime = new TimeSpan(18, 45, 0);
        timePicker.IsDropDownOpen = true;
        timePicker.IsDropDownOpen = false;

        Assert.Equal("Arrival", timePicker.Header);
        Assert.Equal("Pick a time", timePicker.PlaceholderText);
        Assert.Equal(15, timePicker.MinuteIncrement);
        Assert.Equal("24HourClock", timePicker.ClockIdentifier);
        Assert.Equal(new TimeSpan(18, 45, 0), timePicker.SelectedTime);
        Assert.Null(oldTime);
        Assert.Equal(new TimeSpan(18, 45, 0), newTime);
        Assert.False(timePicker.IsDropDownOpen);
        Assert.Equal(1, changed);

        timePicker.ClockIdentifier = "12HourClock";
        Assert.Equal("12HourClock", timePicker.ClockIdentifier);
    }

    [Fact]
    public void FWCalendar_ShouldTrackDisplaySelectionBoundsBlackoutsAndOptions()
    {
        var today = DateTime.Today;
        var calendar = new FWCalendar
        {
            DisplayDate = today,
            DisplayDateStart = today.AddDays(-14),
            DisplayDateEnd = today.AddDays(45),
            FirstDayOfWeek = DayOfWeek.Monday,
            IsTodayHighlighted = true,
            SelectionMode = CalendarSelectionMode.SingleDate
        };
        calendar.BlackoutDates.Add(today.AddDays(2).Date);
        var selectedChanged = 0;
        var displayChanged = 0;
        calendar.SelectedDateChanged += (_, _) => selectedChanged++;
        calendar.DisplayDateChanged += (_, _) => displayChanged++;

        calendar.SelectedDate = today.AddDays(1);
        calendar.DisplayDate = today.AddMonths(1);
        calendar.FirstDayOfWeek = DayOfWeek.Sunday;
        calendar.IsTodayHighlighted = false;

        Assert.Equal(today.AddDays(-14), calendar.DisplayDateStart);
        Assert.Equal(today.AddDays(45), calendar.DisplayDateEnd);
        Assert.Contains(today.AddDays(2).Date, calendar.BlackoutDates);
        Assert.Equal(today.AddDays(1), calendar.SelectedDate);
        Assert.Equal(today.AddMonths(1), calendar.DisplayDate);
        Assert.Equal(DayOfWeek.Sunday, calendar.FirstDayOfWeek);
        Assert.False(calendar.IsTodayHighlighted);
        Assert.Equal(CalendarSelectionMode.SingleDate, calendar.SelectionMode);
        Assert.Equal(1, selectedChanged);
        Assert.Equal(2, displayChanged);

        var calendarView = new FWCalendarView
        {
            DisplayDate = today,
            SelectedDate = today.AddDays(3),
            DisplayDateStart = today.AddDays(-10),
            DisplayDateEnd = today.AddDays(40),
            FirstDayOfWeek = DayOfWeek.Monday,
            IsTodayHighlighted = true
        };

        Assert.Equal(today.AddDays(3), calendarView.SelectedDate);
        Assert.Equal(today.AddDays(-10), calendarView.DisplayDateStart);
        Assert.Equal(today.AddDays(40), calendarView.DisplayDateEnd);
        Assert.Equal(DayOfWeek.Monday, calendarView.FirstDayOfWeek);
        Assert.True(calendarView.IsTodayHighlighted);
    }

    [Fact]
    public void FWCalendarView_ShouldTrackSelectionDisplayAndBlackouts()
    {
        var today = DateTime.Today;
        var calendarView = new FWCalendarView
        {
            DisplayDate = today,
            DisplayDateStart = today.AddDays(-5),
            DisplayDateEnd = today.AddDays(45),
            FirstDayOfWeek = DayOfWeek.Monday,
            IsTodayHighlighted = true,
            SelectionMode = CalendarSelectionMode.SingleDate
        };
        calendarView.BlackoutDates.Add(today.AddDays(4).Date);
        var selectedChanged = 0;
        var displayChanges = new List<CalendarDateChangedEventArgs>();
        calendarView.SelectedDateChanged += (_, e) =>
        {
            selectedChanged++;
            Assert.Contains(today.AddDays(2), e.AddedItems.Cast<DateTime>());
        };
        calendarView.DisplayDateChanged += (_, e) => displayChanges.Add(e);

        calendarView.SelectedDate = today.AddDays(2);
        calendarView.DisplayDate = today.AddMonths(1);
        calendarView.FirstDayOfWeek = DayOfWeek.Sunday;
        calendarView.IsTodayHighlighted = false;

        Assert.Equal(today.AddDays(2), calendarView.SelectedDate);
        Assert.Equal(today.AddMonths(1), calendarView.DisplayDate);
        Assert.Equal(today.AddDays(-5), calendarView.DisplayDateStart);
        Assert.Equal(today.AddDays(45), calendarView.DisplayDateEnd);
        Assert.Contains(today.AddDays(4).Date, calendarView.BlackoutDates);
        Assert.Equal(DayOfWeek.Sunday, calendarView.FirstDayOfWeek);
        Assert.False(calendarView.IsTodayHighlighted);
        Assert.Equal(CalendarSelectionMode.SingleDate, calendarView.SelectionMode);
        Assert.Equal(1, selectedChanged);
        Assert.Equal(2, displayChanges.Count);
        Assert.Equal(today, displayChanges[0].RemovedDate);
        Assert.Equal(today.AddMonths(1), displayChanges[0].AddedDate);
        Assert.Null(displayChanges[1].RemovedDate);
        Assert.Null(displayChanges[1].AddedDate);
    }

    [Fact]
    public void FWDateTimeControls_ShouldExposeMaterialPlanningPanelState()
    {
        var today = DateTime.Today;
        var datePicker = new FWDatePicker
        {
            Header = "Planning date",
            PlaceholderText = "Select",
            DisplayDateStart = today,
            DisplayDateEnd = today.AddDays(60),
            SelectedDate = today.AddDays(3),
            SelectedDateFormat = DatePickerFormat.Short
        };
        var calendarDatePicker = new FWCalendarDatePicker
        {
            Header = "Review date",
            PlaceholderText = "Select",
            DisplayDateStart = today,
            DisplayDateEnd = today.AddDays(90),
            SelectedDate = today.AddDays(7),
            SelectedDateFormat = DatePickerFormat.Long
        };
        var timePicker = new FWTimePicker
        {
            Header = "Focus block",
            SelectedTime = new TimeSpan(14, 15, 0),
            MinuteIncrement = 15,
            ClockIdentifier = "24HourClock"
        };
        var calendar = new FWCalendar
        {
            DisplayDate = today,
            DisplayDateStart = today,
            DisplayDateEnd = today.AddDays(60),
            SelectedDate = datePicker.SelectedDate,
            FirstDayOfWeek = DayOfWeek.Monday,
            IsTodayHighlighted = true,
            SelectionMode = CalendarSelectionMode.SingleDate
        };
        var calendarView = new FWCalendarView
        {
            DisplayDate = today,
            DisplayDateStart = today,
            DisplayDateEnd = today.AddDays(120),
            SelectedDate = calendarDatePicker.SelectedDate,
            FirstDayOfWeek = DayOfWeek.Monday,
            IsTodayHighlighted = true,
            SelectionMode = CalendarSelectionMode.SingleDate
        };
        calendar.BlackoutDates.Add(today.AddDays(1));

        var panel = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Children =
            {
                datePicker,
                calendarDatePicker,
                timePicker,
                calendar,
                calendarView
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

        Assert.Equal(today.AddDays(3), datePicker.SelectedDate);
        Assert.Equal(DatePickerFormat.Short, datePicker.SelectedDateFormat);
        Assert.Equal(today, datePicker.DisplayDateStart);
        Assert.Equal(today.AddDays(60), datePicker.DisplayDateEnd);
        Assert.Equal(today.AddDays(7), calendarDatePicker.SelectedDate);
        Assert.Equal(DatePickerFormat.Long, calendarDatePicker.SelectedDateFormat);
        Assert.Equal(today.AddDays(90), calendarDatePicker.DisplayDateEnd);
        Assert.Equal(new TimeSpan(14, 15, 0), timePicker.SelectedTime);
        Assert.Equal(15, timePicker.MinuteIncrement);
        Assert.Equal("24HourClock", timePicker.ClockIdentifier);
        Assert.Equal(datePicker.SelectedDate, calendar.SelectedDate);
        Assert.Equal(DayOfWeek.Monday, calendar.FirstDayOfWeek);
        Assert.True(calendar.IsTodayHighlighted);
        Assert.Contains(today.AddDays(1), calendar.BlackoutDates);
        Assert.Equal(calendarDatePicker.SelectedDate, calendarView.SelectedDate);
        Assert.Equal(today.AddDays(120), calendarView.DisplayDateEnd);
        Assert.Equal(FWFluentMaterialKind.LiquidGlass, surface.MaterialKind);
        Assert.True(surface.LiquidGlass);
        Assert.Equal(70, surface.RefractionAmount);
        Assert.Equal(0.42, surface.ChromaticAberration);
        Assert.Equal(24, surface.FusionRadius);
        Assert.Equal(BorderShape.SuperEllipse, surface.Shape);
        Assert.Equal(4, surface.SuperEllipseN);
        Assert.Same(panel, surface.Child);
    }

    private static ResourceDictionary LoadGenericThemeDictionary()
    {
        var loaded = ResourceDictionary.SourceLoader?.Invoke(
            new ResourceDictionary(),
            new Uri("/FluentJalium;component/Themes/Generic.jalxaml", UriKind.Relative),
            FluentThemeManager.ThemeAssembly);

        return Assert.IsType<ResourceDictionary>(loaded);
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

    private static Color GetBrushColor(object? value)
    {
        return Assert.IsType<SolidColorBrush>(value).Color;
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
