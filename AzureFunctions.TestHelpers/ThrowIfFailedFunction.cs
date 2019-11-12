using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace AzureFunctions.TestHelpers
{
    public static class ThrowIfFailedFunction
    {
        [FunctionName(nameof(ThrowIfFailedFunction))]
        public static async Task Run([DurableClient]IDurableOrchestrationClient client)
        {
            var failed = (await client.GetStatusAsync()).Where(x => x.RuntimeStatus == OrchestrationRuntimeStatus.Failed).ToList();
            if (failed.Any())
            {
                throw new AggregateException(failed.Select(x => new Exception(x.Output.ToString())));
            }
        }
    }
}