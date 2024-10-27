namespace Distal.Core.Tests.Parsers;


using Distal.Core.Services.Implementations;
using Distal.Core.Models.Formats;
using Distal.Core.Models.Maths;

public class WavefrontMeshFileParserTests
{
    private readonly WavefrontMeshFileParser _parser;

    public WavefrontMeshFileParserTests()
    {
        _parser = new WavefrontMeshFileParser();
    }

    [Fact]
    public async Task ParseFromStreamAsync_ShouldParseVerticesCorrectly()
    {
        var objData = "v 1.0 2.0 3.0\nv -1.0 -2.0 -3.0";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(objData));

        var result = await _parser.ParseFromStreamAsync(stream);

        if (result is Wavefront wavefront)
        {
            wavefront.Should().NotBeNull();
            wavefront.Positions.Should().HaveCount(2);
            wavefront.Positions[0].Should().BeEquivalentTo(new Vector3f { X = 1.0f, Y = 2.0f, Z = 3.0f });
            wavefront.Positions[1].Should().BeEquivalentTo(new Vector3f { X = -1.0f, Y = -2.0f, Z = -3.0f });
        }

    }

    [Fact]
    public async Task ParseFromStreamAsync_ShouldParseNormalsCorrectly()
    {
        var objData = "vn 1.0 0.0 0.0\nvn 0.0 1.0 0.0";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(objData));

        var result = await _parser.ParseFromStreamAsync(stream);

        if (result is Wavefront wavefront)
        {
            wavefront.Should().NotBeNull();
            wavefront.Normals.Should().HaveCount(2);
            wavefront.Normals[0].Should().BeEquivalentTo(new Vector3f { X = 1.0f, Y = 0.0f, Z = 0.0f });
            wavefront.Normals[1].Should().BeEquivalentTo(new Vector3f { X = 0.0f, Y = 1.0f, Z = 0.0f });
        }

    }

    [Fact]
    public async Task ParseFromStreamAsync_ShouldParseTextureCoordinatesCorrectly()
    {
        var objData = "vt 0.5 0.5\nvt 0.1 0.9";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(objData));

        var result = await _parser.ParseFromStreamAsync(stream);

        if (result is Wavefront wavefront)
        {
            wavefront.Should().NotBeNull();
            wavefront.TextureCoordinates.Should().HaveCount(2);
            wavefront.TextureCoordinates[0].Should().BeEquivalentTo(new Vector2f { X = 0.5f, Y = 0.5f });
            wavefront.TextureCoordinates[1].Should().BeEquivalentTo(new Vector2f { X = 0.1f, Y = 0.9f });
        }
    }

    [Fact]
    public async Task ParseFromStreamAsync_ShouldParseFacesCorrectly()
    {
        var objData = "f 1/1/1 2/2/2 3/3/3";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(objData));

        var result = await _parser.ParseFromStreamAsync(stream);

        result.ExecuteWithType<Wavefront>((wavefront) =>
        {
            wavefront.Should().NotBeNull();
            wavefront.Faces.Should().HaveCount(1);

            var face = wavefront.Faces[0];
            face.VertexIndices.Should().BeEquivalentTo([1, 2, 3]);
            face.TextureIndices.Should().BeEquivalentTo([1, 2, 3]);
            face.NormalIndices.Should().BeEquivalentTo([1, 2, 3]);
        });
    }

    [Fact]
    public async Task ParseFromStreamAsync_ShouldReturnNull_WhenStreamIsEmpty()
    {
        var stream = new MemoryStream();

        var result = await _parser.ParseFromStreamAsync(stream);

        result.Should().BeNull();
    }

    [Fact]
    public async Task ParseFromStreamAsync_ShouldThrowException_WhenFileSizeExceedsLimit()
    {
        var largeData = new string('v', WavefrontMeshFileParser.MaxFileSize + 1);
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(largeData));

        Func<Task> action = async () => await _parser.ParseFromStreamAsync(stream);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"File size exceeds the maximum limit of {WavefrontMeshFileParser.MaxFileSize / (1024 * 1024)} MB.");
    }

    [Fact]
    public async Task ParseFromStreamAsync_ShouldThrowFormatException_WhenVertexFormatIsInvalid()
    {
        var objData = "v invalid_value";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(objData));

        Func<Task> action = async () => await _parser.ParseFromStreamAsync(stream);

        await action.Should().ThrowAsync<FormatException>()
            .WithMessage("Error parsing line 1:*");
    }

    [Fact]
    public async Task ParseFromStreamAsync_ShouldThrowFormatException_WhenFaceFormatIsInvalid()
    {
        var objData = "f 1// 2// 3//";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(objData));

        Func<Task> action = async () => await _parser.ParseFromStreamAsync(stream);

        await action.Should().ThrowAsync<FormatException>()
            .WithMessage("Error parsing line 1:*");
    }
}

