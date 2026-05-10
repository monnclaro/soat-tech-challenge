using MockQueryable.Moq;
using Moq;
using SoatTechChallenge.Application.Authentication.DTOs.Requests;
using SoatTechChallenge.Application.Authentication.DTOs.Responses;
using SoatTechChallenge.Application.Authentication.Interfaces;
using SoatTechChallenge.Application.Authentication.Services;
using SoatTechChallenge.Domain.Common.Interfaces;
using SoatTechChallenge.Domain.Usuarios;
using SoatTechChallenge.Domain.Usuarios.Roles;
using Xunit;

namespace SoatTechChallenge.Tests.Authentication.Unit;

public class AuthenticationServiceTests
{
    private readonly Mock<IRepository<Usuario>> _repositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ITokenProvider> _tokenProviderMock;
    private readonly AuthenticationService _sut;

    public AuthenticationServiceTests()
    {
        _repositoryMock    = new Mock<IRepository<Usuario>>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _tokenProviderMock  = new Mock<ITokenProvider>();

        _sut = new AuthenticationService(
            _repositoryMock.Object,
            _passwordHasherMock.Object,
            _tokenProviderMock.Object);
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
        var usuario = CriarUsuario("email@email.com");
        var request = new LoginRequest(usuario.Email, "senhaErrada");
        SetupRepositoryReturning(usuario);
        _passwordHasherMock
            .Setup(h => h.Verificar(request.Senha, usuario.SenhaHash))
            .Returns(false);

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
        var usuario = CriarUsuario("email@email.com");
        var request = new LoginRequest(usuario.Email, "senha123");
        SetupRepositoryReturning(usuario);
        _passwordHasherMock
            .Setup(h => h.Verificar(request.Senha, usuario.SenhaHash))
            .Returns(true);
        _tokenProviderMock
            .Setup(t => t.GerarToken(usuario))
            .Returns("token-gerado");

        // Act
        var result = await _sut.Login(request);

        // Assert
        Assert.Equal(LoginResponseStatusResultado.Sucesso, result.Status);
        Assert.Equal("token-gerado", result.Token);
    }

    [Fact]
    public async Task Login_QuandoSucesso_InvocaGerarTokenComUsuarioCorreto()
    {
        // Arrange
        var usuario = CriarUsuario("email@email.com", nome: "João Silva");
        var request = new LoginRequest(usuario.Email, "senha123");
        SetupRepositoryReturning(usuario);
        _passwordHasherMock
            .Setup(h => h.Verificar(request.Senha, usuario.SenhaHash))
            .Returns(true);
        _tokenProviderMock
            .Setup(t => t.GerarToken(It.IsAny<Usuario>()))
            .Returns("token-qualquer");

        // Act
        await _sut.Login(request);

        // Assert
        _tokenProviderMock.Verify(t => t.GerarToken(
            It.Is<Usuario>(u => u.Nome == "João Silva" && u.Email == "email@email.com")),
            Times.Once);
    }

    [Fact]
    public async Task Login_QuandoSucesso_NaoInvocaGerarTokenComSenhaErrada()
    {
        // Arrange
        var usuario = CriarUsuario("email@email.com");
        var request = new LoginRequest(usuario.Email, "senhaErrada");
        SetupRepositoryReturning(usuario);
        _passwordHasherMock
            .Setup(h => h.Verificar(request.Senha, usuario.SenhaHash))
            .Returns(false);

        // Act
        await _sut.Login(request);

        // Assert
        _tokenProviderMock.Verify(t => t.GerarToken(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public async Task Login_QuandoSucesso_RetornaTokenDoProvider()
    {
        // Arrange
        const string tokenEsperado = "jwt.token.gerado";
        var usuario = CriarUsuario("email@email.com");
        var request = new LoginRequest(usuario.Email, "senha123");
        SetupRepositoryReturning(usuario);
        _passwordHasherMock
            .Setup(h => h.Verificar(request.Senha, usuario.SenhaHash))
            .Returns(true);
        _tokenProviderMock
            .Setup(t => t.GerarToken(usuario))
            .Returns(tokenEsperado);

        // Act
        var result = await _sut.Login(request);

        // Assert
        Assert.Equal(tokenEsperado, result.Token);
    }

    private static Usuario CriarUsuario(
        string email,
        string senhaHash = "hash-qualquer",
        string nome = "Usuário Teste",
        List<UsuarioRole>? roles = null)
    {
        var usuario = new Usuario(nome, email, senhaHash);

        if (roles is { Count: > 0 })
            usuario.AdicionarRoles(roles);

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
}