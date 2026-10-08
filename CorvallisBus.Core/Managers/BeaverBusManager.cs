using CorvallisBus.Core.Models;
using CorvallisBus.Core.Models.BeaverBus;
using CorvallisBus.Core.WebClients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
namespace CorvallisBus.Core
{
	/// <summary>
	/// Class for managing Beaver Bus data
	/// </summary>
	public class BeaverBusManager : ITransitManager {
		private readonly BeaverBusClient _client = new BeaverBusClient(new HttpClient());
		
		/// <inheritdoc/>
		public async Task<List<BusRoute>> GetRoutes() {
			List<BusRoute>? routes = await _client.GetRoutes();
			return routes ?? new List<BusRoute>();
		}

		/// <inheritdoc/>
		public async Task<List<OSUStop>> GetStops(List<string> routeIds) {
			var tasks = routeIds.Select(getStopsForRoute);
			var results = await Task.WhenAll(tasks);

ILogger<BeaverBusClient> logger = LoggerFactory
        .Create(logging => logging.AddConsole())
        .CreateLogger<BeaverBusClient>();
			List<OSUStop> stops = new List<OSUStop>();
			foreach (List<OSUStop> stop in results)
			{
				logger.LogInformation(stop.First().ToString());
				stops.AddRange(stop);
			}
			return stops;

			async Task<List<OSUStop>> getStopsForRoute(string id)
				=> await _client.GetStops(id);
		}

	}
}
