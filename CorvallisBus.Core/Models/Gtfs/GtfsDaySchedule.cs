using System.Collections.Generic;

namespace CorvallisBus.Core.Models.Gtfs
{
    /// <summary>
    /// Represents a route's schedule for particular days of the week in Google Transit.
    /// </summary>
    public record GtfsDaySchedule(
        DaysOfWeek Days,
        List<GtfsStopSchedule> StopSchedules);
}