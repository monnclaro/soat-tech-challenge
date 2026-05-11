using Application.Authentication.DTOs.Requests;
using Application.Authentication.DTOs.Responses;
using Application.Authentication.Interfaces;
using Domain.Common.Interfaces;
using Domain.Usuarios;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Authentication.Services;

public class AuthenticationService : IAuthenticationService, IScopedService
{
    private readonly IRepository<Usuario> _usuarioRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenProvider _tokenProvider;
    
    public AuthenticationService(
        IRepository<Usuario> usuarioRepository, 
        IPasswordHasher passwordHasher, 
        ITokenProvider tokenProvider)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
        _tokenProvider = tokenProvider;
    }

    public async Task<LoginResponse> Login(LoginRequest request)
    {
        var usuario = await _usuarioRepository
            .GetQueryable().AsNoTracking()
            .Include(l => l.Roles).AsSplitQuery()
            .Where(l => l.Email == request.Email)
            .FirstOrDefaultAsync();

        if (usuario is null)
            return new LoginResponse(LoginResponseStatusResultado.UsuarioNaoEncontrado);

        var senhaValida = _passwordHasher.Verificar(request.Senha, usuario.SenhaHash);
        if (!senhaValida)
            return new LoginResponse(LoginResponseStatusResultado.SenhaInvalida);

        var token = _tokenProvider.GerarToken(usuario);
        return new LoginResponse(token);
    }
}