using Newtonsoft.Json;

namespace CorvallisBus.Core.Models
{
	/// <summary>
	/// The available transit systems that this API can be used with.
	/// </summary>
	public enum TransitSystem
	{
		/// <summary>
		/// Corvallis Transit System
		/// </summary>
		CTS,

		/// <summary>
		/// Oregon State University's Beaver Bus
		/// </summary>
		OSU
	}
}
