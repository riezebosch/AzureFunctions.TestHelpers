[![build](https://ci.appveyor.com/api/projects/status/ee41yv4jpp40xj7d?svg=true)](https://ci.appveyor.com/project/riezebosch/azurefunctions-testhelpers/branch/master)
[![nuget](https://img.shields.io/nuget/v/AzureFunctions.TestHelpers.svg)](https://www.nuget.org/packages/AzureFunctions.TestHelpers/)

# AzureFunctions.TestHelpers ⚡

Host and invoke Azure Functions from a test by combining the bits and pieces of
the [WebJobs SDK](https://docs.microsoft.com/en-us/azure/app-service/webjobs-sdk-how-to),
[Azure Functions](https://docs.microsoft.com/en-us/azure/azure-functions/functions-overview)
and [Durable Functions](https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-concepts)
and adding some convenience classes and extension methods on top.

## HTTP Additions

### `DummyHttpRequest`

Because you can't invoke an HTTP-triggered function without a request.

## WebJobsBuilder Extensions

### `builder.ConfigureServices(services => ...))`

Register and replace services that are injected into your functions.

### `builder.AddDurableTaskInTestHub()`

Add and configure using [the durable task extensions](https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-webjobs-sdk#webjobs-sdk-3x) and
use a random generated hub name to start with a clean history.

## JobHost Extensions

### `jobs.WaitForOrchestrationsCompletion()`

Invoke function to monitor orchestrations status and wait for all to complete and throw if any one failed.

## Examples

Invoke a regular http triggered function:

```c#
[Fact]
public static async Task HttpTriggeredFunctionWithDependencyReplacement()
{
    // Arrange
    var mock = Substitute.For<IInjectable>();
    using (var host = new HostBuilder()
        .ConfigureWebJobs(builder => builder
            .AddHttp()
            .UseWebJobsStartup<Startup>()
            .ConfigureServices(services => services.Replace(ServiceDescriptor.Singleton(mock))))
        .Build())
    {
        await host.StartAsync();
        var jobs = host.Services.GetService<IJobHost>();

        // Act
        await jobs.CallAsync(nameof(DemoInjection), new Dictionary<string, object>
        {
            ["request"] = new DummyHttpRequest()
        });

        // Assert
        mock
            .Received()
            .Execute();
    }
}
```

Invoke a time-triggered durable function:

```c#
[Fact]
public static async Task DurableFunction()
{
    // Arrange
    var mock = Substitute.For<IInjectable>();
    using (var host = new HostBuilder()
        .ConfigureWebJobs(builder => builder
            .AddTimers()
            .AddDurableTaskInTestHub()
            .AddAzureStorageCoreServices()
            .UseWebJobsStartup<Startup>()
            .ConfigureServices(services => services.Replace(ServiceDescriptor.Singleton(mock))))
        .Build())
    {
        await host.StartAsync();
        var jobs = host.Services.GetService<IJobHost>();

        // Act
        await jobs.CallAsync(nameof(Starter), new Dictionary<string, object>
        {
            ["timerInfo"] = new TimerInfo(new WeeklySchedule(), new ScheduleStatus())
        });

        await jobs.WaitForOrchestrationsCompletion();

        // Assert
        mock
            .Received()
            .Execute();
    }
}
```

## Dependency Injection

Include `Microsoft.Azure.Functions.Extensions` in your test project to [enable dependency injection](https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection)!

## Azure Storage Account

You need an azure storage table to store the state of the durable functions.

### Azure

Just copy the [connection string from your storage account](https://docs.microsoft.com/en-us/azure/storage/common/storage-configure-connection-string#view-and-copy-a-connection-string),
works everywhere.

### Azure Storage Emulator

Set the connection string to `UseDevelopmentStorage=true`. Unfortunately works only on Windows. See this [blog](https://zimmergren.net/azure-devops-unit-tests-storage-emulator-hosted-agent/)
on how to enable the storage emulator in a azure devops pipeline.

### Azurite

Unfortunately, `azurite@2` doesn't work with the current version of durable functions,
and `azurite@3` doesn't have the [required features (implemented yet)](https://github.com/Azure/Azurite#azurite-v3).

## Host connection strings

The storage connection string setting [is required](https://docs.microsoft.com/en-us/azure/app-service/webjobs-sdk-how-to#host-connection-strings).

### Option 1:

Set the environment variable `AzureWebJobsStorage`. Hereby you can also overwrite the configured connection from option 2 on your local dev machine.

### Option 2:

Include an `appsettings.json` in your test project:

```json
{
  "AzureWebJobsStorage": "DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...==;EndpointSuffix=core.windows.net"
}
```

```xml
<ItemGroup>
<None Include="appsettings.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
</None>
</ItemGroup>
```
