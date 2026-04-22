using System;
using Xunit;
using Moq;
using ProtoBuf;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using System.Resources;
using CorvallisBus.Core.GtfsRealtimeGenerated;
using CorvallisBus.Core.Models;
using CorvallisBus.Core.Models.Gtfs;
using CorvallisBus.Core.DataAccess;
using CorvallisBus.Core.Models.Connexionz;
using CorvallisBus.Core.WebClients;

namespace CorvallisBus.Test
{
    public partial class TransitManagerTests
    {
        // [Fact]
        // public void TestTimerFunctionality()
        // {
        //     // Unused
        //     DateTimeOffset testTime = new DateTime(2015, 10, 20, 12, 00, 00);

        //     var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("CorvallisBus.Test.Resources.Alert.pb") ?? throw new Exception();
        //     var feed = Serializer.Deserialize<FeedMessage>(resource);
        //     var entity = feed.Entities.First();

        //     var alert = GtfsServiceAlert.Create(entity);

        //     var serviceAlerts = new List<GtfsServiceAlert> { };

        //     var mockRepo = new Mock<ITransitRepository>();
        //     mockRepo.Setup(repo => repo.GetServiceAlertsAsync()).Returns(Task.FromResult<List<GtfsServiceAlert>?>(serviceAlerts));

        //     var mockClient = new Mock<ITransitClient>();

        //     var expected = new Dictionary<string, ServiceAlert>
        //     {
        //         {
        //             "1", 
        //             new ServiceAlert(
        //                 Title: new Dictionary<string, string> {
        //                     { "en", "4/6-4/7 Routes 3, 8 & PC Detours" }
        //                 },
        //                 Description: new Dictionary<string, string> {
        //                     { "en", "This is a test Alert Message" }
        //                 }
        //             )
        //         }
        //     };

        //     var actual = TransitManager.GetServiceAlerts(mockRepo.Object, mockClient.Object, testTime).Result;

        //     Assert.Equal(expected.Count, actual.Count);
        //     foreach(KeyValuePair<string, ServiceAlert> entry in expected)
        //     {
        //         Assert.NotNull(actual[entry.Key]);
        //         Assert.Equal(entry.Value.Title, actual[entry.Key].Title);
        //         Assert.Equal(entry.Value.Description, actual[entry.Key].Description);
        //     }
        // }

        // [Fact]
        // public void TestServiceAlertFromClient()
        // {
        //     // Unused
        //     DateTimeOffset testTime = new DateTime(2015, 10, 20, 12, 00, 00);

        //     var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("CorvallisBus.Test.Resources.Alert.pb") ?? throw new Exception();
        //     var feed = Serializer.Deserialize<FeedMessage>(resource);
        //     var entity = feed.Entities.First();

        //     var alert = GtfsServiceAlert.Create(entity);

        //     var mockRepo = new Mock<ITransitRepository>();
        //     mockRepo.Setup(repo => repo.GetServiceAlertsAsync()).Returns(Task.FromResult<List<GtfsServiceAlert>?>(null));

        //     var mockClient = new Mock<ITransitClient>();
        //     mockClient.Setup(client => client.GetServiceAlerts()).Returns(Task.FromResult(new List<GtfsServiceAlert> { alert }));

        //     var expected = new Dictionary<string, ServiceAlert>
        //     {
        //         {
        //             "1", 
        //             new ServiceAlert(
        //                 Title: new Dictionary<string, string> {
        //                     { "en", "4/6-4/7 Routes 3, 8 & PC Detours" }
        //                 },
        //                 Description: new Dictionary<string, string> {
        //                     { "en", "This is a test Alert Message" }
        //                 }
        //             )
        //         }
        //     };

        //     var actual = TransitManager.GetServiceAlerts(mockRepo.Object, mockClient.Object, testTime).Result;

        //     Assert.Equal(expected.Count, actual.Count);
        //     foreach(KeyValuePair<string, ServiceAlert> entry in expected)
        //     {
        //         Assert.NotNull(actual[entry.Key]);
        //         Assert.Equal(entry.Value.Title, actual[entry.Key].Title);
        //         Assert.Equal(entry.Value.Description, actual[entry.Key].Description);
        //     }
        // }

        // [Fact]
        // public void TestServiceAlertRepositoryMissingData()
        // {
        //     // Unused
        //     DateTimeOffset testTime = new DateTime(2015, 10, 20, 12, 00, 00);

        //     var mockRepo = new Mock<ITransitRepository>();
        //     mockRepo.Setup(repo => repo.GetServiceAlertsAsync()).Returns(Task.FromResult<List<GtfsServiceAlert>?>(new List<GtfsServiceAlert> { }));

        //     var mockClient = new Mock<ITransitClient>();

        //     var expected = new Dictionary<string, ServiceAlert> { };

        //     var actual = TransitManager.GetServiceAlerts(mockRepo.Object, mockClient.Object, testTime).Result;

        //     Assert.Equal(expected.Count, actual.Count);
        //     Assert.Empty(actual);
        // }

        // [Fact]
        // public void TestServiceAlertRepositoryDataChange()
        // {
        //     // Unused
        //     DateTimeOffset testTime = new DateTime(2015, 10, 20, 12, 00, 00);

        //     const string en_header = "Service Alert";
        //     const string de_header = "Service-Meldung";

        //     const string en_description = "Description";
        //     const string de_description = "Beschreibung";

        //     var en_alert = new GtfsServiceAlert("1", new Dictionary<string, string>(){
        //         { "en", en_header }
        //     }, new Dictionary<string, string>(){
        //         { "en", en_description }
        //     });
        //     var de_alert = new GtfsServiceAlert("2", new Dictionary<string, string>(){
        //         { "de", de_header }
        //     }, new Dictionary<string, string>(){
        //         { "de", de_description }
        //     });

        //     var testRepositoryData = new List<GtfsServiceAlert> { en_alert };

        //     var mockRepo = new Mock<ITransitRepository>();
        //     mockRepo.Setup(repo => repo.GetServiceAlertsAsync()).Returns(Task.FromResult<List<GtfsServiceAlert>?>(testRepositoryData));

        //     var mockClient = new Mock<ITransitClient>();

        //     var en_expected = new Dictionary<string, ServiceAlert>
        //     {
        //         {
        //             "1", 
        //             new ServiceAlert(
        //                 Title: new Dictionary<string, string>() {
        //                     { "en", "Service Alert" }
        //                 },
        //                 Description: new Dictionary<string, string>() {
        //                     { "en", "Description" }
        //                 }
        //             )
        //         }
        //     };

        //     var en_actual = TransitManager.GetServiceAlerts(mockRepo.Object, mockClient.Object, testTime).Result;

        //     Assert.Equal(en_expected.Count, en_actual.Count);
        //     foreach(KeyValuePair<string, ServiceAlert> entry in en_expected)
        //     {
        //         Assert.NotNull(en_actual[entry.Key]);
        //         Assert.Equal(entry.Value.Title, en_actual[entry.Key].Title);
        //         Assert.Equal(entry.Value.Description, en_actual[entry.Key].Description);
        //     }

        //     testRepositoryData.Remove(en_alert);
        //     testRepositoryData.Add(de_alert);

        //     var de_expected = new Dictionary<string, ServiceAlert>
        //     {
        //         {
        //             "2", 
        //             new ServiceAlert(
        //                 Title: new Dictionary<string, string>() {
        //                     { "de", "Service-Meldung" }
        //                 },
        //                 Description: new Dictionary<string, string>() {
        //                     { "de", "Beschreibung" }
        //                 }
        //             )
        //         }
        //     };

        //     var de_actual = TransitManager.GetServiceAlerts(mockRepo.Object, mockClient.Object, testTime).Result;

        //     Assert.Equal(de_expected.Count, de_actual.Count);
        //     foreach(KeyValuePair<string, ServiceAlert> entry in de_expected)
        //     {
        //         Assert.NotNull(de_actual[entry.Key]);
        //         Assert.Equal(entry.Value.Title, de_actual[entry.Key].Title);
        //         Assert.Equal(entry.Value.Description, de_actual[entry.Key].Description);
        //     }
        // }

        // [Fact]
        // public void TestServiceAlertRepositoryMultipleAlerts()
        // {
        //     // Unused
        //     DateTimeOffset testTime = new DateTime(2015, 10, 20, 12, 00, 00);

        //     const string en_header = "Service Alert";
        //     const string de_header = "Service-Meldung";

        //     const string en_description = "Description";
        //     const string de_description = "Beschreibung";

        //     var en_alert = new GtfsServiceAlert("1", new Dictionary<string, string>(){
        //         { "en", en_header }
        //     }, new Dictionary<string, string>(){
        //         { "en", en_description }
        //     });
        //     var de_alert = new GtfsServiceAlert("2", new Dictionary<string, string>(){
        //         { "de", de_header }
        //     }, new Dictionary<string, string>(){
        //         { "de", de_description }
        //     });

        //     var testRepositoryData = new List<GtfsServiceAlert> { en_alert, de_alert };

        //     var mockRepo = new Mock<ITransitRepository>();
        //     mockRepo.Setup(repo => repo.GetServiceAlertsAsync()).Returns(Task.FromResult<List<GtfsServiceAlert>?>(testRepositoryData));

        //     var mockClient = new Mock<ITransitClient>();

        //     var expected = new Dictionary<string, ServiceAlert>
        //     {
        //         {
        //             "1", 
        //             new ServiceAlert(
        //                 Title: new Dictionary<string, string>() {
        //                     { "en", "Service Alert" }
        //                 },
        //                 Description: new Dictionary<string, string>() {
        //                     { "en", "Description" }
        //                 }
        //             )
        //         },
        //         {
        //             "2", 
        //             new ServiceAlert(
        //                 Title: new Dictionary<string, string>() {
        //                     { "de", "Service-Meldung" }
        //                 },
        //                 Description: new Dictionary<string, string>() {
        //                     { "de", "Beschreibung" }
        //                 }
        //             )
        //         }
        //     };

        //     var actual = TransitManager.GetServiceAlerts(mockRepo.Object, mockClient.Object, testTime).Result;

        //     Assert.Equal(expected.Count, actual.Count);
        //     foreach(KeyValuePair<string, ServiceAlert> entry in expected)
        //     {
        //         Assert.NotNull(actual[entry.Key]);
        //         Assert.Equal(entry.Value.Title, actual[entry.Key].Title);
        //         Assert.Equal(entry.Value.Description, actual[entry.Key].Description);
        //     }

        //     testRepositoryData.Remove(en_alert);
        //     testRepositoryData.Add(de_alert);
        // }
    }
}
