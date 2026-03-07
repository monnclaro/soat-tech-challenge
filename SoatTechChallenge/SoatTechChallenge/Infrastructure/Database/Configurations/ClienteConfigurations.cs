using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoatTechChallenge.Clientes;

namespace SoatTechChallenge.Infrastructure.Database.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("clientes");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");

        builder.Property(c => c.Nome)
               .HasColumnName("nome")
               .HasMaxLength(150)
               .IsRequired();
        
        builder.HasIndex(c => c.Documento).IsUnique();
        builder.Property(c => c.Documento)
               .HasColumnName("documento")
               .HasMaxLength(14)
               .IsRequired();

        builder.Property(c => c.TipoDocumento)
               .HasColumnName("tipo_documento")
               .HasConversion<string>()
               .HasMaxLength(4)
               .IsRequired();

        builder.Property(c => c.DataCriacao)
               .HasColumnName("data_criacao")
               .IsRequired();

        builder.HasMany(c => c.Veiculos)
               .WithOne()
               .HasForeignKey(v => v.IdCliente)
               .OnDelete(DeleteBehavior.Cascade);
    }
}