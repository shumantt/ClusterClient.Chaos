using System;
using System.Threading;
using System.Threading.Tasks;
using ClusterClient.Chaos.Latency;

namespace ClusterClient.Chaos.Tests.Mocks
{
    public class MockLatencyPerformer : ILatencyPerformer
    {
        public TimeSpan TotalAddedLatency { get; private set; } = TimeSpan.Zero;

        public Task PerformLatencyAsync(TimeSpan latency, CancellationToken cancellationToken)
        {
            TotalAddedLatency += latency;
            return Task.CompletedTask;
        }
    }
}