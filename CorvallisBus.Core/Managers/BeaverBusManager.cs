using CorvallisBus.Core.Models;
using CorvallisBus.Core.WebClients;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

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

	}
}
