namespace Application.OrdensServico.UseCases.InserirCompleta;

public record InserirOrdemServicoCompletaInput(
    InserirOrdemServicoCompletaClienteInput Cliente,
    List<InserirOrdemServicoCompletaServicoInput> Servicos,
    List<InserirOrdemServicoCompletaProdutoInput> Produtos
);

public record InserirOrdemServicoCompletaClienteInput(string Nome, string Documento, InserirOrdemServicoCompletaVeiculoInput Veiculo);
public record InserirOrdemServicoCompletaVeiculoInput(string Placa, string Marca, string Modelo, int Ano);
public record InserirOrdemServicoCompletaServicoInput(string Nome, string Descricao, decimal Valor);
public record InserirOrdemServicoCompletaProdutoInput(string Nome, string Descricao, decimal Valor, int QuantidadeEmEstoque, int QuantidadeNaOrdem);