using EmDia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmDia.Infrastructure.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.ToTable("refresh_tokens");
        b.HasKey(r => r.Id);
        b.Property(r => r.Id).HasColumnName("id");
        b.Property(r => r.UsuarioId).HasColumnName("usuario_id").IsRequired();
        b.Property(r => r.TokenHash).HasColumnName("token_hash").HasMaxLength(128).IsRequired();
        b.Property(r => r.CriadoEm).HasColumnName("criado_em").IsRequired();
        b.Property(r => r.ExpiraEm).HasColumnName("expira_em").IsRequired();
        b.Property(r => r.RevogadoEm).HasColumnName("revogado_em");
        b.Property(r => r.FamiliaId).HasColumnName("familia_id").IsRequired();
        b.Property(r => r.SubstituidoEm).HasColumnName("substituido_em");

        b.HasIndex(r => r.TokenHash).IsUnique()
         .HasDatabaseName("ix_refresh_tokens_hash");

        b.HasIndex(r => r.UsuarioId)
         .HasDatabaseName("ix_refresh_tokens_usuario");

        b.HasIndex(r => r.FamiliaId)
         .HasDatabaseName("ix_refresh_tokens_familia");
    }
}
