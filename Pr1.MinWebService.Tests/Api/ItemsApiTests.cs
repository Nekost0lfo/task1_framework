using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Pr1.MinWebService.Domain;
using Xunit;

namespace Pr1.MinWebService.Tests.Api;

/// <summary>
/// Проверки взаимодействия с веб-службой: создание элемента и получение его обратно,
/// запрос по несуществующему идентификатору возвращает согласованный ответ об ошибке.
/// </summary>
public sealed class ItemsApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ItemsApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateItem_ThenGetById_ReturnsSameItem()
    {
        var create = new CreateItemRequest("Test Game", 2023);
        var createResponse = await _client.PostAsJsonAsync("/api/items", create);
        createResponse.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<Item>(JsonOptions);
        Assert.NotNull(created);
        Assert.Equal(create.Name, created.Name);
        Assert.Equal(create.Year, created.Year);

        var getResponse = await _client.GetAsync($"/api/items/{created.Id}");
        getResponse.EnsureSuccessStatusCode();
        var retrieved = await getResponse.Content.ReadFromJsonAsync<Item>(JsonOptions);
        Assert.NotNull(retrieved);
        Assert.Equal(created.Id, retrieved.Id);
        Assert.Equal(created.Name, retrieved.Name);
        Assert.Equal(created.Year, retrieved.Year);
    }

    [Fact]
    public async Task GetByNonExistentId_Returns404_WithConsistentErrorFormat()
    {
        var id = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var response = await _client.GetAsync($"/api/items/{id}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<ErrorResponse>(body, JsonOptions);
        Assert.NotNull(error);
        Assert.Equal("not_found", error.Code);
        Assert.False(string.IsNullOrEmpty(error.Message));
        Assert.False(string.IsNullOrEmpty(error.RequestId));
    }

    [Fact]
    public async Task Post_EmptyName_Returns400_WithConsistentErrorFormat()
    {
        var create = new CreateItemRequest("", 2020);
        var response = await _client.PostAsJsonAsync("/api/items", create);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<ErrorResponse>(body, JsonOptions);
        Assert.NotNull(error);
        Assert.Equal("validation", error.Code);
        Assert.Contains("name", error.Message, StringComparison.OrdinalIgnoreCase);
        Assert.False(string.IsNullOrEmpty(error.RequestId));
    }

    [Fact]
    public async Task Post_NegativeYear_Returns400_WithConsistentErrorFormat()
    {
        var create = new CreateItemRequest("Игра", -5);
        var response = await _client.PostAsJsonAsync("/api/items", create);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<ErrorResponse>(body, JsonOptions);
        Assert.NotNull(error);
        Assert.Equal("validation", error.Code);
        Assert.Contains("year", error.Message, StringComparison.OrdinalIgnoreCase);
        Assert.False(string.IsNullOrEmpty(error.RequestId));
    }

    [Fact]
    public async Task GetItems_Returns200_AndArray()
    {
        var response = await _client.GetAsync("/api/items");
        response.EnsureSuccessStatusCode();
        var items = await response.Content.ReadFromJsonAsync<Item[]>(JsonOptions);
        Assert.NotNull(items);
    }

    [Fact]
    public async Task Response_ContainsRequestIdHeader()
    {
        var response = await _client.GetAsync("/api/items");
        response.EnsureSuccessStatusCode();
        Assert.True(response.Headers.TryGetValues("X-Request-Id", out var values));
        var requestId = values!.FirstOrDefault();
        Assert.False(string.IsNullOrEmpty(requestId));
    }
}
