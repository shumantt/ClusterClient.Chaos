using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Sending;
using Vostok.Clusterclient.Core.Strategies;

namespace ClusterClient.Chaos.Latency
{
    public class LatencyStrategy : IRequestStrategy
    {
        private readonly Func<TimeSpan> delayProvider;
        private readonly Func<double> rateProvider;
        private readonly IRequestStrategy baseStrategy;

        public LatencyStrategy(Func<TimeSpan> delayProvider, Func<double> rateProvider, IRequestStrategy baseStrategy)
        {
            this.delayProvider = delayProvider;
            this.rateProvider = rateProvider;
            this.baseStrategy = baseStrategy;
        }

        public Task SendAsync(Request request, RequestParameters parameters, IRequestSender sender, IRequestTimeBudget budget,
            IEnumerable<Uri> replicas, int replicasCount, CancellationToken cancellationToken)
        {
            var senderWithLatency = new LatencyRequestSender(sender, delayProvider(), rateProvider());
            return baseStrategy.SendAsync(request, parameters, senderWithLatency, budget, replicas, replicasCount, cancellationToken);
        }

        public override string ToString()
        {
            return $"{baseStrategy} with latency";
        }
    }
}