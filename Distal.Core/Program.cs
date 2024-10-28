using Distal.Core.Configuration;
using Distal.Core.Endpoints.v1.Mesh;
using Distal.Core.Endpoints.v1.User;
using Distal.Core.Exceptions;
using Distal.Core.Extensions;
using Distal.Core.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var externalApiSection = builder.Configuration.GetSection(ExternalAPIOptions.Identifier);
builder.Services.AddOptions<ExternalAPIOptions>().Bind(externalApiSection).ValidateDataAnnotations().ValidateOnStart();

JsonSerializerOptions globalSerializerOptions = new()
{
    PropertyNameCaseInsensitive = true
};


builder.Services.AddHttpClient("Random", client =>
{
    var found = externalApiSection.Get<ExternalAPIOptions>() ?? throw new MissingConfigurationException(ExternalAPIOptions.Identifier);
    client.BaseAddress = new Uri(found.Random.Address);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(found.Random.Timeout);
});

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

builder.Services.AddDistalEntityConfiguration(builder.Configuration);
builder.Services.AddDistalServices();
builder.Services.AddMeshFileParsers();

builder.Services.AddAntiforgery();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGenerationWithAuth(builder.Configuration);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, e =>
    {
        e.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        e.Audience = builder.Configuration["Authentication:Audience"];
        e.Authority = builder.Configuration["Authentication:Authority"];
        e.MetadataAddress = builder.Configuration["Authentication:MetadataAddress"]!;
        e.TokenValidationParameters = new()
        {
            ValidIssuer = builder.Configuration["Authentication:ValidIssuer"],
            RoleClaimType = "groups",
            SignatureValidator = (string token, TokenValidationParameters parameters) =>
            {
                return new JsonWebToken(token);
            }
        };
        e.Configuration = new();
    });
builder.Services.AddAuthorization();
builder.Services.AddOpenTelemetry()
    .ConfigureResource(e => e.AddService("Distal.Core"))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();

        tracing.AddOtlpExporter(conf =>
            conf.Endpoint = new Uri(builder.Configuration.GetConnectionString("OtelOtlpCollector")!)
        );
    });

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(e =>
{
    e.DocumentTitle = "Distal Core (Swagger)";
    e.OAuthClientId("distal-public");
    e.OAuthScopes("profile", "openid");
});
app.UseForwardedHeaders();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();

var baseGroup = app.MapGroup("api");
MeshEndpoints.MapEndpoints(baseGroup);
UserEndpoints.MapEndpoints(baseGroup);


await app.RunAsync();
