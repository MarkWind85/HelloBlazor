using System.Text.Json;

namespace HelloWorld.Shared;

/// <summary>
/// De JSON-vorm van de draad, op één plek. Client en Api serialiseren allebei hiermee,
/// dus ze kunnen niet uit elkaar lopen. (In het echte project: <c>Shared/Json.cs</c>, <c>EcJson.Wire</c>.)
/// </summary>
public static class HelloJson
{
    public static readonly JsonSerializerOptions Wire = new(JsonSerializerDefaults.Web);
}
