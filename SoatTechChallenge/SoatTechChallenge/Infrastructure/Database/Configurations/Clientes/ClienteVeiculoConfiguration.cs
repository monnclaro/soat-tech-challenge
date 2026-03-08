using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoatTechChallenge.Domain.Clientes.Veiculos;

namespace SoatTechChallenge.Infrastructure.Database.Configurations.Clientes;

public class ClienteVeiculoConfiguration : IEntityTypeConfiguration<ClienteVeiculo>
{
    public void Configure(EntityTypeBuilder<ClienteVeiculo> builder)
    {
        builder.ToTable("cliente_veiculos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.IdCliente).HasColumnName("cliente_id");
        
        builder.HasIndex(x => x.Placa).IsUnique();
        builder.Property(x => x.Placa)
            .HasColumnName("placa")
            .HasMaxLength(8)
            .IsRequired();
        
        builder.Property(x => x.Marca)
            .HasColumnName("marca")
            .HasMaxLength(50)
            .IsRequired();
        
        builder.Property(x => x.Modelo)
            .HasColumnName("modelo")
            .HasMaxLength(80)
            .IsRequired();
        
        builder.Property(x => x.Ano)
            .HasColumnName("ano")
            .IsRequired();
    }
}