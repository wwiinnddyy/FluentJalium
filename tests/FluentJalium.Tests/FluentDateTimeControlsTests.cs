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
            AssertBasedOnStyle<FWTimePicker, TimePicker>(app.Resources);
            AssertBasedOnStyle<FWCalendar, Calendar>(app.Resources);
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

        var timePickerStyle = AssertStyle<TimePicker>(dictionary);
        AssertSetter(timePickerStyle, Control.BackgroundProperty);
        AssertSetter(timePickerStyle, Control.ForegroundProperty);
        AssertSetter(timePickerStyle, Control.BorderBrushProperty);
        AssertSetter(timePickerStyle, Control.CornerRadiusProperty);
        AssertSetter(timePickerStyle, Control.MinWidthProperty);

        var calendarStyle = AssertStyle<Calendar>(dictionary);
        AssertSetter(calendarStyle, Control.BackgroundProperty);
        AssertSetter(calendarStyle, Control.ForegroundProperty);
        AssertSetter(calendarStyle, Control.BorderBrushProperty);
        AssertSetter(calendarStyle, Control.PaddingProperty);
        AssertSetter(calendarStyle, Control.CornerRadiusProperty);

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
