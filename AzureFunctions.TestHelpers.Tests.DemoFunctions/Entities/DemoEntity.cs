using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace AzureFunctions.TestHelpers.Entities
{
    public class DemoEntity : IDemoEntity
    {
        private readonly IInjectable _injectable;

        public DemoEntity(IInjectable injectable) => _injectable = injectable;

        [FunctionName(nameof(DemoEntity))]
        public Task Run([EntityTrigger]IDurableEntityContext context) => context.DispatchAsync<DemoEntity>();

        public Task Do() => _injectable.Execute("from an entity");
    }
}