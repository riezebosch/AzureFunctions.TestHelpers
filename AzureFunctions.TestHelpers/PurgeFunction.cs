using System;
using System.Threading.Tasks;
using DurableTask.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace AzureFunctions.TestHelpers
{
    public static class PurgeFunction
    {
        [FunctionName(nameof(PurgeFunction))]
        public static async Task Run([DurableClient]IDurableOrchestrationClient client)
        {
            await client.PurgeInstanceHistoryAsync(
                DateTime.MinValue, 
                null, 
                new []{ OrchestrationStatus.Completed, OrchestrationStatus.Terminated, OrchestrationStatus.Failed });
        }
    }
}