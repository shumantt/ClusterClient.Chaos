using System;
using ClusterClient.Chaos.Latency;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Misc;
using Vostok.Clusterclient.Core.Modules;

namespace ClusterClient.Chaos.Configuration
{
    public static class ClusterClientConfigurationExtensions
    {
        public static void InjectTotalLatency(
            this IClusterClientConfiguration configuration, 
            Func<TimeSpan> latencyProvider, 
            Func<double> rateProvider)
        {
            configuration.AddRequestModule(new LatencyModule(new LatencyPerformer(), latencyProvider, rateProvider), RequestModule.RequestRetry, ModulePosition.Before);
        }
        
        public static void InjectTotalLatency(
            this IClusterClientConfiguration configuration, 
            ILatencyPerformer latencyPerformer,
            Func<TimeSpan> latencyProvider, 
            Func<double> rateProvider)
        {
            configuration.AddRequestModule(new LatencyModule(latencyPerformer, latencyProvider, rateProvider), RequestModule.RequestRetry, ModulePosition.Before);
        }

        public static void InjectLatencyOnEveryRetry(
            this IClusterClientConfiguration configuration, 
            Func<TimeSpan> latencyProvider, 
            Func<double> rateProvider)
        {
            configuration.AddRequestModule(new LatencyModule(new LatencyPerformer(), latencyProvider, rateProvider), RequestModule.RequestRetry, ModulePosition.After);
        }
        
        public static void InjectLatencyOnEveryRetry(
            this IClusterClientConfiguration configuration, 
            ILatencyPerformer latencyPerformer,
            Func<TimeSpan> latencyProvider, 
            Func<double> rateProvider)
        {
            configuration.AddRequestModule(new LatencyModule(latencyPerformer, latencyProvider, rateProvider), RequestModule.RequestRetry, ModulePosition.After);
        }

        public static void InjectLatencyOnEveryNetworkCall(
            this IClusterClientConfiguration configuration,
            Func<TimeSpan> delayProvider,
            Func<double> rateProvider)
        {
            configuration.DefaultRequestStrategy = new LatencyStrategy(new LatencyPerformer(), delayProvider, rateProvider, configuration.DefaultRequestStrategy);
        }

        public static void InjectLatencyOnEveryNetworkCall(
            this IClusterClientConfiguration configuration,
            ILatencyPerformer latencyPerformer,
            Func<TimeSpan> latencyProvider,
            Func<double> rateProvider)
        {
            configuration.DefaultRequestStrategy = new LatencyStrategy(latencyPerformer, latencyProvider, rateProvider, configuration.DefaultRequestStrategy);
        }
    }
}