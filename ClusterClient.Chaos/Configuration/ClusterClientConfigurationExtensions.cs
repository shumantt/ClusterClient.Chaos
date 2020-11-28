using System;
using ClusterClient.Chaos.Latency;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Misc;
using Vostok.Clusterclient.Core.Modules;
using Vostok.Clusterclient.Core.Strategies;

namespace ClusterClient.Chaos.Configuration
{
    public static class ClusterClientConfigurationExtensions
    {
        public static void SetupTotalLatency(
            this IClusterClientConfiguration configuration, 
            Func<TimeSpan> delayProvider, 
            Func<double> rateProvider)
        {
            configuration.AddRequestModule(new LatencyModule(delayProvider, rateProvider), RequestModule.RequestRetry, ModulePosition.Before);
        }

        public static void SetupLatencyOnEveryRetry(
            this IClusterClientConfiguration configuration, 
            Func<TimeSpan> delayProvider, 
            Func<double> rateProvider)
        {
            configuration.AddRequestModule(new LatencyModule(delayProvider, rateProvider), RequestModule.RequestRetry, ModulePosition.After);
        }

        public static void SetupLatencyOnEveryNetworkCall(
            this IClusterClientConfiguration configuration,
            IRequestStrategy defaultRequestStrategy,
            Func<TimeSpan> delayProvider,
            Func<double> rateProvider)
        {
            configuration.DefaultRequestStrategy = new LatencyStrategy(delayProvider, rateProvider, defaultRequestStrategy);
        }
    }
}