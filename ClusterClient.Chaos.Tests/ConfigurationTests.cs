using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClusterClient.Chaos.Configuration;
using ClusterClient.Chaos.Tests.Mocks;
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
    public class ConfigurationTests
    {
        [Test]
        public async Task InjectTotalLatency_Should_AddLatencyOnceForSendMethod()
        {
            var transport = Substitute.For<ITransport>();
            var clusterProvider = Substitute.For<IClusterProvider>();
            transport.Capabilities.Returns(TransportCapabilities.None);
            transport
                .SendAsync(Arg.Any<Request>(), Arg.Any<TimeSpan?>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new Response(ResponseCode.Ok)));
            clusterProvider.GetCluster().Returns(new List<Uri> { new Uri("http://localhost1") });
            
            var delay = TimeSpan.FromMilliseconds(200);
            var latencyPerformer = new MockLatencyPerformer(_ => true);
            var clusterClient = new Vostok.Clusterclient.Core.ClusterClient(null,
                configuration =>
                {
                    configuration.Transport = transport;
                    configuration.ClusterProvider = clusterProvider;
                    configuration.DefaultRequestStrategy = new SingleReplicaRequestStrategy();
                    configuration.InjectTotalLatency(latencyPerformer,() => delay, () => 1);
                });
            var request = new Request("GET", new Uri("/fakemethod", UriKind.Relative));
            
            await clusterClient.SendAsync(request);

            latencyPerformer.TotalAddedLatency.Should().Be(delay);
        }
        
        [Test]
        public async Task InjectLatencyOnEveryRetry_Should_AddLatencyOnEveryRetryCall()
        {
            var transport = Substitute.For<ITransport>();
            var clusterProvider = Substitute.For<IClusterProvider>();

            var callsCount = 0;
            transport.Capabilities.Returns(TransportCapabilities.None);
            transport
                .SendAsync(Arg.Any<Request>(), Arg.Any<TimeSpan?>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new Response(ResponseCode.TooManyRequests)))
                .AndDoes(_ => callsCount++);
            clusterProvider.GetCluster().Returns(new List<Uri>
            {
                new Uri("http://localhost1"),
                new Uri("http://localhost2"),
                new Uri("http://localhost3")
            });
            
            
            var latency = TimeSpan.FromMilliseconds(200);
            var latencyPerformer = new MockLatencyPerformer(_ => true);
            var clusterClient = new Vostok.Clusterclient.Core.ClusterClient(null,
                configuration =>
                {
                    configuration.Transport = transport;
                    configuration.ClusterProvider = clusterProvider;
                    configuration.RetryPolicy = new AdHocRetryPolicy((_, __, ___) => callsCount < 3);
                    configuration.RetryStrategy = new ImmediateRetryStrategy(3);
                    configuration.DefaultRequestStrategy = new SingleReplicaRequestStrategy();
                    configuration.InjectLatencyOnEveryRetry(latencyPerformer, () => latency, () => 1);
                });

            var request = new Request("GET", new Uri("/fakemethod", UriKind.Relative));

           
            await clusterClient.SendAsync(request);

            callsCount.Should().Be(3);
            latencyPerformer.TotalAddedLatency.Should().Be(latency * 3);
        }

        [Test]
        public async Task InjectLatencyOnEveryNetworkCall_Should_AddLatencyOnEveryStrategyCall()
        {
            var transport = Substitute.For<ITransport>();
            var clusterProvider = Substitute.For<IClusterProvider>();
            
            var callsCount = 0;
            transport.Capabilities.Returns(TransportCapabilities.None);
            transport
                .SendAsync(Arg.Any<Request>(), Arg.Any<TimeSpan?>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new Response(ResponseCode.TooManyRequests)))
                .AndDoes(_ => callsCount++);
            clusterProvider.GetCluster().Returns(new List<Uri>
            {
                new Uri("http://localhost1"),
                new Uri("http://localhost2"),
                new Uri("http://localhost3")
            });
            
            
            var latency = TimeSpan.FromMilliseconds(200);
            var latencyPerformer = new MockLatencyPerformer(_ => true);
            var clusterClient = new Vostok.Clusterclient.Core.ClusterClient(null,
                configuration =>
                {
                    configuration.Transport = transport;
                    configuration.ClusterProvider = clusterProvider;
                    configuration.DefaultRequestStrategy = Strategy.Sequential3;
                    configuration.InjectLatencyOnEveryNetworkCall(latencyPerformer, () => latency, () => 1);
                });

            var request = new Request("GET", new Uri("/fakemethod", UriKind.Relative));
            
            await clusterClient.SendAsync(request);

            callsCount.Should().Be(3);
            latencyPerformer.TotalAddedLatency.Should().Be(latency * 3);
        }
    }
}