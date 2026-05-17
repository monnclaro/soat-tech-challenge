namespace Application.OrdensServico.UseCases.InserirProdutos;

public interface IInserirProdutosOutputPort
{
    void NaoEncontrado();
    void EstoqueInsuficiente(string mensagem);
    void Ok();
}