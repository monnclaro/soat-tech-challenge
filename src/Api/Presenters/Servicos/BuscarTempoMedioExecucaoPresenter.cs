using Api.Extensions.Markers;
using Application.Servicos.DTOs;
using Application.Servicos.Queries.BuscarTempoMedioExecucao;
using Microsoft.AspNetCore.Mvc;

namespace Api.Presenters.Servicos;

public class BuscarTempoMedioExecucaoPresenter : IBuscarTempoMedioExecucaoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void Ok(IReadOnlyList<TempoMedioExecucaoOutput> r) => Result = new OkObjectResult(r);
}