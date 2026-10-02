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
		string Vehicle
	)
	{
		// FIXME: This needs to be documented
		public string RouteId()
		{
			return RouteName.GetHashCode().ToString();
		}
	}
}
