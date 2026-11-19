using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using CorvallisBus.Core.DataAccess;
using CorvallisBus.Core.Models;
using CorvallisBus.Core.WebClients;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CorvallisBus.Core
{
    /// <summary>
    /// Background Worker Object for the server. Runs scheduled tasks in a background process.
    /// </summary>
    /// <param name="logger">Logger Instance</param>
    /// <param name="repository">Transit Repository</param>
    /// <param name="client">Transit Client</param>
    public class Worker(ILogger<Worker> logger, ITransitRepository repository, ITransitClient client) : BackgroundService
    {
        private int _workerIntervalSeconds = 15;
        private int _workerIntervalDays = 7;

        /// <summary>
        /// Number of seconds to schedule the timer for
        /// </summary>
        public int WORKER_INTERVAL_SECONDS
        {
            get
            {
                return _workerIntervalSeconds;
            }
            set
            {
                _workerIntervalSeconds = value;
            }
        }

        /// <summary>
        /// Number of days to schedule the timer for
        /// </summary>
        public int WORKER_INTERVAL_DAYS
        {
            get
            {
                return _workerIntervalDays;
            }
            set
            {
                _workerIntervalDays = value;
            }
        }

        private readonly ILogger<Worker> _logger = logger;
        private readonly ITransitRepository _repository = repository;
        private readonly ITransitClient _client = client;

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BeavBus Server Worker Service Running");

            await PerformWeeklyWork();

            using PeriodicTimer timer = new(TimeSpan.FromSeconds(WORKER_INTERVAL_SECONDS));
            using PeriodicTimer weeklyTimer = new(TimeSpan.FromDays(WORKER_INTERVAL_DAYS));

            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await PerformWork();
                }
                while (await weeklyTimer.WaitForNextTickAsync(stoppingToken))
                {
                    await PerformWeeklyWork();
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Stopping BeavBus Server Worker Service");
            }
        }

        private async Task PerformWork()
        {
            _logger.LogInformation("Worker executing tasks at: {time}", DateTimeOffset.Now);

            // Service Alerts

            (List<ServiceAlert>, ulong)? currentAlerts = await _repository.GetServiceAlertsAsync();

            DateTimeOffset? timestamp = null;
            if (currentAlerts is not null) timestamp = DateTimeOffset.FromUnixTimeSeconds((long)currentAlerts.Value.Item2);

            var alerts = await _client.GetServiceAlerts(timestamp);
            _repository.SetServiceAlerts(alerts);

            // Bus Positions

            List<BusPosition>? busPositions = await _repository.GetBusPositionsAsync();
            DateTimeOffset? pos_timestamp = null;

            if (busPositions is not null) pos_timestamp = DateTimeOffset.FromUnixTimeMilliseconds(
                (long)(busPositions?.Select(p => p.Timestamp).DefaultIfEmpty().Max() ?? 0)
            );

            var positions = await _client.GetVehiclePositions(pos_timestamp, _repository);
            _repository.SetBusPositions(positions);
        }

        private async Task PerformWeeklyWork()
        {
            _logger.LogInformation("Worker executing tasks at: {time}", DateTimeOffset.Now);

            // Data Initialisation
            try
            {
                var (busSystemData, errors) = _client.LoadTransitData();

                _repository.SetStaticData(busSystemData.StaticData);
                _repository.SetPlatformTags(busSystemData.PlatformIdToPlatformTag);
                _repository.SetSchedule(busSystemData.Schedule);

                if (errors.Count != 0)
                {
                    _logger.LogWarning("Data Fetch Job had validation errors");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Data fetch job failed: {ex.Message}, {ex.StackTrace}");
            }
        }
    }
}