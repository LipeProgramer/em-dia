using EmDia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmDia.Infrastructure.Data.Configurations;

public class HistoricoConfiguration : IEntityTypeConfiguration<Historico>
{
    public void Configure(EntityTypeBuilder<Historico> b)
    {
        b.ToTable("historicos");
        b.HasKey(h => h.Id);
        b.Property(h => h.Id).HasColumnName("id");
        b.Property(h => h.ItemId).HasColumnName("item_id").IsRequired();
        b.Property(h => h.ConcluidoEm).HasColumnName("concluido_em").IsRequired();
        b.Property(h => h.KmNaConclusao).HasColumnName("km_na_conclusao");
        b.Property(h => h.ValorGasto).HasColumnName("valor_gasto").HasColumnType("numeric(10,2)");
        b.Property(h => h.VencimentoAnterior).HasColumnName("vencimento_anterior");
        b.Property(h => h.ProximoVencimento).HasColumnName("proximo_vencimento");
        b.Property(h => h.Observacao).HasColumnName("observacao").HasMaxLength(500);
        b.Property(h => h.CriadoEm).HasColumnName("criado_em").IsRequired();

        b.ToTable(t =>
        {
            t.HasCheckConstraint("ck_historico_km", "km_na_conclusao IS NULL OR km_na_conclusao >= 0");
            t.HasCheckConstraint("ck_historico_valor", "valor_gasto IS NULL OR valor_gasto >= 0");
        });

        b.HasIndex(h => h.ItemId).HasDatabaseName("ix_historicos_item");
    }
}
