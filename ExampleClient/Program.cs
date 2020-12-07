using System;
using System.Text.Json;
using ClusterClient.Chaos.Configuration;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Transport;

namespace ExampleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            SendWithAddedTotalLatency();
            Console.WriteLine("----------------------------");
        }

        private static void SendWithAddedTotalLatency()
        {
            Console.WriteLine("Sending with total added latency");
            SendAndLog(c => c.SetupTotalLatency(() => TimeSpan.FromSeconds(1), () => 0.5));
            Console.WriteLine("Finished with total added latency");
        }

        private static void SendAndLog(Action<IClusterClientConfiguration> configureClient)
        {
            var client = new Vostok.Clusterclient.Core.ClusterClient(null, configuration =>
            {
                configuration.SetupUniversalTransport(new UniversalTransportSettings
                {
                    AllowAutoRedirect = true
                });
                configuration.ClusterProvider = new FixedClusterProvider(new Uri("http://localhost:5000"));
                configureClient(configuration);
            });

            for (int i = 0; i < 10; i++)
            {
                var response = client.Send(
                    Request
                        .Post("example/measure")
                        .WithAdditionalQueryParameter("success", true)
                        .WithContentTypeHeader("application/json")
                        .WithContent(JsonSerializer.Serialize(DateTime.Now))
                );
                Console.WriteLine($"Response: {response.Response.Content}");
            }
        }
    }
}