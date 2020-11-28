using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ClusterClient.Chaos.Latency;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Modules;

namespace ClusterClient.Chaos.Tests
{
    public class LatencyModuleTests
    {
        private IRequestContext defaultContext;
        private Func<IRequestContext, Task<ClusterResult>> defaultNext;
        private Stopwatch runNextStopwatch;
        private bool nextExecuted;
        
        
        [SetUp]
        public void Setup()
        {
            defaultContext = Substitute.For<IRequestContext>();
            defaultContext.Budget.Remaining.Returns(TimeSpan.MaxValue);
            defaultContext.CancellationToken.Returns(new CancellationToken());
            
            runNextStopwatch = new Stopwatch();
            nextExecuted = false;
            defaultNext = _ =>
            {
                nextExecuted = true;
                runNextStopwatch.Stop();
                return Task.FromResult<ClusterResult>(null);
            };
        }

        [Test]
        public async Task TimeExpiredReturn_WhenRequestedLatencyLessThanBudget()
        {
            var delay = TimeSpan.FromSeconds(1);
            var budget = TimeSpan.FromSeconds(0.5);
            var module = new LatencyModule(() => delay, () => 1);
            var context = Substitute.For<IRequestContext>();
            context.Budget.Remaining.Returns(budget);
            
            var result = await module.ExecuteAsync(context, defaultNext);

            nextExecuted.Should().BeFalse();
            result.Status.Should().Be(ClusterResultStatus.TimeExpired);
        }
    }
}