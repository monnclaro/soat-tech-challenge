using Application.OrdensServico.Queries;
using Domain.Clientes;
using Domain.Clientes.Gateways;
using Domain.Clientes.ValueObjects;
using Domain.Clientes.Veiculos;
using Domain.Clientes.Veiculos.ValueObjects;
using Domain.OrdensServico;
using Domain.OrdensServico.Enums;
using Domain.OrdensServico.Gateways;
using Domain.OrdensServico.Produtos;
using Domain.OrdensServico.Servicos;
using Domain.Produtos;
using Domain.Produtos.Gateways;
using Domain.Servicos;
using Domain.Servicos.Gateways;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;
using SoatTechChallenge.Infrastucture.Database;
using SoatTechChallenge.Infrastucture.DomainEvents;
using SoatTechChallenge.Infrastucture.Gateways.Clientes;
using SoatTechChallenge.Infrastucture.Gateways.OrdensServico;
using SoatTechChallenge.Infrastucture.Gateways.Produtos;
using SoatTechChallenge.Infrastucture.Gateways.Servicos;
using Testcontainers.PostgreSql;
using Tests.Infrastructure;
using Xunit;

namespace Tests.OrdensServico.Integration;

public class OrdemServicoGatewayIntegrationTests : IntegrationTestBase
{
    protected override void RegisterServices(IServiceCollection services)
    {
        services.AddScoped<IClienteGateway, ClienteGateway>();
        services.AddScoped<IServicoGateway, ServicoGateway>();
        services.AddScoped<IProdutoGateway, ProdutoGateway>();
        services.AddScoped<IOrdemServicoGateway, OrdemServicoGateway>();
        services.AddScoped<IOrdemServicoQueryGateway, OrdemServicoQueryGateway>();
    }
    
    private static readonly int AnoAtual = DateTime.Now.Year;

    // ── Salvar / BuscarPorId ─────────────────────────────────────

    [Fact]
    public async Task Salvar_QuandoDadosValidos_PersisteBanco()
    {
        var (cliente, veiculo) = await SeedClienteComVeiculoAsync();
        var os = CriarOrdemServico(cliente.Id, veiculo.Id);
        await SeedOSAsync(os);

        using var scope = CreateScope();
        var gateway   = scope.ServiceProvider.GetRequiredService<IOrdemServicoGateway>();
        var resultado = await gateway.BuscarPorId(os.Id, CancellationToken.None);

        Assert.NotNull(resultado);
        Assert.Equal(StatusOrdemServico.Recebida, resultado!.Status);
        Assert.Equal(cliente.Id, resultado.IdCliente);
    }

    [Fact]
    public async Task BuscarPorId_QuandoNaoExiste_RetornaNull()
    {
        using var scope = CreateScope();
        var gateway   = scope.ServiceProvider.GetRequiredService<IOrdemServicoGateway>();
        var resultado = await gateway.BuscarPorId(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(resultado);
    }

    // ── BuscarComServicos ────────────────────────────────────────

    [Fact]
    public async Task BuscarComServicos_RetornaComServicosCarregados()
    {
        var (cliente, veiculo) = await SeedClienteComVeiculoAsync();
        var servico            = await SeedServicoAsync();
        var servicoOs          = new OrdemServicoServico(Guid.NewGuid(), servico.Id, servico.Nome, servico.Valor);
        var os                 = CriarOrdemServico(cliente.Id, veiculo.Id, [servicoOs]);
        await SeedOSAsync(os);

        using var scope = CreateScope();
        var gateway   = scope.ServiceProvider.GetRequiredService<IOrdemServicoGateway>();
        var resultado = await gateway.BuscarComServicos(os.Id, CancellationToken.None);

        Assert.NotNull(resultado);
        Assert.Single(resultado!.Servicos);
    }

    // ── BuscarComServicosProdutos ────────────────────────────────

    [Fact]
    public async Task BuscarComServicosProdutos_RetornaComAmbosCarregados()
    {
        var (cliente, veiculo) = await SeedClienteComVeiculoAsync();
        var servico            = await SeedServicoAsync();
        var produto            = await SeedProdutoAsync();
        var servicoOs          = new OrdemServicoServico(Guid.NewGuid(), servico.Id, servico.Nome, servico.Valor);
        var produtoOs          = new OrdemServicoProduto(Guid.NewGuid(), produto.Id, produto.Nome, produto.Valor, 2);
        var os                 = CriarOrdemServico(cliente.Id, veiculo.Id, [servicoOs], [produtoOs]);
        await SeedOSAsync(os);

        using var scope = CreateScope();
        var gateway   = scope.ServiceProvider.GetRequiredService<IOrdemServicoGateway>();
        var resultado = await gateway.BuscarComServicosProdutos(os.Id, CancellationToken.None);

        Assert.NotNull(resultado);
        Assert.Single(resultado!.Servicos);
        Assert.Single(resultado.Produtos);
    }

    // ── BuscarComDetalhes (QueryGateway) ─────────────────────────

    [Fact]
    public async Task BuscarComDetalhes_RetornaComJoinsDeClienteEVeiculo()
    {
        var (cliente, veiculo) = await SeedClienteComVeiculoAsync();
        var os                 = CriarOrdemServico(cliente.Id, veiculo.Id);
        await SeedOSAsync(os);

        using var scope = CreateScope();
        var gateway   = scope.ServiceProvider.GetRequiredService<IOrdemServicoQueryGateway>();
        var resultado = await gateway.BuscarComDetalhes(os.Id, CancellationToken.None);

        Assert.NotNull(resultado);
        Assert.Equal(cliente.Nome, resultado!.Cliente.Nome);
        Assert.Equal(veiculo.Placa, resultado.Veiculo.Placa);
    }

    // ── BuscarStatus ─────────────────────────────────────────────

    [Fact]
    public async Task BuscarStatus_RetornaStatusCorreto()
    {
        var (cliente, veiculo) = await SeedClienteComVeiculoAsync();
        var os                 = CriarOrdemServico(cliente.Id, veiculo.Id);
        await SeedOSAsync(os);

        using var scope = CreateScope();
        var gateway   = scope.ServiceProvider.GetRequiredService<IOrdemServicoQueryGateway>();
        var resultado = await gateway.BuscarStatus(os.Id, CancellationToken.None);

        Assert.NotNull(resultado);
        Assert.Equal("Recebida", resultado!.Status);
    }

    // ── BuscarPaginado ───────────────────────────────────────────

    [Fact]
    public async Task BuscarPaginado_RetornaApenasNaoFinalizadas()
    {
        var (cliente, veiculo) = await SeedClienteComVeiculoAsync();
        var os1 = CriarOrdemServico(cliente.Id, veiculo.Id);
        await SeedOSAsync(os1);

        using var scope = CreateScope();
        var gateway = scope.ServiceProvider.GetRequiredService<IOrdemServicoQueryGateway>();

        var (items, total) = await gateway.BuscarPaginado(new PagedRequest(1, 10), CancellationToken.None);

        Assert.Equal(1, total);
        Assert.Single(items);
    }

    // ── Atualizar ────────────────────────────────────────────────

    [Fact]
    public async Task Atualizar_QuandoStatusMuda_PersisteMudanca()
    {
        var (cliente, veiculo) = await SeedClienteComVeiculoAsync();
        var os                 = CriarOrdemServico(cliente.Id, veiculo.Id);
        await SeedOSAsync(os);

        using var scope = CreateScope();
        var gateway   = scope.ServiceProvider.GetRequiredService<IOrdemServicoGateway>();
        var carregado = await gateway.BuscarPorId(os.Id, CancellationToken.None);
        carregado!.IniciarDiagnostico();
        await gateway.Atualizar(carregado, CancellationToken.None);

        using var verifyScope = CreateScope();
        var verificado = await verifyScope.ServiceProvider
            .GetRequiredService<IOrdemServicoGateway>()
            .BuscarPorId(os.Id, CancellationToken.None);

        Assert.Equal(StatusOrdemServico.EmDiagnostico, verificado!.Status);
    }

    // ── Remover ──────────────────────────────────────────────────

    [Fact]
    public async Task Remover_QuandoExiste_ExcluiDoBanco()
    {
        var (cliente, veiculo) = await SeedClienteComVeiculoAsync();
        var os                 = CriarOrdemServico(cliente.Id, veiculo.Id);
        await SeedOSAsync(os);

        using var scope = CreateScope();
        var gateway   = scope.ServiceProvider.GetRequiredService<IOrdemServicoGateway>();
        var carregado = await gateway.BuscarPorId(os.Id, CancellationToken.None);
        await gateway.Remover(carregado!, CancellationToken.None);

        using var verifyScope = CreateScope();
        var resultado = await verifyScope.ServiceProvider
            .GetRequiredService<IOrdemServicoGateway>()
            .BuscarPorId(os.Id, CancellationToken.None);

        Assert.Null(resultado);
    }

    // ── BuscarPaginadoPorDocumento ───────────────────────────────

    [Fact]
    public async Task BuscarPaginadoPorDocumento_FiltraPorDocumentoDoCliente()
    {
        var (cliente, veiculo) = await SeedClienteComVeiculoAsync();
        var os                 = CriarOrdemServico(cliente.Id, veiculo.Id);
        await SeedOSAsync(os);

        using var scope = CreateScope();
        var gateway = scope.ServiceProvider.GetRequiredService<IOrdemServicoQueryGateway>();

        var (items, total) = await gateway.BuscarPaginadoPorDocumento(
            "52998224725", new PagedRequest(1, 10), CancellationToken.None);

        Assert.Equal(1, total);
        Assert.Equal(cliente.Nome, items[0].Cliente.Nome);
    }

    // ── Helpers ──────────────────────────────────────────────────

    private static OrdemServico CriarOrdemServico(
        Guid idCliente,
        Guid idVeiculo,
        List<OrdemServicoServico>? servicos = null,
        List<OrdemServicoProduto>? produtos = null)
    {
        var os = new OrdemServico();
        os.Inserir(idCliente, idVeiculo, servicos ?? [], produtos ?? []);
        return os;
    }

    private async Task<(Cliente cliente, Veiculo veiculo)> SeedClienteComVeiculoAsync()
    {
        using var scope         = CreateScope();
        var clienteGateway      = scope.ServiceProvider.GetRequiredService<IClienteGateway>();
        var db                  = scope.ServiceProvider.GetRequiredService<SoatTechChallengeDbContext>();

        var cliente = new Cliente();
        cliente.Inserir("Cliente Teste", DocumentoCliente.Criar("52998224725"));
        await clienteGateway.Salvar(cliente, CancellationToken.None);

        var veiculo = new Veiculo();
        veiculo.Inserir(cliente.Id, Placa.Criar("ABC1234"), "Honda", "Civic", AnoAtual);
        db.Set<Veiculo>().Add(veiculo);
        await db.SaveChangesAsync();

        return (cliente, veiculo);
    }

    private async Task<Servico> SeedServicoAsync()
    {
        using var scope = CreateScope();
        var gateway     = scope.ServiceProvider.GetRequiredService<IServicoGateway>();
        var s           = new Servico();
        s.Inserir("Serviço Teste", "Desc", 100m);
        await gateway.Salvar(s, CancellationToken.None);
        return s;
    }

    private async Task<Produto> SeedProdutoAsync()
    {
        using var scope = CreateScope();
        var gateway     = scope.ServiceProvider.GetRequiredService<IProdutoGateway>();
        var p           = new Produto();
        p.Inserir("Produto Teste", "Desc", 50m, 20);
        await gateway.Salvar(p, CancellationToken.None);
        return p;
    }

    private async Task SeedOSAsync(OrdemServico os)
    {
        using var scope = CreateScope();
        var gateway     = scope.ServiceProvider.GetRequiredService<IOrdemServicoGateway>();
        await gateway.Salvar(os, CancellationToken.None);
    }
}