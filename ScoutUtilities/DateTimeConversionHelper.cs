// ***********************************************************************
// <copyright file="DateTimeConversionHelper.cs" company="Beckman Coulter Life Sciences">
//     Copyright (C) 2019 Beckman Coulter Life Sciences. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;

namespace ScoutUtilities
{
    public class DateTimeConversionHelper
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime LocalEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);


        public static DateTime FromSecondUnixToDateTime(ulong unixTime)
        {
            TimeZoneInfo zone = TimeZoneInfo.Local;
            var selectedTimeZone = TimeZoneInfo.FindSystemTimeZoneById(zone.Id);
            var date = TimeZoneInfo.ConvertTimeFromUtc(Epoch.AddSeconds(unixTime).ToUniversalTime(), selectedTimeZone);
            return date;
        }

        public static DateTime FromSecondUnixToDateTime(double unixTime)
        {
            TimeZoneInfo zone = TimeZoneInfo.Local;
            var selectedTimeZone = TimeZoneInfo.FindSystemTimeZoneById(zone.Id);
            var date = TimeZoneInfo.ConvertTimeFromUtc(Epoch.AddSeconds(unixTime).ToUniversalTime(), selectedTimeZone);
            return date;
        }

        public static DateTime FromMinUnixToDateTime(ulong unixTime)
        {
            TimeZoneInfo zone = TimeZoneInfo.Local;
            var selectedTimeZone = TimeZoneInfo.FindSystemTimeZoneById(zone.Id);
            var date = TimeZoneInfo.ConvertTimeFromUtc(Epoch.AddMinutes(unixTime).ToUniversalTime(), selectedTimeZone);
            return date;
        }

        public static DateTime FromDaysUnixToDateTime(ulong unixTime)
        {
            return LocalEpoch.AddDays(unixTime);
        }

        public static double DateTimeToUnixSecond(DateTime dateTime)
        {
            return dateTime.ToUniversalTime().Subtract(Epoch).TotalSeconds;
        }

        public static ulong DateTimeToUnixSecondRounded(DateTime dateTime)
        {
            return (ulong) DateTimeToUnixSecond(dateTime);
        }

        public static double DateTimeToEndOfDayUnixSecond(DateTime dateTime)
        {
            var endOfDay = dateTime.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            return DateTimeToUnixSecond(endOfDay);
        }

        public static ulong DateTimeToEndOfDayUnixSecondRounded(DateTime dateTime)
        {
            var endOfDay = dateTime.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            return DateTimeToUnixSecondRounded(endOfDay);
        }

        public static double DateTimeToUnixDays(DateTime dateTime)
        {
            return (TimeZoneInfo.ConvertTimeToUtc(dateTime) - Epoch).TotalDays;
        }

        public static double DateTimeToUnixMin(DateTime dateTime)
        {
            return (TimeZoneInfo.ConvertTimeToUtc(dateTime) - Epoch).TotalMinutes;
        }

        public static ulong DaysElapsedSinceEpochAbsolute(DateTime dateTime)
        {
            var absoluteDT = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToUInt64(absoluteDT.Subtract(Epoch).TotalDays);
        }

        public static DateTime DateTimeToEndOfDay(DateTime dateTime)
        {
            var endOfDay = dateTime.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            return endOfDay;
        }

        public static DateTime DateTimeToStartOfDay(DateTime dateTime)
        {
            var startOfDay = dateTime.Date;
            return startOfDay;
        }
    }
}