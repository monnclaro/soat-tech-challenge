using Domain.Usuarios;
using Domain.Usuarios.Gateways;
using Microsoft.EntityFrameworkCore;
using SoatTechChallenge.Infrastucture.Database;

namespace SoatTechChallenge.Infrastucture.Gateways.Usuarios;

public class UsuarioGateway : IUsuarioGateway
{
    private readonly SoatTechChallengeDbContext _db;

    public UsuarioGateway(SoatTechChallengeDbContext db) => _db = db;

    public Task<Usuario?> BuscarPorEmail(string email, CancellationToken ct) => _db.Usuario
            .AsNoTracking()
            .Include(u => u.Roles)
            .AsSplitQuery()
            .Where(u => u.Email == email)
            .FirstOrDefaultAsync(ct);
    
    public async Task Salvar(Usuario usuario, CancellationToken ct)
    {
        _db.Usuario.Add(usuario);
        await _db.SaveChangesAsync(ct);
    }
}