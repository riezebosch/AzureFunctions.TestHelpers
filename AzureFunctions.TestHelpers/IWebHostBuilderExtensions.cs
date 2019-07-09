using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Azure.WebJobs.Script;

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
                [EnvironmentSettingNames.AzureWebJobsScriptRoot] = builder.FindFunctionsFromContentRoot(),
                ["AzureWebJobsStorage"] = azureWebJobsStorage
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