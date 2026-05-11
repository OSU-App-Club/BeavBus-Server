using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CorvallisBus.Core.GtfsRealtimeGenerated;
using CorvallisBus.Core.Models.Gtfs;
using ProtoBuf;

namespace CorvallisBus.Test
{
    public class Utilities
    {
        /// <summary>
        /// The filename for the embedded ServiceAlert Protobuf Test File
        /// </summary>
        static public string SERVICE_ALERT_PROTOBUF_FILE = "CorvallisBus.Test.Resources.Alert.pb";
        
        static public GtfsServiceAlert LoadServiceAlert()
        {
            Stream? resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(SERVICE_ALERT_PROTOBUF_FILE) ?? throw new Exception();
            FeedMessage? feed = Serializer.Deserialize<FeedMessage>(resource);
            FeedEntity? entity = feed.Entities.First();

            return GtfsServiceAlert.Create(entity);
        }

        static public GtfsServiceAlert CreateMultilingualServiceAlert()
        {
            const string en_header = "Service Alert";
            const string de_header = "Service-Meldung";

            const string en_description = "Description";
            const string de_description = "Beschreibung";
            
            return new GtfsServiceAlert("1", new Dictionary<string, string>(){
                { "en", en_header },
                { "de", de_header }
            }, new Dictionary<string, string>(){
                { "en", en_description },
                { "de", de_description }
            });
        }
    }
}