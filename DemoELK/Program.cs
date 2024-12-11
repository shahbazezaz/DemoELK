using Serilog.Sinks.Elasticsearch;
using Serilog;
using Microsoft.Extensions.Hosting.Internal;
using System.Reflection;
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

try
{
    var builder = WebApplication.CreateBuilder(args);
    var context = builder.Environment;
    var environment = context.EnvironmentName;
    var formattedEnvironmentName = environment?.ToLower().Replace(".", "-");

    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(configuration)
        .Enrich.WithMachineName()
        .WriteTo.Console()
        .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(node:new Uri(configuration["ElasticConfiguration:Uri"]))
        {
            IndexFormat = $"{configuration["ApplicationName"]}-logs-{formattedEnvironmentName}-{DateTime.UtcNow:yyyy-MM}",
            AutoRegisterTemplate = true,
            NumberOfShards = 2,
            NumberOfReplicas = 1
        })
        .Enrich.WithProperty(name: "Environment", environment)
        .ReadFrom.Configuration(configuration)
        .CreateLogger();

    // Logging configuration
    builder.Logging.ClearProviders();
    builder.Logging.AddSerilog(dispose: true);

    // Add services to the container.
    builder.Services.AddSingleton<Serilog.ILogger>(Log.Logger);
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
//var builder = WebApplication.CreateBuilder(args);
//var context = builder.Environment;

//var applicationName = configuration["ApplicationName"];
//var environment = context.EnvironmentName;
//var formattedEnvironmentName = environment?.ToLower().Replace(".", "-");

//Log.Logger = new LoggerConfiguration()
//            .Enrich.WithMachineName()
//            .WriteTo.Console()
//            .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(configuration["ElasticConfiguration:Uri"]))
//            {
//                IndexFormat = $"{applicationName}-logs-{formattedEnvironmentName}-{DateTime.UtcNow:yyyy-MM}",
//                AutoRegisterTemplate = true,
//                NumberOfShards = 2,
//                NumberOfReplicas = 1
//            })
//            .Enrich.WithProperty(name:"Environment", environment)
//            .ReadFrom.Configuration(configuration)
//            .CreateLogger();

//// Add services to the container.

//builder.Services.AddControllers();
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();
