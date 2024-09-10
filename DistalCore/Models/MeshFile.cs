using System.ComponentModel.DataAnnotations.Schema;

namespace DistalCore.Models;

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
    public MeshFormat Format { get; set; }
    public long Size { get; set; }
    public required byte[] Content { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Foreign key reference to the user who uploaded the file
    /// </summary>
    public Guid UserId { get; set; }
    /// <summary>
    /// Navigation property for the user
    /// </summary>
    public required User User { get; set; }

    public string? Description { get; set; }
    public string? Tags { get; set; }
}