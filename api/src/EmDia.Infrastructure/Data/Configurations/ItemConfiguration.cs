using EmDia.Domain.Entities;
using EmDia.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmDia.Infrastructure.Data.Configurations;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> b)
    {
        b.ToTable("itens");
        b.HasKey(i => i.Id);
        b.Property(i => i.Id).HasColumnName("id");
        b.Property(i => i.UsuarioId).HasColumnName("usuario_id").IsRequired();
        b.Property(i => i.CategoriaId).HasColumnName("categoria_id").IsRequired();
        b.Property(i => i.ModeloItemId).HasColumnName("modelo_item_id");
        b.Property(i => i.VeiculoId).HasColumnName("veiculo_id");
        b.Property(i => i.Nome).HasColumnName("nome").HasMaxLength(80).IsRequired();
        b.Property(i => i.TipoVencimento).HasColumnName("tipo_vencimento").IsRequired();
        b.Property(i => i.DataBase).HasColumnName("data_base");
        b.Property(i => i.DataVencimento).HasColumnName("data_vencimento");
        b.Property(i => i.IntervaloDias).HasColumnName("intervalo_dias");
        b.Property(i => i.IntervaloKm).HasColumnName("intervalo_km");
        b.Property(i => i.KmBase).HasColumnName("km_base");
        b.Property(i => i.GarantiaFabricanteMeses).HasColumnName("garantia_fabricante_meses");
        b.Property(i => i.AntecedenciaDias).HasColumnName("antecedencia_dias").HasDefaultValue((short)30).IsRequired();
        b.Property(i => i.ProximoVencimentoData).HasColumnName("proximo_vencimento_data");
        b.Property(i => i.ProximoVencimentoKm).HasColumnName("proximo_vencimento_km");
        b.Property(i => i.Situacao).HasColumnName("situacao").HasDefaultValue(SituacaoItem.Ativo).IsRequired();
        b.Property(i => i.Observacao).HasColumnName("observacao").HasMaxLength(500);
        b.Property(i => i.CriadoEm).HasColumnName("criado_em").IsRequired();
        b.Property(i => i.AtualizadoEm).HasColumnName("atualizado_em").IsRequired();

        // CHECKs por tipo de vencimento (spec §2.5)
        b.ToTable(t =>
        {
            t.HasCheckConstraint("ck_item_antecedencia", "antecedencia_dias BETWEEN 0 AND 365");
            t.HasCheckConstraint("ck_item_km_base", "km_base IS NULL OR km_base >= 0");
            t.HasCheckConstraint("ck_item_intervalo_dias", "intervalo_dias IS NULL OR intervalo_dias > 0");
            t.HasCheckConstraint("ck_item_intervalo_km", "intervalo_km IS NULL OR intervalo_km > 0");
            t.HasCheckConstraint("ck_item_garantia", "garantia_fabricante_meses IS NULL OR garantia_fabricante_meses > 0");
            // Restrições por tipo
            t.HasCheckConstraint("ck_item_data_unica",
                "tipo_vencimento <> 1 OR data_vencimento IS NOT NULL");
            t.HasCheckConstraint("ck_item_recorrente",
                "tipo_vencimento <> 2 OR (data_base IS NOT NULL AND intervalo_dias IS NOT NULL)");
            t.HasCheckConstraint("ck_item_uso_tempo",
                "tipo_vencimento <> 3 OR (veiculo_id IS NOT NULL AND data_base IS NOT NULL AND km_base IS NOT NULL AND intervalo_km IS NOT NULL)");
            t.HasCheckConstraint("ck_item_calculado",
                "tipo_vencimento <> 4 OR (data_base IS NOT NULL AND garantia_fabricante_meses IS NOT NULL)");
        });

        // Índice principal do Radar e da rotina de avisos (spec §2.5)
        b.HasIndex(i => new { i.UsuarioId, i.Situacao, i.ProximoVencimentoData })
         .HasDatabaseName("ix_itens_radar");

        b.HasIndex(i => i.UsuarioId).HasDatabaseName("ix_itens_usuario");
        b.HasIndex(i => i.VeiculoId).HasDatabaseName("ix_itens_veiculo");

        // Relacionamentos
        b.HasMany(i => i.Passos)
         .WithOne()
         .HasForeignKey(p => p.ItemId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(i => i.Historicos)
         .WithOne(h => h.Item)
         .HasForeignKey(h => h.ItemId)
         .OnDelete(DeleteBehavior.Cascade);

        // veiculo_id: RESTRICT (não exclui veículo com itens — RN13)
        b.HasOne<Veiculo>()
         .WithMany()
         .HasForeignKey(i => i.VeiculoId)
         .OnDelete(DeleteBehavior.Restrict)
         .IsRequired(false);

        // modelo_item_id: SET NULL ao excluir modelo
        b.HasOne<ModeloItem>()
         .WithMany()
         .HasForeignKey(i => i.ModeloItemId)
         .OnDelete(DeleteBehavior.SetNull)
         .IsRequired(false);
    }
}
