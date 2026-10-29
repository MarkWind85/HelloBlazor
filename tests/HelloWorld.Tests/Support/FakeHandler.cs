using System.Net;
using System.Text;

namespace HelloWorld.Tests.Support;

/// <summary>
/// Een nagebootste backend: antwoordt via een delegate en noteert elke request. De eenvoudige vorm van
/// <c>tests/EnterpriseCanonical.Api.Tests/Support/FakeApiHandler.cs</c>, die elke request ook door de echte router stuurt.
/// </summary>
public sealed class FakeHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
{
    public List<(HttpMethod Method, string Path)> Calls { get; } = [];

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        Calls.Add((request.Method, request.RequestUri!.AbsolutePath));
        return Task.FromResult(respond(request));
    }

    /// <summary>Een HttpClient op deze handler. ApiClient en DemoDataService vragen relatieve paden.</summary>
    public HttpClient CreateClient() => new(this) { BaseAddress = new Uri("http://localhost/") };

    public static HttpResponseMessage Json(string json, HttpStatusCode status = HttpStatusCode.OK)
        => new(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    /// <summary>De Api draait niet: HttpClient gooit dit, en dat is wat <c>GreetingStore.LoadAsync</c> vangt.</summary>
    public static HttpResponseMessage Unreachable(HttpRequestMessage _)
        => throw new HttpRequestException("Api niet bereikbaar");
}
