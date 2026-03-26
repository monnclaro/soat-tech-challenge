using DotNet.Testcontainers.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SoatTechChallenge.Application.Authentication.Services;
using SoatTechChallenge.Application.Authentication.Services.DTOs.Requests;
using SoatTechChallenge.Application.Authentication.Services.DTOs.Responses;
using SoatTechChallenge.Application.Common.DTOs;
using SoatTechChallenge.Domain.Common.Interfaces;
using SoatTechChallenge.Domain.Usuarios;
using SoatTechChallenge.Domain.Usuarios.Roles;
using SoatTechChallenge.Infrastucture.Database;
using SoatTechChallenge.Infrastucture.Persistence;
using Testcontainers.PostgreSql;
using Xunit;

namespace SoatTechChallenge.Tests.Authentication.Integration;

public class AuthenticationServiceIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("soattest")
        .WithUsername("soatuser")
        .WithPassword("soatpass")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
        .Build();

    private ServiceProvider _provider = null!;
    private AuthenticationService _sut = null!;

    #region Lifecycle

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var services = new ServiceCollection();

        services.AddDbContext<SoatTechChallengeDbContext>(opts => opts.UseNpgsql(_postgres.GetConnectionString()));
        services.AddScoped<IRepository<Usuario>, Repository<Usuario>>();

        services.Configure<JwtSettings>(opts =>
        {
            opts.Secret = "integration-test-secret-key-minimum-32-chars!";
            opts.ExpirationHours = 1;
        });

        services.AddScoped<AuthenticationService>();

        _provider = services.BuildServiceProvider();

        // Garantir schema atualizado
        using var scope = _provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SoatTechChallengeDbContext>();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _provider.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    #endregion

    #region Helpers

    private AuthenticationService CreateService(IServiceScope scope) => scope.ServiceProvider.GetRequiredService<AuthenticationService>();

    private async Task SeedUsuarioAsync(Usuario usuario)
    {
        using var scope = _provider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<Usuario>>();
        await repo.InsertAsync(usuario);
    }

    private static Usuario CriarUsuario(
        string email,
        string senha,
        string nome = "Usuário Integração",
        List<UsuarioRole>? roles = null)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(senha);
        var usuario = new Usuario(nome, email, hash);
        
        if (roles is { Count: > 0 }) usuario.AdicionarRoles(roles);
        
        return usuario;
    }

    #endregion

    [Fact]
    public async Task Login_QuandoUsuarioNaoExisteNoBanco_RetornaUsuarioNaoEncontrado()
    {
        using var scope = _provider.CreateScope();
        var service = CreateService(scope);

        var result = await service.Login(new LoginRequest("naoexiste@test.com", "qualquer"));

        Assert.Equal(LoginResponseStatusResultado.UsuarioNaoEncontrado, result.Status);
    }

    [Fact]
    public async Task Login_QuandoSenhaEstaErrada_RetornaSenhaInvalida()
    {
        // Arrange
        var usuario = CriarUsuario("senha-errada@test.com", "senhaCorreta");
        await SeedUsuarioAsync(usuario);

        using var scope = _provider.CreateScope();
        var service = CreateService(scope);

        // Act
        var result = await service.Login(new LoginRequest(usuario.Email, "senhaErrada"));

        // Assert
        Assert.Equal(LoginResponseStatusResultado.SenhaInvalida, result.Status);
    }

    [Fact]
    public async Task Login_QuandoCredenciaisCorretas_RetornaTokenValido()
    {
        // Arrange
        const string senha = "minhasenha123";
        var usuario = CriarUsuario("valido@test.com", senha);
        await SeedUsuarioAsync(usuario);

        using var scope = _provider.CreateScope();
        var service = CreateService(scope);

        // Act
        var result = await service.Login(new LoginRequest(usuario.Email, senha));

        // Assert
        Assert.Equal(LoginResponseStatusResultado.Sucesso, result.Status);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
    }

    [Fact]
    public async Task Login_QuandoUsuarioPossuiRoles_TokenContemRoles()
    {
        // Arrange
        const string senha = "minhasenha123";
        var roles = new List<UsuarioRole> { new("Admin"), new("Gerente") };
        var usuario = CriarUsuario("roles@test.com", senha, roles: roles);
        await SeedUsuarioAsync(usuario);

        using var scope = _provider.CreateScope();
        var service = CreateService(scope);

        // Act
        var result = await service.Login(new LoginRequest(usuario.Email, senha));

        // Assert
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(result.Token);
        var roleClaims = jwt.Claims
            .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        Assert.Contains("Admin", roleClaims);
        Assert.Contains("Gerente", roleClaims);
    }

    [Fact]
    public async Task Login_EmailCaseSensitive_DistingueUsuarios()
    {
        // Arrange
        var u1 = CriarUsuario("usuario@test.com", "senha1");
        var u2 = CriarUsuario("USUARIO@test.com", "senha2");
        await SeedUsuarioAsync(u1);
        await SeedUsuarioAsync(u2);

        using var scope = _provider.CreateScope();
        var service = CreateService(scope);

        // Act – login com email em maiúsculo
        var result = await service.Login(new LoginRequest("USUARIO@test.com", "senha2"));

        // Assert – deve encontrar o usuário correto
        Assert.Equal(LoginResponseStatusResultado.Sucesso, result.Status);
    }

    [Fact]
    public async Task Login_QuandoMultiplosLogins_SempreFunciona()
    {
        // Arrange
        const string senha = "senhaMultiplos";
        var usuario = CriarUsuario("multiplos@test.com", senha);
        await SeedUsuarioAsync(usuario);

        using var scope = _provider.CreateScope();
        var service = CreateService(scope);
        var request = new LoginRequest(usuario.Email, senha);

        // Act & Assert – chamadas repetidas devem funcionar consistentemente
        for (var i = 0; i < 5; i++)
        {
            var result = await service.Login(request);
            Assert.Equal(LoginResponseStatusResultado.Sucesso, result.Status);
        }
    }
}