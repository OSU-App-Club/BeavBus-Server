using CorvallisBus.Core.GtfsRealtimeGenerated;
using CorvallisBus.Core.Models.Connexionz;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CorvallisBus.Core.Models.GtfsRealtime
{
    /// <summary>
    /// How the vehicle is approaching a stop
    /// </summary>
    internal enum StopState
    {
        /// <summary>
        /// The vehicle is in transit to a stop (in-between)
        /// </summary>
        InTransit = 0,
        /// <summary>
        /// The vehicle is stopped
        /// </summary>
        Stopped = 1,
    }

    /// <summary>
    /// Details on a particular stop
    /// </summary>
    /// <param name="SequencePosition">What position the stop is for the trip</param>
    /// <param name="StopId">The ID of the stop</param>
    /// <param name="ArrivalTime">What time the vehicle arrived at/is arriving at</param>
    /// <param name="DepartureTime">What time the vehicle departed at/is departing at</param>
    /// <param name="State">The state of the stop</param>
    internal record StopDetails(
        int SequencePosition,
        string StopId,
        long? ArrivalTime,
        long? DepartureTime,
        StopState State
    )
    {
        internal static StopDetails Create(GtfsRealtimeGenerated.TripUpdate.StopTimeUpdate update)
        {
            int pos = (int) update.StopSequence;
            var id = update.StopId;
            long? arrival_time = null;
            if (update.Arrival is not null)
                arrival_time = update.Arrival.Time;
            long? departure_time = null;
            if (update.Departure is not null)
                departure_time = update.Departure.Time;

            StopState state = (arrival_time is not null && departure_time is not null) ? StopState.Stopped : StopState.InTransit;

            return new StopDetails(
                SequencePosition: pos,
                StopId: id,
                ArrivalTime: arrival_time,
                DepartureTime: departure_time,
                State: state
            );
        }
    }

    /// <summary>
    /// A Trip Update
    /// </summary>
    /// <param name="Id">Bus ID</param>
    /// <param name="TripId">Trip ID (map to GTFS)</param>
    /// <param name="Label">Bus Label</param>
    /// <param name="Stops">Stop Details</param>
    internal record TripUpdate(
        string Id,
        string Label,
        string TripId,
        List<StopDetails> Stops
        )
    {
        /// <summary>
        /// Create a new Trip Update
        /// </summary>
        /// <param name="feed">GTFS Feed Data</param>
        /// <returns>a new Trip Update</returns>
        internal static TripUpdate Create(FeedEntity feed)
        {
            var update = feed.TripUpdate;

            var id = update.Vehicle.Id;
            var label = update.Vehicle.Label;

            var trip_id = update.Trip.TripId;

            var stops = update.StopTimeUpdates.Select(t => StopDetails.Create(t)).ToList();

            return new TripUpdate(
                Id: id,
                Label: label,
                TripId: trip_id,
                Stops: stops
            );
        }
    }
}
