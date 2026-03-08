using Microsoft.EntityFrameworkCore;
using SoatTechChallenge.Domain.OrdensServico.Enums;
using SoatTechChallenge.Domain.OrdensServico.Produtos;
using SoatTechChallenge.Domain.OrdensServico.Services.Validators;
using SoatTechChallenge.Domain.OrdensServico.Servicos;
using SoatTechChallenge.Domain.Produtos;
using SoatTechChallenge.Domain.Servicos;
using SoatTechChallenge.Host.Controllers.OrdensServico.DTOs.Requests;
using SoatTechChallenge.Host.Middlewares.Exceptions;
using SoatTechChallenge.Infrastructure.Interfaces;

namespace SoatTechChallenge.Domain.OrdensServico;

public class OrdemServico
{
    public Guid Id { get; private set; }
    public Guid IdCliente { get; private set; }
    public Guid IdVeiculo { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public DateTime? DataInicioExecucao { get; private set; }
    public DateTime? DataFinalizacao { get; private set; }
    public StatusOrdemServico Status { get; private set; }
    public decimal ValorTotal { get; private set; }
    public List<OrdemServicoServico> Servicos { get; init; } = new();
    public List<OrdemServicoProduto> Produtos { get; init; } = new();

    public OrdemServico() { }

    public async Task Inserir(
        InserirOrdemServicoRequest request,
        IOrdemServicoValidatorService validatorService,
        IRepository<Produto> produtoRepository,
        IRepository<Servico> servicoRepository)
    {
        await validatorService.Validar(request);
        
        Id = Guid.NewGuid();
        IdCliente = request.IdCliente;
        IdVeiculo = request.IdVeiculo;
        Status = StatusOrdemServico.Recebida;
        DataCriacao = DateTime.UtcNow;

        var dicionarioServicos = servicoRepository.Query().AsNoTracking()
            .Where(s => request.Servicos.Select(rs => rs.IdServico).Contains(s.Id))
            .ToDictionary(s => s.Id);

        foreach (var requestServico in request.Servicos)
        {
            if (dicionarioServicos.TryGetValue(requestServico.IdServico, out var servico))
            {
                Servicos.Add(new OrdemServicoServico(Id, servico.Id, servico.Nome, servico.Valor));
            }
        }
     
        var dicionarioProdutos = produtoRepository.Query().AsNoTracking()
            .Where(p => request.Produtos.Select(rp => rp.IdProduto).Contains(p.Id))
            .ToDictionary(s => s.Id);

        foreach (var requestProduto in request.Produtos)
        {
            if (dicionarioProdutos.TryGetValue(requestProduto.IdProduto, out var produto))
            {
                Produtos.Add(new OrdemServicoProduto(Id, produto.Id, produto.Nome, requestProduto.Quantidade, produto.Valor));
            }
        }
        
        CalcularTotal();
    }

    public void IniciarDiagnostico()
    {
        if (Status != StatusOrdemServico.Recebida)
        {
            throw new DomainException("O diagnóstico só ser iniciado após recebimento.");
        }

        Status = StatusOrdemServico.EmDiagnostico;
    }

    public void EnviarOrcamento()
    {
        if (Status != StatusOrdemServico.EmDiagnostico)
        {
            throw new DomainException("O orçamento só pode ser enviado após diagnóstico.");
        }
        
        Status = StatusOrdemServico.AguardandoAprovacao;
    }

    public void AprovarOrcamento()
    {
        if (Status != StatusOrdemServico.AguardandoAprovacao)
        {
            throw new DomainException("O orçamento não está aguardando aprovação.");
        }
        
        DataInicioExecucao = DateTime.UtcNow;
        Status = StatusOrdemServico.EmExecucao;
    }
    
    public void IniciarExecucao()
    {
        if (Status != StatusOrdemServico.Recebida &&
            Status != StatusOrdemServico.AguardandoAprovacao)
        {
            throw new DomainException("A execução não pode ser iniciada neste status.");
        }

        DataInicioExecucao = DateTime.UtcNow;
        Status = StatusOrdemServico.EmExecucao;
    }

    public void FinalizarServico()
    {
        if (Status != StatusOrdemServico.EmExecucao)
        {
            throw new DomainException("O serviço não está em execução.");
        }
       
        DataFinalizacao = DateTime.UtcNow;
        Status = StatusOrdemServico.Finalizada;
    }

    public void Entregar()
    {
        if (Status != StatusOrdemServico.Finalizada)
        {
            throw new DomainException("A entrega só pode ocorrer após finalização.");
        }
       
        Status = StatusOrdemServico.Entregue;
    }
    
    private void CalcularTotal()
    {
        ValorTotal = Servicos.Sum(s => s.Valor) + Produtos.Sum(p => p.Subtotal);
    }
}