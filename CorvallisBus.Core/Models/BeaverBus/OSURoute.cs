using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace CorvallisBus.Core.Models.BeaverBus
{
	internal record OSURoute(
		[property: JsonProperty("routeName")]
		string RouteName,

		[property: JsonProperty("hasActiveInstance")]
		string IsActive,

		[property: JsonProperty("routeColor")]
		string Color,

		[property: JsonProperty("encodedPolyline")]
		string Polyline,

		// FIXME: Handle Stops
		[property: JsonProperty("vehicleId")]
		string Vehicle,

		[property: JsonProperty("stops")]
		List<OSUStop> Stops,

		[property: JsonProperty("routeInstanceId")]
		long RouteInstance
	)
	{
		// FIXME: This needs to be documented
		// FIXME: I mean MD5 hashes are stable, they just look ugly as hell
		public string RouteId()
		{
			using (MD5 hasher = MD5.Create())
			{
				byte[] data = hasher.ComputeHash(Encoding.UTF8.GetBytes(RouteName));
        		var sBuilder = new StringBuilder();
        		for (int i = 0; i < data.Length; i++)
        		{
            		sBuilder.Append(data[i].ToString("x2"));
        		}
		        return sBuilder.ToString();
			}
		}
	}
}
