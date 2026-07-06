using System.Diagnostics;

namespace Cubido.Template.Web;

public class McpTelemetryMiddleware
{
    public static void RequestFilter(IMcpRequestFilterBuilder requestFilterBuilder)
    {
        requestFilterBuilder.AddCallToolFilter(next => (context, cancellationToken) =>
        {
            // see https://github.com/open-telemetry/semantic-conventions-genai/blob/main/docs/gen-ai/mcp.md#server
            if (context.MatchedPrimitive is { Id: { } primitiveId })
            {
                Activity.Current?.AddTag("gen_ai.tool.name", primitiveId);
            }
            return next(context, cancellationToken);
        });
    }
}
