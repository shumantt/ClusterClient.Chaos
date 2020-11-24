using System;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Modules;
using Vostok.Commons.Threading;

namespace ClusterClient.Chaos.Latency
{
    public class LatencyModule : IRequestModule
    {
        private readonly int delayMillisecond;
        private readonly double probability;

        public LatencyModule(int delayMillisecond, double probability)
        {
            if (delayMillisecond < 0)
            {
                throw new ArgumentException($"{nameof(delayMillisecond)} should be a positive number");
            }
            
            if (probability < 0 || probability > 1)
            {
                throw new ArgumentException($"{nameof(probability)} should be in a range from 0.0 to 1.0");
            }
            
            this.delayMillisecond = delayMillisecond;
            this.probability = probability;
        }
        
        public async Task<ClusterResult> ExecuteAsync(IRequestContext context, Func<IRequestContext, Task<ClusterResult>> next)
        {
            if (ShouldAddLatency())
            {
                var delay = Math.Min(delayMillisecond, context.Budget.Remaining.Milliseconds);
                await Task.Delay(delay, context.CancellationToken).ConfigureAwait(false);
            }
            
            return await next(context).ConfigureAwait(false);
        }

        private bool ShouldAddLatency()
        {
            return ThreadSafeRandom.NextDouble() <= probability;
        }
    }
}