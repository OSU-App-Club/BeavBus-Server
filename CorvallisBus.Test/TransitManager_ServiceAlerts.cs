using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CorvallisBus.Core.DataAccess;
using CorvallisBus.Core.GtfsRealtimeGenerated;
using CorvallisBus.Core.Models;
using CorvallisBus.Core.WebClients;
using Moq;
using ProtoBuf;
using Xunit;

namespace CorvallisBus.Test
{
    public partial class TransitManagerTests
    {
        /// <summary>
        /// The filename for the embedded ServiceAlert Protobuf Test File
        /// </summary>
        static public string SERVICE_ALERT_PROTOBUF_FILE = "CorvallisBus.Test.Resources.ServiceAlert.pb";

        private FeedMessage LoadServiceAlert()
        {
            Stream? resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(SERVICE_ALERT_PROTOBUF_FILE) ?? throw new Exception();
            FeedMessage? feed = Serializer.Deserialize<FeedMessage>(resource);

            return feed;
        }

        [Fact]
        public async Task ServiceAlert_FromRepository()
        {
            ulong timestamp = LoadServiceAlert().Header.Timestamp;
            ServiceAlert? alert = ServiceAlert.Create(LoadServiceAlert().Entities[0], "en");
            Assert.NotNull(alert);

            var mockClient = new Mock<ITransitClient>();
            var mockRepo = new Mock<ITransitRepository>();
            mockRepo.Setup(repo => repo.GetServiceAlertsAsync()).Returns(
                Task.FromResult<(List<ServiceAlert>, ulong)?>((new List<ServiceAlert> { alert }, timestamp))
            );

            var actual = await TransitManager.GetServiceAlerts(mockRepo.Object, mockClient.Object);

            Assert.Single(actual);
            Assert.Equal("1", actual[0].Id);
            Assert.Equal("4/6-4/7 Routes 3, 8 & PC Detours", actual[0].Title);
            Assert.Equal("This is a test Alert Message", actual[0].Description);
        }

        [Fact]
        public async Task ServiceAlert_FromClient()
        {
            ulong timestamp = LoadServiceAlert().Header.Timestamp;
            ServiceAlert? alert = ServiceAlert.Create(LoadServiceAlert().Entities[0], "en");
            Assert.NotNull(alert);

            var mockClient = new Mock<ITransitClient>();
            var mockRepo = new Mock<ITransitRepository>();
            mockClient.Setup(client => client.GetServiceAlerts(null)).Returns(Task.FromResult<(List<ServiceAlert>, ulong)?>((new List<ServiceAlert> { alert }, timestamp)));
            mockRepo.Setup(repo => repo.GetServiceAlertsAsync()).Returns(
                Task.FromResult<(List<ServiceAlert>, ulong)?>(null)
            );

            var actual = await TransitManager.GetServiceAlerts(mockRepo.Object, mockClient.Object);

            Assert.Single(actual);
            Assert.Equal("1", actual[0].Id);
            Assert.Equal("4/6-4/7 Routes 3, 8 & PC Detours", actual[0].Title);
            Assert.Equal("This is a test Alert Message", actual[0].Description);
        }

        [Fact]
        public async Task ServiceAlert_ChangingData()
        {
            ulong timestamp = LoadServiceAlert().Header.Timestamp;
            ServiceAlert? alert = ServiceAlert.Create(LoadServiceAlert().Entities[0], "en");
            Assert.NotNull(alert);

            (List<ServiceAlert>, ulong)? testRepositoryData = (new List<ServiceAlert> { }, timestamp);

            var mockClient = new Mock<ITransitClient>();
            var mockRepo = new Mock<ITransitRepository>();
            mockRepo.Setup(repo => repo.GetServiceAlertsAsync()).Returns(Task.FromResult(testRepositoryData));

            var actual = await TransitManager.GetServiceAlerts(mockRepo.Object, mockClient.Object);

            Assert.Empty(actual);

            testRepositoryData.Value.Item1.Add(alert);
            actual = await TransitManager.GetServiceAlerts(mockRepo.Object, mockClient.Object);

            Assert.Single(actual);
            Assert.Equal("1", actual[0].Id);
            Assert.Equal("4/6-4/7 Routes 3, 8 & PC Detours", actual[0].Title);
            Assert.Equal("This is a test Alert Message", actual[0].Description);
        }

        [Fact]
        public async Task ServiceAlert_NoAlerts()
        {
            ulong timestamp = LoadServiceAlert().Header.Timestamp;

            var mockClient = new Mock<ITransitClient>();
            var mockRepo = new Mock<ITransitRepository>();
            mockRepo.Setup(repo => repo.GetServiceAlertsAsync()).Returns(
                Task.FromResult<(List<ServiceAlert>, ulong)?>((new List<ServiceAlert> { }, timestamp))
            );

            var actual = await TransitManager.GetServiceAlerts(mockRepo.Object, mockClient.Object);

            Assert.Empty(actual);
        }
    }
}