using Api.Extensions.Markers;
using Application.Servicos.DTOs;
using Application.Servicos.Queries.BuscarListaPaginada;
using Application.Servicos.Queries.BuscarServico;
using Application.Servicos.Queries.BuscarTempoMedioExecucao;
using Application.Servicos.UseCases.AtualizarServico;
using Application.Servicos.UseCases.InserirServico;
using Application.Servicos.UseCases.RemoverServico;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.DTOs;

namespace Api.Controllers.Servicos.Presenters;

public class BuscarServicoPresenter : IBuscarServicoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void Ok(ServicoOutput output) => Result = new OkObjectResult(output);
}

public class BuscarListaPaginadaPresenter : IBuscarListaPaginadaOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void Ok(PagedResult<ServicoOutput> r) => Result = new OkObjectResult(r);
}

public class BuscarTempoMedioExecucaoPresenter : IBuscarTempoMedioExecucaoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void Ok(IReadOnlyList<TempoMedioExecucaoOutput> r) => Result = new OkObjectResult(r);
}

public class InserirServicoPresenter : IInserirServicoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void Ok(ServicoOutput output) => Result = new CreatedAtActionResult("Buscar", "Servicos", new { id = output.Id }, output);
}

public class AtualizarServicoPresenter : IAtualizarServicoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void Ok(ServicoOutput output) => Result = new OkObjectResult(output);
}

public class RemoverServicoPresenter : IRemoverServicoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void Ok() => Result = new NoContentResult();
}