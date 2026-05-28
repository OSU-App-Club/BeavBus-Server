using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CorvallisBus.Core.GtfsRealtimeGenerated;
using CorvallisBus.Core.Models;
using CorvallisBus.Core.Models.GtfsRealtime;
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

        private BusPosition LoadVehiclePosition()
        {
            Stream? resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(VEHICLE_POSITION_PROTOBUF_FILE) ?? throw new Exception();
            FeedMessage? feed = Serializer.Deserialize<FeedMessage>(resource);

            GtfsVehiclePosition gtfsPos = GtfsVehiclePosition.Create(feed.Entities[0]);

            return BusPosition.Create(gtfsPos);
        }
    
        [Fact]
        public void BusPosition_Generation()
        {
            BusPosition actual = LoadVehiclePosition();

            Assert.Equal("1", actual.Id);
            Assert.Equal("749", actual.Label);
            Assert.Equal((ulong) 1779327322, actual.Timestamp);
            Assert.Equal((float) 44.5647659, actual.Latitude);
            Assert.Equal((float) -123.263306, actual.Longitude);
            Assert.Equal(0.0, actual.Speed);
        }

        [Fact]
        public void BusPosition_JSON_Serialization()
        {
            BusPosition actual = LoadVehiclePosition();

            Assert.NotNull(actual);

            string json = JsonConvert.SerializeObject(actual);
            Assert.Equal("{\"id\":\"1\",\"label\":\"749\",\"timestamp\":1779327322,\"latitude\":44.564766,\"longitude\":-123.263306,\"speed\":0.0}", json);
        }
    }
}