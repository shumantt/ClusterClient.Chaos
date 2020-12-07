using System;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Commons.Threading;

namespace ClusterClient.Chaos.Latency
{
    public class LatencyPerformer : ILatencyPerformer
    {
        public bool ShouldPerformLatency(double rate)
        {
            return rate > 0 && ThreadSafeRandom.NextDouble() <= rate;
        }

        public Task PerformLatencyAsync(TimeSpan latency, CancellationToken cancellationToken)
        {
            return Task.Delay(latency, cancellationToken);
        }
    }
}