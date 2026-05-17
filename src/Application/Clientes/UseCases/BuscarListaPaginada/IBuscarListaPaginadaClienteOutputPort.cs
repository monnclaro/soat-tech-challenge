using SharedKernel;

namespace Application.Clientes.UseCases.BuscarListaPaginada;

public interface IBuscarListaPaginadaClienteOutputPort
{
    void Ok(PagedResult<ClienteOutput> resultado);
}
