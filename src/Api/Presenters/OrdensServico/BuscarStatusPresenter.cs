using Api.Extensions.Markers;
using Application.OrdensServico.UseCases;
using Application.OrdensServico.UseCases.BuscarStatus;
using Microsoft.AspNetCore.Mvc;

namespace Api.Presenters.OrdensServico;

public class BuscarStatusPresenter : IBuscarStatusOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void Ok(OrdemServicoStatusOutput output) => Result = new OkObjectResult(output);
}