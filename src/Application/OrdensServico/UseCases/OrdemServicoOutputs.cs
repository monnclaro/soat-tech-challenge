namespace Application.OrdensServico.UseCases;

public record OrdemServicoClienteOutput(Guid Id, string Nome, string Documento);
public record OrdemServicoVeiculoOutput(Guid Id, string Placa, string Marca, string Modelo, int Ano);
public record OrdemServicoServicoOutput(Guid Id, Guid IdServico, string NomeServico, decimal Valor, string Status);
public record OrdemServicoProdutoOutput(Guid Id, Guid IdProduto, string NomeProduto, decimal ValorUnitario, decimal Quantidade);

public record OrdemServicoOutput(
    Guid Id,
    OrdemServicoClienteOutput Cliente,
    OrdemServicoVeiculoOutput Veiculo,
    DateTime DataCriacao,
    DateTime? DataInicioExecucao,
    DateTime? DataFinalizacao,
    string Status,
    decimal ValorTotal,
    List<OrdemServicoServicoOutput> Servicos,
    List<OrdemServicoProdutoOutput> Produtos);

public record OrdemServicoStatusOutput(Guid Id, string Status);

public record OrdemServicoClientePorDocumentoOutput(string Nome, string Documento);
public record OrdemServicoVeiculoPorDocumentoOutput(string Placa, string Marca, string Modelo, int Ano);
public record OrdemServicoServicoPorDocumentoOutput(string NomeServico, string Status);

public record OrdemServicoPorDocumentoOutput(
    string Status,
    OrdemServicoClientePorDocumentoOutput Cliente,
    OrdemServicoVeiculoPorDocumentoOutput Veiculo,
    List<OrdemServicoServicoPorDocumentoOutput> Servicos);
