using Api.Controllers.OrdensServico.Presenters;
using Application.OrdensServico.Controllers;
using Application.OrdensServico.UseCases.AprovarOrcamento;
using Application.OrdensServico.UseCases.ReprovarOrcamento;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.OrdensServico.Controllers;

[ApiController]
[Route("api/v1/orcamento")]
[Produces("application/json")]
public class OrcamentoController : ControllerBase
{
    private readonly OrdemServicoController _controller;
    private readonly AprovarOrcamentoPresenter _aprovarPresenter;
    private readonly ReprovarOrcamentoPresenter _reprovarPresenter;

    public OrcamentoController(
        OrdemServicoController controller,
        AprovarOrcamentoPresenter aprovarPresenter,
        ReprovarOrcamentoPresenter reprovarPresenter)
    {
        _controller       = controller;
        _aprovarPresenter  = aprovarPresenter;
        _reprovarPresenter = reprovarPresenter;
    }
   
    [HttpPost("{idOrdemServico:guid}/aprovacao")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AprovarOrcamento(
        [FromRoute] Guid idOrdemServico,
        CancellationToken ct)
    {
        await _controller.AprovarOrcamento(new AprovarOrcamentoInput(idOrdemServico), ct);
        return _aprovarPresenter.Result!;
    }
 
    [HttpPost("{idOrdemServico:guid}/reprovacao")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReprovarOrcamento(
        [FromRoute] Guid idOrdemServico,
        CancellationToken ct)
    {
        await _controller.ReprovarOrcamento(new ReprovarOrcamentoInput(idOrdemServico), ct);
        return _reprovarPresenter.Result!;
    }
}