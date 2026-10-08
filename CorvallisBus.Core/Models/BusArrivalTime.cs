using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorvallisBus.Core.Models
{
    /// <summary>
    /// Bus Arrival Times, as estimates and minutes away.
    /// </summary>
    public struct BusArrivalTime : IComparable<BusArrivalTime>
    {
        /// <summary>
        /// Create a new Bus Arrival Time struct
        /// </summary>
        public BusArrivalTime(int minutesFromNow, bool isEstimate)
        {
            MinutesFromNow = minutesFromNow;
            IsEstimate = isEstimate;
        }

        /// <summary>
        /// Return the closest bus arrival time. That is, return the arrival time closest to now.
        /// </summary>
        public static BusArrivalTime Min(BusArrivalTime a, BusArrivalTime b)
        {
            return a.MinutesFromNow < b.MinutesFromNow ? a : b;
        }

        /// <summary>
        /// Compare two arrival times. Does not work for estimates.
        /// </summary>
        public int CompareTo(BusArrivalTime other)
        {
            if (IsEstimate && !other.IsEstimate)
            {
                return -1;
            }

            if (!IsEstimate && other.IsEstimate)
            {
                return 1;
            }

            return MinutesFromNow - other.MinutesFromNow;
        }

        /// <summary>
        /// Arrival time in minutes
        /// </summary>
        [JsonProperty("minutesFromNow")]
        public int MinutesFromNow { get; }
        
        /// <summary>
        /// Whether the arrival is an estimate
        /// </summary>
        [JsonProperty("isEstimate")]
        public bool IsEstimate { get; }

        /// <inheritdoc />
        public override string ToString() => $"{{ MinutesFromNow = {MinutesFromNow}, IsEstimate = {IsEstimate} }}";
    }
}
