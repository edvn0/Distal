namespace DistalCore.Models;

public class MeshData
{
    public Guid Id { get; set; }

    public required byte[] Data { get; set; }

    public Guid MeshFileId { get; set; }
    public required MeshFile MeshFile { get; set; }

    public long Size => Data.Length * sizeof(byte);
}