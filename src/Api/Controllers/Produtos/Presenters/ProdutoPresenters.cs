using Api.Extensions.Markers;
using Application.Produtos.DTOs;
using Application.Produtos.UseCases.AtualizarProduto;
using Application.Produtos.UseCases.BuscarListaPaginada;
using Application.Produtos.UseCases.BuscarProduto;
using Application.Produtos.UseCases.DecrementarEstoque;
using Application.Produtos.UseCases.IncrementarEstoque;
using Application.Produtos.UseCases.InserirProduto;
using Application.Produtos.UseCases.RemoverProduto;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.DTOs;

namespace Api.Controllers.Produtos.Presenters;

public class BuscarProdutoPresenter : IBuscarProdutoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void Ok(ProdutoOutput output) => Result = new OkObjectResult(output);
}

public class BuscarListaPaginadaProdutoPresenter : IBuscarListaPaginadaProdutoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void Ok(PagedResult<ProdutoOutput> resultado) => Result = new OkObjectResult(resultado);
}

public class InserirProdutoPresenter : IInserirProdutoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void Ok(ProdutoOutput output) => Result = new CreatedAtActionResult(
        "Buscar", "Produtos", new { id = output.Id }, output);
}

public class AtualizarProdutoPresenter : IAtualizarProdutoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void Ok(ProdutoOutput output) => Result = new OkObjectResult(output);
}

public class IncrementarEstoquePresenter : IIncrementarEstoqueOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void Ok(ProdutoOutput output) => Result = new OkObjectResult(output);
}

public class DecrementarEstoquePresenter : IDecrementarEstoqueOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void Ok() => Result = new NoContentResult();
}

public class RemoverProdutoPresenter : IRemoverProdutoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void Ok() => Result = new NoContentResult();
}
