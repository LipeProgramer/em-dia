using System.Text.Json;

namespace EmDia.Domain.Entities;

/// <summary>
/// Registro de cada check-up realizado pelo usuário (spec §2.3).
/// As respostas sim/não são salvas como jsonb para não criar tabela separada.
/// </summary>
public class Checkup
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public DateTimeOffset RealizadoEm { get; private set; }

    /// <summary>JSON: { "TEM_CARRO": true, "TEM_CNH": false, ... }</summary>
    public string Respostas { get; private set; } = "{}";

    public short ItensCriados { get; private set; }

    // EF Core
    protected Checkup() { }

    public Checkup(
        Guid usuarioId,
        Dictionary<string, bool> respostas,
        short itensCriados,
        DateTimeOffset agora)
    {
        Id = Guid.NewGuid();
        UsuarioId = usuarioId;
        Respostas = JsonSerializer.Serialize(respostas);
        ItensCriados = itensCriados;
        RealizadoEm = agora;
    }
}
