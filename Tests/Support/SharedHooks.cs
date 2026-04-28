using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

namespace Tests.Support;

[Binding]
public class SharedHooks
{
    private readonly ScenarioState _state;

    public SharedHooks(ScenarioState state)
    {
        _state = state;
    }

    [BeforeScenario(Order = 1)]
    public async Task BeforeScenario()
    {
        _state.Factory = new CatalogApiFactory();
        _state.Client = _state.Factory.CreateClient();

        // Verifica que la API responde
        var ping = await _state.Client.GetAsync("/api/v1/categories");
        Console.WriteLine($"[DEBUG] API ping status: {ping.StatusCode}");

        using var scope = _state.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    [AfterScenario]
    public async Task AfterScenario()
    {
        _state.Client.Dispose();
        await _state.Factory.DisposeAsync();
    }
}