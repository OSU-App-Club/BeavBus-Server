using CorvallisBus.Core.DataAccess;
using CorvallisBus.Core.WebClients;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace CorvallisBus
{
    /// <summary>
    /// Interval timer for executing TransitClient operations scheduled
    /// </summary>
    public class TransitTimer
    {
        /// <summary>
        /// The number of seconds to schedule for the timer.
        /// </summary>
        public const int TIMER_INTERVAl_SECONDS = 30;

        private readonly ITransitRepository _repository;
        private readonly ITransitClient _client;
        private readonly Timer _timer;
        
        /// <summary>
        /// Create a new `TransitTimer`. This will automatically start the timing process.
        /// </summary>
        /// <param name="repository">An `ITransitRepository` to store results</param>
        /// <param name="client">An `ITransitClient` to fetch data from</param>
        public TransitTimer(ITransitRepository repository, ITransitClient client)
        {
            _repository = repository;
            _client = client;
            _timer = new Timer(OnTimerInterval, this, 0, 30 * 60);
        }

        private async void OnTimerInterval(object? state)
        {
            // Service Alerts
            var alerts = await _client.GetServiceAlerts();
            _repository.SetServiceAlerts(alerts);

            // Vehicle Positions
            var positions = await _client.GetVehiclePositions();
            _repository.SetVehiclePositions(positions);
        }

        /// <summary>
        /// Quit and Dispose of the Timer
        /// </summary>
        public void Quit()
        {
            _timer.Dispose();
        }
    }
}