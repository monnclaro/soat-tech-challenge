namespace Application.OrdensServico.UseCases.InserirOrdemServico;

public record InserirOrdemServicoInput(
    Guid IdCliente,
    Guid IdVeiculo,
    List<Guid> IdsServicos,
    List<InserirOrdemServicoProdutoInput> Produtos);