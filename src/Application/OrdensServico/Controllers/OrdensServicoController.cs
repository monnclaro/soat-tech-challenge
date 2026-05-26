using Application.OrdensServico.UseCases.AprovarOrcamento;
using Application.OrdensServico.UseCases.AtualizarStatus;
using Application.OrdensServico.UseCases.AtualizarStatus.DTOs;
using Application.OrdensServico.UseCases.BuscarListaPaginada;
using Application.OrdensServico.UseCases.BuscarListaPaginadaPorDocumento;
using Application.OrdensServico.UseCases.BuscarOrdemServico;
using Application.OrdensServico.UseCases.BuscarStatus;
using Application.OrdensServico.UseCases.Entregar;
using Application.OrdensServico.UseCases.FinalizarDiagnostico;
using Application.OrdensServico.UseCases.FinalizarExecucaoServico;
using Application.OrdensServico.UseCases.IniciarDiagnostico;
using Application.OrdensServico.UseCases.IniciarExecucaoServico;
using Application.OrdensServico.UseCases.Inserir;
using Application.OrdensServico.UseCases.InserirCompleta;
using Application.OrdensServico.UseCases.InserirProdutos;
using Application.OrdensServico.UseCases.InserirServicos;
using Application.OrdensServico.UseCases.Remover;
using Application.OrdensServico.UseCases.RemoverProduto;
using Application.OrdensServico.UseCases.RemoverServico;
using Application.OrdensServico.UseCases.ReprovarOrcamento;
using SharedKernel.Interfaces;

namespace Application.OrdensServico.Controllers;

public class OrdemServicoController : IScoped
{
    private readonly BuscarOrdemServicoUseCase _buscar;
    private readonly BuscarStatusUseCase _buscarStatus;
    private readonly BuscarListaPaginadaOrdemServicoUseCase _listar;
    private readonly BuscarListaPaginadaPorDocumentoUseCase _listarPorDocumento;
    private readonly InserirOrdemServicoUseCase _inserir;
    private readonly InserirProdutosUseCase _inserirProdutos;
    private readonly InserirServicosUseCase _inserirServicos;
    private readonly IniciarDiagnosticoUseCase _iniciarDiagnostico;
    private readonly FinalizarDiagnosticoUseCase _finalizarDiagnostico;
    private readonly AprovarOrcamentoUseCase _aprovarOrcamento;
    private readonly ReprovarOrcamentoUseCase _reprovarOrcamento;
    private readonly IniciarExecucaoServicoUseCase _iniciarExecucao;
    private readonly FinalizarExecucaoServicoUseCase _finalizarExecucao;
    private readonly EntregarUseCase _entregar;
    private readonly RemoverOrdemServicoUseCase _remover;
    private readonly RemoverProdutoUseCase _removerProduto;
    private readonly RemoverServicoUseCase _removerServico;
    private readonly AtualizarStatusUseCase _atualizarStatusUseCase;
    private readonly InserirOrdemServicoCompletaUseCase _useCase;
    
    public OrdemServicoController(
        BuscarOrdemServicoUseCase buscar,
        BuscarStatusUseCase buscarStatus,
        BuscarListaPaginadaOrdemServicoUseCase listar,
        BuscarListaPaginadaPorDocumentoUseCase listarPorDocumento,
        InserirOrdemServicoUseCase inserir,
        InserirProdutosUseCase inserirProdutos,
        InserirServicosUseCase inserirServicos,
        IniciarDiagnosticoUseCase iniciarDiagnostico,
        FinalizarDiagnosticoUseCase finalizarDiagnostico,
        AprovarOrcamentoUseCase aprovarOrcamento,
        ReprovarOrcamentoUseCase reprovarOrcamento,
        IniciarExecucaoServicoUseCase iniciarExecucao,
        FinalizarExecucaoServicoUseCase finalizarExecucao,
        EntregarUseCase entregar,
        RemoverOrdemServicoUseCase remover,
        RemoverProdutoUseCase removerProduto,
        RemoverServicoUseCase removerServico, 
        AtualizarStatusUseCase atualizarStatusUseCase, 
        InserirOrdemServicoCompletaUseCase useCase)
    {
        _buscar = buscar;
        _buscarStatus = buscarStatus;
        _listar = listar;
        _listarPorDocumento = listarPorDocumento;
        _inserir = inserir;
        _inserirProdutos = inserirProdutos;
        _inserirServicos = inserirServicos;
        _iniciarDiagnostico = iniciarDiagnostico;
        _finalizarDiagnostico = finalizarDiagnostico;
        _aprovarOrcamento = aprovarOrcamento;
        _reprovarOrcamento = reprovarOrcamento;
        _iniciarExecucao = iniciarExecucao;
        _finalizarExecucao = finalizarExecucao;
        _entregar = entregar;
        _remover = remover;
        _removerProduto = removerProduto;
        _removerServico = removerServico;
        _atualizarStatusUseCase = atualizarStatusUseCase;
        _useCase = useCase;
    }

    public async Task Buscar(BuscarOrdemServicoInput input, CancellationToken ct = default)
        => await _buscar.Execute(input, ct);

    public async Task BuscarStatus(BuscarStatusInput input, CancellationToken ct = default)
        => await _buscarStatus.Execute(input, ct);

    public async Task BuscarListaPaginada(BuscarListaPaginadaOrdemServicoInput input, CancellationToken ct = default)
        => await _listar.Execute(input, ct);

    public async Task BuscarListaPaginadaPorDocumento(BuscarListaPaginadaPorDocumentoInput input, CancellationToken ct = default)
        => await _listarPorDocumento.Execute(input, ct);

    public async Task Inserir(InserirOrdemServicoInput input, CancellationToken ct = default)
        => await _inserir.Execute(input, ct);
    
    public async Task InserirCompleta(InserirOrdemServicoCompletaInput input, CancellationToken ct = default)
        => await _useCase.Execute(input, ct);

    public async Task InserirProdutos(InserirProdutosInput input, CancellationToken ct = default)
        => await _inserirProdutos.Execute(input, ct);

    public async Task InserirServicos(InserirServicosOrdemServicoInput input, CancellationToken ct = default)
        => await _inserirServicos.Execute(input, ct);

    public async Task IniciarDiagnostico(IniciarDiagnosticoInput input, CancellationToken ct = default)
        => await _iniciarDiagnostico.Execute(input, ct);

    public async Task FinalizarDiagnostico(FinalizarDiagnosticoInput input, CancellationToken ct = default)
        => await _finalizarDiagnostico.Execute(input, ct);

    public async Task AprovarOrcamento(AprovarOrcamentoInput input, CancellationToken ct = default)
        => await _aprovarOrcamento.Execute(input, ct);

    public async Task ReprovarOrcamento(ReprovarOrcamentoInput input, CancellationToken ct = default)
        => await _reprovarOrcamento.Execute(input, ct);

    public async Task IniciarExecucaoServico(IniciarExecucaoServicoInput input, CancellationToken ct = default)
        => await _iniciarExecucao.Execute(input, ct);

    public async Task FinalizarExecucaoServico(FinalizarExecucaoServicoInput input, CancellationToken ct = default)
        => await _finalizarExecucao.Execute(input, ct);

    public async Task Entregar(EntregarInput input, CancellationToken ct = default)
        => await _entregar.Execute(input, ct);

    public async Task Remover(RemoverOrdemServicoInput input, CancellationToken ct = default)
        => await _remover.Execute(input, ct);

    public async Task RemoverProduto(RemoverProdutoInput input, CancellationToken ct = default)
        => await _removerProduto.Execute(input, ct);

    public async Task RemoverServico(RemoverServicoOrdemServicoInput input, CancellationToken ct = default)
        => await _removerServico.Execute(input, ct);
    
    public async Task AtualizarStatus(AtualizarStatusOrdemServicoInput input, CancellationToken ct = default)
        => await _atualizarStatusUseCase.Execute(input, ct);
}