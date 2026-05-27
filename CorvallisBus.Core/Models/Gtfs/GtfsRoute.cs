using CsvHelper.Configuration.Attributes;

namespace CorvallisBus.Core.Models.Gtfs
{
    /// <summary>
    /// Representation of a CTS Route from GTFS data.
    /// </summary>
    public class GtfsRoute
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        /// <summary>
        /// A route ID from the GTFS CSV
        /// </summary>
        [Name("route_id")]
        public string Id { get; set; }

        /// <summary>
        /// A name from the GTFS CSV
        /// </summary>
        [Name("route_long_name")]
        public string Name { get; set; }

        /// <summary>
        /// The color of the route as a hex string, e.g. "35EFA0".
        /// </summary>
        [Name("route_color")]
        public string Color { get; set; }

        /// <summary>
        /// The URL of the route schedule
        /// </summary>
        [Name("route_url")]
        public string Url { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    }
}