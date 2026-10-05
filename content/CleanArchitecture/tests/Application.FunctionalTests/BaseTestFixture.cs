using static Cubido.Template.Application.FunctionalTests.Testing;

namespace Cubido.Template.Application.FunctionalTests;

[TestFixture]
public abstract class BaseTestFixture
{
    [SetUp]
    public async Task TestSetUp()
    {
        await ResetState();
    }
}
