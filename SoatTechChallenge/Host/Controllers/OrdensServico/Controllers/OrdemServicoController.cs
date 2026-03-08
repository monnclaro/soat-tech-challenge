using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoatTechChallenge.Domain.OrdensServico.Services;
using SoatTechChallenge.Host.Common.DTOs;
using SoatTechChallenge.Host.Controllers.OrdensServico.DTOs.Requests;
using SoatTechChallenge.Host.Controllers.OrdensServico.DTOs.Responses;

namespace SoatTechChallenge.Host.Controllers.OrdensServico.Controllers;

[ApiController]
[Route("api/v1/ordens-servico")]
[Authorize(Roles = "Admin")] 
[Produces("application/json")]
public class OrdemServicosController : ControllerBase
{
    private readonly IOrdemServicoService _ordemServicoService;

    public OrdemServicosController(IOrdemServicoService ordemServicoService)
    {
        _ordemServicoService = ordemServicoService;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrdemServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Buscar([FromRoute] Guid id)
    {
        var resultado = await _ordemServicoService.Buscar(id);
        return Ok(resultado);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<OrdemServicoDetailedResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarListaPaginada([FromQuery] PagedRequest request)
    {
        var resultado = await _ordemServicoService.BuscarListaPaginada(request);
        return Ok(resultado);
    }
    
    [HttpGet("tempo-medio-execucao")]
    [ProducesResponseType(typeof(TempoMedioExecucaoOrdensServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BuscarTempoMedioExecucao()
    {
        var resultado = await _ordemServicoService.BuscarTempoMedioExecucao();
        if (resultado is null)
        {
            return NotFound("Não há ordens finalizadas para calcular o tempo médio de execução.");
        }
 
        return Ok(resultado);
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrdemServicoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Inserir([FromBody] InserirOrdemServicoRequest request)
    {
        var resultado = await _ordemServicoService.Inserir(request);
        return CreatedAtAction(nameof(Buscar), new { id = resultado.Id }, resultado);
    }
    
    [HttpPatch("{id:guid}/iniciar-diagnostico")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> IniciarDiagnostico([FromRoute] Guid id)
    {
        await _ordemServicoService.IniciarDiagnostico(id);
        return Ok();
    }
    
    [HttpPatch("{id:guid}/enviar-orcamento")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EnviarOrcamento([FromRoute] Guid id)
    {
        await _ordemServicoService.EnviarOrcamento(id);
        return Ok();
    }
    
    [HttpPatch("{id:guid}/aprovar-orcamento")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AprovarOrcamento([FromRoute] Guid id)
    {
        await _ordemServicoService.AprovarOrcamento(id);
        return Ok();
    }
    
    [HttpPatch("{id:guid}/iniciar-execucao")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> IniciarExecucao([FromRoute] Guid id)
    {
        await _ordemServicoService.IniciarExecucao(id);
        return Ok();
    }
    
    [HttpPatch("{id:guid}/finalizar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Finalizar([FromRoute] Guid id)
    {
        await _ordemServicoService.Finalizar(id);
        return Ok();
    }
    
    [HttpPatch("{id:guid}/entrega")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Entregar([FromRoute] Guid id)
    {
        await _ordemServicoService.Entregar(id);
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover([FromRoute] Guid id)
    {
        await _ordemServicoService.Remover(id);
        return NoContent();
    }
}