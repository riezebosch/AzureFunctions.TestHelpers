using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace AzureFunctions.TestHelpers
{
    public static class ThrowIfFailedFunction
    {
        [FunctionName(nameof(ThrowIfFailedFunction))]
        [NoAutomaticTrigger]
        public static async Task Run([DurableClient]IDurableOrchestrationClient client)
        {
            var failed = await client.ListInstancesAsync(new OrchestrationStatusQueryCondition{ RuntimeStatus = new[] { OrchestrationRuntimeStatus.Failed } }, CancellationToken.None);
            if (failed.DurableOrchestrationState.Any())
            {
                throw new AggregateException(failed.DurableOrchestrationState.Select(x => new Exception(x.Output.ToString())));
            }
        }
    }
}