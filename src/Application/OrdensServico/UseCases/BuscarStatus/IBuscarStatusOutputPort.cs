namespace Application.OrdensServico.UseCases.BuscarStatus;

public interface IBuscarStatusOutputPort
{
    void NaoEncontrado();
    void Ok(OrdemServicoStatusOutput output);
}