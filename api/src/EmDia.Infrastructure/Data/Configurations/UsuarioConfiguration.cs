using EmDia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmDia.Infrastructure.Data.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> b)
    {
        b.ToTable("usuarios");
        b.HasKey(u => u.Id);
        b.Property(u => u.Id).HasColumnName("id");
        b.Property(u => u.Nome).HasColumnName("nome").HasMaxLength(100).IsRequired();
        b.Property(u => u.Email).HasColumnName("email").HasMaxLength(150).IsRequired();
        b.Property(u => u.SenhaHash).HasColumnName("senha_hash").HasMaxLength(100).IsRequired();
        b.Property(u => u.UltimoCheckupEm).HasColumnName("ultimo_checkup_em");
        b.Property(u => u.FalhasLogin).HasColumnName("falhas_login").HasDefaultValue((short)0).IsRequired();
        b.Property(u => u.BloqueadoAte).HasColumnName("bloqueado_ate");
        b.Property(u => u.CriadoEm).HasColumnName("criado_em").IsRequired();

        // Índice único no email em minúsculas (spec §2.5)
        b.HasIndex(u => u.Email)
         .HasDatabaseName("ix_usuarios_email")
         .IsUnique();

        b.HasOne(u => u.Preferencias)
         .WithOne()
         .HasForeignKey<PreferenciasAviso>(p => p.UsuarioId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
