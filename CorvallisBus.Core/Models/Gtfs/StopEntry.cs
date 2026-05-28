using CsvHelper.Configuration.Attributes;

namespace CorvallisBus.Core.Models.Gtfs
{
    /// <summary>
    /// Representation of a CTS Stop from GTFS data.
    /// </summary>
    public class GtfsStop
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        /// <summary>
        /// The public stop ID
        /// </summary>
        [Name("stop_code")]
        public string Id { get; set; }

        /// <summary>
        /// The internal name used throughout GTFS and GTFS Realtime
        /// </summary>
        [Name("stop_id")]
        public string InternalId { get; set; }

        /// <summary>
        /// The name of the stop (i.e. what streets the stop is at)
        /// </summary>
        [Name("stop_name")]
        public string Name { get; set; }

        /// <summary>
        /// The latitude of the stop
        /// </summary>
        [Name("stop_lat")]
        public float Latitude { get; set; }

        /// <summary>
        /// The longitude of the stop
        /// </summary>
        [Name("stop_lon")]
        public float Longitude { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    }
}