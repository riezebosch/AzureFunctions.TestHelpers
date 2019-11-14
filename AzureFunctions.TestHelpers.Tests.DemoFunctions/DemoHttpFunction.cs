using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace AzureFunctions.TestHelpers
{
    public class DemoHttpFunction
    {
        private readonly IInjectable _injectable;

        public DemoHttpFunction(IInjectable injectable)
        {
            _injectable = injectable;
        }
        
        [FunctionName(nameof(DemoHttpFunction))]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, Route = "demo-injection")]HttpRequestMessage request)
        {
            _injectable.Execute("from an http triggered function");
            return new OkResult();
        }
    }
}