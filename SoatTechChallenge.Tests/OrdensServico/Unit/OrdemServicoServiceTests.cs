using MockQueryable.Moq;
using Moq;
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
using SoatTechChallenge.Domain.OrdensServico.Produtos;
using SoatTechChallenge.Domain.OrdensServico.Servicos;
using SoatTechChallenge.Domain.OrdensServico.Servicos.Enums;
using SoatTechChallenge.Domain.Produtos;
using SoatTechChallenge.Domain.Servicos;
using Xunit;

namespace SoatTechChallenge.Tests.OrdensServico.Unit;

public class OrdemServicoServiceTests
{
    private readonly Mock<IRepository<OrdemServico>> _osRepoMock = new();
    private readonly Mock<IOrdemServicoValidatorService> _validatorMock = new();
    private readonly Mock<IRepository<Cliente>> _clienteRepoMock = new();
    private readonly Mock<IRepository<Veiculo>> _veiculoRepoMock = new();
    private readonly Mock<IRepository<Produto>> _produtoRepoMock = new();
    private readonly Mock<IRepository<Servico>> _servicoRepoMock = new();
   
    private readonly OrdemServicoService _sut;

    public OrdemServicoServiceTests()
    {
        _sut = new OrdemServicoService(
            _osRepoMock.Object,
            _validatorMock.Object,
            _clienteRepoMock.Object,
            _veiculoRepoMock.Object,
            _produtoRepoMock.Object,
            _servicoRepoMock.Object);
    }

    // ────────────────────────────────────────────────────────────
    // Buscar
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Buscar_QuandoNaoExiste_RetornaNull()
    {
        SetupOrdemServico();
        SetupClientes();
        SetupVeiculos();

        var result = await _sut.Buscar(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task Buscar_QuandoExiste_RetornaResponseMapeado()
    {
        var (os, cliente, veiculo) = CriarOrdemServicoComClienteEVeiculo();
        SetupOrdemServico(os);
        SetupClientes(cliente);
        SetupVeiculos(veiculo);

        var result = await _sut.Buscar(os.Id);

        Assert.NotNull(result);
        Assert.Equal(os.Id, result!.Id);
        Assert.Equal(cliente.Nome, result.Cliente.Nome);
        Assert.Equal(veiculo.Placa, result.Veiculo.Placa);
    }

    // ────────────────────────────────────────────────────────────
    // BuscarListaPaginada
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task BuscarListaPaginada_QuandoSemResultados_RetornaListaVazia()
    {
        SetupOrdemServico();
        SetupClientes();
        SetupVeiculos();

        var result = await _sut.BuscarListaPaginada(new PagedRequest(1, 10));

        Assert.Empty(result.Itens);
        Assert.Equal(0, result.Total);
    }

    [Fact]
    public async Task BuscarListaPaginada_AplicaPaginacaoERetornaTotalCorreto()
    {
        var triplas = Enumerable.Range(0, 5)
            .Select(_ => CriarOrdemServicoComClienteEVeiculo())
            .ToList();

        SetupOrdemServico(triplas.Select(t => t.os).ToArray());
        SetupClientes(triplas.Select(t => t.cliente).ToArray());
        SetupVeiculos(triplas.Select(t => t.veiculo).ToArray());

        var result = await _sut.BuscarListaPaginada(new PagedRequest(1, 3));

        Assert.Equal(5, result.Total);
        Assert.Equal(3, result.Itens.Count);
    }

    // ────────────────────────────────────────────────────────────
    // BuscarListaPaginadaPorDocumento
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task BuscarListaPaginadaPorDocumento_LimpaDocumentoEFiltroCorretamente()
    {
        var (os, cliente, veiculo) = CriarOrdemServicoComClienteEVeiculo(documento: "12345678900");

        SetupOrdemServico(os);
        SetupClientes(cliente);
        SetupVeiculos(veiculo);

        // Passa com máscara; deve limpar e encontrar
        var result = await _sut.BuscarListaPaginadaPorDocumento("123.456.789-00", new PagedRequest(1, 10));

        Assert.Equal(1, result.Total);
        Assert.Equal(cliente.Nome, result.Itens[0].Cliente.Nome);
    }

    [Fact]
    public async Task BuscarListaPaginadaPorDocumento_QuandoDocumentoNaoExiste_RetornaVazio()
    {
        SetupOrdemServico();
        SetupClientes();
        SetupVeiculos();

        var result = await _sut.BuscarListaPaginadaPorDocumento("99999999999", new PagedRequest(1, 10));

        Assert.Empty(result.Itens);
    }

    // ────────────────────────────────────────────────────────────
    // Inserir
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Inserir_ChamaValidatorEInsertAsync()
    {
        var servico = CriarServico();
        SetupServicos(servico);

        var request = new InserirOrdemServicoRequest(Guid.NewGuid(), Guid.NewGuid(), new List<Guid> { servico.Id });

        await _sut.Inserir(request);

        _validatorMock.Verify(v => v.Validar(request), Times.Once);
        _osRepoMock.Verify(r => r.InsertAsync(It.IsAny<OrdemServico>()), Times.Once);
    }

    [Fact]
    public async Task Inserir_QuandoValidatorLanca_NaoInsere()
    {
        _validatorMock
            .Setup(v => v.Validar(It.IsAny<InserirOrdemServicoRequest>()))
            .ThrowsAsync(new DomainException("Inválido"));

        var request = new InserirOrdemServicoRequest(Guid.NewGuid(), Guid.NewGuid(), new List<Guid>());

        await Assert.ThrowsAsync<DomainException>(() => _sut.Inserir(request));
        _osRepoMock.Verify(r => r.InsertAsync(It.IsAny<OrdemServico>()), Times.Never);
    }

    [Fact]
    public async Task Inserir_SemServicos_InsereOSVazia()
    {
        SetupServicos();
        var request = new InserirOrdemServicoRequest(Guid.NewGuid(), Guid.NewGuid(), new List<Guid>());

        await _sut.Inserir(request);

        _osRepoMock.Verify(r => r.InsertAsync(It.Is<OrdemServico>(os => !os.Servicos.Any())), Times.Once);
    }

    // ────────────────────────────────────────────────────────────
    // InserirProdutos
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task InserirProdutos_QuandoOSNaoExiste_LancaNotFoundException()
    {
        SetupOrdemServico();
        var request = new InserirProdutosOrdemServicoRequest(new List<InserirProdutosOrdemServicoProdutoRequest>());

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.InserirProdutos(Guid.NewGuid(), request));
    }

    [Fact]
    public async Task InserirProdutos_QuandoOSEmDiagnostico_AdicionaProdutosEChamaUpdate()
    {
        var os = CriarOrdemServicoEmDiagnostico();
        var produto = CriarProduto();
        SetupOrdemServico(os);
        SetupProdutos(produto);

        var request = new InserirProdutosOrdemServicoRequest(new List<InserirProdutosOrdemServicoProdutoRequest>
        {
            new(produto.Id, 2m)
        });

        await _sut.InserirProdutos(os.Id, request);

        _osRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // ────────────────────────────────────────────────────────────
    // InserirServicos
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task InserirServicos_QuandoOSNaoExiste_LancaNotFoundException()
    {
        SetupOrdemServico();
        var request = new InserirServicosOrdemServicoRequest(new List<InserirServicosOrdemServicoServicoRequest>());

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.InserirServicos(Guid.NewGuid(), request));
    }

    [Fact]
    public async Task InserirServicos_QuandoOSEmDiagnostico_AdicionaServicosEChamaUpdate()
    {
        var os = CriarOrdemServicoEmDiagnostico();
        var servico = CriarServico();
        SetupOrdemServico(os);
        SetupServicos(servico);

        var request = new InserirServicosOrdemServicoRequest(new List<InserirServicosOrdemServicoServicoRequest>
        {
            new(servico.Id)
        });

        await _sut.InserirServicos(os.Id, request);

        _osRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // ────────────────────────────────────────────────────────────
    // IniciarDiagnostico
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task IniciarDiagnostico_QuandoOSNaoExiste_LancaNotFoundException()
    {
        SetupOrdemServico();
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.IniciarDiagnostico(Guid.NewGuid()));
    }

    [Fact]
    public async Task IniciarDiagnostico_QuandoRecebida_MudaStatusEChamaUpdate()
    {
        var os = CriarOrdemServicoRecebida();
        SetupOrdemServico(os);

        await _sut.IniciarDiagnostico(os.Id);

        Assert.Equal(StatusOrdemServico.EmDiagnostico, os.Status);
        _osRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // ────────────────────────────────────────────────────────────
    // FinalizarDiagnostico
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task FinalizarDiagnostico_QuandoOSNaoExiste_LancaNotFoundException()
    {
        SetupOrdemServico();
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.FinalizarDiagnostico(Guid.NewGuid()));
    }

    [Fact]
    public async Task FinalizarDiagnostico_QuandoEmDiagnosticoComServicos_EnviaOrcamentoEChamaUpdate()
    {
        var os = CriarOrdemServicoEmDiagnostico();
        SetupOrdemServico(os);

        await _sut.FinalizarDiagnostico(os.Id);

        Assert.Equal(StatusOrdemServico.AguardandoAprovacao, os.Status);
        _osRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // ────────────────────────────────────────────────────────────
    // AprovarOrcamento
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task AprovarOrcamento_QuandoOSNaoExiste_LancaNotFoundException()
    {
        SetupOrdemServico();
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.AprovarOrcamento(Guid.NewGuid()));
    }

    [Fact]
    public async Task AprovarOrcamento_QuandoAguardandoAprovacao_MudaParaEmExecucaoEChamaUpdate()
    {
        var os = CriarOrdemServicoAguardandoAprovacao();
        SetupOrdemServico(os);

        await _sut.AprovarOrcamento(os.Id);

        Assert.Equal(StatusOrdemServico.EmExecucao, os.Status);
        _osRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // ────────────────────────────────────────────────────────────
    // IniciarExecucaoServico
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task IniciarExecucaoServico_QuandoOSNaoExiste_LancaNotFoundException()
    {
        SetupOrdemServico();
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.IniciarExecucaoServico(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task IniciarExecucaoServico_QuandoEmExecucao_IniciaServicoEChamaUpdate()
    {
        var servico = new OrdemServicoServico(Guid.NewGuid(), Guid.NewGuid(), "Serviço", 100m);
        var os = CriarOrdemServicoEmExecucao(servico);
        SetupOrdemServico(os);

        await _sut.IniciarExecucaoServico(os.Id, servico.Id);

        Assert.Equal(StatusOrdemServicoServico.EmExecucao, servico.Status);
        _osRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // ────────────────────────────────────────────────────────────
    // FinalizarExecucaoServico
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task FinalizarExecucaoServico_QuandoOSNaoExiste_LancaNotFoundException()
    {
        SetupOrdemServico();
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.FinalizarExecucaoServico(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task FinalizarExecucaoServico_QuandoUltimoServico_FinalizaOSEDecrementaEstoque()
    {
        var produto = CriarProduto(estoque: 10m);
        var servicoOs = new OrdemServicoServico(Guid.NewGuid(), Guid.NewGuid(), "Serviço", 100m);
        var os = CriarOrdemServicoEmExecucao(servicoOs);
        os.Servicos.Remove(os.Servicos.First());

        // Adiciona produto à OS via reflexão para simular estado persistido
        var produtoOs = new OrdemServicoProduto(os.Id, produto.Id, produto.Nome, produto.Valor, 2m);
        os.Produtos.Add(produtoOs);

        servicoOs.IniciarExecucao();
        SetupOrdemServico(os);

        await _sut.FinalizarExecucaoServico(os.Id, servicoOs.Id);

        Assert.Equal(StatusOrdemServico.Finalizada, os.Status);
        Assert.Single(os.DomainEvents);
        _osRepoMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task FinalizarExecucaoServico_QuandoAindaHaServicoPendente_NaoDecrementaEstoque()
    {
        var s1 = new OrdemServicoServico(Guid.NewGuid(), Guid.NewGuid(), "S1", 100m);
        var s2 = new OrdemServicoServico(Guid.NewGuid(), Guid.NewGuid(), "S2", 100m);
        var os = CriarOrdemServicoEmExecucao(s1, s2);

        s1.IniciarExecucao();
        SetupOrdemServico(os);
        SetupProdutos();

        await _sut.FinalizarExecucaoServico(os.Id, s1.Id);

        Assert.Equal(StatusOrdemServico.EmExecucao, os.Status);
        _produtoRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Produto>()), Times.Never);
    }

    // ────────────────────────────────────────────────────────────
    // Entregar
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Entregar_QuandoOSNaoExiste_LancaNotFoundException()
    {
        SetupOrdemServico();
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.Entregar(Guid.NewGuid()));
    }

    [Fact]
    public async Task Entregar_QuandoFinalizada_MudaParaEntregueEChamaUpdate()
    {
        var os = CriarOrdemServicoFinalizada();
        SetupOrdemServico(os);

        await _sut.Entregar(os.Id);

        Assert.Equal(StatusOrdemServico.Entregue, os.Status);
        _osRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // ────────────────────────────────────────────────────────────
    // Remover
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Remover_QuandoOSNaoExiste_LancaNotFoundException()
    {
        SetupOrdemServico();
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.Remover(Guid.NewGuid()));
    }

    [Fact]
    public async Task Remover_QuandoExiste_ChamaDeleteAsync()
    {
        var os = CriarOrdemServicoRecebida();
        SetupOrdemServico(os);

        await _sut.Remover(os.Id);

        _osRepoMock.Verify(r => r.DeleteAsync(os), Times.Once);
    }

    // ────────────────────────────────────────────────────────────
    // RemoverProduto
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task RemoverProduto_QuandoOSNaoExiste_LancaNotFoundException()
    {
        SetupOrdemServico();
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.RemoverProduto(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task RemoverProduto_QuandoEmDiagnosticoEProdutoVinculado_RemoveEChamaUpdate()
    {
        var os = CriarOrdemServicoEmDiagnostico();
        var produtoOs = new OrdemServicoProduto(os.Id, Guid.NewGuid(), "Produto", 50m, 1m);
        os.Produtos.Add(produtoOs);
        SetupOrdemServico(os);

        await _sut.RemoverProduto(os.Id, produtoOs.Id);

        Assert.Empty(os.Produtos);
        _osRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // ────────────────────────────────────────────────────────────
    // RemoverServico
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task RemoverServico_QuandoOSNaoExiste_LancaNotFoundException()
    {
        SetupOrdemServico();
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.RemoverServico(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task RemoverServico_QuandoEmDiagnosticoEServicoVinculado_RemoveEChamaUpdate()
    {
        var os = CriarOrdemServicoEmDiagnostico();
        var servicoOS = new OrdemServicoServico(os.Id, Guid.NewGuid(), "Serviço Extra", 200m);
        os.Servicos.Add(servicoOS);
        SetupOrdemServico(os);

        await _sut.RemoverServico(os.Id, servicoOS.Id);

        // Deve restar apenas o serviço base inserido pelo factory
        Assert.Single(os.Servicos);
        _osRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // ────────────────────────────────────────────────────────────
    // Setup helpers
    // ────────────────────────────────────────────────────────────

    private void SetupOrdemServico(params OrdemServico[] lista)
    {
        _osRepoMock.Setup(r => r.GetQueryable()).Returns(lista.ToList().AsQueryable().BuildMock());
    }

    private void SetupClientes(params Cliente[] lista)
    {
        _clienteRepoMock.Setup(r => r.GetQueryable()).Returns(lista.ToList().AsQueryable().BuildMock());
    }

    private void SetupVeiculos(params Veiculo[] lista)
    {
        _veiculoRepoMock.Setup(r => r.GetQueryable()).Returns(lista.ToList().AsQueryable().BuildMock());
    }

    private void SetupProdutos(params Produto[] lista)
    {
        _produtoRepoMock.Setup(r => r.GetQueryable()).Returns(lista.ToList().AsQueryable().BuildMock());
    }

    private void SetupServicos(params Servico[] lista)
    {
        _servicoRepoMock.Setup(r => r.GetQueryable()).Returns(lista.ToList().AsQueryable().BuildMock());
    }

    // ────────────────────────────────────────────────────────────
    // Domain factories
    // ────────────────────────────────────────────────────────────

    private static Servico CriarServico(decimal valor = 100m)
    {
        var s = new Servico();
        s.Inserir("Serviço Teste", "Descrição Teste", valor);
        return s;
    }

    private static Produto CriarProduto(decimal valor = 50m, decimal estoque = 10m)
    {
        var p = new Produto();
        p.Inserir("Produto Teste", "Desc", valor, estoque);
        return p;
    }

    private static Cliente CriarCliente(string documento = "12345678900")
    {
        var c = new Cliente();
        c.Inserir("Cliente Teste", documento, TipoDocumentoCliente.Cpf);
        return c;
    }

    private static Veiculo CriarVeiculo(Guid? idCliente = null)
    {
        var v = new Veiculo();
        v.Inserir(idCliente ?? Guid.NewGuid(), "ABC1D23", "Honda", "Civic", DateTime.Now.Year);
        return v;
    }

    private static (OrdemServico os, Cliente cliente, Veiculo veiculo) CriarOrdemServicoComClienteEVeiculo(string documento = "12345678900")
    {
        var cliente = CriarCliente(documento);
        var veiculo = CriarVeiculo(cliente.Id);
        cliente.Veiculos.Add(veiculo);

        var servicoOS = new OrdemServicoServico(Guid.NewGuid(), Guid.NewGuid(), "Serviço", 100m);
        var os = new OrdemServico();
        os.Inserir(cliente.Id, veiculo.Id, new List<OrdemServicoServico> { servicoOS });

        return (os, cliente, veiculo);
    }

    private static OrdemServico CriarOrdemServicoRecebida()
    {
        var os = new OrdemServico();
        os.Inserir(Guid.NewGuid(), Guid.NewGuid(),
            new List<OrdemServicoServico> { new(Guid.NewGuid(), Guid.NewGuid(), "Serviço", 100m) });
        return os;
    }

    private static OrdemServico CriarOrdemServicoEmDiagnostico()
    {
        var os = CriarOrdemServicoRecebida();
        os.IniciarDiagnostico();
        return os;
    }

    private static OrdemServico CriarOrdemServicoAguardandoAprovacao()
    {
        var os = CriarOrdemServicoEmDiagnostico();
        os.FinalizarDiagnostico();
        return os;
    }

    private static OrdemServico CriarOrdemServicoEmExecucao(params OrdemServicoServico[] extras)
    {
        var servicoBase = new OrdemServicoServico(Guid.NewGuid(), Guid.NewGuid(), "Base", 100m);
        var todos = new List<OrdemServicoServico> { servicoBase };
        todos.AddRange(extras);

        var os = new OrdemServico();
        os.Inserir(Guid.NewGuid(), Guid.NewGuid(), todos);
        os.IniciarDiagnostico();
        os.FinalizarDiagnostico();
        os.AprovarOrcamento();
        return os;
    }

    private static OrdemServico CriarOrdemServicoFinalizada()
    {
        var servico = new OrdemServicoServico(Guid.NewGuid(), Guid.NewGuid(), "Serviço", 100m);
        var os = new OrdemServico();
        os.Inserir(Guid.NewGuid(), Guid.NewGuid(), new List<OrdemServicoServico> { servico });
        os.IniciarDiagnostico();
        os.FinalizarDiagnostico();
        os.AprovarOrcamento();
        servico.IniciarExecucao();
        os.FinalizarExecucaoServico(servico.Id);
        return os;
    }
}