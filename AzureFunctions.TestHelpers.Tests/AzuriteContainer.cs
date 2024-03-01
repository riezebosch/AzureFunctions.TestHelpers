using System;
using Ductus.FluentDocker.Services;
using FluentAssertions.Extensions;

namespace AzureFunctions.TestHelpers.Tests;

public sealed class AzuriteContainer : IDisposable
{
    private readonly IContainerService _container = new Ductus.FluentDocker.Builders.Builder()
        .UseContainer()
        .UseImage("mcr.microsoft.com/azure-storage/azurite")
        .ExposePort(10000, 10000)
        .ExposePort(10001, 10001)
        .ExposePort(10002, 10002)
        .WaitForPort("10000/tcp", 30.Seconds())
        .Build()
        .Start();

    public AzuriteContainer() => 
        Environment.SetEnvironmentVariable("AzureWebJobsStorage", "UseDevelopmentStorage=true");

    void IDisposable.Dispose() =>
        _container.Dispose();
}
