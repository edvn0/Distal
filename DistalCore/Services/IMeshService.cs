using DistalCore.Configuration;
using DistalCore.Models;
using Microsoft.EntityFrameworkCore;

namespace DistalCore.Services;

public interface IMeshService
{
    public Task<IEnumerable<MeshFile>> GetAllMeshFilesAsync(CancellationToken cancellationToken = default);
    public Task<MeshFile?> CreateMetadataAsync(MeshFile meshFile, CancellationToken cancellationToken = default);
    public Task<MeshFile?> UploadFileContentAsync(MeshFile meshFile, Stream dataStream, CancellationToken cancellationToken = default);
    public Task<MeshFile?> GetMeshFileByIdAsync(Guid id, CancellationToken cancellationToken = default);
}


public class MeshService(ApplicationDbContext context) : IMeshService
{
    public async Task<MeshFile?> CreateMetadataAsync(MeshFile meshFile, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return null;
        }

        await context.MeshFiles.AddAsync(meshFile, cancellationToken);
        meshFile.User?.MeshFiles.Add(meshFile);

        await context.SaveChangesAsync(cancellationToken);

        return meshFile;
    }

    public async Task<MeshFile?> UploadFileContentAsync(MeshFile meshFile, Stream fileStream, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return null;
        }

        var meshFileData = new MeshData
        {
            Data = await StreamFileToDatabaseAsync(fileStream, cancellationToken),
            MeshFile = meshFile
        };

        meshFile.MeshData = meshFileData;

        context.MeshFiles.Update(meshFile);
        await context.SaveChangesAsync(cancellationToken);

        return meshFile;
    }

    public async Task<byte[]> StreamFileToDatabaseAsync(Stream fileStream, CancellationToken cancellationToken)
    {
        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream, cancellationToken);
        return memoryStream.ToArray();
    }

    public async Task<IEnumerable<MeshFile>> GetAllMeshFilesAsync(CancellationToken cancellationToken = default)
    {
        return await context.MeshFiles.ToListAsync(cancellationToken);
    }

    public async Task<MeshFile?> GetMeshFileByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.MeshFiles
            .Include(mf => mf.MeshData)
            .FirstOrDefaultAsync(mf => mf.Id == id, cancellationToken);
    }
}
