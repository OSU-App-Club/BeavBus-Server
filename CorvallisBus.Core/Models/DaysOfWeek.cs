using System;
using System.Text.RegularExpressions;
using System.Linq;

namespace CorvallisBus.Core.Models
{
    /// <summary>
    /// Represents the days of the week in during which a schedule is in effect.
    /// Differs from the built in DayOfWeek because it's intended to be ORed together.
    /// </summary>
    [Flags]
    public enum DaysOfWeek
    {
        /// <summary></summary>
        Sunday = 1,

        /// <summary></summary>
        Monday = 2,

        /// <summary></summary>
        Tuesday = 4,

        /// <summary></summary>
        Wednesday = 8,

        /// <summary></summary>
        Thursday = 16,

        /// <summary></summary>
        Friday = 32,

        /// <summary></summary>
        Saturday = 64,

        /// <summary></summary>
        None = 0,

        /// <summary></summary>
        All = Weekdays | Weekend,

        /// <summary></summary>
        Weekdays = Monday | Tuesday | Wednesday | Thursday | Friday,

        /// <summary></summary>
        Weekend = Sunday | Saturday
    }

    /// <summary>
    /// Utility functions for working with weekdays
    /// </summary>
    public static class DaysOfWeekUtils
    {
        /// <summary>
        /// Convert a string to a weekday representation
        /// </summary>
        public static DaysOfWeek ToDaysOfWeek(string day)
        {
            switch (day)
            {
                case "Mon": return DaysOfWeek.Monday;
                case "Tue": return DaysOfWeek.Tuesday;
                case "Wed": return DaysOfWeek.Wednesday;
                case "Thu": return DaysOfWeek.Thursday;
                case "Fri": return DaysOfWeek.Friday;
                case "Sat": return DaysOfWeek.Saturday;
                case "Sun": return DaysOfWeek.Sunday;
                default: return DaysOfWeek.None;
            }
        }

        /// <summary>
        /// There is a real reason to have this enum instead of just DayOfWeek--
        /// it's useful to be able to OR days together the way we do.
        /// </summary>
        /// <param name="day"></param>
        /// <returns></returns>
        public static DaysOfWeek ToDaysOfWeek(DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Monday: return DaysOfWeek.Monday;
                case DayOfWeek.Tuesday: return DaysOfWeek.Tuesday;
                case DayOfWeek.Wednesday: return DaysOfWeek.Wednesday;
                case DayOfWeek.Thursday: return DaysOfWeek.Thursday;
                case DayOfWeek.Friday: return DaysOfWeek.Friday;
                case DayOfWeek.Saturday: return DaysOfWeek.Saturday;
                case DayOfWeek.Sunday: return DaysOfWeek.Sunday;
                default: return DaysOfWeek.None;
            }
        }

        /// <summary>
        /// Returns a value indicating whether the provided DateTimeOffset falls in to the DaysOfWeek specified
        /// </summary>
        public static bool TodayMayFallInsideDaySchedule(BusStopRouteDaySchedule ds, DateTimeOffset currentTime)
        {
            DaysOfWeek currentDay = ToDaysOfWeek(currentTime.DayOfWeek);

            // simple case where current time is definitely inside the current schedule
            return (ds.Days & currentDay) == currentDay || TimeInSpilloverWindow(ds, currentTime);
        }

        /// <summary>
        /// Returns true if the current time is in the early morning of e.g. Tuesday, and this day schedule contains 'late night runs' for a Monday schedule that spill over in to Tuesday morning
        /// </summary>
        public static bool TimeInSpilloverWindow(BusStopRouteDaySchedule ds, DateTimeOffset currentTime)
        {
            DaysOfWeek previousDay = ToDaysOfWeek(currentTime.AddDays(-1).DayOfWeek);
            TimeSpan lastTime = ds.Times.Last();

            if ((ds.Days & previousDay) == previousDay && lastTime.Days >= 1)
            {
                DateTimeOffset lastScheduleDateTime = new DateTimeOffset(currentTime.Year, currentTime.Month, currentTime.Day, lastTime.Hours, lastTime.Minutes, 0, 0, currentTime.Offset);
                return currentTime < lastScheduleDateTime.AddMinutes(TransitManager.ESTIMATES_MAX_ADVANCE_MINUTES);
            }

            return false;
        }
    }
}
