using Application.Servicos.DTOs;
using Application.Servicos.UseCases;

namespace Application.Servicos.Queries.BuscarServico;

public interface IBuscarServicoOutputPort
{
    void NaoEncontrado();
    void Ok(ServicoOutput output);
}
