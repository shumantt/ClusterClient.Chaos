using System;
using System.Threading;
using System.Threading.Tasks;
using ClusterClient.Chaos.Latency;

namespace ClusterClient.Chaos.Tests.Mocks
{
    public class MockLatencyPerformer : ILatencyPerformer
    {
        private readonly Func<double, bool> shouldPerformLatency;
        public TimeSpan TotalAddedLatency = TimeSpan.Zero;
        
        public MockLatencyPerformer(Func<double, bool> shouldPerformLatency)
        {
            this.shouldPerformLatency = shouldPerformLatency;
        }

        public bool ShouldPerformLatency(double rate) => shouldPerformLatency(rate);

        public Task PerformLatencyAsync(TimeSpan delay, CancellationToken cancellationToken)
        {
            TotalAddedLatency += delay;
            return Task.CompletedTask;
        }
    }
}