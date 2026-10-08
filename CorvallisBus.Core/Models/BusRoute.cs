using System.Collections.Generic;
using System.Linq;

using CorvallisBus.Core.Models.BeaverBus;
using CorvallisBus.Core.Models.Connexionz;
using CorvallisBus.Core.Models.Gtfs;

using Newtonsoft.Json;

namespace CorvallisBus.Core.Models
{
    /// <summary>
    /// Represents a Bus Route for any provider.
    /// </summary>
    /// <param name="Id">
    /// A unique identifier for this route, to use in API requests.
    /// </param>
    /// <param name="Route">
    /// Route Number (e.g. 1, 2, PK1, Central Route, etc).
    /// </param>
    /// <param name="Stops">
    /// List of Stop IDs (see /stops) on this route, in the order the bus reaches them.
    /// </param>
    /// <param name="Color">
    /// Defined color for this route. Should be used for coloring UI elements related to this route.
    /// </param>
    /// <param name="Url">
    /// URL to the schedule page for this route.
    /// </param>
    /// <param name="Polyline">
    /// Google Maps-compatible polyline for this route.
    /// </param>
    public record BusRoute(
        [property: JsonProperty("id")]
        string Id,

        [property: JsonProperty("route")]
        string Route,

        [property: JsonProperty("stops")]
        List<int> Stops,

        [property: JsonProperty("color")]
        string Color,

        [property: JsonProperty("url")]
        string Url,

        [property: JsonProperty("polyline")]
        string Polyline)
    {
        internal static BusRoute Create(ConnexionzRoute connectionzRoute, Dictionary<string, GtfsRoute> googleRoutes)
        {
            var routeNo = connectionzRoute.RouteNo;
            var googleRoute = googleRoutes[routeNo];
            var path = connectionzRoute.Path
                .Select(platform => platform.PlatformId)
                .ToList();

            var routeUrlSuffix = routeNo switch
            {
                "NON" => "night-owl-north",
                "NOSE" => "night-owl-southeast",
                "NOSW" => "night-owl-southwest",
                _ => routeNo
            };
            var url = "https://www.corvallisoregon.gov/cts/page/cts-route-" + routeUrlSuffix;

            return new BusRoute(routeNo, routeNo, path, googleRoute.Color, url, connectionzRoute.Polyline);
        }

        internal static BusRoute Create(BeaverBusRoute beaverBusRoute)
        {
            return new BusRoute(
                beaverBusRoute.RouteId(), // Id
                beaverBusRoute.Route,     // Route
                new List<int>(),          // Stops
                beaverBusRoute.Color,     // Color
                "https://transportation.oregonstate.edu/beaver-bus-schedules",  // URL
                beaverBusRoute.Polyline   // Polyline
            );
        }
    }
}