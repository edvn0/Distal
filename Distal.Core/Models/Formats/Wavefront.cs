using Distal.Core.Models.Maths;

namespace Distal.Core.Models.Formats;

public class Wavefront : IParseableMeshFileFormat
{
    public override IList<Vector3f> Positions { get; init; } = [];
    public override IList<Vector3f> Normals { get; init; } = [];
    public override IList<Vector2f> TextureCoordinates { get; init; } = [];
    public override IList<Vector3f> Bitangents { get; init; } = [];
    public IList<Face> Faces { get; init; } = [];
}

public class Face
{
    public IList<int> VertexIndices { get; init; } = [];
    public IList<int> TextureIndices { get; init; } = [];
    public IList<int> NormalIndices { get; init; } = [];
}