using Microsoft.EntityFrameworkCore;
using SoatTechChallenge.Domain.Usuarios;
using SoatTechChallenge.Domain.Usuarios.Roles;
using SoatTechChallenge.Infrastucture.Database;

namespace SoatTechChallenge.Infrastucture.Seeders.Usuarios;

public static class UsuarioSeeder
{
    public static async Task SeedAsync(SoatTechChallengeDbContext context)
    {
        if (await context.Usuario.AnyAsync()) return;

        var adminUser = new Usuario("Admin", BCrypt.Net.BCrypt.HashPassword("123456"), new List<UsuarioRole>() { new ("Admin") });
        context.Usuario.Add(adminUser);
        
        await context.SaveChangesAsync();
    }
}