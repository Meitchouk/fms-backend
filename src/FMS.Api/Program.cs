using FMS.Application.Configuration;
using FMS.Application.Interfaces;
using FMS.Infrastructure;
using FMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;

// ---------------------------------------------------------------------------
// Bootstrap Serilog early for startup logging
// ---------------------------------------------------------------------------
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting FMS Backend...");

    var builder = WebApplication.CreateBuilder(args);

    // -----------------------------------------------------------------------
    // Serilog (replaces default logging)
    // -----------------------------------------------------------------------
    builder.Host.UseSerilog((context, services, configuration) =>
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "FMS.Api")
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"));

    // -----------------------------------------------------------------------
    // Configuration: bind Fms options
    // -----------------------------------------------------------------------
    builder.Services.Configure<FmsOptions>(
        builder.Configuration.GetSection(FmsOptions.SectionName));

    // -----------------------------------------------------------------------
    // Infrastructure layer (EF Core, PostgreSQL, services)
    // -----------------------------------------------------------------------
    builder.Services.AddInfrastructure(builder.Configuration);

    // -----------------------------------------------------------------------
    // Health checks
    // -----------------------------------------------------------------------
    builder.Services.AddHealthChecks()
        .AddNpgSql(
            builder.Configuration.GetConnectionString("FmsDatabase")!,
            name: "postgresql",
            tags: new[] { "db", "ready" });

    // -----------------------------------------------------------------------
    // Build app
    // -----------------------------------------------------------------------
    var app = builder.Build();

    // -----------------------------------------------------------------------
    // Auto-migrate in Development only
    // -----------------------------------------------------------------------
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FmsDbContext>();
        await dbContext.Database.MigrateAsync();
        Log.Information("Database migration applied (Development mode)");
    }

    // -----------------------------------------------------------------------
    // Log startup configuration
    // -----------------------------------------------------------------------
    var fmsOptions = builder.Configuration
        .GetSection(FmsOptions.SectionName)
        .Get<FmsOptions>() ?? new FmsOptions();

    Log.Information("FMS Backend started — Mode: {Mode}, Version: {Version}, Environment: {Environment}",
        fmsOptions.Mode, fmsOptions.Version, app.Environment.EnvironmentName);

    // -----------------------------------------------------------------------
    // Middleware pipeline
    // -----------------------------------------------------------------------
    app.UseSerilogRequestLogging();

    // -----------------------------------------------------------------------
    // Endpoints
    // -----------------------------------------------------------------------

    // Health check endpoints
    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready")
    });
    app.MapHealthChecks("/health/live");

    // System status endpoint — returns environment, version, mode
    app.MapGet("/api/status", (IConfiguration configuration) =>
    {
        var opts = configuration
            .GetSection(FmsOptions.SectionName)
            .Get<FmsOptions>() ?? new FmsOptions();

        return Results.Ok(new
        {
            status = "ok",
            version = opts.Version,
            mode = opts.Mode,
            environment = app.Environment.EnvironmentName,
            timestamp = DateTime.UtcNow
        });
    })
    .WithName("SystemStatus")
    .WithTags("system");

    // Bootstrap endpoint — validates DB connectivity and migrations
    app.MapGet("/api/bootstrap", async (IDatabaseHealthService healthService, CancellationToken ct) =>
    {
        var result = await healthService.CheckHealthAsync(ct);

        var statusCode = result.IsConnected && result.MigrationsApplied
            ? StatusCodes.Status200OK
            : StatusCodes.Status503ServiceUnavailable;

        return Results.Json(new
        {
            database = new
            {
                connected = result.IsConnected,
                migrationsApplied = result.MigrationsApplied,
                pendingMigrations = result.PendingMigrations,
                error = result.Error
            },
            timestamp = DateTime.UtcNow
        }, statusCode: statusCode);
    })
    .WithName("Bootstrap")
    .WithTags("system");

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
