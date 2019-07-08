using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Flurl.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Azure.WebJobs.Script;
using Microsoft.Azure.WebJobs.Script.WebHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Xunit;

namespace AzureFunctions.TestHelpers.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async Task Test1()
        {
            SetupEnvironment();

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

        private static void SetupEnvironment()
        {
            var settings = new Dictionary<string, string>
            {
                [EnvironmentSettingNames.AzureWebJobsScriptRoot] = LookupFunctionDirectory().FullName,
                ["AzureWebJobsStorage"] = "UseDevelopmentStorage=true",
                ["AzureWebJobsDashboard"] = "UseDevelopmentStorage=true",
                ["connectionString"] = "UseDevelopmentStorage=true",
                ["FUNCTIONS_WORKER_RUNTIME"] = "dotnet",
                ["WEBSITE_HOSTNAME"] = "localhost:7071",
                ["AZURE_FUNCTIONS_ENVIRONMENT"] = "Development"
            };

            settings.ToList().ForEach(s => Environment.SetEnvironmentVariable(s.Key, s.Value));
        }

        [Fact]
        public void LookupFunctionsDirectoryTest()
        {
            var function = LookupFunctionDirectory();
            function.FullName.Should()
                .MatchRegex(@"\WAzureFunctions\.TestHelpers\WAzureFunctions\.TestHelpers\W");
        }
        
        [Fact]
        public void SearchUpThrowsWhenFileNotFound()
        {
            Assert.Throws<FileNotFoundException>(() => SearchUp(new DirectoryInfo(Environment.CurrentDirectory), "agoaiejgasdf.json", 5));
        }
        
        [Fact]

        private static DirectoryInfo LookupFunctionDirectory()
        {
            return SearchUp(new DirectoryInfo(Environment.CurrentDirectory), "function.json", 5).Directory.Parent;
        }

        private static FileInfo SearchUp(DirectoryInfo directory, string search, int maxLevel)
        {
            for (var i = 0; i <= maxLevel; i++)
            {
                var function = directory.EnumerateFiles(search, SearchOption.AllDirectories).FirstOrDefault();
                if (function != null)
                {
                    return function;
                }

                directory = directory.Parent;
            }

            throw new FileNotFoundException();
        }
    }
}
