using Microsoft.EntityFrameworkCore;
using SoatTechChallenge.Domain.Servicos;
using SoatTechChallenge.Infrastucture.Database;

namespace SoatTechChallenge.Infrastucture.Seeders.Servicos;

public static class ServicoSeeder
{
    public static async Task SeedAsync(SoatTechChallengeDbContext context)
    {
        if (await context.Servico.AnyAsync()) return;

        var servicos = new List<Servico>
        {
            CriarServico("Troca de Óleo", "Substituição do óleo do motor", 120),
            CriarServico("Alinhamento", "Alinhamento de direção", 90),
            CriarServico("Balanceamento", "Balanceamento das rodas", 80),
            CriarServico("Troca de Pastilhas de Freio", "Substituição das pastilhas de freio", 150),
            CriarServico("Diagnóstico Eletrônico", "Leitura de falhas via scanner", 100)
        };

        await context.Servico.AddRangeAsync(servicos);
        await context.SaveChangesAsync();
    }

    private static Servico CriarServico(string nome, string descricao, decimal valor)
    {
        var servico = new Servico();
        servico.Inserir(nome, descricao, valor);
        return servico;
    }
}