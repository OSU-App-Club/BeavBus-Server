
using CorvallisBus.Core.GtfsRealtimeGenerated;
using CorvallisBus.Core.Models;
using CorvallisBus.Core.Models.Gtfs;
using ProtoBuf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Resources;
using Xunit;

namespace CorvallisBus.Test
{
    public class GtfsAlertTests
    {
        [Fact]
        public void GtfsServiceAlertGeneration()
        {
            var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("CorvallisBus.Test.Resources.Alert.pb") ?? throw new Exception();
            var feed = Serializer.Deserialize<FeedMessage>(resource);
            var entity = feed.Entities.First();

            var alert = GtfsServiceAlert.Create(entity);

            Assert.Equal("1", alert.Id);
            Assert.Single(alert.Headers);
            Assert.Contains<string, string>("en", (IDictionary<string, string>)alert.Headers);

            Assert.Equal("4/6-4/7 Routes 3, 8 & PC Detours", alert.Headers["en"]);
            Assert.Equal("This is a test Alert Message", alert.Descriptions["en"]);
        }

        [Fact]
        public void GtfsServiceAlertMultiLanguage()
        {
            const string en_header = "Service Alert";
            const string de_header = "Service-Meldung";

            const string en_description = "Description";
            const string de_description = "Beschreibung";
            
            var alert = new GtfsServiceAlert("1", new Dictionary<string, string>(){
                { "en", en_header },
                { "de", de_header }
            }, new Dictionary<string, string>(){
                { "en", en_description },
                { "de", de_description }
            });

            Assert.Equal("1", alert.Id);
            Assert.Equal(2, alert.Headers.Count);
            Assert.Contains<string, string>("en", (IDictionary<string, string>)alert.Headers);
            Assert.Contains<string, string>("de", (IDictionary<string, string>)alert.Headers);

            Assert.Equal("Service Alert", alert.Headers["en"]);
            Assert.Equal("Service-Meldung", alert.Headers["de"]);

            Assert.Equal(2, alert.Descriptions.Count);
            Assert.Contains<string, string>("en", (IDictionary<string, string>)alert.Descriptions);
            Assert.Contains<string, string>("de", (IDictionary<string, string>)alert.Descriptions);

            Assert.Equal("Description", alert.Descriptions["en"]);
            Assert.Equal("Beschreibung", alert.Descriptions["de"]);
        }

        [Fact]
        public void GtfsServiceAlertMultiLanguageMissingDescription()
        {
            const string en_header = "Service Alert";
            const string de_header = "Service-Meldung";

            const string en_description = "Description";
            
            var alert = new GtfsServiceAlert("1", new Dictionary<string, string>(){
                { "en", en_header },
                { "de", de_header }
            }, new Dictionary<string, string>(){
                { "en", en_description }
            });

            Assert.Equal("1", alert.Id);
            Assert.Equal(2, alert.Headers.Count);
            Assert.Contains<string, string>("en", (IDictionary<string, string>)alert.Headers);
            Assert.Contains<string, string>("de", (IDictionary<string, string>)alert.Headers);

            Assert.Equal("Service Alert", alert.Headers["en"]);
            Assert.Equal("Service-Meldung", alert.Headers["de"]);

            Assert.Single(alert.Descriptions);
            Assert.Contains<string, string>("en", (IDictionary<string, string>)alert.Descriptions);

            Assert.Equal("Description", alert.Descriptions["en"]);
        }

        [Fact]
        public void ServiceAlertGeneration()
        {
            var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("CorvallisBus.Test.Resources.Alert.pb") ?? throw new Exception();
            var feed = Serializer.Deserialize<FeedMessage>(resource);
            var entity = feed.Entities.First();

            var gtfs_alert = GtfsServiceAlert.Create(entity);

            var alert = ServiceAlert.Create(gtfs_alert);

            Assert.Single(alert.Title);
            Assert.Single(alert.Description);

            Assert.Equal("4/6-4/7 Routes 3, 8 & PC Detours", alert.Title["en"]);
            Assert.Equal("This is a test Alert Message", alert.Description["en"]);
        }

        [Fact]
        public void ServiceAlertMultiLanguage()
        {
            const string en_header = "Service Alert";
            const string de_header = "Service-Meldung";

            const string en_description = "Description";
            const string de_description = "Beschreibung";

            var gtfs_alert = new GtfsServiceAlert("1", new Dictionary<string, string>(){
                { "en", en_header },
                { "de", de_header }
            }, new Dictionary<string, string>(){
                { "en", en_description },
                { "de", de_description }
            });

            var alert = ServiceAlert.Create(gtfs_alert);

            Assert.Equal(2, alert.Title.Count);
            Assert.Contains<string, string>("en", (IDictionary<string, string>)alert.Title);
            Assert.Contains<string, string>("de", (IDictionary<string, string>)alert.Title);

            Assert.Equal("Service Alert", alert.Title["en"]);
            Assert.Equal("Service-Meldung", alert.Title["de"]);

            Assert.Equal(2, alert.Description.Count);
            Assert.Contains<string, string>("en", (IDictionary<string, string>)alert.Description);
            Assert.Contains<string, string>("de", (IDictionary<string, string>)alert.Description);

            Assert.Equal("Description", alert.Description["en"]);
            Assert.Equal("Beschreibung", alert.Description["de"]);
        }

        [Fact]
        public void ServiceAlertLocalisedMultiLanguage()
        {
            const string en_header = "Service Alert";
            const string de_header = "Service-Meldung";

            const string en_description = "Description";
            const string de_description = "Beschreibung";

            var gtfs_alert = new GtfsServiceAlert("1", new Dictionary<string, string>(){
                { "en", en_header },
                { "de", de_header }
            }, new Dictionary<string, string>(){
                { "en", en_description },
                { "de", de_description }
            });

            var en_alert = LocalisedServiceAlert.Create(gtfs_alert, "en");

            Assert.Equal("Service Alert", en_alert.Title);
            Assert.Equal("Description", en_alert.Description);

            var de_alert = LocalisedServiceAlert.Create(gtfs_alert, "de");

            Assert.Equal("Service-Meldung", de_alert.Title);
            Assert.Equal("Beschreibung", de_alert.Description);

            var fr_alert = LocalisedServiceAlert.Create(gtfs_alert, "fr");

            Assert.Equal("Missing Localisation", fr_alert.Title);
            Assert.Equal("Missing Localisation", fr_alert.Description);
        }

        [Fact]
        public void ServiceAlertToJSON()
        {
            var alert = new ServiceAlert(new Dictionary<string, string>(){
                { "en", "Service Alert" }
            }, new Dictionary<string, string>(){
                { "en", "Description" },
            });

            string jsonString = JsonConvert.SerializeObject(alert);
            Assert.Equal("{\"title\":{\"en\":\"Service Alert\"},\"description\":{\"en\":\"Description\"}}", jsonString);
        }

        [Fact]
        public void ServiceAlertMultiLanguageToJSON()
        {
            const string en_header = "Service Alert";
            const string de_header = "Service-Meldung";

            const string en_description = "Description";
            const string de_description = "Beschreibung";

            var alert = new ServiceAlert(new Dictionary<string, string>(){
                { "en", en_header },
                { "de", de_header }
            }, new Dictionary<string, string>(){
                { "en", en_description },
                { "de", de_description }
            });

            string jsonString = JsonConvert.SerializeObject(alert);
            Assert.Equal("{\"title\":{\"en\":\"Service Alert\",\"de\":\"Service-Meldung\"},\"description\":{\"en\":\"Description\",\"de\":\"Beschreibung\"}}", jsonString);
        }

        [Fact]
        public void ServiceAlertLocalisedMultiLanguageToJSON()
        {
            const string en_header = "Service Alert";
            const string de_header = "Service-Meldung";

            const string en_description = "Description";
            const string de_description = "Beschreibung";

            var gtfs_alert = new GtfsServiceAlert("1", new Dictionary<string, string>(){
                { "en", en_header },
                { "de", de_header }
            }, new Dictionary<string, string>(){
                { "en", en_description },
                { "de", de_description }
            });

            var en_alert = LocalisedServiceAlert.Create(gtfs_alert, "en");

            string en_jsonString = JsonConvert.SerializeObject(en_alert);
            Assert.Equal("{\"title\":\"Service Alert\",\"description\":\"Description\"}", en_jsonString);

            var de_alert = LocalisedServiceAlert.Create(gtfs_alert, "de");

            string de_jsonString = JsonConvert.SerializeObject(de_alert);
            Assert.Equal("{\"title\":\"Service-Meldung\",\"description\":\"Beschreibung\"}", de_jsonString);

            var fr_alert = LocalisedServiceAlert.Create(gtfs_alert, "fr");

            string fr_jsonString = JsonConvert.SerializeObject(fr_alert);
            Assert.Equal("{\"title\":\"Missing Localisation\",\"description\":\"Missing Localisation\"}", fr_jsonString);
        }
    }
}