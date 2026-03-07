/*using Microsoft.AspNetCore.Mvc;
using SoatTechChallenge.Application.DTOs;
using SoatTechChallenge.Domain.OrdensServico.Services;
using SoatTechChallenge.Host.Controllers.Clientes.DTOs;

namespace SoatTechChallenge.Host.Controllers.OrdensServico.Controllers;

[ApiController]
[Route("api/v1/ordens-servico")]
[Produces("application/json")]
public class ClientesController : ControllerBase
{
    private readonly IOrdemServicoService _ordemServicoService;

    public ClientesController(IOrdemServicoService ordemServicoService)
    {
        _ordemServicoService = ordemServicoService;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Buscar([FromRoute] Guid id)
    {
        var resultado = await _ordemServicoService.Buscar(id);
        return Ok(resultado);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ClienteResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarListaPaginada([FromQuery] PagedRequest request)
    {
        var resultado = await _ordemServicoService.BuscarListaPaginada(request);
        return Ok(resultado);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Inserir([FromBody] InserirClienteRequest request)
    {
        var resultado = await _ordemServicoService.Inserir(request);
        return CreatedAtAction(nameof(Buscar), new { id = resultado.Id }, resultado);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar([FromRoute] Guid id, [FromBody] AtualizarClienteRequest request)
    {
        var resultado = await _ordemServicoService.Atualizar(id, request);
        return Ok(resultado);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover([FromRoute] Guid id)
    {
        await _ordemServicoService.Remover(id);
        return NoContent();
    }
}*/