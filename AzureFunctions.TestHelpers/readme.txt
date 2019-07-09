Add a NuGet.config to your project with the following package source:

------------------------------------------------------------------------------------------------
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <packageSources>
        <add key="azure_app_service" value="https://www.myget.org/F/azure-appservice/api/v2" />
    </packageSources>
</configuration>
------------------------------------------------------------------------------------------------

Add the webhost package to the package references:

------------------------------------------------------------------------------------------------
<PackageReference Include="Microsoft.Azure.WebJobs.Script.WebHost" Version="2.0.12543" />
------------------------------------------------------------------------------------------------

Initialize the webhost from the official webhost program and configure it using my extension method:

------------------------------------------------------------------------------------------------
using (var host = Microsoft.Azure.WebJobs.Script.WebHost.Program.CreateWebHostBuilder()
    .UseUrls("http://localhost:7071")
    .UseSolutionRelativeAzureWebJobs("<THE_NAME_OF_THE_FUNCTIONS_LIBRARY_FOLDER_HERE>")
    .Build())
{
    await host.StartAsync();
    await host.Services.GetRequiredService<WebJobsScriptHostService>().DelayUntilHostReady();
}
------------------------------------------------------------------------------------------------