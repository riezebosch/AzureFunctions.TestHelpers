using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;

namespace AzureFunctions.TestHelpers
{
    public static class WebJobsBuilderExtensions
    {
        public static IWebJobsBuilder ConfigureServices(this IWebJobsBuilder builder, Action<IServiceCollection> configure)
        {
            configure(builder.Services);
            return builder;
        }
    }
}