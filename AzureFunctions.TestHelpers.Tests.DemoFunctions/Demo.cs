using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace AzureFunctions.TestHelpers
{
    public static class Demo
    {
        [FunctionName(nameof(DemoFunc))]
        public static void DemoFunc([HttpTrigger(AuthorizationLevel.Anonymous, Route = "demo")]HttpRequestMessage request)
        {
        }
    }
}