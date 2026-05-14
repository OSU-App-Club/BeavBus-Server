using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CorvallisBus.Core.DataAccess;
using CorvallisBus.Core.Models;
using CorvallisBus.Core.Models.Gtfs;
using CorvallisBus.Core.WebClients;
using Moq;
using Xunit;

namespace CorvallisBus.Test
{
    public partial class TransitManagerTests
    {
        private DateTimeOffset StubClientTime = new DateTime(2000, 01, 01, 12, 00, 00);

        [Fact]
        public void ServiceAlert_FromRepository()
        {
            GtfsServiceAlert alert = Utilities.LoadServiceAlert();
            Dictionary<string, string> title = new Dictionary<string, string> {
                { "en", "4/6-4/7 Routes 3, 8 & PC Detours" }
            };
            Dictionary<string, string> description = new Dictionary<string, string> {
                { "en", "This is a test Alert Message" }
            };
            Dictionary<string, ServiceAlert> expected = new Dictionary<string, ServiceAlert>
            {
                {
                    "1", 
                    new ServiceAlert(
                        Title: title,
                        Description: description
                    )
                }
            };

            // Mocks
            var mockClient = new Mock<ITransitClient>();
            var mockRepo = new Mock<ITransitRepository>();
            mockRepo.Setup(repo => repo.GetServiceAlertsAsync()).Returns(Task.FromResult<List<GtfsServiceAlert>?>(new List<GtfsServiceAlert> { alert }));

            
            Dictionary<string, ServiceAlert> actual = TransitManager.GetServiceAlerts(mockRepo.Object, mockClient.Object, StubClientTime).Result;

            Assert.Single(actual);

            Assert.NotNull(actual["1"]);
            Assert.Equal(expected["1"].Title, actual["1"].Title);
            Assert.Equal(expected["1"].Description, actual["1"].Description);
        }

        [Fact]
        public void ServiceAlert_FromRepository_NoAlerts()
        {
            Dictionary<string, ServiceAlert> expected = new Dictionary<string, ServiceAlert> { };

            var mockClient = new Mock<ITransitClient>();
            var mockRepo = new Mock<ITransitRepository>();
            mockRepo.Setup(repo => repo.GetServiceAlertsAsync()).Returns(Task.FromResult<List<GtfsServiceAlert>?>(new List<GtfsServiceAlert> { }));

            var actual = TransitManager.GetServiceAlerts(mockRepo.Object, mockClient.Object, StubClientTime).Result;

            Assert.Equal(expected.Count, actual.Count);
            Assert.Empty(actual);
        }

        [Fact]
        public void ServiceAlert_FromRepository_ChangingData()
        {
            const string en_header = "Service Alert";
            const string de_header = "Service-Meldung";

            const string en_description = "Description";
            const string de_description = "Beschreibung";

            var en_alert = new GtfsServiceAlert("1", new Dictionary<string, string>(){
                { "en", en_header }
            }, new Dictionary<string, string>(){
                { "en", en_description }
            });
            var de_alert = new GtfsServiceAlert("2", new Dictionary<string, string>(){
                { "de", de_header }
            }, new Dictionary<string, string>(){
                { "de", de_description }
            });

            var en_expected = new Dictionary<string, ServiceAlert>
            {
                {
                    "1", 
                    new ServiceAlert(
                        Title: new Dictionary<string, string>() {
                            { "en", en_header }
                        },
                        Description: new Dictionary<string, string>() {
                            { "en", en_description }
                        }
                    )
                }
            };

            var de_expected = new Dictionary<string, ServiceAlert>
            {
                {
                    "2", 
                    new ServiceAlert(
                        Title: new Dictionary<string, string>() {
                            { "de", de_header }
                        },
                        Description: new Dictionary<string, string>() {
                            { "de", de_description }
                        }
                    )
                }
            };

            var testRepositoryData = new List<GtfsServiceAlert> { en_alert };

            // Mocks
            var mockClient = new Mock<ITransitClient>();
            var mockRepo = new Mock<ITransitRepository>();
            mockRepo.Setup(repo => repo.GetServiceAlertsAsync()).Returns(Task.FromResult<List<GtfsServiceAlert>?>(testRepositoryData));

            var actual = TransitManager.GetServiceAlerts(mockRepo.Object, mockClient.Object, StubClientTime).Result;

            Assert.Single(actual);
            Assert.NotNull(actual["1"]);
            Assert.Equal(en_expected["1"].Title, actual["1"].Title);
            Assert.Equal(en_expected["1"].Description, actual["1"].Description);

            testRepositoryData.Remove(en_alert);
            testRepositoryData.Add(de_alert);

            actual = TransitManager.GetServiceAlerts(mockRepo.Object, mockClient.Object, StubClientTime).Result;

            Assert.Single(actual);
            Assert.NotNull(actual["2"]);
            Assert.Equal(de_expected["2"].Title, actual["2"].Title);
            Assert.Equal(de_expected["2"].Description, actual["2"].Description);
        }

        [Fact]
        public void ServiceAlert_FromRepository_MultipleAlerts()
        {
            const string en_header = "Service Alert";
            const string de_header = "Service-Meldung";

            const string en_description = "Description";
            const string de_description = "Beschreibung";

            var en_alert = new GtfsServiceAlert("1", new Dictionary<string, string>(){
                { "en", en_header }
            }, new Dictionary<string, string>(){
                { "en", en_description }
            });
            var de_alert = new GtfsServiceAlert("2", new Dictionary<string, string>(){
                { "de", de_header }
            }, new Dictionary<string, string>(){
                { "de", de_description }
            });

            var expected = new Dictionary<string, ServiceAlert>
            {
                {
                    "1", 
                    new ServiceAlert(
                        Title: new Dictionary<string, string>() {
                            { "en", en_header }
                        },
                        Description: new Dictionary<string, string>() {
                            { "en", en_description }
                        }
                    )
                },
                {
                    "2", 
                    new ServiceAlert(
                        Title: new Dictionary<string, string>() {
                            { "de", de_header }
                        },
                        Description: new Dictionary<string, string>() {
                            { "de", de_description }
                        }
                    )
                }
            };

            // Mocks
            var mockClient = new Mock<ITransitClient>();
            var mockRepo = new Mock<ITransitRepository>();
            mockRepo.Setup(repo => repo.GetServiceAlertsAsync()).Returns(Task.FromResult<List<GtfsServiceAlert>?>(new List<GtfsServiceAlert> { en_alert, de_alert }));

            var actual = TransitManager.GetServiceAlerts(mockRepo.Object, mockClient.Object, StubClientTime).Result;

            Assert.Equal(2, actual.Count);

            Assert.NotNull(actual["1"]);
            Assert.Equal(expected["1"].Title, actual["1"].Title);
            Assert.Equal(expected["1"].Description, actual["1"].Description);
            Assert.Equal(expected["2"].Title, actual["2"].Title);
            Assert.Equal(expected["2"].Description, actual["2"].Description);
        }

        [Fact]
        public void ServiceAlert_FromClient()
        {
            GtfsServiceAlert alert = Utilities.LoadServiceAlert();
            Dictionary<string, string> title = new Dictionary<string, string> {
                { "en", "4/6-4/7 Routes 3, 8 & PC Detours" }
            };
            Dictionary<string, string> description = new Dictionary<string, string> {
                { "en", "This is a test Alert Message" }
            };
            Dictionary<string, ServiceAlert> expected = new Dictionary<string, ServiceAlert>
            {
                {
                    "1", 
                    new ServiceAlert(
                        Title: title,
                        Description: description
                    )
                }
            };

            // Mocks
            var mockClient = new Mock<ITransitClient>();
            var mockRepo = new Mock<ITransitRepository>();
            mockClient.Setup(client => client.GetServiceAlerts(DateTimeOffset.MinValue)).Returns(Task.FromResult<List<GtfsServiceAlert>?>(new List<GtfsServiceAlert> { alert }));
            mockRepo.Setup(repo => repo.GetServiceAlertsAsync()).Returns(Task.FromResult<List<GtfsServiceAlert>?>(null));


            Dictionary<string, ServiceAlert> actual = TransitManager.GetServiceAlerts(mockRepo.Object, mockClient.Object, StubClientTime).Result;

            Assert.Single(actual);

            Assert.NotNull(actual["1"]);
            Assert.Equal(expected["1"].Title, actual["1"].Title);
            Assert.Equal(expected["1"].Description, actual["1"].Description);
        }
    }
}