using System;
using System.Collections.Generic;

namespace CorvallisBus.Core.Models.Gtfs
{
    /// <summary>
    /// Represents the times at which a bus stops at a particular stop on a particular day.
    /// </summary>
    public record GtfsStopSchedule(
        int PlatformTag,
        List<TimeSpan> Times);
}