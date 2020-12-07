using System;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Sending;

namespace ClusterClient.Chaos.Latency
{
    internal class LatencyRequestSender : IRequestSender
    {
        private readonly IRequestSender baseRequestSender;
        private readonly ILatencyPerformer latencyPerformer;
        private readonly TimeSpan latency;
        private readonly double rate;

        public LatencyRequestSender(IRequestSender baseRequestSender, ILatencyPerformer latencyPerformer, TimeSpan latency, double rate)
        {
            this.baseRequestSender = baseRequestSender;
            this.latencyPerformer = latencyPerformer;
            this.latency = latency;
            this.rate = rate;
        }
        
        public async Task<ReplicaResult> SendToReplicaAsync(Uri replica, Request request, TimeSpan? connectionTimeout, TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            var leftTimeout = timeout;
            if (latencyPerformer.ShouldPerformLatency(rate))
            {
                leftTimeout = latency > timeout ? TimeSpan.Zero : timeout - latency;
                var addedLatency = leftTimeout > TimeSpan.Zero ? latency : timeout; 
                await latencyPerformer.PerformLatencyAsync(addedLatency, cancellationToken).ConfigureAwait(false);
            }

            return await baseRequestSender
                .SendToReplicaAsync(replica, request, connectionTimeout, leftTimeout, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}