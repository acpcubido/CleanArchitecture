namespace Cubido.Template.Web.Infrastructure;

public static class ArgumentExceptionExtensions
{
    extension(ArgumentException)
    {
        public static void ThrowIfAnonymous(Delegate input)
        {
            var invalidChars = new[] { '<', '>' };
            if (input.Method.Name.Any(invalidChars.Contains))
            {
                throw new ArgumentException("The endpoint name must be specified when using anonymous handlers.");
            }
        }
    }
}
