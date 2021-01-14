using System;
using System.Threading.Tasks;
using ClusterClient.Chaos.Common;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Modules;

namespace ClusterClient.Chaos.Latency
{
    internal class LatencyModule : IRequestModule
    {
        private readonly Func<TimeSpan> latencyProvider;
        private readonly Func<double> rateProvider;
        private ILatencyPerformer latencyPerformer;
        private readonly RateManager rateManager;

        public LatencyModule(ILatencyPerformer latencyPerformer, RateManager rateManager, Func<TimeSpan> latencyProvider, Func<double> rateProvider)
        {
            this.latencyProvider = latencyProvider;
            this.rateProvider = rateProvider;
            this.latencyPerformer = latencyPerformer;
            this.rateManager = rateManager;
        }
        
        public async Task<ClusterResult> ExecuteAsync(IRequestContext context, Func<IRequestContext, Task<ClusterResult>> next)
        {
            var latency = latencyProvider();
            var rate = rateProvider();
            var remainingTimeBudget = context.Budget.Remaining;
            if (rateManager.ShouldInjectWithRate(rate))
            {
                if (latency > remainingTimeBudget)
                {
                    await latencyPerformer.PerformLatencyAsync(remainingTimeBudget, context.CancellationToken).ConfigureAwait(false);
                    return new ClusterResult(ClusterResultStatus.TimeExpired, new ReplicaResult[] {}, null, context.Request);
                }
                
                await latencyPerformer.PerformLatencyAsync(latency, context.CancellationToken).ConfigureAwait(false);
            }
            
            return await next(context).ConfigureAwait(false);
        }
    }
}