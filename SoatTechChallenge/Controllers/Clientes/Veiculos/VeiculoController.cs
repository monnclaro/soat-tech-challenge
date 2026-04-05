using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoatTechChallenge.Application.Clientes.Veiculos.DTOs;
using SoatTechChallenge.Application.Clientes.Veiculos.DTOs.Requests;
using SoatTechChallenge.Application.Clientes.Veiculos.DTOs.Responses;
using SoatTechChallenge.Application.Clientes.Veiculos.Services;
using SoatTechChallenge.Application.Common.DTOs;

namespace SoatTechChallenge.Controllers.Clientes.Veiculos;

[ApiController]
[Route("api/v1/clientes/{idCliente:guid}/veiculos")]
[Authorize(Roles = "Admin")] 
[Produces("application/json")]
public class VeiculosController : ControllerBase
{
    private readonly IVeiculoService _veiculoService;

    public VeiculosController(IVeiculoService veiculoService)
    {
        _veiculoService = veiculoService;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VeiculoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Buscar([FromRoute] Guid id)
    {
        var resultado = await _veiculoService.Buscar(id);
        return Ok(resultado);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<VeiculoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarListaPaginada([FromRoute] Guid idCliente, [FromQuery] PagedRequest request)
    {
        var resultado = await _veiculoService.BuscarListaPaginada(idCliente, request);
        return Ok(resultado);
    }

    [HttpPost]
    [ProducesResponseType(typeof(VeiculoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Inserir([FromRoute] Guid idCliente, [FromBody] InserirVeiculoRequest request)
    {
        var resultado = await _veiculoService.Inserir(idCliente, request);
        return CreatedAtAction(nameof(Buscar), new { idCliente = idCliente, id = resultado.Id }, resultado);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(VeiculoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar([FromRoute] Guid id, [FromBody] AtualizarVeiculoRequest request)
    {
        var resultado = await _veiculoService.Atualizar(id, request);
        return Ok(resultado);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover([FromRoute] Guid id)
    {
        await _veiculoService.Remover(id);
        return NoContent();
    }
}