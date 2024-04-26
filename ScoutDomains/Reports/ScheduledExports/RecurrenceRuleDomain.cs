using System;
using System.Collections.ObjectModel;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;

namespace ScoutDomains.Reports.ScheduledExports
{
    /// <summary>
    /// This class is modeled after the ICalendar standard for
    /// recurring events (https://tools.ietf.org/html/rfc5545#section-3.3.10).
    /// </summary>
    public class RecurrenceRuleDomain : BaseNotifyPropertyChanged, ICloneable
    {
        public RecurrenceRuleDomain()
        {
            RecurrenceFrequency = RecurrenceFrequency.Once;
            Weekday = Weekday.Sunday;
            DayOfTheMonth = 1;
            Hour = 0;
            Minutes = 0;
            ExportOnDate = DateTime.Now.Date;
        }

        public ushort Get24Hour()
        {
            switch (SelectedClockFormat)
            {
                case ClockFormat.PM:
                    if (Hour == (ushort) 0 || Hour == (ushort) 12)
                    {
                        return (ushort) 12; // 0:00 PM & 12:00 PM is considered 12:00 PM
                    }
                    return (ushort) (Hour + 12);

                case ClockFormat.AM:
                    if (Hour == 12)
                    {
                        return 0;
                    }
                    return Hour;

                default:
                    return Hour;
            }
        }

        public bool RecurrenceRulesAreValid()
        {
            if (SelectedClockFormat != ClockFormat.Hour24 &&
                Hour > 12)
            {
                Log.Warn($"SelectedClockFormat is not Hour24 and Hour > 12");
                return false;
            }

            switch (RecurrenceFrequency)
            {
                case RecurrenceFrequency.Once:
                    // Get start date, add hours and minutes. Check if valid
                    DateTime exportDate = ExportOnDate.Date;
                    DateTime exportDateHours = exportDate.AddHours(Hour);
                    if (SelectedClockFormat == ClockFormat.PM && Hour < 12) // 0:00 PM & 12:00 PM is considered 12:00 PM
                    {
                        exportDateHours = exportDateHours.AddHours(12);
                    }
                    DateTime exportDateTime = exportDateHours.AddMinutes(Minutes);
                    if ( exportDateTime < DateTime.Now)
                    {
                        Log.Warn($"ExportOnDate is < DateTime.Now");
                        return false;
                    }
                    break;
                case RecurrenceFrequency.Daily:
                case RecurrenceFrequency.Weekly:
                case RecurrenceFrequency.Monthly:
                    // nothing more to check
                    break;
            }

            return true;
        }

        /// <summary>
        /// How often the event should be fired.
        /// </summary>
        public RecurrenceFrequency RecurrenceFrequency
        {
            get { return GetProperty<RecurrenceFrequency>(); }
            set { SetProperty(value); }
        }
        
        /// <summary>
        /// 0 -> 23 - the hour of the day for the event to occur.
        /// </summary>
        public ushort Hour
        {
            get { return GetProperty<ushort>();}
            set
            {
                if (value > 23)
                {
                    SetProperty((ushort) 23);
                }
                else
                {
                    SetProperty(value);
                }
            }
        }

        /// <summary>
        /// 0 -> 59 - the minute of the hour for the event to occur.
        /// </summary>
        public ushort Minutes
        {
            get { return GetProperty<ushort>(); }
            set
            {
                if (value > 59)
                {
                    SetProperty((ushort) 59);
                }
                else
                {
                    SetProperty(value);
                }
            }
        }

        /// <summary>
        /// The selected clock format to be used in conjunction with
        /// Hour and Minutes.
        /// </summary>
        public ClockFormat SelectedClockFormat
        {
            get { return GetProperty<ClockFormat>(); }
            set { SetProperty(value); }
        }

        #region Repeat is Once

        /// <summary>
        /// The date that the export should take place on. Only used when
        /// RecurrenceFrequency is Once.
        /// </summary>
        public DateTime ExportOnDate
        {
            get { return GetProperty<DateTime>(); }
            set { SetProperty(value); }
        }
        
        #endregion

        #region Repeat is Weekly

        /// <summary>
        /// The day of the week for the recurring event. This is only used
        /// in conjunction with RecurrenceFrequency.Weekly.
        /// </summary>
        public Weekday Weekday
        {
            get { return GetProperty<Weekday>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Repeat is Monthly

        /// <summary>
        /// 1 -> 31 - some months won't have all these days.This is only used
        /// in conjunction with RecurrenceFrequency.Monthly.
        /// </summary>
        public ushort DayOfTheMonth
        {
            get { return GetProperty<ushort>(); }
            set
            {
                if (value < 1)
                {
                    SetProperty((ushort) 1);
                }
                else if (value > 31)
                {
                    SetProperty((ushort) 31);
                }
                else
                {
                    SetProperty(value);
                }
            }
        }

        #endregion

        public object Clone()
        {
            var cloneObj = (RecurrenceRuleDomain) MemberwiseClone();
            CloneBaseProperties(cloneObj);
            return cloneObj;
        }
    }
}