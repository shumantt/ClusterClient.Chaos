using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClusterClient.Chaos.Configuration;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Strategies;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Core.Transport;

namespace ClusterClient.Chaos.Tests.Error
{
    public class ConfigurationTests
    {
        [Test]
        public async Task InjectCommonServerError_Should_TransformResponseToError()
        {
            var transport = Substitute.For<ITransport>();
            var clusterProvider = Substitute.For<IClusterProvider>();
            transport.Capabilities.Returns(TransportCapabilities.None);
            transport
                .SendAsync(Arg.Any<Request>(), Arg.Any<TimeSpan?>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new Response(ResponseCode.Ok)));
            clusterProvider.GetCluster().Returns(new List<Uri> { new Uri("http://localhost1") });
            var clusterClient = new Vostok.Clusterclient.Core.ClusterClient(null,
                configuration =>
                {
                    configuration.Transport = transport;
                    configuration.ClusterProvider = clusterProvider;
                    configuration.DefaultRequestStrategy = new SingleReplicaRequestStrategy();
                    configuration.InjectCommonServerError(() => 1);
                });
            var request = new Request("GET", new Uri("/fakemethod", UriKind.Relative));
            
            var actualResponse = await clusterClient.SendAsync(request);
            
            actualResponse.Response.Should().BeEquivalentTo(new Response(ResponseCode.InternalServerError));
        }
    }
}