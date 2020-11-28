using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ClusterClient.Chaos.Configuration;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Retry;
using Vostok.Clusterclient.Core.Strategies;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Core.Transport;

namespace ClusterClient.Chaos.Tests
{
    public class ClusterClientConfigurationExtensionsTests
    {
        [Test]
        public async Task SetupLatencyChaos_Should_AddLatencyOnEveryRetryCall()
        {
            var transport = Substitute.For<ITransport>();
            var clusterProvider = Substitute.For<IClusterProvider>();
            var calledTimespans = new List<TimeSpan>();
            var callStopWatch = new Stopwatch();
            transport.Capabilities.Returns(TransportCapabilities.None);
            transport
                .SendAsync(Arg.Any<Request>(), Arg.Any<TimeSpan?>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new Response(ResponseCode.TooManyRequests)))
                .AndDoes(_ => calledTimespans.Add(callStopWatch.Elapsed));
            clusterProvider.GetCluster().Returns(new List<Uri>
            {
                new Uri("http://localhost1"),
                new Uri("http://localhost2"),
                new Uri("http://localhost3")
            });
            
            
            var delay = TimeSpan.FromMilliseconds(100);
            var clusterClient = new Vostok.Clusterclient.Core.ClusterClient(null,
                configuration =>
                {
                    configuration.Transport = transport;
                    configuration.ClusterProvider = clusterProvider;
                    configuration.RetryPolicy = new AdHocRetryPolicy((_, __, ___) => calledTimespans.Count < 3);
                    configuration.RetryStrategy = new ImmediateRetryStrategy(3);
                    configuration.DefaultRequestStrategy = new SingleReplicaRequestStrategy();
                    configuration.SetupLatencyChaos(delay, 1);
                });

            var request = new Request("GET", new Uri("/fakemethod", UriKind.Relative));

            callStopWatch.Start();
            await clusterClient.SendAsync(request, timeout: TimeSpan.MaxValue);

            calledTimespans.Count.Should().Be(3);
            for (int i = 0; i < calledTimespans.Count; i++)
            {
                var timeSpan = calledTimespans[i];
                timeSpan.Should().BeGreaterOrEqualTo(delay * (i + 1));
            }
        }
    }
}