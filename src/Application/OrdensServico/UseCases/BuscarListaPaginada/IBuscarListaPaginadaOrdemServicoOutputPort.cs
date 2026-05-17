using SharedKernel;

namespace Application.OrdensServico.UseCases.BuscarListaPaginada;

public interface IBuscarListaPaginadaOrdemServicoOutputPort
{
    void Ok(PagedResult<OrdemServicoOutput> resultado);
}