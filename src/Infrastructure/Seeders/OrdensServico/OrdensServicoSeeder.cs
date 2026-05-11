using Domain.Clientes;
using Domain.OrdensServico;
using Domain.OrdensServico.Produtos;
using Domain.OrdensServico.Servicos;
using Domain.Produtos;
using Domain.Servicos;
using Microsoft.EntityFrameworkCore;
using SoatTechChallenge.Infrastucture.Database;

namespace SoatTechChallenge.Infrastucture.Seeders.OrdensServico;

public static class OrdensServicoSeeder
{
    public static async Task SeedAsync(SoatTechChallengeDbContext context)
    {
        if (await context.OrdemServico.AsNoTracking().AnyAsync()) return;

        var clientes = await context.Cliente.Include(l => l.Veiculos).Take(3).ToListAsync();
        var produtos = await context.Produto.Take(3).ToListAsync();
        var servicos = await context.Servico.Take(3).ToListAsync();
        
        var ordens = new List<OrdemServico>
        {
            CriarOrdem(clientes[0], produtos, servicos),
            CriarOrdem(clientes[1], produtos, servicos),
            CriarOrdem(clientes[2], produtos, servicos)
        };

        await context.OrdemServico.AddRangeAsync(ordens);
        await context.SaveChangesAsync();
    }

    private static OrdemServico CriarOrdem(Cliente cliente, List<Produto> produtos, List<Servico> servicos)
    {
        var ordem = new OrdemServico();
        ordem.Inserir(cliente.Id, cliente.Veiculos[0].Id, new List<OrdemServicoServico>());
        ordem.IniciarDiagnostico();
        
        var servicosInserir = servicos.Select(l => new OrdemServicoServico(ordem.Id, l.Id, l.Nome, l.Valor)).ToList();
        servicosInserir.ForEach(l => l.IniciarExecucao());
        servicosInserir.ForEach(l => l.FinalizarExecucao());
        ordem.InserirServicos(servicosInserir);

        var produtosInserir = produtos.Select(l => new OrdemServicoProduto(ordem.Id, l.Id, l.Nome, l.Valor, 100)).ToList();
        ordem.InserirProdutos(produtosInserir);
        
        return ordem;
    }
}