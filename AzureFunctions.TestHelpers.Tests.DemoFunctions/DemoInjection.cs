using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace AzureFunctions.TestHelpers
{
    public class DemoInjection
    {
        private readonly IInjectable _injectable;

        public DemoInjection(IInjectable injectable)
        {
            _injectable = injectable;
        }
        
        [FunctionName(nameof(DemoInjection))]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, Route = "demo-injection")]HttpRequestMessage request)
        {
            _injectable.Execute();
            return new OkResult();
        }
    }
}