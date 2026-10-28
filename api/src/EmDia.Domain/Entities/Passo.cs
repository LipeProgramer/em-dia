namespace EmDia.Domain.Entities;

/// <summary>
/// Passo pequeno de um item (spec §2.3).
/// Volta a false quando o item é renovado (RN05).
/// </summary>
public class Passo
{
    public Guid Id { get; private set; }
    public Guid ItemId { get; private set; }
    public short Ordem { get; private set; }
    public string Descricao { get; private set; } = string.Empty;
    public bool Concluido { get; private set; }
    public DateTimeOffset? ConcluidoEm { get; private set; }

    // EF Core
    protected Passo() { }

    public Passo(Guid itemId, short ordem, string descricao)
    {
        Id = Guid.NewGuid();
        ItemId = itemId;
        Ordem = ordem;
        Descricao = descricao;
        Concluido = false;
    }

    public void Marcar(bool concluido, DateTimeOffset agora)
    {
        Concluido = concluido;
        ConcluidoEm = concluido ? agora : null;
    }

    /// <summary>Reinicia o passo ao renovar o item (RN05).</summary>
    internal void Reiniciar()
    {
        Concluido = false;
        ConcluidoEm = null;
    }
}
