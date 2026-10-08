using System;

using CorvallisBus.Core.WebClients;

using Xunit;
using Xunit.Abstractions;

namespace CorvallisBus.Test
{
    public class TransitClientTests
    {
        private readonly ITestOutputHelper _output;

        public TransitClientTests(ITestOutputHelper output)
        {
            _output = output;
        }

        public sealed class IntegrationTest : FactAttribute
        {
            public IntegrationTest()
            {
                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TRAVIS")))
                {
                    Skip = "Test should not run in CI";
                }
            }
        }


        [IntegrationTest]
        public void ValidateInitJob()
        {
            var client = new TransitClient(new System.Net.Http.HttpClient());
            var (_, errors) = client.LoadTransitData();
            foreach (var error in errors)
            {
                _output.WriteLine(error);
            }
        }
    }
}