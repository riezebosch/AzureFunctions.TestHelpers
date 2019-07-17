using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Flurl.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Script;
using Microsoft.Azure.WebJobs.Script.WebHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Xunit;

namespace AzureFunctions.TestHelpers.Tests
{
    public class WebHostBuilderExtensionsTests
    {
        [Fact]
        public async Task HostDemoFunctionsAreAccessibleFromClient()
        {
            using (var host = Microsoft.Azure.WebJobs.Script.WebHost.Program.CreateWebHostBuilder()
                .UseUrls("http://localhost:7071")
                .UseSolutionRelativeAzureWebJobs("AzureFunctions.TestHelpers.Tests.DemoFunctions")
                .Build())
            {
                await host.StartAsync();
                var hostService = host.Services.GetRequiredService<WebJobsScriptHostService>();

                await hostService.DelayUntilHostReady();

                var scriptHost = hostService.Services.GetRequiredService<IScriptJobHost>();
                scriptHost.Functions.Should().NotBeEmpty();

                await "http://localhost:7071/api/demo".GetAsync();
            }
        }

        [Fact(Skip = "not working because assemblies loaded from different location.")]
        public async Task IntegrationTest()
        {
            var mock = Substitute.For<IInjectable>();

            using (var host = Microsoft.Azure.WebJobs.Script.WebHost.Program.CreateWebHostBuilder()
                .UseUrls("http://localhost:7071")
                .UseSolutionRelativeAzureWebJobs("AzureFunctions.TestHelpers.Tests.DemoFunctions")
                .ConfigureServices(services => services.AddSingleton<IConfigureBuilder<IWebJobsBuilder>>(new ConfigureTestServices(s => s.Replace(ServiceDescriptor.Singleton(mock)))))
                .Build())
            {
                await host.StartAsync();
                var hostService = host.Services.GetRequiredService<WebJobsScriptHostService>();

                var ready = await hostService.DelayUntilHostReady();
                ready.Should().BeTrue();

                var scriptHost = hostService.Services.GetRequiredService<IScriptJobHost>();
                scriptHost.Functions.Should().NotBeEmpty();

                await "http://localhost:7071/api/demo-injection".GetAsync();
                mock.Received().Execute();
            }
        }

        [Fact]
        public void Throws_DirectoryNotFound()
        {
            Assert.Throws<DirectoryNotFoundException>(
                () => new WebHostBuilder().UseSolutionRelativeAzureWebJobs("asdf"));
        }

        [Fact]
        public void ExistingDirectoryButNoFunctions_Throws_FunctionsNotFound()
        {
            var ex = Assert.Throws<FileNotFoundException>(() =>
                new WebHostBuilder().UseSolutionRelativeAzureWebJobs("AzureFunctions.TestHelpers.Tests"));
            ex.Message.Should().Contain("found beneath");
        }
    }

    public class ConfigureTestServices : IConfigureBuilder<IWebJobsBuilder>
    {
        private readonly Action<IServiceCollection> _func;

        public ConfigureTestServices(Action<IServiceCollection> func)
        {
            _func = func;
        }

        public void Configure(IWebJobsBuilder builder)
        {
            _func(builder.Services);
        }
    }
}
 
