using Distal.Core.Models;
using Distal.Core.Models.Formats;
using Distal.Core.Services.Implementations;

namespace Distal.Core.Services;

public interface IMeshFileParserFactory
{
    IMeshFileParser GetParser(MeshFormat format);
}

public class MeshFileParserFactory(IServiceProvider serviceProvider) : IMeshFileParserFactory
{
    public IMeshFileParser GetParser(MeshFormat format) => format switch
    {
        MeshFormat.Wavefront => serviceProvider.GetRequiredService<WavefrontMeshFileParser>(),
        MeshFormat.FBX => throw new NotImplementedException(),
        MeshFormat.OBJ => throw new NotImplementedException(),
        MeshFormat.Blender => throw new NotImplementedException(),
        _ => throw new NotImplementedException(),
    };
}