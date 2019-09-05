using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DurableTask.Core;
using Microsoft.Azure.WebJobs;

namespace AzureFunctions.TestHelpers
{
    public static class WaitForCompletion
    {
        [FunctionName(nameof(WaitForCompletion))]
        [NoAutomaticTrigger]
        public static async Task Run([OrchestrationClient]DurableOrchestrationClientBase client)
        {
            while (true)
            {
                var status = await client.GetStatusAsync();
                if (status.All(x => x.RuntimeStatus.IsReady()))
                {
                    break;
                }

                Thread.Sleep(TimeSpan.FromSeconds(5));
            }

            await ThrowIfFailed(client);
            await client.PurgeInstanceHistoryAsync(
                DateTime.MinValue, 
                null, 
                new []{ OrchestrationStatus.Completed });
        }

        private static async Task ThrowIfFailed(DurableOrchestrationClientBase client)
        {
            var failed = (await client.GetStatusAsync()).Where(x => x.RuntimeStatus == OrchestrationRuntimeStatus.Failed);
            if (failed.Any())
            {
                throw new AggregateException(failed.Select(x => new Exception(x.Output.ToString())));
            }
        }
    }
}