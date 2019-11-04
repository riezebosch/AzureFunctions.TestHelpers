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
    public class DurableFunctionsHelpers : IDisposable
    {
        private readonly IInjectable _mock;
        private readonly IHost _host;

        public DurableFunctionsHelpers()
        {
            _mock = Substitute.For<IInjectable>();
            _host = new HostBuilder()
                .ConfigureWebJobs(builder => builder
                    .AddTimers()
                    .AddDurableTaskInTestHub()
                    .AddAzureStorageCoreServices()
                    .UseWebJobsStartup<Startup>()
                    .ConfigureServices(services => services.Replace(ServiceDescriptor.Singleton(_mock))))
                .Build();
            
            _host.StartAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        [Fact]
        public async Task Wait()
        {
            // Arrange
            var jobs = _host.Services.GetService<IJobHost>();

            // Act
            await jobs.CallAsync(nameof(Starter), new Dictionary<string, object>
            {
                ["timerInfo"] = new TimerInfo(new WeeklySchedule(), new ScheduleStatus())
            });

            await jobs.Wait()
                .ThrowIfFailed()
                .Purge();

            // Assert
            _mock
                .Received()
                .Execute();
        }

        [Fact]
        public async Task WaitWithTimeout()
        {
            // Arrange
            _mock
                .When(x => x.Execute())
                .Do(x => Thread.Sleep(60000));

            var jobs = _host.Services.GetService<IJobHost>();

            // Act
            await jobs.CallAsync(nameof(Starter), new Dictionary<string, object>
            {
                ["timerInfo"] = new TimerInfo(new WeeklySchedule(), new ScheduleStatus())
            });

            jobs.Invoking(async x => await x.Wait(TimeSpan.FromSeconds(20)))
                .Should()
                .Throw<TaskCanceledException>();

            // Assert
            _mock.Received()
                .Execute();
        }

        [Fact]
        public async Task WaitDoesNotThrow()
        {
            // Arrange
            _mock
                .When(x => x.Execute())
                .Do(x => throw new InvalidOperationException());

            var jobs = _host.Services.GetService<IJobHost>();

            // Act
            await jobs.CallAsync(nameof(Starter), new Dictionary<string, object>
            {
                ["timerInfo"] = new TimerInfo(new WeeklySchedule(), new ScheduleStatus())
            });

            await jobs
                .Wait()
                .Purge();

            // Assert
            _mock.Received()
                .Execute();
        }

        [Fact]
        public async Task ThrowIfFailed()
        {
            // Arrange
            _mock
                .When(x => x.Execute())
                .Do(x => throw new InvalidOperationException());

            var jobs = _host.Services.GetService<IJobHost>();

            // Act
            await jobs.CallAsync(nameof(Starter), new Dictionary<string, object>
            {
                ["timerInfo"] = new TimerInfo(new WeeklySchedule(), new ScheduleStatus())
            });


            // Assert
            jobs.Invoking(x => x
                    .Wait(TimeSpan.FromSeconds(20))
                    .ThrowIfFailed())
                .Should()
                .Throw<Exception>();

            await jobs
                .Wait()
                .Purge();
        }

        public void Dispose()
        {
            _host.Dispose();
        }
    }
}