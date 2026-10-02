using CorvallisBus.Core;
using CorvallisBus.Core.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CorvallisBus.Controllers
{
	[Route("osu")]
	public class APIController : Controller
	{
		private ITransitManager osuManager = new BeaverBusManager();
		
		[HttpGet("routes")]
		[Produces("application/json")]
		[ProducesResponseType<List<BusRoute>>(200)]
		[ProducesResponseType(500)]
		public async Task<ActionResult> GetRoutes()
		{
			var routes = await osuManager.GetRoutes();
			var routesJson = JsonConvert.SerializeObject(routes);
			return Content(routesJson, "application/json");
		}
	}
}
