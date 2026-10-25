using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EmDia.Api.IntegrationTests.Fixtures;
using EmDia.Application.DTOs.Auth;
using EmDia.Application.DTOs.Itens;
using EmDia.Application.DTOs.Preferencias;
using EmDia.Application.Interfaces;
using EmDia.Domain.Enums;
using EmDia.Infrastructure.Data;
using EmDia.Infrastructure.Jobs;
using EmDia.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EmDia.Api.IntegrationTests.Avisos;

/// <summary>
/// CT38-CT41 – Testes da rotina de avisos e BackgroundService (spec §1.2, §4.6, §6.4).
/// </summary>
[Collection("Integration")]
public class AvisosTests : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory;

    public AvisosTests(IntegrationTestFactory factory) => _factory = factory;

    private async Task<(HttpClient Client, Guid UsuarioId, string Email)> ClienteAutenticadoAsync(string sufixo)
    {
        var reg = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        var email = $"aviso.{sufixo}@teste.com";
        await reg.PostAsJsonAsync("/api/v1/auth/registrar",
            new RegistrarRequest($"User {sufixo}", email, "Senha@123"));
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

    // ── CT38 ──────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT38_AvisosWorker_ExecutarAgora_DisparaRotinaComSucesso()
    {
        var worker = _factory.Services.GetRequiredService<AvisosWorker>();

        var resultado = await worker.ExecutarAgoraAsync();

        resultado.Should().NotBeNull();
        resultado.TotalErros.Should().Be(0);
    }

    // ── CT39 ──────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT39_AvisoD30_ItemEmAtencao_EnviaEmailERegistraAviso()
    {
        var (client, usuarioId, email) = await ClienteAutenticadoAsync("ct39");
        var emailFake = (EmailServiceFake)_factory.Services.GetRequiredService<IEmailService>();

        DateOnly hoje;
        using (var scope = _factory.Services.CreateScope())
        {
            var relogio = scope.ServiceProvider.GetRequiredService<IRelogio>();
            hoje = relogio.Hoje;
        }

        // Criar item com vencimento em 20 dias e antecedência de 30 dias (status Atenção)
        var itemReq = new CriarItemRequest(
            Nome: "CNH CT39",
            CategoriaId: 2,
            TipoVencimento: TipoVencimento.DataUnica,
            DataBase: null,
            DataVencimento: hoje.AddDays(20),
            IntervaloDias: null,
            IntervaloKm: null,
            KmBase: null,
            GarantiaFabricanteMeses: null,
            AntecedenciaDias: 30,
            VeiculoId: null,
            ModeloItemId: null,
            Observacao: "Item para teste de aviso CT39",
            Passos: ["Passo 1"]);

        var criarResp = await client.PostAsJsonAsync("/api/v1/itens", itemReq);
        criarResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var itemCriado = await criarResp.Content.ReadFromJsonAsync<ItemResponse>();

        // Executar a rotina diária
        var worker = _factory.Services.GetRequiredService<AvisosWorker>();
        var resultado = await worker.ExecutarAgoraAsync();

        resultado.TotalErros.Should().Be(0);

        // Validar que o e-mail foi disparado para o usuário
        emailFake.EmailsEnviados.Should().Contain(e =>
            e.Destinatario == email &&
            e.Assunto.Contains("CNH CT39") &&
            e.Assunto.Contains("Aviso Antecipado"));

        // Validar que foi persistido na tabela de Avisos
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EmDiaDbContext>();
            var aviso = await db.Avisos.FirstOrDefaultAsync(a => a.ItemId == itemCriado!.Id);
            aviso.Should().NotBeNull();
            aviso!.Sucesso.Should().BeTrue();
            aviso.Numero.Should().Be(0);
            aviso.Tipo.Should().Be(TipoAviso.Antecipado);
            aviso.Canal.Should().Be(CanalAviso.Email);
            aviso.VencimentoReferencia.Should().Be(hoje.AddDays(20));
        }
    }

    // ── CT40 ──────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT40_SemDuplicata_NoMesmoCiclo_NaoEnviaSegundoEmail()
    {
        var (client, usuarioId, email) = await ClienteAutenticadoAsync("ct40");
        var emailFake = (EmailServiceFake)_factory.Services.GetRequiredService<IEmailService>();

        DateOnly hoje;
        using (var scope = _factory.Services.CreateScope())
        {
            var relogio = scope.ServiceProvider.GetRequiredService<IRelogio>();
            hoje = relogio.Hoje;
        }

        // Criar item em Atenção
        var itemReq = new CriarItemRequest(
            Nome: "Seguro CT40",
            CategoriaId: 1,
            TipoVencimento: TipoVencimento.DataUnica,
            DataBase: null,
            DataVencimento: hoje.AddDays(10),
            IntervaloDias: null,
            IntervaloKm: null,
            KmBase: null,
            GarantiaFabricanteMeses: null,
            AntecedenciaDias: 30,
            VeiculoId: null,
            ModeloItemId: null,
            Observacao: null,
            Passos: []);

        var criarResp = await client.PostAsJsonAsync("/api/v1/itens", itemReq);
        criarResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var worker = _factory.Services.GetRequiredService<AvisosWorker>();

        // 1ª execução -> envia 1 e-mail
        await worker.ExecutarAgoraAsync();
        var emailsPrimeiraVez = emailFake.EmailsEnviados.Count(e => e.Destinatario == email);
        emailsPrimeiraVez.Should().Be(1);

        // 2ª execução no mesmo dia / ciclo -> NÃO deve enviar duplicata
        await worker.ExecutarAgoraAsync();
        var emailsSegundaVez = emailFake.EmailsEnviados.Count(e => e.Destinatario == email);
        emailsSegundaVez.Should().Be(1);
    }

    // ── CT41 ──────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT41_AvisoComEmailDesabilitado_NaoEnviaEmail()
    {
        var (client, usuarioId, email) = await ClienteAutenticadoAsync("ct41");
        var emailFake = (EmailServiceFake)_factory.Services.GetRequiredService<IEmailService>();

        // Desabilitar e-mails nas preferências do usuário
        var prefReq = new AtualizarPreferenciasRequest(
            ReceberEmail: false,
            MaxReavisos: 2,
            IntervaloReavisoDias: 3,
            ModoFocoPadrao: false);

        var putResp = await client.PutAsJsonAsync("/api/v1/usuarios/me/preferencias", prefReq);
        putResp.StatusCode.Should().Be(HttpStatusCode.OK);

        DateOnly hoje;
        using (var scope = _factory.Services.CreateScope())
        {
            var relogio = scope.ServiceProvider.GetRequiredService<IRelogio>();
            hoje = relogio.Hoje;
        }

        // Criar item em Atenção
        var itemReq = new CriarItemRequest(
            Nome: "IPVA CT41",
            CategoriaId: 1,
            TipoVencimento: TipoVencimento.DataUnica,
            DataBase: null,
            DataVencimento: hoje.AddDays(5),
            IntervaloDias: null,
            IntervaloKm: null,
            KmBase: null,
            GarantiaFabricanteMeses: null,
            AntecedenciaDias: 30,
            VeiculoId: null,
            ModeloItemId: null,
            Observacao: null,
            Passos: []);

        var criarResp = await client.PostAsJsonAsync("/api/v1/itens", itemReq);
        criarResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var worker = _factory.Services.GetRequiredService<AvisosWorker>();
        await worker.ExecutarAgoraAsync();

        // Usuário optou por não receber e-mail -> nenhum e-mail enviado para seu endereço
        emailFake.EmailsEnviados.Should().NotContain(e => e.Destinatario == email);
    }
}
