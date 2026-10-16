using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}

string connectionString = Environment.GetEnvironmentVariable("CosmosDbConnectionString") ?? throw new InvalidOperationException("CosmosDB connection string is not set in the environment variables.");

builder.Services.AddSingleton<CosmosClient>(_ => new CosmosClient(connectionString, new CosmosClientOptions() { ConnectionMode = ConnectionMode.Gateway }));
builder.Services.AddSingleton<IPersonRepository, PersonRepository>();
builder.Services.AddSingleton<IPersonService, PersonService>();



builder.Build().Run();
