using Newtonsoft.Json;

namespace CorvallisBus.Core.Models.BeaverBus
{
	/// <summary>
	/// Represents an API Response from the Beaver Bus tracker.
	/// </summary>
	/// <param name="ResponseSucceeded">
	/// Whether the response was successful
	/// </param>
	/// <param name="Body">
	/// The main body of the response.
	/// </param>
	/// <param name="Timestamp">
	/// Timestamp of the response.
	/// </param>
	internal record APIResponse<T>(
		[property: JsonProperty("success")]
		bool ResponseSucceeded,

		[property: JsonProperty("data")]
		T Body,

		[property: JsonProperty("timestamp")]
		double Timestamp)
	{
	}
}
