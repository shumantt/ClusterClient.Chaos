using System;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Sending;

namespace ClusterClient.Chaos.Latency
{
    public class LatencyRequestSender : IRequestSender
    {
        private readonly IRequestSender baseRequestSender;
        private readonly ILatencyPerformer latencyPerformer;
        private readonly TimeSpan delay;
        private readonly double rate;

        public LatencyRequestSender(IRequestSender baseRequestSender, ILatencyPerformer latencyPerformer, TimeSpan delay, double rate)
        {
            this.baseRequestSender = baseRequestSender;
            this.latencyPerformer = latencyPerformer;
            this.delay = delay;
            this.rate = rate;
        }
        
        public async Task<ReplicaResult> SendToReplicaAsync(Uri replica, Request request, TimeSpan? connectionTimeout, TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            var leftTimeout = timeout;
            if (latencyPerformer.ShouldPerformLatency(rate))
            {
                leftTimeout = delay > timeout ? TimeSpan.Zero : timeout - delay;
                var addedLatency = leftTimeout > TimeSpan.Zero ? delay : timeout; 
                await latencyPerformer.PerformLatencyAsync(addedLatency, cancellationToken).ConfigureAwait(false);
            }

            return await baseRequestSender
                .SendToReplicaAsync(replica, request, connectionTimeout, leftTimeout, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}