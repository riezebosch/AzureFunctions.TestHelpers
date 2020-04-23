using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace AzureFunctions.TestHelpers
{
    internal static class DurableOrchestrationClientExtensions
    {
        public static async Task Wait(this IDurableOrchestrationClient client,
            Func<IEnumerable<DurableOrchestrationStatus>, bool> until, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var instances = await client.ListInstancesAsync(new OrchestrationStatusQueryCondition
                {
                    RuntimeStatus = new[] { OrchestrationRuntimeStatus.Pending, OrchestrationRuntimeStatus.Running, OrchestrationRuntimeStatus.ContinuedAsNew }, 
                    TaskHubNames = new[] { client.TaskHubName }
                },  token);
                
                if (until(instances.DurableOrchestrationState))
                {
                    break;
                }

                await Task.Delay(TimeSpan.FromSeconds(5), token);
            }
        }
    }
}