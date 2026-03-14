using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SoatTechChallenge.Application.Common.Interfaces;
using SoatTechChallenge.Application.OrdensServico.DTOs.Requests;
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
using SoatTechChallenge.Host.Common.DTOs;
using SoatTechChallenge.Host.Controllers.OrdensServico.DTOs.Responses;

namespace SoatTechChallenge.Application.OrdensServico.Services;

public class OrdemServicoService : IOrdemServicoService, IScopedService
{
    private readonly IRepository<OrdemServico> _repository;
    private readonly IOrdemServicoValidatorService _ordemServicoValidatorService;
    private readonly IRepository<Cliente> _clienteRepository;
    private readonly IRepository<ClienteVeiculo> _clienteVeiculoRepository;
    private readonly IRepository<Produto> _produtoRepository;
    private readonly IRepository<Servico> _servicoRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public OrdemServicoService(
        IRepository<OrdemServico> repository,
        IOrdemServicoValidatorService ordemServicoValidatorService,
        IRepository<Cliente> clienteRepository,
        IRepository<ClienteVeiculo> clienteVeiculoRepository,
        IRepository<Produto> produtoRepository,
        IRepository<Servico> servicoRepository, 
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _ordemServicoValidatorService = ordemServicoValidatorService;
        _clienteRepository = clienteRepository;
        _clienteVeiculoRepository = clienteVeiculoRepository;
        _produtoRepository = produtoRepository;
        _servicoRepository = servicoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OrdemServicoResponse?> Buscar(Guid id)
    {
        var query = from os in _repository.GetQueryable().AsNoTracking()
                    .Where(l => l.Id == id)
                    .Include(l => l.Produtos)
                    .Include(l => l.Servicos)
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
        var query = from os in _repository.GetQueryable().AsNoTracking()
                .Include(l => l.Produtos)
                .Include(l => l.Servicos)
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
        var documentoLimpo = Regex.Replace(documento, @"\D", string.Empty);

        var query = from os in _repository.GetQueryable().AsNoTracking()
                .Include(l => l.Produtos)
                .Include(l => l.Servicos)
                    join c in _clienteRepository.GetQueryable().AsNoTracking().Where(l => l.Documento == documentoLimpo) on os.IdCliente equals c.Id
                    join v in _clienteVeiculoRepository.GetQueryable().AsNoTracking() on os.IdVeiculo equals v.Id
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
        var resultado = await _repository.GetQueryable().AsNoTracking()
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
    
    public async Task Inserir(InserirOrdemServicoRequest request)
    {
        await _ordemServicoValidatorService.Validar(request);

        var ordemServico = new OrdemServico();
        ordemServico.Inserir(request.IdCliente, request.IdVeiculo);

        await _repository.InsertAsync(ordemServico);
    }

    public async Task InserirProdutos(Guid id, InserirProdutosOrdemServicoRequest request)
    {
        var ordemServico = await _repository.GetQueryable().FirstOrDefaultAsync(l => l.Id == id);
        if (ordemServico is null) throw new NotFoundException($"Ordem de serviço de id: {id} não encontrado.");

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
                return new OrdemServicoProduto(id, produto.Id, produto.Nome, produto.Valor, l.Quantidade);
            }).ToList();

        ordemServico.InserirProdutos(produtosInserir);
        await _repository.UpdateAsync(ordemServico);
    }

    public async Task InserirServicos(Guid id, InserirServicosOrdemServicoRequest request)
    {
        var ordemServico = await _repository.GetQueryable().FirstOrDefaultAsync(l => l.Id == id);
        if (ordemServico is null) throw new NotFoundException($"Ordem de serviço de id: {id} não encontrado.");
      
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
        await _repository.UpdateAsync(ordemServico);
    }

    public async Task IniciarDiagnostico(Guid id)
    {
        var ordemServico = await _repository.GetQueryable().FirstOrDefaultAsync(l => l.Id == id);
        if (ordemServico is null) throw new NotFoundException($"Ordem de serviço de id: {id} não encontrado.");

        ordemServico.IniciarDiagnostico();
        await _repository.UpdateAsync(ordemServico);
   }

    public async Task<OrdemServicoOrcamentoResponse> EnviarOrcamento(Guid id)
    {
        var ordemServico = await _repository.GetQueryable()
            .Include(l => l.Produtos)
            .Include(l => l.Servicos)
            .FirstOrDefaultAsync(l => l.Id == id);
        
        if (ordemServico is null) throw new NotFoundException($"Ordem de serviço de id: {id} não encontrado.");

        ordemServico.EnviarOrcamento();
        await _repository.UpdateAsync(ordemServico);
   
        return new OrdemServicoOrcamentoResponse(ordemServico.Id, ordemServico.ValorTotal);
    }

    public async Task AprovarOrcamento(Guid id)
    {
        var ordemServico = await _repository.GetQueryable().FirstOrDefaultAsync(l => l.Id == id);
        if (ordemServico is null) throw new NotFoundException($"Ordem de serviço de id: {id} não encontrado.");

        ordemServico.AprovarOrcamento();
        await _repository.UpdateAsync(ordemServico);
   }

    public async Task Finalizar(Guid id)
    {
        var ordemServico = await _repository.GetQueryable().FirstOrDefaultAsync(l => l.Id == id);
        if (ordemServico is null) throw new NotFoundException($"Ordem de serviço de id: {id} não encontrado.");

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            ordemServico.FinalizarServico();

            var produtos = await _produtoRepository.GetQueryable()
                .Where(p => ordemServico.IdsProdutos.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            foreach (var item in ordemServico.Produtos)
            {
                if (produtos.TryGetValue(item.IdProduto, out var produto))
                {
                    produto.DecrementarQuantidadeEmEstoque(item.Quantidade);
                }
            }
        });
    }

    public async Task Entregar(Guid id)
    {
        var ordemServico = await _repository.GetQueryable().FirstOrDefaultAsync(l => l.Id == id);
        if (ordemServico is null) throw new NotFoundException($"Ordem de serviço de id: {id} não encontrado.");

        ordemServico.Entregar();
        await _repository.UpdateAsync(ordemServico);
   }

    public async Task Remover(Guid id)
    {
        var ordemServico = await _repository.GetQueryable().FirstOrDefaultAsync(l => l.Id == id);
        if (ordemServico is null) throw new NotFoundException($"Ordem de serviço de id: {id} não encontrado.");

        await _repository.DeleteAsync(ordemServico.Id);
    }
}