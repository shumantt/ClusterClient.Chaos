using System;
using ClusterClient.Chaos.Latency;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Misc;
using Vostok.Clusterclient.Core.Modules;

namespace ClusterClient.Chaos.Configuration
{
    public static class ClusterClientConfigurationExtensions
    {
        public static void SetupLatencyChaos(
            this IClusterClientConfiguration configuration, 
            Func<TimeSpan> delayProvider, 
            Func<double> rateProvider)
        {
            configuration.AddRequestModule(new LatencyModule(delayProvider, rateProvider), RequestModule.RequestRetry, ModulePosition.After);
        }
        
        public static void SetupLatencyChaos(
            this IClusterClientConfiguration configuration, 
            TimeSpan delay, 
            double rate)
        {
            SetupLatencyChaos(configuration, () => delay, () => rate);
        }
    }
}