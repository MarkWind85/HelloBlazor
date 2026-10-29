using System.Net;
using HelloWorld.Client.Kern;
using HelloWorld.Client.Stores;
using HelloWorld.Shared;
using HelloWorld.Tests.Support;

namespace HelloWorld.Tests;

// GreetingStore tegen een nagebootste Api en nagebootste demodata: de terugval, de rollback,
// en de casus — een lege begroeting komt de lijst niet in, met én zonder Api.
public class GreetingStoreTests
{
    private const string DemoJson = """
        [
          { "id": "demo-1", "text": "Hallo (demo)", "language": "nl" },
          { "id": "demo-2", "text": "Hello (demo)", "language": "en" }
        ]
        """;

    private static readonly Greeting[] DemoRows =
    [
        new() { Id = "demo-1", Text = "Hallo (demo)", Language = "nl" },
        new() { Id = "demo-2", Text = "Hello (demo)", Language = "en" },
    ];

    private static (GreetingStore Store, FakeHandler Api) Setup(Func<HttpRequestMessage, HttpResponseMessage> api)
    {
        var apiHandler = new FakeHandler(api);
        var demoHandler = new FakeHandler(_ => FakeHandler.Json(DemoJson));
        var store = new GreetingStore(
            new ApiClient(apiHandler.CreateClient()),
            new DemoDataService(demoHandler.CreateClient()));
        return (store, apiHandler);
    }

    [Fact]
    public async Task Valt_terug_op_demodata_als_de_Api_er_niet_is()
    {
        var (store, _) = Setup(FakeHandler.Unreachable);

        await store.LoadAsync();

        Assert.True(store.IsDemo);
        Assert.Equal(DemoRows, store.Items);
    }

    [Fact]
    public async Task Draait_een_geweigerde_begroeting_terug()
    {
        var (store, _) = Setup(req => req.Method == HttpMethod.Post
            ? FakeHandler.Json("""{"error":"Nee."}""", HttpStatusCode.BadRequest)
            : FakeHandler.Json("[]"));
        await store.LoadAsync();

        await store.AddAsync("Hoi", "nl");

        Assert.Empty(store.Items);
        Assert.Equal("Nee.", store.LastError);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Een_lege_begroeting_gaat_niet_de_deur_uit(string text)
    {
        // De Api weigert hem net als de echte. Lijst en melding kloppen daardoor ook zonder
        // de check in de store (rollback); alleen het aantal POSTs laat het verschil zien.
        var (store, api) = Setup(req => req.Method == HttpMethod.Post
            ? FakeHandler.Json("""{"error":"Een begroeting heeft tekst nodig."}""", HttpStatusCode.BadRequest)
            : FakeHandler.Json("[]"));
        await store.LoadAsync();

        await store.AddAsync(text, "nl");

        Assert.Equal("Een begroeting heeft tekst nodig.", store.LastError);
        Assert.Empty(store.Items);
        Assert.DoesNotContain(api.Calls, call => call.Method == HttpMethod.Post);
    }

    [Fact]
    public async Task Ook_zonder_Api_wordt_een_lege_begroeting_geweigerd()
    {
        // De casus: in demo-modus is er geen Api die weigert, dus de store moet het zelf doen.
        var (store, _) = Setup(FakeHandler.Unreachable);
        await store.LoadAsync();

        await store.AddAsync("", "nl");

        Assert.Equal(DemoRows.Length, store.Items.Count);
        Assert.Equal("Een begroeting heeft tekst nodig.", store.LastError);
    }
}
