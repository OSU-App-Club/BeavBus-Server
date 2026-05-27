using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CorvallisBus.Core.DataAccess;
using CorvallisBus.Core.Models;
using CorvallisBus.Core.WebClients;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CorvallisBus.Core
{
    public class Worker(ILogger<Worker> logger, ITransitRepository repository, ITransitClient client) : BackgroundService
    {
        /// <summary>
        /// Number of seconds to schedule the timer for
        /// </summary>
        public const int WORKER_INTERVAL_SECONDS = 15;
        
        private readonly ILogger<Worker> _logger = logger;
        private readonly ITransitRepository _repository = repository;
        private readonly ITransitClient _client = client;

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BeavBus Server Worker Service Running");

            await PerformWork();

            using PeriodicTimer timer = new(TimeSpan.FromSeconds(WORKER_INTERVAL_SECONDS));

            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await PerformWork();
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
            if (currentAlerts is not null) timestamp = DateTimeOffset.FromUnixTimeSeconds((long) currentAlerts.Value.Item2);

            var alerts = await _client.GetServiceAlerts(timestamp);
            _repository.SetServiceAlerts(alerts);
        }
    }
}