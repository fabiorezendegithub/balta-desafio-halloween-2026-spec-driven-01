using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PasswordGenerator.Application;
using PasswordGenerator.Data;

namespace PasswordGenerator.Tests.Api;

public class PasswordEndpointsTests : IDisposable
{
    private const string Endpoint = "/api/password";
    private const string Specials = "!@#$%^&*()-_=+[]{}";

    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"passwords-{Guid.NewGuid():N}.db");
    private readonly List<PasswordApiFactory> _factories = [];

    public void Dispose()
    {
        foreach (var factory in _factories)
        {
            factory.Dispose();
        }

        SqliteConnection.ClearAllPools();
        if (File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }
    }

    private PasswordApiFactory CreateFactory(IPasswordGenerator? generator = null)
    {
        var factory = new PasswordApiFactory(_databasePath, generator);
        _factories.Add(factory);
        return factory;
    }

    [Fact]
    public async Task Post_HappyPath_Returns201WithGuidAndWithoutPassword()
    {
        var client = CreateFactory().CreateClient();

        var response = await client.PostAsync(Endpoint, null);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(Guid.TryParse(body.GetProperty("guid").GetString(), out _));
        Assert.False(body.TryGetProperty("password", out _));
    }

    [Fact]
    public async Task Post_WhenInternalFailure_Returns500ProblemDetailsAndCreatesNoGuid()
    {
        var factory = CreateFactory(new FailingPasswordGenerator());
        var client = factory.CreateClient();

        var response = await client.PostAsync(Endpoint, null);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Erro interno", body.GetProperty("title").GetString());
        Assert.Equal("Ocorreu um erro interno. Tente novamente mais tarde.", body.GetProperty("detail").GetString());
        Assert.Equal(0, await CountRowsAsync(factory));
    }

    [Fact]
    public async Task Get_HappyPath_Returns200WithPasswordNoStoreAndRepeatable()
    {
        var client = CreateFactory().CreateClient();
        var guid = await CreateGuidAsync(client);

        var first = await GetAsync(client, guid.ToString("D"));
        var second = await GetAsync(client, guid.ToString("D"));

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Contains("no-store", first.Headers.CacheControl?.ToString() ?? string.Empty);
        var password = (await first.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("password").GetString();
        var repeated = (await second.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("password").GetString();
        Assert.Equal(16, password!.Length);
        Assert.Equal(password, repeated);
    }

    [Fact]
    public async Task Get_UnknownWellFormedGuid_Returns404ProblemDetailsDistinctFrom400()
    {
        var client = CreateFactory().CreateClient();

        var response = await GetAsync(client, Guid.NewGuid().ToString("D"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("O GUID informado não existe.", body.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Get_MissingHeader_Returns400ProblemDetails()
    {
        var client = CreateFactory().CreateClient();

        var response = await client.GetAsync(Endpoint);

        await AssertValidationErrorAsync(response);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("nao-e-um-guid")]
    [InlineData("3f2504e04f8911d39a0c0305e82c3301")]
    [InlineData("{3f2504e0-4f89-11d3-9a0c-0305e82c3301}")]
    public async Task Get_EmptyOrMalformedHeader_Returns400ProblemDetails(string header)
    {
        var client = CreateFactory().CreateClient();

        var response = await GetAsync(client, header);

        await AssertValidationErrorAsync(response);
    }

    [Fact]
    public async Task Get_RepeatedAcrossRestart_ReturnsSamePassword()
    {
        // Persistência (RF-04): cria, reinicia a aplicação e consulta
        Guid guid;
        string password;

        using (var first = CreateFactory())
        {
            var client = first.CreateClient();
            guid = await CreateGuidAsync(client);
            password = await ReadPasswordAsync(client, guid);
        }

        var restarted = new PasswordApiFactory(_databasePath);
        _factories.Add(restarted);
        var afterRestart = await ReadPasswordAsync(restarted.CreateClient(), guid);

        Assert.Equal(password, afterRestart);
    }

    [Fact]
    public async Task Acceptance_ManyCreations_YieldUniqueGuidsAndValidPasswords()
    {
        var factory = CreateFactory();
        var client = factory.CreateClient();

        var guids = new HashSet<Guid>();
        var passwords = new HashSet<string>();
        for (var i = 0; i < 25; i++)
        {
            var guid = await CreateGuidAsync(client);
            guids.Add(guid);
            passwords.Add(await ReadPasswordAsync(client, guid));
        }

        Assert.Equal(25, guids.Count);
        Assert.Equal(25, passwords.Count);
        Assert.All(passwords, AssertPasswordFormat);
        Assert.Equal(25, await CountRowsAsync(factory));
    }

    private static void AssertPasswordFormat(string password)
    {
        Assert.Equal(16, password.Length);
        Assert.Contains(password, char.IsUpper);
        Assert.Contains(password, char.IsLower);
        Assert.Contains(password, char.IsDigit);
        Assert.Contains(password, c => Specials.Contains(c));
        Assert.DoesNotContain(password, char.IsWhiteSpace);
    }

    private static async Task AssertValidationErrorAsync(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Requisição inválida", body.GetProperty("title").GetString());
    }

    private static async Task<Guid> CreateGuidAsync(HttpClient client)
    {
        var response = await client.PostAsync(Endpoint, null);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("guid").GetGuid();
    }

    private static async Task<string> ReadPasswordAsync(HttpClient client, Guid guid)
    {
        var response = await GetAsync(client, guid.ToString("D"));
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("password").GetString()!;
    }

    private static async Task<HttpResponseMessage> GetAsync(HttpClient client, string header)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, Endpoint);
        request.Headers.Add("X-Guid", header);
        return await client.SendAsync(request);
    }

    private static async Task<int> CountRowsAsync(PasswordApiFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PasswordDbContext>();
        return await db.Passwords.CountAsync();
    }
}
