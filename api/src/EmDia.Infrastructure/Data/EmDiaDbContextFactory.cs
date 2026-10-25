using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace EmDia.Infrastructure.Data;

/// <summary>
/// Factory usada APENAS pelo dotnet-ef em tempo de design (migrations).
/// Lê a connection string do appsettings.Development.json da Api, ou de variável de ambiente.
/// </summary>
public class EmDiaDbContextFactory : IDesignTimeDbContextFactory<EmDiaDbContext>
{
    public EmDiaDbContext CreateDbContext(string[] args)
    {
        // Procura o appsettings na pasta da Api (dois níveis acima de Infrastructure)
        var apiDir = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "EmDia.Api"));

        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.Exists(apiDir) ? apiDir : Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connStr = config.GetConnectionString("Default")
            ?? "Host=localhost;Port=5432;Database=emdia;Username=emdia;Password=emdia_dev;SSL Mode=Disable";

        var opts = new DbContextOptionsBuilder<EmDiaDbContext>();
        opts.UseNpgsql(connStr, o => o.MigrationsAssembly("EmDia.Infrastructure"));

        return new EmDiaDbContext(opts.Options);
    }
}
