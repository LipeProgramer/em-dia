using EmDia.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmDia.Infrastructure.Data.Configurations;

/// <summary>
/// Chaves estrangeiras do DER (spec §2.4) que não pertencem a uma navegação das entidades.
/// Excluir um usuário apaga em cascata todos os seus dados (LGPD); categorias e veículos com itens
/// não podem ser excluídos (RESTRICT).
/// </summary>
public static class RelacionamentosConfiguration
{
    public static void Aplicar(ModelBuilder b)
    {
        // usuarios → refresh_tokens, checkups, veiculos, itens (1:N, CASCADE)
        b.Entity<RefreshToken>().HasOne<Usuario>().WithMany()
            .HasForeignKey(r => r.UsuarioId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<Checkup>().HasOne<Usuario>().WithMany()
            .HasForeignKey(c => c.UsuarioId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<Veiculo>().HasOne<Usuario>().WithMany()
            .HasForeignKey(v => v.UsuarioId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<Item>().HasOne<Usuario>().WithMany()
            .HasForeignKey(i => i.UsuarioId).OnDelete(DeleteBehavior.Cascade);

        // categorias → itens, modelos_item (1:N, RESTRICT)
        b.Entity<Item>().HasOne<Categoria>().WithMany()
            .HasForeignKey(i => i.CategoriaId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<ModeloItem>().HasOne<Categoria>().WithMany()
            .HasForeignKey(m => m.CategoriaId).OnDelete(DeleteBehavior.Restrict);

        // itens → avisos (1:N, CASCADE)
        b.Entity<Aviso>().HasOne<Item>().WithMany()
            .HasForeignKey(a => a.ItemId).OnDelete(DeleteBehavior.Cascade);

        // Ids uuid são gerados pela API (no Domain), nunca pelo banco (spec §2.1/§2.5). Assim o EF trata como
        // novos os registros adicionados por navegação (ex.: histórico criado ao concluir um item).
        foreach (var entidade in b.Model.GetEntityTypes())
        {
            var id = entidade.FindPrimaryKey()?.Properties.SingleOrDefault();
            if (id?.ClrType == typeof(Guid))
                id.ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never;
        }

        // modelos: nome único dentro da categoria
        b.Entity<ModeloItem>().HasIndex(m => new { m.CategoriaId, m.Nome }).IsUnique()
            .HasDatabaseName("ix_modelos_categoria_nome");
    }
}
