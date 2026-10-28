using EmDia.Domain.Enums;
using EmDia.Domain.Exceptions;

namespace EmDia.Domain.Entities;

/// <summary>Veículo do usuário, usado nos vencimentos por quilometragem.</summary>
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

    protected Veiculo() { }

    public Veiculo(Guid usuarioId, string apelido, TipoVeiculo tipo, int kmAtual, int mediaKmMes, DateTimeOffset agora)
    {
        if (kmAtual < 0)
            throw new CampoInvalidoException("kmAtual", "A km não pode ser menor que 0.");
        Id = Guid.NewGuid();
        UsuarioId = usuarioId;
        KmAtual = kmAtual;
        KmAtualizadoEm = agora;
        CriadoEm = agora;
        Editar(apelido, tipo, mediaKmMes);
    }

    /// <summary>RN07: a km de um veículo nunca diminui.</summary>
    public void AtualizarKm(int novaKm, DateTimeOffset agora)
    {
        if (novaKm < KmAtual)
            throw new DomainException(CodigosErro.KmMenor, $"A km não pode ser menor que {Formatos.Km(KmAtual)}.");

        KmAtual = novaKm;
        KmAtualizadoEm = agora;
    }

    public void Editar(string apelido, TipoVeiculo tipo, int mediaKmMes)
    {
        if (mediaKmMes is < 1 or > 20_000)
            throw new CampoInvalidoException("mediaKmMes", "Informe uma média entre 1 e 20.000 km por mês.");
        Apelido = apelido.Trim();
        Tipo = tipo;
        MediaKmMes = mediaKmMes;
    }
}
