namespace Cubido.Template.Application.FunctionalTests;

using System.Data.Common;

public interface ITestDatabase
{
    Task InitializeAsync();

    DbConnection GetConnection();

    Task ResetAsync();

    Task DisposeAsync();
}
