using System;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Commons.Threading;

namespace ClusterClient.Chaos.Latency
{
    public static class LatencyPerformer
    {
        public static Task PerformLatencyAsync(TimeSpan delay, double rate, CancellationToken cancellationToken)
        {
            if (rate > 0 && ThreadSafeRandom.NextDouble() <= rate)
            {
                return Task.Delay(delay, cancellationToken);
            }
            
            return Task.CompletedTask;
        }
    }
}