using Application.Common.Interfaces;
using Domain.Clientes;
using Domain.Clientes.Gateways;
using Domain.Clientes.ValueObjects;
using Domain.Clientes.Veiculos;
using Domain.Clientes.Veiculos.Gateways;
using Domain.Clientes.Veiculos.ValueObjects;
using Domain.OrdensServico;
using Domain.OrdensServico.Gateways;
using Domain.OrdensServico.Produtos;
using Domain.OrdensServico.Servicos;
using Domain.Produtos;
using Domain.Produtos.Gateways;
using Domain.Servicos;
using Domain.Servicos.Gateways;

namespace Application.OrdensServico.UseCases.InserirCompleta;

public class InserirOrdemServicoCompletaUseCase : IUseCase
{
    private readonly IClienteGateway _clienteGateway;
    private readonly IVeiculoGateway _veiculoGateway;
    private readonly IServicoGateway _servicoGateway;
    private readonly IProdutoGateway _produtoGateway;
    private readonly IOrdemServicoGateway _ordemGateway;
    private readonly IInserirOrdemServicoCompletaOutputPort _outputPort;

    public InserirOrdemServicoCompletaUseCase(
        IClienteGateway clienteGateway,
        IVeiculoGateway veiculoGateway,
        IServicoGateway servicoGateway,
        IProdutoGateway produtoGateway,
        IOrdemServicoGateway ordemGateway,
        IInserirOrdemServicoCompletaOutputPort outputPort)
    {
        _clienteGateway = clienteGateway;
        _veiculoGateway = veiculoGateway;
        _servicoGateway = servicoGateway;
        _produtoGateway = produtoGateway;
        _ordemGateway = ordemGateway;
        _outputPort = outputPort;
    }

    public async Task Execute(InserirOrdemServicoCompletaInput input, CancellationToken ct = default)
    {
        var documento = DocumentoCliente.Criar(input.Cliente.Documento);

        var cliente = await _clienteGateway.BuscarPorDocumento(documento.Numero, ct);
        if (cliente is null)
        {
            cliente = new Cliente();
            cliente.Inserir(input.Cliente.Nome, documento);
            await _clienteGateway.Salvar(cliente, ct);
        }

        var placa = Placa.Criar(input.Cliente.Veiculo.Placa);

        var veiculo = await _veiculoGateway.BuscarPorPlaca(placa.Valor, ct);
        if (veiculo is null)
        {
            veiculo = new Veiculo();
            veiculo.Inserir(cliente.Id, placa, input.Cliente.Veiculo.Marca, input.Cliente.Veiculo.Modelo, input.Cliente.Veiculo.Ano);
            await _veiculoGateway.Inserir(veiculo, ct);
        }
    
        var servicosSalvos = new List<Servico>();
        foreach (var s in input.Servicos)
        {
            var servico = new Servico();
            servico.Inserir(s.Nome, s.Descricao, s.Valor);
            await _servicoGateway.Salvar(servico, ct);
            servicosSalvos.Add(servico);
        }
     
        var produtosSalvos = new List<(Produto produto, int quantidade)>();
        foreach (var p in input.Produtos)
        {
            var produto = new Produto();
            produto.Inserir(p.Nome, p.Descricao, p.Valor, p.QuantidadeEmEstoque);
            await _produtoGateway.Salvar(produto, ct);
            produtosSalvos.Add((produto, p.QuantidadeNaOrdem));
        }
    
        var ordemServico = new OrdemServico();
        var servicosDaOrdem = servicosSalvos
            .Select(s => new OrdemServicoServico(ordemServico.Id, s.Id, s.Nome, s.Valor))
            .ToList();

        var produtosDaOrdem = produtosSalvos
            .Select(x => new OrdemServicoProduto(ordemServico.Id, x.produto.Id, x.produto.Nome, x.produto.Valor, x.quantidade))
            .ToList();

        ordemServico.Inserir(cliente.Id, veiculo.Id, servicosDaOrdem, produtosDaOrdem);
        await _ordemGateway.Salvar(ordemServico, ct);

        _outputPort.Ok(ordemServico.Id);
    }
}