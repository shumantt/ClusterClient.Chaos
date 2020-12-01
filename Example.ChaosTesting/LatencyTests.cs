using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using ClusterClient.Chaos.Configuration;
using ExampleServer;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Transport;
using Vostok.Logging.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.Hosting;
using Vostok.Clusterclient.Core.Model;

namespace Example.ChaosTesting
{
    //NOTE Tests descriptions can be found at ...
    [TestFixture]
    public class LatencyTests
    {
        private Vostok.Clusterclient.Core.ClusterClient client;
        private IHost server;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            server = Program.CreateHostBuilder(new string[0]).Build();
            server.Start();
            client = new Vostok.Clusterclient.Core.ClusterClient(new SilentLog(), configuration =>
            {
                configuration.SetupUniversalTransport(new UniversalTransportSettings
                {
                    AllowAutoRedirect = true
                });
                configuration.DefaultTimeout = TimeSpan.FromSeconds(5);
                configuration.ClusterProvider = new FixedClusterProvider("http://localhost:5000");
                configuration.SetupTotalLatency(() => TimeSpan.FromSeconds(1), () => 0.05);
            });
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            server?.StopAsync().GetAwaiter().GetResult();
        }
        
        //NOTE This test should fail
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