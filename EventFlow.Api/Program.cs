using EventFlow.Api;
using EventFlow.Api.Services;
using EventFlow.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();   

builder.Services.AddSingleton<IEventService, EventService>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddOpenApi();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        options.IncludeXmlComments(xmlPath);
    });

    builder.Host.UseDefaultServiceProvider(options =>
    {
        // Проверяет Captive Dependency во время выполнения
        options.ValidateScopes = true;

        // Проверяет корректность всех регистраций при старте приложения
        options.ValidateOnBuild = true;
    });
}
var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{

    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

