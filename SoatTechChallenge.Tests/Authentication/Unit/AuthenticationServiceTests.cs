using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using MockQueryable.Moq;
using Moq;
using SoatTechChallenge.Application.Authentication.Services;
using SoatTechChallenge.Application.Authentication.Services.DTOs.Requests;
using SoatTechChallenge.Application.Authentication.Services.DTOs.Responses;
using SoatTechChallenge.Application.Common.DTOs;
using SoatTechChallenge.Domain.Common.Interfaces;
using SoatTechChallenge.Domain.Usuarios;
using SoatTechChallenge.Domain.Usuarios.Roles;
using Xunit;

namespace SoatTechChallenge.Tests.Authentication.Unit;

public class AuthenticationServiceTests
{
    private readonly Mock<IRepository<Usuario>> _repositoryMock;
    private readonly JwtSettings _jwtSettings;
    private readonly AuthenticationService _sut;

    public AuthenticationServiceTests()
    {
        _repositoryMock = new Mock<IRepository<Usuario>>();

        _jwtSettings = new JwtSettings
        {
            Secret = "super-secret-key-for-testing-purposes-minimum-32-chars",
            ExpirationHours = 2
        };

        var jwtOptions = Options.Create(_jwtSettings);
        _sut = new AuthenticationService(_repositoryMock.Object, jwtOptions);
    }

    [Fact]
    public async Task Login_QuandoUsuarioNaoExiste_RetornaUsuarioNaoEncontrado()
    {
        // Arrange
        var request = new LoginRequest("naoexiste@email.com", "qualquersenha");
        SetupRepositoryReturning();

        // Act
        var result = await _sut.Login(request);

        // Assert
        Assert.Equal(LoginResponseStatusResultado.UsuarioNaoEncontrado, result.Status);
        Assert.Null(result.Token);
    }

    [Fact]
    public async Task Login_QuandoSenhaInvalida_RetornaSenhaInvalida()
    {
        // Arrange
        var usuario = CriarUsuario("email@email.com", "senha123");
        var request = new LoginRequest(usuario.Email, "senhaErrada");
        SetupRepositoryReturning(usuario);

        // Act
        var result = await _sut.Login(request);

        // Assert
        Assert.Equal(LoginResponseStatusResultado.SenhaInvalida, result.Status);
        Assert.Null(result.Token);
    }

    [Fact]
    public async Task Login_QuandoCredenciaisValidas_RetornaToken()
    {
        // Arrange
        var senha = "senha123";
        var usuario = CriarUsuario("email@email.com", senha);
        var request = new LoginRequest(usuario.Email, senha);
        SetupRepositoryReturning(usuario);

        // Act
        var result = await _sut.Login(request);

        // Assert
        Assert.Equal(LoginResponseStatusResultado.Sucesso, result.Status);
        Assert.NotNull(result.Token);
        Assert.NotEmpty(result.Token!);
    }

    [Fact]
    public async Task Login_QuandoSucesso_TokenContemNomeDoUsuario()
    {
        // Arrange
        var usuario = CriarUsuario("email@email.com", "senha123", nome: "João Silva");
        SetupRepositoryReturning(usuario);

        // Act
        var result = await _sut.Login(new LoginRequest(usuario.Email, "senha123"));

        // Assert
        var jwt = LerToken(result.Token!);
        var nameClaim = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);

        Assert.NotNull(nameClaim);
        Assert.Equal("João Silva", nameClaim!.Value);
    }

    [Fact]
    public async Task Login_QuandoSucesso_TokenContemRolesDoUsuario()
    {
        // Arrange
        var roles = new List<UsuarioRole> { new("Admin"), new("Operador") };
        var usuario = CriarUsuario("email@email.com", "senha123", roles: roles);
        SetupRepositoryReturning(usuario);

        // Act
        var result = await _sut.Login(new LoginRequest(usuario.Email, "senha123"));

        // Assert
        var jwt = LerToken(result.Token!);
        var roleClaims = jwt.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        Assert.Contains("Admin", roleClaims);
        Assert.Contains("Operador", roleClaims);
    }

    [Fact]
    public async Task Login_QuandoSucesso_TokenExpiracaoCorreta()
    {
        // Arrange
        var usuario = CriarUsuario("email@email.com", "senha123");
        SetupRepositoryReturning(usuario);

        var antes = DateTime.UtcNow.AddHours(_jwtSettings.ExpirationHours).AddMinutes(-1);
        var depois = DateTime.UtcNow.AddHours(_jwtSettings.ExpirationHours).AddMinutes(1);

        // Act
        var result = await _sut.Login(new LoginRequest(usuario.Email, "senha123"));

        // Assert
        var jwt = LerToken(result.Token!);
        Assert.InRange(jwt.ValidTo, antes, depois);
    }

    private static Usuario CriarUsuario(
        string email,
        string senha,
        string nome = "Usuário Teste",
        List<UsuarioRole>? roles = null)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(senha);
        var usuario = new Usuario(nome, email, hash);
        
        if (roles is { Count: > 0 }) usuario.AdicionarRoles(roles);
        
        return usuario;
    }

    /// <summary>
    /// Usa MockQueryable para criar um IQueryable compatível com IAsyncQueryProvider
    /// do EF Core, necessário para FirstOrDefaultAsync, Include, AsSplitQuery, etc.
    /// </summary>
    private void SetupRepositoryReturning(params Usuario[] usuarios)
    {
        var mockQueryable = usuarios
            .ToList()
            .AsQueryable()
            .BuildMock();

        _repositoryMock
            .Setup(r => r.GetQueryable())
            .Returns(mockQueryable);
    }

    private static JwtSecurityToken LerToken(string token) => new JwtSecurityTokenHandler().ReadJwtToken(token);
}