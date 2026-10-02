using CorvallisBus.Core.Models;
using CorvallisBus.Core.Models.BeaverBus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CorvallisBus.Core.WebClients
{
	internal class BeaverBusClient(HttpClient client)
	{
		private const string BASE_URL = "https://portal.flexigo.com/commute/public/osushuttles/";

		private readonly HttpClient _client = client;

		private async Task<APIResponse<T>> GetEntityAsync<T>(string url, DateTime currentDate) where T : class
		{
			Stream? stream = await _client.GetStreamAsync(BASE_URL + url + "?date=" + currentDate.ToString("yyyyMMdd"));

			using (StreamReader reader = new StreamReader(stream)) {
				JsonSerializer serializer = new JsonSerializer();
				APIResponse<T>? entity = (APIResponse<T>?)serializer.Deserialize(reader, typeof(APIResponse<T>));
				return entity ?? throw new ArgumentException("GetEntityAsync cannot deserialize", nameof(T));
			}
		}

		public async Task<List<BusRoute>?> GetRoutes()
		{
			// FIXME: Force Specify America/Los_Angeles (Pacific) timezone
			APIResponse<List<OSURoute>>? routes = await GetEntityAsync<List<OSURoute>>("routes", DateTime.Now);

			if (routes is null) return null;
			return routes.Body
				.GroupBy(r => r.RouteId())
				.Select(r => r.First())
				.Select(r => new BusRoute(r.RouteName, new List<int>(), r.Color, "", r.Polyline))
				.ToList();
		}
	}
}
