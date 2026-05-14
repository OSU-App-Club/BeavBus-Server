using CorvallisBus.Core.DataAccess;
using CorvallisBus.Core.WebClients;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using System;
using System.Threading;

namespace CorvallisBus.Core
{
    /// <summary>
    /// Interval timer for executing TransitClient operations on a schedule
    /// </summary>
    public class Worker : BackgroundService
    {
        /// <summary>
        /// The number of seconds to schedule for the timer.
        /// </summary>
        public const int WORKER_INTERVAL_SECONDS = 15;

        private readonly ILogger<Worker> _logger;
        private readonly ITransitRepository _repository;
        private readonly ITransitClient _client;
        
        /// <summary>
        /// Create a new `Worker`. This will automatically start the schedule process.
        /// </summary>
        /// <param name="logger">An `ILogger' for logging information</param>
        /// <param name="repository">An `ITransitRepository` to store results</param>
        /// <param name="client">An `ITransitClient` to fetch data from</param>
        public Worker(ILogger<Worker> logger, ITransitRepository repository, ITransitClient client)
        {
            _logger = logger;

            _repository = repository;
            _client = client;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Corvallis Bus Worker Service Running");

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
                _logger.LogInformation("Stopping Corvallis Bus Worker Service");
            }
        }

        private async Task PerformWork()
        {
            _logger.LogInformation("Worker executing tasks at: {time}", DateTimeOffset.Now);

            // Service Alerts

            // FIXME: get timestamp from repository. but i don' tknow how I want that API yet.
            var alerts = await _client.GetServiceAlerts(DateTimeOffset.MinValue);
            _repository.SetServiceAlerts(alerts);

            // Vehicle Positions
            var positions = await _client.GetVehiclePositions(DateTimeOffset.MinValue);
            _repository.SetVehiclePositions(positions);
        }
    }
}
