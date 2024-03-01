using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AzureFunctions.TestHelpers.Orchestrators;
using AzureFunctions.TestHelpers.Starters;
using FluentAssertions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using NSubstitute.ClearExtensions;
using Xunit;

namespace AzureFunctions.TestHelpers.Tests
{
    public class DurableFunctionsHelper : 
        IClassFixture<DurableFunctionsHelper.HostFixture>,
        IAsyncLifetime,
        IClassFixture<AzuriteContainer>
    {
        private readonly HostFixture _host;

        public DurableFunctionsHelper(HostFixture host)
        {
            _host = host;
            _host.Mock.ClearSubstitute();
        }
        
        [Fact]
        public async Task WaitFor()
        {
            // Arrange
            var jobs = _host.Jobs;

            // Act
            await jobs.CallAsync(nameof(DemoStarter), new Dictionary<string, object>
            {
                ["timerInfo"] = new TimerInfo(new WeeklySchedule(), new ScheduleStatus())
            });

            await jobs
                .WaitFor(nameof(DemoOrchestration))
                .ThrowIfFailed()
                .Purge();

            // Assert
            await _host.Mock
                .Received()
                .Execute("from an activity");
            
            await _host.Mock
                .Received()
                .Execute("from an entity");
        }

        [Fact]
        public async Task WaitForWith_fast_retry()
        {
            // Arrange
            var jobs = _host.Jobs;

            // Act
            await jobs.CallAsync(nameof(DemoStarter), new Dictionary<string, object>
            {
                ["timerInfo"] = new TimerInfo(new WeeklySchedule(), new ScheduleStatus())
            });

            await jobs
                .WaitFor(nameof(DemoOrchestration), retry: TimeSpan.FromMilliseconds(100))
                .ThrowIfFailed()
                .Purge();

            // Assert
            await _host.Mock
                .Received()
                .Execute("from an activity");
            
            await _host.Mock
                .Received()
                .Execute("from an entity");
        }

        [Fact]
        public async Task WaitForTimeout()
        {
            // Arrange
            _host.Mock
                .When(x => x.Execute(Arg.Any<string>()))
                .Do(_ => Thread.Sleep(60000));

            var jobs = _host.Jobs;

            // Act
            await jobs.CallAsync(nameof(DemoStarter), new Dictionary<string, object>
            {
                ["timerInfo"] = new TimerInfo(new WeeklySchedule(), new ScheduleStatus())
            });

            // Act & Assert
            await jobs.Invoking(async x => await x.WaitFor(nameof(DemoOrchestration), TimeSpan.FromSeconds(20)))
                .Should()
                .ThrowAsync<TaskCanceledException>();
        }

        [Fact]
        public async Task WaitDoesNotThrow()
        {
            // Arrange
            _host.Mock
                .When(x => x.Execute(Arg.Any<string>()))
                .Do(_ => throw new InvalidOperationException());

            var jobs = _host.Jobs;
            await jobs.CallAsync(nameof(DemoStarter), new Dictionary<string, object>
            {
                ["timerInfo"] = new TimerInfo(new WeeklySchedule(), new ScheduleStatus())
            });

            // Act
            await jobs
                .WaitFor(nameof(DemoOrchestration))
                .Purge();

            // Assert
            await _host.Mock.Received()
                .Execute(Arg.Any<string>());
        }

        [Fact]
        public async Task ThrowIfFailed()
        {
            // Arrange
            _host.Mock
                .When(x => x.Execute(Arg.Any<string>()))
                .Do(_ => throw new InvalidOperationException());

            var jobs = _host.Jobs;

            // Act
            await jobs.CallAsync(nameof(DemoStarter), new Dictionary<string, object>
            {
                ["timerInfo"] = new TimerInfo(new WeeklySchedule(), new ScheduleStatus())
            });
            
            // Assert
            await jobs.Invoking(x => x
                    .WaitFor(nameof(DemoOrchestration), TimeSpan.FromSeconds(20))
                    .ThrowIfFailed())
                .Should()
                .ThrowAsync<Exception>();
        }
        
        [Fact]
        public async Task Terminate()
        {
            // Arrange
            _host.Mock
                .When(x => x.Execute(Arg.Any<string>()))
                .Do(_ => Thread.Sleep(600000));

            var jobs = _host.Jobs;
            await jobs.CallAsync(nameof(DemoStarter), new Dictionary<string, object>
            {
                ["timerInfo"] = new TimerInfo(new WeeklySchedule(), new ScheduleStatus())
            });

            // Act
            await jobs
                .Terminate()
                .Purge();

            // Assert
            var sw = Stopwatch.StartNew();
            await jobs.WaitFor(nameof(DemoOrchestration));
            sw.Elapsed
                .Should()
                .BeLessThan(TimeSpan.FromMinutes(1));
        }
        
        public class HostFixture : IDisposable, IAsyncLifetime
        {
            private readonly IHost _host;
            public IJobHost Jobs => _host.Services.GetService<IJobHost>();
            public IInjectable Mock { get; } = Substitute.For<IInjectable>();

            public HostFixture()
            {
                _host = new HostBuilder()
                    .ConfigureWebJobs(builder => builder
                        .AddDurableTask(options => options.HubName = nameof(DurableFunctionsHelper))
                        .AddAzureStorageCoreServices()
                        .ConfigureServices(services => services.AddSingleton(Mock)))
                    .Build();
            }
        
            public void Dispose() => _host.Dispose();

            public async Task InitializeAsync() => await _host.StartAsync();

            public Task DisposeAsync() => Task.CompletedTask;
        }

        public Task InitializeAsync() =>  _host.Jobs
            .Terminate()
            .Purge();

        public Task DisposeAsync() => InitializeAsync();
    }
}