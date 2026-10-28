using HelloWorld.Client.Kern;
using HelloWorld.Shared;

namespace HelloWorld.Client.Stores;

/// <summary>
/// De datalaag voor begroetingen: houdt de lijst vast, laadt hem één keer (live of demo) en
/// meldt elke wijziging aan de pagina's. Pagina's lezen <see cref="Items"/> en roepen
/// <see cref="AddAsync"/> aan; ze weten niets van HTTP. (Echt: <c>Stores/Stores.cs</c>,
/// <c>ApplicationStore</c> op <c>EntityStore&lt;T&gt;</c>.)
/// </summary>
public sealed class GreetingStore(ApiClient api, DemoDataService demoData) : IChangeNotifier
{
    private const string Endpoint = "api/greetings";
    private readonly List<Greeting> _items = [];

    public event Action? OnChange;

    public IReadOnlyList<Greeting> Items => _items;
    public bool IsLoaded { get; private set; }
    public bool IsLoading { get; private set; }

    /// <summary>True als de Api niet bereikbaar was en de ingebouwde demodata is geladen.</summary>
    public bool IsDemo { get; private set; }

    /// <summary>De laatste mislukte actie, voor de pagina om te tonen.</summary>
    public string? LastError { get; private set; }

    /// <summary>Eén load per sessie. Faalt de Api, dan valt de store terug op demodata.</summary>
    public async Task LoadAsync()
    {
        if (IsLoaded || IsLoading) return;
        IsLoading = true;
        Notify();
        try
        {
            try
            {
                Replace(await api.GetRowsAsync<Greeting>(Endpoint));
            }
            catch (HttpRequestException)
            {
                IsDemo = true;
                Replace(await demoData.LoadGreetingsAsync());
            }
            IsLoaded = true;
        }
        finally
        {
            IsLoading = false;
            Notify();
        }
    }

    /// <summary>
    /// Optimistisch: de begroeting staat meteen in de lijst, daarna gaat hij naar de Api.
    /// Weigert de Api hem (bijvoorbeeld een lege tekst), dan draait de store het terug en
    /// zet de reden in <see cref="LastError"/>. In demo-modus blijft het lokaal.
    /// (Echt: <c>Stores/ApiSync.cs</c> — optimistic sync met rollback.)
    /// </summary>
    public async Task AddAsync(string text, string language)
    {
        LastError = null;
        var greeting = new Greeting { Id = Guid.NewGuid().ToString(), Text = text, Language = language };

        _items.Add(greeting);
        Notify();
        if (IsDemo) return;

        try
        {
            var saved = await api.PostAsync(Endpoint, greeting);
            _items[_items.IndexOf(greeting)] = saved;
        }
        catch (HttpRequestException ex)
        {
            _items.Remove(greeting);
            LastError = ex.Message;
        }
        Notify();
    }

    private void Replace(IEnumerable<Greeting> rows)
    {
        _items.Clear();
        _items.AddRange(rows);
    }

    private void Notify() => OnChange?.Invoke();
}
