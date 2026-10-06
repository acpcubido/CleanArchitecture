using System.Diagnostics;

namespace Cubido.Template.Web;

public static class Telemetry
{
    public static ActivitySource ActivitySource => new(typeof(Telemetry).Namespace!);
}
