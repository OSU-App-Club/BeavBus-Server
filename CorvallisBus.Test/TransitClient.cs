using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using CorvallisBus.Core.Models;
using CorvallisBus.Core.WebClients;

using Xunit;

namespace CorvallisBus.Test
{
    public class GtfsRealtimeMessageHandler(HttpResponseMessage message) : HttpMessageHandler
    {
        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return message;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return message;
        }
    };

    public class TransitClient_Tests
    {
        /// <summary>
        /// The filename for the embedded ServiceAlert Protobuf Test File
        /// </summary>
        private static readonly string SERVICE_ALERT_PROTOBUF_FILE = "CorvallisBus.Test.Resources.ServiceAlert.pb";

        private Stream LoadServiceAlert()
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(SERVICE_ALERT_PROTOBUF_FILE) ?? throw new Exception();
        }

        [Fact]
        public async Task ServiceAlert_NewData()
        {
            HttpResponseMessage? res = new HttpResponseMessage
            {
                Content = new StreamContent(LoadServiceAlert())
            };
            GtfsRealtimeMessageHandler? mockHttp = new GtfsRealtimeMessageHandler(res);
            TransitClient underTest = new TransitClient(new HttpClient(mockHttp));

            (List<ServiceAlert>, ulong)? fullAlertObject = await underTest.GetServiceAlerts(null);
            Assert.NotNull(fullAlertObject);
            List<ServiceAlert> alerts = fullAlertObject.Value.Item1;

            Assert.NotNull(alerts);
            Assert.Single(alerts);

            ServiceAlert alert = alerts.First();

            Assert.Equal("4/6-4/7 Routes 3, 8 & PC Detours", alert.Title);
            Assert.Equal("This is a test Alert Message", alert.Description);
        }

        [Fact]
        public async Task ServiceAlert_TimestampJustUnder()
        {
            HttpResponseMessage? res = new HttpResponseMessage
            {
                Content = new StreamContent(LoadServiceAlert())
            };
            GtfsRealtimeMessageHandler? mockHttp = new GtfsRealtimeMessageHandler(res);
            TransitClient underTest = new TransitClient(new HttpClient(mockHttp));

            (List<ServiceAlert>, ulong)? alerts = await underTest.GetServiceAlerts(DateTimeOffset.FromUnixTimeSeconds(1776316799));

            Assert.NotNull(alerts);
        }

        [Fact]
        public async Task ServiceAlert_TimestampEqual()
        {
            HttpResponseMessage? res = new HttpResponseMessage
            {
                Content = new StreamContent(LoadServiceAlert())
            };
            GtfsRealtimeMessageHandler? mockHttp = new GtfsRealtimeMessageHandler(res);
            TransitClient underTest = new TransitClient(new HttpClient(mockHttp));

            (List<ServiceAlert>, ulong)? alerts = await underTest.GetServiceAlerts(DateTimeOffset.FromUnixTimeSeconds(1776316800));

            Assert.Null(alerts);
        }

        [Fact]
        public async Task ServiceAlert_TimestampJustGreater()
        {
            HttpResponseMessage? res = new HttpResponseMessage
            {
                Content = new StreamContent(LoadServiceAlert())
            };
            GtfsRealtimeMessageHandler? mockHttp = new GtfsRealtimeMessageHandler(res);
            TransitClient underTest = new TransitClient(new HttpClient(mockHttp));

            (List<ServiceAlert>, ulong)? alerts = await underTest.GetServiceAlerts(DateTimeOffset.FromUnixTimeSeconds(1776316801));

            Assert.Null(alerts);
        }
    }
}