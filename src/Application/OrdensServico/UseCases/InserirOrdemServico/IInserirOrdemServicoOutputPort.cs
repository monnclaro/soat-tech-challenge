namespace Application.OrdensServico.UseCases.InserirOrdemServico;

public interface IInserirOrdemServicoOutputPort
{
    void ClienteNaoEncontrado();
    void VeiculoNaoPertenceAoCliente(string nomeCliente);
    void Ok(Guid id);
}