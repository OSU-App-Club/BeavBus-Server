using System.Collections.Generic;
using CorvallisBus.Core.Models.Gtfs;
using Xunit;

namespace CorvallisBus.Test
{
    public class Gtfs_ServiceAlert_Tests
    {
        private GtfsServiceAlert CreateMissingDescriptionMultilingualAlert()
        {
            const string en_header = "Service Alert";
            const string de_header = "Service-Meldung";

            const string en_description = "Description";
            
            return new GtfsServiceAlert("1", new Dictionary<string, string>(){
                { "en", en_header },
                { "de", de_header }
            }, new Dictionary<string, string>(){
                { "en", en_description }
            });
        }

        [Fact]
        public void Gtfs_ServiceAlert_LoadsFromFile()
        {
            GtfsServiceAlert alert = Utilities.LoadServiceAlert();

            Assert.Equal("1", alert.Id);
            Assert.Single(alert.Headers);
            Assert.Contains("en", (IDictionary<string, string>)alert.Headers);

            Assert.Equal("4/6-4/7 Routes 3, 8 & PC Detours", alert.Headers["en"]);
            Assert.Equal("This is a test Alert Message", alert.Descriptions["en"]);
        }

        [Fact]
        public void Gtfs_ServiceAlert_CreateMultiLanguage()
        {
            GtfsServiceAlert alert = Utilities.CreateMultilingualServiceAlert();

            Assert.Equal("1", alert.Id);
            Assert.Equal(2, alert.Headers.Count);
            Assert.Contains("en", (IDictionary<string, string>)alert.Headers);
            Assert.Contains("de", (IDictionary<string, string>)alert.Headers);

            Assert.Equal("Service Alert", alert.Headers["en"]);
            Assert.Equal("Service-Meldung", alert.Headers["de"]);

            Assert.Equal(2, alert.Descriptions.Count);
            Assert.Contains("en", (IDictionary<string, string>)alert.Descriptions);
            Assert.Contains("de", (IDictionary<string, string>)alert.Descriptions);

            Assert.Equal("Description", alert.Descriptions["en"]);
            Assert.Equal("Beschreibung", alert.Descriptions["de"]);
        }

        [Fact]
        public void Gtfs_ServiceAlert_MultiLanguage_MissingDescription()
        {
            GtfsServiceAlert alert = CreateMissingDescriptionMultilingualAlert();

            Assert.Equal("1", alert.Id);
            Assert.Equal(2, alert.Headers.Count);
            Assert.Contains("en", (IDictionary<string, string>)alert.Headers);
            Assert.Contains("de", (IDictionary<string, string>)alert.Headers);

            Assert.Equal("Service Alert", alert.Headers["en"]);
            Assert.Equal("Service-Meldung", alert.Headers["de"]);

            Assert.Single(alert.Descriptions);
            Assert.Contains("en", (IDictionary<string, string>)alert.Descriptions);

            Assert.Equal("Description", alert.Descriptions["en"]);
        }
    }
}