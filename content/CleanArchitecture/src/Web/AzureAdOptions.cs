namespace Cubido.Template.Web;

public class AzureAdOptions
{
    public string Audience { get; set; } = default!;
    public string JwtTenantId { get; set; } = default!;
    public string McpScope { get; set; } = default!;
}
