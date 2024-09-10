using System.Security.Claims;
using DistalCore.Services;
using Microsoft.AspNetCore.Mvc;

namespace DistalCore.Endpoints.v1.User;

public static class UserEndpoints
{
    public static void MapEndpoints(RouteGroupBuilder builder)
    {
        var baseGroup = builder.MapGroup("v1/user").RequireAuthorization();

        baseGroup.MapGet("me", async (ClaimsPrincipal principal, CancellationToken cancellationToken, [FromServices] IUserService service) =>
        {
            var user = await service.GetOrCreateUserAsync(principal, cancellationToken);
            return Results.Ok(user);
        });

        baseGroup.MapGet("", async (ClaimsPrincipal principal, CancellationToken cancellationToken, [FromServices] IUserService service) =>
        {
            return Results.Ok(await service.GetAllUsersAsync(cancellationToken));
        });
    }
}