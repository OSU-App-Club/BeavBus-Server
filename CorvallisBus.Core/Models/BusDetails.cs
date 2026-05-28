using System.Collections.Generic;
using CorvallisBus.Core.Models.GtfsRealtime;
using Newtonsoft.Json;

namespace CorvallisBus.Core.Models
{
    /// <summary>
    /// Represents the configuration of a bus in relation to the trip
    /// </summary>
    /// <param name="Id">The Bus ID. Guaranteed to be static for a bus</param>
    /// <param name="Label">Publicly Exposed Bus Label</param>
    /// <param name="RouteId">Route ID of this bus</param>
    /// <param name="StoppedAt">ID of where the bus is stopped at, or "" if not stopped </param>
    /// <param name="StoppingAt">Dictionary of Stop IDs and Timestamps</param>
    public record BusDetails(
        [property: JsonProperty("id")]
        string Id,

        [property: JsonProperty("label")]
        string Label,

        [property: JsonProperty("routeId")]
        string RouteId,

        [property: JsonProperty("stoppedAt")]
        string StoppedAt,

        [property: JsonProperty("stoppingAt")]
        Dictionary<string, long> StoppingAt
    )
    {
        internal static BusDetails Create(TripUpdate update, string RouteId, string CurrentlyStoppedAt, Dictionary<string, long> StoppingAt)
        {
            return new BusDetails(
                Id: update.Id,
                Label: update.Label,
                RouteId: RouteId,
                StoppedAt: CurrentlyStoppedAt,
                StoppingAt: StoppingAt
            );
        }
    }
}