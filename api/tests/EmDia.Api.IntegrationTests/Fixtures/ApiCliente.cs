using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EmDia.Api.IntegrationTests.Fixtures;

/// <summary>Cliente HTTP de um usuário de teste: cadastra, entra e guarda o token e o cookie de refresh.</summary>
public class ApiCliente
{
    public const string Senha = "Senha123";

    public HttpClient Http { get; }
    public string Email { get; }
    public string? RefreshCookie { get; set; }
    public Guid UsuarioId { get; private set; }

    private ApiCliente(HttpClient http, string email)
    {
        Http = http;
        Email = email;
    }

    public static HttpClient NovoHttp(WebApplicationFactory<Program> f) =>
        f.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false, AllowAutoRedirect = false });

    /// <summary>Cria uma conta nova (e-mail único) e já entra.</summary>
    public static async Task<ApiCliente> CriarAsync(WebApplicationFactory<Program> f, string nome = "Rodrigo")
    {
        var cliente = new ApiCliente(NovoHttp(f), $"teste-{Guid.NewGuid():N}@email.com");
        var reg = await cliente.Http.PostAsJsonAsync("/api/v1/auth/registrar", new { nome, email = cliente.Email, senha = Senha });
        reg.StatusCode.Should().Be(HttpStatusCode.Created);
        await cliente.EntrarAsync();
        return cliente;
    }

    public async Task<HttpResponseMessage> EntrarAsync(string? senha = null)
    {
        Http.DefaultRequestHeaders.Authorization = null;
        var resp = await Http.PostAsJsonAsync("/api/v1/auth/login", new { email = Email, senha = senha ?? Senha });
        if (resp.IsSuccessStatusCode)
        {
            var json = await Json(resp);
            UsarToken(json["accessToken"]!.GetValue<string>());
            UsuarioId = Guid.Parse(json["usuario"]!["id"]!.GetValue<string>());
            RefreshCookie = CookieRefresh(resp);
        }
        return resp;
    }

    public void UsarToken(string token) =>
        Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    public Task<HttpResponseMessage> RefreshAsync(string? cookie)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/refresh");
        if (cookie is not null) req.Headers.Add("Cookie", $"refreshToken={cookie}");
        return Http.SendAsync(req);
    }

    public static string? CookieRefresh(HttpResponseMessage resp) =>
        resp.Headers.TryGetValues("Set-Cookie", out var valores)
            ? valores.Select(v => v.Split(';')[0]).Where(v => v.StartsWith("refreshToken="))
                .Select(v => v["refreshToken=".Length..]).FirstOrDefault(v => v.Length > 0)
            : null;

    // ── Atalhos ──────────────────────────────────────────────────────────────

    public Task<HttpResponseMessage> Get(string url) => Http.GetAsync(url);
    public Task<HttpResponseMessage> Post(string url, object corpo) => Http.PostAsJsonAsync(url, corpo);
    public Task<HttpResponseMessage> Put(string url, object corpo) => Http.PutAsJsonAsync(url, corpo);
    public Task<HttpResponseMessage> Patch(string url, object corpo) => Http.PatchAsJsonAsync(url, corpo);
    public Task<HttpResponseMessage> Delete(string url) => Http.DeleteAsync(url);

    public Task<HttpResponseMessage> Delete(string url, object corpo) =>
        Http.SendAsync(new HttpRequestMessage(HttpMethod.Delete, url) { Content = JsonContent.Create(corpo) });

    public static async Task<JsonNode> Json(HttpResponseMessage resp)
    {
        var texto = await resp.Content.ReadAsStringAsync();
        return JsonNode.Parse(texto) ?? throw new JsonException("Resposta vazia");
    }

    /// <summary>Cria um veículo e devolve o id.</summary>
    public async Task<Guid> CriarVeiculoAsync(string apelido = "Gol", int km = 84_500, int media = 2_500)
    {
        var resp = await Post("/api/v1/veiculos", new { apelido, tipo = "Carro", kmAtual = km, mediaKmMes = media });
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        return Guid.Parse((await Json(resp))["id"]!.GetValue<string>());
    }

    /// <summary>Cria um item e devolve o JSON do detalhe (E14).</summary>
    public async Task<JsonNode> CriarItemAsync(object corpo)
    {
        var resp = await Post("/api/v1/itens", corpo);
        resp.StatusCode.Should().Be(HttpStatusCode.Created, await resp.Content.ReadAsStringAsync());
        return await Json(resp);
    }

    public Task<JsonNode> CriarDataUnicaAsync(string vencimento, string nome = "CNH", string[]? passos = null, int antecedencia = 45) =>
        CriarItemAsync(new
        {
            nome, categoriaId = 2, tipoVencimento = "DataUnica", dataVencimento = vencimento,
            antecedenciaDias = antecedencia, passos = passos ?? Array.Empty<string>()
        });

    public Task<JsonNode> CriarRecorrenteAsync(string dataBase, int intervaloDias, string nome = "Filtro", string[]? passos = null) =>
        CriarItemAsync(new
        {
            nome, categoriaId = 3, tipoVencimento = "RecorrentePorTempo", dataBase, intervaloDias,
            antecedenciaDias = 15, passos = passos ?? Array.Empty<string>()
        });
}
