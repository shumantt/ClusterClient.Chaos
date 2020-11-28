using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ClusterClient.Chaos.Latency;
using FluentAssertions;
using NUnit.Framework;

namespace ClusterClient.Chaos.Tests
{
    public class LatencyPerformerTests
    {
        [Test]
        public async Task LatencyPerformed()
        {
            var delay = TimeSpan.FromMilliseconds(100);
            var stopwatch = new Stopwatch();
            
            stopwatch.Start();
            await LatencyPerformer.PerformLatencyAsync(delay, 1,  new CancellationToken());
            stopwatch.Stop();

            stopwatch.Elapsed.Should().BeGreaterOrEqualTo(delay);
        }

        [Test]
        public async Task NoLatencyAdded_WhenRateIsZero()
        {
            var delay = TimeSpan.FromSeconds(5);
            var stopwatch = new Stopwatch();
            
            stopwatch.Start();
            await LatencyPerformer.PerformLatencyAsync(delay, 0,  new CancellationToken());
            stopwatch.Stop();

            stopwatch.Elapsed.Should().BeLessOrEqualTo(delay);
        }

        [Test]
        public async Task ShouldBeCancelled_WhenCancelRequestedWithToken()
        {
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            
            Func<Task> action = async () => await LatencyPerformer.PerformLatencyAsync(TimeSpan.FromMilliseconds(100), 1, cancellationToken);
            cts.Cancel();

            await action.Should().ThrowAsync<TaskCanceledException>();
        }
    }
}