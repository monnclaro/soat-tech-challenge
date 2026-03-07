using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoatTechChallenge.Clientes;
using SoatTechChallenge.Domain.Clientes;
using SoatTechChallenge.Domain.Servicos;

namespace SoatTechChallenge.Infrastructure.Database.Configurations;

public class ServicoConfiguration : IEntityTypeConfiguration<Servico>
{
    public void Configure(EntityTypeBuilder<Servico> builder)
    {
        builder.ToTable("servicos");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        
        builder.Property(x => x.Nome)
            .HasColumnName("nome")
               .IsRequired()
               .HasMaxLength(150);

        builder.Property(x => x.Descricao)
            .HasColumnName("descricao")
            .HasMaxLength(500);

        builder.Property(x => x.Preco)
            .HasColumnName("preco")
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(x => x.TempoEstimadoMinutos)
            .HasColumnName("tempo_estimado_minutos")
            .IsRequired();
    }
}