using Api.Extensions.Markers;
using Application.OrdensServico.UseCases;
using Application.OrdensServico.UseCases.AprovarOrcamento;
using Application.OrdensServico.UseCases.AtualizarStatus;
using Application.OrdensServico.UseCases.BuscarListaPaginada;
using Application.OrdensServico.UseCases.BuscarListaPaginadaPorDocumento;
using Application.OrdensServico.UseCases.BuscarOrdemServico;
using Application.OrdensServico.UseCases.BuscarStatus;
using Application.OrdensServico.UseCases.Entregar;
using Application.OrdensServico.UseCases.FinalizarDiagnostico;
using Application.OrdensServico.UseCases.FinalizarExecucaoServico;
using Application.OrdensServico.UseCases.IniciarDiagnostico;
using Application.OrdensServico.UseCases.IniciarExecucaoServico;
using Application.OrdensServico.UseCases.InserirOrdemServico;
using Application.OrdensServico.UseCases.InserirProdutos;
using Application.OrdensServico.UseCases.InserirServicos;
using Application.OrdensServico.UseCases.Remover;
using Application.OrdensServico.UseCases.RemoverProduto;
using Application.OrdensServico.UseCases.RemoverServico;
using Application.OrdensServico.UseCases.ReprovarOrcamento;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace Api.Controllers.OrdensServico.Presenters;

public class BuscarOrdemServicoPresenter : IBuscarOrdemServicoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void Ok(OrdemServicoOutput output) => Result = new OkObjectResult(output);
}

public class BuscarStatusPresenter : IBuscarStatusOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void Ok(OrdemServicoStatusOutput output) => Result = new OkObjectResult(output);
}

public class BuscarListaPaginadaOrdemServicoPresenter : IBuscarListaPaginadaOrdemServicoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void Ok(PagedResult<OrdemServicoOutput> resultado) => Result = new OkObjectResult(resultado);
}

public class BuscarListaPaginadaPorDocumentoPresenter : IBuscarListaPaginadaPorDocumentoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void Ok(PagedResult<OrdemServicoPorDocumentoOutput> resultado) => Result = new OkObjectResult(resultado);
}

public class InserirOrdemServicoPresenter : IInserirOrdemServicoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void ClienteNaoEncontrado() => Result = new NotFoundObjectResult(new { mensagem = "Cliente não encontrado." });
    public void VeiculoNaoPertenceAoCliente(string nomeCliente) => Result = new BadRequestObjectResult(new { mensagem = $"Veículo não encontrado para o cliente '{nomeCliente}'." });
    public void Ok(Guid id) => Result = new CreatedAtActionResult("Buscar", "OrdemServicos", new { id }, new { id });
}

public class InserirProdutosPresenter : IInserirProdutosOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void EstoqueInsuficiente(string mensagem) => Result = new ConflictObjectResult(new { mensagem });
    public void Ok() => Result = new OkResult();
}

public class InserirServicosPresenter : IInserirServicosOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void Ok() => Result = new OkResult();
}

public class IniciarDiagnosticoPresenter : IIniciarDiagnosticoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void Ok() => Result = new OkResult();
}

public class FinalizarDiagnosticoPresenter : IFinalizarDiagnosticoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void Ok() => Result = new OkResult();
}

public class AprovarOrcamentoPresenter : IAprovarOrcamentoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void Ok() => Result = new OkResult();
}

public class ReprovarOrcamentoPresenter : IReprovarOrcamentoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void Ok() => Result = new OkResult();
}

public class IniciarExecucaoServicoPresenter : IIniciarExecucaoServicoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void Ok() => Result = new OkResult();
}

public class FinalizarExecucaoServicoPresenter : IFinalizarExecucaoServicoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void Ok() => Result = new OkResult();
}

public class EntregarPresenter : IEntregarOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void Ok() => Result = new OkResult();
}

public class RemoverOrdemServicoPresenter : IRemoverOrdemServicoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void Ok() => Result = new NoContentResult();
}

public class RemoverProdutoPresenter : IRemoverProdutoOrdemServicoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void Ok() => Result = new OkResult();
}

public class RemoverServicoPresenter : IRemoverServicoOrdemServicoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void Ok() => Result = new OkResult();
}

public class AtualizarStatusPresenter : IAtualizarStatusOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void Ok()            => Result = new OkResult();
}