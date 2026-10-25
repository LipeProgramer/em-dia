using EmDia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmDia.Infrastructure.Data.Configurations;

public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> b)
    {
        b.ToTable("categorias");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(c => c.Nome).HasColumnName("nome").HasMaxLength(40).IsRequired();
        b.Property(c => c.Icone).HasColumnName("icone").HasMaxLength(40).IsRequired();
        b.Property(c => c.Ordem).HasColumnName("ordem").IsRequired();

        b.HasIndex(c => c.Nome).IsUnique()
         .HasDatabaseName("ix_categorias_nome");
    }
}
