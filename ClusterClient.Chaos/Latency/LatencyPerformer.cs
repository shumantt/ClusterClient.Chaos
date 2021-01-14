using System;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Commons.Threading;

namespace ClusterClient.Chaos.Latency
{
    internal class LatencyPerformer : ILatencyPerformer
    {
        public Task PerformLatencyAsync(TimeSpan latency, CancellationToken cancellationToken)
        {
            return Task.Delay(latency, cancellationToken);
        }
    }
}