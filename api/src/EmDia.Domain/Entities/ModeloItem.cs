using EmDia.Domain.Enums;

namespace EmDia.Domain.Entities;

/// <summary>
/// Modelo de item pré-definido para check-up e cadastro rápido (seed).
/// spec §2.3 — tabela modelos_item.
/// </summary>
public class ModeloItem
{
    public Guid Id { get; private set; }
    public short CategoriaId { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public TipoVencimento TipoVencimento { get; private set; }
    public int? IntervaloDias { get; private set; }
    public int? IntervaloKm { get; private set; }
    public short? GarantiaFabricanteMeses { get; private set; }
    public short AntecedenciaDias { get; private set; }
    public string? CodigoCheckup { get; private set; }
    public bool Ativo { get; private set; } = true;

    // Navegação
    private readonly List<PassoModelo> _passos = new();
    public IReadOnlyList<PassoModelo> Passos => _passos.AsReadOnly();

    // EF Core
    protected ModeloItem() { }

    public ModeloItem(
        Guid id,
        short categoriaId,
        string nome,
        TipoVencimento tipoVencimento,
        int? intervaloDias,
        int? intervaloKm,
        short? garantiaFabricanteMeses,
        short antecedenciaDias,
        string? codigoCheckup)
    {
        Id = id;
        CategoriaId = categoriaId;
        Nome = nome;
        TipoVencimento = tipoVencimento;
        IntervaloDias = intervaloDias;
        IntervaloKm = intervaloKm;
        GarantiaFabricanteMeses = garantiaFabricanteMeses;
        AntecedenciaDias = antecedenciaDias;
        CodigoCheckup = codigoCheckup;
        Ativo = true;
    }
}
