using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.RateLimiting;
using EmDia.Api.IntegrationTests.Fixtures;
using EmDia.Application.DTOs.Auth;
using EmDia.Application.DTOs.Itens;
using EmDia.Domain.Enums;
using EmDia.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EmDia.Api.IntegrationTests.Seguranca;

/// <summary>
/// CT44-CT46 – Testes de Segurança (spec §6.4, §6.5).
/// CT44: Rate limit global / endpoint → 429 Too Many Requests
/// CT45: SQL Injection em buscas e parâmetros → proteção via ORM e parametrização segura
/// CT46: XSS (Cross-Site Scripting) em campos de entrada de texto → sanitização / encoding seguro
/// </summary>
[Collection("Integration")]
public class SegurancaTests : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory;

    public SegurancaTests(IntegrationTestFactory factory) => _factory = factory;

    private async Task<(HttpClient Client, Guid UsuarioId, string Email)> ClienteAutenticadoAsync(string sufixo)
    {
        var reg = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        var email = $"sec.{sufixo}@teste.com";
        await reg.PostAsJsonAsync("/api/v1/auth/registrar",
            new RegistrarRequest($"User Sec {sufixo}", email, "Senha@123"));
        var loginResp = await reg.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(email, "Senha@123"));
        var login = await loginResp.Content.ReadFromJsonAsync<LoginResponse>();

        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", login!.AccessToken);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EmDiaDbContext>();
        var user = await db.Usuarios.FirstAsync(u => u.Email == email);

        return (client, user.Id, email);
    }

    // ── CT44: Rate Limit ──────────────────────────────────────────────────────
    [Fact]
    public async Task CT44_RateLimit_ExcederLimite_Retorna429()
    {
        // Cria uma instância de teste onde o limite de requisições é configurado para 2 requisições por janela
        var rateLimitFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.Configure<RateLimiterOptions>(opts =>
                {
                    opts.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                    opts.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(_ =>
                        RateLimitPartition.GetFixedWindowLimiter("test_ratelimit", _ =>
                            new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = 2,
                                Window = TimeSpan.FromMinutes(1),
                                QueueLimit = 0
                            }));
                });
            });
        });

        var client = rateLimitFactory.CreateClient();

        // 1ª requisição permitida
        var resp1 = await client.GetAsync("/api/v1/catalogo/categorias");
        resp1.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2ª requisição permitida
        var resp2 = await client.GetAsync("/api/v1/catalogo/categorias");
        resp2.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3ª requisição deve estourar o limite da janela e retornar 429 Too Many Requests
        var resp3 = await client.GetAsync("/api/v1/catalogo/categorias");
        resp3.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    // ── CT45: SQL Injection ───────────────────────────────────────────────────
    [Fact]
    public async Task CT45_SqlInjection_BuscasEParametros_NaoExecutamComandosArbitrarios()
    {
        var (client, usuarioId, _) = await ClienteAutenticadoAsync("ct45");

        // Payload 1: Tentativa de DROP TABLE via busca de categorias
        var sqlDropPayload = "'; DROP TABLE \"Usuarios\"; --";
        var respCategorias = await client.GetAsync($"/api/v1/catalogo/categorias?busca={Uri.EscapeDataString(sqlDropPayload)}");
        // A API não deve lançar erro 500 do banco
        respCategorias.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);

        // Validar que a tabela Usuarios permanece intacta no banco
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EmDiaDbContext>();
            var usuarioExiste = await db.Usuarios.AnyAsync(u => u.Id == usuarioId);
            usuarioExiste.Should().BeTrue("a tabela de usuários não pode ser afetada por SQL injection");
        }

        // Payload 2: Tentativa de SQL Injection no Login (' OR '1'='1)
        var unauthClient = _factory.CreateClient();
        var respLoginSql = await unauthClient.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest("' OR '1'='1' --", "qualquerSenha"));

        // Deve retornar 400 (email inválido) ou 401 (não autorizado), NUNCA 200 nem 500
        respLoginSql.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);

        // Payload 3: Injeção de UNION SELECT em filtro de itens
        var respItens = await client.GetAsync($"/api/v1/itens?busca={Uri.EscapeDataString("' UNION SELECT * FROM \"Usuarios\" --")}");
        respItens.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    // ── CT46: XSS (Cross-Site Scripting) ──────────────────────────────────────
    [Fact]
    public async Task CT46_XSS_InputsTextoComScript_ArmazenadosESerializadosComSeguranca()
    {
        var (client, _, _) = await ClienteAutenticadoAsync("ct46");

        var scriptPayload = "<script>alert('XSS_ATTACK')</script>";
        var imgPayload = "<img src=x onerror=\"javascript:alert('XSS')\">";

        var itemReq = new CriarItemRequest(
            Nome: scriptPayload,
            CategoriaId: 2,
            TipoVencimento: TipoVencimento.DataUnica,
            DataBase: null,
            DataVencimento: new DateOnly(2027, 12, 31),
            IntervaloDias: null,
            IntervaloKm: null,
            KmBase: null,
            GarantiaFabricanteMeses: null,
            AntecedenciaDias: 30,
            VeiculoId: null,
            ModeloItemId: null,
            Observacao: imgPayload,
            Passos: ["<script>var bad=true;</script>"]);

        var criarResp = await client.PostAsJsonAsync("/api/v1/itens", itemReq);
        criarResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var itemCriado = await criarResp.Content.ReadFromJsonAsync<ItemResponse>();
        itemCriado.Should().NotBeNull();

        // O cabeçalho Content-Type deve ser estritamente JSON (application/json), impedindo execução HTML no navegador
        criarResp.Content.Headers.ContentType!.MediaType.Should().Be("application/json");

        // Obter o item via GET e validar a serialização segura
        var getResp = await client.GetAsync($"/api/v1/itens/{itemCriado!.Id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        getResp.Content.Headers.ContentType!.MediaType.Should().Be("application/json");

        var rawJson = await getResp.Content.ReadAsStringAsync();

        // Verifica que o payload é armazenado como string literal no JSON e que a estrutura JSON permanece válida
        rawJson.Should().Contain(scriptPayload);
        rawJson.Should().Contain("onerror=");

        var itemObtido = await getResp.Content.ReadFromJsonAsync<ItemResponse>();
        itemObtido!.Nome.Should().Be(scriptPayload);
        itemObtido.Observacao.Should().Be(imgPayload);
        itemObtido.Passos.Should().ContainSingle(p => p.Descricao == "<script>var bad=true;</script>");
    }
}
