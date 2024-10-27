using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;
using Distal.Core.Services.Implementations;
using System.Globalization;

namespace Distal.Core.Tests.Parsers;

public class WavefrontRegexTests
{
    private static float ParseFloat(string value) => float.Parse(value, CultureInfo.InvariantCulture);

    [Theory]
    [InlineData("v 1.0 2.0 3.0", 1.0f, 2.0f, 3.0f)]
    [InlineData("v -1.5 -2.5 -3.5", -1.5f, -2.5f, -3.5f)]
    public void VertexRegex_ShouldMatchValidVertexLines(string line, float expectedX, float expectedY, float expectedZ)
    {
        var match = WavefrontRegex.Vertex().Match(line);

        match.Success.Should().BeTrue();
        ParseFloat(match.Groups[1].Value).Should().BeApproximately(expectedX, 0.0001f);
        ParseFloat(match.Groups[2].Value).Should().BeApproximately(expectedY, 0.0001f);
        ParseFloat(match.Groups[3].Value).Should().BeApproximately(expectedZ, 0.0001f);
    }

    [Theory]
    [InlineData("v 1.0 2.0", false)]
    [InlineData("v 1.0 2.0 three", false)]
    public void VertexRegex_ShouldNotMatchInvalidVertexLines(string line, bool shouldMatch)
    {
        var match = WavefrontRegex.Vertex().IsMatch(line);
        match.Should().Be(shouldMatch);
    }

    [Theory]
    [InlineData("vn 1.0 0.0 0.0", 1.0f, 0.0f, 0.0f)]
    [InlineData("vn -0.5 -1.5 2.5", -0.5f, -1.5f, 2.5f)]
    public void NormalRegex_ShouldMatchValidNormalLines(string line, float expectedX, float expectedY, float expectedZ)
    {
        var match = WavefrontRegex.Normal().Match(line);

        match.Success.Should().BeTrue();
        ParseFloat(match.Groups[1].Value).Should().BeApproximately(expectedX, 0.0001f);
        ParseFloat(match.Groups[2].Value).Should().BeApproximately(expectedY, 0.0001f);
        ParseFloat(match.Groups[3].Value).Should().BeApproximately(expectedZ, 0.0001f);
    }

    [Theory]
    [InlineData("vn 1.0 2.0", false)]
    [InlineData("vn 1.0 2.0 three", false)]
    public void NormalRegex_ShouldNotMatchInvalidNormalLines(string line, bool shouldMatch)
    {
        var match = WavefrontRegex.Normal().IsMatch(line);
        match.Should().Be(shouldMatch);
    }

    [Theory]
    [InlineData("vt 0.5 0.5", 0.5f, 0.5f)]
    [InlineData("vt 1.0 0.0", 1.0f, 0.0f)]
    public void TextureRegex_ShouldMatchValidTextureCoordinateLines(string line, float expectedX, float expectedY)
    {
        var match = WavefrontRegex.Texture().Match(line);

        match.Success.Should().BeTrue();
        ParseFloat(match.Groups[1].Value).Should().BeApproximately(expectedX, 0.0001f);
        ParseFloat(match.Groups[2].Value).Should().BeApproximately(expectedY, 0.0001f);
    }

    [Theory]
    [InlineData("vt 0.5", false)]
    [InlineData("vt 1.0 two", false)]
    public void TextureRegex_ShouldNotMatchInvalidTextureCoordinateLines(string line, bool shouldMatch)
    {
        var match = WavefrontRegex.Texture().IsMatch(line);
        match.Should().Be(shouldMatch);
    }

    [Theory(Skip = "Whatever")]
    [InlineData("f 1/1/1 2/2/2 3/3/3", new[] { "1/1/1", "2/2/2", "3/3/3" })]
    [InlineData("f 4//4 5//5 6//6", new[] { "4//4", "5//5", "6//6" })]
    [InlineData("f 7/7 8/8 9/9", new[] { "7/7", "8/8", "9/9" })]
    public void FaceRegex_ShouldMatchValidFaceLines(string line, string[] expectedFaces)
    {
        var match = WavefrontRegex.Face().Match(line);

        match.Success.Should().BeTrue();
        var faceElements = match.Groups[1].Value.Trim().Split(' ');

        faceElements.Should().BeEquivalentTo(expectedFaces);
    }

    [Theory(Skip = "Whatever")]
    [InlineData("f 1 2", false)]
    [InlineData("f 1// 2// three//", false)]
    public void FaceRegex_ShouldNotMatchInvalidFaceLines(string line, bool shouldMatch)
    {
        var match = WavefrontRegex.Face().IsMatch(line);
        match.Should().Be(shouldMatch);
    }
}
