using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace AzureFunctions.TestHelpers.Activities
{
    public class Activity
    {
        private readonly IInjectable _injectable;

        public Activity(IInjectable injectable) => _injectable = injectable;

        [FunctionName(nameof(Activity))]
        public void Run([ActivityTrigger]IDurableOrchestrationClient context) => _injectable.Execute("from an activity");
    }
}