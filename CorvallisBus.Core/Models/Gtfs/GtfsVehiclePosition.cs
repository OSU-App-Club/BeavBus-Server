using CorvallisBus.Core.GtfsRealtimeGenerated;
using System.Collections.Generic;
using System.Linq;

namespace CorvallisBus.Core.Models.Gtfs
{
    /// <summary>
    /// Represents a CTS Service Alert with raw data from Gtfs.
    /// </summary>
    /// <param name="Id">
    /// The Service Alert ID
    /// </param>
    /// <param name="Headers">
    /// A set of localised headers, in the format "language code": "header value"
    /// </param>
    /// <param name="Descriptions">
    /// A set of localised descriptions, in the format "language code": "description value"
    /// </param>
    public record GtfsVehiclePosition(
        string Id,
        ulong Timestamp,
        uint CurrentStopSequence,
        VehiclePosition.VehicleStopStatus CurrentStopStatus,
        GtfsVehicleTrip TripInfo,
        GtfsVehiclePositionDetails Position,
        GtfsVehicleDescriptor Vehicle)
    {
        /// <summary>
        /// Create a Service Alert from a GTFS Feed Entity
        /// </summary>
        public static GtfsVehiclePosition Create(FeedHeader header, FeedEntity entity)
        {
            var id = entity.Id;
            var timestamp = header.Timestamp;

            var stop_seq = entity.Vehicle.CurrentStopSequence;
            var stop_status = entity.Vehicle.CurrentStatus;
                
            var trip = GtfsVehicleTrip.Create(entity);
            var position = GtfsVehiclePositionDetails.Create(entity);
            var vehicle = GtfsVehicleDescriptor.Create(entity);

            return new GtfsVehiclePosition(id, timestamp, stop_seq, stop_status, trip, position, vehicle);
        }
    }

    public record GtfsVehicleTrip(
        string TripID)
    {
        public static GtfsVehicleTrip Create(FeedEntity entity)
        {
            var trip = entity.Vehicle.Trip;

            return new GtfsVehicleTrip(trip.TripId);
        }
    }

    public record GtfsVehiclePositionDetails(
        float Latitude,
        float Longitude,
        float Speed)
    {
        public static GtfsVehiclePositionDetails Create(FeedEntity entity)
        {
            var pos = entity.Vehicle.Position;

            return new GtfsVehiclePositionDetails(pos.Latitude, pos.Longitude, pos.Speed);
        }
    }

    public record GtfsVehicleDescriptor(
        string Id,
        string Label)
    {
        public static GtfsVehicleDescriptor Create(FeedEntity entity)
        {
            var vehicle = entity.Vehicle.Vehicle;

            return new GtfsVehicleDescriptor(vehicle.Id, vehicle.Label);
        }
    }
}
