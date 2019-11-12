using System.Threading.Tasks;
using AzureFunctions.TestHelpers.Activities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace AzureFunctions.TestHelpers.Orchestrators
{
    public static class Orchestration
    {
        [FunctionName(nameof(Orchestration))]
        public static async Task Run([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            await context.CallActivityAsync(nameof(Activity), null);
        }
    }
}