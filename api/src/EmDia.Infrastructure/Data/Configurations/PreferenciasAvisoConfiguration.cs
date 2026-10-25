using EmDia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmDia.Infrastructure.Data.Configurations;

public class PreferenciasAvisoConfiguration : IEntityTypeConfiguration<PreferenciasAviso>
{
    public void Configure(EntityTypeBuilder<PreferenciasAviso> b)
    {
        b.ToTable("preferencias_aviso");
        b.HasKey(p => p.UsuarioId);
        b.Property(p => p.UsuarioId).HasColumnName("usuario_id");
        b.Property(p => p.ReceberEmail).HasColumnName("receber_email").HasDefaultValue(true).IsRequired();
        b.Property(p => p.MaxReavisos).HasColumnName("max_reavisos").HasDefaultValue((short)2).IsRequired();
        b.Property(p => p.IntervaloReavisoDias).HasColumnName("intervalo_reaviso_dias").HasDefaultValue((short)3).IsRequired();
        b.Property(p => p.ModoFocoPadrao).HasColumnName("modo_foco_padrao").HasDefaultValue(false).IsRequired();
        b.Property(p => p.AtualizadoEm).HasColumnName("atualizado_em").IsRequired();

        // CHECKs da spec §2.5
        b.ToTable(t =>
        {
            t.HasCheckConstraint("ck_pref_max_reavisos", "max_reavisos BETWEEN 0 AND 5");
            t.HasCheckConstraint("ck_pref_intervalo", "intervalo_reaviso_dias BETWEEN 1 AND 7");
        });
    }
}
