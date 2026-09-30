namespace Cubido.Template.Application.FunctionalTests;

using Testcontainers.MsSql;

public class SqlContainerTestDatabase : SqlTestDatabase
{
    private MsSqlContainer? container;

    public override async Task InitializeAsync()
    {
        // https://github.com/testcontainers/testcontainers-dotnet/issues/1264
        container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU22-ubuntu-22.04")
            .WithAutoRemove(true)
            .Build();

        await container.StartAsync();
        ConnectionString = container.GetConnectionString();

        await base.InitializeAsync();
    }

    public override async Task DisposeAsync()
    {
        await base.DisposeAsync();
        if (container != null)
        {
            await container.DisposeAsync();
        }
    }
}
