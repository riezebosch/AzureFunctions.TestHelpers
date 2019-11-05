using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureFunctions.TestHelpers.Starters;
using FluentAssertions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Xunit;

namespace AzureFunctions.TestHelpers.Tests
{
    public class HostFixture : IDisposable
    {
        public IInjectable Mock { get; }
        private readonly IHost _host;
        public IJobHost Jobs => _host.Services.GetService<IJobHost>();

        public HostFixture()
        {
            Mock = Substitute.For<IInjectable>();
            _host = new HostBuilder()
                .ConfigureWebJobs(builder => builder
                    .AddDurableTaskInTestHub(options => options.MaxQueuePollingInterval = TimeSpan.FromSeconds(2))
                    .AddAzureStorageCoreServices()
                    .ConfigureServices(services => services.AddSingleton(Mock)))
                .Build();

            _host.StartAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }
        
        public void Dispose()
        {
            _host.Dispose();
        }
    }

    public class DurableFunctionsHelper : IClassFixture<HostFixture>
    {
        private readonly HostFixture _host;

        public DurableFunctionsHelper(HostFixture host)
        {
            _host = host;
        }
        
        [Fact]
        public async Task Ready()
        {
            // Arrange
            _host.Mock
                .When(x => x.Execute())
                .Do(x => Thread.Sleep(15000)); // waiting long enough to let the failure orchestration kick in (when enabled).

            var jobs = _host.Jobs;

            // Act
            await jobs.CallAsync(nameof(Starter), new Dictionary<string, object>
            {
                ["timerInfo"] = new TimerInfo(new WeeklySchedule(), new ScheduleStatus())
            });

            await jobs
                .Ready()
                .ThrowIfFailed()
                .Purge();

            // Assert
            _host.Mock
                .Received()
                .Execute();
        }

        [Fact]
        public async Task WaitWithTimeout()
        {
            // Arrange
            _host.Mock
                .When(x => x.Execute())
                .Do(x => Thread.Sleep(60000));

            var jobs = _host.Jobs;

            // Act
            await jobs.CallAsync(nameof(Starter), new Dictionary<string, object>
            {
                ["timerInfo"] = new TimerInfo(new WeeklySchedule(), new ScheduleStatus())
            });

            jobs.Invoking(async x => await x.Ready(TimeSpan.FromSeconds(20)))
                .Should()
                .Throw<TaskCanceledException>();

            // Assert
            _host.Mock.Received()
                .Execute();
        }

        [Fact]
        public async Task WaitDoesNotThrow()
        {
            // Arrange
            _host.Mock
                .When(x => x.Execute())
                .Do(x => throw new InvalidOperationException());

            var jobs = _host.Jobs;

            // Act
            await jobs.CallAsync(nameof(Starter), new Dictionary<string, object>
            {
                ["timerInfo"] = new TimerInfo(new WeeklySchedule(), new ScheduleStatus())
            });

            await jobs
                .Ready()
                .Purge();

            // Assert
            _host.Mock.Received()
                .Execute();
        }

        [Fact]
        public async Task ThrowIfFailed()
        {
            // Arrange
            _host.Mock
                .When(x => x.Execute())
                .Do(x => throw new InvalidOperationException());

            var jobs = _host.Jobs;

            // Act
            await jobs.CallAsync(nameof(Starter), new Dictionary<string, object>
            {
                ["timerInfo"] = new TimerInfo(new WeeklySchedule(), new ScheduleStatus())
            });
            
            // Assert
            jobs.Invoking(x => x
                    .Ready(TimeSpan.FromSeconds(20))
                    .ThrowIfFailed())
                .Should()
                .Throw<Exception>();

            await jobs
                .Ready()
                .Purge();
        }
    }
}