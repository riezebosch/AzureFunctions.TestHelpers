using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Xunit;

namespace AzureFunctions.TestHelpers.Tests
{
    public static class HostBuilderTests
    {
        [Fact]
        public static async Task HttpTriggeredFunctionWithDependencyReplacement()
        {
            // Arrange
            object response = null;
            var mock = Substitute.For<IInjectable>();

            using var host = new HostBuilder()
                .ConfigureWebJobs(builder => builder
                    .AddHttp(options => options.SetResponse = (request, o) => response = o)
                    .AddDurableTask()
                    .UseWebJobsStartup<Startup>()
                    .ConfigureServices(services => services.Replace(ServiceDescriptor.Singleton(mock))))
                .Build();
            await host.StartAsync();
            var jobs = host.Services.GetService<IJobHost>();
                
            // Act
            await jobs.CallAsync(nameof(DemoHttpFunction), new Dictionary<string, object>
            {
                ["request"] = new DummyHttpRequest()
            });
                
            // Assert
            await mock
                .Received()
                .Execute("from an http triggered function");

            response
                .Should()
                .BeOfType<OkResult>();
        }
    }
}