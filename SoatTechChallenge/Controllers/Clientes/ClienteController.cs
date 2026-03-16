using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoatTechChallenge.Application.Clientes.DTOs;
using SoatTechChallenge.Application.Clientes.Services;
using SoatTechChallenge.Application.Common.DTOs;

namespace SoatTechChallenge.Controllers.Clientes;

[ApiController]
[Route("api/v1/clientes")]
[Authorize(Roles = "Admin")] 
[Produces("application/json")]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _clienteService;

    public ClientesController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Buscar([FromRoute] Guid id)
    {
        var resultado = await _clienteService.Buscar(id);
        return Ok(resultado);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ClienteResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarListaPaginada([FromQuery] PagedRequest request)
    {
        var resultado = await _clienteService.BuscarListaPaginada(request);
        return Ok(resultado);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Inserir([FromBody] InserirClienteRequest request)
    {
        var resultado = await _clienteService.Inserir(request);
        return CreatedAtAction(nameof(Buscar), new { id = resultado.Id }, resultado);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar([FromRoute] Guid id, [FromBody] AtualizarClienteRequest request)
    {
        var resultado = await _clienteService.Atualizar(id, request);
        return Ok(resultado);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover([FromRoute] Guid id)
    {
        await _clienteService.Remover(id);
        return NoContent();
    }
}