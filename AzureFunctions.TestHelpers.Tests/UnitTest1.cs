using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public class UnitTest1
    {
        [Fact]
        public async Task Test1()
        {
            var settings = new Dictionary<string, string>
            {
                [EnvironmentSettingNames.AzureWebJobsScriptRoot] = Path.GetFullPath("../../../../AzureFunctions.TestHelpers/bin/Debug/netstandard2.0"),
                ["AzureWebJobsStorage"] = "UseDevelopmentStorage=true",
                ["AzureWebJobsDashboard"] = "UseDevelopmentStorage=true",
                ["connectionString"] = "UseDevelopmentStorage=true",
                ["FUNCTIONS_WORKER_RUNTIME"] = "dotnet",
                ["WEBSITE_HOSTNAME"] = "localhost:7071",
                ["AZURE_FUNCTIONS_ENVIRONMENT"] = "Development"
            };
            
            settings.ToList().ForEach(s => Environment.SetEnvironmentVariable(s.Key, s.Value));
            
            using (var host = Microsoft.Azure.WebJobs.Script.WebHost.Program.CreateWebHostBuilder()
                .UseUrls("http://localhost:7071")
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
    }
}
