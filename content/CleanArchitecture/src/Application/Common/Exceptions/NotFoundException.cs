using System.Diagnostics.CodeAnalysis;

namespace Cubido.Template.Application.Common.Exceptions;

public class NotFoundException(string message) : Exception(message)
{
    public static void ThrowIfNull<T, Key>([NotNull] T? value, Key? key = default) where T : class
    {
        if (value is null)
        {
            throw new NotFoundException($"Queried object {typeof(T).Name} was not found{(key is null ? string.Empty : $", Key: {key}")}.");
        }
    }

    public static void ThrowIfNull<T>([NotNull] T? value) where T : class
    {
        if (value is null)
        {
            throw new NotFoundException($"Queried object {typeof(T).Name} was not found.");
        }
    }
}
