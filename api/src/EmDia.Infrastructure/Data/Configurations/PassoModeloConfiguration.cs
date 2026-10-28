using EmDia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmDia.Infrastructure.Data.Configurations;

public class PassoModeloConfiguration : IEntityTypeConfiguration<PassoModelo>
{
    public void Configure(EntityTypeBuilder<PassoModelo> b)
    {
        b.ToTable("passos_modelo");
        b.HasKey(p => p.Id);
        b.Property(p => p.Id).HasColumnName("id");
        b.Property(p => p.ModeloItemId).HasColumnName("modelo_item_id").IsRequired();
        b.Property(p => p.Ordem).HasColumnName("ordem").IsRequired();
        b.Property(p => p.Descricao).HasColumnName("descricao").HasMaxLength(120).IsRequired();

        // UK (modelo_item_id, ordem)
        b.HasIndex(p => new { p.ModeloItemId, p.Ordem }).IsUnique()
         .HasDatabaseName("ix_passos_modelo_ordem");
    }
}
