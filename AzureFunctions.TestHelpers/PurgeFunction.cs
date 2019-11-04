using System;
using System.Linq;
using System.Threading.Tasks;
using DurableTask.Core;
using Microsoft.Azure.WebJobs;

namespace AzureFunctions.TestHelpers
{
    public static class PurgeFunction
    {
        [FunctionName(nameof(PurgeFunction))]
        public static async Task Run([OrchestrationClient]DurableOrchestrationClientBase client)
        {
            await client.PurgeInstanceHistoryAsync(
                DateTime.MinValue, 
                null, 
                new []{ OrchestrationStatus.Completed, OrchestrationStatus.Terminated, OrchestrationStatus.Failed });
        }
    }
}