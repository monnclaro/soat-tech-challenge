using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoatTechChallenge.Clientes.Veiculos;

namespace SoatTechChallenge.Infrastructure.Database.Configurations;

public class VeiculoConfiguration : IEntityTypeConfiguration<ClienteVeiculo>
{
    public void Configure(EntityTypeBuilder<ClienteVeiculo> builder)
    {
        builder.ToTable("clienteveiculos");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id).HasColumnName("id");
        builder.Property(v => v.IdCliente).HasColumnName("cliente_id");

        builder.Property(v => v.Placa)
            .HasColumnName("placa")
            .HasMaxLength(8)
            .IsRequired();

        builder.HasIndex(v => v.Placa).IsUnique();
        builder.Property(v => v.Marca).HasColumnName("marca").HasMaxLength(50).IsRequired();
        builder.Property(v => v.Modelo).HasColumnName("modelo").HasMaxLength(80).IsRequired();
        builder.Property(v => v.Ano).HasColumnName("ano").IsRequired();
    }
}