using Reqnroll;

namespace EventCo.Api.Tests.Support;

[Binding]
public static class Hooks
{
    public static ApiWebApplicationFactory Factory { get; private set; } = null!;

    [BeforeTestRun]
    public static async Task BeforeTestRun()
    {
        Factory = new ApiWebApplicationFactory();
        await Factory.InitializeAsync();
    }

    [AfterTestRun]
    public static async Task AfterTestRun()
    {
        await Factory.StopAsync();
    }
}
