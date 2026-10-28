using EmDia.Application.DTOs.Veiculos;
using EmDia.Application.Interfaces;
using EmDia.Application.Interfaces.Repositories;
using EmDia.Domain.Entities;
using EmDia.Domain.Exceptions;
using FluentValidation;

namespace EmDia.Application.Services;

/// <summary>Veículos e quilometragem (E20–E24 · RN07, RN13).</summary>
public class VeiculoService(
    IVeiculoRepository veiculos,
    IItemRepository itens,
    IUnitOfWork uow,
    IRelogio relogio,
    IValidator<CriarVeiculoRequest> validadorCriar,
    IValidator<EditarVeiculoRequest> validadorEditar,
    IValidator<AtualizarKmRequest> validadorKm)
{
    public async Task<IReadOnlyList<VeiculoResponse>> ListarAsync(Guid usuarioId, CancellationToken ct)
    {
        var contagem = await veiculos.ContarItensPorVeiculoAsync(usuarioId, ct);
        return (await veiculos.ListarPorUsuarioAsync(usuarioId, ct))
            .Select(v => ParaResposta(v, contagem.GetValueOrDefault(v.Id)))
            .ToList();
    }

    /// <summary>E21: apelido único por usuário.</summary>
    public async Task<VeiculoResponse> CriarAsync(Guid usuarioId, CriarVeiculoRequest req, CancellationToken ct)
    {
        await validadorCriar.ValidateAndThrowAsync(req, ct);
        await GarantirApelidoLivreAsync(usuarioId, req.Apelido, null, ct);

        var veiculo = new Veiculo(usuarioId, req.Apelido, req.Tipo, req.KmAtual, req.MediaKmMes, relogio.Agora);
        await veiculos.AdicionarAsync(veiculo, ct);
        await uow.SalvarAsync(ct);
        return ParaResposta(veiculo, 0);
    }

    /// <summary>E22: muda apelido, tipo e média. A nova média altera as datas estimadas pela km (RN01, calculadas na leitura).</summary>
    public async Task<VeiculoResponse> EditarAsync(Guid id, Guid usuarioId, EditarVeiculoRequest req, CancellationToken ct)
    {
        await validadorEditar.ValidateAndThrowAsync(req, ct);
        var veiculo = await ObterVeiculoAsync(id, usuarioId, ct);
        await GarantirApelidoLivreAsync(usuarioId, req.Apelido, id, ct);

        veiculo.Editar(req.Apelido, req.Tipo, req.MediaKmMes);
        await uow.SalvarAsync(ct);

        var contagem = await veiculos.ContarItensPorVeiculoAsync(usuarioId, ct);
        return ParaResposta(veiculo, contagem.GetValueOrDefault(id));
    }

    /// <summary>E23: atualiza a km (RN07: não pode diminuir) e devolve o novo status dos itens do veículo.</summary>
    public async Task<AtualizarKmResponse> AtualizarKmAsync(Guid id, Guid usuarioId, AtualizarKmRequest req, CancellationToken ct)
    {
        await validadorKm.ValidateAndThrowAsync(req, ct);
        var veiculo = await ObterVeiculoAsync(id, usuarioId, ct);

        veiculo.AtualizarKm(req.KmAtual, relogio.Agora);
        await uow.SalvarAsync(ct);

        var afetados = (await itens.ListarAtivosPorVeiculoAsync(id, usuarioId, ct))
            .Select(i => new ItemAfetado(i.Id, i.Nome, i.CalcularStatus(relogio.Hoje, veiculo)))
            .ToList();
        return new AtualizarKmResponse(veiculo.KmAtual, afetados);
    }

    /// <summary>E24 (RN13): bloqueado enquanto houver itens ligados ao veículo.</summary>
    public async Task ExcluirAsync(Guid id, Guid usuarioId, CancellationToken ct)
    {
        var veiculo = await ObterVeiculoAsync(id, usuarioId, ct);
        var contagem = await veiculos.ContarItensPorVeiculoAsync(usuarioId, ct);
        if (contagem.GetValueOrDefault(id) > 0)
            throw new ConflictException(CodigosErro.VeiculoComItens,
                "Este veículo ainda tem itens ligados a ele. Exclua ou mude esses itens antes.");

        veiculos.Remover(veiculo);
        await uow.SalvarAsync(ct);
    }

    private async Task<Veiculo> ObterVeiculoAsync(Guid id, Guid usuarioId, CancellationToken ct) =>
        await veiculos.ObterAsync(id, usuarioId, ct)
        ?? throw new NotFoundException("VEICULO_NAO_ENCONTRADO", "Não encontramos este veículo.");

    private async Task GarantirApelidoLivreAsync(Guid usuarioId, string apelido, Guid? ignorarId, CancellationToken ct)
    {
        if (await veiculos.ApelidoExisteAsync(usuarioId, apelido.Trim(), ignorarId, ct))
            throw new ConflictException(CodigosErro.ApelidoDuplicado, $"Você já tem um veículo chamado \"{apelido.Trim()}\".");
    }

    private static VeiculoResponse ParaResposta(Veiculo v, int quantidadeItens) =>
        new(v.Id, v.Apelido, v.Tipo, v.KmAtual, v.MediaKmMes, v.KmAtualizadoEm, quantidadeItens);
}
