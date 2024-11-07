using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Json;
using Distal.Core.Endpoints.v1.Mesh.Models;
using Distal.Core.Models;
using Distal.Core.Models.Formats;
using Distal.Core.Services;
using Distal.Core.Services.Implementations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Distal.Core.Endpoints.v1.Mesh;

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

        group.MapGet("search", async (string? name, CancellationToken cancellationToken, [FromServices] IMeshService service) =>
        {
            var searchResult = await service.SearchByParametersAsync(new(name));
            return Results.Ok(searchResult.Select(MapToResponse));
        });

        group.MapPost("", async (IPrincipal principal, [FromBody] CreateMeshFileRequest modelDto, [FromServices] IUserService userService, [FromServices] IMeshService meshService) =>
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

        group.MapPut("{id:guid}/content", async (Guid id, IPrincipal principal, IFormFile content, CancellationToken cancellationToken, [FromServices] IUserService userService, [FromServices] IMeshService meshService,
            [FromServices] IMeshFileParserFactory factory) =>
        {
            var user = await userService.GetOrCreateUserAsync(principal);

            var meshFile = await meshService.GetMeshFileByIdAsync(id);
            if (meshFile == null)
                return Results.NotFound(new { Message = "Mesh file not found." });

            if (meshFile.UserId != user.Id)
                return Results.Forbid();

            using var stream = content.OpenReadStream();
            using var parsingStream = new MemoryStream(10 * 1024);
            using var uploadStream = new MemoryStream(10 * 1024);

            await stream.CopyToAsync(parsingStream, cancellationToken);
            await stream.CopyToAsync(uploadStream, cancellationToken);

            parsingStream.Position = 0;
            uploadStream.Position = 0;

            var parser = factory.GetParser(meshFile.Format);
            var parsingTask = parser.ParseFromStreamAsync(parsingStream, cancellationToken: cancellationToken);
            var updatedMeshFileTask = meshService.UploadFileContentAsync(meshFile, uploadStream, cancellationToken);

            await Task.WhenAll(parsingTask, updatedMeshFileTask);

            return Results.Ok(MapToResponse(await updatedMeshFileTask));
        })
            .Produces<CreateMeshFileResponse?>()
            .Produces<NotFoundResult>(StatusCodes.Status404NotFound)
            .Produces<ForbidResult>(StatusCodes.Status403Forbidden)
            .WithName("Put Mesh Content")
            .WithDisplayName("Put Mesh Content")
            .WithDescription("Puts the mesh data (from a file, stream, binary stream) into the meshdata of the specified id.")
            .Accepts<IFormFile>("multipart/form-data")
            .DisableAntiforgery();
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