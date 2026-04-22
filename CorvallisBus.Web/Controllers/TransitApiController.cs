using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using CorvallisBus.Core.DataAccess;
using CorvallisBus.Core.WebClients;
using CorvallisBus.Core.Models;
using Microsoft.AspNetCore.Hosting;
using System.Runtime.InteropServices;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.IO;

namespace CorvallisBus.Controllers
{
    [ApiController]
    [Route("api")]
    public class TransitApiController : Controller
    {
        private static readonly string _destinationTimeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "Pacific Standard Time"
                : "America/Los_Angeles";

        private readonly string _webRootPath;
        private readonly ITransitRepository _repository;
        private readonly ITransitClient _client;
        private readonly TransitTimer _timer;
        private readonly Func<DateTimeOffset> _getCurrentTime;

        public TransitApiController(IWebHostEnvironment env)
        {
            _webRootPath = env.WebRootPath;
            _repository = new MemoryTransitRepository(env.WebRootPath);
            _client = new TransitClient();
            _timer = new TransitTimer(_repository, _client);
            _getCurrentTime = () => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTimeOffset.Now, _destinationTimeZoneId);
        }

        /// <summary>
        /// Redirects the user to the Swagger API descriptions
        /// </summary>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public ActionResult Index()
        {
            return Redirect("/swagger/");
        }

        /// <summary>
        /// Gets static CTS system data (routes and stops).
        /// </summary>
        /// <remarks>
        /// Gets static CTS system data (routes and stops). This contains all the useful metadata about routes and stops except for the schedule itself.
        /// 
        /// This data should be considered accurate for 24 hours. Caching it on the client side is encouraged.
        /// </remarks>
        /// <response code="200">The CTS route and stop data.</response>
        [HttpGet("static")]
        [Produces("application/json")]
        [ProducesResponseType<BusStaticData>(200)]
        [Tags(["CTS"])]
        public ActionResult GetStaticData()
        {
            return PhysicalFile(_repository.StaticDataPath, "application/json");
        }

        /// <summary>
        /// Gets the ETA information for any number of stop IDs.
        /// </summary>
        /// <remarks>
        /// Returns a JSON dictionary, where the keys are the supplied stop IDs, and the values are dictionaries.
        /// 
        /// These nested dictionaries are such that the keys are route numbers, and the values are lists of integers corresponding to the ETAs for that route to that stop.
        /// 
        /// For example, `"6": [1, 21]` means that Route 6 is arriving at the given stop in 1 minute, and again in 21 minutes. ETAs are limited to 30 minutes in the future by the city.
        /// </remarks>
        /// <param name="stopIds" type="array">Stop IDs to get ETAs for</param>
        /// <response code="200">The stop ETAs.</response>
        /// <response code="400">An error occured validating the Stop IDs</response>
        [HttpGet("eta")]
        [Produces("application/json")]
        [ProducesResponseType<Dictionary<int, Dictionary<string, List<int>>>>(200)] // FIXME: this should be a better type
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [Tags(["CTS"])]
        public async Task<ActionResult> GetETAs([FromQuery, BindRequired]List<int> stopIds)
        {
            if (stopIds == null || stopIds.Count == 0)
            {
                return StatusCode(400);
            }

            try
            {
                var etas = await TransitManager.GetEtas(_repository, _client, stopIds);
                var etasJson = JsonConvert.SerializeObject(etas);
                return Content(etasJson, "application/json");
            }
            catch
            {
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Gets the schedule that CTS routes adhere to for a set of stops.
        /// </summary>
        /// <remarks>
        /// Returns an interleaved list of arrival times for each route for each stop ID provided. Most of the stops in the Corvallis Transit System don't have a schedule. This app fabricates schedules for them by interpolating between those stops that have a schedule. The time between two officially scheduled stops is divided by the number of unscheduled stops between them. This turns out to be a reasonably accurate method.
        ///
        /// Since buses can run behind by 15 minutes or more, or have runs cancelled outright, some interpretation is necessary to communicate the schedule and the estimates in the most informative way possible for users.
        /// 
        /// For instance, in the case of a bus running late, scheduled arrival times can be shown only at least 20 minutes in advance. If they instead were shown only at least 30 minutes in advance, there would be gaps in time where a bus's likely arrival wouldn't be apparent to the user. In other words, the API allows the schedule a 10-minute grace period to "pass" as an estimate, but when the city starts putting out an estimate for that same bus's arrival, the scheduled time gets replaced by the estimated time.
        /// 
        /// Returns a JSON dictionary where the keys are Stop IDs and the values are dictionaries of `{ routeNo: schedule }`. The schedule is a list of pairs of a boolean "is an estimate" and integer "minutes from now." Integers are used because ETAs are interleaved with scheduled arrival times. This avoids a problem where an ETA appears to go up by a minute at the same time the minute on the system clock increments. It introduces a problem where the scheduled times vary by a minute if the server has a different minute value at the time it creates the payload than the client has at the time it consumes the payload.
        /// 
        /// For the time being, it's recommended to use the endpoints which interpret these times and produce user-friendly descriptions for you, such as `/arrivals-summary`.
        /// </remarks>
        /// <param name="stopIds" type="array">Stop IDs to get schedules for</param>
        /// <response code="200">A nested dictionary which groups the arrival times first by stop, then by route name.</response>
        /// <response code="400">An error occured validating the Stop IDs</response>
        [HttpGet("schedule")]
        [Produces("application/json")]
        [ProducesResponseType<Dictionary<int, Dictionary<string, List<BusArrivalTime>>>>(200)] // FIXME: better type
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [Tags(["CTS"])]
        public async Task<ActionResult> GetSchedule([FromQuery, BindRequired]List<int> stopIds)
        {
            if (stopIds == null || stopIds.Count == 0)
            {
                return StatusCode(400);
            }

            try
            {
                var todaySchedule = await TransitManager.GetSchedule(_repository, _client, _getCurrentTime(), stopIds);
                var todayScheduleJson = JsonConvert.SerializeObject(todaySchedule);
                return Content(todayScheduleJson, "application/json");
            }
            catch
            {
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Gets an arrivals summary for the given stop IDs.
        /// </summary>
        /// <remarks>
        /// Gets an arrivals summary for the given stop IDs. The list of route arrivals for a stop will be ordered by the soonest arrival time.
        /// 
        /// The server tries to determine if each route arrives at a given stop "pretty much" hourly or half-hourly. Most routes arrive hourly, with a 10-minute break in the middle of the day. Thus if all the scheduled times left in the day are between 50-70 minutes from each other, it's considered to be an hourly schedule. Similarly with all being 20-40 minutes apart to be considered half-hourly.
        /// </remarks>
        /// <param name="stopIds" type="array">Stop IDs to get arrivals summary for</param>
        /// <response code="200">A dictionary with the stop IDs as the key and the summaries array as the value.</response>
        /// <response code="400">An error occured validating the Stop IDs</response>
        [HttpGet("arrivals-summary")]
        [Produces("application/json")]
        [ProducesResponseType<Dictionary<int, List<RouteArrivalsSummary>>>(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [Tags(["CTS"])]
        public async Task<ActionResult> GetArrivalsSummary([FromQuery, BindRequired]List<int> stopIds)
        {
            if (stopIds == null || stopIds.Count == 0)
            {
                return StatusCode(400);
            }

            try
            {
                var arrivalsSummary = await TransitManager.GetArrivalsSummary(_repository, _client, _getCurrentTime(), stopIds);
                var arrivalsSummaryJson = JsonConvert.SerializeObject(arrivalsSummary);
                return Content(arrivalsSummaryJson, "application/json");
            }
            catch
            {
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Redirects to the official CTS service alerts page.
        /// </summary>
        /// <response code="302">Redirects to the service alerts page.</response>
        [HttpGet("service-alerts")]
        [Produces("application/json")]
        [ProducesResponseType<List<ServiceAlert>>(200)]
        [ProducesResponseType(500)]
        [Tags(["CTS"])]
        public async Task<ActionResult> GetServiceAlerts()
        {
            try
            {
                var alerts = await TransitManager.GetServiceAlerts(_repository, _client, _getCurrentTime());
                var alertsJson = JsonConvert.SerializeObject(alerts);
                return Content(alertsJson, "application/json");
            }
            catch
            {
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Redirects to the official CTS service alerts page.
        /// </summary>
        /// <response code="302">Redirects to the service alerts page.</response>
        [HttpGet("positions")]
        [Produces("application/json")]
        [ProducesResponseType<List<BusPosition>>(200)]
        [ProducesResponseType(500)]
        [Tags(["CTS"])]
        public async Task<ActionResult> GetPositions()
        {
            try
            {
                var positions = await TransitManager.GetBusPositions(_repository, _client, _getCurrentTime());
                var positionsJSON = JsonConvert.SerializeObject(positions);
                return Content(positionsJSON, "application/json");
            }
            catch
            {
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Performs a first-time setup and import of static data.
        /// </summary>
        [HttpPost("job/init")]
        [ApiExplorerSettings(IgnoreApi = true)] // Private API
        public ActionResult Init()
        {
            var expectedAuth = Environment.GetEnvironmentVariable("CorvallisBusAuthorization");
            if (!string.IsNullOrEmpty(expectedAuth))
            {
                var authValue = Request.Headers["Authorization"];
                if (expectedAuth != authValue)
                {
                    return Unauthorized();
                }
            }

            try
            {
                var errors = DataLoadJob();
                if (errors.Count != 0)
                {
                    var message = GetValidationErrorMessage(errors);
                    SendNotification("corvallisb.us init job had validation errors", message).Wait();
                    return Ok(message);
                }

                return Ok("Init job successful.");
            }
            catch (Exception ex)
            {
                SendExceptionNotification(ex).Wait();
                throw;
            }
        }

        private Task SendExceptionNotification(Exception ex)
        {
            var lastWriteTime = System.IO.File.GetLastWriteTime(_repository.StaticDataPath);
            var htmlContent =
$@"<h2>Init job failed: {ex.Message}</h2>
<pre>{ex.StackTrace}</pre>

<p>Init files last updated on {lastWriteTime}</p>";
            return SendNotification(subject: "corvallisb.us init task threw an exception", htmlContent);
        }

        private string GetValidationErrorMessage(List<string> errors)
        {
            var lastWriteTime = System.IO.File.GetLastWriteTime(_repository.StaticDataPath);
            return $@"<h2>Init job had {errors.Count} validation errors</h2>
<pre>{string.Join('\n', errors)}</pre>

<p>Init files last updated on {lastWriteTime}</p>";
        }


        private async Task SendNotification(string subject, string htmlContent)
        {
            var apiKey = Environment.GetEnvironmentVariable("CorvallisBusSendGridKey");
            if (string.IsNullOrEmpty(apiKey))
            {
                return;
            }

            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("azure_78fe1edda7f051b6116e50e4617e08f1@azure.com", "corvallisb.us Notification");
            var to = new EmailAddress("rikkigibson@gmail.com", "Rikki Gibson");

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent: subject, htmlContent);
            _ = await client.SendEmailAsync(msg);
        }

        private List<string> DataLoadJob()
        {
            var (busSystemData, errors) = _client.LoadTransitData();

            _repository.SetStaticData(busSystemData.StaticData);
            _repository.SetPlatformTags(busSystemData.PlatformIdToPlatformTag);
            _repository.SetSchedule(busSystemData.Schedule);

            return errors;
        }
    }
}
