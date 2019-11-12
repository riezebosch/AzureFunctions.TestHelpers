using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace AzureFunctions.TestHelpers
{
    public static class WaitForCompletion
    {
        [FunctionName(nameof(WaitForCompletion))]
        [NoAutomaticTrigger]
        public static async Task Run([DurableClient]IDurableOrchestrationClient client, TimeSpan? timeout)
        {
            using (var cts = new CancellationTokenSource())
            {
                if (timeout != null) cts.CancelAfter(timeout.Value);
                await Wait(client, cts.Token);
            }
        }

        private static async Task Wait(IDurableOrchestrationClient client, CancellationToken token)
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
    }
}