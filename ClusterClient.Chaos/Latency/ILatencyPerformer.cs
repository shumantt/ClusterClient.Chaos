using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClusterClient.Chaos.Latency
{
    public interface ILatencyPerformer
    {
        Task PerformLatencyAsync(TimeSpan latency, CancellationToken cancellationToken);
    }
}