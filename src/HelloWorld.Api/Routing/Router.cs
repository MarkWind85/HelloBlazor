namespace HelloWorld.Api.Routing;

public static class Router
{
    /// <summary>Zoekt het endpoint bij methode + pad. Null = onbekende route (404).</summary>
    public static Endpoint? Match(string method, string path)
        => EndpointRegistry.Endpoints.FirstOrDefault(e => e.Method == method && e.Path == path);
}
