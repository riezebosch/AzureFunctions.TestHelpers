using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace AzureFunctions.TestHelpers
{
    public static class JobHostExtensions
    {
        /// <summary>
        /// REMARK: This method does NOT throw when orchestrations have failed.
        /// Please, chain the <see cref="ThrowIfFailed" /> and <see cref="Purge"/> to this method for that behavior.
        /// </summary>
        public static async Task<IJobHost> Ready(this IJobHost jobs, TimeSpan? timeout = null)
        {
            await jobs.CallAsync(nameof(WaitForCompletion),
                new Dictionary<string, object> {["timeout"] = timeout});

            return jobs;
        }
        
        /// <summary>
        /// Query the status of all orchestrations in current hub and throw an exception if any one failed. 
        /// </summary>
        public static async Task<IJobHost> ThrowIfFailed(this Task<IJobHost> task)
        {
            var jobs = await task;
            await jobs.CallAsync(nameof(ThrowIfFailedFunction));
            
            return jobs;
        }
        
        /// <summary>
        /// Purge the history of all (completed, failed and terminated) orchestrations. 
        /// </summary>
        public static async Task<IJobHost> Purge(this Task<IJobHost> task)
        {
            var jobs = await task;
            await Task.Delay(TimeSpan.FromSeconds(1));
            await jobs.CallAsync(nameof(PurgeFunction));
            
            return jobs;
        }
        
        /// <summary>
        /// Terminates all orchestrations. 
        /// </summary>
        public static async Task<IJobHost> Terminate(this IJobHost jobs)
        {
            await jobs.CallAsync(nameof(TerminateFunction));
            return jobs;
        }
    }
}