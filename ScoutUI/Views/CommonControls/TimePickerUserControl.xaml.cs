using ScoutUtilities.Enums;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ScoutUI.Views.CommonControls
{
    /// <summary>
    /// Interaction logic for TimePickerUserControl.xaml
    /// </summary>
    public partial class TimePickerUserControl : UserControl
    {
        public TimePickerUserControl()
        {
            InitializeComponent();
        }
        
        #region Dependency Properties

        #region ShowDatePicker

        public bool ShowDatePicker
        {
            get { return (bool)GetValue(ShowDatePickerProperty); }
            set { SetValue(ShowDatePickerProperty, value); }
        }

        public static readonly DependencyProperty ShowDatePickerProperty =
            DependencyProperty.Register(nameof(ShowDatePicker), typeof(bool),
                typeof(TimePickerUserControl), new PropertyMetadata(false));

        #endregion

        #region ShowDayOfWeekPicker

        public bool ShowDayOfWeekPicker
        {
            get { return (bool)GetValue(ShowDayOfWeekPickerProperty); }
            set { SetValue(ShowDayOfWeekPickerProperty, value); }
        }

        public static readonly DependencyProperty ShowDayOfWeekPickerProperty =
            DependencyProperty.Register(nameof(ShowDayOfWeekPicker), typeof(bool),
                typeof(TimePickerUserControl), new PropertyMetadata(false));

        #endregion

        #region ShowDayOfMonthPicker

        public bool ShowDayOfMonthPicker
        {
            get { return (bool)GetValue(ShowDayOfMonthPickerProperty); }
            set { SetValue(ShowDayOfMonthPickerProperty, value); }
        }

        public static readonly DependencyProperty ShowDayOfMonthPickerProperty =
            DependencyProperty.Register(nameof(ShowDayOfMonthPicker), typeof(bool),
                typeof(TimePickerUserControl), new PropertyMetadata(false));

        #endregion

        #region DatePickerDateSelected

        public DateTime DatePickerDateSelected
        {
            get { return (DateTime)GetValue(DatePickerDateSelectedProperty); }
            set { SetValue(DatePickerDateSelectedProperty, value); }
        }

        public static readonly DependencyProperty DatePickerDateSelectedProperty =
            DependencyProperty.Register(nameof(DatePickerDateSelected), typeof(DateTime),
                typeof(TimePickerUserControl), new PropertyMetadata(DateTime.Now));

        #endregion

        #region WeekdaySelected

        public Weekday WeekdaySelected
        {
            get { return (Weekday)GetValue(WeekdaySelectedProperty); }
            set { SetValue(WeekdaySelectedProperty, value); }
        }

        public static readonly DependencyProperty WeekdaySelectedProperty =
            DependencyProperty.Register(nameof(WeekdaySelected), typeof(Weekday),
                typeof(TimePickerUserControl), new PropertyMetadata(Weekday.Sunday));

        #endregion

        #region DayOfMonthSelected

        public ushort DayOfMonthSelected
        {
            get { return (ushort)GetValue(DayOfMonthSelectedProperty); }
            set { SetValue(DayOfMonthSelectedProperty, value); }
        }

        public static readonly DependencyProperty DayOfMonthSelectedProperty =
            DependencyProperty.Register(nameof(DayOfMonthSelected), typeof(ushort),
                typeof(TimePickerUserControl), new PropertyMetadata((ushort) 1));

        #endregion

        #region HourSelected

        public ushort HourSelected
        {
            get { return (ushort)GetValue(HourSelectedProperty); }
            set { SetValue(HourSelectedProperty, value); }
        }

        public static readonly DependencyProperty HourSelectedProperty =
            DependencyProperty.Register(nameof(HourSelected), typeof(ushort),
                typeof(TimePickerUserControl), new PropertyMetadata((ushort)0));

        #endregion

        #region MinuteSelected

        public ushort MinuteSelected
        {
            get { return (ushort)GetValue(MinuteSelectedProperty); }
            set { SetValue(MinuteSelectedProperty, value); }
        }

        public static readonly DependencyProperty MinuteSelectedProperty =
            DependencyProperty.Register(nameof(MinuteSelected), typeof(ushort),
                typeof(TimePickerUserControl), new PropertyMetadata((ushort)0));

        #endregion

        #region ClockFormat

        public ClockFormat ClockFormat
        {
            get { return (ClockFormat)GetValue(ClockFormatProperty); }
            set { SetValue(ClockFormatProperty, value); }
        }

        public static readonly DependencyProperty ClockFormatProperty =
            DependencyProperty.Register(nameof(ClockFormat), typeof(ClockFormat),
                typeof(TimePickerUserControl), new PropertyMetadata(ClockFormat.Hour24));

        #endregion

        #endregion
    }
}
