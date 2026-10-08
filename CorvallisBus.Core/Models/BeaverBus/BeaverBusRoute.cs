using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace CorvallisBus.Core.Models.BeaverBus
{
	/// <summary>
	/// Represents a Bus Route for the Oregon State Beaver Bus
	/// Note that this object may have many different representations,
	/// as the upstream server creates a new Route for each frequency.
	/// </summary>
	/// <param name="Route">
	/// Route Name (e.g. Central Route, LBCC, etc).
	/// </param>
	/// <param name="Active">
	/// Whether this frequency is currently active
	/// </param>
	/// <param name="Color">
	/// Beaver Bus-defined color for this route.
	/// </param>
	/// <param name="Polyline">
	/// Google Maps-compatible polyline for this route
	/// </param>
	/// <param name="Stops">
	/// List of stops on this frequency, in the order in which they arrive.
	/// </param>
	/// <param name="RouteInstance">
	/// Instance ID for this frequency. This is a unique value representing this frequency,
	/// and is used internally to track real-time vehicle status. It should not be publicly shared
	/// outside the server.
	/// </param>
	internal record BeaverBusRoute(
		[property: JsonProperty("routeName")]
		string Route,

		[property: JsonProperty("hasActiveInstance")]
		bool Active,
		
		[property: JsonProperty("routeColor")]
		string Color,
		
		[property: JsonProperty("encodedPolyline")]
		string Polyline,
		
		[property: JsonProperty("stops")]
		List<BeaverBusStop> Stops,
		
		[property: JsonProperty("routeInstanceId")]
		long RouteInstance)
	{
		/// <summary>
		/// A unique, stable identifer that can be used to identify a route across frequencies.
		/// This value should be shared in public API.
		/// </summary>
		internal string RouteId()
		{
			// MD5 hash the Route Name
			using (MD5 hasher = MD5.Create())
			{
				byte[] data = hasher.ComputeHash(Encoding.UTF8.GetBytes(Route));
				var builder = new StringBuilder();
				for (int i = 0; i < data.Length; i++)
					builder.Append(data[i].ToString("x2"));
				return builder.ToString();
			}
		}
	}
}
