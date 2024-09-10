using Distal.Core.Models.Maths;

namespace Distal.Core.Models.Formats;

/// <summary>
/// Marker interface for Wavefront representations, FBX, etc
/// </summary>
public abstract class IParseableMeshFileFormat
{
    public abstract IList<Vector3f> Positions { get; init; }
    public abstract IList<Vector3f> Normals { get; init; }
    public abstract IList<Vector2f> TextureCoordinates { get; init; }
    public abstract IList<Vector3f> Bitangents { get; init; }
    public int Count => Positions.Count;
}