using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using EmDia.Api.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace EmDia.Api.IntegrationTests;

/// <summary>CT01–CT12 · Autenticação (E01–E04 · spec §4).</summary>
[Collection("Integration")]
public class AuthTests(IntegrationTestFactory f)
{
    private HttpClient Anonimo() => ApiCliente.NovoHttp(f);

    [Fact(DisplayName = "CT01 · Cadastro válido → 201; conta e preferências criadas; senha gravada como hash")]
    public async Task CT01_CadastroValido()
    {
        var email = $"novo-{Guid.NewGuid():N}@email.com";
        var resp = await Anonimo().PostAsJsonAsync("/api/v1/auth/registrar", new { nome = "Rodrigo", email, senha = "Senha123" });

        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var json = await ApiCliente.Json(resp);
        json["email"]!.GetValue<string>().Should().Be(email);
        json.AsObject().Should().NotContainKey("senha").And.NotContainKey("senhaHash");

        var cliente = ApiCliente.NovoHttp(f);
        var login = await cliente.PostAsJsonAsync("/api/v1/auth/login", new { email, senha = "Senha123" });
        var token = (await ApiCliente.Json(login))["accessToken"]!.GetValue<string>();
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", token);
        var me = await ApiCliente.Json(await cliente.GetAsync("/api/v1/usuarios/me"));
        me["preferencias"]!["maxReavisos"]!.GetValue<int>().Should().Be(2);
        me["preferencias"]!["intervaloReavisoDias"]!.GetValue<int>().Should().Be(3);
    }

    [Fact(DisplayName = "CT02 · E-mail repetido (maiúsculas diferentes) → 409")]
    public async Task CT02_EmailRepetido()
    {
        var c = await ApiCliente.CriarAsync(f);
        var resp = await Anonimo().PostAsJsonAsync("/api/v1/auth/registrar",
            new { nome = "Outro", email = c.Email.ToUpperInvariant(), senha = "Senha123" });

        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact(DisplayName = "CT03 · Senha fraca → 400 com erro no campo senha")]
    public async Task CT03_SenhaFraca()
    {
        var resp = await Anonimo().PostAsJsonAsync("/api/v1/auth/registrar",
            new { nome = "Rodrigo", email = $"x-{Guid.NewGuid():N}@email.com", senha = "abc" });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var json = await ApiCliente.Json(resp);
        json["title"]!.GetValue<string>().Should().Be("Dados inválidos");
        json["errors"]!["senha"]![0]!.GetValue<string>().Should().Be("Use pelo menos 8 caracteres, com letras e números.");
    }

    [Fact(DisplayName = "CT04 · Login válido → 200 com accessToken e cookie de refresh HttpOnly")]
    public async Task CT04_LoginValido()
    {
        var c = await ApiCliente.CriarAsync(f);
        var resp = await c.EntrarAsync();

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await ApiCliente.Json(resp);
        json["accessToken"]!.GetValue<string>().Should().NotBeNullOrEmpty();
        json["usuario"]!["precisaCheckup"]!.GetValue<bool>().Should().BeTrue();
        var cookie = resp.Headers.GetValues("Set-Cookie").Single(v => v.StartsWith("refreshToken="));
        cookie.Should().ContainEquivalentOf("httponly").And.ContainEquivalentOf("samesite=strict")
            .And.ContainEquivalentOf("path=/api/v1/auth");
    }

    [Fact(DisplayName = "CT05 · Senha errada → 401 com mensagem genérica")]
    public async Task CT05_SenhaErrada()
    {
        var c = await ApiCliente.CriarAsync(f);
        var resp = await c.EntrarAsync("Errada999");

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await ApiCliente.Json(resp))["detail"]!.GetValue<string>().Should().Be("E-mail ou senha inválidos.");

        var inexistente = await Anonimo().PostAsJsonAsync("/api/v1/auth/login", new { email = "ninguem@email.com", senha = "Senha123" });
        (await ApiCliente.Json(inexistente))["detail"]!.GetValue<string>().Should().Be("E-mail ou senha inválidos.");
    }

    [Fact(DisplayName = "CT06 · 5 falhas seguidas, depois a senha certa → 401 enquanto durar o bloqueio")]
    public async Task CT06_Bloqueio()
    {
        var c = await ApiCliente.CriarAsync(f);
        for (int i = 0; i < 5; i++)
            (await c.EntrarAsync("Errada999")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        (await c.EntrarAsync()).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "CT07 · 6 logins em menos de 1 minuto do mesmo IP → 429")]
    public async Task CT07_LimiteDeTentativas()
    {
        var comLimite = f.WithWebHostBuilder(b => b.UseSetting("Security:LoginTentativasPorMinuto", "5"));
        var http = ApiCliente.NovoHttp(comLimite);
        var status = new List<HttpStatusCode>();
        for (int i = 0; i < 6; i++)
            status.Add((await http.PostAsJsonAsync("/api/v1/auth/login", new { email = "a@b.com", senha = "Senha123" })).StatusCode);

        status.Take(5).Should().OnlyContain(s => s == HttpStatusCode.Unauthorized);
        status[5].Should().Be(HttpStatusCode.TooManyRequests);
    }

    [Fact(DisplayName = "DV21 · Limite geral por IP: passou do máximo no minuto → 429 MUITAS_REQUISICOES")]
    public async Task LimiteGeralPorIp()
    {
        var comLimite = f.WithWebHostBuilder(b => b.UseSetting("Security:RequisicoesPorMinuto", "3"));
        var http = ApiCliente.NovoHttp(comLimite);
        var respostas = new List<HttpResponseMessage>();
        for (int i = 0; i < 4; i++)
            respostas.Add(await http.GetAsync("/api/v1/radar"));

        respostas.Take(3).Should().OnlyContain(r => r.StatusCode == HttpStatusCode.Unauthorized);
        respostas[3].StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        respostas[3].Headers.RetryAfter.Should().NotBeNull();
        (await ApiCliente.Json(respostas[3]))["code"]!.GetValue<string>().Should().Be("MUITAS_REQUISICOES");
    }

    [Fact(DisplayName = "CT08 · GET /radar sem Authorization → 401")]
    public async Task CT08_SemToken()
    {
        (await Anonimo().GetAsync("/api/v1/radar")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "CT09 · Token com exp no passado → 401")]
    public async Task CT09_TokenExpirado()
    {
        var c = await ApiCliente.CriarAsync(f);
        var credenciais = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(IntegrationTestFactory.ChaveJwt)), SecurityAlgorithms.HmacSha256);
        var expirado = new JwtSecurityToken("emdia-api", "emdia-web", [new Claim("sub", c.UsuarioId.ToString())],
            DateTime.UtcNow.AddHours(-2), DateTime.UtcNow.AddHours(-1), credenciais);
        c.UsarToken(new JwtSecurityTokenHandler().WriteToken(expirado));

        (await c.Get("/api/v1/radar")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "CT10 · Renovar sessão com cookie válido → 200; novo token; refresh antigo revogado")]
    public async Task CT10_Refresh()
    {
        var c = await ApiCliente.CriarAsync(f);
        var antigo = c.RefreshCookie;

        var resp = await c.RefreshAsync(antigo);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ApiCliente.Json(resp))["accessToken"]!.GetValue<string>().Should().NotBeNullOrEmpty();
        ApiCliente.CookieRefresh(resp).Should().NotBe(antigo);
    }

    /// <summary>Executa com "hoje" um dia à frente: passa da tolerância de reuso (60 s) sem vencer os tokens (7 dias).</summary>
    private async Task DepoisDaTolerancia(Func<Task> acao)
    {
        try
        {
            f.Relogio.Hoje = RelogioFixo.HojePadrao.AddDays(1);
            await acao();
        }
        finally
        {
            f.Relogio.Hoje = RelogioFixo.HojePadrao;
        }
    }

    [Fact(DisplayName = "CT11 · Reuso de um refresh já trocado → 401; a sessão daquele aparelho é revogada, a dos outros não (DV25)")]
    public async Task CT11_ReusoDeRefresh()
    {
        var c = await ApiCliente.CriarAsync(f);
        var antigo = c.RefreshCookie;
        var outroAparelho = ApiCliente.CookieRefresh(await c.EntrarAsync());
        var novo = ApiCliente.CookieRefresh(await c.RefreshAsync(antigo));

        await DepoisDaTolerancia(async () =>
        {
            var reuso = await c.RefreshAsync(antigo);
            reuso.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            (await ApiCliente.Json(reuso))["code"]!.GetValue<string>().Should().Be("REFRESH_REUTILIZADO");
            (await c.RefreshAsync(novo)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            (await c.RefreshAsync(outroAparelho)).StatusCode.Should().Be(HttpStatusCode.OK);
        });
    }

    [Fact(DisplayName = "DV25 · PC e celular logados juntos: cada um renova a própria sessão; sair em um não derruba o outro")]
    public async Task DoisAparelhosAoMesmoTempo()
    {
        var c = await ApiCliente.CriarAsync(f);
        var pc = c.RefreshCookie;
        var celular = ApiCliente.CookieRefresh(await c.EntrarAsync());

        var pcRenovado = await c.RefreshAsync(pc);
        var celularRenovado = await c.RefreshAsync(celular);
        pcRenovado.StatusCode.Should().Be(HttpStatusCode.OK);
        celularRenovado.StatusCode.Should().Be(HttpStatusCode.OK);

        var sairNoPc = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/logout");
        sairNoPc.Headers.Add("Cookie", $"refreshToken={ApiCliente.CookieRefresh(pcRenovado)}");
        (await c.Http.SendAsync(sairNoPc)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await c.RefreshAsync(ApiCliente.CookieRefresh(celularRenovado))).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "DV25 · Duas abas renovando com o mesmo cookie (até 60 s) → as duas seguem logadas, sem revogar nada")]
    public async Task DuasAbasRenovandoJuntas()
    {
        var c = await ApiCliente.CriarAsync(f);
        var cookie = c.RefreshCookie;

        var aba1 = await c.RefreshAsync(cookie);
        var aba2 = await c.RefreshAsync(cookie);

        aba1.StatusCode.Should().Be(HttpStatusCode.OK);
        aba2.StatusCode.Should().Be(HttpStatusCode.OK);
        (await c.RefreshAsync(ApiCliente.CookieRefresh(aba1))).StatusCode.Should().Be(HttpStatusCode.OK);
        (await c.RefreshAsync(ApiCliente.CookieRefresh(aba2))).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "App mobile (DV17) · refresh no corpo, rotação e reuso detectado; o navegador nunca recebe o refresh no corpo")]
    public async Task AppMobile_RefreshNoCorpo()
    {
        var c = await ApiCliente.CriarAsync(f);
        var http = ApiCliente.NovoHttp(f);
        http.DefaultRequestHeaders.Add("X-Cliente", "mobile");

        var login = await http.PostAsJsonAsync("/api/v1/auth/login", new { email = c.Email, senha = ApiCliente.Senha });
        var json = await ApiCliente.Json(login);
        var refresh1 = json["refreshToken"]!.GetValue<string>();
        login.Headers.Contains("Set-Cookie").Should().BeFalse();

        var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/refresh");
        req.Headers.Add("X-Refresh-Token", refresh1);
        var renovado = await http.SendAsync(req);
        renovado.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ApiCliente.Json(renovado))["refreshToken"]!.GetValue<string>().Should().NotBe(refresh1);

        var reuso = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/refresh");
        reuso.Headers.Add("X-Refresh-Token", refresh1);
        await DepoisDaTolerancia(async () =>
            (await http.SendAsync(reuso)).StatusCode.Should().Be(HttpStatusCode.Unauthorized));

        // Sem o cabeçalho X-Cliente, o cabeçalho X-Refresh-Token é ignorado e o corpo não traz o refresh
        var web = await c.EntrarAsync();
        (await ApiCliente.Json(web)).AsObject().Should().NotContainKey("refreshToken");
        var semCookie = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/refresh");
        semCookie.Headers.Add("X-Refresh-Token", refresh1);
        (await c.Http.SendAsync(semCookie)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "CT12 · Logout → 204; refresh revogado; cookie apagado")]
    public async Task CT12_Logout()
    {
        var c = await ApiCliente.CriarAsync(f);
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/logout");
        req.Headers.Add("Cookie", $"refreshToken={c.RefreshCookie}");

        var resp = await c.Http.SendAsync(req);

        resp.StatusCode.Should().Be(HttpStatusCode.NoContent);
        resp.Headers.GetValues("Set-Cookie").Should().Contain(v => v.StartsWith("refreshToken=;"));
        (await c.RefreshAsync(c.RefreshCookie)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
