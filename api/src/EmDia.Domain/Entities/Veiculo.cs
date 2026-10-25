using EmDia.Domain.Enums;
using EmDia.Domain.Exceptions;

namespace EmDia.Domain.Entities;

/// <summary>
/// Veículo do usuário. Usado nos vencimentos por uso/tempo (spec §2.3).
/// RN07: a quilometragem nunca diminui.
/// </summary>
public class Veiculo
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string Apelido { get; private set; } = string.Empty;
    public TipoVeiculo Tipo { get; private set; }
    public int KmAtual { get; private set; }
    public int MediaKmMes { get; private set; }
    public DateTimeOffset KmAtualizadoEm { get; private set; }
    public DateTimeOffset CriadoEm { get; private set; }

    // EF Core
    protected Veiculo() { }

    public Veiculo(
        Guid usuarioId,
        string apelido,
        TipoVeiculo tipo,
        int kmAtual,
        int mediaKmMes,
        DateTimeOffset agora)
    {
        Id = Guid.NewGuid();
        UsuarioId = usuarioId;
        Apelido = apelido;
        Tipo = tipo;
        KmAtual = kmAtual;
        MediaKmMes = mediaKmMes;
        KmAtualizadoEm = agora;
        CriadoEm = agora;
    }

    /// <summary>
    /// RN07 — Atualiza a quilometragem. Nunca pode diminuir.
    /// </summary>
    public void AtualizarKm(int novaKm, DateTimeOffset agora)
    {
        if (novaKm < KmAtual)
            throw new DomainException(
                "KM_INVALIDA",
                $"A quilometragem não pode ser menor que {KmAtual:N0} km.");

        KmAtual = novaKm;
        KmAtualizadoEm = agora;
    }

    public void Editar(string apelido, TipoVeiculo tipo, int mediaKmMes)
    {
        Apelido = apelido;
        Tipo = tipo;
        MediaKmMes = mediaKmMes;
    }
}
