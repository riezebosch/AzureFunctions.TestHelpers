using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureFunctions.TestHelpers.Orchestrators;
using AzureFunctions.TestHelpers.Starters;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace AzureFunctions.TestHelpers.Tests;

public class UsingAzurite : IClassFixture<AzuriteContainer>
{
    private readonly AzuriteContainer _container;

    public UsingAzurite(AzuriteContainer container) => _container = container;

    [Fact]
    public async Task Do()
    {
        Environment.SetEnvironmentVariable("AzureWebJobsStorage", "UseDevelopmentStorage=true");
        using var host = new HostBuilder()
            .ConfigureWebJobs(builder => builder
                .AddDurableTask(options => options.LocalRpcEndpointEnabled = false)
                .AddAzureStorageCoreServices()
                .UseWebJobsStartup<Startup>())
            .Build();

        await host.StartAsync();
        
        var jobs = host.Services.GetRequiredService<IJobHost>();
        await jobs
            .Terminate()
            .Purge();
        
        await jobs.CallAsync(nameof(DemoStarter), new Dictionary<string, object>
        {
            ["timerInfo"] = new TimerInfo(new WeeklySchedule(), new ScheduleStatus())
        });
        
        await jobs.WaitFor(nameof(DemoOrchestration));
    }
}