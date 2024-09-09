using DistalCore.Extensions;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

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
    .ConfigureResource(e => e.AddService("DistalCore"))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();
        tracing.AddOtlpExporter();
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(e =>
    {
        e.DocumentTitle = "Distal Core (Swagger)";
        e.OAuthClientId("distal-public");
        e.OAuthScopes("profile", "openid");
    });
}

app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();

app.MapGet("users/me", (ClaimsPrincipal principal, [FromServices] ILogger<Program> logger) =>
{
    var claims = principal.Claims;
    Dictionary<string, List<string>> typeValues = [];
    foreach (var claim in claims)
    {
        if (!typeValues.TryAdd(claim.Type, [claim.Value]))
        {
            typeValues[claim.Type].Add(claim.Value);
        }
    }
    return Results.Ok(typeValues);
}).RequireAuthorization();

app.Run();

