using OpenTelemetry;
using System.Diagnostics;

namespace Cubido.Template.Web.Infrastructure;

/// <summary>
/// Filters out activities marked for suppression
/// </summary>
public class SuppressedActivityProcessor : BaseProcessor<Activity>
{
    public override void OnStart(Activity data)
    {
        // Check if this activity or any of its parents are marked for suppression
        if (ShouldSuppress(data))
        {
            data.IsAllDataRequested = false;
        }
    }

    private static bool ShouldSuppress(Activity activity)
    {
        var current = activity;
        while (current != null)
        {
            if (current.GetTagItem("suppress.telemetry") is true)
            {
                return true;
            }
            current = current.Parent;
        }
        return false;
    }
}
