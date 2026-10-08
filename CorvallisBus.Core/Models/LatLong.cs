
namespace CorvallisBus.Core.Models
{
    /// <summary>A latitude and longitude representation</summary>
    public struct LatLong
    {
        /// <summary>Create a new LatLong</summary>
        public LatLong(double lat, double lon)
        {
            Lat = lat;
            Lon = lon;
        }

        /// <summary>Latitude</summary>
        public double Lat { get; }

        /// <summary>Longitude</summary>
        public double Lon { get; }
    }
}
