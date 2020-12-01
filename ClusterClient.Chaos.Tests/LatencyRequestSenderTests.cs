using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClusterClient.Chaos.Latency;
using ClusterClient.Chaos.Tests.Mocks;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Sending;

namespace ClusterClient.Chaos.Tests
{
    public class LatencyRequestSenderTests
    {
        [TestCaseSource(nameof(GenerateTimeoutReducedTests))]
        public async Task TestRequestTimeoutReducedWithDelay(TimeSpan delay, TimeSpan callTimeout, TimeSpan expectedTimeout, TimeSpan expectedPerformedDelay)
        {
            var baseSender = Substitute.For<IRequestSender>();
            var request = new Request("GET", new Uri("/fakemethod", UriKind.Relative));
            var replica = new Uri("http://localhost");
            var cancellationToken = new CancellationToken();
            baseSender
                .SendToReplicaAsync(replica, request, null, expectedTimeout,  cancellationToken)
                .Returns(Task.FromResult<ReplicaResult>(null));
            var latencyPerformer = new MockLatencyPerformer(_ => true);
            var latencySender = new LatencyRequestSender(baseSender, latencyPerformer, delay, 1);

            await latencySender.SendToReplicaAsync(replica, request, null, callTimeout, cancellationToken);

            baseSender.Received()
                .SendToReplicaAsync(replica, request, null, expectedTimeout, cancellationToken);
            latencyPerformer.TotalAddedLatency.Should().Be(expectedPerformedDelay);
        }

        private static IEnumerable<TestCaseData> GenerateTimeoutReducedTests()
        {
            yield return new TestCaseData(TimeSpan.FromMilliseconds(40), TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(60), TimeSpan.FromMilliseconds(40))
                .SetName("Reduce timeout for added latency and perform it");
            yield return new TestCaseData(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(50), TimeSpan.Zero, TimeSpan.FromMilliseconds(50))
                .SetName("Timeout is zero, when delay is more than timeout, latency for timeout performed");
        }
    }
}