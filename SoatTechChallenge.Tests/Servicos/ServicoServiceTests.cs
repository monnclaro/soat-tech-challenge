using MockQueryable.Moq;
using Moq;
using SoatTechChallenge.Application.Common.DTOs;
using SoatTechChallenge.Application.Servicos.DTOs.Requests;
using SoatTechChallenge.Application.Servicos.Services;
using SoatTechChallenge.Domain.Common.Exceptions;
using SoatTechChallenge.Domain.Common.Interfaces;
using SoatTechChallenge.Domain.OrdensServico.Servicos;
using SoatTechChallenge.Domain.Servicos;
using Xunit;

namespace SoatTechChallenge.Tests.Servicos;

public class ServicoServiceTests
{
    private readonly Mock<IRepository<Servico>> _repoMock = new();
    private readonly Mock<IRepository<OrdemServicoServico>> _ossRepoMock = new();
    private readonly ServicoService _sut;

    public ServicoServiceTests()
    {
        _sut = new ServicoService(_repoMock.Object, _ossRepoMock.Object);
    }

    // ────────────────────────────────────────────────────────────
    // Buscar
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Buscar_QuandoNaoExiste_LancaNotFoundException()
    {
        SetupServicos();
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.Buscar(Guid.NewGuid()));
    }

    [Fact]
    public async Task Buscar_QuandoExiste_RetornaResponseMapeadoCorretamente()
    {
        var servico = CriarServico("Alinhamento", 150m);
        SetupServicos(servico);

        var result = await _sut.Buscar(servico.Id);

        Assert.Equal(servico.Id, result.Id);
        Assert.Equal(servico.Nome, result.Nome);
        Assert.Equal(servico.Descricao, result.Descricao);
        Assert.Equal(servico.Valor, result.Valor);
    }

    // ────────────────────────────────────────────────────────────
    // BuscarListaPaginada
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task BuscarListaPaginada_QuandoSemServicos_RetornaListaVazia()
    {
        SetupServicos();
        var result = await _sut.BuscarListaPaginada(new PagedRequest(1, 10));

        Assert.Empty(result.Itens);
        Assert.Equal(0, result.Total);
    }

    [Fact]
    public async Task BuscarListaPaginada_RetornaTotalEItemsCorretos()
    {
        var servicos = new[]
        {
            CriarServico("Zebra", 100m),
            CriarServico("Abacate", 200m),
            CriarServico("Manga", 300m),
        };
        SetupServicos(servicos);

        var result = await _sut.BuscarListaPaginada(new PagedRequest(1, 10));

        Assert.Equal(3, result.Total);
        Assert.Equal(3, result.Itens.Count);
    }

    [Fact]
    public async Task BuscarListaPaginada_RetornaOrdenadoPorNome()
    {
        var servicos = new[]
        {
            CriarServico("Zebra", 100m),
            CriarServico("Abacate", 200m),
            CriarServico("Manga", 300m),
        };
        SetupServicos(servicos);

        var result = await _sut.BuscarListaPaginada(new PagedRequest(1, 10));

        var nomes = result.Itens.Select(s => s.Nome).ToList();
        Assert.Equal(new[] { "Abacate", "Manga", "Zebra" }, nomes);
    }

    [Fact]
    public async Task BuscarListaPaginada_AplicaPaginacaoCorretamente()
    {
        var servicos = Enumerable.Range(1, 6)
            .Select(i => CriarServico($"Serviço {i:D2}", i * 10m))
            .ToArray();
        SetupServicos(servicos);

        var result = await _sut.BuscarListaPaginada(new PagedRequest(Pagina: 2, Tamanho: 2));

        Assert.Equal(6, result.Total);
        Assert.Equal(2, result.Itens.Count);
    }

    // ────────────────────────────────────────────────────────────
    // BuscarTempoMedioExecucao
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task BuscarTempoMedioExecucao_QuandoSemExecucoes_RetornaListaVazia()
    {
        SetupServicos();
        SetupOSS();

        var result = await _sut.BuscarTempoMedioExecucao();

        Assert.Empty(result);
    }

    [Fact]
    public async Task BuscarTempoMedioExecucao_IgnoraExecucoesSemDataInicio()
    {
        var servico = CriarServico("Alinhamento", 100m);
        // OSS sem data de início/fim — deve ser ignorada
        var oss = CriarOSSSemDatas(servico.Id);
        SetupServicos(servico);
        SetupOSS(oss);

        var result = await _sut.BuscarTempoMedioExecucao();

        Assert.Empty(result);
    }

    [Fact]
    public async Task BuscarTempoMedioExecucao_CalculaTemposMedioMinMaxCorretamente()
    {
        var servico = CriarServico("Alinhamento", 100m);
        var agora = DateTime.UtcNow;

        var oss1 = CriarOSSComDatas(servico.Id, agora, agora.AddMinutes(30));  // 30 min
        var oss2 = CriarOSSComDatas(servico.Id, agora, agora.AddMinutes(60));  // 60 min
        var oss3 = CriarOSSComDatas(servico.Id, agora, agora.AddMinutes(90));  // 90 min

        SetupServicos(servico);
        SetupOSS(oss1, oss2, oss3);

        var result = await _sut.BuscarTempoMedioExecucao();

        Assert.Single(result);
        var stats = result[0];
        Assert.Equal("Alinhamento", stats.Servico);
        Assert.Equal(60.0, stats.TempoMedioMinutos, precision: 1);
        Assert.Equal(30.0, stats.TempoMinimoMinutos, precision: 1);
        Assert.Equal(90.0, stats.TempoMaximoMinutos, precision: 1);
    }

    [Fact]
    public async Task BuscarTempoMedioExecucao_AgrupaPorServico()
    {
        var s1 = CriarServico("Alinhamento", 100m);
        var s2 = CriarServico("Balanceamento", 150m);
        var agora = DateTime.UtcNow;

        SetupServicos(s1, s2);
        SetupOSS(
            CriarOSSComDatas(s1.Id, agora, agora.AddMinutes(20)),
            CriarOSSComDatas(s2.Id, agora, agora.AddMinutes(40))
        );

        var result = await _sut.BuscarTempoMedioExecucao();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Servico == "Alinhamento");
        Assert.Contains(result, r => r.Servico == "Balanceamento");
    }

    // ────────────────────────────────────────────────────────────
    // Inserir
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Inserir_QuandoDadosValidos_ChamaInsertAsyncERetornaResponse()
    {
        var request = new InserirServicoRequest("Troca de Óleo", "Troca completa", 250m);

        var result = await _sut.Inserir(request);

        _repoMock.Verify(r => r.InsertAsync(It.IsAny<Servico>()), Times.Once);
        Assert.Equal("Troca de Óleo", result.Nome);
        Assert.Equal(250m, result.Valor);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task Inserir_QuandoNomeInvalido_LancaDomainException()
    {
        var request = new InserirServicoRequest("", "desc", 100m);

        await Assert.ThrowsAsync<DomainException>(() => _sut.Inserir(request));
        _repoMock.Verify(r => r.InsertAsync(It.IsAny<Servico>()), Times.Never);
    }

    [Fact]
    public async Task Inserir_QuandoValorInvalido_LancaDomainException()
    {
        var request = new InserirServicoRequest("Nome", "desc", 0m);

        await Assert.ThrowsAsync<DomainException>(() => _sut.Inserir(request));
        _repoMock.Verify(r => r.InsertAsync(It.IsAny<Servico>()), Times.Never);
    }

    // ────────────────────────────────────────────────────────────
    // Atualizar
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Atualizar_QuandoNaoExiste_LancaNotFoundException()
    {
        SetupServicos();
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.Atualizar(Guid.NewGuid(), new AtualizarServicoRequest("Nome", "desc", 100m)));
    }

    [Fact]
    public async Task Atualizar_QuandoExiste_AtualizaDadosEChamaUpdateAsync()
    {
        var servico = CriarServico("Antigo", 100m);
        SetupServicos(servico);

        var result = await _sut.Atualizar(servico.Id, new AtualizarServicoRequest("Novo", "Nova desc", 300m));

        Assert.Equal("Novo", result.Nome);
        Assert.Equal("Nova desc", result.Descricao);
        Assert.Equal(300m, result.Valor);
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Servico>()), Times.Once);
    }

    [Fact]
    public async Task Atualizar_QuandoValorInvalido_LancaDomainExceptionENaoChamaUpdate()
    {
        var servico = CriarServico();
        SetupServicos(servico);

        await Assert.ThrowsAsync<DomainException>(() =>
            _sut.Atualizar(servico.Id, new AtualizarServicoRequest("Nome", "desc", -1m)));

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Servico>()), Times.Never);
    }

    // ────────────────────────────────────────────────────────────
    // Remover
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Remover_QuandoNaoExiste_NaoLancaExcecaoENaoChamaDelete()
    {
        SetupServicos();
        var ex = await Record.ExceptionAsync(() => _sut.Remover(Guid.NewGuid()));

        Assert.Null(ex);
        _repoMock.Verify(r => r.DeleteAsync(It.IsAny<Servico>()), Times.Never);
    }

    [Fact]
    public async Task Remover_QuandoExiste_ChamaDeleteAsyncComIdCorreto()
    {
        var servico = CriarServico();
        SetupServicos(servico);

        await _sut.Remover(servico.Id);

        _repoMock.Verify(r => r.DeleteAsync(servico), Times.Once);
    }

    // ────────────────────────────────────────────────────────────
    // Helpers
    // ────────────────────────────────────────────────────────────

    private void SetupServicos(params Servico[] lista)
    {
        _repoMock.Setup(r => r.GetQueryable())
            .Returns(lista.ToList().AsQueryable().BuildMock());
    }

    private void SetupOSS(params OrdemServicoServico[] lista)
    {
        _ossRepoMock.Setup(r => r.GetQueryable())
            .Returns(lista.ToList().AsQueryable().BuildMock());
    }

    private static Servico CriarServico(string nome = "Serviço Teste", decimal valor = 100m)
    {
        var s = new Servico();
        s.Inserir(nome, "Descrição", valor);
        return s;
    }

    private static OrdemServicoServico CriarOSSComDatas(
        Guid idServico, DateTime inicio, DateTime fim)
    {
        var oss = new OrdemServicoServico(Guid.NewGuid(), idServico, "Serviço", 100m);
        oss.IniciarExecucao();

        // Força datas específicas via reflexão para controlar o tempo calculado
        typeof(OrdemServicoServico)
            .GetProperty(nameof(OrdemServicoServico.DataInicioExecucao))!
            .SetValue(oss, inicio);

        oss.FinalizarExecucao();

        typeof(OrdemServicoServico)
            .GetProperty(nameof(OrdemServicoServico.DataFinalizacaoExecucao))!
            .SetValue(oss, fim);

        return oss;
    }

    private static OrdemServicoServico CriarOSSSemDatas(Guid idServico) =>
        new(Guid.NewGuid(), idServico, "Serviço", 100m);
}