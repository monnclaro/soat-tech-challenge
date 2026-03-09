using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SoatTechChallenge.Domain.Clientes;
using SoatTechChallenge.Domain.Clientes.Veiculos;
using SoatTechChallenge.Domain.OrdensServico.Services.Validators;
using SoatTechChallenge.Domain.Produtos;
using SoatTechChallenge.Domain.Servicos;
using SoatTechChallenge.Host.Common.DTOs;
using SoatTechChallenge.Host.Common.Services;
using SoatTechChallenge.Host.Controllers.OrdensServico.DTOs.Requests;
using SoatTechChallenge.Host.Controllers.OrdensServico.DTOs.Responses;
using SoatTechChallenge.Host.Middlewares.Exceptions;
using SoatTechChallenge.Infrastructure.Database;
using SoatTechChallenge.Infrastructure.Interfaces;

namespace SoatTechChallenge.Domain.OrdensServico.Services;

public class OrdemServicoService : IOrdemServicoService, IScopedService
{
    private readonly IRepository<OrdemServico> _repository;
    private readonly IOrdemServicoValidatorService _ordemServicoValidatorService;
    private readonly SoatTechChallengeDbContext _context;
    private readonly IRepository<Cliente> _clienteRepository;
    private readonly IRepository<ClienteVeiculo> _clienteVeiculoRepository;
    private readonly IRepository<Produto> _produtoRepository;
    private readonly IRepository<Servico> _servicoRepository;

    public OrdemServicoService(
        IRepository<OrdemServico> repository, 
        IOrdemServicoValidatorService ordemServicoValidatorService, 
        SoatTechChallengeDbContext context,
        IRepository<Cliente> clienteRepository,
        IRepository<ClienteVeiculo> clienteVeiculoRepository,
        IRepository<Produto> produtoRepository,
        IRepository<Servico> servicoRepository)
    {
        _repository = repository;
        _ordemServicoValidatorService = ordemServicoValidatorService;
        _context = context;
        _clienteRepository = clienteRepository;
        _clienteVeiculoRepository = clienteVeiculoRepository;
        _produtoRepository = produtoRepository;
        _servicoRepository = servicoRepository;
    }

    public async Task<OrdemServicoResponse?> Buscar(Guid id)
    {
        var query = from os in _repository.Query().AsNoTracking()
                    .Where(l => l.Id == id)
                    .Include(l => l.Produtos)
                    .Include(l => l.Servicos).AsSplitQuery()
                join c in _clienteRepository.Query().AsNoTracking() on os.IdCliente equals c.Id
                join v in _clienteVeiculoRepository.Query().AsNoTracking() on os.IdVeiculo equals v.Id
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
                        s.Valor
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

    public async Task<PagedResponse<OrdemServicoResponse>> BuscarListaPaginada(PagedRequest request)
    {
        var query = from os in _repository.Query().AsNoTracking()
                .Include(l => l.Produtos)
                .Include(l => l.Servicos).AsSplitQuery()
            join c in _clienteRepository.Query().AsNoTracking() on os.IdCliente equals c.Id
            join v in _clienteVeiculoRepository.Query().AsNoTracking() on os.IdVeiculo equals v.Id
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
                    s.Valor
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
        var documentoLimpo =  Regex.Replace(documento, @"\D", string.Empty);
        
        var query = from os in _repository.Query().AsNoTracking()
                .Include(l => l.Produtos)
                .Include(l => l.Servicos).AsSplitQuery()
            join c in _clienteRepository.Query().AsNoTracking().Where(l => l.Documento == documentoLimpo) on os.IdCliente equals c.Id
            join v in _clienteVeiculoRepository.Query().AsNoTracking() on os.IdVeiculo equals v.Id
            orderby os.DataCriacao
            select new OrdemServicoPorDocumentoResponse(
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
                os.DataCriacao,
                os.DataInicioExecucao,
                os.DataFinalizacao,
                os.Status.ToString(),
                os.ValorTotal,
                os.Servicos.Select(s => new OrdemServicoServicoPorDocumentoResponse(
                    s.NomeServico,
                    s.Valor
                )).ToList(),
                os.Produtos.Select(p => new OrdemServicoProdutoPorDocumentoResponse(
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

        return new PagedResponse<OrdemServicoPorDocumentoResponse>(resultado, total, request.Pagina, request.Tamanho);
    }

    public async Task<TempoMedioExecucaoOrdensServicoResponse?> BuscarTempoMedioExecucao()
    {
        var resultado = await _repository.Query().AsNoTracking()
            .Where(o => o.DataInicioExecucao != null && o.DataFinalizacao != null)
            .Select(o => new
            {
                Duracao = (o.DataFinalizacao!.Value - o.DataInicioExecucao!.Value).TotalMinutes
            })
            .ToListAsync();

        if (!resultado.Any()) return null;

        return new TempoMedioExecucaoOrdensServicoResponse
        {
            TempoMedioMinutos = resultado.Average(x => x.Duracao),
            TempoMinimoMinutos = resultado.Min(x => x.Duracao),
            TempoMaximoMinutos = resultado.Max(x => x.Duracao),
            TotalOrdens = resultado.Count
        };
    }

    public async Task<OrdemServicoOrcamentoResponse> Inserir(InserirOrdemServicoRequest request)
    {
        var ordemServico = new OrdemServico();
        await ordemServico.Inserir(request, _ordemServicoValidatorService, _produtoRepository, _servicoRepository);

        await _repository.AddAsync(ordemServico);
        await _repository.SaveChangesAsync();

        return new OrdemServicoOrcamentoResponse(ordemServico.Id, ordemServico.ValorTotal);
    }

    public async Task<OrdemServicoOrcamentoResponse> Atualizar(Guid id, AtualizarOrdemServicoRequest request)
    {
        var ordemServico = await _repository.Query()
            .Include(l => l.Produtos)
            .Include(l => l.Servicos).AsSplitQuery()
            .FirstOrDefaultAsync(l => l.Id == id);
        
        if (ordemServico is null) throw new NotFoundException($"Ordem de serviço de id: {id} não encontrado.");

        await ordemServico.Atualizar(request, _produtoRepository, _servicoRepository);
        
        _repository.Update(ordemServico);
        await _repository.SaveChangesAsync();
        
        return new OrdemServicoOrcamentoResponse(ordemServico.Id, ordemServico.ValorTotal);
    }

    public async Task IniciarDiagnostico(Guid id)
    {
        var ordemServico = await _repository.Query().FirstOrDefaultAsync(l => l.Id == id);
        if (ordemServico is null) throw new NotFoundException($"Ordem de serviço de id: {id} não encontrado.");

        ordemServico.IniciarDiagnostico();
        
        _repository.Update(ordemServico);
        await _repository.SaveChangesAsync();
    }

    public async Task<OrdemServicoOrcamentoResponse> EnviarOrcamento(Guid id)
    {
        var ordemServico = await _repository.Query()
            .Include(l => l.Produtos)
            .Include(l => l.Servicos).AsSplitQuery()
            .FirstOrDefaultAsync(l => l.Id == id);
        
        if (ordemServico is null) throw new NotFoundException($"Ordem de serviço de id: {id} não encontrado.");
        
        ordemServico.EnviarOrcamento();
        
        _repository.Update(ordemServico);
        await _repository.SaveChangesAsync();
        
        return new OrdemServicoOrcamentoResponse(ordemServico.Id, ordemServico.ValorTotal);
    }

    public async Task AprovarOrcamento(Guid id)
    {
        var ordemServico = await _repository.Query().FirstOrDefaultAsync(l => l.Id == id);
        if (ordemServico is null) throw new NotFoundException($"Ordem de serviço de id: {id} não encontrado.");
        
        ordemServico.AprovarOrcamento();
        
        _repository.Update(ordemServico);
        await _repository.SaveChangesAsync();
    }

    public async Task Finalizar(Guid id)
    {
        var ordemServico = await _repository.Query().FirstOrDefaultAsync(l => l.Id == id);
        if (ordemServico is null) throw new NotFoundException($"Ordem de serviço de id: {id} não encontrado.");
        
        ordemServico.FinalizarServico();
        
        _repository.Update(ordemServico);
        await _repository.SaveChangesAsync();
    }

    public async Task Entregar(Guid id)
    {
        var ordemServico = await _repository.Query()
            .Include(l => l.Produtos)
            .FirstOrDefaultAsync(l => l.Id == id);
        
        if (ordemServico is null) throw new NotFoundException($"Ordem de serviço de id: {id} não encontrado.");
        
        ordemServico.Entregar();
        
        var dicionarioProdutos = await _produtoRepository.Query()
            .Where(p => ordemServico.Produtos.Select(l => l.IdProduto).Contains(p.Id))
            .ToDictionaryAsync(l => l.Id);

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var item in ordemServico.Produtos)
            {
                if (dicionarioProdutos.TryGetValue(item.IdProduto, out var produto))
                {
                    produto.DecrementarQuantidadeEmEstoque(item.Quantidade);
                }
            }
           
            await _repository.SaveChangesAsync(); 
            await _produtoRepository.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task Remover(Guid id)
    {
        var ordemServico = await _repository.Query().FirstOrDefaultAsync(l => l.Id == id);
        if (ordemServico is null) throw new NotFoundException($"Ordem de serviço de id: {id} não encontrado.");

        _repository.Delete(ordemServico);
        await _repository.SaveChangesAsync();
    }
}