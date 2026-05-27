using System.Collections.Generic;

namespace CorvallisBus.Core.Models.Gtfs
{
    /// <summary>
    /// Represents the schedule for a route taken from Google Transit.
    /// </summary>
    public record GtfsRouteSchedule(
        string RouteNo,
        List<GtfsDaySchedule> Days);
}