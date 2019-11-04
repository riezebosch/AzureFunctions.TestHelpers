using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureFunctions.TestHelpers.Starters;
using DurableTask.Core.Exceptions;
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
    public static class HostBuilderTests
    {
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

                await jobs.Wait()
                    .ThrowIfFailed()
                    .Purge();

                // Assert
                mock
                    .Received()
                    .Execute();
            }
        }

        [Fact]
        public static async Task WaitWithTimeout()
        {
            // Arrange
            var mock = Substitute.For<IInjectable>();  
            mock
                .When(x => x.Execute())
                .Do(x => Thread.Sleep(60000));
            
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

                jobs.Invoking(async x => await x.Wait(TimeSpan.FromSeconds(20)))
                    .Should()
                    .Throw<TaskCanceledException>();

                // Assert
                mock.Received()
                    .Execute();
            }
        }
        
        [Fact]
        public static async Task WaitDoesNotThrow()
        {
            // Arrange
            var mock = Substitute.For<IInjectable>();  
            mock
                .When(x => x.Execute())
                .Do(x => throw new InvalidOperationException());
            
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

                await jobs
                    .Wait(TimeSpan.FromSeconds(20))
                    .Purge();

                // Assert
                mock.Received()
                    .Execute();
            }
        }

        [Fact]
        public static async Task ThrowIfFailed()
        {
            // Arrange
            var mock = Substitute.For<IInjectable>();
            mock
                .When(x => x.Execute())
                .Do(x => throw new InvalidOperationException());

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
        }
    }
}