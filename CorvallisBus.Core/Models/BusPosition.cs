using System.Linq;

using CorvallisBus.Core.DataAccess;
using CorvallisBus.Core.GtfsRealtimeGenerated;

using Newtonsoft.Json;

namespace CorvallisBus.Core.Models
{
    /// <summary>
    /// Represents the position of a bus in relation to geospatial location
    /// and trip status.
    /// </summary>
    /// <param name="Id">The Bus ID. Guaranteed to be static for a bus</param>
    /// <param name="Label">Publicly Exposed Bus Label</param>
    /// <param name="Timestamp">Timestamp of the position update</param>
    /// <param name="Latitude">Latitude of the bus</param>
    /// <param name="Longitude">Longitude of the bus</param>
    /// <param name="Speed">Current bus speed (for estimations and live updating)</param>
    /// <param name="RouteId">The route this bus is currently on</param>
    public record BusPosition(
        [property: JsonProperty("id")]
        string Id,

        [property: JsonProperty("label")]
        string Label,

        [property: JsonProperty("timestamp")]
        ulong Timestamp,

        [property: JsonProperty("latitude")]
        float Latitude,

        [property: JsonProperty("longitude")]
        float Longitude,

        [property: JsonProperty("speed")]
        float Speed,

        [property: JsonProperty("routeId")]
        string RouteId
    )
    {
        // For testing deserialization
        internal static BusPosition Create(FeedEntity gtfsBusPosition)
        {
            return new BusPosition(
                gtfsBusPosition.Vehicle.Vehicle.Id,
                gtfsBusPosition.Vehicle.Vehicle.Label,
                gtfsBusPosition.Vehicle.Timestamp,
                gtfsBusPosition.Vehicle.Position.Latitude,
                gtfsBusPosition.Vehicle.Position.Longitude,
                gtfsBusPosition.Vehicle.Position.Speed,
                ""
            );
        }

        internal static BusPosition Create(FeedEntity gtfsBusPosition, BusStaticData staticData)
        {
            string tripId = gtfsBusPosition.Vehicle.Trip.TripId;
            string routeId = staticData.Trips.Where(t => t.Id.ToString() == tripId).First().RouteId;

            return new BusPosition(
                gtfsBusPosition.Vehicle.Vehicle.Id,
                gtfsBusPosition.Vehicle.Vehicle.Label,
                gtfsBusPosition.Vehicle.Timestamp,
                gtfsBusPosition.Vehicle.Position.Latitude,
                gtfsBusPosition.Vehicle.Position.Longitude,
                gtfsBusPosition.Vehicle.Position.Speed,
                routeId
            );
        }
    }
}