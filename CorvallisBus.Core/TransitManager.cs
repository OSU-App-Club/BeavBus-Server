using CorvallisBus.Core.DataAccess;
using CorvallisBus.Core.Models;
using CorvallisBus.Core.Models.Gtfs;
using CorvallisBus.Core.Models.Connexionz;
using CorvallisBus.Core.WebClients;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CorvallisBus
{
    // Maps a stop ID to a dictionary that maps a route number to a list of arrival times.
    // Intended for client consumption.
    using ClientBusSchedule = Dictionary<int, Dictionary<string, List<BusArrivalTime>>>;

    // Maps a 5-digit stop ID to a dictionary that maps a route number to an arrival estimate in minutes.
    // Exists to provide some compile-time semantics to differ between schedules and estimates.
    using BusArrivalEstimates = Dictionary<int, Dictionary<string, List<int>>>;

    public static class TransitManager
    {
        /// <summary>
        /// The greatest number of minutes from now that an estimate can have.
        /// </summary>
        public const int ESTIMATES_MAX_ADVANCE_MINUTES = 30;

        /// <summary>
        /// The range of minutes in which an estimate time can replace a scheduled time.
        /// </summary>
        public const int ESTIMATE_CORRELATION_TOLERANCE_MINUTES = 10;

        /// <summary>
        /// The smallest number of minutes from now that a scheduled time can be rendered.
        /// </summary>
        public const int SCHEDULE_CUTOFF_MINUTES = 20;

        /// <summary>
        /// Returns the bus schedule for the given stop IDs, incorporating the ETA from Connexionz.
        /// </summary>
        public static async Task<ClientBusSchedule> GetSchedule(ITransitRepository repository, ITransitClient client, DateTimeOffset currentTime, IEnumerable<int> stopIds)
        {
            var schedulesTask = repository.GetScheduleAsync();
            var estimatesTask = GetEtas(repository, client, stopIds);

            var schedule = await schedulesTask;
            var estimates = await estimatesTask;

            var todaySchedule = stopIds.Where(schedule.ContainsKey)
                                       .ToDictionary(platformNo => platformNo, makePlatformSchedule);

            return todaySchedule;

            Dictionary<string, List<BusArrivalTime>> makePlatformSchedule(int platformNo) =>
                schedule[platformNo].ToDictionary(routeSchedule => routeSchedule.RouteNo,
                    routeSchedule => InterleaveRouteScheduleAndEstimates(
                        routeSchedule,
                        estimates.ContainsKey(platformNo)
                            ? estimates[platformNo]
                            : new Dictionary<string, List<int>>(),
                        currentTime));
        }

        private static BusStopRouteDaySchedule? GetBestGuessDaySchedule(BusStopRouteSchedule routeSchedule, DateTimeOffset currentTime)
        {
            BusStopRouteDaySchedule? daySchedule = null;
            var potentialDaySchedules = routeSchedule.DaySchedules.Where(ds => DaysOfWeekUtils.TodayMayFallInsideDaySchedule(ds, currentTime));

            int numPotentialSchedules = potentialDaySchedules.Count();
            if (numPotentialSchedules == 1)
                daySchedule = potentialDaySchedules.First();


            // ASSUMPTION: if there are multiple matches, it is because one is a spillover from the previous day, and one is the current day
            // In this case, we wish to continue showing the spillover until it is no longer valid
            if (numPotentialSchedules > 1)
            {
                daySchedule = potentialDaySchedules.First(
                    ds => (ds.Days & DaysOfWeekUtils.ToDaysOfWeek(currentTime.DayOfWeek)) != DaysOfWeekUtils.ToDaysOfWeek(currentTime.DayOfWeek));
            }

            if (daySchedule != null && DaysOfWeekUtils.TimeInSpilloverWindow(daySchedule, currentTime))
                daySchedule = new BusStopRouteDaySchedule(daySchedule.Days, daySchedule.Times.Select(t => t - TimeSpan.FromDays(1)).ToList());

            return daySchedule;
        }

        private static List<BusArrivalTime> InterleaveRouteScheduleAndEstimates(BusStopRouteSchedule routeSchedule,
            Dictionary<string, List<int>> stopEstimates, DateTimeOffset currentTime)
        {
            var arrivalTimes = Enumerable.Empty<BusArrivalTime>();

            var daySchedule = GetBestGuessDaySchedule(routeSchedule, currentTime);

            if (daySchedule != null)
            {
                var relativeSchedule = MakeRelativeScheduleWithinCutoff(daySchedule, currentTime);
                arrivalTimes = relativeSchedule.Select(minutes => new BusArrivalTime(minutes, isEstimate: false));
            }

            if (stopEstimates.ContainsKey(routeSchedule.RouteNo))
            {
                var routeEstimates = stopEstimates[routeSchedule.RouteNo];
                arrivalTimes = arrivalTimes.Where(arrivalTime =>
                        !routeEstimates.Any(estimate =>
                            Math.Abs(arrivalTime.MinutesFromNow - estimate) <= ESTIMATE_CORRELATION_TOLERANCE_MINUTES));
                arrivalTimes = arrivalTimes.Concat(
                    routeEstimates.Select(estimate => new BusArrivalTime(estimate, isEstimate: true)));
            }

            arrivalTimes = arrivalTimes.OrderBy(arrivalTime => arrivalTime);

            return arrivalTimes.ToList();
        }

        // don't show arrival times at 9 AM if it's already 11 AM
        private static IEnumerable<int> MakeRelativeScheduleWithinCutoff(BusStopRouteDaySchedule daySchedule, DateTimeOffset currentTime)
        {
            var scheduleCutoff = currentTime.TimeOfDay + TimeSpan.FromMinutes(20);

            var timeOfDay = currentTime.TimeOfDay;
            // Truncate any seconds on timeOfDay so that we don't get off-by-one errors
            // when converting a scheduled time with a seconds component to minutesFromNow and back.
            timeOfDay -= TimeSpan.FromSeconds(timeOfDay.Seconds);

            return daySchedule.Times.Where(ts => ts > scheduleCutoff)
                .Select(ts => (int)(ts - timeOfDay).TotalMinutes);
        }

        /// <summary>
        /// Gets the ETA info for a set of stop IDS.
        /// The outer dictionary takes a route number and gives a dictionary that takes a stop ID to an ETA.
        /// </summary>
        public static async Task<BusArrivalEstimates> GetEtas(ITransitRepository repository, ITransitClient client, IEnumerable<int> stopIds)
        {
            var toPlatformTag = await repository.GetPlatformTagsAsync();

            var tasks = stopIds.Select(getEtaIfTagExists);
            var results = await Task.WhenAll(tasks);

            return results.ToDictionary(eta => eta.stopId,
                eta => eta.platformET?.RouteEstimatedArrivals
                                ?.ToDictionary(routeEta => routeEta.RouteNo,
                                               routeEta => routeEta.EstimatedArrivalTime)
                       ?? new Dictionary<string, List<int>>());

            async Task<(int stopId, ConnexionzPlatformET? platformET)> getEtaIfTagExists(int id)
                => (id, toPlatformTag.TryGetValue(id, out int tag) ? await client.GetEta(tag) : null);
        }

        /// <summary>
        /// Gets a user friendly arrivals summary for the requested stops.
        /// Returns a dictionary which takes a stop ID and returns the list of route arrival summaries (used to populate a table).
        /// </summary>
        public static async Task<Dictionary<int, List<RouteArrivalsSummary>>> GetArrivalsSummary(ITransitRepository repository, ITransitClient client, DateTimeOffset currentTime, IEnumerable<int> stopIds)
        {
            var schedule = await GetSchedule(repository, client, currentTime, stopIds);
            var staticData = await repository.GetStaticDataAsync();

            var matchingStopIds = stopIds.Where(staticData.Stops.ContainsKey);
            var arrivalsSummaries = matchingStopIds.ToDictionary(stopId => stopId,
                stopId => ToRouteArrivalsSummaries(staticData.Stops[stopId].RouteNames, schedule[stopId], currentTime, staticData));
            return arrivalsSummaries;
        }

        private static readonly BusArrivalTime ARRIVAL_TIME_SEED = new BusArrivalTime(int.MaxValue, isEstimate: false);
        private static List<RouteArrivalsSummary> ToRouteArrivalsSummaries(List<string> routeNames,
            Dictionary<string, List<BusArrivalTime>> stopArrivals, DateTimeOffset currentTime, BusStaticData staticData)
        {
            var arrivalsSummaries =
                routeNames.Select(routeName =>
                            new KeyValuePair<string, List<BusArrivalTime>>(routeName,
                                stopArrivals.ContainsKey(routeName) ? stopArrivals[routeName] : new List<BusArrivalTime>()))
                          .OrderBy(kvp => kvp.Value.DefaultIfEmpty(ARRIVAL_TIME_SEED).Min())
                          .Select(kvp => RouteArrivalsSummary.Create(routeName: kvp.Key, routeArrivalTimes: kvp.Value, currentTime: currentTime))
                          .Where(ras => staticData.Routes.ContainsKey(ras.RouteName))
                          .ToList();

            return arrivalsSummaries;
        }

        /// <summary>
        /// Returns the service alerts for any connected transit agencies.
        /// </summary>
        public static async Task<Dictionary<string, ServiceAlert>> GetServiceAlerts(ITransitRepository repository, ITransitClient client, DateTimeOffset currentTime)
        {
            var alerts = await repository.GetServiceAlertsAsync();

            if (alerts is null)
            {
                alerts = await client.GetServiceAlerts();
            }

            var activeAlerts = alerts.ToDictionary(s => s.Id, (GtfsServiceAlert alert) => ServiceAlert.Create(alert));

            return activeAlerts;
        }

        public static async Task<Dictionary<string, BusPosition>> GetBusPositions(ITransitRepository repository, ITransitClient client, DateTimeOffset currentTime)
        {
            var positions = await repository.GetVehiclePositionsAsync();

            if (positions is null)
            {
                positions = await client.GetVehiclePositions();
            }

            var convertedPositions = positions.ToDictionary(s => s.Id, (GtfsVehiclePosition pos) => BusPosition.Create(pos));

            return convertedPositions;
        }
    }
}
