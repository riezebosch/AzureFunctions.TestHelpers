using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace AzureFunctions.TestHelpers
{
    public static class RuntimeStatusExtension
    {
        public static bool IsReady(this OrchestrationRuntimeStatus status) =>
            status != OrchestrationRuntimeStatus.Pending && 
            status != OrchestrationRuntimeStatus.Running &&
            status != OrchestrationRuntimeStatus.ContinuedAsNew;
    }
}