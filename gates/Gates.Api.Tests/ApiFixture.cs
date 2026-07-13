using Xunit;

namespace Gates.Api.Tests;

/// <summary>
/// Black-box HTTP fixture: no compile-time reference to /src on purpose.
/// Gates must compile even when /src is empty; the API is expected to
/// already be running (started by scripts/verify) at <see cref="BaseUrl"/>.
/// </summary>
public sealed class ApiFixture : IDisposable
{
    public string BaseUrl { get; } =
        Environment.GetEnvironmentVariable("GATE_API_BASE_URL") ?? "http://localhost:5087";

    public HttpClient Client { get; }

    public ApiFixture()
    {
        Client = new HttpClient { BaseAddress = new Uri(BaseUrl) };
    }

    public void Dispose() => Client.Dispose();
}

[CollectionDefinition("Api")]
public sealed class ApiCollection : ICollectionFixture<ApiFixture>
{
}
