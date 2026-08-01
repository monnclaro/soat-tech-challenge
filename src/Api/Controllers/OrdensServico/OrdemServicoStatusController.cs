using Api.Controllers.OrdensServico.Requests;
using Api.Presenters.OrdensServico;
using Application.OrdensServico.Controllers;
using Application.OrdensServico.UseCases;
using Application.OrdensServico.UseCases.AtualizarStatus.DTOs;
using Application.OrdensServico.UseCases.BuscarStatus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.OrdensServico;

[ApiController]
[Route("api/v1/ordens-servico/{idOrdemServico:guid}/status")]
[Produces("application/json")]
public class OrdemServicoStatusController : ControllerBase
{
    private readonly OrdemServicoController _controller;
    private readonly BuscarStatusPresenter _buscarStatusPresenter;
    private readonly AtualizarStatusPresenter _atualizarStatusPresenter;
    
    public OrdemServicoStatusController(
        OrdemServicoController controller,
        BuscarStatusPresenter buscarStatusPresenter, 
        AtualizarStatusPresenter atualizarStatusPresenter)
    {
        _controller                    = controller;
        _buscarStatusPresenter         = buscarStatusPresenter;
        _atualizarStatusPresenter = atualizarStatusPresenter;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(OrdemServicoStatusOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BuscarStatus(
        [FromRoute] Guid idOrdemServico,
        CancellationToken ct)
    {
        await _controller.BuscarStatus(new BuscarStatusInput(idOrdemServico), ct);
        return _buscarStatusPresenter.Result!;
    }

    [HttpPatch]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AtualizarStatus(
        [FromRoute] Guid idOrdemServico,
        [FromBody] AtualizarStatusOrdemServicoRequest request,
        CancellationToken ct)
    {
        await _controller.AtualizarStatus(new AtualizarStatusOrdemServicoInput(idOrdemServico, request.Status), ct);
        return _atualizarStatusPresenter.Result!;
    }
}