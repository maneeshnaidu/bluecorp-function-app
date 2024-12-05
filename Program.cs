using bluecorp_function_app.Interfaces;
using bluecorp_function_app.Services;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

var builder = FunctionsApplication.CreateBuilder(args);

// Add related dependencies
builder.Services.AddScoped<IJsonToCsvMapper, JsonToCsvMapper>();
builder.Services.AddScoped<ISftpService, SftpService>();
builder.Services.AddScoped<IHttpRetryService, HttpRetryService>();

// Get the Redis connection string from environment variables or Azure Application Settings
var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");

// Add Redis as a singleton
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));
builder.Services.AddSingleton<IControlNumberValidationService, ControlNumberValidationService>();

builder.ConfigureFunctionsWebApplication();

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
