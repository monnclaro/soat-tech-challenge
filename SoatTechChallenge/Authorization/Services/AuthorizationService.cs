

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SoatTechChallenge.Authorization.Controllers.DTOs;
using SoatTechChallenge.Authorization.Services.DTOs;
using SoatTechChallenge.Domain.Usuarios;
using SoatTechChallenge.Host.Common.Services;
using SoatTechChallenge.Infrastructure.Interfaces;

namespace SoatTechChallenge.Authorization.Services;

public class AuthorizationService : IAuthorizationService, IScopedService
{
    private readonly IRepository<Usuario> _repository;
    private readonly JwtSettings _jwtSettings;

    public AuthorizationService(IRepository<Usuario> repository, IOptions<JwtSettings> jwtSettings)
    {
        _repository = repository;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<string?> Login(LoginRequest request)
    {
        var usuario = await _repository.Query().AsNoTracking()
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(x => x.Nome == request.Usuario);

        if (usuario is null)
        {
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
        {
            return null;
        }

        return GerarToken(usuario);
    }

    private string GerarToken(Usuario usuario)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim> { new (ClaimTypes.Name, usuario.Nome) };
        foreach (var role in usuario.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Role));
        }

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_jwtSettings.ExpirationHours),
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}