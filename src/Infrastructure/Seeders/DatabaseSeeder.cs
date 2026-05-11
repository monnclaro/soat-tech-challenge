using SoatTechChallenge.Infrastucture.Database;
using SoatTechChallenge.Infrastucture.Seeders.Clientes;
using SoatTechChallenge.Infrastucture.Seeders.OrdensServico;
using SoatTechChallenge.Infrastucture.Seeders.Produtos;
using SoatTechChallenge.Infrastucture.Seeders.Servicos;
using SoatTechChallenge.Infrastucture.Seeders.Usuarios;

namespace SoatTechChallenge.Infrastucture.Seeders;

public sealed class DatabaseSeeder : IDatabaseSeeder
{
    private readonly SoatTechChallengeDbContext _db;

    public DatabaseSeeder(SoatTechChallengeDbContext db)
    {
        _db = db;
    }

    public async Task SeedAsync()
    {
        await ClienteSeeder.SeedAsync(_db);
        await ProdutoSeeder.SeedAsync(_db);
        await ServicoSeeder.SeedAsync(_db);
        await UsuarioSeeder.SeedAsync(_db);
        await OrdensServicoSeeder.SeedAsync(_db);
    }
}