using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace AzureFunctions.TestHelpers
{
    public static class JobHostExtensions
    {
        /// <summary>
        /// Block until all orchestrations are either completed, failed or terminated.
        /// </summary>
        /// <remarks>
        /// This method does NOT throw when orchestrations have failed.
        /// Please, chain the <see cref="ThrowIfFailed" /> and <see cref="Purge"/> to this method for that behavior.
        /// </remarks>
        /// <remarks>
        /// Use <see cref="WaitFor(IJobHost, string, TimeSpan?)"/> when using durable entities or you'll wait forever.
        /// </remarks>
        public static async Task<IJobHost> Ready(this IJobHost jobs, TimeSpan? timeout = null)
        {
            await jobs.CallAsync(nameof(ReadyFunction), new Dictionary<string, object> {["timeout"] = timeout});
            return jobs;
        }
        
        /// <summary>
        /// Block until all orchestrations are either completed, failed or terminated.
        /// </summary>
        /// <remarks>
        /// This method does NOT throw when orchestrations have failed.
        /// Please, chain the <see cref="ThrowIfFailed" /> and <see cref="Purge"/> to this method for that behavior.
        /// </remarks>
        public static async Task<IJobHost> Ready(this Task<IJobHost> task, TimeSpan? timeout = null)
        {
            var jobs = await task;
            return await jobs.Ready(timeout);
        }
        
        /// <summary>
        /// REMARK: This method does NOT throw when orchestrations have failed.
        /// Please, chain the <see cref="ThrowIfFailed" /> and <see cref="Purge"/> to this method for that behavior.
        /// </summary>
        public static async Task<IJobHost> WaitFor(this IJobHost jobs, string orchestration, TimeSpan? timeout = null)
        {
            await jobs.CallAsync(nameof(WaitForFunction), new Dictionary<string, object>
            {
                ["timeout"] = timeout,
                ["name"] = orchestration
            });

            return jobs;
        }
        
        /// <summary>
        /// REMARK: This method does NOT throw when orchestrations have failed.
        /// Please, chain the <see cref="ThrowIfFailed" /> and <see cref="Purge"/> to this method for that behavior.
        /// </summary>
        public static async Task<IJobHost> WaitFor(this Task<IJobHost> task, string orchestration, TimeSpan? timeout = null)
        {
            var jobs = await task;
            return await jobs.WaitFor(orchestration, timeout);
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