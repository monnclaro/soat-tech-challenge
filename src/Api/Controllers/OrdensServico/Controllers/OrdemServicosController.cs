using Api.Controllers.OrdensServico.Presenters;
using Api.Controllers.OrdensServico.Requests;
using Application.OrdensServico.Controllers;
using Application.OrdensServico.UseCases;
using Application.OrdensServico.UseCases.AprovarOrcamento;
using Application.OrdensServico.UseCases.BuscarListaPaginada;
using Application.OrdensServico.UseCases.BuscarListaPaginadaPorDocumento;
using Application.OrdensServico.UseCases.BuscarOrdemServico;
using Application.OrdensServico.UseCases.BuscarStatus;
using Application.OrdensServico.UseCases.Entregar;
using Application.OrdensServico.UseCases.FinalizarDiagnostico;
using Application.OrdensServico.UseCases.FinalizarExecucaoServico;
using Application.OrdensServico.UseCases.IniciarDiagnostico;
using Application.OrdensServico.UseCases.IniciarExecucaoServico;
using Application.OrdensServico.UseCases.InserirOrdemServico;
using Application.OrdensServico.UseCases.InserirProdutos;
using Application.OrdensServico.UseCases.InserirServicos;
using Application.OrdensServico.UseCases.Remover;
using Application.OrdensServico.UseCases.RemoverProduto;
using Application.OrdensServico.UseCases.RemoverServico;
using Application.OrdensServico.UseCases.ReprovarOrcamento;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace Api.Controllers.OrdensServico.Controllers;

[ApiController]
[Route("api/v1/ordens-servico")]
[Produces("application/json")]
public class OrdemServicosController : ControllerBase
{
    private readonly OrdemServicoController _controller;
    private readonly BuscarOrdemServicoPresenter _buscarPresenter;
    private readonly BuscarStatusPresenter _buscarStatusPresenter;
    private readonly BuscarListaPaginadaOrdemServicoPresenter _listarPresenter;
    private readonly BuscarListaPaginadaPorDocumentoPresenter _listarPorDocumentoPresenter;
    private readonly InserirOrdemServicoPresenter _inserirPresenter;
    private readonly InserirProdutosPresenter _inserirProdutosPresenter;
    private readonly InserirServicosPresenter _inserirServicosPresenter;
    private readonly IniciarDiagnosticoPresenter _iniciarDiagnosticoPresenter;
    private readonly FinalizarDiagnosticoPresenter _finalizarDiagnosticoPresenter;
    private readonly AprovarOrcamentoPresenter _aprovarOrcamentoPresenter;
    private readonly ReprovarOrcamentoPresenter _reprovarOrcamentoPresenter;
    private readonly IniciarExecucaoServicoPresenter _iniciarExecucaoPresenter;
    private readonly FinalizarExecucaoServicoPresenter _finalizarExecucaoPresenter;
    private readonly EntregarPresenter _entregarPresenter;
    private readonly RemoverOrdemServicoPresenter _removerPresenter;
    private readonly RemoverProdutoPresenter _removerProdutoPresenter;
    private readonly RemoverServicoPresenter _removerServicoPresenter;

    public OrdemServicosController(
        OrdemServicoController controller,
        BuscarOrdemServicoPresenter buscarPresenter,
        BuscarStatusPresenter buscarStatusPresenter,
        BuscarListaPaginadaOrdemServicoPresenter listarPresenter,
        BuscarListaPaginadaPorDocumentoPresenter listarPorDocumentoPresenter,
        InserirOrdemServicoPresenter inserirPresenter,
        InserirProdutosPresenter inserirProdutosPresenter,
        InserirServicosPresenter inserirServicosPresenter,
        IniciarDiagnosticoPresenter iniciarDiagnosticoPresenter,
        FinalizarDiagnosticoPresenter finalizarDiagnosticoPresenter,
        AprovarOrcamentoPresenter aprovarOrcamentoPresenter,
        ReprovarOrcamentoPresenter reprovarOrcamentoPresenter,
        IniciarExecucaoServicoPresenter iniciarExecucaoPresenter,
        FinalizarExecucaoServicoPresenter finalizarExecucaoPresenter,
        EntregarPresenter entregarPresenter,
        RemoverOrdemServicoPresenter removerPresenter,
        RemoverProdutoPresenter removerProdutoPresenter,
        RemoverServicoPresenter removerServicoPresenter)
    {
        _controller                  = controller;
        _buscarPresenter             = buscarPresenter;
        _buscarStatusPresenter       = buscarStatusPresenter;
        _listarPresenter             = listarPresenter;
        _listarPorDocumentoPresenter = listarPorDocumentoPresenter;
        _inserirPresenter            = inserirPresenter;
        _inserirProdutosPresenter    = inserirProdutosPresenter;
        _inserirServicosPresenter    = inserirServicosPresenter;
        _iniciarDiagnosticoPresenter = iniciarDiagnosticoPresenter;
        _finalizarDiagnosticoPresenter = finalizarDiagnosticoPresenter;
        _aprovarOrcamentoPresenter   = aprovarOrcamentoPresenter;
        _reprovarOrcamentoPresenter  = reprovarOrcamentoPresenter;
        _iniciarExecucaoPresenter    = iniciarExecucaoPresenter;
        _finalizarExecucaoPresenter  = finalizarExecucaoPresenter;
        _entregarPresenter           = entregarPresenter;
        _removerPresenter            = removerPresenter;
        _removerProdutoPresenter     = removerProdutoPresenter;
        _removerServicoPresenter     = removerServicoPresenter;
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(OrdemServicoOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Buscar([FromRoute] Guid id, CancellationToken ct)
    {
        await _controller.Buscar(new BuscarOrdemServicoInput(id), ct);
        return _buscarPresenter.Result!;
    }

    [HttpGet("{id:guid}/status")]
    [ProducesResponseType(typeof(OrdemServicoStatusOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BuscarStatus([FromRoute] Guid id, CancellationToken ct)
    {
        await _controller.BuscarStatus(new BuscarStatusInput(id), ct);
        return _buscarStatusPresenter.Result!;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PagedResult<OrdemServicoOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarListaPaginada([FromQuery] PagedRequest request, CancellationToken ct)
    {
        await _controller.BuscarListaPaginada(new BuscarListaPaginadaOrdemServicoInput(request), ct);
        return _listarPresenter.Result!;
    }

    [HttpGet("cliente")]
    [ProducesResponseType(typeof(PagedResult<OrdemServicoPorDocumentoOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarListaPaginadaPorDocumento([FromQuery] string documento, [FromQuery] PagedRequest request, CancellationToken ct)
    {
        await _controller.BuscarListaPaginadaPorDocumento(new BuscarListaPaginadaPorDocumentoInput(documento, request), ct);
        return _listarPorDocumentoPresenter.Result!;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Inserir([FromBody] InserirOrdemServicoRequest request, CancellationToken ct)
    {
        await _controller.Inserir(new InserirOrdemServicoInput(
            request.IdCliente, request.IdVeiculo,
            request.IdsServicos,
            request.Produtos.Select(p => new InserirOrdemServicoProdutoInput(p.IdProduto, p.Quantidade)).ToList()), ct);
        return _inserirPresenter.Result!;
    }

    [HttpPost("{id:guid}/produtos")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> InserirProdutos([FromRoute] Guid id, [FromBody] InserirProdutosOrdemServicoRequest request, CancellationToken ct)
    {
        await _controller.InserirProdutos(new InserirProdutosInput(
            id, request.Produtos.Select(p => new InserirProdutosItemInput(p.IdProduto, p.Quantidade)).ToList()), ct);
        return _inserirProdutosPresenter.Result!;
    }

    [HttpPost("{id:guid}/servicos")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> InserirServicos([FromRoute] Guid id, [FromBody] InserirServicosOrdemServicoRequest request, CancellationToken ct)
    {
        await _controller.InserirServicos(new InserirServicosOrdemServicoInput(id, request.Servicos.Select(s => new InserirServicosOrdemServicoItemInput(s.IdServico)).ToList()), ct);
        return _inserirServicosPresenter.Result!;
    }

    [HttpPatch("{id:guid}/iniciar-diagnostico")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> IniciarDiagnostico([FromRoute] Guid id, CancellationToken ct)
    {
        await _controller.IniciarDiagnostico(new IniciarDiagnosticoInput(id), ct);
        return _iniciarDiagnosticoPresenter.Result!;
    }

    [HttpPatch("{id:guid}/finalizar-diagnostico")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FinalizarDiagnostico([FromRoute] Guid id, CancellationToken ct)
    {
        await _controller.FinalizarDiagnostico(new FinalizarDiagnosticoInput(id), ct);
        return _finalizarDiagnosticoPresenter.Result!;
    }

    [HttpPatch("{id:guid}/orcamento/aprovacao")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AprovarOrcamento([FromRoute] Guid id, CancellationToken ct)
    {
        await _controller.AprovarOrcamento(new AprovarOrcamentoInput(id), ct);
        return _aprovarOrcamentoPresenter.Result!;
    }

    [HttpPatch("{id:guid}/orcamento/reprovacao")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReprovarOrcamento([FromRoute] Guid id, CancellationToken ct)
    {
        await _controller.ReprovarOrcamento(new ReprovarOrcamentoInput(id), ct);
        return _reprovarOrcamentoPresenter.Result!;
    }

    [HttpPatch("{id:guid}/servicos/{idServico:guid}/iniciar-execucao")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> IniciarExecucaoServico([FromRoute] Guid id, [FromRoute] Guid idServico, CancellationToken ct)
    {
        await _controller.IniciarExecucaoServico(new IniciarExecucaoServicoInput(id, idServico), ct);
        return _iniciarExecucaoPresenter.Result!;
    }

    [HttpPatch("{id:guid}/servicos/{idServico:guid}/finalizar-execucao")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FinalizarExecucaoServico([FromRoute] Guid id, [FromRoute] Guid idServico, CancellationToken ct)
    {
        await _controller.FinalizarExecucaoServico(new FinalizarExecucaoServicoInput(id, idServico), ct);
        return _finalizarExecucaoPresenter.Result!;
    }

    [HttpPatch("{id:guid}/entrega")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Entregar([FromRoute] Guid id, CancellationToken ct)
    {
        await _controller.Entregar(new EntregarInput(id), ct);
        return _entregarPresenter.Result!;
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover([FromRoute] Guid id, CancellationToken ct)
    {
        await _controller.Remover(new RemoverOrdemServicoInput(id), ct);
        return _removerPresenter.Result!;
    }

    [HttpDelete("{id:guid}/produtos/{idProduto:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoverProduto([FromRoute] Guid id, [FromRoute] Guid idProduto, CancellationToken ct)
    {
        await _controller.RemoverProduto(new RemoverProdutoInput(id, idProduto), ct);
        return _removerProdutoPresenter.Result!;
    }

    [HttpDelete("{id:guid}/servicos/{idServico:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoverServico([FromRoute] Guid id, [FromRoute] Guid idServico, CancellationToken ct)
    {
        await _controller.RemoverServico(new RemoverServicoOrdemServicoInput(id, idServico), ct);
        return _removerServicoPresenter.Result!;
    }
}