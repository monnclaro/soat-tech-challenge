using Api.Extensions.Markers;
using Application.Clientes.UseCases;
using Application.Clientes.UseCases.AtualizarCliente;
using Application.Clientes.UseCases.BuscarCliente;
using Application.Clientes.UseCases.BuscarListaPaginada;
using Application.Clientes.UseCases.InserirCliente;
using Application.Clientes.UseCases.RemoverCliente;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.DTOs;

namespace Api.Controllers.Clientes.Presenters;

public class BuscarClientePresenter : IBuscarClienteOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void Ok(ClienteOutput output) => Result = new OkObjectResult(output);
}

public class BuscarListaPaginadaClientePresenter : IBuscarListaPaginadaClienteOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void Ok(PagedResult<ClienteOutput> resultado) => Result = new OkObjectResult(resultado);
}

public class InserirClientePresenter : IInserirClienteOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }

    public void DocumentoDuplicado(string mensagem) =>
        Result = new ConflictObjectResult(new { mensagem });

    public void Ok(ClienteOutput output) =>
        Result = new CreatedAtActionResult("Buscar", "Clientes", new { id = output.Id }, output);
}

public class AtualizarClientePresenter : IAtualizarClienteOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void Ok(ClienteOutput output) => Result = new OkObjectResult(output);
}

public class RemoverClientePresenter : IRemoverClienteOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void Ok() => Result = new NoContentResult();
}
