using CorvallisBus.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CorvallisBus.Core.Models.Connexionz;
using CorvallisBus.Core.Models.Gtfs;
using System.Diagnostics;
using System.Net.Http;

namespace CorvallisBus.Core.WebClients
{
    /// <summary>
    /// Merges data obtained from Connexionz and Google Transit
    /// and makes it ready for delivery to clients.
    /// </summary>
    public class TransitClient(HttpClient client) : ITransitClient
    {
        private readonly HttpClient _client = client;
        private GtfsRealtimeClient gtfsRealtimeClient => new GtfsRealtimeClient(_client);

        /// <inheritdoc/>
        public (BusSystemData data, List<string> errors) LoadTransitData()
        {
            var connexionzPlatforms = ConnexionzClient.LoadPlatforms();
            var connexionzRoutes = ConnexionzClient.LoadRoutes();
            var googleData = GtfsClient.LoadData();

            var routes = CreateRoutes(googleData.Routes, connexionzRoutes);
            var stops = CreateStops(connexionzPlatforms, connexionzRoutes);

            var staticData = new BusStaticData(
                Routes: routes.ToDictionary(r => r.Route),
                Stops: stops.ToDictionary(s => s.Id)
            );

            var platformTagsLookup = connexionzPlatforms.ToDictionary(p => p.PlatformNo, p => p.PlatformTag);
            var schedule = CreateSchedule(googleData.Schedules, connexionzRoutes, connexionzPlatforms);

            var transitData = new BusSystemData(
                staticData,
                schedule,
                platformTagsLookup);

            var errors = ValidateTransitData(transitData);

            return (transitData, errors);
        }

        /// <summary>
        /// Validate system data to ensure correctness.
        /// </summary>
        public static List<string> ValidateTransitData(BusSystemData data)
        {
            var errors = new List<string>();

            // Validate schedule within each stop
            foreach (var kvp in data.Schedule)
            {
                var (stopId, stopRouteSchedules) = (kvp.Key, kvp.Value);
                foreach (var routeSchedule in stopRouteSchedules)
                {
                    DaysOfWeek usedDays = 0;
                    foreach (var routeDaySchedule in routeSchedule.DaySchedules)
                    {
                        if (routeDaySchedule.Days == DaysOfWeek.None)
                        {
                            errors.Add($"Route {routeSchedule.RouteNo} at stop {stopId} has a day schedule for DaysOfWeek.None.");
                        }

                        if ((routeDaySchedule.Days & usedDays) != 0)
                        {
                            errors.Add($"Route {routeSchedule.RouteNo} at stop {stopId} has a overlapping day schedule {usedDays & routeDaySchedule.Days}");
                        }

                        usedDays = usedDays | routeDaySchedule.Days;

                        var currentTime = TimeSpan.MinValue;
                        foreach (var nextTime in routeDaySchedule.Times)
                        {
                            if (nextTime <= currentTime)
                            {
                                errors.Add($"Route {routeSchedule.RouteNo} at stop {stopId} has an ordering discrepancy in its schedule. Arrival time {currentTime} is followed by {nextTime}");
                            }
                            currentTime = nextTime;
                        }
                    }
                }
            }

            // Validate schedule for each route
            foreach (var route in data.StaticData.Routes.Values)
            {
                var firstStopId = route.Stops[0];
                var firstStopSchedules = data.Schedule[firstStopId].FirstOrDefault(rs => rs.RouteNo == route.Route);
                if (firstStopSchedules == null)
                {
                    errors.Add($"Route {route.Route} has no schedule for stop ID {firstStopId}");
                    continue;
                }

                foreach (var firstStopDaySchedule in firstStopSchedules.DaySchedules)
                {
                    for (var arrivalNo = 0; arrivalNo < firstStopDaySchedule.Times.Count; arrivalNo++)
                    {
                        // get the i'th arrival for each stop in the path, ensure monotonically increasing
                        var currentArrivalTime = TimeSpan.MinValue;

                        for (var stopIdx = 0; stopIdx < route.Stops.Count; stopIdx++)
                        {
                            var stopId = route.Stops[stopIdx];
                            if (stopId == 0)
                            {
                                errors.Add($"Route {route.Route} has a missing stop in its path at index {stopIdx}");
                                continue;
                            }
                            else if (stopId < 1000)
                            {
                                errors.Add($"Route {route.Route} is using platform tag {stopId} as an ID because it has no stop ID");
                                continue;
                            }
                            else if (!data.Schedule.ContainsKey(stopId))
                            {
                                errors.Add($"Route {route.Route} is using stop ID {stopId} which has no schedule");
                                continue;
                            }

                            var routeStopDayArrivalTimes = data
                                .Schedule[stopId]
                                .Single(rs => rs.RouteNo == route.Route)
                                .DaySchedules
                                .Single(ds => ds.Days == firstStopDaySchedule.Days)
                                .Times;

                            if (routeStopDayArrivalTimes.Count != firstStopDaySchedule.Times.Count)
                            {
                                errors.Add($"Warning: {route.Route} does not have the same number of arrivals at all stops. Stop {stopId} has {routeStopDayArrivalTimes.Count} arrivals while stop ID {firstStopId} has {firstStopDaySchedule.Times.Count} arrivals.");
                                continue;
                            }

                            var nextArrivalTime = routeStopDayArrivalTimes[arrivalNo];
                            if (nextArrivalTime <= currentArrivalTime)
                            {
                                Debug.Assert(stopIdx > 0);
                                errors.Add($"Route {route.Route} has a schedule discrepancy across stops {route.Stops[stopIdx - 1]} and {route.Stops[stopIdx]}. Arrival time {currentArrivalTime} is followed by {nextArrivalTime}");
                            }

                            currentArrivalTime = nextArrivalTime;
                        }
                    }
                }
            }

            return errors.Distinct().ToList();
        }

        private static bool ShouldAppendDirection(ConnexionzPlatform platform, List<ConnexionzPlatform> platforms)
        {
            if (platform.Name == "Downtown Transit Center" || platform.Name == "Downtown Transit Centre")
                return false;

            bool existsSameNamedStop = platforms.Any(p => p.PlatformNo != platform.PlatformNo && p.CompactName == platform.CompactName);
            return existsSameNamedStop;
        }

        private static List<BusStop> CreateStops(List<ConnexionzPlatform> platforms, List<ConnexionzRoute> routes)
        {
            return platforms
                .Select(p =>
                    BusStop.Create(p,
                        routes.Where(r => r.Path.Any(rp => rp.PlatformId == p.PlatformNo))
                            .Select(r => r.RouteNo)
                            .ToList(),
                        ShouldAppendDirection(p, platforms)))
                .Where(r => r.RouteNames.Any())
                .ToList();
        }

        private static List<BusRoute> CreateRoutes(List<GtfsRoute> googleRoutes, List<ConnexionzRoute> connexionzRoutes)
        {
            var googleRoutesDict = googleRoutes.ToDictionary(gr => gr.Id);
            var routes = connexionzRoutes.Where(r => r.IsActive && googleRoutesDict.ContainsKey(r.RouteNo));
            return routes.Select(r => BusRoute.Create(r, googleRoutesDict)).ToList();
        }

        /// <inheritdoc/>
        public async Task<ConnexionzPlatformET?> GetEta(int platformTag) => await ConnexionzClient.GetPlatformEta(platformTag);

        /// <summary>
        /// Creates a bus schedule based on Google Transit data.
        /// </summary>
        public ServerBusSchedule CreateSchedule(
            List<GtfsRouteSchedule> googleSchedules,
            List<ConnexionzRoute> connexionzRoutes,
            List<ConnexionzPlatform> connexionzPlatforms)
        {
            var googleSchedulesDict = googleSchedules.ToDictionary(schedule => schedule.RouteNo);
            var routes = connexionzRoutes.Where(r => r.IsActive && googleSchedulesDict.ContainsKey(r.RouteNo));

            var routeSchedules = routes.Select(r => new
            {
                routeNo = r.RouteNo,
                daySchedules = googleSchedulesDict[r.RouteNo].Days.Select(
                    d => new
                    {
                        days = d.Days,
                        stopSchedules = d.StopSchedules.Zip(r.Path, (ss, stop) => (stop.PlatformId, ss.Times))
                    })
            });

            // Now turn it on its head so it's easy to query from a stop-oriented way.
            var result = connexionzPlatforms.ToDictionary(p => p.PlatformNo,
                p => routeSchedules.Select(r => new BusStopRouteSchedule(
                    RouteNo: r.routeNo,
                    DaySchedules: r.daySchedules.Select(ds => new BusStopRouteDaySchedule(
                        Days: ds.days,
                        Times: ds.stopSchedules.FirstOrDefault(ss => ss.PlatformId == p.PlatformNo || ss.PlatformId == p.PlatformTag).Times
                    ))
                    .Where(ds => ds.Times != null)
                    .ToList()
                ))
                .Where(r => r.DaySchedules.Any())
                .ToList()
                .AsEnumerable()
            );

            return result;
        }

        /// <inheritdoc/>
        public async Task<(List<ServiceAlert>, ulong)?> GetServiceAlerts(DateTimeOffset? lastSavedTimestamp) => await gtfsRealtimeClient.GetServiceAlerts(lastSavedTimestamp);

        /// <inheritdoc/>
        public async Task<List<BusPosition>?> GetVehiclePositions(DateTimeOffset? lastSavedTimestamp) => await gtfsRealtimeClient.GetVehiclePositions(lastSavedTimestamp);
    }
}
