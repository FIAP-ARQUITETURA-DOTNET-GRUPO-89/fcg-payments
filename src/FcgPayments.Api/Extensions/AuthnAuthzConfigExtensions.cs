using System.Text;
using FcgPayments.SharedKernel.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FcgPayments.Api.Extensions;

public static class AuthnAuthzConfigExtensions
{
    public static IServiceCollection AuthnAuthzConfig(this IServiceCollection services)
    {
        ConfigureAuthentication(services);
        ConfigureJwtOptions(services);
        ConfigureAuthorization(services);

        return services;
    }

    private static void ConfigureAuthentication(IServiceCollection services) => services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer();

    private static void ConfigureJwtOptions(IServiceCollection services) => services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtSettings>>((options, jwtOptions) =>
            {
                var jwt = jwtOptions.Value;

                options.TokenValidationParameters = BuildTokenValidationParameters(jwt);
                options.Events = BuildJwtEvents();
            });

    private static TokenValidationParameters BuildTokenValidationParameters(JwtSettings jwt) => new()
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt.Issuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecurityKey)),
        ClockSkew = TimeSpan.Zero
    };

    private static JwtBearerEvents BuildJwtEvents() => new()
    {
        OnForbidden = async context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Title = "Forbidden",
                Status = StatusCodes.Status403Forbidden,
                Detail = "Você não tem permissão para acessar este recurso."
            });
        },
        OnChallenge = async context =>
        {
            context.HandleResponse();

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/problem+json";

                await context.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Title = "Unauthorized",
                    Status = StatusCodes.Status401Unauthorized,
                    Detail = "Credenciais de autenticação ausentes ou inválidas."
                });
            }
        }
    };

    private static void ConfigureAuthorization(IServiceCollection services) =>
        services.AddAuthorizationBuilder()
            .AddPolicy("CustomerPolicy", policy =>
                policy.RequireRole("Admin", "Customer"))
            .AddPolicy("AdminPolicy", policy =>
                policy.RequireRole("Admin"));
}
