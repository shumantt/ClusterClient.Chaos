using ClusterClient.Chaos.Common;
using FluentAssertions;
using NUnit.Framework;

namespace ClusterClient.Chaos.Tests.Common
{
    public class RateManagerTests
    {
        private RateManager rateManager;

        [SetUp]
        public void SetUp()
        {
            rateManager = new RateManager();
        }
        
        [Test]
        public void ShouldPerformLatency_Return_False_WhenRateIsZero()
        {
            var actual = rateManager.ShouldInjectWithRate(0);
            actual.Should().BeFalse();
        }
        
        [Test]
        public void ShouldPerformLatency_Return_True_WhenRateIsOne()
        {
            var actual = rateManager.ShouldInjectWithRate(1);
            actual.Should().BeTrue();
        }
    }
}