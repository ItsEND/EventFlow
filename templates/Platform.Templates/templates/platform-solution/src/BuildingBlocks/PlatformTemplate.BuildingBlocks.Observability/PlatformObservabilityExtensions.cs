using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Formatting.Compact;

namespace PlatformTemplate.BuildingBlocks.Observability;

public static class PlatformObservabilityExtensions
{
    public static WebApplicationBuilder AddPlatformObservability(this WebApplicationBuilder builder)
    {
        var serviceName = builder.Configuration["OTEL_SERVICE_NAME"]
            ?? builder.Environment.ApplicationName;

        builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration).Enrich.FromLogContext().Enrich.WithProperty("service.name", serviceName).WriteTo.Console(new CompactJsonFormatter()));

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.Filter = context =>
                        !context.Request.Path.StartsWithSegments("/health") &&
                        !context.Request.Path.StartsWithSegments("/metrics");
                })
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddOtlpExporter())
            .WithMetrics(metrics => metrics.AddAspNetCoreInstrumentation().AddRuntimeInstrumentation().AddPrometheusExporter());

        return builder;
    }

    public static WebApplication UsePlatformObservability(this WebApplication app)
    {
        app.UseSerilogRequestLogging();
        return app;
    }

    public static WebApplication MapPlatformObservability(this WebApplication app)
    {
        app.MapPrometheusScrapingEndpoint("/metrics").AllowAnonymous();
        return app;
    }
}
