using Application.Servicos.Queries;
using Domain.Clientes;
using Domain.Clientes.Gateways;
using Domain.Clientes.ValueObjects;
using Domain.Clientes.Veiculos;
using Domain.Clientes.Veiculos.ValueObjects;
using Domain.OrdensServico;
using Domain.OrdensServico.Gateways;
using Domain.OrdensServico.Servicos;
using Domain.Servicos;
using Domain.Servicos.Gateways;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;
using SoatTechChallenge.Infrastucture.Database;
using SoatTechChallenge.Infrastucture.Gateways.Clientes;
using SoatTechChallenge.Infrastucture.Gateways.OrdensServico;
using SoatTechChallenge.Infrastucture.Gateways.Servicos;
using Tests.Infrastructure;
using Xunit;

namespace Tests.Servicos.Integration;

public class ServicoGatewayIntegrationTests : IntegrationTestBase
{
    private static readonly int AnoAtual = DateTime.Now.Year;
    private Guid? _osCompartilhadaId;

    protected override void RegisterServices(IServiceCollection services)
    {
        services.AddScoped<IServicoGateway, ServicoGateway>();
        services.AddScoped<IServicoQueryGateway, ServicoQueryGateway>();
        services.AddScoped<IClienteGateway, ClienteGateway>();
        services.AddScoped<IOrdemServicoGateway, OrdemServicoGateway>();
    }

    // ── Salvar / BuscarPorId ─────────────────────────────────────

    [Fact]
    public async Task Salvar_QuandoServicoValido_PersisteBanco()
    {
        var servico   = CriarServico("Alinhamento", 180m);
        await SeedAsync(servico);

        using var scope = CreateScope();
        var resultado   = await GetService<IServicoGateway>(scope)
            .BuscarPorId(servico.Id, CancellationToken.None);

        Assert.NotNull(resultado);
        Assert.Equal("Alinhamento", resultado!.Nome);
        Assert.Equal(180m, resultado.Valor);
    }

    [Fact]
    public async Task BuscarPorId_QuandoNaoExiste_RetornaNull()
    {
        using var scope = CreateScope();
        var resultado   = await GetService<IServicoGateway>(scope)
            .BuscarPorId(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(resultado);
    }

    // ── BuscarPaginado ───────────────────────────────────────────

    [Fact]
    public async Task BuscarPaginado_RetornaOrdenadoPorNomeEPaginado()
    {
        await SeedAsync(CriarServico("Zebra"), CriarServico("Abacate"), CriarServico("Manga"));

        using var scope   = CreateScope();
        var (items, total) = await GetService<IServicoQueryGateway>(scope)
            .BuscarPaginado(null, new PagedRequest(1, 2), CancellationToken.None);

        Assert.Equal(3, total);
        Assert.Equal(2, items.Count);
        Assert.Equal("Abacate", items[0].Nome);
        Assert.Equal("Manga", items[1].Nome);
    }

    [Fact]
    public async Task BuscarPaginado_SegundaPagina_RetornaRestantes()
    {
        await SeedAsync(CriarServico("Abacate"), CriarServico("Manga"), CriarServico("Zebra"));

        using var scope    = CreateScope();
        var (items, total) = await GetService<IServicoQueryGateway>(scope)
            .BuscarPaginado(null, new PagedRequest(2, 2), CancellationToken.None);

        Assert.Equal(3, total);
        Assert.Single(items);
        Assert.Equal("Zebra", items[0].Nome);
    }

    // ── BuscarTempoMedioExecucao ─────────────────────────────────

    [Fact]
    public async Task BuscarTempoMedioExecucao_QuandoSemExecucoes_RetornaVazio()
    {
        using var scope = CreateScope();
        var resultado   = await GetService<IServicoQueryGateway>(scope)
            .BuscarTempoMedioExecucao(CancellationToken.None);

        Assert.Empty(resultado);
    }

    [Fact]
    public async Task BuscarTempoMedioExecucao_CalculaEstatisticasCorretamente()
    {
        var servico = CriarServico("Troca de Óleo");
        await SeedAsync(servico);

        var agora = DateTime.UtcNow;
        await SeedOSSAsync(servico.Id, agora, agora.AddMinutes(30));
        await SeedOSSAsync(servico.Id, agora, agora.AddMinutes(60));
        await SeedOSSAsync(servico.Id, agora, agora.AddMinutes(90));

        using var scope = CreateScope();
        var resultado   = await GetService<IServicoQueryGateway>(scope)
            .BuscarTempoMedioExecucao(CancellationToken.None);

        Assert.Single(resultado);
        var stats = resultado[0];
        Assert.Equal("Troca de Óleo", stats.Servico);
        Assert.Equal(60.0, stats.TempoMedioMinutos, precision: 0);
        Assert.Equal(30.0, stats.TempoMinimoMinutos, precision: 0);
        Assert.Equal(90.0, stats.TempoMaximoMinutos, precision: 0);
    }

    // ── Atualizar ────────────────────────────────────────────────

    [Fact]
    public async Task Atualizar_QuandoExiste_PersisteMudancas()
    {
        var servico = CriarServico("Nome Antigo", 100m);
        await SeedAsync(servico);

        using var scope   = CreateScope();
        var gateway       = GetService<IServicoGateway>(scope);
        var carregado     = await gateway.BuscarPorId(servico.Id, CancellationToken.None);
        carregado!.Atualizar("Nome Novo", "Nova desc", 300m);
        await gateway.Atualizar(carregado, CancellationToken.None);

        using var verifyScope = CreateScope();
        var verificado        = await GetService<IServicoGateway>(verifyScope)
            .BuscarPorId(servico.Id, CancellationToken.None);

        Assert.Equal("Nome Novo", verificado!.Nome);
        Assert.Equal(300m, verificado.Valor);
    }

    // ── Remover ──────────────────────────────────────────────────

    [Fact]
    public async Task Remover_QuandoExiste_ExcluiDoBanco()
    {
        var servico = CriarServico();
        await SeedAsync(servico);

        using var scope   = CreateScope();
        var gateway       = GetService<IServicoGateway>(scope);
        var carregado     = await gateway.BuscarPorId(servico.Id, CancellationToken.None);
        await gateway.Remover(carregado!, CancellationToken.None);

        using var verifyScope = CreateScope();
        var resultado         = await GetService<IServicoGateway>(verifyScope)
            .BuscarPorId(servico.Id, CancellationToken.None);

        Assert.Null(resultado);
    }

    // ── Helpers ──────────────────────────────────────────────────

    private static Servico CriarServico(string nome = "Serviço Teste", decimal valor = 100m)
    {
        var s = new Servico();
        s.Inserir(nome, "Descrição", valor);
        return s;
    }

    private async Task SeedAsync(params Servico[] servicos)
    {
        using var scope = CreateScope();
        var gateway     = GetService<IServicoGateway>(scope);
        foreach (var s in servicos)
            await gateway.Salvar(s, CancellationToken.None);
    }

    private async Task SeedOSSAsync(Guid idServico, DateTime inicio, DateTime fim)
    {
        // Cria a OrdemServico pai uma única vez por instância de teste
        if (_osCompartilhadaId is null)
            _osCompartilhadaId = await CriarOrdemServicoParenteAsync();

        using var scope = CreateScope();
        var db          = GetService<SoatTechChallengeDbContext>(scope);

        var oss = new OrdemServicoServico(_osCompartilhadaId.Value, idServico, "Serviço", 100m);
        oss.IniciarExecucao();

        typeof(OrdemServicoServico)
            .GetProperty(nameof(OrdemServicoServico.DataInicioExecucao))!
            .SetValue(oss, inicio);

        oss.FinalizarExecucao();

        typeof(OrdemServicoServico)
            .GetProperty(nameof(OrdemServicoServico.DataFinalizacaoExecucao))!
            .SetValue(oss, fim);

        db.Set<OrdemServicoServico>().Add(oss);
        await db.SaveChangesAsync();
    }

    private async Task<Guid> CriarOrdemServicoParenteAsync()
    {
        using var scope = CreateScope();
        var db          = GetService<SoatTechChallengeDbContext>(scope);

        // Cliente
        var cliente = new Cliente();
        cliente.Inserir("Cliente OSS", DocumentoCliente.Criar("52998224725"));
        db.Set<Cliente>().Add(cliente);
        await db.SaveChangesAsync();

        // Veiculo
        var veiculo = new Veiculo();
        veiculo.Inserir(cliente.Id, Placa.Criar("ABC1234"), "Honda", "Civic", AnoAtual);
        db.Set<Veiculo>().Add(veiculo);
        await db.SaveChangesAsync();

        // OrdemServico — pai obrigatório para OrdemServicoServico
        var os = new OrdemServico();
        os.Inserir(cliente.Id, veiculo.Id, [], []);
        db.Set<OrdemServico>().Add(os);
        await db.SaveChangesAsync();

        return os.Id;
    }
}