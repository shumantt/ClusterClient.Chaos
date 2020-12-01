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
        private ILatencyPerformer latencyPerformer;

        public LatencyModule(ILatencyPerformer latencyPerformer, Func<TimeSpan> delayProvider, Func<double> rateProvider)
        {
            this.delayProvider = delayProvider;
            this.rateProvider = rateProvider;
            this.latencyPerformer = latencyPerformer;
        }
        
        public async Task<ClusterResult> ExecuteAsync(IRequestContext context, Func<IRequestContext, Task<ClusterResult>> next)
        {
            var delay = delayProvider();
            var rate = rateProvider();
            var remainingTimeBudget = context.Budget.Remaining;
            if (latencyPerformer.ShouldPerformLatency(rate))
            {
                if (delay > remainingTimeBudget)
                {
                    await latencyPerformer.PerformLatencyAsync(remainingTimeBudget, context.CancellationToken).ConfigureAwait(false);
                    return new ClusterResult(ClusterResultStatus.TimeExpired, new ReplicaResult[] {}, null, context.Request);
                }
                
                await latencyPerformer.PerformLatencyAsync(delay, context.CancellationToken).ConfigureAwait(false);
            }
            
            return await next(context).ConfigureAwait(false);
        }
    }
}