using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CorvallisBus.Core.GtfsRealtimeGenerated;
using CorvallisBus.Core.Models;
using CorvallisBus.Core.Models.GtfsRealtime;
using ProtoBuf;

namespace CorvallisBus.Core.WebClients
{
    // (FeedEntities, Timestamp)
    using Entity = (List<FeedEntity>, ulong)?;

    /// <summary>
    /// Exposes methods for retreiving vehicle and service alert data from a Gtfs realtime service
    /// </summary>
    internal class GtfsRealtimeClient(HttpClient client)
    {
        private const string BASE_URL = "http://www.corvallistransit.com/rtt/public/utility/gtfsrealtime.aspx/";

        private readonly HttpClient _client = client;

        private async Task<Entity> GetEntityAsync(string url, DateTimeOffset lastSavedTimestamp)
        {
            Stream? stream = await _client.GetStreamAsync(BASE_URL + url);
            FeedMessage? message = Serializer.Deserialize<FeedMessage>(stream);

            if (message is null)
            {
                return null;
            }

            ulong timestamp = message.Header.Timestamp;

            // If the last saved timestamp is less than the new timestamp, then the data is newer and should be used
            return ((ulong)lastSavedTimestamp.ToUnixTimeSeconds()) < timestamp ? (message.Entities, timestamp) : null;
        }

        /// <summary>
        /// Gets the most recent service alerts for the CTS network.
        /// </summary>
        /// <param name="lastSavedTimestamp">The last saved timestamp of the last service alerts</param>
        /// <returns>
        /// A List of Service Alerts, or Null if no new service alerts are present. Note that Null means that the data
        /// should not be updated, not that no data exists.
        /// </returns>
        public async Task<(List<ServiceAlert>, ulong)?> GetServiceAlerts(DateTimeOffset? lastSavedTimestamp)
        {
            Entity alerts = await GetEntityAsync("alert", lastSavedTimestamp ?? DateTimeOffset.UnixEpoch);

            if (alerts is null) return null;

            List<ServiceAlert>? alertsList = alerts?.Item1
            .Select(a =>
            {
                Func<TranslatedString, string?> languageCodeFunc = t =>
                {
                    if (t.Translations.Count == 0) return null;
                    return t.Translations.First().Language;
                };

                string? languageCode = a.Alert.HeaderText is not null ?
                    languageCodeFunc(a.Alert.HeaderText) :
                    languageCodeFunc(a.Alert.DescriptionText);

                try
                {
                    return ServiceAlert.Create(a, languageCode ?? "");
                }
                catch
                {
                    return null;
                }
            })
            .Where(sa => sa is not null)
            .Select(sa => sa!)
            .ToList();

            if (alertsList is null) return null;

            return (alertsList, alerts?.Item2 ?? (ulong)DateTimeOffset.UnixEpoch.ToUnixTimeSeconds());
        }

        public async Task<List<GtfsVehiclePosition>?> GetVehiclePositions(DateTimeOffset? lastSavedTimestamp)
        {
            Entity positions = await GetEntityAsync("vehicleposition", lastSavedTimestamp ?? DateTimeOffset.UnixEpoch);

            if (positions is null) return null;

            List<GtfsVehiclePosition>? positionsList = positions?.Item1
            .Select(GtfsVehiclePosition.Create)
            .Where(pa => pa is not null)
            .Select(pa => pa!)
            .ToList();

            if (positionsList is null) return null;

            return positionsList;
        }
    }
}