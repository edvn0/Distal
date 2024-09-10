using System.Collections.Immutable;
using System.Security.Claims;
using Distal.Core.Configuration;
using Distal.Core.Exceptions;
using Distal.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Distal.Core.Services;

internal readonly struct KeycloakClaims
{
    public const string UsernameClaim = "preferred_username";
    public const string IdentifierClaim = "sub";
    public const string NameClaim = "name";
    public const string EmailClaim = "email";
}

public interface IUserService
{
    Task<User> GetOrCreateUserAsync(ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetAllUsersAsync(CancellationToken cancellationToken = default);
}

public class UserService(ApplicationDbContext context) : IUserService
{
    public async Task<IEnumerable<User>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        return await context.Users.ToListAsync(cancellationToken);
    }

    public async Task<User> GetOrCreateUserAsync(ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken = default)
    {
        var userIdString = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userEmail = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value;

        if (userEmail is null || userIdString is null)
        {
            throw new UnauthorizedAccessException("Invalid user claims");
        }

        if (!Guid.TryParse(userIdString, out Guid resultGuid))
        {
            throw new InvalidDataException("Invalid Guid formatting");
        }

        var user = await context.Users.FirstOrDefaultAsync(user => user.Id == resultGuid, cancellationToken);
        if (user is not null) return user;

        user = new()
        {
            Id = resultGuid,
            Username = claimsPrincipal.FindFirst(KeycloakClaims.UsernameClaim)?.Value ?? throw new MissingClaimsException("Name"),
            Email = userEmail,
        };

        _ = await context.Users.AddAsync(user, cancellationToken);
        _ = await context.SaveChangesAsync(cancellationToken);

        return user;
    }
}
