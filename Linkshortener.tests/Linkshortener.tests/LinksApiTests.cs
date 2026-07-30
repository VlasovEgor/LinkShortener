using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LinkShortener.Tests;

public sealed class LinksApiTests : IClassFixture<LinkShortenerWebApplicationFactory>
{
    private readonly LinkShortenerWebApplicationFactory _factory;

    public LinksApiTests(LinkShortenerWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Post_ValidUrl_ReturnsBase62CodeWithExpectedLength()
    {
        using HttpClient client = CreateClient();

        using HttpResponseMessage response = await PostLinkAsync(client, "https://example.com/code-test");

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        string code = await ReadCodeAsync(response);

        Assert.InRange(code.Length, 6, 7);
        Assert.All(code, c =>
            Assert.True(
                char.IsAsciiLetterOrDigit(c),
                $"Character '{c}' is not a base62 character."));
    }

    [Fact]
    public async Task Post_SameIdempotencyKey_ReturnsSameCodeAndCreatesOneRow()
    {
        using HttpClient client = CreateClient();
        string idempotencyKey = $"test-{Guid.NewGuid():N}";

        using HttpResponseMessage firstResponse = await PostLinkAsync(
            client, "https://example.com/idempotency", idempotencyKey);

        using HttpResponseMessage secondResponse = await PostLinkAsync(
            client, "https://example.com/idempotency", idempotencyKey);

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);

        string firstCode = await ReadCodeAsync(firstResponse);
        string secondCode = await ReadCodeAsync(secondResponse);

        Assert.Equal(firstCode, secondCode);

        using IServiceScope scope = _factory.Services.CreateScope();
        LinkDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<LinkDbContext>();

        int rowCount = await dbContext.ShortLinks.CountAsync(link =>
            link.IdempotencyKey == idempotencyKey);

        Assert.Equal(1, rowCount);
    }

    [Fact]
    public async Task Get_NonExistingCode_Returns404()
    {
        using HttpClient client = CreateClient();

        using HttpResponseMessage response = await client.GetAsync("/definitely-not-existing-code");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_ExistingCode_IncrementsClickCount()
    {
        using HttpClient client = CreateClient();

        using HttpResponseMessage createResponse = await PostLinkAsync(
            client, "https://example.com/click-test");

        string code = await ReadCodeAsync(createResponse);

        using HttpResponseMessage redirectResponse = await client.GetAsync($"/{code}");

        Assert.Equal(HttpStatusCode.Found, redirectResponse.StatusCode);

        using HttpResponseMessage statisticsResponse = await client.GetAsync($"/api/links/{code}");

        Assert.Equal(HttpStatusCode.OK, statisticsResponse.StatusCode);

        using JsonDocument document = JsonDocument.Parse(await statisticsResponse.Content.ReadAsStringAsync());

        int clickCount = document.RootElement.GetProperty("clickCount").GetInt32();

        Assert.Equal(1, clickCount);
    }

    [Fact]
    public async Task Post_InvalidUrl_Returns400WithProblemDetails()
    {
        using HttpClient client = CreateClient();

        using HttpResponseMessage response = await PostLinkAsync(client, "not-a-url");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_SecondRequest_UsesCachedUrl()
    {
        using HttpClient client = CreateClient();
        const string originalUrl = "https://example.com/cache-test";

        using HttpResponseMessage createResponse = await PostLinkAsync(client, originalUrl);

        string code = await ReadCodeAsync(createResponse);
        
        using HttpResponseMessage firstRedirect = await client.GetAsync($"/{code}");

        Assert.Equal(HttpStatusCode.Found, firstRedirect.StatusCode);
        
        using (IServiceScope scope = _factory.Services.CreateScope())
        {
            LinkDbContext dbContext = scope.ServiceProvider.GetRequiredService<LinkDbContext>();
            await dbContext.ShortLinks.Where(link => link.Code == code).ExecuteDeleteAsync();
        }
        
        using HttpResponseMessage secondRedirect = await client.GetAsync($"/{code}");

        Assert.Equal(HttpStatusCode.Found, secondRedirect.StatusCode);
        Assert.Equal(originalUrl, secondRedirect.Headers.Location?.ToString());
    }

    private HttpClient CreateClient()
    {
        return _factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                BaseAddress = new Uri("https://localhost")
            });
    }

    private async Task<HttpResponseMessage> PostLinkAsync(HttpClient client, string url, string? idempotencyKey = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/links")
        {
            Content = JsonContent.Create(new { url })
        };

        if (idempotencyKey is not null)
            request.Headers.TryAddWithoutValidation("Idempotency-Key", idempotencyKey);
        
        return await client.SendAsync(request);
    }

    private async Task<string> ReadCodeAsync(HttpResponseMessage response)
    {
        string json = await response.Content.ReadAsStringAsync();
        using JsonDocument document = JsonDocument.Parse(json);

        string? code = document.RootElement.GetProperty("code").GetString();

        return Assert.IsType<string>(code);
    }
}