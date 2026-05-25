using Domain.Common.Exceptions;
using Domain.Usuarios;
using Domain.Usuarios.Roles;

namespace Tests.Usuarios;

public class UsuarioTests
{
    [Theory]
    [InlineData("", "email@email.com", "hash")]
    [InlineData("   ", "email@email.com", "hash")]
    [InlineData(null, "email@email.com", "hash")]
    public void Construtor_QuandoNomeInvalido_LancaDomainException(string? nome, string email, string hash)
    {
        Assert.Throws<DomainException>(() => new Usuario(nome!, email, hash));
    }

    [Theory]
    [InlineData("Nome", "", "hash")]
    [InlineData("Nome", "   ", "hash")]
    [InlineData("Nome", null, "hash")]
    public void Construtor_QuandoEmailInvalido_LancaDomainException(string nome, string? email, string hash)
    {
        Assert.Throws<DomainException>(() => new Usuario(nome, email!, hash));
    }

    [Theory]
    [InlineData("Nome", "email@email.com", "")]
    [InlineData("Nome", "email@email.com", "   ")]
    [InlineData("Nome", "email@email.com", null)]
    public void Construtor_QuandoSenhaInvalida_LancaDomainException(string nome, string email, string? hash)
    {
        Assert.Throws<DomainException>(() => new Usuario(nome, email, hash!));
    }

    [Fact]
    public void Construtor_QuandoDadosValidos_CriaUsuarioCorreto()
    {
        // Arrange & Act
        var usuario = new Usuario("João", "joao@email.com", "hash123");

        // Assert
        Assert.Equal("João", usuario.Nome);
        Assert.Equal("joao@email.com", usuario.Email);
        Assert.Equal("hash123", usuario.SenhaHash);
        Assert.NotEqual(Guid.Empty, usuario.Id);
        Assert.True(usuario.DataCriacao <= DateTime.UtcNow);
        Assert.Empty(usuario.Roles);
    }

    [Fact]
    public void AdicionarRoles_QuandoListaValida_AdicionaRoles()
    {
        // Arrange
        var usuario = new Usuario("João", "joao@email.com", "hash123");
        var roles = new List<UsuarioRole> { new("Admin"), new("Operador") };

        // Act
        usuario.AdicionarRoles(roles);

        // Assert
        Assert.Equal(2, usuario.Roles.Count);
        Assert.Contains(usuario.Roles, r => r.Role == "Admin");
        Assert.Contains(usuario.Roles, r => r.Role == "Operador");
    }

    [Fact]
    public void AdicionarRoles_QuandoChamadoMultiplasVezes_AcumulaRoles()
    {
        // Arrange
        var usuario = new Usuario("João", "joao@email.com", "hash123");

        // Act
        usuario.AdicionarRoles(new List<UsuarioRole> { new("Admin") });
        usuario.AdicionarRoles(new List<UsuarioRole> { new("Operador") });

        // Assert
        Assert.Equal(2, usuario.Roles.Count);
    }

    [Fact]
    public void Construtor_GeraIdUnico_CadaInstancia()
    {
        var u1 = new Usuario("A", "a@a.com", "hash");
        var u2 = new Usuario("B", "b@b.com", "hash");

        Assert.NotEqual(u1.Id, u2.Id);
    }
}