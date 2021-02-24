using System;
using ClusterClient.Chaos.Common;
using ClusterClient.Chaos.Error;
using ClusterClient.Chaos.Latency;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Misc;
using Vostok.Clusterclient.Core.Modules;

namespace ClusterClient.Chaos.Configuration
{
    public static partial class ClusterClientConfigurationExtensions
    {
        /// <summary>
        /// Injects error response as final result of the request pipeline
        /// </summary>
        /// <param name="configuration">IClusterClientConfiguration instance</param>
        /// <param name="rateProvider">Func returning injection probability (rate)</param>
        public static void InjectCommonServerError(
            this IClusterClientConfiguration configuration,
            Func<double> rateProvider)
        {
            configuration.AddResponseTransform(new ErrorResponseTransform(new RateManager(), rateProvider));
        }
    }
}