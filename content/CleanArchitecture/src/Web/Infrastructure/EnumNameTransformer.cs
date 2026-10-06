using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Text.Json.Nodes;

namespace Cubido.Template.Web.Infrastructure;

public class EnumNameTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (context.JsonTypeInfo.Type.IsEnum)
        {
            schema.Enum = [.. Enum.GetValues(context.JsonTypeInfo.Type)
                        .Cast<object>()
                        .Select(value => JsonValue.Create(Convert.ToInt32(value)))];

            var enumNames = new JsonArray();
            foreach (var enumName in Enum.GetNames(context.JsonTypeInfo.Type))
            {
                enumNames.Add(enumName);
            }

            schema.Extensions = new Dictionary<string, IOpenApiExtension>
            {
                ["x-enumNames"] = new JsonNodeExtension(enumNames)
            };
        }

        return Task.CompletedTask;
    }
}
