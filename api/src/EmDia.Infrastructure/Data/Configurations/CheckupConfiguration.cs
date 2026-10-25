using EmDia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmDia.Infrastructure.Data.Configurations;

public class CheckupConfiguration : IEntityTypeConfiguration<Checkup>
{
    public void Configure(EntityTypeBuilder<Checkup> b)
    {
        b.ToTable("checkups");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).HasColumnName("id");
        b.Property(c => c.UsuarioId).HasColumnName("usuario_id").IsRequired();
        b.Property(c => c.RealizadoEm).HasColumnName("realizado_em").IsRequired();
        b.Property(c => c.Respostas).HasColumnName("respostas")
         .HasColumnType("jsonb").IsRequired();
        b.Property(c => c.ItensCriados).HasColumnName("itens_criados").IsRequired();

        b.ToTable(t => t.HasCheckConstraint("ck_checkup_itens", "itens_criados >= 0"));

        b.HasIndex(c => c.UsuarioId).HasDatabaseName("ix_checkups_usuario");
    }
}
