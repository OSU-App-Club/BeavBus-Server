using System;
using System.IO;
using System.Linq;
using System.Reflection;

using CorvallisBus.Core.GtfsRealtimeGenerated;

using ProtoBuf;

using Xunit;

namespace CorvallisBus.Test
{
    public class Gtfs_TripUpdate_Deserialization_Tests
    {
        /// <summary>
        /// The filename for the embedded TripUpdate Protobuf Test File
        /// </summary>
        static public string TRIP_UPDATE_PROTOBUF_FILE = "CorvallisBus.Test.Resources.TripUpdate.pb";

        private FeedMessage LoadTripUpdate()
        {
            Stream? resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(TRIP_UPDATE_PROTOBUF_FILE) ?? throw new Exception();
            FeedMessage? feed = Serializer.Deserialize<FeedMessage>(resource);

            return feed;
        }

        [Fact]
        public void Gtfs_TripUpdate_Deserialization_Header()
        {
            FeedMessage feed = LoadTripUpdate();

            var header = feed.Header;

            Assert.Equal("2.0", header.GtfsRealtimeVersion);
            Assert.Equal(FeedHeader.Incrementality.FullDataset, header.incrementality);
            Assert.Equal((ulong)1776316800, header.Timestamp);
        }

        [Theory]
        [InlineData(0, "277")]
        [InlineData(1, "278")]
        [InlineData(2, "298")]
        [InlineData(3, "354")]
        public void Gtfs_TripUpdate_Deserialization_Message(int TripIndex, string TripId)
        {
            FeedMessage feed = LoadTripUpdate();

            var entities = feed.Entities;

            Assert.Equal(4, entities.Count);

            var message = entities[TripIndex];

            Assert.Equal(TripId, message.Id);
            Assert.False(message.IsDeleted);

            Assert.Null(message.Vehicle);
            Assert.NotNull(message.TripUpdate);
            Assert.Null(message.Alert);
        }

        [Theory]
        [InlineData(0, "277")]
        [InlineData(1, "278")]
        [InlineData(2, "298")]
        [InlineData(3, "354")]
        public void Gtfs_TripUpdate_Deserialization_TripDetails(int TripIndex, string TripId)
        {
            FeedMessage feed = LoadTripUpdate();

            var trip = feed.Entities[TripIndex].TripUpdate;

            Assert.Equal(TripId, trip.Trip.TripId);

            // Validate Defaults
            Assert.Equal(0.0, trip.Trip.DirectionId);
            Assert.Empty(trip.Trip.RouteId);
            Assert.Empty(trip.Trip.StartDate);
            Assert.Empty(trip.Trip.StartTime);
            Assert.Equal(TripDescriptor.ScheduleRelationship.Scheduled, trip.Trip.schedule_relationship);

            Assert.Equal(0.0, trip.Delay);
            Assert.Equal(0.0, trip.Timestamp);
            Assert.Null(trip.trip_properties);
        }

        [Theory]
        [InlineData(0, "1", "749")]
        [InlineData(1, "1", "749")]
        [InlineData(2, "16", "762")]
        [InlineData(3, "14", "757")]
        public void Gtfs_TripUpdate_Deserialization_VehicleData(int TripIndex, string VehicleId, string VehicleLabel)
        {
            FeedMessage feed = LoadTripUpdate();

            var vehicle = feed.Entities[TripIndex].TripUpdate.Vehicle;

            Assert.Equal(VehicleId, vehicle.Id);
            Assert.Equal(VehicleLabel, vehicle.Label);

            // Validate Defaults
            Assert.Empty(vehicle.LicensePlate);
        }

        [Theory]
        [InlineData(0, 1, 1779326969, 1779327900, "419")]
        [InlineData(1, 1, 1779329644, 1779329700, "419")]
        [InlineData(2, 1, 1779326969, 1779327000, "359")]
        public void Gtfs_TripUpdate_Deserialization_SingleStopTimeUpdate(int TripIndex, double StopSequence, int ArrivalTime, int DepartureTime, string StopId)
        {
            FeedMessage feed = LoadTripUpdate();

            var updates = feed.Entities[TripIndex].TripUpdate.StopTimeUpdates;

            Assert.Single(updates);

            var update = updates.First();

            Assert.Equal(StopSequence, update.StopSequence);
            Assert.Equal(StopId, update.StopId);

            Assert.Equal(ArrivalTime, update.Arrival.Time);
            Assert.Equal(DepartureTime, update.Departure.Time);

            // Validate Defaults
            Assert.Equal(TripUpdate.StopTimeUpdate.ScheduleRelationship.Scheduled, update.schedule_relationship);
            Assert.Null(update.stop_time_properties);
            Assert.Equal(0.0, update.Arrival.Delay);
            Assert.Equal(0.0, update.Departure.Delay);
        }

        [Fact]
        public void Gtfs_TripUpdate_Deserialization_MultipleStopTimeUpdates_Departure()
        {
            FeedMessage feed = LoadTripUpdate();

            var updates = feed.Entities[3].TripUpdate.StopTimeUpdates;

            Assert.Equal(2, updates.Count);

            var update = updates.First();

            Assert.Equal(29.0, update.StopSequence);
            Assert.Equal("163", update.StopId);

            Assert.Equal(1779326928, update.Departure.Time);

            // Validate Defaults
            Assert.Equal(TripUpdate.StopTimeUpdate.ScheduleRelationship.Scheduled, update.schedule_relationship);
            Assert.Null(update.stop_time_properties);
            Assert.Equal(0.0, update.Departure.Delay);
            Assert.Null(update.Arrival);
        }

        [Fact]
        public void Gtfs_TripUpdate_Deserialization_MultipleStopTimeUpdates_Arrival()
        {
            FeedMessage feed = LoadTripUpdate();

            var updates = feed.Entities[3].TripUpdate.StopTimeUpdates;

            Assert.Equal(2, updates.Count);

            var update = updates.Last();

            Assert.Equal(30.0, update.StopSequence);
            Assert.Equal("164", update.StopId);

            Assert.Equal(1779326974, update.Arrival.Time);

            // Validate Defaults
            Assert.Equal(TripUpdate.StopTimeUpdate.ScheduleRelationship.Scheduled, update.schedule_relationship);
            Assert.Null(update.stop_time_properties);
            Assert.Equal(0.0, update.Arrival.Delay);
            Assert.Null(update.Departure);
        }
    }
}