using System.Collections.Generic;
using System.Linq;

namespace CorvallisBus.Core.Models.Connexionz
{
    /// <summary></summary>
    public record ConnexionzRouteET(
        string RouteNo,
        List<int> EstimatedArrivalTime)
    {
        /// <summary></summary>
        public ConnexionzRouteET(RoutePositionPlatformRoute routePositionPlatformRoute)
            : this(routePositionPlatformRoute.RouteNo,
                  routePositionPlatformRoute.Destination[0].Trip.Select(t => t.ETA).ToList())
        {
        }
    }
}