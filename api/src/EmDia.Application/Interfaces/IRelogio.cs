namespace EmDia.Application.Interfaces;

/// <summary>
/// Re-exporta IRelogio do Domain para que a camada Application use
/// a mesma abstração sem criar dependência circular.
/// </summary>
public interface IRelogio : EmDia.Domain.Interfaces.IRelogio { }
