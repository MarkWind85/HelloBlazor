using HelloWorld.Api.Procedures;
using HelloWorld.Api.Routing;
using HelloWorld.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<GreetingProcedures>();

// De client draait op een andere poort dan de Api; zonder CORS blokkeert de browser de call.
// (Echt: in local.settings.json, en gedeployd proxyt de Static Web App /api hierheen.)
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins("http://localhost:5071")
    .AllowAnyHeader()
    .AllowAnyMethod()));

var app = builder.Build();
app.UseCors();

// Eén catch-all voor alles onder /api. Geen controllers: route → procedure, zie EndpointRegistry.
// (Echt: Api/Functions/ExecuteFunction.cs, een Azure Function met Route = "{*path}".)
app.Map("/api/{**path}", async (HttpRequest req, string? path, GreetingProcedures procs) =>
{
    var endpoint = Router.Match(req.Method, "/" + path);
    if (endpoint is null) return Results.Json(new { error = "Onbekende route" }, statusCode: 404);

    try
    {
        object result = endpoint.Proc switch
        {
            "sp_greetings_list" => procs.sp_greetings_list(),
            "sp_greetings_create" => procs.sp_greetings_create(
                await req.ReadFromJsonAsync<Greeting>(HelloJson.Wire) ?? new Greeting()),
            _ => throw new InvalidOperationException($"Procedure {endpoint.Proc} bestaat niet."),
        };
        return Results.Json(result, HelloJson.Wire, statusCode: endpoint.Status);
    }
    catch (RuleViolation ex)
    {
        return Results.Json(new { error = ex.Message }, statusCode: 400);
    }
});

app.Run();
