using Microsoft.OpenApi.Models;

namespace DistalCore.Extensions;

internal static class SwaggerGenerationExtensions
{
    internal static IServiceCollection AddSwaggerGenerationWithAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSwaggerGen(o =>
        {
            o.CustomSchemaIds(ids =>
            {
                return ids.FullName?.Replace('+', '-');
            });

            o.AddSecurityDefinition("Keycloak", new()
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new()
                {
                    Implicit = new()
                    {
                        AuthorizationUrl = new Uri(configuration["Keycloak:AuthorizationUrl"]!),
                        Scopes = { { "openid", "openid" }, { "profile", "profile" }, }
                    }
                }
            });

            var securityRequirement = new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference()
                        {
                            Id = "Keycloak",
                            Type = ReferenceType.SecurityScheme,
                        },
                        In = ParameterLocation.Header,
                        Name = "Bearer",
                        Scheme = "Bearer"
                    },
                    []
                }
            };

            o.AddSecurityRequirement(securityRequirement);
        });

        return services;
    }
}