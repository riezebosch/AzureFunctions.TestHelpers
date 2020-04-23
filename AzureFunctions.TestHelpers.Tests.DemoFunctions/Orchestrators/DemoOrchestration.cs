using System;
using System.Threading.Tasks;
using AzureFunctions.TestHelpers.Activities;
using AzureFunctions.TestHelpers.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace AzureFunctions.TestHelpers.Orchestrators
{
    public static class DemoOrchestration
    {
        [FunctionName(nameof(DemoOrchestration))]
        public static async Task Run([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var proxy = context.CreateEntityProxy<IDemoEntity>(new EntityId(nameof(DemoEntity), context.GetInput<Guid>().ToString()));
            await proxy.Do();

            await context.CallActivityAsync(nameof(Activity), null);
        }
    }
}