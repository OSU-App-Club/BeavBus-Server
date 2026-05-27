using CorvallisBus.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CorvallisBus.Core.DataAccess
{
    /// <summary>
    /// This interface abstracts over persistent and cache storage.
    /// </summary>
    public interface ITransitRepository
    {
        string StaticDataPath { get; }

        /// <summary>
        /// Returns route and stop information intended for direct client consumption.
        /// This is specifically left as a string instead of a BusStaticData
        /// to eliminate the need for deserialization and reserialization.
        /// </summary>
        Task<string> GetSerializedStaticDataAsync();

        Task<BusStaticData> GetStaticDataAsync();

        Task<Dictionary<int, int>> GetPlatformTagsAsync();

        Task<ServerBusSchedule> GetScheduleAsync();

        /// <summary>
        /// Get Service Alerts from Repository
        /// </summary>
        /// <returns>Tuple of Service Alerts and timestamp</returns>
        Task<(List<ServiceAlert>, ulong)?> GetServiceAlertsAsync();

        void SetStaticData(BusStaticData staticData);

        void SetSchedule(ServerBusSchedule schedule);

        void SetPlatformTags(Dictionary<int, int> platformTags);

        /// <summary>
        /// Save Service Alerts into Repository
        /// </summary>
        /// <param name="serviceAlerts">List of Service Alerts</param>
        void SetServiceAlerts((List<ServiceAlert>, ulong)? serviceAlerts);
    }
}
