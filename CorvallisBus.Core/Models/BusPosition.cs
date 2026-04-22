using CorvallisBus.Core.GtfsRealtimeGenerated;
using CorvallisBus.Core.Models.Gtfs;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace CorvallisBus.Core.Models
{
    /// <summary>
    /// Represents a CTS Service Alert.
    /// </summary>
    /// <param name="Title">
    /// Localised title of the alert, in the format `{"language code": "title"}`. Usually this contains details like the affected routes.
    /// </param>
    /// <param name="Description">
    /// Localised description of the alert, in the format `{"language code": "title"}`.
    /// </param>
    public record BusPosition(
        [property: JsonProperty("busLabel")]
        string Label, // vehicle label

        // FIXME: how do we want current status serialised?

        [property: JsonProperty("timestamp")]
        ulong Timestamp,

        [property: JsonProperty("latitude")]
        float Latitude,

        [property: JsonProperty("longitude")]
        float Longitude,

        [property: JsonProperty("speed")]
        float Speed)
    {
        /// <summary>
        /// Create a Bus Position from a GtfsVehiclePosition
        /// </summary>
        public static BusPosition Create(GtfsVehiclePosition vehiclePosition)
        {
            return new BusPosition(
                vehiclePosition.Vehicle.Label,
                vehiclePosition.Timestamp,
                vehiclePosition.Position.Latitude,
                vehiclePosition.Position.Longitude,
                vehiclePosition.Position.Speed);
        }
    }
}
