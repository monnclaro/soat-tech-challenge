using Domain.Usuarios;
using Domain.Usuarios.Roles;
using Microsoft.EntityFrameworkCore;
using SoatTechChallenge.Infrastucture.Database;

namespace SoatTechChallenge.Infrastucture.Seeders.Usuarios;

public static class UsuarioSeeder
{
    public static async Task SeedAsync(SoatTechChallengeDbContext context)
    {
        if (await context.Usuario.AnyAsync()) return;

        var adminUser = new Usuario("Admin", "admin@gmail.com", BCrypt.Net.BCrypt.HashPassword("123"), "52998224725");
        adminUser.AdicionarRoles(new List<UsuarioRole>() { new("Admin") });

        context.Usuario.Add(adminUser);
        await context.SaveChangesAsync();
    }
}