using System.Reflection;

namespace Cubido.Template.Web.Infrastructure;

public static class ArgumentExceptionExtensions
{
    public static bool IsAnonymous(this MethodInfo method)
    {
        var invalidChars = new[] { '<', '>' };
        return method.Name.Any(invalidChars.Contains);
    }

    extension(ArgumentException)
    {
        public static void ThrowIfAnonymous(Delegate input)
        {
            if (input.Method.IsAnonymous())
            {
                throw new ArgumentException("The endpoint name must be specified when using anonymous handlers.");
            }
        }
    }
}
