using System.Net;
using System.Net.Http.Json;
using EmDia.Api.IntegrationTests.Fixtures;
using EmDia.Application.DTOs.Auth;
using FluentAssertions;

namespace EmDia.Api.IntegrationTests.Auth;

/// <summary>
/// CT01-CT12, CT42-CT44 — Testes de autenticação (spec §6.4).
/// </summary>
[Collection("Integration")]
public class AuthTests : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory;
    private readonly HttpClient _client;

    public AuthTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = true,
            AllowAutoRedirect = false
        });
    }

    private HttpClient NovoClienteAutenticado(string token)
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = true,
            AllowAutoRedirect = false
        });
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private HttpClient NovoClienteSemCookies()
        => _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = false,
            AllowAutoRedirect = false
        });

    // ── CT01 ─────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT01_RegistrarUsuarioValido_Retorna201()
    {
        var req = new RegistrarRequest("Ana Lima", "ana.ct01@teste.com", "Senha@123");
        var resp = await _client.PostAsJsonAsync("/api/v1/auth/registrar", req);

        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await resp.Content.ReadFromJsonAsync<LoginResponse>();
        body!.Email.Should().Be("ana.ct01@teste.com");
        body.Nome.Should().Be("Ana Lima");
        body.AccessToken.Should().NotBeNullOrEmpty();
        body.PrecisaCheckup.Should().BeTrue();
    }

    // ── CT02 ─────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT02_RegistrarEmailDuplicado_Retorna409()
    {
        var req = new RegistrarRequest("Bob Silva", "bob.ct02@teste.com", "Senha@123");
        await _client.PostAsJsonAsync("/api/v1/auth/registrar", req);

        var resp2 = await _client.PostAsJsonAsync("/api/v1/auth/registrar", req);
        resp2.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await resp2.Content.ReadAsStringAsync()).Should().Contain("EMAIL_DUPLICADO");
    }

    // ── CT03 ─────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT03_RegistrarEmailInvalido_Retorna400()
    {
        var resp = await _client.PostAsJsonAsync("/api/v1/auth/registrar",
            new RegistrarRequest("Carla", "nao-e-email", "Senha@123"));
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── CT04 ─────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT04_LoginCorreto_Retorna200ComTokenECookie()
    {
        var email = "diana.ct04@teste.com";
        await _client.PostAsJsonAsync("/api/v1/auth/registrar",
            new RegistrarRequest("Diana", email, "Senha@123"));

        // Usar cliente separado para login e verificar cookie
        var loginClient = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });
        var resp = await loginClient.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(email, "Senha@123"));

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadFromJsonAsync<LoginResponse>();
        body!.AccessToken.Should().NotBeNullOrEmpty();
        resp.Headers.TryGetValues("Set-Cookie", out var cookies);
        cookies.Should().Contain(c => c.Contains("refresh_token="));
    }

    // ── CT05 ─────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT05_LoginSenhaErrada_Retorna401()
    {
        var email = "eduardo.ct05@teste.com";
        await _client.PostAsJsonAsync("/api/v1/auth/registrar",
            new RegistrarRequest("Eduardo", email, "Senha@123"));

        var resp = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(email, "SenhaErrada"));
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── CT06 ─────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT06_LoginUsuarioInexistente_Retorna401()
    {
        var resp = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest("naoexiste@teste.com", "Senha@123"));
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await resp.Content.ReadAsStringAsync()).Should().Contain("CREDENCIAIS_INVALIDAS");
    }

    // ── CT07 ─────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT07_RefreshTokenValido_RetornaNovoAccessToken()
    {
        var email = "fernanda.ct07@teste.com";
        await _client.PostAsJsonAsync("/api/v1/auth/registrar",
            new RegistrarRequest("Fernanda", email, "Senha@123"));
        await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(email, "Senha@123"));

        // O _client tem o cookie; chamar refresh
        var refreshResp = await _client.PostAsync("/api/v1/auth/refresh", null);
        refreshResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await refreshResp.Content.ReadFromJsonAsync<RefreshResponse>();
        body!.AccessToken.Should().NotBeNullOrEmpty();
    }

    // ── CT08 ─────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT08_RefreshSemCookie_Retorna401()
    {
        // Cliente sem cookies
        var cleanClient = NovoClienteSemCookies();
        var resp = await cleanClient.PostAsync("/api/v1/auth/refresh", null);
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── CT09 ─────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT09_Logout_RevogaRefreshToken()
    {
        var email = "gabriel.ct09@teste.com";
        var sessionClient = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });

        await sessionClient.PostAsJsonAsync("/api/v1/auth/registrar",
            new RegistrarRequest("Gabriel", email, "Senha@123"));
        await sessionClient.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(email, "Senha@123"));

        var logoutResp = await sessionClient.PostAsync("/api/v1/auth/logout", null);
        logoutResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Refresh após logout → 401
        var refreshResp = await sessionClient.PostAsync("/api/v1/auth/refresh", null);
        refreshResp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── CT10 ─────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT10_AcessoSemToken_Retorna401()
    {
        var resp = await NovoClienteSemCookies().GetAsync("/api/v1/usuarios/me");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── CT11 ─────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT11_AcessoAutenticado_RetornaPerfil()
    {
        var email = "helena.ct11@teste.com";
        await _client.PostAsJsonAsync("/api/v1/auth/registrar",
            new RegistrarRequest("Helena", email, "Senha@123"));

        var loginResp = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(email, "Senha@123"));
        var login = await loginResp.Content.ReadFromJsonAsync<LoginResponse>();

        using var authed = NovoClienteAutenticado(login!.AccessToken);
        var resp = await authed.GetAsync("/api/v1/usuarios/me");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var perfil = await resp.Content.ReadFromJsonAsync<UsuarioResponse>();
        perfil!.Email.Should().Be(email);
    }

    // ── CT12 ─────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT12_AlterarNome_Retorna200ComNovoNome()
    {
        var email = "igor.ct12@teste.com";
        await _client.PostAsJsonAsync("/api/v1/auth/registrar",
            new RegistrarRequest("Igor", email, "Senha@123"));

        var loginResp = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(email, "Senha@123"));
        var login = await loginResp.Content.ReadFromJsonAsync<LoginResponse>();

        using var authed = NovoClienteAutenticado(login!.AccessToken);
        var resp = await authed.PatchAsJsonAsync("/api/v1/usuarios/me",
            new AlterarNomeRequest("Igor Atualizado"));

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var perfil = await resp.Content.ReadFromJsonAsync<UsuarioResponse>();
        perfil!.Nome.Should().Be("Igor Atualizado");
    }

    // ── CT42 ─────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT42_ExcluirConta_Retorna204()
    {
        var email = "julia.ct42@teste.com";
        var sessionClient = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });

        await sessionClient.PostAsJsonAsync("/api/v1/auth/registrar",
            new RegistrarRequest("Julia", email, "Senha@123"));

        var loginResp = await sessionClient.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(email, "Senha@123"));
        var login = await loginResp.Content.ReadFromJsonAsync<LoginResponse>();

        using var authed = NovoClienteAutenticado(login!.AccessToken);
        var resp = await authed.DeleteAsync("/api/v1/usuarios/me");
        resp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Login após exclusão deve falhar
        var loginApos = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(email, "Senha@123"));
        loginApos.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── CT43 ─────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT43_RegistrarSenhaCurta_Retorna400()
    {
        var resp = await _client.PostAsJsonAsync("/api/v1/auth/registrar",
            new RegistrarRequest("Kaue", "kaue.ct43@teste.com", "123"));
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── CT44 ─────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT44_RegistrarSemNome_Retorna400()
    {
        var resp = await _client.PostAsJsonAsync("/api/v1/auth/registrar",
            new RegistrarRequest("", "sem.nome@teste.com", "Senha@123"));
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
