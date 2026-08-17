using EventFlow.Users.Api.Common;
using EventFlow.Users.Application;
using EventFlow.Users.Infrastructure;
using EventFlow.Users.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Formatting.Compact;
using System.Reflection;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);

var jwtSecret = jwtSection["Secret"]
    ?? throw new InvalidOperationException("Секретный ключ JWT не настроен.");

var jwtIssuer = jwtSection["Issuer"]
    ?? throw new InvalidOperationException("Издатель JWT не настроен.");

var jwtAudience = jwtSection["Audience"]
    ?? throw new InvalidOperationException("Аудитория JWT не настроена.");

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var problem = new ValidationProblemDetails(context.ModelState)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Некорректный запрос",
                Detail = "Одна или несколько ошибок валидации",
                Type = "https://httpstatuses.com/400",
                Instance = context.HttpContext.Request.Path
            };

            return new BadRequestObjectResult(problem);
        };
    });

builder.Services.AddProblemDetails();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,

        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),

        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,

        ValidateAudience = true,
        ValidAudience = jwtAudience,

        ValidateLifetime = true,
        RequireExpirationTime = true,

        ClockSkew = TimeSpan.Zero,

        NameClaimType = ClaimTypes.Name,
        RoleClaimType = ClaimTypes.Role
    };
});

builder.Services.AddAuthorization();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
         .AddAspNetCoreInstrumentation(options =>
         {
             options.Filter = httpContext =>
             {
                 var path = httpContext.Request.Path;
                 return !path.StartsWithSegments("/metrics") &&
                        !path.StartsWithSegments("/health");
             };
         })
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddPrometheusExporter());

builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration)
        .WriteTo.Console(new CompactJsonFormatter()));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddOpenApi();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        options.IncludeXmlComments(xmlPath);
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "Введите JWT без слова Bearer.",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });
        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
    });

    builder.Host.UseDefaultServiceProvider(options =>
    {
        options.ValidateScopes = true;
        options.ValidateOnBuild = true;
    });
}

var app = builder.Build();

await app.ApplyMigrationsAsync();

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPrometheusScrapingEndpoint();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
