using Newtonsoft.Json;

namespace CorvallisBus.Core.Models.BeaverBus
{
	// FIXME: Internal!
	public record OSUStop(
		[property: JsonProperty("id")]
		long StopId,

		[property: JsonProperty("name")]
		string Name,

		[property: JsonProperty("sequence")]
		int SequenceWithinRoute,

		[property: JsonProperty("latitude")]
		double Latitude,

		[property: JsonProperty("longitude")]
		double Longitude,

		// [property: JsonProperty("plannedArrival")]
		// int? Arrival,

		// [property: JsonProperty("plannedDeparture")]
		// int? Departure,

		// Which route this stop is associated with.
		string RouteId
	)
	{
	}
}
