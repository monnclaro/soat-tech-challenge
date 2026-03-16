using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoatTechChallenge.Application.Common.DTOs;
using SoatTechChallenge.Application.Produtos.DTOs;
using SoatTechChallenge.Application.Produtos.Services;
using SoatTechChallenge.Host.Controllers.Produtos.DTOs;

namespace SoatTechChallenge.Controllers.Produtos;

[ApiController]
[Route("api/v1/produtos")]
[Authorize(Roles = "Admin")] 
[Produces("application/json")]
public class ProdutosController : ControllerBase
{
    private readonly IProdutoService _produtoService;

    public ProdutosController(IProdutoService produtoService)
    {
        _produtoService = produtoService;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Buscar([FromRoute] Guid id)
    {
        var resultado = await _produtoService.Buscar(id);
        return Ok(resultado);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ProdutoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarListaPaginada([FromQuery] PagedRequest request)
    {
        var resultado = await _produtoService.BuscarListaPaginada(request);
        return Ok(resultado);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Inserir([FromBody] InserirProdutoRequest request)
    {
        var resultado = await _produtoService.Inserir(request);
        return CreatedAtAction(nameof(Buscar), new { id = resultado.Id }, resultado);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar([FromRoute] Guid id, [FromBody] AtualizarProdutoRequest request)
    {
        var resultado = await _produtoService.Atualizar(id, request);
        return Ok(resultado);
    }
    
    [HttpPatch("{id:guid}/incrementar-estoque")]
    [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> IncrementarEstoque([FromRoute] Guid id, [FromBody] AtualizarQuantidadeEstoqueProdutoRequest request)
    {
        var resultado = await _produtoService.IncrementarEstoque(id, request);
        return Ok(resultado);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover([FromRoute] Guid id)
    {
        await _produtoService.Remover(id);
        return NoContent();
    }
}