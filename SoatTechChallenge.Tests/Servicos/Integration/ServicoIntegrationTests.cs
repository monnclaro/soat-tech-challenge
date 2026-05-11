using DotNet.Testcontainers.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SoatTechChallenge.Application.Common.DTOs;
using SoatTechChallenge.Application.Servicos.DTOs.Requests;
using SoatTechChallenge.Application.Servicos.DTOs.Responses;
using SoatTechChallenge.Application.Servicos.Services;
using SoatTechChallenge.Domain.Clientes;
using SoatTechChallenge.Domain.Clientes.Enums;
using SoatTechChallenge.Domain.Clientes.Veiculos;
using SoatTechChallenge.Domain.Common.Exceptions;
using SoatTechChallenge.Domain.Common.Interfaces;
using SoatTechChallenge.Domain.OrdensServico;
using SoatTechChallenge.Domain.OrdensServico.Produtos;
using SoatTechChallenge.Domain.OrdensServico.Servicos;
using SoatTechChallenge.Infrastucture.Database;
using SoatTechChallenge.Infrastucture.DomainEvents;
using SoatTechChallenge.Infrastucture.Persistence;
using Testcontainers.PostgreSql;
using Xunit;

namespace SoatTechChallenge.Tests.Servicos.Integration;

[Collection(nameof(IntegrationTestCollection))]
public class ServicoServiceIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("soattest_servicos")
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
        services.AddScoped<IDomainEventsDispatcher, NoopDomainEventsDispatcher>();
        services.AddDbContext<SoatTechChallengeDbContext>(o => o.UseNpgsql(_postgres.GetConnectionString()));
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ServicoService>();

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

    private ServicoService CreateService(IServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<ServicoService>();

    private async Task<ServicoResponse> SeedServicoAsync(
        string nome = "Serviço Seed",
        string descricao = "Desc",
        decimal valor = 100m)
    {
        using var scope = _provider.CreateScope();
        return await CreateService(scope)
            .Inserir(new InserirServicoRequest(nome, descricao, valor));
    }

    // OrdemServico compartilhada entre chamadas de SeedOSSAsync na mesma instância de teste.
    // Criada uma única vez para evitar duplicata de CPF/placa/FK.
    private Guid? _osCompartilhadaId;

    /// <summary>
    /// Cria (na primeira chamada) ou reutiliza (nas demais) uma OrdemServico pai,
    /// e insere uma OrdemServicoServico vinculada a ela.
    /// Evita violar FK e unique constraints ao chamar SeedOSSAsync múltiplas vezes.
    /// </summary>
    private async Task SeedOSSAsync(Guid idServico, DateTime inicio, DateTime fim)
    {
        if (_osCompartilhadaId is null)
            _osCompartilhadaId = await CriarOrdemServicoParenteAsync();

        using var scope = _provider.CreateScope();
        var ossRepo = scope.ServiceProvider.GetRequiredService<IRepository<OrdemServicoServico>>();

        var oss = new OrdemServicoServico(_osCompartilhadaId.Value, idServico, "Serviço", 100m);
        oss.IniciarExecucao();

        typeof(OrdemServicoServico)
            .GetProperty(nameof(OrdemServicoServico.DataInicioExecucao))!
            .SetValue(oss, inicio);

        oss.FinalizarExecucao();

        typeof(OrdemServicoServico)
            .GetProperty(nameof(OrdemServicoServico.DataFinalizacaoExecucao))!
            .SetValue(oss, fim);

        await ossRepo.InsertAsync(oss);
        await ossRepo.SaveChangesAsync();
    }

    /// <summary>
    /// Cria Cliente → Veiculo → OrdemServico com valores únicos (sem CPF/placa fixos)
    /// para não colidir entre testes que rodam no mesmo banco.
    /// </summary>
    private async Task<Guid> CriarOrdemServicoParenteAsync()
    {
        using var scope = _provider.CreateScope();

        var clienteRepo = scope.ServiceProvider.GetRequiredService<IRepository<Cliente>>();
        var veiculoRepo = scope.ServiceProvider.GetRequiredService<IRepository<Veiculo>>();
        var osRepo = scope.ServiceProvider.GetRequiredService<IRepository<OrdemServico>>();

        // 11 dígitos únicos — o validator de CPF não é chamado aqui (inserção direta)
        var docUnico = Guid.NewGuid().ToString("N")[..11];

        var cliente = new Cliente();
        cliente.Inserir("Cliente OSS", docUnico, TipoDocumentoCliente.Cpf);
        await clienteRepo.InsertAsync(cliente);
  
        // Placa Mercosul única: prefixo "OS" + 6 chars do Guid
        var placaUnica = ("OS" + Guid.NewGuid().ToString("N")[..5]).ToUpper();

        var veiculo = new Veiculo();
        veiculo.Inserir(cliente.Id, placaUnica, "Honda", "Civic", DateTime.Now.Year);
        await veiculoRepo.InsertAsync(veiculo);

        var os = new OrdemServico();
        os.Inserir(cliente.Id, veiculo.Id, new List<OrdemServicoServico>(), new List<OrdemServicoProduto>());
        
        await osRepo.InsertAsync(os);
        await osRepo.SaveChangesAsync();
        
        return os.Id;
    }

    // ────────────────────────────────────────────────────────────
    // Inserir
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Inserir_QuandoDadosValidos_PersistERetornaResponse()
    {
        using var scope = _provider.CreateScope();
        var result = await CreateService(scope)
            .Inserir(new InserirServicoRequest("Alinhamento", "Alinhamento 4 rodas", 180m));

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Alinhamento", result.Nome);
        Assert.Equal(180m, result.Valor);
    }

    [Theory]
    [InlineData("", "desc", 100)]  
    [InlineData("Nome", "desc", -1)]
    public async Task Inserir_QuandoDadosInvalidos_LancaDomainException(
        string nome, string descricao, decimal valor)
    {
        using var scope = _provider.CreateScope();
        await Assert.ThrowsAsync<DomainException>(() =>
            CreateService(scope).Inserir(new InserirServicoRequest(nome, descricao, valor)));
    }

    // ────────────────────────────────────────────────────────────
    // Buscar
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Buscar_QuandoExiste_RetornaDadosCorretos()
    {
        var criado = await SeedServicoAsync("Balanceamento", "Balanceamento completo", 120m);

        using var scope = _provider.CreateScope();
        var result = await CreateService(scope).Buscar(criado.Id);

        Assert.Equal(criado.Id, result.Id);
        Assert.Equal("Balanceamento", result.Nome);
        Assert.Equal(120m, result.Valor);
    }

    [Fact]
    public async Task Buscar_QuandoNaoExiste_LancaNotFoundException()
    {
        using var scope = _provider.CreateScope();
        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService(scope).Buscar(Guid.NewGuid()));
    }

    // ────────────────────────────────────────────────────────────
    // BuscarListaPaginada
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task BuscarListaPaginada_RetornaOrdenadoPorNomeEPaginado()
    {
        await SeedServicoAsync("Zebra");
        await SeedServicoAsync("Abacate");
        await SeedServicoAsync("Manga");

        using var scope = _provider.CreateScope();
        var result = await CreateService(scope)
            .BuscarListaPaginada(new PagedRequest(Pagina: 1, Tamanho: 2));

        Assert.Equal(3, result.Total);
        Assert.Equal(2, result.Itens.Count);
        Assert.Equal("Abacate", result.Itens[0].Nome);
        Assert.Equal("Manga", result.Itens[1].Nome);
    }

    [Fact]
    public async Task BuscarListaPaginada_SegundaPagina_RetornaRestantes()
    {
        await SeedServicoAsync("Abacate");
        await SeedServicoAsync("Manga");
        await SeedServicoAsync("Zebra");

        using var scope = _provider.CreateScope();
        var result = await CreateService(scope)
            .BuscarListaPaginada(new PagedRequest(Pagina: 2, Tamanho: 2));

        Assert.Equal(3, result.Total);
        Assert.Single(result.Itens);
        Assert.Equal("Zebra", result.Itens[0].Nome);
    }

    // ────────────────────────────────────────────────────────────
    // BuscarTempoMedioExecucao
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task BuscarTempoMedioExecucao_QuandoSemExecucoes_RetornaListaVazia()
    {
        using var scope = _provider.CreateScope();
        var result = await CreateService(scope).BuscarTempoMedioExecucao();
        Assert.Empty(result);
    }

    [Fact]
    public async Task BuscarTempoMedioExecucao_CalculaEstatisticasCorretamente()
    {
        var servico = await SeedServicoAsync("Troca de Óleo");
        var agora = DateTime.UtcNow;

        await SeedOSSAsync(servico.Id, agora, agora.AddMinutes(30));
        await SeedOSSAsync(servico.Id, agora, agora.AddMinutes(60));
        await SeedOSSAsync(servico.Id, agora, agora.AddMinutes(90));

        using var scope = _provider.CreateScope();
        var result = await CreateService(scope).BuscarTempoMedioExecucao();

        Assert.Single(result);
        var stats = result[0];
        Assert.Equal("Troca de Óleo", stats.Servico);
        Assert.Equal(60.0, stats.TempoMedioMinutos, precision: 0);
        Assert.Equal(30.0, stats.TempoMinimoMinutos, precision: 0);
        Assert.Equal(90.0, stats.TempoMaximoMinutos, precision: 0);
    }

    [Fact]
    public async Task BuscarTempoMedioExecucao_AgrupaPorServico_ComDoisServicos()
    {
        var s1 = await SeedServicoAsync("Alinhamento");
        var s2 = await SeedServicoAsync("Balanceamento");
        var agora = DateTime.UtcNow;

        await SeedOSSAsync(s1.Id, agora, agora.AddMinutes(20));
        await SeedOSSAsync(s1.Id, agora, agora.AddMinutes(40));
        await SeedOSSAsync(s2.Id, agora, agora.AddMinutes(10));

        using var scope = _provider.CreateScope();
        var result = await CreateService(scope).BuscarTempoMedioExecucao();

        Assert.Equal(2, result.Count);

        var statsS1 = result.First(r => r.Servico == "Alinhamento");
        Assert.Equal(30.0, statsS1.TempoMedioMinutos, precision: 0);
        Assert.Equal(20.0, statsS1.TempoMinimoMinutos, precision: 0);
        Assert.Equal(40.0, statsS1.TempoMaximoMinutos, precision: 0);

        var statsS2 = result.First(r => r.Servico == "Balanceamento");
        Assert.Equal(10.0, statsS2.TempoMedioMinutos, precision: 0);
    }

    [Fact]
    public async Task BuscarTempoMedioExecucao_IgnoraOSSAguardandoExecucao()
    {
        var servico = await SeedServicoAsync("Suspensão");

        // Seed uma OrdemServico pai e uma OSS sem datas de execução
        using var seedScope = _provider.CreateScope();

        var clienteRepo = seedScope.ServiceProvider.GetRequiredService<IRepository<Cliente>>();
        var veiculoRepo = seedScope.ServiceProvider.GetRequiredService<IRepository<Veiculo>>();
        var osRepo = seedScope.ServiceProvider.GetRequiredService<IRepository<OrdemServico>>();
        var ossRepo = seedScope.ServiceProvider.GetRequiredService<IRepository<OrdemServicoServico>>();

        var cliente = new Cliente();
        cliente.Inserir("Cliente Sem Datas", "01290124180", TipoDocumentoCliente.Cpf);
        await clienteRepo.InsertAsync(cliente);
        await clienteRepo.SaveChangesAsync();
        
        var veiculo = new Veiculo();
        veiculo.Inserir(cliente.Id, "ZZZ9999", "Ford", "Ka", DateTime.Now.Year);
        await veiculoRepo.InsertAsync(veiculo);
        await veiculoRepo.SaveChangesAsync();
        
        var os = new OrdemServico();
        os.Inserir(cliente.Id, veiculo.Id, new List<OrdemServicoServico>(), new List<OrdemServicoProduto>());
        await osRepo.InsertAsync(os);
        await osRepo.SaveChangesAsync();
        
        var ossSemDatas = new OrdemServicoServico(os.Id, servico.Id, "Suspensão", 100m);
        await ossRepo.InsertAsync(ossSemDatas);

        using var scope = _provider.CreateScope();
        var result = await CreateService(scope).BuscarTempoMedioExecucao();

        Assert.Empty(result);
    }

    // ────────────────────────────────────────────────────────────
    // Atualizar
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Atualizar_QuandoExiste_PersistMudancasNoBanco()
    {
        var criado = await SeedServicoAsync("Nome Antigo", valor: 100m);

        using var scope = _provider.CreateScope();
        await CreateService(scope)
            .Atualizar(criado.Id, new AtualizarServicoRequest("Nome Novo", "Desc Nova", 250m));

        using var verifyScope = _provider.CreateScope();
        var buscado = await CreateService(verifyScope).Buscar(criado.Id);
        Assert.Equal("Nome Novo", buscado.Nome);
        Assert.Equal(250m, buscado.Valor);
    }

    [Fact]
    public async Task Atualizar_QuandoNaoExiste_LancaNotFoundException()
    {
        using var scope = _provider.CreateScope();
        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService(scope).Atualizar(
                Guid.NewGuid(),
                new AtualizarServicoRequest("Nome", "desc", 100m)));
    }

    // ────────────────────────────────────────────────────────────
    // Remover
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Remover_QuandoExiste_ExcluiDoBanco()
    {
        var criado = await SeedServicoAsync();

        using (var scope = _provider.CreateScope())
            await CreateService(scope).Remover(criado.Id);

        using var verifyScope = _provider.CreateScope();
        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService(verifyScope).Buscar(criado.Id));
    }

    [Fact]
    public async Task Remover_QuandoNaoExiste_NaoLancaExcecao()
    {
        using var scope = _provider.CreateScope();
        var ex = await Record.ExceptionAsync(() =>
            CreateService(scope).Remover(Guid.NewGuid()));
        Assert.Null(ex);
    }
}