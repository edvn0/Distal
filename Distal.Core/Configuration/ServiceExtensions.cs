using Distal.Core.Services;
using Distal.Core.Services.Implementations;
using Microsoft.EntityFrameworkCore;

namespace Distal.Core.Configuration;

public static class ServiceExtensions
{
    public static void AddDistalEntityConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(opts =>
        {
            opts.UseNpgsql(configuration.GetConnectionString("DistalContext"), ops => ops.MigrationsAssembly("Distal.Core"));
        });
    }

    public static void AddDistalServices(this IServiceCollection services)
    {
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IMeshService, MeshService>();
    }

    public static void AddMeshFileParsers(this IServiceCollection services)
    {
        services.AddTransient<WavefrontMeshFileParser, WavefrontMeshFileParser>();
        // services.AddScoped<IMeshFileParser<FBX>, FbxMeshFileParser>();

        services.AddScoped<IMeshFileParserFactory, MeshFileParserFactory>();
    }

}