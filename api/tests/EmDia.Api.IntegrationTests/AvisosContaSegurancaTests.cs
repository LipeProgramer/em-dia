using System.Net;
using EmDia.Api.IntegrationTests.Fixtures;
using EmDia.Application.Interfaces.Repositories;
using EmDia.Domain.Entities;
using EmDia.Infrastructure.Data;
using EmDia.Infrastructure.Jobs;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EmDia.Api.IntegrationTests;

/// <summary>CT38–CT46 · Rotina de avisos, conta e segurança (RN08/RN09, E05–E07, spec §4).</summary>
[Collection("Integration")]
public class AvisosContaSegurancaTests(IntegrationTestFactory f)
{
    private async Task RodarRotinaAsync(DateOnly hoje)
    {
        f.Relogio.Hoje = hoje;
        try
        {
            await f.Services.GetRequiredService<AvisosWorker>().ExecutarAgoraAsync();
        }
        finally
        {
            f.Relogio.Hoje = RelogioFixo.HojePadrao;
        }
    }

    private int EmailsPara(ApiCliente c) => f.Emails.EmailsEnviados.Count(e => e.Destinatario == c.Email);

    private async Task<List<Aviso>> AvisosDoItemAsync(string itemId)
    {
        using var scope = f.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EmDiaDbContext>();
        return await db.Avisos.Where(a => a.ItemId == Guid.Parse(itemId)).OrderBy(a => a.Numero).ToListAsync();
    }

    // ── Avisos ───────────────────────────────────────────────────────────────

    [Fact(DisplayName = "CT38 · Item entra em Atenção hoje → 1 e-mail registrado; linha em avisos (número 0)")]
    public async Task CT38_AvisoAntecipado()
    {
        var c = await ApiCliente.CriarAsync(f);
        var item = await c.CriarDataUnicaAsync("2026-11-14", antecedencia: 30); // 30 dias → Atenção hoje

        await RodarRotinaAsync(RelogioFixo.HojePadrao);

        EmailsPara(c).Should().Be(1);
        var avisos = await AvisosDoItemAsync(item["id"]!.GetValue<string>());
        avisos.Should().ContainSingle();
        avisos[0].Numero.Should().Be(0);
        avisos[0].Sucesso.Should().BeTrue();
    }

    [Fact(DisplayName = "CT39 · Rotina executada 2 vezes no mesmo dia → apenas 1 e-mail")]
    public async Task CT39_SemDuplicar()
    {
        var c = await ApiCliente.CriarAsync(f);
        await c.CriarDataUnicaAsync("2026-11-01", antecedencia: 30);

        await RodarRotinaAsync(RelogioFixo.HojePadrao);
        await RodarRotinaAsync(RelogioFixo.HojePadrao);

        EmailsPara(c).Should().Be(1);
    }

    [Fact(DisplayName = "CT40 · max_reavisos = 1 e item sem conclusão por 10 dias → antecipado + 1 reaviso; nada mais")]
    public async Task CT40_LimiteDeReavisos()
    {
        var c = await ApiCliente.CriarAsync(f);
        (await c.Put("/api/v1/usuarios/me/preferencias",
            new { receberEmail = true, maxReavisos = 1, intervaloReavisoDias = 3, modoFocoPadrao = false }))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        var item = await c.CriarDataUnicaAsync("2026-11-30", antecedencia: 60);

        for (int d = 0; d <= 10; d++)
            await RodarRotinaAsync(RelogioFixo.HojePadrao.AddDays(d));

        EmailsPara(c).Should().Be(2);
        (await AvisosDoItemAsync(item["id"]!.GetValue<string>())).Select(a => (int)a.Numero).Should().Equal(0, 1);
    }

    [Fact(DisplayName = "CT41 · receber_email = false → nenhum e-mail enviado")]
    public async Task CT41_EmailDesativado()
    {
        var c = await ApiCliente.CriarAsync(f);
        await c.Put("/api/v1/usuarios/me/preferencias",
            new { receberEmail = false, maxReavisos = 2, intervaloReavisoDias = 3, modoFocoPadrao = false });
        await c.CriarDataUnicaAsync("2026-10-20", antecedencia: 30);

        await RodarRotinaAsync(RelogioFixo.HojePadrao);

        EmailsPara(c).Should().Be(0);
    }

    // ── Conta e segurança ─────────────────────────────────────────────────────

    [Fact(DisplayName = "CT42 · Excluir conta com senha correta → 204; usuário e todos os dados removidos")]
    public async Task CT42_ExcluirConta()
    {
        var c = await ApiCliente.CriarAsync(f);
        var v = await c.CriarVeiculoAsync();
        var item = await c.CriarItemAsync(new
        {
            nome = "Troca de óleo", categoriaId = 1, tipoVencimento = "UsoOuTempo", veiculoId = v, dataBase = "2026-05-01",
            kmBase = 75_000, intervaloKm = 10_000, intervaloDias = 365, antecedenciaDias = 30, passos = new[] { "Agendar" }
        });
        await c.Post($"/api/v1/itens/{item["id"]}/concluir",
            new { dataConclusao = "2026-10-15", km = 85_000, confirmarPassosPendentes = true });

        var resp = await c.Delete("/api/v1/usuarios/me", new { senha = ApiCliente.Senha });

        resp.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var scope = f.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EmDiaDbContext>();
        (await db.Usuarios.AnyAsync(u => u.Id == c.UsuarioId)).Should().BeFalse();
        (await db.Itens.AnyAsync(i => i.UsuarioId == c.UsuarioId)).Should().BeFalse();
        (await db.Veiculos.AnyAsync(x => x.UsuarioId == c.UsuarioId)).Should().BeFalse();
        (await db.RefreshTokens.AnyAsync(r => r.UsuarioId == c.UsuarioId)).Should().BeFalse();
        (await db.Historicos.AnyAsync(h => h.ItemId == Guid.Parse(item["id"]!.GetValue<string>()))).Should().BeFalse();
    }

    [Fact(DisplayName = "CT43 · Excluir conta com senha incorreta → 422; nada removido")]
    public async Task CT43_ExcluirContaSenhaErrada()
    {
        var c = await ApiCliente.CriarAsync(f);

        var resp = await c.Delete("/api/v1/usuarios/me", new { senha = "Errada999" });

        resp.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await c.Get("/api/v1/usuarios/me")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "CT44 · Nenhuma resposta da API contém senha, hash ou token de refresh")]
    public async Task CT44_DadosSensiveis()
    {
        var c = await ApiCliente.CriarAsync(f);
        var respostas = new List<HttpResponseMessage>
        {
            await c.EntrarAsync(),
            await c.Get("/api/v1/usuarios/me"),
            await c.RefreshAsync(c.RefreshCookie),
            await c.Get("/api/v1/radar"),
        };

        foreach (var r in respostas)
        {
            var corpo = await r.Content.ReadAsStringAsync();
            corpo.Should().NotContainEquivalentOf("senha").And.NotContainEquivalentOf("hash")
                .And.NotContain("$2a$").And.NotContain("$2b$").And.NotContain(c.RefreshCookie!);
        }
    }

    [Fact(DisplayName = "CT45 · Falha simulada no repositório → 500 genérico, sem stack trace")]
    public async Task CT45_ErroInesperado()
    {
        var quebrada = f.WithWebHostBuilder(b => b.ConfigureTestServices(s =>
        {
            s.RemoveAll<IItemRepository>();
            s.AddScoped<IItemRepository, RepositorioQuebrado>();
        }));
        var c = await ApiCliente.CriarAsync(quebrada);

        var resp = await c.Get("/api/v1/itens");

        resp.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var corpo = await resp.Content.ReadAsStringAsync();
        corpo.Should().Contain("Erro inesperado").And.NotContain("Falha simulada").And.NotContain(" at ");
    }

    [Fact(DisplayName = "CT46 · Requisição de outra origem (CORS) → bloqueada pelo navegador (sem Allow-Origin)")]
    public async Task CT46_Cors()
    {
        var http = ApiCliente.NovoHttp(f);

        async Task<HttpResponseMessage> Preflight(string origem)
        {
            var req = new HttpRequestMessage(HttpMethod.Options, "/api/v1/radar");
            req.Headers.Add("Origin", origem);
            req.Headers.Add("Access-Control-Request-Method", "GET");
            return await http.SendAsync(req);
        }

        (await Preflight("https://site-malicioso.com")).Headers.Contains("Access-Control-Allow-Origin").Should().BeFalse();
        (await Preflight("http://localhost:5173")).Headers.GetValues("Access-Control-Allow-Origin")
            .Should().ContainSingle("http://localhost:5173");
    }

    private sealed class RepositorioQuebrado : IItemRepository
    {
        private static Exception Falha() => new InvalidOperationException("Falha simulada no repositório");
        public Task<IReadOnlyList<Item>> ListarPorUsuarioAsync(Guid u, CancellationToken ct = default) => throw Falha();
        public Task<IReadOnlyList<Item>> ListarAtivosPorVeiculoAsync(Guid v, Guid u, CancellationToken ct = default) => throw Falha();
        public Task<Item?> ObterAsync(Guid id, Guid u, CancellationToken ct = default) => throw Falha();
        public Task<bool> ExisteAsync(Guid id, Guid u, CancellationToken ct = default) => throw Falha();
        public Task AdicionarAsync(Item item, CancellationToken ct = default) => throw Falha();
        public void Remover(Item item) => throw Falha();
    }
}
