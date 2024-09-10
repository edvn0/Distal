using System.ComponentModel.DataAnnotations.Schema;

namespace Distal.Core.Models;

public enum MeshFormat
{
    Wavefront,
    FBX,
    OBJ,
    Blender
}

public class MeshFile
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required MeshFormat Format { get; set; }
    public long Size => MeshData?.Size ?? 0;
    public bool Valid => MeshData is not null && Size > 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public required DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Foreign key reference to the user who uploaded the file
    /// </summary>
    public required Guid UserId { get; set; }
    /// <summary>
    /// Navigation property for the user
    /// </summary>
    public required User User { get; set; }

    public MeshData? MeshData { get; set; }

    public string? Description { get; set; }
    public IList<string> Tags { get; } = [];
}