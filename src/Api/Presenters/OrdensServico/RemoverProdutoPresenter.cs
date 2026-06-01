using Api.Extensions.Markers;
using Application.OrdensServico.UseCases.RemoverProduto;
using Microsoft.AspNetCore.Mvc;

namespace Api.Presenters.OrdensServico;

public class RemoverProdutoPresenter : IRemoverProdutoOrdemServicoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void Ok() => Result = new OkResult();
}