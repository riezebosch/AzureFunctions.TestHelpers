using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Flurl.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.WebJobs.Script;
using Microsoft.Azure.WebJobs.Script.WebHost;
using Microsoft.Extensions.DependencyInjection;
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
        
        [Fact]
        public void Throws_DirectoryNotFound()
        {
            Assert.Throws<DirectoryNotFoundException>(() => new WebHostBuilder().UseSolutionRelativeAzureWebJobs("asdf"));
        }

        [Fact]
        public void ExistingDirectoryButNoFunctions_Throws_FunctionsNotFound()
        {
            var ex = Assert.Throws<FileNotFoundException>(() => new WebHostBuilder().UseSolutionRelativeAzureWebJobs("AzureFunctions.TestHelpers.Tests"));
            ex.Message.Should().Contain("found beneath");
        }
    }
}
