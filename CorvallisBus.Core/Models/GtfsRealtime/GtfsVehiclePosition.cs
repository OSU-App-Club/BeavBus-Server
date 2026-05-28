using CorvallisBus.Core.GtfsRealtimeGenerated;

namespace CorvallisBus.Core.Models.GtfsRealtime
{
    /// <summary>
    /// Stop Status for a Vehicle
    /// </summary>
    public enum VehiclePositionStopStatus {
        /// <summary>
        /// The Vehicle is approaching a stop
        /// </summary>
        Incoming = 0,
        /// <summary>
        /// The vehicle is at a stop
        /// </summary>
        Stopped = 1,
        /// <summary>
        /// The vehicle is in transit to a stop
        /// </summary>
        InTransit = 2,
    }

    /// <summary>
    /// A Raw Vehicle Position from GTFS Data
    /// </summary>
    /// <param name="Id">The Vehicle ID</param>
    /// <param name="Label">The Vehicle Label</param>
    /// <param name="Timestamp">Timestamp of this position</param>
    /// <param name="TripId">The ID of the trip the vehicle is on</param>
    /// <param name="Latitude">The latitude of the vehicle</param>
    /// <param name="Longitude">The longitude of the vehicle</param>
    /// <param name="Speed">The speed of the vehicle</param>
    /// <param name="CurrentStopSequence">Where in the trip the vehicle currently is</param>
    /// <param name="CurrentStopStatus">What the vehicle is doing with regards to the stop</param>
    public record GtfsVehiclePosition (
        string Id,
        string Label,
        ulong Timestamp,
        string TripId,
        float Latitude,
        float Longitude,
        float Speed,
        int CurrentStopSequence,
        VehiclePositionStopStatus CurrentStopStatus
    )
    {
        /// <summary>
        /// Create a new GTFS Vehicle Position from raw GTFS Realtime Data
        /// </summary>
        /// <param name="entity">A GTFS Realtime feed</param>
        /// <returns></returns>
        public static GtfsVehiclePosition Create(FeedEntity entity)
        {
            var vehicle_data = entity.Vehicle;

            var id = vehicle_data.Vehicle.Id;
            var vehicle_label = vehicle_data.Vehicle.Label;

            var timestamp = vehicle_data.Timestamp;

            var trip_id = vehicle_data.Trip.TripId;

            var latitude = vehicle_data.Position.Latitude;
            var longitude = vehicle_data.Position.Longitude;
            var speed = vehicle_data.Position.Speed;

            int sequence = (int) vehicle_data.CurrentStopSequence;
            VehiclePositionStopStatus status = (VehiclePositionStopStatus) vehicle_data.CurrentStatus;

            return new GtfsVehiclePosition(
                id,
                vehicle_label,
                timestamp,
                trip_id,
                latitude,
                longitude,
                speed,
                sequence,
                status
            );
        }
    }
}