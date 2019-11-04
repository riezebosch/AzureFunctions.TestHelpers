using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace AzureFunctions.TestHelpers
{
    public static class JobHostExtensions
    {
        public static async Task WaitForOrchestrationsCompletion(this IJobHost jobs, TimeSpan? timeout = null) => 
            await jobs.CallAsync(nameof(WaitForCompletion), new Dictionary<string, object> {["timeout"] = timeout });
    }
}