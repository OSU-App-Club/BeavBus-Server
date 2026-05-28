using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CorvallisBus.Core.Models;
using Newtonsoft.Json;
using System.IO;
using Microsoft.Extensions.Hosting;

namespace CorvallisBus.Core.DataAccess
{
    /// <summary>
    /// Transit repository which stores data in memory and in flat files for when memory is cleared.
    /// </summary>
    public class MemoryTransitRepository : ITransitRepository
    {
        private static Dictionary<int, int>? s_platformTags;
        private static ServerBusSchedule? s_schedule;
        private static string? s_serializedStaticData;
        private static BusStaticData? s_staticData;
        private static (List<ServiceAlert>, ulong)? s_serviceAlerts;
        private static (List<BusDetails>, ulong)? s_busDetails;
        private static List<BusPosition>? s_busPositions;

        private readonly string _platformTagsPath;
        private readonly string _schedulePath;

        /// <inheritdoc/>
        public string StaticDataPath { get; }

        /// <summary>
        /// Create an in-memory and local repository
        /// </summary>
        /// <param name="env">Location on disk to store permanent data</param>
        public MemoryTransitRepository(IHostEnvironment env)
        {
            var folder = env.ContentRootPath + "/cache";
            Directory.CreateDirectory(folder);

            _platformTagsPath = folder + "/platformTags.json";
            _schedulePath = folder + "/schedule.json";
            StaticDataPath = folder + "/staticData.json";
        }

        /// <inheritdoc/>
        public Task<Dictionary<int, int>> GetPlatformTagsAsync()
        {
            if (s_platformTags == null)
            {
                s_platformTags = JsonConvert.DeserializeObject<Dictionary<int, int>>(File.ReadAllText(_platformTagsPath)) ?? throw new InvalidOperationException();
            }
            return Task.FromResult(s_platformTags);
        }

        /// <inheritdoc/>
        public Task<ServerBusSchedule> GetScheduleAsync()
        {
            if (s_schedule == null)
            {
                s_schedule = JsonConvert.DeserializeObject<ServerBusSchedule>(File.ReadAllText(_schedulePath)) ?? throw new InvalidOperationException();
            }
            return Task.FromResult(s_schedule);
        }

        /// <inheritdoc/>
        public Task<string> GetSerializedStaticDataAsync()
        {
            if (s_serializedStaticData == null)
            {
                s_serializedStaticData = File.ReadAllText(StaticDataPath);
            }
            return Task.FromResult(s_serializedStaticData);
        }

        /// <inheritdoc/>
        public async Task<BusStaticData> GetStaticDataAsync()
        {
            if (s_staticData == null)
            {
                s_staticData = JsonConvert.DeserializeObject<BusStaticData>(await GetSerializedStaticDataAsync()) ?? throw new InvalidOperationException();
            }
            return s_staticData;
        }

        /// <inheritdoc/>
        public async Task<(List<ServiceAlert>, ulong)?> GetServiceAlertsAsync()
        {
            return s_serviceAlerts;
        }

        /// <inheritdoc/>
        public async Task<List<BusPosition>?> GetBusPositionsAsync()
        {
            return s_busPositions;
        }

        public async Task<(List<BusDetails>, ulong)?> GetBusDetailsAsync()
        {
            return s_busDetails;
        }

        /// <inheritdoc/>
        public void SetPlatformTags(Dictionary<int, int> platformTags)
        {
            s_platformTags = platformTags;
            File.WriteAllText(_platformTagsPath, JsonConvert.SerializeObject(platformTags));
        }

        /// <inheritdoc/>
        public void SetSchedule(ServerBusSchedule schedule)
        {
            s_schedule = schedule;
            File.WriteAllText(_schedulePath, JsonConvert.SerializeObject(schedule));
        }

        /// <inheritdoc/>
        public void SetStaticData(BusStaticData staticData)
        {
            s_staticData = staticData;
            s_serializedStaticData = JsonConvert.SerializeObject(staticData);
            File.WriteAllText(StaticDataPath, JsonConvert.SerializeObject(staticData));
        }

        /// <inheritdoc/>
        public void SetServiceAlerts((List<ServiceAlert>, ulong)? serviceAlerts)
        {
            s_serviceAlerts = serviceAlerts;
        }

        /// <inheritdoc/>
        public void SetBusPositions(List<BusPosition>? busPositions)
        {
            s_busPositions = busPositions;
        }

        /// <inheritdoc/>
        public void SetBusDetails((List<BusDetails>, ulong)? busDetails)
        {
            s_busDetails = busDetails;
        }
    }
}
