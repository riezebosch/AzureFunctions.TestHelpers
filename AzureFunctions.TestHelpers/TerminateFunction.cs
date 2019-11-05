using System;
using System.Linq;
using System.Threading.Tasks;
using DurableTask.Core;
using Microsoft.Azure.WebJobs;

namespace AzureFunctions.TestHelpers
{
    public static class TerminateFunction
    {
        [FunctionName(nameof(TerminateFunction))]
        public static async Task Run([OrchestrationClient]DurableOrchestrationClientBase client)
        {
            var orchestrations = await client.GetStatusAsync();
            foreach (var orchestration in orchestrations)
            {
                await client.TerminateAsync(orchestration.InstanceId, "just cleaning.");
            }
        }
    }
}