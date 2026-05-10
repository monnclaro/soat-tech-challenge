using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoatTechChallenge.Application.Common.DTOs;
using SoatTechChallenge.Application.OrdensServico.DTOs.Requests;
using SoatTechChallenge.Application.OrdensServico.DTOs.Responses;
using SoatTechChallenge.Application.OrdensServico.Services;
using SoatTechChallenge.Host.Controllers.OrdensServico.DTOs.Responses;

namespace SoatTechChallenge.Controllers.OrdensServico;

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
    
    [HttpGet("{id:guid}/status")]
    public async Task<IActionResult> BuscarStatus(Guid id)
    {
        var response = await _ordemServicoService.BuscarStatus(id);
        if (response is null) return NotFound();
        
        return Ok(response);
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

    [HttpPost]
    [Authorize(Roles = "Admin")] 
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Inserir([FromBody] InserirOrdemServicoRequest request)
    {
        var response = await _ordemServicoService.Inserir(request);
        return CreatedAtAction(nameof(Buscar), new { id = response }, response);
    }

    [HttpPost("{id:guid}/produtos")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> InserirProdutos([FromRoute] Guid id, [FromBody] InserirProdutosOrdemServicoRequest request)
    {
        await _ordemServicoService.InserirProdutos(id, request);
        return Ok();
    }

    [HttpPost("{id:guid}/servicos")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InserirServicos([FromRoute] Guid id, [FromBody] InserirServicosOrdemServicoRequest request)
    {
        await _ordemServicoService.InserirServicos(id, request);
        return Ok();
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
    [HttpPatch("{id:guid}/finalizar-diagnostico")]
    [Authorize(Roles = "Admin")] 
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FinalizarDiagnostico([FromRoute] Guid id)
    {
        await _ordemServicoService.FinalizarDiagnostico(id);
        return Ok();
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
    
    [HttpPatch("{id:guid}/servicos/{idServico:guid}/iniciar-execucao")]
    [Authorize(Roles = "Admin")] 
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> IniciarExecucaoServico([FromRoute] Guid id, [FromRoute] Guid idServico)
    {
        await _ordemServicoService.IniciarExecucaoServico(id, idServico);
        return Ok();
    }
    
    [HttpPatch("{id:guid}/servicos/{idServico:guid}/finalizar-execucao")]
    [Authorize(Roles = "Admin")] 
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FinalizarExecucaoServico([FromRoute] Guid id, [FromRoute] Guid idServico)
    {
        await _ordemServicoService.FinalizarExecucaoServico(id, idServico);
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
    
    [HttpDelete("{id:guid}/produtos/{idProduto:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoverProduto([FromRoute] Guid id, [FromRoute] Guid idProduto)
    {
        await _ordemServicoService.RemoverProduto(id, idProduto);
        return Ok();
    }

    [HttpDelete("{id:guid}/servicos/{idServico:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoverServico([FromRoute] Guid id, [FromRoute] Guid idServico)
    {
        await _ordemServicoService.RemoverServico(id, idServico);
        return Ok();
    }
}