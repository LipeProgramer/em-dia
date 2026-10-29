namespace EmDia.Application;

/// <summary>Seção "Security" do appsettings (spec §4.6).</summary>
public class SegurancaOptions
{
    public const string Secao = "Security";

    public int MaxFalhasLogin { get; set; } = 5;
    public int BloqueioMinutos { get; set; } = 15;
    public int LoginTentativasPorMinuto { get; set; } = 5;

    /// <summary>Limite geral por IP em toda a API (DV21). Alto o bastante para uma sala inteira no mesmo Wi-Fi.</summary>
    public int RequisicoesPorMinuto { get; set; } = 300;
}

/// <summary>Seção "Jwt" do appsettings (spec §4.6). A chave vem de user-secrets ou variável de ambiente.</summary>
public class JwtOptions
{
    public const string Secao = "Jwt";

    public string Issuer { get; set; } = "emdia-api";
    public string Audience { get; set; } = "emdia-web";
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
    public string Key { get; set; } = string.Empty;
}

/// <summary>
/// Seção "Site": endereço público do site, usado no link de redefinição de senha (DV24). Vem da configuração, nunca
/// do cabeçalho Host da requisição (que um atacante poderia trocar para receber o link). Sem ela, usa a primeira
/// origem do CORS.
/// </summary>
public class SiteOptions
{
    public const string Secao = "Site";

    public string Url { get; set; } = "http://localhost:5173";
}
