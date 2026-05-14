using CorvallisBus.Core.GtfsRealtimeGenerated;
using CorvallisBus.Core.Models.Gtfs;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

// FIXME: Optional/Nulable type for values where timestamp isn't updated
namespace CorvallisBus.Core.WebClients
{
    /// <summary>
    /// Exposes methods for for retreiving realtime vehicle and alert data from CTS.
    /// </summary>
    public static class GtfsRealtimeClient
    {
        private const string BASE_URL = "http://www.corvallistransit.com/rtt/public/utility/gtfsrealtime.aspx/";

        private static async Task<Stream> GetDataStreamAsync(string url)
        {
            // FIXME: IHttpClientFactory
            var client = new HttpClient();
            var stream = await client.GetStreamAsync(url);
            return stream;
        }

        /// <summary>
        /// Gets and deserialises GTFS Realtime Protobuf data from the specified CTS endpoints.
        /// </summary>
        private static async Task<(List<FeedEntity>, ulong)?> GetEntityAsync(string url, DateTimeOffset lastSavedTimestamp)
        {
            var stream = await GetDataStreamAsync(url);
            
            var message = Serializer.Deserialize<FeedMessage>(stream);
            var timestamp = message.Header.Timestamp;

            // If the last saved timestamp is less than the new timestamp, then the data is newer
            return (ulong) lastSavedTimestamp.ToUnixTimeSeconds() > timestamp ? (message.Entities, timestamp) : null;
        }

        /// <summary>
        /// Gets any available service alerts for the CTS network.
        /// </summary>
        public async static Task<List<GtfsServiceAlert>?> GetServiceAlerts(DateTimeOffset lastSavedTimestamp)
        {
            var alerts = await GetEntityAsync(BASE_URL + "alert", lastSavedTimestamp);

            return alerts is not null ? alerts?.Item1.Select(GtfsServiceAlert.Create).ToList() : null;
        }

        /// <summary>
        /// Gets the current vehicle positions for all active vehicles in the CTS network
        /// </summary>
        public async static Task<List<GtfsVehiclePosition>?> GetVehiclePositions(DateTimeOffset lastSavedTimestamp)
        {
            var positions = await GetEntityAsync(BASE_URL + "vehicleposition", lastSavedTimestamp);

            return positions is not null ? positions?.Item1.Select(position => GtfsVehiclePosition.Create(position, positions?.Item2 ?? 0)).ToList() : null;
        }
    }
}