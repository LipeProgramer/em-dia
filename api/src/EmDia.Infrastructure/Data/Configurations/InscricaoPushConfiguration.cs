using EmDia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmDia.Infrastructure.Data.Configurations;

/// <summary>Aparelhos inscritos para notificações (extra · DV20). Excluir o usuário apaga as inscrições.</summary>
public class InscricaoPushConfiguration : IEntityTypeConfiguration<InscricaoPush>
{
    public void Configure(EntityTypeBuilder<InscricaoPush> b)
    {
        b.ToTable("inscricoes_push");
        b.HasKey(i => i.Id);
        b.Property(i => i.Id).HasColumnName("id");
        b.Property(i => i.UsuarioId).HasColumnName("usuario_id").IsRequired();
        b.Property(i => i.Endpoint).HasColumnName("endpoint").HasMaxLength(1000).IsRequired();
        b.Property(i => i.P256dh).HasColumnName("p256dh").HasMaxLength(200).IsRequired();
        b.Property(i => i.Auth).HasColumnName("auth").HasMaxLength(100).IsRequired();
        b.Property(i => i.CriadoEm).HasColumnName("criado_em").IsRequired();

        b.HasIndex(i => i.Endpoint).IsUnique().HasDatabaseName("ix_inscricoes_push_endpoint");
        b.HasIndex(i => i.UsuarioId).HasDatabaseName("ix_inscricoes_push_usuario");

        b.HasOne<Usuario>().WithMany().HasForeignKey(i => i.UsuarioId).OnDelete(DeleteBehavior.Cascade);
    }
}
