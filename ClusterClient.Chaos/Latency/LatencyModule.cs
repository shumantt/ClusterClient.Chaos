using System;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Modules;
using Vostok.Commons.Threading;

namespace ClusterClient.Chaos.Latency
{
    public class LatencyModule : IRequestModule
    {
        private readonly Func<TimeSpan> delayProvider;
        private readonly Func<double> rateProvider;

        public LatencyModule(Func<TimeSpan> delayProvider, Func<double> rateProvider)
        {
            this.delayProvider = delayProvider;
            this.rateProvider = rateProvider;
        }
        
        public async Task<ClusterResult> ExecuteAsync(IRequestContext context, Func<IRequestContext, Task<ClusterResult>> next)
        {
            var delay = delayProvider();
            var remainingTimeBudget = context.Budget.Remaining;
            if (delay > remainingTimeBudget)
            {
                return new ClusterResult(ClusterResultStatus.TimeExpired, new ReplicaResult[] {}, null, context.Request);
            }

            await LatencyPerformer.PerformLatencyAsync(delay, rateProvider(), context.CancellationToken).ConfigureAwait(false);
            
            return await next(context).ConfigureAwait(false);
        }
    }
}