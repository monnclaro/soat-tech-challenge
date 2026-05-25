using Application.Servicos.DTOs;

namespace Application.Servicos.Queries.BuscarServico;

public interface IBuscarServicoOutputPort
{
    void NaoEncontrado();
    void Ok(ServicoOutput output);
}
