using EmDia.Application.Interfaces;
using EmDia.Infrastructure.Data;
using EmDia.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace EmDia.Api.IntegrationTests.Fixtures;

/// <summary>
/// WebApplicationFactory que usa Testcontainers para provisionar
/// um PostgreSQL efêmero em cada execução dos testes.
/// </summary>
public class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _pg = new PostgreSqlBuilder()
        .WithDatabase("emdia_test")
        .WithUsername("emdia_test")
        .WithPassword("emdia_test")
        .Build();

    public string ConnectionString => _pg.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _pg.StartAsync();

        // Aplicar migrations no banco de teste
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EmDiaDbContext>();
        await db.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _pg.StopAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // ConfigureAppConfiguration roda ANTES do Program.cs ler a configuração
        builder.ConfigureAppConfiguration((ctx, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Chave"]                  = "chave-testes-integracao-32chars-emdia2026xx",
                ["Jwt:Emissor"]                = "emdia-api",
                ["Jwt:Audiencia"]              = "emdia-web",
                ["Cors:Origens"]               = "http://localhost:5173",
                ["ConnectionStrings:Default"]  = _pg.GetConnectionString()
            });
        });

        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Substituir DbContext pelo Testcontainer
            services.RemoveAll<DbContextOptions<EmDiaDbContext>>();
            services.RemoveAll<EmDiaDbContext>();

            services.AddDbContext<EmDiaDbContext>(opts =>
                opts.UseNpgsql(_pg.GetConnectionString(),
                    o => o.MigrationsAssembly("EmDia.Infrastructure")));

            // E-mail fake nos testes
            services.RemoveAll<IEmailService>();
            services.AddSingleton<IEmailService, EmailServiceFake>();
        });
    }
}
