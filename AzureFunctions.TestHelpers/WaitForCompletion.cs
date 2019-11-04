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
        public static async Task Run([OrchestrationClient]DurableOrchestrationClientBase client, TimeSpan? timeout)
        {
            using (var cts = new CancellationTokenSource())
            {
                if (timeout != null) cts.CancelAfter(timeout.Value);
                await Wait(client, cts.Token);
            }

            await ThrowIfFailed(client);
            await client.PurgeInstanceHistoryAsync(
                DateTime.MinValue, 
                null, 
                new []{ OrchestrationStatus.Completed });
        }

        private static async Task Wait(DurableOrchestrationClientBase client, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var status = await client.GetStatusAsync(token);
                if (status.All(x => x.RuntimeStatus.IsReady()))
                {
                    break;
                }

                await Task.Delay(TimeSpan.FromSeconds(5), token);
            }
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