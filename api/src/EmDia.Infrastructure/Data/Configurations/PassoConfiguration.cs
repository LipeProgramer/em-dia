using EmDia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmDia.Infrastructure.Data.Configurations;

public class PassoConfiguration : IEntityTypeConfiguration<Passo>
{
    public void Configure(EntityTypeBuilder<Passo> b)
    {
        b.ToTable("passos");
        b.HasKey(p => p.Id);
        b.Property(p => p.Id).HasColumnName("id");
        b.Property(p => p.ItemId).HasColumnName("item_id").IsRequired();
        b.Property(p => p.Ordem).HasColumnName("ordem").IsRequired();
        b.Property(p => p.Descricao).HasColumnName("descricao").HasMaxLength(120).IsRequired();
        b.Property(p => p.Concluido).HasColumnName("concluido").HasDefaultValue(false).IsRequired();
        b.Property(p => p.ConcluidoEm).HasColumnName("concluido_em");

        // UK (item_id, ordem) spec §2.3
        b.HasIndex(p => new { p.ItemId, p.Ordem }).IsUnique()
         .HasDatabaseName("ix_passos_item_ordem");
    }
}
