using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using CorvallisBus.Core.DataAccess;
using CorvallisBus.Core.GtfsRealtimeGenerated;
using CorvallisBus.Core.Models;
// FIXME: Internal Data
using CorvallisBus.Core.Models.Gtfs;
using CorvallisBus.Core.WebClients;

using CsvHelper;

using Moq;

using ProtoBuf;

using Xunit;

namespace CorvallisBus.Test
{
    public partial class TransitManagerTests
    {
        /// <summary>
        /// The filename for the embedded VehiclePosition Protobuf Test File
        /// </summary>
        static public string VEHICLE_POSITION_PROTOBUF_FILE = "CorvallisBus.Test.Resources.VehiclePosition.pb";
        static public string TRIPS_FILE = "CorvallisBus.Test.Resources.Trips.txt";

        private FeedMessage LoadVehiclePosition()
        {
            Stream? resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(VEHICLE_POSITION_PROTOBUF_FILE) ?? throw new Exception();
            FeedMessage? feed = Serializer.Deserialize<FeedMessage>(resource);

            return feed;
        }

        // The way GtfsData handles loading information is complicated and likely tested somewhere else.
        // FIXME: Decouple Gtfs data loading from individual functions
        private List<TripsEntry> LoadTrips()
        {
            Stream? resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(TRIPS_FILE) ?? throw new Exception();
            using var csv = new CsvReader(new StreamReader(resource));
            var records = csv.GetRecords<TripsEntry>();
            var trips = records.ToList();
            return trips;
        }

        [Fact]
        public async Task BusPosition_FromRepository()
        {
            BusPosition position = BusPosition.Create(LoadVehiclePosition().Entities[0]);

            var mockClient = new Mock<ITransitClient>();
            var mockRepo = new Mock<ITransitRepository>();
            mockRepo.Setup(repo => repo.GetBusPositionsAsync()).Returns(
                Task.FromResult<List<BusPosition>?>(new List<BusPosition> { position })
            );

            var actual = await TransitManager.GetBusPositions(mockRepo.Object, mockClient.Object);

            Assert.Single(actual);
            Assert.Equal("1", actual[0].Id);
            Assert.Equal("749", actual[0].Label);
            Assert.Equal((ulong)1779327322, actual[0].Timestamp);
            Assert.Equal((float)44.5647659, actual[0].Latitude);
            Assert.Equal((float)-123.263306, actual[0].Longitude);
            Assert.Equal(0.0, actual[0].Speed);
        }

        [Fact]
        public async Task BusPosition_FromClient()
        {
            BusPosition position = BusPosition.Create(LoadVehiclePosition().Entities[0]);

            var mockClient = new Mock<ITransitClient>();
            var mockRepo = new Mock<ITransitRepository>();
            mockClient.Setup(client => client.GetVehiclePositions(null, mockRepo.Object)).Returns(Task.FromResult<List<BusPosition>?>(new List<BusPosition> { position }));
            mockRepo.Setup(repo => repo.GetBusPositionsAsync()).Returns(
                Task.FromResult<List<BusPosition>?>(null)
            );

            var actual = await TransitManager.GetBusPositions(mockRepo.Object, mockClient.Object);

            Assert.Single(actual);
            Assert.Equal("1", actual[0].Id);
            Assert.Equal("749", actual[0].Label);
            Assert.Equal((ulong)1779327322, actual[0].Timestamp);
            Assert.Equal((float)44.5647659, actual[0].Latitude);
            Assert.Equal((float)-123.263306, actual[0].Longitude);
            Assert.Equal(0.0, actual[0].Speed);
        }

        [Fact]
        public async Task BusPosition_ChangingData()
        {
            BusPosition position = BusPosition.Create(LoadVehiclePosition().Entities[0]);

            List<BusPosition>? testRepositoryData = new List<BusPosition> { };

            var mockClient = new Mock<ITransitClient>();
            var mockRepo = new Mock<ITransitRepository>();
            mockRepo.Setup(repo => repo.GetBusPositionsAsync()).Returns(Task.FromResult<List<BusPosition>?>(testRepositoryData));

            var actual = await TransitManager.GetBusPositions(mockRepo.Object, mockClient.Object);

            Assert.Empty(actual);

            testRepositoryData.Add(position);
            actual = await TransitManager.GetBusPositions(mockRepo.Object, mockClient.Object);

            Assert.Single(actual);
            Assert.Equal("1", actual[0].Id);
            Assert.Equal("749", actual[0].Label);
            Assert.Equal((ulong)1779327322, actual[0].Timestamp);
            Assert.Equal((float)44.5647659, actual[0].Latitude);
            Assert.Equal((float)-123.263306, actual[0].Longitude);
            Assert.Equal(0.0, actual[0].Speed);
        }

        [Fact]
        public async Task BusPosition_NoAlerts()
        {
            var mockClient = new Mock<ITransitClient>();
            var mockRepo = new Mock<ITransitRepository>();
            mockRepo.Setup(repo => repo.GetBusPositionsAsync()).Returns(
                Task.FromResult<List<BusPosition>?>(new List<BusPosition> { })
            );

            var actual = await TransitManager.GetBusPositions(mockRepo.Object, mockClient.Object);

            Assert.Empty(actual);
        }

        [Fact]
        public async Task BusPositions_MatchRoute()
        {
            BusStaticData staticData = new BusStaticData(Routes: new Dictionary<string, BusRoute>(), Stops: new Dictionary<int, BusStop>(), Trips: LoadTrips().Select(s => BusFrequency.Create(s)).ToList());
            BusPosition position = BusPosition.Create(LoadVehiclePosition().Entities[0], staticData);

            var mockClient = new Mock<ITransitClient>();
            var mockRepo = new Mock<ITransitRepository>();

            mockClient.Setup(client => client.GetVehiclePositions(null, mockRepo.Object)).Returns(Task.FromResult<List<BusPosition>?>(new List<BusPosition> { position }));
            mockRepo.Setup(repo => repo.GetBusPositionsAsync()).Returns(
                Task.FromResult<List<BusPosition>?>(null)
            );

            var actual = await TransitManager.GetBusPositions(mockRepo.Object, mockClient.Object);

            Assert.Single(actual);
            Assert.Equal("1", actual[0].Id);
            Assert.Equal("749", actual[0].Label);
            Assert.Equal((ulong)1779327322, actual[0].Timestamp);
            Assert.Equal((float)44.5647659, actual[0].Latitude);
            Assert.Equal((float)-123.263306, actual[0].Longitude);
            Assert.Equal(0.0, actual[0].Speed);
            Assert.Equal("3", actual[0].RouteId);
        }
    }
}