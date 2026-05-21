using CorvallisBus.Core.GtfsRealtimeGenerated;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace CorvallisBus.Core.Models
{
    /// <summary>
    /// Represents a Service Alert.
    /// </summary>
    /// <param name="Title">
    /// Title of the alert. Usually this contains details like the affected routes.
    /// </param>
    /// <param name="Description">
    /// Description of the alert.
    /// </param>
    public record ServiceAlert(
        [property: JsonProperty("title")]
        string Title,

        [property: JsonProperty("description")]
        string Description)
    {
        /// <summary>
        /// Create a new Service Alert
        /// </summary>
        /// <param name="alert">A GtfsRealtime Feed Entity providing alert data</param>
        /// <param name="language_code">A two-letter BCP-47 code</param>
        /// <returns>A new ServiceAlert</returns>
        /// <exception cref="Exception">Multiple titles or descriptions exist for the same langugae</exception>
        public static ServiceAlert? Create(FeedEntity alert, string language_code)
        {
            var header = alert.Alert.HeaderText
                .Translations
                .Where(t => t.Language == language_code)
                .Select(t => t.Text);
            var description = alert.Alert.DescriptionText
                .Translations
                .Where(t => t.Language == language_code)
                .Select(t => t.Text);
            
            if (header.Count() > 1)
                throw new Exception("Entity '" + alert.Id + "' has more than one title for language '" + language_code + "'");
            if (description.Count() > 1)
                throw new Exception("Entity '" + alert.Id + "' has more than one description for language '" + language_code + "'");

            return new ServiceAlert(
                header.SingleOrDefault("No Title Provided"),
                description.SingleOrDefault("No Description Provided")
            );
        }
    }
}