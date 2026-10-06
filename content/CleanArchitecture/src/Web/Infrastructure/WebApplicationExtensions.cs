using Microsoft.AspNetCore.Antiforgery;
using System.Reflection;

namespace Cubido.Template.Web.Infrastructure;

public static class WebApplicationExtensions
{
    public static RouteGroupBuilder MapGroup(this WebApplication app, EndpointGroupBase group)
    {
        var groupName = group.GroupName ?? group.GetType().Name;

        return app
            .MapGroup($"/api/{groupName}")
            .WithTags(groupName);
    }

    public static WebApplication MapEndpoints(this WebApplication app)
    {
        var endpointGroupType = typeof(EndpointGroupBase);

        var assembly = Assembly.GetExecutingAssembly();

        var endpointGroupTypes = assembly.GetExportedTypes()
            .Where(t => t.IsSubclassOf(endpointGroupType));

        foreach (var type in endpointGroupTypes)
        {
            if (Activator.CreateInstance(type) is EndpointGroupBase instance)
            {
                instance.Map(app.MapGroup(instance));
            }
        }

        return app;
    }

    public static WebApplication UseAntiforgeryCookie(this WebApplication app)
    {
        app.Use(next => context =>
        {
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
                var tokens = antiforgery.GetAndStoreTokens(context);
                context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken ?? "", new CookieOptions() { HttpOnly = false, SameSite = SameSiteMode.Strict });
            }

            return next(context);
        });
        return app;
    }
}
