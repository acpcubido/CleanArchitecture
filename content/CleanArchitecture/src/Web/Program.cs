using Cubido.Template.Infrastructure.Data;
#if (IncludeMcpServer && UseEntraIdAuthentication)
using ModelContextProtocol.AspNetCore.Authentication;
#endif
using Scalar.AspNetCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddKeyVaultIfConfigured();
builder.AddApplicationServices(builder.Configuration);
builder.AddInfrastructureServices();
builder.AddWebServices();
builder.AddOpenTelemetryInstrumentation();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:Origins").Get<string>()!.Split(';', StringSplitOptions.RemoveEmptyEntries);
        policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Dont run migrations during an OpenAPI generator run
    if (Assembly.GetEntryAssembly()?.GetName().Name != "GetDocument.Insider")
    {
        await app.InitialiseDatabaseAsync();
    }
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();

    // Dont run migrations during an OpenAPI generator run
    if (Assembly.GetEntryAssembly()?.GetName().Name != "GetDocument.Insider")
    {
        await app.MigrateDatabase();
    }
}

app.UseHealthChecks("/health");
app.UseHttpsRedirection();
app.UseForwardedHeaders();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();
app.UseAntiforgeryCookie();

app.MapOpenApi("/api/swagger.json");
app.MapScalarApiReference("/api/swagger", options =>
{
    options.WithOpenApiRoutePattern("/api/swagger.json");
});

app.UseCors();

app.UseExceptionHandler(options => { });

app.MapEndpoints();
#if (IncludeMcpServer)
app.MapMcp("/api/mcp")
#if (UseEntraIdAuthentication)
    .RequireAuthorization(McpAuthenticationDefaults.AuthenticationScheme)
#endif
    ;
#endif

app.Run();


public partial class Program { }
