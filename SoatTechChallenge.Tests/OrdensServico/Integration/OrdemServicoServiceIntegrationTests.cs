using DotNet.Testcontainers.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SoatTechChallenge.Application.Common.DTOs;
using SoatTechChallenge.Application.OrdensServico.DTOs.Requests;
using SoatTechChallenge.Application.OrdensServico.Services;
using SoatTechChallenge.Application.OrdensServico.Services.Validators;
using SoatTechChallenge.Domain.Clientes;
using SoatTechChallenge.Domain.Clientes.Enums;
using SoatTechChallenge.Domain.Clientes.Veiculos;
using SoatTechChallenge.Domain.Common.Exceptions;
using SoatTechChallenge.Domain.Common.Interfaces;
using SoatTechChallenge.Domain.OrdensServico;
using SoatTechChallenge.Domain.OrdensServico.Enums;
using SoatTechChallenge.Domain.Produtos;
using SoatTechChallenge.Domain.Servicos;
using SoatTechChallenge.Infrastucture.Database;
using SoatTechChallenge.Infrastucture.Persistence;
using Testcontainers.PostgreSql;
using Xunit;

namespace SoatTechChallenge.Tests.OrdensServico.Integration;

[Collection(nameof(IntegrationTestCollection))]
public class OrdemServicoServiceIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("soattest_os")
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
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOrdemServicoValidatorService, OrdemServicoValidatorService>();
        services.AddScoped<OrdemServicoService>();

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
    // Seed helpers
    // ────────────────────────────────────────────────────────────

    private async Task<Cliente> SeedClienteComVeiculoAsync()
    {
        using var scope = _provider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<Cliente>>();

        var cliente = new Cliente();
        cliente.Inserir("Cliente Integração", "98765432100", TipoDocumentoCliente.Cpf);

        var veiculo = new Veiculo();
        veiculo.Inserir(cliente.Id, "INT1A23", "Toyota", "Corolla", DateTime.Now.Year);
        cliente.Veiculos.Add(veiculo);

        await repo.InsertAsync(cliente);
        return cliente;
    }

    private async Task<Servico> SeedServicoAsync(string nome = "Serviço Int", decimal valor = 200m)
    {
        using var scope = _provider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<Servico>>();
        var s = new Servico();
        s.Inserir(nome, "", valor);
        await repo.InsertAsync(s);
        return s;
    }

    private async Task<Produto> SeedProdutoAsync(string nome = "Produto Int", decimal valor = 50m, decimal estoque = 20m)
    {
        using var scope = _provider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<Produto>>();
        var p = new Produto();
        p.Inserir(nome, "Desc", valor, estoque);
        await repo.InsertAsync(p);
        return p;
    }

    private async Task<OrdemServico> SeedOSAsync(Guid idCliente, Guid idVeiculo, List<Guid>? idsServicos = null)
    {
        using var scope = _provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<OrdemServicoService>();
        var request = new InserirOrdemServicoRequest(idCliente, idVeiculo, idsServicos ?? new List<Guid>());
        await service.Inserir(request);

        // Recuperar a OS recém-criada
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<OrdemServico>>();
        return await repo.GetQueryable()
            .Include(o => o.Servicos)
            .Include(o => o.Produtos)
            .OrderByDescending(o => o.DataCriacao)
            .FirstAsync();
    }

    private OrdemServicoService CreateService(IServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<OrdemServicoService>();

    // ────────────────────────────────────────────────────────────
    // Inserir + Validator integrado
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Inserir_QuandoClienteNaoExiste_LancaDomainException()
    {
        using var scope = _provider.CreateScope();
        var request = new InserirOrdemServicoRequest(Guid.NewGuid(), Guid.NewGuid(), new List<Guid>());

        await Assert.ThrowsAsync<DomainException>(() => CreateService(scope).Inserir(request));
    }

    [Fact]
    public async Task Inserir_QuandoVeiculoNaoPertenceAoCliente_LancaDomainException()
    {
        var cliente = await SeedClienteComVeiculoAsync();

        using var scope = _provider.CreateScope();
        var request = new InserirOrdemServicoRequest(cliente.Id, Guid.NewGuid(), new List<Guid>());

        await Assert.ThrowsAsync<DomainException>(() => CreateService(scope).Inserir(request));
    }

    [Fact]
    public async Task Inserir_QuandoDadosValidos_PersistOSComServicos()
    {
        var cliente = await SeedClienteComVeiculoAsync();
        var servico = await SeedServicoAsync();

        using var scope = _provider.CreateScope();
        var request = new InserirOrdemServicoRequest(
            cliente.Id,
            cliente.Veiculos[0].Id,
            new List<Guid> { servico.Id });

        await CreateService(scope).Inserir(request);

        // Verificar persistência
        using var verifyScope = _provider.CreateScope();
        var os = await verifyScope.ServiceProvider
            .GetRequiredService<IRepository<OrdemServico>>()
            .GetQueryable()
            .Include(o => o.Servicos)
            .FirstAsync();

        Assert.Equal(StatusOrdemServico.Recebida, os.Status);
        Assert.Single(os.Servicos);
        Assert.Equal(servico.Valor, os.ValorTotal);
    }

    // ────────────────────────────────────────────────────────────
    // Buscar
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Buscar_QuandoExiste_RetornaDadosComJoinsCorretamente()
    {
        var cliente = await SeedClienteComVeiculoAsync();
        var os = await SeedOSAsync(cliente.Id, cliente.Veiculos[0].Id);

        using var scope = _provider.CreateScope();
        var result = await CreateService(scope).Buscar(os.Id);

        Assert.NotNull(result);
        Assert.Equal(cliente.Nome, result!.Cliente.Nome);
        Assert.Equal(cliente.Veiculos[0].Placa, result.Veiculo.Placa);
    }

    [Fact]
    public async Task Buscar_QuandoNaoExiste_RetornaNull()
    {
        using var scope = _provider.CreateScope();
        var result = await CreateService(scope).Buscar(Guid.NewGuid());

        Assert.Null(result);
    }

    // ────────────────────────────────────────────────────────────
    // Fluxo completo: Inserir → Diagnóstico → Execução → Entrega
    // com decremento de estoque transacional
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task FluxoCompleto_DecrementaEstoqueNaFinalizacaoEmTransacao()
    {
        var cliente = await SeedClienteComVeiculoAsync();
        var servico = await SeedServicoAsync();
        var produto = await SeedProdutoAsync(estoque: 15m);

        // 1. Inserir
        var os = await SeedOSAsync(cliente.Id, cliente.Veiculos[0].Id, new List<Guid> { servico.Id });

        // 2. Iniciar diagnóstico
        using (var s = _provider.CreateScope())
            await CreateService(s).IniciarDiagnostico(os.Id);

        // 3. Inserir produto
        using (var s = _provider.CreateScope())
            await CreateService(s).InserirProdutos(os.Id, new InserirProdutosOrdemServicoRequest(
                new List<InserirProdutosOrdemServicoProdutoRequest> { new(produto.Id, 3m) }));

        // 4. Finalizar diagnóstico
        using (var s = _provider.CreateScope())
            await CreateService(s).FinalizarDiagnostico(os.Id);

        // 5. Aprovar orçamento
        using (var s = _provider.CreateScope())
            await CreateService(s).AprovarOrcamento(os.Id);

        // 6. Iniciar serviço
        var osAtual = await ObterOSAsync(os.Id);
        var idServicoOS = osAtual.Servicos[0].Id;

        using (var s = _provider.CreateScope())
            await CreateService(s).IniciarExecucaoServico(os.Id, idServicoOS);

        // 7. Finalizar serviço → deve decrementar estoque em transação
        using (var s = _provider.CreateScope())
            await CreateService(s).FinalizarExecucaoServico(os.Id, idServicoOS);

        // 8. Verificar OS finalizada
        var osFinal = await ObterOSAsync(os.Id);
        Assert.Equal(StatusOrdemServico.Finalizada, osFinal.Status);

        // 9. Verificar estoque decrementado
        using var verifyScope = _provider.CreateScope();
        var produtoAtualizado = await verifyScope.ServiceProvider
            .GetRequiredService<IRepository<Produto>>()
            .GetQueryable()
            .FirstAsync(p => p.Id == produto.Id);

        Assert.Equal(12m, produtoAtualizado.QuantidadeEmEstoque); // 15 - 3

        // 10. Entregar
        using (var s = _provider.CreateScope())
            await CreateService(s).Entregar(os.Id);

        var osEntregue = await ObterOSAsync(os.Id);
        Assert.Equal(StatusOrdemServico.Entregue, osEntregue.Status);
    }

    // ────────────────────────────────────────────────────────────
    // BuscarListaPaginadaPorDocumento
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task BuscarListaPaginadaPorDocumento_ComMascara_RetornaOSDoCliente()
    {
        var cliente = await SeedClienteComVeiculoAsync();
        await SeedOSAsync(cliente.Id, cliente.Veiculos[0].Id);

        using var scope = _provider.CreateScope();
        // Passa com máscara — deve limpar e filtrar corretamente
        var result = await CreateService(scope)
            .BuscarListaPaginadaPorDocumento("987.654.321-00", new PagedRequest(1, 10));

        Assert.Equal(1, result.Total);
        Assert.Equal(cliente.Nome, result.Itens[0].Cliente.Nome);
    }

    // ────────────────────────────────────────────────────────────
    // Remover
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Remover_QuandoExiste_ExcluiDoBanco()
    {
        var cliente = await SeedClienteComVeiculoAsync();
        var os = await SeedOSAsync(cliente.Id, cliente.Veiculos[0].Id);

        using (var s = _provider.CreateScope())
            await CreateService(s).Remover(os.Id);

        using var verifyScope = _provider.CreateScope();
        var result = await CreateService(verifyScope).Buscar(os.Id);
        Assert.Null(result);
    }

    [Fact]
    public async Task Remover_QuandoNaoExiste_LancaNotFoundException()
    {
        using var scope = _provider.CreateScope();
        await Assert.ThrowsAsync<NotFoundException>(() => CreateService(scope).Remover(Guid.NewGuid()));
    }

    // ────────────────────────────────────────────────────────────
    // Helper
    // ────────────────────────────────────────────────────────────

    private async Task<OrdemServico> ObterOSAsync(Guid id)
    {
        using var scope = _provider.CreateScope();
        return await scope.ServiceProvider
            .GetRequiredService<IRepository<OrdemServico>>()
            .GetQueryable()
            .AsSplitQuery()
            .Include(o => o.Servicos)
            .Include(o => o.Produtos)
            .FirstAsync(o => o.Id == id);
    }
}