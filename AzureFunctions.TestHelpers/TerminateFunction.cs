using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace AzureFunctions.TestHelpers
{
    public static class TerminateFunction
    {
        [FunctionName(nameof(TerminateFunction))]
        public static async Task Run([DurableClient]IDurableOrchestrationClient client)
        {
            var orchestrations = await client.GetStatusAsync();
            foreach (var orchestration in orchestrations)
            {
                await client.TerminateAsync(orchestration.InstanceId, "just cleaning.");
            }
        }
    }
}