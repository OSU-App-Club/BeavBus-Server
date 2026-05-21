using CorvallisBus.Core.GtfsRealtimeGenerated;
using ProtoBuf;
using System;
using System.Linq;
using System.IO;
using System.Reflection;
using Xunit;

namespace CorvallisBus.Test
{
    public class Gtfs_ServiceAlert_Deserialization_Tests
    {
        /// <summary>
        /// The filename for the embedded ServiceAlert Protobuf Test File
        /// </summary>
        static public string SERVICE_ALERT_PROTOBUF_FILE = "CorvallisBus.Test.Resources.ServiceAlert.pb";

        private FeedMessage LoadServiceAlert()
        {
            Stream? resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(SERVICE_ALERT_PROTOBUF_FILE) ?? throw new Exception();
            FeedMessage? feed = Serializer.Deserialize<FeedMessage>(resource);
            
            return feed;
        }

        [Fact]
        public void Gtfs_ServiceAlert_Deserialization_Header()
        {
            FeedMessage feed = LoadServiceAlert();

            var header = feed.Header;

            Assert.Equal("2.0", header.GtfsRealtimeVersion);
            Assert.Equal(FeedHeader.Incrementality.FullDataset, header.incrementality);
            Assert.Equal((ulong) 1776316800, header.Timestamp);
        }

        [Theory]
        [InlineData(0, "1")]
        [InlineData(1, "2")]
        [InlineData(2, "3")]
        public void Gtfs_ServiceAlert_Deserialization_Message(int AlertIndex, string AlertId)
        {
            FeedMessage feed = LoadServiceAlert();

            var entities = feed.Entities;

            Assert.Equal(3, entities.Count);

            var message = entities[AlertIndex];

            Assert.Equal(AlertId, message.Id);
            Assert.False(message.IsDeleted);

            Assert.Null(message.Vehicle);
            Assert.Null(message.TripUpdate);
            Assert.NotNull(message.Alert);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void Gtfs_ServiceAlert_Deserialization_AlertDetails(int AlertIndex)
        {
            FeedMessage feed = LoadServiceAlert();

            var alert = feed.Entities[AlertIndex].Alert;

            Assert.Empty(alert.ActivePeriods);
            Assert.Single(alert.InformedEntities);
            Assert.Equal(Alert.Cause.UnknownCause, alert.cause);
            Assert.Equal(Alert.Effect.UnknownEffect, alert.effect);

            Assert.Null(alert.Url);
            Assert.NotNull(alert.HeaderText);
            Assert.NotNull(alert.DescriptionText);
            Assert.Null(alert.TtsHeaderText);
            Assert.Null(alert.TtsDescriptionText);

            Assert.Equal(Alert.SeverityLevel.UnknownSeverity, alert.severity_level);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void Gtfs_ServiceAlert_Deserialization_InformedEntities(int AlertIndex)
        {
            FeedMessage feed = LoadServiceAlert();

            var alert = feed.Entities[AlertIndex].Alert;

            var informed = alert.InformedEntities.First();

            Assert.Equal("1", informed.AgencyId);

            // Validate Defaults
            Assert.Equal("", informed.RouteId);
            Assert.Equal(0, informed.RouteType);
            Assert.Null(informed.Trip);
            Assert.Equal("", informed.StopId);
            Assert.Equal((uint) 0, informed.DirectionId);
        }

        [Fact]
        public void Gtfs_ServiceAlert_Deserialization_TranslatedText()
        {
            FeedMessage feed = LoadServiceAlert();

            var alert = feed.Entities.First().Alert;

            // Header Text
            var header_text = alert.HeaderText;

            Assert.Single(header_text.Translations);

            var en_translated_header = header_text.Translations.First();

            Assert.Equal("4/6-4/7 Routes 3, 8 & PC Detours", en_translated_header.Text);
            Assert.Equal("en", en_translated_header.Language);

            // Description Text
            var description_text = alert.DescriptionText;

            Assert.Single(description_text.Translations);

            var en_translated_description = description_text.Translations.First();

            Assert.Equal("This is a test Alert Message", en_translated_description.Text);
            Assert.Equal("en", en_translated_description.Language);
        }

        [Fact]
        public void Gtfs_ServiceAlert_Deserialization_TranslatedText_DuplicatedHeader()
        {
            FeedMessage feed = LoadServiceAlert();

            var alert = feed.Entities[1].Alert;

            // Header Text
            var header_text = alert.HeaderText;

            Assert.Equal(2, header_text.Translations.Count);

            var en_translated_header = header_text.Translations.First();

            Assert.Equal("Invalid Header", en_translated_header.Text);
            Assert.Equal("en", en_translated_header.Language);

            var second_en_translated_header = header_text.Translations.Last();

            Assert.Equal("Second Invalid Header", second_en_translated_header.Text);
            Assert.Equal("en", second_en_translated_header.Language);

            // Description Text
            var description_text = alert.DescriptionText;

            Assert.Single(description_text.Translations);

            var en_translated_description = description_text.Translations.First();

            Assert.Equal("Valid Desc", en_translated_description.Text);
            Assert.Equal("en", en_translated_description.Language);
        }

        [Fact]
        public void Gtfs_ServiceAlert_Deserialization_TranslatedText_DuplicatedDescription()
        {
            FeedMessage feed = LoadServiceAlert();

            var alert = feed.Entities[2].Alert;

            // Header Text
            var header_text = alert.HeaderText;

            Assert.Single(header_text.Translations);

            var en_translated_header = header_text.Translations.First();

            Assert.Equal("Valid Header", en_translated_header.Text);
            Assert.Equal("en", en_translated_header.Language);

            // Description Text
            var description_text = alert.DescriptionText;

            Assert.Equal(2, description_text.Translations.Count);

            var en_translated_description = description_text.Translations.First();

            Assert.Equal("Invalid Desc", en_translated_description.Text);
            Assert.Equal("en", en_translated_description.Language);

            var second_en_translated_description = description_text.Translations.Last();

            Assert.Equal("Second Invalid Desc", second_en_translated_description.Text);
            Assert.Equal("en", second_en_translated_description.Language);
        }
    }
}