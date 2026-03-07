using Microsoft.AspNetCore.Mvc;
using SoatTechChallenge.Application.DTOs;
using SoatTechChallenge.Domain.Clientes.Veiculos.Services;
using SoatTechChallenge.Host.Controllers.Clientes.Veiculos.DTOs;

namespace SoatTechChallenge.Clientes.Controllers.Veiculos;

[ApiController]
[Route("api/v1/clientes/{idCliente:guid}/veiculos")]
[Produces("application/json")]
public class ClientesVeiculosController : ControllerBase
{
    private readonly IClienteVeiculoService _clienteVeiculoService;

    public ClientesVeiculosController(IClienteVeiculoService clienteVeiculoService)
    {
        _clienteVeiculoService = clienteVeiculoService;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ClienteVeiculoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Buscar([FromRoute] Guid id)
    {
        var resultado = await _clienteVeiculoService.Buscar(id);
        return Ok(resultado);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ClienteVeiculoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarListaPaginada([FromRoute] Guid idCliente, [FromQuery] PagedRequest request)
    {
        var resultado = await _clienteVeiculoService.BuscarListaPaginada(idCliente, request);
        return Ok(resultado);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ClienteVeiculoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Inserir([FromRoute] Guid idCliente, [FromBody] InserirClienteVeiculoRequest request)
    {
        var resultado = await _clienteVeiculoService.Inserir(idCliente, request);
        return CreatedAtAction(nameof(Buscar), new { idCliente = idCliente, id = resultado.Id }, resultado);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ClienteVeiculoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar([FromRoute] Guid id, [FromBody] AtualizarClienteVeiculoRequest request)
    {
        var resultado = await _clienteVeiculoService.Atualizar(id, request);
        return Ok(resultado);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover([FromRoute] Guid id)
    {
        await _clienteVeiculoService.Remover(id);
        return NoContent();
    }
}