using HelloWorld.Client;
using HelloWorld.Client.Kern;
using HelloWorld.Client.Stores;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ApiBaseUrl (wwwroot/appsettings*.json): leeg = same-origin, gevuld = losse Api-host
// zoals lokaal. Hetzelfde mechanisme als in het echte project.
var apiBaseUrl = builder.Configuration["ApiBaseUrl"];
var appOrigin = new Uri(builder.HostEnvironment.BaseAddress);

builder.Services.AddScoped(sp => new ApiClient(
    new HttpClient { BaseAddress = string.IsNullOrEmpty(apiBaseUrl) ? appOrigin : new Uri(apiBaseUrl) }));
builder.Services.AddScoped(sp => new DemoDataService(new HttpClient { BaseAddress = appOrigin }));

// Scoped, maar in WASM is er één root-scope: dit gedraagt zich als een singleton.
builder.Services.AddScoped<GreetingStore>();

await builder.Build().RunAsync();
