using Azure.Identity;
using Cubido.Template.Application.Common.Interfaces;
using Cubido.Template.Infrastructure.Data;
using Cubido.Template.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Sqiddler.OpenApi;
#if (IncludeMcpServer)
using Cubido.Template.Web;
using Cubido.Template.Web.Tools;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ModelContextProtocol.AspNetCore.Authentication;
using System.Security.Claims;
#endif

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddWebServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddScoped<IUser, CurrentUser>();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

        builder.Services.AddExceptionHandler<CustomExceptionHandler>();


        // Customise default API behaviour
        builder.Services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddOpenApi(options =>
        {
            options.OpenApiVersion = OpenApi.OpenApiSpecVersion.OpenApi3_1;
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
            options.AddSqids();
        });
#if (IncludeMcpServer)

        // MCP server
        var azureAdOptions = builder.Configuration.GetRequiredSection("AzureAd").Get<AzureAdOptions>()!;
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultChallengeScheme = McpAuthenticationDefaults.AuthenticationScheme;
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.Authority = $"https://login.microsoftonline.com/{azureAdOptions.JwtTenantId}/v2.0";
            options.Audience = azureAdOptions.Audience;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidAudience = azureAdOptions.Audience,
                ValidIssuer = $"https://sts.windows.net/{azureAdOptions.JwtTenantId}/",
                NameClaimType = ClaimTypes.Name,
            };
        })
        .AddMcp(options =>
        {
            options.ResourceMetadata = new()
            {
                AuthorizationServers = { $"https://login.microsoftonline.com/{azureAdOptions.JwtTenantId}/v2.0" },
                ScopesSupported = [azureAdOptions.Scope],
            };
        });
        builder.Services.AddAuthorization();
        builder.Services.AddAuthorizationBuilder().AddPolicy(McpAuthenticationDefaults.AuthenticationScheme, policy =>
        {
            policy.AddAuthenticationSchemes(McpAuthenticationDefaults.AuthenticationScheme);
            policy.RequireAuthenticatedUser();
        });
        builder.Services.AddMcpServer()
            .WithRequestFilters(McpTelemetryMiddleware.RequestFilter)
            .WithTools<TodoItemTools>()
            .WithHttpTransport();
#endif
    }

    public static void AddKeyVaultIfConfigured(this IHostApplicationBuilder builder)
    {
        var keyVaultUri = builder.Configuration["AZURE_KEY_VAULT_ENDPOINT"];
        if (!string.IsNullOrWhiteSpace(keyVaultUri))
        {
            builder.Configuration.AddAzureKeyVault(
                new Uri(keyVaultUri),
                new DefaultAzureCredential());
        }
    }
}
