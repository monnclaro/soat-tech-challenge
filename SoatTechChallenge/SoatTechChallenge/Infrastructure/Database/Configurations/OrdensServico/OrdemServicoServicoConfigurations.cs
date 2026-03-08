using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoatTechChallenge.Domain.OrdensServico;
using SoatTechChallenge.Domain.OrdensServico.Servicos;

namespace SoatTechChallenge.Infrastructure.Database.Configurations.OrdensServico;

public class OrdemServicoServicoConfiguration : IEntityTypeConfiguration<OrdemServicoServico>
{
    public void Configure(EntityTypeBuilder<OrdemServicoServico> builder)
    {
        builder.ToTable("ordem_servico_servicos");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.IdOrdemServico).HasColumnName("idordemservico").IsRequired();
        builder.Property(x => x.IdServico).HasColumnName("idservico").IsRequired();
        
        builder.Property(x => x.NomeServico)
            .HasColumnName("nomeservico")
            .HasMaxLength(150)
            .IsRequired();
        
        builder.Property(x => x.Valor)
            .HasColumnName("valorunitario")
            .HasColumnType("decimal(10,2)")
            .IsRequired();
    }
}