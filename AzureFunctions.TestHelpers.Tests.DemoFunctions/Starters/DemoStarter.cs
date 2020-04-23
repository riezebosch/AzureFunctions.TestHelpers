using System;
using System.Threading.Tasks;
using AzureFunctions.TestHelpers.Orchestrators;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace AzureFunctions.TestHelpers.Starters
{
    public static class DemoStarter
    {
        [FunctionName(nameof(DemoStarter))]
        public static async Task Run([TimerTrigger("0 0 1 * * *")]
            TimerInfo timerInfo,
            [DurableClient] IDurableOrchestrationClient orchestrationClient)
        {
            await orchestrationClient.StartNewAsync(nameof(DemoOrchestration), null, Guid.NewGuid());
        }
    }
}