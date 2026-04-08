using DotNet.Testcontainers.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SoatTechChallenge.Application.Clientes.Veiculos.DTOs;
using SoatTechChallenge.Application.Clientes.Veiculos.DTOs.Requests;
using SoatTechChallenge.Application.Clientes.Veiculos.DTOs.Responses;
using SoatTechChallenge.Application.Clientes.Veiculos.Services;
using SoatTechChallenge.Application.Clientes.Veiculos.Services.Validators;
using SoatTechChallenge.Application.Common.DTOs;
using SoatTechChallenge.Domain.Clientes;
using SoatTechChallenge.Domain.Clientes.Enums;
using SoatTechChallenge.Domain.Common.Exceptions;
using SoatTechChallenge.Domain.Common.Interfaces;
using SoatTechChallenge.Infrastucture.Database;
using SoatTechChallenge.Infrastucture.DomainEvents;
using SoatTechChallenge.Infrastucture.Persistence;
using Testcontainers.PostgreSql;
using Xunit;

namespace SoatTechChallenge.Tests.Clientes.Veiculos.Integration;

[Collection(nameof(IntegrationTestCollection))]
public class VeiculoServiceIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("soattest_veiculos")
        .WithUsername("soatuser")
        .WithPassword("soatpass")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
        .Build();

    private ServiceProvider _provider = null!;
    private static readonly int AnoAtual = DateTime.Now.Year;

    // ────────────────────────────────────────────────────────────
    // Lifecycle
    // ────────────────────────────────────────────────────────────

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var services = new ServiceCollection();
        services.AddDbContext<SoatTechChallengeDbContext>(o => o.UseNpgsql(_postgres.GetConnectionString()));
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IVeiculoValidatorService, VeiculoValidatorService>();
        services.AddScoped<VeiculoService>();
        services.AddScoped<IDomainEventsDispatcher, NoopDomainEventsDispatcher>();

        _provider = services.BuildServiceProvider();

        using var scope = _provider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<SoatTechChallengeDbContext>().Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _provider.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    // ────────────────────────────────────────────────────────────
    // Helpers
    // ────────────────────────────────────────────────────────────

    private VeiculoService CreateService(IServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<VeiculoService>();

    private async Task<Cliente> SeedClienteAsync(string nome = "Cliente Teste")
    {
        using var scope = _provider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<Cliente>>();
        var cliente = new Cliente();
        cliente.Inserir(nome, "52998224725", TipoDocumentoCliente.Cpf);
        await repo.InsertAsync(cliente);
        await repo.SaveChangesAsync();
        return cliente;
    }

    private async Task<VeiculoResponse> SeedVeiculoAsync(
        Guid idCliente,
        string placa = "ABC1234")
    {
        using var scope = _provider.CreateScope();
        return await CreateService(scope)
            .Inserir(idCliente, new InserirVeiculoRequest(placa, "Honda", "Civic", AnoAtual));
    }

    // ────────────────────────────────────────────────────────────
    // Inserir — validator integrado
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Inserir_QuandoClienteNaoExiste_LancaDomainException()
    {
        using var scope = _provider.CreateScope();
        await Assert.ThrowsAsync<DomainException>(() =>
            CreateService(scope).Inserir(
                Guid.NewGuid(),
                new InserirVeiculoRequest("ABC1234", "Honda", "Civic", AnoAtual)));
    }

    [Fact]
    public async Task Inserir_QuandoPlacaInvalida_LancaDomainException()
    {
        var cliente = await SeedClienteAsync();

        using var scope = _provider.CreateScope();
        await Assert.ThrowsAsync<DomainException>(() =>
            CreateService(scope).Inserir(
                cliente.Id,
                new InserirVeiculoRequest("INVALIDA", "Honda", "Civic", AnoAtual)));
    }

    [Fact]
    public async Task Inserir_QuandoDadosValidos_PersistVeiculoNoBanco()
    {
        var cliente = await SeedClienteAsync();

        using var scope = _provider.CreateScope();
        var result = await CreateService(scope).Inserir(
            cliente.Id,
            new InserirVeiculoRequest("ABC1234", "Honda", "Civic", AnoAtual));

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(cliente.Id, result.IdCliente);
        Assert.Equal("ABC1234", result.Placa);
    }

    [Fact]
    public async Task Inserir_QuandoPlacaDuplicada_LancaDomainException()
    {
        var cliente = await SeedClienteAsync();
        await SeedVeiculoAsync(cliente.Id, "ABC1234");

        using var scope = _provider.CreateScope();
        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            CreateService(scope).Inserir(
                cliente.Id,
                new InserirVeiculoRequest("ABC1234", "Toyota", "Corolla", AnoAtual)));

        Assert.Contains("ABC1234", ex.Message);
    }

    [Theory]
    [InlineData("ABC1234")]   // antiga sem hífen
    [InlineData("ABC-1234")]  // antiga com hífen
    [InlineData("ABC1D23")]   // Mercosul
    public async Task Inserir_FormatsDePlacaValidos_PersistComSucesso(string placa)
    {
        var cliente = await SeedClienteAsync();

        using var scope = _provider.CreateScope();
        var result = await CreateService(scope).Inserir(
            cliente.Id,
            new InserirVeiculoRequest(placa, "Honda", "Civic", AnoAtual));

        // Placa normalizada (sem hífen, maiúsculo)
        Assert.False(string.IsNullOrWhiteSpace(result.Placa));
        Assert.Equal(result.Placa, result.Placa.ToUpper());
    }

    // ────────────────────────────────────────────────────────────
    // Buscar
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Buscar_QuandoVeiculoExiste_RetornaDadosCorretos()
    {
        var cliente = await SeedClienteAsync();
        var criado = await SeedVeiculoAsync(cliente.Id, "ABC1D23");

        using var scope = _provider.CreateScope();
        var result = await CreateService(scope).Buscar(criado.Id);

        Assert.Equal(criado.Id, result.Id);
        Assert.Equal("ABC1D23", result.Placa);
        Assert.Equal(cliente.Id, result.IdCliente);
    }

    [Fact]
    public async Task Buscar_QuandoVeiculoNaoExiste_LancaNotFoundException()
    {
        using var scope = _provider.CreateScope();
        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService(scope).Buscar(Guid.NewGuid()));
    }

    // ────────────────────────────────────────────────────────────
    // BuscarListaPaginada
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task BuscarListaPaginada_FiltraPorClienteRetornaSomenteSeuVeiculos()
    {
        var c1 = await SeedClienteAsync("Cliente 1");

        await SeedVeiculoAsync(c1.Id, "AAA1111");
        await SeedVeiculoAsync(c1.Id, "AAA2222");

        using var scope = _provider.CreateScope();
        var result = await CreateService(scope).BuscarListaPaginada(c1.Id, new PagedRequest(1, 10));

        Assert.Equal(2, result.Total);
        Assert.All(result.Itens, r => Assert.Equal(c1.Id, r.IdCliente));
    }

    [Fact]
    public async Task BuscarListaPaginada_AplicaPaginacao()
    {
        var cliente = await SeedClienteAsync();
        await SeedVeiculoAsync(cliente.Id, "AAA1111");
        await SeedVeiculoAsync(cliente.Id, "AAA2222");
        await SeedVeiculoAsync(cliente.Id, "AAA3333");

        using var scope = _provider.CreateScope();
        var result = await CreateService(scope)
            .BuscarListaPaginada(cliente.Id, new PagedRequest(Pagina: 2, Tamanho: 2));

        Assert.Equal(3, result.Total);
        Assert.Single(result.Itens);
    }

    // ────────────────────────────────────────────────────────────
    // Atualizar
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Atualizar_QuandoDadosValidos_PersistMudancasNoBanco()
    {
        var cliente = await SeedClienteAsync();
        var criado = await SeedVeiculoAsync(cliente.Id, "ABC1234");

        using var scope = _provider.CreateScope();
        var result = await CreateService(scope).Atualizar(
            criado.Id,
            new AtualizarVeiculoRequest("XYZ9W87", "Toyota", "Corolla", AnoAtual - 1));

        Assert.Equal("XYZ9W87", result.Placa);
        Assert.Equal("Toyota", result.Marca);
        Assert.Equal("Corolla", result.Modelo);
        
        using var verifyScope = _provider.CreateScope();
        var buscado = await CreateService(verifyScope).Buscar(criado.Id);
        Assert.Equal("XYZ9W87", buscado.Placa);
    }

    [Fact]
    public async Task Atualizar_QuandoPlacaDuplicadaPorOutroVeiculo_LancaDomainException()
    {
        var cliente = await SeedClienteAsync();
        await SeedVeiculoAsync(cliente.Id, "ABC1234");
        var v2 = await SeedVeiculoAsync(cliente.Id, "XYZ9W87");

        using var scope = _provider.CreateScope();
        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            CreateService(scope).Atualizar(
                v2.Id,
                new AtualizarVeiculoRequest("ABC1234", "Honda", "Civic", AnoAtual)));

        Assert.Contains("ABC1234", ex.Message);
    }

    [Fact]
    public async Task Atualizar_ComAMesmaPlacaDoProprioVeiculo_NaoLancaExcecao()
    {
        var cliente = await SeedClienteAsync();
        var criado = await SeedVeiculoAsync(cliente.Id, "ABC1234");

        using var scope = _provider.CreateScope();
        // Atualizar o próprio veículo com a mesma placa deve funcionar
        var ex = await Record.ExceptionAsync(() =>
            CreateService(scope).Atualizar(
                criado.Id,
                new AtualizarVeiculoRequest("ABC1234", "Toyota", "Corolla", AnoAtual)));

        Assert.Null(ex);
    }

    [Fact]
    public async Task Atualizar_QuandoVeiculoNaoExiste_LancaNotFoundException()
    {
        using var scope = _provider.CreateScope();
        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService(scope).Atualizar(
                Guid.NewGuid(),
                new AtualizarVeiculoRequest("ABC1234", "Honda", "Civic", AnoAtual)));
    }

    // ────────────────────────────────────────────────────────────
    // Remover
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Remover_QuandoVeiculoExiste_ExcluiDoBanco()
    {
        var cliente = await SeedClienteAsync();
        var criado = await SeedVeiculoAsync(cliente.Id);

        using (var scope = _provider.CreateScope())
            await CreateService(scope).Remover(criado.Id);

        using var verifyScope = _provider.CreateScope();
        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService(verifyScope).Buscar(criado.Id));
    }

    [Fact]
    public async Task Remover_QuandoVeiculoNaoExiste_LancaNotFoundException()
    {
        using var scope = _provider.CreateScope();
        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService(scope).Remover(Guid.NewGuid()));
    }

    [Fact]
    public async Task Remover_ApósRemocao_PlacaFicaDisponivel()
    {
        var cliente = await SeedClienteAsync();
        var criado = await SeedVeiculoAsync(cliente.Id, "ABC1234");

        using (var scope = _provider.CreateScope())
            await CreateService(scope).Remover(criado.Id);

        // Mesma placa deve poder ser inserida novamente
        using var insertScope = _provider.CreateScope();
        var ex = await Record.ExceptionAsync(() =>
            CreateService(insertScope).Inserir(
                cliente.Id,
                new InserirVeiculoRequest("ABC1234", "Ford", "Ka", AnoAtual)));

        Assert.Null(ex);
    }
}