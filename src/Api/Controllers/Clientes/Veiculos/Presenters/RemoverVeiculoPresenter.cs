using Api.Extensions.Markers;
using Application.Clientes.Veiculos.UseCases.RemoverVeiculo;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Clientes.Veiculos.Presenters;

public class RemoverVeiculoPresenter : IRemoverVeiculoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void Ok() => Result = new NoContentResult();
}
