using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoatTechChallenge.Domain.Produtos;

namespace SoatTechChallenge.Infrastructure.Database.Configurations;

public class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("produtos");

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

        builder.Property(x => x.QuantidadeEmEstoque)
               .HasColumnName("quantidade_estoque")
               .IsRequired();
    }
}