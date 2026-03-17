using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SoatTechChallenge.Application.Authentication.Services.DTOs.Requests;
using SoatTechChallenge.Application.Authentication.Services.DTOs.Responses;
using SoatTechChallenge.Application.Common.DTOs;
using SoatTechChallenge.Application.Common.Interfaces;
using SoatTechChallenge.Domain.Common.Interfaces;
using SoatTechChallenge.Domain.Usuarios;

namespace SoatTechChallenge.Application.Authentication.Services;

public class AuthenticationService : IAuthenticationService, IScopedService
{
    private readonly IRepository<Usuario> _repository;
    private readonly JwtSettings _jwtSettings;

    public AuthenticationService(IRepository<Usuario> repository, IOptions<JwtSettings> jwtSettings)
    {
        _repository = repository;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<LoginResponse> Login(LoginRequest request)
    {
        var usuario = await _repository
            .GetQueryable()
            .AsNoTracking()
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(l => l.Nome == request.Usuario);

        if (usuario is null)
        {
            return new LoginResponse(LoginResponseStatusResultado.UsuarioNaoEncontrado);
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
        {
            return new LoginResponse(LoginResponseStatusResultado.SenhaInvalida);
        }

        return new LoginResponse(GerarToken(usuario));
    }

    private string GerarToken(Usuario usuario)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim> { new(ClaimTypes.Name, usuario.Nome) };
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