using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Distal.Core.Models.Formats;
using Distal.Core.Models.Maths;

namespace Distal.Core.Services.Implementations;

public static partial class WavefrontRegex
{
    [GeneratedRegex(@"^v\s+(-?\d+\.?\d*)\s+(-?\d+\.?\d*)\s+(-?\d+\.?\d*)$")]
    public static partial Regex Vertex();
    [GeneratedRegex(@"^vn\s+(-?\d+\.?\d*)\s+(-?\d+\.?\d*)\s+(-?\d+\.?\d*)$")]
    public static partial Regex Normal();
    [GeneratedRegex(@"^vt\s+(-?\d+\.?\d*)\s+(-?\d+\.?\d*)$")]
    public static partial Regex Texture();
    [GeneratedRegex(@"^f\s+((?:\d+(?:/\d+)?(?:/\d+)?\s*)+)$")]
    public static partial Regex Face();

}

public class WavefrontMeshFileParser : IMeshFileParser
{
    public const int MaxFileSize = 100 * 1024 * 1024; // 100 MB limit

    public async Task<IParseableMeshFileFormat?> ParseFromStreamAsync(Stream? fileStream, ParseConfiguration configuration = default, CancellationToken cancellationToken = default)
    {
        if (fileStream == null || !fileStream.CanRead)
            return null;

        if (fileStream.Length > MaxFileSize)
            throw new InvalidOperationException($"File size exceeds the maximum limit of {MaxFileSize / (1024 * 1024)} MB.");

        var wavefront = new Wavefront();
        using var reader = new StreamReader(fileStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 4096, leaveOpen: true);

        string? line;
        int lineNumber = 0;
        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            lineNumber++;
            try
            {
                ParseLine(line.Trim(), ref wavefront);
            }
            catch (Exception ex)
            {
                throw new FormatException($"Error parsing line {lineNumber}: {ex.Message}", ex);
            }
        }

        return wavefront.IsValid() ? wavefront : null;
    }

    private static void ParseLine(string line, ref Wavefront wavefront)
    {
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
            return;

        if (TryParseVertex(line, out var vertex))
            wavefront.Positions.Add(vertex);
        else if (TryParseNormal(line, out var normal))
            wavefront.Normals.Add(normal);
        else if (TryParseTextureCoordinate(line, out var texCoord))
            wavefront.TextureCoordinates.Add(texCoord);
        else if (TryParseFace(line, out var face))
            wavefront.Faces.Add(face);
    }

    private static bool TryParseVertex(string line, out Vector3f vertex)
    {
        var match = WavefrontRegex.Vertex().Match(line);
        if (match.Success)
        {
            vertex = new Vector3f
            {
                X = ParseFloat(match.Groups[1].Value),
                Y = ParseFloat(match.Groups[2].Value),
                Z = ParseFloat(match.Groups[3].Value)
            };
            return true;
        }
        vertex = default;
        return false;
    }

    private static bool TryParseNormal(string line, out Vector3f normal)
    {
        var match = WavefrontRegex.Normal().Match(line);
        if (match.Success)
        {
            normal = new Vector3f
            {
                X = ParseFloat(match.Groups[1].Value),
                Y = ParseFloat(match.Groups[2].Value),
                Z = ParseFloat(match.Groups[3].Value)
            };
            return true;
        }
        normal = default;
        return false;
    }

    private static bool TryParseTextureCoordinate(string line, out Vector2f texCoord)
    {
        var match = WavefrontRegex.Texture().Match(line);
        if (match.Success)
        {
            texCoord = new Vector2f
            {
                X = ParseFloat(match.Groups[1].Value),
                Y = ParseFloat(match.Groups[2].Value)
            };
            return true;
        }
        texCoord = default;
        return false;
    }

    private static bool TryParseFace(string line, out Face face)
    {
        var match = WavefrontRegex.Face().Match(line);
        if (match.Success)
        {
            var parts = match.Groups[1].Value.Split([' '], StringSplitOptions.RemoveEmptyEntries);
            face = new Face
            {
                VertexIndices = new int[parts.Length],
                TextureIndices = new int[parts.Length],
                NormalIndices = new int[parts.Length]
            };

            for (int i = 0; i < parts.Length; i++)
            {
                var indices = parts[i].Split('/');
                face.VertexIndices[i] = ParseInt(indices[0]);
                if (indices.Length > 1 && !string.IsNullOrEmpty(indices[1]))
                    face.TextureIndices[i] = ParseInt(indices[1]);
                if (indices.Length > 2 && !string.IsNullOrEmpty(indices[2]))
                    face.NormalIndices[i] = ParseInt(indices[2]);
            }
            return true;
        }
        face = new Face();
        return false;
    }

    private static float ParseFloat(string value)
    {
        if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
            throw new FormatException($"Invalid float value: {value}");
        return result;
    }

    private static int ParseInt(string value)
    {
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
            throw new FormatException($"Invalid integer value: {value}");
        return result;
    }
}

public static class WavefrontExtensions
{
    public static bool IsValid(this Wavefront wavefront)
    {
        return wavefront.Positions.Count > 0 && wavefront.Faces.Count > 0;
    }
}