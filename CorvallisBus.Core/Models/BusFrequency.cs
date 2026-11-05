using System;

using CorvallisBus.Core.Models.Gtfs;

using Newtonsoft.Json;

namespace CorvallisBus.Core.Models
{
    /// <summary>
    /// Represents a particular route frequency. This is also known as a 'Trip'.
    /// A Frequency can be used to map buses currently travelling to a route ID,
    /// identifying what route a bus is doing.
    /// </summary>
    /// <param name="Id">The Frequency ID</param>
    /// <param name="RouteId">The Route ID. This matches the Route IDs exported throughout this server</param>
    public record BusFrequency(
        [property: JsonProperty("id")]
        int Id,

        [property: JsonProperty("routeId")]
        string RouteId)
    {
        internal static BusFrequency Create(TripsEntry trip)
        {
            return new BusFrequency(
                Id: trip.TripId,
                RouteId: trip.RouteId);
        }
    }
}