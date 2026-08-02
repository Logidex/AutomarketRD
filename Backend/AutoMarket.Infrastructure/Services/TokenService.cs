using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerarToken(Usuario usuario)
    {
        // 1. Obtener la clave secreta desde el archivo appsettings.json
        var jwtSecret = _config["Jwt:Secret"] 
            ?? throw new InvalidOperationException("La clave JWT no está configurada.");
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 2. Definir la información que viajará dentro del token (Claims)
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Rol)
        };

        // 3. Configurar la estructura y expiración del token
        var tokenOptions = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2), // Expira en 2 horas
            signingCredentials: creds
        );

        // 4. Escribir el token como una cadena de texto (String)
        return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
    }
}
