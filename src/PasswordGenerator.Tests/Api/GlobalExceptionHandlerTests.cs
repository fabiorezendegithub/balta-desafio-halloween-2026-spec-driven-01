using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using PasswordGenerator.Api;
using PasswordGenerator.Application;

namespace PasswordGenerator.Tests.Api;

public class GlobalExceptionHandlerTests
{
    private readonly GlobalExceptionHandler _handler = new(NullLogger<GlobalExceptionHandler>.Instance);

    [Fact]
    public async Task ValidationException_Returns400ProblemDetailsInPortuguese()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var handled = await _handler.TryHandleAsync(
            context,
            new PasswordValidationException("O header X-Guid é obrigatório e não pode ser vazio."),
            CancellationToken.None);

        var body = await ReadBodyAsync(context);
        Assert.True(handled);
        Assert.Equal(400, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);
        Assert.Equal(400, body.GetProperty("status").GetInt32());
        Assert.Equal("Requisição inválida", body.GetProperty("title").GetString());
        Assert.Equal("O header X-Guid é obrigatório e não pode ser vazio.", body.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task GuidNotFound_Returns404ProblemDetailsInPortuguese()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await _handler.TryHandleAsync(context, new GuidNotFoundException(), CancellationToken.None);

        var body = await ReadBodyAsync(context);
        Assert.Equal(404, context.Response.StatusCode);
        Assert.Equal(404, body.GetProperty("status").GetInt32());
        Assert.Equal("O GUID informado não existe.", body.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task InternalFailure_Returns500WithoutExposingInternalDetail()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var handled = await _handler.TryHandleAsync(
            context,
            new InvalidOperationException("segredo interno da base"),
            CancellationToken.None);

        var raw = await ReadRawBodyAsync(context);
        var body = JsonDocument.Parse(raw).RootElement;
        Assert.True(handled);
        Assert.Equal(500, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);
        Assert.Equal("Erro interno", body.GetProperty("title").GetString());
        Assert.Equal("Ocorreu um erro interno. Tente novamente mais tarde.", body.GetProperty("detail").GetString());
        Assert.DoesNotContain("segredo interno da base", raw);
    }

    private static async Task<JsonElement> ReadBodyAsync(HttpContext context)
    {
        return JsonDocument.Parse(await ReadRawBodyAsync(context)).RootElement;
    }

    private static async Task<string> ReadRawBodyAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }
}
