using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClusterClient.Chaos.Common;
using ClusterClient.Chaos.Latency;
using ClusterClient.Chaos.Tests.Mocks;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Modules;

namespace ClusterClient.Chaos.Tests.Latency
{
    public class LatencyModuleTests
    {
        private IRequestContext defaultContext;
        private Func<IRequestContext, Task<ClusterResult>> defaultNext;
        private bool nextExecuted;
        
        
        [SetUp]
        public void SetUp()
        {
            defaultContext = Substitute.For<IRequestContext>();
            defaultContext.Budget.Remaining.Returns(TimeSpan.MaxValue);
            defaultContext.CancellationToken.Returns(new CancellationToken());
            
            nextExecuted = false;
            defaultNext = ctx =>
            {
                nextExecuted = true;
                return Task.FromResult(new ClusterResult(ClusterResultStatus.Success, new List<ReplicaResult>(),null, ctx.Request));
            };
        }

        [Test]
        public async Task LatencyFullyPerformed_WhenLatencyIsLessThanBudget()
        {
            var latency = TimeSpan.FromSeconds(1);
            var budget = TimeSpan.FromSeconds(2);
            var latencyPerformer = new MockLatencyPerformer();
            var context = Substitute.For<IRequestContext>();
            context.Budget.Remaining.Returns(budget);
            
            var module = new LatencyModule(latencyPerformer, new RateManager(), () => latency, () => 1);

            var result = await module.ExecuteAsync(context, defaultNext);

            nextExecuted.Should().BeTrue();
            result.Status.Should().Be(ClusterResultStatus.Success);
            latencyPerformer.TotalAddedLatency.Should().Be(latency);
        }

        [Test]
        public async Task TimeExpiredReturn_WhenRequestedLatencyLessThanBudget()
        {
            var latency = TimeSpan.FromSeconds(2);
            var budget = TimeSpan.FromSeconds(1);
            var latencyPerformer = new MockLatencyPerformer();
            var context = Substitute.For<IRequestContext>();
            context.Budget.Remaining.Returns(budget);
            
            var module = new LatencyModule(latencyPerformer, new RateManager(), () => latency, () => 1);

            var result = await module.ExecuteAsync(context, defaultNext);

            nextExecuted.Should().BeFalse();
            result.Status.Should().Be(ClusterResultStatus.TimeExpired);
            latencyPerformer.TotalAddedLatency.Should().Be(budget);
            
        }
    }
}