using EmDia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmDia.Infrastructure.Data.Configurations;

public class VeiculoConfiguration : IEntityTypeConfiguration<Veiculo>
{
    public void Configure(EntityTypeBuilder<Veiculo> b)
    {
        b.ToTable("veiculos");
        b.HasKey(v => v.Id);
        b.Property(v => v.Id).HasColumnName("id");
        b.Property(v => v.UsuarioId).HasColumnName("usuario_id").IsRequired();
        b.Property(v => v.Apelido).HasColumnName("apelido").HasMaxLength(50).IsRequired();
        b.Property(v => v.Tipo).HasColumnName("tipo").IsRequired();
        b.Property(v => v.KmAtual).HasColumnName("km_atual").IsRequired();
        b.Property(v => v.MediaKmMes).HasColumnName("media_km_mes").IsRequired();
        b.Property(v => v.KmAtualizadoEm).HasColumnName("km_atualizado_em").IsRequired();
        b.Property(v => v.CriadoEm).HasColumnName("criado_em").IsRequired();

        b.ToTable(t =>
        {
            t.HasCheckConstraint("ck_veiculo_km", "km_atual >= 0");
            t.HasCheckConstraint("ck_veiculo_media_km", "media_km_mes BETWEEN 1 AND 20000");
        });

        // Apelido único por usuário (spec §2.5)
        b.HasIndex(v => new { v.UsuarioId, v.Apelido }).IsUnique()
         .HasDatabaseName("ix_veiculos_usuario_apelido");

        b.HasIndex(v => v.UsuarioId).HasDatabaseName("ix_veiculos_usuario");
    }
}
