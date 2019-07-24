using System.Threading.Tasks;
using AzureFunctions.TestHelpers.Activities;
using Microsoft.Azure.WebJobs;

namespace AzureFunctions.TestHelpers.Orchestrators
{
    public static class Orchestration
    {
        [FunctionName(nameof(Orchestration))]
        public static async Task Run([OrchestrationTrigger] DurableOrchestrationContextBase context)
        {
            await context.CallActivityAsync(nameof(Activity), null);
        }
    }
}