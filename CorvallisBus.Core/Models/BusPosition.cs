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
        float Speed
    )
    {
        /// <summary>
        /// Create a Bus Position from a raw GTFS Bus Position record
        /// </summary>
        /// <param name="gtfsBusPosition">a GTFS Bus Position record</param>
        /// <returns>a newly created BusPosition</returns>
        public static BusPosition Create(FeedEntity gtfsBusPosition)
        {
            return new BusPosition(
                gtfsBusPosition.Vehicle.Vehicle.Id,
                gtfsBusPosition.Vehicle.Vehicle.Label,
                gtfsBusPosition.Vehicle.Timestamp,
                gtfsBusPosition.Vehicle.Position.Latitude,
                gtfsBusPosition.Vehicle.Position.Longitude,
                gtfsBusPosition.Vehicle.Position.Speed
            );
        }
    }
}