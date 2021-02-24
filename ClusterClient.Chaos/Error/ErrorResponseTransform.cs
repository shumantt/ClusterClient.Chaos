using System;
using ClusterClient.Chaos.Common;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Transforms;

namespace ClusterClient.Chaos.Error
{
    public class ErrorResponseTransform : IResponseTransform
    {
        private readonly RateManager rateManager;
        private readonly Func<double> rateProvider;

        public ErrorResponseTransform(RateManager rateManager, Func<double> rateProvider)
        {
            this.rateManager = rateManager;
            this.rateProvider = rateProvider;
        }
        
        public Response Transform(Response response)
        {
            return rateManager.ShouldInjectWithRate(rateProvider()) 
                ? new Response(ResponseCode.InternalServerError) 
                : response;
        }
    }
}