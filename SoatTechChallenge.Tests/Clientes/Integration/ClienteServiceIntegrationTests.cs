using DotNet.Testcontainers.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SoatTechChallenge.Application.Clientes.DTOs;
using SoatTechChallenge.Application.Clientes.Services;
using SoatTechChallenge.Application.Clientes.Services.Validators;
using SoatTechChallenge.Application.Common.DTOs;
using SoatTechChallenge.Domain.Common.Exceptions;
using SoatTechChallenge.Domain.Common.Interfaces;
using SoatTechChallenge.Infrastucture.Database;
using SoatTechChallenge.Infrastucture.Persistence;
using Testcontainers.PostgreSql;
using Xunit;

namespace SoatTechChallenge.Tests.Clientes.Integration;

[Collection(nameof(IntegrationTestCollection))]
public class ClienteServiceIntegrationTests : IAsyncLifetime
{
    // CPFs e CNPJs matematicamente válidos
    private const string CpfValido1 = "529.982.247-25";
    private const string CpfValido1Limpo = "52998224725";
    private const string CpfValido2 = "111.444.777-35";
    private const string CnpjValido = "11.222.333/0001-81";
    private const string CnpjValidoLimpo = "11222333000181";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("soattest_clientes")
        .WithUsername("soatuser")
        .WithPassword("soatpass")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
        .Build();

    private ServiceProvider _provider = null!;

    // ────────────────────────────────────────────────────────────
    // Lifecycle
    // ────────────────────────────────────────────────────────────

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var services = new ServiceCollection();
        services.AddDbContext<SoatTechChallengeDbContext>(o => o.UseNpgsql(_postgres.GetConnectionString()));
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IClienteValidatorService, ClienteValidatorService>();
        services.AddScoped<ClienteService>();

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

    private ClienteService CreateService(IServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<ClienteService>();

    private async Task<ClienteResponse> SeedClienteAsync(
        string nome = "Cliente Seed",
        string cpf = CpfValido1)
    {
        using var scope = _provider.CreateScope();
        return await CreateService(scope).Inserir(new InserirClienteRequest(nome, cpf));
    }

    // ────────────────────────────────────────────────────────────
    // Inserir — validator integrado
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Inserir_QuandoCpfValido_PersistERetornaResponse()
    {
        using var scope = _provider.CreateScope();
        var result = await CreateService(scope)
            .Inserir(new InserirClienteRequest("João Silva", CpfValido1));

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("João Silva", result.Nome);
        Assert.Equal(CpfValido1Limpo, result.Documento);
    }

    [Fact]
    public async Task Inserir_QuandoCnpjValido_IdentificaComoCnpjEPersiste()
    {
        using var scope = _provider.CreateScope();
        var result = await CreateService(scope)
            .Inserir(new InserirClienteRequest("Empresa X", CnpjValido));

        Assert.Equal(CnpjValidoLimpo, result.Documento);
    }

    [Fact]
    public async Task Inserir_QuandoCpfJaCadastrado_LancaConflictException()
    {
        await SeedClienteAsync(cpf: CpfValido1);

        using var scope = _provider.CreateScope();
        await Assert.ThrowsAsync<ConflictException>(() =>
            CreateService(scope).Inserir(new InserirClienteRequest("Outro Nome", CpfValido1)));
    }

    [Theory]
    [InlineData("11111111111")]  // todos iguais
    [InlineData("12345678901")]  // dígitos verificadores inválidos
    [InlineData("1234567")]      // tamanho inválido
    [InlineData("abc")]          // sem dígitos
    public async Task Inserir_QuandoDocumentoInvalido_LancaDomainException(string documento)
    {
        using var scope = _provider.CreateScope();
        await Assert.ThrowsAsync<DomainException>(() =>
            CreateService(scope).Inserir(new InserirClienteRequest("Nome", documento)));
    }

    [Fact]
    public async Task Inserir_ComMascaraCpf_NormalizaEPersiste()
    {
        using var scope = _provider.CreateScope();
        var result = await CreateService(scope)
            .Inserir(new InserirClienteRequest("Mascarado", CpfValido1));

        // Documento persistido sem máscara
        Assert.Equal(CpfValido1Limpo, result.Documento);
        Assert.DoesNotContain(".", result.Documento);
        Assert.DoesNotContain("-", result.Documento);
    }

    // ────────────────────────────────────────────────────────────
    // Buscar
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Buscar_QuandoClienteExiste_RetornaDadosCorretamente()
    {
        var criado = await SeedClienteAsync("Maria Souza");

        using var scope = _provider.CreateScope();
        var result = await CreateService(scope).Buscar(criado.Id);

        Assert.Equal(criado.Id, result.Id);
        Assert.Equal("Maria Souza", result.Nome);
        Assert.Equal(CpfValido1Limpo, result.Documento);
    }

    [Fact]
    public async Task Buscar_QuandoClienteNaoExiste_LancaNotFoundException()
    {
        using var scope = _provider.CreateScope();
        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService(scope).Buscar(Guid.NewGuid()));
    }

    // ────────────────────────────────────────────────────────────
    // BuscarListaPaginada
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task BuscarListaPaginada_RetornaTotalEPaginacaoCorretos()
    {
        await SeedClienteAsync("Cliente A", CpfValido1);
        await SeedClienteAsync("Cliente B", CpfValido2);

        using var scope = _provider.CreateScope();
        var result = await CreateService(scope)
            .BuscarListaPaginada(new PagedRequest(Pagina: 1, Tamanho: 1));

        Assert.Equal(2, result.Total);
        Assert.Single(result.Itens);
    }

    [Fact]
    public async Task BuscarListaPaginada_SegundaPagina_RetornaItemsRestantes()
    {
        await SeedClienteAsync("Cliente A", CpfValido1);
        await SeedClienteAsync("Cliente B", CpfValido2);

        using var scope = _provider.CreateScope();
        var result = await CreateService(scope)
            .BuscarListaPaginada(new PagedRequest(Pagina: 2, Tamanho: 1));

        Assert.Equal(2, result.Total);
        Assert.Single(result.Itens);
    }

    // ────────────────────────────────────────────────────────────
    // Atualizar
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Atualizar_QuandoClienteExiste_PersistNovonome()
    {
        var criado = await SeedClienteAsync("Nome Antigo");

        using var scope = _provider.CreateScope();
        var result = await CreateService(scope)
            .Atualizar(criado.Id, new AtualizarClienteRequest("Nome Novo"));

        Assert.Equal("Nome Novo", result.Nome);
        Assert.Equal(criado.Documento, result.Documento); // documento inalterado

        // Confirmar persistência
        using var verifyScope = _provider.CreateScope();
        var buscado = await CreateService(verifyScope).Buscar(criado.Id);
        Assert.Equal("Nome Novo", buscado.Nome);
    }

    [Fact]
    public async Task Atualizar_QuandoClienteNaoExiste_LancaNotFoundException()
    {
        using var scope = _provider.CreateScope();
        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService(scope).Atualizar(Guid.NewGuid(), new AtualizarClienteRequest("Nome")));
    }

    // ────────────────────────────────────────────────────────────
    // Remover
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Remover_QuandoClienteExiste_ExcluiDoBanco()
    {
        var criado = await SeedClienteAsync();

        using (var scope = _provider.CreateScope())
            await CreateService(scope).Remover(criado.Id);

        using var verifyScope = _provider.CreateScope();
        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService(verifyScope).Buscar(criado.Id));
    }

    [Fact]
    public async Task Remover_QuandoClienteNaoExiste_LancaNotFoundException()
    {
        using var scope = _provider.CreateScope();
        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService(scope).Remover(Guid.NewGuid()));
    }

    // ────────────────────────────────────────────────────────────
    // CPF duplicado após remoção — deve permitir reuso
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Inserir_ApósRemoverClienteComMesmoCpf_Permite()
    {
        var criado = await SeedClienteAsync(cpf: CpfValido1);

        using (var scope = _provider.CreateScope())
            await CreateService(scope).Remover(criado.Id);

        using var insertScope = _provider.CreateScope();
        var ex = await Record.ExceptionAsync(() =>
            CreateService(insertScope).Inserir(new InserirClienteRequest("Novo Dono", CpfValido1)));

        Assert.Null(ex);
    }
}