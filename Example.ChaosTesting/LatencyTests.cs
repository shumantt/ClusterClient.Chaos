using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using ClusterClient.Chaos.Configuration;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Transport;
using Vostok.Logging.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.Hosting;
using Vostok.Clusterclient.Core.Model;
using Server = ExampleServer.Program;

namespace Example.ChaosTesting
{
    /*
        This test fixture illustrates how chaos testing can show you the problems in your application 
        if random latencies happen in http requests. Which is pretty common in a real world.
        Let's consider our client app (e.g. backend web-app) has to make 3 requests 
        to different endpoints (e.g. backend microservices) to gather required information
        Our usability specialist made a statement, that it is ok for the user if the method for gathering 
        information works no longer than 1 second. 
        And usually it does but sometimes in periods of high load on services extra latency added.
        To simulate these situations and prepare our codebase 
        we can inject extra latency for requests using ClusterClient.Chaos lib.
        See comments next to test methods for further details.
    */
    
    // NOTE The tests rely on some randomness, so in very rare cases results can differ from expected, so they should not be a part of your usual CI/CD pipeline
    // NOTE (in this implementation at least). It is just the usage example of chaos testing with ClusterClient.Chaos and Vostok.ClusterClient libraries.
    [TestFixture]
    public class LatencyTests
    {
        private Vostok.Clusterclient.Core.ClusterClient client;
        private IHost server;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            server = Server.CreateHostBuilder(new string[0]).Build();
            server.Start();
            client = new Vostok.Clusterclient.Core.ClusterClient(new SilentLog(), configuration =>
            {
                configuration.SetupUniversalTransport(new UniversalTransportSettings
                {
                    AllowAutoRedirect = true
                });
                configuration.DefaultTimeout = TimeSpan.FromSeconds(5);
                configuration.ClusterProvider = new FixedClusterProvider("http://localhost:5000");
                configuration.InjectTotalLatency(() => TimeSpan.FromSeconds(1), () => 0.05);
            });
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            server?.StopAsync().GetAwaiter().GetResult();
        }
        
        //NOTE This test should fail
        /*
            Initially we built our method to gather the info request by request sequentially.
            No way the method can make it in timeout if extra latency happens.
         */
        [Repeat(20)]
        [Test]
        public async Task TestBuildInfoSequentially()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var info = await BuildInfoSequentially();
            stopwatch.Stop();

            Console.WriteLine(stopwatch.Elapsed);
            stopwatch.Elapsed.Should().BeLessOrEqualTo(TimeSpan.FromSeconds(1));
            info.Should().NotBeEmpty();
        }
        
        //NOTE This test should fail
        /*
            Let's try to improve our building method and gather info in parallel. The method execution time is equal
            to the worst time of the subtask. Again, if any random extra latency happens - the method execution is timed out.
        */
        [Repeat(20)]
        [Test]
        public async Task TestBuildInfoInParallel()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var info = await BuildInfoInParallel();
            stopwatch.Stop();

            Console.WriteLine(stopwatch.Elapsed);
            stopwatch.Elapsed.Should().BeLessOrEqualTo(TimeSpan.FromSeconds(1));
            info.Should().NotBeEmpty();
        }
        
        //NOTE This test should pass
        /*
            In order to meet the required time of the method execution, the method should not wait for the longest subtask
            but has to cancel it after the specified timeout.
            This way, our method will meet required execution time condition but without some trade-off. 
            We'll loose some info in a method result.But in practice it is often better for the service accessibility 
            and such trade-off provides better user experience if it is used for UI-depended actions. 
            It is not always the case though. 
        */
        [Repeat(20)]
        [Test]
        public async Task TestBuildInfoWithConfiguredTimeouts()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var info = await BuildInfoWithConfiguredTimeouts();
            stopwatch.Stop();

            Console.WriteLine(stopwatch.Elapsed);
            stopwatch.Elapsed.Should().BeLessOrEqualTo(TimeSpan.FromSeconds(1));
            info.Should().NotBeEmpty();
        }

        private async Task<List<string>> BuildInfoSequentially()
        {
            var infos = new List<string>();
            var nameResult = await client.SendAsync(Request.Get("/name"));
            
            if (nameResult.Status == ClusterResultStatus.Success)
            {
                infos.Add(nameResult.Response.Content.ToString());
            }

            var ageResult = await client.SendAsync(Request.Get("age"));
           
            if (ageResult.Status == ClusterResultStatus.Success)
            {
                infos.Add(ageResult.Response.Content.ToString());
            }
            
            var cityResult = await client.SendAsync(Request.Get("city"));
           
            if (cityResult.Status == ClusterResultStatus.Success)
            {
                infos.Add(cityResult.Response.Content.ToString());
            }

            return infos;
        }

        private async Task<List<string>> BuildInfoInParallel()
        {
            var infos = new List<string>();
            var nameTask = client.SendAsync(Request.Get("name"));
            var ageTask = client.SendAsync(Request.Get("age"));
            var cityTask = client.SendAsync(Request.Get("city"));
            await Task.WhenAll(nameTask, ageTask, cityTask);
            
            if (nameTask.Result.Status == ClusterResultStatus.Success)
            {
                infos.Add(nameTask.Result.Response.Content.ToString());
            }
            
            if (ageTask.Result.Status == ClusterResultStatus.Success)
            {
                infos.Add(ageTask.Result.Response.Content.ToString());
            }
            
            if (cityTask.Result.Status == ClusterResultStatus.Success)
            {
                infos.Add(cityTask.Result.Response.Content.ToString());
            }

            return infos;
        }

        private async Task<List<string>> BuildInfoWithConfiguredTimeouts()
        {
            var infoBuildTimeout = TimeSpan.FromSeconds(0.9);
            var infos = new List<string>();
            var nameTask = client.SendAsync(Request.Get("name"), timeout: infoBuildTimeout);
            var ageTask = client.SendAsync(Request.Get("age"), timeout: infoBuildTimeout);
            var cityTask = client.SendAsync(Request.Get("city"), timeout: infoBuildTimeout);
            await Task.WhenAll(nameTask, ageTask, cityTask);
            
            if (nameTask.Result.Status == ClusterResultStatus.Success)
            {
                infos.Add(nameTask.Result.Response.Content.ToString());
            }
            
            if (ageTask.Result.Status == ClusterResultStatus.Success)
            {
                infos.Add(ageTask.Result.Response.Content.ToString());
            }
            
            if (cityTask.Result.Status == ClusterResultStatus.Success)
            {
                infos.Add(cityTask.Result.Response.Content.ToString());
            }

            return infos;
        }
    }
}