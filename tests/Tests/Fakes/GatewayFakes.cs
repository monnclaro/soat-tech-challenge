using Domain.Clientes;
using Domain.Clientes.Gateways;
using Domain.Clientes.Veiculos;
using Domain.Clientes.Veiculos.Gateways;
using Domain.OrdensServico;
using Domain.OrdensServico.Gateways;
using Domain.Produtos;
using Domain.Produtos.Gateways;
using Domain.Servicos;
using Domain.Servicos.Gateways;
using SharedKernel.DTOs;

namespace Tests.Fakes;

public class FakeClienteGateway : IClienteGateway
{
    private readonly List<Cliente> _clientes;
    private readonly bool _existeDocumento;
    public bool SalvarFoiChamado    { get; private set; }
    public bool AtualizarFoiChamado { get; private set; }
    public bool RemoverFoiChamado   { get; private set; }

    public FakeClienteGateway(bool existeDocumento, params Cliente[] clientes)
    {
        _clientes        = [..clientes];
        _existeDocumento = existeDocumento;
    }

    public FakeClienteGateway(params Cliente[] clientes) : this(false, clientes) { }

    public Task<Cliente?> BuscarPorId(Guid id, CancellationToken ct)
        => Task.FromResult(_clientes.FirstOrDefault(c => c.Id == id));

    public Task<Cliente?> BuscarPorDocumento(string documento, CancellationToken ct)
        => Task.FromResult(_clientes.FirstOrDefault(c => c.Documento == documento));

    public Task<Cliente?> BuscarComVeiculos(Guid id, CancellationToken ct)
        => Task.FromResult(_clientes.FirstOrDefault(c => c.Id == id));

    public Task<bool> ExisteComDocumento(string documento, CancellationToken ct)
        => Task.FromResult(_existeDocumento);

    public Task<(IReadOnlyList<Cliente> Items, int Total)> BuscarPaginado(PagedRequest p, CancellationToken ct)
    {
        var items = _clientes.Skip((p.Pagina - 1) * p.Tamanho).Take(p.Tamanho).ToList();
        return Task.FromResult(((IReadOnlyList<Cliente>)items, _clientes.Count));
    }

    public Task Salvar(Cliente cliente, CancellationToken ct)
    {
        SalvarFoiChamado = true;
        _clientes.Add(cliente);
        return Task.CompletedTask;
    }

    public Task Atualizar(Cliente cliente, CancellationToken ct)
    {
        AtualizarFoiChamado = true;
        return Task.CompletedTask;
    }

    public Task Remover(Cliente cliente, CancellationToken ct)
    {
        RemoverFoiChamado = true;
        _clientes.Remove(cliente);
        return Task.CompletedTask;
    }
}

public class FakeVeiculoGateway : IVeiculoGateway
{
    private readonly List<Veiculo> _veiculos;
    private readonly bool _placaEmUso;
    private readonly bool _placaEmUsoExcetoId;
    public bool InserirFoiChamado   { get; private set; }
    public bool AtualizarFoiChamado { get; private set; }
    public bool RemoverFoiChamado   { get; private set; }

    public FakeVeiculoGateway(bool placaEmUso, bool placaEmUsoExcetoId, params Veiculo[] veiculos)
    {
        _veiculos           = [..veiculos];
        _placaEmUso         = placaEmUso;
        _placaEmUsoExcetoId = placaEmUsoExcetoId;
    }

    public FakeVeiculoGateway(params Veiculo[] veiculos) : this(false, false, veiculos) { }

    public Task<Veiculo?> BuscarPorId(Guid id, CancellationToken ct)
        => Task.FromResult(_veiculos.FirstOrDefault(v => v.Id == id));

    public Task<Veiculo?> BuscarPorPlaca(string placa, CancellationToken ct)
        => Task.FromResult(_veiculos.FirstOrDefault(v => v.Placa == placa));

    public Task<bool> ExisteComPlaca(string placa, CancellationToken ct)
        => Task.FromResult(_placaEmUso);

    public Task<bool> ExisteComPlacaExcetoId(string placa, Guid idVeiculo, CancellationToken ct)
        => Task.FromResult(_placaEmUsoExcetoId);

    public Task<(IReadOnlyList<Veiculo> Items, int Total)> BuscarPaginadoPorCliente(Guid idCliente, PagedRequest p, CancellationToken ct)
    {
        var filtered = _veiculos.Where(v => v.IdCliente == idCliente).ToList();
        var items    = filtered.Skip((p.Pagina - 1) * p.Tamanho).Take(p.Tamanho).ToList();
        return Task.FromResult(((IReadOnlyList<Veiculo>)items, filtered.Count));
    }

    public Task Inserir(Veiculo veiculo, CancellationToken ct)
    {
        InserirFoiChamado = true;
        _veiculos.Add(veiculo);
        return Task.CompletedTask;
    }

    public Task Atualizar(Veiculo veiculo, CancellationToken ct)
    {
        AtualizarFoiChamado = true;
        return Task.CompletedTask;
    }

    public Task Remover(Veiculo veiculo, CancellationToken ct)
    {
        RemoverFoiChamado = true;
        _veiculos.Remove(veiculo);
        return Task.CompletedTask;
    }
}

public class FakeOrdemServicoGateway : IOrdemServicoGateway
{
    private readonly OrdemServico? _os;
    public bool SalvarFoiChamado    { get; private set; }
    public bool AtualizarFoiChamado { get; private set; }
    public bool RemoverFoiChamado   { get; private set; }
    public OrdemServico? OsSalva    { get; private set; }

    public FakeOrdemServicoGateway(OrdemServico? os = null) => _os = os;

    public Task<OrdemServico?> BuscarPorId(Guid id, CancellationToken ct)
        => Task.FromResult(_os?.Id == id ? _os : null);

    public Task<OrdemServico?> BuscarComServicos(Guid id, CancellationToken ct)
        => Task.FromResult(_os?.Id == id ? _os : null);

    public Task<OrdemServico?> BuscarComProdutos(Guid id, CancellationToken ct)
        => Task.FromResult(_os?.Id == id ? _os : null);

    public Task<OrdemServico?> BuscarComServicosProdutos(Guid id, CancellationToken ct)
        => Task.FromResult(_os?.Id == id ? _os : null);

    public Task Salvar(OrdemServico os, CancellationToken ct)
    {
        SalvarFoiChamado = true;
        OsSalva = os;
        return Task.CompletedTask;
    }

    public Task Atualizar(OrdemServico os, CancellationToken ct)
    {
        AtualizarFoiChamado = true;
        return Task.CompletedTask;
    }

    public Task Remover(OrdemServico os, CancellationToken ct)
    {
        RemoverFoiChamado = true;
        return Task.CompletedTask;
    }
}

public class FakeServicoGateway : IServicoGateway
{
    private readonly List<Servico> _servicos;
    public bool SalvarFoiChamado    { get; private set; }
    public bool AtualizarFoiChamado { get; private set; }
    public bool RemoverFoiChamado   { get; private set; }

    public FakeServicoGateway(params Servico[] servicos) => _servicos = [..servicos];

    public Task<Servico?> BuscarPorId(Guid id, CancellationToken ct)
        => Task.FromResult(_servicos.FirstOrDefault(s => s.Id == id));

    public Task<Dictionary<Guid, Servico>> BuscarPorIds(IReadOnlyList<Guid> ids, CancellationToken ct)
        => Task.FromResult(_servicos.Where(s => ids.Contains(s.Id)).ToDictionary(s => s.Id));

    public Task Salvar(Servico servico, CancellationToken ct)
    {
        SalvarFoiChamado = true;
        _servicos.Add(servico);
        return Task.CompletedTask;
    }

    public Task Atualizar(Servico servico, CancellationToken ct)
    {
        AtualizarFoiChamado = true;
        return Task.CompletedTask;
    }

    public Task Remover(Servico servico, CancellationToken ct)
    {
        RemoverFoiChamado = true;
        _servicos.Remove(servico);
        return Task.CompletedTask;
    }
}

public class FakeProdutoGateway : IProdutoGateway
{
    private readonly List<Produto> _produtos;
    public bool SalvarFoiChamado        { get; private set; }
    public bool AtualizarFoiChamado     { get; private set; }
    public bool AtualizarLoteFoiChamado { get; private set; }
    public bool RemoverFoiChamado       { get; private set; }

    public FakeProdutoGateway(params Produto[] produtos) => _produtos = [..produtos];

    public Task<Produto?> BuscarPorId(Guid id, CancellationToken ct)
        => Task.FromResult(_produtos.FirstOrDefault(p => p.Id == id));

    public Task<IReadOnlyList<Produto>> BuscarPorIds(IReadOnlyList<Guid> ids, CancellationToken ct)
        => Task.FromResult<IReadOnlyList<Produto>>(_produtos.Where(p => ids.Contains(p.Id)).ToList());

    public Task<Dictionary<Guid, Produto>> BuscarDicionarioPorIds(IReadOnlyList<Guid> ids, CancellationToken ct)
        => Task.FromResult(_produtos.Where(p => ids.Contains(p.Id)).ToDictionary(p => p.Id));

    public Task<(IReadOnlyList<Produto> Items, int Total)> BuscarPaginado(string? filtro, PagedRequest p, CancellationToken ct)
    {
        var items = _produtos.Skip((p.Pagina - 1) * p.Tamanho).Take(p.Tamanho).ToList();
        return Task.FromResult(((IReadOnlyList<Produto>)items, _produtos.Count));
    }

    public Task Salvar(Produto produto, CancellationToken ct)
    {
        SalvarFoiChamado = true;
        _produtos.Add(produto);
        return Task.CompletedTask;
    }

    public Task Atualizar(Produto produto, CancellationToken ct)
    {
        AtualizarFoiChamado = true;
        return Task.CompletedTask;
    }

    public Task AtualizarLote(IReadOnlyList<Produto> produtos, CancellationToken ct)
    {
        AtualizarLoteFoiChamado = true;
        return Task.CompletedTask;
    }

    public Task Remover(Produto produto, CancellationToken ct)
    {
        RemoverFoiChamado = true;
        _produtos.Remove(produto);
        return Task.CompletedTask;
    }
}
