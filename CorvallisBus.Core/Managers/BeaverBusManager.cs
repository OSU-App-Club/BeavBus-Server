using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using CorvallisBus.Core.DataAccess;
using CorvallisBus.Core.Models;
using CorvallisBus.Core.Models.BeaverBus;
using CorvallisBus.Core.WebClients;

namespace CorvallisBus.Core
{
    /// <summary>
    /// Beaver Bus Transit Manager. See ITransitManager documentation.
    /// </summary>
    /// <param name="http">An HttpClient used for making network requests</param>
    public class BeaverBusManager(HttpClient http) : ITransitManager
    {
        private readonly BeaverBusClient client = new BeaverBusClient(http);

        /// <inheritdoc/>
        public async Task<List<BusRoute>?> GetRoutes(ITransitRepository repository)
        {
            List<BeaverBusRoute>? routes = await client.GetRoutes();

            // FIXME: Save routes to repository, and ensure automatic deletion occurs (i.e. cache)
            if (routes is null) return null;

            return routes
                .GroupBy(r => r.RouteId())
                .Select(r => r.First()) // Select the first route in the grouping. For this API, we don't care about individual timing differences
                .Select(r => BusRoute.Create(r))
                .ToList();
        }

        /// <inheritdoc/>
        public async Task<List<BusStop>?> GetStops(ITransitRepository repository, string routeId)
        {
            List<BeaverBusRoute>? routes = await client.GetRoutes();

            // FIXME: Save stops to repository
            if (routes is null) return null;

            var parsedRoutes = routes
                .Where(r => r.RouteId() == routeId)
                .GroupBy(r => r.RouteId())
                .Select(r => r.First())
                .First();

            return parsedRoutes.Stops
                .Select(s => BusStop.Create(s, parsedRoutes))
                .ToList();
        }
    }
}