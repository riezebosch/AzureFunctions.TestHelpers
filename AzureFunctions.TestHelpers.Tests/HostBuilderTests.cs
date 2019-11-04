using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
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
            
            using (var host = new HostBuilder()
                .ConfigureWebJobs(builder => builder
                    .AddHttp(options => options.SetResponse = (request, o) => response = o)
                    .UseWebJobsStartup<Startup>()
                    .ConfigureServices(services => services.Replace(ServiceDescriptor.Singleton(mock))))
                .Build())
            {
                await host.StartAsync();
                var jobs = host.Services.GetService<IJobHost>();
                
                // Act
                await jobs.CallAsync(nameof(DemoInjection), new Dictionary<string, object>
                {
                    ["request"] = new DummyHttpRequest()
                });
                
                // Assert
                mock
                    .Received()
                    .Execute();

                response
                    .Should()
                    .BeOfType<OkResult>();
            }
        }
    }
}