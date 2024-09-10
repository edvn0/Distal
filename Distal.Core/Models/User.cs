namespace Distal.Core.Models;

public class User
{
    public Guid Id { get; set; }
    public required string Username { get; init; }
    public required string Email { get; init; }
    public bool IsFederatedUser { get; set; } = false;
    public int UploadLimit { get; set; } = 1000;
    public ICollection<MeshFile> MeshFiles { get; } = [];
}