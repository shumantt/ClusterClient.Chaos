using System;
using ClusterClient.Chaos.Common;
using ClusterClient.Chaos.Latency;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Misc;
using Vostok.Clusterclient.Core.Modules;

namespace ClusterClient.Chaos.Configuration
{
    public static class ClusterClientConfigurationExtensions
    {
        /// <summary>
        /// Inject latency to the whole request pipeline with default latency performer
        /// </summary>
        /// <example>
        /// <code>
        ///     ---► + injected latency ---► replica(1..n)
        ///     ◄-------------------------------------|
        /// </code>
        /// </example>
        /// <param name="configuration">IClusterClientConfiguration instance</param>
        /// <param name="latencyProvider">Func returning latency to inject</param>
        /// <param name="rateProvider">Func returning injection probability (rate)</param>
        public static void InjectTotalLatency(
            this IClusterClientConfiguration configuration, 
            Func<TimeSpan> latencyProvider, 
            Func<double> rateProvider)
        {
            configuration.AddRequestModule(new LatencyModule(new LatencyPerformer(), new RateManager(), latencyProvider, rateProvider), RequestModule.RequestRetry, ModulePosition.Before);
        }
        
        /// <summary>
        /// Inject latency to the whole request pipeline with custom <paramref name="latencyPerformer"/>
        /// </summary>
        /// <example>
        /// <code>
        ///     ---► + injected latency ---► replica(1..n)
        ///     ◄-------------------------------------|
        /// </code>
        /// </example>
        /// <param name="configuration">IClusterClientConfiguration instance</param>
        /// <param name="latencyPerformer">Custom class for latency performing or simulation</param>
        /// <param name="latencyProvider">Func returning latency to inject</param>
        /// <param name="rateProvider">Func returning injection probability (rate)</param>
        public static void InjectTotalLatency(
            this IClusterClientConfiguration configuration, 
            ILatencyPerformer latencyPerformer,
            Func<TimeSpan> latencyProvider, 
            Func<double> rateProvider)
        {
            configuration.AddRequestModule(new LatencyModule(latencyPerformer, new RateManager(), latencyProvider, rateProvider), RequestModule.RequestRetry, ModulePosition.Before);
        }

        /// <summary>
        /// Inject latency to the every request retry with default latency performer
        /// </summary>
        /// <example>
        /// <code>
        ///   ------► + injected latency ---► replica(1..n)
        ///     ↑__________retry (1..n)__________________|
        /// </code>
        /// </example>
        /// <param name="configuration">IClusterClientConfiguration instance</param>
        /// <param name="latencyProvider">Func returning latency to inject</param>
        /// <param name="rateProvider">Func returning injection probability (rate)</param>
        public static void InjectLatencyOnEveryRetry(
            this IClusterClientConfiguration configuration, 
            Func<TimeSpan> latencyProvider, 
            Func<double> rateProvider)
        {
            configuration.AddRequestModule(new LatencyModule(new LatencyPerformer(), new RateManager(), latencyProvider, rateProvider), RequestModule.RequestRetry, ModulePosition.After);
        }
        
        /// <summary>
        /// Inject latency to the every request retry with custom <paramref name="latencyPerformer"/>
        /// </summary>
        /// <example>
        /// <code>
        ///   ------► + injected latency ---► replica(1..n)
        ///     ↑__________retry (1..n)__________________|
        /// </code>
        /// </example>
        /// <param name="configuration">IClusterClientConfiguration instance</param>
        /// <param name="latencyPerformer">Custom class for latency performing or simulation</param>
        /// <param name="latencyProvider">Func returning latency to inject</param>
        /// <param name="rateProvider">Func returning injection probability (rate)</param>
        public static void InjectLatencyOnEveryRetry(
            this IClusterClientConfiguration configuration, 
            ILatencyPerformer latencyPerformer,
            Func<TimeSpan> latencyProvider, 
            Func<double> rateProvider)
        {
            configuration.AddRequestModule(new LatencyModule(latencyPerformer, new RateManager(), latencyProvider, rateProvider), RequestModule.RequestRetry, ModulePosition.After);
        }

        /// <summary>
        /// Inject latency to the every strategy IRequestSender.SendToReplicaAsync call with default latency performer
        /// </summary>
        /// <example>
        /// <code>
        ///   ---► + injected latency ---► replica1 ---► + injected latency ---> replica2 ...
        ///                                                          ◄---------------------|
        ///     
        /// </code>
        /// </example>
        /// <param name="configuration">IClusterClientConfiguration instance</param>
        /// <param name="latencyProvider">Func returning latency to inject</param>
        /// <param name="rateProvider">Func returning injection probability (rate)</param>
        public static void InjectLatencyOnEveryNetworkCall(
            this IClusterClientConfiguration configuration,
            Func<TimeSpan> latencyProvider,
            Func<double> rateProvider)
        {
            configuration.DefaultRequestStrategy = new LatencyStrategy(new LatencyPerformer(), new RateManager(), latencyProvider, rateProvider, configuration.DefaultRequestStrategy);
        }

        /// <summary>
        /// Inject latency to the every strategy IRequestSender.SendToReplicaAsync call with custom <paramref name="latencyPerformer"/>
        /// </summary>
        /// <example>
        /// <code>
        ///   ---► + injected latency ---► replica1 ---► + injected latency ---> replica2 ...
        ///                                                          ◄---------------------|
        /// </code>
        /// </example>
        /// <param name="configuration">IClusterClientConfiguration instance</param>
        /// <param name="latencyPerformer">Custom class for latency performing</param>
        /// <param name="latencyProvider">Func returning latency to inject</param>
        /// <param name="rateProvider">Func returning injection probability (rate)</param>
        public static void InjectLatencyOnEveryNetworkCall(
            this IClusterClientConfiguration configuration,
            ILatencyPerformer latencyPerformer,
            Func<TimeSpan> latencyProvider,
            Func<double> rateProvider)
        {
            configuration.DefaultRequestStrategy = new LatencyStrategy(latencyPerformer, new RateManager(), latencyProvider, rateProvider, configuration.DefaultRequestStrategy);
        }
    }
}