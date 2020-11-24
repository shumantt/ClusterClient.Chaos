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

        public LatencyModule(TimeSpan delay, double rate)
        {
            delayProvider = () => delay;
            rateProvider = () => rate;
        }
        
        public async Task<ClusterResult> ExecuteAsync(IRequestContext context, Func<IRequestContext, Task<ClusterResult>> next)
        {
            if (ShouldAddLatency())
            {
                var delay = delayProvider();
                var remainingTimeBudget = context.Budget.Remaining;
                var latency = delay > remainingTimeBudget ? remainingTimeBudget : delay;
                await Task.Delay(latency, context.CancellationToken).ConfigureAwait(false);
            }
            
            return await next(context).ConfigureAwait(false);
        }

        private bool ShouldAddLatency()
        {
            return ThreadSafeRandom.NextDouble() <= rateProvider();
        }
    }
}