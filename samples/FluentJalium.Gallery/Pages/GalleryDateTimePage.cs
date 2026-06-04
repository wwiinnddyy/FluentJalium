using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWButton = FluentJalium.Controls.FWButton;
using FWCalendar = FluentJalium.Controls.FWCalendar;
using FWDateTimePickerDensity = FluentJalium.Controls.FWDateTimePickerDensity;
using FWDatePicker = FluentJalium.Controls.FWDatePicker;
using FWFluentMaterialKind = FluentJalium.Controls.FWFluentMaterialKind;
using FWFluentMaterialSurface = FluentJalium.Controls.FWFluentMaterialSurface;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWTimePicker = FluentJalium.Controls.FWTimePicker;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;

namespace FluentJalium.Gallery.Pages;

internal sealed class GalleryDateTimePage
{
    public UIElement CreateContent()
    {
        var panel = CreateSection("Date and Time");
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        examples.Children.Add(CreateDateTimeExampleCard(
            FluentIconRegular.CalendarLtr24,
            "FWDatePicker",
            "Header, placeholder, long and short formats, bounded dates, dropdown events, and live selected-date output.",
            CreateDatePickerDateTimeSample()));
        examples.Children.Add(CreateDateTimeExampleCard(
            FluentIconRegular.TimePicker24,
            "FWTimePicker",
            "Minute increments, 12-hour and 24-hour clocks, dropdown state, keyboard-ready selection, and output.",
            CreateTimePickerDateTimeSample()));
        examples.Children.Add(CreateDateTimeExampleCard(
            FluentIconRegular.CalendarMonth24,
            "FWCalendar",
            "CalendarView-style single-date selection with first-day-of-week, today highlight, blackout, and range bounds.",
            CreateCalendarDateTimeSample()));
        examples.Children.Add(CreateDateTimeExampleCard(
            FluentIconRegular.CalendarSettings24,
            "States and bounds",
            "Disabled, placeholder, focused-border, bounded range, and high-contrast-friendly resource states.",
            CreateDateTimeStateSample()));
        examples.Children.Add(CreateDateTimeExampleCard(
            FluentIconRegular.LayerDiagonalSparkle24,
            "Material schedule panel",
            "DatePicker, TimePicker, and Calendar stay legible on a LiquidGlass planning surface.",
            CreateMaterialSchedulePanelSample()));

        panel.Children.Add(examples);
        return panel;
    }

    private static UIElement CreateDatePickerDateTimeSample()
    {
        var today = DateTime.Today;
        var output = CreateDateTimeOutput($"Selected date: {FormatDateTimeDate(today)}");
        var datePicker = new FWDatePicker
        {
            Header = "Appointment date",
            Density = FWDateTimePickerDensity.Comfortable,
            PlaceholderText = "Pick a date",
            DisplayDateStart = today.AddDays(-14),
            DisplayDateEnd = today.AddDays(45),
            SelectedDate = today,
            SelectedDateFormat = DatePickerFormat.Long
        };
        datePicker.SelectedDateChanged += (_, _) =>
        {
            output.Text = $"Selected date: {FormatDateTimeDate(datePicker.SelectedDate)}";
        };
        datePicker.CalendarOpened += (_, _) => output.Text = "Calendar opened";
        datePicker.CalendarClosed += (_, _) => output.Text = $"Calendar closed. Selected date: {FormatDateTimeDate(datePicker.SelectedDate)}";

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                datePicker,
                CreateDateTimeButtonRow(
                    CreateDateTimeActionButton(FluentIconRegular.CalendarToday24, "Today", () => datePicker.SelectedDate = today),
                    CreateDateTimeActionButton(FluentIconRegular.CalendarArrowRight24, "Next week", () => datePicker.SelectedDate = today.AddDays(7)),
                    CreateDateTimeActionButton(FluentIconRegular.CalendarDate24, "Short", () =>
                    {
                        datePicker.SelectedDateFormat = DatePickerFormat.Short;
                        output.Text = "SelectedDateFormat: Short";
                    }),
                    CreateDateTimeActionButton(FluentIconRegular.CalendarMonth24, "Long", () =>
                    {
                        datePicker.SelectedDateFormat = DatePickerFormat.Long;
                        output.Text = "SelectedDateFormat: Long";
                    }),
                    CreateDateTimeActionButton(FluentIconRegular.TextDensity24, "Density", () =>
                    {
                        datePicker.Density = NextDensity(datePicker.Density);
                        output.Text = $"DatePicker density: {FormatDensity(datePicker.Density)}";
                    }),
                    CreateDateTimeActionButton(FluentIconRegular.ChevronDown24, "Flyout", () => datePicker.IsDropDownOpen = !datePicker.IsDropDownOpen)),
                CreateDateTimeStatus(output)
            }
        };
    }

    private static UIElement CreateTimePickerDateTimeSample()
    {
        var output = CreateDateTimeOutput("Selected time: 10:30 AM");
        var timePicker = new FWTimePicker
        {
            Header = "Arrival time",
            Density = FWDateTimePickerDensity.Comfortable,
            PlaceholderText = "Pick a time",
            SelectedTime = new TimeSpan(10, 30, 0),
            MinuteIncrement = 15
        };
        timePicker.SelectedTimeChanged += (_, e) =>
        {
            output.Text = $"Selected time: {FormatDateTimeTime(e.NewTime, timePicker.ClockIdentifier)}";
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                timePicker,
                CreateDateTimeButtonRow(
                    CreateDateTimeActionButton(FluentIconRegular.Clock24, "10:30", () => timePicker.SelectedTime = new TimeSpan(10, 30, 0)),
                    CreateDateTimeActionButton(FluentIconRegular.Clock24, "18:45", () => timePicker.SelectedTime = new TimeSpan(18, 45, 0)),
                    CreateDateTimeActionButton(FluentIconRegular.TimePicker24, "12 hour", () =>
                    {
                        timePicker.ClockIdentifier = "12HourClock";
                        output.Text = $"ClockIdentifier: {timePicker.ClockIdentifier}";
                    }),
                    CreateDateTimeActionButton(FluentIconRegular.TimePicker24, "24 hour", () =>
                    {
                        timePicker.ClockIdentifier = "24HourClock";
                        output.Text = $"ClockIdentifier: {timePicker.ClockIdentifier}";
                    }),
                    CreateDateTimeActionButton(FluentIconRegular.CalendarClock24, "15 min", () =>
                    {
                        timePicker.MinuteIncrement = 15;
                        output.Text = "MinuteIncrement: 15";
                    }),
                    CreateDateTimeActionButton(FluentIconRegular.TextDensity24, "Density", () =>
                    {
                        timePicker.Density = NextDensity(timePicker.Density);
                        output.Text = $"TimePicker density: {FormatDensity(timePicker.Density)}";
                    }),
                    CreateDateTimeActionButton(FluentIconRegular.ChevronDown24, "Flyout", () => timePicker.IsDropDownOpen = !timePicker.IsDropDownOpen)),
                CreateDateTimeStatus(output)
            }
        };
    }

    private static UIElement CreateCalendarDateTimeSample()
    {
        var today = DateTime.Today;
        var output = CreateDateTimeOutput($"Selected: {FormatDateTimeDate(today.AddDays(1))}");
        var calendar = new FWCalendar
        {
            DisplayDate = today,
            DisplayDateStart = today.AddDays(-14),
            DisplayDateEnd = today.AddDays(45),
            FirstDayOfWeek = DayOfWeek.Monday,
            IsTodayHighlighted = true,
            SelectedDate = today.AddDays(1),
            SelectionMode = CalendarSelectionMode.SingleDate
        };
        calendar.BlackoutDates.Add(today.AddDays(2).Date);
        calendar.SelectedDateChanged += (_, _) =>
        {
            output.Text = $"Selected: {FormatDateTimeDate(calendar.SelectedDate)}";
        };
        calendar.DisplayDateChanged += (_, e) =>
        {
            output.Text = $"Display month: {e.AddedDate?.ToString("MMMM yyyy") ?? "none"}";
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                calendar,
                CreateDateTimeButtonRow(
                    CreateDateTimeActionButton(FluentIconRegular.CalendarToday24, "Today", () => calendar.SelectedDate = today),
                    CreateDateTimeActionButton(FluentIconRegular.CalendarArrowRight24, "Next month", () => calendar.DisplayDate = calendar.DisplayDate.AddMonths(1)),
                    CreateDateTimeActionButton(FluentIconRegular.CalendarWeekStart24, "Monday", () =>
                    {
                        calendar.FirstDayOfWeek = DayOfWeek.Monday;
                        output.Text = "FirstDayOfWeek: Monday";
                    }),
                    CreateDateTimeActionButton(FluentIconRegular.CalendarWeekStart24, "Sunday", () =>
                    {
                        calendar.FirstDayOfWeek = DayOfWeek.Sunday;
                        output.Text = "FirstDayOfWeek: Sunday";
                    }),
                    CreateDateTimeActionButton(FluentIconRegular.CalendarCheckmark24, "Today ring", () =>
                    {
                        calendar.IsTodayHighlighted = !calendar.IsTodayHighlighted;
                        output.Text = $"IsTodayHighlighted: {calendar.IsTodayHighlighted}";
                    })),
                CreateDateTimeStatus(output)
            }
        };
    }

    private static UIElement CreateDateTimeStateSample()
    {
        var today = DateTime.Today;
        var boundedDate = new FWDatePicker
        {
            Header = "Bounded date",
            Width = 240,
            PlaceholderText = "Within the next 30 days",
            DisplayDateStart = today,
            DisplayDateEnd = today.AddDays(30)
        };
        var disabledDate = new FWDatePicker
        {
            Header = "Disabled date",
            Width = 240,
            PlaceholderText = "Unavailable",
            IsEnabled = false
        };
        var disabledTime = new FWTimePicker
        {
            Header = "Disabled time",
            Width = 220,
            SelectedTime = new TimeSpan(8, 0, 0),
            IsEnabled = false
        };
        var disabledCalendar = new FWCalendar
        {
            DisplayDate = today,
            SelectedDate = today,
            IsEnabled = false
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                boundedDate,
                disabledDate,
                disabledTime,
                disabledCalendar,
                CreateDateTimeStatus(CreateDateTimeOutput("Disabled and bounded controls reuse DatePicker, TimePicker, and CalendarView tokens."))
            }
        };
    }

    private static UIElement CreateMaterialSchedulePanelSample()
    {
        var today = DateTime.Today;
        var output = CreateDateTimeOutput($"Plan: {FormatDateTimeDate(today.AddDays(3))} at 14:15. Density: comfortable");
        var datePicker = new FWDatePicker
        {
            Header = "Planning date",
            Density = FWDateTimePickerDensity.Comfortable,
            PlaceholderText = "Select",
            DisplayDateStart = today,
            DisplayDateEnd = today.AddDays(60),
            SelectedDate = today.AddDays(3),
            SelectedDateFormat = DatePickerFormat.Short
        };
        var timePicker = new FWTimePicker
        {
            Header = "Focus block",
            Density = FWDateTimePickerDensity.Comfortable,
            SelectedTime = new TimeSpan(14, 15, 0),
            MinuteIncrement = 15,
            ClockIdentifier = "24HourClock"
        };
        var calendar = new FWCalendar
        {
            DisplayDate = today,
            DisplayDateStart = today,
            DisplayDateEnd = today.AddDays(60),
            SelectedDate = today.AddDays(3),
            FirstDayOfWeek = DayOfWeek.Monday,
            IsTodayHighlighted = true,
            SelectionMode = CalendarSelectionMode.SingleDate
        };
        calendar.BlackoutDates.Add(today.AddDays(1));

        void UpdateOutput()
        {
            output.Text = $"Plan: {FormatDateTimeDate(datePicker.SelectedDate)} at {FormatDateTimeTime(timePicker.SelectedTime, timePicker.ClockIdentifier)}. Density: {FormatDensity(datePicker.Density)}";
        }

        datePicker.SelectedDateChanged += (_, _) =>
        {
            calendar.SelectedDate = datePicker.SelectedDate;
            UpdateOutput();
        };
        timePicker.SelectedTimeChanged += (_, _) => UpdateOutput();
        calendar.SelectedDateChanged += (_, _) =>
        {
            datePicker.SelectedDate = calendar.SelectedDate;
            UpdateOutput();
        };

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
                    new FWWrapPanel
                    {
                        HorizontalSpacing = 12,
                        VerticalSpacing = 10,
                        Children =
                        {
                            datePicker,
                            timePicker
                        }
                    },
                    calendar,
                    CreateDateTimeButtonRow(
                        CreateDateTimeActionButton(FluentIconRegular.CalendarToday24, "Today", () =>
                        {
                            datePicker.SelectedDate = today;
                            calendar.SelectedDate = today;
                            UpdateOutput();
                        }),
                        CreateDateTimeActionButton(FluentIconRegular.CalendarArrowRight24, "Next week", () =>
                        {
                            datePicker.SelectedDate = today.AddDays(7);
                            calendar.SelectedDate = today.AddDays(7);
                            UpdateOutput();
                        }),
                        CreateDateTimeActionButton(FluentIconRegular.Clock24, "Evening", () =>
                        {
                            timePicker.SelectedTime = new TimeSpan(18, 45, 0);
                            UpdateOutput();
                        }),
                        CreateDateTimeActionButton(FluentIconRegular.TextDensity24, "Density", () =>
                        {
                            var density = NextDensity(datePicker.Density);
                            datePicker.Density = density;
                            timePicker.Density = density;
                            UpdateOutput();
                        })),
                    CreateDateTimeStatus(output)
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
                CreateIcon(FluentIconRegular.CalendarClock24, 18, ThemeBrush("TextPrimary")),
                new FWTextBlock
                {
                    Text = "Layered planning surface",
                    FontSize = 15,
                    Foreground = ThemeBrush("TextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
    }

    private static FWBorder CreateDateTimeExampleCard(FluentIconRegular icon, string title, string description, UIElement content)
    {
        return new FWBorder
        {
            Width = 570,
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

    private static FWWrapPanel CreateDateTimeButtonRow(params FWButton[] buttons)
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

    private static FWButton CreateDateTimeActionButton(FluentIconRegular icon, string text, Action action)
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

    private static TextBlock CreateDateTimeOutput(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        };
    }

    private static FWBorder CreateDateTimeStatus(TextBlock status)
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

    private static string FormatDateTimeDate(DateTime? date)
    {
        return date?.ToString("D") ?? "none";
    }

    private static string FormatDateTimeTime(TimeSpan? time, string clockIdentifier)
    {
        if (!time.HasValue)
        {
            return "none";
        }

        if (clockIdentifier == "24HourClock")
        {
            return $"{time.Value.Hours:D2}:{time.Value.Minutes:D2}";
        }

        var hour = time.Value.Hours % 12;
        if (hour == 0)
        {
            hour = 12;
        }

        return $"{hour}:{time.Value.Minutes:D2} {(time.Value.Hours >= 12 ? "PM" : "AM")}";
    }

    private static FWDateTimePickerDensity NextDensity(FWDateTimePickerDensity density)
    {
        return density switch
        {
            FWDateTimePickerDensity.Compact => FWDateTimePickerDensity.Comfortable,
            FWDateTimePickerDensity.Comfortable => FWDateTimePickerDensity.Spacious,
            _ => FWDateTimePickerDensity.Compact
        };
    }

    private static string FormatDensity(FWDateTimePickerDensity density)
    {
        return density switch
        {
            FWDateTimePickerDensity.Compact => "compact",
            FWDateTimePickerDensity.Spacious => "spacious",
            _ => "comfortable"
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
                        CreateIcon(FluentIconRegular.CalendarLtr24, 24, ThemeBrush("TextPrimary")),
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
