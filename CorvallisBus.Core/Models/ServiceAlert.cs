using CorvallisBus.Core.GtfsRealtimeGenerated;
using CorvallisBus.Core.Models.Gtfs;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace CorvallisBus.Core.Models
{
    /// <summary>
    /// Represents a CTS Service Alert.
    /// </summary>
    /// <param name="Title">
    /// Localised title of the alert, in the format `{"language code": "title"}`. Usually this contains details like the affected routes.
    /// </param>
    /// <param name="Description">
    /// Localised description of the alert, in the format `{"language code": "title"}`.
    /// </param>
    public record ServiceAlert(
        [property: JsonProperty("title")]
        Dictionary<string, string> Title,

        [property: JsonProperty("description")]
        Dictionary<string, string> Description)
    {
        public static ServiceAlert Create(GtfsServiceAlert alert)
        {
            return new ServiceAlert(alert.Headers, alert.Descriptions);
        }
    }

    /// <summary>
    /// Represents a CTS Service Alert.
    /// </summary>
    /// <param name="Title">
    /// Title of the alert. Usually this contains details like the affected routes.
    /// </param>
    /// <param name="Description">
    /// Description of the alert.
    /// </param>
    public record LocalisedServiceAlert(
        [property: JsonProperty("title")]
        string Title,

        [property: JsonProperty("description")]
        string Description)
    {
        public static LocalisedServiceAlert Create(GtfsServiceAlert alert, string language_code)
        {
            if (!alert.Headers.TryGetValue(language_code, out string? title))
                title = "Missing Localisation";
            if (!alert.Descriptions.TryGetValue(language_code, out string? description))
                description = "Missing Localisation";

            return new LocalisedServiceAlert(title, description);
        }
    }
}
