using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Cubido.Template.Application.Common.Interfaces;
using Cubido.Template.Infrastructure.Data;
using Cubido.Template.Web;
using Cubido.Template.Web.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Sqiddler.OpenApi;
using System.Reflection;

#if (UseEntraIdAuthentication)
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Security.Claims;

#if (IncludeMcpServer)
using ModelContextProtocol.AspNetCore.Authentication;
#endif
#endif

#if (IncludeMcpServer)
using Cubido.Template.Web.Tools;
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
        builder.Services.AddAntiforgery(options => { options.HeaderName = "X-XSRF-TOKEN"; });

        // Configure forwarded headers for Azure Container Apps
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
                | ForwardedHeaders.XForwardedProto
                | ForwardedHeaders.XForwardedHost;
            // By default, ForwardedHeadersOptions only allows loopback addresses.
            // We need to accept the traffic from Azure Container Apps Ingress.
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });


        builder.Services.AddDataProtection()
            .PersistKeysToDbContext<ApplicationDbContext>()
            .SetApplicationName("Cubido.Template.Web");
#if (UseEntraIdAuthentication)

        var azureAdOptions = builder.Configuration.GetRequiredSection("AzureAd").Get<AzureAdOptions>()!;
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
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
#if (IncludeMcpServer)
        .AddMcp(options =>
        {
            options.ResourceMetadata = new()
            {
                AuthorizationServers = { $"https://login.microsoftonline.com/{azureAdOptions.JwtTenantId}/v2.0" },
                ScopesSupported = [azureAdOptions.McpScope],
            };
        })
#endif
        .AddMicrosoftIdentityWebApp(
                builder.Configuration.GetSection("AzureAd"),
                openIdConnectScheme: OpenIdConnectDefaults.AuthenticationScheme,
                cookieScheme: CookieAuthenticationDefaults.AuthenticationScheme);

        builder.Services.AddAuthorizationBuilder()
            .AddDefaultPolicy("default", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AuthenticationSchemes = [JwtBearerDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme];
            })
#if (IncludeMcpServer)
            .AddPolicy(McpAuthenticationDefaults.AuthenticationScheme, policy =>
            {
                policy.AddAuthenticationSchemes(McpAuthenticationDefaults.AuthenticationScheme);
                policy.RequireAuthenticatedUser();
            })
#endif
            ;

        builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
        {
            options.TokenValidationParameters.NameClaimType = "name";
            options.TokenValidationParameters.RoleClaimType = ClaimConstants.Role;

            options.Events.OnRedirectToIdentityProvider = context =>
            {
                if (!context.Request.Path.StartsWithSegments("/api/auth"))
                {
                    // Always return 401 except when trying to access the /api/auth endpoint
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    context.HandleResponse();
                }
                else if (builder.Environment.IsDevelopment())
                {
                    // Only overwrite redirect url when running over the Angular proxy
                    context.ProtocolMessage.RedirectUri = "http://localhost:4200/signin-oidc";
                }
                return Task.CompletedTask;
            };
        });
        builder.Services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.LogoutPath = "/auth/logout";
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return Task.CompletedTask;
            };
        });
#endif

#if (IncludeMcpServer)
        builder.Services.AddMcpServer()
            .WithRequestFilters(McpTelemetryMiddleware.RequestFilter)
            .WithTools<TodoItemTools>()
            .WithHttpTransport();
#endif
    }


    public static void AddKeyVaultIfConfigured(this IHostApplicationBuilder builder)
    {
        // Skip KeyVault for OpenAPI generator runs
        if (Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider")
        {
            return;
        }

        var keyVaultUri = builder.Configuration["KeyVault:Uri"];
        if (!string.IsNullOrWhiteSpace(keyVaultUri))
        {
            var defaultCredentials = new DefaultAzureCredential();

            builder.Configuration.AddAzureKeyVault(
                new Uri(keyVaultUri),
                defaultCredentials);

            builder.Services.AddDataProtection()
                .ProtectKeysWithAzureKeyVault(builder.Configuration["KeyVault:DataProtectionKeyUri"], defaultCredentials);
        }
    }


    public static void AddOpenTelemetryInstrumentation(this IHostApplicationBuilder builder)
    {
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource.AddService(serviceName: builder.Environment.ApplicationName, serviceNamespace: typeof(Telemetry).Namespace);
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSqlClientInstrumentation();

                // reduce excessive metrics from HttpClient instrumentation
                metrics
                    .AddView("http.client.open_connections", new MetricStreamConfiguration() { TagKeys = [] })
                    .AddView("http.connection.duration", new MetricStreamConfiguration() { TagKeys = [] })
                    .AddView("http.client.active_requests", new MetricStreamConfiguration() { TagKeys = [] })
                    .AddView("http.server.active_requests", new MetricStreamConfiguration() { TagKeys = [] })
                    .AddView("http.client.request.duration", MetricStreamConfiguration.Drop)
                    .AddView("http.server.request.duration", MetricStreamConfiguration.Drop);

                metrics
                    .AddView("kestrel.connection.duration", MetricStreamConfiguration.Drop)
                    .AddView("kestrel.active_connections", MetricStreamConfiguration.Drop)
                    .AddView("kestrel.queued_connections", MetricStreamConfiguration.Drop);
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(
                        builder.Environment.ApplicationName,
                        Telemetry.ActivitySource.Name,
                        Cubido.Template.Infrastructure.Telemetry.ActivitySource.Name,
                        Cubido.Template.Application.Telemetry.ActivitySource.Name
                    )
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSqlClientInstrumentation()
                    .AddProcessor<SuppressedActivityProcessor>();
            });

        var connectionString = builder.Configuration["AzureMonitor:ConnectionString"];
        if (!string.IsNullOrEmpty(connectionString))
        {
            builder.Services.AddOpenTelemetry().UseAzureMonitor(options =>
            {
                options.ConnectionString = connectionString;
                options.SamplingRatio = 0.1f;
            });
        }
    }
}
