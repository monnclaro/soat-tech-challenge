using Application.Servicos.DTOs;
using Application.Servicos.UseCases;
using SharedKernel;

namespace Application.Servicos.Queries.BuscarListaPaginada;

public interface IBuscarListaPaginadaOutputPort
{
    void Ok(PagedResult<ServicoOutput> resultado);
}
