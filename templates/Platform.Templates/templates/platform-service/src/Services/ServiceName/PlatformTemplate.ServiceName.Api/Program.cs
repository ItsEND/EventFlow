using PlatformTemplate.BuildingBlocks.Api;
using PlatformTemplate.BuildingBlocks.Authentication;
using PlatformTemplate.BuildingBlocks.Observability;
using PlatformTemplate.ServiceName.Application;
using PlatformTemplate.ServiceName.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddPlatformAuthentication();
builder.AddPlatformObservability();
builder.AddPlatformApi();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UsePlatformObservability();
app.UsePlatformApi();
app.UsePlatformAuthentication();

app.MapPlatformApi();
app.MapPlatformObservability();

app.Run();

public partial class Program;
