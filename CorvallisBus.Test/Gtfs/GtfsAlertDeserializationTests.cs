
using CorvallisBus.Core.GtfsRealtimeGenerated;
using CorvallisBus.Core.Models;
using ProtoBuf;
using System;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Resources;
using Xunit;

namespace CorvallisBus.Test
{
    public class GtfsDeserializationTests
    {
        [Fact]
        public void AlertDeserializationHeader()
        {
            var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("CorvallisBus.Test.Resources.Alert.pb") ?? throw new Exception();
            var feed = Serializer.Deserialize<FeedMessage>(resource);

            var header = feed.Header;

            // Cast to ulong
            ulong timestamp = 1776316800;

            Assert.Equal("2.0", header.GtfsRealtimeVersion);
            Assert.Equal(FeedHeader.Incrementality.FullDataset, header.incrementality);
            Assert.Equal(timestamp, header.Timestamp);
        }

        [Fact]
        public void AlertDeserializationMessage()
        {
            var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("CorvallisBus.Test.Resources.Alert.pb") ?? throw new Exception();
            var feed = Serializer.Deserialize<FeedMessage>(resource);

            var entities = feed.Entities;

            Assert.Single(entities);

            var message = entities.First();

            Assert.Equal("1", message.Id);
            Assert.False(message.IsDeleted);

            Assert.Null(message.Vehicle);
            Assert.Null(message.TripUpdate);
            Assert.NotNull(message.Alert);
        }

        [Fact]
        public void AlertDeserializationAlertDetails()
        {
            var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("CorvallisBus.Test.Resources.Alert.pb") ?? throw new Exception();
            var feed = Serializer.Deserialize<FeedMessage>(resource);

            var alert = feed.Entities.First().Alert;

            // FIXME: These are the only values that are present in the sample. I do not know if CTS uses the other available properties

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

        [Fact]
        public void AlertDeserializationInformedEntities()
        {
            var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("CorvallisBus.Test.Resources.Alert.pb") ?? throw new Exception();
            var feed = Serializer.Deserialize<FeedMessage>(resource);

            var alert = feed.Entities.First().Alert;

            // FIXME: These are the only values that are present in the sample. I do not know if CTS uses the other available properties
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
        public void AlertDeserializationTranslatedText()
        {
            var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("CorvallisBus.Test.Resources.Alert.pb") ?? throw new Exception();
            var feed = Serializer.Deserialize<FeedMessage>(resource);

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
    }
}