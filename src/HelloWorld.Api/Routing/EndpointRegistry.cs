namespace HelloWorld.Api.Routing;

/// <summary>
/// Eén declaratieve regel per endpoint: methode, pad, de "procedure" die het werk doet en de
/// success-status. (In het echte project: <c>Api/Routing/Endpoints.cs</c> — daar is de
/// procedure een stored procedure in SQL Server.)
/// </summary>
public sealed record Endpoint(string Method, string Path, string Proc, int Status);

public static class EndpointRegistry
{
    public static readonly Endpoint[] Endpoints =
    [
        new("GET",  "/greetings", "sp_greetings_list",   200),
        new("POST", "/greetings", "sp_greetings_create", 201),
    ];
}
