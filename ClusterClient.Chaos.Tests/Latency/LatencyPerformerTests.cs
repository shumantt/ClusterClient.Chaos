using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ClusterClient.Chaos.Latency;
using FluentAssertions;
using NUnit.Framework;

namespace ClusterClient.Chaos.Tests.Latency
{
    public class LatencyPerformerTests
    {
        private LatencyPerformer latencyPerformer;

        [SetUp]
        public void SetUp()
        {
            latencyPerformer = new LatencyPerformer();
        }
        
        [Test]
        public async Task PerformLatencyAsync_Should_PerformLatency()
        {
            var latency = TimeSpan.FromMilliseconds(100);
            var stopwatch = new Stopwatch();
            
            stopwatch.Start();
            await latencyPerformer.PerformLatencyAsync(latency, new CancellationToken());
            stopwatch.Stop();

            stopwatch.Elapsed.Should().BeGreaterOrEqualTo(latency);
        }

        [Test]
        public async Task PerformLatencyAsync_Should_BeCancelled_WhenCancelRequestedWithToken()
        {
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            
            Func<Task> action = async () => await latencyPerformer.PerformLatencyAsync(TimeSpan.FromMilliseconds(100), cancellationToken);
            cts.Cancel();

            await action.Should().ThrowAsync<TaskCanceledException>();
        }
    }
}