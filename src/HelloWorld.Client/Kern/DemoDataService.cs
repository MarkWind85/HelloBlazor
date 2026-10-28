using System.Net.Http.Json;
using HelloWorld.Shared;

namespace HelloWorld.Client.Kern;

/// <summary>
/// Leest de ingebouwde demodata uit wwwroot/data. Dezelfde JSON-vorm als de Api, dus de store
/// hoeft niet te weten waar zijn rijen vandaan komen. Dit is wat de app laat draaien als de
/// Api er niet is. (Echt: <c>Kern/DemoDataService.cs</c>, data gegenereerd uit de seed.)
/// </summary>
public sealed class DemoDataService(HttpClient appOrigin)
{
    public async Task<List<Greeting>> LoadGreetingsAsync()
        => await appOrigin.GetFromJsonAsync<List<Greeting>>("data/demo-greetings.json", HelloJson.Wire) ?? [];
}
