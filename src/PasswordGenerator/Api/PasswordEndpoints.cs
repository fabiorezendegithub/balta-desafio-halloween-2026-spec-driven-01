using Microsoft.AspNetCore.Mvc;
using PasswordGenerator.Application;

namespace PasswordGenerator.Api;

public static class PasswordEndpoints
{
    public static IEndpointRouteBuilder MapPasswordEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/password");

        group.MapPost("", async (PasswordService service, CancellationToken cancellationToken) =>
        {
            var guid = await service.CreateAsync(cancellationToken);

            return Results.Json(new CreatePasswordResponse(guid), statusCode: StatusCodes.Status201Created);
        });

        group.MapGet("", async (
            [FromHeader(Name = "X-Guid")] string? guid,
            PasswordService service,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var password = await service.GetPasswordAsync(guid, cancellationToken);

            httpContext.Response.Headers.CacheControl = "no-store";

            return Results.Ok(new GetPasswordResponse(password));
        });

        return app;
    }
}

public record CreatePasswordResponse(Guid Guid);

public record GetPasswordResponse(string Password);
