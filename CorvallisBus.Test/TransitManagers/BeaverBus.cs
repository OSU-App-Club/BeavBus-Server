using CorvallisBus.Core;
using CorvallisBus.Core.DataAccess;
using CorvallisBus.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace CorvallisBus.Test
{
	public class TransitManagerMessageHandler(HttpResponseMessage message) : HttpMessageHandler
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

	public class BeaverBus_TransitManager_Tests
	{
		static private string ROUTES_FILE = "CorvallisBus.Test.Resources.BeaverBusRoutes.json";

    	private Stream LoadRoutes()
    	{
    	    return Assembly.GetExecutingAssembly().GetManifestResourceStream(ROUTES_FILE) ?? throw new Exception();
    	}

		[Fact]
		public async Task Routes_Grouped()
		{
			HttpResponseMessage? res = new HttpResponseMessage
			{
				Content = new StreamContent(LoadRoutes())
			};
			TransitManagerMessageHandler? mockHttp = new TransitManagerMessageHandler(res);

            var mockRepo = new Mock<ITransitRepository>();
			BeaverBusManager underTest = new BeaverBusManager(new HttpClient(mockHttp));

			List<BusRoute>? routes = await underTest.GetRoutes(mockRepo.Object);

			Assert.NotNull(routes);
			Assert.Equal(2, routes.Count);
		}

		[Theory]
		[InlineData(0, "Central Route", "#dbaa00", "https://transportation.oregonstate.edu")]
		[InlineData(1, "LBCC Corvallis Route", "#306b9d", "https://transportation.oregonstate.edu")]
		public async Task Routes_Parsed(int Index, string Name, string Color, string UrlPrefix)
		{
			HttpResponseMessage? res = new HttpResponseMessage
			{
				Content = new StreamContent(LoadRoutes())
			};
			TransitManagerMessageHandler? mockHttp = new TransitManagerMessageHandler(res);

            var mockRepo = new Mock<ITransitRepository>();
			BeaverBusManager underTest = new BeaverBusManager(new HttpClient(mockHttp));

			List<BusRoute>? routes = await underTest.GetRoutes(mockRepo.Object);
			Assert.NotNull(routes);
			BusRoute route = routes[Index];

			Assert.Equal(Name, route.Route);
			Assert.Equal(Color, route.Color);
			Assert.Contains(UrlPrefix, route.Url);
		}
	};
}
