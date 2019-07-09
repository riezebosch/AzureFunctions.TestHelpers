using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace AzureFunctions.TestHelpers
{
    public static class IWebHostBuilderExtensions
    {
        public static IWebHostBuilder UseSolutionRelativeAzureWebJobs(
            this IWebHostBuilder builder,
            string solutionRelativePath, 
            string azureWebJobsStorage = "UseDevelopmentStorage=true")
        {
            builder.UseSolutionRelativeContentRoot(solutionRelativePath);
            UpdateEnvironmentVariables(new Dictionary<string, string>
            {
                ["AzureWebJobsScriptRoot"] = builder.FindFunctionsFromContentRoot(),
                ["AzureWebJobsStorage"] = azureWebJobsStorage,
                
                // Required for this: https://github.com/Azure/azure-functions-host/blob/acca4b6f6a800218876c4c7692189268ca161987/src/WebJobs.Script/Rpc/Configuration/WorkerConfigFactory.cs#L30
                ["languageWorkers:workersDirectory"] = Environment.CurrentDirectory
            });
            
            return builder;
        }

        private static string FindFunctionsFromContentRoot(this IWebHostBuilder builder)
        {
            var root = new DirectoryInfo(builder.GetSetting(WebHostDefaults.ContentRootKey));
            var function = root
                               .EnumerateFiles("function.json", SearchOption.AllDirectories)
                               .FirstOrDefault() ?? throw new FileNotFoundException($"No function.json found beneath {root.FullName}.");
            
            return function.Directory.Parent.FullName;
        }

        private static void UpdateEnvironmentVariables(Dictionary<string, string> settings)
        {
            settings
                .ToList()
                .ForEach(s => Environment.SetEnvironmentVariable(s.Key, s.Value));
        }
    }
}