using CorvallisBus.Core.DataAccess;
using CorvallisBus.Core.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace CorvallisBus.Core
{
	/// <summary>
	/// Interface defining a Transit Manager.
	/// Transit Managers interface between the web requests, data manipulation,
	/// and data storage. As such, they are the main API that should be used for
	/// the Core.
	/// </summary>
	public interface ITransitManager
	{
		/// <summary>
		/// Fetch all available routes.
		/// </summary>
		/// <returns>A list of currently active routes</returns>
		/// <param name="repository">A Transit Repository used to store data</param>
		Task<List<BusRoute>?> GetRoutes(ITransitRepository repository);
	}
}
