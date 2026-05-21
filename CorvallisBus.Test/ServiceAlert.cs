using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CorvallisBus.Core.GtfsRealtimeGenerated;
using CorvallisBus.Core.Models;
using Newtonsoft.Json;
using ProtoBuf;
using Xunit;

namespace CorvallisBus.Test
{
    public class ServiceAlert_Tests
    {
        static public string SERVICE_ALERT_PROTOBUF_FILE = "CorvallisBus.Test.Resources.ServiceAlert.pb";

        static public ServiceAlert? LoadServiceAlert()
        {
            Stream? resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(SERVICE_ALERT_PROTOBUF_FILE) ?? throw new Exception();
            FeedMessage? feed = Serializer.Deserialize<FeedMessage>(resource);
            FeedEntity? entity = feed.Entities.First();

            return ServiceAlert.Create(entity, entity.Alert.HeaderText.Translations.First().Language);
        }
    
        [Fact]
        public void ServiceAlert_Generation()
        {
            ServiceAlert? alert = LoadServiceAlert();

            Assert.NotNull(alert);

            Assert.Equal("4/6-4/7 Routes 3, 8 & PC Detours", alert.Title);
            Assert.Equal("This is a test Alert Message", alert.Description);
        }

        [Fact]
        public void ServiceAlert_FailedGeneration_Header()
        {
            Stream? resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(SERVICE_ALERT_PROTOBUF_FILE) ?? throw new Exception();
            FeedMessage? feed = Serializer.Deserialize<FeedMessage>(resource);
            
            Assert.Equal(3, feed.Entities.Count);

            FeedEntity? entity = feed.Entities[1];

            Exception exp = Assert.Throws<Exception>(() => ServiceAlert.Create(entity, entity.Alert.HeaderText.Translations.First().Language));
            Assert.Equal("Entity '2' has more than one title for language 'en'", exp.Message);
        }

        [Fact]
        public void ServiceAlert_FailedGeneration_Description()
        {
            Stream? resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(SERVICE_ALERT_PROTOBUF_FILE) ?? throw new Exception();
            FeedMessage? feed = Serializer.Deserialize<FeedMessage>(resource);
            
            Assert.Equal(3, feed.Entities.Count);

            FeedEntity? entity = feed.Entities.Last();

            Exception exp = Assert.Throws<Exception>(() => ServiceAlert.Create(entity, entity.Alert.HeaderText.Translations.First().Language));
            Assert.Equal("Entity '3' has more than one description for language 'en'", exp.Message);
        }

        [Fact]
        public void ServiceAlert_JSON_Serialization()
        {
            ServiceAlert? alert = LoadServiceAlert();

            Assert.NotNull(alert);

            string json = JsonConvert.SerializeObject(alert);
            Assert.Equal("{\"title\":\"4/6-4/7 Routes 3, 8 & PC Detours\",\"description\":\"This is a test Alert Message\"}", json);
        }
    }
}