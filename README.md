# Hello World — Blazor + API

Een verkleinde kopie van de structuur van Enterprise Canonical: dezelfde lagen, dezelfde namen, een fractie van de code.
Bedoeld als opwarmer voor de rondleiding door de echte codebase.

## Draaien

Vereist de .NET 10 SDK. Twee terminals:

```bash
cd src/HelloWorld.Api    && dotnet run     # http://localhost:5070
cd src/HelloWorld.Client && dotnet run     # http://localhost:5071
```

Api niet gestart? Dan draait de client op ingebouwde demodata, met een banner. Dat is bewust.

## Testen

```bash
dotnet test     # vanuit de root van de repo
```

- `GreetingStoreTests.cs`: de store tegen een nagebootste Api en demodata. Terugval op demodata, rollback na een geweigerde begroeting, en de casus: een lege begroeting komt de lijst niet in, met én zonder Api.
- `ArchitectureTests.cs`: alleen `Kern` kent de `HttpClient`.

De twee casustests falen zonder de `if` in `GreetingStore.AddAsync` en slagen ermee.

## Structuur en wat het in het echte project is

| Hier | Echt (`c-enterprise-canonical`) | Rol |
|---|---|---|
| `Shared/Greeting.cs`, `HelloJson.cs` | `Shared/Application.cs`, `Json.cs` | Record en JSON-vorm, gedeeld door client en Api |
| `Api/Program.cs` (catch-all `/api/{**path}`) | `Api/Functions/ExecuteFunction.cs` | Eén ingang voor alle routes |
| `Api/Routing/EndpointRegistry.cs`, `Router.cs` | `Api/Routing/Endpoints.cs`, `Router.cs` | Route → procedure, declaratief |
| `Api/Procedures/GreetingProcedures.cs` | `Api/Sql/procedures/*.sql` | De business rules (hier in het geheugen, daar in SQL) |
| `Client/Kern/ApiClient.cs`, `StoreComponent.cs`, `DemoDataService.cs` | `Client/Kern/…` | Infrastructuur zonder domeinkennis |
| `Client/Stores/GreetingStore.cs` | `Client/Stores/Stores.cs`, `ApiSync.cs` | State, laden, optimistisch muteren met rollback |
| `Client/Features/Greetings/GreetingsPage.razor` | `Client/Features/Applicatie/ApplicationsPage.razor` | Eén feature, één map |
| `Client/Layout/MainLayout.razor` | `Client/Layout/MainLayout.razor` | Laadt de stores bij de eerste render |
| `Client/wwwroot/data/demo-greetings.json` | `Client/wwwroot/data/demo-*.json` | Demodata voor als de Api er niet is |
| `tests/HelloWorld.Tests/` | `tests/EnterpriseCanonical.Api.Tests/` | xUnit: storegedrag tegen een nagebootste backend, en een architectuurregel |

## Live bouwen in vier stappen

1. **Home** (`Pages/Home.razor`): een Blazor-component met markup, `@bind` en `@code`.
2. **Shared**: `Greeting` als record in een eigen project, door client én Api gebruikt.
3. **De Api-call**: `ApiClient` + de catch-all en de twee procedures. Laat `GET /api/greetings` in de browser zien.
4. **De store**: `GreetingStore` en `StoreComponent`. De pagina praat niet meer met HTTP.

Twee dingen om live te laten zien:

- **De regel**: voeg een lege begroeting toe. Hij verschijnt even en verdwijnt weer (rollback), met de melding uit de procedure.
- **De terugval**: stop de Api en herlaad. De pagina werkt op demodata.

## Wat hier bewust anders is

- Een gewone ASP.NET Core minimal API in plaats van Azure Functions, en een lijst in het geheugen in plaats van SQL Server. Geen Docker of Functions Core Tools nodig.
- Geen authenticatie, rollen, versies of retry.
