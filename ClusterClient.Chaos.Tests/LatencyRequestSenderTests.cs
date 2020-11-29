using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClusterClient.Chaos.Latency;
using NSubstitute;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Sending;

namespace ClusterClient.Chaos.Tests
{
    public class LatencyRequestSenderTests
    {
        [TestCaseSource(nameof(GenerateTimeoutReducedTests))]
        public async Task TestRequestTimeoutReducedWithDelay(TimeSpan delay, TimeSpan callTimeout, TimeSpan expectedTimeout)
        {
            var baseSender = Substitute.For<IRequestSender>();
            var request = new Request("GET", new Uri("/fakemethod", UriKind.Relative));
            var replica = new Uri("http://localhost");
            var cancellationToken = new CancellationToken();
            baseSender
                .SendToReplicaAsync(replica, request, null, expectedTimeout,  cancellationToken)
                .Returns(Task.FromResult<ReplicaResult>(null));
            
            var latencySender = new LatencyRequestSender(baseSender, delay, 1);

            await latencySender.SendToReplicaAsync(replica, request, null, callTimeout, cancellationToken);

            baseSender.Received()
                .SendToReplicaAsync(replica, request, null, expectedTimeout, cancellationToken);
        }

        private static IEnumerable<TestCaseData> GenerateTimeoutReducedTests()
        {
            yield return new TestCaseData(TimeSpan.FromMilliseconds(40), TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(60))
                .SetName("Reduce timeout for added latency");
            yield return new TestCaseData(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(50), TimeSpan.Zero)
                .SetName("Timeout is zero, when delay is more than timeout");
        }
    }
}