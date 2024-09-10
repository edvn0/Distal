using System.Security.Claims;
using DistalCore.Endpoints.v1.Mesh.Models;
using DistalCore.Models;
using DistalCore.Services;
using Microsoft.AspNetCore.Mvc;

namespace DistalCore.Endpoints.v1.Mesh;

public static class MeshEndpoints
{
    public static void MapEndpoints(RouteGroupBuilder builder)
    {
        var group = builder.MapGroup("v1/mesh").RequireAuthorization();

        group.MapGet("", async (ClaimsPrincipal principal, CancellationToken cancellationToken, [FromServices] IMeshService service) =>
        {
            return await service.GetAllMeshFilesAsync(cancellationToken);
        });

        group.MapPost("", async (ClaimsPrincipal principal, [FromServices] IUserService userService, [FromServices] IMeshService meshService, [FromBody] CreateMeshFileRequest modelDto) =>
        {
            var user = await userService.GetOrCreateUserAsync(principal);

            if (user.MeshFiles.Count > user.UploadLimit) return Results.BadRequest(new { Message = "Upload limit is reached." });

            return Results.Ok(MapToResponse(await meshService.CreateAsync(MapToMeshFile(user, modelDto))));
        });
    }

    private static MeshFile MapToMeshFile(DistalCore.Models.User user, CreateMeshFileRequest createModelFileRequest)
    {
        var byteArray = Convert.FromBase64String(createModelFileRequest.ContentBase64);
        return new()
        {
            Id = Guid.NewGuid(),
            Name = createModelFileRequest.Name,
            Content = byteArray,
            User = user,
            Size = byteArray.Length * sizeof(byte),
            Format = createModelFileRequest.Format,
            Description = createModelFileRequest.Description,
            Tags = createModelFileRequest.Tags,
        };
    }

    private static CreateMeshFileResponse? MapToResponse(MeshFile? file)
    {
        if (file is null) return null;

        return new()
        {
            Id = file.Id,
            Name = file.Name,
            Format = file.Format,
            Description = file.Description,
            Tags = file.Tags
        };
    }
}