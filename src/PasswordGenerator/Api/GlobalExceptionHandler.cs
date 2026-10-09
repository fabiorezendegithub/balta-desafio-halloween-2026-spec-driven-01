using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PasswordGenerator.Application;

namespace PasswordGenerator.Api;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private const string ProblemContentType = "application/problem+json";

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problem = exception switch
        {
            PasswordValidationException ex => Build(StatusCodes.Status400BadRequest, "Requisição inválida", ex.Message),
            GuidNotFoundException ex => Build(StatusCodes.Status404NotFound, "GUID não encontrado", ex.Message),
            _ => BuildInternal(exception)
        };

        httpContext.Response.StatusCode = problem.Status!.Value;
        await httpContext.Response.WriteAsJsonAsync(
            problem,
            options: null,
            contentType: ProblemContentType,
            cancellationToken: cancellationToken);

        return true;
    }

    private ProblemDetails BuildInternal(Exception exception)
    {
        // O detalhe técnico fica só no log; a resposta não expõe mensagem interna
        logger.LogError(exception, "Falha interna ao processar a requisição");

        return Build(
            StatusCodes.Status500InternalServerError,
            "Erro interno",
            "Ocorreu um erro interno. Tente novamente mais tarde.");
    }

    private static ProblemDetails Build(int status, string title, string detail) =>
        new()
        {
            Status = status,
            Title = title,
            Detail = detail
        };
}
