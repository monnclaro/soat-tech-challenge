using Domain.Usuarios;
using Domain.Usuarios.Gateways;
using Domain.Usuarios.Roles;
using Microsoft.Extensions.DependencyInjection;
using SoatTechChallenge.Infrastucture.Gateways.Usuarios;
using Tests.Infrastructure;
using Xunit;

namespace Tests.Authentication.Integration;

public class UsuarioGatewayIntegrationTests : IntegrationTestBase
{
    protected override void RegisterServices(IServiceCollection services)
    {
        services.AddScoped<IUsuarioGateway, UsuarioGateway>();
    }

    [Fact]
    public async Task BuscarPorEmail_QuandoUsuarioExiste_RetornaUsuario()
    {
        var usuario = new Usuario("João", "joao@test.com", "hash");
        await SeedAsync(usuario);

        using var scope = CreateScope();
        var resultado   = await GetService<IUsuarioGateway>(scope)
            .BuscarPorEmail("joao@test.com", CancellationToken.None);

        Assert.NotNull(resultado);
        Assert.Equal("João", resultado!.Nome);
    }

    [Fact]
    public async Task BuscarPorEmail_QuandoUsuarioNaoExiste_RetornaNull()
    {
        using var scope = CreateScope();
        var resultado   = await GetService<IUsuarioGateway>(scope)
            .BuscarPorEmail("naoexiste@test.com", CancellationToken.None);

        Assert.Null(resultado);
    }

    [Fact]
    public async Task BuscarPorEmail_QuandoUsuarioPossuiRoles_RetornaComRoles()
    {
        var roles   = new List<UsuarioRole> { new("Admin"), new("Gerente") };
        var usuario = new Usuario("Maria", "maria@test.com", "hash");
        usuario.AdicionarRoles(roles);
        await SeedAsync(usuario);

        using var scope = CreateScope();
        var resultado   = await GetService<IUsuarioGateway>(scope)
            .BuscarPorEmail("maria@test.com", CancellationToken.None);

        Assert.NotNull(resultado);
        Assert.Contains(resultado!.Roles, r => r.Role == "Admin");
        Assert.Contains(resultado.Roles, r => r.Role == "Gerente");
    }

    [Fact]
    public async Task BuscarPorEmail_EmailCaseSensitive_DistingueUsuarios()
    {
        await SeedAsync(new Usuario("Lower", "usuario@test.com", "hash1"));
        await SeedAsync(new Usuario("Upper", "USUARIO@test.com", "hash2"));

        using var scope = CreateScope();
        var resultado   = await GetService<IUsuarioGateway>(scope)
            .BuscarPorEmail("USUARIO@test.com", CancellationToken.None);

        Assert.Equal("Upper", resultado?.Nome);
    }

    private async Task SeedAsync(Usuario usuario)
    {
        using var scope = CreateScope();
        await GetService<IUsuarioGateway>(scope).Salvar(usuario, CancellationToken.None);
    }
}