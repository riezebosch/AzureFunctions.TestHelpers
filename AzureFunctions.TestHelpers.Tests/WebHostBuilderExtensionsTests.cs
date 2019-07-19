using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Flurl.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Script;
using Microsoft.Azure.WebJobs.Script.WebHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using NSubstitute;
using Xunit;

namespace AzureFunctions.TestHelpers.Tests
{
    public class WebHostBuilderExtensionsTests
    {
        [Fact]
        public async Task HostDemoFunctionsAreAccessibleFromClient()
        {
            using (var host = Microsoft.Azure.WebJobs.Script.WebHost.Program.CreateWebHostBuilder()
                .UseUrls("http://localhost:7071")
                .UseSolutionRelativeAzureWebJobs("AzureFunctions.TestHelpers.Tests.DemoFunctions")
                .Build())
            {
                await host.StartAsync();
                var hostService = host.Services.GetRequiredService<WebJobsScriptHostService>();

                await hostService.DelayUntilHostReady();

                var scriptHost = hostService.Services.GetRequiredService<IScriptJobHost>();
                scriptHost.Functions.Should().NotBeEmpty();

                await "http://localhost:7071/api/demo".GetAsync();
            }
        }

        [Fact(Skip = "not working because assemblies loaded from different location.")]
        public async Task IntegrationTest()
        {
            var mock = Substitute.For<IInjectable>();

            using (var host = Microsoft.Azure.WebJobs.Script.WebHost.Program.CreateWebHostBuilder()
                .UseUrls("http://localhost:7071")
                .UseSolutionRelativeAzureWebJobs("AzureFunctions.TestHelpers.Tests.DemoFunctions")
                .ConfigureServices(services => services.AddSingleton<IConfigureBuilder<IWebJobsBuilder>>(new ConfigureTestServices(s => s.Replace(ServiceDescriptor.Singleton(mock)))))
                .Build())
            {                
                await host.StartAsync();
                var hostService = host.Services.GetRequiredService<WebJobsScriptHostService>();

                var ready = await hostService.DelayUntilHostReady();
                ready.Should().BeTrue();

                var scriptHost = hostService.Services.GetRequiredService<IScriptJobHost>();
                scriptHost.Functions.Should().NotBeEmpty();

                await "http://localhost:7071/api/demo-injection".GetAsync();
                mock.Received().Execute();
            }
        }
        
        [Fact]
        public async Task UsingHostBuilder()
        {
            var mock = Substitute.For<IInjectable>();
            var request = new DummyHttpRequest { Method = "Get" };

            using (var host = new HostBuilder()
                .ConfigureWebJobs((context, builder) =>
                {
                    builder
                        .AddHttp()
                        .AddAzureStorageCoreServices();
                    
                    new Startup().Configure(builder);
                    builder.Services.Replace(ServiceDescriptor.Singleton(mock));
                })
                .Build())
            {
                await host.StartAsync();
                
                var jobs = host.Services.GetService(typeof(IJobHost)) as JobHost;
                await jobs.CallAsync(nameof(DemoInjection), new Dictionary<string, object>
                {
                    ["request"] = request
                });
                
                mock
                    .Received()
                    .Execute();
            }
        }

        [Fact]
        public void Throws_DirectoryNotFound()
        {
            Assert.Throws<DirectoryNotFoundException>(
                () => new WebHostBuilder().UseSolutionRelativeAzureWebJobs("asdf"));
        }

        [Fact]
        public void ExistingDirectoryButNoFunctions_Throws_FunctionsNotFound()
        {
            var ex = Assert.Throws<FileNotFoundException>(() =>
                new WebHostBuilder().UseSolutionRelativeAzureWebJobs("AzureFunctions.TestHelpers.Tests"));
            ex.Message.Should().Contain("found beneath");
        }
    }

    public class DummyHttpRequest : HttpRequest
    {
        public DummyHttpRequest()
        {
           HttpContext = new DummyHttpContext(this);
        }
        public override Task<IFormCollection> ReadFormAsync(CancellationToken cancellationToken = new CancellationToken()) => 
            Task.FromResult((IFormCollection)new FormCollection(new Dictionary<string, StringValues>()));

        public override HttpContext HttpContext { get; }
        public override string Method { get; set; }
        public override string Scheme { get; set; }
        public override bool IsHttps { get; set; }
        public override HostString Host { get; set; }
        public override PathString PathBase { get; set; }
        public override PathString Path { get; set; }
        public override QueryString QueryString { get; set; }
        public override IQueryCollection Query { get; set; } = new QueryCollection();
        public override string Protocol { get; set; }
        public override IHeaderDictionary Headers { get; } = new HeaderDictionary();
        public override IRequestCookieCollection Cookies { get; set; }
        public override long? ContentLength { get; set; }
        public override string ContentType { get; set; }
        public override Stream Body { get; set; } = new MemoryStream();
        public override bool HasFormContentType { get; }
        public override IFormCollection Form { get; set; }
    }

    public class DummyHttpContext : HttpContext
    {
        public DummyHttpContext(DummyHttpRequest request)
        {
            Request = request;
        }

        public override void Abort()
        {
        }

        public override IFeatureCollection Features { get; }
        public override HttpRequest Request { get; }
        public override HttpResponse Response { get; }
        public override ConnectionInfo Connection { get; }
        public override WebSocketManager WebSockets { get; }
        public override AuthenticationManager Authentication { get; }
        public override ClaimsPrincipal User { get; set; }
        public override IDictionary<object, object> Items { get; set; } = new Dictionary<object, object>();
        public override IServiceProvider RequestServices { get; set; }
        public override CancellationToken RequestAborted { get; set; }
        public override string TraceIdentifier { get; set; }
        public override ISession Session { get; set; }
    }

    public class ConfigureTestServices : IConfigureBuilder<IWebJobsBuilder>
    {
        private readonly Action<IServiceCollection> _func;

        public ConfigureTestServices(Action<IServiceCollection> func)
        {
            _func = func;
        }

        public void Configure(IWebJobsBuilder builder)
        {
            _func(builder.Services);
        }
    }
}
 
