using Api.Extensions.Markers;
using Application.OrdensServico.UseCases.InserirCompleta;
using Microsoft.AspNetCore.Mvc;

namespace Api.Presenters.OrdensServico;

public class InserirOrdemServicoCompletaPresenter : IInserirOrdemServicoCompletaOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }

    public void Ok(Guid idOrdem)
        => Result = new CreatedResult($"/ordens-servico/{idOrdem}", new { id = idOrdem });

    public void DocumentoDuplicado(string mensagem)
        => Result = new ConflictObjectResult(mensagem);

    public void PlacaDuplicada(string mensagem)
        => Result = new ConflictObjectResult(mensagem);
}