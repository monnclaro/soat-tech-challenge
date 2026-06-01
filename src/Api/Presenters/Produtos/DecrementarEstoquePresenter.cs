using Api.Extensions.Markers;
using Application.Produtos.UseCases.DecrementarEstoque;
using Microsoft.AspNetCore.Mvc;

namespace Api.Presenters.Produtos;

public class DecrementarEstoquePresenter : IDecrementarEstoqueOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void Ok() => Result = new NoContentResult();
}