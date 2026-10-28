using System.Net.Http.Json;
using HelloWorld.Shared;

namespace HelloWorld.Client.Kern;

/// <summary>
/// De enige plek die HTTP spreekt. Stores praten hiermee, nooit met HttpClient zelf.
/// Een foutstatus wordt een exception met de melding uit de Api, zodat de store hem kan
/// tonen. (Echt: <c>Kern/ApiClient.cs</c>, plus retry bij een cold start.)
/// </summary>
public sealed class ApiClient(HttpClient http)
{
    public async Task<List<T>> GetRowsAsync<T>(string endpoint)
    {
        using var response = await http.GetAsync(endpoint);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<List<T>>(HelloJson.Wire) ?? [];
    }

    public async Task<T> PostAsync<T>(string endpoint, T body)
    {
        using var response = await http.PostAsJsonAsync(endpoint, body, HelloJson.Wire);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<T>(HelloJson.Wire))!;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;
        var body = await response.Content.ReadAsStringAsync();
        // De Api antwoordt { "error": "..." } bij een overtreden regel.
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("error", out var error))
                throw new HttpRequestException(error.GetString());
        }
        catch (System.Text.Json.JsonException) { /* geen JSON: val terug op de statuscode */ }
        throw new HttpRequestException($"HTTP {(int)response.StatusCode}");
    }
}
