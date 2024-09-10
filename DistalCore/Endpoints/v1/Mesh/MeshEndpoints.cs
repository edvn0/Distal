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

        group.MapGet("{id:guid}", async (ClaimsPrincipal principal, Guid id, CancellationToken cancellationToken, [FromServices] IMeshService service) =>
        {
            var meshFile = await service.GetMeshFileByIdAsync(id, cancellationToken);
            if (meshFile is null)
                return Results.NotFound(new { Message = "Mesh file not found." });

            return Results.Ok(MapToResponse(meshFile));
        });

        group.MapPost("", async (ClaimsPrincipal principal, [FromServices] IUserService userService, [FromServices] IMeshService meshService,
            [FromBody] CreateMeshFileRequest modelDto) =>
        {
            var user = await userService.GetOrCreateUserAsync(principal);

            if (user.MeshFiles.Count > user.UploadLimit)
                return Results.BadRequest(new { Message = "Upload limit is reached." });

            var meshFile = new MeshFile
            {
                Name = modelDto.Name,
                Format = modelDto.Format,
                Description = modelDto.Description,
                User = user,
                UserId = user.Id,
                UpdatedAt = DateTime.UtcNow,
            };

            foreach (var tag in modelDto.Tags)
            {
                meshFile.Tags.Add(tag);
            }

            var createdMeshFile = await meshService.CreateMetadataAsync(meshFile);

            if (createdMeshFile is null)
                return Results.BadRequest();

            return Results.Ok(createdMeshFile.Id);
        }).Produces<Guid?>().DisableAntiforgery();

        group.MapPut("{id:guid}/content", async (Guid id, ClaimsPrincipal principal, [FromServices] IUserService userService, [FromServices] IMeshService meshService,
            IFormFile content) =>
        {
            var user = await userService.GetOrCreateUserAsync(principal);

            var meshFile = await meshService.GetMeshFileByIdAsync(id);
            if (meshFile == null)
                return Results.NotFound(new { Message = "Mesh file not found." });

            if (meshFile.UserId != user.Id)
                return Results.Forbid();

            using var stream = content.OpenReadStream();

            var updatedMeshFile = await meshService.UploadFileContentAsync(meshFile, stream);

            return Results.Ok(MapToResponse(updatedMeshFile));
        }).Produces<CreateMeshFileResponse?>().DisableAntiforgery();
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
            Tags = file.Tags.ToList(),
        };
    }
}