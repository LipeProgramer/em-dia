using EmDia.Domain.Entities;

namespace EmDia.Domain.Services;

/// <summary>RN10: o check-up não duplica um item do mesmo modelo (e mesmo veículo) que já exista ativo.</summary>
public static class CheckupDomain
{
    public static bool ItemJaExiste(IEnumerable<Item> itensDoUsuario, Guid modeloId, Guid? veiculoId) =>
        itensDoUsuario.Any(i =>
            !i.Encerrado &&
            i.ModeloItemId == modeloId &&
            i.VeiculoId == veiculoId);
}
