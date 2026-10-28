using EmDia.Domain.Enums;

namespace EmDia.Domain.Entities;

/// <summary>
/// Registro de e-mail de aviso enviado.
/// UK (item_id, vencimento_referencia, numero) evita duplicatas (spec §2.3, RN08/RN09).
/// </summary>
public class Aviso
{
    public Guid Id { get; private set; }
    public Guid ItemId { get; private set; }
    public TipoAviso Tipo { get; private set; }
    public CanalAviso Canal { get; private set; }
    public DateOnly VencimentoReferencia { get; private set; }

    /// <summary>0 = antecipado; 1, 2... = reavisos.</summary>
    public short Numero { get; private set; }

    public DateTimeOffset EnviadoEm { get; private set; }
    public bool Sucesso { get; private set; }
    public string? Erro { get; private set; }

    // EF Core
    protected Aviso() { }

    public Aviso(
        Guid itemId,
        TipoAviso tipo,
        CanalAviso canal,
        DateOnly vencimentoReferencia,
        short numero,
        DateTimeOffset enviadoEm,
        bool sucesso,
        string? erro = null)
    {
        Id = Guid.NewGuid();
        ItemId = itemId;
        Tipo = tipo;
        Canal = canal;
        VencimentoReferencia = vencimentoReferencia;
        Numero = numero;
        EnviadoEm = enviadoEm;
        Sucesso = sucesso;
        Erro = erro;
    }

    /// <summary>Registra o resultado do envio (uma tentativa com falha pode ser refeita depois).</summary>
    public void RegistrarResultado(bool sucesso, string? erro, DateTimeOffset agora)
    {
        Sucesso = sucesso;
        Erro = erro is { Length: > 300 } ? erro[..300] : erro;
        EnviadoEm = agora;
    }
}
