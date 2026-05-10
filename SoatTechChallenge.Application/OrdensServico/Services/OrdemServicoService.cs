using Microsoft.EntityFrameworkCore;
using SoatTechChallenge.Application.Common.DTOs;
using SharedKernel;
using SoatTechChallenge.Application.OrdensServico.DTOs.Requests;
using SoatTechChallenge.Application.OrdensServico.DTOs.Responses;
using SoatTechChallenge.Application.OrdensServico.Services.Validators;
using SoatTechChallenge.Domain.Clientes;
using SoatTechChallenge.Domain.Clientes.Veiculos;
using SoatTechChallenge.Domain.Common.Exceptions;
using SoatTechChallenge.Domain.Common.Interfaces;
using SoatTechChallenge.Domain.OrdensServico;
using SoatTechChallenge.Domain.OrdensServico.Produtos;
using SoatTechChallenge.Domain.OrdensServico.Servicos;
using SoatTechChallenge.Domain.Produtos;
using SoatTechChallenge.Domain.Servicos;
using SoatTechChallenge.Host.Controllers.OrdensServico.DTOs.Responses;

namespace SoatTechChallenge.Application.OrdensServico.Services;

public class OrdemServicoService : IOrdemServicoService, IScopedService
{
    private readonly IRepository<OrdemServico> _repository;
    private readonly IOrdemServicoValidatorService _ordemServicoValidatorService;
    private readonly IRepository<Cliente> _clienteRepository;
    private readonly IRepository<Veiculo> _clienteVeiculoRepository;
    private readonly IRepository<Produto> _produtoRepository;
    private readonly IRepository<Servico> _servicoRepository;

    public OrdemServicoService(
        IRepository<OrdemServico> repository,
        IOrdemServicoValidatorService ordemServicoValidatorService,
        IRepository<Cliente> clienteRepository,
        IRepository<Veiculo> clienteVeiculoRepository,
        IRepository<Produto> produtoRepository,
        IRepository<Servico> servicoRepository)
    {
        _repository = repository;
        _ordemServicoValidatorService = ordemServicoValidatorService;
        _clienteRepository = clienteRepository;
        _clienteVeiculoRepository = clienteVeiculoRepository;
        _produtoRepository = produtoRepository;
        _servicoRepository = servicoRepository;
    }

    public async Task<OrdemServicoResponse?> Buscar(Guid id)
    {
        var query = from os in _repository.GetQueryable().AsNoTracking().Where(l => l.Id == id)
            join c in _clienteRepository.GetQueryable().AsNoTracking() on os.IdCliente equals c.Id
            join v in _clienteVeiculoRepository.GetQueryable().AsNoTracking() on os.IdVeiculo equals v.Id
            select new OrdemServicoResponse(
                os.Id,
                new OrdemServicoClienteResponse(
                    c.Id,
                    c.Nome,
                    c.Documento
                ),
                new OrdemServicoVeiculoResponse(
                    v.Id,
                    v.Placa,
                    v.Marca,
                    v.Modelo,
                    v.Ano
                ),
                os.DataCriacao,
                os.DataInicioExecucao,
                os.DataFinalizacao,
                os.Status.ToString(),
                os.ValorTotal,
                os.Servicos.Select(s => new OrdemServicoServicoResponse(
                    s.Id,
                    s.IdServico,
                    s.NomeServico,
                    s.Valor,
                    s.Status.ToString()
                )).ToList(),
                os.Produtos.Select(p => new OrdemServicoProdutoResponse(
                    p.Id,
                    p.IdProduto,
                    p.NomeProduto,
                    p.ValorUnitario,
                    p.Quantidade
                )).ToList()
            );

        return await query.FirstOrDefaultAsync();
    }
    
    public async Task<OrdemServicoStatusResponse?> BuscarStatus(Guid id)
    {
        return await _repository
            .GetQueryable()
            .AsNoTracking()
            .Where(l => l.Id == id)
            .Select(l => new OrdemServicoStatusResponse
            {
                Id = l.Id,
                Status = l.Status.ToString()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<PagedResponse<OrdemServicoResponse>> BuscarListaPaginada(PagedRequest request)
    {
        var query = from os in _repository.GetQueryable().AsNoTracking()
            join c in _clienteRepository.GetQueryable().AsNoTracking() on os.IdCliente equals c.Id
            join v in _clienteVeiculoRepository.GetQueryable().AsNoTracking() on os.IdVeiculo equals v.Id
            orderby os.DataCriacao
            select new OrdemServicoResponse(
                os.Id,
                new OrdemServicoClienteResponse(
                    c.Id,
                    c.Nome,
                    c.Documento
                ),
                new OrdemServicoVeiculoResponse(
                    v.Id,
                    v.Placa,
                    v.Marca,
                    v.Modelo,
                    v.Ano
                ),
                os.DataCriacao,
                os.DataInicioExecucao,
                os.DataFinalizacao,
                os.Status.ToString(),
                os.ValorTotal,
                os.Servicos.Select(s => new OrdemServicoServicoResponse(
                    s.Id,
                    s.IdServico,
                    s.NomeServico,
                    s.Valor,
                    s.Status.ToString()
                )).ToList(),
                os.Produtos.Select(p => new OrdemServicoProdutoResponse(
                    p.Id,
                    p.IdProduto,
                    p.NomeProduto,
                    p.ValorUnitario,
                    p.Quantidade
                )).ToList()
            );

        var total = await query.CountAsync();
        var resultado = await query
            .Skip((request.Pagina - 1) * request.Tamanho)
            .Take(request.Tamanho)
            .ToListAsync();

        return new PagedResponse<OrdemServicoResponse>(resultado, total, request.Pagina, request.Tamanho);
    }

    public async Task<PagedResponse<OrdemServicoPorDocumentoResponse>> BuscarListaPaginadaPorDocumento(string documento, PagedRequest request)
    {
        var documentoLimpo = new string(documento.Where(char.IsDigit).ToArray());

        var query = from os in _repository.GetQueryable().AsNoTracking()
            join c in _clienteRepository.GetQueryable().AsNoTracking().Where(l => l.Documento == documentoLimpo) on
                os.IdCliente equals c.Id
            join v in _clienteVeiculoRepository.GetQueryable().AsNoTracking() on os.IdVeiculo equals v.Id
            orderby os.DataCriacao
            select new OrdemServicoPorDocumentoResponse(
                os.Status.ToString(),
                new OrdemServicoClientePorDocumentoResponse(
                    c.Nome,
                    c.Documento
                ),
                new OrdemServicoVeiculoPorDocumentoResponse(
                    v.Placa,
                    v.Marca,
                    v.Modelo,
                    v.Ano
                ),
                os.Servicos.Select(l => new OrdemServicoServicoPorDocumentoResponse(l.NomeServico, l.Status.ToString())).ToList()
            );

        var total = await query.CountAsync();
        var resultado = await query
            .Skip((request.Pagina - 1) * request.Tamanho)
            .Take(request.Tamanho)
            .ToListAsync();

        return new PagedResponse<OrdemServicoPorDocumentoResponse>(resultado, total, request.Pagina, request.Tamanho);
    }

    public async Task<Guid> Inserir(InserirOrdemServicoRequest request)
    {
        await _ordemServicoValidatorService.Validar(request);

        var ordemServico = new OrdemServico();

        var servicos = new List<OrdemServicoServico>();
        if (request.IdsServicos.Any())
        {
            var dicionarioServicos = await _servicoRepository.GetQueryable()
                .AsNoTracking()
                .Where(s => request.IdsServicos.Contains(s.Id))
                .Distinct()
                .ToDictionaryAsync(s => s.Id);

            servicos = request.IdsServicos
                .Select(s =>
                {
                    var servico = dicionarioServicos[s];
                    return new OrdemServicoServico(ordemServico.Id, servico.Id, servico.Nome, servico.Valor);
                }).ToList();
        }

        ordemServico.Inserir(request.IdCliente, request.IdVeiculo, servicos);

        await _repository.InsertAsync(ordemServico);
        await _repository.SaveChangesAsync();

        return ordemServico.Id;
    }

    public async Task InserirProdutos(Guid id, InserirProdutosOrdemServicoRequest request)
    {
        var ordemServico = await _repository
            .GetQueryable()
            .AsSplitQuery()
            .Include(l => l.Produtos)
            .Include(l => l.Servicos)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (ordemServico is null) throw new NotFoundException("Ordem de serviço não encontrada.");

        var dicionarioProdutos = await _produtoRepository.GetQueryable()
            .AsNoTracking()
            .Where(l => request.Produtos.Select(l1 => l1.IdProduto).Contains(l.Id))
            .Distinct()
            .ToDictionaryAsync(s => s.Id);

        var produtosInserir = request.Produtos
            .Where(l => dicionarioProdutos.ContainsKey(l.IdProduto))
            .Select(l =>
            {
                var produto = dicionarioProdutos[l.IdProduto];
            
                if (produto.QuantidadeEmEstoque < l.Quantidade)
                {
                    throw new ConflictException(
                        $"Estoque insuficiente para o produto '{produto.Nome}'. " +
                        $"Disponível: {produto.QuantidadeEmEstoque}, solicitado: {l.Quantidade}."
                    );
                }

                return new OrdemServicoProduto(id, produto.Id, produto.Nome, produto.Valor, l.Quantidade);
            }).ToList();

        ordemServico.InserirProdutos(produtosInserir);
        await _repository.SaveChangesAsync();
    }

    public async Task InserirServicos(Guid id, InserirServicosOrdemServicoRequest request)
    {
        var ordemServico = await _repository
            .GetQueryable()
            .AsSplitQuery()
            .Include(l => l.Produtos)
            .Include(l => l.Servicos)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (ordemServico is null) 
            throw new NotFoundException("Ordem de serviço não encontrada.");

        var dicionarioServicos = await _servicoRepository.GetQueryable()
            .AsNoTracking()
            .Where(s => request.Servicos.Select(s2 => s2.IdServico).Contains(s.Id))
            .Distinct()
            .ToDictionaryAsync(s => s.Id);

        var servicosInserir = request.Servicos
            .Where(s => dicionarioServicos.ContainsKey(s.IdServico))
            .Select(s =>
            {
                var servico = dicionarioServicos[s.IdServico];
                return new OrdemServicoServico(id, servico.Id, servico.Nome, servico.Valor);
            }).ToList();

        ordemServico.InserirServicos(servicosInserir);
        await _repository.SaveChangesAsync();
    }

    public async Task IniciarDiagnostico(Guid id)
    {
        var ordemServico = await _repository.GetQueryable().FirstOrDefaultAsync(l => l.Id == id);
        if (ordemServico is null) throw new NotFoundException("Ordem de serviço não encontrada.");

        ordemServico.IniciarDiagnostico();
        await _repository.SaveChangesAsync();
    }

    public async Task FinalizarDiagnostico(Guid id)
    {
        var ordemServico = await _repository
            .GetQueryable()
            .Include(l => l.Produtos)
            .Include(l => l.Servicos)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (ordemServico is null) throw new NotFoundException("Ordem de serviço não encontrada.");

        ordemServico.FinalizarDiagnostico();
        await _repository.SaveChangesAsync();
    }

    public async Task AprovarOrcamento(Guid id)
    {
        var ordemServico = await _repository.GetQueryable().FirstOrDefaultAsync(l => l.Id == id);
        if (ordemServico is null) throw new NotFoundException("Ordem de serviço não encontrada.");

        ordemServico.AprovarOrcamento();
        await _repository.SaveChangesAsync();
    }

    public async Task IniciarExecucaoServico(Guid id, Guid idServico)
    {
        var ordemServico = await _repository
            .GetQueryable()
            .Include(l => l.Servicos)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (ordemServico is null) throw new NotFoundException("Ordem de serviço não encontrada.");

        ordemServico.IniciarExecucaoServico(idServico);
        await _repository.SaveChangesAsync();
    }

    public async Task FinalizarExecucaoServico(Guid id, Guid idServico)
    {
        var ordemServico = await _repository.GetQueryable()
            .AsSplitQuery()
            .Include(l => l.Servicos)
            .Include(l => l.Produtos)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (ordemServico is null) throw new NotFoundException("Ordem de serviço não encontrada.");

        ordemServico.FinalizarExecucaoServico(idServico);
        await _repository.SaveChangesAsync();
    }

    public async Task Entregar(Guid id)
    {
        var ordemServico = await _repository.GetQueryable().FirstOrDefaultAsync(l => l.Id == id);
        if (ordemServico is null) throw new NotFoundException("Ordem de serviço não encontrada.");

        ordemServico.Entregar();
        await _repository.SaveChangesAsync();
    }

    public async Task Remover(Guid id)
    {
        var ordemServico = await _repository.GetQueryable().FirstOrDefaultAsync(l => l.Id == id);
        if (ordemServico is null) throw new NotFoundException("Ordem de serviço não encontrada.");

        await _repository.DeleteAsync(ordemServico);
        await _repository.SaveChangesAsync();
    }

    public async Task RemoverProduto(Guid id, Guid idProduto)
    {
        var ordemServico = await _repository
            .GetQueryable()
            .AsSplitQuery()
            .Include(l => l.Produtos)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (ordemServico is null) throw new NotFoundException("Ordem de serviço não encontrada.");

        ordemServico.RemoverProduto(idProduto);
        await _repository.SaveChangesAsync();
    }

    public async Task RemoverServico(Guid id, Guid idServico)
    {
        var ordemServico = await _repository
            .GetQueryable()
            .AsSplitQuery()
            .Include(l => l.Servicos)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (ordemServico is null) throw new NotFoundException("Ordem de serviço não encontrada.");

        ordemServico.RemoverServico(idServico);

        await _repository.SaveChangesAsync();
    }
}