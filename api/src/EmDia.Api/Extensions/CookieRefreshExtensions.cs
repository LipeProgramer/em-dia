namespace EmDia.Api.Extensions;

/// <summary>
/// Cookie do token de renovação: HttpOnly; Secure; SameSite=Strict; Path=/api/v1/auth (spec §4.3).
/// O JavaScript da página não consegue lê-lo, o que protege contra roubo por XSS.
/// </summary>
public static class CookieRefreshExtensions
{
    public const string NomeCookieRefresh = "refreshToken";
    private const string Caminho = "/api/v1/auth";

    public static void EmitirCookieRefresh(this HttpResponse response, string valor, DateTimeOffset expiraEm,
        IWebHostEnvironment env) =>
        response.Cookies.Append(NomeCookieRefresh, valor, Opcoes(env, expiraEm));

    public static void ApagarCookieRefresh(this HttpResponse response, IWebHostEnvironment env) =>
        response.Cookies.Delete(NomeCookieRefresh, Opcoes(env, null));

    private static CookieOptions Opcoes(IWebHostEnvironment env, DateTimeOffset? expiraEm) => new()
    {
        HttpOnly = true,
        // O servidor de testes (TestServer) usa http; nos demais ambientes o cookie é sempre Secure.
        // Navegadores aceitam cookies Secure em http://localhost no desenvolvimento.
        Secure = !env.IsEnvironment("Testing"),
        SameSite = SameSiteMode.Strict,
        Path = Caminho,
        Expires = expiraEm
    };
}
