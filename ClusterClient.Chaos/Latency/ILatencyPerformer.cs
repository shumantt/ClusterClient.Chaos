using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClusterClient.Chaos.Latency
{
    public interface ILatencyPerformer
    {
        bool ShouldPerformLatency(double rate);
        Task PerformLatencyAsync(TimeSpan latency, CancellationToken cancellationToken);
    }
}