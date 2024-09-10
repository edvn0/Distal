using System.ComponentModel.DataAnnotations;
using DistalCore.Models;

namespace DistalCore.Endpoints.v1.Mesh.Models;

public class CreateMeshFileRequest
{
    public required string Name { get; init; }
    [Required]
    [EnumDataType(typeof(MeshFormat))]
    public required MeshFormat Format { get; init; }
    public required string ContentBase64 { get; init; }
    public string? Description { get; init; }
    public string? Tags { get; init; }
}