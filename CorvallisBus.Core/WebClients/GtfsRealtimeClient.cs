using CorvallisBus.Core.GtfsRealtimeGenerated;
using CorvallisBus.Core.Models.Gtfs;
using ProtoBuf;
using System.Collections.Generic;
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

        /// <summary>
        /// Gets and deserialises GTFS Realtime Protobuf data from the specified CTS endpoints.
        /// </summary>
        private static async Task<FeedMessage> GetEntityAsync(string url)
        {
            var client = new HttpClient();
            var stream = await client.GetStreamAsync(url);
            
            var message = Serializer.Deserialize<FeedMessage>(stream);
            return message;
        }

        /// <summary>
        /// Gets any available service alerts for the CTS network.
        /// </summary>
        public async static Task<List<GtfsServiceAlert>> GetServiceAlerts()
        {
            var alerts = await GetEntityAsync(BASE_URL + "alert");

            return alerts.Entities.Select(alert => GtfsServiceAlert.Create(alert)).ToList();
        }

        public async static Task<List<GtfsVehiclePosition>> GetVehiclePositions()
        {
            var positions = await GetEntityAsync(BASE_URL + "vehicleposition");
            var header = positions.Header;

            return positions.Entities.Select(position => GtfsVehiclePosition.Create(header, position)).ToList();
        }
    }
}