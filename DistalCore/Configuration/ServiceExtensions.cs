using DistalCore.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DistalCore.Configuration;

public static class ServiceExtensions
{
    public static void AddDistalEntityConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(opts =>
        {
            opts.UseNpgsql(configuration.GetConnectionString("DistalContext"), ops => ops.MigrationsAssembly("DistalCore"));
        });
    }

    public static void AddDistalServices(this IServiceCollection services)
    {
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IMeshService, MeshService>();
    }

}