using EmDia.Domain.Entities;
using EmDia.Infrastructure.Data.Configurations;
using EmDia.Infrastructure.Data.Seed;
using Microsoft.EntityFrameworkCore;

namespace EmDia.Infrastructure.Data;

public class EmDiaDbContext : DbContext
{
    public EmDiaDbContext(DbContextOptions<EmDiaDbContext> options) : base(options) { }

    // ── DbSets (12 tabelas) ───────────────────────────────────────────────────
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<PreferenciasAviso> PreferenciasAviso => Set<PreferenciasAviso>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Checkup> Checkups => Set<Checkup>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<ModeloItem> ModelosItem => Set<ModeloItem>();
    public DbSet<PassoModelo> PassosModelo => Set<PassoModelo>();
    public DbSet<Veiculo> Veiculos => Set<Veiculo>();
    public DbSet<Item> Itens => Set<Item>();
    public DbSet<Passo> Passos => Set<Passo>();
    public DbSet<Historico> Historicos => Set<Historico>();
    public DbSet<Aviso> Avisos => Set<Aviso>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica todas as configurações (IEntityTypeConfiguration<T>)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EmDiaDbContext).Assembly);

        // Seed de dados iniciais
        RelacionamentosConfiguration.Aplicar(modelBuilder);
        SeedData.Aplicar(modelBuilder);
    }
}
