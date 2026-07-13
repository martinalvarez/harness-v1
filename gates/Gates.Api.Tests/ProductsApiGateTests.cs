using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Gates.Api.Tests;

/// <summary>
/// Acceptance gates derived 1:1 from /specs/verification.md (AC-01..AC-13).
/// Black-box: talks to a running instance of the API over HTTP, started by
/// scripts/verify. Does not reference /src at compile time, so this project
/// builds even when /src is empty. Until the API exists/runs, these tests
/// are expected to fail (connection refused) — that failure IS the gate.
/// </summary>
[Collection("Api")]
public sealed class ProductsApiGateTests
{
    private readonly HttpClient _client;

    public ProductsApiGateTests(ApiFixture fixture)
    {
        _client = fixture.Client;
    }

    private static object ValidProduct(string name, decimal price = 9.99m, int stock = 10) =>
        new { Name = name, Price = price, Stock = stock };

    private static string UniqueName(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

    [Fact]
    public async Task AC01_GetAll_Returns200WithArray()
    {
        var response = await _client.GetAsync("/api/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<object>>();
        Assert.NotNull(body);
    }

    [Fact]
    public async Task AC02_Post_ValidProduct_Returns201WithLocationAndId()
    {
        var response = await _client.PostAsJsonAsync("/api/products", ValidProduct(UniqueName("AC02")));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var created = await response.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created!.Id);
    }

    [Fact]
    public async Task AC03_GetById_Existing_Returns200WithMatchingData()
    {
        var name = UniqueName("AC03");
        var create = await _client.PostAsJsonAsync("/api/products", ValidProduct(name, price: 12.5m, stock: 3));
        var created = await create.Content.ReadFromJsonAsync<ProductResponse>();

        var response = await _client.GetAsync($"/api/products/{created!.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var fetched = await response.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.Equal(name, fetched!.Name);
        Assert.Equal(12.5m, fetched.Price);
        Assert.Equal(3, fetched.Stock);
    }

    [Fact]
    public async Task AC04_GetById_NonExistent_Returns404()
    {
        var response = await _client.GetAsync($"/api/products/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AC05_Post_PriceNotPositive_Returns400MentioningPrice()
    {
        var response = await _client.PostAsJsonAsync("/api/products", ValidProduct(UniqueName("AC05"), price: 0m));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var detail = await response.Content.ReadAsStringAsync();
        Assert.Contains("Price", detail, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AC06_Post_EmptyName_Returns400MentioningName()
    {
        var response = await _client.PostAsJsonAsync("/api/products", ValidProduct(""));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var detail = await response.Content.ReadAsStringAsync();
        Assert.Contains("Name", detail, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AC07_Post_DuplicateName_Returns400()
    {
        var name = UniqueName("AC07");
        var first = await _client.PostAsJsonAsync("/api/products", ValidProduct(name));
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var duplicate = await _client.PostAsJsonAsync("/api/products", ValidProduct(name.ToUpperInvariant()));

        Assert.Equal(HttpStatusCode.BadRequest, duplicate.StatusCode);
    }

    [Fact]
    public async Task AC08_Post_NegativeStock_Returns400MentioningStock()
    {
        var response = await _client.PostAsJsonAsync("/api/products", ValidProduct(UniqueName("AC08"), stock: -1));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var detail = await response.Content.ReadAsStringAsync();
        Assert.Contains("Stock", detail, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AC09_Put_Existing_ValidData_Returns200Updated()
    {
        var create = await _client.PostAsJsonAsync("/api/products", ValidProduct(UniqueName("AC09")));
        var created = await create.Content.ReadFromJsonAsync<ProductResponse>();

        var updatedName = UniqueName("AC09-updated");
        var response = await _client.PutAsJsonAsync(
            $"/api/products/{created!.Id}", ValidProduct(updatedName, price: 55m, stock: 7));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.Equal(updatedName, updated!.Name);
        Assert.Equal(55m, updated.Price);
        Assert.Equal(7, updated.Stock);
    }

    [Fact]
    public async Task AC10_Put_NonExistent_Returns404()
    {
        var response = await _client.PutAsJsonAsync(
            $"/api/products/{Guid.NewGuid()}", ValidProduct(UniqueName("AC10")));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AC11_Put_InvalidData_Returns400()
    {
        var create = await _client.PostAsJsonAsync("/api/products", ValidProduct(UniqueName("AC11")));
        var created = await create.Content.ReadFromJsonAsync<ProductResponse>();

        var response = await _client.PutAsJsonAsync(
            $"/api/products/{created!.Id}", ValidProduct(UniqueName("AC11-b"), price: -5m));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AC12_Delete_Existing_Returns204ThenGetReturns404()
    {
        var create = await _client.PostAsJsonAsync("/api/products", ValidProduct(UniqueName("AC12")));
        var created = await create.Content.ReadFromJsonAsync<ProductResponse>();

        var delete = await _client.DeleteAsync($"/api/products/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var getAfter = await _client.GetAsync($"/api/products/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfter.StatusCode);
    }

    [Fact]
    public async Task AC13_Delete_NonExistent_Returns404()
    {
        var response = await _client.DeleteAsync($"/api/products/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private sealed class ProductResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}
