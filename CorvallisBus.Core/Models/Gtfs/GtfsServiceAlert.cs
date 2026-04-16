using CorvallisBus.Core.GtfsRealtimeGenerated;
using System.Collections.Generic;
using System.Linq;

namespace CorvallisBus.Core.Models.Gtfs
{
    /// <summary>
    /// Represents a CTS Service Alert with raw data from Gtfs.
    /// </summary>
    /// <param name="Headers">
    /// A set of localised headers, in the format "language code": "header value"
    /// </param>
    /// <param name="Descriptions">
    /// A set of localised descriptions, in the format "language code": "description value"
    /// </param>
    public record GtfsServiceAlert(
        string Id,
        Dictionary<string, string> Headers,
        Dictionary<string, string> Descriptions)
    {
        public static GtfsServiceAlert Create(FeedEntity entity)
        {
            var id = entity.Id;
                
            var headers = entity.Alert.HeaderText.Translations.ToDictionary(
                o => o.Language,
                o => o.Text
            );
            var descriptions = entity.Alert.DescriptionText.Translations.ToDictionary(
                o => o.Language,
                o => o.Text
            );

            return new GtfsServiceAlert(id, headers, descriptions);
        }
    }
}
