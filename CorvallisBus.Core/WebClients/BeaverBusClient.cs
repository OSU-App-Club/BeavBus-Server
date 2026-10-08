using CorvallisBus.Core.Models.BeaverBus;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CorvallisBus.Core.WebClients
{
	/// <summary>
	/// Web Client for fetching route, stop, and vehicle alert data from the Oregon State Beaver Bus.
	/// </summary>
	internal class BeaverBusClient(HttpClient _client)
	{
		private const string BASE_URL = "https://portal.flexigo.com/commute/public/osushuttles/";

		private readonly HttpClient client = _client;

		private DateTime CurrentDate()
		{
			return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
				   TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"));
		}

		private async Task<T?> FetchAsync<T>(string url) where T : class
		{
			// Fetches a request with a date of yyyy MM dd. For example, 07 Oct 2026 would be 20261007
			Stream? stream = await client.GetStreamAsync(BASE_URL + url + "?date=" + CurrentDate().ToString("yyyyMMdd"));

			using (StreamReader reader = new StreamReader(stream)) {
				JsonSerializer serializer = new JsonSerializer();
				BeaverBusApiResponse<T>? data = (BeaverBusApiResponse<T>?)serializer.Deserialize(reader, typeof(BeaverBusApiResponse<T>));

				if (data is null || !data.ResponseSucceeded) return null;

				return data.Body ?? throw new ArgumentException("FetchAsync cannot deserialize", nameof(T));
			}
		}

		/// <summary>
		/// Get the currently available routes from upstream.
		/// </summary>
		public async Task<List<BeaverBusRoute>?> GetRoutes()
		{
			List<BeaverBusRoute>? routes = await FetchAsync<List<BeaverBusRoute>>("routes");

			if (routes is null) return null;
			return routes;
		}
	}
}
