using Application.Servicos.DTOs;
using SharedKernel.DTOs;

namespace Application.Servicos.Queries.BuscarListaPaginada;

public interface IBuscarListaPaginadaOutputPort
{
    void Ok(PagedResult<ServicoOutput> resultado);
}
