using CorvallisBus.Core.GtfsRealtimeGenerated;
using ProtoBuf;
using System;
using System.IO;
using System.Reflection;
using Xunit;

namespace CorvallisBus.Test
{
    public class Gtfs_VehiclePosition_Deserialization_Tests
    {
        /// <summary>
        /// The filename for the embedded VehiclePosition Protobuf Test File
        /// </summary>
        static public string VEHICLE_POSITION_PROTOBUF_FILE = "CorvallisBus.Test.Resources.VehiclePosition.pb";

        private FeedMessage LoadVehiclePosition()
        {
            Stream? resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(VEHICLE_POSITION_PROTOBUF_FILE) ?? throw new Exception();
            FeedMessage? feed = Serializer.Deserialize<FeedMessage>(resource);
            
            return feed;
        }

        [Fact]
        public void Gtfs_VehiclePosition_Deserialization_Header()
        {
            FeedMessage feed = LoadVehiclePosition();

            var header = feed.Header;

            Assert.Equal("2.0", header.GtfsRealtimeVersion);
            Assert.Equal(FeedHeader.Incrementality.FullDataset, header.incrementality);
            Assert.Equal((ulong) 1776316800, header.Timestamp);
        }

        [Theory]
        [InlineData(0, "1")]
        [InlineData(1, "4")]
        public void Gtfs_VehiclePosition_Deserialization_Message(int VehicleIndex, string VehicleId)
        {
            FeedMessage feed = LoadVehiclePosition();

            var entities = feed.Entities;

            Assert.Equal(2, entities.Count);

            var message = entities[VehicleIndex];

            Assert.Equal(VehicleId, message.Id);
            Assert.False(message.IsDeleted);

            Assert.NotNull(message.Vehicle);
            Assert.Null(message.TripUpdate);
            Assert.Null(message.Alert);
        }

        [Theory]
        [InlineData(0, 1, VehiclePosition.VehicleStopStatus.StoppedAt, 1779327322)]
        [InlineData(1, 11, VehiclePosition.VehicleStopStatus.InTransitTo, 1779327301)]
        public void Gtfs_VehiclePosition_Deserialization_VehicleDetails(int VehicleIndex, uint StopSequence, VehiclePosition.VehicleStopStatus StopStatus, ulong Timestamp)
        {
            FeedMessage feed = LoadVehiclePosition();

            var vehicle = feed.Entities[VehicleIndex].Vehicle;

            Assert.Equal(StopSequence, vehicle.CurrentStopSequence);
            Assert.Equal(StopStatus, vehicle.CurrentStatus);
            Assert.Equal(Timestamp, vehicle.Timestamp);

            // Validate Defaults
            Assert.Equal(VehiclePosition.CongestionLevel.UnknownCongestionLevel, vehicle.congestion_level);
            Assert.Empty(vehicle.MultiCarriageDetails);
            Assert.Equal(0.0, vehicle.OccupancyPercentage);
            Assert.Equal(VehiclePosition.OccupancyStatus.Empty, vehicle.occupancy_status);
            Assert.Empty(vehicle.StopId);
        }

        [Theory]
        [InlineData(0, "277")]
        [InlineData(1, "640")]
        public void Gtfs_VehiclePosition_Deserialization_TripData(int VehicleIndex, string TripId)
        {
            FeedMessage feed = LoadVehiclePosition();

            var trip = feed.Entities[VehicleIndex].Vehicle.Trip;

            Assert.Equal(TripId, trip.TripId);

            // Validate Defaults
            Assert.Equal(0.0, trip.DirectionId);
            Assert.Empty(trip.RouteId);
            Assert.Empty(trip.StartDate);
            Assert.Empty(trip.StartTime);
            Assert.Equal(TripDescriptor.ScheduleRelationship.Scheduled, trip.schedule_relationship);
        }

        [Theory]
        [InlineData(0, "1", "749")]
        [InlineData(1, "4", "761")]
        public void Gtfs_VehiclePosition_Deserialization_VehicleData(int VehicleIndex, string VehicleId, string VehicleLabel)
        {
            FeedMessage feed = LoadVehiclePosition();

            var entity = feed.Entities[VehicleIndex];

            Assert.Equal(VehicleId, entity.Id);

            var vehicle = entity.Vehicle.Vehicle;

            Assert.Equal(vehicle.Id, entity.Id);
            Assert.Equal(VehicleId, vehicle.Id);

            Assert.Equal(VehicleLabel, vehicle.Label);

            // Validate Defaults
            Assert.Empty(vehicle.LicensePlate);
        }

        [Theory]
        [InlineData(0, 44.5647659, -123.263306, 0.0)]
        [InlineData(1, 44.5646858, -123.263367, 7.0)]
        public void Gtfs_VehiclePosition_Deserialization_PositionData(int VehicleIndex, float VehicleLatitude, float VehicleLongitude, float VehicleSpeed)
        {
            FeedMessage feed = LoadVehiclePosition();

            var position = feed.Entities[VehicleIndex].Vehicle.Position;

            Assert.Equal(VehicleLatitude, position.Latitude);
            Assert.Equal(VehicleLongitude, position.Longitude);
            Assert.Equal(VehicleSpeed, position.Speed);

            // Validate Defaults
            Assert.Equal(0.0, position.Bearing);
            Assert.Equal(0.0, position.Odometer);
        }
    }
}