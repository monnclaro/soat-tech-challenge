using Api.Extensions.Markers;
using Application.OrdensServico.UseCases.InserirProdutos;
using Microsoft.AspNetCore.Mvc;

namespace Api.Presenters.OrdensServico;

public class InserirProdutosPresenter : IInserirProdutosOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void EstoqueInsuficiente(string mensagem) => Result = new ConflictObjectResult(new { mensagem });
    public void Ok() => Result = new OkResult();
}