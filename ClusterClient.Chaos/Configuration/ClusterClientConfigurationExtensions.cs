using System;
using ClusterClient.Chaos.Latency;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Misc;
using Vostok.Clusterclient.Core.Modules;

namespace ClusterClient.Chaos.Configuration
{
    public static class ClusterClientConfigurationExtensions
    {
        public static void SetupTotalLatency(
            this IClusterClientConfiguration configuration, 
            Func<TimeSpan> delayProvider, 
            Func<double> rateProvider)
        {
            configuration.AddRequestModule(new LatencyModule(new LatencyPerformer(), delayProvider, rateProvider), RequestModule.RequestRetry, ModulePosition.Before);
        }
        
        public static void SetupTotalLatency(
            this IClusterClientConfiguration configuration, 
            ILatencyPerformer latencyPerformer,
            Func<TimeSpan> delayProvider, 
            Func<double> rateProvider)
        {
            configuration.AddRequestModule(new LatencyModule(latencyPerformer, delayProvider, rateProvider), RequestModule.RequestRetry, ModulePosition.Before);
        }

        public static void SetupLatencyOnEveryRetry(
            this IClusterClientConfiguration configuration, 
            Func<TimeSpan> delayProvider, 
            Func<double> rateProvider)
        {
            configuration.AddRequestModule(new LatencyModule(new LatencyPerformer(), delayProvider, rateProvider), RequestModule.RequestRetry, ModulePosition.After);
        }
        
        public static void SetupLatencyOnEveryRetry(
            this IClusterClientConfiguration configuration, 
            ILatencyPerformer latencyPerformer,
            Func<TimeSpan> delayProvider, 
            Func<double> rateProvider)
        {
            configuration.AddRequestModule(new LatencyModule(latencyPerformer, delayProvider, rateProvider), RequestModule.RequestRetry, ModulePosition.After);
        }

        public static void SetupLatencyOnEveryNetworkCall(
            this IClusterClientConfiguration configuration,
            Func<TimeSpan> delayProvider,
            Func<double> rateProvider)
        {
            configuration.DefaultRequestStrategy = new LatencyStrategy(new LatencyPerformer(), delayProvider, rateProvider, configuration.DefaultRequestStrategy);
        }

        public static void SetupLatencyOnEveryNetworkCall(
            this IClusterClientConfiguration configuration,
            ILatencyPerformer latencyPerformer,
            Func<TimeSpan> delayProvider,
            Func<double> rateProvider)
        {
            configuration.DefaultRequestStrategy = new LatencyStrategy(latencyPerformer, delayProvider, rateProvider, configuration.DefaultRequestStrategy);
        }
    }
}