using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(AzureFunctions.TestHelpers.Startup))]
namespace AzureFunctions.TestHelpers
{
    public static class Demo
    {
        [FunctionName(nameof(Demo))]
        public static void Run([HttpTrigger(AuthorizationLevel.Anonymous, Route = "demo")]HttpRequestMessage request)
        {
        }
        
    }
}