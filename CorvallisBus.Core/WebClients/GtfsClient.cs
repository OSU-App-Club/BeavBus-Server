using CorvallisBus.Core.Models.Gtfs;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;

namespace CorvallisBus.Core.WebClients
{
    internal class GtfsData
    {
        public List<GtfsRoute> Routes { get; }

        public List<GtfsStop> Stops { get; }
        public List<TripsEntry> Trips { get; }
        public List<GtfsRouteSchedule> Schedules { get; }

        public GtfsData(
            List<GtfsRoute> routes,
            List<GtfsStop> stops,
            List<TripsEntry> trips,
            List<GtfsRouteSchedule> schedules)
        {
            Routes = routes;
            Stops = stops;
            Trips = trips;
            Schedules = schedules;
        }
    }

    /// <summary>
    /// Contains the task for importing route and schedule data from GTFS. This task is run once every night.
    /// </summary>
    internal static class GtfsClient
    {
        private static string GTFS_URL = "http://www.corvallistransit.com/rtt/public/utility/gtfs.aspx";
        /// <summary>
        /// Downloads and interprets the ZIP file CTS uploads for GTFS compliance.
        /// This is primarily to get route colors and route schedules, and mapping of buses to routes/stops
        /// </summary>
        public static GtfsData LoadData()
        {
            var stream = new HttpClient().GetStreamAsync(GTFS_URL).Result;
            using var archive = new ZipArchive(stream);

            var routesEntry = archive.GetEntry("routes.txt")
                ?? throw new FileNotFoundException("The GTFS archive did not contain routes.txt.");
            
            var stopsEntry = archive.GetEntry("stops.txt")
                ?? throw new FileNotFoundException("The GTFS archive did not contain stops.txt");

            var scheduleEntry = archive.GetEntry("stop_times.txt")
                ?? throw new FileNotFoundException("The GTFS archive did not contain stop_times.txt.");

            var tripsEntry = archive.GetEntry("trips.txt")
                ?? throw new FileNotFoundException("The GTFS archive did not contain trips.txt.");

            var calendarEntry = archive.GetEntry("calendar.txt")
                ?? throw new FileNotFoundException("The GTFS archive did not contain calendar.txt.");

            var routes = ParseRouteCSV(routesEntry);
            var stops = ParseStopCSV(stopsEntry);
            var trips = ParseTripsCSV(tripsEntry);
            var schedules = ParseScheduleCSV(scheduleEntry, tripsEntry, calendarEntry);

            return new GtfsData(
                routes: routes,
                stops: stops,
                trips: trips,
                schedules: schedules
            );
        }

        /// <summary>
        /// Reads a ZipArchive entry as the routes CSV and extracts the route colors and URLs.
        /// </summary>
        private static List<GtfsRoute> ParseRouteCSV(ZipArchiveEntry entry)
        {
            using var csv = new CsvReader(new StreamReader(entry.Open()));
            var records = csv.GetRecords<GtfsRoute>();
            var routes = records.ToList();
            return routes;
        }

        private static List<GtfsStop> ParseStopCSV(ZipArchiveEntry entry)
        {
            using var csv = new CsvReader(new StreamReader(entry.Open()));
            var records = csv.GetRecords<GtfsStop>();
            var stops = records.ToList();
            return stops;
        }

        private static List<TripsEntry> ParseTripsCSV(ZipArchiveEntry entry)
        {
            using var tripsCsv = new CsvReader(new StreamReader(entry.Open()));
            var records = tripsCsv.GetRecords<TripsEntry>();
            var trips = records.ToList();
            return trips;
        }

        private static List<GtfsRouteSchedule> ParseScheduleCSV(ZipArchiveEntry stopTimesTxt, ZipArchiveEntry tripsTxt, ZipArchiveEntry calendarTxt)
        {
            using var stopTimesCsv = new CsvReader(new StreamReader(stopTimesTxt.Open()));
            var stopTimes = stopTimesCsv.GetRecords<StopTimesEntry>();

            using var tripsCsv = new CsvReader(new StreamReader(tripsTxt.Open()));
            var trips = tripsCsv.GetRecords<TripsEntry>();

            using var calendarCsv = new CsvReader(new StreamReader(calendarTxt.Open()));
            var calendars = calendarCsv.GetRecords<CalendarEntry>().ToList();

            var joinedEntries = stopTimes
                .Join(trips, st => st.TripId, t => t.TripId, (stopTime, trip) => (stopTime, trip))
                .Join(calendars, entry => entry.trip.ServiceId, cal => cal.ServiceId, (entry, calendar) => (entry.stopTime, entry.trip, calendar));

            var aggTimesAtStop = joinedEntries
                .GroupBy(t => new { routeNo = t.trip.RouteId, platformTag = t.stopTime.PlatformTag, stopSequence = t.stopTime.StopSequence, days = t.calendar.DaysOfWeek })
                .Select(g => g.OrderBy(t => t.stopTime.ArrivalTime)
                    .Aggregate(new List<TimeSpan>(),
                        (times, t) => { times.Add(t.stopTime.ArrivalTime); return times; },
                        times => new { g.Key.routeNo, g.Key.days, stopSchedule = new GtfsStopSchedule(g.Key.platformTag, times) }));

            var aggStopsForRoute = aggTimesAtStop
                .GroupBy(t => new { t.routeNo, t.days })
                .Select(g => g.Aggregate(new List<GtfsStopSchedule>(),
                    (list, t) => { list.Add(t.stopSchedule); return list; },
                    list => new { g.Key.routeNo, g.Key.days, stopSchedules = list }));

            var aggDaysForRoute = aggStopsForRoute
                .GroupBy(t => t.routeNo)
                .Select(g => g.Aggregate(new List<GtfsDaySchedule>(),
                    (list, t) => { list.Add(new GtfsDaySchedule(t.days, t.stopSchedules)); return list; },
                    list => new GtfsRouteSchedule(g.Key, list)))
                .ToList();

            return aggDaysForRoute;
        }
    }
}