using Distal.Core.Models.Formats;

namespace Distal.Core.Services;

public readonly struct ParseConfiguration(bool computeBitangents = true)
{
    public readonly bool ComputeBitangents { get; } = computeBitangents;
}

public interface IMeshFileParser
{
    public Task<IParseableMeshFileFormat?> ParseFromStreamAsync(Stream? fileStream, ParseConfiguration parseConfiguration = default, CancellationToken cancellationToken = default);
}