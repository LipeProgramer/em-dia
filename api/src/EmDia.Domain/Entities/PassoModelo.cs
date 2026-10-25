namespace EmDia.Domain.Entities;

/// <summary>
/// Passo sugerido de um modelo (copiado para o item ao criá-lo — RN14).
/// spec §2.3 — tabela passos_modelo.
/// </summary>
public class PassoModelo
{
    public Guid Id { get; private set; }
    public Guid ModeloItemId { get; private set; }
    public short Ordem { get; private set; }
    public string Descricao { get; private set; } = string.Empty;

    // EF Core
    protected PassoModelo() { }

    public PassoModelo(Guid modeloItemId, short ordem, string descricao)
    {
        Id = Guid.NewGuid();
        ModeloItemId = modeloItemId;
        Ordem = ordem;
        Descricao = descricao;
    }
}
