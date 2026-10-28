namespace EmDia.Application;

/// <summary>Seção "Security" do appsettings (spec §4.6).</summary>
public class SegurancaOptions
{
    public const string Secao = "Security";

    public int MaxFalhasLogin { get; set; } = 5;
    public int BloqueioMinutos { get; set; } = 15;
    public int LoginTentativasPorMinuto { get; set; } = 5;
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
