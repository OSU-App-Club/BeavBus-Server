using System;
using System.IO;
using System.Reflection;
using CorvallisBus.Core.GtfsRealtimeGenerated;
using CorvallisBus.Core.Models;
using Newtonsoft.Json;
using ProtoBuf;
using Xunit;

namespace CorvallisBus.Test
{
    public class BusPosition_Tests
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

        [Theory]
        [InlineData(0, "1", "749", 1779327322, 44.5647659, -123.263306, 0.0)]
        [InlineData(1, "4", "761", 1779327301, 44.5646858, -123.263367, 7.0)]
        public void Gtfs_VehiclePosition_Deserialization_Record(int VehicleIndex, string VehicleId, string VehicleLabel, long VehicleTimestamp, float VehicleLatitude, float VehicleLongitude, float VehicleSpeed)
        {
            FeedMessage feed = LoadVehiclePosition();

            var position = feed.Entities[VehicleIndex];

            BusPosition vehicle = BusPosition.Create(position);

            Assert.Equal(VehicleId, vehicle.Id);
            Assert.Equal(VehicleLabel, vehicle.Label);
            Assert.Equal((ulong) VehicleTimestamp, vehicle.Timestamp);
            Assert.Equal(VehicleLatitude, vehicle.Latitude);
            Assert.Equal(VehicleLongitude, vehicle.Longitude);
            Assert.Equal(VehicleSpeed, vehicle.Speed);
        }

        [Fact]
        public void BusPosition_JSON_Serialization()
        {
            FeedMessage feed = LoadVehiclePosition();

            var position = feed.Entities[0];

            BusPosition vehicle = BusPosition.Create(position);

            string json = JsonConvert.SerializeObject(vehicle);
            Assert.Equal("{\"id\":\"1\",\"label\":\"749\",\"timestamp\":1779327322,\"latitude\":44.564766,\"longitude\":-123.263306,\"speed\":0.0}", json);
        }
    }
}