using Serilog;
using FluentValidation;
using System.Threading.RateLimiting;
using HelloWorldBackend.Configuration;
using HelloWorldBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "Hello World Backend API", 
        Version = "v1",
        Description = ".NET 8 Web API for Hello World application with AI text summarization functionality"
    });
});

// Configure OpenAI settings
builder.Services.Configure<OpenAISettings>(builder.Configuration.GetSection(OpenAISettings.SectionName));

// Add HttpClient factory
builder.Services.AddHttpClient();

// Register OpenAI service
builder.Services.AddHttpClient<IOpenAIService, OpenAIService>();

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add Rate Limiting - TODO: Implement rate limiting

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy())
    .AddCheck<OpenAIHealthCheck>("openai");

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hello World Backend API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at app's root
    });
}

app.UseHttpsRedirection();

// Add security headers
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    await next();
});

app.UseCors("AllowAll");
// app.UseRateLimiter(); // Disabled for now

app.MapControllers();
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(x => new
            {
                name = x.Key,
                status = x.Value.Status.ToString(),
                exception = x.Value.Exception?.Message,
                duration = x.Value.Duration.ToString()
            }),
            duration = report.TotalDuration.ToString()
        };
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }
});

try
{
    Log.Information("Starting Hello World Backend API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}