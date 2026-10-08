using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CorvallisBus.Core.Models;

namespace CorvallisBus.Core.DataAccess
{
    /// <summary>
    /// This interface abstracts over persistent and cache storage.
    /// </summary>
    public interface ITransitRepository
    {
        /// <summary>
        /// The filesystem path for static data.
        /// </summary>
        string StaticDataPath { get; }

        /// <summary>
        /// Returns route and stop information intended for direct client consumption.
        /// This is specifically left as a string instead of a BusStaticData
        /// to eliminate the need for deserialization and reserialization.
        /// </summary>
        Task<string> GetSerializedStaticDataAsync();

        /// <summary>
        /// Returns route and stop information intended for direct client consumption.
        /// </summary>
        Task<BusStaticData> GetStaticDataAsync();

        /// <summary>
        /// Get saved platform tags from repository
        /// </summary>
        Task<Dictionary<int, int>> GetPlatformTagsAsync();

        /// <summary>
        /// Get saved schedule data from repository
        /// </summary>
        Task<ServerBusSchedule> GetScheduleAsync();

        /// <summary>
        /// Get Service Alerts from Repository
        /// </summary>
        /// <returns>Tuple of Service Alerts and timestamp</returns>
        Task<(List<ServiceAlert>, ulong)?> GetServiceAlertsAsync();

        /// <summary>
        /// Get Bus Positions from Repository
        /// </summary>
        /// <returns>List of Bus Positions</returns>
        Task<List<BusPosition>?> GetBusPositionsAsync();

        /// <summary>
        /// Set static data for repository
        /// </summary>
        void SetStaticData(BusStaticData staticData);

        /// <summary>
        /// Set schedule for repository
        /// </summary>
        void SetSchedule(ServerBusSchedule schedule);

        /// <summary>
        /// Set platform tags for repository
        /// </summary>
        void SetPlatformTags(Dictionary<int, int> platformTags);

        /// <summary>
        /// Save Service Alerts into Repository
        /// </summary>
        /// <param name="serviceAlerts">List of Service Alerts</param>
        void SetServiceAlerts((List<ServiceAlert>, ulong)? serviceAlerts);

        /// <summary>
        /// Save Bus Positions into Repository
        /// </summary>
        /// <param name="busPositions">List of Bus Positions</param>
        void SetBusPositions(List<BusPosition>? busPositions);
    }
}