using EmDia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmDia.Infrastructure.Data.Configurations;

/// <summary>Pedidos de redefinição de senha (extra · DV24). Excluir o usuário apaga os pedidos.</summary>
public class RedefinicaoSenhaConfiguration : IEntityTypeConfiguration<RedefinicaoSenha>
{
    public void Configure(EntityTypeBuilder<RedefinicaoSenha> b)
    {
        b.ToTable("redefinicoes_senha");
        b.HasKey(r => r.Id);
        b.Property(r => r.Id).HasColumnName("id");
        b.Property(r => r.UsuarioId).HasColumnName("usuario_id").IsRequired();
        b.Property(r => r.TokenHash).HasColumnName("token_hash").HasMaxLength(128).IsRequired();
        b.Property(r => r.CriadoEm).HasColumnName("criado_em").IsRequired();
        b.Property(r => r.ExpiraEm).HasColumnName("expira_em").IsRequired();
        b.Property(r => r.UsadoEm).HasColumnName("usado_em");

        b.HasIndex(r => r.TokenHash).IsUnique().HasDatabaseName("ix_redefinicoes_senha_hash");
        b.HasIndex(r => r.UsuarioId).HasDatabaseName("ix_redefinicoes_senha_usuario");

        b.HasOne<Usuario>().WithMany().HasForeignKey(r => r.UsuarioId).OnDelete(DeleteBehavior.Cascade);
    }
}
