[![Build status](https://ci.appveyor.com/api/projects/status/ee41yv4jpp40xj7d/branch/master?svg=true)](https://ci.appveyor.com/project/riezebosch/azurefunctions-testhelpers/branch/master)
[![nuget](https://img.shields.io/nuget/v/AzureFunctions.TestHelpers.svg)](https://www.nuget.org/packages/AzureFunctions.TestHelpers/)

# AzureFunctions.TestHelpers ⚡

Test your Azure Functions! Spin up integration tests. By combining bits and pieces of
the [WebJobs SDK](https://docs.microsoft.com/en-us/azure/app-service/webjobs-sdk-how-to),
[Azure Functions](https://docs.microsoft.com/en-us/azure/azure-functions/functions-overview)
and [Durable Functions](https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-concepts)
and adding some convenience classes and extension methods on top.

You'll ❤ the feedback!

## Updates

* v3.3: Allow to pass a retry delay on Wait and Ready methods
* v3.2: Updated dependencies, Ready also ignored durable entities
* v3.1: [WaitFor](#waitfor) to better support durable entities
* v3.0: Upgrade to durable task v2
* v2.1: Removed AddDurableTaskInTestHub
* v2.0: Wait, ThrowIfFailed and Purge separated.

## Configure Services for Dependency Injection

I just found out the default `ConfigureServices` on the `HostBuilder` also works.
But if it makes more sense to you to configure services on the `WebJobsBuilder` since
you also configure the `Startup` there you can use:

```c#
mock = Substitute.For<IInjectable>();
host = new HostBuilder()
    .ConfigureWebJobs(builder => builder
        .UseWebJobsStartup<Startup>()
        .ConfigureServices(services => services.Replace(ServiceDescriptor.Singleton(mock))))
    .Build();
```

Register and replace services that are injected into your functions.
Include `Microsoft.Azure.Functions.Extensions` in your test project to [enable dependency injection](https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection)!

*Note:* Not sure if this is still a requirement for `Azure Functions >= v2.0`.

## HTTP Triggered Functions

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
            .ConfigureServices(services => services.AddSingleton(mock)))
        .Build())
    {
        await host.StartAsync();
        var jobs = host.Services.GetService<IJobHost>();

        // Act
        await jobs.CallAsync(nameof(DemoHttpFunction), new Dictionary<string, object>
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

### HTTP Request

Because you can't invoke an HTTP-triggered function without a request, and I couldn't find one
in the standard libraries, I created the `DummyHttpRequest`.

```c#
await jobs.CallAsync(nameof(DemoInjection), new Dictionary<string, object>
{
    ["request"] = new DummyHttpRequest("{ \"some-key\": \"some value\" }")
});
```
*New*: Now you can set string content via the constructor overload! 

You can set all kinds of regular settings on the request when needed:

```c#
var request = new DummyHttpRequest
{
    Scheme = "http",
    Host = new HostString("some-other"),
    Headers = {
        ["Authorization"] = $"Bearer {token}",
        ["Content-Type"] =  "application/json"
    }
};
```

_New_: Now you can use a DummyQueryCollection to mock the url query:

```c#
var request = new DummyHttpRequest
{
    Query = new DummyQueryCollection
    {
        ["firstname"] = "Jane",
        ["lastname"] = "Doe"
    }
};
```

### HTTP Response

To capture the result(s) of http-triggered functions you use the `options.SetResponse` 
callback on the `AddHttp` extension method:

```c#
// Arrange
var hypothesis = Hypothesis.For<object>()
    .Any(o => o is OkResult);

using (var host = new HostBuilder()
    .ConfigureWebJobs(builder => builder
        .AddHttp(options => options.SetResponse = async (_, o) => await hypothesis.Test(o)))
    .Build())
{
    await host.StartAsync();
    var jobs = host.Services.GetService<IJobHost>();

    // Act
    await jobs.CallAsync(nameof(DemoHttpFunction), new Dictionary<string, object>
    {
        ["request"] = new DummyHttpRequest()
    });
}

// Assert
await hypothesis.Validate(10.Seconds());
```

I'm using [Hypothesist](https://github.com/riezebosch/hypothesist) for easy async testing.

## Durable Functions

Invoke a (time-triggered) durable function:

```c#
[Fact]
public static async Task DurableFunction()
{
    // Arrange
    var mock = Substitute.For<IInjectable>();
    using (var host = new HostBuilder()
        .ConfigureWebJobs(builder => builder
            .AddDurableTask(options => options.HubName = nameof(MyTestFunction))
            .AddAzureStorageCoreServices()
            .ConfigureServices(services => services.AddSingleton(mock)))
        .Build())
    {
        await host.StartAsync();
        var jobs = host.Services.GetService<IJobHost>();
        await jobs.
            Terminate()
            .Purge();

        // Act
        await jobs.CallAsync(nameof(DemoStarter), new Dictionary<string, object>
        {
            ["timerInfo"] = new TimerInfo(new WeeklySchedule(), new ScheduleStatus())
        });

        await jobs
            .Ready()
            .ThrowIfFailed()
            .Purge();

        // Assert
        mock
            .Received()
            .Execute();
    }
}
```

You'll have to [configure Azure WebJobs Storage](#azure-storage-account) to run durable functions!

### Time Triggered Functions

Do NOT add _timers_ to the web jobs host!

 ```c#
using (var host = new HostBuilder()
        .ConfigureWebJobs(builder => builder
            //.AddTimers() <-- DON'T ADD TIMERS
            .AddDurableTask(options => options.HubName = nameof(MyTestFunction))
            .AddAzureStorageCoreServices()
            .ConfigureServices(services => services.AddSingleton(mock)))
        .Build())
    {
    }
}
```

It turns out it is not required to invoke time-triggered functions, and by doing so 
your functions will be triggered randomly, messing up the status of your orchestration instances.

### Isolate Durable Functions

Add and configure Durable Functions using [the durable task extensions](https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-webjobs-sdk#webjobs-sdk-3x) and
use a specific hub name to [isolate from other parallel tests](https://docs.microsoft.com/nl-nl/azure/azure-functions/durable/durable-functions-task-hubs).

```c#
host = new HostBuilder()
    .ConfigureWebJobs(builder => builder
        .AddDurableTask(options => options.HubName = nameof(MyTestFunction))
        .AddAzureStorageCoreServices()
    .Build();
```

*BREAKING:* In `v2.1` I removed the `AddDurableTaskInTestHub()` method. You can easily do it yourself with
 `AddDurableTask(options => ...)` and be more specific about the context of your test. This way, you don't 
 end up with hundreds of empty history and instance tables in your storage account.

### Cleanup

```c#
await jobs
    .Terminate()
    .Purge();
```

To cleanup from previous runs, you terminate leftover orchestrations
and durable entities and purge the history.

### WaitFor

```c#
await jobs
    .WaitFor(nameof(DemoOrchestration), TimeSpan.FromSeconds(30))
    .ThrowIfFailed();
```

With the `WaitFor` you specify what orchestration you want to wait for. 
You can either use the [`Ready`](#ready) function if you just want all orchestrations to complete.

### Ready

```c#
await jobs
    .Ready(TimeSpan.FromSeconds(30))
    .ThrowIfFailed();
```

The `Ready` function is handy if you want to wait for termination.

*BREAKING:* In `v2` the `WaitForOrchestrationsCompletion` is broken down into `Wait()`, `ThrowIfFailed()` and `Purge()`.

### Reuse

When injecting a configured host into your test, make sure **you do NOT initialize nor clean it
in the constructor**. For example, when using `xUnit` you use the [`IAsyncLifetime`](https://github.com/xunit/xunit/blob/master/src/xunit.core/IAsyncLifetime.cs)
for that, otherwise your test will probably hang forever.

Initialize and start the host in a fixture:

```c#
public class HostFixture : IDisposable, IAsyncLifetime
{
    private readonly IHost _host;
    public IJobHost Jobs => _host.Services.GetService<IJobHost>();

    public HostFixture() =>
        _host = new HostBuilder()
            .ConfigureWebJobs(builder => builder
                .AddDurableTask(options => options.HubName = nameof(MyTest))
                .AddAzureStorageCoreServices())
            .Build();

    public void Dispose() => 
        _host.Dispose();

    public Task InitializeAsync() => 
        _host.StartAsync();

    public Task DisposeAsync() => 
        Task.CompletedTask;
}
```

Inject and cleanup the host in the test class:

```c#
public class MyTest : IClassFixture<HostFixture>, IAsyncLifetime
{
    private readonly HostFixture _host;

    public MyTest(HostFixture host) =>
        _host = host;

    public Task InitializeAsync() => 
        _host.Jobs
            .Terminate()
            .Purge();

    public Task DisposeAsync() => 
        Task.CompletedTask;
}
```

But please, don't to do a `ConfigureAwait(false).GetAwaiter().GetResult()`.
> Using ConfigureAwait(false) to avoid deadlocks is a dangerous practice. You would have to use ConfigureAwait(false) for every await in the transitive closure of all methods called by the blocking code, including all third- and second-party code. Using ConfigureAwait(false) to avoid deadlock is at best just a hack).

[Don’t block on async code](https://blog.stephencleary.com/2012/07/dont-block-on-async-code.html).

## Azure Storage Account

You need an azure storage table to store the state of the durable functions.
The only two options currently are [Azure](#option-1-azure) and the [Azure Storage Emulator](#option-2-azure-storage-emulator).

### Option 1: Azure

Just copy the [connection string from your storage account](https://docs.microsoft.com/en-us/azure/storage/common/storage-configure-connection-string#view-and-copy-a-connection-string),
works everywhere.

### Option 2: Azure Storage Emulator

Set the connection string to `UseDevelopmentStorage=true`. Unfortunately works only on Windows. See this [blog](https://zimmergren.net/azure-devops-unit-tests-storage-emulator-hosted-agent/)
on how to enable the storage emulator in an Azure DevOps pipeline.

### Option 3: Azurite

Unfortunately, `azurite@v2` doesn't work with the current version of durable functions,
and `azurite@v3` doesn't have the [required features (implemented yet)](https://github.com/Azure/Azurite#azurite-v3).

### Set the Storage Connection String

The storage connection string setting [is required](https://docs.microsoft.com/en-us/azure/app-service/webjobs-sdk-how-to#host-connection-strings).

#### Option 1: with an environment variable

Set the environment variable `AzureWebJobsStorage`. Hereby you can also overwrite the configured connection from [option 2](#option-2-from-a-configuration-file) on your local dev machine.

#### Option 2: with a configuration file

Include an `appsettings.json` in your test project:

```json
{
  "AzureWebJobsStorage": "DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...==;EndpointSuffix=core.windows.net"
}
```

and make sure it is copied to the output directory:

```xml
<ItemGroup>
    <Content Include="appsettings.json" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

---

Happy coding!
