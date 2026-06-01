namespace Application.OrdensServico.UseCases.Inserir;

public record InserirOrdemServicoInput(
    Guid IdCliente,
    Guid IdVeiculo,
    List<Guid> IdsServicos,
    List<InserirOrdemServicoProdutoInput> Produtos);

public record InserirOrdemServicoProdutoInput(Guid IdProduto, int Quantidade);