using System.Diagnostics;

namespace Cubido.Template.Application;

public static class Telemetry
{
    public static ActivitySource ActivitySource => new(typeof(Telemetry).Namespace!);
}
