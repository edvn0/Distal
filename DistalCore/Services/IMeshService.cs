using DistalCore.Configuration;
using DistalCore.Models;
using Microsoft.EntityFrameworkCore;

namespace DistalCore.Services;

public interface IMeshService
{
    public Task<IEnumerable<MeshFile>> GetAllMeshFilesAsync(CancellationToken cancellationToken = default);
    public Task<MeshFile?> CreateAsync(MeshFile meshFile, CancellationToken cancellationToken = default);
}

public class MeshService(ApplicationDbContext context) : IMeshService
{
    public async Task<MeshFile?> CreateAsync(MeshFile meshFile, CancellationToken cancellationToken = default)
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

    public async Task<IEnumerable<MeshFile>> GetAllMeshFilesAsync(CancellationToken cancellationToken = default)
    {
        return await context.MeshFiles.ToListAsync(cancellationToken);
    }
}
