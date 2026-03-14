using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoatTechChallenge.Application.Servicos.DTOs;
using SoatTechChallenge.Application.Servicos.Services;
using SoatTechChallenge.Host.Common.DTOs;
using SoatTechChallenge.Host.Controllers.Servicos.DTOs;

namespace SoatTechChallenge.Controllers.Servicos;

[ApiController]
[Route("api/v1/servicos")]
[Authorize(Roles = "Admin")] 
[Produces("application/json")]
public class ServicosController : ControllerBase
{
    private readonly IServicoService _servicoService;

    public ServicosController(IServicoService servicoService)
    {
        _servicoService = servicoService;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Buscar([FromRoute] Guid id)
    {
        var resultado = await _servicoService.Buscar(id);
        return Ok(resultado);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ServicoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarListaPaginada([FromQuery] PagedRequest request)
    {
        var resultado = await _servicoService.BuscarListaPaginada(request);
        return Ok(resultado);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ServicoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Inserir([FromBody] InserirServicoRequest request)
    {
        var resultado = await _servicoService.Inserir(request);
        return CreatedAtAction(nameof(Buscar), new { id = resultado.Id }, resultado);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar([FromRoute] Guid id, [FromBody] AtualizarServicoRequest request)
    {
        var resultado = await _servicoService.Atualizar(id, request);
        return Ok(resultado);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover([FromRoute] Guid id)
    {
        await _servicoService.Remover(id);
        return NoContent();
    }
}