using System.Collections.Generic;
using CorvallisBus.Core.Models;
using CorvallisBus.Core.Models.Gtfs;
using Newtonsoft.Json;
using Xunit;

namespace CorvallisBus.Test
{
    public class ServiceAlert_Tests
    {
        [Fact]
        public void ServiceAlert_Generation()
        {
            GtfsServiceAlert gtfs_alert = Utilities.LoadServiceAlert();
            ServiceAlert alert = ServiceAlert.Create(gtfs_alert);

            Assert.Single(alert.Title);
            Assert.Single(alert.Description);

            Assert.Equal("4/6-4/7 Routes 3, 8 & PC Detours", alert.Title["en"]);
            Assert.Equal("This is a test Alert Message", alert.Description["en"]);
        }

        [Fact]
        public void ServiceAlert_Multilingual()
        {
            GtfsServiceAlert gtfs_alert = Utilities.CreateMultilingualServiceAlert();
            ServiceAlert alert = ServiceAlert.Create(gtfs_alert);

            Assert.Equal(2, alert.Title.Count);
            Assert.Contains("en", (IDictionary<string, string>)alert.Title);
            Assert.Contains("de", (IDictionary<string, string>)alert.Title);

            Assert.Equal("Service Alert", alert.Title["en"]);
            Assert.Equal("Service-Meldung", alert.Title["de"]);

            Assert.Equal(2, alert.Description.Count);
            Assert.Contains("en", (IDictionary<string, string>)alert.Description);
            Assert.Contains("de", (IDictionary<string, string>)alert.Description);

            Assert.Equal("Description", alert.Description["en"]);
            Assert.Equal("Beschreibung", alert.Description["de"]);
        }

        [Theory]
        [InlineData("en", "Service Alert", "Description")]
        [InlineData("de", "Service-Meldung", "Beschreibung")]
        [InlineData("fr", "Missing Localisation", "Missing Localisation")]
        public void ServiceAlert_Localised_Multilingual(string langCode, string expectedTitle, string expectedDescription)
        {
            GtfsServiceAlert gtfs_alert = Utilities.CreateMultilingualServiceAlert();
            LocalisedServiceAlert alert = LocalisedServiceAlert.Create(gtfs_alert, langCode);

            Assert.Equal(expectedTitle, alert.Title);
            Assert.Equal(expectedDescription, alert.Description);
        }

        [Fact]
        public void ServiceAlert_JSON_Serialization()
        {
            GtfsServiceAlert gtfs_alert = Utilities.LoadServiceAlert();
            ServiceAlert alert = ServiceAlert.Create(gtfs_alert);

            string json = JsonConvert.SerializeObject(alert);
            Assert.Equal("{\"title\":{\"en\":\"4/6-4/7 Routes 3, 8 & PC Detours\"},\"description\":{\"en\":\"This is a test Alert Message\"}}", json);
        }

        [Fact]
        public void ServiceAlert_Multilingual_JSON_Serialization()
        {
            GtfsServiceAlert gtfs_alert = Utilities.CreateMultilingualServiceAlert();
            ServiceAlert alert = ServiceAlert.Create(gtfs_alert);

            string json = JsonConvert.SerializeObject(alert);
            Assert.Equal("{\"title\":{\"en\":\"Service Alert\",\"de\":\"Service-Meldung\"},\"description\":{\"en\":\"Description\",\"de\":\"Beschreibung\"}}", json);
        }

        [Theory]
        [InlineData("en", "{\"title\":\"Service Alert\",\"description\":\"Description\"}")]
        [InlineData("de", "{\"title\":\"Service-Meldung\",\"description\":\"Beschreibung\"}")]
        [InlineData("fr", "{\"title\":\"Missing Localisation\",\"description\":\"Missing Localisation\"}")]
        public void ServiceAlert_Localised_Multilingual_JSON_Serialization(string langCode, string expectedJSON)
        {
            GtfsServiceAlert gtfs_alert = Utilities.CreateMultilingualServiceAlert();
            LocalisedServiceAlert alert = LocalisedServiceAlert.Create(gtfs_alert, langCode);

            string json = JsonConvert.SerializeObject(alert);
            Assert.Equal(expectedJSON, json);
        }
    }
}