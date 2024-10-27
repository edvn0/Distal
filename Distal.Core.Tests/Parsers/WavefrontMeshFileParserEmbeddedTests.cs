using System.Reflection;
using Distal.Core.Models.Formats;
using Distal.Core.Models.Maths;
using Distal.Core.Services.Implementations;

namespace Distal.Core.Tests.Parsers;

public class WavefrontMeshFileParserEmbeddedTests
{
    private readonly WavefrontMeshFileParser _parser = new();

    [Fact]
    public async Task ParseFromStreamAsync_ShouldParseEmbeddedObjFileCorrectly()
    {
        using var stream = EmbeddedResources.GetStream("sample.obj");

        var result = await _parser.ParseFromStreamAsync(stream);

        result.ExecuteWithType<Wavefront>(wavefront =>
        {
            wavefront.Should().NotBeNull();
            wavefront.Positions.Should().HaveCount(4); // Update based on sample file data
            wavefront.Normals.Should().HaveCount(2);   // Update based on sample file data
            wavefront.TextureCoordinates.Should().HaveCount(2); // Update based on sample file data
            wavefront.Faces.Should().HaveCount(1);     // Update based on sample file data

            // Verify specific data points
            wavefront.Positions[0].Should().BeEquivalentTo(new Vector3f { X = 1.0f, Y = 1.0f, Z = 1.0f });
            wavefront.Normals[0].Should().BeEquivalentTo(new Vector3f { X = 0.0f, Y = 0.0f, Z = 1.0f });
            wavefront.TextureCoordinates[0].Should().BeEquivalentTo(new Vector2f { X = 0.5f, Y = 0.5f });

            var face = wavefront.Faces[0];
            face.VertexIndices.Should().BeEquivalentTo([1, 2, 3]);
            face.TextureIndices.Should().BeEquivalentTo([1, 2, 3]);
            face.NormalIndices.Should().BeEquivalentTo([1, 2, 3]);
        });


    }
}