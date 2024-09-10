using System.ComponentModel.DataAnnotations;
using DistalCore.Models;

namespace DistalCore.Endpoints.v1.Mesh.Models;

public class CreateMeshFileRequest
{
    [Required]
    public required string Name { get; init; }
    [Required]
    [EnumDataType(typeof(MeshFormat))]
    public required MeshFormat Format { get; init; }
    public string? Description { get; init; }
    public IList<string> Tags { get; init; } = [];
}