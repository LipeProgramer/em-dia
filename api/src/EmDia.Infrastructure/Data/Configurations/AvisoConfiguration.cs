using EmDia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmDia.Infrastructure.Data.Configurations;

public class AvisoConfiguration : IEntityTypeConfiguration<Aviso>
{
    public void Configure(EntityTypeBuilder<Aviso> b)
    {
        b.ToTable("avisos");
        b.HasKey(a => a.Id);
        b.Property(a => a.Id).HasColumnName("id");
        b.Property(a => a.ItemId).HasColumnName("item_id").IsRequired();
        b.Property(a => a.Tipo).HasColumnName("tipo").IsRequired();
        b.Property(a => a.Canal).HasColumnName("canal").IsRequired();
        b.Property(a => a.VencimentoReferencia).HasColumnName("vencimento_referencia").IsRequired();
        b.Property(a => a.Numero).HasColumnName("numero").IsRequired();
        b.Property(a => a.EnviadoEm).HasColumnName("enviado_em").IsRequired();
        b.Property(a => a.Sucesso).HasColumnName("sucesso").IsRequired();
        b.Property(a => a.Erro).HasColumnName("erro").HasMaxLength(300);

        // UK (item_id, vencimento_referencia, numero) — evita aviso duplicado (spec §2.5)
        b.HasIndex(a => new { a.ItemId, a.VencimentoReferencia, a.Numero }).IsUnique()
         .HasDatabaseName("ix_avisos_uk");

        // Índice para controle de reavisos (spec §2.5)
        b.HasIndex(a => new { a.ItemId, a.VencimentoReferencia })
         .HasDatabaseName("ix_avisos_item_vencimento");
    }
}
