using Application.Produtos.DTOs;
using SharedKernel;

namespace Application.Produtos.UseCases.BuscarListaPaginada;

public interface IBuscarListaPaginadaProdutoOutputPort
{
    void Ok(PagedResult<ProdutoOutput> resultado);
}
