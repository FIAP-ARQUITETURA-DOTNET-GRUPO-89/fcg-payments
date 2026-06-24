using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FcgPayments.SharedKernel.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FcgPayments.IntegrationTests.TestHelpers;

/// <summary>
/// Gera tokens JWT válidos para testes de integração com base nas configurações da aplicação.
/// </summary>
public class JwtTestTokenGenerator(IOptions<JwtSettings> settings)
{
    private readonly JwtSettings _settings = settings.Value;

    public string Generate(string email, string role)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, email),
            new(ClaimTypes.Role, role)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_settings.SecurityKey));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: null,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_settings.ExpirationHours),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
