using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CorvallisBus.Core;
using CorvallisBus.Core.DataAccess;
using CorvallisBus.Core.Models;
using CorvallisBus.Core.WebClients;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CorvallisBus.Test
{
    public class Worker_Tests
    {
        private const int MILLISECOND_TO_SECOND_FACTOR = 1000;

        [Fact]
        public async Task Worker_ActivatesOnTimer()
        {
            // Indicated how many times the test activated
            int WasThreadRan = 0;

            var mockClient = new Mock<ITransitClient>();
            var mockRepo = new Mock<ITransitRepository>();
            mockRepo.Setup(repo => repo.GetServiceAlertsAsync())
                .Callback(() => WasThreadRan += 1)
                .Returns(Task.FromResult<(List<ServiceAlert>, ulong)?>((new List<ServiceAlert> { }, (ulong) 0)));

            var mockLogger = new Mock<ILogger<Worker>>();

            var worker = new Worker(mockLogger.Object, mockRepo.Object, mockClient.Object);

            // Setup Mock Service Provider
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton(mockClient.Object);
            services.AddSingleton(mockRepo.Object);
            services.AddSingleton(worker);
            var serviceProvider = services.BuildServiceProvider();
            var hostedService = serviceProvider.GetService<Worker>();
            Assert.NotNull(hostedService);

            int seconds = worker.WORKER_INTERVAL_SECONDS;
            worker.WORKER_INTERVAL_SECONDS = 5;

            await hostedService.StartAsync(CancellationToken.None);

            SpinWait.SpinUntil(() => WasThreadRan > 1, MILLISECOND_TO_SECOND_FACTOR * 15);
            Assert.Equal(2, WasThreadRan);

            await hostedService.StopAsync(CancellationToken.None);
        }
    }
}