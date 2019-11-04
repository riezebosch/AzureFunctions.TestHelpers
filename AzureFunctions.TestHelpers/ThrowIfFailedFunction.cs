using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace AzureFunctions.TestHelpers
{
    public static class ThrowIfFailedFunction
    {
        [FunctionName(nameof(ThrowIfFailedFunction))]
        public static async Task Run([OrchestrationClient]DurableOrchestrationClientBase client)
        {
            var failed = (await client.GetStatusAsync()).Where(x => x.RuntimeStatus == OrchestrationRuntimeStatus.Failed).ToList();
            if (failed.Any())
            {
                throw new AggregateException(failed.Select(x => new Exception(x.Output.ToString())));
            }
        }
    }
}