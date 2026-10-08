using System;
using Newtonsoft.Json;

namespace CorvallisBus.Core.Models.BeaverBus
{
	/// <summary>
	/// Represents a Bus Stop for the Oregon State Beaver Bus
	/// Note that this object may have many different representations,
	/// as the upstream server creates a new Stop for each frequency.
	/// </summary>
	/// <param name="Id">
	/// Unique ID for this stop. This will remain consistent among frequencies.
	/// </param>
	/// <param name="Name">
	/// Stop Name (e.g. Reser Stadium, University Plaza, etc).
	/// </param>
	/// <param name="Latitude">
	/// Latitude of the stop.
	/// </param>
	/// <param name="Longitude">
	/// Longitude of the stop.
	/// </param>
	/// <param name="Arrival">
	/// The planned arrival of the stop. This will change among frequencies. There is no guarantee
	/// that a bus will arrive at this time.
	/// This is a four-digit Integer where the first two digits represent the hour, and the last two digits represent
	/// the Minute. For example, 1007 is equivalent to 10:07. This is a 24-hour time representation.
	/// </param>
	/// <param name="Departure">
	/// The planned departure of the stop. This will change among frequencies. There is no guarantee
	/// that a bus will depart at this time.
	/// This is a four-digit Integer where the first two digits represent the hour, and the last two digits represent
	/// the Minute. For example, 1007 is equivalent to 10:07. This is a 24-hour time representation.
	/// </param>
	internal record BeaverBusStop(
		[property: JsonProperty("id")]
		long Id,

		[property: JsonProperty("name")]
		string Name,
		
		[property: JsonProperty("latitude")]
		double Latitude,
		
		[property: JsonProperty("longitude")]
		double Longitude,
		
		[property: JsonProperty("plannedArrival")]
		int? Arrival,
		
		[property: JsonProperty("plannedDeparture")]
		int? Departure)
	{
	}
}
