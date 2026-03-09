using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoatTechChallenge.Domain.OrdensServico.Services;
using SoatTechChallenge.Host.Common.DTOs;
using SoatTechChallenge.Host.Controllers.OrdensServico.DTOs.Requests;
using SoatTechChallenge.Host.Controllers.OrdensServico.DTOs.Responses;

namespace SoatTechChallenge.Host.Controllers.OrdensServico;

[ApiController]
[Route("api/v1/ordens-servico")]

[Produces("application/json")]
public class OrdemServicosController : ControllerBase
{
    private readonly IOrdemServicoService _ordemServicoService;

    public OrdemServicosController(IOrdemServicoService ordemServicoService)
    {
        _ordemServicoService = ordemServicoService;
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin")] 
    [ProducesResponseType(typeof(OrdemServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Buscar([FromRoute] Guid id)
    {
        var resultado = await _ordemServicoService.Buscar(id);
        if(resultado is null)
        {
            return NotFound();
        }
        
        return Ok(resultado);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")] 
    [ProducesResponseType(typeof(PagedResponse<OrdemServicoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarListaPaginada([FromQuery] PagedRequest request)
    {
        var resultado = await _ordemServicoService.BuscarListaPaginada(request);
        return Ok(resultado);
    }
    
    [HttpGet("cliente")]
    [ProducesResponseType(typeof(PagedResponse<OrdemServicoPorDocumentoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarListaPaginadaPorDocumento([FromQuery] string documento, [FromQuery] PagedRequest request)
    {
        var resultado = await _ordemServicoService.BuscarListaPaginadaPorDocumento(documento, request);
        return Ok(resultado);
    }
    
    [HttpGet("tempo-medio-execucao")]
    [Authorize(Roles = "Admin")] 
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
    [Authorize(Roles = "Admin")] 
    [ProducesResponseType(typeof(OrdemServicoOrcamentoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Inserir([FromBody] InserirOrdemServicoRequest request)
    {
        var resultado = await _ordemServicoService.Inserir(request);
        return Ok(resultado);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(OrdemServicoOrcamentoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Atualizar([FromRoute] Guid id, [FromBody] AtualizarOrdemServicoRequest request)
    {
        var resultado = await _ordemServicoService.Atualizar(id, request);
        return Ok(resultado);
    }

    [HttpPatch("{id:guid}/iniciar-diagnostico")]
    [Authorize(Roles = "Admin")] 
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> IniciarDiagnostico([FromRoute] Guid id)
    {
        await _ordemServicoService.IniciarDiagnostico(id);
        return Ok();
    }
    
    [HttpPatch("{id:guid}/enviar-orcamento")]
    [Authorize(Roles = "Admin")] 
    [ProducesResponseType(typeof(OrdemServicoOrcamentoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EnviarOrcamento([FromRoute] Guid id)
    {
        var resultado = await _ordemServicoService.EnviarOrcamento(id);
        return Ok(resultado);
    }
    
    [HttpPatch("{id:guid}/aprovar-orcamento")]
    [Authorize(Roles = "Admin")] 
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AprovarOrcamento([FromRoute] Guid id)
    {
        await _ordemServicoService.AprovarOrcamento(id);
        return Ok();
    }
    
    [HttpPatch("{id:guid}/finalizar")]
    [Authorize(Roles = "Admin")] 
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Finalizar([FromRoute] Guid id)
    {
        await _ordemServicoService.Finalizar(id);
        return Ok();
    }
    
    [HttpPatch("{id:guid}/entrega")]
    [Authorize(Roles = "Admin")] 
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Entregar([FromRoute] Guid id)
    {
        await _ordemServicoService.Entregar(id);
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")] 
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover([FromRoute] Guid id)
    {
        await _ordemServicoService.Remover(id);
        return NoContent();
    }
}