namespace HelloWorld.Shared;

/// <summary>
/// Eén begroeting. Dit is het record dat Client én Api gebruiken: dezelfde klasse aan
/// beide kanten van de draad. (In het echte project: <c>Shared/Application.cs</c>.)
/// </summary>
public record Greeting
{
    public string Id { get; set; } = "";
    public string Text { get; set; } = "";
    public string Language { get; set; } = "nl";
}
