using EmDia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmDia.Infrastructure.Data.Configurations;

public class ModeloItemConfiguration : IEntityTypeConfiguration<ModeloItem>
{
    public void Configure(EntityTypeBuilder<ModeloItem> b)
    {
        b.ToTable("modelos_item");
        b.HasKey(m => m.Id);
        b.Property(m => m.Id).HasColumnName("id");
        b.Property(m => m.CategoriaId).HasColumnName("categoria_id").IsRequired();
        b.Property(m => m.Nome).HasColumnName("nome").HasMaxLength(80).IsRequired();
        b.Property(m => m.TipoVencimento).HasColumnName("tipo_vencimento").IsRequired();
        b.Property(m => m.IntervaloDias).HasColumnName("intervalo_dias");
        b.Property(m => m.IntervaloKm).HasColumnName("intervalo_km");
        b.Property(m => m.GarantiaFabricanteMeses).HasColumnName("garantia_fabricante_meses");
        b.Property(m => m.AntecedenciaDias).HasColumnName("antecedencia_dias").IsRequired();
        b.Property(m => m.CodigoCheckup).HasColumnName("codigo_checkup").HasMaxLength(30);
        b.Property(m => m.Ativo).HasColumnName("ativo").HasDefaultValue(true).IsRequired();

        b.ToTable(t =>
        {
            t.HasCheckConstraint("ck_modelo_intervalo_dias", "intervalo_dias IS NULL OR intervalo_dias > 0");
            t.HasCheckConstraint("ck_modelo_intervalo_km", "intervalo_km IS NULL OR intervalo_km > 0");
            t.HasCheckConstraint("ck_modelo_garantia", "garantia_fabricante_meses IS NULL OR garantia_fabricante_meses > 0");
            t.HasCheckConstraint("ck_modelo_antecedencia", "antecedencia_dias BETWEEN 0 AND 365");
        });

        b.HasIndex(m => m.CategoriaId).HasDatabaseName("ix_modelos_categoria");

        b.HasMany(m => m.Passos)
         .WithOne()
         .HasForeignKey(p => p.ModeloItemId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
