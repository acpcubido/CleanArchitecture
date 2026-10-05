using System.Diagnostics;

namespace Cubido.Template.Infrastructure;

public static class Telemetry
{
    public static ActivitySource ActivitySource => new(typeof(Telemetry).Namespace!);
}
