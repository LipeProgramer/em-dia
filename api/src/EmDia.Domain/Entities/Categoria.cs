namespace EmDia.Domain.Entities;

/// <summary>
/// Categoria fixa do Radar (seed). PK smallint conforme spec §2.3.
/// 1 Veículo | 2 Documentos | 3 Casa | 4 Saúde | 5 Finanças | 6 Garantias
/// </summary>
public class Categoria
{
    public short Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Icone { get; private set; } = string.Empty;
    public short Ordem { get; private set; }

    // EF Core
    protected Categoria() { }

    public Categoria(short id, string nome, string icone, short ordem)
    {
        Id = id;
        Nome = nome;
        Icone = icone;
        Ordem = ordem;
    }
}
