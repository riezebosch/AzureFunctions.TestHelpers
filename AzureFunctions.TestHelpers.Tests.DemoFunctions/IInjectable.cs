using System.Threading.Tasks;

namespace AzureFunctions.TestHelpers
{
    public interface IInjectable
    {
        Task Execute(string message);
    }
}