using DistalCore.Models;

namespace DistalCore.Endpoints.v1.Mesh.Models;

public class CreateMeshFileResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required MeshFormat Format { get; init; }
    public string? Description { get; init; }
    public IReadOnlyCollection<string> Tags { get; init; } = [];
}