using Domain.Clientes;
using Domain.Clientes.Enums;
using Domain.Clientes.Veiculos;
using Microsoft.EntityFrameworkCore;
using SoatTechChallenge.Infrastucture.Database;

namespace SoatTechChallenge.Infrastucture.Seeders.Clientes;

public static class ClienteSeeder
{
    public static async Task SeedAsync(SoatTechChallengeDbContext context)
    {
        if (await context.Cliente.AnyAsync()) return;

        var cliente1 = CriarCliente(
            "João Silva",
            "12345678909",
            TipoDocumentoCliente.Cpf,
            new List<Veiculo>
            {
                CriarVeiculo("ABC1234", "Toyota", "Corolla", 2019),
                CriarVeiculo("BRA2E19", "Honda", "Civic", 2021)
            });

        var cliente2 = CriarCliente(
            "Maria Oliveira",
            "98765432100",
            TipoDocumentoCliente.Cpf,
            new List<Veiculo>
            {
                CriarVeiculo("DEF5678", "Volkswagen", "Golf", 2018)
            });

        var cliente3 = CriarCliente(
            "Oficina Mecânica Brasil LTDA",
            "11222333000181",
            TipoDocumentoCliente.Cnpj,
            new List<Veiculo>
            {
                CriarVeiculo("DCF5678", "Volkswagen", "Golf", 2014)
            });

        await context.Cliente.AddRangeAsync(cliente1, cliente2, cliente3);
        await context.SaveChangesAsync();
    }

    private static Cliente CriarCliente(string nome, string documento, TipoDocumentoCliente tipo, List<Veiculo> veiculos)
    {
        var cliente = new Cliente();
        cliente.Inserir(nome, documento, tipo);
        cliente.Veiculos.AddRange(veiculos);
        return cliente;
    }

    private static Veiculo CriarVeiculo(string placa, string marca, string modelo, int ano)
    {
        var veiculo = new Veiculo();
        veiculo.Inserir(Guid.Empty, placa, marca, modelo, ano);
        return veiculo;
    }
}