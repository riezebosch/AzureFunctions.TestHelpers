using FluentAssertions;
using Xunit;

namespace AzureFunctions.TestHelpers.Tests
{
    public class DummyQueryCollectionTests
    {
        [Fact]
        public static void Index()
        {
            var request = new DummyHttpRequest
            {
                Query = new DummyQueryCollection { ["first"] = "Jane" }
            };

            request
                .Query["first"]
                .Should()
                .BeEquivalentTo("Jane");
        }
        
        [Fact]
        public static void Add()
        {
            var request = new DummyHttpRequest
            {
                Query = new DummyQueryCollection { { "first", "Jane" }, { "last", "Doe"} }
            };

            request
                .Query["first"]
                .Should()
                .BeEquivalentTo("Jane");
        }
    }
}