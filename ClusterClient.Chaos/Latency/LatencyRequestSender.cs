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
        private readonly TimeSpan delay;
        private readonly double rate;

        public LatencyRequestSender(IRequestSender baseRequestSender, TimeSpan delay, double rate)
        {
            this.baseRequestSender = baseRequestSender;
            this.delay = delay;
            this.rate = rate;
        }
        
        public async Task<ReplicaResult> SendToReplicaAsync(Uri replica, Request request, TimeSpan? connectionTimeout, TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            var leftTimeout = delay > timeout ? TimeSpan.Zero : timeout - delay;

            if (leftTimeout > TimeSpan.Zero)
            {
                await LatencyPerformer.PerformLatencyAsync(delay, rate, cancellationToken).ConfigureAwait(false);
            }
            
            return await baseRequestSender
                .SendToReplicaAsync(replica, request, connectionTimeout, leftTimeout, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}