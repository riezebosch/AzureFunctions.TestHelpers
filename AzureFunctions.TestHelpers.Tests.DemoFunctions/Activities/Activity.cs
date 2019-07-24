using Microsoft.Azure.WebJobs;

namespace AzureFunctions.TestHelpers.Activities
{
    public class Activity
    {
        private readonly IInjectable _injectable;

        public Activity(IInjectable injectable)
        {
            _injectable = injectable;
        }

        [FunctionName(nameof(Activity))]
        public void Run([ActivityTrigger]DurableActivityContextBase context) => _injectable.Execute();
    }
}