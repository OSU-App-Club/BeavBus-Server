using CorvallisBus.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CorvallisBus.Core
{
	public interface ITransitManager
	{
		/// <summary>
		/// Fetch all available routes
		/// </summary>
		/// <returns>A list of currently active routes</returns>
		Task<List<BusRoute>> GetRoutes();
	}
}
