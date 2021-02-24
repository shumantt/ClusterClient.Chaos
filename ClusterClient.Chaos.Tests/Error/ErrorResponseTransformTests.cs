using ClusterClient.Chaos.Common;
using ClusterClient.Chaos.Error;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;

namespace ClusterClient.Chaos.Tests.Error
{
    public class ErrorResponseTransformTests
    {
        [Test]
        public void ResponseIsNotChanged_WhenInjectionNotPerformed()
        {
            var errorResponseTransform = new ErrorResponseTransform(new RateManager(), () => 0);
            var originalResponse = new Response(ResponseCode.Ok);

            var actual = errorResponseTransform.Transform(originalResponse);

            actual.Should().Be(originalResponse);
        }

        [Test]
        public void ResponseIsServerError_WhenInjectionPerformed()
        {
            var errorResponseTransform = new ErrorResponseTransform(new RateManager(), () => 1);
            var originalResponse = new Response(ResponseCode.Ok);

            var actual = errorResponseTransform.Transform(originalResponse);

            actual.Should().BeEquivalentTo(new Response(ResponseCode.InternalServerError));
        }
    }
}