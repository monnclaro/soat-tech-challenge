using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoatTechChallenge.Clientes;
using SoatTechChallenge.Domain.Clientes;

namespace SoatTechChallenge.Infrastructure.Database.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("clientes");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Nome)
               .HasColumnName("nome")
               .HasMaxLength(150)
               .IsRequired();
        
        builder.HasIndex(x => x.Documento).IsUnique();
        builder.Property(x => x.Documento)
               .HasColumnName("documento")
               .HasMaxLength(14)
               .IsRequired();

        builder.Property(x => x.TipoDocumento)
               .HasColumnName("tipo_documento")
               .HasConversion<string>()
               .HasMaxLength(4)
               .IsRequired();

        builder.Property(x => x.DataCriacao)
               .HasColumnName("data_criacao")
               .IsRequired();

        builder.HasMany(x => x.Veiculos)
               .WithOne()
               .HasForeignKey(v => v.IdCliente)
               .OnDelete(DeleteBehavior.Cascade);
    }
}