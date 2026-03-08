using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoatTechChallenge.Domain.Usuarios;

namespace SoatTechChallenge.Infrastructure.Database.Configurations.Usuarios;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuario");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Nome)
            .HasColumnName("nome")
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.SenhaHash)
            .HasColumnName("senha_hash")
            .IsRequired()
            .HasMaxLength(255);

        builder.HasMany(x => x.Roles)
            .WithOne()
            .HasForeignKey(x => x.IdUsuario);
    }
}