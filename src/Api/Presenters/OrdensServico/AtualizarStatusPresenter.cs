using Api.Extensions.Markers;
using Application.OrdensServico.UseCases.AtualizarStatus;
using Microsoft.AspNetCore.Mvc;

namespace Api.Presenters.OrdensServico;

public class AtualizarStatusPresenter : IAtualizarStatusOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void Ok()            => Result = new OkResult();
}