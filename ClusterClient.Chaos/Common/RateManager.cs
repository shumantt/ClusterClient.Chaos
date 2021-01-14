using Vostok.Commons.Threading;

namespace ClusterClient.Chaos.Common
{
    public class RateManager
    {
        public bool ShouldInjectWithRate(double rate)
        {
            return rate > 0 && ThreadSafeRandom.NextDouble() <= rate;
        }
    }
}