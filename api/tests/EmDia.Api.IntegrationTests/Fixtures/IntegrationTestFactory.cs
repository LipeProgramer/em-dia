using EmDia.Application.Interfaces;
using EmDia.Infrastructure.Data;
using EmDia.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace EmDia.Api.IntegrationTests.Fixtures;

/// <summary>
/// Sobe a API com um PostgreSQL descartável (spec §6.1):
/// por padrão via Testcontainers (exige Docker); se a variável EMDIA_TEST_DB tiver uma connection string,
/// usa esse banco (que é apagado e recriado a cada execução).
/// "Hoje" é fixado em 15/10/2026 por <see cref="RelogioFixo"/> e o e-mail é falso (só registra as mensagens).
/// </summary>
public class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string ChaveJwt = "chave-de-testes-de-integracao-emdia-2026-com-mais-de-256-bits";

    private PostgreSqlContainer? _container;
    private string _connectionString = string.Empty;

    public RelogioFixo Relogio { get; } = new();
    public EmailServiceFake Emails { get; } = new();

    public async Task InitializeAsync()
    {
        var externo = Environment.GetEnvironmentVariable("EMDIA_TEST_DB");
        if (!string.IsNullOrWhiteSpace(externo))
        {
            _connectionString = externo;
        }
        else
        {
            _container = new PostgreSqlBuilder("postgres:17").Build();
            await _container.StartAsync();
            _connectionString = _container.GetConnectionString();
        }

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EmDiaDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        if (_container is not null)
            await _container.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Default", _connectionString);
        builder.UseSetting("Jwt:Key", ChaveJwt);
        builder.UseSetting("Cors:Origens:0", "http://localhost:5173");
        // O limite real (5/min) é testado à parte no CT07; aqui ele não pode atrapalhar os outros testes
        builder.UseSetting("Security:LoginTentativasPorMinuto", "100000");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IRelogio>();
            services.AddSingleton<IRelogio>(Relogio);
            services.RemoveAll<IEmailService>();
            services.AddSingleton<IEmailService>(Emails);
        });
    }
}

/// <summary>
/// Relógio controlado: "hoje" fixo (15/10/2026 por padrão). O momento atual cai no mesmo dia (entre 12h e 14h24 UTC,
/// avançando com o relógio real para manter a ordem dos registros).
/// </summary>
public class RelogioFixo : IRelogio
{
    public static readonly DateOnly HojePadrao = new(2026, 10, 15);

    public DateOnly Hoje { get; set; } = HojePadrao;

    public DateTimeOffset Agora =>
        new DateTimeOffset(Hoje.ToDateTime(new TimeOnly(12, 0)), TimeSpan.Zero) + DateTime.UtcNow.TimeOfDay / 10;
}

[CollectionDefinition("Integration")]
public class IntegrationCollection : ICollectionFixture<IntegrationTestFactory>;
