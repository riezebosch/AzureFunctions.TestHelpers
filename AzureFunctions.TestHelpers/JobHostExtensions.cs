using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace AzureFunctions.TestHelpers
{
    public static class JobHostExtensions
    {
        public static async Task<IJobHost> Wait(this IJobHost jobs, TimeSpan? timeout = null)
        {
            await jobs.CallAsync(nameof(WaitForCompletion),
                new Dictionary<string, object> {["timeout"] = timeout});

            return jobs;
        }
        
        public static async Task<IJobHost> ThrowIfFailed(this Task<IJobHost> task)
        {
            var jobs = await task;
            await jobs.CallAsync(nameof(ThrowIfFailedFunction));
            
            return jobs;
        }
        
        public static async Task<IJobHost> Purge(this Task<IJobHost> task)
        {
            var jobs = await task;
            await jobs.CallAsync(nameof(PurgeFunction));
            
            return jobs;
        }
    }
}